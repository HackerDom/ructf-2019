using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace NotificationsApi.Requests
{
	internal class NotificationApiRequest
	{
		private const string SourceParamName = "source";
		private const string TokenParamName = "token";
		private const string MessageParamName = "message";
		private const string TimeParamName = "time";
		private const string PasswordParamName = "password";
		private const string IsPublicParamName = "isPublic";
		public string Base64Message { get; private set; }
		public string SourceName { get; private set; }
		public string Password { get; private set; }
		public string Token { get; private set; }
		public long Time { get; private set; }
		public bool IsPublic { get; private set; }
		[JsonIgnore]
		public HttpContext HttpContext { get; private set; }

		public static NotificationApiRequest CreateFromBody(string body, HttpContext context)
		{
			var result = JsonConvert.DeserializeObject<NotificationApiRequest>(body);
			result.HttpContext = context;
			return result;
		}

		public static NotificationApiRequest CreateFromQueryCollection(IQueryCollection options, HttpContext context)
		{
			string source = null;
			string token = null;
			string password = null;
			string message = null;
			var time = 0L;
			var isPublic = false;

			if(options.ContainsKey(SourceParamName))
				source = options[SourceParamName];

			if(options.ContainsKey(TokenParamName))
				token = options[TokenParamName];

			if(options.ContainsKey(TimeParamName))
				time = long.Parse(options[TimeParamName]);

			if(options.ContainsKey(PasswordParamName))
				password = options[PasswordParamName];

			if(options.ContainsKey(IsPublicParamName))
				isPublic = options[IsPublicParamName] == "true";

			if(options.ContainsKey(MessageParamName))
			{
				message = options[MessageParamName];
			}

			return new NotificationApiRequest
			{
				Base64Message = message,
				Token = token,
				SourceName = source,
				HttpContext = context,
				Time = time,
				Password = password,
				IsPublic = isPublic
			};
		}
	}
}