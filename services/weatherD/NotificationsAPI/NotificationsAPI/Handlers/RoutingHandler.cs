using System.Net;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using NotificationsApi.Requests;

namespace NotificationsApi.Handlers
{
	public class RoutingHandler
	{
		private readonly HandlerMapper handlerMapper;
		private readonly ILog log;

		internal RoutingHandler(HandlerMapper handlerMapper, ILog log)
		{
			this.handlerMapper = handlerMapper;
			this.log = log;
		}

		internal async Task HandleAsync(NotificationApiRequest request, string path, HttpMethod method)
		{
			log.Info($"Starting process request {path}");

			var handler = handlerMapper.Get(path, method);
			if(handler != null)
			{
				await handler.HandleAsync(request);
				return;
			}

			request.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		}
	}


}