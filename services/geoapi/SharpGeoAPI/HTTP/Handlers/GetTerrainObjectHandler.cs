using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Storages;
using SharpGeoAPI.Utils;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GetTerrainObjectHandler : BaseHandler
    {

        private readonly IStorage storage;
        private readonly ITerrainObjectStore terrainObjectStore;

        public GetTerrainObjectHandler(IStorage storage, ITerrainObjectStore terrainObjectStore) : base("GET", "object")
        {
            this.storage = storage;
            this.terrainObjectStore = terrainObjectStore;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var objectKey = context.Request.QueryString[ObjectKeyParameter];

            var agentKey = context.Request.QueryString[AgentKeyParameter];

            var agent = storage.GetAgent(agentKey);

            if (agent == null)
            {
                await context.Response.Send(404, "Object not found");
                return;
            }

            var tObject = terrainObjectStore.GetTerrainObject(objectKey);

            await context.Response.Send(200, tObject.ToJson());
        }
    }
}