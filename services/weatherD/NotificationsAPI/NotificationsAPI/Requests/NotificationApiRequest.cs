using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace NotificationsApi.Requests
{
    [JsonObject]
	internal class NotificationApiRequest
	{
		private const string SourceParamName = "source";
		private const string TokenParamName = "token";
		private const string MessageParamName = "message";
		private const string TimeParamName = "time";
		private const string PasswordParamName = "password";
		private const string IsPublicParamName = "isPublic";
	    public string Base64Message;
	    public string source;
	    public string password;
	    public string Token;
	    public long timestamp;
	    public bool IsPublic;
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
				source = source,
				HttpContext = context,
				timestamp = time,
				password = password,
				IsPublic = isPublic
			};
		}
	}
}