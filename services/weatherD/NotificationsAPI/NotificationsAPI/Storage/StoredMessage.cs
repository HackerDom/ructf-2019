using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationsApi.Storage
{
	public class StoredMessage
	{
		public string Content;
		[BsonElement("expireAt")]
		public DateTime expireAt;

		public string SourceName;
		[BsonRepresentation(BsonType.ObjectId)]
		public ObjectId Id;

		public StoredMessage(string sourceName, string content, DateTime expireAt)
		{
			Id = ObjectId.GenerateNewId();
			Content = content;
			this.expireAt = expireAt;
			SourceName = sourceName;
		}
	}
}