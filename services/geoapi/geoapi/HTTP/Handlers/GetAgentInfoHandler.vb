Imports System.Net
Imports System.Threading.Tasks
Imports geoapi.Storages
Imports geoapi.Utils

Namespace HTTP.Handlers
    Public Class GetAgentInfoHandler
        Inherits BaseHandler

        Private Shared ReadOnly Property QueryAgentParameter As String
            Get
                Return "AgentKey"
            End Get
        End Property

        Private ReadOnly agentStorage As IAgentStorage

        Public Sub New(ByVal agentStorage As IAgentStorage)
            MyBase.New("GET", "agent")
            Me.agentStorage = agentStorage
        End Sub

        Protected Overrides Async Function HandleRequestAsync(ByVal context As HttpListenerContext) As Task
            Dim key = context.Request.QueryString(QueryAgentParameter)
            Dim agent = agentStorage.GetAgent(key)

            If agent Is Nothing Then
                Await context.Response.Send(404, "Agent not found")
            End If

            Await context.Response.Send(200, agent.ToJson())
        End Function
    End Class
End Namespace
