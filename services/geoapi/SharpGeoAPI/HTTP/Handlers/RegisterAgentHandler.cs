using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Models;
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

            await context.Response.Send(200, "");

            var request = content.FromJson<RegisterAgentRequests>();

            var agent = new AgentInfo
            {
                AgentId = CreateNewAgentId(),
                AgentKey = request.AgentKey,
            };

            storage.AddAgent(agent);

            await context.Response.OutputStream.WriteAsync(agent.ToJson().ToBytes());
            context.Response.Close();
        }

        private class RegisterAgentRequests
        {
            public string AgentKey { get; set; }
        }

        private string CreateNewAgentId() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
