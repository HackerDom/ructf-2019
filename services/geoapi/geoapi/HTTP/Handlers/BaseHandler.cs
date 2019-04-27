using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP.Handlers
{
    public abstract class BaseHandler : IBaseHandler
    {
        protected static string ObjectKeyParameter => "ObjectKey";
        protected static string AgentKeyParameter => "AgentKey";
        protected static string SkipParameter => "skip";
        protected static string TakeParameter => "take";


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


        protected string GenerateId(int size)
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[size];
                rng.GetBytes(tokenData);

                return  Convert.ToBase64String(tokenData);
            }
        }

        protected abstract Task HandleRequestAsync(HttpListenerContext context);

        public readonly string Method;

        public readonly string Path;
        public string Key => $"{Method}/{Path}";
    }
}