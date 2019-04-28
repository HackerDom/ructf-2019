using System;
using System.Net;
using System.Threading.Tasks;
using geoapi2.Models;
using geoapi2.Storages;
using geoapi2.Utils;

namespace geoapi2.HTTP.Handlers
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

            var agent = new AgentInfo(GenerateId(settings.AgentIdSize), request.AgentName,
                DateTime.UtcNow + settings.TTL);

            agentStorage.AddAgent(agent);

            await context.Response.Send(200, agent.ToJson());
            context.Response.Close();
        }
    }
}
