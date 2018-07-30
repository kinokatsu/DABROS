Imports AjaxControlToolkit
Imports System.Data
Imports System.Drawing

Partial Class Table_Summary
    Inherits System.Web.UI.Page

    '表における個々の列情報
    Private Structure TblColumnInfo
        Dim TableHeader As String
        Dim wordAlign As String
        Dim ShowFormat As String
        Dim DateFieldWidth As Integer
        Dim DataFontSize As Single
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
        ''Dim ThinInf As String                   '表示データの間引き情報
        ''Dim ShowAvg As Boolean                  '平均値を表示するかどうか
        ''Dim DateFieldWidth As Integer           '日付列幅
        ''Dim DataFieldWidth As Integer           'データ列幅
        Dim outCh As String                     '出力するチャンネル番号
        Dim TitleFont As Integer                'タイトルのフォントプロパティ
        Dim SubTitleFont As Integer             'タイトルのフォントプロパティ
        Dim DataFont As Integer                 'データのフォントプロパティ
        Dim Sensor() As TblColumnInfo           '表に記述する内容の情報
        Dim DataFileNo As Integer               'データファイル番号
        Dim PagingEnable As Boolean             'ページングするかどうか
        Dim PointName As String                 '測点名称
        Dim TruncFlg As Boolean                 '表示桁以下の切捨ての有無

    End Structure

    Private TblProp As TblInfo                  '表描画に関する情報
    Private MaxMinData(,) As Single             '列ごとの最大、最小、平均値を格納
    Private DataFileNames() As ClsReadDataFile.DataFileInf

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

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        ''
        '' ページを閉じるときにデータオブジェクトとグリッドをDisposeする
        ''

        ''Me.AccessDataSource1.Dispose()
        'Me.GrdTimeSeries.Dispose()
        'Me.ViewState.Clear()

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"
        If User.Identity.IsAuthenticated = False Then
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer

        LoginStatus = Session.Item("LgSt")          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            'Response.Redirect("sessionerror.aspx")
            Response.Write(strScript)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする

        Dim siteName As String
        Dim siteDirectory As String
        Dim tblDataFile As String
        Dim intRet As Integer
        Dim clsNewDate As New ClsReadDataFile
        Dim clsSetScript As New ClsGraphCommon
        Dim OldTerm As String

        OldTerm = CType(Session.Item("OldTerm"), String)            '過去データ表示制限
        siteName = CType(Session.Item("SN"), String)                '現場名
        siteDirectory = CType(Session.Item("SD"), String)           '現場ディレクトリ
        tblDataFile = IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", "SummaryTableInfo.mdb")
        TblProp.WindowTitle = CType(Request.Item("GNa"), String)    '表タイトル（ウィンドウタイトル）
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)

        intRet = clsNewDate.GetDataFileNames(IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", "MenuInfo.mdb"), DataFileNames)       'データファイル名、共通情報ファイル名、識別名を取得
        intRet = clsNewDate.GetDataLastUpdate(IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data"), DataFileNames)                  '2010/05/24 Kino Add データの最古、最新日付を取得
        Dim OldFlg As Boolean = IO.File.Exists(IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", "Old.flg"))                          ' 2011/04/19 Kino Add 前の設定と調整するため

        If IsPostBack = False Then      'ポストバックではないときにはデータベースから設定を読み込む

            If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
                ''==== フォームのサイズを調整する ====
                If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
                    Dim OpenString As String

                    OpenString = "<SCRIPT LANGUAGE='javascript'>"
                    OpenString &= "window.resizeTo(" & wid & "," & Hei & ");"
                    OpenString &= "<" & "/SCRIPT>"
                    Page.ClientScript.RegisterStartupScript(Me.GetType(), "集計表", OpenString)
                End If
            End If

            '初回読み込みはデータベースから行なう
            Dim DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & tblDataFile & ";" & "Jet OLEDB:Engine Type= 5"
            ''DbCon.Open()

            intRet = ReadTableInfoFromDB(DbCon, "集計表")                   '作表情報を取得
            intRet = TableColumnInfoFromDB(DbCon)                           '作表の表示内容(列情報)を取得

            Call SetBIND2Controls(DbCon)                                    '各コントロールのリスト内容をDBから作成する

            ''DbCon.Close()
            DbCon.Dispose()

            Call SetViewState()                                             '読みこんだ内容をビューステートに格納

            TblProp.LastUpdate = CType(Session.Item("LastUpdate" & TblProp.DataFileNo.ToString), DateTime)        '最新データ

            Call clsSetScript.SetSelectDateScript(Me.Form)                       'コントロールにJavaScriptを割り当て

            Dim OldDate As Date
            If OldTerm <> "None" Then
                Dim clsCalc As New ClsReadDataFile
                OldDate = clsCalc.CalcOldDateLimit(OldTerm)
                clsCalc = Nothing
            Else
                OldDate = DataFileNames(0).MostOldestDate.AddDays(+1)
            End If
            Me.RngValidSt.MinimumValue = OldDate.ToString("yyyy/MM/dd")
            Me.RngValidSt.MaximumValue = Date.Parse(TblProp.LastUpdate.ToString("yyyy/MM/dd"))
            Me.RngValidSt.ErrorMessage = "開始日は" + OldDate + "以降、終了日は" + TblProp.LastUpdate.ToString("yyyy/MM/dd") + "以前としてください。"
            Me.C1WebCalendar.MinDate = OldDate
            Me.C1WebCalendar.MaxDate = TblProp.LastUpdate

            Me.nwdt.Value = DataFileNames(TblProp.DataFileNo).NewestDate.ToString("yyyy/MM/dd HH:mm:ss")
            Me.nwdtno.Value = TblProp.DataFileNo.ToString                                                       ' 2018/03/16 Kino Add
            If siteName.Contains("【完了現場】") = True Then                                                    ' 2018/03/16 Kino Add
                Me.nwdt.Value = "NC"                                                                            ' 完了現場は、更新チェックしない
            End If

        Else                            'ポストバックの時はビューステートから設定を読み込む

            Call ReadViewState()

        End If

        If ToolkitScriptManager1.AsyncPostBackSourceElementID = "ImbtnRedrawGraph" Or ToolkitScriptManager1.AsyncPostBackSourceElementID.Length = 0 Then                 'ポストバックがどのコントロールから発生したのかで判断(カレンダーのイベントではここに入らない!!)

            Call GetMostUpdate()                                                '最新データの日付を取得
            TblProp.LastUpdate = CType(Session.Item("LastUpdate" & TblProp.DataFileNo.ToString), String)        '最新データ
            Me.C1WebCalendar.MaxDate = Date.Parse(TblProp.LastUpdate)

            TblProp.PaperOrientaion = 0                                         '用紙は縦固定
            '表のタイトル表示、縦横設定
            Call SetTableTitle(OldFlg)

            If IsPostBack = False Then
                Me.TxtStartDate.Text = TblProp.LastUpdate.ToString("yyyy/MM/dd")                                    '開始日の初期状態を設定
            End If

            Me.LblLastUpdate.Text = "最新データ日時：" & TblProp.LastUpdate.ToString("yyyy/MM/dd HH:mm")
            Me.MyTitle.Text = TblProp.TableTitle & " [集計表] - " & siteName                                    'ブラウザのタイトルバーに表示するタイトル

            Call Bind2DB()                                                                                      'データを表に成形

            ''Dim ImgSt As System.Web.UI.WebControls.Image = CType(Me.FindControl("ImgStDate"), System.Web.UI.WebControls.Image)
            ''ImgSt.Attributes.Add("onclick", "DropCalendar(this);")                    ' 2012/11/02 Kino Changed jQuery
            ''Me.imgCalTxtStartDate.Attributes.Add("onclick", "DropCalendar(this);")
            ''Me.imgCalTxtStartDate.Attributes.Add("onclick", "DropCalendar();")
            Me.RdBtnList.Items(0).Attributes.Add("onclick", "ChangeState(0);")
            Me.RdBtnList.Items(1).Attributes.Add("onclick", "ChangeState(1);")
            Dim StDate As Date = CDate((Me.TxtStartDate.Text))

        End If

        clsSetScript = Nothing
        clsNewDate = Nothing

    End Sub

    Public Sub SetBIND2Controls(ByVal dbCon As OleDb.OleDbConnection)
        ''
        ''ドロップダウンリストボックスにおける各設定のデータセットバインド
        ''
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strSQL As String

        ''■■日付範囲
        strSQL = "SELECT * FROM set_開始時刻 WHERE 有効 = True ORDER BY ID"
        '' 現場名称の読込
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        With Me.DDLhour
            .DataSource = DtSet.Tables("DData")
            .DataMember = DtSet.DataSetName
            .DataTextField = "表示"
            .DataValueField = "値"
            .DataBind()

            Dim RowCount As Integer = DtSet.Tables("DData").Rows.Count - 1
            Dim iloop As Integer

            For iloop = 0 To RowCount
                If DtSet.Tables("DData").Rows(iloop).Item(4) = True Then
                    .SelectedIndex = iloop
                    Exit For
                End If
            Next
        End With

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
        TblProp.DateFormat = CType(ViewState("DteFormat"), String)              '日付の表示フォーマット         String
        TblProp.PaperOrientaion = CType(ViewState("PaOrient"), Integer)         '紙の向き                       Integer
        TblProp.TitleFont = CType(ViewState("TtlFont"), Integer)                'タイトルフォントのサイズ       Integer
        TblProp.SubTitleFont = CType(ViewState("SubTtlFont"), Integer)          'サブタイトルフォントのサイズ   Integer
        TblProp.DataFont = CType(ViewState("DatFont"), Integer)                 'データのフォントのサイズ       Integer
        TblProp.outCh = CType(ViewState("outCh"), String)                       '出力チャンネル情報             String
        TblProp.DataFileNo = CType(ViewState("DatFileNo"), Integer)             'データファイル番号             Integer
        TblProp.TruncFlg = CType(ViewState("Trunc"), Boolean)                   '表示桁以下の切り捨て           Boolean

        ReDim TblProp.Sensor(TblProp.ColCount - 1)                  '個々の列の情報を格納する配列設定

        For iloop = 0 To TblProp.ColCount - 1
            strCnt = iloop.ToString
            TblProp.Sensor(iloop).TableHeader = ViewState("TableHd" & strCnt)                   '列タイトル String
            TblProp.Sensor(iloop).wordAlign = ViewState("wAlign" & strCnt)                      'センサ名   文字横配置
            TblProp.Sensor(iloop).ShowFormat = CType(ViewState("ShowFmt" & strCnt), String)     '表示形式       String

            TblProp.Sensor(iloop).DateFieldWidth = CType(ViewState("FieldWid" & strCnt), Integer)   '列幅           Integer
            TblProp.Sensor(iloop).DataFontSize = CType(ViewState("FontSize" & strCnt), Single)      'フォントサイズ Single
        Next iloop

    End Sub

    Protected Sub CalcDate(ByVal setDateProperty As String)
        ''
        '' 選択されたラジオボタンから日付を検索
        ''

        Dim Interval As Double
        Dim DateType As String
        Dim endDate As Date
        Dim startDate As Date
        Dim clsCalc As New ClsReadDataFile


        If setDateProperty <> "99X" Then                                                                        '「任意」以外の場合
            Me.TxtStartDate.BackColor = Drawing.Color.LightGray

            'endDate = clsCalc.CalcEndDate(Me.RdBFromNewest.Checked, TblProp.LastUpdate, Me.TxtEndDate.Text, 1)

            Interval = Integer.Parse(setDateProperty.Substring(0, 2))
            DateType = setDateProperty.Substring(2, 1)

            Select Case DateType
                Case "A"
                    ''startDate = CType(Session.Item("OldestDate"), String)                                     '2009/02/02 Kino Changed
                    startDate = CType(Session.Item("OldestDate" & TblProp.DataFileNo.ToString), Date)           '2009/02/02 Kino Add
                Case Else                                                       '指定日数、月、年

                    startDate = clsCalc.CalcStartDate(Me.TxtStartDate.Text, endDate, DateType, Interval, 1)
            End Select

            Me.TxtStartDate.Text = startDate.ToString("d")

        End If

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
        ViewState("DteFormat") = TblProp.DateFormat             '日付の表示フォーマット         String
        ViewState("PaOrient") = TblProp.PaperOrientaion         '紙の向き                       Integer
        ViewState("TtlFont") = TblProp.TitleFont                'タイトルフォントのサイズ       Integer
        ViewState("SubTtlFont") = TblProp.SubTitleFont          'サブタイトルフォントのサイズ   Integer
        ViewState("DatFont") = TblProp.DataFont                 'データのフォントのサイズ       Integer
        ViewState("outCh") = TblProp.outCh                      '出力チャンネル情報             String
        ViewState("DatFileNo") = TblProp.DataFileNo             'データファイル番号             Integer
        ViewState("Trunc") = TblProp.TruncFlg                   '表示桁以下の切り捨て           Boolean

        For iloop = 0 To TblProp.ColCount - 1
            strCnt = iloop.ToString
            ViewState("TableHd" & strCnt) = TblProp.Sensor(iloop).TableHeader       '列タイトル String
            ViewState("wAlign" & strCnt) = TblProp.Sensor(iloop).wordAlign          'センサ名   文字横配置
            ViewState("ShowFmt" & strCnt) = TblProp.Sensor(iloop).ShowFormat        '表示形式       String
            ViewState("FieldWid" & strCnt) = TblProp.Sensor(iloop).DateFieldWidth   '列幅           Integer
            ViewState("FontSize" & strCnt) = TblProp.Sensor(iloop).DataFontSize     'フォントサイズ Single
        Next iloop

    End Sub

    Protected Sub SetTableTitle(ByVal OldFlg As Boolean)

        Dim TblHeight As Integer
        Dim TblWidth As Integer
        Dim strAddWord As String = "'"

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
        Me.GrdSummary.PageSize = TblProp.RowCount

        With Me.GrdSummary                                              'グラフサイズの調整
            Select Case TblProp.PaperOrientaion
                Case 0      'A4縦の場合
                    TblHeight = 902 '912
                    TblWidth = 640 '649
                    Me.TableOuter.Attributes.Add("class", "GraphOuterPortrait")                     ' 2016/01/19 Kino Add ページ高さ設定
                Case 1      'A4横の場合
                    TblHeight = 590
                    TblWidth = 960
                    Me.TableOuter.Attributes.Add("class", "GraphOuterLandscape")                    ' 2016/01/19 Kino Add ページ高さ設定
                Case 2      'A3縦の場合
                Case 3      'A3横の場合
            End Select

            .Height = TblHeight                                         'GridViewの高さ
            .Width = TblWidth                                           'GridViewの幅
        End With
        '================================================ [ 表サイズ・表示設定 ] ================================================↑

        If Me.RdBtnList.SelectedValue = 0 Then              '日報
            strAddWord = "　集計日報"
        Else
            strAddWord = "　集計月報"
        End If

        '---グラフタイトル 表示位置、フォントサイズ設定---
        Dim objLabel As System.Web.UI.WebControls.Label
        ''Me.LitSpace.Text = "<img src='./img/space_S.gif'>"
        If TblProp.TitlePosition = 0 Then           '上
            'Me.LitUpper.Text = "<br /><img src='./img/space_S.gif'><br />"        '"<br /><br />"
            'Me.LitLower.Text = ""
            Me.GrdSummary.CssClass = "TitleMarginTop"
            objLabel = Me.LblTitleUpper
            'Me.LblTitleUpper.Visible = True
            'Me.LblTitleLower.Visible = False
        Else                                        '下
            'Me.LitUpper.Text = ""
            'Me.LitLower.Text = "<br /><img src='./img/space_M.gif'>"        '"<br />"
            Me.GrdSummary.CssClass = "TitleMarginBottom"
            objLabel = Me.LblTitleLower
            'Me.LblTitleUpper.Visible = False
            'Me.LblTitleLower.Visible = True
        End If

        Dim AdjValue As Integer
        If OldFlg = True Then AdjValue = -3 ' 2011/04/19 Kino Add      現行の設定と合わせるため
        objLabel.Text = TblProp.TableTitle + strAddWord
        objLabel.Font.Size = TblProp.TitleFont + AdjValue          ' 2011/04/19 Kino Add  　現行の設定と合わせるため
        objLabel.Width = TblWidth
        Me.LblDateSpan.Width = TblWidth
        'Me.lblCalcExp.Width = TblWidth

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
        Dim strOption As String = ""
        Dim intTOP As Integer = 0
        Dim intInterval As Single = 1
        Dim strRangeType As String = ""
        Dim clsNewDate As New ClsReadDataFile
        Dim outCh() As Integer = {}
        Dim stDate As Date
        Dim edDate As Date
        Dim sqlOpt As String
        Dim Datas() As Array = {}
        Dim DispData As Single
        Dim AveTime As Long
        Dim strDateFormat As String
        'Dim AveWord As String = "時間平均"
        Dim Calcs As New ClsReadDataFile

        Me.GrdSummary.AutoGenerateColumns = True
        DataFile = IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", "MenuInfo.mdb")
        intRet = clsNewDate.GetDataFileNames(DataFile, DataFileNames)                           'データファイル名、共通情報ファイル名、識別名を取得

        Dim AccDataFile As String = IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", DataFileNames(TblProp.DataFileNo).CommonInf) ''siteDirectory & "_CalculatedData.mdb")

        Dim strRet As String = clsNewDate.GetOutputChannel(outCh, TblProp.outCh, 0)
        sqlOpt = "DataBaseCh IN("
        For iloop = 0 To outCh.Length - 1
            '' sqlOpt += outCh(iloop).ToString + ","
            sqlOpt &= String.Format("{0},", outCh(iloop).ToString)
        Next iloop
        ''sqlOpt = sqlOpt.Substring(0, sqlOpt.Length - 1) & ")"
        sqlOpt = sqlOpt.TrimEnd(Convert.ToChar(",")) & ")"

        If Me.RdBtnList.SelectedValue = 0 Then              '日報
            edDate = Date.Parse(Me.TxtStartDate.Text + " " + Me.DDLhour.SelectedValue)
            stDate = edDate.AddDays(-1)
            strDateFormat = "yyyy/MM/dd HH:mm"
            ''Me.lblCalcExp.Text = "開始値：開始日時以降の最初の測定時刻　終了値：終了日時以前の最後の測定時刻　　変動値 = 終了値 - 開始値"
        Else                                                '月報
            edDate = Date.Parse(Me.TxtStartDate.Text + " 00:00")
            stDate = edDate.AddMonths(-1)
            strDateFormat = "yyyy/MM/dd"
            ''Me.lblCalcExp.Text = "開始値：開始日以降の最初の測定時刻　終了値：終了日時以前の最後の測定時刻　　変動値 = 終了値 - 開始値"
            'AveWord = "日平均"
        End If
        ''Me.lblCalcExp.Text = "変動値 = 終了値 - 開始値"
        If Me.RdBtnList.SelectedValue = 0 Then              '日報
            AveTime = DateDiff(DateInterval.Hour, stDate, edDate)                       '平均演算の時間
        Else
            AveTime = DateDiff(DateInterval.Day, stDate, edDate)                        '平均演算の日数
        End If

        Me.LblDateSpan.Text = String.Format("開始日時：{0} ～ 終了日時：{1}", stDate.ToString(strDateFormat), edDate.AddMinutes(-1).ToString(strDateFormat))

        '--------------------------

        Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim DbCon As New OleDb.OleDbConnection
        Dim sb As New StringBuilder
        sb.Append("SELECT ")

        'strSQL = "SELECT 共通情報.タイトル AS 計器種別, 共通情報.計器記号, 共通情報.出力単位 AS 単位, 共通情報.初期値 AS 開始値, " + _
        '        "共通情報.初期値 AS 終了値, 共通情報.初期値 AS 変動値, 共通情報.初期値 AS " + AveWord + " FROM 共通情報" + _
        '        " where " + sqlOpt

        ' 2018/03/27 Kino Add 固定列から、変動列に変更
        For iloop = 0 To TblProp.ColCount - 1
            Select Case TblProp.Sensor(iloop).TableHeader
                Case "計器種別"
                    sb.Append("共通情報.タイトル AS 計器種別,")
                Case "計器記号"
                    sb.Append("共通情報.計器記号,")
                Case "単位"
                    sb.Append("共通情報.出力単位 AS 単位,")
                Case "開始値"
                    sb.Append("共通情報.初期値 AS 開始値,")
                Case "終了値"
                    sb.Append("共通情報.初期値 AS 終了値,")
                Case "変動値"
                    sb.Append("共通情報.初期値 AS 変動値,")
                Case "平均値"
                    ''sb.Append(String.Format("共通情報.初期値 AS {0},", AveWord))
                    sb.Append("共通情報.初期値 AS 平均値,")
                Case "最大値"
                    sb.Append("共通情報.初期値 AS 最大値,")
                Case "最小値"
                    sb.Append("共通情報.初期値 AS 最小値,")
            End Select
        Next

        strSQL = sb.ToString
        strSQL = strSQL.TrimEnd(Convert.ToChar(","))
        strSQL &= String.Format(" FROM 共通情報 where {0}", sqlOpt)
        sb.Length = 0

        DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & AccDataFile & ";" & "Jet OLEDB:Engine Type= 5"
        ''DbCon.Open()
        ''clsNewDate.GetSQLString(TblProp.ColCount - 1, dataCh, startDate, endDate, TimeCond, , strOption, intTOP)
        DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
        DbDa.Fill(DtSet, "DData")
        ''DbCon.Close()

        Dim DataCount As Integer = DtSet.Tables("DData").Rows.Count - 1

        '---データセットのデータを作成し書き込み---　開始値、最終値、変動値(終了値－開始値)、時間平均

        AccDataFile = IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", DataFileNames(TblProp.DataFileNo).FileName)

        '描画データの読み込み
        intRet = clsNewDate.GetDrawData(AccDataFile, DataCount, outCh, stDate, edDate.AddMinutes(-1), Datas)

        Dim stIndex As Integer = searchHeder("開始値")
        Dim edIndex As Integer = searchHeder("終了値")
        Dim diffIndex As Integer = searchHeder("変動値")
        Dim aveIndex As Integer = searchHeder("平均値")
        Dim maxIndex As Integer = searchHeder("最大値")
        Dim minIndex As Integer = searchHeder("最小値")
        Dim MaxData(DataCount) As Single
        Dim MinData(DataCount) As Single

        If intRet <> -1 Then                                                'データが１つでもあれば次の処理を行う
            For iloop = 0 To DataCount
                DtSet.Tables("DData").Rows(iloop).Item(stIndex) = Datas(0).GetValue(iloop + 1)                '開始日時
                DtSet.Tables("DData").Rows(iloop).Item(edIndex) = Datas(intRet).GetValue(iloop + 1)           '終了日時
                DispData = 7.7E+30
                If Datas(0).GetValue(iloop + 1) < 1.1E+30 And Datas(intRet).GetValue(iloop + 1) < 1.1E+30 Then
                    '四捨五入や計算誤差による表示数値の誤差をなくすため、桁を調整した後で計算を行う
                    ''DispData = Single.Parse(Datas(intRet).GetValue(iloop + 1)).ToString(TblProp.Sensor(3).ShowFormat) _
                    ''         - Single.Parse(Datas(0).GetValue(iloop + 1)).ToString(TblProp.Sensor(3).ShowFormat)         '変動値(開始日時-終了日時)
                    DispData = Calcs.trunc_round(Datas(intRet).GetValue(iloop + 1), TblProp.Sensor(3).ShowFormat, TblProp.TruncFlg) _
                             - Calcs.trunc_round(Datas(0).GetValue(iloop + 1), TblProp.Sensor(3).ShowFormat, TblProp.TruncFlg)         '変動値(開始日時-終了日時)       ' 2016/02/16 Kino Add

                    If diffIndex > 0 Then
                        ''DtSet.Tables("DData").Rows(iloop).Item(5) = DispData                                '変動値
                        DtSet.Tables("DData").Rows(iloop).Item(diffIndex) = DispData                                '変動値5
                    End If

                    ' ''DtSet.Tables("DData").Rows(iloop).Item(6) = DispData / AveTime                      '時間平均値
                    ''If aveIndex > 0 Then
                    ''    DtSet.Tables("DData").Rows(iloop).Item(aveIndex) = DispData / AveTime                       '時間平均値6      ' 集計関数で平均値を算出するように変更
                    ''End If
                Else
                    ' ''DtSet.Tables("DData").Rows(iloop).Item(5) = 7.7E+30                                 '変動値 　　 異常値
                    ' ''DtSet.Tables("DData").Rows(iloop).Item(6) = 7.7E+30                                 '時間平均値　異常値

                    If diffIndex > 0 Then
                        DtSet.Tables("DData").Rows(iloop).Item(diffIndex) = 7.7E+30                                 '変動値 　　 異常値5
                    End If
                    ''If aveIndex > 0 Then
                    ''    DtSet.Tables("DData").Rows(iloop).Item(aveIndex) = 7.7E+30                                  '時間平均値　異常値
                    ''End If
                End If

            Next iloop

            ' 最大値、最小値を取得・格納
            If DbCon IsNot Nothing Then DbCon.Dispose()
            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & AccDataFile & ";" & "Jet OLEDB:Engine Type= 5")
            If maxIndex > 0 Or aveIndex > 0 Then
                For iloop = 0 To DataCount
                    Dim sb2 As New StringBuilder
                    Dim dm As Integer = Convert.ToInt32(Math.Truncate(outCh(iloop) * 0.01) * 100)
                    Dim Table As String = String.Format("C{0:D4}_C{1:D4}", dm + 1, dm + 100)
                    'Dim Table As String = ("C" + (dm + 1).ToString("0000") & "_C" & (dmdm + 100).ToString("0000"))
                    sb2.Append("SELECT ")
                    sb2.Append(String.Format("MAX({0}.Ch{1}) AS max{2},MIN({0}.Ch{1}) AS min{2},AVG({0}.Ch{1}) AS ave{2}", Table, outCh(iloop), iloop.ToString))
                    sb2.Append(String.Format(" FROM 日付 INNER JOIN {0} ON 日付.日付ID={0}.日付ID WHERE", Table))
                    sb2.Append(String.Format(" {0}.Ch{1} < 1.1E+30 AND", Table, outCh(iloop)))
                    sb2.Append(String.Format(" (日付.日付 BETWEEN #{0}# AND #{1}#)", stDate, edDate.AddMinutes(-1)))
                    strSQL = sb2.ToString
                    Diagnostics.Debug.WriteLine(strSQL)
                    Dim DtSet2 As DataSet = Nothing
                    Dim DbDa2 As OleDb.OleDbDataAdapter = Nothing
                    Using DbDa2
                        Using DtSet2
                            DbDa2 = New OleDb.OleDbDataAdapter(strSQL, DbCon)
                            DtSet2 = New DataSet("DData")
                            DbDa2.Fill(DtSet2, "DData")
                            Dim sngTemp As Single = 7.7E+30
                            If maxIndex > 0 Then
                                If IsDBNull(DtSet2.Tables("DData").Rows(0).Item(0)) = False Then
                                    sngTemp = Convert.ToSingle(DtSet2.Tables("DData").Rows(0).Item(0))
                                    If sngTemp < 1.1E+30 Then
                                        sngTemp = Calcs.trunc_round(sngTemp, TblProp.Sensor(maxIndex).ShowFormat, TblProp.TruncFlg)
                                    End If
                                End If
                                DtSet.Tables("DData").Rows(iloop).Item(maxIndex) = sngTemp
                            End If
                            sngTemp = 7.7E+30
                            If minIndex > 0 Then
                                If IsDBNull(DtSet2.Tables("DData").Rows(0).Item(1)) = False Then
                                    sngTemp = Convert.ToSingle(DtSet2.Tables("DData").Rows(0).Item(1))
                                    If sngTemp < 1.1E+30 Then
                                        sngTemp = Calcs.trunc_round(sngTemp, TblProp.Sensor(minIndex).ShowFormat, TblProp.TruncFlg)
                                    End If
                                End If
                                DtSet.Tables("DData").Rows(iloop).Item(minIndex) = sngTemp
                            End If
                            sngTemp = 7.7E+30
                            If aveIndex > 0 Then
                                If IsDBNull(DtSet2.Tables("DData").Rows(0).Item(2)) = False Then
                                    sngTemp = Convert.ToSingle(DtSet2.Tables("DData").Rows(0).Item(2))
                                    If sngTemp < 1.1E+30 Then
                                        sngTemp = Calcs.trunc_round(sngTemp, TblProp.Sensor(aveIndex).ShowFormat, TblProp.TruncFlg)
                                    End If
                                End If
                                DtSet.Tables("DData").Rows(iloop).Item(aveIndex) = sngTemp
                            End If

                        End Using
                    End Using
                Next
            End If
            DbCon.Dispose()

            '---GridViewにデータセットをバインドする---
            With Me.GrdSummary
                .AllowPaging = TblProp.PagingEnable                                                     '2010/08/25
                .BorderColor = Color.Black
                .DataSource = DtSet.Tables("DData")
                .DataMember = DtSet.DataSetName
                .DataBind()
            End With
        Else                                                                'データが1つもなかった場合はその旨を表示する
            Me.LblDateSpan.Text += "  <- この期間にデータは存在しません。"
        End If
        ''joinCells(Me.GrdSummary, 0)       ''列の縦結合　　　・・・　ページングすると結合がなくなってしまう・・・　orz

        DbDa.Dispose()
        DbCom.Dispose()
        DtSet.Dispose()
        Calcs = Nothing

    End Sub

    ''' <summary>
    ''' Tblpropのセンサー情報から、合致したヘッダーのインデックスを取得
    ''' </summary>
    ''' <param name="strHeader">取得するヘッダー名</param>
    ''' <returns>取得したインデックス　見つからない場合は -1</returns>
    ''' <remarks></remarks>
    Protected Function searchHeder(ByVal strHeader As String) As Integer

        Dim intRet As Integer = -1
        Dim iloop As Integer

        For iloop = 0 To TblProp.ColCount - 1
            If TblProp.Sensor(iloop).TableHeader = strHeader Then
                intRet = iloop
                Exit For
            End If
        Next

        Return intRet

    End Function

    ''この方法では、ページングした時に結合されないので×
    Sub joinCells(ByVal gv As GridView, ByVal column As Integer)
        ''同列内で同じデータの場合縦に結合する
        Dim numRow As Integer = gv.Rows.Count
        Dim B As Integer = 0
        Dim N As Integer = 0

        Dim baseCell As TableCell = New TableCell
        Dim nextCell As TableCell = New TableCell
        '***************************************************       
        While B < numRow - 1

            N = B + 1
            baseCell = gv.Rows(B).Cells(column)

            While N < numRow

                nextCell = gv.Rows(N).Cells(column)
                If baseCell.Text = nextCell.Text Then
                    If baseCell.RowSpan = 0 Then
                        baseCell.RowSpan = 2
                        'baseCell.CssClass = "GridJoined"
                    Else
                        baseCell.RowSpan = baseCell.RowSpan + 1
                        'baseCell.CssClass = "GridJoined"
                    End If
                    gv.Rows(N).Cells.Remove(nextCell)
                    'gv.Rows(N).Cells(column).Visible = False
                    N = N + 1
                Else
                    Exit While
                End If
            End While

            B = N
            '***
        End While

    End Sub


    Protected Sub GrdSummary_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles GrdSummary.DataBound
        ''    '' GrdSummary_RowDataBound の方が先に実行される

        Call SetGridFormat()                                            'グリッドの書式設定     ここにしないと、ページングしたときに最後のページで空行が挿入されない

    End Sub

    'Protected Sub GrdTimeSeries_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GrdTimeSeries.PreRender
    '    ''
    '    '' グリッドビューの読み込み後、表示を開始する前に発生するイベント
    '    ''

    '    'Me.GrdTimeSeries.Sort("日付", SortDirection.Ascending)

    'End Sub

    Protected Sub SetGridFormat()
        ''
        '' グリッドの全般的な書式設定
        ''

        Dim iloop As Integer
        Dim intTemp As Integer
        Dim jloop As Integer

        With Me.GrdSummary
            .BorderStyle = WebControls.BorderStyle.Solid
            .BorderWidth = 1

            ''.Font.Size = TblProp.DataFont
            ''.HeaderRow.BorderColor = Color.Black
            '.HeaderRow.Cells(0).Width = TblProp.DateFieldWidth
            .HeaderRow.Font.Size = TblProp.SubTitleFont
            .HeaderRow.BorderStyle = WebControls.BorderStyle.Solid
            .HeaderRow.BorderWidth = 1
            ''.HeaderRow.Font.Bold = False
            .HeaderRow.Height = TblProp.SubTitleFont + 10
            .PagerStyle.HorizontalAlign = HorizontalAlign.Center

            '行が足りない場合は、空行で埋める
            intTemp = (TblProp.RowCount - .Rows.Count)
            If intTemp <> 0 Then
                For iloop = 1 To intTemp
                    'Dim GridViewRow = New GridViewRow(-1, -1, DataControlRowType.DataRow, DataControlRowState.Normal)
                    'Dim TableCell = New TableCell()
                    '.Controls.Add(GridViewRow)

                    Dim row1 As New GridViewRow(-1, -1, DataControlRowType.DataRow, DataControlRowState.Normal)
                    For jloop = 1 To TblProp.ColCount '+ 1
                        Dim cell11 As New TableCell
                        ''cell11.Font.Size = TblProp.DataFont
                        cell11.HorizontalAlign = HorizontalAlign.Right                      ''Center 2009/01/29 Kino Changed
                        cell11.Text = "-"
                        cell11.BorderColor = Color.Black
                        row1.Cells.Add(cell11)
                        row1.Height = Me.GrdSummary.RowStyle.Height
                        row1.BackColor = Color.White
                        .Controls(0).Controls.AddAt(.Rows.Count + 1, row1)
                        cell11.BorderWidth = 1
                    Next jloop

                Next iloop
            End If
            '.Font.Size = TblProp.DataFont

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

    Protected Sub GrdSummary_RowCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdSummary.RowCreated

        With e.Row
            If .RowType = DataControlRowType.DataRow Then
                '' マウスオーバーで行に色を設定
                ' onmouseover属性を設定　　'#CC99FF'
                .Attributes("onmouseover") = "setBg(this, '#84D7FF')"

                ' データ行が通常行／代替行であるかで処理を分岐（2）
                If .RowState = DataControlRowState.Normal Then
                    .Attributes("onmouseout") = _
                      String.Format("setBg(this, '{0}')", _
                        ColorTranslator.ToHtml(Me.GrdSummary.RowStyle.BackColor))
                Else
                    .Attributes("onmouseout") = _
                      String.Format("setBg(this, '{0}')", _
                        ColorTranslator.ToHtml( _
                          Me.GrdSummary.AlternatingRowStyle.BackColor))
                End If
                .BorderWidth = 1
                .Height = TblProp.SubTitleFont + 5
            End If
        End With

    End Sub


    Protected Sub GrdSummary_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GrdSummary.RowDataBound
        ''
        '' グリッドのセル単位の書式設定
        ''
        Dim iloop As Integer
        Dim sngData As Single
        Dim dteDate As Date
        Dim DummyCount As Short = 0
        Dim colCount As Integer
        Dim Calcs As New ClsReadDataFile

        colCount = TblProp.ColCount                                                                     '2009/01/29 Kino Changed これにした

        With e.Row
            If .RowType = DataControlRowType.DataRow Then                                   'データ
                For iloop = 0 To .Cells.Count - 1
                    .Cells(iloop).Width = TblProp.Sensor(iloop).DateFieldWidth
                Next

                For iloop = 0 To .Cells.Count - 1
                    .Cells(iloop).BorderWidth = 1
                    .Cells(iloop).BorderStyle = WebControls.BorderStyle.Solid
                    .Cells(iloop).BorderColor = Drawing.Color.Black
                    .Cells(iloop).Font.Size = CSng(TblProp.Sensor(iloop).DataFontSize)

                    If iloop >= 0 And iloop <= 2 Then
                        If Date.TryParse(.Cells(iloop).Text, dteDate) = True Then
                            dteDate = Date.Parse(.Cells(iloop).Text)
                            .Cells(iloop).Text = dteDate.ToString(TblProp.DateFormat)
                        End If
                    Else
                        If Single.TryParse(e.Row.Cells(iloop).Text, sngData) Then
                            If sngData < 1.1E+30 And TblProp.Sensor(iloop).ShowFormat = "-" Then                '2008/08/20 Kino ダミー列用
                                .Cells(iloop).Text = "-"
                            ElseIf sngData < 1.1E+30 Then
                                ''.Cells(iloop).Text = String.Format("{0:F2}", sngData)
                                '.Cells(iloop).Text = sngData.ToString(TblProp.Sensor(iloop).ShowFormat)

                                ''.Cells(iloop).Text = sngData.ToString(TblProp.Sensor(iloop).ShowFormat)
                                .Cells(iloop).Text = Calcs.trunc_round(sngData, TblProp.Sensor(iloop).ShowFormat, TblProp.TruncFlg)      ' 2016/02/16 Kino Add                            Else
                            Else
                                .Cells(iloop).Text = "--"
                            End If
                        Else
                            .Cells(iloop).HorizontalAlign = HorizontalAlign.Left
                        End If
                        ''.Cells(iloop).HorizontalAlign = HorizontalAlign.Right
                        ''.Cells(iloop).Width = intTemp
                        ''Else
                        ''    .Cells(iloop).Width = 0
                        ''    .Cells(iloop).Text = ""
                    End If
                    .Cells(iloop).HorizontalAlign = Convert.ToInt32(TblProp.Sensor(iloop).wordAlign)
                Next iloop

                'Dim NowRow As Integer = Me.GrdSummary.Rows.Count - 1
                'If NowRow >= 2 Then
                '    If Me.GrdSummary.Rows(NowRow - 1).Cells(0).Text = .Cells(0).Text Then
                '        If NowRow = 2 Then
                '            .Cells(0).RowSpan = 2
                '        Else
                '            .Cells(0).RowSpan = .Cells(0).RowSpan + 1
                '        End If

                '    End If
                '    preText = .Cells(0).Text
                'End If

            ElseIf .RowType = DataControlRowType.Header Then                               'ヘッダー

                For iloop = 0 To .Cells.Count - 1
                    .Cells(iloop).Font.Size = TblProp.SubTitleFont
                    .Cells(iloop).BorderColor = Drawing.Color.Black
                    ''If iloop > 0 Then .Cells(iloop).Text = TblProp.Sensor(iloop - 1).SensorSymbol
                Next iloop

                'ElseIf .RowType = DataControlRowType.Pager Then
                '    Me.GrdSummary.PagerStyle.Width = Me.GrdSummary.Width

            Else                                                                        'データ、ヘッダー以外
                ''    For iloop = 0 To .Cells.Count - 1
                ''        .Cells(iloop).BorderColor = Drawing.Color.Black
                ''        If iloop <> 0 Then                                                  '2009/02/02 平均値、ページャのセンタリングのため
                ''            .Cells(iloop).Width = intTemp
                ''        End If
                ''    Next iloop
                .Height = TblProp.SubTitleFont + 5
                Me.GrdSummary.PagerStyle.Width = Me.GrdSummary.Width
            End If

        End With

    End Sub

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
                TblProp.PointName = .GetValue(3).ToString               '測点名称
                TblProp.RowCount = Integer.Parse(.GetValue(4))          '行数
                TblProp.ColCount = Integer.Parse(.GetValue(5))          '列数
                TblProp.TableTitle = .GetValue(6).ToString              '表のタイトル
                TblProp.TableSubTitle = .GetValue(7).ToString           '表のサブタイトル
                TblProp.TitlePosition = Short.Parse(.GetValue(8))       'タイトルの位置(上or下)
                TblProp.TitleFont = Integer.Parse(.GetValue(9))         'タイトルフォントのサイズ
                TblProp.SubTitleFont = Integer.Parse(.GetValue(10))     'サブタイトルフォントのサイズ
                TblProp.DataFont = Integer.Parse(.GetValue(11))         'データのフォントのサイズ
                TblProp.outCh = .GetValue(12).ToString                  '出力するチャンネル番号
                TblProp.DataFileNo = Integer.Parse(.GetValue(13))       'データファイル番号
                TblProp.PagingEnable = Boolean.Parse(.GetValue(14))     'ページングするかどうか
                TblProp.TruncFlg = Boolean.Parse(.GetValue(16))         '表示桁以下の切り捨て有無
            End With
        Next

        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        .Read()                                             '１レコード分読み込み
        ''        TblProp.PointName = .GetString(3)                   '測点名称
        ''        TblProp.RowCount = .GetInt16(4)                     '行数
        ''        TblProp.ColCount = .GetInt16(5)                     '列数
        ''        TblProp.TableTitle = .GetString(6)                  '表のタイトル
        ''        TblProp.TableSubTitle = .GetString(7)               '表のサブタイトル
        ''        TblProp.TitlePosition = CType(.GetInt16(8), Short)  'タイトルの位置(上or下)
        ''        TblProp.TitleFont = .GetInt16(9)                    'タイトルフォントのサイズ
        ''        TblProp.SubTitleFont = .GetInt16(10)                'サブタイトルフォントのサイズ
        ''        TblProp.DataFont = .GetInt16(11)                    'データのフォントのサイズ
        ''        TblProp.outCh = .GetString(12)                      '出力するチャンネル番号
        ''        TblProp.DataFileNo = .GetInt16(13)                  'データファイル番号
        ''        TblProp.PagingEnable = .GetBoolean(14)              'ページングするかどうか
        ''    End If
        ''End With
        ''DbDr.Close()
        ''DbCom.Dispose()

        DbDa.Dispose()
        DtSet.Dispose()

        ReDim TblProp.Sensor(TblProp.ColCount - 1)                  '個々の列の情報を格納する配列設定

        ReadTableInfoFromDB = 1

    End Function

    Protected Function TableColumnInfoFromDB(ByVal dbCon As OleDb.OleDbConnection) As Integer
        ''
        '' 項目名におけるページに表示する列の情報
        ''
        '' Dim DbDr As OleDb.OleDbDataReader
        '' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strSQL As String
        Dim iloop As Integer

        strSQL = "SELECT * FROM [" & TblProp.WindowTitle & "＿列情報] ORDER BY ID ASC;"
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        With DtSet.Tables("DData")                '表ヘッダ
            If .Rows.Count > 0 Then
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).TableHeader = .Rows(0).Item(iloop).ToString
                Next

                '文字配置
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).wordAlign = .Rows(1).Item(iloop).ToString
                Next

                '表示形式
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).ShowFormat = .Rows(2).Item(iloop).ToString
                Next

                '列幅
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).DateFieldWidth = Integer.Parse(.Rows(3).Item(iloop))
                Next

                'データ行フォントサイズ
                For iloop = 1 To TblProp.ColCount
                    TblProp.Sensor(iloop - 1).DataFontSize = Single.Parse(.Rows(4).Item(iloop))
                Next
            End If
        End With


        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        '表ヘッダ
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).TableHeader = .GetString(iloop)
        ''        Next

        ''        '文字配置
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).wordAlign = .GetString(iloop)
        ''        Next

        ''        '表示形式
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).ShowFormat = .GetString(iloop)
        ''        Next

        ''        '列幅
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).DateFieldWidth = CInt(.GetString(iloop))
        ''        Next

        ''        'データ行フォントサイズ
        ''        .Read()
        ''        For iloop = 1 To TblProp.ColCount
        ''            TblProp.Sensor(iloop - 1).DataFontSize = CSng(.GetString(iloop))
        ''        Next

        ''    End If
        ''    Return 1
        ''End With
        ''DbDr.Close()
        ''DbCom.Dispose()

        DbDa.Dispose()
        DtSet.Dispose()

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

End Class
