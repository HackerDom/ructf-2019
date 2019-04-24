using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Models;
using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Storages
{
    class AgentsStorage : IAgentStorage
    {
        private readonly IMongoCollection<Agent> agents;

        
        public AgentsStorage(Settings settings, string dbCollection)
        {
            var client = new MongoClient(settings.MongoDBConnectionString);
            var database = client.GetDatabase(settings.MongoDBName);
            agents = database.GetCollection<Agent>(settings.CollectionName);
        }

        public Agent GetAgent(string agentId)
        {
            return agents.Find(agent => agent.AgentId == agentId).FirstOrDefault();
        }

        public void AddAgent(Agent agent)
        {
            agents.InsertOne(agent);
        }

        public void RemoveAgent(Agent agentToDelete)
        {
            agents.DeleteOne(a => a.AgentId == agentToDelete.AgentId);
        }
    }

}