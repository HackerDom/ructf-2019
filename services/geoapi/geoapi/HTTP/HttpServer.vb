Imports System
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports geoapi.Utils
Imports log4net

Namespace HTTP
    Public Class HttpServer
        Implements IDisposable, ILoggerProvider

        Private ReadOnly log As ILog
        Private service As Service
        Private settings As Settings
        Private serverThread As Thread
        Private listener As HttpListener

        Public Sub New(ByVal settings As Settings)
            Me.settings = settings
            Me.log = ILoggerProvider_GetLog()
            service = Service.BuildWithService(settings)
            serverThread = New Thread(AddressOf Listen)
            serverThread.Start()
        End Sub

        Private Sub Listen()
            listener = New HttpListener()

            listener.Prefixes.Add("http://*:" & settings.Port & "/")
            listener.Start()

            Using semaphore = New SemaphoreSlim(settings.ParallelismDegree, settings.ParallelismDegree)

                While True
                    Dim context = listener.GetContext()
                    Task.Run(Function()  service.ProcessRequest(semaphore, context))
                End While
            End Using
        End Sub

        Public Sub IDisposable_Dispose() Implements IDisposable.Dispose
            listener.[Stop]()
        End Sub

        Public Function ILoggerProvider_GetLog() As ILog Implements ILoggerProvider.GetLog
            Return LogManager.GetLogger([GetType]())
        End Function
    End Class
End Namespace
