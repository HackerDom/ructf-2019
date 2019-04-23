using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using LiteDB;
using System.ServiceModel;
using log4net;
using log4net.Core;
using Newtonsoft.Json;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.HTTP.Handlers;

namespace SharpGeoAPI
{

    class Program
    {
        static void Main(string[] args)
        {
            var storage = new AgentsStorage();

            var server1 = new HttpService(
                new Settings
                {
                    Port = 9008,
                    ParallelismDegree = 10
                }, 
                new List<IBaseHandler>() {new RegisterAgentHandler()},
                storage,
                LogManager.GetLogger(typeof(HttpService)));


            var settings = new Settings
            {
                Port = 9007,
                ParallelismDegree = 10
            };

            var c = new List<IBaseHandler>() {new RegisterAgentHandler()}; 


            var a = JsonConvert.SerializeObject(new HttpService(settings,c, storage,
                LogManager.GetLogger(typeof(HttpService))), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });


            var b = JsonConvert.DeserializeObject<List<object>>("[{'$type':'SharpGeoAPI.Chunk, SharpGeoAPI', 'a':'sasdasd'}]", 
            new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            new Process()
            {
                StartInfo = new ProcessStartInfo("cmd", "/ccalc")
            };



            //Console.WriteLine(b.GetType());

            //server.Start();
        }
    }
}
