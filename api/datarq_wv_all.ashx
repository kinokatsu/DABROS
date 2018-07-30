<%@ WebHandler Language="VB" Class="datarq_wv_all" %>
'' 鵜泊用に作成した、非同期で要求がきた場合にCSVを返すハンドラー　全チャンネル(列)用
Option Explicit On
Option Strict On
''Imports System
''Imports System.Web
Imports System.Data
'' 波形データのCSVファイル作成用（全測点データ対応版）

Public Class datarq_wv_all : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim currentContext As HttpContext = HttpContext.Current
        Dim retString As String = Nothing
        Dim csvDate As String = DirectCast(currentContext.Request.QueryString("dt"), String)

        Try
            Dim siteDirectory As String = context.Session("SD").ToString
            Dim csvFilePath As String = context.Session("csvpath").ToString

            Try
                Dim getCsv As New ClsCSVData
                csvFilePath = IO.Path.Combine(csvFilePath, (csvDate & ".csv"))
                retString = getCsv.ReadDataFromCSVUdomariStrAll(csvFilePath)

            Catch ex As Exception
                retString = "Error"

            End Try

            context.Response.ContentType = "application/csv"

            If retString.StartsWith("Error") = True Then
                context.Response.StatusCode = Net.HttpStatusCode.InternalServerError
            Else
                context.Response.StatusCode = Net.HttpStatusCode.OK
            End If
            context.Response.Charset = "shift-jis"      '"UTF-8"
            context.Response.Write(retString)

        Catch ex As Exception

            context.Response.Write("Error")
            context.Response.Charset = "shift-jis"      '"UTF-8"            
            context.Response.Status = "500"
            context.Response.StatusCode = Net.HttpStatusCode.InternalServerError

        End Try

        context.Response.Flush()
        context.Response.SuppressContent = True                             ' 余計なごみ(HTML関連)がついてしまうのを回避する　Flushと一緒に利用

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class