Imports System.Data
Imports System.Data.OleDb
Imports System.Net
Imports System.Web.UI.Page

Partial Class MasterPage01
    Inherits System.Web.UI.MasterPage

    Public Structure showMenuText

        Dim itemText As String
        Dim naviURL As String
        Dim graphCode As String
        Dim target As String
        Dim Layer As String                         ' サブメニューでしか使用しない
        Dim LayerItem() As Integer                  ' サブメニューでしか使用しない
        Dim addAccord() As Integer                  ' Accordionの追加フラグ
        Dim width As Integer
        Dim height As Integer

    End Structure

    ''Public Structure MenuText                     ' 上の構造体に共通化した
    ''    Dim itemText As String
    ''    Dim naviURL As String
    ''    Dim target As String
    ''    Dim GraphTitle As String
    ''    Dim Width As Integer
    ''    Dim Height As Integer
    ''End Structure

    Public Structure menuProperty
        Dim _Width As Short
        Dim _Height As Short
        Dim _FontSize As Single
        Dim _SelFontSize As Single
        Dim _btnFontSize As Single
        Dim _selFontBold As Boolean
        Dim _buttonBackColor As String
        Dim _buttonColor As String
        Dim _contentColor() As String
        Dim _contentFontColor() As String
        Dim _contentSelColor() As String
        Dim _contentSelFontColor() As String
        Dim _duration As Integer
    End Structure

    Protected Sub LoginStatus1_LoggingOut(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.LoginCancelEventArgs) Handles LoginStatus1.LoggingOut

        ''Session.RemoveAll()                     'セッション状態のコレクションからすべての値とキーを削除します。   2011/06/08 Kino Changed これはすべての情報を破棄してしまうらしい。
        Session.Remove("LgSt")                  ' ログインステータスのセッションを削除
        Session.Abandon()                       '現在のセッションに破棄のマークを付けます（セッションを破棄する）　現在のセッションをキャンセルします。
        Session.Clear()                         ' セッション状態のコレクションからすべてのキーと値を削除します。  

    End Sub

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed

        ''Session.RemoveAll()                     'セッション状態のコレクションからすべての値とキーを削除します。   2011/06/08 Kino Changed これはすべての情報を破棄してしまうらしい。
        Session.Remove("LgSt")                  ' ログインステータスのセッションを削除
        Session.Abandon()                       '現在のセッションに破棄のマークを付けます（セッションを破棄する）　現在のセッションをキャンセルします。
        Session.Clear()                         ' セッション状態のコレクションからすべてのキーと値を削除します。  

    End Sub

    Protected Sub WriteErrorLog(ByVal ProcName As String, ByVal ex As Exception)

        Dim sw As IO.StreamWriter = Nothing

        Try
            If ex IsNot Nothing Then
                sw = New IO.StreamWriter(Server.MapPath("~/log/errorSummary_" & Date.Now.ToString("yyMM") & ".log"), True, Encoding.GetEncoding("Shift_JIS"))
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

        Dim SiteName As String
        Dim SiteDirectory As String
        ''Dim SelItem As String
        Dim SiteNo As String

        'If AjaxControlToolkit.ToolkitScriptManager.AsyncPostBackSourceElementID = "ctl00_LoginStatus1" Or ToolkitScriptManager.AsyncPostBackSourceElementID.Length = 0 Then Exit Sub
        Dim ctrlname As String = Page.Request.Params.Get("__EVENTTARGET")                           ' 2013/05/29 Kino Add ログアウト
        If ctrlname IsNot Nothing AndAlso ctrlname = "ctl00$LoginStatus1$ctl00" Then
            Exit Sub
        End If

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
                Response.Redirect("Login.aspx", False)
                Exit Sub
            End If
        End If

        SiteName = Session.Item("SN")                           '現場名
        SiteDirectory = Session.Item("SD")                      '現場ディレクトリ
        SiteNo = Session.Item("SNo")
        ''SelItem = "TOP"                                         ' のため、直接記載

        Me.Lit1.Text = SiteName

        If IsPostBack = False Then
            ViewState("SelectedAccordionIndex") = -1
            'Me.MyAccordion.Style("left") = ("-" & Me.MyAccordion.Width.ToString)
        End If

        'If IsPostBack = False Then
        Call SetMenuItemsNew()
        'End If

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Dim SiteName As String
        'Dim SiteDirectory As String
        'Dim SelItem As String
        'Dim SiteNo As String

        'Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)
        ''ログインしていない場合は、ログイン画面へ
        'If LoginStatus = 0 Then
        '    System.Threading.Thread.Sleep(5000)                                                     ' 2010/06/01 Kino Add 再取得確認
        '    LoginStatus = CType(Session.Item("LgSt"), Integer)
        '    If LoginStatus = 0 Then
        '        Dim ex As Exception = Server.GetLastError().GetBaseException()
        '        Call WriteErrorLog(MyBase.GetType.BaseType.FullName & "LoginStatus not Found by Masterpage", ex)

        '        Response.Redirect("Login.aspx")
        '        Exit Sub
        '    End If
        'End If

        'SiteName = Session.Item("SN")                           '現場名
        'SiteDirectory = Session.Item("SD")                      '現場ディレクトリ
        'SiteNo = Session.Item("SNo")
        'SelItem = "TOP"                                         ' のため、直接記載

        'Me.Lit1.Text = SiteName

        ''If IsPostBack = False Then
        '    Call SetMenuItemsNew()
        ''End If

        'If IsPostBack = False Then
        ' ''    Me.MyAccordion.SelectedIndex = -1
        'Else

        If ViewState("SelectedAccordionIndex") IsNot Nothing Then
            Me.MyAccordion.SelectedIndex = CType(ViewState("SelectedAccordionIndex"), Integer)
        End If
        'End If
    End Sub
    ''' <summary>
    ''' メニューのプロパティをDBから読み込むかデフォルト値を格納
    ''' </summary>
    ''' <param name="DbCon">OleDBConnection</param>
    ''' <param name="menuProp">メニュープロパティ構造体</param>
    ''' <param name="menuTitle">読込メニューのタイトル</param>
    ''' <remarks></remarks>
    Protected Sub readMenuProperties(ByVal DbCon As OleDbConnection, ByRef menuProp As menuProperty, Optional ByVal menuTitle As String = "TOP")

        'Dim DbDa As OleDb.OleDbDataAdapter = Nothing
        'Dim DtSet As New DataSet("DData")
        Dim strSql As String = Nothing
        Dim TBCheck As New ClsReadDataFile

        If TBCheck.isExistTable(DbCon, "AccordionMenu設定") = True Then         ' テーブルが存在していたら読み込む

            strSql = "SELECT * FROM AccordionMenu設定 WHERE (項目名 = '" & menuTitle & "')"

            'DtSet = New DataSet
            Using DbDa As New OleDb.OleDbDataAdapter(strSql, DbCon)
                Using DtSet As New DataSet("DData")
                    DbDa.Fill(DtSet, "DData")

                    With DtSet.Tables("DData").Rows(0)
                        menuProp._Width = CType(.Item(2), Short)                ' メニュー幅
                        menuProp._Height = CType(.Item(3), Short)               ' メニュー高さ
                        menuProp._FontSize = CType(.Item(4), Single)            ' フォントサイズ
                        menuProp._SelFontSize = CType(.Item(5), Single)         ' 選択時フォントサイズ
                        menuProp._btnFontSize = CType(.Item(6), Single)         ' ボタンフォントサイズ
                        menuProp._selFontBold = CType(.Item(7), Boolean)        ' 選択時フォントボールド
                        menuProp._buttonBackColor = .Item(8).ToString           ' ボタン背景色
                        menuProp._buttonColor = .Item(9).ToString               ' ボタン文字色
                        menuProp._contentColor(0) = .Item(10).ToString          ' 項目背景色
                        menuProp._contentFontColor(0) = .Item(11).ToString      ' 項目文字色
                        menuProp._contentSelColor(0) = .Item(12).ToString       ' 選択時背景色
                        menuProp._contentSelFontColor(0) = .Item(13).ToString   ' 選択時文字色
                        menuProp._contentColor(1) = .Item(14).ToString          ' 項目背景色(第２階層)
                        menuProp._contentFontColor(1) = .Item(15).ToString      ' 項目文字色(第２階層)
                        menuProp._contentSelColor(1) = .Item(16).ToString       ' 選択時背景色(第２階層)
                        menuProp._contentSelFontColor(1) = .Item(17).ToString   ' 選択時文字色(第２階層)
                        menuProp._duration = CType(.Item(18), Integer)          ' Duration値
                    End With
                End Using
            End Using
        Else                                                                    ' テーブルがなければデフォルト値

            menuProp._Width = 120                                       ' メニュー幅
            menuProp._Height = 0                                        ' メニュー高さ
            menuProp._FontSize = 12                                     ' フォントサイズ
            menuProp._SelFontSize = 12                                  ' 選択時フォントサイズ
            menuProp._btnFontSize = 12                                  ' ボタンフォントサイズ
            menuProp._selFontBold = True                                ' 選択時フォントボールド
            menuProp._buttonBackColor = "#FFFFFF"                       ' ボタン背景色
            menuProp._buttonColor = "#000000"                           ' ボタン文字色
            menuProp._contentColor(0) = "#3843A3"                       ' 項目背景色"
            menuProp._contentFontColor(0) = "#FFFFFF"                   ' 項目文字色
            menuProp._contentSelColor(0) = "#387DA3"                    ' 選択時背景色
            menuProp._contentSelFontColor(0) = "#FCFFBE"                ' 選択時文字色
            menuProp._contentColor(1) = "#5462E0"                       ' 項目背景色(第２階層)
            menuProp._contentFontColor(1) = "#FFFFFF"                   ' 項目文字色(第２階層)
            menuProp._contentSelColor(1) = "#64A0C7"                    ' 選択時背景色(第２階層)
            menuProp._contentSelFontColor(1) = "#RCFFBE"                ' 選択時文字色(第２階層)
            menuProp._duration = 150
        End If

        TBCheck = Nothing

    End Sub


    ''' <summary>
    ''' データベースから情報を読み込んでパラメータ文字列を作成する
    ''' </summary>
    ''' <param name="DbCon">oledbConnction</param>
    ''' <returns>window.openのパラメータ</returns>
    ''' <remarks></remarks>
    Protected Function readOpenWindowParameter(ByVal DbCon As OleDbConnection) As String

        'Dim DbDa As OleDb.OleDbDataAdapter = Nothing
        'Dim DtSet As New DataSet("DData")
        Dim strSql As String = Nothing
        Dim TBCheck As New ClsReadDataFile
        Dim intTemp As Integer
        Dim blnTemp As Boolean
        ''Dim retWord As String = ""
        Dim retWord As New StringBuilder
        Dim retURLQuery As String = Nothing

        If TBCheck.isExistTable(DbCon, "OpenParam") = True Then         ' テーブルが存在していたら読み込む

            retWord.Capacity = 100

            strSql = "SELECT * FROM OpenParam"
            'DtSet = New DataSet
            Using DbDa As New OleDb.OleDbDataAdapter(strSql, DbCon)
                Using DtSet As New DataSet("DData")

                    DbDa.Fill(DtSet, "DData")

                    With DtSet.Tables("DData").Rows(0)
                        intTemp = CType(.Item(1), Integer)                      ' 左位置
                        If intTemp >= 0 Then
                            ''retWord = "left=" & intTemp.ToString & ","
                            retWord.Append(String.Format("left={0},", intTemp.ToString))
                        End If

                        intTemp = CType(.Item(2), Integer)                      ' 上位置
                        If intTemp >= 0 Then
                            'retWord += "top=" & intTemp.ToString & ","
                            retWord.Append(String.Format("top={0},", intTemp.ToString))
                        End If

                        blnTemp = CType(.Item(3), Boolean)                      ' メニューバー有無
                        If blnTemp = True Then
                            ''retWord += "menubar=yes,"
                            retWord.Append("menubar=yes,")
                        Else
                            ''retWord += "menubar=no,"
                            retWord.Append("menubar=no,")

                        End If

                        blnTemp = CType(.Item(4), Boolean)                      ' ツールバー有無
                        If blnTemp = True Then
                            ''retWord += "toolbar=yes,"
                            retWord.Append("toolbar=yes,")
                        Else
                            ''retWord += "toolbar=no,"
                            retWord.Append("toolbar=no,")
                        End If

                        blnTemp = CType(.Item(5), Boolean)                      ' アドレスバー有無
                        If blnTemp = True Then
                            ''retWord += "location=yes,"
                            retWord.Append("location=yes,")
                        Else
                            ''retWord += "location=no,"
                            retWord.Append("location=no,")
                        End If

                        blnTemp = CType(.Item(6), Boolean)                      ' ステータスバー有無
                        If blnTemp = True Then
                            ''retWord += "status=yes,"
                            retWord.Append("status=yes,")
                        Else
                            ''retWord += "status=no,"
                            retWord.Append("status=no,")
                        End If

                        blnTemp = CType(.Item(7), Boolean)                      ' リサイズ可否
                        If blnTemp = True Then
                            ''retWord += "resizable=yes,"
                            retWord.Append("resizable=yes,")
                        Else
                            ''retWord += "resizable=no,"
                            retWord.Append("resizable=no,")
                        End If

                        blnTemp = CType(.Item(8), Boolean)                      ' スクロールバー有無
                        If blnTemp = True Then
                            ''retWord += "scrollbars=yes,"
                            retWord.Append("scrollbars=yes,")
                        Else
                            ''retWord += "scrollbars=no,"
                            retWord.Append("scrollbars=no,")
                        End If
                    End With

                    retURLQuery = retWord.ToString
                    retURLQuery.TrimEnd(Convert.ToChar(","))
                    retWord.Length = 0
                    ''If retWord.EndsWith(",") = True Then
                    ''    retWord = retWord.Substring(0, retWord.Length - 1)      ' 最後の文字が「，」だったらそれを取る
                    ''End If
                End Using
            End Using
        End If

        Return retURLQuery

        TBCheck = Nothing

    End Function

    ''' <summary>
    ''' アコーディオン形式メニューを構成する
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetMenuItemsNew()

        'Dim DbCon As New OleDbConnection
        'Dim DbDa As OleDb.OleDbDataAdapter = Nothing        ' メニュー追加グラフ読込用
        'Dim DtSet As New DataSet("DData")
        'Dim DbDa2 As OleDb.OleDbDataAdapter = Nothing       ' メニュー基本情報読込用
        'Dim DtSet2 As New DataSet("DData")
        'Dim DbDa3 As OleDb.OleDbDataAdapter = Nothing       ' 外部リンクデータ読込用
        'Dim DtSet3 As New DataSet("DData")
        Dim strSQL As String
        Dim SiteDirectory As String = Nothing
        Dim GrpDataFile As String
        Dim showMenu() As showMenuText = Nothing            ' メニュー表示項目
        Dim subMenus() As showMenuText = Nothing            ' メニュー階層
        Dim linkMenus() As showMenuText = Nothing           ' リンク
        Dim clsLevelCheck As New ClsCheckUser
        Dim ULNo(5) As String
        Dim uLevel As Integer = CType(Session.Item("UL"), Integer)
        Dim userLevel As String = uLevel.ToString("000000")     'ユーザレベル
        Dim iloop As Integer
        Dim intCount As Integer
        Dim rowCount As Integer = 0
        Dim menuProp As menuProperty = Nothing
        Dim openWinParam As String
        Dim layerCount As Integer
        Dim mnuTopItem As String = Nothing
        Dim linkEnable As Short = 0

        ReDim menuProp._contentColor(1)
        ReDim menuProp._contentFontColor(1)
        ReDim menuProp._contentSelColor(1)
        ReDim menuProp._contentSelFontColor(1)

        SiteDirectory = Session.Item("SD")      '現場ディレクトリ

        ''セッションタイムアウトした時には、ログイン画面を出す
        If [String].IsNullOrEmpty(SiteDirectory) = True Then Response.Redirect("Login.aspx", False)

        ''ユーザレベル情報取得
        Dim intTemp As Integer = clsLevelCheck.GetWord(userLevel, ULNo)

        GrpDataFile = Server.MapPath(SiteDirectory & "\App_Data\MenuInfo.mdb")

        Using DbCon As New OleDbConnection
            If System.IO.File.Exists(GrpDataFile) = True Then
                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & GrpDataFile & ";" & "Jet OLEDB:Engine Type= 5")

                Call readMenuProperties(DbCon, menuProp)                                        ' メニュー用のパラメータを読み込む
                openWinParam = readOpenWindowParameter(DbCon)                                   ' window.openに付加するパラメータを読み込む

                With Me.MyAccordion                                                             ' アコーディオンの基本プロパティ設定
                    .SelectedIndex = -1
                    .AutoSize = AjaxControlToolkit.AutoSize.None
                    .FadeTransitions = True
                    .TransitionDuration = menuProp._duration
                    .RequireOpenedPane = False
                    .SuppressHeaderPostbacks = False
                    .Width = menuProp._Width        'Unit.Parse(CType(menuProp._Width, String))
                    .FramesPerSecond = 40
                    ''.Visible = False
                    .Style("margin-left") = "-300px"                                            ' 2015/12/28 Kino Add とりあえず、メニューのマージンをマイナス値にしておいて非表示に見せかける
                End With

                Me.ColL.Width = menuProp._Width + 12
                Me.pnlColR.Style("margin-left") = Unit.Parse(CType(CType(menuProp._Width, Integer) + 15, String)).ToString          ' メニュー幅を変えた場合は、右側のPanelのマージンを変えないと正常に表示されなかった(IE8、Chrome)

                '' ==== メニュー項目を取得する
                strSQL = "SELECT * FROM メニュー追加グラフ WHERE (((メニュー追加グラフ.出力)=True)) ORDER BY メニュー追加グラフ.[No]"
                'DtSet = New DataSet
                Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                    Using DtSet As New DataSet("DData")

                        DbDa.Fill(DtSet, "DData")
                        intCount = DtSet.Tables("DData").Rows.Count - 1
                        ReDim showMenu(intCount)

                        mnuTopItem = "|"
                        For Each DTR As DataRow In DtSet.Tables("DData").Rows                           '表示グラフ項目(メニュー追加グラフテーブル)でループ　（最上層項目）
                            With DTR.ItemArray
                                showMenu(rowCount).itemText = .GetValue(1).ToString                     ' グラフ種別
                                showMenu(rowCount).naviURL = .GetValue(2)                               ' URL
                                showMenu(rowCount).graphCode = .GetValue(3)                             ' グラフコード
                                showMenu(rowCount).target = .GetValue(5)                                ' ターゲット
                                showMenu(rowCount).width = .GetValue(6)                                 ' 幅
                                showMenu(rowCount).height = .GetValue(7)                                ' 高さ
                                mnuTopItem &= (showMenu(rowCount).itemText & "@" & rowCount.ToString("000") & "|")  ' 最上階層の項目とインデックスを文字列に格納して、後で検索に使う
                                If showMenu(rowCount).graphCode = "LNK" Then
                                    linkEnable += 1                                                     ' リンクが有効なら１を加算        
                                End If
                                rowCount += 1
                            End With
                        Next
                    End Using
                End Using

                '' === 表示グラフ項目(メニュー追加グラフテーブル)でループして、メニュー基本情報テーブル内の情報を読みこむ
                Using DtSet2 As New DataSet("DData2")
                    For iloop = 0 To showMenu.Length - 1
                        '出力情報テーブルに作図情報があれば、メニューに項目を追加する
                        strSQL = "SELECT 項目名,種別,グラフタイトル,URL,メニュー階層,幅,高さ FROM メニュー基本情報 WHERE 種別='" & showMenu(iloop).itemText & "' ORDER BY メニュー階層,ID ASC"
                        Using DbDa2 As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                            DbDa2.Fill(DtSet2, "DData2")                                                ' ループで取得したデータを１つのデータセット内に格納する
                        End Using
                    Next
                    intCount = DtSet2.Tables("DData2").Rows.Count - 1
                    ReDim subMenus(intCount)

                    '' === 外部リンク項目
                    If linkEnable = 1 Then
                        'DtSet3 = New DataSet
                        strSQL = "SELECT 名称,URL FROM その他リンク"                                ' IDが主キーのため、IDで自動的にソートされる
                        Using DbDa3 As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                            Using DtSet3 As New DataSet("DData")
                                DbDa3.Fill(DtSet3, "DData3")
                                intCount = DtSet3.Tables("DData3").Rows.Count - 1
                                ReDim linkMenus(intCount)
                                rowCount = 0
                                For Each DTR As DataRow In DtSet3.Tables("DData3").Rows                     '表示グラフ項目(メニュー追加グラフテーブル)でループ　（最上層項目）
                                    With DTR.ItemArray
                                        linkMenus(rowCount).itemText = .GetValue(0).ToString
                                        linkMenus(rowCount).naviURL = .GetValue(1).ToString
                                        rowCount += 1
                                    End With
                                Next
                            End Using
                        End Using
                    End If

                    ''DbCon.Close()

                    ''DbDa.Dispose()
                    ''DtSet.Dispose()

                    rowCount = 0
                    For Each dtr As DataRow In DtSet2.Tables("DData2").Rows                         ' ひとまず配列(構造体)にサブ項目データを格納してデータセットをクローズしておく()
                        With dtr.ItemArray
                            subMenus(rowCount).itemText = .GetValue(0).ToString                     ' 項目名
                            subMenus(rowCount).target = .GetValue(1).ToString                       ' 種別
                            subMenus(rowCount).graphCode = .GetValue(2).ToString                    ' グラフタイトル
                            subMenus(rowCount).naviURL = .GetValue(3).ToString                      ' URL
                            subMenus(rowCount).Layer = .GetValue(4).ToString                        ' 階層情報
                            layerCount = CType(subMenus(rowCount).Layer.Length, Integer) * 0.5 - 1
                            ReDim subMenus(rowCount).LayerItem(layerCount)
                            For iloop = 0 To layerCount
                                subMenus(rowCount).LayerItem(iloop) = CType(subMenus(rowCount).Layer.Substring(iloop * 2, 2), Integer)
                            Next iloop
                            subMenus(rowCount).width = CType(.GetValue(5), Integer)                 ' 幅
                            subMenus(rowCount).height = CType(.GetValue(6), Integer)                ' 高さ
                            If subMenus(rowCount).width = 0 Then subMenus(rowCount).width = 1000
                            If subMenus(rowCount).height = 0 Then subMenus(rowCount).height = 1000
                            rowCount += 1
                        End With
                    Next

                End Using

                'DbDa2.Dispose()
                'DtSet2.Dispose()
                'DbDa3.Dispose()
                'DtSet3.Dispose()

                Call setPain(showMenu, menuProp, subMenus, linkMenus, ULNo, openWinParam)                     ' アコーディオンメニューの最上層作成
                Call setSubPain(showMenu, menuProp, subMenus, mnuTopItem, openWinParam)                       ' 第2階層以下を作成
                If IsPostBack = False Then Me.MyAccordion.SelectedIndex = -1

            End If
        End Using

    End Sub

    ''' <summary>
    ''' アコーディオンメニューの第2階層以下を作成する
    ''' </summary>
    ''' <param name="showMenu">メニュー項目構造体</param>
    ''' <param name="menuProp">メニューのプロパティ構造体</param>
    ''' <param name="subMenus">サブメニュー項目の構造体</param>
    ''' <remarks></remarks>
    Protected Sub setSubPain(ByVal showMenu() As showMenuText, ByVal menuProp As menuProperty, _
                            ByVal subMenus() As showMenuText, ByVal mnuTopItem As String, ByVal openWinParam As String)

        Dim iloop As Integer
        'Dim jloop As Integer
        Dim menuCount As Integer = subMenus.Length - 1                                  ' メニュー総項目数
        Dim maxLayer As Integer                                                         ' 階層数
        Dim nowItemTitle As String = Nothing                                            ' 現在処理中の最上層項目名
        Dim nowItemIndex As Integer                                                     ' 現在処理中の最上層のインデックス(Mn**)
        Dim nowSubItemTitle As String                                                   ' 現在処理中のサブ項目　グラフ種別
        Dim preSubItemTitle As String = Nothing                                         ' 前回処理したサブ項目　グラフ種別
        Dim idText As String = Nothing
        Dim numOf1st As Integer = 0
        Dim idOf1st As String = Nothing
        Dim numOf2nd As Integer = 0
        Dim idOf2nd As String = Nothing
        Dim numOf3rd As Integer = 0
        Dim idOf3rd As String = Nothing
        Dim ajAccObj As AjaxControlToolkit.Accordion = Nothing
        Dim ajAccdPane As AjaxControlToolkit.AccordionPane = Nothing
        Dim winopenParam As String = Nothing

        For iloop = 0 To menuCount                                                      ' 「メニュー基本情報」テーブルにおける内容を全て処理する

            maxLayer = subMenus(iloop).LayerItem.Length - 1                             ' 層数を配列数として格納
            nowSubItemTitle = subMenus(iloop).target                                    ' 種別

            If subMenus(iloop).LayerItem(maxLayer) = 0 And subMenus(iloop).LayerItem(maxLayer - 1) = 0 Then
                idText = "1st" '"Master_Content"
            ElseIf subMenus(iloop).LayerItem(maxLayer) = 0 And subMenus(iloop).LayerItem(maxLayer - 1) <> 0 Then
                idText = "2rd" 'Master_Sub_Content"
            Else
                idText = "3rd"
            End If

            If preSubItemTitle <> nowSubItemTitle Then                                  ' 第2階層の項目が前回と違う場合にチェックする
                Dim intTemp As Integer                                                  ' 第2階層の項目と同じ最上階層の項目のインデックスと項目名を取得する
                Dim intTempEd As Integer
                intTemp = mnuTopItem.IndexOf(nowSubItemTitle, 0)
                intTempEd = mnuTopItem.IndexOf("@", intTemp + 1)
                nowItemIndex = CType(mnuTopItem.Substring(intTempEd + 1, 3), Integer)                                   ' 大元のアコーディオンを構成した項目のインデックスを取得
                nowItemTitle = mnuTopItem.Substring(intTemp, (intTempEd - intTemp))                                     ' 大元のアコーディオンを構成した項目の名称を取得

                If subMenus(iloop).naviURL = "sub" Then                                                                 ' URLが「sub」の場合(リンクでない場合)はアコーディオンのインスタンス生成をする
                    ''If subMenus(iloop).LayerItem(maxLayer) = 0 Then                                                         ' 第3階層が0の場合(リンクでない場合)は
                    ajAccObj = New AjaxControlToolkit.Accordion                                                         ' 　　新規アコーディオンのインスタンスを生成
                    idOf1st = "ac_" & idText & showMenu(nowItemIndex).graphCode & numOf1st.ToString("000")              ' IDを作成
                    numOf1st += 1
                    Call setAccordionProperties(idOf1st, ajAccObj, menuProp._duration)                                  ' アコーディオンのプロパティを設定

                    'Else
                    'ajAccObj = CType(FindControl("MyAccordion"), AjaxControlToolkit.Accordion)
                End If
            End If

            Select Case subMenus(iloop).LayerItem(maxLayer)

                Case 0                                                                                                  ' 第3階(最下)層が0の場合(リンクではない場合)

                    If subMenus(iloop).LayerItem(maxLayer - 1) = 0 Then                                                 ' 第2階層が0の場合は、まずアコーディオンを追加する

                        ajAccdPane = New AjaxControlToolkit.AccordionPane                                               ' Accordionのペインを作成
                        idOf2nd = "hd_" & idText & showMenu(nowItemIndex).graphCode & numOf2nd.ToString("000")
                        numOf2nd += 1
                        Call addAccordionPane(ajAccdPane, menuProp, idOf2nd, subMenus, iloop)                           ' 各種プロパティとヘッダーコンテナを作成する

                    Else



                    End If

                Case Else                                                                                               ' 第3階(最下)層が0ではない場合(リンク(ボタン)を追加する)

                    If subMenus(iloop).LayerItem(maxLayer - 1) = 0 Then                                                 ' 第2階層が0の場合はリンクを作成
                        If ajAccdPane IsNot Nothing Then

                            With ajAccdPane
                                idOf3rd = idText & showMenu(nowItemIndex).graphCode & numOf3rd.ToString("000")              ' IDを作成
                                .ID = "ap_" & idOf3rd
                                numOf3rd += 1
                                ''.Width = Unit.Parse(CType(menuProp._Width - 20, String))
                                'winopenParam = "window.open('" & subMenus(iloop).naviURL & "?GNa=" & HttpUtility.UrlEncode(subMenus(iloop).itemText) & _
                                '                                "&W=" & subMenus(iloop).width & _
                                '                                "&H=" & subMenus(iloop).height & "','" & _
                                '                                subMenus(iloop).target & _
                                '                                "','width=" & subMenus(iloop).width & _
                                '                                ",height=" & subMenus(iloop).height & "," & _
                                '                                openWinParam & "');return false;"
                                winopenParam = makeWindowOpenParameters(subMenus, iloop, openWinParam)
                                Dim linkButton As New HyperLink 'Button
                                linkButton.Text = subMenus(iloop).graphCode
                                linkButton.NavigateUrl = "javascript:void(0);"
                                'linkButton.Target = "_blank"
                                linkButton.Attributes.Add("onclick", winopenParam)
                                'linkButton.Width = menuProp._Width - 20
                                linkButton.Font.Size = menuProp._btnFontSize
                                .ContentContainer.ID = idOf3rd
                                .ContentContainer.Controls.Add(linkButton)
                                ''.ContentContainer.Style("display") = "block"

                                If iloop = menuCount OrElse subMenus(iloop).LayerItem(0) <> subMenus(iloop + 1).LayerItem(0) OrElse subMenus(iloop).target <> subMenus(iloop + 1).target Then
                                    ajAccObj.Panes.Add(ajAccdPane)
                                    ajAccObj.SelectedIndex = -1
                                    ajAccdPane.Dispose()
                                    ajAccdPane = Nothing
                                End If
                            End With
                        End If

                    Else

                    End If

            End Select

            preSubItemTitle = nowSubItemTitle

            If iloop = menuCount OrElse subMenus(iloop).target <> subMenus(iloop + 1).target Then
                If ajAccObj IsNot Nothing Then
                    Me.MyAccordion.Panes(("Mn" & nowItemIndex.ToString("00"))).ContentContainer.Controls.Add(ajAccObj)      ' アコーディオンを大元のアコーディオンに追加
                    'Me.MyAccordion.Panes(0).ContentContainer.Controls.Add(ajAccObj)      ' アコーディオンを大元のアコーディオンに追加
                    ajAccObj.Dispose()
                    ajAccObj = Nothing
                End If
            End If

        Next

        '' リンク項目は別扱い


    End Sub

    ''' <summary>
    ''' JavaScriptにおけるWindow.Open関連のパラメータ文字列を作成する
    ''' </summary>
    ''' <param name="subMenus">サブメニュー項目の構造体</param>
    ''' <param name="loopCount">メニュ構築におけるループカウント</param>
    ''' <param name="openWinParam">ツールバーやロケーション関連のTrue/Falseが格納された文字列</param>
    ''' <returns>作成した文字列 window.open(～);return false;</returns>
    ''' <remarks></remarks>
    Protected Function makeWindowOpenParameters(ByVal subMenus() As showMenuText, ByVal loopCount As Integer, ByVal openWinParam As String) As String

        Dim strTemp As String = Nothing
        Dim winWidth As String = Nothing
        Dim winHeight As String = Nothing
        Dim sizeParam As String = Nothing
        ''Dim sizeQuery As String = Nothing
        'Dim sbQuery As New StringBuilder           'サイズ指定をやめた　2018/06/05 Kino Changed

        If subMenus(loopCount).width < 0 Then
            winWidth = ""
            ' ''sizeQuery = "&W=1000"
            'sbQuery.Append("&W=1000")
        Else
            ' ''winWidth = ("width=" & subMenus(loopCount).width & ",")
            winWidth = String.Format("width={0},", subMenus(loopCount).width)
            ' ''sizeQuery = "&W=" & subMenus(loopCount).width
            'sbQuery.Append(String.Format("&W={0}", subMenus(loopCount).width))
        End If

        If subMenus(loopCount).height < 0 Then
            winHeight = ""
            ' ''sizeQuery &= "&H=1000"
            'sbQuery.Append("&H=1000")
        Else
            ' ''winHeight = ("height=" & subMenus(loopCount).height & ",")
            winHeight = String.Format("height={0},", subMenus(loopCount).height)
            ' ''sizeQuery &= "&h=" & subMenus(loopCount).height
            'sbQuery.Append(String.Format("&h={0}", subMenus(loopCount).height))
        End If

        If winWidth Is Nothing = True And winHeight.Length = 0 Then
            sizeParam = "'"
        ElseIf winWidth.Length = 0 Then
            ''sizeParam = ("'" & winWidth)
            sizeParam = (String.Format("'{0}", winWidth))
        ElseIf winHeight.Length = 0 Then
            ''sizeParam = ("'" & winHeight)
            sizeParam = (String.Format("'{0}", winHeight))
        Else
            ''sizeParam = ("'" & winWidth & winHeight)
            sizeParam = (String.Format("'{0}{1}", winWidth, winHeight))
        End If

        ''strTemp = "window.open('" & subMenus(loopCount).naviURL & "?GNa=" & HttpUtility.UrlEncode(subMenus(loopCount).itemText) & sizeQuery &
        ''                        "','" & subMenus(loopCount).target & "'," & sizeParam & openWinParam & "');return false;"
        Dim sbURL As New StringBuilder

        sbURL.Capacity = 100
        sbURL.Append(String.Format("window.open('{0}", subMenus(loopCount).naviURL))
        If subMenus(loopCount).itemText.Contains("現場") = False Then
            sbURL.Append(String.Format("?GNa={0}", HttpUtility.UrlEncode(subMenus(loopCount).itemText)))
        End If

        'sbURL.Append(String.Format("{0}','{1}',{2}');return false;", sbQuery.ToString, subMenus(loopCount).target, sizeParam & openWinParam))
        sbURL.Append(String.Format("','{0}',{1}');return false;", subMenus(loopCount).target, sizeParam & openWinParam))

        strTemp = sbURL.ToString
        sbURL.Length = 0
        ''sbQuery.Length = 0

        'sizeQuery & "','" & "new" & "');return false;"     ' これの場合はIEでもタブで表示　ただし、インターネットオプションの「タブ」で
        '「ポップアップを開く方法をInternet Explolerで自動的に判定する」にしないとＮＧ

        Return strTemp

    End Function

    Protected Sub addAccordionPane(ByVal ajAccdPane As AjaxControlToolkit.AccordionPane, ByVal menuProp As menuProperty, ByVal idText As String, ByVal subMenus() As showMenuText, ByVal loopNum As Integer)

        With ajAccdPane
            .ID = idText
            .HeaderContainer.ID = "A" & idText
            .HeaderCssClass = "accordionHeaderSub"
            '.HeaderSelectedCssClass = "accordionHeaderSelected"
            .ContentCssClass = "accordionContentSub"
            .Width = menuProp._Width - 10
            '.BorderWidth = Me.MyAccordion.BorderWidth
            '.BorderColor = Me.MyAccordion.BorderColor
            '.BorderStyle = Me.MyAccordion.BorderStyle
            Using labelText As New Literal
                labelText.Text = subMenus(loopNum).graphCode                                ' ヘッダーコンテナのインスタンスを作成
                .HeaderContainer.Controls.Add(labelText)
                ''.HeaderContainer.Style("display") = "block"

                .HeaderContainer.Controls.Add(labelText)                                    ' ヘッダーコンテナを追加する
            End Using
        End With


    End Sub


    Protected Sub setAccordionProperties(ByVal idText As String, ByVal ajAccObj As AjaxControlToolkit.Accordion, ByVal transitionDuration As Integer)

        With ajAccObj
            .ID = idText
            .HeaderCssClass = "accordionHeaderSub"
            .HeaderSelectedCssClass = "accordionHeaderSelectedSub"
            .ContentCssClass = "accordionContentSub"
            .AutoSize = AjaxControlToolkit.AutoSize.None
            .FadeTransitions = True
            .TransitionDuration = transitionDuration
            .RequireOpenedPane = False
            .SuppressHeaderPostbacks = False
            '.Width = Unit.Parse(CType(menuProp._Width - 10, String))
            .FramesPerSecond = 40
            '.BorderWidth = Me.MyAccordion.BorderWidth
            '.BorderColor = Me.MyAccordion.BorderColor
            '.BorderStyle = Me.MyAccordion.BorderStyle
        End With

    End Sub


    ''' <summary>
    ''' アコーディオンメニューの最上層(初期に表示されている項目)を作成する
    ''' </summary>
    ''' <param name="showMenu">メニュー項目構造体</param>
    ''' <param name="menuProp">メニューのプロパティ構造体</param>
    ''' <remarks></remarks>
    Protected Sub setPain(ByVal showMenu() As showMenuText, ByVal menuProp As menuProperty, ByVal subMenus() As showMenuText, _
                        ByVal linkMenus() As showMenuText, ByVal ULNo() As String, ByVal openWinParam As String)

        Dim ajAccdPane As AjaxControlToolkit.AccordionPane
        'Dim headerLabel As Literal = Nothing
        Dim iloop As Integer
        Dim jloop As Integer
        Dim loopCount As Integer = subMenus.Length - 1
        'Dim linkButton As HyperLink = Nothing
        Dim addFlg As Integer
        Dim winopenParam As String = Nothing
        Dim adminCheck As Boolean                               ' 2014/09/05 Kino Add   プルダウントップに入れる場合

        Me.MyAccordion.Width = menuProp._Width
        Me.MenuHead.Width = menuProp._Width - 10
        If menuProp._Height <> 0 Then
            Me.MyAccordion.Height = menuProp._Height
        End If

        For iloop = 0 To showMenu.Length - 1
            addFlg = 0
            ajAccdPane = New AjaxControlToolkit.AccordionPane
            With ajAccdPane
                .ID = ("Mn" & iloop.ToString("00"))
                .HeaderContainer.ID = ("AcMn" & iloop.ToString("00"))
                ''.BackColor = Drawing.Color.Aqua
                'If showMenu(iloop).naviURL <> "#" Then
                '    '.HeaderCssClass = ""
                '    .HeaderContainer.CssClass = "accordionHeaderNotSelected"
                'Else
                '    .HeaderContainer.CssClass = "accordionHeader"
                'End If

                Select Case showMenu(iloop).graphCode

                    Case "TXT"                                              ' テキストファイルダウンロード
                        If ULNo(0) = 1 Then
                            addFlg = 1
                        End If

                    Case "MNG"                                              ' 管理用
                        If ULNo(1) = 1 Then
                            addFlg = 1
                        End If

                    Case "SCH"                                              ' 現場切替 

                        addFlg = 1

                    Case "P01"                                              ' 画像送信
                        If ULNo(3) = 1 Then
                            addFlg = 1
                        End If

                    Case "LNK"                                              ' 外部リンク

                        Using headerLabel As New Literal
                            headerLabel.Text = showMenu(iloop).itemText
                            .HeaderContainer.Controls.Add(headerLabel)
                            .HeaderContainer.Style("display") = "block"
                            addFlg = 2

                            For jloop = 0 To linkMenus.Length - 1
                                Using linkButton As New HyperLink 'Button
                                    linkButton.Text = linkMenus(jloop).itemText
                                    linkButton.NavigateUrl = linkMenus(jloop).naviURL
                                    linkButton.Target = "_blank"
                                    linkButton.Width = menuProp._Width - 15
                                    linkButton.Font.Size = 8
                                    'linkButton.Style("text-align") = "Left"
                                    .ContentContainer.ID = ("Mn" & iloop.ToString("00") & jloop.ToString("_000"))
                                    .ContentContainer.Controls.Add(linkButton)
                                End Using
                            Next
                        End Using
                    Case "CON"                                              ' 現場PC制御系       2015/11/25 Kino Add まだ途中・・・

                        If showMenu(iloop).itemText.IndexOf("制御") > -1 Then
                            adminCheck = getPcControlAuthority(showMenu(iloop).itemText, ULNo(4))
                        End If


                    Case Else                                               ' その他帳票類

                        Using headerLabel As New Literal
                            headerLabel.Text = showMenu(iloop).itemText
                            .HeaderContainer.Controls.Add(headerLabel)
                            .HeaderContainer.Style("display") = "block"
                            addFlg = 2

                            For jloop = 0 To loopCount
                                If subMenus(jloop).target = showMenu(iloop).itemText AndAlso (subMenus(jloop).LayerItem(0) = "00" And subMenus(jloop).LayerItem(1) = "00") Then ' トップ階層に直接リンクを張る設定の場合
                                    winopenParam = makeWindowOpenParameters(subMenus, jloop, openWinParam)
                                    ''winopenParam = "window.open('" & subMenus(jloop).naviURL & "?GNa=" & HttpUtility.UrlEncode(subMenus(jloop).itemText) & _
                                    ''                                "&W=" & subMenus(jloop).width & _
                                    ''                                "&H=" & subMenus(jloop).height & "','" & _
                                    ''                                subMenus(jloop).target & _
                                    ''                                "','width=" & subMenus(jloop).width & _
                                    ''                                ",height=" & subMenus(jloop).height & "," & _
                                    ''                                openWinParam & "');return false;"
                                    Using LinkButton As New HyperLink 'Button
                                        LinkButton.Text = subMenus(jloop).graphCode
                                        LinkButton.NavigateUrl = "javascript:void(0);"
                                        LinkButton.Attributes.Add("onclick", winopenParam)
                                        'linkButton.Target = "_blank"
                                        LinkButton.Width = menuProp._Width - 15
                                        .ContentContainer.ID = ("Mn" & iloop.ToString("00") & jloop.ToString("_000"))
                                        .ContentContainer.Controls.Add(LinkButton)
                                        .ContentContainer.Style("display") = "block"
                                    End Using
                                End If
                            Next
                        End Using
                End Select

                If addFlg = 1 Or (showMenu(iloop).itemText.IndexOf("制御") > -1 AndAlso adminCheck = True) Then
                    winopenParam = makeWindowOpenParameters(showMenu, iloop, openWinParam)
                    ''winopenParam = "window.open('" & showMenu(iloop).naviURL & "?&W=" & showMenu(iloop).width & _
                    ''                                "&H=" & showMenu(iloop).height & "','" & _
                    ''                                showMenu(iloop).target & _
                    ''                                "','width=" & showMenu(iloop).width & _
                    ''                                ",height=" & showMenu(iloop).height & "," & _
                    ''                                openWinParam & "');return false;"
                    Using LinkButton As New HyperLink
                        LinkButton.Text = showMenu(iloop).itemText
                        LinkButton.NavigateUrl = "javascript:void(0);"          ''showMenu(iloop).naviURL
                        LinkButton.Attributes.Add("onclick", winopenParam)
                        'linkButton.Target = showMenu(iloop).target
                        LinkButton.Width = menuProp._Width - 15
                        .HeaderContainer.Controls.Add(LinkButton)
                        .HeaderContainer.Style("text-align") = "center"
                        .HeaderContainer.Style("display") = "block"
                    End Using
                End If
            End With

            If addFlg >= 1 Then Me.MyAccordion.Panes.Add(ajAccdPane)

        Next iloop

    End Sub

    ''' <summary>
    ''' 現場PC制御ページの使用権限確認（ユーザーアカウント権限）
    ''' </summary>
    ''' <param name="gCode">グラフコード</param>
    ''' <param name="UL">ユーザーレベル(右から5番目)</param>
    ''' <returns>権限のあり:True　なし:False</returns>
    ''' <remarks>2014/09/05 Kino Add</remarks>
    Private Function getPcControlAuthority(ByVal gCode As String, ByVal UL As String) As Boolean

        Dim blnRet As Boolean = False
        Dim strTemp As String = Nothing
        Dim gCodeNo As Integer = 0
        Dim ULNo As Integer = 0

        If gCode.Length = 3 Then
            Return blnRet
        End If

        strTemp = gCode.Substring(gCode.Length - 1)
        If IsNumeric(strTemp) = True Then
            gCodeNo = CType(strTemp, Integer)
            ULNo = CType(UL, Integer)
            If ULNo = 9 Or (gCodeNo And ULNo) <> 0 Then
                blnRet = True
            End If
        End If

        Return blnRet

    End Function

    Protected Sub TScMan1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs) Handles TScMan1.AsyncPostBackError

        Using sw As New IO.StreamWriter(Server.MapPath("~/log/errorAsync_" & Date.Now.ToString("yyMM") & ".log"), True, Encoding.GetEncoding("Shift_JIS"))

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

            'sw.Dispose()
        End Using
    End Sub

End Class

