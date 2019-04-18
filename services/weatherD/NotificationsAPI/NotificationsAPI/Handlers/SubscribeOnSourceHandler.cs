using System.Net;
using System.Threading.Tasks;
using NotificationsApi.Requests;
using NotificationsAPI.SSE;

namespace NotificationsApi.Handlers
{
	internal class SubscribeOnSourceHandler : INotificationApiHandler
	{
		private readonly Subscriber subscriber;
		public SubscribeOnSourceHandler(Subscriber subscriber)
		{
			this.subscriber = subscriber;
		}

		public async Task HandleAsync(NotificationApiRequest request)
		{
			request.HttpContext.Response.Headers.Add("Content-Type", "text/event-stream");
			request.HttpContext.Response.Headers.Add("Keep-alive", "true");
			request.HttpContext.Response.ContentType = "text/event-stream";

			if(await subscriber.Subscribe(request.SourceName, request.Token, request.HttpContext))
			{
				//request.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
			}
			else
				request.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

			var cts = new TaskCompletionSource<byte>();
			request.HttpContext.RequestAborted.Register(() => cts.TrySetResult(1));
			await cts.Task;
		}
	}
}