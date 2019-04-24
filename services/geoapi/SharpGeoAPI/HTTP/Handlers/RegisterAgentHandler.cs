using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Models;
using SharpGeoAPI.Models.Geo;
using SharpGeoAPI.Storages;

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

            var agentKey = content.FromJson<string>();

            var agent = new Agent
            {
                AgentId = CreateNewAgentId(),
                AgentKey = agentKey,
                Position = GetStartPosition()
            };

            await agentStorage.AddAgent(agent);

            await context.Response.OutputStream.WriteAsync(agent.ToJson().ToBytes());
            context.Response.Close();
        }


        //TODO: remove this shit
        private string CreateNewAgentId() => Guid.NewGuid().ToString();

        private Vector2 GetStartPosition() => throw new NotImplementedException();
    }
}
