using GSMMODEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logger;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class GSMTaskService : IDisposable
    {
        private GSMPool pool;

        private SMSPlatformLogger logger;

        public GSMTaskServiceStatus Status = GSMTaskServiceStatus.Stop;

        private Dictionary<string, Task> tasks = new Dictionary<string, Task>();
        private Dictionary<string, CancellationTokenSource> tokens = new Dictionary<string, CancellationTokenSource>();
        private Dictionary<string, WaitHandle> handles = new Dictionary<string, WaitHandle>();
        private Dictionary<string, bool> normalLoopContinues = new Dictionary<string, bool>();
        
        public GSMTaskService()
        {
            pool = AppDomain.CurrentDomain.GetData("Pool") as GSMPool;
            logger = AppDomain.CurrentDomain.GetData("Logger") as SMSPlatformLogger;

        }
        
        public void OnFireOpen(object sender, EventArgs e)
        {
            var tokenSource = new CancellationTokenSource();

            var gsmModem = sender as GsmModem;

            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);


            var task = new Task(x => ProceedSMSSendQueue(x), new Dictionary<string, object>()
            {
                { "modem",gsmModem},
                { "waitHandle",waitHandle},
                { "normalLoopContinues",normalLoopContinues },

            }, tokenSource.Token);
            if (!tasks.ContainsKey(gsmModem.PhoneNumber))
            {
                tasks.Add(gsmModem.PhoneNumber, task);
                task.Start();

            }
            if (!tokens.ContainsKey(gsmModem.PhoneNumber))
            {
                tokens.Add(gsmModem.PhoneNumber, tokenSource);

            }
            if (!tokens.ContainsKey(gsmModem.PhoneNumber))
            {
                handles.Add(gsmModem.PhoneNumber, waitHandle);

            }

        }

        public void OnFireClose(object sender, EventArgs e)
        {
            var gsmModem = sender as GsmModem;
            if (tokens.ContainsKey(gsmModem.PhoneNumber))
            {
                tokens[gsmModem.PhoneNumber].Cancel();
            }
            if (tasks.ContainsKey(gsmModem.PhoneNumber))
            {
                tasks.Remove(gsmModem.PhoneNumber);
            }
        }

        private void ProceedSMSSendQueue(object context)
        {
            var contextDic = context as Dictionary<string, object>;
            var modem = contextDic["modem"] as GsmModem;
            var waitHandle = contextDic["waitHandle"] as WaitHandle;
            var normalLoopContinues = contextDic["normalLoopContinues"] as Dictionary<string, bool>;

            while (normalLoopContinues.ContainsKey(modem.PhoneNumber) && normalLoopContinues[modem.PhoneNumber])
            {
                if (OnGetNextData != null)
                {
                    var model = new ValueContainer<SMSSendQueueModel>();
                    OnGetNextData(modem, model);
                    if (model.Value != null)
                    {
                        try
                        {
                            modem.SendMsg(model.Value.ToPhoneNumber, model.Value.SMSContent, out string error,
                                out int sendCount);
                            model.Msg = error;
                            model.SendCount = sendCount;
                            if (!string.IsNullOrWhiteSpace(error))
                            {
                                OnError?.Invoke(modem, model);
                            }
                            else
                            {
                                OnSuccess?.Invoke(modem, model);
                            }
                        }
                        catch (Exception e)
                        {
                            model.Msg = e.ToString();
                            OnError?.Invoke(modem, model);
                        }

                    }
                    else
                    {
                        logger.Debug("locked by no task");
                        modem.Status = GSMModemStatus.Pause;
                        waitHandle.WaitOne();
                        modem.Status = GSMModemStatus.StandBy;
                        logger.Debug("unlock by new start(phone)");

                    }

                }

            }




        }

        public event EventHandler<ValueContainer<SMSSendQueueModel>> OnGetNextData;

        public event EventHandler<ValueContainer<SMSSendQueueModel>> OnError;

        public event EventHandler<ValueContainer<SMSSendQueueModel>> OnSuccess;

        public event EventHandler OnStart;

        public event EventHandler OnStop;






        public void Start()
        {
            if (Status == GSMTaskServiceStatus.Running)
            {
                return;
            }
            lock (this)
            {
                foreach (GsmModem gsmModem in pool)
                {
                    CancellationTokenSource tokenSource = null;
                    if (tokens.ContainsKey(gsmModem.PhoneNumber))
                    {
                        tokenSource = tokens[gsmModem.PhoneNumber];
                    }
                    else
                    {
                        tokenSource = new CancellationTokenSource();
                        tokens.Add(gsmModem.PhoneNumber, tokenSource);
                    }

                    WaitHandle waitHandle;
                    if (handles.ContainsKey(gsmModem.PhoneNumber))
                    {
                        waitHandle = handles[gsmModem.PhoneNumber];
                    }
                    else
                    {
                        waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                        handles.Add(gsmModem.PhoneNumber, waitHandle);

                    }

                    normalLoopContinues.Add(gsmModem.PhoneNumber, true);


                    var task = new Task(x => ProceedSMSSendQueue(x), new Dictionary<string, object>()
                {
                    { "modem",gsmModem},
                    { "waitHandle",waitHandle},
                    { "normalLoopContinues",normalLoopContinues },



                }, tokenSource.Token);
                    tasks.Add(gsmModem.PhoneNumber, task);
                    task.Start();

                }
                pool.OnModemClose += OnFireClose;
                pool.OnModemOpen += OnFireOpen;
                Status = GSMTaskServiceStatus.Running;
            }
            if (OnStart != null)
            {
                OnStart(this, new EventArgs());
            }
        }

        public void Start(string phone)
        {
            if (handles.ContainsKey(phone))
            {
                (handles[phone] as EventWaitHandle).Set();

            }
            else
            {
                logger.Error("给定的phone找不到对应handle");
            }
        }

        public void Stop()
        {
            if (Status == GSMTaskServiceStatus.Stop)
            {
                return;
            }
            lock (this)
            {

                pool.OnModemClose -= OnFireClose;
                pool.OnModemOpen -= OnFireOpen;

                foreach (var token in tokens)
                {
                    //                    force cancel
                    //                    token.Value.Cancel();
                    //                    normal cancel
                    normalLoopContinues[token.Key] = false;
                    (handles[token.Key] as EventWaitHandle).Set();
                    tasks.Remove(token.Key);
                    normalLoopContinues.Remove(token.Key);
                }
                Task.WaitAll(tasks.Values.ToArray());
                Status = GSMTaskServiceStatus.Stop;
            }
            if (OnStop != null)
            {
                OnStop(this, new EventArgs());
            }
        }

        public void Dispose()
        {
            Stop();


            pool?.Dispose();
            foreach (KeyValuePair<string, WaitHandle> keyValuePair in handles)
            {
                keyValuePair.Value.Close();
                keyValuePair.Value.Dispose();
            }
        }
    }





    public class ValueContainer<T> where T : new()
    {
        public T Value { get; set; }
        public string Msg { get; set; }
        public int SendCount { get; set; }
    }

    public enum GSMTaskServiceStatus
    {
        Running, StandBy, Stop
    }

}
