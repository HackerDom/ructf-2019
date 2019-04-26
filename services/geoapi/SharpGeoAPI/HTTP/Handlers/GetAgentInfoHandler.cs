using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GetAgentInfoHandler : BaseHandler
    {
        private static string QueryAgentParameter => "AgentKey";
        private readonly IStorage storage;

        public GetAgentInfoHandler(IStorage storage) : base("GET", "agent")
        {
            this.storage = storage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var key = context.Request.QueryString[QueryAgentParameter];

            var agent = storage.GetAgent(key);

            if (agent == null)
            {
                await context.Response.Send(404, "Agent not found");
            }

            await context.Response.Send(200, agent.ToJson());
        }
    }
}