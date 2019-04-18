using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace NotificationsApi.Requests
{
	internal class NotificationApiRequest
	{
		private const string SourceParamName = "source";
		private const string TokenParamName = "token";
		private const string MessageParamName = "message";
		private const string TimeParamName = "time";
		private const string PasswordParamName = "password";
		public byte[] Message { get; private set; }
		public string SourceName { get; private set; }
		public string Password { get; private set; }
		public string Token { get; private set; }
		public long Time { get; private set; }
		public HttpContext HttpContext { get; private set; }

		public static NotificationApiRequest CreateFromQueryCollection(IQueryCollection options, HttpContext context)
		{
			string source = null;
			string token = null;
			string password = null;
			byte[] message = null;
			var time = 0L;

			if(options.ContainsKey(SourceParamName))
				source = options[SourceParamName];

			if(options.ContainsKey(TokenParamName))
				token = options[TokenParamName];

			if(options.ContainsKey(TimeParamName))
				time = long.Parse(options[TimeParamName]);

			if(options.ContainsKey(PasswordParamName))
				password = options[PasswordParamName];

			if(options.ContainsKey(MessageParamName))
			{
				string hex = options[MessageParamName];
				message = Enumerable.Range(0, hex.Length)
					.Where(x => x % 2 == 0)
					.Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
					.ToArray();
			}

			return new NotificationApiRequest
			{
				Message = message,
				Token = token,
				SourceName = source,
				HttpContext = context,
				Time = time,
				Password = password
			};
		}
	}
}