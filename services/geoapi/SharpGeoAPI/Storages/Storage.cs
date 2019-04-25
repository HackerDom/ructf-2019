using MongoDB.Driver;
using SharpGeoAPI.HTTP;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    class Storage : IStorage
    {
        private readonly IMongoCollection<SimulationAgent> agents;

        public Storage(Settings settings)
        {
            var client = new MongoClient(settings.MongoDBConnectionString);
            var database = client.GetDatabase(settings.MongoDBName);
            agents = database.GetCollection<SimulationAgent>(settings.CollectionName);
        }

        public SimulationAgent GetAgent(string agentId)
        {
            return agents.Find(agent => agent.AgentId == agentId).FirstOrDefault();
        }

        public void AddAgent(SimulationAgent simulationAgent)
        {
            agents.InsertOne(simulationAgent);
        }

        public void UpdateAgent(SimulationAgent simulationAgent)
        {
            agents.ReplaceOne(origin => origin.AgentId == simulationAgent.AgentId, simulationAgent);
        }

        public void RemoveAgent(SimulationAgent simulationAgentToDelete)
        {
            agents.DeleteOne(origin => origin.AgentId == simulationAgentToDelete.AgentId);
        }


    }
}