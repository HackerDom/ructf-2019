using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NotificationsApi;
using NotificationsApi.Documens;
//using NotificationsApi;
using NotificationsApi.Handlers;
using NotificationsApi.Storage;
using NotificationsAPI.SSE;

namespace NotificationsAPI
{
	class Program
	{
		static void Main(string[] args)
		{
			var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
			XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
			var log = log4net.LogManager.GetLogger(typeof(Program));
			var settings = GetSettings();
			var mongoDbClient = new MongoDbClient(settings.MongoConnectionString, TimeSpan.FromSeconds(2));
			ThreadPool.SetMaxThreads(32767, 32767);
			ThreadPool.SetMinThreads(2048, 2048); 
		//mongoDbClient.InsertMessage("ab", new Message("a", DateTime.UtcNow + TimeSpan.FromSeconds(2))).GetAwaiter().GetResult();
		//	var a = mongoDbClient.GetAllMessages().GetAwaiter().GetResult();
		//	Thread.Sleep(TimeSpan.FromSeconds(30));
		//	var b = mongoDbClient.GetAllMessages().GetAwaiter().GetResult();
			var (authorizer, sourceStorage) = new StateRestorer(mongoDbClient).Restore().GetAwaiter().GetResult();

			var addUserInfoHandler = new AddSourceInfoHandler(mongoDbClient, authorizer, sourceStorage, log);
			var sseClient = new SseClient();
			var subscriber = new Subscriber(authorizer, sourceStorage, sseClient);
			var messagSender = new MessageSender();
			var sendMessageHandler = new SendMessageHandler(messagSender, mongoDbClient, sourceStorage, authorizer, settings.dataTtl);
			var handlerMapper = new HandlerMapper();

			var expDaemon = new ExpirationDaemon(sourceStorage, settings.dataTtl);

			handlerMapper.Add("/addUserInfo", HttpMethod.Post, addUserInfoHandler);
			handlerMapper.Add("/subscribe", HttpMethod.Get, new SubscribeOnSourceHandler(subscriber));
			handlerMapper.Add("/sendMessage", HttpMethod.Post, sendMessageHandler);
			//sourceStorage.Add("123");
			//authorizer.RegisterPublic("123", "123");
			//sourceStorage.TryGetInfo("123", out var info);
			//info.AddMessage(new Message("bla bla", DateTime.MaxValue));
			//messagSender.Send("bla bla", info);

			var routingHandler = new RoutingHandler(handlerMapper, log);

			var host = WebHost.CreateDefaultBuilder(args)
				.ConfigureServices(s =>
				{
					s.AddSingleton(routingHandler);
					s.AddHttpContextAccessor();
				})
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseKestrel()
				.UseStartup<Startup>()
				.ConfigureKestrel((context, options) =>
				{
					//options.Listen(new IPAddress(new byte[] { 10, 33, 54, 120 }), 5000);
					options.Listen(new IPAddress(new byte[] { 0, 0, 0, 0 }), 5000);
				})
				.Build();

			host.Run();
		}

		private static Settings GetSettings()
		{
			var filepath = "../../../../settings";
			if(!File.Exists(filepath))
				return new Settings();
			var readText = File.ReadAllText(filepath);
			return JsonConvert.DeserializeObject<Settings>(readText);
		}
	}
}
