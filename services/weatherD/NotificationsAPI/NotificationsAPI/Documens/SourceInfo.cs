using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace NotificationsApi.Documens
{
	internal class SourceInfo
	{
		private readonly HashSet<HttpContext> subscribers = new HashSet<HttpContext>();
		public readonly ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>();

		public void AddSubscriber(HttpContext context)
		{
			lock(subscribers)
			{
				subscribers.Add(context);
			}
		}

		public void AddMessage(Message message)
		{
			Messages.Enqueue(message);
		}

		public List<byte[]> GetMessagesHistory()
		{
			return Messages.Select(x => x.Content).ToList();
		}

		public List<HttpContext> GetSubscribers()
		{
			lock(subscribers)
			{
				return subscribers.ToList();
			}
		}
	}
}