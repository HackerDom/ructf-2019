using System.Net;
using System.Threading.Tasks;
using geoapi2.Storages;
using geoapi2.Utils;

namespace geoapi2.HTTP.Handlers
{
    public class GetAgentInfoHandler : BaseHandler
    {
        private static string QueryAgentParameter => "AgentKey";
        private readonly IAgentStorage agentStorage;

        public GetAgentInfoHandler(IAgentStorage agentStorage) : base("GET", "agent")
        {
            this.agentStorage = agentStorage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var key = context.Request.QueryString[QueryAgentParameter];

            var agent = agentStorage.GetAgent(key);

            if (agent == null)
            {
                await context.Response.Send(404, "Agent not found");
            }

            await context.Response.Send(200, agent.ToJson());
        }
    }
}