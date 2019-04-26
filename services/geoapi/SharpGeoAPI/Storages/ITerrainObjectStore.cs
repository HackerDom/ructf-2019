using System.Collections.Generic;
using System.Threading.Tasks;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    public interface ITerrainObjectStore
    {
        TerrainObject GetTerrainObject(string agentName, string objectId);
        void UploadTerrainObject(string agentName, string objectId, TerrainObject terrainObject);
        IEnumerable<TerrainObject> GetTerrainObject(string agentName);
    }
}