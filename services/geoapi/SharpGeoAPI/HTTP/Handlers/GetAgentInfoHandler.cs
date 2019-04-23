using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GetAgentInfoHandler : BaseHandler
    {
        private readonly IAgentStorage agentStorage;

        public GetAgentInfoHandler(IAgentStorage agentStorage) : base("GET", "agent")
        {
            this.agentStorage = agentStorage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();
            var request = content.FromJson<GetSeedRequest>();

            var seed = await agentStorage.GetSeed(request.SessionId, request.SeedName);

            if (seed == null)
            {
                await context.Response.Send(404, seed.ToJson());
            }

            context.Response.Close();
        }


        private class GetSeedRequest
        {
            public string SessionId { get; set; }
            public string SeedName { get; set; }
        }
    }
}