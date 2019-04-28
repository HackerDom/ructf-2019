Imports System
Imports System.Net
Imports System.Threading.Tasks
Imports geoapi.Models
Imports geoapi.Storages
Imports geoapi.Utils

Namespace HTTP.Handlers
    Public Class RegisterAgentHandler
        Inherits BaseHandler

        Private ReadOnly agentStorage As IAgentStorage
        Private ReadOnly settings As ISettings

        Public Sub New(ByVal agentStorage As IAgentStorage, ByVal settings As ISettings)
            MyBase.New("POST", "agent")
            Me.agentStorage = agentStorage
            Me.settings = settings
        End Sub

        Protected Overrides Async Function HandleRequestAsync(ByVal context As HttpListenerContext) As Task
            Dim content = Await context.Request.ReadContentAsync()
            Dim request = content.FromJson(Of RegisterAgentRequests)()
            Dim agent = New AgentInfo(GenerateId(settings.AgentIdSize), request.AgentName, DateTime.UtcNow + settings.TTL)
            agentStorage.AddAgent(agent)
            Await context.Response.Send(200, agent.ToJson())
            context.Response.Close()
        End Function
    End Class
End Namespace
