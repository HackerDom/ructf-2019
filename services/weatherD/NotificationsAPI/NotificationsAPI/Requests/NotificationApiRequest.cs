using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace NotificationsApi.Requests
{
	internal class NotificationApiRequest
	{
		public byte[] Message { get; private set; }
		public string SourceName { get; private set; }
		public string Password { get; private set; }
		public string Token { get; private set; }
		public long Time { get; private set; }
		public bool IsPublic { get; private set; }
		[JsonIgnore]
		public HttpContext HttpContext { get; private set; }

		public static NotificationApiRequest CreateFromQueryCollection(string body, HttpContext context)
		{
			var result = JsonConvert.DeserializeObject<NotificationApiRequest>(body);
			result.HttpContext = context;
			return result;
		}
	}
}