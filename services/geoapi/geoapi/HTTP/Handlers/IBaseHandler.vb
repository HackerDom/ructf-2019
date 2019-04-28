Imports System.Net
Imports System.Threading.Tasks

Namespace HTTP.Handlers
    Public Interface IBaseHandler
        Function ProcessRequest(ByVal context As HttpListenerContext) As Task
        ReadOnly Property Key As String
    End Interface
End Namespace
