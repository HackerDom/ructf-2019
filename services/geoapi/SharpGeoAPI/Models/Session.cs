using System;
using System.Linq;

namespace SharpGeoAPI.Models
{
    public class Session
    {
        public string SessionId { get; set; }

        public string Owner { get; set; }

        public bool IsValidSeed(Seed seed)
        {
            return seed.Vertices.All(Bound.Contain);
        }

        public Bound Bound;
        public DateTime CratedAt;
    }
}