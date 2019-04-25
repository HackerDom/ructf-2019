using MongoDB.Driver;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    class Storage : IStorage
    {
        private readonly IMongoCollection<AgentSession> agents;

        public Storage(ISettings settings)
        {
            var client = new MongoClient(settings.MongoDBConnectionString);
            var database = client.GetDatabase(settings.MongoDBName);
            agents = database.GetCollection<AgentSession>(settings.CollectionName);
        }

        public AgentSession GetAgent(string agentId)
        {
            return agents.Find(agent => agent.AgentId == agentId).FirstOrDefault();
        }

        public void AddAgent(AgentSession agentSession)
        {
            agents.InsertOne(agentSession);
        }

        public void UpdateAgent(AgentSession agentSession)
        {
            agents.ReplaceOne(origin => origin.AgentId == agentSession.AgentId, agentSession);
        }

        public void RemoveAgent(AgentSession agentSessionToDelete)
        {
            agents.DeleteOne(origin => origin.AgentId == agentSessionToDelete.AgentId);
        }
    }
}