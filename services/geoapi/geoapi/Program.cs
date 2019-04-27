using System.Threading;
using geoapi.HTTP;
using geoapi.Utils;
using log4net;


namespace geoapi
{
    public interface ILoggerProvider
    {
        ILog GetLog();
    }
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new Settings();

            ThreadPool.SetMaxThreads(32767, 32767);
            ThreadPool.SetMinThreads(2048, 2048);

            using (var server = new HttpServer(settings))
            {
                Thread.Sleep(-1);
            }
        }

    }
}
