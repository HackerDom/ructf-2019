using System.Net;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP.Handlers
{
    public class GenerateSurfaceHandler : BaseHandler
    {
        public GenerateSurfaceHandler() : base("GET", "surface")
        {
        }

        protected override async Task HandleRequestAsync(HttpListenerContext context)
        {

            await context.Response.OutputStream.WriteAsync("adasdasd".ToJson().ToBytes());
            context.Response.Close();
        }
    }
}