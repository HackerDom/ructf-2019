using System;
using System.Linq;
using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Models
{
    public class Agent
    {
        public string AgentId { get; set; }

        public string AgentKey { get; set; }

        public Vector2 Position { get; set; }
    }
}