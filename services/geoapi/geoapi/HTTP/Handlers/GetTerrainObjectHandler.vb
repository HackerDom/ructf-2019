Imports System.Net
Imports System.Threading.Tasks
Imports geoapi.Storages
Imports geoapi.Utils

Namespace HTTP.Handlers
    Public Class GetTerrainObjectHandler
        Inherits BaseHandler

        Private ReadOnly agentStorage As IAgentStorage
        Private ReadOnly terrainObjectStore As ITerrainObjectStore

        Public Sub New(ByVal agentStorage As IAgentStorage, ByVal terrainObjectStore As ITerrainObjectStore)
            MyBase.New("GET", "object")
            Me.agentStorage = agentStorage
            Me.terrainObjectStore = terrainObjectStore
        End Sub

        Protected Overrides Async Function HandleRequestAsync(ByVal context As HttpListenerContext) As Task
            Dim objectKey = context.Request.QueryString(ObjectKeyParameter)
            Dim agentKey = context.Request.QueryString(AgentKeyParameter)
            Dim agent = agentStorage.GetAgent(agentKey)

            If agent Is Nothing Then
                Await context.Response.Send(404, "Agent not found")
                Return
            End If

            Dim tObject = terrainObjectStore.GetTerrainObject(objectKey)

            If tObject Is Nothing Then
                Await context.Response.Send(404, "Object not found")
                Return
            End If

            Await context.Response.Send(200, tObject.ToJson())
        End Function
    End Class
End Namespace
