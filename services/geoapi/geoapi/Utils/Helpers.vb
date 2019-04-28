Imports System.IO
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json

Namespace Utils
    Module Helpers
        <Extension()>
        Async Function ReadContentAsync(ByVal request As HttpListenerRequest) As Task(Of String)
            Return Await request.InputStream.ReadToEndAsync()
        End Function

        <Extension()>
        Async Function Send(ByVal response As HttpListenerResponse, ByVal statusCode As Integer, ByVal message As String) As Task
            response.StatusCode = statusCode
            Await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(message))
        End Function

        <Extension()>
        Function ToJson(Of T)(ByVal source As T) As String
            Return JsonConvert.SerializeObject(source)
        End Function

        <Extension()>
        Function FromJson(Of T)(ByVal source As String) As T
            Return JsonConvert.DeserializeObject(Of T)(source, New JsonSerializerSettings() With {
                                                          .TypeNameHandling = TypeNameHandling.All
                                                          })
        End Function

        <Extension()>
        Async Function ReadToEndAsync(ByVal stream As Stream) As Task(Of String)
            Using reader As StreamReader = New StreamReader(stream, Encoding.UTF8)
                Return Await reader.ReadToEndAsync()
            End Using
        End Function
    End Module
End Namespace
