using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using DataBaseAccessHelper;
using GSMMODEM;
using Logger;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Nancy;
using Nancy.Conventions;
using Nancy.Cryptography;
using Newtonsoft.Json;
using Owin;
using SMSPlatform.Filters;
using SMSPlatform.Services;

namespace SMSPlatform
{
    class Program
    {

        public static AutoResetEvent handle = new AutoResetEvent(false);
        public static Process process;
        public static GSMPool pool;


        static void Main(string[] args)
        {
            
            SMSPlatformLogger logger = new SMSPlatformLogger();

            GSMTaskService taskService = null;


            string url = "http://localhost:64453";
            SetCookieAuthentication("localhost");
            var host = WebApp.Start<StartUp>(url);
            try
            {

                Console.WriteLine("system online...");
                Console.WriteLine($"the url is {url}");
                pool = new GSMPool(logger);
                Console.WriteLine("gsm system online...");
                Console.WriteLine("press any key to exit...");

                AppDomain.CurrentDomain.SetData("Logger", logger);
                AppDomain.CurrentDomain.SetData("Pool", pool);
                taskService = new GSMTaskService();
                TaskServiceStartUp taskServiceStartUp = new TaskServiceStartUp(taskService);
                AppDomain.CurrentDomain.SetData("TaskService", taskService);



                process = new Process();
                process.StartInfo = new ProcessStartInfo(url+"/pages/login.html");


                process.Start();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                host.Dispose();
                handle.Close();
                handle.Dispose();
                taskService?.Dispose();
                if (process != null)
                {
                    process.Close();
                    process.Dispose();
                }
                if (pool != null)
                {
                    pool.Dispose();
                }

            }

        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            handle.Set();
        }

        public static void SetCookieAuthentication(string domain)
        {
            AuthenticationServiceExtensions.HeaderName = "UserInfo";
            CookieService.AuthenticationCookieDomain = domain;
        }
    }



    public class StartUp
    {
        public void Configuration(IAppBuilder builder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Filters.Add(new AuthenticationFilter());
            config.Routes.MapHttpRoute(
                "default", "api/{controller}/{action}/{id}", new
                {
                    id = RouteParameter.Optional
                });

            config.Formatters.Remove(config.Formatters.Where(x => x.GetType() == typeof(JsonMediaTypeFormatter)).Single());
            config.Formatters.Add(new NewtonFormatter());

            //            config.Routes.MapHttpRoute(
            //                "resource", "/api/{controller}/{action]/{id}", new
            //                {
            //                    id = RouteParameter.Optional
            //                });
            //            var cors = new CorsProvider();
            //            cors.AddOrigin("localhost:64453");
            //            CorsOptions.AllowAll.PolicyProvider = ;


#if DEBUG

#endif
            builder

                                //                .UseNancy(options => options.PerformPassThrough =
                                //                    context =>
                                //                    {
                                //                        var values = context.Request.Url.Path.Split('/');
                                //                        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.ToLower() == "api";
                                //                    }
                                //                )
                                .Use(myhandle)
                .UseCors(CorsOptions.AllowAll)
                .MapSignalR("/api/signalr", new HubConfiguration())

                //                .UseCors()
                .UseWebApi(config);

        }


        public Task myhandle(IOwinContext context, Func<Task> next)
        {

            //获取物理文件路径  
            var path = GetFilePath(context.Request.Path.Value);

            //验证路径是否存在  
            if (File.Exists(path))
            {
                return SetResponse(context, path);
            }

            //不存在返回下一个请求  
            return next();
        }
        public static string GetFilePath(string relPath)
        {
#if DEBUG
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory
                , "..\\..\\"
                , relPath.TrimStart('/').Replace('/', '\\'));
#else
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory
                , relPath.TrimStart('/').Replace('/', '\\'));
#endif

        }

        public Task SetResponse(IOwinContext context, string path)
        {
            var perfix = Path.GetExtension(path);
            if (perfix == ".html")
                context.Response.ContentType = "text/html; charset=utf-8";
            else if (perfix == ".js")
                context.Response.ContentType = "application/x-javascript";
            else if (perfix == ".css")
                context.Response.ContentType = "text/css";
            else if (perfix == ".eot")
                context.Response.ContentType = "application/vnd.ms-fontobject";
            else if (perfix == ".ttf")
                context.Response.ContentType = "application/octet-stream";
            else if (perfix == ".svg")
                context.Response.ContentType = "mage/svg+xml";
            else if (perfix == ".woff")
                context.Response.ContentType = "application/font-woff";
            else if (perfix == ".woff2")
                context.Response.ContentType = "font/woff2";

            return context.Response.WriteAsync(File.ReadAllBytes(path));
        }
    }

    public class CustomerBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/Content", "Content"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/Scripts", "Scripts"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/Pages", "Pages"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/Upload", "Upload"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/Download", "Download"));



        }

        protected override IRootPathProvider RootPathProvider { get; } = new PathRootProvider();
    }


    public class PathRootProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
#if DEBUG
            return Path.Combine(Environment.CurrentDirectory, "../../");
#else
            return Path.Combine(Environment.CurrentDirectory);
#endif

        }
    }

    public class CorsProvider : ICorsPolicyProvider
    {

        CorsPolicy policy = new CorsPolicy();
        public Task<CorsPolicy> GetCorsPolicyAsync(IOwinRequest request)
        {

            policy.SupportsCredentials = true;

            policy.AllowAnyHeader = true;
            policy.AllowAnyMethod = true;

            return Task.FromResult(policy);
        }

        public void AddOrigin(string origin)
        {
            policy.Origins.Add(origin);
        }
    }


    public class NewtonFormatter : JsonMediaTypeFormatter
    {


        public override object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding, IFormatterLogger formatterLogger)
        {
            byte[] buffer = new byte[readStream.Length];
            readStream.Read(buffer, 0, buffer.Length);
            var str = Encoding.UTF8.GetString(buffer);
            var obj = JsonConvert.DeserializeObject(str, type);
            return obj;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            byte[] buffer = new byte[readStream.Length];
            readStream.Read(buffer, 0, buffer.Length);
            var str = Encoding.UTF8.GetString(buffer);
            var obj = JsonConvert.DeserializeObject(str, type);
            return Task.FromResult(obj);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger,
            CancellationToken cancellationToken)
        {
            //            var str = content.ReadAsStringAsync().GetAwaiter().GetResult();
            byte[] buffer = new byte[content.Headers.ContentLength.Value];
            readStream.Read(buffer, 0, buffer.Length);
            var str = Encoding.UTF8.GetString(buffer);
            var obj = JsonConvert.DeserializeObject(str, type);
            return Task.FromResult(obj);
        }
    }
}
