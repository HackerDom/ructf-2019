Imports System.Threading
Imports geoapi.HTTP
Imports geoapi.Utils
Imports log4net


Public Interface ILoggerProvider
    Function GetLog() As ILog
End Interface

Public Class Program
    Public Shared Sub Main(ByVal args As String())
        Dim settings = New Settings()
        ThreadPool.SetMaxThreads(32767, 32767)
        ThreadPool.SetMinThreads(2048, 2048)

        Using server = New HttpServer(settings)
            Thread.Sleep(-1)
        End Using
    End Sub
End Class