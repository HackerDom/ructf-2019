using System;
using System.Linq;
using NotificationsApi.Documens;
using NotificationsAPI.SSE;

namespace NotificationsApi.Storage
{
	internal class MessageSender
	{
		private readonly SseClient sseClient;
		private readonly object lockObj = new object();

		public MessageSender()
		{
			sseClient = new SseClient();
		}

		public void Send(string message, SourceInfo info)
		{
			var subscribers = info.GetSubscribers();
			if(subscribers.Count == 0)
				return;

			lock(lockObj)
			{
				subscribers
					.AsParallel()
					.WithDegreeOfParallelism(Environment.ProcessorCount)
					.ForAll(async x => await sseClient.SendMessageAsync(x, message));
			}
		}
	}
}