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
using SharpGeoAPI.Logic;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP
{
    public class Service
    {
        private static readonly ContainerBuilder ServiceBuilder;

        private ConcurrentDictionary<string, IBaseHandler> handlers;
        private ILog log;


        static Service()
        {
            ServiceBuilder = new ContainerBuilder();

            ServiceBuilder.RegisterInstance(LogManager.GetLogger(typeof(Service))).As<ILog>();
            RegisterHandlers();
            RegisterStorages();
        }

        private static void RegisterHandlers()
        {
            ServiceBuilder.RegisterType<AgentActionHandler>().As<IBaseHandler>();
            ServiceBuilder.RegisterType<GetAgentInfoHandler>().As<IBaseHandler>();
            ServiceBuilder.RegisterType<RegisterAgentHandler>().As<IBaseHandler>();
            ServiceBuilder.RegisterType<GetAgentInfoHandler>().As<IBaseHandler>();
        }

        private static void RegisterStorages()
        {
            ServiceBuilder.RegisterType<ChunkSaver>().As<IChunkSaver>().SingleInstance();
            ServiceBuilder.RegisterType<ChunkManager>().As<IChunkManager>().SingleInstance();
            ServiceBuilder.RegisterType<Storage>().As<IStorage>().SingleInstance();
        }

        public Service(Settings settings)
        {
            InitWithSettings(settings);
        }

        private void InitWithSettings(Settings settings)
        {
            ServiceBuilder.RegisterInstance(settings).As<ISettings>();
            var container = ServiceBuilder.Build();

            handlers = new ConcurrentDictionary<string, IBaseHandler>(container.Resolve<IEnumerable<IBaseHandler>>()
                .ToDictionary(handler => handler.Key, handler => handler));

            log = container.Resolve<ILog>();
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