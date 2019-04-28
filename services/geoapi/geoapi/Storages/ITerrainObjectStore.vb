Imports System.Collections.Generic
Imports geoapi.Models

Namespace Storages
    Public Interface ITerrainObjectStore
        Function GetTerrainObject(ByVal key As String) As TerrainObject
        Sub UploadTerrainObject(ByVal terrainObject As TerrainObject)
        Function GetTerrainObjects(ByVal agentToken As String, ByVal skip As Integer, ByVal take As Integer) As IEnumerable(Of TerrainObject)
    End Interface
End Namespace
