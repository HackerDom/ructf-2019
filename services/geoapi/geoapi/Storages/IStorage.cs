using geoapi.Models;

namespace geoapi.Storages
{
    public interface IStorage
    {
        AgentInfo GetAgent(string agentId);
        void AddAgent(AgentInfo agentInfo);
    }
}