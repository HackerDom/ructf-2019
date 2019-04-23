using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using NotificationsApi.Handlers;
using NotificationsApi.Requests;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace NotificationsAPI
{
	public class Startup
	{
		private readonly RoutingHandler handler;
		private readonly IHttpContextAccessor a;
		public Startup(RoutingHandler handler, IHttpContextAccessor a)
		{
			this.handler = handler;
			this.a = a;
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env,
			ILoggerFactory loggerFactory)
		{
			app.Run(async (context) =>
			{
				var b = a.HttpContext;
				string body;
				using(StreamReader reader = new StreamReader(b.Request.Body, Encoding.UTF8, true, 1024, true))
				{
					body = reader.ReadToEnd();
				}

				b.Request.Body.Position = 0;
				var notificationApiRequest = NotificationApiRequest.CreateFromQueryCollection(body, b);
				var method = b.Request.Method == "GET" ? HttpMethod.Get : HttpMethod.Post; 
				Console.WriteLine(b.Request.Method);
				await handler.HandleAsync(notificationApiRequest, b.Request.Path, method);
			});
		}
	}
}