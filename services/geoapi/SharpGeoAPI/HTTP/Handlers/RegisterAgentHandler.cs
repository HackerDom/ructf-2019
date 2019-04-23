using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class RegisterAgentHandler : BaseHandler
    {
        private readonly IAgentStorage agentStorage;

        public RegisterAgentHandler(IAgentStorage agentStorage) : base("POST", "agent")
        {
            this.agentStorage = agentStorage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var request = content.FromJson<AgentRegistrationRequest>();

            var agent = new Agent
            {
                AgentId = CreateNewAgentId(),
                AgentKey = request.AgentKey,
                Position = GetStartPosition()
            };

            await agentStorage.AddAgent(agent);

            await context.Response.OutputStream.WriteAsync(agent.ToJson().ToBytes());
            context.Response.Close();
        }


        //TODO: remove this shit
        private string CreateNewAgentId() => Guid.NewGuid().ToString();

        private Vector2 GetStartPosition() => throw new NotImplementedException();

        private class AgentRegistrationRequest
        {
            public string AgentKey { get; set; }
        }

    }
}
