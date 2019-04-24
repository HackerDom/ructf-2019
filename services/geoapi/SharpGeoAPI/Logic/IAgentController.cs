using SharpGeoAPI.Models.Geo;

namespace SharpGeoAPI.Storages
{
    public interface IAgentController
    {
        void ChangeTileType(string agentId, Vector2 position, CellType cellType);
    }
}