using System;
using System.Threading.Tasks;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    public interface IStorage
    {
        SimulationAgent GetAgent(string agentId);
        void AddAgent(SimulationAgent simulationAgent);
    }
}