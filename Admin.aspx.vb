Imports System.Data
Imports System.Data.OleDb

Partial Class Admin

    Inherits System.Web.UI.Page

    Private UserNameList As String                                  '' ユーザー名称の一覧を文字列で格納
    Private SiteNo As String                                        '' 指定している現場の番号を文字列で格納

    Private Sub ConstructionSite_Add(ByVal ConstructionSiteName As String, ByVal HttpAddress As String)

        '' 現場名称の追加
        'Dim DbCn As OleDbConnection = Nothing
        ' Dim DbCm As New OleDb.OleDbCommand
        'Dim DbDa As OleDbDataAdapter = Nothing
        'Dim DtSet As DataSet = Nothing
        Dim clsUser As New ClsCheckUser

        Dim strWrite As String

        Try
            Using DbCn As New OleDbConnection
                DbCn.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & clsUser.UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5"

                'strWrite = "INSERT INTO ConstructionSite VALUES(1,'" & ConstructionSiteName & "','" & HttpAddress & "')"
                ''DbCn.Open()
                'Try
                strWrite = "INSERT INTO ConstructionSite VALUES(1,'" & ConstructionSiteName & "','" & HttpAddress & "')"

                Using DbDa As New OleDbDataAdapter(strWrite, DbCn)

                    Using DtSet As New DataSet("DData")
                        'DtSet = New DataSet("DData")
                    'DbDa = New OleDb.OleDbDataAdapter(strWrite, DbCn)
                    DbDa.Fill(DtSet, "DData")

                    ''DbCm = New OleDb.OleDbCommand(strWrite, DbCn)
                    ''DbCm.ExecuteNonQuery()
                    End Using
                End Using
            End Using
        Catch ex As Exception

            'Finally
            '    ''DbCm.Dispose()
            '    If DtSet IsNot Nothing Then DtSet.Dispose()
            '    If DbDa IsNot Nothing Then DbDa.Dispose()
            '    If DbCn IsNot Nothing Then DbCn.Dispose()
            '    DbCn.Close()

        End Try

        clsUser = Nothing

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load


        ''        UserInformation_DatabaseFile = Session.Item("dbName")
        ''If UserInformation_DatabaseFile = "" Then
        ''    Response.Redirect("Login.aspx")
        ''End If
        ''DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")

        '認証済みであるかを確認して、未承認であれば、ログインページへリダイレクトする
        If User.Identity.IsAuthenticated = False Then Response.Redirect("~/Login.aspx", False)

        If IsPostBack = False Then

            Call GetSiteNameInf()

        End If

        '' '' ユーザー名称を読み込む
        ''DbCom = New OleDb.OleDbCommand("Select * From UserInformation", DbCon)
        ''DbDr = DbCom.ExecuteReader

        ''If DbDr.HasRows = True Then
        ''    ListNothing.Visible = False
        ''    DbDa = New OleDb.OleDbDataAdapter("Select * From UserInformation", DbCon)
        ''    DbDa.Fill(DtSet, "UserInformation")

        ''    UserNameList = ""
        ''    For iLoop = 0 To DtSet.Tables("UserInformation").Rows.Count - 1
        ''        UserNameList = UserNameList & "@@@" & DtSet.Tables("UserInformation").Rows(iLoop).Item("UserName").ToString
        ''    Next
        ''End If
        '' '' ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. 読込完了
        ''DbCon.Close()

    End Sub


    Protected Sub GetSiteNameInf()
        ''
        '' 現場一覧を読み込んで表示する
        ''
        'Dim DbCon As OleDbConnection = Nothing
        ' Dim DbDr As OleDb.OleDbDataReader
        ' Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDbDataAdapter = Nothing
        'Dim DtSet As DataSet = Nothing
        Dim iloop As Integer
        Dim clsUser As New ClsCheckUser

        Try

            Using DbCon As New OleDbConnection
                'DbCon = New OleDbConnection
                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & clsUser.UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")
                ''DbCon.Open()

                Dim DbDa As New OleDbDataAdapter("Select * From ConstructionSite", DbCon)
                '' 現場名称の読込       "
                Using DSet As New DataSet("DData")
                    'DbDa = New OleDb.OleDbDataAdapter("Select * From ConstructionSite", DbCon)
                    DbDa.Fill(DSet, "DData")
                    ''DbCom = New OleDb.OleDbCommand("Select * From ConstructionSite", DbCon)
                    ''DbDr = DbCom.ExecuteReader

                    ''If DbDr.HasRows = True Then
                    If DSet.Tables("DData").Rows.Count > 0 Then
                        ListNothing.Visible = False
                        DbDa.Dispose()
                        DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM ConstructionSite ORDER BY Complete DESC,Type,OrderInType,SiteNo ASC", DbCon)
                        'DtSet.Dispose()
                        Using DtSet As New DataSet
                            DbDa.Fill(DtSet, "ConstructionSite")

                            Me.ConstructionList.Items.Clear()
                            For iloop = 0 To DtSet.Tables("ConstructionSite").Rows.Count - 1
                                If CType(DtSet.Tables("ConstructionSite").Rows(iloop).Item("Complete"), Boolean) = False Then
                                    Me.ConstructionList.Items.Add(DtSet.Tables("ConstructionSite").Rows(iloop).Item("SiteName").ToString & vbTab & " [フォルダ名:" & DtSet.Tables("ConstructionSite").Rows(iloop).Item("SiteFolder").ToString & "]")
                                Else
                                    Me.ConstructionList.Items.Add("【完了現場】" & DtSet.Tables("ConstructionSite").Rows(iloop).Item("SiteName").ToString & vbTab & " [フォルダ名:" & DtSet.Tables("ConstructionSite").Rows(iloop).Item("SiteFolder").ToString & "]")
                                End If
                            Next
                        End Using
                    Else
                        Call ConstructionSite_Add("坂田電機株式会社", "http:/www.sakatadenki.co.jp")
                        ListNothing.Visible = True
                    End If
                    '' ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. 読込完了
                End Using
            End Using
        Catch ex As Exception

        Finally
            'DbDa.Dispose()
            'DtSet.Dispose()
            'DbCon.Dispose()
            'DbCon.Close()
            clsUser = Nothing

        End Try

    End Sub

    Protected Sub BtnSiteAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnSiteAdd.Click
        ''
        '' 現場名を追加する
        ''
        Dim iloop As Integer
        Dim NewSite As String
        Dim SiteFolder As String
        Dim ExistSiteName As String
        Dim ExistFolderName As String
        Dim intCoron As Integer
        Dim EndBracked As Integer

        NewSite = Me.TxtNewSite.Text
        SiteFolder = Me.TxtFolderName.Text

        For iloop = 0 To Me.ConstructionList.Items.Count - 1
            ExistSiteName = Left$(Me.ConstructionList.Items(iloop).Text, Me.ConstructionList.Items(iloop).Text.IndexOf(vbTab))
            If ExistSiteName.Contains(NewSite) = True Then
                Me.lblAlert.Visible = True
                Exit Sub
            End If

            intCoron = Me.ConstructionList.Items(iloop).Text.IndexOf(":") + 1
            EndBracked = Me.ConstructionList.Items(iloop).Text.LastIndexOf("]")

            ExistFolderName = Me.ConstructionList.Items(iloop).Text.Substring(intCoron, EndBracked - intCoron)
            If Me.ConstructionList.Items(iloop).Text.Contains(NewSite) = True Then
                Me.lblAlert.Visible = True
                Exit Sub
            End If
        Next

        '' ユーザの追加
        'Dim DbCn As New OleDb.OleDbConnection
        'Dim DbCm As OleDb.OleDbCommand = Nothing                      ' 2017/05/08 Kino Changed   Delete [New]
        Dim DbDr As OleDb.OleDbDataReader = Nothing
        Dim strWrite As String
        Dim userMax As Integer
        Dim clsUser As New ClsCheckUser

        Using DbCn As New OleDb.OleDbConnection

            Try
                DbCn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & clsUser.UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")
                DbCn.Open()

                strWrite = "SELECT MAX(SiteNo) FROM ConstructionSite"
                Using DbDr
                    Using DbCm As New OleDb.OleDbCommand(strWrite, DbCn)
                        'DbCm = New OleDb.OleDbCommand(strWrite, DbCn)
                        DbDr = DbCm.ExecuteReader
                        DbDr.Read()
                        userMax = DbDr.GetInt32(0)
                    End Using
                End Using

                strWrite = "INSERT INTO ConstructionSite([SiteNo],[SiteName],[Address]) VALUES(" & (userMax + 1) & ",'" & NewSite & "','" & SiteFolder & "')"
                'Try

                'DbCm = New OleDb.OleDbCommand(strWrite, DbCn)
                Using DbCm As New OleDb.OleDbCommand(strWrite, DbCn)
                    DbCm.ExecuteNonQuery()
                    DbCn.Close()
                End Using
                Me.TxtNewSite.Text = ""
                Me.TxtFolderName.Text = ""

                'Response.Redirect("Admin.aspx", False)         ' 2017/05/08 Kino Moved to lower line.
                Call GetSiteNameInf()

            Catch ex As Exception

            Finally
                If DbDr IsNot Nothing Then DbDr.Close()
                ''If DbCm IsNot Nothing Then DbCm.Dispose()
                ''If DbCn IsNot Nothing Then DbCn.Close()
                ''DbCn.Dispose()
                Response.Redirect("Admin.aspx", False)          ' 2017/05/08 Kino Add
                clsUser = Nothing
            End Try
        End Using

    End Sub

    Protected Sub NewUser_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles NewUser.Click
        ''
        '' 新規ユーザを追加する
        ''

        Dim iLoop As Integer
        Dim OKFlg As Boolean

        SiteNo = ""
        OKFlg = False
        For iLoop = 0 To Me.ConstructionList.Items.Count - 1
            If Me.ConstructionList.Items(iLoop).Selected = True Then
                SiteNo = SiteNo & iLoop.ToString & ","
                OKFlg = True
            End If
        Next

        If OKFlg = True Then
            SiteNo = SiteNo.Substring(0, SiteNo.Length - 1)
            If InStr(UserNameList, "@@@" & Me.NewUserName.Text, CompareMethod.Text) = 0 Then
                Call UserAndPassword_Add()
                ''MsgBox("新規ユーザー登録完了", MsgBoxStyle.OkOnly, "正常")

                Dim DbDr As OleDb.OleDbDataReader = Nothing
                Dim DbCon As New OleDb.OleDbConnection
                Dim DbCom As New OleDb.OleDbCommand
                Dim DbDa As OleDb.OleDbDataAdapter = Nothing
                Dim DtSet As New DataSet
                Dim clsUser As New ClsCheckUser

                Try
                    DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & clsUser.UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")
                    DbCon.Open()

                    '' ユーザー名称を読み込む
                    DbCom = New OleDb.OleDbCommand("Select * From UserInformation", DbCon)
                    DbDr = DbCom.ExecuteReader

                    If DbDr.HasRows = True Then
                        ListNothing.Visible = False
                        DbDa = New OleDb.OleDbDataAdapter("Select * From UserInformation", DbCon)
                        DbDa.Fill(DtSet, "UserInformation")

                        UserNameList = ""
                        For iLoop = 0 To DtSet.Tables("UserInformation").Rows.Count - 1
                            UserNameList = UserNameList & "@@@" & DtSet.Tables("UserInformation").Rows(iLoop).Item("UserName").ToString
                        Next
                    End If
                    '' ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. 読込完了

                Catch ex As Exception

                Finally
                    DbCom.Dispose()
                    DtSet.Dispose()
                    DbDr.Close()
                    DbCon.Close()
                    clsUser = Nothing
                End Try

            Else
                Me.LblNewUserAlert.Text = "既に登録されているユーザー名称です。"
                ''MsgBox("既に登録されているユーザー名称です。", MsgBoxStyle.OkOnly, "警告")
                Me.LblNewUserAlert.Visible = True
            End If
        Else
            Me.LblNewUserAlert.Text = "最低１つの現場を選択して下さい。"
            ''MsgBox("最低１つの現場を選択して下さい。")
            Me.LblNewUserAlert.Visible = True
        End If

    End Sub

    Private Sub UserAndPassword_Add()

        '' ユーザの追加
        Dim DbCn As OleDb.OleDbConnection = New OleDb.OleDbConnection()
        Dim DbCm As New OleDb.OleDbCommand
        Dim DbDr As OleDb.OleDbDataReader
        Dim strWrite As String
        Dim userMax As Long
        Dim clsUser As New ClsCheckUser

        DbCn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & clsUser.UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")
        DbCn.Open()

        strWrite = "SELECT MAX(UserNo) FROM UserInformation"
        DbCm = New OleDb.OleDbCommand(strWrite, DbCn)
        DbDr = DbCm.ExecuteReader
        DbDr.Read()
        userMax = DbDr.GetInt32(0)
        DbDr.Close()

        strWrite = "INSERT INTO UserInformation([UserNo],[UserName],[PassWord],[SetPlaceNo],[客先]) VALUES(" & (userMax + 1) & ",'" & Me.NewUserName.Text & "','" & Me.NewUserPassword.Text & "','" & SiteNo & "','" & Me.NewCustomerName.Text & "')"
        Try
            DbCm = New OleDb.OleDbCommand(strWrite, DbCn)
            DbCm.ExecuteNonQuery()

            Me.NewUserName.Text = ""
            Me.NewUserPassword.Text = ""
            Me.NewCustomerName.Text = ""

            'Response.Redirect("Admin.aspx",false)
        Catch ex As Exception

        Finally
            DbDr.Close()
            DbCm.Dispose()
            DbCn.Close()
            DbCn.Dispose()
            clsUser = Nothing
        End Try

    End Sub

    Protected Sub LoginStatus1_LoggingOut(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.LoginCancelEventArgs) Handles LoginStatus1.LoggingOut
        ''
        '' ログアウト
        ''
        Session.Abandon()
        Session.Clear()

    End Sub

    Protected Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click
        ''
        '' アクセス履歴を表示する
        ''
        Response.Redirect("~/LoginHistory.aspx", False)

    End Sub


End Class
