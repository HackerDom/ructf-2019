using geoapi.Models;

namespace geoapi.Storages
{
    public interface IAgentStorage
    {
        AgentInfo GetAgent(string agentId);
        void AddAgent(AgentInfo agentInfo);
    }
}