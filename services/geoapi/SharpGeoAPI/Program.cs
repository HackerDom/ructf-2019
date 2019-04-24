using System;
using System.IO;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI
{

    class Program
    {
        static void Main(string[] args)
        {
            var storage = new AgentsStorage();

            /*var server1 = new HttpService(
                new Settings
                {
                    Port = 9008,
                    ParallelismDegree = 10
                }, 
                new List<IBaseHandler>() {new RegisterAgentHandler(storage)},
                storage,
                LogManager.GetLogger(typeof(HttpService)));

    */
            // var path = Path.Combine("C:\\Users\\d.lukshto\\source\\ructf-2019\\ructf-2019\\services\\geoapi\\SharpGeoAPI\\bin\\Debug\\netcoreapp2.2", "/a", "/b", "", "\\asdasd.txt");

            // Console.WriteLine(path);
            // Console.WriteLine(File.Exists(path));
        }
    }
}
