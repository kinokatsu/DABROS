Imports System.Data

Partial Class SiteSelect
    Inherits System.Web.UI.Page

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed

        'Session.RemoveAll()                     'セッション状態のコレクションからすべての値とキーを削除します。
        'Session.Abandon()                       '現在のセッションに破棄のマークを付けます（セッションを破棄する）
        'Session.Clear()

    End Sub


    Protected Sub Page_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error        ' 2011/05/30 Kino Add

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

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim intTemp As Integer
        Dim SiteNo As String = Nothing                   '' データベースより現場番号を取得
        Dim UserName As String = Nothing
        Dim Password As String = Nothing
        Dim LoginStatus As Integer
        Dim AllSiteNames As String = ""
        Dim AllSiteNo As String = ""
        Dim strTemp As String
        Dim showSubSiteName As String = CType(Request.Item("SSN"), String)
        Dim clsconvChData As New ClsReadDataFile
        Dim siteInf() As ClsReadDataFile.siteInfo = Nothing
        Dim selectedSiteNo As Integer
        ''Dim menuType As Integer = 0                 ' 2016/01/15 Kino Add
        ''Dim sb_AllSiteNames As New StringBuilder
        ''Dim sb_AllSiteNo As New StringBuilder
        'Dim alertShowFlg() As Integer               ' 2012/11/07

        If User.Identity.IsAuthenticated = False Then
            Response.Redirect("Login.aspx", False)
            Exit Sub
        End If

        If IsPostBack = False Then

            LoginStatus = CType(Session.Item("LgSt"), Integer)

            'ログインしていない場合は、ログイン画面へ
            If LoginStatus = 0 Then
                Response.Redirect("Login.aspx", False)
                Exit Sub
            End If

            UserName = CType(Session.Item("UN"), String)
            Password = CType(Session.Item("PW"), String)
            SiteNo = CType(Session.Item("SNo"), String)

            If SiteNo Is Nothing Then
                selectedSiteNo = 0
            Else
                selectedSiteNo = Convert.ToInt32(Session.Item("SSNO"))
            End If

            If SiteNo.IndexOf("-") <> 0 Then                                    'チャンネル文字列をチャンネルデータに変換
                'Dim clsconvChData As New ClsReadDataFile
                Dim dummy() As Integer = {}
                Dim strRet As String = clsconvChData.GetOutputChannel(dummy, SiteNo)
                SiteNo = strRet
                'clsconvChData = Nothing
            End If

            Dim SiteViewTitle As String = CType(Session.Item("TT"), String)

            'ユーザ名が登録されていなかったら、ログイン画面へ
            If LoginStatus = 0 Then
                Response.Redirect("Login.aspx", False)
            End If
            ''            End If
            Me.lblSiteName.Text = SiteViewTitle
            Me.MyTitle.Text = SiteViewTitle
            If IsPostBack = True Then Exit Sub

            ''    Me.RBLConstructionList.Items.Clear()
            ''            Me.RBLConstructionList.Items.Add(strComlete + "　" + DTR.Item(1).ToString)   '.PadRight(30, "　"))    'DTR.Item("SiteName").ToString)      '2009/12/21 Kino Changed
            ''            Me.RBLConstructionList.Items(iLoop).Value = DTR.Item(2).ToString + "@@@" + DTR.Item(4).ToString + "@@@" + DTR.Item(7).ToString       'DTR.Item("Address").ToString '2009/12/21 Kino Changed  2012/08/06 Kino Changed  Add MenuType
            ''            If iLoop = 20 Then
            ''                Me.RBLConstructionList.Items(iLoop).Attributes.Add("class", "ShowAreaAlert")                                        ' 警報発生個所表示用
            ''            End If
            ''    Me.RBLConstructionList.Items(0).Selected = True
            '' '' ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. 読込完了
            ' 表示用現場の一覧を取得してsiteInf()へ格納
            intTemp = getSiteInfomations(SiteNo, AllSiteNames, AllSiteNo, siteInf)                                                          ' 現場一覧を取得

            ' 警報値超過センサの取得(現場選択一覧表示における、現場の点滅表示のための情報取得）
            Dim sd As String = Request.PhysicalApplicationPath.ToString                                                                     ' サイトルートの物理パス
            Dim intTemp2 As Integer = clsconvChData.getSiteAlertStatus(sd, siteInf)                                                         ' 各現場の警報出力状況取得 2013/02/06 Kino Changed intTemp2

            Call setRadioButtonList(siteInf, selectedSiteNo)                                                                                ' 現場一覧作成

            Session("ASN") = AllSiteNames                                                                                                   '2010/05/18 Kino Add
            Session("ASNO") = AllSiteNo                                                                                                     '2010/05/18 Kino Add
            Session("SNUM") = intTemp                                                                                                         '2010/05/18 Kino Add

            clsconvChData = Nothing

        End If

        Dim opeID As String = ""
        If IsPostBack = True Then
            For iLoop = 0 To Page.Request.Form.Count - 1
                If Page.Request.Form.Keys(iLoop).EndsWith(".x") = True Then
                    opeID = Page.Request.Form.Keys(iLoop).Substring(0, Page.Request.Form.Keys(iLoop).Length - 2)
                End If
            Next
            'For Each ctl As String In Page.Request.Form                'イメージボタンはこれでは探せない・・・
            '    Dim c As Control = Page.FindControl(ctl)
            '    If TypeOf c Is System.Web.UI.WebControls.Button Then
            '        opeID = DirectCast(c, System.Web.UI.WebControls.Button).ClientID
            '        Exit For
            '    End If
            'Next
        End If

        If opeID <> "ImageButton1" Then                                 '決定ボタンならバインドしない
            ''If Page.Request.Params.Get("__EVENTTARGET") <> "ImageButton1" Then　　''ボタン系はこれで取得できない
            Call Bind2DB()
        End If

        ToolkitScriptManager1.SetFocus(Me.ImageButton1)

        If IsPostBack = False Then
            Dim AutoSiteNo As Integer = CType(Session.Item("AutoSiteNo"), Integer)                                                          'クエリ文字列で?SN= として現場番号を入れてログインすると自動的に現場を開く
            Dim NoteView As Integer = CType(Session.Item("NoteView"), Integer)
            AutoSiteNo -= 1                                     'インデックスが0からなので調整
            If AutoSiteNo >= 0 Then
                If Me.RBLConstructionList.Items(AutoSiteNo).Enabled = True Then
                    Me.RBLConstructionList.SelectedIndex = AutoSiteNo
                    Session("SN") = RBLConstructionList.Items(RBLConstructionList.SelectedIndex).Text                               '現場名
                    strTemp = RBLConstructionList.Items(RBLConstructionList.SelectedIndex).Value.ToString                           '現場ディレクトリ(ディレクトリ + 省略名称 ) '2009/12/21 Kino Add
                    ''intTemp = strTemp.IndexOf("@@@")                                                                                ' 2016/01/15 Kino Add      
                    ''Dim intTemp2 As Integer = strTemp.IndexOf("@@@", intTemp + 3)                                                   ' 2016/01/15 Kino Add
                    Dim strSplit As String() = strTemp.Split(Convert.ToChar("|"))
                    ''menuType = CType(strTemp.Substring(intTemp2 + 3), Integer)                                                      ' 2016/01/15 Kino Add
                    ''Session("SD") = strTemp.Substring(0, strTemp.IndexOf("@@@"))                                                    ' 2009/12/21 Kino Changed        '現場ディレクトリ(ディレクトリのみ)
                    Session.Item("SD") = strSplit(0)                                                                                         ' 2017/02/23 Kino Add
                    ''Session("MnSel") = "TOP"                                                                                        '選択メニュー
                    ''Session("SSN") = strTemp.Substring(strTemp.IndexOf("@@@") + 3, (intTemp2 - strTemp.IndexOf("@@@") - 3))         ' 2016/01/15 Kino Add   現場省略名称
                    Session.Item("SSN") = strSplit(1)                                                                                        ' 2017/02/23 Kino Add
                    Session.Item("SSNo") = Me.RBLConstructionList.SelectedIndex.ToString                                                 ' 2016/01/15 Kino Add
                    Session.Item("NoteView") = 1                                                                                    ' 2015/04/28 Kino Add これが0だとあると、現場切り替えができないので、最初だけ使用してあとは1にする
                    Session.Add("AutoSiteNo", 0)                                                                                    ' 2016/01/15 Kino Add 0クリアしないとHiddenFieldを使用した場合に現場一覧がでない
                    If NoteView = 1 Then
                        ''For iLoop = 0 To Me.ConstructionList.Items.Count - 1                                              ' 2015/04/28 Kino Add これがあると、オプションが選択できないのでコメント
                        ''    If iLoop <> AutoSiteNo Then
                        ''        Me.RBLConstructionList.Items(iLoop).Enabled = False
                        ''    End If
                        ''Next
                    Else
                        'Response.Redirect("main.aspx", False)                                                                      ' 2016/01/15 Kino Add
                        Dim menuType As String = strSplit(2).ToString
                        If menuType.EndsWith(".aspx") = False Then

                            If menuType = "0" Then                                                                                        ' 2016/01/15 Kino Add メニュータイプ追加
                                menuType = "./main.aspx"
                                ''Response.Redirect("main.aspx", False)
                            Else
                                Dim strMenu As String = String.Format("./main{0:d2}.aspx", Convert.ToInt16(menuType))

                                menuType = "./main01.aspx"
                                ''Response.Redirect("main01.aspx", False)
                            End If
                        End If
                        Response.Redirect(menuType, False)
                    End If
                End If
            End If
        End If

    End Sub

    Protected Sub setRadioButtonList(siteInf() As ClsReadDataFile.siteInfo, selectedSiteNo As Integer, Optional ByVal subSiteName As String = Nothing)

        Dim iloop As Integer
        Dim jloop As Integer

        With Me.RBLConstructionList
            .Items.Clear()

            For iloop = 0 To siteInf.Length - 1
                .Items.Add(siteInf(iloop).Name)
                ''.Items(iloop).Value = siteInf(iloop).Folder & "@@@" & siteInf(iloop).shortName & "@@@" & siteInf(iloop).menuType
                .Items(iloop).Value = siteInf(iloop).Folder & "|" & siteInf(iloop).shortName & "|" & siteInf(iloop).menuType

                For jloop = 0 To siteInf(iloop).alertMaxLevel.Length - 1
                    If siteInf(iloop).alertMaxLevel(jloop) <> 0 AndAlso siteInf(iloop).alertMaxLevel(jloop) <> -99 Then
                        .Items(iloop).Attributes.Add("class", "ShowAreaAlert")
                        Exit For
                    End If
                Next

                ''    Me.RBLConstructionList.Items.Clear()
                ''            Me.RBLConstructionList.Items.Add(strComlete + "　" + DTR.Item(1).ToString)   '.PadRight(30, "　"))    'DTR.Item("SiteName").ToString)      '2009/12/21 Kino Changed
                ''            Me.RBLConstructionList.Items(iLoop).Value = DTR.Item(2).ToString + "@@@" + DTR.Item(4).ToString + "@@@" + DTR.Item(7).ToString       'DTR.Item("Address").ToString '2009/12/21 Kino Changed  2012/08/06 Kino Changed  Add MenuType
                ''            If iLoop = 20 Then
                ''                Me.RBLConstructionList.Items(iLoop).Attributes.Add("class", "ShowAreaAlert")                                        ' 警報発生個所表示用
                ''            End If
                ''    Me.RBLConstructionList.Items(0).Selected = True


            Next

            .Items(selectedSiteNo).Selected = True

        End With

    End Sub

    ''' <summary>
    ''' UserAndPassword.mdbから指定した番号の現場一覧を取得して変数へ格納する
    ''' </summary>
    ''' <param name="siteNo">取得する現場番号を指定</param>
    ''' <param name="allSiteNames">取得した現場名をCSVで戻す</param>
    ''' <param name="allSiteNo">取得した現場番号をCSVで戻す</param>
    ''' <returns>取得した現場数</returns>
    ''' <remarks></remarks>
    Protected Function getSiteInfomations(siteNo As String, ByRef allSiteNames As String, ByRef allSiteNo As String, ByRef siteInf() As ClsReadDataFile.siteInfo) As Integer

        Dim clsUser As New ClsCheckUser
        Dim UserInformation_DatabaseFile As String = clsUser.UserInformation_DatabaseFile
        Dim retTemp As Integer = 0

        If IO.File.Exists(UserInformation_DatabaseFile) = True Then
            Dim DbCon As New OleDb.OleDbConnection
            Dim DbDa As OleDb.OleDbDataAdapter
            Dim DtSet As New DataSet
            Dim siteComplete As String
            Dim iloop As Integer = 0
            Dim sb_AllSiteNames As New StringBuilder
            Dim sb_AllSiteNo As New StringBuilder

            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + UserInformation_DatabaseFile + ";" + "Jet OLEDB:Engine Type= 5")
            '' 現場名称の読込
            DbDa = New OleDb.OleDbDataAdapter("SELECT* FROM ConstructionSite", DbCon)
            DtSet = New DataSet("DData")
            DbDa.Fill(DtSet, "DData")
            Dim existRow As Boolean = CType(DtSet.Tables("DData").Rows.Count + 1, Boolean)

            If existRow = True Then
                Try

                    DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM ConstructionSite WHERE SiteNo IN(" + siteNo + ") ORDER BY Complete DESC,Type,OrderInType,SiteNo ASC", DbCon)

                    DtSet = New DataSet("ConstructionSite")
                    DbDa.Fill(DtSet, "ConstructionSite")
                    Dim dataRowsCount As Integer = DtSet.Tables("ConstructionSite").Rows.Count
                    ReDim siteInf(dataRowsCount - 1)
                    If dataRowsCount <> 0 Then
                        For Each DTR As DataRow In DtSet.Tables("ConstructionSite").Rows

                            If CBool(DTR.Item("Complete")) = False Then
                                siteComplete = ""                                               ' 2015/12/23 Kino Changed  " " -> ""
                            Else
                                siteComplete = "【完了現場】 "
                            End If

                            With siteInf(iloop)
                                .Name = siteComplete & DTR.Item(1).ToString
                                .Folder = DTR.Item(2).ToString
                                .shortName = DTR.Item(4).ToString
                                .menuType = DTR.Item(7).ToString    'CType(DTR.Item(7), Short)
                                .subSite = DTR.Item(8).ToString
                                .showAlertFlg = CType(DTR.Item(9), Short)
                                '.alertMaxLevel = 0
                                '.allSensorSymbol = ""
                            End With
                            sb_AllSiteNames.Append(DTR.Item(1).ToString).Append(",")
                            sb_AllSiteNo.Append(DTR.Item(0).ToString).Append(",")
                            iloop += 1
                        Next
                    End If
                    allSiteNames = sb_AllSiteNames.ToString.Substring(0, sb_AllSiteNames.Length - 1)
                    allSiteNo = sb_AllSiteNo.ToString.Substring(0, sb_AllSiteNo.Length - 1)
                    retTemp = dataRowsCount

                    If iloop <= 10 Then
                        'Dim txtbox As HtmlControls.HtmlControl = CType(FindControl("gsearchbox"), HtmlControls.HtmlControl)
                        'Me.Controls.Remove(txtbox)
                        Me.gsearchbox.Visible = False
                    End If

                Catch ex As Exception
                    retTemp = -1
                End Try
            Else
                retTemp = -1
            End If
        End If

        clsUser = Nothing

        Return retTemp

    End Function


    ''Protected Sub makeSiteNames(DbCon As OleDb.OleDbConnection, SiteNo As String)

    ''    Dim DbDa As OleDb.OleDbDataAdapter
    ''    Dim DtSet As DataSet
    ''    Dim alertShowFlg() As Integer               ' 2012/11/07
    ''    Dim strComlete As String = Nothing
    ''    Dim sb_AllSiteNames As New StringBuilder
    ''    Dim sb_AllSiteNo As New StringBuilder

    ''    Me.RBLConstructionList.Items.Clear()
    ''    DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM ConstructionSite WHERE SiteNo IN(" + SiteNo + ") ORDER BY Complete DESC,Type,OrderInType,SiteNo ASC", DbCon)
    ''    ''
    ''    DtSet = New DataSet("ConstructionSite")
    ''    DbDa.Fill(DtSet, "ConstructionSite")

    ''    Dim dataRowsCount As Integer = DtSet.Tables("ConstructionSite").Rows.Count
    ''    ReDim alertShowFlg(dataRowsCount - 1)
    ''    '' GetAlertShowStatus()         ' 警報出力状態を取得　●●●●●● これから作成
    ''    If dataRowsCount <> 0 Then
    ''        ''For iLoop = 0 To DtSet.Tables("ConstructionSite").Rows.Count - 1        'UBound(SiteData)
    ''        For Each DTR As DataRow In DtSet.Tables("ConstructionSite").Rows

    ''            ''If DtSet.Tables("ConstructionSite").Rows(iLoop).Item("Complete") = False Then
    ''            If CBool(DTR.Item("Complete")) = False Then
    ''                strComlete = ""
    ''            Else
    ''                strComlete = "【完了現場】"
    ''            End If

    ''            Me.RBLConstructionList.Items.Add(strComlete + "　" + DTR.Item(1).ToString)   '.PadRight(30, "　"))    'DTR.Item("SiteName").ToString)      '2009/12/21 Kino Changed
    ''            Me.RBLConstructionList.Items(iLoop).Value = DTR.Item(2).ToString + "@@@" + DTR.Item(4).ToString + "@@@" + DTR.Item(7).ToString       'DTR.Item("Address").ToString '2009/12/21 Kino Changed  2012/08/06 Kino Changed  Add MenuType
    ''            '' If iLoop = 2 Then Me.ConstructionList.Items(2).Enabled = False                                               ' 2011/06/08 Kino Add 表示はするが選択不可とする場合に使用する
    ''            sb_AllSiteNames.Append(DTR.Item(1).ToString).Append(",")                                                            ' 2012/08/29 Kino Add
    ''            sb_AllSiteNo.Append(DTR.Item(0).ToString).Append(",")                                                               ' 2012/08/29 Kino Add
    ''            'If alertShowFlg(iLoop) = 1 Then
    ''            If iLoop = 20 Then
    ''                Me.RBLConstructionList.Items(iLoop).Attributes.Add("class", "ShowAreaAlert")                                        ' 警報発生個所表示用
    ''            End If
    ''            ''AllSiteNames += (DTR.Item(1).ToString + ",")                                                                    '2010/05/18 Kino Add 2012/08/29 Kino Changed
    ''            ''AllSiteNo += (DTR.Item(0).ToString + ",")
    ''            iLoop += 1

    ''        Next
    ''    End If
    ''    Me.RBLConstructionList.Items(0).Selected = True


    ''End Sub

    ''' <summary>
    ''' お知らせのDBをGridViewにバインドする
    ''' </summary>
    Protected Sub Bind2DB()
        ''
        '' データファイルおよびSQLのクエリを用いてデータベースにバインドする        2010/05/06 Kino Add
        ''
        Dim strSQL As String = ""
        Dim clsUser As New ClsCheckUser
        Dim DataFile As String = clsUser.UserAnnouncement_DatabaseFile
        Dim UserKey As String
        Dim UserGroup As String
        Dim AllSiteNames As String = CType(Session.Item("ASN"), String)
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
            strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 識別名称 AS 現場名称, 内容 FROM お知らせ WHERE (識別名称 IN (" + showSiteName + ") AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
        Else
            If Me.RBLConstructionList.Items.Count = 1 Then         '単一現場の場合
                strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 内容 FROM お知らせ WHERE (((識別名称 IN (" + showSiteName + ") AND " + _
                        "(INSTR(ユーザ名,'@" & UserKey & "@')<>0 OR INSTR(ユーザ名,'@" & UserGroup & "@')<>0))  OR (INSTR(ユーザ名,'@ALL@')<>0)) AND " + _
                        "(表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
                ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 表示開始日時, 内容 FROM お知らせ WHERE (識別名称 IN (" + showSiteName + ") AND (ユーザ名 LIKE " + dq + "%@" & UserKey & "@%" + dq + " OR ユーザ名 LIKE " + dq + "%@ALL@%" + dq + ") AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
            Else                                                '複数現場の場合
                strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時, 識別名称 AS 現場名称, 内容 FROM お知らせ WHERE (((識別名称 IN (" + showSiteName + ") AND " + _
                        "(INSTR(ユーザ名,'@" & UserGroup & "@')<>0)) OR (INSTR(ユーザ名,'@ALL@')<>0)) AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
                ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 更新日時, 識別名称 AS 現場名称, 内容 FROM お知らせ WHERE (識別名称 IN (" + showSiteName + ") AND ('ユーザ名' LIKE " + dq + "%@" & UserKey & "@%" + dq + " OR 'ユーザ名' LIKE " + dq + "%@ALL@%" + dq + ") AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
            End If
            ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時,内容 FROM お知らせ WHERE (識別名称 IN(" + showSiteName + ") AND ('ユーザ名' LIKE " + dq + "%@" & UserKey & "@%" + dq + " OR 'ユーザ名' LIKE " + dq + "%@ALL@%" + dq + ") AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"  ''SQL Server はこっちかな・・
            ''strSQL = "SELECT FORMAT(更新日時, 'yyyy/mm/dd hh:mm') AS 日時,内容 FROM お知らせ WHERE (識別名称 IN (" + showSiteName + ") AND (INSTR(ユーザ名,'@" & UserKey & "@')<>0 OR INSTR(ユーザ名,'@ALL@')<>0) AND (表示期限 >= NOW()) AND (削除=False) AND (更新日時 <= NOW())) ORDER BY 更新日時 DESC"
        End If

        '---DataObjectSourceにDBをバインドする---
        Me.AccessDataSource1.DataFile = DataFile
        Me.AccessDataSource1.SelectCommand = strSQL
        Me.GrdView.DataSourceID = Me.AccessDataSource1.ID
        Me.AccessDataSource1.DataBind()

        clsUser = Nothing

    End Sub

    ''' <summary>
    ''' 選択された現場の表示を行う
    ''' </summary>
    Protected Sub ImageButton1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImageButton1.Click
        Dim strTemp As String = ""              '2009/12/21 Kino Add
        Dim shortName As String = ""            '2009/12/21 Kino Add
        Dim menuType As String = Nothing    'Integer = 0             ' 2012/08/06 Kino Add

        If Me.RBLConstructionList.SelectedIndex <> -1 Then
            Dim SiteDirectory As String
            Session("SN") = Trim$(RBLConstructionList.Items(RBLConstructionList.SelectedIndex).Text)   '現場名
            ''Session("FSD") = Server.MapPath(ConstructionList.Items(ConstructionList.SelectedIndex).Value.ToString)    '現場ディレクトリ(フルパス)
            strTemp = RBLConstructionList.Items(RBLConstructionList.SelectedIndex).Value.ToString                       '現場ディレクトリ(ディレクトリ + 省略名称 ) '2009/12/21 Kino Add
            '------------------------- 2017/01/06 Kino Changed  to lower ↓
            ''Dim intTemp As Integer = strTemp.IndexOf("@@@")                                                             ' 2012/08/06 Kino Add
            ''SiteDirectory = strTemp.Substring(0, intTemp)                                                               ' 2009/12/21 Kino Changed 2012/08/06 Kino Changed
            ''Dim intTemp2 As Integer = strTemp.IndexOf("@@@", intTemp + 3)                                               ' 2012/08/06 Kino Add
            ''shortName = strTemp.Substring(strTemp.IndexOf("@@@") + 3, (intTemp2 - strTemp.IndexOf("@@@") - 3))          ' 2009/12/21 Kino Add 2012/08/06 Kino Changed
            ''menuType = CType(strTemp.Substring(intTemp2 + 3), Integer)                                                  ' 2012/08/06 Kino Add
            '------------------------- 2017/01/06 Kino Add
            Dim strInf() As String = strTemp.Split(Convert.ToChar("|"))
            SiteDirectory = strInf(0)
            shortName = strInf(1)
            menuType = strInf(2)
            '------------------------- 
            Session("SD") = SiteDirectory
            Session("SSN") = shortName                                              '2009/12/21 Kino Add
            'Session("MnSel") = "TOP"                                                                                    '選択メニュー    2018/07/01 Kino Changed 使用していないようなのでコメントとする
            Session("SSNo") = Me.RBLConstructionList.SelectedIndex.ToString

            'Session.Add("SN", ConstructionList.Items(ConstructionList.SelectedIndex).Text)                         '現場名
            'Session.Add("SD", ConstructionList.Items(ConstructionList.SelectedIndex).Value.ToString)               '現場ディレクトリ
            'Session.Add("SNo", Session.Item("SNo"))                                                                '登録されている現場番号
            ' '' ''Session.Add("UN", Session.Item("UN"))                                                                'ユーザ名
            ' '' ''Session.Add("PW", Session.Item("PW"))                                                                'パスワード
            'Session.Add("LgSt", CInt(Session.Item("LgSt")))                                                        'ログインステータス
            'Session.Add("MnSel", "TOP")                                                                            '選択メニュメニュー

            If IO.Directory.Exists(Server.MapPath(SiteDirectory)) = True And IO.File.Exists(Server.MapPath(SiteDirectory + "\App_Data\MenuInfo.mdb")) = True Then

                If menuType.EndsWith(".aspx") = False Then
                    menuType = "./main01.aspx"
                End If
                Response.Redirect(menuType, False)                                                                    ' 2017/01/06 Kino Add  Change from lower  これでDBの設定から起動トップ画面を選択できるようになる（Page_Loadも確認）
                HttpContext.Current.ApplicationInstance.CompleteRequest()       '' System.Threading.ThreadAbortException' の例外対策

                ''If menuType = "0" Then                                                                                    ' 2012/08/06 Kino Add メニュータイプ追加
                ''    Response.Redirect("main.aspx", False)
                ''Else
                ''    Response.Redirect("main01.aspx", False)
                ''End If
                '' ''Server.Transfer("main.aspx")
            End If
        End If
    End Sub

    ''' <summary>
    ''' HTMLにおいて改行を<BR />におきかえる これをしないと文字として表示されてしまう
    ''' </summary>
    Protected Sub GrdView_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GrdView.PreRender

        ''For Each rr As TableRow In sender.rows
        ''    For Each TC As TableCell In rr.Cells
        ''        For Each CTL As Control In TC.Controls
        ''            If TypeOf CTL Is Label Then
        ''                CType(CTL, Label).Text = CType(CTL, Label).Text.Replace(Environment.NewLine, "<br />")
        ''            End If
        ''        Next
        ''    Next
        ''Next

        Dim r As TableRow
        For Each r In CType(sender, GridView).Rows
            Dim tc As TableCell
            For Each tc In r.Cells
                tc.Text = tc.Text.Replace(Environment.NewLine, "<br />")
                'r.Cells(2).Text = r.Cells(2).Text.Replace(Environment.NewLine, "<br />")            '"&lt;br&gt;", "<br />")
                ''Dim c As Control
                ''tc.Text = tc.Text.Replace("&lt;br&gt;", "<br />")
                '' ''For Each c In tc.Controls
                '' ''    If TypeOf c Is Label Then
                '' ''CType(c, Label).Text = CType(c, Label).Text.Replace("<br>", "<br />")    'Environment.NewLine, "<br />")
                ''End If
                ''Next c
            Next tc
        Next r

    End Sub


    ''' <summary>
    ''' ヘッダの調整
    ''' </summary>
    Protected Sub GrdView_RowCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdView.RowCreated
        Dim iloop As Integer

        With e.Row
            If .RowType = DataControlRowType.Header Then
                For iloop = 0 To .Cells.Count - 1
                    .Cells(iloop).HorizontalAlign = HorizontalAlign.Center                                                      '2009/02/02 Kino Add
                    If iloop = 0 Then
                        If Me.RBLConstructionList.Items.Count = 1 Then
                            .Cells(0).Width = 110
                        Else
                            .Cells(0).Width = 110
                            .Cells(1).Width = 170
                        End If
                    End If
                Next iloop
                ''ElseIf .RowType = DataControlRowType.DataRow Then                                                             ' 2016/04/07 Kino Changed これがあると落ちる
                ''    .Cells(2).HorizontalAlign = HorizontalAlign.Left
            End If
        End With
    End Sub


    'Protected Sub GrdView_RowCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdView.RowCreated

    '    With e.Row
    '        .Height = 25
    '    End With

    'End Sub


    ''Protected Sub GrdView_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdView.RowDataBound

    ''    Dim strTemp As String

    ''    With e.Row
    ''        If .RowType = DataControlRowType.DataRow Then
    ''            strTemp = .Cells(2).Text
    ''            strTemp = strTemp.Replace("&lt;br&gt;", "<br />")
    ''        End If
    ''    End With

    ''End Sub


End Class
