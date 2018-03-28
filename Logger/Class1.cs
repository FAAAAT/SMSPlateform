using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace Logger
{
    public class SMSPlatformLogger
    {

        private ILog logger;

        public SMSPlatformLogger()
        {
#if DEBUG
            FileInfo configFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, "../../app.config"));
#else
            
            FileInfo configFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, "SMSPlatform.exe.config"));
#endif
            XmlConfigurator.ConfigureAndWatch(configFile);

            logger = LogManager.GetLogger("");
        }

        public void Info(string content)
        {
            logger.Info(content);
        }

        public void Warn(string content)
        {
            logger.Warn(content);
        }

        public void Error(string content)
        {
            logger.Error(content);
        }

        public void Debug(string content)
        {
            logger.Debug(content);
        }




    }
}
