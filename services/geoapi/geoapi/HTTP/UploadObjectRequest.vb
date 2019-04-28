Imports geoapi.Models
Imports log4net

Namespace HTTP
    Public Class UploadObjectRequest
        Public Property AgentId As String
        Public Property Cells As CellTypes(,)
        Public Property Info As String

        Public Sub New(ByVal agentId As String, ByVal cells As CellTypes(,), ByVal info As String, ByVal Optional loggerProvider As ILoggerProvider = Nothing)
            Me.AgentId = agentId
            Me.Cells = cells
            Me.Info = info
            Dim log = If(loggerProvider?.GetLog(), LogManager.GetLogger([GetType]()))
            log.Info("Recived upload object request")
        End Sub
    End Class
End Namespace
