Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports Autofac
Imports geoapi.HTTP.Handlers
Imports geoapi.Storages
Imports geoapi.Utils
Imports log4net

Namespace HTTP
    Public Class Service
        Public Shared Function BuildWithService(ByVal settings As Settings) As Service
            Dim serviceBuilder = New ContainerBuilder()
            serviceBuilder.RegisterInstance(settings).[As](Of ISettings)()
            serviceBuilder.RegisterInstance(LogManager.GetLogger(GetType(Service))).[As](Of ILog)()
            serviceBuilder.RegisterType(Of UploadTerrainObjectHandler)().[As](Of IBaseHandler)()
            serviceBuilder.RegisterType(Of GetAgentInfoHandler)().[As](Of IBaseHandler)()
            serviceBuilder.RegisterType(Of GetTerrainObjectsHandler)().[As](Of IBaseHandler)()
            serviceBuilder.RegisterType(Of GetTerrainObjectHandler)().[As](Of IBaseHandler)()
            serviceBuilder.RegisterType(Of RegisterAgentHandler)().[As](Of IBaseHandler)()
            serviceBuilder.RegisterType(Of TerrainObjectStore)().[As](Of ITerrainObjectStore)()
            serviceBuilder.RegisterType(Of AgentStorage)().[As](Of IAgentStorage)()
            Dim container = serviceBuilder.Build()
            Dim handlers = New ConcurrentDictionary(Of String, IBaseHandler)(container.Resolve(Of IEnumerable(Of IBaseHandler))().ToDictionary(Function(handler) handler.Key, Function(handler) handler))
            Dim log = container.Resolve(Of ILog)()
            Return New Service(handlers, log)
        End Function

        Private ReadOnly handlers As ConcurrentDictionary(Of String, IBaseHandler)
        Private ReadOnly log As ILog

        Public Sub New(ByVal handlers As ConcurrentDictionary(Of String, IBaseHandler), ByVal log As ILog)
            Me.handlers = handlers
            Me.log = log
        End Sub

        Public Async Function ProcessRequest(ByVal semaphoreSlim As SemaphoreSlim, ByVal context As HttpListenerContext) As Task
            Await semaphoreSlim.WaitAsync()

            Try
                Dim key = GetHandlerKey(context)

                If handlers.ContainsKey(key) Then
                    Await handlers(key).ProcessRequest(context)
                End If

            Catch e As Exception
                log.[Error]($"Can't process {context.Request.HttpMethod} {context.Request.Url}")
                context.Response.Send(500, "Unexpected error")
            Finally
                semaphoreSlim.Release()
            End Try

            context.Response.Close()
        End Function

        Private Shared Function GetHandlerKey(ByVal context As HttpListenerContext) As String
            Dim method = New HttpMethod(context.Request.HttpMethod)
            Return $"{method}{context.Request.Url.LocalPath}"
        End Function
    End Class
End Namespace
