using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Storages
{
    public interface IAgentController
    {
        string MoveAgent(string agentId, MoveType moveType);
    }
}