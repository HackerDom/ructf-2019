using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using SharpGeoAPI.Logic;
using SharpGeoAPI.Models;
using SharpGeoAPI.Models.Geo;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class AgentActionHandler : BaseHandler
    {
        private readonly IStorage storage;
        private readonly Settings settings;
        private readonly ConcurrentDictionary<string, ActionHandler> handlers = new ConcurrentDictionary<string, ActionHandler>();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<ActionRequest>> actionBatches = new ConcurrentDictionary<string, ConcurrentQueue<ActionRequest>>();

        public AgentActionHandler(IStorage storage, Settings settings) : base("PUT", "action")
        {
            this.storage = storage;
            this.settings = settings;
            //Task.Run(RemoveExpiredHandlers);
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var request = content.FromJson<ActionRequest>();

            var agent = storage.GetAgent(request.SessionId);

            if (agent == null)
            {
                await context.Response.Send(404, "Can't find session");
                return;
            }

            var handler = handlers.GetOrAdd(agent.AgentKey, new ActionHandler(settings));

            if (!handler.TryAddAction(async () => await HandleAction(request)))
            {
                context.Response.Send(423);
            }

            context.Response.Send(200);
        }

        private Task HandleAction(ActionRequest actionRequest)
        {
            throw new NotImplementedException();
        }

        private async Task RemoveExpiredHandlers()
        {
            while (true)
            {
                try
                {
                    var expired =  handlers.Where(pair => pair.Value.IsExpired).ToList();
                    await Task.WhenAll(expired.Select(pair => pair.Value.Stop().ContinueWith(task =>
                    {
                        handlers.TryRemove(pair.Key, out _);
                    })).ToArray());
                }
                catch (Exception e)
                {

                }
            }
        }



        private class ActionRequest
        {
            public string SessionId { get; set; }
            public CellType Cell { get; set; }
        }
    }
}