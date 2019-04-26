using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GetTerrainObjectHandler : BaseHandler
    {
        private readonly IStorage storage;
        private readonly ITerrainObjectStore terrainObjectStore;

        public GetTerrainObjectHandler(IStorage storage, ITerrainObjectStore terrainObjectStore) : base("GET", "chunk")
        {
            this.storage = storage;
            this.terrainObjectStore = terrainObjectStore;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();
            var request = content.FromJson<GetTerrainObjectRequest>();

            await context.Response.Send(200, "");

            var agent = storage.GetAgent(request.AgentId);

            if (agent == null)
            {
                await context.Response.Send(404, agent.ToJson());
                return;
            }

            var tObject = terrainObjectStore.GetTerrainObject(request.AgentId,request.ObjectId);

            await context.Response.Send(200, tObject.GetView());
        }

        private class GetTerrainObjectRequest
        {
            public string AgentId { get; set; }
            public string ObjectId { get; set; }
        }
    }
}