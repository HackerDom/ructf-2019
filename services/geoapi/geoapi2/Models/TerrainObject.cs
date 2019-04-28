using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace geoapi2.Models
{
    public class TerrainObject
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }

        public string IndexKey { get; set; }

        public string Info { get; set; }
        public  CellTypes[,] Cells { get; set; }

        [JsonIgnore]
        [BsonElement("expireAt")]
        public DateTime ExpireAt { get; }

        public TerrainObject(string agentKey, string objectKey, DateTime expireAt)
        {
            ExpireAt = expireAt;
            IndexKey = agentKey + objectKey;
        }
    }
}