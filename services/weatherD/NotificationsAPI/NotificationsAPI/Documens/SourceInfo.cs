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

		public void RemoveSubscriber(HttpContext context)
		{
			lock(subscribers)
			{
				subscribers.Remove(context);
			}
		}

		public bool AddMessage(Message message)
		{
			if(Messages.Count > 4)
				return false;

			Messages.Enqueue(message);
			return true;
		}

		public List<string> GetMessagesHistory()
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