using System;

namespace NotificationsApi.Documens
{
	public class Message
	{
		public readonly byte[] Content;
		public DateTime ExpireAt;

		public Message(byte[] content, DateTime expireAt)
		{
			Content = content;
			ExpireAt = expireAt;
		}
	}
}