Option Strict On

Imports System.Data
Imports System.Drawing
Imports C1.Win.C1Chart
Imports C1.Web.C1WebChart
Imports AjaxControlToolkit

Partial Class DisplacementDistributionChart
    Inherits System.Web.UI.Page

#Region "グラフ描画における個々のグラフ情報構造体"
    Private Structure GrpPersonalizedInfo
        ''' <summary>変位データ       データ系列,深度数</summary>
        Dim DrawDispData(,) As Single
        ''' <summary>チャンネル、位置、[変位データ]　設置台数分</summary>
        Dim DrawData() As ClsReadDataFile.DataInfo
        ''' <summary>スケール既定値か入力か</summary>
        Dim SpecifiedValueEnable As Boolean
        ''' <summary>既定値の場合のリストインデックス</summary>
        Dim SpecifiedValueIndex As Integer
        ''' <summary>Y軸タイトル</summary>
        Dim DataTitle As String
        ''' <summary>Y軸タイトルのサイズ</summary>
        Dim DataTitleSize As Integer
        ''' <summary>Y軸タイトルの色</summary>
        Dim DataTitleColor As Color
        ''' <summary>データの単位</summary>
        Dim DataUnit As String
        ''' <summary>Y軸の最大値</summary>
        Dim Max As Single
        ''' <summary>Y軸の最小値</summary>
        Dim Min As Single
        ''' <summary>Y主メモリ間隔</summary>
        Dim UnitMajor As Single
        ''' <summary>Y副メモリ間隔</summary>
        Dim UnitMinor As Single
        ''' <summary>データCh数</summary>
        Dim DataCount As Short
        ''' <summary>描画データチャンネルの文字列(後で分解するため)</summary>
        Dim DispDatas As String
        ''' <summary>データファイルNo</summary>
        Dim DataFileNo As Integer
        ''' <summary>サブタイトルの表示有無（グラフ追加説明）</summary>
        Dim SubTitleShow As Boolean
        ''' <summary>サブタイトル文字列</summary>
        Dim SubTitle As String
        ''' <summary>サブタイトル文字サイズ</summary>
        Dim SubTitleSize As Integer
        ''' <summary>棒グラフ表示</summary>
        Dim BarGraphEnable As Boolean
        ''' <summary>散布図表示</summary>
        Dim ScatterGraphEanble As Boolean
        ''' <summary>散布図の場合の線の太さ</summary>
        Dim ScatterDataLineWidth As Integer
        ''' <summary>散布図の場合の線の種類</summary>
        Dim ScatterDataLineType As Integer
        ''' <summary>軸反転</summary>
        Dim Revesed As Boolean
        ''' <summary>対数軸</summary>
        Dim Logarithm As Boolean
        ''' <summary>Y軸スケールフォーマット</summary>
        Dim DataScaleFormat As String
        ''' <summary>X軸における設置距離表示</summary>
        Dim ShowDistance As Boolean
        ''' <summary>X軸におけるラベル設定</summary>
        Dim X_Label As String
        ''' <summary>X軸ラベル回転角</summary>
        Dim X_LabelAngle As Integer
        ''' <summary>個別管理値</summary>
        Dim IndivisualAlertData As String
        ''' <summary>X軸最大値</summary>
        Dim X_Max As Single
        ''' <summary>X軸最小値</summary>
        Dim X_Min As Single
        ''' <summary>X軸主メモリ間隔</summary>
        Dim X_UnitMajor As Single
        ''' <summary>X軸副メモリ間隔</summary>
        Dim X_UnitMinor As Single
        ''' <summary>凡例幅</summary>
        Dim LegendWidth As Integer
        ''' <summary>凡例左位置割合</summary>
        Dim LegendLeft As Single
        ''' <summary>管理値表示種別（エリア／線）</summary>
        Dim AlertType As Integer
        ''' <summary>管理値表示を線にした場合の線の割合</summary>
        Dim AlertLineWidth As Single
        ''' <summary>警報値表示</summary>
        Dim ShowAlert As Boolean
        ''' <summary>グラフのプロットエリアの左位置</summary>
        Dim GraphLocation_X As Integer
        ''' <summary>マーカーのサイズ</summary>
        Dim MarkerSize As Integer
        ''' <summary>右軸のスケール表示フラグ</summary>
        Dim ShowY2Scale As Boolean
        ''' <summary>近似曲線の表示設定　0:直線　1:スプライン　2:ベジェ曲線</summary>
        Dim FitCurve As Integer
    End Structure
#End Region

    'グラフ描画全般の情報
    Private Structure GrpInfo
        ''' <summary>グラフ描画における個々のグラフ情報</summary>
        Dim DrawInfo() As GrpPersonalizedInfo
        '''' <summary>各文字サイズや色の設定</summary>
        'Dim DrawSizeColorInf() As Integer
        ''' <summary>データの測定時刻</summary>
        Dim DataTime() As DateTime
        '--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--
        Dim WindowTitle As String
        Dim PointName As String
        ''' <summary>描画グラフ数</summary>
        Dim GraphCount As Integer
        ''' <summary>グラフの枠数</summary>
        Dim GraphBoxCount As Integer
        ''' <summary>１つのグラフに描画するデータの数(最新：１個　指定日時：指定日付分)</summary>
        Dim DataCount As Short
        ''' <summary>グラフタイトル</summary>
        Dim GraphTitle As String
        ''' <summary>ページタイトルの位置(上or下)</summary>
        Dim TitlePosition As Short
        ''' <summary>ページタイトルのサイズ</summary>
        Dim TitleSize As Integer
        ''' <summary>ページタイトルの　色</summary>
        Dim TitleColor As Color
        ''' <summary>日付フォーマット</summary>
        Dim DateFormat As String
        ''' <summary>用紙の向き</summary>
        Dim PaperOrientaion As Integer
        ''' <summary>X軸スケールの文字サイズ</summary>
        Dim X_ScaleSize As Integer
        ''' <summary>X軸スケールの文字色</summary>
        Dim X_ScaleColor As Color
        ''' <summary>Y軸スケールの文字サイズ</summary>
        Dim Y_ScaleSize As Integer
        ''' <summary>Y軸スケールの文字色</summary>
        Dim Y_ScaleColor As Color
        ''' <summary>主盛り線の幅</summary>
        Dim GridLineWidth As Integer
        ''' <summary>主盛り線の色</summary>
        Dim GridLineColor As Color
        ''' <summary>副盛りの幅</summary>
        Dim MinorGridLineWidth As Integer
        ''' <summary>副目盛りの幅</summary>
        Dim MinorGridLineColor As Color
        ''' <summary>グラフ表示ページのaspxファイル名</summary>
        Dim URL As String
        ''' <summary>凡例表示</summary>
        Dim ShowLegend As Integer
        ''' <summary>凡例の枠幅</summary>
        Dim LegendOutlineWidth As Integer
        ''' <summary>凡例の枠線の色</summary>
        Dim LegendOutlineColor As Color
        ''' <summary>データの最終更新日</summary>
        Dim LastUpdate As DateTime
        ''' <summary>欠測データの連結</summary>
        Dim MissingDataContinuous As Integer
        ''' <summary>警報値表示</summary>
        Dim ShowAlert As Integer
        Dim StartDate As DateTime                   'グラフ描画開始日
        Dim EndDate As DateTime                     'グラフ描画終了日
    End Structure

    Private DataFileNames() As ClsReadDataFile.DataFileInf
    Private GrpProp As GrpInfo                  'グラフ描画に関する情報
    Private TmrState As Boolean                             'タイマーの起動状態
    Private TxtDates(6) As TextBox
    Private imgDates(6) As System.Web.UI.WebControls.Image
    Private GrpBox(5) As WebControls.Panel                  ''グラフ枠
    Private Grps(5) As C1.Web.C1WebChart.C1WebChart         ''グラフコントロール
    Private SetPanel(5) As UI.HtmlControls.HtmlContainerControl       ' WebControls.Panel                ''横軸スケールの設定パネル
    Private Defval(5) As DropDownList                       ''デフォルトスケール値
    Private scaleDef(5) As RadioButtonList                  ''デフォルト設定
    Private scaleMin(5) As TextBox                          ''縦軸スケール最小値
    Private scaleMax(5) As TextBox                          ''縦軸スケール最大値

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

        clsLogWrite.writeErrorLog(ex, Server.MapPath("~/errorSummary.log"), userName, siteDirectory, MyBase.GetType.BaseType.FullName)

        Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)

        If LoginStatus = 0 Then     ''ログインステータスが０ならログイン画面
            Response.Redirect("sessionerror.aspx")
        Else                        ''そうでなければ、データ表示画面を再構築
            Dim strScript As String = "<html><head><title>タイムアウト</title></head><body>接続タイムアウトになりました。</body></html>" + "<script language=javascript>alert('画面を閉じて再表示してください。');window.close();</script>"
            Response.Write(strScript)
        End If

        clsLogWrite = Nothing

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = ("<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>")

        Dim LoginStatus As Integer = DirectCast(Session.Item("LgSt"), Integer)                                      'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            Response.Redirect("sessionerror.aspx")
            Exit Sub
        End If

        If User.Identity.IsAuthenticated = False Then
            Response.Write(strScript)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)                ''キャッシュなしとする
        'ClientScript.RegisterClientScriptBlock(GetType(String), "/js/Simple2D.js", "<script language=""javascript"" type=""text/javascript"" src=""" + Me.TemplateSourceDirectory + "/js/Simple2D.js" + """></script>")

        Dim siteName As String
        Dim siteDirectory As String
        Dim grpDataFile As String
        Dim intRet As Integer
        Dim OldestDate As DateTime
        Dim clsNewDate As New ClsReadDataFile
        Dim clsSetScript As New ClsGraphCommon
        Dim OldTerm As String

        OldTerm = CType(Session.Item("OldTerm"), String)                        '過去データ表示制限
        siteName = CType(Session.Item("SN"), String)                            '現場名
        siteDirectory = CType(Session.Item("SD"), String)                       '現場ディレクトリ

        grpDataFile = Server.MapPath(siteDirectory + "\App_Data\DispDistChartGraphInfo.mdb")
        ''grpDataFile = "I:\WebDataSystem2\totsuka\App_Data\DispDistChartInfo.mdb"               '◆◆◆◆◆ デバッグ用
        OldestDate = CType(Session.Item("OldestDate"), Date)
        GrpProp.WindowTitle = CType(Request.Item("GNa"), String)                 'グラフタイトル（ウィンドウタイトル）
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)
        Dim RemoteInf As String
        Dim OpenString As String = ""

        'セッション変数に保存しないようにする 2018/03/30 Kino
        ''Dim RemoteInf As String = CType(Session("RemoteInfo"), String)
        Dim clsBr As New ClsHttpCapData
        RemoteInf = clsBr.GetRemoteInfo(Request, Server.MapPath(""))
        clsBr = Nothing
        Dim broinf() As String
        If RemoteInf IsNot Nothing Then                                                             ' 2012/08/24 Kino Add
            broinf = RemoteInf.Split(","c)
            If broinf(2) = "IE 8.0 ie" Then
                Me.C1WebCalendar.Height = 217
                'Me.LstBTime.Height = 217
            Else
                Me.C1WebCalendar.Height = 246
                'Me.LstBTime.Height = 246
            End If

        Else
            Me.C1WebCalendar.Height = 217       '246      '217
            'Me.LstBTime.Height = 246            '217
        End If

        intRet = clsNewDate.GetDataFileNames(Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb"), DataFileNames)   'データファイル名、共通情報ファイル名、識別名を取得  データの最古、最新日付を取得も一緒に

        If IsPostBack = False Then

            If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
                '==== フォームのサイズを調整する ====
                If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
                    OpenString = "<SCRIPT LANGUAGE='javascript'>"                                                          ' 2015/11/24 Kino Changed 下に集約のため、変更
                    OpenString += ("window.resizeTo(" + wid + "," + Hei + ");")
                    OpenString += "<" + "/SCRIPT>"
                    Page.ClientScript.RegisterStartupScript(Me.GetType(), "変位分布図", OpenString)
                End If
            End If

            ''設定の初回読み込みはデータベースから行なう
            Dim DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & grpDataFile & ";" & "Jet OLEDB:Engine Type= 5")

            intRet = ReadGraphInfoFromDB(DbCon, "変位分布図")                   'グラフの全体情報を取得(「メニュー基本情報」テーブル)
            intRet = GraphLineInfoFromDB(DbCon)                                 '個々のグラフ描画の情報(タイトル、最大・最小スケールなど)を取得(グラフ情報テーブル)　

            Call SetObj2Arg()                                                   '出力日付設定に関するコントロールの、オブジェクトを配列変数へ格納

            Call SetBIND2Controls(DbCon)                                        '各コントロールのリスト内容をDBにバインドする
            Call SetInfo2FormControl()

            DbCon.Dispose()

            Call GetMostUpdate()                                                'データファイルの最新日時を取得する

            ''GrpProp.LastUpdate = New DateTime(2009, 5, 11, 18, 0, 0)                       '◆◆◆◆◆ デバッグ用
            Me.RdBFromNewest.Checked = True

            Call clsSetScript.CheckAutoUpdate(Me.Form, Server.MapPath(siteDirectory + "\App_Data\MenuInfo.mdb"), "変位分布図")      '自動更新設定の読み込み
            TmrState = Me.Tmr_Update.Enabled

            Call SetViewState()                                                 'コントロールに配置しないDBから読み込んだデータをヴューステートに保存する

            Call clsSetScript.SetSelectDateScrpt_Depth(Me.Form)                 'コントロールにJavaScriptを割り当て(最新／過去切り替え時のDOM制御）

            ''ポップアップカレンダー関連の設定
            Dim iloop As Integer
            ''Dim intYear As Integer

            '' '' ''GrpProp.LastUpdate = Now()
            '' ''Me.C1WebCalendar1.MaxDate = GrpProp.LastUpdate      'New DateTime(2009, 5, 5)
            '' '' ''Me.C1WebCalendar1.MaxDate = New DateTime(Year(GrpProp.LastUpdate), Month(GrpProp.LastUpdate), Day(GrpProp.LastUpdate))

            ''For iloop = 0 To 9
            ''    ''「年」ドロップダウンリスト作成
            ''    intYear = Integer.Parse(Now().AddYears(-5).Year) + iloop
            ''    Me.DropDownList1.Items.Add(StrConv(intYear, VbStrConv.Wide) + "年")
            ''    Me.DropDownList1.Items(iloop).Value = intYear
            ''Next

            Me.C1WebCalendar.DisplayDate = GrpProp.LastUpdate.Date 'Now().Date
            Me.C1WebCalendar.SelectedDate = GrpProp.LastUpdate.Date 'Now().Date
            ''Me.DropDownList1.SelectedValue = GrpProp.LastUpdate.Year 'Now().Year
            ''Me.DropDownList2.SelectedValue = GrpProp.LastUpdate.Month 'Now().Month
            ''Me.LblStatus.Text = "選択されている日付：" + GrpProp.LastUpdate.ToShortDateString   'Now().ToShortDateString
            ''Me.PnlCalendar.Height = 367
            Me.PnlCalendar.Height = 350                         ' 2016/02/17 Kino Add　コードビハインドで記述しないとデザイナに問題がある

            Dim OldDate As DateTime
            If OldTerm <> "None" Then
                Dim clsCalc As New ClsReadDataFile
                OldDate = clsCalc.CalcOldDateLimit(OldTerm)
                clsCalc = Nothing
            Else
                OldDate = DataFileNames(0).MostOldestDate
            End If
            Dim NewDate As DateTime = Date.Parse(GrpProp.LastUpdate.AddDays(1).ToString("yyyy/MM/dd"))
            For iloop = 0 To 6
                Dim rangeValid As New RangeValidator
                rangeValid = CType(FindControl("RngValid" & (iloop).ToString), RangeValidator)
                rangeValid.MinimumValue = OldDate.ToString
                rangeValid.MaximumValue = NewDate.ToString
                'rangeValid.ErrorMessage = ( "日付は" + OldDate + " ～ " + NewDate + " までです")
                rangeValid.ErrorMessage = String.Format("日付は{0} ～ {1} までです", OldDate, NewDate)
            Next iloop
            Me.C1WebCalendar.MinDate = OldDate
            Me.C1WebCalendar.MaxDate = NewDate

            Dim fno As String = getFileNo()
            Dim fdt As String = getFileDate(fno)

            Me.nwdt.Value = fdt                                                                 ' 2018/03/16 Kino Add
            Me.nwdtno.Value = fno                                                               ' 2018/03/16 Kino Add
            If siteName.Contains("【完了現場】") = True Then
                Me.nwdt.Value = "NC"                                                            ' 完了現場は、更新チェックしない
            End If

        Else

            Call ReadViewState()                                                'ヴューステートから変数へ格納
            Call SetObj2Arg()                                                   '出力日付設定に関するコントロールの、オブジェクト配列変数への格納
            Call ReadFromControl()
            Call GetMostUpdate()                                                'データファイルの最新日時を取得する

            If TmrState = True Then                                             '過去データ表示の場合は自動更新しない
                If Me.RdBFromNewest.Checked = True Then
                    Me.Tmr_Update.Enabled = True
                Else
                    Me.Tmr_Update.Enabled = False
                End If
            End If

        End If

        If ToolkitScriptManager1.AsyncPostBackSourceElementID = "ImbtnRedrawGraph" Or ToolkitScriptManager1.AsyncPostBackSourceElementID.Length = 0 Then    'ポストバックがどのコントロールから発生したのかで判断(カレンダーのイベントではここに入らない!!)
            ''チャンネル設定からDBのデータを読み込む
            intRet = GraphDataInfoFromDB(siteDirectory)                                             '個々のグラフに表示するデータと警報情報などを取得(最新データ)

            If intRet = 0 Then
                Throw New Exception("出力日付が指定されていません。")
            End If
            Me.MyTitle.Text = String.Format("{0} [変位分布図] - {1}", GrpProp.GraphTitle, siteName)                    'ブラウザのタイトルバーに表示するタイトル

            If Me.RdBFromNewest.Checked = True Then
                If GrpProp.DataTime(0) < GrpProp.LastUpdate Then
                    Me.LblLastUpdate.Text = "最新データ日時：" + GrpProp.DataTime(0).ToString("yyyy/MM/dd HH:mm")
                Else
                    Me.LblLastUpdate.Text = "最新データ日時：" + GrpProp.LastUpdate.ToString("yyyy/MM/dd HH:mm")
                End If
            Else
                Me.LblLastUpdate.Text = "最新データ日時：" + GrpProp.LastUpdate.ToString("yyyy/MM/dd HH:mm")
            End If

            Call SetControlVisible()                                                'グラフのサイズ調整
            ''Call SetCalenderFunction()                                            'カレンダーコントロールの起動設定       使用しない
            ''グラフ描画
            Call SetGraphCommonInit()
        End If

        clsNewDate = Nothing
        clsSetScript = Nothing

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
    ''' JSのアラートを表示
    ''' </summary>
    ''' <param name="Message">表示するメッセージ</param>
    ''' <remarks></remarks>
    Public Sub ASPNET_MsgBox(ByVal Message As String)

        Dim strScript As String = "alert('" & Message & "');"
        ScriptManager.RegisterClientScriptBlock(UpdPanel2, Me.GetType(), "msgbox", strScript, True)

    End Sub

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

        For infLoop = 0 To GrpProp.DrawInfo.Length - 1
            intTemp = GrpProp.DrawInfo(infLoop).DataFileNo
            If ht.ContainsKey(intTemp) = False Then
                ht.Add(intTemp, infLoop)
                strRet &= String.Format("{0},", intTemp.ToString)
            End If
        Next

        strRet = strRet.TrimEnd(Convert.ToChar(","))

        Return strRet

    End Function

    ''' <summary>
    ''' グラフの描画に関する情報をDBから読み込む_1
    ''' </summary>
    ''' <param name="dbCon">OLEDBConnection</param>
    ''' <param name="graphType">グラフ種別</param>
    ''' <param name="PostBackFlg">ポストバックであるかどうか</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function ReadGraphInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, ByVal graphType As String, Optional ByVal PostBackFlg As Boolean = False) As Integer

        Dim strSQL As String
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim intRet As Integer = 0

        strSQL = ("Select * From メニュー基本情報 Where (項目名 = '" & GrpProp.WindowTitle & "' And 種別 = '" & graphType & "')")
        Using DbDa As New OleDb.OleDbDataAdapter(strSQL, dbCon)
            Using DtSet As New DataSet("DData")
                DbDa.Fill(DtSet, "DData")

                With DtSet.Tables("DData")
                    If .Rows.Count > 0 Then
                        GrpProp.PointName = .Rows(0).Item(3).ToString                                   'ポイント名称
                        GrpProp.GraphCount = DirectCast(.Rows(0).Item(4), Int32)                        'グラフの数
                        GrpProp.GraphBoxCount = DirectCast(.Rows(0).Item(5), Int32)                     'グラフ枠の数(パネル)
                        GrpProp.GraphTitle = .Rows(0).Item(6).ToString                                  'グラフのタイトル
                        GrpProp.TitlePosition = Convert.ToInt16(DirectCast(.Rows(0).Item(7), Int32))    'タイトルの位置(上or下)        
                        GrpProp.TitleSize = DirectCast(.Rows(0).Item(8), Int32)                         ' タイトルサイズ
                        'GrpProp.TitleColor = Color.FromArgb(.Rows(0).Item(9))                          ' タイトル色        System.Drawing.ColorTranslator.FromHtml("FFFFFF") こっちにするかどうか・・・
                        GrpProp.TitleColor = Color.FromArgb(DirectCast(DtSet.Tables("DData").Rows(0).Item(9), Int32))            ' タイトル色        System.Drawing.ColorTranslator.FromHtml("FFFFFF")
                        GrpProp.DateFormat = .Rows(0).Item(10).ToString                                 '日付の表示フォーマット        
                        GrpProp.PaperOrientaion = DirectCast(.Rows(0).Item(11), Int32)                  '紙の向き                      
                        GrpProp.X_ScaleSize = DirectCast(.Rows(0).Item(12), Int32)                      ' X軸スケールのサイズ
                        GrpProp.X_ScaleColor = Color.FromArgb(DirectCast(.Rows(0).Item(13), Int32))     ' X軸スケールの色
                        GrpProp.Y_ScaleSize = DirectCast(.Rows(0).Item(14), Int32)                      ' Y軸スケールのサイズ
                        GrpProp.Y_ScaleColor = Color.FromArgb(DirectCast(.Rows(0).Item(15), Int32))     ' Y軸スケールの色
                        GrpProp.GridLineWidth = DirectCast(.Rows(0).Item(16), Int32)                    ' 主目盛りのグリッド線の幅
                        GrpProp.GridLineColor = Color.FromArgb(DirectCast(.Rows(0).Item(17), Int32))    ' 主目盛りのグリッド線の色
                        GrpProp.MinorGridLineWidth = DirectCast(.Rows(0).Item(18), Int32)               ' 副目盛りのグリッド線の幅
                        GrpProp.MinorGridLineColor = Color.FromArgb(DirectCast(.Rows(0).Item(19), Int32))   ' 副目盛りのグリッド線の色
                        GrpProp.URL = .Rows(0).Item(20).ToString                                        ' URL
                        GrpProp.ShowLegend = DirectCast(.Rows(0).Item(21), Int32)                       ' 凡例の表示
                        GrpProp.LegendOutlineWidth = DirectCast(.Rows(0).Item(22), Int32)               ' 凡例枠の線幅
                        GrpProp.LegendOutlineColor = Color.FromArgb(DirectCast(.Rows(0).Item(23), Int32))   ' 凡例枠の色
                        GrpProp.MissingDataContinuous = DirectCast(.Rows(0).Item(24), Int32)            ' 欠測データの連結
                        GrpProp.ShowAlert = DirectCast(.Rows(0).Item(25), Int32)                        ' 警報値表示(全体のOff/ON)
                    End If
                End With

                ReDim GrpProp.DrawInfo(GrpProp.GraphCount - 1)          '個々のグラフの情報を格納する配列設定(グラフ個数分)

                'DbDa.Dispose()
                'DtSet.Dispose()

                'DtSet = Nothing

                intRet = 1
            End Using
        End Using

        Return intRet

    End Function

    ''' <summary>
    ''' 各ページに表示する個々のグラフ情報　_2
    ''' </summary>
    ''' <param name="dbCon">OleDBConnection</param>
    ''' <param name="postBackFlg">ポストバックであるかどうか</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function GraphLineInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, Optional ByVal postBackFlg As Boolean = False) As Integer

        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String
        Dim iloop As Integer = 0
        Dim intRet As Integer = 0

        GraphLineInfoFromDB = 0

        strSQL = ("Select * From グラフ情報 WHERE 項目名='" & GrpProp.WindowTitle & "' ORDER BY グラフNo ASC;")
        Using DbDa As New OleDb.OleDbDataAdapter(strSQL, dbCon)
            Using DtSet As New DataSet("DData")
                DbDa.Fill(DtSet, "DData")

                Try
                    For Each DTR As DataRow In DtSet.Tables("DData").Rows                                   'グラフ数でループ
                        If iloop = GrpProp.GraphCount Then                                                  ' 2012/01/18 Kino Add
                            Exit For
                        End If

                        With DTR.ItemArray
                            GrpProp.DrawInfo(iloop).SpecifiedValueEnable = DirectCast(.GetValue(3), Boolean)            '既定値かどうか                     Cnt
                            GrpProp.DrawInfo(iloop).SpecifiedValueIndex = DirectCast(.GetValue(4), Int32)               '既定値の場合のリストインデックス   Cnt
                            GrpProp.DrawInfo(iloop).DataTitle = .GetValue(5).ToString                                   'データ(Y軸)タイトル
                            GrpProp.DrawInfo(iloop).DataTitleSize = DirectCast(.GetValue(6), Int32)                     'データ(Y軸)タイトルサイズ
                            GrpProp.DrawInfo(iloop).DataTitleColor = Color.FromArgb(DirectCast(.GetValue(7), Int32))    'データ(Y軸)タイトル色
                            GrpProp.DrawInfo(iloop).DataUnit = .GetValue(8).ToString                                    '単位タイトル
                            GrpProp.DrawInfo(iloop).Max = DirectCast(.GetValue(9), Single)                              'スケール最大値
                            GrpProp.DrawInfo(iloop).Min = DirectCast(.GetValue(10), Single)                             'スケール最小値
                            GrpProp.DrawInfo(iloop).UnitMajor = DirectCast(.GetValue(11), Single)                       '主メモリ間隔    
                            GrpProp.DrawInfo(iloop).UnitMinor = DirectCast(.GetValue(12), Single)                       '副メモリ間隔
                            GrpProp.DrawInfo(iloop).DataCount = DirectCast(.GetValue(13), Int16)                        'データ数       
                            GrpProp.DrawInfo(iloop).DispDatas = .GetValue(14).ToString                                  'データ表示チャンネル
                            GrpProp.DrawInfo(iloop).DataFileNo = DirectCast(.GetValue(15), Int32)                       'データファイル番号
                            GrpProp.DrawInfo(iloop).SubTitleShow = DirectCast(.GetValue(16), Boolean)                   'サブタイトル有無
                            GrpProp.DrawInfo(iloop).SubTitle = .GetValue(17).ToString                                   'サブタイトル（ヘッダ利用）
                            GrpProp.DrawInfo(iloop).SubTitleSize = DirectCast(.GetValue(18), Int32)                     'サブタイトルサイズ
                            GrpProp.DrawInfo(iloop).BarGraphEnable = DirectCast(.GetValue(19), Boolean)                 '棒グラフ表示有無
                            GrpProp.DrawInfo(iloop).ScatterGraphEanble = DirectCast(.GetValue(20), Boolean)             '散布図表示有無
                            GrpProp.DrawInfo(iloop).ScatterDataLineWidth = DirectCast(.GetValue(21), Int32)             '散布図の場合の線の太さ
                            GrpProp.DrawInfo(iloop).ScatterDataLineType = DirectCast(.GetValue(22), Int32)              '散布図の場合の線の種類
                            GrpProp.DrawInfo(iloop).Revesed = DirectCast(.GetValue(23), Boolean)                        'Y軸反転有無
                            GrpProp.DrawInfo(iloop).Logarithm = DirectCast(.GetValue(24), Boolean)                      'Y軸の対数表示有無
                            GrpProp.DrawInfo(iloop).DataScaleFormat = .GetValue(25).ToString                            'Y軸スケールのフォーマット
                            GrpProp.DrawInfo(iloop).ShowDistance = DirectCast(.GetValue(26), Boolean)                   'X軸の設置距離表示有無
                            GrpProp.DrawInfo(iloop).X_Label = .GetValue(27).ToString                                    'X軸のラベル文字列
                            GrpProp.DrawInfo(iloop).X_LabelAngle = DirectCast(.GetValue(28), Int32)                     'X軸のラベル文字回転角度(0、-90)
                            GrpProp.DrawInfo(iloop).IndivisualAlertData = .GetValue(29).ToString                        '個別管理値設定
                            GrpProp.DrawInfo(iloop).X_Max = DirectCast(.GetValue(30), Single)                           'X軸の最大値
                            GrpProp.DrawInfo(iloop).X_Min = DirectCast(.GetValue(31), Single)                           'X軸の最小値
                            GrpProp.DrawInfo(iloop).X_UnitMajor = DirectCast(.GetValue(32), Single)                     'X軸主目盛り間隔
                            GrpProp.DrawInfo(iloop).X_UnitMinor = DirectCast(.GetValue(33), Single)                     'X軸副目盛り間隔
                            GrpProp.DrawInfo(iloop).LegendWidth = DirectCast(.GetValue(34), Int32)                      '凡例幅
                            GrpProp.DrawInfo(iloop).LegendLeft = DirectCast(.GetValue(35), Single)                      '凡例枠左位置
                            GrpProp.DrawInfo(iloop).AlertType = DirectCast(.GetValue(36), Int32)                        '管理値表示種別
                            GrpProp.DrawInfo(iloop).AlertLineWidth = DirectCast(.GetValue(37), Single)                  '管理値 線タイプ時の線幅割合
                            GrpProp.DrawInfo(iloop).ShowAlert = DirectCast(.GetValue(38), Boolean)                      '個別管理値 表示有無　
                            GrpProp.DrawInfo(iloop).GraphLocation_X = DirectCast(.GetValue(39), Int32)                  'グラフ枠左位置
                            GrpProp.DrawInfo(iloop).MarkerSize = DirectCast(.GetValue(40), Integer)                     'マーカーサイズ
                            GrpProp.DrawInfo(iloop).ShowY2Scale = DirectCast(.GetValue(41), Boolean)                    '右軸スケール表示有無
                            GrpProp.DrawInfo(iloop).FitCurve = Convert.ToInt32(.GetValue(42))                       '散布図の場合に近似曲線とするフラグ　0:直線　1:スプライン　2:ベジェ
                        End With

                        ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount - 1)

                        iloop += 1
                    Next
                    intRet = 1

                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine(ex.ToString)
                    'Finally
                    '    DbDa.Dispose()
                    '    DtSet.Dispose()
                End Try
            End Using
        End Using
        Return intRet

    End Function


    ''' <summary>
    ''' 各オブジェクトを配列変数に格納
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetObj2Arg()

        Dim iloop As Integer

        'オブジェクトの指定: グラフコントロール：スケール設定リストボックス : スケール既定・入力
        For iloop = 0 To 6
            TxtDates(iloop) = DirectCast(FindControl("TxtDate" & iloop.ToString), TextBox)
            imgDates(iloop) = DirectCast(FindControl("ImgDate" & iloop.ToString), System.Web.UI.WebControls.Image)
        Next

        For iloop = 0 To 5
            Grps(iloop) = DirectCast(FindControl("WC" & (iloop + 1).ToString), C1WebChart)
            GrpBox(iloop) = DirectCast(FindControl("PnlGraph" & (iloop + 1).ToString), Panel)
            SetPanel(iloop) = DirectCast(FindControl("Pnl" & (iloop + 1).ToString), UI.HtmlControls.HtmlContainerControl)                              ' 規定値やMax,Minを格納しているDIV
            'LabelNo(iloop) = CType(FindControl("LblNo" + (iloop + 1).ToString), System.Web.UI.WebControls.Label)
            Defval(iloop) = DirectCast(FindControl("DdlScale" & (iloop + 1).ToString), DropDownList)
            scaleDef(iloop) = DirectCast(FindControl("RBList" & (iloop + 1).ToString), RadioButtonList)
            scaleMin(iloop) = DirectCast(FindControl("TxtMin" & (iloop + 1).ToString), TextBox)
            scaleMax(iloop) = CType(FindControl("TxtMax" & (iloop + 1).ToString), TextBox)
        Next

        ''TxtDates(0) = Me.TxtDate0 : imgDates(0) = Me.ImgDate0 'imgGetTime(0) = Me.ImgBtn0 : DDLTimes(0) = Me.DDLTime0
        ''TxtDates(1) = Me.TxtDate1 : imgDates(1) = Me.ImgDate1 'imgGetTime(1) = Me.ImgBtn1 : DDLTimes(1) = Me.DDLTime1
        ''TxtDates(2) = Me.TxtDate2 : imgDates(2) = Me.ImgDate2 'imgGetTime(2) = Me.ImgBtn2 : DDLTimes(2) = Me.DDLTime2
        ''TxtDates(3) = Me.TxtDate3 : imgDates(3) = Me.ImgDate3 'imgGetTime(3) = Me.ImgBtn3 : DDLTimes(3) = Me.DDLTime3
        ''TxtDates(4) = Me.TxtDate4 : imgDates(4) = Me.ImgDate4 'imgGetTime(4) = Me.ImgBtn4 : DDLTimes(4) = Me.DDLTime4
        ''TxtDates(5) = Me.TxtDate5 : imgDates(5) = Me.ImgDate5 'imgGetTime(5) = Me.ImgBtn5 : DDLTimes(5) = Me.DDLTime5
        ''TxtDates(6) = Me.TxtDate6 : imgDates(6) = Me.ImgDate6 'imgGetTime(6) = Me.ImgBtn6 : DDLTimes(6) = Me.DDLTime6

    End Sub

    ''' <summary>
    ''' 各コントロールにDBをバインド
    ''' </summary>
    ''' <param name="dbCon">OleDBConnection</param>
    ''' <remarks></remarks>
    Public Sub SetBIND2Controls(ByVal dbCon As OleDb.OleDbConnection)

        Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strSQL As String

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
        Dim iloop As Integer

        strSQL = "SELECT * FROM set_スケール WHERE 有効 = True ORDER BY ID"
        DtSet = New DataSet("DData")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        For iloop = 0 To 3
            With Defval(iloop)
                .DataSource = DtSet.Tables("DData")
                .DataMember = DtSet.DataSetName
                .DataTextField = "表示"
                .DataValueField = "値"
                .DataBind()
            End With
        Next iloop

        DbDa.Dispose()
        DbCom.Dispose()
        DtSet.Dispose()

    End Sub

    ''' <summary>
    ''' DBから読み込んだ内容をフォームのコントロールに情報を記載する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetInfo2FormControl()

        Dim iloop As Integer

        ''■表示一般設定
        Me.DdlDateFormat.SelectedValue = GrpProp.DateFormat.ToString                            '日付フォーマット
        Me.DdlEnableLegend.SelectedIndex = GrpProp.ShowLegend                                   '凡例表示　0:しない 1:する
        Me.DdlPaperOrientation.SelectedIndex = GrpProp.PaperOrientaion                          '用紙の向き
        Me.DdlTitlePosition.SelectedIndex = GrpProp.TitlePosition                               'グラフタイトル位置
        Me.DdlContinous.SelectedIndex = GrpProp.MissingDataContinuous                           '欠測データの連結　0:しない 1:する
        Me.DDLPaintWarningValue.SelectedIndex = GrpProp.ShowAlert                               '警報値表示　0:しない 1:する

        ''■データスケール設定
        For iloop = 0 To GrpProp.GraphCount - 1
            scaleDef(iloop).SelectedIndex = Convert.ToInt32(Not GrpProp.DrawInfo(iloop).SpecifiedValueEnable)    '既定か入力か Conver.ToInt32の場合Trueは+1となる
            If GrpProp.DrawInfo(iloop).SpecifiedValueEnable = True Then
                Defval(iloop).SelectedIndex = GrpProp.DrawInfo(iloop).SpecifiedValueIndex                       '既定値の場合のリストボックスの項目インデックス
            End If
            scaleMin(iloop).Text = GrpProp.DrawInfo(iloop).Min.ToString                                         ' 最小値
            scaleMax(iloop).Text = GrpProp.DrawInfo(iloop).Max.ToString                                         ' 最大値
        Next iloop

    End Sub

    ''' <summary>
    ''' 使用しているデータファイルの中から、一番最新のデータ日付を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetMostUpdate()

        Dim iloop As Integer
        Dim dteSession As DateTime

        For iloop = 0 To GrpProp.DrawInfo.Length - 1
            dteSession = DateTime.Parse(Session.Item("LastUpdate" & GrpProp.DrawInfo(iloop).DataFileNo.ToString).ToString)
            If GrpProp.LastUpdate < dteSession Then
                GrpProp.LastUpdate = dteSession
            End If
        Next

    End Sub


    ''' <summary>
    ''' 変数から取得した内容をビューステートに格納する     変数　→　ビューステート
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetViewState()

        Dim iloop As Integer
        Dim strCount As String

        ViewState("PointName") = GrpProp.PointName                  'ポイント名称
        ViewState("GBoxCnt") = GrpProp.GraphBoxCount                'グラフ枠の数(パネル)
        ViewState("GCnt") = GrpProp.GraphCount                      'グラフの数(True Web Chartコントロール)
        ViewState("GDTitle") = GrpProp.GraphTitle                   'グラフのタイトル
        ViewState("PgTitle") = GrpProp.WindowTitle                  'ページタイトル
        ViewState("PgTitSz") = GrpProp.TitleSize                    'ページタイトルサイズ
        ViewState("PgTitCol") = GrpProp.TitleColor                  'ページタイトル色
        ViewState("XSclSz") = GrpProp.X_ScaleSize                   'X軸スケールサイズ
        ViewState("XSclCol") = GrpProp.X_ScaleColor                 'X軸スケール色
        ViewState("YSclSz") = GrpProp.Y_ScaleSize                   'Y軸スケールサイズ
        ViewState("YSclCol") = GrpProp.Y_ScaleColor                 'Y軸スケール色
        ViewState("MjGrdLineW") = GrpProp.GridLineWidth             '主目盛りグリッド線太さ
        ViewState("MjGrdLineC") = GrpProp.GridLineColor             '主目盛りグリッド線色
        ViewState("MnGrdLineW") = GrpProp.MinorGridLineWidth        '副目盛りグリッド線太さ
        ViewState("MnGrdLineC") = GrpProp.MinorGridLineColor        '副目盛りグリッド線色
        ViewState("LgdOutLineW") = GrpProp.LegendOutlineWidth       '凡例枠線太さ
        ViewState("LgdOutLineC") = GrpProp.LegendOutlineColor       '凡例枠線色
        ViewState("aspxFile") = GrpProp.URL                         'グラフ表示ファイル名称
        ViewState("AtUpDt") = TmrState                              'タイマーの状態

        For iloop = 0 To GrpProp.GraphCount - 1
            strCount = iloop.ToString
            ViewState("DtTtl" + strCount) = GrpProp.DrawInfo(iloop).DataTitle                               'データタイトル
            ViewState("DtTtlSize" + strCount) = GrpProp.DrawInfo(iloop).DataTitleSize                       'データタイトルサイズ
            ViewState("DtTtlC" + strCount) = GrpProp.DrawInfo(iloop).DataTitleColor                         'データタイトル色
            ViewState("DtUnit" + strCount) = GrpProp.DrawInfo(iloop).DataUnit                               '単位タイトル
            ViewState("YMax" + strCount) = GrpProp.DrawInfo(iloop).Max                                      'スケール最大値
            ViewState("YMin" + strCount) = GrpProp.DrawInfo(iloop).Min                                      'スケール最小値
            ViewState("UtMajor" + strCount) = GrpProp.DrawInfo(iloop).UnitMajor                             '主メモリ間隔    
            ViewState("UtMinor" + strCount) = GrpProp.DrawInfo(iloop).UnitMinor                             '副メモリ間隔
            ViewState("DataCnt" + strCount) = GrpProp.DrawInfo(iloop).DataCount                             'データ数
            ViewState("DataCh" + strCount) = GrpProp.DrawInfo(iloop).DispDatas                              'データチャンネル
            ViewState("DatFlNo" + strCount) = GrpProp.DrawInfo(iloop).DataFileNo                            'データファイル番号  
            ViewState("SubTtlShow" & strCount) = GrpProp.DrawInfo(iloop).SubTitleShow                       'サブタイトル有無
            ViewState("SubTtl" & strCount) = GrpProp.DrawInfo(iloop).SubTitle                               'サブタイトル
            ViewState("SubTtlSz" & strCount) = GrpProp.DrawInfo(iloop).SubTitleSize                         'サブタイトルサイズ
            ViewState("BarGrpShow" & strCount) = GrpProp.DrawInfo(iloop).BarGraphEnable                     '棒グラフ表示有無
            ViewState("SctGrpShow" & strCount) = GrpProp.DrawInfo(iloop).ScatterGraphEanble                 '散布図表示有無
            ViewState("SctGrpLineW" & strCount) = GrpProp.DrawInfo(iloop).ScatterDataLineWidth              '散布図の場合の線幅
            ViewState("SctGrpLineTyp" & strCount) = GrpProp.DrawInfo(iloop).ScatterDataLineType             '散布図の場合の線種
            ViewState("Reverse" & strCount) = GrpProp.DrawInfo(iloop).Revesed                               'Y軸反転
            ViewState("Y_Log" & strCount) = GrpProp.DrawInfo(iloop).Logarithm                               'Y軸対数表示有無
            ViewState("Y_ScaleFormat" & strCount) = GrpProp.DrawInfo(iloop).DataScaleFormat                 'Y軸スケールフォーマット
            ViewState("DistShow" & strCount) = GrpProp.DrawInfo(iloop).ShowDistance                         'X軸 設置距離表示
            ViewState("X_Lbl" & strCount) = GrpProp.DrawInfo(iloop).X_Label                                 'X軸 ラベル
            ViewState("X_LblAgl" & strCount) = GrpProp.DrawInfo(iloop).X_LabelAngle                         'X軸 ラベル表示角度
            ViewState("IndiViAlert" & strCount) = GrpProp.DrawInfo(iloop).IndivisualAlertData               '個別管理値
            ViewState("XMax" & strCount) = GrpProp.DrawInfo(iloop).X_Max                                    'X軸 最大値
            ViewState("XMin" & strCount) = GrpProp.DrawInfo(iloop).X_Min                                    'X軸 最小値
            ViewState("XMajor" & strCount) = GrpProp.DrawInfo(iloop).X_UnitMajor                            'X軸 主目盛り間隔
            ViewState("XMinor" & strCount) = GrpProp.DrawInfo(iloop).X_UnitMinor                            'X軸 副目盛り間隔
            ViewState("LgdWidth" & strCount) = GrpProp.DrawInfo(iloop).LegendWidth                          '凡例枠幅
            ViewState("LgdLeft" & strCount) = GrpProp.DrawInfo(iloop).LegendLeft                            '凡例枠左位置
            ViewState("AltTyp" & strCount) = GrpProp.DrawInfo(iloop).AlertType                              '管理値表示種別
            ViewState("AltLineW" & strCount) = GrpProp.DrawInfo(iloop).AlertLineWidth                       '管理値 線表示　線幅
            ViewState("AltShow" & strCount) = GrpProp.DrawInfo(iloop).ShowAlert                             '管理値 表示
            ViewState("GrpLeftPos" & strCount) = GrpProp.DrawInfo(iloop).GraphLocation_X                    'グラフチャートエリア左位置
            ViewState("MkSize" & strCount) = GrpProp.DrawInfo(iloop).MarkerSize                             'マーカーサイズ
            ViewState("ShowY2Sc" & strCount) = GrpProp.DrawInfo(iloop).ShowY2Scale                          '右軸スケール表示有無
            ViewState("FitCrv" & strCount) = GrpProp.DrawInfo(iloop).FitCurve                               '近似曲線フラグ
        Next iloop

        'For iloop = 10 To 25
        '    ViewState("Color" + (iloop - 10).ToString) = GrpProp.DrawSizeColorInf(iloop - 10)   'グラフの各部分のフォントサイズなど
        'Next iloop

    End Sub

    ''' <summary>
    ''' ビューステートから取得した内容を変数へ格納する　　　ビューステート　→　変数
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ReadViewState()

        Dim iloop As Integer
        Dim strCount As String

        'ReDim GrpProp.DrawSizeColorInf(15)

        GrpProp.PointName = ViewState("PointName").ToString                                 'ポイント名称
        GrpProp.GraphBoxCount = DirectCast(ViewState("GBoxCnt"), Int32)                     'グラフ枠の数(パネル)
        GrpProp.GraphCount = DirectCast(ViewState("GCnt"), Int32)                           'グラフの数(TrueWebChartコントロール)
        GrpProp.GraphTitle = ViewState("GDTitle").ToString                                  'グラフのタイトル
        GrpProp.WindowTitle = ViewState("PgTitle").ToString                                 'ページタイトル
        GrpProp.TitleSize = DirectCast(ViewState("PgTitSz"), Int32)                         'ページタイトルサイズ
        GrpProp.TitleColor = DirectCast(ViewState("PgTitCol"), Color)                       'ページタイトル色
        GrpProp.X_ScaleSize = DirectCast(ViewState("XSclSz"), Int32)                        'X軸スケールサイズ
        GrpProp.X_ScaleColor = DirectCast(ViewState("XSclCol"), Color)                      'X軸スケール色
        GrpProp.Y_ScaleSize = DirectCast(ViewState("YSclSz"), Int32)                        'Y軸スケールサイズ
        GrpProp.Y_ScaleColor = DirectCast(ViewState("YSclCol"), Color)                      'Y軸スケール色
        GrpProp.GridLineWidth = DirectCast(ViewState("MjGrdLineW"), Int32)                  '主目盛りグリッド線太さ
        GrpProp.GridLineColor = DirectCast(ViewState("MjGrdLineC"), Color)                  '主目盛りグリッド線色
        GrpProp.MinorGridLineWidth = DirectCast(ViewState("MnGrdLineW"), Int32)             '副目盛りグリッド線太さ
        GrpProp.MinorGridLineColor = DirectCast(ViewState("MnGrdLineC"), Color)             '副目盛りグリッド線色
        GrpProp.LegendOutlineWidth = DirectCast(ViewState("LgdOutLineW"), Int32)            '凡例枠線太さ
        GrpProp.LegendOutlineColor = DirectCast(ViewState("LgdOutLineC"), Color)            '凡例枠線色
        GrpProp.URL = ViewState("aspxFile").ToString 'グラフ表示ファイル名称
        TmrState = DirectCast(ViewState("AtUpDt"), Boolean)                                 'タイマーの状態

        ReDim GrpProp.DrawInfo(GrpProp.GraphCount - 1)          '個々のグラフの情報を格納する配列設定(グラフ個数分)

        For iloop = 0 To GrpProp.GraphCount - 1
            strCount = iloop.ToString
            GrpProp.DrawInfo(iloop).DataTitle = ViewState("DtTtl" + strCount).ToString                              'データタイトル
            GrpProp.DrawInfo(iloop).DataTitleSize = DirectCast(ViewState("DtTtlSize" + strCount), Int32)            'データタイトルサイズ
            GrpProp.DrawInfo(iloop).DataTitleColor = DirectCast(ViewState("DtTtlC" + strCount), Color)              'データタイトル色
            GrpProp.DrawInfo(iloop).DataUnit = ViewState("DtUnit" + strCount).ToString                              '単位タイトル
            GrpProp.DrawInfo(iloop).Max = DirectCast(ViewState("YMax" + strCount), Single)                          'Y軸スケール最大値
            GrpProp.DrawInfo(iloop).Min = DirectCast(ViewState("YMin" + strCount), Single)                          'Y軸スケール最小値
            GrpProp.DrawInfo(iloop).UnitMajor = DirectCast(ViewState("UtMajor" + strCount), Single)                 '主メモリ間隔
            GrpProp.DrawInfo(iloop).UnitMinor = DirectCast(ViewState("UtMinor" + strCount), Single)                 '副メモリ間隔
            GrpProp.DrawInfo(iloop).DataCount = DirectCast(ViewState("DataCnt" + strCount), Int16)                  'データ数

            ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount - 1)

            GrpProp.DrawInfo(iloop).DispDatas = ViewState("DataCh" + strCount).ToString                             'データチャンネル
            GrpProp.DrawInfo(iloop).DataFileNo = DirectCast(ViewState("DatFlNo" + strCount), Int32)                 'データファイル番号
            GrpProp.DrawInfo(iloop).SubTitleShow = DirectCast(ViewState("SubTtlShow" & strCount), Boolean)          'サブタイトル有無
            GrpProp.DrawInfo(iloop).SubTitle = ViewState("SubTtl" & strCount).ToString                              'サブタイトル
            GrpProp.DrawInfo(iloop).SubTitleSize = DirectCast(ViewState("SubTtlSz" & strCount), Int32)              'サブタイトルサイズ
            GrpProp.DrawInfo(iloop).BarGraphEnable = DirectCast(ViewState("BarGrpShow" & strCount), Boolean)        '棒グラフ表示有無
            GrpProp.DrawInfo(iloop).ScatterGraphEanble = DirectCast(ViewState("SctGrpShow" & strCount), Boolean)    '散布図表示有無 
            GrpProp.DrawInfo(iloop).ScatterDataLineWidth = DirectCast(ViewState("SctGrpLineW" & strCount), Int32)   '散布図の場合の線幅
            GrpProp.DrawInfo(iloop).ScatterDataLineType = DirectCast(ViewState("SctGrpLineTyp" & strCount), Int32)  '散布図の場合の線種
            GrpProp.DrawInfo(iloop).Revesed = DirectCast(ViewState("Reverse" & strCount), Boolean)                  'Y軸反転
            GrpProp.DrawInfo(iloop).Logarithm = DirectCast(ViewState("Y_Log" & strCount), Boolean)                  'Y軸対数表示有無
            GrpProp.DrawInfo(iloop).DataScaleFormat = ViewState("Y_ScaleFormat" & strCount).ToString                'Y軸スケールフォーマット
            GrpProp.DrawInfo(iloop).ShowDistance = DirectCast(ViewState("DistShow" & strCount), Boolean)            'X軸設置距離表示
            GrpProp.DrawInfo(iloop).X_Label = ViewState("X_Lbl" & strCount).ToString                                'X軸ラベル
            GrpProp.DrawInfo(iloop).X_LabelAngle = DirectCast(ViewState("X_LblAgl" & strCount), Int32)              'X軸ラベル表示角度
            GrpProp.DrawInfo(iloop).IndivisualAlertData = ViewState("IndiViAlert" & strCount).ToString              '個別管理値
            GrpProp.DrawInfo(iloop).X_Max = DirectCast(ViewState("XMax" & strCount), Single)                        'X軸最大値
            GrpProp.DrawInfo(iloop).X_Min = DirectCast(ViewState("XMin" & strCount), Single)                        'Y軸最大値
            GrpProp.DrawInfo(iloop).X_UnitMajor = DirectCast(ViewState("XMajor" & strCount), Single)                'X軸主目盛り間隔
            GrpProp.DrawInfo(iloop).X_UnitMinor = DirectCast(ViewState("XMinor" & strCount), Single)                'X軸副目盛り間隔
            GrpProp.DrawInfo(iloop).LegendWidth = DirectCast(ViewState("LgdWidth" & strCount), Int32)               '凡例枠幅
            GrpProp.DrawInfo(iloop).LegendLeft = DirectCast(ViewState("LgdLeft" & strCount), Single)                '凡例枠左位置
            GrpProp.DrawInfo(iloop).AlertType = DirectCast(ViewState("AltTyp" & strCount), Int32)                   '管理値表示種別
            GrpProp.DrawInfo(iloop).AlertLineWidth = DirectCast(ViewState("AltLineW" & strCount), Single)           '管理値線表示　線幅
            GrpProp.DrawInfo(iloop).ShowAlert = DirectCast(ViewState("AltShow" & strCount), Boolean)                '管理値表示
            GrpProp.DrawInfo(iloop).GraphLocation_X = DirectCast(ViewState("GrpLeftPos" & strCount), Int32)         'グラフチャートエリア左位置
            GrpProp.DrawInfo(iloop).MarkerSize = DirectCast(ViewState("MkSize" & strCount), Int32)                  'マーカーサイズ
            GrpProp.DrawInfo(iloop).ShowY2Scale = DirectCast(ViewState("ShowY2Sc" & strCount), Boolean)             '右軸スケール表示有無
            GrpProp.DrawInfo(iloop).FitCurve = DirectCast(ViewState("FitCrv" & strCount), Integer)                  '近似曲線フラグ

        Next iloop

        'For iloop = 10 To 25
        '    GrpProp.DrawSizeColorInf(iloop - 10) = CType(ViewState("Color" + (iloop - 10).ToString), Integer)       'グラフの各部分のフォントサイズなど
        'Next iloop

    End Sub

    ''' <summary>
    ''' コントロールに配置された値を読んで、変数へ格納する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function ReadFromControl() As Integer

        Dim intRet As Integer = 0

        ''■表示一般設定
        GrpProp.DateFormat = Me.DdlDateFormat.SelectedValue                                     '日付フォーマット
        GrpProp.ShowLegend = Me.DdlEnableLegend.SelectedIndex                                   '凡例表示　0:しない 1:する
        GrpProp.PaperOrientaion = Me.DdlPaperOrientation.SelectedIndex                          '用紙の向き
        GrpProp.TitlePosition = Convert.ToInt16(Me.DdlTitlePosition.SelectedIndex)              'グラフタイトル位置
        GrpProp.MissingDataContinuous = Me.DdlContinous.SelectedIndex                           '欠測データの連結
        GrpProp.ShowAlert = Me.DDLPaintWarningValue.SelectedIndex                               '警報値表示

        Return intRet

    End Function

    ''' <summary>
    ''' 各グラフに描画する測定データの情報
    ''' </summary>
    ''' <param name="siteDirectory">現場フォルダ（フルパス）</param>
    ''' <param name="postBackFlg">ポストバックであるかどうか</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function GraphDataInfoFromDB(ByVal siteDirectory As String, Optional ByVal postBackFlg As Boolean = False) As Integer

        Dim strSQL As String = ""
        Dim iloop As Integer
        Dim jloop As Integer
        Dim kloop As Integer
        Dim DataLoop As Integer
        Dim intRet As Integer = 0
        Dim strtemp() As String = {}
        Dim outch() As Integer = {}
        Dim clsconvChData As New ClsReadDataFile
        Dim DbCon As New OleDb.OleDbConnection
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DsetCount As Integer
        Dim RowDatas() As Array = {}
        Dim GetCh As Integer
        'Dim St As Integer
        Dim DtSet As New DataSet("DData")
        Dim strSqlDate As String = "IN("
        ''Dim blnRet As Boolean
        Dim tmpDate As DateTime

        If ToolkitScriptManager1.AsyncPostBackSourceElementID = "ImbtnRedrawGraph" Or ToolkitScriptManager1.AsyncPostBackSourceElementID.Length = 0 Then        'ポストバックがどのコントロールから発生したのかで判断(カレンダーのイベントではここに入らない!!)

            GrpProp.DataCount = 0

            ''■■ 初期設定 警報値の定義 設置距離の読込み
            If Me.RdBFromNewest.Checked = True Then
                ReDim GrpProp.DataTime(0)
                GrpProp.DataCount = 1                                                                   '最新データのみなので、データ数は１つ
            Else
                For iloop = 0 To 6
                    If DateTime.TryParse(TxtDates(iloop).Text, tmpDate) = True Then
                        GrpProp.DataCount += 1S
                        strSqlDate += (tmpDate.ToString("#yyyy/MM/dd HH:mm:ss#") + ",")
                    End If
                Next

                If strSqlDate.EndsWith(",") = True Then strSqlDate.Remove(strSqlDate.Length - 1, 1)
                ReDim GrpProp.DataTime(GrpProp.DataCount - 1)

                If strSqlDate = "IN(" Then strSqlDate = "" '' 2011/04/06 Kino Add 指定日と設定しているのに日付けが設定されていない場合

            End If

            For iloop = 0 To GrpProp.GraphCount - 1                                                         '1ページ当たりのグラフ個数分のループ

                For jloop = 0 To GrpProp.DrawInfo(iloop).DataCount - 1
                    ReDim GrpProp.DrawInfo(iloop).DrawData(jloop).AlertData(5)                                              '警報値配列定義　上限1～3　下限1～3
                Next jloop

                ReDim GrpProp.DrawInfo(iloop).DrawDispData(GrpProp.DataCount - 1, GrpProp.DrawInfo(iloop).DataCount - 1)    '変位データの配列定義(データ系列(測定日付)数,深度) 

                Dim strRet As String = clsconvChData.GetOutputChannel(outch, GrpProp.DrawInfo(iloop).DispDatas, 0)          'チャンネル文字列をチャンネルデータ（配列）に変換

                For jloop = 0 To outch.Length - 1
                    GrpProp.DrawInfo(iloop).DrawData(jloop).DataCh = outch(jloop)                                           ' データのチャンネル番号を格納
                Next

'---------------
                '---CommonInfからの計器記号、設置位置、管理値取得
                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & Server.MapPath(siteDirectory & "\App_Data\" & DataFileNames(GrpProp.DrawInfo(iloop).DataFileNo).CommonInf) & ";" & "Jet OLEDB:Engine Type= 5")
                strSQL = clsconvChData.GetSQLString_Bunpu(GrpProp.DrawInfo(0).DataCount, GrpProp.DrawInfo(0).DrawData, DateTime.Now(), DateTime.Now(), "", True)

                DtSet = New DataSet("DData")                                                                                            ' 2015/05/14 Kino Add　Newしないとデータセットが追記となってしまうため
                '' データセット
                DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
                DbDa.Fill(DtSet, "DData")                                       '全レコードを開く

                DbCon.Dispose()

                DsetCount = DtSet.Tables("DData").Rows.Count                    '全レコード数を取得
                ReDim RowDatas(DirectCast(DtSet.Tables("DData").Rows(DtSet.Tables("DData").Rows.Count - 1).Item(1), Int32))             '最大CH番号で配列を設定   DataBaceChを配列番号として扱う
                Dim DbCh As Integer

                '' 2009/07/17 Kino Changed 高速化
                For jloop = 0 To DsetCount - 1
                    DbCh = DirectCast(DtSet.Tables("DData").Rows(jloop).Item(1), Int32)
                    RowDatas(DbCh) = DtSet.Tables("DData").Rows.Item(DbCh - 1).ItemArray                                                'DRow.ItemArray         'DataBaseChを配列番号として配列に、配列として格納する
                Next jloop

                '---

                GetCh = GrpProp.DrawInfo(iloop).DrawData(0).DataCh
                For DataLoop = 0 To GrpProp.DataCount - 1                                                                           'データ系列数分のループ ↓

                    GrpProp.DataTime(DataLoop) = DateTime.Parse(RowDatas(GetCh).GetValue(0).ToString)                               'データの測定時刻（警報判定時刻）       
                    For jloop = 0 To (GrpProp.DrawInfo(iloop).DataCount - 1)                                                        'センサー数分のループ       
                        GetCh = GrpProp.DrawInfo(iloop).DrawData(jloop).DataCh
                        GrpProp.DrawInfo(iloop).DrawData(jloop).SensorSymbol = RowDatas(GetCh).GetValue(2).ToString                 '●計器記号
                        GrpProp.DrawInfo(iloop).DrawData(jloop).DepthData = DirectCast(RowDatas(GetCh).GetValue(3), Single)         '●設置位置

                        For kloop = 0 To 5                                                                                          '警報値分のループ（上限3つ下限3つ）
                            GrpProp.DrawInfo(iloop).DrawData(jloop).AlertData(kloop) = DirectCast(RowDatas(GetCh).GetValue(kloop + 4), Single) '●警報値
                        Next kloop
                    Next jloop
                Next DataLoop
                DbDa.Dispose()
'---------------

                '■■最新データの場合       
                If Me.RdBFromNewest.Checked = True Then
                    ''警報値、計器記号、測定時刻の読込み 上下限警報値1～3・データ・測定時刻・計器記号 取得
                    For DataLoop = 0 To (GrpProp.DataCount - 1)                                                                     'データ系列数分のループ ↓
                        GrpProp.DataTime(DataLoop) = DateTime.Parse(RowDatas(GetCh).GetValue(0).ToString)                           '　データの測定時刻
                        For jloop = 0 To (GrpProp.DrawInfo(iloop).DataCount - 1)                                                    '　センサー数分のループ
                            GetCh = GrpProp.DrawInfo(iloop).DrawData(jloop).DataCh
                            GrpProp.DrawInfo(iloop).DrawDispData(DataLoop, jloop) = DirectCast(RowDatas(GetCh).GetValue(10), Single)    '●変位データ  
                        Next jloop
                    Next DataLoop
                    DtSet.Dispose()
                    DbCon.Dispose()

                    intRet = 1

                Else
                    '■■過去データの場合

                    ''上の警報値読込みの分をDispose
                    DtSet.Dispose()
                    DbCon.Dispose()

                    If strSqlDate.Length = 0 Then
                        intRet = 0
                        Return intRet
                        Exit Function
                    Else
                        strSqlDate = strSqlDate.Substring(0, strSqlDate.Length - 1) + ")" '' 2011/04/06 Kino Add If...
                    End If

                    ''For iloop = 0 To GrpProp.GraphCount - 1                                                             '■■■■グラフ個数分のループ 2015/05/25 Kino Changed 個別のループを大きな１つのループにした
                    Dim NewDataCount As Integer = GrpProp.DrawInfo(iloop).DataCount - 1
                    DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Server.MapPath(siteDirectory + "\App_Data\" + DataFileNames(GrpProp.DrawInfo(iloop).DataFileNo).FileName) + ";" + "Jet OLEDB:Engine Type= 5")

                    strSQL = clsconvChData.GetSQLString_Bunpu(Convert.ToInt16(NewDataCount), GrpProp.DrawInfo(iloop).DrawData, Now(), Now(), strSqlDate, False)

                    '' データセット
                    DtSet = New DataSet("DData")
                    DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
                    DbDa.Fill(DtSet, "DData")                                                                       '指定日時のレコードを取得

                    DbCon.Dispose()

                    DsetCount = DtSet.Tables("DData").Rows.Count                                                        'データ数(日付数)

                    DataLoop = 0
                    For Each DTR As DataRow In DtSet.Tables("DData").Rows
                        GrpProp.DataTime(DataLoop) = DirectCast(DTR.Item(0), DateTime)                       'データの測定時刻 
                        For jloop = 0 To NewDataCount '- 1                                                              '2010/08/25 Kino Changed 
                            GrpProp.DrawInfo(iloop).DrawDispData(DataLoop, jloop) = DirectCast(DTR.Item(jloop + 1), Single)   '●変位データ
                        Next jloop
                        DataLoop += 1
                    Next
                    ''Next iloop

                    DtSet.Dispose()
                    DbDa.Dispose()

                    intRet = 1
                End If

                Array.Clear(RowDatas, 0, RowDatas.Length)                                                               ' 2015/05/25 Kino Add
            Next iloop

        clsconvChData = Nothing

        DbCon.Dispose()                                                                                                 ' 2015/05/25 Kino Add
        DbCon = Nothing                                                                                                 ' 2015/05/25 Kino Add

        End If

        Return intRet

    End Function


    Protected Sub ImgBtn1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
        ''
        '' 時刻取得ボタンクリックベント
        ''
        Dim DbCon As New OleDb.OleDbConnection
        Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim DRow As DataRow
        Dim iloop As Integer
        Dim Work As String = ""
        Dim DataCount As Short = 0
        Dim DataCh() As Integer = {}
        Dim stDate As DateTime
        Dim edDate As DateTime
        Dim DataFilePath As String = ""
        Dim Datas() As Date
        Dim clsSQL As New ClsReadDataFile

        stDate = Date.Parse(Me.TxtDate1.Text)
        edDate = Date.Parse(stDate.ToString("yyyy/MM/dd 23:59:59"))

        Work = clsSQL.GetSQLString(DataCount, DataCh, stDate, edDate)

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DataFilePath & ";" & "Jet OLEDB:Engine Type= 5")
        DbCon.Open()

        '' 現場名称の読込
        DbDa = New OleDb.OleDbDataAdapter(Work, DbCon)
        DbDa.Fill(DtSet, "DData")

        Dim DsetCount As Integer = DtSet.Tables("DData").Rows.Count - 1
        ReDim Datas(DsetCount)
        For iloop = 0 To DsetCount
            DRow = DtSet.Tables("DData").Rows.Item(iloop)
            ''Datas(iloop) = DRow.ItemArray
            'For jloop = 0 To DataCount
            '    strData = strData & DRow(jloop) & ","
            'Next jloop
            'strData = strData & Environment.NewLine
        Next iloop

        ''GetDrawData = 1

    End Sub

    Protected Sub SetCalenderFunction()
        ''
        '' テキストボックスとイメージにカレンダー機能を割り当てる
        ''
        'Dim TextDate(3) As TextBox
        'Dim imgDate(3) As System.Web.UI.WebControls.Image
        'Dim imgButton(3) As ImageButton
        'Dim dDLTime(3) As DropDownList
        'Dim rangeValid(3) As RangeValidator
        'Dim iloop As Integer

        'TextDate(0) = Me.TxtDate1 : imgDate(0) = Me.imgCalTxtDate0 : rangeValid(0) = Me.RngValid1
        'TextDate(1) = Me.TxtDate2 : imgDate(1) = Me.imgCalTxtDate1 : rangeValid(1) = Me.RngValid2
        'TextDate(2) = Me.TxtDate3 : imgDate(2) = Me.imgCalTxtDate2 : rangeValid(2) = Me.RngValid3
        'TextDate(3) = Me.TxtDate4 : imgDate(3) = Me.imgCalTxtDate3 : rangeValid(3) = Me.RngValid4
        'TextDate(4) = Me.TxtDate5 : imgDate(4) = Me.imgCalTxtDate4 : rangeValid(4) = Me.RngValid5
        'TextDate(5) = Me.TxtDate6 : imgDate(5) = Me.imgCalTxtDate5 : rangeValid(5) = Me.RngValid6

        'For iloop = 0 To 5
        '    TextDate(iloop).Visible = True
        '    imgDate(iloop).Visible = True
        '    imgButton(iloop).Visible = True
        '    dDLTime(iloop).Visible = True
        '    rangeValid(iloop).Visible = True
        '    TextDate(iloop).Attributes.Add("onblur", "validateDate(this);")
        '    imgDate(iloop).Attributes.Add("onclick", "DropCalendar(this);")
        'Next

        Dim iloop As Integer

        For iloop = 0 To 6
            TxtDates(iloop).Attributes.Clear()
            imgDates(iloop).Attributes.Clear()

            TxtDates(iloop).Visible = True
            imgDates(iloop).Visible = True

            imgDates(iloop).Attributes.Add("onclick", "DropCalendar(this);")
        Next

    End Sub


    Protected Sub SetControlVisible()
        ''
        '' グラフ枠、設定項目の表示を設定
        ''
        Dim iloop As Integer
        ''Dim GrpBox(5) As WebControls.Panel
        ''Dim Grps(5) As C1.Web.C1WebChart.C1WebChart
        ''Dim LabelNo(5) As WebControls.Label
        ''Dim SetPanel(5) As WebControls.Panel
        ''Dim SetPanelR(5) As WebControls.Panel
        Dim GrpHeight As Integer
        Dim GrpWidth As Integer
        Dim ChartHeight As Integer
        Dim ChartWidth As Integer
        Dim tableRow(5) As HtmlControls.HtmlTableRow

        ''GrpProp.GraphBoxCount = 4
        ''GrpProp.GraphCount = 4
        ''GrpProp.TitlePosition = 1
        ''GrpProp.GraphTitle = "変位分布図"
        GrpProp.PaperOrientaion = Me.DdlPaperOrientation.SelectedIndex

        For iloop = 0 To 5
            tableRow(iloop) = CType(FindControl("r" + (iloop + 1).ToString), HtmlControls.HtmlTableRow)
        Next iloop

        'オブジェクトの指定:グラフ                  'オブジェクトの指定:左軸関連                     'オブジェクトの指定:右軸関連
        'Grps(0) = Me.WC1 : GrpBox(0) = Me.PnlGraph1 : SetPanel(0) = Me.Pnl1 : LabelNo(0) = Me.LblNo1 : SetPanelR(0) = Me.PnlR1
        'Grps(1) = Me.WC2 : GrpBox(1) = Me.PnlGraph2 : SetPanel(1) = Me.Pnl2 : LabelNo(1) = Me.LblNo2 : SetPanelR(1) = Me.PnlR2
        'Grps(2) = Me.WC3 : GrpBox(2) = Me.PnlGraph3 : SetPanel(2) = Me.Pnl3 : LabelNo(2) = Me.LblNo3 : SetPanelR(2) = Me.PnlR3
        'Grps(3) = Me.WC4 : GrpBox(3) = Me.PnlGraph4 : SetPanel(3) = Me.Pnl4 : LabelNo(3) = Me.LblNo4 : SetPanelR(3) = Me.PnlR4

        'グラフと枠の表示設定
        For iloop = 0 To 5
            If iloop <= GrpProp.GraphCount - 1 Then
                GrpBox(iloop).Visible = True
                Grps(iloop).Visible = True
                SetPanel(iloop).Visible = True
                'LabelNo(iloop).Visible = True
            Else
                Me.Controls.Remove(Grps(iloop))
                Grps(iloop).Dispose()
                Me.Controls.Remove(SetPanel(iloop))
                'Controls.Remove(LabelNo(iloop))
                'Controls.Remove(SetPanelR(iloop))
                Me.Controls.Remove(GrpBox(iloop))
                GrpBox(iloop).Dispose()
                tableRow(iloop).Visible = False                                 ' 2016/01/29 Kino Add これで行が削除できる
            End If
        Next

        '======================================================== [ グラフサイズ・表示設定 ] ================================================↓
        For iloop = 0 To GrpProp.GraphBoxCount - 1
            With Grps(iloop)

                'グラフサイズの調整
                Select Case GrpProp.PaperOrientaion
                    Case 0      'A4縦の場合
                        GrpHeight = Convert.ToInt32(Math.Truncate(913 / GrpProp.GraphBoxCount))                 'FIX
                        GrpWidth = 650
                        ChartHeight = GrpHeight
                        If GrpProp.ShowLegend = 1 Then ChartWidth = 520 Else ChartWidth = 620
                    Case 1      'A4横の場合
                        GrpHeight = Convert.ToInt32(Math.Truncate(587 / GrpProp.GraphBoxCount))                 'FIX
                        GrpWidth = 1000
                        ChartHeight = GrpHeight
                        If GrpProp.ShowLegend = 1 Then ChartWidth = 860 Else ChartWidth = 960
                    Case 2      'A3縦の場合
                    Case 3      'A3横の場合
                End Select

                GrpBox(iloop).Height = GrpHeight                            'パネルの高さ
                GrpBox(iloop).Width = GrpWidth                              'パネルの幅
                .Height = GrpHeight                                         'WebChartの高さ
                .Width = GrpWidth                                           'WebChartの幅
                .ChartArea.Location = New System.Drawing.Point(10, 0)
                .ChartArea.Size = New Size(ChartWidth, ChartHeight)         'チャートエリアの幅と高さ
                '.ChartArea.Margins.SetMargins(15, 15, 5, 15)
                .ChartArea.Margins.SetMargins(15, 13, 5, 5)

                '================================================ [ グラフサイズ・表示設定 ] ================================================↑
            End With
        Next iloop

        'グラフタイトル 表示位置、フォントサイズ設定
        Dim objLabel As System.Web.UI.WebControls.Label
        If GrpProp.TitlePosition = 0 Then
            objLabel = Me.LblTitleUpper
            Me.LblTitleUpper.Visible = True
            Me.LblTitleLower.Visible = False
        Else
            objLabel = Me.LblTitleLower
            Me.LblTitleUpper.Visible = False
            Me.LblTitleLower.Visible = True
        End If
        objLabel.Text = GrpProp.GraphTitle
        objLabel.Font.Size = GrpProp.TitleSize
        objLabel.ForeColor = GrpProp.TitleColor
        objLabel.Width = GrpWidth

    End Sub

    Protected Sub SetGraphCommonInit()
        ''
        '' グラフ描画
        ''
        Dim iloop As Integer
        Dim jloop As Integer
        Dim kloop As Integer
        Dim MarkerInfo(6) As Integer
        Dim LineColorInfo(6) As String
        Dim clsGraphInf As New ClsReadDataFile
        Dim intRet As Integer
        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ
        Dim DBPath As String
        Dim strShowGL As String = ""                                                '2009/09/15 Kino Add
        Dim Y1 As Chart2DTypeEnum                                                   ' 2016/05/12 Kino Add ↓
        Dim Y2 As Chart2DTypeEnum
        Dim ChartGp1 As ChartGroup = Nothing
        Dim ChartGp2 As ChartGroup = Nothing                                        ' 2016/05/12 Kino Add ↑

        Dim UsePlotArea As Boolean = True

        'オブジェクトの指定: グラフコントロール：スケール設定リストボックス : スケール既定・入力    Privateとした
        ''Grps(0) = Me.WC1 : Defval(0) = Me.DdlScale1 : scaleDef(0) = Me.RBList1
        ''Grps(1) = Me.WC2 : Defval(1) = Me.DdlScale2 : scaleDef(1) = Me.RBList2
        ''Grps(2) = Me.WC3 : Defval(2) = Me.DdlScale3 : scaleDef(2) = Me.RBList3
        ''Grps(3) = Me.WC4 : Defval(3) = Me.DdlScale4 : scaleDef(3) = Me.RBList4
        ''scaleMin(0) = Me.TxtMin1 : scaleMax(0) = Me.TxtMax1
        ''scaleMin(1) = Me.TxtMin2 : scaleMax(1) = Me.TxtMax2
        ''scaleMin(2) = Me.TxtMin3 : scaleMax(2) = Me.TxtMax3
        ''scaleMin(3) = Me.TxtMin4 : scaleMax(3) = Me.TxtMax4

        Try
            DBPath = Server.MapPath(siteDirectory & "\App_Data\DispDistChartGraphInfo.mdb")
            ''If GrpProp.DrawInfo(iloop).Marker = True Then                                                           'マーカ情報読み込み    iloop???

            '' ''If GrpProp.DrawInfo(0).Marker = True Then                                                               'マーカ情報読み込み  2013/11/08 このままでは、グラフごとのマーカー有無設定が有効にならないので、判定をコメント
            intRet = clsGraphInf.GetMarkerNumber(DBPath, MarkerInfo)
            ReDim Preserve MarkerInfo(intRet)
            '' ''End If

            intRet = clsGraphInf.GetLineColor(DBPath, LineColorInfo)                                                'データ線色情報読み込み
            ReDim Preserve LineColorInfo(intRet)

            For iloop = 0 To GrpProp.GraphCount - 1                                                                 '■■■グラフ数のループ
                With Grps(iloop)
                    Dim txtToolTip As String = ""
                    If Me.DdlGraphColorType.SelectedIndex = 1 Then                                                  'グラフの色をグレースケールにするかどうか
                        Grps(iloop).UseGrayscale = True
                    End If
                    .ChartArea.PlotArea.UseAntiAlias = False
                    '方向性ヘッダを軸タイトルではなくヘッダとした場合のソース
                    If GrpProp.DrawInfo(iloop).SubTitle.Length > 0 Then
                        .Header.Location = New System.Drawing.Point(GrpProp.DrawInfo(iloop).SubTitleSize, 3)
                        .Header.Compass = CompassEnum.North
                        .Header.Text = GrpProp.DrawInfo(iloop).SubTitle ' + Environment.NewLine + Environment.NewLine + Environment.NewLine
                        .Header.Style.HorizontalAlignment = AlignHorzEnum.Near
                        .Header.Style.GradientStyle = GradientStyleEnum.None
                        .Header.Visible = True
                    Else

                    End If

                    '描画するグラフの判定　基本的には散布図を表示
                    If GrpProp.DrawInfo(iloop).BarGraphEnable = True AndAlso GrpProp.DrawInfo(iloop).ScatterGraphEanble = True Then
                        '棒グラフと散布図がONの場合
                        Y1 = Chart2DTypeEnum.XYPlot
                        Y2 = Chart2DTypeEnum.Bar
                        ChartGp1 = .ChartGroups.Group0
                        ChartGp2 = .ChartGroups.Group1

                    ElseIf (GrpProp.DrawInfo(iloop).BarGraphEnable = True) Then
                        '棒グラフのみONの場合
                        Y1 = Chart2DTypeEnum.Bar
                        Y2 = Nothing
                        ChartGp1 = .ChartGroups.Group0
                        .ChartGroups(0).ShowOutline = False
                        ChartGp2 = Nothing

                    ElseIf GrpProp.DrawInfo(iloop).ScatterGraphEanble = True Or (GrpProp.DrawInfo(iloop).BarGraphEnable = False AndAlso GrpProp.DrawInfo(iloop).ScatterGraphEanble = False) Then
                        '散布図のみONの場合と　どっちもOFFの場合
                        Y1 = Chart2DTypeEnum.XYPlot
                        Y2 = Nothing
                        ChartGp1 = .ChartGroups.Group0
                        ChartGp2 = Nothing

                    End If

                    '●●●●●　X1軸設定
                    With .ChartArea.AxisX
                        ' フォントスタイルの設定
                        Dim XFont As Single
                        If GrpProp.X_ScaleSize <= 0 Then
                            XFont = .Font.Size
                        Else
                            XFont = GrpProp.X_ScaleSize
                        End If
                        Dim XScalefnt As New Font(.Font.Name, XFont)
                        .Font = XScalefnt

                        .Text = ""
                        '.Text = (GrpProp.DrawInfo(iloop).SubTitle & Environment.NewLine + Environment.NewLine)  'X軸タイトル
                        .Compass = CompassEnum.South
                        .Alignment = Drawing.StringAlignment.Center
                        .AnnoFormat = FormatEnum.NumericGeneral                     'ラベルの表示方法は　数値
                        .Visible = True
                        If GrpProp.DrawInfo(iloop).ShowDistance = True Then         ' 横軸は距離を出すか、計器記号とするか
                            .AnnoMethod = AnnotationMethodEnum.Values
                        Else
                            .AnnoMethod = AnnotationMethodEnum.ValueLabels
                            For kloop = 0 To GrpProp.DrawInfo(iloop).DataCount - 1
                                .ValueLabels.Add(GrpProp.DrawInfo(iloop).DrawData(kloop).DepthData, GrpProp.DrawInfo(iloop).DrawData(kloop).SensorSymbol)
                            Next
                        End If
                        .ForeColor = GrpProp.X_ScaleColor
                        .TickMinor = TickMarksEnum.Outside                          '副メモリの突出方向
                        .TickMajor = TickMarksEnum.Outside                          '主メモリの突出方向
                        .GridMajor.Visible = True                                   '主メモリ表示
                        .Thickness = GrpProp.GridLineWidth
                        .AnnotationRotation = GrpProp.DrawInfo(iloop).X_LabelAngle

                        .GridMajor.Thickness = GrpProp.GridLineWidth                ' 主目盛り関連
                        If GrpProp.GridLineColor.Name <> "ffffffff" Then            ' 　色指定 -1 指定は変更なし
                            .GridMajor.Color = GrpProp.GridLineColor
                        End If

                        .GridMinor.Thickness = GrpProp.MinorGridLineWidth           ' 副目盛り関連
                        If GrpProp.MinorGridLineColor.Name <> "ffffffff" Then       '　　色指定 -1 は表示もしない
                            .GridMinor.Color = GrpProp.MinorGridLineColor
                            .GridMinor.Visible = True                               '副メモリ表示
                        Else
                            .GridMinor.Visible = False
                        End If

                        If GrpProp.DrawInfo(iloop).X_Max = -99 Then                 ' X軸最大値
                            .AutoMax = True
                        Else
                            .Max = Convert.ToDouble(GrpProp.DrawInfo(iloop).X_Max.ToString)
                        End If

                        If GrpProp.DrawInfo(iloop).X_UnitMajor <= 0 Then
                            .AutoMajor = True                                           '主目盛りのラベルを自動的に計算
                            .GridMajor.AutoSpace = True                                 '主目盛りのグリッド線の間隔を自動的に計算
                        Else
                            .UnitMajor = Convert.ToDouble(GrpProp.DrawInfo(iloop).X_UnitMajor.ToString)            '主目盛りラベル
                            .GridMajor.Spacing = Convert.ToDouble(GrpProp.DrawInfo(iloop).X_UnitMajor.ToString)    '主目盛りのグリッド線の間隔
                        End If

                        If GrpProp.DrawInfo(iloop).X_Min = -99 Then                 'X軸最小値
                            .AutoMin = True
                        Else
                            .Min = Convert.ToDouble(GrpProp.DrawInfo(iloop).X_Min.ToString)
                        End If
                        If GrpProp.DrawInfo(iloop).X_UnitMinor <= 0 Then
                            .AutoMinor = True
                            .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                        Else
                            .UnitMinor = Convert.ToDouble(GrpProp.DrawInfo(iloop).X_UnitMinor.ToString)
                            .GridMinor.Spacing = Convert.ToDouble(GrpProp.DrawInfo(iloop).X_UnitMinor)
                        End If
                        .Origin = .Min

                    End With

                    '●●●●●　Y1軸設定
                    With .ChartArea.AxisY
                        Dim YFont As Single
                        If GrpProp.Y_ScaleSize <= 0 Then
                            YFont = .Font.Size
                        Else
                            YFont = GrpProp.Y_ScaleSize
                        End If
                        Dim footerfnt As New Font(.Font.Name, YFont)
                        .Font = footerfnt
                        .AnnoFormat = FormatEnum.NumericManual                          'Y軸ラベルの表示方法はしない
                        If GrpProp.DrawInfo(iloop).DataScaleFormat.Length <> 0 Then
                            .AnnoFormatString = GrpProp.DrawInfo(iloop).DataScaleFormat
                        Else
                            .AnnoFormatString = " ; "
                        End If
                        .Text = GrpProp.DrawInfo(iloop).DataUnit
                        .Compass = CompassEnum.West
                        .Visible = True
                        .ForeColor = GrpProp.Y_ScaleColor                           '線色
                        .TickMinor = TickMarksEnum.Outside                          '副メモリの突出方向
                        .TickMajor = TickMarksEnum.Outside                          '主メモリの突出方向

                        .GridMajor.Thickness = GrpProp.GridLineWidth                ' 主目盛り関連
                        .GridMajor.Visible = True
                        If GrpProp.GridLineColor.Name <> "ffffffff" Then            '　　色指定 -1 なら変更なし
                            .GridMajor.Color = GrpProp.GridLineColor
                        End If

                        .GridMinor.Thickness = GrpProp.MinorGridLineWidth           ' 副メモリ関連
                        If GrpProp.MinorGridLineColor.Name <> "ffffffff" Then       '　　色指定 -1 なら表示もなし
                            .GridMinor.Color = GrpProp.MinorGridLineColor
                            .GridMinor.Visible = True
                        End If

                        .Reversed = GrpProp.DrawInfo(iloop).Revesed                 ' 軸反転
                        .IsLogarithmic = GrpProp.DrawInfo(iloop).Logarithm          ' 対数スケール

                        .GridMajor.AutoSpace = True                                     '主メモリのグリッド線の間隔を自動的に計算
                        .GridMinor.AutoSpace = True                                     '副メモリのグリッド線の間隔を自動的に計算
                        .AutoMajor = True                                               '主目盛の値を自動的に計算
                        .AutoMinor = True                                               '副目盛の値を自動的に計算

                        'If GrpProp.DrawInfo(iloop).SpecifiedValueEnable = True Then  '既定値か入力値か
                        If scaleDef(iloop).SelectedIndex = 0 Then                       ' 既定の場合
                            If Defval(iloop).SelectedValue = "9999" Then
                                .AutoMax = True
                                .AutoMin = True
                            Else
                                .Max = Convert.ToDouble(Defval(iloop).SelectedValue.ToString)       'スケール最大値
                                .Min = -Convert.ToDouble(Defval(iloop).SelectedValue.ToString)      'スケール最小値
                            End If
                        Else                                                            ' 入力の場合
                            .Max = Convert.ToDouble(GrpProp.DrawInfo(iloop).Max.ToString)           'スケール最大値
                            .Min = Convert.ToDouble(GrpProp.DrawInfo(iloop).Min.ToString)           'スケール最小値
                            .Max = Convert.ToDouble(scaleMax(iloop).Text)           'スケール最大値
                            .Min = Convert.ToDouble(scaleMin(iloop).Text)           'スケール最小値

                            If GrpProp.DrawInfo(iloop).UnitMajor > 0 AndAlso GrpProp.DrawInfo(iloop).Max = Convert.ToSingle(scaleMax(iloop).Text) Then      ' 主目盛り
                                .UnitMajor = Convert.ToDouble(GrpProp.DrawInfo(iloop).UnitMajor.ToString)
                                .GridMajor.Spacing = Convert.ToDouble(GrpProp.DrawInfo(iloop).UnitMajor.ToString)
                            End If
                            If GrpProp.DrawInfo(iloop).UnitMinor > 0 AndAlso GrpProp.DrawInfo(iloop).Max = Convert.ToSingle(scaleMax(iloop).Text) Then      ' 副目盛り
                                .UnitMinor = Convert.ToDouble(GrpProp.DrawInfo(iloop).UnitMinor.ToString)
                                .GridMinor.Spacing = Convert.ToDouble(GrpProp.DrawInfo(iloop).UnitMinor.ToString)
                            End If
                        End If
                    End With

                    '●●●●●　Y2軸設定       右軸は左軸と同じ設定にする
                    If ChartGp2 IsNot Nothing Then
                        With .ChartArea.AxisY2
                            Dim YFont As Single
                            If GrpProp.Y_ScaleSize <= 0 Then
                                YFont = .Font.Size
                            Else
                                YFont = GrpProp.Y_ScaleSize
                            End If
                            Dim footerfnt As New Font(.Font.Name, YFont)
                            .Font = footerfnt
                            .AnnoFormat = FormatEnum.NumericManual                          'Y軸ラベルの表示方法はしない
                            If GrpProp.DrawInfo(iloop).DataScaleFormat.Length <> 0 Then
                                .AnnoFormatString = GrpProp.DrawInfo(iloop).DataScaleFormat
                            Else
                                .AnnoFormatString = " ; "
                            End If

                            .Text = GrpProp.DrawInfo(iloop).DataUnit
                            .Compass = CompassEnum.East
                            .Visible = True
                            .ForeColor = GrpProp.Y_ScaleColor                           '線色
                            .TickMinor = TickMarksEnum.Outside                          '副メモリの突出方向
                            .TickMajor = TickMarksEnum.Outside                          '主メモリの突出方向

                            .GridMajor.Thickness = GrpProp.GridLineWidth                ' 主目盛り関連
                            .GridMajor.Visible = True
                            If GrpProp.GridLineColor.Name <> "ffffffff" Then            '　　色指定 -1 なら変更なし
                                .GridMajor.Color = GrpProp.GridLineColor
                            End If

                            .GridMinor.Thickness = GrpProp.MinorGridLineWidth           ' 副メモリ関連
                            If GrpProp.MinorGridLineColor.Name <> "ffffffff" Then       '　　色指定 -1 なら表示もなし
                                .GridMinor.Color = GrpProp.MinorGridLineColor
                                .GridMinor.Visible = True
                            End If

                            .Reversed = GrpProp.DrawInfo(iloop).Revesed                 ' 軸反転
                            .IsLogarithmic = GrpProp.DrawInfo(iloop).Logarithm          ' 対数スケール

                            .GridMajor.AutoSpace = True                                     '主メモリのグリッド線の間隔を自動的に計算
                            .GridMinor.AutoSpace = True                                     '副メモリのグリッド線の間隔を自動的に計算
                            .AutoMajor = True                                               '主目盛の値を自動的に計算
                            .AutoMinor = True                                               '副目盛の値を自動的に計算

                            If GrpProp.DrawInfo(iloop).SpecifiedValueEnable = True Then  '既定値か入力値か
                                If Defval(iloop).SelectedValue = "9999" Then
                                    .AutoMax = True
                                    .AutoMin = True
                                Else
                                    .Max = Convert.ToDouble(Defval(iloop).SelectedValue.ToString)       'スケール最大値
                                    .Min = -Convert.ToDouble(Defval(iloop).SelectedValue.ToString)      'スケール最小値
                                End If
                            Else
                                .Max = Convert.ToDouble(GrpProp.DrawInfo(iloop).Max.ToString)           'スケール最大値
                                .Min = Convert.ToDouble(GrpProp.DrawInfo(iloop).Min.ToString)           'スケール最小値

                                .Max = Convert.ToDouble(scaleMax(iloop).Text)           'スケール最大値
                                .Min = Convert.ToDouble(scaleMin(iloop).Text)           'スケール最小値

                                If GrpProp.DrawInfo(iloop).UnitMajor > 0 Then                           ' 主目盛り
                                    .UnitMajor = Convert.ToDouble(GrpProp.DrawInfo(iloop).UnitMajor.ToString)
                                    .GridMajor.Spacing = Convert.ToDouble(GrpProp.DrawInfo(iloop).UnitMajor.ToString)
                                End If
                                If GrpProp.DrawInfo(iloop).UnitMinor > 0 Then                           ' 副目盛り
                                    .UnitMinor = Convert.ToDouble(GrpProp.DrawInfo(iloop).UnitMinor.ToString)
                                    .GridMinor.Spacing = Convert.ToDouble(GrpProp.DrawInfo(iloop).UnitMinor.ToString)
                                End If
                            End If
                            .Visible = GrpProp.DrawInfo(iloop).ShowY2Scale

                        End With

                    Else
                        .ChartArea.AxisY2.Visible = False
                    End If

                    ''●●●●●　描画データの設定
                    .ChartGroups(0).ChartData.SeriesList.Clear()                                                    'データ系列を一括削除
                    .ChartGroups(1).ChartData.SeriesList.Clear()                                                    'データ系列を一括削除
                    .ChartGroups.Group0.ChartData.Hole = 7.7E+30                                                    '描画しないデータ(異常値)
                    .ChartGroups.Group0.ChartData.Hole = 7.7E+30                                                    '描画しないデータ(異常値)

                    ''凡例設定  
                    If GrpProp.LegendOutlineWidth > 0 Then
                        .Legend.Style.Border.Color = GrpProp.LegendOutlineColor
                        .Legend.Style.Border.Thickness = GrpProp.LegendOutlineWidth
                    End If
                    .Legend.Visible = Convert.ToBoolean(GrpProp.ShowLegend)                                          '凡例の有無
                    .Legend.Location = New Point(Convert.ToInt32(.ChartArea.Size.Width * GrpProp.DrawInfo(iloop).LegendLeft * 0.01), .ChartArea.Margins.Top + 8)
                    .Legend.Size = New Size(GrpProp.DrawInfo(iloop).LegendWidth, .Legend.SizeDefault.Height)                                ' 凡例枠の大きさ

                    Dim SensorCount As Integer                                                                   ''実際の深度数(不動点がない場合 -1する)
                    Dim ErrorDataCount As Integer
                    Dim MaxDisp As Single = 0
                    Dim MaxDispPoint As Integer = -1
                    Dim calcTemp As Single


                    ''　　●●●　個々のグラフのデータ軸タイトル　（フッターを使用する）
                    ''.ChartArea.SizeDefault = New Size(500, 470)                                                   ' チャートエリアのサイズは指定できるが、PlotAreaはNG
                    With .Footer
                        Dim footerfnt As New Font(.Style.Font.Name, GrpProp.Y_ScaleSize)
                        .Style.Font = footerfnt
                        ''.Style.BackColor = Color.Aqua
                        ''.Style.Opaque = True
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .Visible = True
                        '.Size = New Size(100, Grps(iloop).ChartArea.SizeDefault.Height)                            'チャートエリアのサイズを指定すると当然下寄りに表示される
                        .Text = GrpProp.DrawInfo(iloop).DataTitle & Environment.NewLine
                        .Compass = CompassEnum.West
                        .Style.Rotation = RotationEnum.Rotate270

                        Dim canvas As New Bitmap(300, 300)                                                          'フォントのサイズと文字から、大きさを求める
                        Dim g As Graphics = Graphics.FromImage(canvas)
                        Dim fnt As New Font(.Style.Font.Name, .Style.Font.Size)
                        Dim sf As New StringFormat
                        g.DrawString(.Text, fnt, Brushes.Black, 0, 0, sf)
                        Dim stringSize As SizeF = g.MeasureString(.Text, fnt, 1000, sf)
                        .Location = New Point(0, Convert.ToInt32(Grps(iloop).ChartArea.Size.Height * 0.5) - Convert.ToInt32(stringSize.Width * 0.5 + Grps(iloop).ChartArea.Margins.Top * 0.4))   '縦位置を計算する
                        'リソースを解放する
                        fnt.Dispose()
                        sf.Dispose()
                        g.Dispose()
                    End With

                    ''　　●●●　データ系列の設定　第１系列の設定（散布図）
                    SensorCount = GrpProp.DrawInfo(iloop).DataCount - 1
                    .ChartGroups(0).ChartType = Y1          'Chart2DTypeEnum.XYPlot
                    For jloop = 0 To GrpProp.DataCount - 1                                                          ''■■データ系列数分ループ

                        'Dim series As ChartDataSeries = .ChartGroups.Group0.ChartData.SeriesList.AddNewSeries()     'データ系列のインスタンスを作成
                        Dim series As ChartDataSeries                                                               ' 2016/05/12 Kino Changed
                        series = ChartGp1.ChartData.SeriesList.AddNewSeries()

                        With series
                            .FitType = DirectCast(GrpProp.DrawInfo(iloop).FitCurve, FitTypeEnum)                               ' 2017/04/10 Kino Add
                            .PointData.Length = GrpProp.DrawInfo(iloop).DataCount
                            If GrpProp.DrawInfo(iloop).MarkerSize > 0 AndAlso Y1 = Chart2DTypeEnum.XYPlot Then                  'マーカサイズが0より大きく、散布図の場合のみ実施     2016/08/08 Kino Changed   Add  [Also]
                                .SymbolStyle.Shape = DirectCast(MarkerInfo(jloop + 1), C1.Win.C1Chart.SymbolShapeEnum)          'マーカの形状設定
                                .SymbolStyle.Color = Color.FromName(LineColorInfo(jloop))                                   'マーカの色
                                .SymbolStyle.Size = GrpProp.DrawInfo(iloop).MarkerSize                                          'マーカのサイズ
                                .SymbolStyle.OutlineColor = Color.FromName(LineColorInfo(jloop))                            'マーカの枠線
                                .SymbolStyle.OutlineWidth = 1 'GrpProp.DrawInfo(iloop).ScatterDataLineWidth                     'マーカの枠線の太さ
                            Else
                                .SymbolStyle.Shape = SymbolShapeEnum.None                                                       'マーカなしに設定
                            End If
                            .LineStyle.Pattern = DirectCast(GrpProp.DrawInfo(iloop).ScatterDataLineType, C1.Win.C1Chart.LinePatternEnum)    '線種
                            .LineStyle.Color = Color.FromName(LineColorInfo(jloop))                                                     '線色
                            .LineStyle.Thickness = GrpProp.DrawInfo(iloop).ScatterDataLineWidth                                             '線の太さ

                            ErrorDataCount = 0
                            For kloop = 0 To SensorCount                                                                        'データ数(センサー数)のループ
                                .Y.Add(Convert.ToDouble(GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop)))                    'DataLoop -> jloop    
                                .X.Add(Convert.ToDouble(GrpProp.DrawInfo(iloop).DrawData(kloop).DepthData))
                                System.Diagnostics.Debug.WriteLine(String.Format("X: {0}  Y: {1}", GrpProp.DrawInfo(iloop).DrawData(kloop).DepthData, GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop)))
                                If GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop) > 1.1E+30 Then
                                    ErrorDataCount += 1
                                End If

                                calcTemp = Math.Abs(GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop))                 '一時的に格納  DataLoop -> jloop   
                                If Math.Abs(MaxDisp) <= Math.Abs(calcTemp) And calcTemp < 1.1E+30 And kloop <= SensorCount Then   '最大変位とその時のインデックスを取得    
                                    MaxDisp = GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop)
                                    MaxDispPoint = kloop                                                                ' 場所はインデックスだけ取得する
                                End If

                            Next kloop

                            If MaxDispPoint >= 0 Then
                                If GrpProp.DrawInfo(iloop).ShowDistance = True Then
                                    txtToolTip = String.Format("最大値：{0:0.00} [{1:0.0} m]", MaxDisp, GrpProp.DrawInfo(iloop).DrawData(MaxDispPoint).DepthData)                 ' MaxDispPoint インデックスのため+1
                                Else
                                    txtToolTip = String.Format("最大値：{0:0.00} [{1}]", MaxDisp, GrpProp.DrawInfo(iloop).DrawData(MaxDispPoint).SensorSymbol)  ' MaxDispPoint インデックスのため+1
                                End If
                            End If

                            If ErrorDataCount = SensorCount Then
                                .Y.Item(SensorCount + 1) = 7.7E+30
                            End If

                            If Me.DdlEnableLegend.SelectedIndex = 1 Then
                                .Label = GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat)
                            End If

                        End With

                        If Me.DdlContinous.SelectedIndex = 1 Then                                                       ' データの連続線設定
                            .ChartGroups(0).ChartData(jloop).Display = SeriesDisplayEnum.ExcludeHoles
                        ''Else
                        ''    .ChartGroups(0).ChartData(jloop).Display = SeriesDisplayEnum.Exclude
                        End If

                    Next jloop

                    ''　　●●●　データ系列の設定　棒グラフ
                    ''If GrpProp.DrawInfo(iloop).BarGraphEnable = True Then                                           ' ●●ここは２系列目の描画　散布図と棒グラフ
                    If ChartGp2 IsNot Nothing Then
                        .ChartGroups(1).ChartType = Y2               'Chart2DTypeEnum.Bar
                        .ChartGroups(1).ShowOutline = False          'True

                        For jloop = 0 To GrpProp.DataCount - 1                                                          ''■■データ系列数分ループ
                            'Dim series As ChartDataSeries = .ChartGroups.Group1.ChartData.SeriesList.AddNewSeries()    'データ系列のインスタンスを作成
                            Dim series As ChartDataSeries
                            series = ChartGp2.ChartData.SeriesList.AddNewSeries()                                       'データ系列のインスタンスを作成

                            With series
                                .PointData.Length = GrpProp.DrawInfo(iloop).DataCount
                                .SymbolStyle.Color = Color.FromName(LineColorInfo(jloop))
                                .FillStyle.Color1 = Color.FromName(LineColorInfo(jloop))

                                'ErrorDataCount = 0                                                                         ' 全く同じデータを描画するので、カウンターリセットは不要と考える
                                For kloop = 0 To SensorCount                                                                 'データ数(深度)のループ
                                    .Y.Add(Convert.ToDouble(GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop)))                    'DataLoop -> jloop      
                                    .X.Add(Convert.ToDouble(GrpProp.DrawInfo(iloop).DrawData(kloop).DepthData))
                                    System.Diagnostics.Debug.WriteLine(String.Format("X: {0}  Y: {1}", GrpProp.DrawInfo(iloop).DrawData(kloop).DepthData, GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop)))
                                Next kloop

                                ''If ErrorDataCount = SensorCount Then
                                ''    .Y.Item(SensorCount + 1) = 7.7E+30
                                ''End If
                                .LegendEntry = False                                                                        ' 棒グラフとしての凡例は出さない

                            End With

                            '.ChartGroups(1).ShowOutline = False                                                             ' 棒グラフのボックス枠線を出さない
                            If Me.DdlContinous.SelectedIndex = 1 Then                                                       ' データの連続線設定
                                .ChartGroups(1).ChartData(jloop).Display = SeriesDisplayEnum.ExcludeHoles
                            End If
                        Next jloop

                    End If

                    If txtToolTip.Length > 2 Then
                        txtToolTip = txtToolTip
                        .ToolTip = txtToolTip
                    End If

                    ''●●●●●　チャートエリア設定
                    With .ChartArea
                        .Style.Border.BorderStyle = BorderStyleEnum.None            'チャートエリアのボーダー
                        .PlotArea.Boxed = True                                      'チャートエリアのボックスを表示する
                        .PlotArea.ForeColor = Drawing.Color.Black                   'ボックスの線色

                        If DDLPaintWarningValue.SelectedIndex = 1 Then              '警報値エリア表示フラグ
                            If GrpProp.DrawInfo(iloop).ShowAlert = True Then        'グラフごとの警報値エリアフラグ
                                '●●●●　警報値領域

                                Dim azloop As Integer
                                Dim azData() As Single = {7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30, 7.7E+30}
                                Dim azMax As Double = 0 '-7.7E+30
                                Dim azMin As Double = 7.7E+30
                                Dim UpperFlg As Short = 0       ''2010/03/26 Kino Add   警報値の設定がない時、上下限の設定が全てスケールMAX/MINの場合に全部塗られるやつの対策
                                Dim LowerFlg As Short = 0       ''2010/03/26 Kino Add

                                Dim azs As AlarmZonesCollection = .PlotArea.AlarmZones
                                Dim az As AlarmZone = azs.AddNewZone
                                az = azs.AddNewZone

                                If GrpProp.DrawInfo(iloop).IndivisualAlertData.Length = 0 Then          '共通情報から読込み(現場情報からの情報）
                                    Call ReadAlarmData(GrpProp.DrawInfo(iloop).DrawData(0).DataCh, GrpProp.DrawInfo(iloop).DataFileNo, azData)
                                Else                                                                    'サブタイトルから読込み(DBに保存した情報)
                                    Dim strazData() As String = {"7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30", "7.7E+30"}

                                    strazData = GrpProp.DrawInfo(iloop).IndivisualAlertData.Split(Convert.ToChar(","))
                                    For azloop = 0 To 2
                                        azData(azloop) = Convert.ToSingle(strazData(azloop))
                                        azData(azloop + 4) = Convert.ToSingle(strazData(azloop + 4))
                                    Next
                                End If

                                For azloop = 0 To 3

                                    If azMax < azData(azloop) And azData(azloop) < 1.1E+30 Then azMax = azData(azloop)
                                    If azData(azloop) > 1.1E+30 Then
                                        If .AxisY.AutoMax = True Then
                                            azData(azloop) = CType(azMax * 10, Single)
                                        End If
                                        If .AxisY.AutoMax = False AndAlso azMax < .AxisY.Max Then
                                            azData(azloop) = Convert.ToSingle(.AxisY.Max)                           '上限3次を塗りつぶす最大値はグラフスケールのMax
                                        End If
                                    Else
                                        UpperFlg = 1
                                    End If

                                    If azMin > azData(azloop + 4) And azData(azloop + 4) < 1.1E+30 Then azMin = azData(azloop + 4) '2009/03/18 Kino Changed 自動スケールの場合は最小値を取得できないので、これにした <= NG
                                    If azData(azloop + 4) > 1.1E+30 Then
                                        If .AxisY.AutoMin = True Then                               '2013/01/09 Kino Add自動スケールの場合は最小値を取得できないので、これにした
                                            azData(azloop + 4) = CType(azMin * 10, Single)
                                            If azData(azloop + 4) > 0 Then azData(azloop + 4) *= -1
                                        End If
                                        If .AxisY.AutoMin = False AndAlso azMin > .AxisY.Min Then
                                            azData(azloop + 4) = Convert.ToSingle(.AxisY.Min)
                                        End If
                                    Else
                                        LowerFlg = 1
                                    End If
                                Next azloop

                                .PlotArea.AlarmZones.Clear()
                                For azloop = 1 To 3
                                    If UpperFlg = 1 Then
                                        ''●上限値
                                        az = azs.AddNewZone
                                        az.BackColor = Color.FromArgb(35 + (azloop - 1) * 15, Color.Red)           '30->15 若干透明度調整
                                        az.Shape = C1.Win.C1Chart.AlarmZoneShapeEnum.Rectangle
                                        az.Name = "RectZoneUp" + azloop.ToString
                                        az.ForeColor = Color.Transparent
                                        If Convert.ToSingle(azData(azloop)) <> Convert.ToSingle(azData(azloop - 1)) Then
                                            az.UpperExtent = Convert.ToSingle(azData(azloop)) - Convert.ToSingle(azData(azloop)) * 0    '0.001        '
                                            az.LowerExtent = Convert.ToSingle(azData(azloop - 1))
                                            az.Visible = True
                                        End If
                                    End If

                                    If LowerFlg = 1 Then
                                        ''●下限値
                                        az = azs.AddNewZone
                                        az.BackColor = Color.FromArgb(35 + (azloop - 1) * 15, Color.Red)
                                        az.ForeColor = Color.Transparent
                                        az.Shape = C1.Win.C1Chart.AlarmZoneShapeEnum.Rectangle
                                        az.Name = "RectZoneLow" + azloop.ToString
                                        If Convert.ToSingle(azData(azloop - 1 + 4)) <> Convert.ToSingle(azData(azloop + 4)) Then
                                            az.UpperExtent = Convert.ToSingle(azData(azloop + 3))                                       '(azloop + 4 - 1) -> azloop + 3
                                            az.LowerExtent = Convert.ToSingle(azData(azloop + 4))
                                            az.Visible = True
                                        End If
                                    End If
                                Next

                            End If
                        End If

                    End With

                End With
            Next

            For iloop = 0 To GrpProp.GraphCount - 1
                With Grps(iloop)
                    ''データ数が増えると、とたんに重くなるので、使用を今後検討してから採用する
                    ''グラフデータにマウスを当てるとデータをツールチップで表示する
                    Dim mac As MapAreaCollection = .ImageAreas
                    'Dim mapPA As MapArea = mac.GetByName("PlotArea")        ' index 4
                    Dim mapData As MapArea = mac.GetByName("ChartData")     ' index 6

                    If UsePlotArea = True Then
                        .EnableCallback = True
                        'mapPA.Tooltip = "プロット領域です"                                  'データ以外の表示
                       ''mapPA.Attributes = "onclick=""ShowDataCoords(event,WC1);"""      'クリック時の表示
                        mapData.Tooltip = "{#YVAL:0.00} [{#XVAL:0.0} m]"                               'データの表示
                    Else
                        .EnableCallback = False
                        ''mapPA.Attributes = String.Empty
                        ''mapPA.HRef = String.Empty
                        ''mapPA.Tooltip = String.Empty
                        mapData.Attributes = String.Empty
                        mapData.HRef = String.Empty
                        mapData.Tooltip = String.Empty
                    End If
                End With
            Next iloop

            For iloop = 0 To 5
                Grps(iloop) = Nothing
                Defval(iloop) = Nothing
            Next

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine(ex.Message)
        End Try


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

        Dim AccDataFile As String = Server.MapPath(siteDirectory & "\App_Data\" & DataFileNames(FileNum).CommonInf)

        sqlOpt = String.Format("DataBaseCh IN({0})", DataCh.ToString)    '"DataBaseCh IN(" + DataCh.ToString + ")"
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Using DbCon As New OleDb.OleDbConnection

            strSQL = ("SELECT 警報値上限1,警報値上限2,警報値上限3,警報値下限1,警報値下限2,警報値下限3 FROM 共通情報 WHERE " & sqlOpt)

            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + AccDataFile + ";" & "Jet OLEDB:Engine Type= 5")
            Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                Using DtSet As New DataSet("DData")
                    DbDa.Fill(DtSet, "DData")

                    For iloop = 0 To 2
                        azData(iloop) = Convert.ToSingle(DtSet.Tables("DData").Rows(0).Item(iloop).ToString)
                        azData(iloop + 4) = Convert.ToSingle(DtSet.Tables("DData").Rows(0).Item(iloop + 3).ToString)
                    Next

                    'DbDa.Dispose()
                    'DtSet.Dispose()
                    'DbCon.Dispose()
                End Using
            End Using
        End Using
    End Sub

    ''Protected Sub TextChange(ByVal sender As Object, ByVal e As System.EventArgs)
    ''    ''
    ''    '' テキストボックスの内容が変更されたときのイベント
    ''    ''

    ''    Dim TextObj As TextBox = FindControl(sender.ClientId)
    ''    Dim GetDate As String = Me.C1WebCalendar1.SelectedDate

    ''    If GetDate.ToString <> "1900/01/01" Then
    ''        TextObj.Text = Me.C1WebCalendar1.SelectedDate
    ''        Me.C1WebCalendar1.SelectedDate = Date.Parse("1900/01/01")
    ''    End If

    ''    System.Diagnostics.Debug.WriteLine(TextObj.Text)

    ''    'Select Case sender.ClientId()
    ''    'sender.Text()

    ''End Sub

    '' '' 以下、各テキストボックスのチェンジイベント
    ''Protected Sub TxtDate1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtDate1.TextChanged
    ''    Call TextChange(sender, e)
    ''End Sub

    ''Protected Sub TxtDate2_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtDate2.TextChanged
    ''    Call TextChange(sender, e)
    ''End Sub

    ''Protected Sub TxtDate3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtDate3.TextChanged
    ''    Call TextChange(sender, e)
    ''End Sub

    ''Protected Sub TxtDate4_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtDate4.TextChanged
    ''    Call TextChange(sender, e)
    ''End Sub

    ''Protected Sub TxtDate5_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtDate5.TextChanged
    ''    Call TextChange(sender, e)
    ''End Sub

    ''Protected Sub TxtDate6_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtDate6.TextChanged
    ''    Call TextChange(sender, e)
    ''End Sub

    ''Protected Sub TxtDate7_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtDate7.TextChanged
    ''    Call TextChange(sender, e)
    ''End Sub

    Protected Sub C1WebCalendar1_DisplayDateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles C1WebCalendar.DisplayDateChanged
        ''
        '' カレンダーコントロールの選択 年／月 を変えた時のイベント
        ''
        'Me.DropDownList1.SelectedValue = Me.C1WebCalendar1.DisplayDate.Year.ToString
        'Me.DropDownList2.SelectedValue = Me.C1WebCalendar1.DisplayDate.Month.ToString

    End Sub

    Protected Sub C1WebCalendar1_SelectedDatesChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles C1WebCalendar.SelectedDatesChanged
        ''
        '' カレンダーコントロールから日付を選択した時に該当日付の測定時刻をリストボックスに記載する
        ''
        Dim DbCon As New OleDb.OleDbConnection
        Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim iloop As Integer
        Dim SelDate As DateTime
        Dim strSQL As String
        Dim siteDirectory As String = CType(Session.Item("SD"), String)                     '現場ディレクトリ

        ''siteDirectory = "totsuka"               '◆◆◆◆◆ デバッグ用
        SelDate = C1WebCalendar.SelectedDate
        C1WebCalendar.DisplayDate = SelDate

        'Me.LblStatus.Text = "選択されている日付：" & SelDate.ToString

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & Server.MapPath(siteDirectory & "\App_Data\" & DataFileNames(GrpProp.DrawInfo(iloop).DataFileNo).FileName) & ";Jet OLEDB:Engine Type= 5")
        ''''DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=I:\WebDataSystem2\totsuka\App_Data\totsuka_CalculatedData.mdb" & ";" & "Jet OLEDB:Engine Type= 5"       '◆◆◆◆◆ デバッグ用
        ''DbCon.Open()

        ''strSQL = "SELECT DISTINCT FORMAT(日付, 'HH:mm:ss') AS 測定時刻,日付 FROM 日付 WHERE (日付 BETWEEN #" & SelDate.ToString("yyyy/MM/dd 0:00:00") & "# AND #" & SelDate.ToString("yyyy/MM/dd 23:59:59") & "#) ORDER BY 日付 ASC"
        strSQL = ("SELECT DISTINCT FORMAT(日付, 'HH:mm') AS 測定時刻,日付.日付 FROM 日付 WHERE (日付 BETWEEN #" + SelDate.ToString("yyyy/MM/dd 0:00:00") + "# AND #" + SelDate.ToString("yyyy/MM/dd 23:59:59") & "#) ORDER BY 日付 ASC")

        DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
        DbDa.Fill(DtSet, "DData")

        ''DbCon.Close()
        DbCon.Dispose()

        Dim DsetCount As Integer = DtSet.Tables("DData").Rows.Count - 1
        Me.LstBTime.Items.Clear()

        If DsetCount = -1 Then                                                      ''測定日時がない場合は-1がくる
            Me.LstBTime.Items.Add("データなし")
        Else
            '' 2009/07/17 Kino Changed 高速化
            ''For iloop = 0 To DsetCount
            ''    Me.LstBTime.Items.Add(DtSet.Tables("DData").Rows.Item(iloop).Item(0))
            ''Next
            iloop = 0
            For Each DTR As DataRow In DtSet.Tables("DData").Rows
                Me.LstBTime.Items.Add(DTR.Item(0).ToString)
                iloop += 1
            Next
        End If

        DbCom.Dispose()
        DbDa.Dispose()

    End Sub

    ''Protected Sub DropDownList1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    ''    ''
    ''    '' 年／月 を変更したときのイベント
    ''    ''
    ''    With Me.C1WebCalendar1
    ''        .DisplayDate = CDate(Me.DropDownList1.SelectedValue & "/" & Me.DropDownList2.SelectedValue & "/" & .SelectedDate.Day)
    ''    End With
    ''End Sub

    Protected Sub LstBTime_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        ''
        '' ドロップダウンリストボックスから時刻を選択したときのイベント
        ''
        Dim popce As PopupControlExtender = PopupControlExtender.GetProxyForCurrentPopup(Me.Page)

        If (popce IsNot Nothing And Me.LstBTime.SelectedItem.Text <> "データなし") Then
            popce.Commit((Me.C1WebCalendar.SelectedDate.ToString("yyyy/MM/dd") + " " + Me.LstBTime.SelectedItem.Text).ToString)
            ''popce.Commit(Me.TxtSelectedDate.Text.ToString)
        Else
            popce.Cancel()
            ''popce.Commit("")
        End If

        popce.Dispose()

    End Sub

    <System.Web.Services.WebMethodAttribute()> <System.Web.Script.Services.ScriptMethodAttribute()> Public Shared Function GetDynamicContent(contextKey As System.String) As System.String

        Return contextKey

    End Function

    Protected Sub WC1_DrawDataSeries(ByVal sender As Object, ByVal e As C1.Win.C1Chart.DrawDataSeriesEventArgs) Handles WC1.DrawDataSeries, _
                                                            WC2.DrawDataSeries, WC3.DrawDataSeries, WC4.DrawDataSeries, WC5.DrawDataSeries, WC6.DrawDataSeries

        '' 折れ線のグラフが鋭角で折り返す場合に、線の幅が太くなるにつれ本来の座標よりも外側の部分にはみ出して描画される問題の回避策
        '' GrapeCity KB25039

        '接合方法を変更するデータセットを判断します
        ' System.Diagnostics.Debug.WriteLine(e.SeriesIndex.ToString)
        If sender.ToString.Length <> 0 Then
            ' System.Diagnostics.Debug.WriteLine(sender.ToString + " " + e.SeriesIndex.ToString + " " + e.GroupIndex.ToString + " を処理")
            ''If e.SeriesIndex <= GrpProp.LineInfo(0).LeftAxis.DataCount - 1 Then  '= 0 Then

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

    Private Function AlarmZoneShapeEnum() As Object
        Throw New NotImplementedException
    End Function

End Class
