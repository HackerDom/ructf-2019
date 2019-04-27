using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationsApi.Storage
{
	public class StoredMessage
	{
		public string Content;
		[BsonElement("expireAt")]
		public DateTime ExpireAt;

		public string SourceName;
		[BsonRepresentation(BsonType.ObjectId)]
		public ObjectId Id;

		public StoredMessage(string sourceName, string content, DateTime expireAt)
		{
			Id = ObjectId.GenerateNewId();
			Content = content;
			this.ExpireAt = expireAt;
			SourceName = sourceName;
		}
	}
}