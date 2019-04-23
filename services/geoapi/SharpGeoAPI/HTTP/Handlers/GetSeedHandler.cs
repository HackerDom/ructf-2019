using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GetSeedHandler : BaseHandler
    {
        private readonly IStorage storage;

        public GetSeedHandler(IStorage storage) : base("GET", "seed")
        {
            this.storage = storage;
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContent();
            var request = content.FromJson<GetSeedRequest>();

            var seed = await storage.GetSeed(request.SessionId, request.SeedName);

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