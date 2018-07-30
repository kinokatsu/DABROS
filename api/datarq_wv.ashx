<%@ WebHandler Language="VB" Class="datarq_wv" %>
'' 鵜泊用に作成した、非同期で要求がきた場合にCSVを返すハンドラー　指定チャンネル(列)用
Option Explicit On
Option Strict On
''Imports System
''Imports System.Web
''Imports System.Data
''Imports System.Xml.Serialization
''Imports Newtonsoft.Json
'' 波形データのCSVファイル作成用（指定測点データのみ版）

'' 鵜泊トンネル向け、非同期データ送信API
'' ページからAjaxで要求を受け、JSONを返す。

Public Class datarq_wv : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest          ' Session変数の読み取り/書き込みアクセス権付き、読み取り専用アクセス権の場合は[IReadOnlySessionState]

        Dim currentContext As HttpContext = HttpContext.Current
        Dim retString As String = Nothing
        Dim Sensor As String = DirectCast(currentContext.Request.QueryString("sn"), String)
        Dim SensorNo As Integer = Convert.ToInt32(Sensor.Replace("No.", ""))
        Dim csvDate As String = DirectCast(currentContext.Request.QueryString("dt"), String)

        Try
            Dim siteDirectory As String = context.Session("SD").ToString                '備忘録 同一のセッションでsession変数を使う場合は、非同期要求しても処理はシリアライズされる
            Dim csvFilePath As String = context.Session("csvpath").ToString             '               「同一セッションの処理が別スレッドで実行されないようにするため」

            Try
                Dim getCsv As New ClsCSVData
                csvFilePath = IO.Path.Combine(csvFilePath, (csvDate & ".csv"))
                retString = getCsv.ReadDataFromCSVatUdomariStr(csvFilePath, SensorNo)

            Catch ex As Exception
                retString = "Error"
            End Try

            ''context.Response.ContentType = "application/json"               'RFC 4627        
            ''context.Response.ContentType = "application/csv"               'RFC 4627        
            context.Response.ContentType = "application/octet-stream"

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
