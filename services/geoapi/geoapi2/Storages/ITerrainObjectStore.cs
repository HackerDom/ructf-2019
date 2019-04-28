using System.Collections.Generic;
using geoapi2.Models;

namespace geoapi2.Storages
{
    public interface ITerrainObjectStore
    {
        TerrainObject GetTerrainObject(string key);
        void UploadTerrainObject(TerrainObject terrainObject);
        IEnumerable<TerrainObject> GetTerrainObjects(string agentToken, int skip, int take);
    }
}