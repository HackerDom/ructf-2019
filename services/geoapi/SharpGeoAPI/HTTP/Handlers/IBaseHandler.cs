using System.Net;
using System.Threading.Tasks;

namespace SharpGeoAPI.HTTP.Handlers
{
    internal interface IBaseHandler
    {
        Task ProcessRequest(HttpListenerContext context);
        string Key { get; }
    }
}