using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebSockets;
using DataBaseAccessHelper;
using GSMMODEM;
using Logger;
using Microsoft.AspNet.SignalR;
using SMSPlatform.Controllers;
using SMSPlatform.Services;

namespace SMSPlatform
{
    public class TaskServiceStartUp
    {
        private GSMTaskService service;
        private SMSPlatformLogger logger;
        public TaskServiceStartUp(GSMTaskService service)
        {
            this.service = service;
            this.service.OnGetNextData += Service_OnGetNextData;
            this.service.OnSuccess += Service_OnSuccess;
            this.service.OnError += Service_OnError;
            this.service.OnStart += Service_OnStart;
            this.service.OnStop += Service_OnStop;


            this.logger = AppDomain.CurrentDomain.GetData("Logger") as SMSPlatformLogger;
        }

        private void Service_OnStop(object sender, EventArgs e)
        {
            GlobalHost.ConnectionManager.GetHubContext<MessageHub>().Clients.All.Stop();
        }

        private void Service_OnStart(object sender, EventArgs e)
        {
            GlobalHost.ConnectionManager.GetHubContext<MessageHub>().Clients.All.Start();
        }



        private void Service_OnError(object sender, ValueContainer<Models.SMSSendQueueModel> e)
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();

            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                SMSSendQueueService queueService = new SMSSendQueueService(helper);
                var phoneNumber = (sender as GsmModem).PhoneNumber;
                queueService.CompleteSMS(e.Value.ID.Value, false);
                GlobalHost.ConnectionManager.GetHubContext<MessageHub>().Clients.All.QueueError(e.Value.ContainerID, queueService.GetContainerStatus(e.Value.ContainerID.Value), e.Value.ID.Value, 3);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                helper.Dispose();
            }
        }

        private void Service_OnSuccess(object sender, ValueContainer<Models.SMSSendQueueModel> e)
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();

            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                SMSSendQueueService queueService = new SMSSendQueueService(helper);
                var phoneNumber = (sender as GsmModem).PhoneNumber;
                queueService.CompleteSMS(e.Value.ID.Value, true);



                GlobalHost.ConnectionManager.GetHubContext<MessageHub>().Clients.All.QueueSuccess(e.Value.ContainerID, queueService.GetContainerStatus(e.Value.ContainerID.Value), e.Value.ID.Value, 2);

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                helper.Dispose();
            }
        }

        private void Service_OnGetNextData(object sender, ValueContainer<Models.SMSSendQueueModel> e)
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();

            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                SMSSendQueueService queueService = new SMSSendQueueService(helper);
                var phoneNumber = (sender as GsmModem).PhoneNumber;
                var model = queueService.GetNextData(phoneNumber);
                e.Value = model;
                if (e.Value != null)
                {
                    GlobalHost.ConnectionManager.GetHubContext<MessageHub>().Clients.All.QueueStart(e.Value.ID.Value, 1, e.Value.ContainerID.Value, 1);

                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                helper.Dispose();
            }
        }

    }
}
