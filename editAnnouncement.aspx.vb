Imports System.Data
Imports System.Drawing

Partial Class editAnnouncement
    Inherits System.Web.UI.Page

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed

        ''Session.RemoveAll()                     'セッション状態のコレクションからすべての値とキーを削除します。
        Session.Abandon()                       '現在のセッションに破棄のマークを付けます（セッションを破棄する）
        Session.Clear()

    End Sub

    Protected Sub Page_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error

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

        clsLogWrite.writeErrorLog(ex, Server.MapPath("~/log/errorSummary_" & Date.Now.ToString("yyMM") & ".log"), userName, siteDirectory, MyBase.GetType.BaseType.FullName)

        Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)

        If LoginStatus = 0 Then     ''ログインステータスが０ならログイン画面
            Response.Redirect("sessionerror.aspx", False)
        Else                        ''そうでなければ、データ表示画面を再構築
            ''Dim DataTime As String = CType(Request.QueryString("DT"), String)
            ''Dim ChNo As Integer = CType(Request.Item("CH"), Integer)
            ''If DataTime = Nothing Then DataTime = "000000000000"
            ''RedirectToMobilePage("MobileTable.aspx?CH=" + Server.UrlEncode(ChNo) + "&DT=" + Server.UrlEncode(DataTime))
            Dim strScript As String = "<html><head><title>タイムアウト</title></head><body>接続タイムアウトになりました。</body></html>" + "<script language=javascript>alert('画面を閉じて再表示してください。');window.close();</script>"
            Response.Write(strScript)

        End If

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"

        ''認証されていない場合は、閉じる
        If User.Identity.IsAuthenticated = False Then
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)          'ログインステータス
        ' ''ログインしていない場合は、閉じる
        If LoginStatus = 0 Then
            'FormsAuthentication.RedirectFromLoginPage("vb", True)
            Response.Redirect("sessionerror.aspx", False)
            ''Response.Write(strScript)
            Exit Sub
        End If

        ''実行権限がない場合は前のページに戻る
        Dim uLevel As Integer = CType(Session.Item("UL"), Integer)
        Dim userLevel As String = uLevel.ToString("000000")     'ユーザレベル
        Dim ULNo(5) As String
        Dim clsLevelCheck As New ClsCheckUser
        Dim strInt As Integer = clsLevelCheck.GetWord(userLevel, ULNo)
        Dim strScriptUL As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('実行する権限がありません');history.back();</script>" ';self.opener=self;setTimeout('self.close()',1);</script>"
        If ULNo(1) = "0" Then
            Response.Write(strScriptUL)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする
        Response.Cache.SetNoStore()
        Response.Cache.SetExpires(DateTime.Now.AddDays(-1))

        Dim iloop As Integer
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)
        Dim showSiteName As String = CType(Session.Item("SN"), String)
        Dim MngSiteNum As Integer = CType(Session.Item("SNUM"), Integer)
        Dim SiteNames As String = CType(Session.Item("ASN"), String)
        Dim UserGroup As String = CType(Session.Item("UG"), String)
        Dim UserName As String = CType(Session.Item("UN"), String)

        If IsPostBack = False Then

            '==== フォームのサイズを調整する ====
            'If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
            '    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
            '        Dim OpenString As String
            '        OpenString = "<SCRIPT LANGUAGE='javascript'>"
            '        OpenString &= "window.resizeTo(" & wid & "," & Hei & ");"
            '        OpenString &= "<" & "/SCRIPT>"
            '        Page.ClientScript.RegisterStartupScript(Me.GetType(), "通知編集", OpenString)
            '    End If
            'End If

            With Me.DDListHour                                      '時刻作成
                For iloop = 0 To 23
                    .Items.Add(iloop.ToString(" 00"))
                    .Items(iloop).Value = iloop.ToString
                    Me.DDListHourSt.Items.Add(iloop.ToString(" 00"))
                    Me.DDListHourSt.Items(iloop).Value = iloop.ToString

                Next
                .SelectedIndex = 0
            End With
            With Me.DDListMinute                                    '分作成
                For iloop = 0 To 59
                    .Items.Add(iloop.ToString(" 00"))
                    .Items(iloop).Value = iloop.ToString
                    Me.DDListMinuteSt.Items.Add(iloop.ToString(" 00"))
                    Me.DDListMinuteSt.Items(iloop).Value = iloop.ToString
                Next
                .SelectedIndex = 0
            End With

            With Me.DDLSiteName                                                                                 '2010/05/18 Kino Add 現場一覧
                Dim UName As String = ""
                Dim UGroup As String = ""
                Dim strGrp() As String
                If MngSiteNum > 1 Then
                    Call getUserGroups(UName, UGroup)                                                           '表示している現場を参照しているユーザ名、グループ名一覧を取得
                    strGrp = UName.Split(","c)
                    With Me.LstUserNames                                                                        'リストボックスにユーザ一覧を表示
                        .Items.Add("追加するユーザを選択してください（不要な場合はここを選択）。")
                        .Items(0).Selected = True
                        For iloop = 0 To strGrp.Length - 2
                            .Items.Add(strGrp(iloop))
                        Next
                    End With

                    Erase strGrp
                    strGrp = UGroup.Split(","c)
                    With Me.LstGroupName                                                                        'リストボックスにグループ一覧を表示
                        .Items.Add("追加するグループを選択してください（不要な場合はここを選択）。")
                        .Items(0).Selected = True
                        For iloop = 0 To strGrp.Length - 2
                            .Items.Add(strGrp(iloop))
                        Next
                    End With

                Else
                    Me.LstUserNames.Visible = False                                                             '複数現場の管理者アカウントでなければ、ユーザリストは表示しない
                    Me.PnlUserNm.Visible = False
                End If

                Dim strTemp() As String = SiteNames.Split(","c)
                For iloop = 0 To MngSiteNum - 1
                    .Items.Add(strTemp(iloop).ToString)
                    .Items(iloop).Value = iloop.ToString
                    If strTemp(iloop) = showSiteName Then
                        .Items(iloop).Selected = True
                    End If
                Next
                If MngSiteNum > 1 And UserGroup = "gadmin" Then
                    .Items.Add("全現場")
                    .Items(iloop).Value = iloop.ToString
                End If
            End With

            With ChkBListUser
                .Items.Add("ログインユーザ(" + UserName + ")") : .Items(0).Value = UserName
                .Items.Add("所属グループ(" + UserGroup + ")") : .Items(1).Value = UserGroup
                If UserGroup = "gadmin" Then
                    .Items.Add("全ユーザ") : .Items(2).Value = "ALL"
                    ''.Items.Add("管理者") : .Items(3).Value = "gadmin"
                End If
                For iloop = 0 To 1
                    .Items(iloop).Selected = True
                    ''.Items(iloop).Enabled = False
                Next
            End With

            Me.TxtStDate.Text = Date.Now.ToString("yyyy/MM/dd")                                                 '期限は１年後とする
            Me.TxtLimitDate.Text = (Date.Now.AddYears(1).AddDays(-1)).ToString("yyyy/MM/dd")                    '期限は１年後とする
            ''Me.RngValidLt.MaximumValue = Date.Parse(Date.Now.AddYears(1).AddDays(-1).ToShortDateString)         '期限は１年後とする
            Me.RngValidSt.MaximumValue = Date.Now.AddMonths(1).AddDays(-1).ToShortDateString      '期限は１ヵ月後とする
            ''Me.C1WebCalendar1.MaxDate = Date.Parse(Date.Now.AddYears(1).AddDays(-1).ToShortDateString)          '期限は１年後とする
            Me.C1WebCalendar.MaxDate = Date.Parse(Date.Now.AddYears(1).AddDays(-1).ToShortDateString)          '期限は１年後とする

        End If

        Call Bind2DB()

        Me.imgCalTxtLimitDate.Attributes.Add("onclick", "DropCalendar(this);")
        Me.imgCalTxtStDate.Attributes.Add("onclick", "DropCalendar(this);")
        Page.SetFocus(DDLSiteName)

    End Sub

    Protected Sub Bind2DB()
        ''
        '' データファイルおよびSQLのクエリを用いてデータベースにバインドする        2010/05/06 Kino Add
        ''
        Dim strSQL As String = ""
        Dim clsUser As New ClsCheckUser
        Dim DataFile As String = clsUser.UserAnnouncement_DatabaseFile
        Dim UserKey As String
        Dim UserGroup As String
        Dim AllSiteNames As String = CType(Session.Item("ASN"), String)             '全現場名カンマ区切り
        Dim strTemp() As String = AllSiteNames.Split(","c)
        Dim iloop As Integer
        Dim showSiteName As String = ""
        Dim dq As String = """"

        For iloop = 0 To strTemp.Length - 1
            showSiteName += (dq + strTemp(iloop).ToString + dq + ",")
        Next
        showSiteName += (dq + "全現場" + dq)
        ''showSiteName = showSiteName.Substring(0, showSiteName.Length - 1)

        UserKey = CType(Session.Item("UN"), String)
        UserGroup = CType(Session.Item("UG"), String)

        If UserGroup = "gadmin" Then                            '管理者グループの場合は全部表示
            strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 識別名称 AS 現場名称,ユーザ名 AS 表示ユーザ,FORMAT(表示期限, 'yyyy/mm/dd hh:mm') AS 表示期限,内容 FROM お知らせ WHERE (識別名称 IN(" + showSiteName + ") AND (表示期限 >= NOW()) AND (削除 = False)) ORDER BY [No] DESC, 更新日時 DESC"
            Me.LstGroupName.Visible = True
        Else
            Me.LstUserNames.Width = 500
            If Me.DDLSiteName.Items.Count = 1 Then         '単一現場の場合
                ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 更新日時,識別名称 AS 現場名称,ユーザ名 AS 表示ユーザ,表示期限, 内容 FROM お知らせ WHERE (識別名称 IN(" + showSiteName + ") AND ('ユーザ名' LIKE " + dq + "%@" & UserKey & "@%" + dq + " OR 'ユーザ名' LIKE " + dq + "%@ALL@%" + dq + ") AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
                ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 識別名称 AS 現場名称,FORMAT(表示期限, 'yyyy/mm/dd hh:mm') AS 表示期限,内容 FROM お知らせ " + "WHERE ((識別名称 IN (" + showSiteName + ") AND (INSTR(ユーザ名,'@" & UserKey & "@')<>0 OR INSTR(ユーザ名,'@" & UserGroup & "@')<>0))  OR (INSTR(ユーザ名,'@ALL@')<>0) AND (表示期限 >= NOW()) AND (削除 = False)) ORDER BY [No] DESC, 更新日時 DESC"
                strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 識別名称 AS 現場名称,FORMAT(表示期限, 'yyyy/mm/dd hh:mm') AS 表示期限,内容 FROM お知らせ " _
                     + "WHERE (((識別名称 IN (" + showSiteName + ") AND (INSTR(ユーザ名,'@" & UserKey & "@')<>0 OR INSTR(ユーザ名,'@" & UserGroup & "@')<>0))  OR (INSTR(ユーザ名,'@ALL@')<>0)) AND " _
                     + "(表示期限 >= NOW()) AND (削除=False)) ORDER BY 更新日時 DESC"
            Else                                                '複数現場の場合
                ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 更新日時, 識別名称 AS 現場名称,ユーザ名 AS 表示ユーザ,表示期限,内容 FROM お知らせ WHERE (識別名称 IN(" + showSiteName + ") AND ('ユーザ名' LIKE " + dq + "%@" & UserKey & "@%" + dq + " OR 'ユーザ名' LIKE " + dq + "%@ALL@%" + dq + ") AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
                ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 識別名称 AS 現場名称,ユーザ名 AS 表示ユーザ,FORMAT(表示期限, 'yyyy/mm/dd hh:mm') AS 表示期限,内容 FROM お知らせ WHERE " + "((識別名称 IN (" + showSiteName + ") AND (INSTR(ユーザ名,'@" & UserKey & "@')<>0)) OR (INSTR(ユーザ名,'@ALL@')<>0) AND (表示期限 >= NOW()) AND (削除 = False)) ORDER BY [No] DESC, 更新日時 DESC"
                strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 識別名称 AS 現場名称,ユーザ名 AS 表示ユーザ,FORMAT(表示期限, 'yyyy/mm/dd hh:mm') AS 表示期限,内容 FROM お知らせ " _
                        + "WHERE (((識別名称 IN (" + showSiteName + ") AND (INSTR(ユーザ名,'@" & UserGroup & "@')<>0)) OR (INSTR(ユーザ名,'@ALL@')<>0)) AND" _
                        + " (表示期限 >= NOW()) AND (削除=False)) ORDER BY 更新日時 DESC"
            End If
            ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 識別名称 AS 現場名称,ユーザ名 AS 表示ユーザ,表示期限,内容 FROM お知らせ WHERE (識別名称 IN(" + showSiteName + ") AND ('ユーザ名' LIKE " + dq + "%@" & UserKey & "@%" + dq + " OR 'ユーザ名' LIKE " + dq + "%@ALL@%" + dq + ") AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
        End If

        '---DataObjectSourceにDBをバインドする---
        Me.AcDatSrc.DataFile = DataFile
        Me.AcDatSrc.SelectCommand = strSQL
        Me.GrdView.DataSourceID = Me.AcDatSrc.ID
        Me.AcDatSrc.DataBind()

        clsUser = Nothing

    End Sub

    Protected Sub getUserGroups(ByRef UName As String, ByRef UGroup As String)
        ''
        '' 現在表示可能な現場を参照しているユーザのグループを検索する
        ''
        Dim clsRead As New ClsReadDataFile
        Dim clsUser As New ClsCheckUser
        Dim DataFile As String = clsUser.UserInformation_DatabaseFile
        Dim DbCon As New OleDb.OleDbConnection
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim iloop As Integer
        Dim DsetCount As Integer = 0
        Dim SiteNums As String = CType(Session.Item("ASNO"), String)
        Dim loginUserName As String = CType(Session.Item("UN"), String)
        Dim strSiteNo() As String = SiteNums.Split(","c)
        Dim outCh() As Integer = {}
        Dim strPlaceNo As String
        Dim strGroupName As String
        Dim strUserName As String
        Dim customerName As String = ""
        Dim strOutCh As String
        Dim strCheck() As String
        Dim UsrName As String = ""
        Dim UsrGroup As String = ""

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + DataFile + ";" + "Jet OLEDB:Engine Type= 5")
        ''DbCon.Open()

        '' 現場名称の読込
        DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM UserInformation WHERE (NOT UserName IN ('sakataadmin','sd001','1001','demo','keisoku','demo2')) ORDER BY UserName ASC", DbCon) '2011/05/24 Kino Changed UserNo -> UserName
        DbDa.Fill(DtSet, "DData")

        ''DbCon.Close()
        DbCon.Dispose()

        DsetCount = DtSet.Tables("DData").Rows.Count - 1

        For Each DTR As DataRow In DtSet.Tables("DData").Rows
            strUserName = DTR.Item(1).ToString
            strPlaceNo = DTR.Item(3).ToString
            customerName = DTR.Item(4).ToString
            strGroupName = DTR.Item(7).ToString
            strCheck = strPlaceNo.Split(","c)
            If strUserName = loginUserName Then GoTo NextStep
            If strCheck.Length > 1 Then
                strOutCh = clsRead.GetOutputChannel(outCh, strPlaceNo)         '複数現場指定があった場合
                strOutCh = ("@" + strOutCh.Replace(",", "@,@") + "@")
            Else
                strOutCh = "@" + strPlaceNo + "@"
            End If
            For iloop = 0 To strSiteNo.Length - 1
                If strOutCh.IndexOf("@" + strSiteNo(iloop).ToString + "@") <> -1 Then
                    UsrName += (strUserName + "：" + customerName + ",")
                    UsrGroup += (strGroupName + "：" + customerName + ",")
                    Exit For
                End If
            Next
NextStep:
            iloop += 1
        Next

        UName = UsrName
        UGroup = UsrGroup

        DbDa.Dispose()
        DbDa = Nothing
        DtSet = Nothing
        DbCon = Nothing
        clsRead = Nothing
        clsUser = Nothing

    End Sub

    ''Protected Sub GrdView_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles GrdView.DataBound

    ''    Call SetGridFormat()

    ''End Sub

    ''Protected Sub SetGridFormat()
    ''    ''
    ''    '' グリッドの全般的な書式設定
    ''    ''
    ''    With Me.GrdView
    ''        .HeaderRow.Cells(1).Width = 105
    ''        .HeaderRow.Cells(2).Width = 150
    ''        .HeaderRow.Cells(3).Width = 80
    ''        .HeaderRow.Cells(4).Width = 105
    ''    End With

    ''End Sub

    Protected Sub GrdView_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GrdView.RowCommand
        ''
        ''レコードの削除処理
        ''
        If e.CommandName <> "RecordDelete" Then Exit Sub '「削除」ボタン以外なら処理をしない

        Dim intTemp As Integer
        Dim RowKey(2) As String
        Dim clsUser As New ClsCheckUser
        Dim anno_DatabaseFile As String = clsUser.UserAnnouncement_DatabaseFile
        Dim DbCon As New OleDb.OleDbConnection()
        '' Dim Dbcom As OleDb.OleDbCommand
        Dim strSQL As String
        Dim intColNum As Integer
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim cmdbuilder As New OleDb.OleDbCommandBuilder

        If Me.DDLSiteName.Items.Count = 1 Then
            intColNum = 3
        Else
            intColNum = 4
        End If

        intTemp = Integer.Parse(e.CommandArgument.ToString)
        RowKey(0) = Me.GrdView.Rows(intTemp).Cells(2).Text
        RowKey(1) = Me.GrdView.Rows(intTemp).Cells(intColNum).Text
        RowKey(2) = Me.GrdView.Rows(intTemp).Cells(intColNum + 1).Text

        strSQL = ("UPDATE お知らせ SET [削除] = True WHERE (識別名称 = '" + RowKey(0) + "') AND (表示期限 = #" + RowKey(1) + "#) AND (内容 = '" + RowKey(2).Replace("<br />", Environment.NewLine) + "')")
        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & anno_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")

        DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon) '"SELECT * FROM お知らせ ORDER BY [No] DESC, 更新日時 DESC", DbCon)  '取り敢えずデータセット用にオープン
        DbDa.Fill(DtSet, "DData")

        ''DbDa.UpdateCommand = New OleDb.OleDbCommand()
        ''DbDa.UpdateCommand.CommandText = strSQL
        ''DbDa.UpdateCommand.Connection = DbCon

        ''DbDa.Update(DtSet, "DData")

        ''DbCon.Open()

        ''Dbcom = New OleDb.OleDbCommand(strSQL, DbCon)
        ''Dbcom.ExecuteNonQuery()

        ''Dbcom.Dispose()

        ''DbCon.Close()
        DbDa.Dispose()
        DtSet.Dispose()
        DbCon.Dispose()
        clsUser = Nothing

        Me.GrdView.DataBind()

    End Sub

    ''' <summary>
    ''' お知らせ情報の追加
    ''' </summary>
    Protected Sub BtnAddRecord_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnAddRecord.Click
        ''
        '' レコード追加
        ''
        Dim IDName As String    '= Date.Now.ToString("yyyy/MM/dd HH:mm") + " 更新分"
        Dim LimitDate As DateTime
        Dim txtCont As String
        'セッション変数に保存しないようにする 2018/03/30 Kino
        'Dim txtRemote As String = Nothing   'CType(Session.Item("RemoteInfo"), String)
        Dim clsBr As New ClsHttpCapData
        Dim txtRemote As String = clsBr.GetRemoteInfo(Request, Server.MapPath(""))
        clsBr = Nothing
        Dim clsUser As New ClsCheckUser
        Dim anno_DatabaseFile As String = clsUser.UserAnnouncement_DatabaseFile
        Dim userName As String = "" '"ALL"
        Dim writeUser As String
        Dim remoinf() As String = Nothing
        If txtRemote IsNot Nothing Then                                     ' 2012/08/24 Kino Add
            remoinf = txtRemote.Split(","c)
        End If
        Dim remoAddr As String
        Dim remoHost As String
        'Dim DbCon As New OleDb.OleDbConnection
        ''Dim Dbcom As OleDb.OleDbCommand
        ''Dim strSQL As String
        Dim re As New Regex("<.*?>", RegexOptions.Singleline)       ''HTMLタグを外す
        Dim iloop As Integer
        Dim startDate As DateTime
        Dim addUser As String = ""
        Dim tempGroup As String = ""
        Dim strTemp As String = ""
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        'Dim cmdbuilder As New OleDb.OleDbCommandBuilder

        With Me.ChkBListUser                                '表示ユーザ
            For iloop = 0 To .Items.Count - 1
                If .Items(iloop).Selected = True Then
                    userName += ("@" + .Items(iloop).Value.ToString + "@,")
                End If
            Next
        End With

        With Me.LstUserNames                                '追加ユーザ名
            For iloop = 1 To .Items.Count - 1
                If .Items(iloop).Selected = True Then
                    userName += ("@" + (.Items(iloop).Text).Substring(0, .Items(iloop).Text.IndexOf("：")) + "@,")
                End If
            Next
        End With

        With Me.LstGroupName                                '追加グループ名
            For iloop = 1 To .Items.Count - 1
                If .Items(iloop).Selected = True Then
                    strTemp = ("@" + (.Items(iloop).Text).Substring(0, .Items(iloop).Text.IndexOf("：")) + "@")
                    If tempGroup.IndexOf(strTemp) = -1 Then
                        tempGroup += (strTemp + ",")
                    End If
                End If
            Next
        End With
        userName += tempGroup
        If userName.Length > 0 Then
            userName = userName.Substring(0, userName.Length - 1)
        End If

        'If userName.IndexOf("@sd01@") = -1 Then userName &= ",@sd01@" '              2011/05/24 Kino Add 管理者系は強制的に追加 2013/02/06 Kino Changed comment
        'If userName.IndexOf("@gadmin@") = -1 Then userName &= ",@gadmin@" '          2011/05/24 Kino Add
        If userName.IndexOf("@gadmin@") = -1 Then userName &= ",@gadmin@" '          2011/05/24 Kino Add

        IDName = Me.DDLSiteName.SelectedItem.ToString           '現場名称

        ''If Me.TxtID.Text.Length <> 0 Then               ''識別名称
        ''  IDName = Me.TxtID.Text.Trim
        ''  IDName = re.Replace(IDName, "")
        ''Else
        ''  Exit Sub
        ''End If

        startDate = Date.Parse(Date.Parse(Me.TxtStDate.Text).ToShortDateString + " " + Me.DDListHourSt.Text + ":" + Me.DDListMinuteSt.Text)   ''表示開始日
        LimitDate = Date.Parse(Date.Parse(Me.TxtLimitDate.Text).ToShortDateString + " " + Me.DDListHour.Text + ":" + Me.DDListMinute.Text)    ''表示期限
        txtCont = Me.txtContent.Text.Trim               ''内容
        txtCont = re.Replace(txtCont, "")
        ''txtCont = txtCont.Replace(Environment.NewLine, "<br />")

        If txtRemote IsNot Nothing Then
            remoAddr = remoinf(0)                           ''リモートアドレス
            remoHost = remoinf(1)                           ''リモートホスト
        Else
            remoAddr = "Err"
            remoHost = "Err"
        End If
        writeUser = CType(Session.Item("UN"), String)   ''書き込みユーザ

        ''strSQL = "INSERT INTO お知らせ (識別名称,ユーザ名,表示期限,更新日時,内容,リモートアドレス,リモートホスト,削除,入力ユーザ)" + _
        ''        " VALUES ('" + IDName + "','" + userName + "',#" + LimitDate + "#,#" + startDate + "#,'" + txtCont + "','" + remoAddr + _
        ''        "','" + remoHost + "',FALSE,'" + writeUser + "')"

        Using DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & anno_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")
            ''DbCon.Open()
            Using DbDa As New OleDb.OleDbDataAdapter("SELECT * FROM お知らせ WHERE 更新日時 >= #" + Date.Now.AddMonths(-1).ToString + "#", DbCon)  '取り敢えずデータセット用にオープン
                Using DtSet As New DataSet("DData")

                    DbDa.Fill(DtSet, "DData")

                    Dim dr As DataRow
                    dr = DtSet.Tables("DData").NewRow()
                    dr(1) = IDName
                    dr(2) = userName
                    dr(3) = LimitDate
                    dr(4) = startDate
                    dr(5) = txtCont
                    dr(6) = remoAddr
                    dr(7) = remoHost
                    dr(8) = False
                    dr(9) = writeUser

                    DtSet.Tables("DData").Rows.Add(dr)
                    Using cmdbuilder As New OleDb.OleDbCommandBuilder
                        cmdbuilder.DataAdapter = DbDa
                        DbDa.Update(DtSet, "DData")                                                                              ' Databaseの更新

                        '' ''DbDa.InsertCommand = New OleDb.OleDbCommand()
                        '' ''DbDa.InsertCommand.CommandText = strSQL
                        '' ''DbDa.InsertCommand.Connection = DbCon
                        '' ''DbDa.Update(DtSet, "DData")

                        Me.GrdView.DataBind()

                        ''DbCon.Close()
                        'cmdbuilder.Dispose()
                        'DtSet.Dispose()
                        'DbDa.Dispose()
                        ' ''Dbcom = New OleDb.OleDbCommand(strSQL, DbCon)
                        ' ''Dbcom.ExecuteNonQuery()
                        ' ''Dbcom.Dispose()
                        'DbCon.Dispose()
                        clsUser = Nothing
                    End Using
                End Using
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' GridViewの表示調整
    ''' </summary>
    Protected Sub GrdView_RowCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdView.RowCreated
        ''
        '' グリッド内の行が作成されたときに発生するイベント
        ''
        ''Dim strTemp As String
        Dim intColNum As Integer

        If Me.DDLSiteName.Items.Count = 1 Then
            intColNum = 4
        Else
            intColNum = 5
        End If
        With e.Row
            If .RowType = DataControlRowType.DataRow Then
                ' onmouseover属性を設定　　'#CC99FF'
                .Attributes("onmouseover") = "setBg(this, '#84D7FF')"                       '' マウスオーバーで行に色を設定

                ' データ行が通常行／代替行であるかで処理を分岐（2）
                If .RowState = DataControlRowState.Normal Then
                    .Attributes("onmouseout") = String.Format("setBg(this, '{0}')", ColorTranslator.ToHtml(Me.GrdView.RowStyle.BackColor))
                Else
                    .Attributes("onmouseout") = String.Format("setBg(this, '{0}')", ColorTranslator.ToHtml(Me.GrdView.AlternatingRowStyle.BackColor))
                End If

                Dim btn As Button = CType(e.Row.FindControl("Button1"), Button)             '' 削除ボタンにインデックスを作成
                btn.CommandArgument = e.Row.RowIndex.ToString

                ''strTemp = .Cells(intColNum).Text
                ''strTemp = strTemp.Replace("<BR>", Environment.NewLine)
                ''.Cells(intColNum).Text = strTemp

            ElseIf .RowType = DataControlRowType.Header Then
                .HorizontalAlign = HorizontalAlign.Center

            End If
        End With
    End Sub

    ''' <summary>
    ''' GridViewの列幅調整
    ''' </summary>
    Protected Sub GrdView_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdView.RowDataBound

        Dim strTemp As String
        Dim intColNum As Integer

        If Me.DDLSiteName.Items.Count = 1 Then
            intColNum = 4
        Else
            intColNum = 5
        End If

        With e.Row
            If .RowType = DataControlRowType.DataRow Then
                strTemp = .Cells(intColNum).Text
                ''strTemp = strTemp.Replace("&lt;br&gt;", "<br>")
                .Cells(intColNum).Text = strTemp

                If intColNum = 4 Then
                    .Cells(0).Width = 40
                    .Cells(1).Width = 105
                    .Cells(2).Width = 150
                    .Cells(3).Width = 105
                Else
                    .Cells(0).Width = 40
                    .Cells(1).Width = 105
                    .Cells(2).Width = 150
                    ''.Cells(3).Attributes.Add("Style", "word-break:break-all")
                    .Cells(3).Text = .Cells(3).Text.Replace(",", ", ")
                    .Cells(3).Width = 80
                    ''.Cells(3).Font.Size = 7
                    .Cells(4).Width = 105
                End If

            ElseIf .RowType = DataControlRowType.Header Then

            End If
        End With

    End Sub

    ''' <summary>
    ''' HTMLにおいて改行を<BR />におきかえる これをしないと文字として表示されてしまう
    ''' </summary>
    Protected Sub GrdView_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GrdView.PreRender

        Dim r As TableRow
        Dim intColNum As Integer

        If Me.DDLSiteName.Items.Count = 1 Then
            intColNum = 4
        Else
            intColNum = 5
        End If


        Try
            If intColNum = 4 Then
                For Each r In CType(sender, GridView).Rows
                    r.Cells(4).Text = r.Cells(4).Text.Replace(Environment.NewLine, "<br />")
                Next r
            Else
                For Each r In CType(sender, GridView).Rows
                    r.Cells(5).Text = r.Cells(5).Text.Replace(Environment.NewLine, "<br />")
                Next r
            End If
        Catch ex As Exception
            Dim aa As Integer
            aa = 1
        End Try

    End Sub


End Class
