Imports System.Data
Imports System.Data.OleDb
''Imports System.Web.Caching
Imports System.Net
Imports System.Web.UI.Page
''Imports C1.Web.Command
''Imports C1.Web.UI.Controls.C1Menu
Imports C1.Web.UI.Controls.C1Menu
''Imports C1.Web.Wijmo.Controls.C1Menu

Partial Class MasterPage
    Inherits System.Web.UI.MasterPage

    Private Structure TopMenuText
        Dim itemText As String
        Dim naviURL As String
        Dim target As String
        Dim GraphTitle As String
        Dim Layer As String
        Dim Width As Integer
        Dim Height As Integer
    End Structure

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed

        ''Session.RemoveAll()                     'セッション状態のコレクションからすべての値とキーを削除します。   2011/06/08 Kino Changed これはすべての情報を破棄してしまうらしい。
        Session.Remove("LgSt")                  ' ログインステータスのセッションを削除
        Session.Abandon()                       '現在のセッションに破棄のマークを付けます（セッションを破棄する）　現在のセッションをキャンセルします。
        Session.Clear()                         ' セッション状態のコレクションからすべてのキーと値を削除します。  

    End Sub

    'Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

    'End Sub

    Protected Sub WriteErrorLog(ByVal ProcName As String, ByVal ex As Exception)

        Dim sw As IO.StreamWriter = Nothing

        Try
            If ex IsNot Nothing Then
                sw = New IO.StreamWriter(Server.MapPath("~/log/errorSummary.log"), True, Encoding.GetEncoding("Shift_JIS"))
                Dim sb As New StringBuilder
                sb.Append(DateTime.Now.ToString)
                sb.Append(ControlChars.Tab)
                sb.Append(ProcName)
                sb.Append(ControlChars.Tab)
                sb.Append(CType(Session.Item("UN"), String))
                sb.Append(ControlChars.Tab)
                Dim strTemp As String = Nothing
                If strTemp IsNot Nothing Then
                    sb.Append(Session.Item("SD").ToString)
                Else
                    sb.Append("---")
                End If
                sb.Append(ControlChars.Tab)
                sb.Append(ex.Message.ToString)
                sb.Append(ControlChars.Tab)
                sb.Append(ex.StackTrace.ToString)
                sw.WriteLine(sb.ToString)
            End If
        Catch

        Finally
            sw.Dispose()
        End Try

    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

        Dim ctrlName As String = Page.Request.Params.Get("__EVENTTARGET")

        If ctrlName IsNot Nothing AndAlso ctrlName = "ctl00$LoginStatus1$ctl00" Then
            Exit Sub
        End If

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        ''Dim MyMenu As Menu = Cache("MyMenuSet")
        ''Dim MyMenu As C1.Web.Command.C1WebMenu = Cache("MyMenuSet")

        'If IsPostBack = False Then

        Dim SiteName As String
        Dim SiteDirectory As String
        Dim SelItem As String
        Dim SiteNo As String

        Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)
        'ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            System.Threading.Thread.Sleep(5000)                                                     ' 2010/06/01 Kino Add 再取得確認
            LoginStatus = CType(Session.Item("LgSt"), Integer)
            If LoginStatus = 0 Then
                If Server.GetLastError() IsNot Nothing Then
                    Dim ex As Exception = Server.GetLastError().GetBaseException()
                    Call WriteErrorLog(MyBase.GetType.BaseType.FullName & "LoginStatus not Found by Masterpage", ex)
                End If
                Response.Redirect("Login.aspx")
                Exit Sub
            End If
        End If

        SiteName = Session.Item("SN")                           '現場名
        SiteDirectory = Session.Item("SD")                      '現場ディレクトリ
        ''            SelItem = Session.Item("MnSel")           '選択されたメニュー項目(この場合はTOP)
        SiteNo = Session.Item("SNo")
        SelItem = "TOP"                                         ' のため、直接記載

        Me.LitTitle.Text = SiteName

        If IsPostBack = False Then
            Call SetMenuItemsNew()
        End If

        ''MyMenu = Me.C1WMenu
        ''Cache("MyMenuSet") = MyMenu

        '' ''If [String].IsNullOrEmpty(SelItem) = False Then
        '' ''    Me.mnuSelect.FindItem(SelItem).Selected = True
        '' ''    If SelItem = "LNK" Then
        '' ''        Me.mnuSelect.FindItem(SelItem).Selected = False
        '' ''    End If
        '' ''End If

        '' ''天気情報表示               'とりあえず、商用仕様に該当する可能性が高いのでコメントとする
        ''Call setXMSDataFile()

        'End If

        ''HTMLソース内　サウンド再生のため、method=getとしていたが、これがあるとViewStateが多いときに不具合がでるため削除
        ''    <form id="MasterPageFrm" runat="server" method="get">

    End Sub

    Protected Sub LoginStatus1_LoggingOut(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.LoginCancelEventArgs) Handles LoginStatus1.LoggingOut

        ''Session.RemoveAll()                     'セッション状態のコレクションからすべての値とキーを削除します。   2011/06/08 Kino Changed これはすべての情報を破棄してしまうらしい。
        Session.Remove("LgSt")                  ' ログインステータスのセッションを削除
        Session.Abandon()                       '現在のセッションに破棄のマークを付けます（セッションを破棄する）　現在のセッションをキャンセルします。
        Session.Clear()                         ' セッション状態のコレクションからすべてのキーと値を削除します。  

    End Sub


    Protected Sub SetMenuItemsNew()
        ''
        ''メニューにグラフ出力項目を設定する
        ''
        Dim DbCon As New OleDbConnection
        '' Dim DbDr As OleDb.OleDbDataReader
        '' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        ''Dim DbDr_Detail As OleDbDataReader
        ''Dim DbCom_Detail As New OleDbCommand
        Dim strSql As String
        Dim SiteDirectory As String
        Dim GrpDataFile As String
        Dim ItemNo As Integer = 0
        Dim ChildItemNo As Short = 0
        Dim TopMenus() As TopMenuText                   ' 構造体
        Dim subMenus() As TopMenuText                   ' 構造体
        Dim TopMenuCount As Integer
        Dim subMenuCount As Integer
        Dim dmy As Short
        Dim iloop As Integer
        Dim jloop As Integer
        Dim menuItem As C1MenuItem
        Dim subMenu As C1MenuItem = New C1MenuItem()  'C1WebSubMenu()
        Dim subMenuItem As C1MenuItem = New C1MenuItem          'C1WebMenuItem = New C1WebMenuItem
        Dim sub2Menu As C1MenuItem = New C1MenuItem()       'C1WebSubMenu = New C1WebSubMenu()
        Dim sep As C1MenuItemSeparator                          'Dim sep As C1WebSeparator = New C1WebSeparator()
        ''Dim sub2MenuItem As C1WebMenuItem
        ''Dim sub3Menu As C1WebSubMenu = New C1WebSubMenu()
        ''Dim sub3MenuItem As C1WebMenuItem
        Dim firstLevel As String = "00"
        Dim secondLevel As String = "00"
        Dim thirdLevel As String = "00"
        Dim preFirstLebel As String = "00"
        ''Dim subItems As New C1WebSubMenu
        Dim clsLevelCheck As New ClsCheckUser
        Dim ULNo(5) As String
        SiteDirectory = Session.Item("SD")      '現場ディレクトリ
        Dim uLevel As Integer = CType(Session.Item("UL"), Integer)
        Dim userLevel As String = uLevel.ToString("000000")     'ユーザレベル

        ''セッションタイムアウトした時には、ログイン画面を出す
        If [String].IsNullOrEmpty(SiteDirectory) = True Then Response.Redirect("Login.aspx")

        ''ユーザレベル情報取得　4ケタ作成しているが現在、実際に使用しているのは右から２ケタのみ 2009/05/20
        Dim intTemp As Integer = clsLevelCheck.GetWord(userLevel, ULNo)

        GrpDataFile = Server.MapPath(SiteDirectory & "\App_Data\MenuInfo.mdb")
        If System.IO.File.Exists(GrpDataFile) = True Then
            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & GrpDataFile & ";" & "Jet OLEDB:Engine Type= 5"
            ''DbCon.Open()

            ''項目数を取得する
            strSql = "Select COUNT('グラフ種別') as GraphCount From メニュー追加グラフ Where 出力 = True"
            DbDa = New OleDb.OleDbDataAdapter(strSql, DbCon)
            DbDa.Fill(DtSet, "DData")
            ''DbCom.Connection = DbCon
            ''DbCom.CommandText = strSql
            ''DbDr = DbCom.ExecuteReader
            ''DbDr.Read()
            ''TopMenuCount = DbDr.GetInt32(0)
            ''DbDr.Close()                            'データリーダを閉じる
            TopMenuCount = DtSet.Tables("DData").Rows(0).Item(0)
            ReDim TopMenus(TopMenuCount - 1)        '配列の宣言と内容クリア

            ''メニュー項目を取得する
            strSql = "SELECT * FROM メニュー追加グラフ WHERE (((メニュー追加グラフ.出力)=True)) ORDER BY メニュー追加グラフ.[No]"
            DtSet = New DataSet
            DbDa = New OleDb.OleDbDataAdapter(strSql, DbCon)
            DbDa.Fill(DtSet, "DData")
            ''DbCom.Connection = DbCon
            ''DbCom.CommandText = strSql
            ''DbDr = DbCom.ExecuteReader
            For Each DTR As DataRow In DtSet.Tables("DData").Rows                                                   'グラフ数でループ
                With DTR.ItemArray
                    TopMenus(dmy).itemText = .GetValue(1)
                    TopMenus(dmy).naviURL = .GetValue(2)
                    TopMenus(dmy).GraphTitle = .GetValue(3)
                    TopMenus(dmy).target = .GetValue(5)
                    TopMenus(dmy).Width = .GetValue(6)
                    TopMenus(dmy).Height = .GetValue(7)
                    dmy += 1
                End With
            Next

            ''If DbDr.HasRows = True Then
            ''    Do While DbDr.Read()
            ''        TopMenus(dmy).itemText = DbDr.GetString(1)
            ''        TopMenus(dmy).naviURL = DbDr.GetString(2)
            ''        TopMenus(dmy).GraphTitle = DbDr.GetString(3)
            ''        TopMenus(dmy).target = DbDr.GetString(5)
            ''        TopMenus(dmy).Width = DbDr.GetInt16(6)
            ''        TopMenus(dmy).Height = DbDr.GetInt16(7)
            ''        dmy += 1
            ''    Loop
            ''End If
            ''DbDr.Close()                            'データリーダを閉じる

            C1WMenu.Items.Clear()                   'メニューのリセット
            subMenuItem.NestedGroupOrientation = C1MenuItemsOrientation.Vertical
            subMenu.NestedGroupOrientation = C1MenuItemsOrientation.Vertical
            sub2Menu.NestedGroupOrientation = C1MenuItemsOrientation.Vertical

            ''下層のメニューを作成
            For iloop = 0 To TopMenuCount - 1

                '●●ルートメニューを追加
                ''C1WMenu.Items.Add(New C1WebMenuItem(TopMenus(iloop).itemText, TopMenus(iloop).naviURL))                           
                ''If userLevel > 0 And TopMenus(iloop).itemText = "テキストファイル" Then GoTo NextStep 'ユーザレベルによって、テキストファイルのメニューを作成しない
                If (ULNo(0) = "0" And TopMenus(iloop).GraphTitle = "TXT") Or (ULNo(1) = "0" And TopMenus(iloop).GraphTitle = "MNG") Then
                    GoTo NextStep 'ユーザレベルによって、テキストファイルのメニューを作成しない
                End If

                menuItem = New C1MenuItem(TopMenus(iloop).itemText)                                      ' プルダウンの最上層を作成
                C1WMenu.Items.Add(menuItem)
                'menuItem.NestedGroupWidth = 120
                menuItem.ID = "mn" & iloop.ToString
                If TopMenus(iloop).naviURL <> "#" Then
                    menuItem.Target = TopMenus(iloop).target

                    If TopMenus(iloop).naviURL <> "-" Then                                                      '   最上層項目にリンク作成
                        If TopMenus(iloop).target = "_self" Then
                            menuItem.NavigateUrl = TopMenus(iloop).naviURL
                        Else
                            menuItem.NavigateUrl = TopMenus(iloop).naviURL & "?W=" & TopMenus(iloop).Width & "&H=" & TopMenus(iloop).Height
                        End If
                    End If
                Else
                    menuItem.NavigateUrl = "#"
                End If
                C1WMenu.Items.Add(New C1MenuItemSeparator())                                                     'セパレータを追加

                If TopMenus(iloop).itemText <> "リンク" Then
                    ''項目数を取得
                    ''DbCom_Detail = New OleDbCommand("SELECT COUNT(項目名) as Rowcount FROM メニュー基本情報 WHERE 種別='" & TopMenus(iloop).itemText & "'", DbCon)
                    ''DbDr_Detail = DbCom_Detail.ExecuteReader
                    ''DbDr_Detail.Read()
                    strSql = "SELECT COUNT(項目名) as Rowcount FROM メニュー基本情報 WHERE 種別='" & TopMenus(iloop).itemText & "'"
                    DtSet = New DataSet
                    DbDa = New OleDb.OleDbDataAdapter(strSql, DbCon)
                    DbDa.Fill(DtSet, "DData")
                    '' subMenuCount = DbDr_Detail.GetInt32(0)
                    subMenuCount = DtSet.Tables("DData").Rows(0).Item(0)

                    If subMenuCount = 0 Then GoTo NextStep
                    ReDim subMenus(subMenuCount - 1)
                    ''DbDr.Close()

                    '出力情報テーブルに作図情報があれば、メニューに項目を追加する
                    ''DbCom_Detail = New OleDbCommand("SELECT 項目名,種別,グラフタイトル,URL,メニュー階層,幅,高さ FROM メニュー基本情報 WHERE 種別='" & TopMenus(iloop).itemText & "' ORDER BY ID,メニュー階層 ASC", DbCon)
                    ''DbDr_Detail = DbCom_Detail.ExecuteReader
                    strSql = "SELECT 項目名,種別,グラフタイトル,URL,メニュー階層,幅,高さ FROM メニュー基本情報 WHERE 種別='" & TopMenus(iloop).itemText & "' ORDER BY ID,メニュー階層 ASC"
                    DtSet = New DataSet
                    DbDa = New OleDb.OleDbDataAdapter(strSql, DbCon)
                    DbDa.Fill(DtSet, "DData")

                    If DtSet.Tables("DData").Rows.Count <> 0 Then
                        dmy = 0
                        menuItem.Text += " <img align=""Baseline"" src=""./img/es.png"" class=""menuarrow"" />"       '' "<img style= vertical-align:middle; src=" + Chr(34) + "./img/es.png" + Chr(34) + " />"
                        For Each DTR As DataRow In DtSet.Tables("DData").Rows                                                   'グラフ数でループ
                            With DTR.ItemArray
                                subMenus(dmy).itemText = .GetValue(0)
                                subMenus(dmy).target = .GetValue(1)
                                subMenus(dmy).GraphTitle = .GetValue(2)
                                subMenus(dmy).naviURL = .GetValue(3)
                                subMenus(dmy).Layer = .GetValue(4)
                                subMenus(dmy).Width = .GetValue(5)
                                subMenus(dmy).Height = .GetValue(6)
                                If subMenus(dmy).Width = 0 Then subMenus(dmy).Width = 1000
                                If subMenus(dmy).Height = 0 Then subMenus(dmy).Height = 1000
                                dmy += 1
                            End With
                        Next

                        subMenu = New C1MenuItem 'C1WebSubMenu()                                                        'サブメニューの宣言
                        menuItem = subMenu       ' 2013/07/17
                        'menuItem.SubMenu = subMenu
                        preFirstLebel = ""

                        ''If DbDr_Detail.HasRows = True Then
                        ''    menuItem.Text += "<img style= vertical-align:middle; src=" + Chr(34) + "./img/es.png" + Chr(34) + " />"
                        ''    ''menuItem.ItemStyle.ImageUrl = "~/img/es.png"                                              'この方法では画像が上に寄ってしまう・・・
                        ''    ''menuItem.ItemStyle.ItemAlign = HorizontalAlign.Right
                        ''    ''menuItem.ItemStyle.ItemImagePosition = ItemImagePositionEnum.Far

                        ''    dmy = 0
                        ''    Do While DbDr_Detail.Read()
                        ''        subMenus(dmy).itemText = DbDr_Detail.GetString(0)
                        ''        subMenus(dmy).target = DbDr_Detail.GetString(1)
                        ''        subMenus(dmy).GraphTitle = DbDr_Detail.GetString(2)
                        ''        subMenus(dmy).naviURL = DbDr_Detail.GetString(3)
                        ''        subMenus(dmy).Layer = DbDr_Detail.GetString(4)
                        ''        subMenus(dmy).Width = DbDr_Detail.GetInt16(5)
                        ''        subMenus(dmy).Height = DbDr_Detail.GetInt16(6)
                        ''        If subMenus(dmy).Width = 0 Then subMenus(dmy).Width = 1000
                        ''        If subMenus(dmy).Height = 0 Then subMenus(dmy).Height = 1000
                        ''        dmy += 1
                        ''    Loop
                        ''    DbDr_Detail.Close()

                        subMenu = New C1MenuItem()             ' 2013/07/17                                                'サブメニューの宣言
                        'subMenu = New C1WebSubMenu()                                                        'サブメニューの宣言
                        menuItem = subMenu       ' 2013/07/17
                        'menuItem.SubMenu = subMenu
                        preFirstLebel = ""                                                                  ''2009/05/28 Kino Add これが足りなかった()

                        For jloop = 0 To subMenuCount - 1
                            firstLevel = subMenus(jloop).Layer.Substring(0, 2)                              'メニュー階層1
                            secondLevel = subMenus(jloop).Layer.Substring(2, 2)                             'メニュー階層2
                            thirdLevel = subMenus(jloop).Layer.Substring(4, 2)                              'メニュー階層3

                            '●●サブ項目のみのとき(下層メニューがある)
                            If subMenus(jloop).naviURL = "sub" Then
                                If subMenus(jloop).itemText <> "-" Then
                                    'subMenuItem = New C1WebMenuItem(subMenus(jloop).GraphTitle)
                                    subMenuItem = New C1MenuItem(subMenus(jloop).GraphTitle)
                                    subMenu.Items.Add(subMenuItem)

                                Else
                                    sep = New C1MenuItemSeparator
                                    sep.Style.Item("BackImageUrl") = "~/img/holi_sep.gif"

                                    'sep = New C1WebSeparator
                                    'sep.SeparatorStyle.BackImageUrl = "~/img/holi_sep.gif"
                                    'sep.SeparatorStyle.BackImageSize = Unit.Parse("5px")

                                    subMenu.Items.Add(sep)                                                  'セパレータを追加
                                    ''subMenu.Items.Add(New C1WebSeparator())                                 'セパレータを追加

                                End If

                                '●●サブ項目(その下層はない場合)でリンク付
                            ElseIf firstLevel = "00" Or secondLevel = "00" Then
                                If subMenus(jloop).itemText <> "-" Then
                                    subMenuItem = New C1MenuItem(subMenus(jloop).GraphTitle)
                                    subMenuItem.NavigateUrl = subMenus(jloop).naviURL & "?GN=" + ItemNo.ToString("000") + _
                                                              ChildItemNo.ToString("00") & "&GNa=" & _
                                                              HttpUtility.UrlEncode(subMenus(jloop).itemText) & _
                                                              "&W=" & subMenus(jloop).Width & "&H=" & subMenus(jloop).Height
                                    subMenuItem.Target = subMenus(jloop).target
                                    subMenu.Items.Add(subMenuItem)

                                    ChildItemNo += 1
                                    ItemNo = 0
                                Else
                                    sep = New C1MenuItemSeparator
                                    ' ■■　とりあえずコメント
                                    'sep.SeparatorStyle.BackImageUrl = "~/img/holi_sep.gif"
                                    'sep.SeparatorStyle.BackImageSize = Unit.Parse("5px")
                                    ' ■■　とりあえずコメント
                                    subMenu.Items.Add(sep)                                                  'セパレータを追加
                                    ''subMenu.Items.Add(New C1WebSeparator())                                 'セパレータを追加
                                End If

                                '●●サブ項目のサブ項目を作成
                            ElseIf firstLevel <> "00" And secondLevel <> "00" Then

                                If preFirstLebel <> firstLevel Then
                                    sub2Menu = New C1MenuItem()
                                    subMenuItem.Items.Add(sub2Menu)
                                    'subMenuItem.SubMenu = sub2Menu
                                End If

                                If subMenus(jloop).itemText <> "-" Then
                                    subMenuItem = New C1MenuItem(subMenus(jloop).GraphTitle)
                                    subMenuItem.NavigateUrl = subMenus(jloop).naviURL & "?GN=" + ItemNo.ToString("000") + _
                                                              ChildItemNo.ToString("00") & "&GNa=" & _
                                                              HttpUtility.UrlEncode(subMenus(jloop).itemText) & _
                                                              "&W=" & subMenus(jloop).Width & "&H=" & subMenus(jloop).Height

                                    subMenuItem.Target = subMenus(jloop).target
                                    sub2Menu.Items.Add(subMenuItem)
                                Else
                                    sep = New C1MenuItemSeparator
                                    'sep.SeparatorStyle.BackImageUrl = "~/img/holi_sep.gif"
                                    'sep.SeparatorStyle.BackImageSize = Unit.Parse("5px")
                                    sub2Menu.Items.Add(sep)                                                 'セパレータを追加
                                    ''sub2Menu.Items.Add(New C1WebSeparator())                                'セパレータを追加
                                End If
                                preFirstLebel = firstLevel
                                ItemNo += 1

                            End If

                        Next

                    End If
                    ''DbDr_Detail.Close()
                    ChildItemNo = 0

                Else

                    ''「リンク」項目の場合のみの処理
                    strSql = "SELECT * FROM その他リンク"
                    DtSet = New DataSet
                    DbDa = New OleDb.OleDbDataAdapter(strSql, DbCon)
                    DbDa.Fill(DtSet, "DData")

                    ''DbCom_Detail = New OleDbCommand("SELECT * FROM その他リンク", DbCon)
                    ''DbDr_Detail = DbCom_Detail.ExecuteReader
                    subMenu = New C1MenuItem()
                    menuItem.Items.Add(subMenu)
                    menuItem.Text += " <img align=""Baseline"" src=""./img/es.png"" class=""menuarrow"" />"

                    For Each DTR As DataRow In DtSet.Tables("DData").Rows                                                   'グラフ数でループ
                        With DTR.ItemArray
                            subMenuItem = New C1MenuItem(.GetValue(1))                       'リンク先名称
                            subMenuItem.NavigateUrl = .GetValue(2)                              'URL
                            subMenuItem.Target = "_Blank"
                            subMenu.Items.Add(subMenuItem)
                        End With
                    Next
                    ''Do While DbDr_Detail.Read()
                    ''    subMenuItem = New C1WebMenuItem(DbDr_Detail.GetString(1))                       'リンク先名称
                    ''    subMenuItem.NavigateUrl = DbDr_Detail.GetString(2)                              'URL
                    ''    subMenuItem.Target = "_Blank"
                    ''    subMenu.Items.Add(subMenuItem)
                    ''Loop
                    ''DbDr_Detail.Close()

                End If

NextStep:
            Next iloop

            ''DbCon.Close()
            DbDa.Dispose()
            DtSet.Dispose()
            DbCon.Dispose()
        End If

        C1WMenu.Width = Unit.Empty                                                                      'メニュー幅のリセット
        C1WMenu.Width = Unit.Empty                                                                      'メニュー幅のリセット
        ''C1WMenu.Style("z-index") = 0

    End Sub

    Protected Sub TScMan1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs) Handles TScMan1.AsyncPostBackError

        Dim sw As New IO.StreamWriter(Server.MapPath("~/log/errorAsync.log"), True, Encoding.GetEncoding("Shift_JIS"))

        Dim ex As Exception = e.Exception

        If ex IsNot Nothing Then
            Dim sb As New StringBuilder
            sb.Append(DateTime.Now.ToString)
            sb.Append(ControlChars.Tab)
            sb.Append(ex.Source)
            sb.Append(ControlChars.Tab)
            sb.Append(ex.Message)
            sw.WriteLine(sb.ToString)
        End If

        sw.Dispose()

    End Sub

End Class

