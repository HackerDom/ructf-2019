using System.Net;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class StartNewSessionHandler : BaseHandler
    {
        public StartNewSessionHandler() : base("POST", "session")
        {
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var content = await context.Request.ReadContent();

            await context.Response.OutputStream.WriteAsync("adasdasd".ToJson().ToBytes());
            context.Response.Close();
        }

        private class StartNewSessionRequest
        {
        }
    }
}
