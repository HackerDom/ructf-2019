using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Logic;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GetChunkHandler : BaseHandler
    {
        private readonly IStorage storage;
        private readonly IChunkManager chunkManager;

        public GetChunkHandler(IStorage storage, IChunkManager chunkManager) : base("GET", "agent")
        {
            this.storage = storage;
            this.chunkManager = chunkManager;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();
            var agentId = content.FromJson<string>();

            var agent = storage.GetAgent(agentId);

            if (agent == null)
            {
                await context.Response.Send(404, agent.ToJson());
                return;
            }

            var chunk = chunkManager.GetChunk(agent.AgentName);
            await chunk.HandleSnapshot(async bytes =>  await context.Response.Send(200, agent.ToJson()));

            context.Response.Close();
        }
    }
}