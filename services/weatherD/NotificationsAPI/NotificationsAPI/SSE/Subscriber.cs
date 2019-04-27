using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NotificationsApi.Storage;

namespace NotificationsAPI.SSE
{
	internal class Subscriber
	{
		private readonly SourceStorage sourceStorage;
		private readonly SseClient sseClient;
		private readonly Authorizer authorizer;

		public Subscriber(Authorizer authorizer, SourceStorage sourceStorage, SseClient sseClient)
		{
			this.authorizer = authorizer;
			this.sourceStorage = sourceStorage;
			this.sseClient = sseClient;
		}

		public async Task<bool> Subscribe(string src, string token, HttpContext httpContext)
		{
			if(authorizer.CanSubscribe(src, token))
			{
				if(sourceStorage.TryGetInfo(src, out var res))
				{
					res.AddSubscriber(httpContext);
					await SendMessagesHistory(res.GetMessagesHistory(), httpContext);
					return true;
				}
			}

			return false;
		}

		public void Unsubscribe(string src, HttpContext httpContext)
		{
			if(sourceStorage.TryGetInfo(src, out var res))
			{
				res.RemoveSubscriber(httpContext);
			}
		}

		private async Task SendMessagesHistory(List<string> messages, HttpContext context)
		{
			foreach(var message in messages)
			{
				await sseClient.SendMessageAsync(context, message);
			}
		}
	}
}