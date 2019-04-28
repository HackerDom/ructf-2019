
Imports geoapi.Models

Namespace Storages
    Public Interface IAgentStorage
        Function GetAgent(ByVal agentId As String) As AgentInfo
        Sub AddAgent(ByVal agentInfo As AgentInfo)
    End Interface
End Namespace
