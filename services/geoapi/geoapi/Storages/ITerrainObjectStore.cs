using System.Collections.Generic;
using SharpGeoAPI.Models;

namespace SharpGeoAPI.Storages
{
    public interface ITerrainObjectStore
    {
        TerrainObject GetTerrainObject(string objectId);
        void UploadTerrainObject(string agentName, string objectId, TerrainObject terrainObject);
        IEnumerable<TerrainObject> GetTerrainObjects(string ownerIndex);
    }
}