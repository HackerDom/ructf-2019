using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationsApi.Documens
{
    public class SourceShortedData
    {
	    [BsonElement]
		public string Source;
	    [BsonElement]
		public string Token;
	    [BsonElement]
	    public string Password;
		[BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id;
        public SourceShortedData(string source, string token, string password)
        {
            Id = ObjectId.GenerateNewId();
            Source = source;
            Token = token;
	        Password = password;
        }
    }
}
