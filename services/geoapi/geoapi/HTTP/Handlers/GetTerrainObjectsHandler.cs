using System.Net;
using System.Threading.Tasks;
using geoapi.Storages;
using geoapi.Utils;

namespace geoapi.HTTP.Handlers
{
    public class GetTerrainObjectsHandler : BaseHandler
    {
        private readonly IAgentStorage agentStorage;
        private readonly ITerrainObjectStore terrainObjectStore;

        public GetTerrainObjectsHandler(IAgentStorage agentStorage, ITerrainObjectStore terrainObjectStore) : base("GET","objects")
        {
            this.agentStorage = agentStorage;
            this.terrainObjectStore = terrainObjectStore;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var agentKey = context.Request.QueryString[AgentKeyParameter];
            
            var agent = agentStorage.GetAgent(agentKey);
            if (agent == null)
            {
                await context.Response.Send(404, "Object not found");
                return;
            }

            if (int.TryParse(context.Request.QueryString[SkipParameter], out var skip) || skip < 0)
            {
                await context.Response.Send(400, $"{SkipParameter} must be integer and greater than 0");
                return;
            }

            if (int.TryParse(context.Request.QueryString[TakeParameter], out var take) || take < 0)
            {
                await context.Response.Send(400, $"{TakeParameter} must be integer and greater than 0");
                return;
            }

            var terrainObjects = terrainObjectStore.GetTerrainObjects(agentKey, skip, take);

            await context.Response.Send(200, terrainObjects.ToJson());
        }
    }
}