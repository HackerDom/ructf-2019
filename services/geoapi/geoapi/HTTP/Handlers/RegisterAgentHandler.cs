using System.Net;
using System.Threading.Tasks;
using geoapi.Models;
using geoapi.Storages;
using geoapi.Utils;

namespace geoapi.HTTP.Handlers
{
    public class RegisterAgentHandler : BaseHandler
    {
        private readonly IAgentStorage agentStorage;
        private readonly ISettings settings;

        public RegisterAgentHandler(IAgentStorage agentStorage, ISettings settings) : base("POST", "agent")
        {
            this.agentStorage = agentStorage;
            this.settings = settings;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var request = content.FromJson<RegisterAgentRequests>();

            var agent = new AgentInfo
            {
                AgentToken = GenerateId(settings.AgentIdSize),
                AgentName = request.AgentName,
            };

            agentStorage.AddAgent(agent);

            await context.Response.Send(200, agent.ToJson());
            context.Response.Close();
        }
    }
}
