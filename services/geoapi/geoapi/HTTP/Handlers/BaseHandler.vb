Imports System
Imports System.Net
Imports System.Security.Cryptography
Imports System.Threading.Tasks

Namespace HTTP.Handlers
    Public MustInherit Class BaseHandler
        Implements IBaseHandler

        Protected Shared ReadOnly Property ObjectKeyParameter As String
            Get
                Return "ObjectKey"
            End Get
        End Property

        Protected Shared ReadOnly Property AgentKeyParameter As String
            Get
                Return "AgentKey"
            End Get
        End Property

        Protected Shared ReadOnly Property SkipParameter As String
            Get
                Return "skip"
            End Get
        End Property

        Protected Shared ReadOnly Property TakeParameter As String
            Get
                Return "take"
            End Get
        End Property

        Protected Sub New(ByVal httpMethod As String, ByVal httpPath As String)
            Method = httpMethod
            Path = httpPath
        End Sub

        Protected Function GenerateId(ByVal size As Integer) As String
            Using rng As RandomNumberGenerator = New RNGCryptoServiceProvider()
                Dim tokenData As Byte() = New Byte(size - 1) {}
                rng.GetBytes(tokenData)
                Return Convert.ToBase64String(tokenData).TrimEnd("-"c).Replace("+"c, "-"c).Replace("/"c, "_"c)
            End Using
        End Function

        Protected MustOverride Function HandleRequestAsync(ByVal context As HttpListenerContext) As Task
        Public ReadOnly Method As String
        Public ReadOnly Path As String

        Public Async Function IBaseHandler_ProcessRequest(context As HttpListenerContext) As Task Implements IBaseHandler.ProcessRequest
            Await HandleRequestAsync(context)
            context.Response.Close()
        End Function

        Public ReadOnly Property Key As String Implements IBaseHandler.Key
            Get
                Return $"{Method}/{Path}"
            End Get
        End Property 
    End Class
End Namespace
