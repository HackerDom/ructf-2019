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
        private readonly IChunkManager chunkManager;
        private readonly ISettings settings;


        public AgentActionHandler(IStorage storage, IChunkManager chunkManager, ISettings settings) : base("PUT", "action")
        {
            this.storage = storage;
            this.chunkManager = chunkManager;
            this.settings = settings;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var request = content.FromJson<ActionRequest>();

            var session = storage.GetAgent(request.SessionId);

            if (session == null)
            {
                await context.Response.Send(404, "Can't find session");
                return;
            }

            context.Response.Send(200);
        }

        private async Task HandleAction(ActionRequest actionRequest, AgentSession session)
        {
            await UpdateTile(actionRequest, session);
        }

        private async Task UpdateTile(ActionRequest actionRequest, AgentSession session)
        {
            var chunk = chunkManager.GetChunk(session.AgentName);
            chunk.TrySet(new Vector2(0, 0), actionRequest.Cell);
            await chunk.Update();
        }

        private class ActionRequest
        {
            public string SessionId { get; set; }
            public CellType Cell { get; set; }
        }
    }
}
 