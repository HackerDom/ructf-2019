using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Models;
using SharpGeoAPI.Models.Geo;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class AgentActionHandler : BaseHandler
    {
        private readonly IStorage storage;

        public AgentActionHandler(IStorage storage) : base("PUT", "action")
        {
            this.storage = storage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContentAsync();

            var request = content.FromJson<PutSeedRequest>();

            var session = storage.GetAgent(request.SessionId);

            if (session == null)
            {
                await context.Response.Send(404, "Can't find session");
                return;
            }

            //await sessionStorage.PutSeed(request.SessionId, request.Seed);

            context.Response.Send(200);

        }

        private class PutSeedRequest
        {
            public string SessionId { get; set; }
            public CellType Cell { get; set; }
        }
    }
}