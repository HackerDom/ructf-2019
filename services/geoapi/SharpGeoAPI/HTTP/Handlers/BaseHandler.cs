using System.Net;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP.Handlers
{
    public abstract class BaseHandler : IBaseHandler
    {
        protected BaseHandler(string httpMethod, string httpPath)
        {
            Method = httpMethod;
            Path = httpPath;
        }

        public async Task ProcessRequest(HttpListenerContext context)
        {
            await HandleRequestAsync(context);
            context.Response.Close();
        }
  

        protected abstract Task HandleRequestAsync(HttpListenerContext context);

        public readonly string Method;

        public readonly string Path;
        public string Key => $"{Method}/{Path}";
    }
}