using System.Net;
using System.Threading.Tasks;
using geoapi.Models;
using geoapi.Storages;
using geoapi.Utils;

namespace geoapi.HTTP.Handlers
{
    public class RegisterAgentHandler : BaseHandler
    {
        private readonly IStorage storage;
        private readonly ISettings settings;

        public RegisterAgentHandler(IStorage storage, ISettings settings) : base("POST", "agent")
        {
            this.storage = storage;
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

            storage.AddAgent(agent);

            await context.Response.Send(200, agent.ToJson());
            context.Response.Close();
        }

        private class RegisterAgentRequests
        {
            public string AgentName { get; set; }
        }

    }
}
