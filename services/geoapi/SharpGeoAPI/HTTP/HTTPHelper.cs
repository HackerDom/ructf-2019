using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP
{
    static class HTTPHelper
    {
        public static async Task<string> ReadContent(this HttpListenerRequest request)
        {
            return await request.InputStream.ReadToEndAsync();
        }

        public static async Task Send(this HttpListenerResponse response, int statusCode, string message)
        {
            response.StatusCode = statusCode;
            await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(message));
        }

        public static void Send(this HttpListenerResponse response, int statusCode)
        {
            response.StatusCode = statusCode;
        }
    }
}