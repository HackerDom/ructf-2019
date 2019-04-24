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
        private readonly IStorage storage;

        public RegisterAgentHandler(IStorage storage) : base("POST", "agent")
        {
            this.storage = storage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var agentKey = content.FromJson<string>();

            var agent = new Agent
            {
                AgentId = CreateNewAgentId(),
                AgentKey = agentKey,
            };

            storage.AddAgent(agent);

            await context.Response.OutputStream.WriteAsync(agent.ToJson().ToBytes());
            context.Response.Close();
        }


        private string CreateNewAgentId() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
