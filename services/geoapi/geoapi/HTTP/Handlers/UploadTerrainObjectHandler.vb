Imports System
Imports System.Net
Imports System.Threading.Tasks
Imports geoapi.Models
Imports geoapi.Storages
Imports geoapi.Utils

Namespace HTTP.Handlers
    Public Class UploadTerrainObjectHandler
        Inherits BaseHandler

        Private ReadOnly agentStorage As IAgentStorage
        Private ReadOnly terrainObjectStore As ITerrainObjectStore
        Private ReadOnly settings As ISettings

        Public Sub New(ByVal agentStorage As IAgentStorage, ByVal terrainObjectStore As ITerrainObjectStore, ByVal settings As ISettings)
            MyBase.New("PUT", "object")
            Me.agentStorage = agentStorage
            Me.terrainObjectStore = terrainObjectStore
            Me.settings = settings
        End Sub

        Protected Overrides Async Function HandleRequestAsync(ByVal context As HttpListenerContext) As Task
            Dim content = Await context.Request.ReadContentAsync()
            Dim request = content.FromJson(Of UploadObjectRequest)()
            Dim agentInfo = agentStorage.GetAgent(request.AgentId)

            If agentInfo Is Nothing Then
                Await context.Response.Send(404, "Can't find session")
                Return
            End If

            Dim tObject = New TerrainObject(request.AgentId, GenerateId(settings.ObjectIdSize), DateTime.UtcNow + settings.TTL) With {
                    .Info = request.Info,
                    .Cells = request.Cells
                    }
            terrainObjectStore.UploadTerrainObject(tObject)
            Await context.Response.Send(200, tObject.IndexKey)
        End Function
    End Class
End Namespace
