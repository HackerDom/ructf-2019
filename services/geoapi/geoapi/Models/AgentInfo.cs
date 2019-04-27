using MongoDB.Bson;
using Newtonsoft.Json;

namespace geoapi.Models
{
    public class AgentInfo
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }

        public string AgentToken { get; set; }

        public string AgentName { get; set; }
    }
}