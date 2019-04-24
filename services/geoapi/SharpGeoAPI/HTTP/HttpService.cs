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
using SharpGeoAPI.Models;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP
{
    class Settings
    {
        public int ParallelismDegree { get; set; } = 8;
        public int Port { get; set; } = 9007;


    }

    class HttpService : IDisposable
    {
        private readonly Settings settings;
        private readonly IAgentStorage agentStorage;
        private readonly ILog log;

        private readonly ConcurrentDictionary<string, IBaseHandler> handlers;

        private Thread serverThread;
        private HttpListener listener;

        public HttpService(Settings settings, ICollection<IBaseHandler> handlers, IAgentStorage agentStorage, ILog log)
        {
            this.handlers = new ConcurrentDictionary<string, IBaseHandler>(handlers.ToDictionary(handler => handler.Key,
                handler => handler));

            this.settings = settings;
            this.agentStorage = agentStorage;
            this.log = log;

            serverThread = new Thread(Listen);
            serverThread.Start();
        }

        private void Listen()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + settings.Port + "/");
            listener.Start();
            using (var semaphore = new SemaphoreSlim(settings.ParallelismDegree, settings.ParallelismDegree))
            using (agentStorage)
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

        public void Dispose()
        {
            serverThread.Abort();
            listener.Stop();
        }
    }
}