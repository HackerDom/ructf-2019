using System;
using System.Threading.Tasks;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    public interface IAgentStorage
    {
        Agent GetAgent(string agentId);
        void AddAgent(Agent agent);
    }
}