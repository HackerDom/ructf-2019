using System;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace index
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SetupThreadPool();

            CreateWebHostBuilder(args).Build().Run();
        }

        private static void SetupThreadPool()
        {
            var threads = Math.Min(Environment.ProcessorCount * 128, short.MaxValue);

            ThreadPool.SetMaxThreads(short.MaxValue, short.MaxValue);
            ThreadPool.SetMinThreads(threads, threads);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseUrls("http://*:80").UseStartup<Startup>();
    }
}