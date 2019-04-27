using System;
using System.Net;
using Newtonsoft.Json;

namespace NotificationsAPI
{
	[JsonObject]
	public class Settings
	{
		public TimeSpan dataTtl = TimeSpan.FromMinutes(20);
		public string MongoConnectionString = "mongodb://localhost:27017";
	}
}