using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Models
{
    public class AgentSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AgentId { get; set; }

        public string AgentKey { get; set; }

        public string AgentName { get; set; }

        public int ChunkId { get; set; }
    }
}