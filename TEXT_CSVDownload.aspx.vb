Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports C1.C1Zip

Partial Class TEXT_CSVDownload
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

    Private mcsvDownloadInf() As csvDLInfo
    Private mDataFileNames() As ClsReadDataFile.DataFileInf
    Private mblnthinoutEnable As Boolean                    '間引きチェックの有無
    Private mstrthinoutCh As String                         '間引きチャンネル情報
    ''Protected WithEvents DLButton As System.Web.UI.WebControls.Button

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        ''
        '' ページを閉じるときのイベント
        ''

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

        clsLogWrite.writeErrorLog(ex, Server.MapPath("~/log/errorSummary_" & Date.Now.ToString("yyMM") & ".log"), userName, siteDirectory, MyBase.GetType.BaseType.FullName)

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
        Dim DLInfoFile As String
        Dim intRet As Integer
        Dim clsNewDate As New ClsReadDataFile
        Dim clsSetScript As New ClsGraphCommon
        Dim EdDate As Date
        Dim StDate As Date
        Dim iloop As Integer
        Dim OldTerm As String

        OldTerm = CType(Session.Item("OldTerm"), String)            '過去データ表示制限
        siteName = CType(Session.Item("SN"), String)                '現場名
        siteDirectory = CType(Session.Item("SD"), String)           '現場ディレクトリ
        DLInfoFile = Server.MapPath(siteDirectory & "\App_Data\csvDownloadInfo.mdb")

        ''セッションタイムアウトした時には、ログイン画面を出す
        If [String].IsNullOrEmpty(siteDirectory) = True Then Response.Redirect("Login.aspx", False)

        intRet = clsNewDate.GetDataFileNames(Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb"), mDataFileNames)       'データファイル名、共通情報ファイル名、識別名を取得
        intRet = clsNewDate.GetDataLastUpdate(Server.MapPath(siteDirectory & "\App_Data\"), mDataFileNames)                  'データファイルにおける最新・最古データ日時を取得

        If IsPostBack = False Then

            ''If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
            ''    ''==== フォームのサイズを調整する ====
            ''    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
            ''        Dim OpenString As String

            ''        OpenString = "<SCRIPT LANGUAGE='javascript'>"
            ''        OpenString &= "window.resizeTo(" & wid & "," & Hei & ");"
            ''        OpenString &= "<" & "/SCRIPT>"

            ''        'Dim instance As ClientScriptManager = Page.ClientScript                            'この方法から↓の方法へ変更した
            ''        'instance.RegisterClientScriptBlock(Me.GetType(), "経時変化図", OpenString)
            ''        Page.ClientScript.RegisterStartupScript(Me.GetType(), "CSVDL", OpenString)
            ''    End If
            ''End If

            ''初回読み込みはデータベースから行なう
            Dim DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DLInfoFile & ";" & "Jet OLEDB:Engine Type= 5"
            ''DbCon.Open()
            intRet = ReadCSVInfoFromDB(DbCon, False)
            Call SetBIND2Controls(DbCon)

            ''DbCon.Close()
            DbCon.Dispose()

            If intRet >= 7 Then Me.CBLDownloadList.RepeatColumns = 2

            'チェックボックスリストに項目を追加
            For iloop = 0 To intRet
                Me.CBLDownloadList.Items.Add(mcsvDownloadInf(iloop).ItemName)
                Me.CBLDownloadList.Items(iloop).Value = mcsvDownloadInf(iloop).csvFileName & ControlChars.Tab & mcsvDownloadInf(iloop).dataFileNo & ControlChars.Tab & mcsvDownloadInf(iloop).chData & mcsvDownloadInf(iloop).sortType
            Next

            EdDate = GetMostUpdate()
            StDate = GetMostOldDate()
            Me.RngValidSt.MaximumValue = EdDate.ToString("yyyy/MM/dd")
            Me.RngValidEd.MaximumValue = EdDate.ToString("yyyy/MM/dd")

            ''変数の内容を各コントロールに配置する
            intRet = Set2FormControl()

            Me.TxtEndDate.Text = EdDate.ToString("yyyy/MM/dd")                  'データファイルの最新の日とする
            Call CalcDate(Me.DDLRange.SelectedValue)                            'スタート日付を算出してテキストボックスに入れる

            Call SetViewState()

            Call clsSetScript.SetSelectDateScript(Me.Form)                      'コントロールにJavaScriptを割り当て

            ''
            '' コントロールのイベントにJavaScriptの関数を割り当てる
            ''

            'カレンダー表示の設定
            ''imgCalTxtStartDate.Attributes.Add("onclick", "DropCalendar(this);")   ' 2012/11/02 Kino Changed jQuery Ajaxと同梱できないのでコメント
            ''imgCalTxtEndDate.Attributes.Add("onclick", "DropCalendar(this);")

            ''UpdatePanelからJavascriptへ制御を移行した
            Me.RdBDsignatedDate.Attributes.Add("onclick", "ChangeSelState(this);")
            Me.RdBFromNewest.Attributes.Add("onclick", "ChangeSelState(this);")

            Dim OldDate As Date
            If OldTerm <> "None" Then
                Dim clsCalc As New ClsReadDataFile
                OldDate = clsCalc.CalcOldDateLimit(OldTerm)
                clsCalc = Nothing
            Else
                OldDate = mDataFileNames(0).MostOldestDate
            End If
            Me.RngValidSt.MinimumValue = OldDate.ToShortDateString
            Me.RngValidSt.ErrorMessage = "開始日は" + OldDate.ToString("yyyy/MM/dd") + "以降としてください。"
            Me.RngValidEd.MinimumValue = OldDate.ToShortDateString
            Me.RngValidEd.ErrorMessage = "終了日は" + EdDate.ToString("yyyy/MM/dd") + "以前としてください。"
            Me.C1WebCalendar.MinDate = OldDate
            Me.C1WebCalendar.MaxDate = EdDate

        Else

            ''ポストバックの場合の処理
            ''ビューステートの値を変数へ格納する()
            intRet = ReadViewState()

            ''Dim o As Integer = CType(ViewState("AsyncPostBackFlg"), Integer)
            ' ''            If (IsNothing(o)) = False Then
            ''If o = 1 Then
            ''    ReadSetRadioButtonViewstate()
            ''    ViewState.Remove("AsyncPostBackFlg")
            ''    '        End If
            ''End If

            'コントロールの値を変数へ格納する
            'Call ReadFromFormControl()

            ''If Me.RdBFromNewest.Checked = True Then Call CalcDate(Me.DDLRange.SelectedValue)

        End If

        Me.LblLastUpdate.Text = "最新データ日時：" & EdDate.ToString("yyyy/MM/dd HH:mm")

        Me.Title = "[テキストファイルダウンロード] - " & siteName

        ''If Not ScriptManager1.IsInAsyncPostBack Then                                    '非同期ポストバックでないときに処理する　アップデートパネル内のみの変更の場合
        ''Call DynamicMakeButtons()

    End Sub

    Protected Function ReadViewState() As Integer
        ''
        '' ビューステートから取得した内容を変数へ格納する　　　ビューステート　→　変数
        ''
        Dim iloop As Integer
        Dim strCnt As String
        Dim LoocCount As Integer


        LoocCount = CType(ViewState("ItemCount"), Short)                                            '表示チャンネル Integer

        ReDim mcsvDownloadInf(LoocCount)                                                             '個々の列の情報を格納する配列設定

        For iloop = 0 To LoocCount
            strCnt = iloop.ToString
            mcsvDownloadInf(iloop).ItemName = CType(ViewState("ItemName" & strCnt), String)         '項目名             String
            mcsvDownloadInf(iloop).chData = CType(ViewState("chData" & strCnt), String)             'チャンネル情報     String
            mcsvDownloadInf(iloop).csvFileName = CType(ViewState("csvFileName" & strCnt), String)   'csvファイル名      String
            mcsvDownloadInf(iloop).dataFileNo = CType(ViewState("dataFileNo" & strCnt), Integer)    'データファイルNo   Integer 
            mcsvDownloadInf(iloop).sortType = CType(ViewState("sortType" & strCnt), Short)          'ソートタイプ       Short
        Next iloop

        Return LoocCount

    End Function

    Protected Sub SetViewState()
        ''
        '' 変数から取得した内容をビューステートに格納する     変数　→　ビューステート
        ''
        Dim iloop As Integer
        Dim strCnt As String

        For iloop = 0 To mcsvDownloadInf.Length - 1
            strCnt = iloop.ToString
            ViewState("ItemCount") = (mcsvDownloadInf.Length - 1).ToString                  '配列数             Integer
            ViewState("ItemName" & strCnt) = mcsvDownloadInf(iloop).ItemName                '項目名             String
            ViewState("chData" & strCnt) = mcsvDownloadInf(iloop).chData                    'チャンネル情報     String
            ViewState("csvFileName" & strCnt) = mcsvDownloadInf(iloop).csvFileName          'csvファイル名      String
            ViewState("dataFileNo" & strCnt) = mcsvDownloadInf(iloop).dataFileNo.ToString   'データファイルNo   Integer
            ViewState("sortType" & strCnt) = mcsvDownloadInf(iloop).sortType.ToString       'ソートタイプ       Short
        Next iloop

    End Sub

    Public Sub SetBIND2Controls(ByVal dbCon As OleDb.OleDbConnection)
        ''
        ''ドロップダウンリストボックスにおける各設定のデータセットバインド
        ''
        Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strSQL As String

        ''■■日付範囲
        strSQL = "SELECT * FROM set_日付範囲 WHERE 有効 = True ORDER BY ID"
        '' 現場名称の読込
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        With Me.DDLRange
            .DataSource = DtSet.Tables("DData")
            .DataMember = DtSet.DataSetName
            .DataTextField = "表示"
            .DataValueField = "値"
            .DataBind()
        End With

        DbDa.Dispose()
        DbCom.Dispose()
        DtSet.Dispose()

    End Sub

    Protected Function Set2FormControl() As Integer
        ''
        '' データベースから変数に格納した内容を、画面のコントロールへ配置する
        ''
        Dim DatCh() As Integer = {}

        '開始日の初期状態を設定
        Me.RdBFromNewest.Checked = True
        ''Me.TxtStartDate.Text = TblProp.LastUpdate.ToString("yyyy/MM/dd")
        ''Me.TxtEndDate.Text = TblProp.LastUpdate.ToString("yyyy/MM/dd")

        ''間引き情報作成                                                    '2009/06/09 Kino Add
        If mblnthinoutEnable = True Then
            Me.ChbPartial.Checked = True
            If mstrthinoutCh <> "None" Then
                Dim clsCh As New ClsReadDataFile
                Dim iloop As Integer
                Dim strRet As String = clsCh.GetOutputChannel(DatCh, mstrthinoutCh)
                clsCh = Nothing

                For iloop = 0 To DatCh.Length - 1
                    Me.CBLPartial.Items(DatCh(iloop)).Selected = True
                Next
            End If

        End If

        Return DatCh.Length - 1

    End Function


    Protected Function GetMostUpdate() As Date
        ''
        '' 使用しているデータファイルの中から、一番最新のデータ日付を取得する
        ''
        Dim iloop As Integer
        Dim dteTemp As Date
        Dim dteSession As Date

        For iloop = 0 To mcsvDownloadInf.Length - 1                        '使用しているデータファイル番号の中で最新のデータ日時を取得する
            dteSession = CType(Session.Item("LastUpdate" & mcsvDownloadInf(iloop).dataFileNo.ToString), Date)
            If dteTemp < dteSession Then
                dteTemp = dteSession
            End If
        Next

        Return dteTemp

    End Function

    Protected Function GetMostOldDate() As Date
        ''
        '' 使用しているデータファイルの中から、一番最新のデータ日付を取得する
        ''
        Dim iloop As Integer
        Dim dteTemp As Date = Date.Now()
        Dim dteSession As Date

        For iloop = 0 To mcsvDownloadInf.Length - 1                        '使用しているデータファイル番号の中で最新のデータ日時を取得する
            dteSession = CType(Session.Item("OldestDate" & mcsvDownloadInf(iloop).dataFileNo.ToString), Date)
            If dteTemp > dteSession Then
                dteTemp = dteSession
            End If
        Next

        Return dteTemp

    End Function

    Protected Function ReadCSVInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, Optional ByVal PostBackFlg As Boolean = False) As Integer
        ''
        '' CSV出力情報を読み込む
        ''

        Dim strSQL As String
        '' Dim DbDr As OleDb.OleDbDataReader
        '' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim intRows As Integer
        Dim DummyCount As Integer = 0

        ReadCSVInfoFromDB = 0

        strSQL = "SELECT COUNT(ダウンロード項目名) AS RowCount FROM メニュー基本情報"
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        If DtSet.Tables("DData").Rows.Count > 0 Then
            intRows = Convert.ToInt32(DtSet.Tables("DData").Rows(0).Item(0))
        Else
            Return 0
            Exit Function
        End If

        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        .Read()
        ''        intRows = .GetInt32(0)
        ''    Else
        ''        Return 0
        ''        Exit Function
        ''    End If
        ''End With

        ReDim mcsvDownloadInf(intRows - 1)

        strSQL = "SELECT * FROM メニュー基本情報 ORDER BY ID"
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DtSet = New DataSet
        DbDa.Fill(DtSet, "DData")

        For Each DTR As DataRow In DtSet.Tables("DData").Rows
            With DTR.ItemArray
                mcsvDownloadInf(DummyCount).ItemName = .GetValue(1).ToString            'ダウンロード項目名
                mcsvDownloadInf(DummyCount).chData = .GetValue(2).ToString              '処理チャンネル
                mcsvDownloadInf(DummyCount).csvFileName = .GetValue(3).ToString         'CSVファイル名
                mcsvDownloadInf(DummyCount).dataFileNo = Convert.ToInt32(.GetValue(4))  'データファイルNo
                Try
                    mcsvDownloadInf(DummyCount).sortType = Convert.ToInt16(.GetValue(5))        'チャンネル番号ソート
                Catch ex As Exception
                    mcsvDownloadInf(DummyCount).sortType = 1
                End Try

                DummyCount += 1
            End With
        Next

        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        Do While .Read()
        ''            mcsvDownloadInf(DummyCount).ItemName = .GetString(1)             'ダウンロード項目名
        ''            mcsvDownloadInf(DummyCount).chData = .GetString(2)               '処理チャンネル
        ''            mcsvDownloadInf(DummyCount).csvFileName = .GetString(3)          'CSVファイル名
        ''            mcsvDownloadInf(DummyCount).dataFileNo = .GetInt16(4)            'データファイルNo
        ''            DummyCount += 1
        ''        Loop
        ''    End If
        ''End With

        strSQL = "SELECT * FROM 間引情報"
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DtSet = New DataSet
        DbDa.Fill(DtSet, "DData")

        For Each DTR As DataRow In DtSet.Tables("DData").Rows
            With DTR.ItemArray
                mblnthinoutEnable = Convert.ToBoolean(.GetValue(0))                         '間引きチェック
                mstrthinoutCh = .GetValue(1).ToString                                   '間引きする
            End With
        Next
        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        .Read()
        ''        mblnthinoutEnable = .GetBoolean(0)                                  '間引きチェック
        ''        mstrthinoutCh = .GetString(1)                                       '間引きする
        ''    End If
        ''End With

        ''DbDr.Close()
        ''DbCom.Dispose()

        DbDa.Dispose()
        DtSet.Dispose()

        Return intRows - 1

    End Function

    ''Protected Sub DynamicMakeButtons()
    ''    ''
    ''    '' ボタンの動的生成
    ''    ''
    ''    Dim iloop As Integer
    ''    ''Dim dlButton(5) As System.Web.UI.WebControls.Button

    ''    PnlDownLoad.Controls.Add(New HtmlGenericControl("br"))

    ''    For iloop = 0 To 10
    ''        ''dlButton(iloop) = New System.Web.UI.WebControls.Button
    ''        ''dlButton(iloop).ID = "dlBtn" & iloop
    ''        ''dlButton(iloop).Text = "ダウンロード" & iloop
    ''        ''dlButton(iloop).Attributes.Add("class", "BTN_MAIN")
    ''        ''AddHandler dlButton(iloop).Click, AddressOf btnDL_Click
    ''        ''PnlDownLoad.Controls.Add(DLButton(iloop))

    ''        DLButton = New System.Web.UI.WebControls.Button
    ''        With DLButton
    ''            .ID = "dlBtn" & iloop
    ''            .Text = "ダウンロード" & iloop
    ''            .Attributes.Add("class", "BTN_MAIN")
    ''            '.Style("Position") = "Absolute"
    ''            .Style("Width") = 350 & "px"

    ''            AddHandler .Click, AddressOf btnDL_Click
    ''            ''PnlDownLoad.Controls.Add(New LiteralControl("　　"))
    ''            PnlDownLoad.Controls.Add(DLButton)
    ''            PnlDownLoad.Controls.Add(New HtmlGenericControl("br"))
    ''        End With
    ''    Next
    ''End Sub

    ''Protected Sub btnDL_Click(ByVal sender As Object, ByVal e As System.EventArgs)

    ''    System.Diagnostics.Debug.WriteLine(sender.ClientId)

    ''End Sub


    Protected Sub RblDayRange_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        ''
        '' ラジオボタンのイベントハンドラ
        ''
        Call CalcDate(Me.DDLRange.SelectedItem.Value)

    End Sub

    Protected Sub CalcDate(ByVal setDateProperty As String)
        ''
        '' 選択されたラジオボタンから日付を検索
        ''

        Dim Interval As Integer
        Dim DateType As String
        ''Dim CalculatedDate As Date
        Dim endDate As Date
        Dim startDate As Date
        Dim clsCalc As New ClsReadDataFile

        ''        If Me.rblDayRange.SelectedItem.Value.Equals("99X") = True Then
        If setDateProperty = "99X" Then                                                 '「任意」の場合
            ''Me.TxtStartDate.BackColor = Drawing.Color.White
            ''Me.TxtEndDate.BackColor = Drawing.Color.White

            ''Me.TxtStartDate.ReadOnly = False
            ''Me.TxtEndDate.ReadOnly = False
            ''Me.ImgStDate.Attributes.Add("onclick", "DropCalendar(this);")
            ''Me.ImgEdDate.Attributes.Add("onclick", "DropCalendar(this);")
            ''Me.ImgStDate.Visible = True
            ''Me.ImgEdDate.Visible = True
        Else                                                                            '「任意」以外の場合
            'Me.TxtEndDate.Text = GrpProp.LastUpdate.ToString("yyyy/MM/dd")
            ''Me.TxtStartDate.BackColor = Drawing.Color.LightGray
            ''Me.TxtEndDate.BackColor = Drawing.Color.LightGray

            ''Me.TxtStartDate.ReadOnly = True
            ''Me.TxtEndDate.ReadOnly = True
            ''Me.ImgStDate.Attributes.Clear()
            ''Me.ImgEdDate.Attributes.Clear()
            ''Me.ImgStDate.Visible = False
            ''Me.ImgEdDate.Visible = False

            'endDate = ClsReadDataFile.CalcEndDate(Me.RdBFromNewest.Checked, GrpProp.LastUpdate, Me.TxtEndDate.Text, 1)

            endDate = Date.Parse(Me.TxtEndDate.Text)
            Interval = Integer.Parse(setDateProperty.Substring(0, 2))
            DateType = setDateProperty.Substring(2, 1)

            'If Me.RdBFromNewest.Checked = True Then                                 '最新データから
            Select Case DateType
                Case "A"
                    startDate = Date.Parse(CType(Session.Item("OldestDate"), String))

                Case Else                                                       '指定日数、月、年

                    startDate = clsCalc.CalcStartDate(Me.TxtStartDate.Text, endDate, DateType, Interval, 1)
            End Select

            Me.TxtStartDate.Text = startDate.ToString("d")
            'Else                                                                    '指定期間

            'startDate = Date.Parse(Me.TxtStartDate.Text)

        End If

        ''DateType = setDateProperty.Substring(2, 1)
        ''If DateType = "D" Then
        ''    EndDate = (DateTime.Parse(Me.TxtEndDate.Text)).AddDays(+1)
        ''Else
        ''    EndDate = DateTime.Parse(Me.TxtEndDate.Text)
        ''End If

        ''Interval = Double.Parse(setDateProperty.Substring(0, 2))

        '' '' 指定日後の日付を計算する
        ''CalculatedDate = ClsReadDataFile.CalcDateInterval(DateType, EndDate, Interval)

        ''Me.TxtStartDate.Text = CalculatedDate.ToString("d")    '描画終了日

    End Sub

    ''Protected Sub RdBDsignatedDate_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles RdBDsignatedDate.CheckedChanged
    ''    ''
    ''    '' 指定日からを選択した場合のイベント
    ''    ''

    ''    Me.TxtStartDate.ReadOnly = False
    ''    Me.TxtEndDate.ReadOnly = False
    ''    Me.ImgStDate.Visible = True
    ''    Me.ImgEdDate.Visible = True
    ''    Me.DDLRange.Enabled = False

    ''End Sub

    ''Protected Sub RdBFromNewest_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles RdBFromNewest.CheckedChanged
    ''    ''
    ''    '' 最新からを選択した場合のイベント
    ''    ''

    ''    Me.TxtStartDate.ReadOnly = True
    ''    Me.TxtEndDate.ReadOnly = True
    ''    Me.ImgStDate.Visible = False
    ''    Me.ImgEdDate.Visible = False
    ''    Me.DDLRange.Enabled = True

    ''End Sub

    Protected Sub ImBDownload_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImBDownload.Click

        Dim iloop As Integer
        Dim jloop As Integer
        Dim kloop As Integer
        Dim outCh() As Integer = {}
        Dim intRet As Integer
        Dim strMsg As String = ""
        Dim MakeFileFlg As Integer = 0
        Dim siteName As String = CType(Session.Item("SN"), String)                            '現場名
        If siteName.Length > 16 Then siteName = CType(Session.Item("SSN"), String) '2009/12/21 Kino Add 名称が長い場合は省略名称を入れる
        Dim siteDirectory As String = CType(Session.Item("SD"), String)                       '現場ディレクトリ
        Dim ZipFilePath As String = Server.MapPath("temp\")
        Dim ZipFileName As String = siteName & Now().ToString("_MMddHHmm") & ".zip"             'yyyy　外し
        Dim csvFileName As String = siteName & Now().ToString("_MMddHHmm") & ".csv"             'yyyy　外し
        Dim SensorInf As CommInf
        Dim csvData() As Array = {}
        Dim strLineData As String = ""
        Dim zip As New C1ZipFile                                                                'とりあえずNewをつけると警告が出ない
        Dim sw As StreamWriter
        Dim aa As New AjaxControlToolkit.AnimationExtender
        Dim clsconvChData As New ClsReadDataFile
        Dim strText As New System.Text.StringBuilder()                                          ' 2012/02/08 Kino Add 文字列結合をStringBuilderへ変更
        Dim ZipFileFullPath As String = Nothing                                                 ' 2018/03/15 Kino Add

        ZipFileName = ZipFileName.Replace(" ", "")                                              ' 2018/06/22 Kino Changed 半角と全角スペースをなしにする　文字化け対策　
        ZipFileName = ZipFileName.Replace("　", "")
        csvFileName = csvFileName.Replace(" ", "")                                              ' 2018/06/22 Kino Changed 半角と全角スペースをなしにする　文字化け対策　
        csvFileName = csvFileName.Replace("　", "")

        Try
            Dim dinf As New DirectoryInfo(ZipFilePath)
            For Each file As FileInfo In dinf.GetFiles()
                If file.CreationTime.AddMinutes(3) < DateTime.Now Then file.Delete()
            Next
        Catch

        End Try

        Dim clsRD As New ClsReadDataFile                                            ' 2012/02/08 Kino Add 異常値の表示変換情報読込
        Dim DbCon As New OleDb.OleDbConnection
        Dim strSQL As String
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim exchangeValue As Boolean                                                ' 異常値の表示変換の有無
        Dim exchangeWord As String = Nothing                                        ' 異常値の表示変換文字列
        ' Dim writeData As Single                                                     ' 出力データ

        ' 2012/02/08 Kino Add =============== ↓
        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & Server.MapPath(siteDirectory & "\App_Data\csvDownloadInfo.mdb") & ";" & "Jet OLEDB:Engine Type= 5")
        If clsRD.isExistTable(DbCon, "異常値変換") = True Then

            strSQL = "SELECT * FROM 異常値変換"
            DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
            DtSet = New DataSet
            DbDa.Fill(DtSet, "DData")

            If DtSet.Tables("DData").Rows.Count > 0 Then
                exchangeValue = Convert.ToBoolean(DtSet.Tables("DData").Rows(0).Item(0))
                exchangeWord = DtSet.Tables("DData").Rows(0).Item(1).ToString
            Else
                exchangeValue = False
            End If

            If exchangeWord = "Null" Then exchangeWord = ""
            DbDa.Dispose()
            DtSet.Dispose()

        Else

            exchangeValue = False

        End If
        clsRD = Nothing
        ' 2012/02/08 Kino Add ===============↑

        ''Dim CheckItem As String = ""
        ''Dim SelItem As ListItem
        ''For Each SelItem In Me.CBLDownloadList.Items                              'チェック項目の確認
        ''    If SelItem.Selected = True Then
        ''        CheckItem &= SelItem.Value & ","
        ''    End If
        ''Next

        For iloop = 0 To Me.CBLDownloadList.Items.Count - 1                         'チェック項目の確認
            If Me.CBLDownloadList.Items(iloop).Selected = True Then
                mcsvDownloadInf(iloop).Operate = True
                ''CheckItem &= iloop & ","
                MakeFileFlg = 1
            Else
                mcsvDownloadInf(iloop).Operate = False
            End If
        Next iloop

        ''If CheckItem.Length <> 0 Then
        ''    CheckItem = CheckItem.Substring(0, CheckItem.Length - 1)
        ''End If

        If MakeFileFlg = 1 Then                                                     '出力項目が選択されていた場合
            System.Windows.Forms.Application.DoEvents()

            If Me.RBLFileFormat.Items(0).Selected = True Then                       '「圧縮する」の場合
                ' メモリストリームを作成します。

                ZipFileFullPath = Path.Combine(ZipFilePath.Trim, ZipFileName.Trim)       ' 2018/03/15 Kino Add
                zip = New C1ZipFile(ZipFileFullPath)                                'ZIPFileクラスの新しいインスタンスを初期化します。
            End If

            For iloop = 0 To mcsvDownloadInf.Length - 1                             'データ読込

                SensorInf.ChNo = ""                                                 '構造体の初期化
                SensorInf.SensorSymbol = ""
                SensorInf.Unit = ""

                If mcsvDownloadInf(iloop).Operate = True Then

                    Try
                        Dim ms As New MemoryStream()
                        If Me.RBLFileFormat.Items(0).Selected = True Then
                            sw = New StreamWriter(ms, Encoding.GetEncoding("shift-jis"))                                '「圧縮する」の場合
                        Else
                            '' sw = New StreamWriter(ZipFilePath & csvFileName, True, Encoding.GetEncoding("shift-jis"))    '「圧縮しない」の場合
                            sw = New StreamWriter(Path.Combine(ZipFilePath.Trim, csvFileName.Trim), True, Encoding.GetEncoding("shift-jis"))    '「圧縮しない」の場合   2018/03/15 Kino Changed  Add Combine & trim
                        End If

                        ''Call GetOutputChannel(outCh, iloop)                                         'チャンネル取得 ↓クラスへ移行
                        ' Dim strRet As String = clsconvChData.GetOutputChannel(outCh, mcsvDownloadInf(iloop).chData, 1)
                        Dim strRet As String = clsconvChData.GetOutputChannel(outCh, mcsvDownloadInf(iloop).chData, mcsvDownloadInf(iloop).sortType)
                        intRet = ReadDatas(outCh, iloop, SensorInf, csvData)                        'データ読込み

                        sw.WriteLine(Me.CBLDownloadList.Items(iloop).Text)                          'ヘッダ作成
                        sw.WriteLine("単位," & SensorInf.Unit)
                        sw.WriteLine("日付," & SensorInf.SensorSymbol)

                        strText.Length = 0
                        If exchangeValue = True Then                                                    ' 2012/02/08 Kino Add
                            For jloop = 0 To csvData.Length - 1
                                'strLineData = ""                                                       ' 2012/01/11 Kino Changed  moved under
                                For kloop = 0 To csvData(jloop).Length - 1
                                    If kloop <> 0 Then
                                        If Convert.ToSingle(csvData(jloop).GetValue(kloop)) >= 7.0E+30 Then
                                            strText.Append(exchangeWord & ",")
                                        Else
                                            strText.Append((csvData(jloop).GetValue(kloop)).ToString & ",")
                                        End If
                                    Else
                                        strText.Append(csvData(jloop).GetValue(kloop).ToString & ",")
                                    End If
                                    ' strLineData &= csvData(jloop).GetValue(kloop).ToString & ","
                                Next kloop
                                If strText.Length <> 0 Then
                                    strText.Remove(strText.Length - 1, 1)
                                End If
                                ' strLineData = strLineData.Substring(0, strLineData.Length - 1)
                                sw.WriteLine(strText.ToString)
                                ' sw.WriteLine(strLineData)
                                strText.Length = 0
                                ' strLineData = ""
                            Next jloop

                        Else

                            For jloop = 0 To csvData.Length - 1
                                'strLineData = ""                                                       ' 2012/01/11 Kino Changed  moved under
                                For kloop = 0 To csvData(jloop).Length - 1
                                    strText.Append(csvData(jloop).GetValue(kloop).ToString & ",")
                                    ' strLineData &= csvData(jloop).GetValue(kloop).ToString & ","
                                Next kloop
                                If strText.Length <> 0 Then
                                    strText.Remove(strText.Length - 1, 1)
                                End If
                                'strLineData = strLineData.Substring(0, strLineData.Length - 1)
                                sw.WriteLine(strText.ToString)
                                ' sw.WriteLine(strLineData)
                                strText.Length = 0
                                ' strLineData = ""
                            Next jloop
                        End If

                        sw.WriteLine(ControlChars.CrLf)
                        sw.Flush()
                        ms.Position = 0

                        Using ms

                            If Me.RBLFileFormat.Items(0).Selected = True Then                   '「圧縮する」の場合
                                ' メモリストリームを圧縮してファイルに保存します。
                                zip.Entries.Add(ms, mcsvDownloadInf(iloop).csvFileName)         'メモリストリームにファイル名をつけてzip化する
                            End If
                        End Using

                        sw.Close()
                        'ms.Close()

                        If intRet = 9999 Then
                            strMsg &= mcsvDownloadInf(iloop).ItemName & ControlChars.CrLf
                        End If

                    Catch ex As Exception

                        If Me.RBLFileFormat.Items(0).Selected = True Then
                            zip.Close()
                        End If

                    End Try
                End If

            Next

            Response.Clear()
            If Me.RBLFileFormat.Items(0).Selected = True Then                           '「圧縮する」の場合
                zip.Close()

                ' ファイルをHTTPレスポンスとして送信します。
                Response.Clear()
                Response.ContentType = "application/zip"
                'Response.AddHeader("Content-Disposition", "inline; filename=" & HttpUtility.UrlEncode(ZipFileName))
                Response.AddHeader("Content-Disposition", "inline; filename=" & ZipFileName & ";filename*=utf-8''" & HttpUtility.UrlEncode(ZipFileName))
                Response.BufferOutput = True
                Response.WriteFile(Path.Combine(ZipFilePath, ZipFileName))              ' 2018/03/15 Kino Changed   Add  combine
                '' Response.WriteFile(ZipFilePath & ZipFileName)

            Else                                                                        '「圧縮しない」の場合

                ' ファイルをHTTPレスポンスとして送信します。
                Response.Clear()
                Response.ContentType = "application/octet-stream"
                'Response.AddHeader("Content-Disposition", "attachment; filename=" & HttpUtility.UrlEncode(csvFileName))
                Response.AddHeader("Content-Disposition", "attachment; filename=" & csvFileName & ";filename*=utf-8''" & HttpUtility.UrlEncode(csvFileName))
                Response.BufferOutput = True
                Response.WriteFile(ZipFilePath & csvFileName)

            End If

            'aa.TargetControlID = "PnlGraphSet"
            'aa.Animations = vbNewLine & "<Sequence>" & vbNewLine & "<FadeIn />" & vbNewLine & "<EnableAction Enabled='True' />" & vbNewLine & "</Sequence>" & vbNewLine
            'Me.Controls.Add(aa)
            'aa.EnableClientState = True

            If strMsg.Length <> 0 Then                                              'メッセージ処理
                Call JMsgBox("チャンネル設定に問題があるため" & strMsg & "の処理は行ないませんでした。")
            End If

            Response.End()

            System.Threading.Thread.Sleep(2000)
            clsconvChData = Nothing

        Else                                                                        '出力項目が１つも選択されていない場合

            Call JMsgBox("出力項目が１つも選択されていません。確認してください。")
        End If

        ''aa.TargetControlID = "PnlGraphSet"
        ''aa.Animations = "<Sequence>" & vbNewLine & "<FadeIn />" & vbNewLine & "<EnableAction Enabled='True' />" & vbNewLine & "</Sequence>"
        '' ''aa = Me.AnimExDownload
        ''aa.EnableClientState = True
        ''Me.Controls.Add(aa)

    End Sub

    Protected Function ReadDatas(ByVal outCh() As Integer, ByVal loopIndex As Integer, ByRef SensorInf As CommInf, ByRef csvData() As Array) As Integer
        ''
        '' データファイルおよびSQLのクエリを用いてデータベースにバインドする
        ''
        Dim DbCon As New OleDb.OleDbConnection
        Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet
        'Dim DtSet As DataSet
        Dim DRow As DataRow
        Dim MaxCh As Integer
        Dim clsDataInf As New ClsReadDataFile
        Dim ChNum As String = ""
        Dim intTemp As Integer
        Dim ChInf() As Integer
        Dim endDate As Date
        Dim strTemp As String

        Dim strSQL As String
        Dim iloop As Integer
        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ
        Dim DataFile As String
        Dim intRet As Integer
        Dim dataCh(255) As Integer
        Dim startDate As Date
        Dim TimeCond As String = ""
        Dim SelItem As ListItem
        Dim strOption As String = ""
        Dim intTOP As Integer = 0
        Dim intInterval As Integer = 1
        Dim strRangeType As String = ""
        Dim clsNewDate As New ClsReadDataFile
        Dim AccDataFile As String
        Dim dView As DataView                                                                                       ' 2011/11/10 Kino Add

        DataFile = Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb")
        intRet = clsNewDate.GetDataFileNames(DataFile, mDataFileNames)                                              'データファイル名、共通情報ファイル名、識別名を取得
        intRet = clsNewDate.GetDataLastUpdate(Server.MapPath(siteDirectory & "\App_Data\"), mDataFileNames)         'データファイルにおける最新・最古データ日時を取得

        '=========================●共通情報からチャンネルの単位と計器記号を取得する===========================
        AccDataFile = Server.MapPath(siteDirectory & "\App_Data\" & mDataFileNames(mcsvDownloadInf(loopIndex).dataFileNo).CommonInf)
        MaxCh = clsDataInf.GetMaxChNo(AccDataFile)                                              '共通情報から最大チャンネルを取得する

        If MaxCh < outCh(outCh.Length - 1) Then
            Return 9999
            Exit Function
        End If

        For iloop = 0 To outCh.Length - 1                                                       '文字列配列に格納しなおす
            ''ChInf(iloop) = outCh(iloop).ToString
            ChNum &= outCh(iloop).ToString & ","
        Next
        ''ChNum = String.Join(",", ChInf)                                                         '配列の内容をカンマ区切りで結合
        ChNum = ChNum.Substring(0, ChNum.Length - 1)

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & AccDataFile & ";" & "Jet OLEDB:Engine Type= 5")
        ''DbCon.Open()

        DtSet = New DataSet("ComData")
        DbDa = New OleDb.OleDbDataAdapter("SELECT DataBaseCh,計器記号,出力単位 FROM 共通情報 WHERE DataBaseCh IN(" & ChNum & ") ORDER BY DataBaseCh ASC;", DbCon)
        DbDa.Fill(DtSet, "ComData")

        intTemp = DtSet.Tables("ComData").Rows.Count - 1                                        'レコードの行数
        ReDim ChInf(intTemp)

        For iloop = 0 To intTemp
            dView = New DataView(DtSet.Tables("ComData"), "DataBaseCh=" & outCh(iloop).ToString, "", DataViewRowState.CurrentRows)       ' 2011/11/10 Kino Add データセットの中から検索
            If dView.Count > 0 Then
                ChInf(iloop) = Convert.ToInt32(dView.Item(0).Item(0))
                SensorInf.ChNo &= Trim(ChInf(iloop).ToString) & ","
                SensorInf.SensorSymbol &= Trim(dView.Item(0).Item(1).ToString) & ","
                SensorInf.Unit &= Trim(dView.Item(0).Item(2).ToString) & ","
            End If

            ''ChInf(iloop) = Convert.ToInt32(DtSet.Tables("ComData").Rows(iloop).Item(0))
            ''SensorInf.ChNo &= Trim(ChInf(iloop).ToString) & ","
            ''SensorInf.SensorSymbol &= Trim(DtSet.Tables("ComData").Rows(iloop).Item(1).ToString) & ","
            ''SensorInf.Unit &= Trim(DtSet.Tables("ComData").Rows(iloop).Item(2).ToString) & ","
        Next

        DtSet = Nothing

        ''DbCon.Close()
        DbCon.Dispose()
        DbDa.Dispose()
        DbCom.Dispose()
        DbDa = Nothing

        '' 最後の余分なカンマを除去(ヘッダ部に当たる部分)
        SensorInf.ChNo = SensorInf.ChNo.Substring(0, SensorInf.ChNo.Length - 1)
        SensorInf.SensorSymbol = SensorInf.SensorSymbol.Substring(0, SensorInf.SensorSymbol.Length - 1)
        SensorInf.Unit = SensorInf.Unit.Substring(0, SensorInf.Unit.Length - 1)

        '======================================================================================================

        '=========================●データファイルから指定期間の指定チャンネルデータを取得するSQLを作成===========================
        AccDataFile = Server.MapPath(siteDirectory & "\App_Data\" & mDataFileNames(mcsvDownloadInf(loopIndex).dataFileNo).FileName)

        endDate = clsDataInf.CalcEndDate(Me.RdBFromNewest.Checked, mDataFileNames(mcsvDownloadInf(loopIndex).dataFileNo).NewestDate, Me.TxtEndDate.Text, 1)
        strTemp = DDLRange.SelectedValue

        intInterval = Integer.Parse(strTemp.Substring(0, 2))
        strRangeType = strTemp.Substring(2, 1)

        If Me.RdBFromNewest.Checked = True Then                                 '最新データから
            Select Case strRangeType
                Case "A"
                    startDate = mDataFileNames(mcsvDownloadInf(loopIndex).dataFileNo).OldestDate
                Case Else                                                       '指定日数、月、年
                    startDate = clsDataInf.CalcStartDate(Me.TxtStartDate.Text, endDate, strRangeType, intInterval, 1)
            End Select

        Else                                                                    '指定期間

            startDate = Date.Parse(Me.TxtStartDate.Text)

        End If
        '--------------------------

        '---間引き---
        If Me.ChbPartial.Checked = True Then
            For Each SelItem In Me.CBLPartial.Items     'LstBPartial.Items
                If SelItem.Selected = True Then
                    TimeCond &= Integer.Parse((SelItem.Text).Replace(":00", "")) & ","
                End If
            Next
            If TimeCond.Length > 1 Then
                TimeCond = TimeCond.Substring(0, TimeCond.Length - 1)
            End If
        End If

        strSQL = clsNewDate.GetSQLString(Convert.ToInt16(ChInf.Length - 1), ChInf, startDate, endDate, TimeCond, , , -1)
        '=========================================================================================================================

        '=========================●データを取得する===========================
        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & AccDataFile & ";" & "Jet OLEDB:Engine Type= 5")
        ''DbCon.Open()

        DtSet = New DataSet("TextData")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
        DbDa.Fill(DtSet, "TextData")

        Dim DsetCount As Integer = DtSet.Tables("TextData").Rows.Count - 1
        ReDim csvData(DsetCount)
        For iloop = 0 To DsetCount
            DRow = DtSet.Tables("TextData").Rows.Item(iloop)
            csvData(iloop) = DRow.ItemArray                                             'Array型の変数へデータを格納
        Next iloop
        '======================================================================

        Return 0

    End Function

    ''Protected Sub GetOutputChannel(ByRef outCh() As Integer, ByVal loopIndex As Integer)      'クラスへ移行した
    ''    ''
    ''    '' 設定情報から出力チャンネルを配列に格納する
    ''    ''
    ''    Dim jloop As Integer
    ''    Dim kloop As Integer
    ''    Dim strTemp() As String = {}
    ''    Dim strTemp2() As String = {}
    ''    Dim st As Integer = 0
    ''    Dim ed As Integer = 0
    ''    Dim dummyCount As Integer = 0
    ''    Dim OutChTemp() As String
    ''    Dim strOutCh As String = ""

    ''    '出力チャンネルチェック
    ''    strTemp = mcsvDownloadInf(loopIndex).chData.Split(",")

    ''    If strTemp.Length = 1 Then                                      '●カンマがなかった場合

    ''        strTemp2 = strTemp(0).Split("-")
    ''        If strTemp2.Length = 1 Then                                 '   ■- がなかった場合
    ''            ReDim outCh(0)
    ''            outCh(0) = strTemp2(0)                                  '       1chのみ
    ''        Else                                                        '   ■- があった場合
    ''            st = Integer.Parse(strTemp2(0))
    ''            ed = Integer.Parse(strTemp2(1))
    ''            ReDim outCh((ed - st))
    ''            For jloop = st To ed                                    '   30-40　なら　30～40 を配列に入れる
    ''                outCh(dummyCount) = jloop
    ''                dummyCount += 1
    ''            Next
    ''        End If

    ''    Else                                                            '●カンマで区切られていた場合

    ''        For jloop = 0 To strTemp.Length - 1

    ''            strTemp2 = strTemp(jloop).Split("-")
    ''            If strTemp2.Length = 1 Then                             '   ■- がなかった場合
    ''                strOutCh &= strTemp2(0) & ","

    ''            Else                                                    '   ■- があった場合
    ''                st = Integer.Parse(strTemp2(0))
    ''                ed = Integer.Parse(strTemp2(1))

    ''                For kloop = st To ed                                '   30-40　なら　30～40 を配列に入れる
    ''                    strOutCh &= kloop & ","
    ''                Next
    ''            End If

    ''        Next
    ''        If strOutCh.Length <> 0 Then
    ''            strOutCh = strOutCh.Substring(0, strOutCh.Length - 1)
    ''        End If
    ''        OutChTemp = strOutCh.Split(",")                             '文字列を配列に変換
    ''        ReDim outCh(OutChTemp.Length - 1)
    ''        For jloop = 0 To OutChTemp.Length - 1                       'stringの配列をintegerの配列に入れなおす
    ''            outCh(jloop) = Integer.Parse(OutChTemp(jloop))
    ''        Next
    ''        Array.Sort(outCh)                                           '配列をソート
    ''    End If

    ''End Sub

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
