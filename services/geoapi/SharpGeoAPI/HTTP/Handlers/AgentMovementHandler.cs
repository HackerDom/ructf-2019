using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Models.Geo;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class AgentMovementHandler : BaseHandler
    {
        private readonly IAgentStorage agentStorage;
        private readonly IAgentController agentsController;

        public AgentMovementHandler(IAgentStorage agentStorage, IAgentController agentsController) : base("PUT", "move")
        {
            this.agentStorage = agentStorage;
            this.agentsController = agentsController;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var moveRequest = content.FromJson<MoveRequest>();

            var agent = agentStorage.GetAgent(moveRequest.AgentId);

            if (agent == null)
            {
                await context.Response.Send(404, "Session not found");
                return;
            }

            agentsController.MoveAgent(agent.AgentId, moveRequest.MoveType);
        }

        private class MoveRequest
        {
            public string AgentId;

            public MoveType MoveType;
        }
    }
}