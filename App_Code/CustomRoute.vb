Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Routing
Imports System.Web.Compilation
Imports System.Web.UI

Public Class CustomRouteHandler : Implements IRouteHandler

    Private _virtualPath As String

    Public Sub New(ByVal vPath As String)
        _virtualPath = vPath
    End Sub

    Public Property VirtualPath() As String
        Get
            Return _virtualPath
        End Get
        Private Set(ByVal value As String)
            _virtualPath = value
        End Set
    End Property

    Public Function GetHttpHandler(ByVal requestContext As System.Web.Routing.RequestContext) _
          As System.Web.IHttpHandler Implements System.Web.Routing.IRouteHandler.GetHttpHandler
        Dim redirectPage As IHttpHandler

        redirectPage = BuildManager.CreateInstanceFromVirtualPath(VirtualPath, GetType(Page))
        Return redirectPage
    End Function

End Class

