Imports System.Data
Imports System.Drawing

Partial Class Table_TimeSeries
    Inherits System.Web.UI.Page

    '表における個々の列情報
    Private Structure TblColumnInfo
        Dim DataCH As Short
        Dim SensorName As String
        Dim SensorSymbol As String
        Dim SensorUnit As String
        Dim ShowFormat As String
        Dim replaceInf() As String
    End Structure

    'グラフ描画全般の情報
    Private Structure TblInfo

        ''Dim StartDate As Date                   '表の描画開始日
        ''Dim EndDate As Date                     '表の描画終了日
        Dim PaperOrientaion As Integer          '用紙の向き
        Dim DateFormat As String                '日付フォーマット
        Dim TitlePosition As Short              '表タイトルの位置(上or下)
        Dim TableTitle As String                '表タイトル
        Dim TableSubTitle As String             '表サブタイトル
        Dim WindowTitle As String               'タイトルバーのタイトル
        Dim RowCount As Integer                 '表の行数
        Dim ColCount As Integer                 '表の列数
        Dim LastUpdate As Date                  'データの最終更新日
        Dim DayRange As Integer                 '表示期間
        Dim ThinInf As String                   '表示データの間引き情報
        Dim ShowAvg As Boolean                  '平均値を表示するかどうか
        Dim DateFieldWidth As Integer           '日付列幅
        Dim DataFieldWidth As Integer           'データ列幅
        Dim TitleFont As Integer                'タイトルのフォントプロパティ
        Dim SubTitleFont As Integer             'タイトルのフォントプロパティ
        Dim DataFont As Integer                 'データのフォントプロパティ
        Dim Sensor() As TblColumnInfo           '表に記述する内容の情報
        Dim DataFileNo As Integer               'データファイル番号
        Dim thinoutEnable As Boolean            '間引きチェックの有無
        Dim TruncFlg As Boolean                 '表示桁以下の切捨ての有無
        Dim SensorSymbolByCommonInf As String   '計器記号を共通情報が取得するためのチャンネル番号をカンマで格納 2010/02/23 Kino Add
    End Structure

    Private TblProp As TblInfo                  '表描画に関する情報
    Private MaxMinData(,) As Single             '列ごとの最大、最小、平均値を格納
    Private DataFileNames() As ClsReadDataFile.DataFileInf
    Private DateLimitTerm As String             '過去データ表示期間の期限
    Private clsDataOpe As New ClsReadDataFile   'データ読込み関連
    Private abnormalValue As Single = 1.1E+20   '異常値判定値

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        ''
        '' ページを閉じるときにデータオブジェクトとグリッドをDisposeする
        ''
        Me.AccessDataSource1.Dispose()
        Me.GrdTimeSeries.Dispose()
        ''Me.ViewState.Clear()

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

        If User.Identity.IsAuthenticated = False Then
            'Response.Redirect("Login.aspx",false)
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer

        LoginStatus = Session.Item("LgSt")          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            Response.Redirect("sessionerror.aspx", False)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする
        Response.Cache.SetNoStore()
        Response.Cache.SetExpires(DateTime.Now.AddDays(-1))

        Dim siteName As String
        Dim siteDirectory As String
        Dim tblDataFile As String
        Dim intRet As Integer
        'Dim clsNewDate As New ClsReadDataFile
        Dim clsSetScript As New ClsGraphCommon
        Dim OldTerm As String
        Dim getFromDB As Short                                      '2010/02/23 Kino Add
        Dim AlertInfo(6) As ClsReadDataFile.AlertInf                ' 2016/10/06 Kino Add

        OldTerm = CType(Session.Item("OldTerm"), String)            '過去データ表示制限
        siteName = CType(Session.Item("SN"), String)                '現場名
        siteDirectory = CType(Session.Item("SD"), String)           '現場ディレクトリ
        tblDataFile = Server.MapPath(siteDirectory & "\App_Data\TimeSeriesTableInfo.mdb")
        TblProp.WindowTitle = CType(Request.Item("GNa"), String)        'CType(Session.Item("OutputName"), String)         '表タイトル（ウィンドウタイトル）
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)
        Dim OldDate As Date

        ''TblProp.LastUpdate = CType(Session.Item("LastUpdate"), String)          '最新データ
        ''Dim AccDataFile As String = Server.MapPath(siteDirectory & "\App_Data\" & siteDirectory & "_CalculatedData.mdb")

        intRet = clsDataOpe.GetDataFileNames(Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb"), DataFileNames)       'データファイル名、共通情報ファイル名、識別名を取得
        intRet = clsDataOpe.GetDataLastUpdate(Server.MapPath(siteDirectory + "\App_Data\"), DataFileNames)                  '2010/05/24 Kino Add データの最古、最新日付を取得
        intRet = clsDataOpe.GetDataFileNames(IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", "MenuInfo.mdb"), DataFileNames)                        'データファイル名、共通情報ファイル名、識別名を取得
        ''intRet = clsDataOpe.GetDataLastUpdate(Path.Combine(Server.MapPath(siteDirectory), "App_Data", "MenuInfo.mdb"), DataFileNames)                  '2010/05/24 Kino Add データの最古、最新日付を取得        ' 2018/06/05 Kino Changed 上で同じことをやっているためコメント

        Dim OldFlg As Boolean = IO.File.Exists(Server.MapPath(siteDirectory + "App_Data\Old.flg"))                          ' 2011/04/19 Kino Add 前の設定と調整するため

        If IsPostBack = False Then      'ポストバックではないときにはデータベースから設定を読み込む

            ''If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
            ''    ''==== フォームのサイズを調整する ====
            ''    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
            ''        Dim OpenString As String

            ''        OpenString = "<SCRIPT LANGUAGE='javascript'>"
            ''        OpenString &= "window.resizeTo(" & wid & "," & Hei & ");"
            ''        OpenString &= "<" & "/SCRIPT>"

            ''        'Dim instance As ClientScriptManager = Page.ClientScript                            'この方法から↓の方法へ変更した
            ''        'instance.RegisterClientScriptBlock(Me.GetType(), "経時変化図", OpenString)
            ''        Page.ClientScript.RegisterStartupScript(Me.GetType(), "経時表", OpenString)
            ''    End If
            ''End If

            '初回読み込みはデータベースから行なう
            Dim DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & tblDataFile & ";" & "Jet OLEDB:Engine Type= 5"
            ''DbCon.Open()

            intRet = ReadTableInfoFromDB(DbCon, "経時表")                   '作表情報を取得
            intRet = TableColumnInfoFromDB(DbCon, getFromDB)                '作表の表示内容を取得
            If getFromDB = 1 Then                                           '2010/02/23 Kino Add
                Call getSensorSymbolFromCommon(siteDirectory)
            End If
            Call GetMostUpdate()                                            '最新データの日付を取得
            TblProp.LastUpdate = CType(Session.Item("LastUpdate" & TblProp.DataFileNo.ToString), String)          '最新データ

            Call SetBIND2Controls(DbCon)                                    '各コントロールのリスト内容をDBから作成する

            ''DbCon.Close()
            DbCon.Dispose()

            Call Set2FormControl()                                          ''変数の内容を各コントロールに配置する
            Call CalcDate(DDLRange.SelectedValue)                           ''開始日付の計算

            Call SetViewState()

            Call clsSetScript.CheckAutoUpdate(Me.Form, Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb"), "経時表")      '自動更新設定の読み込み
            Call clsSetScript.SetSelectDateScript(Me.Form)                   'コントロールにJavaScriptを割り当て

            ''Me.RngValidSt.MinimumValue = OldDate
            If OldTerm <> "None" Then
                OldDate = clsDataOpe.CalcOldDateLimit(OldTerm)
            Else
                OldDate = DataFileNames(0).MostOldestDate
            End If
            Me.RngValidSt.MinimumValue = OldDate.ToString("yyyy/MM/dd")
            Me.RngValidSt.MaximumValue = Date.Parse(TblProp.LastUpdate.ToString("yyyy/MM/dd"))
            Me.RngValidSt.ErrorMessage = "開始日は" + OldDate.ToString("yyyy/MM/dd") + "以降としてください。"
            Me.RngValidEd.MinimumValue = OldDate.ToString("yyyy/MM/dd")
            Me.RngValidEd.MaximumValue = Date.Parse(TblProp.LastUpdate.ToString("yyyy/MM/dd"))
            Me.RngValidEd.ErrorMessage = "終了日は" + TblProp.LastUpdate.ToString("yyyy/MM/dd") + "以前としてください。"
            Me.C1WebCalendar.MinDate = OldDate
            Me.C1WebCalendar.MaxDate = TblProp.LastUpdate

            ' ''管理値の配色をMenuInfo.mdbから取得して、変数に格納する 2016/10/06 Kino Add-------------- とりあえずコメント
            ''Dim DataFile As String = Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb")
            ''Dim TempCount As Integer = 0
            ''Dim clsSummary As New ClsReadDataFile
            ''Dim iloop As Integer
            ''Dim strCount As String
            ''TempCount = clsSummary.GetAlertDisplayInf(DataFile, AlertInfo)

            ''ViewState("AlertSetCount") = TempCount - 1
            ''For iloop = 0 To TempCount - 1
            ''    strCount = iloop.ToString
            ''    ViewState("AlertLevels" + strCount) = AlertInfo(iloop).Levels
            ''    ''ViewState("AlertWords" + strCount) = AlertInfo(iloop).Words
            ''    ViewState("BackColors" + strCount) = AlertInfo(iloop).BackColor
            ''    ''ViewState("ForeColors" + strCount) = AlertInfo(iloop).ForeColor
            ''Next iloop
            ' ''----------------------------------------------------------------------------------------

            Me.nwdt.Value = TblProp.LastUpdate.ToString                                                                 ' 2018/03/16 Kino Add postbackでない時だけ書き込む。あとはJSから。
            Me.nwdtno.Value = TblProp.DataFileNo.ToString
            If siteName.Contains("【完了現場】") = True Then
                Me.nwdt.Value = "NC"                                                                                    ' 完了現場は、更新チェックしない
            End If

        Else                            'ポストバックの時はビューステートから設定を読み込む
            Call ReadViewState()
            Call GetMostUpdate()                                            '最新データの日付を取得
            TblProp.LastUpdate = CType(Session.Item("LastUpdate" & TblProp.DataFileNo.ToString), String)          '最新データ
            ''Call GetMostUpdate()
            If Me.RdBFromNewest.Checked = True Then Call CalcDate(Me.DDLRange.SelectedValue)

        End If

        Dim checkStDate As Date = Date.Parse(Me.TxtStartDate.Text)
        If checkStDate < DataFileNames(TblProp.DataFileNo.ToString).OldestDate Then
            Me.TxtStartDate.Text = DataFileNames(TblProp.DataFileNo.ToString).OldestDate.ToString("yyyy/MM/dd")
        End If


        ''If Date.Parse(OldDate) <= DataFileNames(0).MostOldestDate Then
        ''    Me.C1WebCalendar1.MinDate = Date.Parse(DataFileNames(0).MostOldestDate.ToString("yyyy/MM/dd"))
        ''    Me.RngValidSt.MinimumValue = Date.Parse(DataFileNames(0).MostOldestDate.ToString("yyyy/MM/dd"))
        ''Else
        ''    Me.C1WebCalendar1.MinDate = OldDate
        ''    Me.RngValidSt.MinimumValue = OldDate
        ''End If
        ''Me.C1WebCalendar1.MaxDate = Date.Parse(TblProp.LastUpdate)

        '表のタイトル表示、縦横設定
        Call SetTableTitle(OldFlg)

        Me.MyTitle.Text = TblProp.TableTitle & " [経時表] - " & siteName                    'ブラウザのタイトルバーに表示するタイトル
        Me.LblLastUpdate.Text = "最新データ日時：" & TblProp.LastUpdate.ToString("yyyy/MM/dd HH:mm")

        ' 管理値の配色をmenuinf.mdb から取得して、Literalに吐き出す。RowDataBoundのデータ描画時に、管理値と比較して、下のスタイルシートタグを追加すると、管理値超過の位置に背景色が付く　2016/10/06 Kino
        '--------------------------------------------- ベースはできた。 ↑の警報比較と管理値読込を作れば完了する
        Dim sb As New StringBuilder
        Dim colname As String = Nothing
        sb.Append("<style type='text/css'>")
        colname = clsDataOpe.getColSeparate(AlertInfo(2).BackColor)
        sb.Append(String.Format(".al1 {{background-color:{0}}}", colname))
        sb.Append(Environment.NewLine)
        colname = clsDataOpe.getColSeparate(AlertInfo(1).BackColor)
        sb.Append(String.Format(".al2 {{background-color:{0}}}", colname))
        sb.Append(Environment.NewLine)
        colname = clsDataOpe.getColSeparate(AlertInfo(0).BackColor)
        sb.Append(String.Format(".al3 {{background-color:{0}}}", colname))
        sb.Append("</style>")
        Me.dcss.Text = sb.ToString
        sb.Length = 0

        Call Bind2DB()

        clsSetScript = Nothing
        ''clsNewDate = Nothing

    End Sub

    Private Sub getSensorSymbolFromCommon(ByVal siteDirectory As String)
        ''
        '' 計器記号を共通情報から取得する
        ''
        Dim DBFilePath As String
        Dim DbCon As New OleDb.OleDbConnection
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As DataSet
        Dim work As String
        Dim strTemp() As String
        Dim iloop As Integer
        Dim GetIndexDataRow() As DataRow

        DBFilePath = Server.MapPath(siteDirectory & "\App_Data\" & DataFileNames(TblProp.DataFileNo).CommonInf)
        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DBFilePath & ";" & "Jet OLEDB:Engine Type= 5")
        ''DbCon.Open()
        work = "SELECT * FROM 共通情報 WHERE DataBaseCh IN(" & TblProp.SensorSymbolByCommonInf & ") ORDER BY DataBaseCh "
        DtSet = New DataSet("ComInf")
        DbDa = New OleDb.OleDbDataAdapter(work, DbCon)
        DbDa.Fill(DtSet, "ComInf")
        ''DbCon.Close()

        strTemp = TblProp.SensorSymbolByCommonInf.Split(",")
        For iloop = 1 To TblProp.ColCount
            If TblProp.Sensor(iloop - 1).SensorSymbol = "@@CH" Then
                GetIndexDataRow = DtSet.Tables("ComInf").Select("DataBaseCh = " & TblProp.Sensor(iloop - 1).DataCH.ToString)
                If GetIndexDataRow.Length <> 0 Then
                    TblProp.Sensor(iloop - 1).SensorSymbol = GetIndexDataRow(0).Item(3)
                End If
            End If
        Next

        DbCon.Dispose()

    End Sub


    Public Sub SetBIND2Controls(ByVal dbCon As OleDb.OleDbConnection)
        ''
        ''ドロップダウンリストボックスにおける各設定のデータセットバインド
        ''
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

        '' デフォルト値を探す 2012/11/08 Kino Add
        For Each DTR As DataRow In DtSet.Tables("DData").Rows
            With DTR.ItemArray
                If Boolean.Parse(.GetValue(4)) = True Then
                    Me.DDLRange.SelectedIndex = Integer.Parse(.GetValue(0))
                    Exit For
                End If
            End With
        Next

        DbDa.Dispose()
        DtSet.Dispose()

    End Sub


    Protected Sub ReadViewState()
        ''
        '' ビューステートから取得した内容を変数へ格納する　　　ビューステート　→　変数
        ''
        Dim iloop As Integer
        Dim strCnt As String

        TblProp.RowCount = CType(ViewState("RowCount"), Integer)                '行数                           Integer
        TblProp.ColCount = CType(ViewState("ColCount"), Integer)                '列数                           Integer
        TblProp.TableTitle = CType(ViewState("TblTtl"), String)                 '表のタイトル                   String
        TblProp.TableSubTitle = CType(ViewState("TblSubTtl"), String)           '表のサブタイトル               String
        TblProp.TitlePosition = CType(ViewState("TtlPos"), Short)               'タイトルの位置(上or下)         Short
        ''TblProp.DayRange = CType(ViewState("DayRange"), Integer)                '表示期間                       Integer
        TblProp.DateFormat = CType(ViewState("DteFormat"), String)              '日付の表示フォーマット         String
        TblProp.PaperOrientaion = CType(ViewState("PaOrient"), Integer)         '紙の向き                       Integer
        TblProp.TitleFont = CType(ViewState("TtlFont"), Integer)                'タイトルフォントのサイズ       Integer
        TblProp.SubTitleFont = CType(ViewState("SubTtlFont"), Integer)          'サブタイトルフォントのサイズ   Integer
        TblProp.DataFont = CType(ViewState("DatFont"), Integer)                 'データのフォントのサイズ       Integer
        ''TblProp.ThinInf = CType(ViewState("ThinInf"), String)                   '間引き情報                     String
        TblProp.ShowAvg = CType(ViewState("ShowAve"), Boolean)                  '平均値表示                     Boolean
        TblProp.DateFieldWidth = CType(ViewState("DateFieldWidth"), Integer)    '日付列幅                       Long
        TblProp.DataFieldWidth = CType(ViewState("DatFieldWidth"), Integer)     'データ列幅                     Integer
        TblProp.DataFileNo = CType(ViewState("DatFileNo"), Integer)             'データファイル番号             Integer
        TblProp.TruncFlg = CType(ViewState("TruncFlg"), Boolean)                '表示桁で切り捨てるか           Boolean

        ReDim TblProp.Sensor(TblProp.ColCount - 1)                  '個々の列の情報を格納する配列設定

        For iloop = 0 To TblProp.ColCount - 1
            strCnt = iloop.ToString
            TblProp.Sensor(iloop).DataCH = CType(ViewState("DataCh" & strCnt), Short)           '表示チャンネル Short
            TblProp.Sensor(iloop).SensorName = CType(ViewState("SenName" & strCnt), String)     'センサ名       String
            TblProp.Sensor(iloop).SensorSymbol = CType(ViewState("SenSym" & strCnt), String)    'センサ記号     String
            TblProp.Sensor(iloop).SensorUnit = CType(ViewState("SenUnit" & strCnt), String)     '単位           String
            TblProp.Sensor(iloop).ShowFormat = CType(ViewState("ShowFmt" & strCnt), String)     '表示形式       String

            Dim strTemp As String = CType(ViewState("RepInf" & strCnt), String)                 '置換情報 
            If strTemp IsNot Nothing Then
                Dim allRep() As String = Nothing
                Dim jloop As Integer

                allRep = strTemp.Split(",")
                ReDim TblProp.Sensor(iloop).replaceInf(allRep.Length - 1)
                For jloop = 0 To allRep.Length - 1
                    TblProp.Sensor(iloop).replaceInf(jloop) = allRep(jloop)
                Next
            End If

        Next iloop
        'If .Rows.Count >= 6 Then
        '    '置換情報                                                                                                   ' 2013/01/21 Kino Add 深度階対応
        '    Dim allRep() As String = Nothing
        '    Dim jloop As Integer
        '    For iloop = 1 To TblProp.ColCount
        '        allRep = .Rows(5).Item(iloop).ToString.Split(",")                       ' まずは全文を分解
        '        If .Rows(5).Item(iloop).ToString.Length <> 0 Then
        '            ReDim TblProp.Sensor(iloop - 1).replaceInf(allRep.Length - 1)
        '            For jloop = 0 To allRep.Length - 1
        '                TblProp.Sensor(iloop - 1).replaceInf(jloop) = allRep(jloop)
        '            Next

        '            'TblProp.Sensor(iloop - 1).replaceInf = .Rows(5).Item(iloop).ToString
        '        Else
        '            TblProp.Sensor(iloop - 1).replaceInf = Nothing
        '        End If
        '    Next
        'End If
    End Sub

    Protected Sub CalcDate(ByVal setDateProperty As String)
        ''
        '' 選択されたラジオボタンから日付を検索
        ''

        Dim Interval As Double
        Dim DateType As String
        Dim endDate As Date
        Dim startDate As Date
        Dim SpanSet As Integer
        ''Dim clsCalc As New ClsReadDataFile


        If setDateProperty <> "99X" Then                                                                        '「任意」以外の場合
            Me.TxtEndDate.Text = TblProp.LastUpdate.ToString("yyyy/MM/dd")
            ''Me.TxtStartDate.BackColor = Drawing.Color.LightGray
            ''Me.TxtEndDate.BackColor = Drawing.Color.LightGray
            DateType = setDateProperty.Substring(2, 1)

            If DateType = "H" Then
                SpanSet = 2
            Else
                SpanSet = 1
            End If

            endDate = clsDataOpe.CalcEndDate(Me.RdBFromNewest.Checked, TblProp.LastUpdate, Me.TxtEndDate.Text, SpanSet)

            Interval = Integer.Parse(setDateProperty.Substring(0, 2))

            Select Case DateType
                Case "A"
                    ''startDate = CType(Session.Item("OldestDate"), String)                                     '2009/02/02 Kino Changed
                    startDate = CType(Session.Item("OldestDate" & TblProp.DataFileNo.ToString), Date)           '2009/02/02 Kino Add
                Case Else                                                       '指定日数、月、年
                    startDate = clsDataOpe.CalcStartDate(Me.TxtStartDate.Text, endDate, DateType, Interval, SpanSet)

            End Select

            Me.TxtStartDate.Text = startDate.ToString("d")

        End If

        ''clsCalc = Nothing

    End Sub

    Protected Sub SetViewState()
        ''
        '' 変数から取得した内容をビューステートに格納する     変数　→　ビューステート
        ''
        Dim iloop As Integer
        Dim strCnt As String

        ViewState("RowCount") = TblProp.RowCount                '行数                           Integer
        ViewState("ColCount") = TblProp.ColCount                '列数                           Integer
        ViewState("TblTtl") = TblProp.TableTitle                '表のタイトル                   String
        ViewState("TblSubTtl") = TblProp.TableSubTitle          '表のサブタイトル               String
        ViewState("TtlPos") = TblProp.TitlePosition             'タイトルの位置(上or下)         Short
        ''ViewState("DayRange") = TblProp.DayRange                '表示期間                       Integer
        ViewState("DteFormat") = TblProp.DateFormat             '日付の表示フォーマット         String
        ViewState("PaOrient") = TblProp.PaperOrientaion         '紙の向き                       Integer
        ViewState("TtlFont") = TblProp.TitleFont                'タイトルフォントのサイズ       Integer
        ViewState("SubTtlFont") = TblProp.SubTitleFont          'サブタイトルフォントのサイズ   Integer
        ViewState("DatFont") = TblProp.DataFont                 'データのフォントのサイズ       Integer
        ''ViewState("ThinInf") = TblProp.ThinInf                  '間引き情報                     String
        ViewState("ShowAve") = TblProp.ShowAvg                  '平均値表示                     Boolean
        ViewState("DateFieldWidth") = TblProp.DateFieldWidth    '日付列幅                       Integer
        ViewState("DatFieldWidth") = TblProp.DataFieldWidth     'データ列幅                     Integer
        ViewState("DatFileNo") = TblProp.DataFileNo             'データファイル番号             Integer
        ViewState("TruncFlg") = TblProp.TruncFlg                '表示桁で切り捨てるか           Boolean

        For iloop = 0 To TblProp.ColCount - 1
            strCnt = iloop.ToString
            ViewState("DataCh" & strCnt) = TblProp.Sensor(iloop).DataCH         '表示チャンネル Short
            ViewState("SenName" & strCnt) = TblProp.Sensor(iloop).SensorName    'センサ名       String
            ViewState("SenSym" & strCnt) = TblProp.Sensor(iloop).SensorSymbol   'センサ記号     String
            ViewState("SenUnit" & strCnt) = TblProp.Sensor(iloop).SensorUnit    '単位           String
            ViewState("ShowFmt" & strCnt) = TblProp.Sensor(iloop).ShowFormat    '表示形式       String
            Dim strTemp As String = Nothing                                                     ' 2013/1/29 Kino Add 置換文字列対応
            If TblProp.Sensor(iloop).replaceInf IsNot Nothing Then
                Dim jloop As Integer
                For jloop = 0 To TblProp.Sensor(iloop).replaceInf.Length - 1
                    strTemp &= (TblProp.Sensor(iloop).replaceInf(jloop).ToString & ",")
                Next
                strTemp = strTemp.Substring(0, strTemp.Length - 1)
            Else
                strTemp = Nothing
            End If
            ViewState("RepInf" & strCnt) = strTemp                              '置換文字列     String 
        Next iloop

    End Sub

    Protected Sub SetTableTitle(ByVal OldFlg As Boolean)

        Dim TblHeight As Integer
        Dim TblWidth As Integer

        '================================================ [ 表サイズ・表示設定 ] ================================================↓
        '
        If TblProp.RowCount = 0 Then                                    '--行／ページの設定--
            Select Case TblProp.PaperOrientaion
                Case 0      '用紙：縦
                    TblProp.RowCount = 36
                Case 1      '用紙：横
                    TblProp.RowCount = 24
            End Select
        End If
        Me.GrdTimeSeries.PageSize = TblProp.RowCount

        With Me.GrdTimeSeries                                           'グラフサイズの調整
            Select Case TblProp.PaperOrientaion
                Case 0      'A4縦の場合
                    TblHeight = 912
                    TblWidth = 649
                Case 1      'A4横の場合
                    TblHeight = 590
                    TblWidth = 960
                Case 2      'A3縦の場合
                Case 3      'A3横の場合
            End Select

            .Height = TblHeight                                         'GridViewの高さ
            .Width = TblWidth                                           'GridViewの幅
        End With
        '================================================ [ 表サイズ・表示設定 ] ================================================↑

        '---グラフタイトル 表示位置、フォントサイズ設定---
        Dim objLabel As System.Web.UI.WebControls.Label
        If TblProp.TitlePosition = 0 Then           '上
            ''Me.LitUpper.Text = "<br /><img src='./img/space_S.gif'><br />"
            ''Me.LitLower.Text = ""
            objLabel = Me.LblTitleUpper
            Me.GrdTimeSeries.CssClass = "TitleMarginTop"
            ''Me.LblTitleUpper.Visible = True
            ''Me.LblTitleLower.Visible = False
        Else                                        '下
            ''Me.LitUpper.Text = ""
            ''Me.LitLower.Text = "<br /><img src='./img/space_S.gif'>"
            objLabel = Me.LblTitleLower
            Me.GrdTimeSeries.CssClass = "TitleMarginBottom"
            ''Me.LblTitleUpper.Visible = False
            ''Me.LblTitleLower.Visible = True
        End If

        Dim AdjValue As Integer
        If OldFlg = True Then AdjValue = -3 ' 2011/04/19 Kino Add      現行の設定と合わせるため
        objLabel.Text = TblProp.TableTitle
        objLabel.Font.Size = TblProp.TitleFont + AdjValue          ' 2011/04/19 Kino Add  　現行の設定と合わせるため
        objLabel.Width = TblWidth
        Me.LblDateSpan.Width = TblWidth

    End Sub

    Protected Sub Bind2DB()
        ''
        '' データファイルおよびSQLのクエリを用いてデータベースにバインドする
        ''
        Dim strSQL As String
        Dim iloop As Integer
        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ
        Dim DataFile As String
        Dim intRet As Integer
        Dim dataCh(TblProp.ColCount - 1) As Integer
        Dim startDate As Date
        Dim endDate As Date
        Dim strTemp As String
        Dim TimeCond As String = ""
        Dim SelItem As ListItem
        Dim strOption As String = ""
        Dim intTOP As Integer = 0
        Dim intInterval As Single = 1
        Dim strRangeType As String = ""
        Dim spanSet As Integer
        ''Dim clsNewDate As New ClsReadDataFile

        DataFile = Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb")
        intRet = clsDataOpe.GetDataFileNames(DataFile, DataFileNames)                           'データファイル名、共通情報ファイル名、識別名を取得

        Dim AccDataFile As String = Server.MapPath(siteDirectory & "\App_Data\" & DataFileNames(TblProp.DataFileNo).FileName) ''siteDirectory & "_CalculatedData.mdb")

        ''Dim PreSQL As String

        '---SQLにおいてフィールド名をセンサ記号として設定するための変数格納---
        For iloop = 0 To TblProp.ColCount - 1
            dataCh(iloop) = TblProp.Sensor(iloop).DataCH
            strOption &= "[" & TblProp.Sensor(iloop).SensorSymbol & "],"            ' & "(" & TblProp.Sensor(iloop).SensorUnit & ")],"
        Next iloop
        strOption = strOption.Substring(0, strOption.Length - 1)

        '---データの表示期間設定---
        ''If Me.RdBFromNewest.Checked = True Then
        ''    '最新から
        ''    endDate = TblProp.LastUpdate
        ''    'endDate = Date.Parse(endDate).AddDays(1).ToString("yyyy/MM/dd 0:00")
        ''Else
        ''    '指定期間
        ''    startDate = Date.Parse(Me.TxtStartDate.Text)
        ''    endDate = Date.Parse(Me.TxtEndDate.Text)

        ''End If
        ''endDate = Date.Parse(endDate.ToString("yyyy/MM/dd 23:59:59"))

        strTemp = DDLRange.SelectedValue
        intInterval = Integer.Parse(strTemp.Substring(0, 2))
        strRangeType = strTemp.Substring(2, 1)

        If strRangeType = "H" Then
            spanSet = 2
        Else
            spanSet = 1
        End If
        endDate = clsDataOpe.CalcEndDate(Me.RdBFromNewest.Checked, TblProp.LastUpdate, Me.TxtEndDate.Text, spanSet)

        If Me.RdBFromNewest.Checked = True Then                                 '最新データから
            Select Case strRangeType
                Case "A"
                    ''startDate = CType(Session.Item("OldestDate"), String)                                     '2009/02/02 Kino Changed
                    startDate = CType(Session.Item("OldestDate" & TblProp.DataFileNo.ToString), Date)           '2009/02/02 Kino Add
                Case "P"
                    startDate = endDate.AddMonths(-3)
                    intTOP = TblProp.RowCount * intInterval
                Case Else                                                       '指定日数、月、年
                    startDate = clsDataOpe.CalcStartDate(Me.TxtStartDate.Text, endDate, strRangeType, intInterval, spanSet)
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

        strSQL = clsDataOpe.GetSQLString(TblProp.ColCount - 1, dataCh, startDate, endDate, TimeCond, , strOption, intTOP)   '2013/01/28 Kino Changed Add ->strReplace 

        ''この方法はだめっぽい・・・
        ' '' ''Dim DbCom As New OleDb.OleDbCommand
        ' '' ''Dim DbDa As OleDb.OleDbDataAdapter
        ' '' ''Dim DtSet As New DataSet("DData")
        ' '' ''Dim DbCon As New OleDb.OleDbConnection
        ' '' ''DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & AccDataFile & ";" & "Jet OLEDB:Engine Type= 5"
        ' '' ''DbCon.Open()
        ' '' ''DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
        ' '' ''DbDa.Fill(DtSet, "DData")
        ' '' ''DbCon.Close()
        ' '' ''For iloop = 1 To TblProp.ColCount
        ' '' ''    If TblProp.Sensor(iloop - 1).SensorSymbol = "-" Then
        ' '' ''    End If
        ' '' ''Next iloop

        '---DataObjectSourceにDBをバインドする---
        Me.AccessDataSource1.DataFile = AccDataFile
        Me.AccessDataSource1.SelectCommand = strSQL
        Me.GrdTimeSeries.DataSourceID = Me.AccessDataSource1.ID
        Me.AccessDataSource1.DataBind()

        '---最初の日付と最後の日付を取得し、最大、最小、平均値の演算を行なう---
        ''PreSQL = CType(ViewState("CalcSQL"), String)                          'ViewStateでSQLを保持しておいて、同じなら演算しない・・はちょっと無理かも
        ''If PreSQL <> strSQL Then
        Call SetStEdDateTime(strSQL, AccDataFile)
        ''Else
        ''MaxMinData = ViewState("CalcedData")
        ''End If

        ''ViewState("CalcSQL") = strSQL
        ''ViewState("CalcedData") = MaxMinData      'これは無理だった　ViewStateが壊れるようだ

    End Sub

    Protected Sub SetStEdDateTime(ByVal strSQL As String, ByVal AccDataFile As String)
        ''
        '' 作表における最初と最後の日付を取得し、表示範囲に記載する
        ''
        Dim con As New OleDb.OleDbConnection
        Dim DbDA As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("Datass")
        Dim RangeSt As Date
        Dim RangeEd As Date
        Dim Datas() As Array
        Dim iloop As Integer
        Dim DRow As DataRow
        ''Dim clsCalcSum As New ClsReadDataFile

        con.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & AccDataFile & ";" & "Jet OLEDB:Engine Type= 5"
        ''con.Open()

        DbDA = New OleDb.OleDbDataAdapter(strSQL, con)
        DbDA.Fill(DtSet, "Datass")

        Dim DsetCount As Integer = DtSet.Tables("Datass").Rows.Count - 1
        RangeSt = DtSet.Tables("Datass").Rows(0).Item(0)
        RangeEd = DtSet.Tables("Datass").Rows(DsetCount).Item(0)

        If RangeSt > RangeEd Then
            Me.LblDateSpan.Text = "表示期間：" + RangeEd.ToString("yyyy/MM/dd HH:mm") + "～" + RangeSt.ToString("yyyy/MM/dd HH:mm")
        Else
            Me.LblDateSpan.Text = "表示期間：" + RangeSt.ToString("yyyy/MM/dd HH:mm") + "～" + RangeEd.ToString("yyyy/MM/dd HH:mm")
        End If

        If TblProp.ShowAvg = True Then              '最大、最小、平均値を出す場合

            ReDim Datas(DsetCount)
            For iloop = 0 To DsetCount
                DRow = DtSet.Tables("Datass").Rows.Item(iloop)
                Datas(iloop) = DRow.ItemArray
            Next iloop

            Dim intRet As Integer
            ReDim MaxMinData(TblProp.ColCount - 1, 4)         '0:総和 1:有効データ数 2:平均値 3:最大値 4:最小値 
            ''        For iloop = 0 To DsetCount
            intRet = clsDataOpe.GetSum(TblProp.ColCount, DsetCount, Datas, MaxMinData)
            ''        Next iloop
        Else
            Me.GrdTimeSeries.ShowFooter = False     '平均値を出さないときはフッターを表示しない
        End If

        DtSet.Dispose()
        DbDA.Dispose()
        ''con.Close()
        con.Dispose()

    End Sub

    Protected Sub Set2FormControl()
        ''
        '' データベースから変数に格納した内容を、画面のコントロールへ配置する
        ''

        '開始日の初期状態を設定
        Me.RdBFromNewest.Checked = True
        Me.TxtStartDate.Text = TblProp.LastUpdate.ToString("yyyy/MM/dd")
        Me.TxtEndDate.Text = TblProp.LastUpdate.ToString("yyyy/MM/dd")

        ''Me.ImgStDate.Visible = False                  ''Javascriptでの制御へ移行
        ''Me.ImgEdDate.Visible = False
        ''Me.TxtStartDate.ReadOnly = True
        ''Me.TxtEndDate.ReadOnly = True

        '' ''Me.DDLRange.SelectedIndex = TblProp.DayRange               プロパティのViewStateで対応

        If TblProp.thinoutEnable = True Then
            If TblProp.ThinInf <> "None" Then
                ''間引き情報作成                                                    '2009/06/09 Kino Add
                Me.ChbPartial.Checked = True
                ''Dim clsCh As New ClsReadDataFile
                Dim DatCh() As Integer = {}
                Dim iloop As Integer
                Call clsDataOpe.GetOutputChannel(DatCh, TblProp.ThinInf)
                ''clsCh = Nothing

                For iloop = 0 To DatCh.Length - 1
                    Me.CBLPartial.Items(DatCh(iloop)).Selected = True
                Next

            End If

            '' ''    Me.ChbPartial.Checked = True

            '' ''    Dim TimeSet() As String
            '' ''    Dim iloop As Integer

            '' ''    TimeSet = TblProp.ThinInf.Split(",")
            '' ''    For iloop = 0 To TimeSet.Length - 1
            '' ''        Me.CBLPartial.Items(TimeSet(iloop)).Selected = True
            '' ''    Next iloop
        End If

    End Sub

    ''Protected Function CalcDate(ByVal setDateProperty As String) As Date
    ''    ''
    ''    '' 選択されたラジオボタンから日付を検索
    ''    ''

    ''    Dim Interval As Double
    ''    Dim DateType As String
    ''    Dim dteTemp As Date
    ''    Dim EndDate As Date

    ''    If Me.RdBFromNewest.Checked = True Then                                                 '「任意」の場合

    ''        Return EndDate

    ''    Else                                                                                    '「任意」以外の場合

    ''        EndDate = DateTime.Parse(Me.TxtStartDate.Text)
    ''        DateType = setDateProperty.Substring(2, 1)
    ''        Interval = Double.Parse(setDateProperty.Substring(0, 2))

    ''        dteTemp = ClsReadDataFile.CalcDateInterval(DateType, EndDate, Interval)
    ''        Return dteTemp

    ''    End If

    ''End Function

    Protected Sub GrdTimeSeries_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles GrdTimeSeries.DataBound
        ''    '' GrdTimeSeries_RowDataBound の方が先に実行される

        ''    ''Me.GrdTimeSeries.BottomPagerRow.BorderColor = Color.Black
        ''    ''Me.GrdTimeSeries.Sort("日付", SortDirection.Ascending)
        Call SetGridFormat()                                            'グリッドの書式設定     ここにしないと、ページングしたときに最後のページで空行が挿入されない

    End Sub

    Protected Sub GrdTimeSeries_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GrdTimeSeries.PreRender
        ''
        '' グリッドビューの読み込み後、表示を開始する前に発生するイベント
        ''

        Me.GrdTimeSeries.Sort("日付", SortDirection.Ascending)

    End Sub

    Protected Sub SetGridFormat()
        ''
        '' グリッドの全般的な書式設定
        ''

        Dim iloop As Integer
        Dim intTemp As Integer
        Dim jloop As Integer

        With Me.GrdTimeSeries
            ''intTemp = (.Width.Value - TblProp.DateFieldWidth - 70) / (.Columns.Count - 1)           'データ列の幅を計算

            '.BorderColor = Color.Gray
            .BorderStyle = WebControls.BorderStyle.Solid
            .BorderWidth = 1

            ''.HeaderRow.BorderColor = Color.Black
            .HeaderRow.Cells(0).Width = TblProp.DateFieldWidth
            .HeaderRow.Font.Size = TblProp.SubTitleFont
            .HeaderRow.BorderStyle = WebControls.BorderStyle.Solid
            .HeaderRow.BorderWidth = 1
            ''.HeaderRow.Font.Bold = False
            .HeaderRow.Height = TblProp.SubTitleFont + 5

            .FooterRow.BorderColor = Color.Black
            .FooterRow.BorderStyle = WebControls.BorderStyle.Solid
            .FooterRow.BorderWidth = 1
            .FooterRow.HorizontalAlign = HorizontalAlign.Right
            .FooterRow.Height = TblProp.SubTitleFont + 5

            .PagerStyle.HorizontalAlign = HorizontalAlign.Center

            '行が足りない場合は、空行で埋める
            intTemp = (TblProp.RowCount - .Rows.Count)
            If intTemp <> 0 Then
                For iloop = 1 To intTemp
                    'Dim GridViewRow = New GridViewRow(-1, -1, DataControlRowType.DataRow, DataControlRowState.Normal)
                    'Dim TableCell = New TableCell()
                    '.Controls.Add(GridViewRow)

                    Dim row1 As New GridViewRow(-1, -1, DataControlRowType.DataRow, DataControlRowState.Normal)
                    For jloop = 1 To TblProp.ColCount + 1
                        Dim cell11 As New TableCell
                        ''cell11.Font.Size = TblProp.DataFont
                        cell11.HorizontalAlign = HorizontalAlign.Center                      ''Center 2009/01/29 Kino Changed
                        cell11.Text = "--"
                        cell11.BorderColor = Color.Black
                        row1.Cells.Add(cell11)
                        row1.Height = Me.GrdTimeSeries.RowStyle.Height
                        row1.BackColor = Color.White
                        .Controls(0).Controls.AddAt(.Rows.Count + 3, row1)
                        cell11.BorderWidth = 1
                    Next jloop

                Next iloop
            End If
            .Font.Size = TblProp.DataFont

            '.Columns(0).HeaderStyle.Width = TblProp.DateFieldWidth
            '.Columns(0).ItemStyle.Width = TblProp.DateFieldWidth
            '.Columns(0).ItemStyle.HorizontalAlign = HorizontalAlign.Center
            'For iloop = 1 To .Columns.Count - 1
            '    .Columns(iloop).HeaderStyle.Width = intTemp
            '    .Columns(iloop).ItemStyle.Width = intTemp
            '    .Columns(iloop).ItemStyle.HorizontalAlign = HorizontalAlign.Right
            'Next iloop
        End With

    End Sub

    Protected Sub AddRowCol(ByVal HeaderFooterFlg As System.Web.UI.WebControls.DataControlRowType, ByVal item As GridViewRow, _
                            ByVal AddIndex As Integer, ByVal SetText As String, ByVal BorderColor As Color, ByVal ColSpan As Integer, _
                            ByVal TextAlign As System.Web.UI.WebControls.HorizontalAlign, _
                            Optional ByVal FontSizeSet As FontSize = FontSize.NotSet, Optional ByVal FontSet As FontStyle = False, Optional ByVal ColWidth As Integer = 0)
        ''
        '' ヘッダー、フッターにセルを追加して文字を記入する
        ''
        Dim cell As TableCell

        cell = New TableCell()
        cell.ColumnSpan = ColSpan
        cell.BorderColor = BorderColor
        cell.BorderWidth = 1
        If FontSizeSet <> WebControls.FontSize.NotSet Then
            cell.Font.Size = FontSizeSet
        End If
        cell.Font.Bold = FontSet
        item.Cells.Add(cell)
        If Val(SetText) < abnormalValue Then                                '2009/08/10 Kino Add abnormalValue
            cell.Text = SetText
        Else
            cell.Text = "--"
        End If
        cell.HorizontalAlign = TextAlign
        cell.Height = TblProp.SubTitleFont + 5
        GrdTimeSeries.Controls(0).Controls.AddAt(AddIndex, item)
        If ColWidth <> 0 And SetText <> "最大値" And SetText <> "最小値" Then                                       '' 2009/01/29 Kino Add
            cell.Width = ColWidth
        End If

    End Sub

    Protected Sub GrdTimeSeries_RowCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdTimeSeries.RowCreated
        ''
        '' ヘッダ行を追加する
        ''
        'Dim item As DataGridItem
        Dim item As GridViewRow
        ''Dim cell As TableCell
        Dim iloop As Integer
        Dim jloop As Integer
        Dim setText As String
        Dim shtTemp As Short
        Dim seDataText As String = Nothing        ' 2012/02/24 Kino Add

        With e.Row
            .BorderWidth = 1
            If .RowType = DataControlRowType.DataRow Then ''Exit Sub

                '' マウスオーバーで行に色を設定
                ' onmouseover属性を設定　　'#CC99FF'
                .Attributes("onmouseover") = "setBg(this, '#84D7FF')"

                ' データ行が通常行／代替行であるかで処理を分岐（2）
                If .RowState = DataControlRowState.Normal Then
                    .Attributes("onmouseout") = _
                      String.Format("setBg(this, '{0}')", _
                        ColorTranslator.ToHtml(Me.GrdTimeSeries.RowStyle.BackColor))
                Else
                    .Attributes("onmouseout") = _
                      String.Format("setBg(this, '{0}')", _
                        ColorTranslator.ToHtml( _
                          Me.GrdTimeSeries.AlternatingRowStyle.BackColor))
                End If
                .Height = TblProp.DataFont + 5

            ElseIf .RowType = DataControlRowType.Header Then

                '=== 単位行を追加＆単位を出力 ===
                item = New GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal)

                Call AddRowCol(DataControlRowType.Header, item, 0, "単位", Color.Black, 1, HorizontalAlign.Center)
                ''cell = New TableCell()
                ''cell.ColumnSpan = 1
                ''cell.BorderColor = Color.Black
                ''item.Cells.Add(cell)
                ''cell.Text = "単位"
                ''GrdTimeSeries.Controls(0).Controls.AddAt(0, item)

                For iloop = 0 To TblProp.ColCount - 1
                    Call AddRowCol(DataControlRowType.Header, item, 0, TblProp.Sensor(iloop).SensorUnit, Color.Black, 1, HorizontalAlign.Center, TblProp.SubTitleFont, False)
                    ''cell = New TableCell()
                    ''cell.ColumnSpan = 1
                    ''cell.Font.Size = TblProp.SubTitleFont
                    ''cell.Font.Bold = False
                    ''cell.BorderColor = Color.Black
                    ''item.Cells.Add(cell)
                    ''cell.Text = TblProp.Sensor(iloop).SensorUnit
                    ''GrdTimeSeries.Controls(0).Controls.AddAt(0, item)
                Next iloop

                '=== 種別を追加＆種別を出力 ===
                Dim dummyCount As Integer = 1
                Dim CellIndex As Integer = 2
                Dim removeFlg As Integer
                item = New GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal)
                Call AddRowCol(DataControlRowType.Header, item, 0, "種別", Color.Black, 1, HorizontalAlign.Center)
                ''cell = New TableCell()
                ''cell.ColumnSpan = 1
                ''cell.BorderColor = Color.Black
                ''item.Cells.Add(cell)
                ''cell.Text = "種別"
                ''GrdTimeSeries.Controls(0).Controls.AddAt(0, item)

                For iloop = 0 To TblProp.ColCount - 1
                    Call AddRowCol(DataControlRowType.Header, item, 0, TblProp.Sensor(iloop).SensorName, Color.Black, 1, HorizontalAlign.Center, TblProp.SubTitleFont, False)
                    ''cell = New TableCell()
                    ''cell.ColumnSpan = 0
                    ''cell.Font.Size = TblProp.SubTitleFont
                    ''cell.Font.Bold = False
                    ''cell.BorderColor = Color.Black
                    ''item.Cells.Add(cell)
                    ''cell.Text = TblProp.Sensor(iloop).SensorName
                    ''GrdTimeSeries.Controls(0).Controls.AddAt(0, item)

                    '=== 種別の列が同じ項目なら行結合する ===
                    If iloop >= 1 Then
                        If TblProp.Sensor(iloop - 1).SensorName = TblProp.Sensor(iloop).SensorName Then
                            dummyCount = dummyCount + 1
                            item.Cells.RemoveAt(CellIndex)
                            item.Cells(CellIndex - 1).ColumnSpan = dummyCount
                            removeFlg = 0
                        Else
                            dummyCount = 1
                            CellIndex += 1
                            If removeFlg = 0 Then removeFlg = 1 Else removeFlg = 0
                        End If
                    End If
                Next iloop

                ''列数が少ない場合は空列を追加する
                ''If TblProp.PaperOrientaion = 0 Then
                ''    shtTemp = TblProp.ColCount ' - 12
                ''Else
                shtTemp = TblProp.ColCount ' - 18
                ''End If
                If shtTemp < 0 Then
                    shtTemp *= -1
                    For iloop = 1 To TblProp.ColCount           'shtTemp
                        Call AddRowCol(DataControlRowType.Header, item, 0, "", Color.Black, TblProp.DataFieldWidth, HorizontalAlign.Center, TblProp.SubTitleFont, False)
                    Next
                End If

            ElseIf .RowType = DataControlRowType.Footer Then

                If TblProp.ShowAvg = True Then                                                                                      '平均値表示ありなら処理する
                    For iloop = 0 To .Cells.Count - 1
                        If iloop = 0 Then
                            .Cells(0).Text = "平均値"
                            .Cells(0).HorizontalAlign = HorizontalAlign.Center                                                      '2009/02/02 Kino Add
                        Else
                            ''If MaxMinData(iloop - 1, 2).ToString(TblProp.Sensor(iloop - 1).ShowFormat) < abnormalValue Then         '2009/09/01 Kino Changed
                            If MaxMinData(iloop - 1, 2) < abnormalValue Then                                                        '2009/08/10 Kino Add abnormalValue
                                ''.Cells(iloop).Text = MaxMinData(iloop - 1, 2).ToString(TblProp.Sensor(iloop - 1).ShowFormat)      ' 2012/02/24 Kino Changed ↓
                                .Cells(iloop).Text = clsDataOpe.trunc_round(MaxMinData(iloop - 1, 2), TblProp.Sensor(iloop - 1).ShowFormat, TblProp.TruncFlg)

                                If TblProp.Sensor(iloop - 1).replaceInf IsNot Nothing Then                                          ' 2013/01/29 Kino Add 文字列置換したときは平均値を消す
                                    .Cells(iloop).Text = "--"
                                End If

                            Else
                                .Cells(iloop).Text = "--"
                            End If
                        End If
                        .Cells(iloop).BorderColor = Drawing.Color.Black
                    Next iloop

                    '=== 単位行を追加＆単位を出力 ===
                    ''最小値
                    Dim intTemp As Integer = (Me.GrdTimeSeries.Width.Value - TblProp.DateFieldWidth - 70) / TblProp.ColCount        '2009/01/29 Kino Add 列幅指定を追加
                    For jloop = 4 To 3 Step -1
                        If jloop = 3 Then setText = "最大値" Else setText = "最小値"

                        item = New GridViewRow(-1, -1, DataControlRowType.Footer, DataControlRowState.Normal)
                        Call AddRowCol(DataControlRowType.Footer, item, Me.GrdTimeSeries.Rows.Count + 3, setText, Color.Black, 1, HorizontalAlign.Center, FontSize.NotSet, False, intTemp)

                        For iloop = 0 To TblProp.ColCount - 1
                            ' Call AddRowCol(DataControlRowType.Header, item, 0, MaxMinData(iloop, jloop).ToString(TblProp.Sensor(iloop).ShowFormat), Color.Black, 1, HorizontalAlign.Right, FontSize.NotSet, False, intTemp)　'2012/02/24 Kino Changed ↓
                            seDataText = clsDataOpe.trunc_round(MaxMinData(iloop, jloop), TblProp.Sensor(iloop).ShowFormat, TblProp.TruncFlg)

                            If TblProp.Sensor(iloop).replaceInf IsNot Nothing Then                                                              ' 2013/01/29 Kino Add 数値を文字列に置換する
                                Dim repData As String = replaceDataString(seDataText, TblProp.Sensor(iloop).replaceInf)
                                If repData IsNot Nothing Then
                                    seDataText = repData
                                End If
                            End If

                            Call AddRowCol(DataControlRowType.Header, item, 0, seDataText, Color.Black, 1, HorizontalAlign.Right, FontSize.NotSet, False, intTemp)
                        Next iloop

                    Next jloop
                End If

            End If
        End With

    End Sub

    Protected Sub GrdTimeSeries_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdTimeSeries.RowDataBound
        ''
        '' グリッドのセル単位の書式設定
        ''
        Dim iloop As Integer
        Dim sngData As Single
        Dim dteDate As Date
        Dim intTemp As Integer
        Dim DummyCount As Short = 0
        Dim colCount As Integer
        ''If TblProp.PaperOrientaion = 0 Then
        ''    If TblProp.ColCount < 12 Then
        ''        colCount = 12
        ''    Else
        ''        colCount = TblProp.ColCount
        ''    End If
        ''Else
        ''    If TblProp.ColCount < 18 Then
        ''        colCount = 18
        ''    Else
        ''        colCount = TblProp.ColCount
        ''    End If
        ''End If
        colCount = TblProp.ColCount                                                                     '2009/01/29 Kino Changed これにした

        intTemp = (Me.GrdTimeSeries.Width.Value - TblProp.DateFieldWidth - 70) / (colCount)           'データ列の幅を計算

        With e.Row
            If .RowType = DataControlRowType.DataRow Then                                   'データ
                For iloop = 0 To .Cells.Count - 1
                    .Cells(iloop).BorderWidth = 1
                    .Cells(iloop).BorderStyle = WebControls.BorderStyle.Solid
                    .Cells(iloop).BorderColor = Drawing.Color.Black
                    '.Cells(iloop).Width = 20

                    If iloop = 0 Then

                        If Date.TryParse(.Cells(iloop).Text, dteDate) = True Then
                            dteDate = Date.Parse(.Cells(iloop).Text)
                            .Cells(iloop).Text = dteDate.ToString(TblProp.DateFormat)
                            ''.Cells(iloop).Text = "2008/08/28 18:50"
                            .Cells(iloop).HorizontalAlign = HorizontalAlign.Right               ''Center 2009/01/29 Kino
                            .Cells(iloop).Width = TblProp.DateFieldWidth
                        End If
                    Else
                        If Single.TryParse(e.Row.Cells(iloop).Text, sngData) Then
                            If sngData < 1.1E+30 And TblProp.Sensor(iloop - 1).ShowFormat = "-" Then                '2008/08/20 Kino ダミー列用
                                .Cells(iloop).Text = "-"
                            ElseIf sngData < abnormalValue Then
                                ''.Cells(iloop).Text = String.Format("{0:F2}", sngData)
                                .Cells(iloop).Text = clsDataOpe.trunc_round(sngData, TblProp.Sensor(iloop - 1).ShowFormat, TblProp.TruncFlg)          'sngData.ToString(TblProp.Sensor(iloop - 1).ShowFormat)

                                If TblProp.Sensor(iloop - 1).replaceInf IsNot Nothing Then                                                              ' 2013/01/29 Kino Add 数値を文字列に置換する
                                    Dim repData As String = replaceDataString(.Cells(iloop).Text, TblProp.Sensor(iloop - 1).replaceInf)
                                    If repData IsNot Nothing Then
                                        .Cells(iloop).Text = repData
                                    End If
                                End If

                                '' 管理値と比較してスタイルシートを設定すると背景色が付く　2016/10/06 Kino  とりあえずテストのみ
                                '' 各チャンネルごとに、上下限１～３次を比較するので、1chあたり6回ループｘデータ数　を回す必要があるため、処理時間が気になるところではある
                                ''Select Case sngData
                                ''    Case Is > 0.1
                                '        .Cells(iloop).CssClass = "al3"
                                '    'Case Is > 0.03
                                '    '    .Cells(iloop).CssClass = "al2"
                                '    'Case Is > 0.01
                                '    '    .Cells(iloop).CssClass = "al1"
                                ''End Select

                            Else
                                .Cells(iloop).Text = "--"
                            End If
                        End If
                        .Cells(iloop).HorizontalAlign = HorizontalAlign.Right
                        .Cells(iloop).Width = intTemp
                        ''Else
                        ''    .Cells(iloop).Width = 0
                        ''    .Cells(iloop).Text = ""
                    End If
                Next iloop

            ElseIf .RowType = DataControlRowType.Header Then                               'ヘッダー

                Dim sortLink As LinkButton = CType(.Cells(0).Controls(0), LinkButton)
                If sortLink.Text = "日付" Then
                    If Me.GrdTimeSeries.SortDirection = SortDirection.Ascending Then
                        sortLink.Text &= "<span class='arrow' title='昇順'>▼</span>"
                        'sortLink.Text &= "<img src='./img/asc.gif' title='昇順' />"
                    Else
                        sortLink.Text &= "<span class='arrow' title='降順'>▲</span>"
                        'sortLink.Text &= "<img src='./img/desc.gif' title='降順' />"
                    End If
                End If
                For iloop = 0 To .Cells.Count - 1
                    .Cells(iloop).Font.Size = TblProp.SubTitleFont
                    .Cells(iloop).BorderColor = Drawing.Color.Black
                    If iloop > 0 Then
                        If TblProp.Sensor(iloop - 1).SensorSymbol.Contains("空@_") Then                 '' 2010/02/22 Kino Add 空列のヘッダ対応
                            .Cells(iloop).Text = "-"
                        Else
                            .Cells(iloop).Text = TblProp.Sensor(iloop - 1).SensorSymbol
                        End If
                    End If
                Next iloop

                Dim Row As GridViewRow = e.Row                                                          '' 2010/07/30 Kino Add 
                Dim cell As TableCell                                                                   '' 2010/07/30 Kino Add 計器記号でドットとダッシュを出すためのおまじない
                For Each cell In Row.Cells
                    If cell.Text.ToString <> "" Then                                                    '' １列目(日付ソートリンクがある列)は飛ばす
                        cell.Text = cell.Text.Replace("\．", ".")                                        ' 2013/01/29 ここをいじって、TblProp.Sensor(iloop - 1).SensorSymbolを直接書き込めばエイリアスは不要になる
                        cell.Text = cell.Text.Replace("\’", "'")
                    End If
                Next cell

            ElseIf .RowType = DataControlRowType.Pager Then                                             ' 2011/02/01 Kino Add 現在ページの文字を大きく

                Dim wc As System.Web.UI.Control                                                         ' 2017/03/29 Kino Changed  Dabrosに準拠 <- System.Web.UI.WebControls.TableCell <- 'Control

                For Each wc In e.Row.Cells(0).Controls(0).Controls(0).Controls                          'System.Web.UI.WebControls.TableRow.CellControlCollection

                    Try
                        Dim pageNo As Label = wc.Controls(0)
                        If Not IsNothing(pageNo) Then   'String.IsNullOrEmpty(pageNo.Text) Then
                            pageNo.Style.Add(HtmlTextWriterStyle.FontSize, "x-large")
                        End If
                    Catch ex As Exception

                    End Try

                Next
                .Height = TblProp.SubTitleFont

            Else                                                                        'データ、ヘッダー以外
                For iloop = 0 To .Cells.Count - 1
                    .Cells(iloop).BorderColor = Drawing.Color.Black
                    If iloop <> 0 Then                                                  '2009/02/02 平均値、ページャのセンタリングのため
                        .Cells(iloop).Width = intTemp
                    End If
                Next iloop
                Me.GrdTimeSeries.PagerStyle.Width = Me.GrdTimeSeries.Width
            End If

        End With

    End Sub

    ''' <summary>
    ''' 文字列を置換して返す
    ''' </summary>
    ''' <param name="dataString">置換する元のデータ</param>
    ''' <param name="repInf">置換情報(配列)</param>
    ''' <returns>置換後の文字列</returns>
    ''' <remarks></remarks>
    Protected Function replaceDataString(dataString As String, repInf() As String) As String

        Dim reploop As Integer
        Dim partRep() As String
        Dim strRet As String = dataString
        For reploop = 0 To repInf.Length - 1
            partRep = repInf(reploop).Split(":")
            strRet = (strRet.Replace(partRep(0), partRep(1)))
        Next

        Return strRet

    End Function

    Protected Function ReadTableInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, ByVal tableType As String, Optional ByVal PostBackFlg As Boolean = False) As Integer
        ''
        '' 表の描画情報を読み込む
        ''
        Dim strSQL As String
        '' Dim DbDr As OleDb.OleDbDataReader
        '' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")

        ReadTableInfoFromDB = 0

        strSQL = "Select * From メニュー基本情報 Where (項目名 = '" & TblProp.WindowTitle & "' And 種別 = '" & tableType & "')"
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        For Each DTR As DataRow In DtSet.Tables("DData").Rows
            With DTR.ItemArray
                TblProp.RowCount = Integer.Parse(.GetValue(4))              '行数
                TblProp.ColCount = Integer.Parse(.GetValue(5))              '列数
                TblProp.TableTitle = .GetValue(6).ToString                  '表のタイトル
                TblProp.TableSubTitle = .GetValue(7).ToString               '表のサブタイトル
                TblProp.TitlePosition = Short.Parse(.GetValue(8))           'タイトルの位置(上or下)
                TblProp.DayRange = Integer.Parse(.GetValue(9))              '表示期間
                TblProp.DateFormat = .GetValue(10).ToString                 '日付の表示フォーマット
                TblProp.PaperOrientaion = Integer.Parse(.GetValue(11))      '紙の向き
                TblProp.TitleFont = Integer.Parse(.GetValue(12))            'タイトルフォントのサイズ
                TblProp.SubTitleFont = Integer.Parse(.GetValue(13))         'サブタイトルフォントのサイズ
                TblProp.DataFont = Integer.Parse(.GetValue(14))             'データのフォントのサイズ
                TblProp.ThinInf = .GetValue(15).ToString                    '間引き情報
                TblProp.ShowAvg = Boolean.Parse(.GetValue(16))              '平均値表示
                TblProp.DateFieldWidth = Integer.Parse(.GetValue(17))       '日付列幅
                TblProp.DataFieldWidth = Integer.Parse(.GetValue(18))       'データ列幅
                TblProp.DataFileNo = Integer.Parse(.GetValue(19))           'データファイル番号
                TblProp.thinoutEnable = Boolean.Parse(.GetValue(21))        '間引きのチェックを入れるかどうか
                TblProp.TruncFlg = Boolean.Parse(.GetValue(22))             '演算後のデータを表示桁で切り捨てるかどうか
            End With
        Next
        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        .Read()                                             '１レコード分読み込み
        ''        TblProp.RowCount = .GetInt16(4)                     '行数
        ''        TblProp.ColCount = .GetInt16(5)                     '列数
        ''        TblProp.TableTitle = .GetString(6)                  '表のタイトル
        ''        TblProp.TableSubTitle = .GetString(7)               '表のサブタイトル
        ''        TblProp.TitlePosition = CType(.GetInt16(8), Short)  'タイトルの位置(上or下)
        ''        TblProp.DayRange = .GetInt16(9)                     '表示期間
        ''        TblProp.DateFormat = .GetString(10)                 '日付の表示フォーマット
        ''        TblProp.PaperOrientaion = .GetInt16(11)             '紙の向き
        ''        TblProp.TitleFont = .GetInt16(12)                   'タイトルフォントのサイズ
        ''        TblProp.SubTitleFont = .GetInt16(13)                'サブタイトルフォントのサイズ
        ''        TblProp.DataFont = .GetInt16(14)                    'データのフォントのサイズ
        ''        TblProp.ThinInf = .GetString(15)                    '間引き情報
        ''        TblProp.ShowAvg = .GetBoolean(16)                   '平均値表示
        ''        TblProp.DateFieldWidth = .GetInt16(17)              '日付列幅
        ''        TblProp.DataFieldWidth = .GetInt16(18)              'データ列幅
        ''        TblProp.DataFileNo = .GetInt16(19)                  'データファイル番号
        ''        TblProp.thinoutEnable = .GetBoolean(21)             '間引きのチェックを入れるかどうか
        ''        TblProp.TruncFlg = .GetBoolean(22)                  '演算後のデータを表示桁で切り捨てるかどうか
        ''    End If
        ''End With
        ''DbDr.Close()
        ''DbCom.Dispose()

        ReDim TblProp.Sensor(TblProp.ColCount - 1)                  '個々の列の情報を格納する配列設定

        ReadTableInfoFromDB = 1

    End Function

    Protected Function TableColumnInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, ByRef getFromDB As Short) As Integer
        ''
        '' 項目名におけるページに表示する列の情報
        ''
        '' Dim DbDr As OleDb.OleDbDataReader
        '' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strSQL As String
        Dim iloop As Integer
        Dim strTemp As String = ""              '2010/02/22 Kino Add
        Dim dmyCount As Integer = 0             '2010/02/22 Kino Add

        strSQL = "Select * From [" & TblProp.WindowTitle & "＿列情報] ORDER BY ID ASC;"
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        With DtSet.Tables("DData")                '表ヘッダ
            If .Rows.Count > 0 Then
                'データチャネル
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).DataCH = Integer.Parse(.Rows(0).Item(iloop))
                Next

                'センサ名
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).SensorName = .Rows(1).Item(iloop).ToString
                Next

                'センサ記号
                For iloop = 1 To TblProp.ColCount
                    strTemp = .Rows(2).Item(iloop).ToString
                    If strTemp = "-" Then
                        dmyCount += 1
                        TblProp.Sensor(iloop - 1).SensorSymbol = "空@_" + dmyCount.ToString         '' 2010/02/22 Kino Add 空列の簡易設定対応
                    ElseIf strTemp = "@@CH" Then
                        TblProp.Sensor(iloop - 1).SensorSymbol = strTemp                            '' 2010/02/22 Kino Add 共通情報が取得するための情報
                        TblProp.SensorSymbolByCommonInf += (TblProp.Sensor(iloop - 1).DataCH.ToString & ",")        ''チャンネル番号を保存
                        getFromDB = 1
                    Else
                        TblProp.Sensor(iloop - 1).SensorSymbol = strTemp
                    End If
                Next
                If getFromDB = 1 Then
                    If TblProp.SensorSymbolByCommonInf.EndsWith(",") = True Then
                        TblProp.SensorSymbolByCommonInf = Left(TblProp.SensorSymbolByCommonInf, TblProp.SensorSymbolByCommonInf.ToString.Length - 1)     '最後のカンマを取る
                    End If
                End If

                '単位
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).SensorUnit = .Rows(3).Item(iloop).ToString
                Next

                '表示形式
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).ShowFormat = .Rows(4).Item(iloop).ToString
                Next

                If .Rows.Count >= 6 Then
                    '置換情報                                                                                                   ' 2013/01/21 Kino Add 深度階対応
                    Dim allRep() As String = Nothing
                    Dim jloop As Integer
                    For iloop = 1 To TblProp.ColCount
                        allRep = .Rows(5).Item(iloop).ToString.Split(",")                       ' まずは全文を分解
                        If .Rows(5).Item(iloop).ToString.Length <> 0 Then
                            ReDim TblProp.Sensor(iloop - 1).replaceInf(allRep.Length - 1)
                            For jloop = 0 To allRep.Length - 1
                                TblProp.Sensor(iloop - 1).replaceInf(jloop) = allRep(jloop)
                            Next

                            'TblProp.Sensor(iloop - 1).replaceInf = .Rows(5).Item(iloop).ToString
                        Else
                            TblProp.Sensor(iloop - 1).replaceInf = Nothing
                        End If
                    Next
                End If

            End If
        End With

        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        'データチャネル
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).DataCH = Integer.Parse(.GetString(iloop))
        ''        Next
        ''        'センサ名
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).SensorName = .GetString(iloop)
        ''        Next
        ''        'センサ記号
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            strTemp = .GetString(iloop)
        ''            If strTemp = "-" Then
        ''                dmyCount += 1
        ''                TblProp.Sensor(iloop - 1).SensorSymbol = "空@_" + dmyCount.ToString         '' 2010/02/22 Kino Add 空列の簡易設定対応
        ''            ElseIf strTemp = "@@CH" Then
        ''                TblProp.Sensor(iloop - 1).SensorSymbol = strTemp                            '' 2010/02/22 Kino Add 共通情報が取得するための情報
        ''                TblProp.SensorSymbolByCommonInf += (TblProp.Sensor(iloop - 1).DataCH.ToString & ",")        ''チャンネル番号を保存
        ''                getFromDB = 1
        ''            Else
        ''                TblProp.Sensor(iloop - 1).SensorSymbol = strTemp
        ''            End If
        ''        Next
        ''        If getFromDB = 1 Then
        ''            If TblProp.SensorSymbolByCommonInf.EndsWith(",") = True Then
        ''                TblProp.SensorSymbolByCommonInf = Left(TblProp.SensorSymbolByCommonInf, TblProp.SensorSymbolByCommonInf.ToString.Length - 1)     '最後のカンマを取る
        ''            End If
        ''        End If
        ''        '単位
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).SensorUnit = .GetString(iloop)
        ''        Next
        ''        '表示形式
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).ShowFormat = .GetString(iloop)
        ''        Next
        ''    End If
        ''    Return 1
        ''End With
        ''DbDr.Close()
        ''DbCom.Dispose()
        Return 1

    End Function


    Protected Sub GetMostUpdate(Optional ByVal DataFileNo As Integer = 0)
        ''
        '' 使用しているデータファイルの中から、一番最新のデータ日付を取得する
        ''
        Dim dteSession As Date

        dteSession = CType(Session.Item("LastUpdate" & TblProp.DataFileNo.ToString), Date)
        If TblProp.LastUpdate < dteSession Then
            TblProp.LastUpdate = dteSession
        End If


    End Sub

    ''Protected Sub ImbtnRedrawGraph_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImbtnRedrawGraph.Click
    ''    ''
    ''    ''イメージボタンクリックイベント
    ''    ''　作図処理のため

    ''    Call Bind2DB()

    ''End Sub

    '' javascriptによる制御へ移行した
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

    ''Protected Sub CheckAutoUpdate(ByVal SiteDirectory As String)          'クラスへ移行

    ''    Dim DataFile As String
    ''    Dim intInterval As Integer '= 600000
    ''    Dim blnAutoUpdate As Boolean '= False
    ''    Dim clsUpdate As New ClsReadDataFile

    ''    DataFile = Server.MapPath(SiteDirectory & "\App_Data\MenuInfo.mdb")
    ''    clsUpdate.CheckAutoUpdate(DataFile, "経時表", intInterval, blnAutoUpdate)

    ''    'タイマーの有効／無効を設定
    ''    Me.Tmr_Update.Enabled = blnAutoUpdate
    ''    Me.Tmr_Update.Interval = intInterval

    ''End Sub

    ''Protected Sub GetMostUpdate(Optional ByVal DataFileNo As Integer = 0)
    ''    ''
    ''    '' 使用しているデータファイルの中から、一番最新のデータ日付を取得する
    ''    ''

    ''    Dim dteSession As Date

    ''    dteSession = CType(Session.Item("LastUpdate" & TblProp.DataFileNo.ToString), Date)
    ''    If TblProp.LastUpdate < dteSession Then
    ''        TblProp.LastUpdate = dteSession
    ''    End If

    ''End Sub

End Class
