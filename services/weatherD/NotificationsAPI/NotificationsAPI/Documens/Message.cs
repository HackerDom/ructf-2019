using System;

namespace NotificationsApi.Documens
{
	public class Message
	{
		public readonly string Content;
		public DateTime ExpireAt;

		public Message(string content, DateTime expireAt)
		{
			Content = content;
			ExpireAt = expireAt;
		}
	}
}