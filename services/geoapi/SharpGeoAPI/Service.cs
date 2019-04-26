using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using log4net;
using SharpGeoAPI.HTTP.Handlers;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI
{
    public class Service
    {
        public static Service BuildWithService(Settings settings)
        {
            var serviceBuilder = new ContainerBuilder();
            serviceBuilder.RegisterInstance(settings).As<ISettings>();
            serviceBuilder.RegisterInstance(LogManager.GetLogger(typeof(Service))).As<ILog>();
            serviceBuilder.RegisterType<UploadTerrainObjectHandler>().As<IBaseHandler>();
            serviceBuilder.RegisterType<GetAgentInfoHandler>().As<IBaseHandler>();
            serviceBuilder.RegisterType<RegisterAgentHandler>().As<IBaseHandler>();
            serviceBuilder.RegisterType<TerrainObjectStore>().As<ITerrainObjectStore>();
            serviceBuilder.RegisterType<Storage>().As<IStorage>();

            var container = serviceBuilder.Build();

            var handlers = new ConcurrentDictionary<string, IBaseHandler>(container.Resolve<IEnumerable<IBaseHandler>>()
                .ToDictionary(handler => handler.Key, handler => handler));
            var log = container.Resolve<ILog>();

            return new Service(handlers, log);
        }

        private readonly ConcurrentDictionary<string, IBaseHandler> handlers;
        private readonly ILog log;

        public Service(ConcurrentDictionary<string, IBaseHandler> handlers, ILog log)
        {
            this.handlers = handlers;
            this.log = log;
        }

        public async Task ProcessRequest(SemaphoreSlim semaphoreSlim, HttpListenerContext context)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var key = GetHandlerKey(context);
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

        private static string GetHandlerKey(HttpListenerContext context)
        {
            var method = new HttpMethod(context.Request.HttpMethod);
            return $"{method}{context.Request.Url.LocalPath}";
        }
    }
}