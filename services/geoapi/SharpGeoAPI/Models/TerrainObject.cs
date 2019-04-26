using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SharpGeoAPI.Models
{
    public class TerrainObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Key { get; set; }

        private readonly byte[,] cells;

        private TerrainObject(byte[,] cells)
        {
            this.cells = cells;
        }

        public string GetView()
        {
            throw new NotImplementedException();
        }

        public static byte[] Encode(TerrainObject terrainObject)
        {
            throw new NotImplementedException();
        }

        public static TerrainObject Decode(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}