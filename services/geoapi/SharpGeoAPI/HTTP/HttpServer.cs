using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Core;
using log4net;
using MongoDB.Driver;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.HTTP
{
    class HttpServer : IDisposable
    {
        private readonly ILog log;
        private readonly Service service;
        private readonly Settings settings;

        private readonly Thread serverThread;
        private HttpListener listener;

        public HttpServer(Settings settings)
        {
            this.settings = settings;
            this.log = LogManager.GetLogger(typeof(HttpServer));

            service = new Service(settings);

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
            serverThread.Abort();
            listener.Stop();
        }

       
    }
}