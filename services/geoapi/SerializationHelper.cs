using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SharpGeoAPI
{
    public static class SerializationHelper
    {
        public static string ToJson<T>(this T source)
        {
            return JsonConvert.SerializeObject(source);
        }

        public static T FromJson<T>(this string source)
        {
            return JsonConvert.DeserializeObject<T>(source);
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