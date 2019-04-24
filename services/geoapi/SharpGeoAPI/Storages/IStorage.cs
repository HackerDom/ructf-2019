using System;
using System.Threading.Tasks;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    public interface IStorage
    {
        Agent GetAgent(string agentId);
        void AddAgent(Agent agent);
    }
}