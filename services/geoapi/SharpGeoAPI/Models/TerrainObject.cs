using System;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SharpGeoAPI.Models
{
    public class TerrainObject
    {

        [JsonIgnore]
        public ObjectId Id { get; set; }

        public string IndexKey { get; }

        public string Info { get; set; }
        public  byte[,] Cells { get; set; }

        public TerrainObject(string agentKey, string objectKey)
        {
            IndexKey = agentKey + objectKey;
        }
    }
}