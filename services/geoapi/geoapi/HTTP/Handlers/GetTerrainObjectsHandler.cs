using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Storages;
using SharpGeoAPI.Utils;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GetTerrainObjectsHandler : BaseHandler
    {
        private readonly IStorage storage;
        private readonly ITerrainObjectStore terrainObjectStore;

        public GetTerrainObjectsHandler(IStorage storage, ITerrainObjectStore terrainObjectStore) : base("GET", "objects")
        {
            this.storage = storage;
            this.terrainObjectStore = terrainObjectStore;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var agentKey = context.Request.QueryString[AgentKeyParameter];

            var agent = storage.GetAgent(agentKey);

            if (agent == null)
            {
                await context.Response.Send(404, "Object not found");
                return;
            }

            var terrainObjects = terrainObjectStore.GetTerrainObjects(agentKey);

            await context.Response.Send(200, terrainObjects.ToJson());
        }
    }
}