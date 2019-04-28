using System.Net;
using System.Threading.Tasks;
using geoapi2.Storages;
using geoapi2.Utils;

namespace geoapi2.HTTP.Handlers
{
    public class GetTerrainObjectHandler : BaseHandler
    {

        private readonly IAgentStorage agentStorage;
        private readonly ITerrainObjectStore terrainObjectStore;

        public GetTerrainObjectHandler(IAgentStorage agentStorage, ITerrainObjectStore terrainObjectStore) : base("GET", "object")
        {
            this.agentStorage = agentStorage;
            this.terrainObjectStore = terrainObjectStore;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var objectKey = context.Request.QueryString[ObjectKeyParameter];

            var agentKey = context.Request.QueryString[AgentKeyParameter];

            var agent = agentStorage.GetAgent(agentKey);

            if (agent == null)
            {
                await context.Response.Send(404, "Agent not found");
                return;
            }

            var tObject = terrainObjectStore.GetTerrainObject(objectKey);

            if (tObject == null)
            {
                await context.Response.Send(404, "Object not found");
                return;
            }

            await context.Response.Send(200, tObject.ToJson());
        }
    }
}