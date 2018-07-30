<%@ WebHandler Language="VB" Class="UpdCheck" %>
Option Explicit On
Option Strict On
Imports System
Imports System.Web
Imports System.Data
Imports System.IO

Public Class UpdCheck : Implements IHttpHandler, IRequiresSessionState

    ' ■■■■ Client側からの要求と応答■======================================================
    ' 複数ファイルの場合は、引数によって必要なファイル番号の日付をカンマ区切りで返す
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        'Diagnostics.Debug.WriteLine(DateTime.Now & " " & context.User.Identity.IsAuthenticated & " " & context.Request.UrlReferrer.ToString) 'My.User.CurrentPrincipal.Identity.IsAuthenticated)
        'context.Response.ContentType = "text/plain"
        'context.Response.Write("Hello World")
        Dim currentContext As HttpContext = HttpContext.Current
        Dim fID As String = CType(currentContext.Request.QueryString("i"), String)
        Dim retString As String = "Error:DataError"
        Dim siteDirectory As String = Nothing
        Dim menuInfoPath As String = Nothing
        Dim dteRet As String = New DateTime(1900, 1, 1, 0, 0, 0).ToString
        Dim RetryCount As Integer = 0

        Try
            If context.Session.Item("SD") Is Nothing Then
                retString = "Error:Unauthorized"
                context.Response.StatusCode = Net.HttpStatusCode.PreconditionFailed             ' Unauthrizedだと、ここを抜けた後に、Login.aspxが呼ばれて、ブラウザで判定できない
            Else

                siteDirectory = context.Session("SD").ToString
                menuInfoPath = IO.Path.Combine(context.Server.MapPath(siteDirectory & "\App_Data"), "MenuInfo.mdb")

                While RetryCount < 3
                    Try
                        If File.Exists(menuInfoPath) = True Then                ' ファイルの存在チェック
                            Dim cn As OleDb.OleDbConnection
                            Dim da As OleDb.OleDbDataAdapter
                            Dim dtSet As DataSet

                            cn = New OleDb.OleDbConnection
                            Using cn
                                da = New OleDb.OleDbDataAdapter
                                Using da
                                    dtSet = New DataSet("Dates")
                                    Using dtSet
                                        cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & menuInfoPath & ";" & "Jet OLEDB:Engine Type= 5")

                                        '' da = New OleDb.OleDbDataAdapter("SELECT MAX(NewestDate) FROM データファイル名設定", cn)
                                        da = New OleDb.OleDbDataAdapter(String.Format("SELECT NewestDate FROM データファイル名設定 WHERE ID IN({0}) ORDER BY ID", fID), cn)
                                        da.Fill(dtSet, "Dates")

                                        'dteRet = Convert.ToDateTime(dtSet.Tables("Dates").Rows(0)(0))
                                        Dim dtr As DataRow

                                        dteRet = Nothing
                                        For Each dtr In dtSet.Tables("Dates").Rows
                                            Dim temp As String = Convert.ToDateTime(dtr.Item(0)).ToString("yyyy/MM/dd HH:mm:ss")
                                            dteRet &= String.Format("{0},", temp)
                                        Next
                                        dteRet = dteRet.TrimEnd(Convert.ToChar(","))

                                        dtSet.Dispose()
                                    End Using
                                    da.Dispose()
                                End Using
                                cn.Dispose()
                            End Using
                            cn.Close()
                            cn.Dispose()

                            retString = dteRet      '.ToString("yyyy/MM/dd HH:mm:ss")

                        End If

                        context.Response.StatusCode = Net.HttpStatusCode.OK

                        RetryCount = 3

                    Catch ex As Exception
                        RetryCount += 1
                        Threading.Thread.Sleep(2000)
                    End Try
                End While
            End If

        Catch ex As Exception
            context.Response.StatusCode = Net.HttpStatusCode.InternalServerError
        End Try

        Try
            context.Response.ContentType = "text/plain"
            context.Response.Charset = "UTF-8"
            context.Response.Write(retString)
            ''context.Response.End()                                            '' これは、TreadAbortExceptionが発生する
            HttpContext.Current.ApplicationInstance.CompleteRequest()
            context.Response.Flush()
            context.Response.SuppressContent = True                             ' 余計なごみ(HTML関連)がついてしまうのを回避する　Flushと一緒に利用
            'System.Diagnostics.Debug.WriteLine(context.Request.UrlReferrer.OriginalString)
            'System.Diagnostics.Debug.WriteLine(retString)
        Catch ex As System.Threading.ThreadAbortException
            System.Diagnostics.Debug.WriteLine("Error ThreadAbortException")    ' 念のため例外処理を入れる
        End Try

    End Sub
    '--------------------------------------------------------------------------------------------

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class