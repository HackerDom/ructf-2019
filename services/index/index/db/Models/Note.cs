using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace index.db.Models
{
    public class Note : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("OwnerName")]
        public string OwnerName { get; set; }

        [BsonElement("Text")]
        public string Text { get; set; }

        [BsonElement("IsPublic")]
        public bool IsPublic { get; set; }
    }
}