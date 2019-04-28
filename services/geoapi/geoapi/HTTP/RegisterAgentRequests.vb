Imports log4net

Namespace HTTP
    Public Class RegisterAgentRequests
        Public Property AgentName As String

        Public Sub New(ByVal agentName As String, ByVal Optional loggerProvider As ILoggerProvider = Nothing)
            Me.AgentName = agentName
            Dim log = If(loggerProvider?.GetLog(), LogManager.GetLogger([GetType]()))
            log.Info("Recived upload object request")
        End Sub
    End Class
End Namespace
