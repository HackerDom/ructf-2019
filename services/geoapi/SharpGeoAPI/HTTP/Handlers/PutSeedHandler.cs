using System.Net;
using System.Threading.Tasks;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class PutSeedHandler : BaseHandler
    {
        private readonly IStorage storage;

        public PutSeedHandler(IStorage storage) : base("PUT", "seed")
        {
            this.storage = storage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContent();

            var request = content.FromJson<PutSeedRequest>();

            var session = await storage.GetSession(request.SessionId);

            if (session == null)
            {
                await context.Response.Send(404, "Can't find session");
                return;
            }

            if (!session.IsValidSeed(request.Seed))
            {
                await context.Response.Send(400, "Seed is not valid");
                return;
            }

            await storage.PutSeed(request.SessionId, request.Seed);

            context.Response.Send(200);

        }

        private class PutSeedRequest
        {
            public string SessionId { get; set; }
            public Seed Seed { get; set; }
        }
    }
}