using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace indexReact.db.Models
{
    public class IndexEntity : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("User")]
        public string User { get; set; }

        [BsonElement("Hash")]
        public Dictionary<string, List<string>> Hash { get; set; }
    }
}