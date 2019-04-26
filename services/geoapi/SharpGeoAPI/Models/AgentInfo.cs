using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SharpGeoAPI.Models
{
    public class AgentInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AgentId { get; set; }

        public string AgentKey { get; set; }

        public string AgentName { get; set; }

        public int ChunkId { get; set; }
    }
}