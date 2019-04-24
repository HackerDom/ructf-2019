using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Storages;

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
            var request = content.FromJson<GetAgentInfoRequest>();

            var seed = await agentStorage.GetAgent(request.AgentId);

            if (seed == null)
            {
                await context.Response.Send(404, seed.ToJson());
            }

            context.Response.Close();
        }


        private class GetAgentInfoRequest
        {
            public string AgentId { get; set; }
        }
    }
}