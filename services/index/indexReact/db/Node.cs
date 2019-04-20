using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace indexReact.db
{
    public class Node : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Children")]
        public Node[] Children { get; set; }
    }
}