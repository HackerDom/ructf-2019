using System;

namespace SharpGeoAPI.Models
{
    public class Seed
    {
        public string Id => $"{SessionId}{SeedName}";
        public string SeedName { get; set; }
        public  string SessionId { get; set; }
        public Point[] Vertices { get; set; }

        public string EncodeSeed(byte[] bytes)
        {
            throw new NotImplementedException();
        }

    }
}