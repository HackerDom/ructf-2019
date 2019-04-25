using System;
using System.IO;
using System.Threading.Tasks;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Logic;
using SharpGeoAPI.Models.Geo;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI
{

    class Program
    {
        static void Main(string[] args)
        {
            var settings = new Settings();
            using (var server = new HttpServer(settings))
            {
                Console.ReadLine();
            }
        }

    }
}
