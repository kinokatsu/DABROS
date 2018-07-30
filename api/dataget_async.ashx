<%@ WebHandler Language="VB" Class="dataget_async" %>
'' 風向風速計関連で、ブラウザから非同期でデータ要求がきたときに応答を返すためのハンドラー

'Imports System
'Imports System.Web
Imports System.Data
Imports System.Xml.Serialization
Imports Newtonsoft.Json
Imports System.io

Public Class dataget_async : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        
        Dim StrRet As String = "Bad Request"
        Dim CodeRet As Integer = 400
        Dim currentContext As HttpContext = HttpContext.Current
        Dim LoginStatus As Integer = Convert.ToInt32(context.Session("LgSt"))
        Dim dataType As String = Convert.ToString(currentContext.Request.QueryString("ty")).ToUpper
        Dim TypeExtension As String = Nothing
        context.Response.ContentType = "text/plain"
        
        If dataType = "J" Then
            TypeExtension = ".json"
        Else
            TypeExtension = ".xml"
        End If
        
        If LoginStatus = 1 Then
            Dim siteDirectory As String = context.Session("SD").ToString
            Dim ReadFile As String = context.Server.MapPath(siteDirectory & "\App_Data\" & siteDirectory & "_NewestData" & TypeExtension)

            If File.Exists(ReadFile) = True Then
                Dim enc As System.Text.Encoding = System.Text.Encoding.UTF8
                Dim DataString As String
                Dim sr As New StreamReader(ReadFile, enc)
                DataString = sr.ReadToEnd()
                sr.Close()
                
                StrRet = DataString
                
                CodeRet = 200
            Else
                
                StrRet = "Not Found"
                CodeRet = 404
            End If

        Else
            
            StrRet = "Unauthorized"
            CodeRet = 401

        End If

        context.Response.Write(strRet)
        context.Response.StatusCode = CodeRet

        context.Response.Flush()
        context.Response.SuppressContent = True                             ' 余計なごみ(HTML関連)がついてしまうのを回避する　Flushと一緒に利用        
        
    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class