using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SharpGeoAPI
{
    public static class Helpers
    {
        public static async Task<string> ReadContentAsync(this HttpListenerRequest request)
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

        public static string ToJson<T>(this T source)
        {
            return JsonConvert.SerializeObject(source);
        }

        public static T FromJson<T>(this string source)
        {
            return JsonConvert.DeserializeObject<T>(source, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static byte[] ToBytes(this string source)
        {
            return Encoding.UTF8.GetBytes(source);
        }

        public static async Task<string> ReadToEndAsync(this Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}