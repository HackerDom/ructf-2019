Imports System.Net
Imports System.Threading.Tasks
Imports geoapi.Storages
Imports geoapi.Utils

Namespace HTTP.Handlers
    Public Class GetTerrainObjectsHandler
        Inherits BaseHandler

        Private ReadOnly agentStorage As IAgentStorage
        Private ReadOnly terrainObjectStore As ITerrainObjectStore

        Public Sub New(ByVal agentStorage As IAgentStorage, ByVal terrainObjectStore As ITerrainObjectStore)
            MyBase.New("GET", "objects")
            Me.agentStorage = agentStorage
            Me.terrainObjectStore = terrainObjectStore
        End Sub

        Protected Overrides Async Function HandleRequestAsync(ByVal context As HttpListenerContext) As Task
            Dim agentKey = context.Request.QueryString(AgentKeyParameter)
            Dim agent = agentStorage.GetAgent(agentKey)

            If agent Is Nothing Then
                Await context.Response.Send(404, "Object not found")
                Return
            End If


            Dim skip As Integer = Nothing

            If Integer.TryParse(context.Request.QueryString(SkipParameter), skip) And skip < 0 Then
                Await context.Response.Send(400, $"{SkipParameter} must be integer and greater than 0")
                Return
            End If

            Dim take As Integer = Nothing

            If Integer.TryParse(context.Request.QueryString(TakeParameter), take) And take < 0 Then
                Await context.Response.Send(400, $"{TakeParameter} must be integer and greater than 0")
                Return
            End If

            Dim terrainObjects = terrainObjectStore.GetTerrainObjects(agentKey, skip, take)
            Await context.Response.Send(200, terrainObjects.ToJson())
        End Function
    End Class
End Namespace
