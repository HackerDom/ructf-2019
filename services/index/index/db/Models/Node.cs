using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace index.db.Models
{
    public class Node : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Children")]
        public List<Node> Children { get; set; }

        public Node(string name)
        {
            Name = name;
            Children = new List<Node>();
        }
    }
}