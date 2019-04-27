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
		public IPAddress ipAddress = new IPAddress(new byte[] { 10, 33, 54, 120 });
	}
}