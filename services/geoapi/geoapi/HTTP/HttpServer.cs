using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using geoapi.HTTP.Handlers;
using geoapi.Utils;
using log4net;

namespace geoapi.HTTP
{
    public class HttpServer : IDisposable, ILoggerProvider
    {
        private readonly ILog log;
        private Service service;
        private Settings settings;

        private Thread serverThread;
        private HttpListener listener;

        public HttpServer(Settings settings)
        {
            this.settings = settings;
            this.log = GetLog();

            service = Service.BuildWithService(settings);
            serverThread = new Thread(Listen);
            serverThread.Start();
        }

        private void Listen()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + settings.Port + "/");
            listener.Start();
            using (var semaphore = new SemaphoreSlim(settings.ParallelismDegree, settings.ParallelismDegree))
            {
                while (true)
                {
                    var context = listener.GetContext();
                    Task.Run(async () => await service.ProcessRequest(semaphore, context));
                }
            }
        }

        public void Dispose()
        {
            listener.Stop();
        }

        public ILog GetLog()
        {
            return LogManager.GetLogger(GetType());
        }
    }
}