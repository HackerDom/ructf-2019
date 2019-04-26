using System;
using System.Collections.Generic;
using SharpGeoAPI.HTTP;

//simple sploite str = @"{""$type"": ""SharpGeoAPI.HTTP.HttpServer, SharpGeoAPI"", ""Settings"": {""ParallelismDegree"":8,""Port"":9008,""MongoDBConnectionString"":""mongodb://localhost:27017"",""CollectionName"":""geoAPIDB"",""MongoDBName"":""AgentsDB"",""ActionHandlerLifeTime"":""00:00:10"",""ActionQueueMaxSize"":10,""ChunkStorageDirectory"":""C:\\Users\\d.lukshto\\source\\ructf-2019\\ructf-2019\\services\\geoapi\\SharpGeoAPI\\bin\\Debug\\netcoreapp2.2"",""ChunkStorageCapacity"":10,""ChunkSize"":4096,""ChunkStorageExpirationTime"":""00:01:00"",""ChunkWidth"":0,""ChunkHeight"":0}}"
//dotnet publish --configuration Release --self-contained true --runtime linux-arm

namespace SharpGeoAPI
{
    public class Test
    {
        public Test()
        {
            Console.WriteLine("yarr");
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var settings = new Settings();

            var str = "[{'$type': 'SharpGeoAPI.Test, SharpGeoAPI' }]".FromJson<object>();

            using (var server = new HttpServer(settings))
            {
                Console.WriteLine("Service started");
                Console.ReadLine();
            }
        }

    }
}
