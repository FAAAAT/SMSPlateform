﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Logger;
using Quartz;
using Quartz.Impl;

namespace GSMMODEM
{
    public class GSMPool : IEnumerable<GsmModem>,IDisposable
    {
        public static int BandRate = 115200;
        private Dictionary<string, GsmModem> pool = new Dictionary<string, GsmModem>();

        private IScheduler scheduler;

        public GSMPool(SMSPlatformLogger logger)
        {
            var job = JobBuilder.Create<GSMDiscoverJob>().WithIdentity("SMSJob", "SMSGroup1").Build();

            var jobListener = new GSMDiscoverJobListener();
            jobListener.poolReference = pool;
            jobListener.logger = logger;

            var trigger = TriggerBuilder.Create().WithIdentity("SMStrigger", "SMSGroup1").WithSimpleSchedule(x => x.WithInterval(new TimeSpan(0, 0, 3))).StartNow().Build();
            
            scheduler = StdSchedulerFactory.GetDefaultScheduler();

            scheduler.ListenerManager.AddJobListener(jobListener);
            scheduler.ScheduleJob(job, trigger);
            scheduler.Start();

        }

        public GsmModem this[string indexer]
        {
            get
            {
                pool.TryGetValue(indexer, out var gm);
                return gm;
            }
        }

        public Dictionary<string, string> PhoneComDic
        {
            get { return pool.Values.ToDictionary(x => x.ComPort, x => x.PhoneNumber); }
        }

        public void Dispose()
        {
            if (scheduler != null && !scheduler.IsShutdown)
            {
                scheduler.Shutdown(true);
            }
            foreach (KeyValuePair<string, GsmModem> kv in pool)
            {   
                kv.Value.Close();
            }

        }

        public IEnumerator<GsmModem> GetEnumerator()
        {
            return pool.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class GSMModemExtension
    {
        public static Regex PhoneReg = new Regex("(\\+?86)?\\d{11}");


        public static string GetPhoneNum(this GsmModem modem)
        {
            var result = modem.SendAT("AT+CPBS=\"SM\"");
            if (result.Contains("ERROR"))
            {
                return null;
            }
            result = modem.SendAT("AT+CPBR=1");
            if (result.Contains("ERROR"))
            {
                return null;
            }

            return PhoneReg.Match(result).Value;


        }

        public static bool SetPhoneNum(this GsmModem modem, string phone)
        {
            var result = modem.SendAT("AT+CPBS=\"" + phone + "\"");

            if (result.Contains("ERROR"))
            {
                return false;
            }
            result = modem.SendAT($"AT+CPBW=1,\"{phone}\"");

            if (result.Contains("ERROR"))
            {
                return false;
            }

            return true;


        }
    }



    public class GSMDiscoverJobListener : IJobListener
    {
        public Dictionary<string, GsmModem> poolReference;
        public SMSPlatformLogger logger;

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            context.JobDetail.JobDataMap.Add("execContext", poolReference);
            context.JobDetail.JobDataMap.Add("logger", logger);
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {

        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {

        }

        public string Name { get; } = "GSMDiscoverJob";
    }

    public class GSMDiscoverJob : IJob
    {


        public void Execute(IJobExecutionContext context)
        {
            var logger = context.JobDetail.JobDataMap.Get("logger") as SMSPlatformLogger;
            var dic = context.JobDetail.JobDataMap.Get("execContext") as Dictionary<string, GsmModem>;
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                Console.WriteLine("opening port "+port);
                try
                {
                    if (dic.ContainsKey(port))
                    {
                        var modem = dic[port];
                        if (!modem.isConnect())
                        {
                            modem.Close();
                            dic.Remove(port);
                        }
                    }
                    else
                    {
                        GsmModem modem = new GsmModem(port, GSMPool.BandRate);
                        modem.Open();
                        if (modem.isConnect())
                        {
                            if (!dic.ContainsKey(modem.ComPort))
                            {
                                dic.Add(modem.ComPort, modem);
                            }
                        }
                        else
                        {
                            modem.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                }
            }
        }



    }
}
