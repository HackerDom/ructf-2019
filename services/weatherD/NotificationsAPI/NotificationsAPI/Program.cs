using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.DependencyInjection;
using NotificationsApi;
using NotificationsApi.Handlers;
using NotificationsApi.Storage;
using NotificationsAPI.SSE;

namespace NotificationsAPI
{
	class Program
	{
		static unsafe Guid g(string s)
		{
			var b = Encoding.ASCII.GetBytes(s);
			fixed(byte* c = b)
				return *(Guid*)c;
		}
		static unsafe string g(Guid s)
		{
			return new string(s.ToByteArray().Select(x => (char)x).ToArray());
		}

		static void Main(string[] args)
		{
			var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
			XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
			var log = log4net.LogManager.GetLogger(typeof(Program));

			var mongoDbClient = new MongoDbClient();
			var ( authorizer, sourceStorage) = new StateRestorer(mongoDbClient).Restore().GetAwaiter().GetResult();

			var addUserInfoHandler = new AddSourceInfoHandler(mongoDbClient, authorizer, sourceStorage, log);
			var sseClient = new SSEClient();
			var subscriber = new Subscriber(authorizer, sourceStorage, sseClient);
			var messagSender = new MessageSender();
			var sendMessageHandler = new SendMessageHandler(messagSender, mongoDbClient, sourceStorage, authorizer);
			var handlerMapper = new HandlerMapper();

			var expDaemon = new ExpirationDaemon(sourceStorage);

			handlerMapper.Add("/addUserInfo", HttpMethod.Post, addUserInfoHandler);
			handlerMapper.Add("/subscribe", HttpMethod.Get, new SubscribeOnSourceHandler(subscriber));
			handlerMapper.Add("/sendMessage", HttpMethod.Post, sendMessageHandler);

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
					options.Listen(IPAddress.Loopback, 5000);

				})
				.Build();

			host.Run();
		}
	}
}
