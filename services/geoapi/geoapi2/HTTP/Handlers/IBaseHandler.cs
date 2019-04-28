using System.Net;
using System.Threading.Tasks;

namespace geoapi2.HTTP.Handlers
{
    public interface IBaseHandler
    {
        Task ProcessRequest(HttpListenerContext context);
        string Key { get; }
    }
}