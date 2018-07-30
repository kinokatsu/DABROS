Option Strict On
Option Explicit On

Imports System.Data.OleDb
Imports C1.Win.C1Chart
Imports C1.Web.C1WebChart
Imports System.Data
Imports System.Drawing
Imports System.IO

Partial Class Graph_TimeChart
    Inherits System.Web.UI.Page

    'グラフデータの情報
    Private Structure LineProperties
        Dim Symbol As String                    '記号
        Dim ChNo As Integer                     'データチャンネル
        Dim Color As Integer                    '線色
        Dim LineType As Short                   '線種
        Dim LineWidth As Integer                  '線幅
        Dim Marker As Short                     'マーカ
        Dim MarkSize As Short                   'マーカサイズ
        Dim MarkOutlineWidth As Short           'マーカアウトライン幅　これが0でない場合は白抜きとする　　0なら塗りつぶし
    End Structure

    '軸の情報
    Private Structure AxisPropaerties
        Dim DefaultSet As Boolean               'スケール既定値か入力か
        Dim DefaultValue As Integer             '既定値の場合の値
        Dim UnitTitle As String                 '単位タイトル
        Dim DataTitle As String                 '軸タイトル
        Dim Max As Single                       '最大値
        Dim Min As Single                       '最小値
        Dim UnitMajor As Single                 '主メモリ間隔
        Dim UnitMinor As Single                 '副メモリ間隔
        Dim DataCount As Short                  'データ系列数
        Dim ChartType As String                 'グラフ種別
        Dim Reversed As Boolean                 '軸反転     False:しない　　True:する
        Dim LogAxis As Boolean                  '縦軸(データ軸)の対数スケール　False:しない　True:する      ' 2011/12/21 Kino Add
        ''Dim ValueFormat As String               '縦軸スケールのフォーマット文字列                           ' 2012/08/02 Kino Add
    End Structure

    'グラフ描画における個々のグラフ情報
    Private Structure GrpPersonalizedInfo
        Dim LeftAxisData() As LineProperties    '左軸グラフ描画における線情報
        Dim RightAxisData() As LineProperties   '右軸グラフ描画における線情報
        Dim SubTitle As String                  'サブタイトル
        Dim EnableRightAxis As Boolean          '右軸有効
        Dim LeftAxis As AxisPropaerties         '左軸設定   
        Dim RightAxis As AxisPropaerties        '右軸設定
        Dim DataFileNo As Integer               'データファイル番号
        Dim spAlertValue As String              ' 個別警報値情報(以前サブタイトルを流用していた)          ' 2013/06/21 Kino Add
        Dim locationX As Integer                ' グラフのチャートエリアの起点位置　X座標                 ' 2013/06/21 Kino Add
        Dim locationY As Integer                ' グラフのチャートエリアの起点位置　Y座標                 ' 2013/06/21 Kino Add
        Dim widthRatio As Integer               ' グラフ横幅に対するチャートエリアの割合                  ' 2013/06/21 Kino Add
        Dim heightRatio As Integer              ' グラフ高さに対するチャートエリアの割合                  ' 2013/06/21 Kino Add
        Dim subTitleSize As Single              ' サブタイトルのフォントサイズ                            ' 2013/06/21 Kino Add
        Dim legendWidth As Integer              ' 凡例の横幅                                              ' 2013/06/21 Kino Add
        Dim alertType As Integer                ' 警報表示種別                                            ' 2013/07/17 Kino Add
        Dim showAlertValue As Boolean           ' 警報値数値表示                                          ' 2013/07/17 Kino Add
        Dim alertValuePosition As Integer       ' 警報値数値表示の場合の左から位置割合                    ' 2013/07/17 Kino Add
    End Structure

    Private Structure YDataSet                  ' 2016/10/25 Kino Add
        Dim Datas() As Double
    End Structure

    'グラフ描画全般の情報
    Private Structure GrpInfo
        Dim LineInfo() As GrpPersonalizedInfo   'グラフにおける線の情報

        Dim DrawSizeColorInf() As Integer       '全般
        '--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--

        Dim StartDate As Date                   'グラフ描画開始日
        Dim EndDate As Date                     'グラフ描画終了日
        Dim PaperOrientaion As Integer          '用紙の向き
        Dim XAxisTileAngle As Integer           'X軸タイトルの角度
        Dim DateFormat As String                '日付フォーマット
        Dim TimeScale As String                 'X軸表示期間
        Dim TitlePosition As Short              'グラフタイトルの位置(上or下)
        Dim GraphTitle As String                'グラフタイトル
        Dim WindowTitle As String               'タイトルバーのタイトル
        Dim GraphBoxCount As Integer            'グラフの枠数
        Dim GraphCount As Integer               '描画グラフ数
        Dim EnableLegend As Integer             '凡例の表示／非表示
        Dim LastUpdate As Date                  'データの最終更新日
        Dim MissingDataContinuous As Integer    '欠測データの連結
        Dim ShowAlarmArea As Integer            '警報値の表示
        Dim thinoutEnable As Boolean            '間引きチェックの有無
        Dim thinoutCh As String                 '間引きチャンネル情報
        Dim alertStyle As Integer               ' 管理値エリア塗りつぶし方法のスタイル　0:新しい方法　1:以前のまま
    End Structure

    Private GrpProp As GrpInfo                  'グラフ描画に関する情報
    Private DataFileNames() As ClsReadDataFile.DataFileInf
    Private AlertInfo(6) As ClsReadDataFile.AlertInf    ' 管理値エリア色 2015/05/16 Kino Add

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
        '' ページを閉じるときのイベント
        ''

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"

        If User.Identity.IsAuthenticated = False Then
            'Response.Redirect("Login.aspx",false)
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer
        LoginStatus = CType(Session.Item("LgSt"), Integer)          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            'Response.Redirect("Login.aspx",false)
            Response.Write(strScript)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする
        Response.Cache.SetNoStore()
        Response.Cache.SetExpires(DateTime.Now.AddDays(-1))
        ''<% Response.Expires = -1 
        ''        Response.AddHeader("Cache-Control", "No-Cache")
        ''        Response.AddHeader("Pragma", "No-Cache")
        ''%> KB927489

        '' '' '' これを使うとデータ点数が多いときにものすごく重くなるので使用する場合は要検討
        ''ClientScript.RegisterClientScriptBlock(GetType(String), "/js/Simple2D.min.js", "<script language=""javascript"" type=""text/javascript"" src=""" + Me.TemplateSourceDirectory + "js/Simple2D.js" + """></script>")
        ClientScript.RegisterClientScriptBlock(GetType(String), "/js/Simple2D.min.js", "<script language=""javascript"" type=""text/javascript"" src=""js/Simple2D.js""></script>")

        Dim siteName As String
        Dim siteDirectory As String
        Dim grpDataFile As String
        Dim intRet As Integer
        Dim OldestDate As Date
        Dim dteTemp As Date = Date.Parse("1900/01/01 0:00:00")
        Dim clsNewDate As New ClsReadDataFile
        Dim clsSetScript As New ClsGraphCommon
        Dim OldTerm As String

        OldTerm = CType(Session.Item("OldTerm"), String)            '過去データ表示制限
        siteName = CType(Session.Item("SN"), String)                            '現場名
        siteDirectory = CType(Session.Item("SD"), String)                       '現場ディレクトリ
        grpDataFile = Server.MapPath(siteDirectory + "\App_Data\TimeChartGraphInfo.mdb")
        OldestDate = CType(Session.Item("OldestDate"), Date)
        GrpProp.WindowTitle = CType(Request.Item("GNa"), String)        'CType(Session.Item("OutputName"), String)      'グラフタイトル（ウィンドウタイトル）
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)
        Dim OpenString As String = ""

        'セッション変数に保存しないようにする 2018/03/30 Kino
        Dim RemoteInf As String = Nothing ''= CType(Session("RemoteInfo"), String)
        Dim clsBr As New ClsHttpCapData
        RemoteInf = clsBr.GetRemoteInfo(Request, Server.MapPath(""))
        clsBr = Nothing
        If RemoteInf IsNot Nothing Then                                     ' 2012/08/24 Kino Add
            Dim remInf() As String = RemoteInf.Split(","c)
            If remInf(2).ToString = "IE 6.0 ie" Then
                Me.UpdPnlAnimEx.Enabled = False
            End If
        End If

        intRet = clsNewDate.GetDataFileNames(Path.Combine(Server.MapPath(siteDirectory), "App_Data", "MenuInfo.mdb"), DataFileNames)                        'データファイル名、共通情報ファイル名、識別名を取得
        ''intRet = clsNewDate.GetDataLastUpdate(Path.Combine(Server.MapPath(siteDirectory), "App_Data", "MenuInfo.mdb"), DataFileNames)                  '2010/05/24 Kino Add データの最古、最新日付を取得        ' 2018/06/05 Kino Changed 上で同じことをやっているためコメント
        Dim OldFlg As Boolean = IO.File.Exists(Server.MapPath(siteDirectory + "App_Data\Old.flg"))                          ' 2011/04/19 Kino Add 前の設定と調整するため

        If IsPostBack = False Then

            '' 2017/03/14 Kino Changed 不要のためコメント
            ''If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
            ''    ''==== フォームのサイズを調整する ====
            ''    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
            ''        'Dim OpenString As String

            ''        OpenString = "<SCRIPT LANGUAGE='javascript'>"                                                          ' 2015/11/24 Kino Changed 下に集約のため、変更
            ''        OpenString += ("window.resizeTo(" + wid + "," + Hei + ");")
            ''        OpenString += "<" + "/SCRIPT>"
            ''        'OpenString = ("window.resizeTo(" + wid + "," + Hei + ");")

            ''        'Dim instance As ClientScriptManager = Page.ClientScript                            'この方法から↓の方法へ変更した
            ''        'instance.RegisterClientScriptBlock(Me.GetType(), "経時変化図", OpenString)
            ''        Page.ClientScript.RegisterStartupScript(Me.GetType(), "経時変化図", OpenString)
            ''    End If
            ''    ''====================================
            ''End If

            '初回読み込みはデータベースから行なう
            Dim DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + grpDataFile + ";" + "Jet OLEDB:Engine Type= 5"
            ''DbCon.Open()

            intRet = ReadGraphInfoFromDB(DbCon, "経時変化図")                   'グラフ情報を取得

            Call SetOtherGraphs(DbCon, "経時変化図")     ', Integer.Parse(wid), Integer.Parse(Hei))      ' 2018/06/05 Kino Changed　サイズ指定をやめた
            intRet = GraphLineInfoFromDB(DbCon)                                 'グラフの軸タイトル、最大・最小スケールなどを取得
            Call GetMostUpdate()                                                'データファイルの最新日時を取得する

            intRet = GraphDataInfoFromDB(DbCon)                                 'グラフに表示するデータの情報を取得
            Call SetBIND2Controls(DbCon)                                        '各コントロールのリスト内容をDBから作成する

            ''DbCon.Close()
            DbCon.Dispose()

            Call getAlertColor(siteDirectory)                                   ' 2015/05/16 Kino Add
            ''Call SetScaleValue2DropDownList()                                 '既定スケールのコンボにDBをバインドする

            Call Set2FormControl()                                              'コントロールの初期状態の設定
            Call CalcDate(GrpProp.TimeScale)                                    '開始日付の計算

            ' 2011/02/01 Kino Add グラフを切り替えた時に前のグラフと同じ表示期間とするための処理
            If CType(Session.Item("GrpChg"), Integer) = 1 Then                      ' セッション変数でグラフ切り替えフラグが立っていた場合のみ処理

                Dim DataSpan As String = CType(Session.Item("DSP"), String)         ' データ表示種類
                Dim qStDate As String = CType(Session.Item("DS"), String)           ' 開始日付け
                Dim qEdDate As String = CType(Session.Item("DE"), String)           ' 最終日付け
                Dim qPartial As Integer = CType(Session.Item("PEn"), Integer)       ' 間引きあり／なし
                Dim qThinTime As String = CType(Session.Item("Thn"), String)        ' 間引き時間インデックス

                If DataSpan = "N" Then
                    Me.RdBFromNewest.Checked = True
                    Me.RdBDsignatedDate.Checked = False
                    Me.DDLRange.SelectedIndex = Integer.Parse(qStDate)
                ElseIf DataSpan = "D" Then
                    Me.RdBFromNewest.Checked = False
                    Me.RdBDsignatedDate.Checked = True
                    Me.TxtStartDate.Text = (qStDate.Substring(0, 4) + "/" + qStDate.Substring(4, 2) + "/" + qStDate.Substring(6, 2))
                    Me.TxtEndDate.Text = (qEdDate.Substring(0, 4) + "/" + qEdDate.Substring(4, 2) + "/" + qEdDate.Substring(6, 2))
                End If
                Session.Remove("GrpChg")                                            ' グラフ切り替えフラグのセッション変数を削除
                Session.Remove("DS")
                Session.Remove("DE")
                Session.Remove("PEn")
                Session.Remove("Thn")
                Me.ChbPartial.Checked = Convert.ToBoolean(qPartial)                                    ' 間引きあり／なし
                Dim DatCh() As String = {}
                Dim iloop As Integer
                DatCh = qThinTime.Split(","c)
                For iloop = 0 To DatCh.Length - 2
                    Me.CBLPartial.Items(Integer.Parse(DatCh(iloop))).Selected = True               ' 間引き時間のインデックス
                Next
            End If
            ''System.Diagnostics.Debug.WriteLine(Page.Request.QueryString.Item("DSp").ToString)

            ''Call CheckAutoUpdate(siteDirectory)                                 '自動更新設定の読み込み ↓
            Call clsSetScript.CheckAutoUpdate(Me.Form, Server.MapPath(siteDirectory + "\App_Data\MenuInfo.mdb"), "経時変化図")      '自動更新設定の読み込み
            Call Set2FormFormControl()                                          '変数の内容を各コントロールに配置する

            Call clsSetScript.SetSelectDateScript(Me.Form)                       'コントロールにJavaScriptを割り当て

            Dim OldDate As Date
            If OldTerm <> "None" Then
                Dim clsCalc As New ClsReadDataFile
                OldDate = clsCalc.CalcOldDateLimit(OldTerm)
                clsCalc = Nothing
            Else
                OldDate = DataFileNames(0).MostOldestDate
            End If
            Me.RngValidSt.MinimumValue = OldDate.ToString("yyyy/MM/dd")
            Me.RngValidSt.ErrorMessage = "開始日は" + OldDate.ToString("yyyy/MM/dd") + "以降としてください。"
            Me.C1WebCalendar.MinDate = OldDate
            ''Me.RngValidEd.MaximumValue = Date.Parse(GrpProp.LastUpdate.ToString("yyyy/MM/dd"))        ''日時スケールによってはMAXを多めにする場合もあるか・・・

            If Date.Parse(Me.TxtStartDate.Text) < OldDate Then Me.TxtStartDate.Text = OldDate.ToString("yyyy/MM/dd") ' 2012/02/29 Kino Add Validationのときに引っかかるのでデータの最古日付を入れるようにした

            Dim fno As String = getFileNo()
            Dim fdt As String = getFileDate(fno)

            Me.nwdt.Value = fdt                                                                 ' 2018/03/16 Kino Add
            Me.nwdtno.Value = fno                                                               ' 2018/03/16 Kino Add
            If siteName.Contains("【完了現場】") = True Then
                Me.nwdt.Value = "NC"                                                            ' 完了現場は、更新チェックしない
            End If

        Else

            ''If DataSpan = "N" Then                                              ' 2011/02/01 Kino Add グラフを切り替えた時に前のグラフと同じ表示期間とするための処理
            ''    Me.RdBFromNewest.Checked = True
            ''    Me.RdBFromNewest.Checked = False
            ''    Me.DDLRange.SelectedIndex = Integer.Parse(qStDate)
            ''ElseIf DataSpan = "D" Then
            ''    Me.RdBFromNewest.Checked = False
            ''    Me.RdBDsignatedDate.Checked = True
            ''    Me.TxtStartDate.Text = qStDate.Substring(0, 4) + "/" + qStDate.Substring(4, 2) + "/" + qStDate.Substring(6, 2)
            ''    Me.TxtEndDate.Text = qEdDate.Substring(0, 4) + "/" + qEdDate.Substring(4, 2) + "/" + qEdDate.Substring(6, 2)
            ''End If

            If Page.Request.Params.Get("__EVENTTARGET") = "DDLSelectGraph" Then Exit Sub ' グラフ切り替えの時はこれ以降の処理時間を削減するため抜ける
            ''If AJAX_TKSM.AsyncPostBackSourceElementID = "DDLSelectGraph" Then Exit Sub

            ''ポストバックの場合の処理
            'ビューステートの値を変数へ格納する
            Call ReadViewState()
            Call GetMostUpdate()                                                'データファイルの最新日時を取得する
            Call ReadFromControl()                                              'コントロールの値を変数へ格納する

            If Me.RdBFromNewest.Checked = True Then Call CalcDate(Me.DDLRange.SelectedValue)

        End If

        GrpProp.StartDate = Date.Parse(Me.TxtStartDate.Text)
        GrpProp.EndDate = Date.Parse(Me.TxtEndDate.Text)

        Me.MyTitle.Text = (GrpProp.GraphTitle + " [経時変化図] - " + siteName)                    'ブラウザのタイトルバーに表示するタイトル
        Me.LblLastUpdate.Text = ("最新データ日時：" + GrpProp.LastUpdate.ToString("yyyy/MM/dd HH:mm"))

        Call SetControlVisible(OldFlg)                                                'グラフ形式書式設定

        '' ビューステートへ変数を格納(コントロールに配置していないもののみ) ｺﾝﾄﾛｰﾙに配置しているものは後で読めるから
        Call SetViewState()

        ''If Not ScriptManager1.IsInAsyncPostBack Then                                    '非同期ポストバックでないときに処理する　アップデートパネル内のみの変更の場合

        ''If IsPostBack = False Then
        ''    Call SetGraphCommonInit()
        ''End If

        Call SetGraphCommonInit()

        ''AddHandler Me.ImbBtMovePre1Week.Click, AddressOf ChangeStartDat
        ''AddHandler Me.btnAdd.Click, AddressOf Calculate

        Me.C1WebCalendar.MaxDate = New Date(Year(GrpProp.LastUpdate), Month(GrpProp.LastUpdate), Day(GrpProp.LastUpdate))

        '' '' グラフ設定の不要な行を削除する 2015/11/24 Kino Add       2016/01/29 Kino Changed  コードビハインドでできた
        ''Dim removeStr As String = Nothing
        ''removeStr = "<script language='JavaScript'>"
        ''removeStr &= OpenString
        ''removeStr &= ("removeRow(" & (GrpProp.GraphCount + 1).ToString & ");")
        ''removeStr &= "<" + "/SCRIPT>"
        ''Page.ClientScript.RegisterStartupScript(Me.GetType, "client", removeStr)

    End Sub

    ''' <summary>
    ''' グラフに描画しようとしているファイル番号の日付を取得してカンマ区切りで返す
    ''' </summary>
    ''' <param name="fno">取得したファイル番号</param>
    ''' <returns>取得した日付</returns>
    ''' <remarks></remarks>
    Protected Function getFileDate(ByVal fno As String) As String

        Dim strRet As String = Nothing
        Dim strTemp() As String = fno.Split(Convert.ToChar(","))
        Dim FileLoop As Integer

        For FileLoop = 0 To strTemp.Length - 1
            strRet &= String.Format("{0},", DataFileNames(Convert.ToInt32(strTemp(FileLoop))).NewestDate.ToString("yyyy/MM/dd HH:mm:ss"))
        Next

        strRet = strRet.TrimEnd(Convert.ToChar(","))

        Return strRet

    End Function

    ''' <summary>
    ''' グラフに描画しようとしているファイル番号を取得してカンマ区切りで返す
    ''' </summary>
    ''' <returns>取得したファイル番号</returns>
    ''' <remarks></remarks>
    Protected Function getFileNo() As String

        Dim strRet As String = Nothing
        Dim ht As New Hashtable
        Dim infLoop As Integer
        Dim intTemp As Integer

        For infLoop = 0 To GrpProp.LineInfo.Length - 1
            intTemp = GrpProp.LineInfo(infLoop).DataFileNo
            If ht.ContainsKey(intTemp) = False Then
                ht.Add(intTemp, infLoop)
                strRet &= String.Format("{0},", intTemp.ToString)
            End If
        Next

        strRet = strRet.TrimEnd(Convert.ToChar(","))

        Return strRet

    End Function

    ''' <summary>
    ''' menuInfo.mdbから管理値色を取得する
    ''' </summary>
    ''' <param name="SiteDirectory"></param>
    ''' <remarks></remarks>
    Protected Sub getAlertColor(ByVal SiteDirectory As String)

        Dim DataFile As String
        Dim TempCount As Integer = 0
        Dim clsSummary As New ClsReadDataFile

        DataFile = Server.MapPath(SiteDirectory + "\App_Data\MenuInfo.mdb")
        TempCount = clsSummary.GetAlertDisplayInf(DataFile, AlertInfo)

    End Sub

    Protected Sub Set2FormControl()
        ''
        '' データベースから変数に格納した内容を、画面のコントロールへ配置する
        ''

        '開始日の初期状態を設定
        Me.RdBFromNewest.Checked = True
        Me.TxtEndDate.Text = GrpProp.LastUpdate.ToString("yyyy/MM/dd")      'データファイルの最新の次の日の0:00とする
        Me.DDLRange.SelectedValue = GrpProp.TimeScale                       '日付範囲をセット

        ''間引き情報作成                                                    '2009/06/09 Kino Add
        If GrpProp.thinoutEnable = True Then
            Me.ChbPartial.Checked = True
            If GrpProp.thinoutCh <> "None" Then
                Dim clsCh As New ClsReadDataFile
                Dim DatCh() As Integer = {}
                Dim iloop As Integer
                Dim strRet As String = clsCh.GetOutputChannel(DatCh, GrpProp.thinoutCh)
                clsCh = Nothing

                For iloop = 0 To DatCh.Length - 1
                    Me.CBLPartial.Items(DatCh(iloop)).Selected = True
                Next
            End If
        End If

        ''Me.ImgStDate.Visible = False                                      'javascriptで処理を行なう方法へ移行
        ''Me.ImgEdDate.Visible = False
        ''Me.TxtStartDate.ReadOnly = True
        ''Me.TxtEndDate.ReadOnly = True

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

        ''■■日付表示形式
        strSQL = "SELECT * FROM set_日付表示形式 WHERE 有効 = True ORDER BY ID"
        DtSet = New DataSet("DData")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        With Me.DdlDateFormat
            .DataSource = DtSet.Tables("DData")
            .DataMember = DtSet.DataSetName
            .DataTextField = "表示"
            .DataBind()
        End With

        ''■■凡例表示
        strSQL = "SELECT * FROM set_共通YN ORDER BY ID"
        DtSet = New DataSet("DData")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        With Me.DdlEnableLegend
            .DataSource = DtSet.Tables("DData")
            .DataMember = DtSet.DataSetName
            .DataTextField = "表示"
            .DataValueField = "値"
            .DataBind()
        End With

        With Me.DdlContinous
            .DataSource = DtSet.Tables("DData")
            .DataMember = DtSet.DataSetName
            .DataTextField = "表示"
            .DataValueField = "値"
            .DataBind()
        End With

        With Me.DDLPaintWarningValue
            .DataSource = DtSet.Tables("DData")
            .DataMember = DtSet.DataSetName
            .DataTextField = "表示"
            .DataValueField = "値"
            .DataBind()
        End With

        ''■■用紙設定
        strSQL = "SELECT * FROM set_用紙種別 WHERE 有効 = True ORDER BY ID"
        DtSet = New DataSet("DData")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        With Me.DdlPaperOrientation
            .DataSource = DtSet.Tables("DData")
            .DataMember = DtSet.DataSetName
            .DataTextField = "用紙方向"
            .DataValueField = "値"
            .DataBind()
        End With

        ''■■スケール設定
        Dim Defval(11) As DropDownList
        Dim iloop As Integer

        'オブジェクトの指定
        Defval(0) = Me.DdlScale1 : Defval(6) = Me.DdlScaleR1
        Defval(1) = Me.DdlScale2 : Defval(7) = Me.DdlScaleR2
        Defval(2) = Me.DdlScale3 : Defval(8) = Me.DdlScaleR3
        Defval(3) = Me.DdlScale4 : Defval(9) = Me.DdlScaleR4
        Defval(4) = Me.DdlScale5 : Defval(10) = Me.DdlScaleR5
        Defval(5) = Me.DdlScale6 : Defval(11) = Me.DdlScaleR6

        strSQL = "SELECT * FROM set_スケール WHERE 有効 = True ORDER BY ID"
        DtSet = New DataSet("DData")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        For iloop = 0 To 11
            With Defval(iloop)
                .DataSource = DtSet.Tables("DData")
                .DataMember = DtSet.DataSetName
                .DataTextField = "表示"
                .DataValueField = "値"
                .DataBind()
            End With
        Next iloop

        DbDa.Dispose()
        DtSet.Dispose()

    End Sub

    'Protected Sub SetScaleValue2DropDownList()

    '    Dim Defval(5) As DropDownList
    '    Dim DefvalR(5) As DropDownList
    '    Dim iloop As Integer

    '    'オブジェクトの指定
    '    Defval(0) = Me.DdlScale1 : DefvalR(0) = Me.DdlScaleR1
    '    Defval(1) = Me.DdlScale2 : DefvalR(1) = Me.DdlScaleR2
    '    Defval(2) = Me.DdlScale3 : DefvalR(2) = Me.DdlScaleR3
    '    Defval(3) = Me.DdlScale4 : DefvalR(3) = Me.DdlScaleR4
    '    Defval(4) = Me.DdlScale5 : DefvalR(4) = Me.DdlScaleR5
    '    Defval(5) = Me.DdlScale6 : DefvalR(5) = Me.DdlScaleR6

    '    For iloop = 0 To 5
    '        If iloop <= GrpProp.GraphCount - 1 Then
    '            '既定のコンボボックスにデータをバインド
    '            '左軸
    '            If IsPostBack = False Then
    '                Defval(iloop).DataSourceID = Me.AccessDataSource1.ID
    '            End If
    '            Defval(iloop).DataBind()

    '            '右軸
    '            If GrpProp.LineInfo(iloop).EnableRightAxis = True Then
    '                If IsPostBack = False Then
    '                    DefvalR(iloop).DataSourceID = Me.AccessDataSource1.ID
    '                End If
    '                DefvalR(iloop).DataBind()
    '            End If
    '        End If
    '    Next iloop

    '    For iloop = 0 To 5
    '        Defval(iloop).Dispose()
    '        DefvalR(iloop).Dispose()
    '    Next iloop

    'End Sub

    Protected Sub GetMostUpdate(Optional ByVal DataFileNo As Integer = 0)
        ''
        '' 使用しているデータファイルの中から、一番最新のデータ日付を取得する
        ''
        Dim iloop As Integer
        ''Dim dteTemp As Date
        Dim dteSession As Date

        For iloop = 0 To GrpProp.LineInfo.Length - 1                        '使用しているデータファイル番号の中で最新のデータ日時を取得する
            dteSession = CType(Session.Item("LastUpdate" + GrpProp.LineInfo(iloop).DataFileNo.ToString), Date)
            If GrpProp.LastUpdate < dteSession Then
                GrpProp.LastUpdate = dteSession
            End If

            ''If dteTemp < ClsReadDataFile.DataFileNames(GrpProp.LineInfo(iloop).DataFileNo).NewestDate Then
            ''    dteTemp = ClsReadDataFile.DataFileNames(GrpProp.LineInfo(iloop).DataFileNo).NewestDateF
            ''End If
        Next

        ''Return dteTemp

    End Sub


    ''Protected Sub CheckAutoUpdate(ByVal SiteDirectory As String)

    ''    Dim DataFile As String
    ''    Dim intInterval As Integer '= 600000
    ''    Dim blnAutoUpdate As Boolean '= False
    ''    Dim clsUpdate As New ClsReadDataFile

    ''    DataFile = Server.MapPath(SiteDirectory & "\App_Data\MenuInfo.mdb")
    ''    clsUpdate.CheckAutoUpdate(DataFile, "経時変化図", intInterval, blnAutoUpdate)

    ''    'タイマーの有効／無効を設定
    ''    Me.Tmr_Update.Enabled = blnAutoUpdate
    ''    Me.Tmr_Update.Interval = intInterval

    ''End Sub

    ''Protected Sub SetRadioButtonViewstate()

    ''    Dim iloop As Integer
    ''    Dim strCount As String

    ''    For iloop = 0 To GrpProp.GraphCount - 1
    ''        strCount = iloop.ToString
    ''        ViewState("GrpLineSet0L" & strCount) = GrpProp.LineInfo(iloop).LeftAxis.DefaultSet                          '左軸   既定値かどうか
    ''        ''ViewState("GrpLineSet1L" & strCount) = GrpProp.LineInfo(iloop).LeftAxis.DefaultValue                       '       既定値の場合の値

    ''        ViewState("GrpLineSet0R" & strCount) = GrpProp.LineInfo(iloop).RightAxis.DefaultSet                         '右軸   既定値かどうか
    ''        ''ViewState("GrpLineSet1R" & strCount) = GrpProp.LineInfo(iloop).RightAxis.DefaultValue                      '       既定値の場合の値
    ''    Next iloop

    ''    ViewState("AsyncPostBackFlg") = "1"                                                                             '非同期ポストバック発生フラグ

    ''End Sub

    ''Protected Sub ReadSetRadioButtonViewstate()

    ''    Dim iloop As Integer
    ''    Dim strCount As String
    ''    Dim ScaleInput(5), ScaleInputR(5) As WebControls.RadioButton
    ''    Dim ScaleDef(5), ScaleDefR(5) As WebControls.RadioButton

    ''    '左軸
    ''    ScaleDef(0) = Me.RdbNo11 : ScaleInput(0) = Me.RdbNo12
    ''    ScaleDef(1) = Me.RdbNo21 : ScaleInput(1) = Me.RdbNo22
    ''    ScaleDef(2) = Me.RdbNo31 : ScaleInput(2) = Me.RdbNo32
    ''    ScaleDef(3) = Me.RdbNo41 : ScaleInput(3) = Me.RdbNo42
    ''    ScaleDef(4) = Me.RdbNo51 : ScaleInput(4) = Me.RdbNo52
    ''    ScaleDef(5) = Me.RdbNo61 : ScaleInput(5) = Me.RdbNo62
    ''    '右軸
    ''    ScaleDefR(0) = Me.RdbNoR11 : ScaleInputR(0) = Me.RdbNoR12
    ''    ScaleDefR(1) = Me.RdbNoR21 : ScaleInputR(1) = Me.RdbNoR22
    ''    ScaleDefR(2) = Me.RdbNoR31 : ScaleInputR(2) = Me.RdbNoR32
    ''    ScaleDefR(3) = Me.RdbNoR42 : ScaleInputR(3) = Me.RdbNoR42
    ''    ScaleDefR(4) = Me.RdbNoR51 : ScaleInputR(4) = Me.RdbNoR52
    ''    ScaleDefR(5) = Me.RdbNoR61 : ScaleInputR(5) = Me.RdbNoR62

    ''    For iloop = 0 To GrpProp.GraphCount - 1
    ''        strCount = iloop.ToString
    ''        GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = ViewState("GrpLineSet0L" & strCount)                         '左軸   既定値かどうか
    ''        ''ViewState("GrpLineSet1L" & strCount) = GrpProp.LineInfo(iloop).LeftAxis.DefaultValue                       '       既定値の場合の値

    ''        GrpProp.LineInfo(iloop).RightAxis.DefaultSet = ViewState("GrpLineSet0R" & strCount)                        '右軸   既定値かどうか
    ''        ''ViewState("GrpLineSet1R" & strCount) = GrpProp.LineInfo(iloop).RightAxis.DefaultValue                      '       既定値の場合の値

    ''        If GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = False Then                     'スケールが既定値でない場合は
    ''            ScaleInput(iloop).Checked = True                                            '入力ラジオボタンを選択状態にする
    ''            ScaleDef(iloop).Checked = False                                             '既定値ラジオボタンを非選択状態にする
    ''        Else
    ''            ScaleInput(iloop).Checked = False                                           '入力ラジオボタンを非選択状態にする
    ''            ScaleDef(iloop).Checked = True                                              '既定値ラジオボタンを選択状態にする
    ''        End If

    ''        '====== 右軸 ======

    ''        If GrpProp.LineInfo(iloop).RightAxis.DefaultSet = False Then                    'スケールが既定値でない場合は
    ''            ScaleInputR(iloop).Checked = True                                           '入力ラジオボタンを選択状態にする
    ''            ScaleDefR(iloop).Checked = False                                            '既定値ラジオボタンを非選択状態にする
    ''        Else
    ''            ScaleInputR(iloop).Checked = False                                          '入力ラジオボタンを非選択状態にする
    ''            ScaleDefR(iloop).Checked = True                                             '既定値ラジオボタンを選択状態にする
    ''        End If
    ''    Next iloop

    ''End Sub

    Protected Function ReadGraphInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, ByVal graphType As String, Optional ByVal PostBackFlg As Boolean = False) As Integer
        ''    Protected Function ReadGraphInfoFromDB(ByVal grpDataFile As String, ByVal graphType As String, Optional ByVal PostBackFlg As Boolean = False) As Integer
        ''
        '' グラフの描画情報を読み込む
        ''
        Dim strSQL As String
        Dim iloop As Integer
        ''Dim DbCom As New OleDb.OleDbCommand
        ''Dim DbDr As OleDb.OleDbDataReader
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        ReDim GrpProp.DrawSizeColorInf(23)                      'グラフの各部分のフォントサイズなどは固定の配列

        ReadGraphInfoFromDB = 0

        strSQL = ("SELECT * FROM メニュー基本情報 WHERE (項目名 = '" + GrpProp.WindowTitle + "' AND 種別 = '" + graphType + "')")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")
        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader

        Try
            With DtSet.Tables("DData")
                If .Rows.Count > 0 Then
                    GrpProp.GraphCount = Convert.ToInt32(.Rows(0).Item(4))              'グラフの数
                    GrpProp.GraphBoxCount = Convert.ToInt32(.Rows(0).Item(5))           'グラフ枠の数(パネル)
                    GrpProp.GraphTitle = .Rows(0).Item(6).ToString                      'グラフのタイトル
                    GrpProp.TitlePosition = Convert.ToInt16(.Rows(0).Item(7))           'タイトルの位置(上or下)
                    GrpProp.TimeScale = .Rows(0).Item(8).ToString                       '横軸の時間軸スケール
                    GrpProp.DateFormat = .Rows(0).Item(10).ToString                     '日付の表示フォーマット
                    GrpProp.XAxisTileAngle = Convert.ToInt32(.Rows(0).Item(11))         '横軸のメモリラベルの角度
                    GrpProp.PaperOrientaion = Convert.ToInt32(.Rows(0).Item(12))        '紙の向き
                    GrpProp.EnableLegend = Convert.ToInt32(.Rows(0).Item(38))           '凡例の有無
                    GrpProp.MissingDataContinuous = Convert.ToInt32(.Rows(0).Item(39))  '欠測データの連結
                    GrpProp.ShowAlarmArea = Convert.ToInt32(.Rows(0).Item(40))          '警報値表示
                    GrpProp.thinoutEnable = Convert.ToBoolean(.Rows(0).Item(42))        '間引きチェックの有無
                    GrpProp.thinoutCh = .Rows(0).Item(43).ToString                      '間引きチャンネル情報 カンマ区切り
                    GrpProp.alertStyle = CType(.Rows(0).Item(44), Integer)              ' 管理値塗りつぶしスタイル　新旧の切り替え　1:旧 0:新      '2014/05/16 Kino Add

                    For iloop = 13 To 35
                        GrpProp.DrawSizeColorInf(iloop - 13) = Convert.ToInt32(.Rows(0).Item(iloop))  'グラフの各部分のフォントサイズなど(詳細は下を参照)
                    Next iloop
                End If
            End With

            ReDim GrpProp.LineInfo(GrpProp.GraphCount - 1)          '個々のグラフの情報を格納する配列設定(グラフ個数分)

            ReadGraphInfoFromDB = 1
        Catch ex As Exception

        Finally
            DbDa.Dispose()
            DtSet.Dispose()

        End Try
        ''With DbDr
        ''    If .HasRows = True Then
        ''        .Read()                                         '１レコード分読み込み

        ''        GrpProp.GraphCount = .GetInt32(4)               'グラフの数
        ''        GrpProp.GraphBoxCount = .GetInt32(5)            'グラフ枠の数(パネル)
        ''        GrpProp.GraphTitle = .GetString(6)              'グラフのタイトル
        ''        GrpProp.TitlePosition = CType(.GetInt32(7), Short)      'タイトルの位置(上or下)
        ''        GrpProp.TimeScale = .GetString(8)               '横軸の時間軸スケール
        ''        GrpProp.DateFormat = .GetString(10)             '日付の表示フォーマット
        ''        GrpProp.XAxisTileAngle = .GetInt32(11)          '横軸のメモリラベルの角度
        ''        GrpProp.PaperOrientaion = .GetInt32(12)         '紙の向き
        ''        GrpProp.EnableLegend = .GetInt16(38)            '凡例の有無
        ''        GrpProp.MissingDataContinuous = .GetInt16(39)   '欠測データの連結
        ''        GrpProp.ShowAlarmArea = .GetInt16(40)           '警報値表示
        ''        GrpProp.thinoutEnable = .GetBoolean(42)         '間引きチェックの有無
        ''        GrpProp.thinoutCh = .GetString(43)              '間引きチャンネル情報 カンマ区切り

        ''        For iloop = 13 To 35
        ''            GrpProp.DrawSizeColorInf(iloop - 13) = .GetInt32(iloop)     'グラフの各部分のフォントサイズなど(詳細は下を参照)
        ''        Next iloop

        ''    End If

        ''End With

        ''DbDr.Close()
        ''DbCom.Dispose()


        '' 配列番号における変数の内容                           '11 24:Y1軸スケールの文字色　
        '23 35:補助スケール線色                                 '10 23:Y1軸タイトルの文字色
        '22 34:補助スケール線幅                                 ' 8 22:Y1軸(左)スケール文字のフォントサイズ
        '20 33:スケール線色                                     ' 8 21:Y1軸(左)タイトルのフォントサイズ
        '19 32:スケール線幅                                     ' 7 20:X軸スケールの文字色
        '18 31:凡例の線色                                       ' 6 19:X軸タイトルの文字色
        '17 30:凡例枠線幅                                       ' 5 18:X軸スケール文字のフォントサイズ
        '16 29:凡例のフォントサイズ                             ' 4 17:X軸タイトルのフォントサイズ
        '15 28:Y2軸スケールの文字色                             ' 3 16:サブタイトルの色
        '14 27:Y2軸タイトルの文字色                             ' 2 15:サブタイトルのフォントサイズ
        '13 26:Y2軸(右)スケール文字のフォントサイズ             ' 1 14:グラフタイトルの色
        '12 25:Y2軸(右)タイトルのフォントサイズ                 ' 0 13:グラフタイトルのフォントサイズ


    End Function

    Protected Function GraphLineInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, Optional ByVal postBackFlg As Boolean = False) As Integer
        ''
        '' 項目名におけるページに表示する個々のグラフ情報
        ''
        ''Dim DbCom As New OleDb.OleDbCommand
        ''Dim DbDr As OleDb.OleDbDataReader
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strSQL As String
        Dim iloop As Integer
        ''Dim ScaleMax(5) As TextBox
        ''Dim ScaleMin(5) As TextBox
        ''Dim ScaleDef(5) As RadioButton
        ''Dim ScaleInput(5) As RadioButton
        ''Dim DefVal(5) As DropDownList

        GraphLineInfoFromDB = 0

        ''ScaleMax(0) = Me.TxtMax1 : ScaleMin(0) = Me.TxtMin1 : ScaleDef(0) = Me.RdbNo11 : ScaleInput(0) = Me.RdbNo12 : DefVal(0) = Me.DdlScale1
        ''ScaleMax(1) = Me.TxtMax2 : ScaleMin(1) = Me.TxtMin2 : ScaleDef(1) = Me.RdbNo21 : ScaleInput(1) = Me.RdbNo22 : DefVal(1) = Me.DdlScale2
        ''ScaleMax(2) = Me.TxtMax3 : ScaleMin(2) = Me.TxtMin3 : ScaleDef(2) = Me.RdbNo31 : ScaleInput(2) = Me.RdbNo32 : DefVal(2) = Me.DdlScale3
        ''ScaleMax(3) = Me.TxtMax4 : ScaleMin(3) = Me.TxtMin4 : ScaleDef(3) = Me.RdbNo41 : ScaleInput(3) = Me.RdbNo42 : DefVal(3) = Me.DdlScale4
        ''ScaleMax(4) = Me.TxtMax5 : ScaleMin(4) = Me.TxtMin5 : ScaleDef(4) = Me.RdbNo51 : ScaleInput(4) = Me.RdbNo52 : DefVal(4) = Me.DdlScale5
        ''ScaleMax(5) = Me.TxtMax6 : ScaleMin(5) = Me.TxtMin6 : ScaleDef(5) = Me.RdbNo61 : ScaleInput(5) = Me.RdbNo62 : DefVal(5) = Me.DdlScale6

        strSQL = ("Select * From [" + GrpProp.WindowTitle + "] ORDER BY グラフNo ASC;")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        Try
            For Each DTR As DataRow In DtSet.Tables("DData").Rows
                If DTR.ItemArray.GetValue(2).ToString.Length <> 0 Then
                    GrpProp.LineInfo(iloop).SubTitle = DTR.ItemArray.GetValue(2).ToString               'サブタイトル
                Else
                    GrpProp.LineInfo(iloop).SubTitle = ""                                               'サブタイトルなし
                End If
                With DTR.ItemArray
                    GrpProp.LineInfo(iloop).EnableRightAxis = Convert.ToBoolean(.GetValue(3))           '右軸の有効／無効
                    GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = Convert.ToBoolean(.GetValue(4))       '左軸   既定値かどうか
                    GrpProp.LineInfo(iloop).LeftAxis.DefaultValue = Convert.ToInt32(.GetValue(5))       '       既定値の場合の値
                    GrpProp.LineInfo(iloop).LeftAxis.DataTitle = .GetValue(6).ToString                  '       データタイトル
                    GrpProp.LineInfo(iloop).LeftAxis.UnitTitle = .GetValue(7).ToString                  '       単位タイトル
                    GrpProp.LineInfo(iloop).LeftAxis.Max = Convert.ToSingle(.GetValue(8))               '       スケール最大値
                    GrpProp.LineInfo(iloop).LeftAxis.Min = Convert.ToSingle(.GetValue(9))               '       スケール最小値
                    GrpProp.LineInfo(iloop).LeftAxis.UnitMajor = Convert.ToSingle(.GetValue(10))        '       主メモリ間隔    
                    GrpProp.LineInfo(iloop).LeftAxis.UnitMinor = Convert.ToSingle(.GetValue(11))        '       副メモリ間隔
                    GrpProp.LineInfo(iloop).LeftAxis.DataCount = Convert.ToInt16(.GetValue(12))         '       データ系列数
                    GrpProp.LineInfo(iloop).DataFileNo = Convert.ToInt32(.GetValue(22))                 '       データファイル番号
                    GrpProp.LineInfo(iloop).LeftAxis.ChartType = .GetValue(23).ToString                 '       グラフ種別
                    GrpProp.LineInfo(iloop).LeftAxis.Reversed = Convert.ToBoolean(.GetValue(25))        '       軸反転
                    If DtSet.Tables("DData").Columns.Count >= 29 Then                                   ' フィールド数チェック
                        GrpProp.LineInfo(iloop).LeftAxis.LogAxis = CType(.GetValue(27), Boolean)        '       対数軸
                    Else
                        GrpProp.LineInfo(iloop).LeftAxis.LogAxis = False                                '       対数軸
                    End If
                    If DtSet.Tables("DData").Columns.Count >= 30 Then                                   ' フィールド数チェック
                        GrpProp.LineInfo(iloop).spAlertValue = .GetValue(29).ToString                   '       個別警報値
                    Else
                        GrpProp.LineInfo(iloop).spAlertValue = ""                                       '       個別警報値
                    End If
                    If DtSet.Tables("DData").Columns.Count >= 31 Then                                   ' フィールド数チェック
                        GrpProp.LineInfo(iloop).locationX = CType(.GetValue(30), Integer)               '       チャートエリアの起点位置 X座標
                        GrpProp.LineInfo(iloop).locationY = CType(.GetValue(31), Integer)               '       チャートエリアの起点位置 Y座標
                        GrpProp.LineInfo(iloop).widthRatio = CType(.GetValue(32), Integer)              '       グラフ横幅に対するチャートエリアの割合
                        GrpProp.LineInfo(iloop).heightRatio = CType(.GetValue(33), Integer)             '       グラフ高さに対するチャートエリアの割合
                        GrpProp.LineInfo(iloop).subTitleSize = CType(.GetValue(34), Single)             '       サブタイトルのフォントサイズ
                        GrpProp.LineInfo(iloop).legendWidth = CType(.GetValue(35), Integer)             '       凡例の横幅
                        GrpProp.LineInfo(iloop).alertType = CType(.GetValue(36), Integer)               '       警報表示種別
                        GrpProp.LineInfo(iloop).showAlertValue = CType(.GetValue(37), Boolean)          '       警報値数値表示
                        GrpProp.LineInfo(iloop).alertValuePosition = CType(.GetValue(38), Integer)      '       警報値数値表示の場合の左から位置割合
                    Else
                        GrpProp.LineInfo(iloop).locationX = 80                                          '       デフォルト値
                        GrpProp.LineInfo(iloop).locationY = 15
                        GrpProp.LineInfo(iloop).widthRatio = 83
                        GrpProp.LineInfo(iloop).heightRatio = 83
                        GrpProp.LineInfo(iloop).subTitleSize = 9
                        GrpProp.LineInfo(iloop).legendWidth = 110
                        GrpProp.LineInfo(iloop).alertType = 0
                        GrpProp.LineInfo(iloop).showAlertValue = False
                        GrpProp.LineInfo(iloop).alertValuePosition = 0
                    End If

                    '右軸が有効なら読み込む
                    If GrpProp.LineInfo(iloop).EnableRightAxis = True Then
                        GrpProp.LineInfo(iloop).RightAxis.DefaultSet = Convert.ToBoolean(.GetValue(13)) '右軸   既定値かどうか
                        GrpProp.LineInfo(iloop).RightAxis.DefaultValue = Convert.ToInt32(.GetValue(14)) '       既定値の場合の値
                        GrpProp.LineInfo(iloop).RightAxis.DataTitle = .GetValue(15).ToString            '       データタイトル
                        GrpProp.LineInfo(iloop).RightAxis.UnitTitle = .GetValue(16).ToString            '       単位タイトル
                        GrpProp.LineInfo(iloop).RightAxis.Max = Convert.ToSingle(.GetValue(17))         '       スケール最大値
                        GrpProp.LineInfo(iloop).RightAxis.Min = Convert.ToSingle(.GetValue(18))         '       スケール最小値
                        GrpProp.LineInfo(iloop).RightAxis.UnitMajor = Convert.ToSingle(.GetValue(19))   '       主メモリ間隔    
                        GrpProp.LineInfo(iloop).RightAxis.UnitMinor = Convert.ToSingle(.GetValue(20))   '       副メモリ間隔
                        GrpProp.LineInfo(iloop).RightAxis.DataCount = Convert.ToInt16(.GetValue(21))    '       データ系列数
                        GrpProp.LineInfo(iloop).RightAxis.ChartType = .GetValue(24).ToString            '       グラフ種別
                        GrpProp.LineInfo(iloop).RightAxis.Reversed = Convert.ToBoolean(.GetValue(26))   '       軸反転
                        If DtSet.Tables("DData").Columns.Count >= 29 Then                               ' フィールド数チェック
                            GrpProp.LineInfo(iloop).RightAxis.LogAxis = Convert.ToBoolean(.GetValue(28)) '       対数軸
                        Else
                            GrpProp.LineInfo(iloop).RightAxis.LogAxis = False                           '       対数軸
                        End If

                    End If
                    iloop += 1
                End With
            Next

            For iloop = 0 To GrpProp.GraphCount - 1
                ReDim GrpProp.LineInfo(iloop).LeftAxisData(GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1)      '個々のグラフのデータ系列数配列を指定
                ReDim GrpProp.LineInfo(iloop).RightAxisData(GrpProp.LineInfo(iloop).RightAxis.DataCount - 1)    '個々のグラフのデータ系列数配列を指定
            Next iloop

            GraphLineInfoFromDB = 1

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine(ex.ToString)
        Finally
            DbDa.Dispose()
            DtSet.Dispose()
        End Try

        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        For iloop = 0 To GrpProp.GraphCount - 1
        ''            .Read()

        ''            If .Item(2).ToString.Length <> 0 Then
        ''                GrpProp.LineInfo(iloop).SubTitle = .GetString(2)                            'サブタイトル
        ''            Else
        ''                GrpProp.LineInfo(iloop).SubTitle = ""                                       'サブタイトルなし
        ''            End If
        ''            GrpProp.LineInfo(iloop).EnableRightAxis = .GetBoolean(3)                        '右軸の有効／無効
        ''            GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = .GetBoolean(4)                    '左軸   既定値かどうか
        ''            GrpProp.LineInfo(iloop).LeftAxis.DefaultValue = .GetInt32(5)                    '       既定値の場合の値
        ''            GrpProp.LineInfo(iloop).LeftAxis.DataTitle = .GetString(6)                      '       データタイトル
        ''            GrpProp.LineInfo(iloop).LeftAxis.UnitTitle = .GetString(7)                      '       単位タイトル
        ''            GrpProp.LineInfo(iloop).LeftAxis.Max = .GetFloat(8)                             '       スケール最大値
        ''            GrpProp.LineInfo(iloop).LeftAxis.Min = .GetFloat(9)                             '       スケール最小値
        ''            GrpProp.LineInfo(iloop).LeftAxis.UnitMajor = .GetInt32(10)                      '       主メモリ間隔    
        ''            GrpProp.LineInfo(iloop).LeftAxis.UnitMinor = .GetInt32(11)                      '       副メモリ間隔
        ''            GrpProp.LineInfo(iloop).LeftAxis.DataCount = CType(.GetInt32(12), Short)        '       データ系列数
        ''            GrpProp.LineInfo(iloop).DataFileNo = CType(.GetInt16(22), Short)                '       データファイル番号
        ''            GrpProp.LineInfo(iloop).LeftAxis.ChartType = CType(.GetString(23), String)      '       グラフ種別
        ''            GrpProp.LineInfo(iloop).LeftAxis.Reversed = CType(.GetBoolean(25), Boolean)     '       軸反転

        ''            ''ScaleMax(iloop - 1).Text = GrpProp.LineInfo(iloop).LeftAxis.Max.ToString    '最大値
        ''            ''ScaleMin(iloop - 1).Text = GrpProp.LineInfo(iloop).LeftAxis.Min.ToString    '最小値

        ''            ''If GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = False Then                     'スケールが既定値でない場合は
        ''            ''    ScaleInput(iloop - 1).Checked = True                                        '入力ラジオボタンを選択状態にする
        ''            ''    ScaleDef(iloop - 1).Checked = False                                         '既定値ラジオボタンを非選択状態にする
        ''            ''    DefVal(iloop - 1).SelectedIndex = 0

        ''            ''Else
        ''            ''    ScaleInput(iloop - 1).Checked = False                                       '入力ラジオボタンを非選択状態にする
        ''            ''    ScaleDef(iloop - 1).Checked = True                                          '既定値ラジオボタンを選択状態にする
        ''            ''    DefVal(iloop - 1).SelectedIndex = GrpProp.LineInfo(iloop).LeftAxis.DefaultValue

        ''            ''End If

        ''            '右軸が有効なら読み込む
        ''            If GrpProp.LineInfo(iloop).EnableRightAxis = True Then
        ''                GrpProp.LineInfo(iloop).RightAxis.DefaultSet = .GetBoolean(13)                  '右軸   既定値かどうか
        ''                GrpProp.LineInfo(iloop).RightAxis.DefaultValue = .GetInt32(14)                  '       既定値の場合の値
        ''                GrpProp.LineInfo(iloop).RightAxis.DataTitle = .GetString(15)                    '       データタイトル
        ''                GrpProp.LineInfo(iloop).RightAxis.UnitTitle = .GetString(16)                    '       単位タイトル
        ''                GrpProp.LineInfo(iloop).RightAxis.Max = .GetFloat(17)                           '       スケール最大値
        ''                GrpProp.LineInfo(iloop).RightAxis.Min = .GetFloat(18)                           '       スケール最小値
        ''                GrpProp.LineInfo(iloop).RightAxis.UnitMajor = .GetInt32(19)                     '       主メモリ間隔    
        ''                GrpProp.LineInfo(iloop).RightAxis.UnitMinor = .GetInt32(20)                     '       副メモリ間隔
        ''                GrpProp.LineInfo(iloop).RightAxis.DataCount = CType(.GetInt32(21), Short)       '       データ系列数
        ''                GrpProp.LineInfo(iloop).RightAxis.ChartType = CType(.GetString(24), String)     '       グラフ種別
        ''                GrpProp.LineInfo(iloop).RightAxis.Reversed = CType(.GetBoolean(26), Boolean)    '       軸反転
        ''            End If
        ''        Next
        ''        GraphLineInfoFromDB = 1
        ''    End If
        ''End With
        ''DbDr.Close()
        ''DbCom.Dispose()

        ''For iloop = 0 To GrpProp.GraphCount - 1
        ''    ReDim GrpProp.LineInfo(iloop).LeftAxisData(GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1)      '個々のグラフのデータ系列数配列を指定
        ''    ReDim GrpProp.LineInfo(iloop).RightAxisData(GrpProp.LineInfo(iloop).RightAxis.DataCount - 1)    '個々のグラフのデータ系列数配列を指定
        ''Next iloop

    End Function

    Protected Function GraphDataInfoFromDB(ByVal dbCon As OleDb.OleDbConnection) As Integer
        ''
        '' 各グラフに描画するデータの情報
        ''

        Dim strSQL As String
        '' Dim DbDr As OleDb.OleDbDataReader
        '' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim GraphNo As Integer
        Dim DataNo As Integer

        GraphDataInfoFromDB = 0

        strSQL = ("Select * From [" + GrpProp.WindowTitle + "データ情報] Order by グラフNo asc,データNo asc")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")
        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader

        Try
            For Each DTR As DataRow In DtSet.Tables("DData").Rows                                                               'グラフ数でループ
                With DTR.ItemArray
                    GraphNo = Convert.ToInt32(.GetValue(0)) - 1
                    DataNo = Convert.ToInt32(.GetValue(1)) - 1
                    If GraphNo >= GrpProp.LineInfo.Length Then Exit For
                    GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).Symbol = .GetValue(2).ToString                               '計器記号
                    GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).ChNo = Convert.ToInt32(.GetValue(3))                         'チャンネル番号
                    GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).Color = Convert.ToInt32(.GetValue(4))                        '線色
                    GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).LineType = Convert.ToInt16(.GetValue(5))                     '線種
                    GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).LineWidth = Convert.ToInt16(.GetValue(6))                    '線幅
                    GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).Marker = Convert.ToInt16(.GetValue(7))                       'マーカ
                    ''If GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).Marker <> 0 Then                                        ' 2018/06/07 Kino Changed 必ず読む
                    GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).MarkSize = Convert.ToInt16(.GetValue(14))                    'マーカサイズ
                    If GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).MarkSize = 0 Then GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).MarkSize = 1 'マウスホバー対応のため、最低１とする
                    GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).MarkOutlineWidth = Convert.ToInt16(.GetValue(15))            'マーカ枠線幅
                    ''End If

                    If GrpProp.LineInfo(GraphNo).EnableRightAxis = True And GrpProp.LineInfo(GraphNo).RightAxis.DataCount >= (DataNo + 1) Then
                        GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Symbol = .GetValue(8).ToString                          '計器記号
                        GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).ChNo = Convert.ToInt32(.GetValue(9))                    'チャンネル番号
                        GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Color = Convert.ToInt32(.GetValue(10))                  '線色
                        GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).LineType = Convert.ToInt16(.GetValue(11))               '線種
                        GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).LineWidth = Convert.ToInt16(.GetValue(12))              '線幅
                        GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Marker = Convert.ToInt16(.GetValue(13))                 'マーカ
                        'If GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Marker <> 0 Then                                    ' 2018/06/07 Kino Changed 必ず読む
                        GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).MarkSize = Convert.ToInt16(.GetValue(16))               'マーカサイズ
                        If GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Marker = 0 Then GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Marker = 1 'マウスホバー対応のため、最低１とする
                        GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).MarkOutlineWidth = Convert.ToInt16(.GetValue(17))       'マーカ枠線幅
                        'End If
                    End If
                End With
            Next

        Catch ex As Exception

        Finally
            DbDa.Dispose()
            DtSet.Dispose()
        End Try
        ''With DbDr
        ''    If .HasRows = True Then
        ''        Do While .Read()                                                        '最終レコードまでを読み込む
        ''            GraphNo = .GetInt32(0) - 1
        ''            DataNo = .GetInt32(1) - 1
        ''            If GraphNo >= GrpProp.LineInfo.Length Then Exit Do
        ''            GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).Symbol = .GetString(2)                       '計器記号
        ''            GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).ChNo = .GetInt32(3)                          'チャンネル番号
        ''            GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).Color = .GetInt32(4)                         '線色
        ''            GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).LineType = CType(.GetInt32(5), Short)        '線種
        ''            GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).LineWidth = CType(.GetFloat(6), Short)       '線幅
        ''            GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).Marker = CType(.GetInt32(7), Short)          'マーカ
        ''            If GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).Marker <> 0 Then
        ''                GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).MarkSize = CType(.GetInt16(14), Short)   'マーカサイズ
        ''                GrpProp.LineInfo(GraphNo).LeftAxisData(DataNo).MarkOutlineWidth = CType(.GetInt16(15), Short)   'マーカ枠線幅
        ''            End If

        ''            If GrpProp.LineInfo(GraphNo).EnableRightAxis = True And GrpProp.LineInfo(GraphNo).RightAxis.DataCount >= (DataNo + 1) Then
        ''                GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Symbol = .GetString(8)                  '計器記号
        ''                GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).ChNo = .GetInt32(9)                     'チャンネル番号
        ''                GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Color = .GetInt32(10)                   '線色
        ''                GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).LineType = CType(.GetInt32(11), Short)  '線種
        ''                GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).LineWidth = CType(.GetFloat(12), Short) '線幅
        ''                GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Marker = CType(.GetInt32(13), Short)    'マーカ
        ''                If GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).Marker <> 0 Then
        ''                    GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).MarkSize = CType(.GetInt16(16), Short)  'マーカサイズ
        ''                    GrpProp.LineInfo(GraphNo).RightAxisData(DataNo).MarkOutlineWidth = CType(.GetInt16(17), Short)  'マーカ枠線幅
        ''                End If
        ''            End If
        ''        Loop
        ''    End If
        ''End With

        ''DbDr.Close()
        ''DbCom.Dispose()

    End Function

    Protected Sub SetControlVisible(ByVal OldFlg As Boolean)
        ''
        '' グラフ枠、設定項目の表示を設定
        ''

        Dim iloop As Integer
        Dim GrpBox(5) As WebControls.Panel
        Dim Grps(5) As C1WebChart
        Dim LabelNo(5) As WebControls.Label
        Dim SetPanel(5) As WebControls.Panel
        Dim SetPanelR(5) As WebControls.Panel
        Dim GrpHeight As Integer
        Dim GrpWidth As Integer
        Dim ChartHeight As Integer
        Dim ChartWidth As Integer
        Dim RExValMax(6) As RegularExpressionValidator      '2009/11/19
        Dim RExValMaxR(6) As RegularExpressionValidator
        Dim RExValMin(6) As RegularExpressionValidator
        Dim RExValMinR(6) As RegularExpressionValidator
        Dim ValidCOExMax(6) As AjaxControlToolkit.ValidatorCalloutExtender
        Dim ValidCOExMin(6) As AjaxControlToolkit.ValidatorCalloutExtender
        Dim ValidCOExMaxR(6) As AjaxControlToolkit.ValidatorCalloutExtender                                                    ' 2017/06/22 Kino Add
        Dim ValidCOExMinR(6) As AjaxControlToolkit.ValidatorCalloutExtender                                                         ' 2017/06/22 Kino Add
        Dim tableRow(5) As HtmlControls.HtmlTableRow

        'オブジェクトの指定:グラフ                  'オブジェクトの指定:左軸関連                     'オブジェクトの指定:右軸関連
        Grps(0) = Me.WC1 : GrpBox(0) = Me.PnlGraph1 ': SetPanel(0) = Me.Pnl1 : LabelNo(0) = Me.LblNo1 ' : SetPanelR(0) = Me.PnlR1
        Grps(1) = Me.WC2 : GrpBox(1) = Me.PnlGraph2 ': SetPanel(1) = Me.Pnl2 : LabelNo(1) = Me.LblNo2 ' : SetPanelR(1) = Me.PnlR2
        Grps(2) = Me.WC3 : GrpBox(2) = Me.PnlGraph3 ': SetPanel(2) = Me.Pnl3 : LabelNo(2) = Me.LblNo3 ' : SetPanelR(2) = Me.PnlR3
        Grps(3) = Me.WC4 : GrpBox(3) = Me.PnlGraph4 ': SetPanel(3) = Me.Pnl4 : LabelNo(3) = Me.LblNo4 ' : SetPanelR(3) = Me.PnlR4
        Grps(4) = Me.WC5 : GrpBox(4) = Me.PnlGraph5 ': SetPanel(4) = Me.Pnl5 : LabelNo(4) = Me.LblNo5 ' : SetPanelR(4) = Me.PnlR5
        Grps(5) = Me.WC6 : GrpBox(5) = Me.PnlGraph6 ': SetPanel(5) = Me.Pnl6 : LabelNo(5) = Me.LblNo6 ' : SetPanelR(5) = Me.PnlR6
        For iloop = 0 To 5
            SetPanelR(iloop) = CType(FindControl("PnlR" + (iloop + 1).ToString), WebControls.Panel)
            'tableRow(iloop) = CType(FindControl("No" + (iloop + 1).ToString), HtmlControls.HtmlTableRow)
            tableRow(iloop) = CType(FindControl("r" + (iloop + 1).ToString), HtmlControls.HtmlTableRow)
        Next iloop

        Dim uLevel As String = Convert.ToString(Session.Item("UL"))             ' 2016/01/27 Kino Add スケール保存ボタンを権限により非表示にする
        Dim ULNo(5) As String
        Dim clsLevelCheck As New ClsCheckUser
        Dim intTemp As Integer = clsLevelCheck.GetWord(uLevel, ULNo)
        If ULNo(5) Is Nothing OrElse ULNo(5) = "0" Then
            Me.lkBtn1.Visible = False : Me.svOut1.Visible = False
            Me.lkBtn2.Visible = False : Me.svOut2.Visible = False
            Me.lkBtn3.Visible = False : Me.svOut3.Visible = False
            Me.lkBtn4.Visible = False : Me.svOut4.Visible = False
            Me.lkBtn5.Visible = False : Me.svOut5.Visible = False
            Me.lkBtn6.Visible = False : Me.svOut6.Visible = False
            Me.lkBtnLeftAll.Visible = False
            Me.lkBtnRightAll.Visible = False
        End If

        'オブジェクトの指定:グラフ  2009/11/19
        For iloop = 1 To 6
            RExValMax(iloop) = CType(Form.FindControl("RExValidMax" + iloop.ToString), RegularExpressionValidator)
            RExValMin(iloop) = CType(Form.FindControl("RExValidMin" + iloop.ToString), RegularExpressionValidator)
            RExValMaxR(iloop) = CType(Form.FindControl("RExValidMaxR" + iloop.ToString), RegularExpressionValidator)
            RExValMinR(iloop) = CType(Form.FindControl("RExValidMinR" + iloop.ToString), RegularExpressionValidator)

            ValidCOExMax(iloop) = CType(Form.FindControl("COEMax" + iloop.ToString), AjaxControlToolkit.ValidatorCalloutExtender)
            ValidCOExMin(iloop) = CType(Form.FindControl("COEMin" + iloop.ToString), AjaxControlToolkit.ValidatorCalloutExtender)
            ValidCOExMaxR(iloop) = CType(Form.FindControl("COEMaxR" + iloop.ToString), AjaxControlToolkit.ValidatorCalloutExtender)
            ValidCOExMinR(iloop) = CType(Form.FindControl("COEMinR" + iloop.ToString), AjaxControlToolkit.ValidatorCalloutExtender)
        Next

        'グラフと枠の表示設定
        Dim rightAxisEnable As Boolean = False                                  '' 2016/01/27 Kino Add
        Dim rowCount As Integer = 1
        For iloop = 0 To 5

            rowCount += 1
            If iloop <= GrpProp.GraphBoxCount - 1 Then                          '' 2010/11/19 Kino Add 枠をスペースとして確保する
                GrpBox(iloop).Visible = True
            Else
                Controls.Remove(GrpBox(iloop))
                GrpBox(iloop).Dispose()
            End If

            If iloop <= GrpProp.GraphCount - 1 Then
                ''SetPanel(iloop).Visible = True
                ''SetPanel(iloop).Height = 50
                ''LabelNo(iloop).Visible = True
                Grps(iloop).Enabled = True                                      ''2009/06/23 kino Add
                Grps(iloop).Visible = True
                If GrpProp.LineInfo(iloop).EnableRightAxis = True Then
                    SetPanelR(iloop).Visible = True
                Else
                    SetPanelR(iloop).Visible = False
                End If
            Else
                AJAX_TKSM.Controls.Remove(ValidCOExMax(iloop + 1))
                AJAX_TKSM.Controls.Remove(ValidCOExMin(iloop + 1))
                AJAX_TKSM.Controls.Remove(ValidCOExMaxR(iloop + 1))                    ' 2017/06/22 Kino Add
                AJAX_TKSM.Controls.Remove(ValidCOExMinR(iloop + 1))                    ' 2017/06/22 Kino Add

                Controls.Remove(RExValMax(iloop + 1))          '2009/11/19
                Controls.Remove(RExValMin(iloop + 1))
                Controls.Remove(RExValMaxR(iloop + 1))
                Controls.Remove(RExValMinR(iloop + 1))

                Me.Controls.Remove(Grps(iloop))
                Grps(iloop).Dispose()
                ''Me.Controls.Remove(SetPanel(iloop))
                ''Me.Controls.Remove(LabelNo(iloop))
                ''Me.Controls.Remove(SetPanelR(iloop))
                'SetPanel(iloop).Visible = False

                SetPanelR(iloop).Visible = False

                tableRow(iloop).Visible = False                                 ' 2016/01/29 Kino Add これで行が削除できる

                ''Table3.Rows.RemoveAt(rowCount)                                '' これにすると、ヴァリデーションコントロールでエラーが出てしまうので、JSでやることにする
                'Me.Controls.Remove(tableRow(iloop))
                rowCount -= 1
            End If
        Next

        '======================================================== [ グラフサイズ・表示設定 ] ================================================↓
        For iloop = 0 To GrpProp.GraphBoxCount - 1
            With Grps(iloop)

                'グラフサイズの調整
                Select Case GrpProp.PaperOrientaion
                    Case 0      'A4縦の場合
                        GrpHeight = Convert.ToInt32(Fix(920 / GrpProp.GraphBoxCount))            '913
                        GrpWidth = 650
                        ChartHeight = GrpHeight
                        If GrpProp.EnableLegend = 1 Then ChartWidth = 580 Else ChartWidth = 650 'ChartWidth = 520 Else ChartWidth = 620
                    Case 1      'A4横の場合
                        GrpHeight = Convert.ToInt32(Fix(583 / GrpProp.GraphBoxCount))
                        GrpWidth = 980
                        ChartHeight = GrpHeight
                        ChartWidth = 980
                        'If GrpProp.EnableLegend = 1 Then ChartWidth = 980 Else ChartWidth = 980 '860 950
                    Case 2      'A3縦の場合
                    Case 3      'A3横の場合
                End Select

                GrpBox(iloop).Height = GrpHeight                            'パネルの高さ
                GrpBox(iloop).Width = GrpWidth                              'パネルの幅
                .Height = GrpHeight                                         'WebChartの高さ
                .Width = GrpWidth                                           'WebChartの幅
                .ChartArea.Size = New System.Drawing.Size(ChartWidth, ChartHeight)         'チャートエリアの幅と高さ
                .ChartArea.Margins.SetMargins(0, 8, 10, 5)                  '2009/05/30 Kino Add

                If GrpProp.GraphCount > iloop AndAlso GrpProp.LineInfo(iloop).EnableRightAxis = True And rightAxisEnable = False Then          ' 2016/01/27 Kino Add
                    rightAxisEnable = True
                End If
                '================================================ [ グラフサイズ・表示設定 ] ================================================↑
            End With
        Next iloop

        'グラフタイトル 表示位置、フォントサイズ設定
        Dim objLabel As System.Web.UI.WebControls.Label
        Dim objLabel2 As System.Web.UI.WebControls.Label
        If GrpProp.TitlePosition = 0 Then
            objLabel = Me.LblTitleUpper
            objLabel2 = Me.LblTitleLower
            Me.GraphOuter.CssClass = "TitleMarginTop"
        Else
            objLabel = Me.LblTitleLower
            objLabel2 = Me.LblTitleUpper
            Me.GraphOuter.CssClass = "TitleMarginBottom"
        End If

        Dim AdjValue As Integer
        If OldFlg = True Then AdjValue = -3 ' 2011/04/19 Kino Add      現行の設定と合わせるため
        objLabel.Text = GrpProp.GraphTitle
        objLabel.Font.Size = GrpProp.DrawSizeColorInf(0) + AdjValue     ' 2011/04/19 現行の設定と合わせるため
        objLabel.Width = GrpWidth
        Me.LblDateSpan.Width = GrpWidth - 10                                                ' 2011/03/28 Kino Add -10
        ''Me.LblDateSpan.Text = "表示期間：" + Date.Parse(Me.TxtStartDate.Text).ToString("yyyy/MM/dd HH:mm") + "～" + Date.Parse(Me.TxtEndDate.Text).ToString("yyyy/MM/dd HH:mm")

        objLabel2.Visible = False
        objLabel2.Dispose()

        For iloop = 0 To 5
            Grps(iloop) = Nothing
            GrpBox(iloop) = Nothing
            ''SetPanel(iloop) = Nothing
            ''LabelNo(iloop) = Nothing
            SetPanelR(iloop) = Nothing
        Next

        For iloop = 1 To 6                                              ' 2015/11/25 Kino Add
            RExValMax(iloop) = Nothing
            RExValMin(iloop) = Nothing
            RExValMaxR(iloop) = Nothing
            RExValMinR(iloop) = Nothing
            ValidCOExMax(iloop) = Nothing
            ValidCOExMin(iloop) = Nothing
        Next

    End Sub

    ''' <summary>
    ''' 経時変化図描画メインプロシージャ
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetGraphCommonInit()

        Dim iloop As Integer
        Dim jloop As Integer
        Dim kloop As Integer
        Dim Grps(5) As C1WebChart
        Dim Defval(5) As DropDownList
        Dim DefvalR(5) As DropDownList
        ''http://dobon.net/vb/dotnet/control/buttonarray.html
        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ
        Dim AccDataFile As String       '= Server.MapPath(siteDirectory & "\App_Data\" & siteDirectory & "_CalculatedData.mdb")

        Dim intRet As Integer
        Dim StDate As Date
        Dim EdDate As Date
        Dim DataCh() As Integer
        Dim Datas() As Array = {}
        Dim RLDataCount As Integer
        Dim UsePlotArea(5) As Boolean                                                   ' マウスホバー
        Dim TimeCond As String = ""
        Dim SelItem As ListItem

        Dim strTemp As String
        Dim intInterval As Integer = 1
        Dim strRangeType As String = ""
        Dim DataFile As String
        Dim clsNewDate As New ClsReadDataFile
        Dim StartDateTime As Date
        Dim LastDateTime As Date
        Dim DataCounts As Integer = 0

        Dim maxValueLabelLength As Integer = 0                                          ' 2012/08/02 Kino Add
        Dim maxValueLabelLengthR As Integer = 0                                         ' 2012/08/23 Kino Add
        Dim rightAxisOpe As Short = 0                                                   ' 2012/08/23 Kino Add
        Dim pixelperpoint = 96 / 72                                                     ' 2013/06/21 Kino Add

        Dim XaxisDate() As DateTime = Nothing                                           ' 2016/10/25 Kino Add
        Dim YData() As YDataSet = Nothing                                               ' 2016/10/25 Kino Add

        'オブジェクトの指定:グラフ
        For iloop = 1 To 6
            Grps(iloop - 1) = CType(Form.FindControl("WC" + iloop.ToString), C1WebChart)
            Defval(iloop - 1) = CType(Form.FindControl("DdlScale" + iloop.ToString), DropDownList)
            DefvalR(iloop - 1) = CType(Form.FindControl("DdlScaleR" + iloop.ToString), DropDownList)
        Next

        ' ''データを拾う範囲の計算
        ''If Me.RblDayRange.SelectedValue = "01D" Then
        ''    StDate = Date.Parse(Date.Parse(Me.TxtStartDate.Text).ToShortDateString & " 0:00")   '横軸の最小日付 表示範囲が1日の場合はその日の0時から表示する
        ''Else
        ''    StDate = Date.Parse(Me.TxtStartDate.Text)                               '横軸の最小日付
        ''End If
        ''EdDate = Date.Parse(Me.TxtEndDate.Text).AddDays(1)                          '横軸の最大日付

        DataFile = Server.MapPath(siteDirectory + "\App_Data\MenuInfo.mdb")
        intRet = clsNewDate.GetDataFileNames(DataFile, DataFileNames)                       'データファイル名、共通情報ファイル名、識別名を取得

        EdDate = clsNewDate.CalcEndDate(Me.RdBFromNewest.Checked, GrpProp.LastUpdate, Me.TxtEndDate.Text, 0)

        strTemp = Me.DDLRange.SelectedValue
        intInterval = Integer.Parse(strTemp.Substring(0, 2))
        strRangeType = strTemp.Substring(2, 1)

        If Me.RdBFromNewest.Checked = True Then                                 '最新データから 
            Select Case strRangeType
                Case "A"
                    ''StDate = CType(Session.Item("OldestDate"), String)        ’2009/02/02 Kino Changed
                    StDate = GrpProp.StartDate                                  ' 2009/02/02 Kino Add
                Case Else                                                       '指定日数、月、年
                    StDate = clsNewDate.CalcStartDate(Me.TxtStartDate.Text, EdDate, strRangeType, intInterval, 0)
            End Select
        Else                                                                    '指定期間

            StDate = Date.Parse(Me.TxtStartDate.Text)

        End If

        For iloop = 0 To GrpProp.GraphCount - 1
            AccDataFile = Server.MapPath(siteDirectory + "\App_Data\" + DataFileNames(GrpProp.LineInfo(iloop).DataFileNo).FileName)          'データファイル名を設定

            With Grps(iloop)

                If Convert.ToInt32(Me.DdlGraphColorType.SelectedValue) = 1 Then                                 'グラフの色をグレースケールにするかどうか
                    Grps(iloop).UseGrayscale = True
                End If

                '●●●●●　データ系列
                RLDataCount = GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1                                    '左軸の合計データ系列数
                If GrpProp.LineInfo(iloop).EnableRightAxis = True Then
                    RLDataCount = GrpProp.LineInfo(iloop).LeftAxis.DataCount + GrpProp.LineInfo(iloop).RightAxis.DataCount - 1  '左右軸の合計データ系列数(左-1+右-1)
                End If
                ReDim DataCh(RLDataCount)

                For jloop = 0 To GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1                                 '左軸描画データのチャンネル番号
                    DataCh(jloop) = Convert.ToInt32(GrpProp.LineInfo(iloop).LeftAxisData(jloop).ChNo)
                Next
                If GrpProp.LineInfo(iloop).EnableRightAxis = True Then                                          '右軸が有効なら
                    For jloop = GrpProp.LineInfo(iloop).LeftAxis.DataCount To RLDataCount                       '　右軸描画データのチャンネル番号
                        DataCh(jloop) = Convert.ToInt32(GrpProp.LineInfo(iloop).RightAxisData(jloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).ChNo)
                    Next
                End If

                '---間引き---
                TimeCond = ""                                                                                   '' 2009/07/15 Kino Add
                If Me.ChbPartial.Checked = True Then
                    For Each SelItem In Me.CBLPartial.Items
                        If SelItem.Selected = True Then
                            'TimeCond += (Integer.Parse((SelItem.Text).Replace(":00", "")).ToString + ",")      ' 2016/10/31 Kino Changed
                            TimeCond &= (Convert.ToInt32(SelItem.Text.Replace(":00", "")).ToString + ",")       ' 2016/10/31 Kino Add
                        End If
                    Next
                    If TimeCond.Length > 1 Then
                        'TimeCond = TimeCond.Substring(0, TimeCond.Length - 1)
                        TimeCond = TimeCond.TrimEnd(","c)                                                       ' 2016/10/31 Kino Add
                    End If
                End If

                '描画データの読み込み
                intRet = clsNewDate.GetDrawData(AccDataFile, Convert.ToInt16(RLDataCount), DataCh, StDate, EdDate, Datas, TimeCond)

                .ChartGroups.Group0.ChartData.SeriesList.Clear()                                                            '左軸のデータ系列を一括削除
                .ChartGroups.Group1.ChartData.SeriesList.Clear()                                                            '右軸のデータ系列を一括削除

                .ChartGroups.Group0.ChartData.Hole = CType("7.7E+30", Single)                                               '左軸の描画しないデータ 2012/11/28 Kino Add 場所移動 & Single 変換 これだったっとは・・・ orz
                .ChartGroups.Group1.ChartData.Hole = CType("7.7E+30", Single)                                               '右軸の描画しないデータ 2012/11/28 Kino Add 場所移動 & Single 変換 これだったっとは・・・ orz

                'DataCounts = UBound(Datas, 1)
                DataCounts = Datas.Length - 1
                If DataCounts <= 1008 Then UsePlotArea(iloop) = True ' 1008データまでなら、ポップアップを出す (10分間隔で7日間)

                '--------------------------------------------------------------------------------------------------------------' 2016/10/25 Kino Add
                ' ''Dim sw As New System.Diagnostics.Stopwatch
                ' ''sw.Start()
                ReDim XaxisDate(DataCounts)                                                                                 '日付はグラフ内で共通なので、変数に取っておくことで高速化を図る
                For jloop = 0 To DataCounts
                    XaxisDate(jloop) = Convert.ToDateTime(Datas(jloop).GetValue(0))
                Next
                '--------------------------------------------------------------------------------------------------------------
                ReDim YData(RLDataCount)        'GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1)                                                 ' Y軸データ用　系列数分を日付分回す
                For kloop = 0 To RLDataCount    'GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1                                             ' 系列数毎に、日付分の配列を切り出す
                    ReDim YData(kloop).Datas(DataCounts)
                Next

                For kloop = 0 To RLDataCount        'GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1
                    For jloop = 0 To DataCounts
                        YData(kloop).Datas(jloop) = Convert.ToDouble(Datas(jloop).GetValue(kloop + 1))                      ' データを入れる
                    Next jloop
                Next kloop
                ''For jloop = 0 To DataCounts                                                                               ' 縦ループを先にした場合数％だけ遅くなるので、上の「先に横ループ」を採用
                ''    For kloop = 0 To GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1
                ''        YData(kloop).Datas(jloop) = Convert.ToDouble(Datas(jloop).GetValue(kloop + 1))                      ' データを入れる
                ''    Next kloop
                ''Next jloop
                ' ''sw.Stop()
                ' ''System.Diagnostics.Debug.WriteLine(String.Format(" {0} 回目 {1} ミリ秒", iloop, sw.ElapsedMilliseconds))
                ' ''sw.Reset()
                '--------------------------------------------------------------------------------------------------------------

                For kloop = 0 To GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1                                             'データ系列数分ループ

                    Dim series As ChartDataSeries = .ChartGroups.Group0.ChartData.SeriesList.AddNewSeries()                 'データ系列のインスタンスを作成
                    With series '.ChartGroups(0).ChartData.SeriesList(kloop - 1)

                        .PointData.Length = DataCounts    'GrpProp.LineInfo(iloop).LeftAxis.DataCount
                        .SymbolStyle.Size = GrpProp.LineInfo(iloop).LeftAxisData(kloop).MarkSize                            'マーカのサイズ　
                        .SymbolStyle.Shape = CType(GrpProp.LineInfo(iloop).LeftAxisData(kloop).Marker, SymbolShapeEnum)     'マーカの形状
                        If GrpProp.LineInfo(iloop).LeftAxisData(kloop).MarkOutlineWidth <> 0 Then
                            .SymbolStyle.Color = Color.White
                            .SymbolStyle.OutlineWidth = GrpProp.LineInfo(iloop).LeftAxisData(kloop).MarkOutlineWidth
                            .SymbolStyle.OutlineColor = Color.FromArgb(GrpProp.LineInfo(iloop).LeftAxisData(kloop).Color)
                        Else
                            .SymbolStyle.Color = Color.FromArgb(GrpProp.LineInfo(iloop).LeftAxisData(kloop).Color)
                            .SymbolStyle.OutlineWidth = 0
                        End If
                        .LineStyle.Pattern = CType(GrpProp.LineInfo(iloop).LeftAxisData(kloop).LineType, LinePatternEnum)    '線種
                        .LineStyle.Thickness = GrpProp.LineInfo(iloop).LeftAxisData(kloop).LineWidth                        '線の太さ
                        .Label = GrpProp.LineInfo(iloop).LeftAxisData(kloop).Symbol                                         '凡例の文字
                        If .LineStyle.Pattern = LinePatternEnum.None Then                                                   ' 2016/10/13 Kino Add 線をなくすには、色を透明化しないと駄目なようだ。
                            .LineStyle.Color = Drawing.Color.Transparent
                        Else
                            .LineStyle.Color = Color.FromArgb(GrpProp.LineInfo(iloop).LeftAxisData(kloop).Color)                '線色
                        End If


                        ''                        For jloop = 0 To UBound(Datas, 1)'データ数のループ
                        '-----------------------------------------------------------------------------------------------------------
                        .X.CopyDataIn(XaxisDate)                                                                            ' 2016/10/25 Kino Add　高速化　ここは、左軸のみ　右軸は下の方
                        .Y.CopyDataIn(YData(kloop).Datas)
                        '-----------------------------------------------------------------------------------------------------------
                        ''For jloop = 0 To DataCounts                                                                         'データ数のループ
                        ''    .X.Add(Datas(jloop).GetValue(0))                                                               ' 2016/10/25 Kino Changed　高速化 ↑ 今まで3分近くかかってたのが、5秒くらいで出る－爆速
                        ''    .Y.Add(Datas(jloop).GetValue(kloop + 1))

                        ''    '' .Y.Add(Datas(jloop).GetValue(kloop + 1))
                        ''    ''    ''.X(jloop) = (Datas(jloop).GetValue(0))
                        ''    ''    ''.Y(jloop) = (Datas(jloop).GetValue(kloop))
                        ''Next jloop
                        '-----------------------------------------------------------------------------------------------------------

                    End With
                    If Me.DdlContinous.SelectedIndex = 1 Then                                                   ' 欠測値の直線補完
                        .ChartGroups(0).ChartData(kloop).Display = SeriesDisplayEnum.ExcludeHoles
                    End If
                Next kloop

                '左軸データの設定
                Select Case GrpProp.LineInfo(iloop).LeftAxis.ChartType                                          'グラフ種別設定
                    Case "BAR"
                        .ChartGroups(0).ChartType = Chart2DTypeEnum.Bar
                        .ChartGroups(0).ShowOutline = False
                        If DataCounts < 14 Then                                                                 ' 2013/01/15 Kino Add 本数が少ないと幅が広くなるための対策
                            .ChartGroups(0).Bar.ClusterWidth = 5
                        End If
                    Case Else
                        .ChartGroups(0).ChartType = Chart2DTypeEnum.XYPlot                                      '基本は散布図
                End Select

                '' == グラフのサイズなど ==
                '.ChartArea.PlotArea.LocationDefault = New Point(80, 14)
                ''Dim grpSize As Size = .ChartArea.PlotArea.Size                                                  ' 2012/11/21 Kino Add グラフサイズ(幅)を固定　暫定対応
                '.ChartArea.PlotArea.SizeDefault = New Size(720, grpSize.Height) 'New Size(720, grpSize.Height)
                .ChartArea.PlotArea.LocationDefault = New Point(GrpProp.LineInfo(iloop).locationX, GrpProp.LineInfo(iloop).locationY)       ' 2013/06/21 Kino Add
                Dim grpSize As Size = .ChartArea.Size
                Dim plotSize As Size
                plotSize.Width = CType(grpSize.Width * (GrpProp.LineInfo(iloop).widthRatio * 0.01), Integer)
                plotSize.Height = CType((grpSize.Height * GrpProp.LineInfo(iloop).heightRatio * 0.01), Integer)
                .ChartArea.PlotArea.SizeDefault = New Size(plotSize.Width, plotSize.Height)
                .ChartArea.SizeDefault = New Size(Convert.ToInt32(plotSize.Width * 1.25), grpSize.Height)

                ''データファイルが複数の場合で、1ページに複数ファイル参照設定していて表示日付範囲のデータがないファイルがあるとエラーが発生するための対策
                If Datas.Length <> 0 Then                                                                       '2010/10/26 Kino Add 
                    StartDateTime = CDate(Datas(0).GetValue(0))
                    LastDateTime = CDate(Datas(UBound(Datas, 1)).GetValue(0))
                    Me.LblDateSpan.Text = "表示期間：" + StartDateTime.ToString("yyyy/MM/dd HH:mm") + "～" + LastDateTime.ToString("yyyy/MM/dd HH:mm")             '' グラフの描画データ範囲で表示
                    ''Me.LblDateSpan.Text = "表示期間：" + StDate.ToString("yyyy/MM/dd HH:mm") + "～" + EdDate.ToString("yyyy/MM/dd HH:mm")                 '' グラフのスケール幅時刻で表示
                    ''Me.LblDateSpan.Text = "表示期間：" + StDate.ToString("yyyy/MM/dd HH:mm") + "～" + GrpProp.LastUpdate.ToString("yyyy/MM/dd HH:mm")       '' グラフに描画しているデータの時刻で表示

                    If .ChartGroups(0).ChartType = Chart2DTypeEnum.XYPlot Then
                        Dim cd As ChartData = .ChartGroups(0).ChartData                                                     '凡例表示で、スペースを入れるため
                        'Group0の最後にダミーのSeriesを追加します
                        cd.SeriesList.AddNewSeries()
                        ''cd.Item(1).LineStyle.Color = Color.White
                        ''cd.SeriesList.Item(0).Group.ChartType = Chart2DTypeEnum.XYPlot

                        'ダミーのSeriesの凡例表示をクリアします
                        Dim series2 As ChartDataSeries = cd(cd.SeriesList.Count - 1)
                        series2.Label = ""
                        series2.LineStyle.Pattern = LinePatternEnum.None
                        series2.SymbolStyle.Shape = SymbolShapeEnum.None
                    End If
                    '.ChartGroups.Group0.ChartData.Hole = 7.7E+30                    '左軸の描画しないデータ   ' 2012/11/28 Kino Changed コメント 場所移動
                    '.ChartGroups.Group1.ChartData.Hole = 7.7E+30                    '右軸の描画しないデータ
                    ''.ChartGroups(0).ChartData.Hole = 7.7E+30
                    ''.ChartGroups(1).ChartData.Hole = 7.7E+30

                    '右軸データの設定
                    If GrpProp.LineInfo(iloop).EnableRightAxis = True Then                                              '右軸が有効なら
                        For kloop = GrpProp.LineInfo(iloop).LeftAxis.DataCount To RLDataCount                           '　データ系列数分ループ
                            Dim series1 As ChartDataSeries = .ChartGroups.Group1.ChartData.SeriesList.AddNewSeries()     '　データ系列のインスタンスを作成
                            With series1
                                .PointData.Length = DataCounts    'GrpProp.LineInfo(iloop).LeftAxis.DataCount
                                .SymbolStyle.Size = GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).MarkSize                          '　マーカのサイズ　
                                .SymbolStyle.Shape = CType(GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).Marker, SymbolShapeEnum)   '　マーカの形状
                                If GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).MarkOutlineWidth <> 0 Then
                                    .SymbolStyle.Color = Color.White
                                    .SymbolStyle.OutlineWidth = GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).MarkOutlineWidth
                                    .SymbolStyle.OutlineColor = Color.FromArgb(GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).Color)
                                Else
                                    .SymbolStyle.Color = Color.FromArgb(GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).Color)
                                    .SymbolStyle.OutlineWidth = 0
                                End If
                                .LineStyle.Pattern = CType(GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).LineType, LinePatternEnum)  '　線種
                                .LineStyle.Thickness = GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).LineWidth                      '　線の太さ
                                .Label = GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).Symbol                                       '　凡例の文字
                                .LineStyle.Color = Color.FromArgb(GrpProp.LineInfo(iloop).RightAxisData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).Color)              '　線色
                                '-----------------------------------------------------------------------------------------------------------
                                .X.CopyDataIn(XaxisDate)                                                                            ' 2016/10/25 Kino Add　高速化　ここは、左軸のみ　右軸は下の方
                                .Y.CopyDataIn(YData(kloop).Datas)
                                '-----------------------------------------------------------------------------------------------------------
                                ''For jloop = 0 To DataCounts                                                       '　データ数のループ
                                ''    .X.Add(Datas(jloop).GetValue(0))
                                ''    .Y.Add(Datas(jloop).GetValue(kloop + 1))
                                ''Next jloop
                                '-----------------------------------------------------------------------------------------------------------
                            End With
                            If Me.DdlContinous.SelectedIndex = 1 Then
                                .ChartGroups(1).ChartData(kloop - GrpProp.LineInfo(iloop).LeftAxis.DataCount).Display = SeriesDisplayEnum.ExcludeHoles
                            End If
                        Next kloop

                        Select Case GrpProp.LineInfo(iloop).RightAxis.ChartType                                         'グラフ種別設定
                            Case "BAR"
                                .ChartGroups(1).ChartType = Chart2DTypeEnum.Bar
                                .ChartGroups(1).ShowOutline = False
                                If DataCounts < 14 Then                                                                 ' 2013/01/15 Kino Add 本数が少ないと幅が広くなるための対策
                                    .ChartGroups(1).Bar.ClusterWidth = 5
                                End If
                            Case Else
                                .ChartGroups(1).ChartType = Chart2DTypeEnum.XYPlot                                      '基本は散布図
                        End Select

                    ElseIf rightAxisOpe <> 0 Then                                                                       ' 2012/08/23 Kino Add どこかで右軸を使っている場合はとりあえずシリーズだけ追加
                        '.ChartGroups.Group1.DrawingOrder = 0
                        '.ChartGroups(1).ChartType = Chart2DTypeEnum.XYPlot                                             '基本は散布図
                    End If
                End If

                'Erase Datas

                '================================================ [ グラフ設定 ] ================================================↓
                '●●●●●　グラフのサブタイトル
                If GrpProp.LineInfo(iloop).SubTitle IsNot Nothing AndAlso GrpProp.LineInfo(iloop).SubTitle.Length <> 0 Then
                    With .Header
                        .Text = GrpProp.LineInfo(iloop).SubTitle                 'グラフサブタイトル(左側の個々のグラフのタイトル)          ' 2013/06/21 Kino Changed
                        Dim f As System.Drawing.Font
                        f = New Font("MS UI Gothic", GrpProp.LineInfo(iloop).subTitleSize)
                        Dim ascent As Integer = Convert.ToInt32(((f.SizeInPoints + 4) * pixelperpoint) * (f.FontFamily.GetCellAscent(FontStyle.Regular) / f.FontFamily.GetEmHeight(FontStyle.Regular))) '縦位置計算
                        .Visible = True
                        .Compass = CompassEnum.North                                                                                        ' 2013/06/21 Kino Add
                        .LocationDefault = New Point(GrpProp.LineInfo(iloop).locationX, GrpProp.LineInfo(iloop).locationY - ascent)
                        .Style.Rotation = RotationEnum.Rotate0
                        .Style.Font = New Font("MS UI Gothic", GrpProp.LineInfo(iloop).subTitleSize)
                    End With
                Else
                    .Header.Visible = False
                End If

                .AlternateText = "経時変化図 No." & iloop.ToString
                .BackColor = Color.White                                        '背景色                  ●● デバッグでコメント
                .ImageAlign = ImageAlign.AbsMiddle                              '画像イメージの位置
                With .ChartArea.Style()
                    .Opaque = False                                                                       '●● デバッグでコメント
                    .VerticalAlignment = AlignVertEnum.Center                   'チャート部の垂直方向の位置
                    .BackColor = Color.Transparent                              'チャート部の背景色        ●●デバッグでコメント
                End With
                .Legend.Visible = CType(GrpProp.EnableLegend, Boolean)          '凡例の有無

                '●●●●●　X1軸設定
                ''Call SetAxisYProperties(Grps(iloop), 0, iloop, Defval)
                With .ChartArea.AxisX
                    ''.Visible = True                     
                    .Font = New Font("MS UI Gothic", 9)
                    .AnnoFormat = FormatEnum.DateManual                         'ラベルの表示方法は　手動日付
                    .AnnoFormatString = Me.DdlDateFormat.Text                   '日付の表示形式

                    Dim dtspan As TimeSpan = (EdDate - StDate)                  ' 2017/02/14 Kino Add 横軸スケールが1日だった場合は、時:分 まで表示する
                    If dtspan.TotalHours = 24 Then
                        .AnnoFormatString = (Me.DdlDateFormat.Text & " HH:mm")
                    End If

                    .ForeColor = Drawing.Color.Black                            '線色
                    .Rotation = CType(Me.TxtXAxisAngle.Text, RotationEnum)       'テキストの回転角度
                    .Compass = CompassEnum.South
                    .Alignment = StringAlignment.Near
                    .OnTop = False
                    ''.Thickness = PlotAreBorder    
                    .TickMinor = TickMarksEnum.None                          '副メモリの突出方向
                    .TickMajor = TickMarksEnum.None                          '主メモリの突出方向

                    ''If Me.RblDayRange.SelectedValue = "01D" Then
                    ''    '                        .Min = Date.Parse(Me.TxtStartDate.Text.ToString("yyyy/MM/dd 0:00")).ToOADate        '横軸の最小日付 表示範囲が1日の場合はその日の0時から表示する
                    ''    .Min = Date.Parse(Date.Parse(Me.TxtStartDate.Text).ToShortDateString & " 0:00").ToOADate
                    ''Else
                    ''    .Min = StDate.ToOADate
                    ''    '                        .Min = Date.Parse(Me.TxtStartDate.Text).ToOADate         '横軸の最小日付
                    ''End If
                    .Min = StDate.ToOADate
                    .Max = EdDate.ToOADate          'Date.Parse(Me.TxtEndDate.Text).AddDays(1).ToOADate     '横軸の最大日付
                    .GridMajor.Visible = True                                   '主メモリ表示
                    .GridMinor.Visible = True                                   '副メモリ表示
                    .AutoMajor = True                                           '主目盛の値を自動的に計算
                    .AutoMinor = True                                           '副目盛の値を自動的に計算
                    .GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                    .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                    '.GridMajor.Spacing = 1
                    '.GridMinor.Spacing = 1
                    ''.AutoOrigin = False
                    .Origin = CDate(StDate).ToOADate                       '軸の起点を最小日付とする  ←あまり意味がないようだ・・
                    'If GrpProp.LineInfo(iloop).LeftAxis.Reversed = True Then
                    '    .Compass = CompassEnum.North
                    'End If
                End With

                Call setAxisYPropInit(Grps(iloop), iloop, maxValueLabelLength, maxValueLabelLengthR, rightAxisOpe, Defval)

                '●●●●　Y1軸設定
                Call SetAxisYProperties(Grps(iloop), 0, iloop, Defval)  ', maxValueLabelLength)

                '●●●●　Y2軸設定
                If GrpProp.LineInfo(iloop).EnableRightAxis = True Then  'Or rightAxisOpe <> 0 Then                                         ' 2012/08/23 Kino Changed どこかで右軸を使っている場合も入る
                    Call SetAxisYProperties(Grps(iloop), 1, iloop, DefvalR) ', maxValueLabelLengthR, rightAxisOpe)                     ' 2012/08/23 Kino Changed 引数追加
                    ''Else
                    ''    .ChartArea.AxisY2.Visible = False
                End If

                With .ChartArea
                    .Style.Border.BorderStyle = C1.Win.C1Chart.BorderStyleEnum.None                 'チャートエリアのボーダー
                    .PlotArea.Boxed = True                              'チャートエリアのボックスを表示する
                    .PlotArea.ForeColor = Drawing.Color.Black           'ボックスの線色
                    ''.PlotArea.UseAntiAlias = False                      'プロットエリアアンチエイリアス
                    If GrpProp.LineInfo(iloop).EnableRightAxis = True Or rightAxisOpe <> 0 Then
                        Grps(iloop).Footer.Location = New System.Drawing.Point(.Size.Width + 10, -1)
                        Grps(iloop).Legend.Location = New System.Drawing.Point(CType(Grps(iloop).Width.Value, Integer) - GrpProp.LineInfo(iloop).legendWidth, .PlotArea.Location.Y)                      ' 凡例位置
                        Grps(iloop).Legend.SizeDefault = New Size(GrpProp.LineInfo(iloop).legendWidth, .PlotArea.SizeDefault.Height)
                        'Grps(iloop).Legend.Location = New System.Drawing.Point(.Location.X + .Size.Width - 40, 20)
                        'Else
                        '    Grps(iloop).Legend.Location = New System.Drawing.Point(.Location.X + .Size.Width, 15)
                    End If

                    If DDLPaintWarningValue.SelectedValue = "1" Then                  '警報値表示「する」の場合は処理を行なう
                        Call showAlertArea(iloop, DataCh, Grps(iloop), GrpProp.alertStyle)
                        ' ''●●●●　警報値領域
                        ''Dim azloop As Integer
                        ''Dim azData() As Single = {7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30}
                        ''Dim azMax As Double
                        ''Dim azMin As Double
                        ''Dim UpperFlg As Short = 0       ''2010/03/26 Kino Add   警報値の設定がない時、上下限の設定が全てスケールMAX/MINの場合に全部塗られるのの対策
                        ''Dim LowerFlg As Short = 0       ''2010/03/26 Kino Add
                        ''If GrpProp.LineInfo(iloop).spAlertValue.Length = 0 Then         '共通情報から読込み
                        ''    Call ReadAlarmData(DataCh(0), GrpProp.LineInfo(iloop).DataFileNo, azData)
                        ''Else                                                        'サブタイトルから読込み
                        ''    Dim strazData() As String = {"7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30"}

                        ''    strazData = GrpProp.LineInfo(iloop).spAlertValue.Split(","c)
                        ''    For azloop = 0 To 2
                        ''        azData(azloop) = Single.Parse(strazData(azloop))
                        ''        azData(azloop + 4) = Single.Parse(strazData(azloop + 4))
                        ''    Next
                        ''End If

                        ''For azloop = 0 To 3
                        ''    If azMax < azData(azloop) And azData(azloop) < 1.1E+30 Then azMax = azData(azloop)
                        ''    If azData(azloop) > 1.1E+30 Then
                        ''        'azData(azloop) = Convert.ToSingle(azMax)            '上限3次を塗りつぶす最大値はグラフスケールのMax  2009/03/18 Kino Changed 自動スケールの場合は最大値を取得できないので、これにした <= NG
                        ''        If .AxisY.AutoMax = True Then                               '2013/01/09 Kino Add自動スケールの場合は最小値を取得できないので、これにした
                        ''            azData(azloop) = CType(azMax * 10, Single)
                        ''        End If
                        ''        If .AxisY.AutoMax = False AndAlso azMax < .AxisY.Max Then
                        ''            azData(azloop) = Convert.ToSingle(.AxisY.Max)   '上限3次を塗りつぶす最大値はグラフスケールのMax
                        ''        End If
                        ''    Else
                        ''        UpperFlg = 1
                        ''    End If
                        ''    If azMin > azData(azloop + 4) And azData(azloop + 4) < 1.1E+30 Then azMin = azData(azloop + 4) '2009/03/18 Kino Changed 自動スケールの場合は最小値を取得できないので、これにした
                        ''    If azData(azloop + 4) > 1.1E+30 Then
                        ''        ''azData(azloop + 4) = Convert.ToSingle(azMin)
                        ''        If .AxisY.AutoMin = True Then                               '2013/01/09 Kino Add自動スケールの場合は最小値を取得できないので、これにした
                        ''            azData(azloop + 4) = CType(azMin * 10, Single)
                        ''            If azData(azloop + 4) > 0 Then azData(azloop + 4) *= -1
                        ''        End If
                        ''        If .AxisY.AutoMin = False AndAlso azMin > .AxisY.Min Then
                        ''            azData(azloop + 4) = Convert.ToSingle(.AxisY.Min)
                        ''        End If
                        ''    Else
                        ''        LowerFlg = 1
                        ''    End If
                        ''Next azloop

                        ''.PlotArea.AlarmZones.Clear()
                        ''Dim azs As AlarmZonesCollection = .PlotArea.AlarmZones
                        ''Dim az As AlarmZone = azs.AddNewZone
                        ''For azloop = 1 To 3
                        ''    If UpperFlg = 1 Then
                        ''        az = azs.AddNewZone
                        ''        az.BackColor = Color.FromArgb(15 + (azloop - 1) * 12, Color.Red)           '30->15 若干透明度調整
                        ''        ' '' ''If azloop = 1 Then
                        ''        ' '' ''    az.BackColor = Color.FromArgb(40, Color.Red)
                        ''        ' '' ''Else
                        ''        ' '' ''    az.BackColor = Color.FromArgb(40, Color.Transparent)
                        ''        ' '' ''End If
                        ''        az.Shape = AlarmZoneShapeEnum.Rectangle
                        ''        az.Name = "RectangleZone上" + azloop.ToString
                        ''        az.ForeColor = Color.Transparent
                        ''        ' '' ''az.UpperExtent = Single.Parse(1000) 
                        ''        If Convert.ToSingle(azData(azloop)) <> Convert.ToSingle(azData(azloop - 1)) Then
                        ''            az.UpperExtent = Convert.ToSingle(azData(azloop))
                        ''            az.LowerExtent = Convert.ToSingle(azData(azloop - 1))
                        ''            'az.FarExtent = 4.5
                        ''            'az.NearExtent = 3
                        ''            az.Visible = True
                        ''        End If
                        ''    End If
                        ''    ' '' ''Dim series3 As ChartDataSeries = Grps(iloop).ChartGroups.Group0.ChartData.SeriesList.AddNewSeries()         'データ系列のインスタンスを作成
                        ''    ' '' ''With series3 '.ChartGroups(0).ChartData.SeriesList(kloop - 1)
                        ''    ' '' ''    .SymbolStyle.Shape = SymbolShapeEnum.None
                        ''    ' '' ''    .LineStyle.Pattern = azloop
                        ''    ' '' ''    .LineStyle.Color = Color.Red
                        ''    ' '' ''    .LineStyle.Thickness = 2
                        ''    ' '' ''    .Label = azloop.ToString & "次管理値"
                        ''    ' '' ''    .X.Add(Grps(iloop).ChartArea.AxisX.Min)
                        ''    ' '' ''    .Y.Add(Single.Parse(azData(azloop - 1)))
                        ''    ' '' ''    .X.Add(Grps(iloop).ChartArea.AxisX.Max)
                        ''    ' '' ''    .Y.Add(Single.Parse(azData(azloop - 1)))
                        ''    ' '' ''End With
                        ''    If LowerFlg = 1 Then
                        ''        az = azs.AddNewZone
                        ''        az.BackColor = Color.FromArgb(15 + (azloop - 1) * 12, Color.Red)
                        ''        az.ForeColor = Color.Transparent
                        ''        az.Shape = AlarmZoneShapeEnum.Rectangle
                        ''        az.Name = "RectangleZone下" + azloop.ToString
                        ''        If Convert.ToSingle(azData(azloop - 1 + 4)) <> Convert.ToSingle(azData(azloop + 4)) Then
                        ''            az.UpperExtent = Convert.ToSingle(azData(azloop - 1 + 4))
                        ''            az.LowerExtent = Convert.ToSingle(azData(azloop + 4))
                        ''            az.Visible = True
                        ''        End If
                        ''    End If
                        ''Next

                        '' '' ''ダミー行作成
                        ' '' ''Dim series As ChartDataSeries = Grps(iloop).ChartGroups.Group0.ChartData.SeriesList.AddNewSeries()         'データ系列のインスタンスを作成
                        ' '' ''With series '.ChartGroups(0).ChartData.SeriesList(kloop - 1)
                        ' '' ''    .SymbolStyle.Shape = SymbolShapeEnum.None
                        ' '' ''    .LineStyle.Pattern = LinePatternEnum.None
                        ' '' ''    .LineStyle.Thickness = 0                    '線の太さ
                        ' '' ''    .Label = ""                                 '凡例の文字
                        ' '' ''    .LineStyle.Color = Color.Transparent        '線色
                        ' '' ''    .X.Add(0)
                        ' '' ''    .Y.Add(0)
                        ' '' ''End With
                    End If

                End With

                ''現段階では使用しない
                '' ''.UseAntiAliasedText = False                               'グラフテキストアンチエイリアス　’’
                '' ''With .ChartArea
                '' ''    .Style.Border.BorderStyle = BorderStyleEnum.None
                '' ''End With      
                '' ''.Header.Visible = True                                    '初期設定済み
                '' ''.Header.Style.Rotation = RotationEnum.Rotate270           
                '' ''.ChartStyle.Border.BorderStyle = BorderStyleEnum.None
            End With

        Next iloop

        ' グラフの頂点でマウスホバーによるデータ表示　500データ未満なら処理する
        If DataCounts < 500 Then
            For iloop = 0 To GrpProp.GraphCount - 1
                With Grps(iloop)
                    ''データ数が増えると、とたんに重くなるので、使用を今後検討してから採用する
                    ''グラフデータにマウスを当てるとデータをツールチップで表示する
                    Dim mac As MapAreaCollection = .ImageAreas
                    ''Dim mapPA As MapArea = mac.GetByName("PlotArea")
                    Dim mapData As MapArea = mac.GetByName("ChartData")

                    If UsePlotArea(iloop) = True Then
                        .EnableCallback = True
                        ''mapPA.Tooltip = "プロット領域です"                                  'データ以外の表示
                        ''mapPA.Attributes = "onclick=""ShowDataCoords(event,WC1);"""      'クリック時の表示
                        mapData.Tooltip = "測定日時　：{#XVAL:yyyy/MM/dd HH:mm}<br />数値データ：{#YVAL:0.00}"                               'データの表示
                        'mapData.Tooltip = "測定日時：{#XVAL:yyyy/MM/dd HH:mm}　" & GrpProp.LineInfo(iloop).LeftAxis.DataTitle & "：{#YVAL:0.00} " & GrpProp.LineInfo(iloop).LeftAxis.UnitTitle                               'データの表示
                    Else
                        .EnableCallback = False
                        'mapPA.Attributes = String.Empty
                        'mapPA.HRef = String.Empty
                        'mapPA.Tooltip = String.Empty
                        mapData.Attributes = String.Empty
                        mapData.HRef = String.Empty
                        mapData.Tooltip = String.Empty
                    End If
                End With
            Next iloop
        End If
        '================================================ [ グラフ設定 ] ================================================↑

        For iloop = 0 To 5
            Grps(iloop) = Nothing
            Defval(iloop) = Nothing
            DefvalR(iloop) = Nothing
        Next
        ''Grps(0) = Nothing
        ''Grps(1) = Nothing
        ''Grps(2) = Nothing
        ''Grps(3) = Nothing
        ''Grps(4) = Nothing
        ''Grps(5) = Nothing
        ''Defval(0) = Nothing
        ''Defval(1) = Nothing
        ''Defval(2) = Nothing
        ''Defval(3) = Nothing
        ''Defval(4) = Nothing
        ''Defval(5) = Nothing
        ''DefvalR(0) = Nothing
        ''DefvalR(1) = Nothing
        ''DefvalR(2) = Nothing
        ''DefvalR(3) = Nothing
        ''DefvalR(4) = Nothing
        ''DefvalR(5) = Nothing

    End Sub


    ''' <summary>
    ''' 警報値エリアを描画(エリア or 線))
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub showAlertArea(iloop As Integer, DataCh() As Integer, Grps As C1WebChart, Optional ByVal oldStyle As Integer = 0)      ' 2014/05/16 Kino Changed   Add oldStyle

        '●●●●　警報値領域
        Dim azloop As Integer
        Dim azData() As Single = {7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30}
        Dim azMax As Double
        Dim azMin As Double
        Dim UpperFlg As Short = 0       ''2010/03/26 Kino Add   警報値の設定がない時、上下限の設定が全てスケールMAX/MINの場合に全部塗られるのの対策
        Dim LowerFlg As Short = 0       ''2010/03/26 Kino Add

        With Grps.ChartArea

            If GrpProp.LineInfo(iloop).spAlertValue Is Nothing OrElse GrpProp.LineInfo(iloop).spAlertValue.Length = 0 Then         '共通情報から読込み
                Call ReadAlarmData(DataCh(0), GrpProp.LineInfo(iloop).DataFileNo, azData)
            Else                                                        'サブタイトルから読込み
                Dim strazData() As String = {"7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30"}

                strazData = GrpProp.LineInfo(iloop).spAlertValue.Split(","c)
                For azloop = 0 To 2
                    azData(azloop) = Single.Parse(strazData(azloop))
                    azData(azloop + 4) = Single.Parse(strazData(azloop + 4))
                Next
            End If

            For azloop = 0 To 3
                If azMax < azData(azloop) And azData(azloop) < 1.1E+30 Then azMax = azData(azloop)
                If azData(azloop) > 1.1E+30 Then
                    'azData(azloop) = Convert.ToSingle(azMax)            '上限3次を塗りつぶす最大値はグラフスケールのMax  2009/03/18 Kino Changed 自動スケールの場合は最大値を取得できないので、これにした <= NG
                    If .AxisY.AutoMax = True Then                               '2013/01/09 Kino Add自動スケールの場合は最小値を取得できないので、これにした
                        azData(azloop) = CType(azMax * 10, Single)
                    End If
                    If .AxisY.AutoMax = False AndAlso azMax < .AxisY.Max AndAlso azloop > 0 Then        ' 2014/05/16 Kino Change  ADD azloop
                        azData(azloop) = Convert.ToSingle(.AxisY.Max)   '上限3次を塗りつぶす最大値はグラフスケールのMax
                    Else                                                ' 2014/05/16 Kino Add 1次管理値がない場合を想定していなかったので、後から追加
                        azData(azloop) = 7.7E+30
                    End If
                Else
                    UpperFlg = 1
                End If
                If azMin > azData(azloop + 4) And azData(azloop + 4) < 1.1E+30 Then azMin = azData(azloop + 4) '2009/03/18 Kino Changed 自動スケールの場合は最小値を取得できないので、これにした
                If azData(azloop + 4) > 1.1E+30 Then
                    ''azData(azloop + 4) = Convert.ToSingle(azMin)
                    If .AxisY.AutoMin = True Then                               '2013/01/09 Kino Add自動スケールの場合は最小値を取得できないので、これにした
                        azData(azloop + 4) = CType(azMin * 10, Single)
                        If azData(azloop + 4) > 0 Then azData(azloop + 4) *= -1
                    End If
                    If .AxisY.AutoMin = False AndAlso azMin > .AxisY.Min AndAlso azloop > 0 Then        ' 2014/05/16 Kino Change  ADD azloop
                        azData(azloop + 4) = Convert.ToSingle(.AxisY.Min)
                    Else                                                ' 2014/05/16 Kino Add
                        azData(azloop + 4) = 7.7E+30
                    End If
                Else
                    LowerFlg = 1
                End If
            Next azloop

            '.PlotArea.AlarmZones.Clear()                                                       ' 2013/06/27 Kino Changed  showWaterLevelLine のためクリアしない
            Dim azs As AlarmZonesCollection = .PlotArea.AlarmZones
            Dim az As AlarmZone = azs.AddNewZone
            For azloop = 1 To 3
                If UpperFlg = 1 AndAlso Convert.ToSingle(azData(azloop - 1)) < 1.1E+30 Then            ' 上限
                    az = azs.AddNewZone
                    If oldStyle = 1 Then                                                            ' 2014/05/16 Kino Add
                        'az.BackColor = Color.FromArgb(15 + (azloop - 1) * 12, Color.Red)            '30->15 若干透明度調整
                        az.BackColor = Color.FromArgb(AlertInfo(3 + azloop).alpha, Color.Red)           ' alpha値を採用する
                    Else
                        az.BackColor = Color.FromArgb(AlertInfo(3 + azloop).alpha, Color.FromArgb(AlertInfo(3 - azloop).BackColor))     ' 2015/05/16 Kino Add
                    End If
                    az.Shape = AlarmZoneShapeEnum.Rectangle
                    az.Name = "RectangleZone上" + azloop.ToString
                    az.ForeColor = Color.Transparent
                    If Convert.ToSingle(azData(azloop)) <> Convert.ToSingle(azData(azloop - 1)) Then
                        az.UpperExtent = Convert.ToSingle(azData(azloop))
                        az.LowerExtent = Convert.ToSingle(azData(azloop - 1))

                        az.Visible = True
                    End If
                End If
                If LowerFlg = 1 Then            ' 下限
                    az = azs.AddNewZone
                    If oldStyle = 1 Then                                                            ' 2014/05/16 Kino Add
                        ''az.BackColor = Color.FromArgb(15 + (azloop - 1) * 12, Color.Red)            '30->15 若干透明度調整
                        az.BackColor = Color.FromArgb(AlertInfo(3 - azloop).alpha, Color.Red)           ' alpha値を採用する
                    Else
                        az.BackColor = Color.FromArgb(AlertInfo(3 - azloop).alpha, Color.FromArgb(AlertInfo(3 + azloop).BackColor))     ' 2015/05/16 Kino Add
                    End If
                    az.ForeColor = Color.Transparent
                    az.Shape = AlarmZoneShapeEnum.Rectangle
                    az.Name = "RectangleZone下" + azloop.ToString
                    If Convert.ToSingle(azData(azloop - 1 + 4)) <> Convert.ToSingle(azData(azloop + 4)) Then
                        az.UpperExtent = Convert.ToSingle(azData(azloop - 1 + 4))
                        az.LowerExtent = Convert.ToSingle(azData(azloop + 4))
                        az.Visible = True
                    End If
                End If
            Next
        End With

    End Sub

    Protected Sub ReadAlarmData(ByVal DataCh As Integer, ByVal FileNum As Integer, ByRef azData() As Single)
        ''
        '' 警報値を共通情報から読込む
        ''
        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ
        Dim DataFile As String
        Dim intRet As Integer
        Dim clsNewDate As New ClsReadDataFile
        Dim outCh() As Integer = {}
        Dim sqlOpt As String
        Dim iloop As Integer
        Dim strSQL As String

        DataFile = Server.MapPath(siteDirectory + "\App_Data\MenuInfo.mdb")
        intRet = clsNewDate.GetDataFileNames(DataFile, DataFileNames)                           'データファイル名、共通情報ファイル名、識別名を取得

        Dim AccDataFile As String = Server.MapPath(siteDirectory + "\App_Data\" + DataFileNames(FileNum).CommonInf) ''siteDirectory & "_CalculatedData.mdb")

        sqlOpt = "DataBaseCh IN(" + DataCh.ToString + ")"
        ''Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim DbCon As New OleDb.OleDbConnection

        strSQL = "SELECT 警報値上限1,警報値上限2,警報値上限3,警報値下限1,警報値下限2,警報値下限3 FROM 共通情報 WHERE " + sqlOpt

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + AccDataFile + ";" & "Jet OLEDB:Engine Type= 5")
        ''DbCon.Open()
        DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
        DbDa.Fill(DtSet, "DData")
        ''DbCon.Close()

        For iloop = 0 To 2
            azData(iloop) = Convert.ToSingle(DtSet.Tables("DData").Rows(0).Item(iloop).ToString)
            azData(iloop + 4) = Convert.ToSingle(DtSet.Tables("DData").Rows(0).Item(iloop + 3).ToString)
        Next

        DbDa.Dispose()
        ''DbCom.Dispose()
        DtSet.Dispose()
        DbCon.Dispose()

    End Sub

    Protected Sub setAxisYPropInit(ByVal tWC As C1WebChart, ByVal loopCount As Integer, ByVal maxValueLabelLength As Integer, ByVal maxValueLabelLengthR As Integer, _
                                    ByVal rightAxisOpe As Short, ByVal defval() As DropDownList)

        With tWC

            With .ChartArea.AxisY
                .Text = ""
                .Font = New Font("MS UI Gothic", 9)
                .AnnoMethod = AnnotationMethodEnum.Mixed
                Dim dummy1 As ValueLabel = tWC.ChartArea.AxisY.ValueLabels.AddNewLabel()
                dummy1.Color = tWC.ChartArea.PlotArea.BackColor
                dummy1.GridLine = False
                dummy1.NumericValue = GrpProp.LineInfo(loopCount).LeftAxis.Min
                If GrpProp.LineInfo(loopCount).LeftAxis.DefaultSet = True Then  '既定値か入力値か
                    If defval(loopCount).SelectedValue <> "9999" Then
                        dummy1.NumericValue = -CDbl(defval(loopCount).SelectedValue)                             'スケール最小値
                    End If
                Else
                    dummy1.NumericValue = GrpProp.LineInfo(loopCount).LeftAxis.Min
                End If

                dummy1.Text = "_".PadLeft(maxValueLabelLength, " "c)            ' <== 指定文字分のスペースを確保します。
            End With

            If rightAxisOpe >= 1 Then
                With .ChartArea.AxisY2
                    .Text = "ダミー"
                    .Font = New Font("MS UI Gothic", 9)
                    If GrpProp.LineInfo(loopCount).EnableRightAxis = False Then
                        Dim series As ChartDataSeries = tWC.ChartGroups.Group1.ChartData.SeriesList.AddNewSeries()
                        series.LegendEntry = False
                        .ForeColor = tWC.ChartArea.PlotArea.BackColor
                        .TickMajor = TickMarksEnum.Outside
                        .TickMinor = TickMarksEnum.Outside
                        .SetMinMax(GrpProp.LineInfo(loopCount).LeftAxis.Min, GrpProp.LineInfo(loopCount).LeftAxis.Max)
                        .AnnoFormat = FormatEnum.NumericManual
                        .AnnoFormatString = "_".PadLeft(maxValueLabelLengthR + 1, "0"c)
                    End If

                    .AnnoMethod = AnnotationMethodEnum.Mixed
                    .Compass = CompassEnum.East
                    Dim dummy2 As ValueLabel = tWC.ChartArea.AxisY2.ValueLabels.AddNewLabel()
                    dummy2.Color = tWC.ChartArea.PlotArea.BackColor
                    dummy2.GridLine = False
                    ''dummy2.NumericValue = GrpProp.LineInfo(loopCount).LeftAxis.min
                    dummy2.Text = "_".PadLeft(maxValueLabelLengthR, " "c)       ' <== 指定文字分のスペースを確保します。
                    .Visible = True
                End With
            End If

        End With

    End Sub


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="tWC"></param>
    ''' <param name="leftRightFlg">処理する軸指定　0:左軸　1:右軸</param>
    ''' <param name="loopCount"></param>
    ''' <param name="defval"></param>
    ''' <remarks> 2012/08/23 Update 引数[rightAxisOpe]追加</remarks>
    Protected Sub SetAxisYProperties(ByVal tWC As C1WebChart, ByVal leftRightFlg As Integer, ByVal loopCount As Integer, ByVal defval() As DropDownList) ', _
        'ByVal maxValueLabelLength As Integer, Optional ByVal rightAxisOpe As Short = 0, Optional ByVal EnableRightAxis As Boolean = False)
        ''
        '' データ軸関連の設定
        ''
        '' 2016/04/21 縦軸スケールの変数 Singleを ToSringで文字列に変換した後でConvert.toでDoubleに変換するようにした（誤差が出てしまうため）

        Dim SetAxis As Axis '.Win.C1Chart.Axis                                      '軸の設定オブジェクト変数
        Dim StructureSetAxis As AxisPropaerties                                 '軸プロパティ構造体変数

        If leftRightFlg = 0 Then                                                '左軸の場合
            SetAxis = tWC.ChartArea.AxisY
            ''tWC.Header.Text = ""        'GrpProp.LineInfo(LoopCount).SubTitle   警報値暫定対応 2008/05/22 
            ''tWC.Header.Visible = True
            StructureSetAxis = GrpProp.LineInfo(loopCount).LeftAxis
        Else                                                                    '右軸の場合
            SetAxis = tWC.ChartArea.AxisY2
            ''tWC.Footer.Text = GrpProp.LineInfo(loopCount).SubTitle
            ''tWC.Footer.Visible = True
            StructureSetAxis = GrpProp.LineInfo(loopCount).RightAxis
        End If

        With tWC
            With SetAxis
                .Visible = True
                .ForeColor = Drawing.Color.Black                                '線色
                ''.Thickness = PlotAreBorder
                .TickMinor = TickMarksEnum.None                              '副メモリの突出方向
                .TickMajor = TickMarksEnum.None                              '主メモリの突出方向
                If leftRightFlg = 0 Then
                    .Text = (StructureSetAxis.DataTitle + StructureSetAxis.UnitTitle + Environment.NewLine + Environment.NewLine) '軸タイトル
                Else
                    .Text = (Environment.NewLine + StructureSetAxis.DataTitle + StructureSetAxis.UnitTitle + Environment.NewLine + Environment.NewLine) '軸タイトル
                End If
                .GridMajor.AutoSpace = True                                     '主メモリのグリッド線の間隔を自動的に計算
                .GridMinor.AutoSpace = True                                     '副メモリのグリッド線の間隔を自動的に計算
                .AutoMajor = True                                               '主目盛の値を自動的に計算
                .AutoMinor = True                                               '副目盛の値を自動的に計算
                .GridMinor.Visible = GrpProp.LineInfo(loopCount).LeftAxis.LogAxis                   ' 2011/12/21 Kino Add 副メモリ線の表示は対数の設定と合わせる

                If StructureSetAxis.DefaultSet = True Then  '既定値か入力値か
                    If defval(loopCount).SelectedValue = "9999" Then
                        .AutoMax = True
                        .AutoMin = True
                    Else
                        .Max = Convert.ToDouble(defval(loopCount).SelectedValue)                            'スケール最大値 2016/04/21 Kino Changed CDb; -> Convert.to...
                        .Min = -Convert.ToDouble(defval(loopCount).SelectedValue)                           'スケール最小値 2016/04/21 Kino Changed CDb; -> Convert.to...
                    End If
                Else
                    If leftRightFlg = 0 Then
                        .Max = Convert.ToDouble(GrpProp.LineInfo(loopCount).LeftAxis.Max.ToString)                     'スケール最大値
                        .Min = Convert.ToDouble(GrpProp.LineInfo(loopCount).LeftAxis.Min.ToString)                     'スケール最小値
                        If GrpProp.LineInfo(loopCount).LeftAxis.UnitMajor > 0 Then
                            .UnitMajor = GrpProp.LineInfo(loopCount).LeftAxis.UnitMajor         '主メモリ幅
                        End If
                        If GrpProp.LineInfo(loopCount).LeftAxis.UnitMinor > 0 Then
                            .UnitMinor = GrpProp.LineInfo(loopCount).LeftAxis.UnitMinor     '副メモリ幅
                        End If

                        .Reversed = GrpProp.LineInfo(loopCount).LeftAxis.Reversed           '軸の反転設定
                        'If GrpProp.LineInfo(loopCount).LeftAxis.Reversed = True Then
                        .AutoOrigin = True
                        'End If
                        .IsLogarithmic = GrpProp.LineInfo(loopCount).LeftAxis.LogAxis       '軸の対数設定   2011/12/21 Kino Add
                    Else
                        .Max = Convert.ToDouble(GrpProp.LineInfo(loopCount).RightAxis.Max.ToString)                    'スケール最大値
                        .Min = Convert.ToDouble(GrpProp.LineInfo(loopCount).RightAxis.Min.ToString)                    'スケール最小値
                        If GrpProp.LineInfo(loopCount).RightAxis.UnitMajor > 0 Then
                            .UnitMajor = GrpProp.LineInfo(loopCount).RightAxis.UnitMajor        '主メモリ幅
                        End If
                        If GrpProp.LineInfo(loopCount).RightAxis.UnitMinor > 0 Then
                            .UnitMinor = GrpProp.LineInfo(loopCount).RightAxis.UnitMinor    '副メモリ幅
                        End If
                        .Reversed = GrpProp.LineInfo(loopCount).RightAxis.Reversed          '軸の反転設定

                        .IsLogarithmic = GrpProp.LineInfo(loopCount).RightAxis.LogAxis      '軸の対数設定   2011/12/21 Kino Add
                    End If
                End If

            End With
        End With

    End Sub

    ''Protected Sub RblDayRange_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    ''    ''
    ''    '' ラジオボタンのイベントハンドラ
    ''    ''
    ''    Call CalcDate(Me.DDLRange.SelectedItem.Value)

    ''End Sub

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
            Me.TxtEndDate.Text = GrpProp.LastUpdate.ToString("yyyy/MM/dd")
            ''Me.TxtStartDate.BackColor = Drawing.Color.LightGray
            ''Me.TxtEndDate.BackColor = Drawing.Color.LightGray

            ''Me.TxtStartDate.ReadOnly = True
            ''Me.TxtEndDate.ReadOnly = True
            ''Me.ImgStDate.Attributes.Clear()
            ''Me.ImgEdDate.Attributes.Clear()
            ''Me.ImgStDate.Visible = False
            ''Me.ImgEdDate.Visible = False

            endDate = clsCalc.CalcEndDate(Me.RdBFromNewest.Checked, GrpProp.LastUpdate, Me.TxtEndDate.Text, 1)

            Interval = Integer.Parse(setDateProperty.Substring(0, 2))
            DateType = setDateProperty.Substring(2, 1)

            'If Me.RdBFromNewest.Checked = True Then                                 '最新データから
            Select Case DateType
                Case "A"
                    ''startDate = CType(Session.Item("OldestDate"), String)         '2009/02/02 Kino Changed

                    Dim iloop As Integer                                            '2009/02/02 Kino Add グラフで使用しているデータファイルの中で最古日付を検索する
                    Dim dteSession As Date
                    For iloop = 0 To GrpProp.LineInfo.Length - 1
                        dteSession = CType(Session.Item("OldestDate" + GrpProp.LineInfo(iloop).DataFileNo.ToString), Date)
                        If iloop = 0 Then startDate = dteSession
                        If startDate > dteSession Then
                            startDate = dteSession
                        End If
                    Next

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

    Protected Sub SetViewState()
        ''
        '' 変数から取得した内容をビューステートに格納する     変数　→　ビューステート
        ''
        Dim iloop As Integer
        Dim jloop As Integer
        Dim strCount As String

        ViewState("GTitle") = GrpProp.GraphTitle            'グラフのタイトル
        ViewState("GBoxCount") = GrpProp.GraphBoxCount      'グラフ枠の数(パネル)
        ViewState("GCount") = GrpProp.GraphCount            'グラフの数(True Web Chartコントロール)
        ''ViewState("LUDate") = GrpProp.LastUpdate            'データの最終更新日
        ViewState("WinTitle") = GrpProp.WindowTitle         'グラフタイトル(ウィンドウタイトル)
        ViewState("AlertSyle") = GrpProp.alertStyle         ' 管理値エリアの塗りつぶしスタイル  ' 2014/05/16 Kino Add

        'DrawSizeColorInf(23)
        For iloop = 13 To 35
            ViewState("Color" + (iloop - 13).ToString) = GrpProp.DrawSizeColorInf(iloop - 13)   'グラフの各部分のフォントサイズなど
        Next iloop

        ViewState("AlertSetCount") = AlertInfo.Length - 1                                       ' 2014/05/16 Kino Add
        For iloop = 0 To AlertInfo.Length - 1
            strCount = iloop.ToString
            ''ViewState("AlertLevels" + strCount) = AlertInfo(iloop).Levels                   ''clsAlert.AlertData(iloop).Levels
            ''ViewState("AlertWords" + strCount) = AlertInfo(iloop).Words                     ''clsAlert.AlertData(iloop).Words
            ViewState("BackColors" + strCount) = AlertInfo(iloop).BackColor
            ''ViewState("ForeColors" + strCount) = AlertInfo(iloop).ForeColor
            ViewState("Alpha" + strCount) = AlertInfo(iloop).alpha
        Next iloop

        For iloop = 0 To GrpProp.GraphCount - 1
            strCount = iloop.ToString
            ViewState("GrpDataSet1L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.DataCount                           'データ数　左軸
            ViewState("GrDatapSet1R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.DataCount                          'データ数　右軸

            ViewState("GrpLineSet1" + strCount.ToString) = GrpProp.LineInfo(iloop).SubTitle                                     'サブタイトル
            ViewState("GrpLineSet2" + strCount.ToString) = GrpProp.LineInfo(iloop).EnableRightAxis                              '右軸の有効／無効
            '            ViewState("GrpLineSet0L" & strCount) = GrpProp.LineInfo(iloop).LeftAxis.DefaultSet                         '左軸   既定値かどうか
            '            ViewState("GrpLineSet1L" & strCount) = GrpProp.LineInfo(iloop).LeftAxis.DefaultValue                       '       既定値の場合の値
            ViewState("GrpLineSet2L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.DataTitle                          '       データタイトル
            ViewState("GrpLineSet3L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.UnitTitle                          '       単位タイトル
            '            ViewState("GrpLineSetL" & strCount) = GrpProp.LineInfo(iloop).LeftAxis.Max                                 '       スケール最大値
            '            ViewState("GrpLineSetL" & strCount) = GrpProp.LineInfo(iloop).LeftAxis.Min                                 '       スケール最小値
            ViewState("GrpLineSet4L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.UnitMajor                          '       主メモリ間隔    
            ViewState("GrpLineSet5L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.UnitMinor                          '       副メモリ間隔
            ViewState("GrpLineSet6L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.ChartType                          '       グラフ種別
            ViewState("GrpLineSet7L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.Reversed                           '       軸反転
            ViewState("GrpLineSet8L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.LogAxis                            '       対数軸
            ViewState("GrpLineSet9L" + strCount.ToString) = GrpProp.LineInfo(iloop).spAlertValue                                '       個別警報    '2013/06/21 Kino Add
            ViewState("GrpLineSet10L" + strCount.ToString) = GrpProp.LineInfo(iloop).locationX                                  '       チャートエリアの起点位置 X座標    '2013/06/21 Kino Add
            ViewState("GrpLineSet11L" + strCount.ToString) = GrpProp.LineInfo(iloop).locationY                                  '       チャートエリアの起点位置 Y座標    '2013/06/21 Kino Add
            ViewState("GrpLineSet12L" + strCount.ToString) = GrpProp.LineInfo(iloop).widthRatio                                 '       グラフ横幅に対するチャートエリアの割合 '2013/06/21 Kino Add
            ViewState("GrpLineSet13L" + strCount.ToString) = GrpProp.LineInfo(iloop).heightRatio                                '       グラフ高さに対するチャートエリアの割合 '2013/06/21 Kino Add
            ViewState("GrpLineSet14L" + strCount.ToString) = GrpProp.LineInfo(iloop).subTitleSize                               '       サブタイトルのフォントサイズ           '2013/06/21 Kino Add
            ViewState("GrpLineSet15L" + strCount.ToString) = GrpProp.LineInfo(iloop).legendWidth                                '       凡例の横幅                             '2013/06/21 Kino Add
            ViewState("GrpLineSet16L" + strCount.ToString) = GrpProp.LineInfo(iloop).alertType                                  '       警報表示種別                           '2013/07/17 Kino Add
            ViewState("GrpLineSet17L" + strCount.ToString) = GrpProp.LineInfo(iloop).showAlertValue                             '       警報値数値表示                         '2013/07/17 Kino Add
            ViewState("GrpLineSet18L" + strCount.ToString) = GrpProp.LineInfo(iloop).alertValuePosition                         '       警報値数値表示の場合の左から位置割合   '2013/07/17 Kino Add

            'ViewState("GrpLineSet9L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxis.ValueFormat                       '       軸ラベルフォーマット

            '            ViewState("GrpLineSet0R" & strCount) = GrpProp.LineInfo(iloop).RightAxis.DefaultSet                        '右軸   既定値かどうか
            '            ViewState("GrpLineSet1R" & strCount) = GrpProp.LineInfo(iloop).RightAxis.DefaultValue                      '       既定値の場合の値
            ViewState("GrpLineSet2R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.DataTitle                         '       データタイトル
            ViewState("GrpLineSet3R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.UnitTitle                         '       単位タイトル
            '            ViewState("GrpLineSetR" & strCount) = GrpProp.LineInfo(iloop).RightAxis.Max                                '       スケール最大値
            '            ViewState("GrpLineSetR" & strCount) = GrpProp.LineInfo(iloop).RightAxis.Min                                '       スケール最小値
            ViewState("GrpLineSet4R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.UnitMajor                         '       主メモリ間隔    
            ViewState("GrpLineSet5R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.UnitMinor                         '       副メモリ間隔
            ViewState("GrpLineSet6R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.ChartType                         '       グラフ種別
            ViewState("GrpLineSet7R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.Reversed                          '       軸反転
            ViewState("GrpLineSet8R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.LogAxis                           '       対数軸
            'ViewState("GrpLineSet9R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxis.ValueFormat                       '       軸ラベルフォーマット

            ViewState("DataFileNo" + strCount.ToString) = GrpProp.LineInfo(iloop).DataFileNo



            '                                                                                                   ''データ系列
            For jloop = 0 To GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1
                strCount = iloop.ToString + jloop.ToString
                ViewState("GrpDataSet1L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxisData(jloop).Symbol       '左 計器記号
                ViewState("GrpDataSet2L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxisData(jloop).ChNo         '   チャンネル番号
                ViewState("GrpDataSet3L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxisData(jloop).Color        '   線色
                ViewState("GrpDataSet4L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxisData(jloop).LineType     '   線種
                ViewState("GrpDataSet5L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxisData(jloop).LineWidth    '   線幅
                ViewState("GrpDataSet6L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxisData(jloop).Marker       '   マーカ
                ViewState("GrpDataSet7L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxisData(jloop).MarkSize     '   マーカサイズ
                ViewState("GrpDataSet8L" + strCount.ToString) = GrpProp.LineInfo(iloop).LeftAxisData(jloop).MarkOutlineWidth    '   マーカ枠線
            Next jloop

            For jloop = 0 To GrpProp.LineInfo(iloop).RightAxis.DataCount - 1
                strCount = iloop.ToString + jloop.ToString
                ViewState("GrpDataSet1R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxisData(jloop).Symbol      '右 計器記号
                ViewState("GrpDataSet2R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxisData(jloop).ChNo        '   チャンネル番号
                ViewState("GrpDataSet3R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxisData(jloop).Color       '   線色
                ViewState("GrpDataSet4R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxisData(jloop).LineType    '   線種
                ViewState("GrpDataSet5R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxisData(jloop).LineWidth   '   線幅
                ViewState("GrpDataSet6R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxisData(jloop).Marker      '   マーカ
                ViewState("GrpDataSet7R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxisData(jloop).MarkSize    '   マーカサイズ
                ViewState("GrpDataSet8R" + strCount.ToString) = GrpProp.LineInfo(iloop).RightAxisData(jloop).MarkOutlineWidth   '   マーカ枠線
            Next jloop

        Next iloop

    End Sub

    Protected Sub ReadViewState()
        ''
        '' ビューステートから取得した内容を変数へ格納する　　　ビューステート　→　変数
        ''
        Dim iloop As Integer
        Dim jloop As Integer
        Dim strCnt As String
        Dim tempCount As Integer        ' 2014/05/16 Kino Add

        GrpProp.GraphTitle = CType(ViewState("GTitle"), String)                                                             'グラフのタイトル
        GrpProp.GraphBoxCount = CType(ViewState("GBoxCount"), Integer)                                                      'グラフ枠の数(パネル)
        GrpProp.GraphCount = CType(ViewState("GCount"), Integer)                                                            'グラフの数(True Web Chartコントロール)
        ''GrpProp.LastUpdate = CType(ViewState("LUDate"), Date)                                                               'データの最終更新日
        GrpProp.WindowTitle = CType(ViewState("WinTitle"), String)                                                          'グラフタイトル(ウィンドウタイトル)
        GrpProp.alertStyle = CType(ViewState("AlertSyle"), Integer)                                                         ' 管理値エリアの塗りつぶしスタイル  ' 2014/05/16 Kino Add

        ReDim GrpProp.DrawSizeColorInf(23)
        ReDim GrpProp.LineInfo(GrpProp.GraphCount - 1)                                                                      '個々のグラフの情報を格納する配列設定(グラフ個数分)

        For iloop = 13 To 35
            GrpProp.DrawSizeColorInf(iloop - 13) = CType(ViewState("Color" + (iloop - 13).ToString), Integer)               'グラフの各部分のフォントサイズなど
        Next iloop

        tempCount = CType(ViewState("AlertSetCount"), Integer)                                                              ' 2014/05/16 Kino Add
        For iloop = 0 To TempCount
            strCnt = iloop.ToString
            ''AlertInfo(iloop).Levels = CType(ViewState("AlertLevels" + strCnt), Short)
            ''AlertInfo(iloop).Words = CType(ViewState("AlertWords" + strCnt), String)
            AlertInfo(iloop).BackColor = CType(ViewState("BackColors" + strCnt), Integer)
            ''AlertInfo(iloop).ForeColor = CType(ViewState("ForeColors" + strCnt), Integer)
            AlertInfo(iloop).alpha = CType(ViewState("Alpha" + strCnt), Integer)
        Next iloop

        For iloop = 0 To GrpProp.GraphCount - 1
            strCnt = iloop.ToString
            GrpProp.LineInfo(iloop).LeftAxis.DataCount = CType(ViewState("GrpDataSet1L" + strCnt.ToString), Short)                    'データ数　左軸
            GrpProp.LineInfo(iloop).RightAxis.DataCount = CType(ViewState("GrDatapSet1R" + strCnt.ToString), Short)                   'データ数　右軸

            ReDim GrpProp.LineInfo(iloop).LeftAxisData(GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1)                      '個々のグラフのデータ系列数配列を指定
            ReDim GrpProp.LineInfo(iloop).RightAxisData(GrpProp.LineInfo(iloop).RightAxis.DataCount - 1)                    '個々のグラフのデータ系列数配列を指定

            GrpProp.LineInfo(iloop).SubTitle = CType(ViewState("GrpLineSet1" + strCnt.ToString), String)                        'サブタイトル
            GrpProp.LineInfo(iloop).EnableRightAxis = CType(ViewState("GrpLineSet2" + strCnt.ToString), Boolean)                     '右軸の有効／無効
            GrpProp.LineInfo(iloop).LeftAxis.DataTitle = CType(ViewState("GrpLineSet2L" + strCnt.ToString), String)                  '左軸　データタイトル
            GrpProp.LineInfo(iloop).LeftAxis.UnitTitle = CType(ViewState("GrpLineSet3L" + strCnt.ToString), String)                  '左軸　単位タイトル
            GrpProp.LineInfo(iloop).LeftAxis.UnitMajor = CType(ViewState("GrpLineSet4L" + strCnt.ToString), Single)                  '左軸　主メモリ間隔    
            GrpProp.LineInfo(iloop).LeftAxis.UnitMinor = CType(ViewState("GrpLineSet5L" + strCnt.ToString), Single)                  '左軸　副メモリ間隔
            GrpProp.LineInfo(iloop).LeftAxis.ChartType = CType(ViewState("GrpLineSet6L" + strCnt.ToString), String)                  '左軸　グラフ種別
            GrpProp.LineInfo(iloop).LeftAxis.Reversed = CType(ViewState("GrpLineSet7L" + strCnt.ToString), Boolean)                  '左軸　軸反転
            GrpProp.LineInfo(iloop).LeftAxis.LogAxis = CType(ViewState("GrpLineSet8L" + strCnt.ToString), Boolean)                   '左軸　対数軸
            GrpProp.LineInfo(iloop).spAlertValue = CType(ViewState("GrpLineSet9L" + strCnt.ToString), String)                       ' 個別警報  '2013/06/21 Kino Add
            GrpProp.LineInfo(iloop).locationX = CType(ViewState("GrpLineSet10L" + strCnt.ToString), Integer)                        ' チャートエリアの起点位置 X座標  '2013/06/21 Kino Add
            GrpProp.LineInfo(iloop).locationY = CType(ViewState("GrpLineSet11L" + strCnt.ToString), Integer)                        ' チャートエリアの起点位置 Y座標  '2013/06/21 Kino Add
            GrpProp.LineInfo(iloop).widthRatio = CType(ViewState("GrpLineSet12L" + strCnt.ToString), Integer)                       ' グラフ横幅に対するチャートエリアの割合   '2013/06/21 Kino Add
            GrpProp.LineInfo(iloop).heightRatio = CType(ViewState("GrpLineSet13L" + strCnt.ToString), Integer)                      ' グラフ高さに対するチャートエリアの割合   '2013/06/21 Kino Add
            GrpProp.LineInfo(iloop).subTitleSize = CType(ViewState("GrpLineSet14L" + strCnt.ToString), Single)                      ' サブタイトルのフォントサイズ             '2013/06/21 Kino Add
            GrpProp.LineInfo(iloop).legendWidth = CType(ViewState("GrpLineSet15L" + strCnt.ToString), Integer)                      '       凡例の横幅                         '2013/06/21 Kino Add
            GrpProp.LineInfo(iloop).alertType = CType(ViewState("GrpLineSet16L" + strCnt.ToString), Integer)                        '       警報表示種別                           '2013/07/17 Kino Add
            GrpProp.LineInfo(iloop).showAlertValue = CType(ViewState("GrpLineSet17L" + strCnt.ToString), Boolean)                   '       警報値数値表示                         '2013/07/17 Kino Add
            GrpProp.LineInfo(iloop).alertValuePosition = CType(ViewState("GrpLineSet18L" + strCnt.ToString), Integer)               '       警報値数値表示の場合の左から位置割合   '2013/07/17 Kino Add

            'GrpProp.LineInfo(iloop).LeftAxis.ValueFormat = CType(ViewState("GrpLineSet9L" + strCnt.ToString), String)                '左軸　軸ラベルフォーマット
            '            GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = CType(ViewState("GrpLineSet0L" & strCnt), Boolean)                '右軸　既定値かどうか
            '            GrpProp.LineInfo(iloop).LeftAxis.DefaultValue = CType(ViewState("GrpLineSet1L" & strCnt), Integer)              '右軸　既定値の場合の値

            GrpProp.LineInfo(iloop).RightAxis.DataTitle = CType(ViewState("GrpLineSet2R" + strCnt.ToString), String)                 '右軸　データタイトル
            GrpProp.LineInfo(iloop).RightAxis.UnitTitle = CType(ViewState("GrpLineSet3R" + strCnt.ToString), String)                 '右軸　単位タイトル
            GrpProp.LineInfo(iloop).RightAxis.UnitMajor = CType(ViewState("GrpLineSet4R" + strCnt.ToString), Single)                 '右軸　主メモリ間隔    
            GrpProp.LineInfo(iloop).RightAxis.UnitMinor = CType(ViewState("GrpLineSet5R" + strCnt.ToString), Single)                 '右軸　副メモリ間隔
            GrpProp.LineInfo(iloop).RightAxis.ChartType = CType(ViewState("GrpLineSet6R" + strCnt.ToString), String)                 '右軸　グラフ種別
            GrpProp.LineInfo(iloop).RightAxis.Reversed = CType(ViewState("GrpLineSet7R" + strCnt), Boolean)                          '右軸　軸反転
            GrpProp.LineInfo(iloop).RightAxis.LogAxis = CType(ViewState("GrpLineSet8R" + strCnt), Boolean)                           '右軸　対数軸
            'GrpProp.LineInfo(iloop).RightAxis.ValueFormat = CType(ViewState("GrpLineSet9R" + strCnt), String)                        '右軸　軸ラベルフォーマット
            '            GrpProp.LineInfo(iloop).RightAxis.DefaultSet = CType(ViewState("GrpLineSet0R" & strCnt), Boolean)               '右軸　既定値かどうか
            '            GrpProp.LineInfo(iloop).RightAxis.DefaultValue = CType(ViewState("GrpLineSet1R" & strCnt), Integer)             '右軸　既定値の場合の値
            GrpProp.LineInfo(iloop).DataFileNo = CType(ViewState("DataFileNo" + strCnt.ToString), Integer)                           'データファイル番号

            '                                                                                                                   ''データ系列
            For jloop = 0 To GrpProp.LineInfo(iloop).LeftAxis.DataCount - 1
                strCnt = (iloop.ToString + jloop.ToString)
                GrpProp.LineInfo(iloop).LeftAxisData(jloop).Symbol = CType(ViewState("GrpDataSet1L" + strCnt.ToString), String)             '左 計器記号
                GrpProp.LineInfo(iloop).LeftAxisData(jloop).ChNo = CType(ViewState("GrpDataSet2L" + strCnt.ToString), Integer)              '   チャンネル番号
                GrpProp.LineInfo(iloop).LeftAxisData(jloop).Color = CType(ViewState("GrpDataSet3L" + strCnt.ToString), Integer)             '   線色
                GrpProp.LineInfo(iloop).LeftAxisData(jloop).LineType = CType(ViewState("GrpDataSet4L" + strCnt.ToString), Short)            '   線種
                GrpProp.LineInfo(iloop).LeftAxisData(jloop).LineWidth = CType(ViewState("GrpDataSet5L" + strCnt.ToString), Short)           '   線幅
                GrpProp.LineInfo(iloop).LeftAxisData(jloop).Marker = CType(ViewState("GrpDataSet6L" + strCnt.ToString), Short)              '   マーカ
                GrpProp.LineInfo(iloop).LeftAxisData(jloop).MarkSize = CType(ViewState("GrpDataSet7L" + strCnt.ToString), Short)            '   マーカサイズ
                GrpProp.LineInfo(iloop).LeftAxisData(jloop).MarkOutlineWidth = CType(ViewState("GrpDataSet8L" + strCnt.ToString), Short)    '   マーカ枠線
            Next jloop

            For jloop = 0 To GrpProp.LineInfo(iloop).RightAxis.DataCount - 1
                strCnt = (iloop.ToString + jloop.ToString)
                GrpProp.LineInfo(iloop).RightAxisData(jloop).Symbol = CType(ViewState("GrpDataSet1R" + strCnt.ToString), String)            '右 計器記号
                GrpProp.LineInfo(iloop).RightAxisData(jloop).ChNo = CType(ViewState("GrpDataSet2R" + strCnt.ToString), Integer)             '   チャンネル番号
                GrpProp.LineInfo(iloop).RightAxisData(jloop).Color = CType(ViewState("GrpDataSet3R" + strCnt.ToString), Integer)            '   線色
                GrpProp.LineInfo(iloop).RightAxisData(jloop).LineType = CType(ViewState("GrpDataSet4R" + strCnt.ToString), Short)           '   線種
                GrpProp.LineInfo(iloop).RightAxisData(jloop).LineWidth = CType(ViewState("GrpDataSet5R" + strCnt.ToString), Short)          '   線幅
                GrpProp.LineInfo(iloop).RightAxisData(jloop).Marker = CType(ViewState("GrpDataSet6R" + strCnt.ToString), Short)             '   マーカ
                GrpProp.LineInfo(iloop).RightAxisData(jloop).MarkSize = CType(ViewState("GrpDataSet7R" + strCnt.ToString), Short)           '   マーカサイズ
                GrpProp.LineInfo(iloop).RightAxisData(jloop).MarkOutlineWidth = CType(ViewState("GrpDataSet8R" + strCnt.ToString), Short)   '   マーカ枠線
            Next jloop

        Next iloop

    End Sub


    ''Protected Sub ImbtnRedrawGraph_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImbtnRedrawGraph.Click
    ''    ''
    ''    '' 再描画ボタンイベント 　手動ページング
    ''    ''

    ''    Dim ScaleMax(5) As TextBox
    ''    Dim ScaleMin(5) As TextBox
    ''    Dim ScaleDef(5) As RadioButton
    ''    Dim ScaleInput(5) As RadioButton
    ''    Dim DefVal(5) As DropDownList
    ''    'Dim iloop As Integer
    ''    Dim strQuery As String
    ''    Dim strTemp As String

    ''    ScaleMax(0) = Me.TxtMax1 : ScaleMin(0) = Me.TxtMin1 : ScaleDef(0) = Me.RdbNo11 : ScaleInput(0) = Me.RdbNo12
    ''    ScaleMax(1) = Me.TxtMax2 : ScaleMin(1) = Me.TxtMin2 : ScaleDef(1) = Me.RdbNo21 : ScaleInput(1) = Me.RdbNo22
    ''    ScaleMax(2) = Me.TxtMax3 : ScaleMin(2) = Me.TxtMin3 : ScaleDef(2) = Me.RdbNo31 : ScaleInput(2) = Me.RdbNo32
    ''    ScaleMax(3) = Me.TxtMax4 : ScaleMin(3) = Me.TxtMin4 : ScaleDef(3) = Me.RdbNo41 : ScaleInput(3) = Me.RdbNo42
    ''    ScaleMax(4) = Me.TxtMax5 : ScaleMin(4) = Me.TxtMin5 : ScaleDef(4) = Me.RdbNo51 : ScaleInput(4) = Me.RdbNo52
    ''    ScaleMax(5) = Me.TxtMax6 : ScaleMin(5) = Me.TxtMin6 : ScaleDef(5) = Me.RdbNo61 : ScaleInput(5) = Me.RdbNo62

    ''    strTemp = Me.RblDayRange.SelectedIndex & "," & Me.TxtStartDate.Text & "," & Me.TxtEndDate.Text & _
    ''            Me.DdlEnableLegend.SelectedIndex & "," & Me.DdlPaperOrientation.SelectedIndex & "," & Me.DdlDateFormat.SelectedIndex


    ''    strQuery = "?DT=" & HttpUtility.UrlEncode("1")

    ''End Sub

    Protected Sub Set2FormFormControl()
        ''
        '' データベースから変数に格納した内容を、画面のコントロールへ配置する
        ''

        Dim iloop As Integer
        Dim ScaleMax(5), ScaleMaxR(5) As WebControls.TextBox
        Dim ScaleMin(5), ScaleMinR(5) As WebControls.TextBox
        Dim ScaleDef(5), ScaleDefR(5) As WebControls.RadioButton
        Dim ScaleInput(5), ScaleInputR(5) As WebControls.RadioButton
        Dim DefVal(5), DefValR(5) As DropDownList

        '左軸
        ScaleMax(0) = Me.TxtMax1 : ScaleMin(0) = Me.TxtMin1 : ScaleDef(0) = Me.RdbNo11 : ScaleInput(0) = Me.RdbNo12 : DefVal(0) = Me.DdlScale1
        ScaleMax(1) = Me.TxtMax2 : ScaleMin(1) = Me.TxtMin2 : ScaleDef(1) = Me.RdbNo21 : ScaleInput(1) = Me.RdbNo22 : DefVal(1) = Me.DdlScale2
        ScaleMax(2) = Me.TxtMax3 : ScaleMin(2) = Me.TxtMin3 : ScaleDef(2) = Me.RdbNo31 : ScaleInput(2) = Me.RdbNo32 : DefVal(2) = Me.DdlScale3
        ScaleMax(3) = Me.TxtMax4 : ScaleMin(3) = Me.TxtMin4 : ScaleDef(3) = Me.RdbNo41 : ScaleInput(3) = Me.RdbNo42 : DefVal(3) = Me.DdlScale4
        ScaleMax(4) = Me.TxtMax5 : ScaleMin(4) = Me.TxtMin5 : ScaleDef(4) = Me.RdbNo51 : ScaleInput(4) = Me.RdbNo52 : DefVal(4) = Me.DdlScale5
        ScaleMax(5) = Me.TxtMax6 : ScaleMin(5) = Me.TxtMin6 : ScaleDef(5) = Me.RdbNo61 : ScaleInput(5) = Me.RdbNo62 : DefVal(5) = Me.DdlScale6
        '右軸
        ScaleMaxR(0) = Me.TxtMaxR1 : ScaleMinR(0) = Me.TxtMinR1 : ScaleDefR(0) = Me.RdbNoR11 : ScaleInputR(0) = Me.RdbNoR12 : DefValR(0) = Me.DdlScaleR1
        ScaleMaxR(1) = Me.TxtMaxR2 : ScaleMinR(1) = Me.TxtMinR2 : ScaleDefR(1) = Me.RdbNoR21 : ScaleInputR(1) = Me.RdbNoR22 : DefValR(1) = Me.DdlScaleR2
        ScaleMaxR(2) = Me.TxtMaxR3 : ScaleMinR(2) = Me.TxtMinR3 : ScaleDefR(2) = Me.RdbNoR31 : ScaleInputR(2) = Me.RdbNoR32 : DefValR(2) = Me.DdlScaleR3
        ScaleMaxR(3) = Me.TxtMaxR4 : ScaleMinR(3) = Me.TxtMinR4 : ScaleDefR(3) = Me.RdbNoR42 : ScaleInputR(3) = Me.RdbNoR42 : DefValR(3) = Me.DdlScaleR4
        ScaleMaxR(4) = Me.TxtMaxR5 : ScaleMinR(4) = Me.TxtMinR5 : ScaleDefR(4) = Me.RdbNoR51 : ScaleInputR(4) = Me.RdbNoR52 : DefValR(4) = Me.DdlScaleR5
        ScaleMaxR(5) = Me.TxtMaxR6 : ScaleMinR(5) = Me.TxtMinR6 : ScaleDefR(5) = Me.RdbNoR61 : ScaleInputR(5) = Me.RdbNoR62 : DefValR(5) = Me.DdlScaleR6
        ''        Me.DdlGraphCount.SelectedIndex = GrpProp.GraphCount - 1                             'グラフ個数
        Me.DdlPaperOrientation.SelectedIndex = GrpProp.PaperOrientaion                      '用紙の向き
        Me.DdlEnableLegend.SelectedIndex = GrpProp.EnableLegend                             '凡例の有無
        Me.DdlContinous.SelectedIndex = GrpProp.MissingDataContinuous                       '欠測値の連結　
        Me.DDLPaintWarningValue.SelectedIndex = GrpProp.ShowAlarmArea                       '警報値表示
        Me.DdlTitlePosition.SelectedIndex = GrpProp.TitlePosition                           'タイトルの位置　0:上　1:下
        Me.DdlDateFormat.Text = GrpProp.DateFormat.ToString                                 '日付の表示形式
        Me.TxtXAxisAngle.Text = GrpProp.XAxisTileAngle.ToString                             'X軸ラベル表示角度

        For iloop = 0 To GrpProp.GraphCount - 1

            '====== 左軸 ======
            ScaleMax(iloop).Text = GrpProp.LineInfo(iloop).LeftAxis.Max.ToString            '最大値
            ScaleMin(iloop).Text = GrpProp.LineInfo(iloop).LeftAxis.Min.ToString            '最小値

            If GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = False Then                     'スケールが既定値でない場合は
                ScaleInput(iloop).Checked = True                                            '入力ラジオボタンを選択状態にする
                ScaleDef(iloop).Checked = False                                             '既定値ラジオボタンを非選択状態にする
            Else
                ScaleInput(iloop).Checked = False                                           '入力ラジオボタンを非選択状態にする
                ScaleDef(iloop).Checked = True                                              '既定値ラジオボタンを選択状態にする
            End If
            DefVal(iloop).SelectedIndex = GrpProp.LineInfo(iloop).LeftAxis.DefaultValue

            '====== 右軸 ======
            If GrpProp.LineInfo(iloop).EnableRightAxis = True Then
                ScaleMaxR(iloop).Text = GrpProp.LineInfo(iloop).RightAxis.Max.ToString          '最大値
                ScaleMinR(iloop).Text = GrpProp.LineInfo(iloop).RightAxis.Min.ToString          '最小値

                If GrpProp.LineInfo(iloop).RightAxis.DefaultSet = False Then                    'スケールが既定値でない場合は
                    ScaleInputR(iloop).Checked = True                                           '入力ラジオボタンを選択状態にする
                    ScaleDefR(iloop).Checked = False                                            '既定値ラジオボタンを非選択状態にする
                Else
                    ScaleInputR(iloop).Checked = False                                          '入力ラジオボタンを非選択状態にする
                    ScaleDefR(iloop).Checked = True                                             '既定値ラジオボタンを選択状態にする
                End If
                DefValR(iloop).SelectedIndex = GrpProp.LineInfo(iloop).RightAxis.DefaultValue
            End If
        Next

        'オブジェクト変数のクリア                                                               ' 2015/11/25 Kino Add
        For iloop = 0 To 5
            ScaleMax(iloop).Dispose()
            ScaleMin(iloop).Dispose()
            ScaleDef(iloop).Dispose()
            ScaleInput(iloop).Dispose()
            DefVal(iloop).Dispose()
            ScaleMaxR(iloop).Dispose()
            ScaleMinR(iloop).Dispose()
            ScaleDefR(iloop).Dispose()
            ScaleInputR(iloop).Dispose()
            DefValR(iloop).Dispose()
        Next

    End Sub

    Protected Function ReadFromControl() As Integer
        ''
        '' コントロールに配置された値を読んで、変数へ格納する
        ''
        ''^(?!([02468][1235679]|[13579][01345789])000229)(([0-9]{4}(01|03|05|07|08|10|12)(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}(04|06|11)(0[1-9]|[12][0-9]|30))|([0-9]{4}02(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])0229))$
        Dim iloop As Integer
        Dim ScaleMax(5), ScaleMaxR(5) As WebControls.TextBox
        Dim ScaleMin(5), ScaleMinR(5) As WebControls.TextBox
        Dim ScaleDef(5), ScaleDefR(5) As WebControls.RadioButton
        Dim ScaleInput(5), ScaleInputR(5) As WebControls.RadioButton
        Dim DefVal(5), DefValR(5) As DropDownList
        Dim blnRet As Boolean
        Dim dteTemp As DateTime
        Dim retVal As Integer = 0
        Dim sngTemp As Single = 0                                               ' 2016/02/15 Kino Add
        Dim strTemp As String = ""                                              ' 2016/02/15 Kino Add

        '左軸
        ScaleMax(0) = Me.TxtMax1 : ScaleMin(0) = Me.TxtMin1 : ScaleDef(0) = Me.RdbNo11 : ScaleInput(0) = Me.RdbNo12 : DefVal(0) = Me.DdlScale1
        ScaleMax(1) = Me.TxtMax2 : ScaleMin(1) = Me.TxtMin2 : ScaleDef(1) = Me.RdbNo21 : ScaleInput(1) = Me.RdbNo22 : DefVal(1) = Me.DdlScale2
        ScaleMax(2) = Me.TxtMax3 : ScaleMin(2) = Me.TxtMin3 : ScaleDef(2) = Me.RdbNo31 : ScaleInput(2) = Me.RdbNo32 : DefVal(2) = Me.DdlScale3
        ScaleMax(3) = Me.TxtMax4 : ScaleMin(3) = Me.TxtMin4 : ScaleDef(3) = Me.RdbNo41 : ScaleInput(3) = Me.RdbNo42 : DefVal(3) = Me.DdlScale4
        ScaleMax(4) = Me.TxtMax5 : ScaleMin(4) = Me.TxtMin5 : ScaleDef(4) = Me.RdbNo51 : ScaleInput(4) = Me.RdbNo52 : DefVal(4) = Me.DdlScale5
        ScaleMax(5) = Me.TxtMax6 : ScaleMin(5) = Me.TxtMin6 : ScaleDef(5) = Me.RdbNo61 : ScaleInput(5) = Me.RdbNo62 : DefVal(5) = Me.DdlScale6
        '右軸
        ScaleMaxR(0) = Me.TxtMaxR1 : ScaleMinR(0) = Me.TxtMinR1 : ScaleDefR(0) = Me.RdbNoR11 : ScaleInputR(0) = Me.RdbNoR12 : DefValR(0) = Me.DdlScaleR1
        ScaleMaxR(1) = Me.TxtMaxR2 : ScaleMinR(1) = Me.TxtMinR2 : ScaleDefR(1) = Me.RdbNoR21 : ScaleInputR(1) = Me.RdbNoR22 : DefValR(1) = Me.DdlScaleR2
        ScaleMaxR(2) = Me.TxtMaxR3 : ScaleMinR(2) = Me.TxtMinR3 : ScaleDefR(2) = Me.RdbNoR31 : ScaleInputR(2) = Me.RdbNoR32 : DefValR(2) = Me.DdlScaleR3
        ScaleMaxR(3) = Me.TxtMaxR4 : ScaleMinR(3) = Me.TxtMinR4 : ScaleDefR(3) = Me.RdbNoR41 : ScaleInputR(3) = Me.RdbNoR42 : DefValR(3) = Me.DdlScaleR4
        ScaleMaxR(4) = Me.TxtMaxR5 : ScaleMinR(4) = Me.TxtMinR5 : ScaleDefR(4) = Me.RdbNoR51 : ScaleInputR(4) = Me.RdbNoR52 : DefValR(4) = Me.DdlScaleR5
        ScaleMaxR(5) = Me.TxtMaxR6 : ScaleMinR(5) = Me.TxtMinR6 : ScaleDefR(5) = Me.RdbNoR61 : ScaleInputR(5) = Me.RdbNoR62 : DefValR(5) = Me.DdlScaleR6

        GrpProp.PaperOrientaion = Me.DdlPaperOrientation.SelectedIndex                      '用紙の向き
        GrpProp.EnableLegend = Me.DdlEnableLegend.SelectedIndex                             '凡例の有無
        GrpProp.TitlePosition = Convert.ToInt16(Me.DdlTitlePosition.SelectedIndex)          'タイトルの位置　0:上　1:下

        For iloop = 0 To GrpProp.GraphCount - 1
            sngTemp = 0 : strTemp = ""
            '====== 左軸 ======
            GrpProp.LineInfo(iloop).LeftAxis.Max = Single.Parse(ScaleMax(iloop).Text)       '最大値
            GrpProp.LineInfo(iloop).LeftAxis.Min = Single.Parse(ScaleMin(iloop).Text)       '最小値

            If ScaleDef(iloop).Checked = False Then                                         'スケールが既定値でない場合は(つまり「入力」の場合)
                GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = False                         '「入力」にする
            Else
                GrpProp.LineInfo(iloop).LeftAxis.DefaultSet = True                          '「既定値」にする
            End If
            GrpProp.LineInfo(iloop).LeftAxis.DefaultValue = DefVal(iloop).SelectedIndex     '既定値の場合のスケール設定インデックス
            GrpProp.LineInfo(iloop).LeftAxis.Max = Single.Parse(ScaleMax(iloop).Text)       '「入力」の時のスケール最大値
            GrpProp.LineInfo(iloop).LeftAxis.Min = Single.Parse(ScaleMin(iloop).Text)           '「入力」の時のスケール最小値

            If GrpProp.LineInfo(iloop).LeftAxis.Max < GrpProp.LineInfo(iloop).LeftAxis.Min Then     ' 最大 ＜ 最小 の場合の入替 2015/02/15 Kino Add
                sngTemp = GrpProp.LineInfo(iloop).LeftAxis.Min
                GrpProp.LineInfo(iloop).LeftAxis.Min = GrpProp.LineInfo(iloop).LeftAxis.Max
                GrpProp.LineInfo(iloop).LeftAxis.Max = sngTemp

                strTemp = ScaleMin(iloop).Text
                ScaleMin(iloop).Text = ScaleMax(iloop).Text
                ScaleMax(iloop).Text = strTemp
            End If

            sngTemp = 0 : strTemp = ""
            '====== 右軸 ======
            If GrpProp.LineInfo(iloop).EnableRightAxis = True Then
                GrpProp.LineInfo(iloop).RightAxis.Max = Single.Parse(ScaleMaxR(iloop).Text)     '最大値
                GrpProp.LineInfo(iloop).RightAxis.Min = Single.Parse(ScaleMinR(iloop).Text)     '最小値
                If ScaleDefR(iloop).Checked = False Then                                        'スケールが既定値でない場合は(つまり「入力」の場合)
                    GrpProp.LineInfo(iloop).RightAxis.DefaultSet = False                        '「入力」にする
                Else
                    GrpProp.LineInfo(iloop).RightAxis.DefaultSet = True                         '「既定値」にする
                End If
                GrpProp.LineInfo(iloop).RightAxis.DefaultValue = DefValR(iloop).SelectedIndex   '既定値の場合のスケール設定インデックス
                GrpProp.LineInfo(iloop).RightAxis.Max = Single.Parse(ScaleMaxR(iloop).Text)     '「入力」の時のスケール最大値
                GrpProp.LineInfo(iloop).RightAxis.Min = Single.Parse(ScaleMinR(iloop).Text)     '「入力」の時のスケール最小値

                If GrpProp.LineInfo(iloop).RightAxis.Max < GrpProp.LineInfo(iloop).RightAxis.Min Then   ' 最大 ＜ 最小 の場合の入替 2015/02/15 Kino Add
                    sngTemp = GrpProp.LineInfo(iloop).RightAxis.Min
                    GrpProp.LineInfo(iloop).RightAxis.Min = GrpProp.LineInfo(iloop).RightAxis.Max
                    GrpProp.LineInfo(iloop).RightAxis.Max = sngTemp

                    strTemp = ScaleMinR(iloop).Text
                    ScaleMinR(iloop).Text = ScaleMaxR(iloop).Text
                    ScaleMaxR(iloop).Text = strTemp

                End If
            End If
        Next

        GrpProp.EnableLegend = Me.DdlEnableLegend.SelectedIndex                             '凡例表示
        GrpProp.XAxisTileAngle = Integer.Parse(Me.TxtXAxisAngle.Text)                       'X軸のラベル表示角度
        blnRet = Date.TryParse(Me.TxtStartDate.Text, dteTemp)                               '日付の検証
        If blnRet = True Then GrpProp.StartDate = Date.Parse(Me.TxtStartDate.Text) '        '描画開始日
        blnRet = Date.TryParse(Me.TxtEndDate.Text, dteTemp)                                 '日付の検証
        If blnRet = True Then GrpProp.EndDate = Date.Parse(Me.TxtEndDate.Text) '            '描画終了日
        GrpProp.PaperOrientaion = Me.DdlPaperOrientation.SelectedIndex                      '用紙の向き
        GrpProp.DateFormat = Me.DdlDateFormat.SelectedValue                                 '日付表示形式
        GrpProp.TimeScale = Me.DDLRange.SelectedValue                                       '描画期間
        GrpProp.TitlePosition = Convert.ToInt16(Me.DdlTitlePosition.SelectedIndex)          'グラフタイトルの表示位置

        'オブジェクト変数のクリア
        For iloop = 0 To 5
            ScaleMax(iloop).Dispose()
            ScaleMin(iloop).Dispose()
            ScaleDef(iloop).Dispose()
            ScaleInput(iloop).Dispose()
            DefVal(iloop).Dispose()
            ScaleMaxR(iloop).Dispose()
            ScaleMinR(iloop).Dispose()
            ScaleDefR(iloop).Dispose()
            ScaleInputR(iloop).Dispose()
            DefValR(iloop).Dispose()
        Next

        Return retVal

    End Function

    ''Protected Sub Tmr_Update_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Tmr_Update.Tick
    ''    ''
    ''    '' グラフの自動更新
    ''    ''

    ''    ''最新データが更新されていた場合には、グラフを更新する
    ''    ''If GrpProp.LastUpdate < CType(Session.Item("LastUpdate"), Date) Then
    ''    ''    GrpProp.LastUpdate = CType(Session.Item("LastUpdate"), Date)
    ''    ''End If

    ''    ''GetMostUpdate()

    ''    ''SetGraphCommonInit()

    ''End Sub

    Protected Sub GraphTimeChartFrm_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles GraphTimeChartFrm.Disposed

        Me.ViewState.Clear()

    End Sub

    Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs) Handles AJAX_TKSM.AsyncPostBackError
        '' 2008/10/22 Kino Add
        ''errorSummary.logに例外発生元(Sourceプロパティ)、例外メッセージ(Messageプロパティ)、発生時刻をタブ区切りテキストとして記録

        Dim sw As New StreamWriter(Server.MapPath("~/log/errorSummary_" & Date.Now.ToString("yyMM") & ".log"), True, Encoding.GetEncoding("Shift_JIS"))

        Dim ex As Exception = e.Exception

        If ex IsNot Nothing Then
            Dim sb As New StringBuilder
            sb.Append(ex.Source)
            sb.Append(ControlChars.Tab)
            sb.Append(ex.Message)
            sb.Append(ControlChars.Tab)
            sb.Append(DateTime.Now.ToString)
            sw.WriteLine(sb.ToString)
        End If

        sw.Dispose()

    End Sub

    '' javascriptによる制御へ移行
    ''Protected Sub RdBDsignatedDate_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    ''    ''
    ''    '' 指定日からを選択した場合のイベント
    ''    ''

    ''    Me.TxtStartDate.ReadOnly = False
    ''    Me.TxtEndDate.ReadOnly = False
    ''    Me.ImgStDate.Visible = True
    ''    Me.ImgEdDate.Visible = True
    ''    Me.DDLRange.Enabled = False

    ''End Sub

    ''Protected Sub RdBFromNewest_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    ''    ''
    ''    '' 最新からを選択した場合のイベント
    ''    ''

    ''    Me.TxtStartDate.ReadOnly = True
    ''    Me.TxtEndDate.ReadOnly = True
    ''    Me.ImgStDate.Visible = False
    ''    Me.ImgEdDate.Visible = False
    ''    Me.DDLRange.Enabled = True

    ''End Sub

    'Protected Sub ImbBtMovePre1Week_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)

    '    Dim ClickButton As String

    '    ClickButton = sender.ClientId
    '    Me.TxtStartDate.Text = Date.Parse(Me.TxtStartDate.Text).AddDays(-1)

    'End Sub

    ''' <summary>
    ''' ドロップダウンリストボックスからほかのグラフを呼び出す
    ''' </summary>
    ''' <param name="dbcon">データベースコネクション</param>
    ''' <param name="graphType">グラフの種別</param>
    Protected Sub SetOtherGraphs(ByVal dbcon As OleDb.OleDbConnection, ByVal graphType As String)   ', ByVal wid As Integer, ByVal Hei As Integer)
        '' <param name="wid">ブラウザの幅</param>
        '' <param name="Hei">ブラウザの高さ</param>

        Dim strSQL As String
        '' Dim DbDr As OleDb.OleDbDataReader
        '' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim itemCount As Integer = 0
        Dim grpTitle As String = ""

        ''strSQL = "Select * From メニュー基本情報 Where (項目名 <> '" & GrpProp.WindowTitle & "'" & " And 種別 = '" & graphType & "')"
        strSQL = ("Select * From メニュー基本情報 Where (種別 = '" + graphType + "') ORDER BY ID")                                          ' 2012/05/07 Kino Add [ ORDER BY ID]
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbcon)
        DbDa.Fill(DtSet, "DData")
        ''DbCom = New OleDb.OleDbCommand(strSQL, dbcon)
        ''DbDr = DbCom.ExecuteReader

        Me.DDLSelectGraph.Items.Clear()
        Try
            For Each DTR As DataRow In DtSet.Tables("DData").Rows
                With DTR.ItemArray
                    grpTitle = .GetValue(6).ToString
                    Me.DDLSelectGraph.Items.Add(grpTitle)      'グラフタイトルを追加
                    'Me.DDLSelectGraph.Items(itemCount).Value = .GetValue(1).ToString + "@," + wid.ToString + "," + Hei.ToString          '2009/06/30 kino Add "@"
                    Me.DDLSelectGraph.Items(itemCount).Value = .GetValue(1).ToString                                                        ' 2018/06/05 Kino Changed サイズ指定なし
                    If grpTitle = GrpProp.GraphTitle Then
                        Me.DDLSelectGraph.Items(itemCount).Selected = True
                    End If
                    itemCount += 1
                End With
            Next
        Catch ex As Exception

        Finally
            DbDa.Dispose()
            DtSet.Dispose()
        End Try
        ''With DbDr
        ''    If .HasRows = True Then
        ''        Do While .Read()                                         '１レコード分読み込み
        ''            grpTitle = .GetString(6)
        ''            Me.DDLSelectGraph.Items.Add(grpTitle)      'グラフタイトルを追加
        ''            Me.DDLSelectGraph.Items(itemCount).Value = .GetString(1).ToString + "@," + wid.ToString + "," + Hei.ToString          '2009/06/30 kino Add "@"
        ''            If grpTitle = GrpProp.GraphTitle Then
        ''                Me.DDLSelectGraph.Items(itemCount).Selected = True
        ''            End If
        ''            itemCount += 1
        ''        Loop
        ''    End If
        ''End With
        ''DbDr.Close()
        ''DbCom.Dispose()

    End Sub

    ''Protected Sub ImbtnRedrawGraph_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImbtnRedrawGraph.Click
    ''    ''    ''
    ''    ''    ''イメージボタンクリックイベント
    ''    ''    ''　作図処理のため

    ''    ''    ''Call SetGraphCommonInit()

    ''End Sub


    ''' <summary>
    ''' ドロップダウンリストボックスからほかのグラフを呼び出す
    ''' </summary>
    ''' <param name="sender">sender</param>
    ''' <param name="e">system.EventArgs</param>
    Protected Sub DDLSelectGraph_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DDLSelectGraph.SelectedIndexChanged

        Dim strURL As String = ""
        ''Dim strTemp() As String
        ''Dim firstSplit() As String
        Dim selectedValue As String = ""
        Dim allURL As String = Page.Request.UrlReferrer.AbsolutePath
        Dim thinTime As String = ""
        Dim SelItem As ListItem
        Dim idxCount As Integer
        'Dim dataSpan As String = ""

        allURL = allURL.Substring(allURL.IndexOf("/", 2) + 1)

        selectedValue = Me.DDLSelectGraph.SelectedValue.ToString
        ''firstSplit = selectedValue.Split("@"c)                                                       '2009/06/30 Kino Add
        ''strTemp = firstSplit(1).Split(","c)

        strURL = allURL + ("?GN=" + Me.DDLSelectGraph.SelectedIndex.ToString("000") & "00" & "&GNa=" & _
                 HttpUtility.UrlEncode(selectedValue))
        ''HttpUtility.UrlEncode(firstSplit(0)) + "&W=" + strTemp(1) + "&H=" + strTemp(2))


        If Me.RdBFromNewest.Checked = True Then         '最新から
            Session.Item("DSP") = "N"
            Session.Item("DS") = Me.DDLRange.SelectedIndex
            'dataSpan = "&DSp=N&DS=" & Me.DDLRange.SelectedIndex                            ' クエリ文字列からセッションに変更
            ''Page.Request.QueryString.Add("DSp", "N")                                      ' 読み取り専用だからセットやアドできないらしい・・
            ''Page.Request.QueryString.Add("DS", Me.DDLRange.SelectedIndex.ToString)
        Else                                            '指定期間
            Session.Item("DSP") = "D"
            Session.Item("DS") = Replace(Me.TxtStartDate.Text, "/", "")
            Session.Item("DE") = Replace(Me.TxtEndDate.Text, "/", "")
            'dataSpan = "&DSp=D&DS=" & st & "&DE=" & ed
        End If

        If Me.ChbPartial.Checked = True Then                                                ' 間引きするかしないか
            Session.Item("PEn") = "1"                                                       ' Partial Enable
        Else
            Session.Item("PEn") = "0"
        End If
        For Each SelItem In Me.CBLPartial.Items                                             ' 間引き設定のインデックス
            If SelItem.Selected = True Then
                thinTime += (idxCount.ToString + ",")       'idxCount.ToString  SelItem.Value
            End If
            idxCount += 1
        Next
        Session.Item("Thn") = thinTime

        Session.Item("GrpChg") = 1                                                          ' 2011/02/01 グラフ切り替えフラグ立て

        strURL = strURL
        Response.Redirect(strURL, False)
        'Server.Transfer(strURL)

    End Sub



    Protected Sub WC1_DrawDataSeries(ByVal sender As Object, ByVal e As C1.Win.C1Chart.DrawDataSeriesEventArgs) Handles WC2.DrawDataSeries, _
                                                            WC1.DrawDataSeries, WC3.DrawDataSeries, WC4.DrawDataSeries, WC5.DrawDataSeries

        '' 折れ線のグラフが鋭角で折り返す場合に、線の幅が太くなるにつれ本来の座標よりも外側の部分にはみ出して描画される問題の回避策
        '' GrapeCity KB25039

        '接合方法を変更するデータセットを判断します
        ' System.Diagnostics.Debug.WriteLine(e.SeriesIndex.ToString)
        If sender.ToString.Length <> 0 Then
            ' System.Diagnostics.Debug.WriteLine(sender.ToString + " " + e.SeriesIndex.ToString + " " + e.GroupIndex.ToString + " を処理")
            ''If e.SeriesIndex <= GrpProp.LineInfo(0).LeftAxis.DataCount - 1 Then  '= 0 Then n

            Dim setColor As New System.Drawing.Color

            setColor = CType(sender, C1.Win.C1Chart.ChartDataSeries).LineStyle.Color

            Dim pen As New Pen(setColor, CType(sender, C1.Win.C1Chart.ChartDataSeries).LineStyle.Thickness)      '(Color.Red, 3)

            ''pen.DashPattern = New Single() {2, 2}  破線パターンを設定する場合はこれを使う 2012/07/19

            '鋭角接合（デフォルト：はみ出しが発生します）
            'pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Miter

            '面取り接合（現象を回避できます）
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Bevel

            '角丸接合（現象を回避できます）
            'pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round

            e.Pen = pen

            setColor = Nothing
        End If

    End Sub

    Private Function TableDataScale() As Object
        Throw New NotImplementedException
    End Function

    'Protected Sub WC1_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles WC1.Paint

    '    System.Diagnostics.Debug.Print("{0}", WC1.ChartArea.PlotArea.Location)
    '    System.Diagnostics.Debug.Print("{0}", WC1.ChartArea.PlotArea.Size)

    'End Sub

    ''' <summary>
    ''' [イベント]保存ボタン　クリック　　　　グラフNoごとのスケール設定保存
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub lkBtn_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lkBtn1.Click, lkBtn2.Click, lkBtn3.Click, lkBtn4.Click, lkBtn5.Click, lkBtn6.Click

        If AJAX_TKSM.AsyncPostBackSourceElementID.IndexOf("lkBtn") >= 0 Or AJAX_TKSM.AsyncPostBackSourceElementID.Length = 0 Then

        End If


        Dim btnSet As String = Page.Request.Params.Get("__EVENTTARGET")
        Dim btnNo As Integer = Convert.ToInt32(btnSet.Substring(btnSet.Length - 1, 1))
        Dim siteDirectory As String = CType(Session.Item("SD"), String)
        Dim graphName As String = CType(Request.Item("GNa"), String)
        Dim grpDataFile As String = Server.MapPath(siteDirectory + "\App_Data\TimeChartGraphInfo.mdb")

        Call saveScaleSet(graphName, btnNo, grpDataFile)

    End Sub

    ''' <summary>
    ''' [イベント]保存ボタン　クリック　　　　軸ごとのスケール設定保存
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub lkBtnAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lkBtnLeftAll.Click, lkBtnRightAll.Click

        Dim btnSet As String = Page.Request.Params.Get("__EVENTTARGET")
        Dim siteDirectory As String = CType(Session.Item("SD"), String)
        Dim graphName As String = CType(Request.Item("GNa"), String)
        Dim grpDataFile As String = Server.MapPath(siteDirectory + "\App_Data\TimeChartGraphInfo.mdb")
        Dim iloop As Integer
        Dim leftRightFlg As Integer = 0

        If btnSet.ToString.Contains("Left") = True Then
            leftRightFlg = 1
        Else
            leftRightFlg = 2
        End If

        For iloop = 0 To GrpProp.GraphCount - 1
            Call saveScaleSet(graphName, (iloop + 1), grpDataFile, leftRightFlg)
        Next iloop

    End Sub


    ''' <summary>
    ''' スケール設定値をDBへ保存
    ''' </summary>
    ''' <param name="graphName"></param>
    ''' <param name="btnNo"></param>
    ''' <param name="grpDataFile"></param>
    ''' <param name="leftRightFlg"></param>
    ''' <remarks></remarks>
    Private Sub saveScaleSet(ByVal graphName As String, ByVal btnNo As Integer, ByVal grpDataFile As String, Optional ByVal leftRightFlg As Integer = 0)

        Dim cn As New OleDb.OleDbConnection
        Dim com As New OleDb.OleDbCommand
        Dim cmdbuilder As New OleDbCommandBuilder
        Dim dataAdp As OleDbDataAdapter = Nothing
        Dim dSet As New DataSet("GraphName")
        Dim DRow(0) As DataRow
        Dim DTable As DataTable = Nothing
        Dim schemaTable As DataTable
        Dim selRadioBtn As RadioButton
        Dim txtBox As TextBox
        Dim scaleIndex As DropDownList

        cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & grpDataFile & ";" & "Jet OLEDB:Engine Type= 5")
        dataAdp = New OleDbDataAdapter("SELECT * FROM [" & graphName & "] WHERE グラフNo=" & btnNo.ToString, cn)
        dataAdp.Fill(dSet, "GraphName")
        DTable = dSet.Tables("GraphName")

        ' 主キーの列数を取得
        cn.Open()
        schemaTable = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, New Object() {Nothing, Nothing, graphName})
        cn.Close()

        If schemaTable.Rows.Count = 0 Then
            cn.Open()
            Dim strSQL As String = "ALTER TABLE [" & graphName & "] ALTER COLUMN ID LONG NOT NULL"
            com.Connection = cn
            com.CommandText = strSQL
            com.ExecuteNonQuery()
            strSQL = "ALTER TABLE [" & graphName & "] ADD CONSTRAINT IDNum PRIMARY KEY (ID)"
            com.CommandText = strSQL
            com.ExecuteNonQuery()
            cn.Close()
        End If
        schemaTable.Dispose()

        Try

            If DTable.Rows.Count > 0 Then
                If leftRightFlg = 0 Or leftRightFlg = 1 Then
                    DRow(0) = dSet.Tables("GraphName").Rows(0)
                    ' 左軸==============================================================
                    ' 既定
                    selRadioBtn = CType(Form.FindControl("RdbNo" & btnNo.ToString & "1"), RadioButton)
                    'If selRadioBtn.Checked = True Then                                 ' 2017/02/14 Kino Changed  チェックがあろうがなかろうが設定しないと駄目だった
                    'DRow(0).Item("左軸スケール既定") = True
                    DRow(0).Item("左軸スケール既定") = selRadioBtn.Checked
                    ' ''既定時のドロップダウンリストボックスのインデックス
                    ''scaleIndex = CType(Form.FindControl("DdlScale" & btnNo.ToString), DropDownList)
                    ''DRow(0).Item("左軸既定値") = scaleIndex.SelectedIndex
                    'End If

                    '既定時のドロップダウンリストボックスのインデックス　－－　規定値を選択していなくても書き換える
                    scaleIndex = CType(Form.FindControl("DdlScale" & btnNo.ToString), DropDownList)
                    DRow(0).Item("左軸既定値") = scaleIndex.SelectedIndex

                    txtBox = CType(Form.FindControl("TxtMax" & btnNo.ToString), TextBox)
                    DRow(0).Item("左Max") = Convert.ToSingle(txtBox.Text)

                    txtBox = CType(Form.FindControl("TxtMin" & btnNo.ToString), TextBox)
                    DRow(0).Item("左Min") = Convert.ToSingle(txtBox.Text)
                End If

                ' 右軸==============================================================有効なら処理をする
                ' 既定
                If GrpProp.LineInfo(btnNo - 1).EnableRightAxis = True Then
                    If leftRightFlg = 0 Or leftRightFlg = 2 Then
                        selRadioBtn = CType(Form.FindControl("RdbNoR" & btnNo.ToString & "1"), RadioButton)
                        'If selRadioBtn.Checked = True Then                             ' 2017/02/14 Kino Changed  チェックがあろうがなかろうが設定しないと駄目だった
                        DRow(0).Item("右軸スケール既定") = selRadioBtn.Checked
                        ' ''既定時のドロップダウンリストボックスのインデックス　－－　規定値を選択していなくても書き換える
                        ''scaleIndex = CType(Form.FindControl("DblScaleR" & btnNo.ToString), DropDownList)
                        ''DRow(0).Item("右軸既定値") = scaleIndex.SelectedIndex
                        'End If

                        '既定時のドロップダウンリストボックスのインデックス
                        scaleIndex = CType(Form.FindControl("DdlScaleR" & btnNo.ToString), DropDownList)
                        DRow(0).Item("右軸既定値") = scaleIndex.SelectedIndex

                        txtBox = CType(Form.FindControl("TxtMaxR" & btnNo.ToString), TextBox)
                        DRow(0).Item("右Max") = Convert.ToSingle(txtBox.Text)

                        txtBox = CType(Form.FindControl("TxtMinR" & btnNo.ToString), TextBox)
                        DRow(0).Item("右Min") = Convert.ToSingle(txtBox.Text)
                    End If
                End If

                cmdbuilder = New OleDbCommandBuilder(dataAdp)
                cmdbuilder.QuotePrefix = "["
                cmdbuilder.QuoteSuffix = "]"
                dataAdp.Update(DRow)
            End If

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine(ex.StackTrace.ToString)

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

        Finally
            dataAdp.Dispose()
            dSet.Dispose()
            cn.Close()
            cn.Dispose()
        End Try

    End Sub
End Class

