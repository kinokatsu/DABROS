Option Strict On

Imports System.Data
Imports System.Data.OleDb
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports C1.C1Zip

Partial Class Control_SiteEquipSetContact
    Inherits System.Web.UI.Page

    '' チャンネルとヘッダ部情報
    Public Structure CommInf
        Dim ChNo As String
        Dim SensorSymbol As String
        Dim Unit As String
    End Structure

    ''csvファイルダウンロード情報
    Private Structure csvDLInfo
        Dim ItemName As String
        Dim chData As String
        Dim csvFileName As String
        Dim dataFileNo As Integer
        Dim Operate As Boolean
        Dim sortType As Short                           ' 2011/11/10 Kino Add
    End Structure

    Private mDataFileNames() As ClsReadDataFile.DataFileInf
    ''Protected WithEvents DLButton As System.Web.UI.WebControls.Button

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        ''
        '' ページを閉じるときのイベント
        ''

        Session.Abandon()                       '現在のセッションに破棄のマークを付けます（セッションを破棄する）
        ''Session.Clear()
        Response.Cookies.Clear()


    End Sub

    Protected Sub Page_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error                  ' 2011/05/30 Kino Add

        Dim ex As Exception = Server.GetLastError().GetBaseException()
        Dim clsLogWrite As New ClsGraphCommon
        Dim userName As String
        Dim siteDirectory As String

        If Session.Item("UN") IsNot Nothing Then
            userName = CType(Session.Item("UN"), String)
        Else
            userName = "UN---"
        End If

        If Session.Item("SD") IsNot Nothing Then
            siteDirectory = CType(Session.Item("SD"), String)
        Else
            siteDirectory = "SD---"
        End If

        clsLogWrite.writeErrorLog(ex, Server.MapPath("~/errorSummary.log"), userName, siteDirectory, MyBase.GetType.BaseType.FullName)

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"

         ''認証されていない場合は、閉じる
        If User.Identity.IsAuthenticated = False Then
            Response.Write(strScript)
            'FormsAuthentication.RedirectFromLoginPage("vb", True)
            Exit Sub
        End If

        Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)          'ログインステータス

         ' ''ログインしていない場合は、閉じる
        If LoginStatus = 0 Then
            'FormsAuthentication.RedirectFromLoginPage("vb", True)
            Response.Redirect("sessionerror.aspx")
            ''Response.Write(strScript)
            Exit Sub
        End If

         ''実行権限がない場合は前のページに戻る
        Dim uLevel As Integer = CType(Session.Item("UL"), Integer)
        Dim userLevel As String = uLevel.ToString("000000")     'ユーザレベル
        Dim ULNo(5) As String
        Dim clsLevelCheck As New ClsCheckUser
        Dim strInt As Integer = clsLevelCheck.GetWord(userLevel, ULNo)
        Dim strScriptUL As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('実行する権限がありません');history.back();</script>"
        If ULNo(0) = "0" Then
            Response.Write(strScriptUL)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする

        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)
        Dim siteName As String
        Dim siteDirectory As String
        Dim intRet As Integer
        Dim clsNewDate As New ClsReadDataFile
        'Dim clsSetScript As New ClsGraphCommon
        Dim EdDate As Date

        siteName = CType(Session.Item("SN"), String)                '現場名
        siteDirectory = CType(Session.Item("SD"), String)           '現場ディレクトリ

         ''セッションタイムアウトした時には、ログイン画面を出す
        If [String].IsNullOrEmpty(siteDirectory) = True Then Response.Redirect("Login.aspx")

        intRet = clsNewDate.GetDataFileNames(Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb"), mDataFileNames)       'データファイル名、共通情報ファイル名、識別名を取得
        intRet = clsNewDate.GetDataLastUpdate(Server.MapPath(siteDirectory & "\App_Data\"), mDataFileNames)                  'データファイルにおける最新・最古データ日時を取得

        If IsPostBack = False Then

            Session.Remove("mes")
            If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
                ''==== フォームのサイズを調整する ====
                If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
                    Dim OpenString As String

                    OpenString = "<SCRIPT LANGUAGE='javascript'>"
                    OpenString &= "window.resizeTo(" & wid & "," & Hei & ");"
                    OpenString &= "<" & "/SCRIPT>"
                    Page.ClientScript.RegisterStartupScript(Me.GetType(), "CSVDL", OpenString)
                End If
            End If

            EdDate = GetMostUpdate(mDataFileNames)
            Me.LblLastUpdate1.Text = "最新データ日時：" & EdDate.ToString("yyyy/MM/dd HH:mm")

            Call GetLoggerInfo2Control(siteDirectory)                                                                       ' 設置場所をコンボに登録

        Else

        End If

    End Sub

    Protected Sub GetLoggerInfo2Control(ByVal siteDirectory As String)

        'Dim cn As New OleDbConnection
        'Dim da As OleDbDataAdapter
        'Dim dtSet As New DataSet("Loc")
        Dim DBPath As String = IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data\ContactControl.mdb")

        Dim DefLi As New ListItem("選択してください", "Undefind")
        Me.DDLMeasPoint.Items.Add(DefLi)

        Using cn As New OleDbConnection

            cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DBPath & ";" & "Jet OLEDB:Engine Type= 5")

            '' 現場名称の読込
            'da = New OleDbDataAdapter("SELECT * FROM ContactInfo ORDER BY ID ASC", cn)
            Using da As New OleDbDataAdapter("SELECT * FROM ContactInfo ORDER BY ID ASC", cn)

                Using dtSet As New DataSet("Loc")
                    da.Fill(dtSet, "Loc")

                    Try
                        With Me.DDLMeasPoint
                            For Each DTR As DataRow In dtSet.Tables("Loc").Rows
                                Dim Li As New ListItem(Trim(DTR.Item(10).ToString), Trim(DTR.Item(1).ToString))
                                .Items.Add(Li)
                            Next
                        End With
                    Catch ex As Exception

                        ''Finally
                        ''    dtSet = Nothing
                        ''    da.Dispose()
                        ''    cn.Dispose()
                    End Try
                End Using
            End Using
        End Using

    End Sub

    Protected Function GetMostUpdate(ByVal DataFileNames() As ClsReadDataFile.DataFileInf) As Date
        ''
        '' 使用しているデータファイルの中から、一番最新のデータ日付を取得する
        ''
        Dim dteTemp As Date
        Dim dteSession As Date
        Dim iloop As Integer

        For iloop = 0 To DataFileNames.Length - 1
            dteSession = DataFileNames(iloop).NewestDate
            If dteTemp < dteSession Then
                dteTemp = dteSession
            End If
        Next

        Return dteTemp

    End Function

    Protected Sub JMsgBox(ByVal msg As String)
        ''
        '' メッセージボックス(Alert)をブラウザに表示させる
        ''

        Dim strScript As String

        strScript = "<script language =javascript>"
        strScript &= "alert('" & msg & "');"
        strScript &= "</script>"
        Response.Write(strScript)

    End Sub

End Class
