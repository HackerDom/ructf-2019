using System.Net;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class MovementHandler : BaseHandler
    {
        private readonly IAgentStorage agentStorage;

        public MovementHandler(IAgentStorage agentStorage) : base("PUT", "move")
        {
            this.agentStorage = agentStorage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var request = content.FromJson<ChunkCreationRequest>();



            context.Response.Close();
        }

        private class ChunkCreationRequest
        {
            public string SessionId;

        }
    }
}