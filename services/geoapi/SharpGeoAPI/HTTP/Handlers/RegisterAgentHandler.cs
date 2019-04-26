using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SharpGeoAPI.Models;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
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

            await context.Response.Send(200, "");

            var request = content.FromJson<RegisterAgentRequests>();

            var agent = new AgentInfo
            {
                AgentToken = GetAgentToken(),
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

        private string GetAgentToken()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[settings.AgentIdSize];
                rng.GetBytes(tokenData);

                return Convert.ToBase64String(tokenData);
            }
        }
    }
}
