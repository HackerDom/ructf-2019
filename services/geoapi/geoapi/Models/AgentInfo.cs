using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace geoapi.Models
{
    public class AgentInfo
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }

        public string AgentToken { get; set; }

        public string AgentName { get; set; }

        [JsonIgnore]
        [BsonElement("expireAt")]
        public DateTime ExpireAt;

        public AgentInfo(string agentToken, string agentName, DateTime expireAt)
        {
            AgentToken = agentToken;
            AgentName = agentName;
            ExpireAt = expireAt;
        }
    }
}