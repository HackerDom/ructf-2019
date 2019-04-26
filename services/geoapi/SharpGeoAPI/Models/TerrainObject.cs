using System;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SharpGeoAPI.Models
{
    public class TerrainObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Key { get; set; }

        public string Info { get; set; }
        public  byte[,] Cells { get; set; }

        private TerrainObject(byte[,] cells)
        {
            this.Cells = cells;
        }
    }
}