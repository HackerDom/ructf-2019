using MongoDB.Driver;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    class Storage : IStorage
    {
        private readonly IMongoCollection<Agent> agents;

        public Storage(Settings settings)
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

        public void UpdateAgent(Agent agent)
        {
            agents.ReplaceOne(origin => origin.AgentId == agent.AgentId, agent);
        }

        public void RemoveAgent(Agent agentToDelete)
        {
            agents.DeleteOne(origin => origin.AgentId == agentToDelete.AgentId);
        }
    }
}