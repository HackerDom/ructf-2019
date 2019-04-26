using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GetAgentInfoHandler : BaseHandler
    {
        private readonly IStorage storage;

        public GetAgentInfoHandler(IStorage storage) : base("GET", "agent")
        {
            this.storage = storage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();
            var agentId = content.FromJson<string>();

            await context.Response.Send(200, "");

            var agent = storage.GetAgent(agentId);

            if (agent == null)
            {
                await context.Response.Send(404, agent.ToJson());
            }

            await context.Response.Send(200, agent.ToJson());

            context.Response.Close();
        }
    }
}