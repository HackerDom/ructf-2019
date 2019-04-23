using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using SharpGeoAPI.HTTP.Handlers;

namespace SharpGeoAPI.HTTP
{
    class Settings
    {
        public int ParallelismDegree { get; } = 8;
        public int Port { get; } = 9007;
    }

    class HttpService
    {
        private readonly Settings settings;
        private readonly IStorage storage;
        private readonly ILog log;

        private readonly ConcurrentDictionary<string, IBaseHandler> handlers;

        private Thread serverThread;
        private HttpListener listener;

        public HttpService(Settings settings, ICollection<IBaseHandler> handlers, IStorage storage, ILog log)
        {
            this.handlers = new ConcurrentDictionary<string, IBaseHandler>(handlers.ToDictionary(handler => handler.Key,
                handler => handler));

            this.settings = settings;
            this.storage = storage;
            this.log = log;
        }

        public void Stop()
        {
            serverThread.Abort();
            listener.Stop();
        }

        private void Listen()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + settings.Port + "/");
            listener.Start();
            using (var semaphore = new SemaphoreSlim(settings.ParallelismDegree, settings.ParallelismDegree))
            using (storage)
            {
                {
                    while (true)
                    {
                        var context = listener.GetContext();
                        Task.Run(() => ProcessRequest(semaphore, context));
                    }
                }
            }
        }

        private async Task ProcessRequest(SemaphoreSlim semaphoreSlim, HttpListenerContext context)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var key = GetHandkerKey(context);
                if (handlers.ContainsKey(key))
                {
                    await handlers[key].ProcessRequest(context);
                }
            }
            catch (Exception e)
            {
                log.Error($"Can't process {context.Request.HttpMethod} {context.Request.Url}");
                await context.Response.Send(500, "Unexpected error");
            }
            finally
            {
                semaphoreSlim.Release();
            }
            context.Response.Close();
        }

        private static string GetHandkerKey(HttpListenerContext context)
        {
            var method = new HttpMethod(context.Request.HttpMethod);
            return $"{method}{context.Request.Url.LocalPath}";
        }

        public void Start()
        {
            serverThread = new Thread(Listen);
            serverThread.Start();
        }
    }
}