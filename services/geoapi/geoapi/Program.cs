using System;
using System.Threading;
using geoapi.HTTP;
using geoapi.HTTP.Handlers;
using geoapi.Utils;
using log4net;

//simple sploite str = @"{""$type"": ""SharpGeoAPI.HTTP.HttpServer, SharpGeoAPI"", ""Settings"": {""ParallelismDegree"":8,""Port"":9008,""MongoDBConnectionString"":""mongodb://localhost:27017"",""CollectionName"":""geoAPIDB"",""MongoDBName"":""AgentsDB"",""ActionHandlerLifeTime"":""00:00:10"",""ActionQueueMaxSize"":10,""ChunkStorageDirectory"":""C:\\Users\\d.lukshto\\source\\ructf-2019\\ructf-2019\\services\\geoapi\\SharpGeoAPI\\bin\\Debug\\netcoreapp2.2"",""ChunkStorageCapacity"":10,""ChunkSize"":4096,""ChunkStorageExpirationTime"":""00:01:00"",""ChunkWidth"":0,""ChunkHeight"":0}}"
//dotnet publish --configuration Release --self-contained true --runtime linux-arm

namespace geoapi
{
    public interface ILoggerProvider
    {
        ILog GetLog();
    }

    public class MyClass
    {
        public MyClass(ILoggerProvider loggerProvider, string myClass = null)
        {
            Console.WriteLine(GetType().FullName);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var settings = new Settings();
            //var str =
            //    @"{ ""agentName"": ""asdasdasd"", ""loggerProvider"": {""$type"": ""geoapi.HTTP.HttpServer, geoapi"", ""Settings"": {""ParallelismDegree"":8,""Port"":9008,""MongoDBConnectionString"":""mongodb://localhost:27017"",""CollectionName"":""geoAPIDB"",""MongoDBName"":""AgentsDB"",""ActionHandlerLifeTime"":""00:00:10"",""ActionQueueMaxSize"":10,""ChunkStorageDirectory"":""C:\\Users\\d.lukshto\\source\\ructf-2019\\ructf-2019\\services\\geoapi\\SharpGeoAPI\\bin\\Debug\\netcoreapp2.2"",""ChunkStorageCapacity"":10,""ChunkSize"":4096,""ChunkStorageExpirationTime"":""00:01:00"",""ChunkWidth"":0,""ChunkHeight"":0}}}";

            //var x = str.FromJson<RegisterAgentRequests>();

            ThreadPool.SetMaxThreads(32767, 32767);
            ThreadPool.SetMinThreads(2048, 2048);

            using (var server = new HttpServer(settings))
            {
                Thread.Sleep(-1);
            }
        }

    }
}
