Imports System.Data
Imports C1.Win.C1Chart
Imports C1.Web.C1WebChart
Imports System.Drawing
Imports AjaxControlToolkit

Partial Class Graph_DepthDistributionChart
    Inherits System.Web.UI.Page

#Region "グラフ描画における個々のグラフ情報構造体"
    Private Structure GrpPersonalizedInfo
        ''' <summary>変位データ       データ系列,深度数</summary>
        Dim DrawDispData(,) As Single
        ''' <summary>チャンネル、深度、[変位データ]　深度データ数分</summary>
        Dim DrawData() As ClsReadDataFile.DataInfo
        ''''' <summary>データの測定時刻</summary>
        ''Dim DataTime() As Date
        ''' <summary>方向性表示文字列</summary>
        Dim DirectionalTitle As String
        ''' <summary>個々のグラフタイトル</summary>
        Dim DataTitle As String
        ''' <summary>スケール既定値か入力か</summary>
        Dim DefaultSet As Boolean
        ''' <summary>既定値の場合のリストインデックス</summary>
        Dim DefaultValue As Integer
        ''' <summary>X軸の最大値</summary>
        Dim Max As Single
        ''' <summary>X軸の最小値</summary>
        Dim Min As Single
        ''' <summary>主メモリ間隔</summary>
        Dim UnitMajor As Single
        ''' <summary>副メモリ間隔</summary>
        Dim UnitMinor As Single
        ''' <summary>最深部から累積         '使用は未定</summary>
        Dim CalcFromDeepest As Boolean
        ''' <summary>線種</summary>
        Dim LineType As Short
        ''' <summary>線幅</summary>
        Dim LineWidth As Short
        ''' <summary>マーカの有無</summary>
        Dim Marker As Boolean
        ''' <summary>データラベル表示の有無</summary>
        Dim DataLabel As Boolean
        ''' <summary>不動点深度</summary>
        Dim FixPointDepth As Single
        ''' <summary>深度データ数</summary>
        Dim DataCount As Short
        ''' <summary>深度データの文字列(後で分解するため)</summary>
        Dim DepthDatas As String
        ''' <summary>描画データチャンネルの文字列(後で分解するため)</summary>
        Dim DispDatas As String
        ''' <summary>データの単位(ツールチップ用)</summary>
        Dim DataUnit As String
        ''' <summary>データファイルNo</summary>
        Dim DataFileNo As Integer
        ''' <summary>警報値表示</summary>
        Dim AlertArea As Boolean
        ''' <summary>1日付あたりのデータ系列数</summary>
        Dim AddDataNum As Short
        ''' <summary>1日付あたりのデータ系列数が２つだった場合の系列説明</summary>
        Dim LineExp As String
        ''' <summary>横軸の反転</summary>
        Dim Reversed As Boolean
    End Structure
#End Region

#Region "グラフ描画全般の情報変数"
    Private Structure GrpInfo
        ''' <summary>グラフ描画における個々のグラフ情報</summary>
        Dim DrawInfo() As GrpPersonalizedInfo
        ''' <summary>各文字サイズや色の設定</summary>
        Dim DrawSizeColorInf() As Integer
        ''' <summary>データの測定時刻</summary>
        Dim DataTime() As Date
        '--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--
        ''' <summary>用紙の向き</summary>
        Dim PaperOrientaion As Integer
        ''' <summary>日付フォーマット</summary>
        Dim DateFormat As String
        ''' <summary>グラフタイトルの位置(上or下)</summary>
        Dim TitlePosition As Short
        ''' <summary>グラフタイトル</summary>
        Dim GraphTitle As String
        ''' <summary>タイトルバーのタイトル</summary>
        Dim WindowTitle As String
        ''' <summary>グラフの枠数</summary>
        Dim GraphBoxCount As Integer
        ''' <summary>描画グラフ数</summary>
        Dim GraphCount As Integer
        ''' <summary>１つのグラフに描画するデータの数(最新：１個　指定日時：指定日付分)</summary>
        Dim DataCount As Short
        ''' <summary>凡例の表示／非表示</summary>
        Dim EnableLegend As Boolean
        ''' <summary>施工情報の表示／非表示</summary>
        Dim ConstructionStatus As Boolean
        ''' <summary>土質情報の表示／非表示</summary>
        Dim ConstructionStatusID As Integer
        ''' <summary>用紙の向き</summary>
        Dim SoilStatus As Boolean
        ''' <summary>土質情報のID</summary>
        Dim SoilStatusID As Integer
        ''' <summary>天端位置深度 Y軸の最大値</summary>
        Dim TopPosition As Single
        ''' <summary>描画深度 Y軸の最小値</summary>
        Dim GraphYScale As Single
        ''' <summary>ポイント深度の表示／非表示</summary>
        Dim PointYScaleShow As Boolean
        ''' <summary>ポイント深度 (縦軸スケール表示グラフ Y軸の最小値)</summary>
        Dim PointYScale As Single
        ''' <summary>ポイント名称</summary>
        Dim PointName As String
        ''' <summary>警報表示の有無</summary>
        Dim AlertZoneShow As Boolean
        ''''' <summary>データファイルNo</summary>
        ''Dim DataFileNo As Integer
        ''' <summary>データの最終更新日</summary>
        Dim LastUpdate As Date
        ''' <summary>深度主メモリスケール間隔</summary>
        Dim GridMajorSpaceY As Single
        ''' <summary>深度副メモリスケール間隔</summary>
        Dim GridMinorSpaceY As Single
        ''' <summary>欠測データの連結</summary>
        Dim MissingDataContinuous As Boolean
        ''' <summary>凡例へのGL表示</summary>
        Dim ShowGL2Legend As Boolean
        ''' <summary>グラフ表示ページのaspxファイル名</summary>
        Dim URL As String
        ''' <summary>グラフ表示ページにおける各文字列の共通フォントサイズ</summary>
        Dim TitleFontSize() As Single
    End Structure
#End Region

    Private DataFileNames() As ClsReadDataFile.DataFileInf
    Private GrpProp As GrpInfo                  'グラフ描画に関する情報
    ''Private Grp
    Private TmrState As Boolean                             'タイマーの起動状態
    Private TxtDates(6) As TextBox
    Private imgDates(6) As System.Web.UI.WebControls.Image
    Private imgGetTime(6) As ImageButton
    Private DDLTimes(6) As DropDownList
    Private GrpBox(3) As WebControls.Panel                  ''グラフ枠
    Private Grps(3) As C1.Web.C1WebChart.C1WebChart         ''グラフコントロール
    Private GrpTitleLabel(3) As WebControls.Label           ''グラフの個々のタイトル
    Private LabelNo(3) As WebControls.Label                 ''グラフの横軸設定の番号ラベル
    Private SetPanel(3) As Object       ' WebControls.Panel                ''横軸スケールの設定パネル
    Private Defval(3) As DropDownList                       ''デフォルトスケール値
    Private scaleDef(3) As RadioButtonList                  ''デフォルト設定
    Private scaleMin(3) As TextBox                          ''横軸スケール最小さい値
    Private scaleMax(3) As TextBox                          ''横軸スケール最大値

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

        Dim iloop As Integer

        ''オブジェクト変数のクリア
        For iloop = 0 To GrpProp.GraphCount - 1
            TxtDates(iloop).Dispose()
            imgDates(iloop).Dispose()
        Next

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        ''Dim LoginStatus As Integer = 1      '◆◆◆◆◆ デバッグ用
        Dim strScript As String = ("<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>")

        Dim LoginStatus As Integer = Session.Item("LgSt")                                      'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            Response.Redirect("sessionerror.aspx", False)
            ''Server.Transfer("sessionerror.aspx")
            Exit Sub
        End If

        If User.Identity.IsAuthenticated = False Then
            Response.Write(strScript)
            ''Server.Transfer("sessionerror.aspx")
            ''Response.Redirect("sessionerror.aspx")
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする
        ''Response.Cache.SetExpires(DateTime.Now.AddSeconds(60))        ''キャッシュの時間制限の場合
        ''Response.Cache.SetCacheability(HttpCacheability.Public)
        ''Response.Cache.SetValidUntilExpires(True)

        '' これを使うとデータ点数が多いときにものすごく重くなるので使用する場合は要検討
        ''ClientScript.RegisterClientScriptBlock(GetType(String), "Simple2D.js", "<script language=""javascript"" type=""text/javascript"" src=""" + Me.TemplateSourceDirectory + "/Simple2D.js" + """></script>")

        Dim siteName As String
        Dim siteDirectory As String
        Dim grpDataFile As String
        Dim intRet As Integer
        Dim OldestDate As Date
        ''Dim dteTemp As Date = Date.Parse("1900/01/01 0:00:00")
        Dim clsNewDate As New ClsReadDataFile
        Dim clsSetScript As New ClsGraphCommon
        Dim OldTerm As String

        OldTerm = CType(Session.Item("OldTerm"), String)            '過去データ表示制限
        siteName = CType(Session.Item("SN"), String)                            '現場名
        siteDirectory = CType(Session.Item("SD"), String)                       '現場ディレクトリ
        ''siteDirectory = "washio"               '◆◆◆◆◆ デバッグ用

        grpDataFile = Server.MapPath(siteDirectory + "\App_Data\DepthDistChartInfo.mdb")
        ''grpDataFile = "I:\WebDataSystem2\totsuka\App_Data\DepthDistChartInfo.mdb"               '◆◆◆◆◆ デバッグ用
        OldestDate = CType(Session.Item("OldestDate"), Date)
        GrpProp.WindowTitle = CType(Request.Item("GNa"), String)                                                        'グラフタイトル（ウィンドウタイトル）
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)
        Dim RemoteInf As String

        'セッション変数に保存しないようにする 2018/03/30 Kino
        ''Dim RemoteInf As String = CType(Session("RemoteInfo"), String)
        Dim clsBr As New ClsHttpCapData
        RemoteInf = clsBr.GetRemoteInfo(Request, Server.MapPath(""))
        clsBr = Nothing
        Dim broinf() As String
        If RemoteInf IsNot Nothing Then                                                             ' 2012/08/24 Kino Add
            broinf = RemoteInf.Split(",")
            If broinf(2) = "IE 8.0 ie" Then Me.C1WebCalendar.Height = 217 Else Me.C1WebCalendar.Height = 246
        Else
            Me.C1WebCalendar.Height = 217
        End If

        ''wid = "1200"      '◆◆◆◆◆ デバッグ用
        ''Hei = "1000"      '◆◆◆◆◆ デバッグ用
        ''OldTerm = "02Y"   '◆◆◆◆◆ デバッグ用

        intRet = clsNewDate.GetDataFileNames(Server.MapPath(siteDirectory + "\App_Data\MenuInfo.mdb"), DataFileNames)   'データファイル名、共通情報ファイル名、識別名を取得
        intRet = clsNewDate.GetDataLastUpdate(Server.MapPath(siteDirectory + "\App_Data\"), DataFileNames)                  '2010/05/24 Kino Add データの最古、最新日付を取得
        ''GrpProp.WindowTitle = "Ａ測点"          '◆◆◆◆◆ デバッグ用
        Dim OldFlg As Boolean = IO.File.Exists(Server.MapPath(siteDirectory + "App_Data\Old.flg"))                          ' 2011/04/19 Kino Add 前の設定と調整するため

        If IsPostBack = False Then

            ' 2017/03/14 Kino Changed 不要のためコメント
            ''If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
            ''    '==== フォームのサイズを調整する ====
            ''    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
            ''        Dim OpenString As String

            ''        OpenString = "<SCRIPT LANGUAGE='javascript'>"
            ''        OpenString += ("window.resizeTo(" + wid + "," + Hei + ");")
            ''        OpenString += "<" + "/SCRIPT>"

            ''        ''Dim instance As ClientScriptManager = Page.ClientScript                            'この方法から↓の方法へ変更した
            ''        ''instance.RegisterClientScriptBlock(Me.GetType(), "経時変化図", OpenString)
            ''        Page.ClientScript.RegisterStartupScript(Me.GetType(), "深度分布図", OpenString)
            ''    End If
            ''End If

            ''設定の初回読み込みはデータベースから行なう
            Using DbCon As New OleDb.OleDbConnection
                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + grpDataFile + ";" + "Jet OLEDB:Engine Type= 5")
                ''DbCon.Open()

                intRet = ReadGraphInfoFromDB(DbCon, "深度分布図")                   'グラフの全体情報を取得(「メニュー基本情報」テーブル)
                intRet = GraphLineInfoFromDB(DbCon)                                 '個々のグラフ描画の情報(タイトル、最大・最小スケールなど)を取得(グラフ名称テーブル)　
                Call SetObj2Arg()                                                       '出力日付設定に関するコントロールの、オブジェクト配列変数への格納

                Call SetBIND2Controls(DbCon)                                        '各コントロールのリスト内容をDBにバインドする
                Call SetInfo2FormControl()

                ''DbCon.Close()
                'DbCon.Dispose()
            End Using
            Call GetMostUpdate()                                                'データファイルの最新日時を取得する

            ''GrpProp.LastUpdate = New DateTime(2009, 5, 11, 18, 0, 0)                       '◆◆◆◆◆ デバッグ用
            Me.RdBFromNewest.Checked = True

            Call clsSetScript.CheckAutoUpdate(Me.Form, Server.MapPath(siteDirectory + "\App_Data\MenuInfo.mdb"), "深度分布図")      '自動更新設定の読み込み
            TmrState = Me.Tmr_Update.Enabled

            Call SetViewState()                                                 'コントロールに配置しないDBから読み込んだデータをヴューステートに保存する

            Call clsSetScript.SetSelectDateScrpt_Depth(Me.Form)                 'コントロールにJavaScriptを割り当て

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

            Dim OldDate As Date
            If OldTerm <> "None" Then
                Dim clsCalc As New ClsReadDataFile
                OldDate = clsCalc.CalcOldDateLimit(OldTerm)
                clsCalc = Nothing
            Else
                OldDate = DataFileNames(0).MostOldestDate
            End If
            Dim NewDate As Date = Date.Parse(GrpProp.LastUpdate.AddDays(1).ToString("yyyy/MM/dd"))

            For iloop = 0 To 6
                Dim rangeValid As New RangeValidator
                rangeValid = CType(FindControl("RngValid" + (iloop).ToString), RangeValidator)
                rangeValid.MinimumValue = OldDate
                rangeValid.MaximumValue = NewDate
                rangeValid.ErrorMessage = ("日付は" + OldDate + " ～ " + NewDate + " までです")
                rangeValid.Dispose()
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
            Call SetObj2Arg()                                                       '出力日付設定に関するコントロールの、オブジェクト配列変数への格納
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

        If ToolkitScriptManager1.AsyncPostBackSourceElementID = "ImbtnRedrawGraph" Or ToolkitScriptManager1.AsyncPostBackSourceElementID.Length = 0 Then                 'ポストバックがどのコントロールから発生したのかで判断(カレンダーのイベントではここに入らない!!)
            ''チャンネル設定からDBのデータを読み込む
            intRet = GraphDataInfoFromDB(siteDirectory)                         '個々のグラフに表示するデータと警報情報などを取得(最新データ)

            If intRet = 0 Then
                '    Throw New Exception("出力日付が指定されていません。")
                Call SetControlVisible(OldFlg, intRet)                                                'グラフのサイズ調整
                Call ASPNET_MsgBox("出力日付が指定されていません。")
                Exit Sub
                ''If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
                ''    Dim OpenString As String = Nothing

                ''    OpenString = "<script language=""javascript"">"
                ''    OpenString += "alert(""出力日時が指定されていません。"");"
                ''    OpenString += "</script>"

                ''    'ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "onload", OpenString, True)
                ''    Page.ClientScript.RegisterStartupScript(Me.GetType(), "深度分布図", OpenString, True)
                ''    Me.GraphOuter.InnerText = "「指定日時」における、出力日時が指定されていないため、グラフを描画できません。"
                ''    Exit Sub
                ''End If
            End If
            Me.MyTitle.Text = GrpProp.GraphTitle + " [深度分布図] - " + siteName                    'ブラウザのタイトルバーに表示するタイトル

            If Me.RdBFromNewest.Checked = True Then
                ''If GrpProp.DrawInfo(0).DataTime(0) < GrpProp.LastUpdate Then
                If GrpProp.DataTime(0) < GrpProp.LastUpdate Then
                    ''Me.LblLastUpdate1.Text = "最新データ日時：" + GrpProp.DrawInfo(0).DataTime.ToString("yyyy/MM/dd HH:mm")
                    Me.LblLastUpdate.Text = "最新データ日時：" + GrpProp.DataTime(0).ToString("yyyy/MM/dd HH:mm")
                Else
                    Me.LblLastUpdate.Text = "最新データ日時：" + GrpProp.LastUpdate.ToString("yyyy/MM/dd HH:mm")
                End If
            Else
                Me.LblLastUpdate.Text = "最新データ日時：" + GrpProp.LastUpdate.ToString("yyyy/MM/dd HH:mm")
            End If

            Call SetControlVisible(OldFlg, intRet)                                                'グラフのサイズ調整
            ''Call SetCalenderFunction()                                            'カレンダーコントロールの起動設定       使用しない
            ''グラフ描画
            Call SetGraphCommonInit()
        End If

        clsSetScript = Nothing
        clsNewDate = Nothing

    End Sub

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

    Protected Sub SetControlVisible(ByVal OldFlg As Boolean, ByVal ErrFlg As Integer)       ' 2018/03/20 Kino Add
        ''
        '' グラフ枠、設定項目の表示を設定
        ''
        Dim iloop As Integer
        Dim GrpHeight As Integer
        Dim GrpWidth As Integer
        Dim ChartHeight As Integer
        Dim ChartWidth As Integer
        Dim StatusInfoShow As Short = 0                     '' 土質情報、施工情報、ポイント深度表示を全てする場合は3
        Dim BaseWidth As Integer                            '' 用紙枠の横幅
        Dim BaseHegiht As Integer                           '' 用紙枠の高さ
        Dim CalcWidth As Integer                            '' 各グラフの幅
        Dim SoilChartTop As Integer                         '' 柱状図・スケールグラフのチャートエリア上部位置
        Dim soilChartHeight As Integer                      ''柱状図・スケールグラフのチャートエリアの高さ 
        Dim soilChartWidth As Integer                       ''柱状図・スケールグラフのチャートエリアの幅
        Dim soilHeight As Integer                           ''柱状図・スケールグラフの高さ
        Dim soilWidth As Integer                            ''柱状図・スケールグラフの幅
        Dim soilHeadLeft As Integer
        Dim soilHeadTop As Integer
        Dim txtHead As String = ""                          ''柱状図・スケールグラフのヘッダ文字列
        Dim legendOffset As Integer                         ''2009/09/01 凡例の位置調整分オフセット
        Dim sizeAdjust As Integer                           '' グラフ数と紙の向きの文字列結合
        Dim tableRow(5) As HtmlControls.HtmlTableRow

        Select Case GrpProp.PaperOrientaion
            Case 0   'A4縦の場合
                BaseWidth = 646 '655     '660
                BaseHegiht = 900
            Case 1   'A4横の場合
                BaseWidth = 978
                BaseHegiht = 590        '598    '600    '582
            Case 2      'A3縦の場合
            Case 3      'A3横の場合
        End Select

        For iloop = 0 To 3
            tableRow(iloop) = CType(FindControl("r" + (iloop + 1).ToString), HtmlControls.HtmlTableRow)
        Next iloop

        ''Privateとした
        ''Grps(0) = Me.WC1 : GrpBox(0) = Me.PnlGraph1 : GrpTitleLabel(0) = Me.LblGraphTitle1
        ''Grps(1) = Me.WC2 : GrpBox(1) = Me.PnlGraph2 : GrpTitleLabel(1) = Me.LblGraphTitle2
        ''Grps(2) = Me.WC3 : GrpBox(2) = Me.PnlGraph3 : GrpTitleLabel(2) = Me.LblGraphTitle3
        ''Grps(3) = Me.WC4 : GrpBox(3) = Me.PnlGraph4 : GrpTitleLabel(3) = Me.LblGraphTitle4

        ' ''Grps(0) = Me.WC0 : GrpBox(0) = Me.PnlGraph0 : GrpTitleLabel(0) = Me.LblGraphTitle0          '土質情報
        ' ''GrpBox(5) = Me.PnlGraph5 : GrpTitleLabel(5) = Me.LblGraphTitle5                             '施工情報

        ''SetPanel(0) = Me.Pnl1 : LabelNo(0) = Me.LblNo1          'ｸﾞﾗﾌｽｹｰﾙ設定のNo表示と、設定枠を指定
        ''SetPanel(1) = Me.Pnl2 : LabelNo(1) = Me.LblNo2
        ''SetPanel(2) = Me.Pnl3 : LabelNo(2) = Me.LblNo3
        ''SetPanel(3) = Me.Pnl4 : LabelNo(3) = Me.LblNo4

        sizeAdjust = Integer.Parse(GrpProp.GraphBoxCount.ToString + GrpProp.PaperOrientaion.ToString)

        'グラフと枠の表示設定
        For iloop = 0 To 3
            If iloop <= GrpProp.GraphCount - 1 Then
                GrpBox(iloop).Visible = True
                Grps(iloop).Visible = True
                SetPanel(iloop).Visible = True
                LabelNo(iloop).Visible = True
                GrpTitleLabel(iloop).Visible = True
                GrpTitleLabel(iloop).Text = GrpProp.DrawInfo(iloop).DataTitle
            Else
                GrpBox(iloop).Visible = False
                'Controls.Remove(GrpBox(iloop))
                Grps(iloop).Visible = False
                Grps(iloop).Dispose()
                GrpTitleLabel(iloop).Visible = False
                SetPanel(iloop).Visible = False
                LabelNo(iloop).Visible = False

                GrpBox(iloop).Dispose()
                tableRow(iloop).Visible = False                                 ' 2016/01/29 Kino Add これで行が削除できる
            End If
        Next

        If GrpProp.SoilStatus = False Then                                                          '土質情報の有無
            ''Me.WC0.Visible = False : Me.WC0.Dispose()
            ''Me.PnlGraph0.Visible = False : Me.PnlGraph0.Dispose()
            StatusInfoShow -= 2
        End If

        If GrpProp.PointYScaleShow = False Then                                                     'TP表示の有無
            StatusInfoShow -= 1
        End If

        soilHeight = BaseHegiht

        Select Case StatusInfoShow                                  'ｽｹｰﾙ表示の幅を調整
            Case -1
                CalcWidth = BaseWidth - 124                         '■GL・土質
                Me.WC0.ChartArea.AxisY.Visible = False
                soilChartWidth = 99
                soilChartHeight = 8                                 'オフセット値
                soilWidth = 100
                SoilChartTop = 7
                txtHead = "GLm   土質"
                soilHeadLeft = 19
                soilHeadTop = 10
            Case -2
                CalcWidth = BaseWidth - 82                          '■TP・GL
                soilChartWidth = 85  '100            
                If GrpProp.GraphBoxCount = 1 Then                   '2009/09/15 Kino Add
                    soilChartHeight = 8
                    SoilChartTop = 7

                Else
                    soilChartHeight = 20        '8
                    SoilChartTop = 19
                End If
                Select Case sizeAdjust                                  ' グラフ個数 + 用紙向き(0:縦/1:横)
                    Case 10, 11, 21
                        soilChartHeight = 8
                        SoilChartTop = 7
                    Case 20, 31, 41
                        soilChartHeight = 8 '20                         ' 2012/08/02 Kino Changed 
                        SoilChartTop = 7    '19
                    Case 30, 40
                        soilChartHeight = 32
                        SoilChartTop = 31
                End Select

                soilWidth = 85 '68

                If GrpProp.PointName.IndexOf(Convert.ToChar(",")) > 0 Then                  ' 2017/05/25 Kino Add
                    Dim tmp() As String = GrpProp.PointName.Split(Convert.ToChar(","))
                    txtHead = (tmp(0) & "　    GLm")
                Else
                    txtHead = (GrpProp.PointName.ToString & "　    GLm")        ' 2016/06/23 Kino Changed  スペース2個を削除
                End If

                soilHeadLeft = 2
                soilHeadTop = 10
            Case -3
                CalcWidth = BaseWidth - 63 '61                          '■GL
                Me.WC0.ChartArea.AxisY.Visible = False
                soilChartWidth = 55
                Select Case sizeAdjust                                  ' グラフ個数 + 用紙向き(0:縦/1:横)
                    Case 10, 11, 21
                        soilChartHeight = 12
                        SoilChartTop = 9
                    Case 20, 31, 41
                        soilChartHeight = 12 '   24
                        SoilChartTop = 9    '21
                    Case 30, 40
                        soilChartHeight = 36
                        SoilChartTop = 33
                End Select

                ''If GrpProp.GraphBoxCount = 1 Then                   '2009/09/15 Kino Add
                ''    soilChartHeight = 12
                ''    SoilChartTop = 9
                ''Else
                ''    soilChartHeight = 12       '8
                ''    SoilChartTop = 9
                ''End If
                ''Select Case GrpProp.PaperOrientaion
                ''    Case 0      'A4縦の場合
                ''        soilChartHeight += 12
                ''        SoilChartTop += 12
                ''    Case 2      'A3縦の場合
                ''    Case 3      'A3横の場合
                ''End Select
                soilWidth = 41 '47
                If GrpProp.ShowGL2Legend = True Then                ' 2013/11/8 Kino Add
                    'txtHead = "   GLm"
                    txtHead = "GLm"
                ElseIf GrpProp.PointName.IndexOf(Convert.ToChar(",")) > 0 Then                  ' 2017/05/25 Kino Add
                    Dim tmp() As String = GrpProp.PointName.Split(Convert.ToChar(","))
                    txtHead = tmp(1)
                End If
                soilHeadLeft = 6
                soilHeadTop = 10
            Case Else
                soilWidth = 130
                CalcWidth = BaseWidth - soilWidth                   '■TP・GL・土質
                'soilChartHeight = ChartHeight - 4
                soilChartHeight = 4
                soilChartWidth = 120S
                SoilChartTop = 5
                If GrpProp.PointName.IndexOf(Convert.ToChar(",")) > 0 Then                  ' 2017/05/25 Kino Add
                    Dim tmp() As String = GrpProp.PointName.Split(Convert.ToChar(","))
                    txtHead = (tmp(0) & "     GLm   土質")
                Else
                    txtHead = (GrpProp.PointName.ToString & "     GLm   土質")
                End If
                soilHeadLeft = 0
                soilHeadTop = 10
        End Select

        txtHead = GrpProp.PointName.ToString                                                    ' 2016/06/22 Kino Add 表示する文字列を全部DBに書き込むように変更

        If GrpProp.ConstructionStatus = False Then                                              '施工情報
            ImgConstructState.Visible = False : ImgConstructState.Dispose()
            LblGraphTitle5.Visible = False ': LblGraphTitle5.Dispose()
            Controls.Remove(Me.LblGraphTitle5)
            Me.PnlGraph5.Visible = False : Me.PnlGraph5.Dispose()
            Controls.Remove(Me.PnlGraph5)
        Else
            BaseWidth -= 50                                                                     '施工情報を表示する場合は50ドットを引く
        End If

        '======================================================== [ グラフサイズ・表示設定 ] ================================================↓
        'グラフサイズの調整
        GrpWidth = Fix(CalcWidth / GrpProp.GraphBoxCount)
        GrpHeight = BaseHegiht
        ChartWidth = GrpWidth

        For iloop = 0 To GrpProp.GraphBoxCount - 1
            With Grps(iloop)
                Select Case GrpProp.PaperOrientaion
                    Case 0      'A4縦の場合
                        If GrpProp.EnableLegend = True Then ChartHeight = 790 Else ChartHeight = 780 '856
                        legendOffset = 2
                        Me.GraphOuter.Attributes.Add("class", "GraphOuterPortrait")                     ' 2016/01/19 Kino Add ページ高さ設定
                    Case 1      'A4横の場合
                        'If GrpProp.EnableLegend = True Then ChartHeight = 470 Else ChartHeight = 580
                        If GrpProp.EnableLegend = True Then ChartHeight = 495 Else ChartHeight = 580
                        legendOffset = 2
                        Me.GraphOuter.Attributes.Add("class", "GraphOuterLandscape")                    ' 2016/01/19 Kino Add ページ高さ設定
                    Case 2      'A3縦の場合
                    Case 3      'A3横の場合
                End Select

                GrpBox(iloop).Height = GrpHeight                            'パネルの高さ
                GrpBox(iloop).Width = GrpWidth                              'パネルの幅
                .Height = GrpHeight                                         'WebChartの高さ
                .Width = GrpWidth                                           'WebChartの幅
                .ChartArea.Location = New System.Drawing.Point(0, 0)
                .ChartArea.Size = New System.Drawing.Size(ChartWidth, ChartHeight)          'チャートエリアの幅と高さ
                .ChartArea.Margins.SetMargins(5, 8, 5, 5)
                .Legend.Location = New System.Drawing.Point(-1, ChartHeight - legendOffset)            '2009/08/18 Kino Changed  5 -> 6
                If ErrFlg = 0 Then                                                                      ' 2018/03/20 Kino Add
                    .Visible = False
                    .ChartGroups.Group0.ChartData.SeriesList.Clear()
                    GrpTitleLabel(0).Text = "出力日付が指定されていません。"
                End If
                '.Legend.Size = New System.Drawing.Size(ChartWidth, 60)
                '================================================ [ グラフサイズ・表示設定 ] ================================================↑
            End With
        Next iloop

        soilChartHeight = ChartHeight - soilChartHeight
        Dim leftMargin As Integer = 0
        With WC0
            If StatusInfoShow = -2 Then
                .ChartGroups(1).ChartData.SeriesList.Clear()
                Dim series As ChartDataSeries = .ChartGroups.Group1.ChartData.SeriesList.AddNewSeries()     'データ系列のインスタンスを作成
                With series
                    .PointData.Length = 1
                    .X.Add(30)                                                                      '2009/09/01 ダミー
                    .Y.Add(30)
                    .X.Add(35)
                    .Y.Add(35)
                End With
            End If
            .ChartArea.AxisX.Origin = 0
            .ChartArea.AxisX.Reversed = False
            .ChartArea.PlotArea.Boxed = False
            If GrpProp.PointYScaleShow = True Then
                '.ChartArea.AxisX.SetMinMax(-0.5, 4)
                .ChartArea.AxisX.SetMinMax(-0.05, 0.01)
                leftMargin = 14
            Else
                .ChartArea.AxisX.SetMinMax(-0.5, 7)
                leftMargin = 14
            End If

            .ChartArea.Location = New System.Drawing.Point(0, SoilChartTop)                          '土質・スケール用グラフ
            .ChartArea.Margins.Left = leftMargin
            .ChartArea.Margins.Right = 0
            Me.PnlGraph0.Width = soilWidth
            .Height = soilHeight
            .Width = soilWidth
            .ChartArea.Size = New System.Drawing.Size(soilChartWidth, soilChartHeight)
            .Header.Location = New System.Drawing.Point(soilHeadLeft, soilHeadTop)
            .Header.Text = txtHead
            If ErrFlg = 0 Then                                                                      ' 2018/03/20 Kino Add
                .Visible = False
            End If
        End With

        'グラフタイトル 表示位置、フォントサイズ設定
        Dim objLabel As System.Web.UI.WebControls.Label
        If GrpProp.TitlePosition = 0 Then
            objLabel = Me.LblTitleUpper
            Me.LblTitleUpper.Visible = True
            Me.LblTitleLower.Visible = False
            ''Me.LblSpaceLower.Visible = False
            ''Me.LblSpaceUpper.Visible = True
            Me.LitUpper.Text = "<img src='./img/space_S.gif'><br />"        '"<br /><br />"
            Me.LitLower.Text = vbNullString
            Me.LitLower.Visible = False
            Controls.Remove(Me.LblTitleLower)
            Controls.Remove(Me.LitLower)
            ''Controls.Remove(Me.LblSpaceLower)
        Else
            Me.LitUpper.Text = vbNullString
            Me.LitUpper.Visible = False
            Me.LitLower.Text = "<img src='./img/space_S.gif'><br />"        '"<br />"
            objLabel = Me.LblTitleLower
            Me.LblTitleUpper.Visible = False
            Me.LblTitleLower.Visible = True
            ''Me.LblSpaceLower.Visible = True
            ''Me.LblSpaceUpper.Visible = False
            Controls.Remove(Me.LblTitleUpper)
            Controls.Remove(Me.LitUpper)
            ''Controls.Remove(Me.LblSpaceUpper)
        End If

        Dim AdjValue As Integer
        If OldFlg = True Then AdjValue = -2 ' 2011/04/19 Kino Add      現行の設定と合わせるため
        objLabel.Text = GrpProp.GraphTitle
        objLabel.Font.Size = GrpProp.DrawSizeColorInf(0) + AdjValue          ' 2011/04/19 Kino Add  　現行の設定と合わせるため
        objLabel.Width = BaseWidth - 20

    End Sub


    Protected Sub SetGraphCommonInit()
        ''
        '' グラフ描画
        ''
        Dim iloop As Integer
        Dim jloop As Integer
        Dim kloop As Integer
        Dim line2Loop As Integer                                                '1日付あたりのデータが２つ以上あった場合のループ
        Dim MarkerInfo(7) As Integer
        Dim LineColorInfo(7) As String
        Dim clsGraphInf As New ClsReadDataFile
        Dim intRet As Integer
        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ
        Dim DBPath As String
        Dim strShowGL As String = ""                                                '2009/09/15 Kino Add
        Dim UsePlotArea As Boolean = True                                           ' 2018/06/11 Kino Add ポップアップ

        'オブジェクトの指定: グラフコントロール：スケール設定リストボックス : スケール既定・入力    Privateとした
        ''Grps(0) = Me.WC1 : Defval(0) = Me.DdlScale1 : scaleDef(0) = Me.RBList1
        ''Grps(1) = Me.WC2 : Defval(1) = Me.DdlScale2 : scaleDef(1) = Me.RBList2
        ''Grps(2) = Me.WC3 : Defval(2) = Me.DdlScale3 : scaleDef(2) = Me.RBList3
        ''Grps(3) = Me.WC4 : Defval(3) = Me.DdlScale4 : scaleDef(3) = Me.RBList4
        ''scaleMin(0) = Me.TxtMin1 : scaleMax(0) = Me.TxtMax1
        ''scaleMin(1) = Me.TxtMin2 : scaleMax(1) = Me.TxtMax2
        ''scaleMin(2) = Me.TxtMin3 : scaleMax(2) = Me.TxtMax3
        ''scaleMin(3) = Me.TxtMin4 : scaleMax(3) = Me.TxtMax4

        ''siteDirectory = "washio"               '◆◆◆◆◆ デバッグ用
        DBPath = Server.MapPath(siteDirectory + "\App_Data\DepthDistChartInfo.mdb")
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
                If Me.DdlGraphColorType.SelectedValue = 1 Then                                                  'グラフの色をグレースケールにするかどうか
                    Grps(iloop).UseGrayscale = True
                End If

                '方向性ヘッダを軸タイトルではなくヘッダとした場合のソース
                ''.Header.Location = New System.Drawing.Point(-1, 5)
                ''.Header.Compass = CompassEnum.North
                ''.Header.Text = GrpProp.DrawInfo(iloop).DirectionalTitle + Environment.NewLine + Environment.NewLine + Environment.NewLine
                ''.Header.Style.HorizontalAlignment = AlignHorzEnum.Center
                ''.Header.Text = ""
                ''.Header.Visible = True

                '●●●●●　X1軸設定
                With .ChartArea.AxisX
                    .Text = (GrpProp.DrawInfo(iloop).DirectionalTitle + Environment.NewLine + Environment.NewLine)  'X軸タイトル
                    .Compass = CompassEnum.North
                    .Alignment = Drawing.StringAlignment.Center
                    .AnnoFormat = FormatEnum.NumericGeneral                     'ラベルの表示方法は　数値
                    .ForeColor = Drawing.Color.Black                            '線色
                    .TickMinor = TickMarksEnum.Outside                          '副メモリの突出方向
                    .TickMajor = TickMarksEnum.Outside                          '主メモリの突出方向
                    .GridMajor.Visible = True                                   '主メモリ表示
                    .GridMinor.Visible = True                                   '副メモリ表示
                    ''.GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                    ''.GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                    ''.AutoMajor = True                                           '主目盛の値を自動的に計算
                    ''.AutoMinor = True                                           '副目盛の値を自動的に計算
                    .Reversed = GrpProp.DrawInfo(iloop).Reversed                ' 2013/06/27 Kino Add 横軸反転

                    If GrpProp.DrawInfo(iloop).UnitMajor = 0 Then
                        .AutoMajor = True                                       '主目盛の値を自動的に計算
                        .GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                    Else
                        .UnitMajor = GrpProp.DrawInfo(iloop).UnitMajor
                        .GridMajor.Spacing = GrpProp.DrawInfo(iloop).UnitMajor
                    End If
                    If GrpProp.DrawInfo(iloop).UnitMinor = 0 Then
                        .AutoMinor = True                                           '副目盛の値を自動的に計算
                        .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                    Else
                        .UnitMinor = GrpProp.DrawInfo(iloop).UnitMinor
                        .GridMinor.Spacing = GrpProp.DrawInfo(iloop).UnitMinor
                    End If

                    If scaleDef(iloop).SelectedValue = 0 And Defval(iloop).SelectedValue = 9999 Then     'Ｘ軸ｽｹｰﾙの設定を自動にするか指示するか ■既定スケール・自動スケールの場合
                        .AutoMax = True
                        .AutoMin = True
                        .AutoMajor = True                                       '主目盛の値を自動的に計算
                        .GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                        .AutoMinor = True                                           '副目盛の値を自動的に計算
                        .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                    ElseIf scaleDef(iloop).SelectedValue = 1 Then                   ' 既定スケールでない(入力)の場合
                        .SetMinMax(Single.Parse(scaleMin(iloop).Text), Single.Parse(scaleMax(iloop).Text))  'X軸の最小、最大値を設定       ■入力　の場合
                        ''.AutoMajor = True                                       '主目盛の値を自動的に計算                         ' 2016/04/27 Kino Changed
                        .GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                        ''.AutoMinor = True                                           '副目盛の値を自動的に計算                     ' 2016/04/27 Kino Changed
                        .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                        'ElseIf Me.DdlScale1.SelectedValue <> 9999 Then
                    ElseIf scaleDef(iloop).SelectedValue = 0 And Defval(iloop).SelectedValue <> 9999 Then                                '■規定　自動ではない　場合
                        Dim scaleVal As Single = Defval(iloop).SelectedValue
                        .SetMinMax(-scaleVal, scaleVal)    'X軸の最小、最大値を設定
                        .AutoMajor = True                                       '主目盛の値を自動的に計算
                        .GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                        .AutoMinor = True                                           '副目盛の値を自動的に計算
                        .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                    End If
                    .Reversed = GrpProp.DrawInfo(iloop).Reversed                    ' 軸反転　2016/02/17 Kino Add
                End With

                '●●●●●　Y1軸設定
                With .ChartArea.AxisY
                    .AnnoFormat = FormatEnum.NumericManual                      'Y軸ラベルの表示方法はしない
                    .AnnoFormatString = " ; "
                    .ForeColor = Drawing.Color.Black                            '線色
                    .TickMinor = TickMarksEnum.Outside                          '副メモリの突出方向
                    .TickMajor = TickMarksEnum.Outside                          '主メモリの突出方向
                    .GridMajor.Visible = True                                   '主メモリ表示
                    .GridMinor.Visible = True                                   '副メモリ表示
                    '.AutoMajor = True                                           '主目盛の値を自動的に計算
                    '.AutoMinor = True                                           '副目盛の値を自動的に計算
                    '.GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                    '.GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算

                    If GrpProp.GridMajorSpaceY = 0 Then
                        .AutoMajor = True                                       '主目盛の値を自動的に計算
                        .GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                    Else
                        .UnitMajor = GrpProp.GridMajorSpaceY
                        .GridMajor.Spacing = GrpProp.GridMajorSpaceY
                    End If
                    If GrpProp.GridMinorSpaceY = 0 Then
                        .AutoMinor = True                                           '副目盛の値を自動的に計算
                        .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                    Else
                        .UnitMinor = GrpProp.GridMinorSpaceY
                        .GridMinor.Spacing = GrpProp.GridMinorSpaceY
                    End If
                    .SetMinMax(GrpProp.TopPosition, GrpProp.GraphYScale)        'Y軸の最小、最大値を設定

                End With

                ''●●●●●　描画データの設定
                .ChartGroups(0).ChartData.SeriesList.Clear()                                                    'データ系列を一括削除
                .ChartGroups.Group0.ChartData.Hole = 7.7E+30                                                    '描画しないデータ(異常値)
                .Legend.Visible = CType(GrpProp.EnableLegend, Boolean)                                          '凡例の有無
                Dim MaxDisp As Single
                Dim MaxDispDepth As Single = 7.7E+30                                                            ' 2012/01/18 Kino Changed   Add 7.7E+30
                Dim calcTemp As Single
                Dim line2OffsetCh As Integer = 0
                Dim DepthDataCount As Integer                                                                   ''実際の深度数(不動点がない場合 -1する)
                Dim ErrorDataCount As Integer

                ''If GrpProp.DrawInfo(iloop).FixPointDepth = -9999 Then                                         ' 2012/07/20 Kino 不動点の取り扱いを変更したのでコメント
                ''    DepthDataCount = GrpProp.DrawInfo(iloop).DataCount - 1
                ''Else
                ''    ' DepthDataCount = GrpProp.DrawInfo(iloop).DataCount                                          ''      不動点深度のデータあるので、-1しない
                ''    DepthDataCount = GrpProp.DrawInfo(iloop).DataCount - 1
                ''End If
                DepthDataCount = GrpProp.DrawInfo(iloop).DataCount - 1                                          ' 2012/07/20 Kino Add 不動点があってもなくても同じように扱えるようにした

                For jloop = 0 To GrpProp.DataCount - 1                                                          ''■■データ系列数分ループ
                    For line2Loop = 0 To GrpProp.DrawInfo(iloop).AddDataNum - 1                                 ''1日付あたりのデータ数分ループ
                        ' line2OffsetCh = line2Loop * (DepthDataCount + 1)
                        line2OffsetCh = line2Loop * (DepthDataCount + 1)
                        Dim series As ChartDataSeries = .ChartGroups.Group0.ChartData.SeriesList.AddNewSeries()     'データ系列のインスタンスを作成
                        With series
                            .PointData.Length = GrpProp.DataCount
                            If GrpProp.DrawInfo(iloop).Marker = True Then
                                .SymbolStyle.Shape = MarkerInfo(jloop)                                              'マーカの形状設定
                                .SymbolStyle.Color = Color.FromName("White")                                            'マーカの色
                                .SymbolStyle.OutlineColor = Color.FromName(LineColorInfo(jloop))                        'マーカの枠線       '2009/07/06 Kino Change iloop -> jloop
                                .SymbolStyle.OutlineWidth = GrpProp.DrawInfo(iloop).LineWidth                           'マーカの枠線の太さ
                            Else
                                .SymbolStyle.Shape = SymbolShapeEnum.None                                               'マーカなしに設定
                            End If

                            If line2Loop = 0 Then
                                .LineStyle.Pattern = (GrpProp.DrawInfo(iloop).LineType \ 10)                        '線種   iloop -> jloop 2012/07/23 2系列での線種設定を行うため2桁で入力対応 1系列目は 左の文字(10の位)
                            Else
                                .LineStyle.Pattern = (GrpProp.DrawInfo(iloop).LineType Mod 10)                      '線種   iloop -> jloop 2012/07/23 2系列での線種設定を行うため2桁で入力対応 2系列目は 右の文字(1の位)
                                ''.LineStyle.Pattern = LinePatternEnum.Dash                                           ' 1日付の2データ目は強制的に破線にする
                            End If
                            .LineStyle.Thickness = GrpProp.DrawInfo(iloop).LineWidth                                '線の太さ   iloop -> jloop
                            .LineStyle.Color = Color.FromName(LineColorInfo(jloop))                                 '線色                   '2009/07/06 Kino Change iloop -> jloop

                            ''For kloop = 0 To GrpProp.DrawInfo(iloop).DataCount                                  'データ数(深度)のループ    
                            ErrorDataCount = 0
                            For kloop = 0 To DepthDataCount                                                     'データ数(深度)のループ

                                .X.Add(Double.Parse(GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop + line2OffsetCh)))              'DataLoop -> jloop      1日付2データ対応
                                .Y.Add(GrpProp.DrawInfo(iloop).DrawData(kloop + line2OffsetCh).DepthData)
                                If GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop + line2OffsetCh) > 1.1E+30 Then
                                    ErrorDataCount += 1
                                End If
                                calcTemp = Math.Abs(GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop + line2OffsetCh)) '一時的に格納  DataLoop -> jloop    1日付2データ対応
                                If Math.Abs(MaxDisp) <= calcTemp And calcTemp < 1.1E+30 And kloop <> DepthDataCount Then                              '最大変位とその時の深度を取得     1日付2データ対応
                                    MaxDisp = GrpProp.DrawInfo(iloop).DrawDispData(jloop, kloop + line2OffsetCh)        'DataLoop -> jloop
                                    MaxDispDepth = GrpProp.DrawInfo(iloop).DrawData(kloop + line2OffsetCh).DepthData    '                       1日付2データ対応
                                End If
                            Next kloop

                            If GrpProp.DrawInfo(iloop).FixPointDepth <> -9999 Then                  ' =======【 不動点がある場合 】=======

                                If GrpProp.DrawInfo(iloop).CalcFromDeepest = True Then              '   不動点が最深部の場合
                                    .X.Add(CType(0, Double))
                                    .Y.Add(CType(GrpProp.DrawInfo(iloop).FixPointDepth, Double))

                                ElseIf GrpProp.DrawInfo(iloop).CalcFromDeepest = False Then         '   不動点が最上部の場合

                                    .X.Insert(0, CType(0, Double))
                                    .Y.Insert(0, CType(GrpProp.DrawInfo(iloop).FixPointDepth, Double))
                                End If
                            End If

                            If ErrorDataCount = DepthDataCount Then
                                .X.Item(DepthDataCount + 1) = 7.7E+30
                            End If

                            If GrpProp.DrawInfo(iloop).AddDataNum >= 2 Then
                                If Me.DdlEnableLegend.SelectedValue = 1 Then
                                    If line2Loop = 0 Then
                                        .Label = GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat)
                                    Else
                                        .LegendEntry = False                                                    '1日付2データの　2データ目は凡例を出さない
                                    End If
                                End If
                            Else
                                If Me.DdlEnableLegend.SelectedValue = 1 Then
                                    '凡例の文字(測定日時)　+　最大変形 + 深度"                  'GrpProp.DrawInfo(DataLoop).DataTime -> GrpProp.DataTime(DataLoop)

                                    If GrpProp.ShowGL2Legend = False Then                                       '2009/09/15 Kino Add
                                        strShowGL = ""
                                    Else
                                        strShowGL = "(GL" + MaxDispDepth.ToString("#0.0") + ")"
                                    End If

                                    If MaxDispDepth < 1.1E+30 Then                                              ' 2012/01/18 Kino Add
                                        If GrpProp.PaperOrientaion = 1 Then             'A4横の場合
                                            If GrpProp.GraphBoxCount <= 3 Then
                                                ''.Label = GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  [最大 " + MaxDisp.ToString("#0.0") + GrpProp.DrawInfo(iloop).DataUnit + "(GL" + MaxDispDepth.ToString("#0.0") + ")]"
                                                .Label = (GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  [最大 " + MaxDisp.ToString("#0.0") + GrpProp.DrawInfo(iloop).DataUnit + strShowGL + "]")
                                            Else                    'DataLoop -> jloop
                                                .Label = (GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  [最大 " + MaxDisp.ToString("#0.0") + "]")
                                                '.Label = GrpProp.DrawInfo(iloop).DataTime.ToString(GrpProp.DateFormat) + "[" + MaxDisp.ToString("#0.0") + "(" + MaxDispDepth.ToString("#0.0") + ")]"
                                            End If
                                        Else
                                            If GrpProp.GraphBoxCount <= 2 Then  'DataLoop -> jloop
                                                ''.Label = GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  [最大 " + MaxDisp.ToString("#0.0") + GrpProp.DrawInfo(iloop).DataUnit + "(GL" + MaxDispDepth.ToString("#0.0") + ")]"
                                                .Label = (GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  [最大 " + MaxDisp.ToString("#0.0") + GrpProp.DrawInfo(iloop).DataUnit + strShowGL + "]")
                                            ElseIf GrpProp.GraphBoxCount = 3 Then
                                                .Label = (GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  [最大 " + MaxDisp.ToString("#0.0") + "]")
                                            ElseIf GrpProp.GraphBoxCount = 4 Then
                                                .Label = GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat)
                                            End If
                                        End If
                                    Else
                                        .Label = GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  ---"     ' 2012/01/18 Kino Add                                                      ' 2012/01/18 Kino Add
                                    End If
                                End If
                            End If
                        End With

                        If Me.DdlContinous.SelectedIndex = 1 Then                                                       'データの連続線設定
                            .ChartGroups(0).ChartData(jloop).Display = SeriesDisplayEnum.ExcludeHoles
                        End If

                        If GrpProp.DrawInfo(iloop).AddDataNum >= 2 Then
                            Dim expTemp() As String
                            expTemp = GrpProp.DrawInfo(iloop).LineExp.Split(",")

                            If MaxDispDepth < 1.1E+30 Then                                                                  ' 2012/01/18 Kino Add
                                If GrpProp.ShowGL2Legend = False Then                                                       '2009/09/15 Kino Add
                                    strShowGL = ""
                                Else
                                    strShowGL = "　深度 :" + MaxDispDepth.ToString("#0.0") + "m"
                                End If

                                If line2Loop = 0 Then
                                    ''txtToolTip += "実線：" + expTemp(line2Loop) + "   最大値：" + MaxDisp.ToString("#0.0") + GrpProp.DrawInfo(iloop).DataUnit + "　深度 :" + MaxDispDepth.ToString("#0.0") + "m" + Environment.NewLine
                                    txtToolTip += ("実線：" + expTemp(line2Loop) + "   最大値：" + MaxDisp.ToString("#0.0") + GrpProp.DrawInfo(iloop).DataUnit + strShowGL + Environment.NewLine)
                                Else
                                    txtToolTip += ("破線：" + expTemp(line2Loop) + "   最大値：" + MaxDisp.ToString("#0.0") + GrpProp.DrawInfo(iloop).DataUnit + strShowGL + Environment.NewLine)
                                End If
                            Else

                                txtToolTip = GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  ---  "               ' 2012/01/18 Kino Add
                            End If
                        Else
                            If MaxDispDepth < 1.1E+30 Then
                                txtToolTip += (GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "　　最大値：" + MaxDisp.ToString("#0.0") + GrpProp.DrawInfo(iloop).DataUnit + strShowGL + Environment.NewLine)
                            Else
                                txtToolTip = GrpProp.DataTime(jloop).ToString(GrpProp.DateFormat) + "  ---  "               ' 2012/01/18 Kino Add
                            End If
                        End If

                        ''                            DataLoop -> jloop
                        MaxDisp = 0             '変数リセット
                        MaxDispDepth = 0        '変数リセット
                    Next line2Loop
                Next jloop

                If txtToolTip.Length > 2 Then
                    txtToolTip = txtToolTip.Substring(0, txtToolTip.Length - 2)
                    .ToolTip = txtToolTip
                End If

                '●●●●●　チャートエリア設定
                With .ChartArea
                    .Style.Border.BorderStyle = BorderStyleEnum.None            'チャートエリアのボーダー
                    .PlotArea.Boxed = True                                      'チャートエリアのボックスを表示する
                    .PlotArea.ForeColor = Drawing.Color.Black                   'ボックスの線色
                    ''.PlotArea.UseAntiAlias = False                            'プロットエリアアンチエイリアス
                    'Grps(iloop).Footer.Visible = False
                    'Grps(iloop).Legend.Visible = False
                    If DDLPaintWarningValue.SelectedValue = 1 Then              '警報値エリア表示フラグ
                        If GrpProp.DrawInfo(iloop).AlertArea = True Then        'グラフごとの警報値エリアフラグ
                            '●●●●　警報値領域
                            .PlotArea.AlarmZones.Clear()
                            Dim azs As AlarmZonesCollection = .PlotArea.AlarmZones
                            Dim az As AlarmZone = azs.AddNewZone
                            az = azs.AddNewZone
                            Dim azloop As Integer
                            Dim sngTemp As Single

                            For azloop = 1 To 3
                                ''●上限値
                                az = azs.AddNewZone
                                az.BackColor = Color.FromArgb(35 + (azloop - 1) * 15, Color.Red)           '30->15->35  若干透明度調整  10 -> 15
                                az.Shape = AlarmZoneShapeEnum.Polygon
                                az.Name = "AlertAreaUp" + azloop.ToString
                                az.ForeColor = Color.Transparent

                                az.PolygonData.X.Add(.AxisX.Max)                                        'X軸スケールの最大値のＹ軸スケールの最上部からスタート  最右上
                                az.PolygonData.Y.Add(GrpProp.TopPosition)                               'Y軸スケールの最上部
                                If GrpProp.DrawInfo(iloop).DrawData(0).AlertData(azloop - 1) < 1.1E+30 Then
                                    az.PolygonData.X.Add(GrpProp.DrawInfo(iloop).DrawData(0).AlertData(azloop - 1)) '最上部データの警報値をＹ軸スケールの最上部まで引っ張る
                                    az.PolygonData.Y.Add(GrpProp.TopPosition)                               'Y軸スケールの最上部
                                    ''az.PolygonData.Y.Add(GrpProp.DrawInfo(iloop).DrawData(0).DepthData)
                                End If

                                For jloop = 0 To GrpProp.DrawInfo(iloop).DataCount - 1          '深度数分のループ
                                    sngTemp = GrpProp.DrawInfo(iloop).DrawData(jloop).AlertData(azloop - 1)
                                    If sngTemp < 1.1E+30 Then 'sngTemp = 7.7E+30
                                        az.PolygonData.X.Add(Double.Parse(sngTemp))
                                        az.PolygonData.Y.Add(GrpProp.DrawInfo(iloop).DrawData(jloop).DepthData)
                                    End If
                                Next jloop

                                ''az.PolygonData.X.Add(sngTemp)                                           '最下部左                 '不動点までの場合
                                ''az.PolygonData.Y.Add(GrpProp.DrawInfo(iloop).FixPointDepth)
                                ''az.PolygonData.X.Add(.AxisX.Max)                                        '最下部スケールMax
                                ''az.PolygonData.Y.Add(GrpProp.DrawInfo(iloop).FixPointDepth)
                                If sngTemp < 1.1E+30 Then
                                    az.PolygonData.X.Add(sngTemp)                                           '最下部左                   'グラフ最大深度までの場合
                                    az.PolygonData.Y.Add(.AxisY.Min)
                                    az.PolygonData.X.Add(.AxisX.Max)                                        '最下部スケールMax          ' 2011/03/02 Kino Changed  inside If...
                                    az.PolygonData.Y.Add(.AxisY.Min)
                                End If
                                az.Visible = True

                                ''●下限値
                                az = azs.AddNewZone
                                az.BackColor = Color.FromArgb(35 + (azloop - 1) * 15, Color.Red)
                                az.ForeColor = Color.Transparent
                                az.Shape = AlarmZoneShapeEnum.Polygon
                                az.Name = "AlertAreaLo" + azloop.ToString
                                az.PolygonData.X.Add(.AxisX.Min)                                        '最左上
                                az.PolygonData.Y.Add(GrpProp.TopPosition)
                                ''az.PolygonData.Y.Add(GrpProp.DrawInfo(iloop).DrawData(0).DepthData)
                                az.PolygonData.X.Add(GrpProp.DrawInfo(iloop).DrawData(0).AlertData(azloop + 2)) '最上部データの警報値をＹ軸スケールの最上部まで引っ張る
                                az.PolygonData.Y.Add(GrpProp.TopPosition)                               'Y軸スケールの最上部


                                For jloop = 0 To GrpProp.DrawInfo(iloop).DataCount - 1          '深度数分のループ
                                    sngTemp = GrpProp.DrawInfo(iloop).DrawData(jloop).AlertData(azloop + 2)
                                    '' If sngTemp > 1.1E+30 Then sngTemp = 7.7E+30                          ' 2011/03/02 Kino Changed Lower statement
                                    If sngTemp < 1.1E+30 Then
                                        az.PolygonData.X.Add(sngTemp)
                                        az.PolygonData.Y.Add(GrpProp.DrawInfo(iloop).DrawData(jloop).DepthData)
                                    End If
                                Next jloop

                                ''az.PolygonData.X.Add(sngTemp)                                           '最下部左                 '不動点までの場合
                                ''az.PolygonData.Y.Add(GrpProp.DrawInfo(iloop).FixPointDepth)
                                ''az.PolygonData.X.Add(.AxisX.Max)                                        '最下部スケールMax
                                ''az.PolygonData.Y.Add(GrpProp.DrawInfo(iloop).FixPointDepth)
                                If sngTemp < 1.1E+30 Then                                                   ' 2011/03/02 Kino Add If...
                                    az.PolygonData.X.Add(sngTemp)                                           '最下部左                   'グラフ最大深度までの場合
                                    az.PolygonData.Y.Add(.AxisY.Min)
                                    az.PolygonData.X.Add(.AxisX.Min)                                        '最下部スケールMin
                                    az.PolygonData.Y.Add(.AxisY.Min)
                                End If
                                az.Visible = True
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
                    'mapPA.Tooltip = "プロット領域です"                                 'データ以外の表示
                    ''mapPA.Attributes = "onclick=""ShowDataCoords(event,WC1);"""        'クリック時の表示
                    mapData.Tooltip = "{#XVAL:0.0} [{#YVAL:0.0} m]"                     'データの表示
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

        With Me.WC0.ChartArea                                               '深度グラフ設定
            '●●●●●　Y2軸設定
            With .AxisY2
                .SetMinMax(GrpProp.TopPosition, GrpProp.GraphYScale)                          'Y軸の最小、最大値を設定
                .ForeColor = Drawing.Color.Black                            '線色
                .TickMinor = TickMarksEnum.Outside                          '副メモリの突出方向
                .TickMajor = TickMarksEnum.Outside                          '主メモリの突出方向
                .GridMajor.Visible = True                                   '主メモリ表示
                .GridMinor.Visible = True                                   '副メモリ表示
                If GrpProp.GridMajorSpaceY = 0 Then
                    .AutoMajor = True                                       '主目盛の値を自動的に計算
                    .GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                Else
                    .UnitMajor = GrpProp.GridMajorSpaceY
                    .GridMajor.Spacing = GrpProp.GridMajorSpaceY
                End If
                If GrpProp.GridMinorSpaceY = 0 Then
                    .AutoMinor = True                                           '副目盛の値を自動的に計算
                    .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                Else
                    .UnitMinor = GrpProp.GridMinorSpaceY
                    .GridMinor.Spacing = GrpProp.GridMinorSpaceY
                End If
            End With

            With .AxisY                                                     'ポイント表示
                Dim depthScale As Single = (GrpProp.TopPosition - GrpProp.GraphYScale)              ' 2014/10/08 Kino Add
                ''.SetMinMax((GrpProp.PointYScale + GrpProp.GraphYScale), GrpProp.PointYScale)        'Y軸の最小、最大値を設定      2014/10/08 Kino Changed 考慮されてなかった・・・orz
                .SetMinMax((GrpProp.PointYScale - depthScale), GrpProp.PointYScale)                     'Y軸の最小、最大値を設定    2014/10/08 Kino Add
                .ForeColor = Drawing.Color.Black                            '線色
                .TickMinor = TickMarksEnum.Outside                          '副メモリの突出方向
                .TickMajor = TickMarksEnum.Outside                          '主メモリの突出方向
                .GridMajor.Visible = True                                   '主メモリ表示
                .GridMinor.Visible = True                                   '副メモリ表示
                If GrpProp.GridMajorSpaceY = 0 Then                         '2010/02/18 Kino Add  GL設定と同じにする
                    .AutoMajor = True                                       '主目盛の値を自動的に計算
                    .GridMajor.AutoSpace = True                                 '主メモリのグリッド線の間隔を自動的に計算
                Else
                    .UnitMajor = GrpProp.GridMajorSpaceY
                    .GridMajor.Spacing = GrpProp.GridMajorSpaceY
                End If
                If GrpProp.GridMinorSpaceY = 0 Then
                    .AutoMinor = True                                           '副目盛の値を自動的に計算
                    .GridMinor.AutoSpace = True                                 '副メモリのグリッド線の間隔を自動的に計算
                Else
                    .UnitMinor = GrpProp.GridMinorSpaceY
                    .GridMinor.Spacing = GrpProp.GridMinorSpaceY
                End If
            End With
        End With

    End Sub

    Protected Function GraphDataInfoFromDB(ByVal siteDirectory As String, Optional ByVal postBackFlg As Boolean = False) As Integer
        ''
        '' 各グラフに描画するデータの情報
        ''
        Dim strSQL As String = ""
        Dim iloop As Integer
        Dim jloop As Integer
        Dim kloop As Integer
        Dim DataLoop As Integer
        Dim intRet As Integer = 0
        Dim strtemp() As String = {}
        Dim outch() As Integer = {}
        Dim clsconvChData As New ClsReadDataFile
        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbDa As OleDb.OleDbDataAdapter
        Dim DsetCount As Integer
        Dim RowDatas() As Array = {}
        Dim GetCh As Integer
        'Dim St As Integer
        'Dim DtSet As New DataSet("DData")
        Dim strSqlDate As String = "IN("

        If ToolkitScriptManager1.AsyncPostBackSourceElementID = "ImbtnRedrawGraph" Or ToolkitScriptManager1.AsyncPostBackSourceElementID.Length = 0 Then        'ポストバックがどのコントロールから発生したのかで判断(カレンダーのイベントではここに入らない!!)
            ''■■ 初期設定 警報値の定義 深度の読込み
            If Me.RdBFromNewest.Checked = True Then
                ReDim GrpProp.DataTime(0)
                GrpProp.DataCount = 1                                                                   '最新データのみなので、データ数は１つ
            Else
                For iloop = 0 To 6
                    If IsDate(TxtDates(iloop).Text) = True Then
                        GrpProp.DataCount += 1
                        strSqlDate += (Date.Parse(TxtDates(iloop).Text).ToString("#yyyy/MM/dd HH:mm:ss#") + ",")
                    End If
                Next
                ReDim GrpProp.DataTime(GrpProp.DataCount - 1)

                If strSqlDate = "IN(" Then strSqlDate = "" '' 2011/04/06 Kino Add 指定日と設定しているのに日付けが設定されていない場合

            End If

            For iloop = 0 To GrpProp.GraphCount - 1                                                     '1ページ当たりのグラフ個数分のループ
                strtemp = GrpProp.DrawInfo(iloop).DepthDatas.Split(",")                                 '深度文字列を深度データに変換
                For jloop = 0 To strtemp.Length - 1
                    GrpProp.DrawInfo(iloop).DrawData(jloop).DepthData = Single.Parse(strtemp(jloop))    '●深度データ
                Next jloop

                If GrpProp.DrawInfo(iloop).AddDataNum >= 2 Then
                    For jloop = 0 To (strtemp.Length \ GrpProp.DrawInfo(iloop).AddDataNum - 1)
                        ReDim GrpProp.DrawInfo(iloop).DrawData(jloop).AlertData(5)                          '警報値配列を設定　上限1～3　下限1～3
                    Next jloop
                Else
                    For jloop = 0 To strtemp.Length - 1
                        ReDim GrpProp.DrawInfo(iloop).DrawData(jloop).AlertData(5)                          '警報値配列を設定　上限1～3　下限1～3
                    Next jloop
                End If

                ReDim GrpProp.DrawInfo(iloop).DrawDispData(GrpProp.DataCount - 1, GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum - 1)   '変位データの配列指定(データ系列,深度)  -9999なら不動点ないので深度数を減らす

                Dim shrtTemp As Integer
                If GrpProp.DrawInfo(iloop).AddDataNum >= 2 Then
                    shrtTemp = 0                                        '並び替えなし(IN　OUTの順番で読み込みたいから)
                Else
                    shrtTemp = 1                                        '昇順で並び替え
                End If
                Dim strRet As String = clsconvChData.GetOutputChannel(outch, GrpProp.DrawInfo(iloop).DispDatas, shrtTemp)     'チャンネル文字列をチャンネルデータに変換

                For jloop = 0 To outch.Length - 1
                    GrpProp.DrawInfo(iloop).DrawData(jloop).DataCh = outch(jloop)
                Next

                '---------------
                '---
                Using DbCon As New OleDb.OleDbConnection
                    DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Server.MapPath(siteDirectory + "\App_Data\" + DataFileNames(GrpProp.DrawInfo(iloop).DataFileNo).CommonInf) + ";" + "Jet OLEDB:Engine Type= 5"
                    strSQL = clsconvChData.GetSQLString_Bunpu(GrpProp.DrawInfo(0).DataCount, GrpProp.DrawInfo(0).DrawData, Now(), Now(), "", True)

                    'DtSet = New DataSet("DData")                                                                        ' 2015/05/14 Kino Add　Newしないとデータセットが追記となってしまうため
                    '' データセット
                    Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                        Using DtSet As New DataSet("DData")
                            DbDa.Fill(DtSet, "DData")                                       '全レコードを開く

                            'DbCon.Dispose()

                            DsetCount = DtSet.Tables("DData").Rows.Count                    '全レコード数を取得
                            ReDim RowDatas(DtSet.Tables("DData").Rows(DtSet.Tables("DData").Rows.Count - 1).Item(1))            '最大CH番号で配列を設定   DataBaceChを配列番号として扱う
                            Dim DbCh As Integer

                            '' 2009/07/17 Kino Changed 高速化
                            For jloop = 0 To DsetCount - 1
                                DbCh = DtSet.Tables("DData").Rows(jloop).Item(1)
                                RowDatas(DbCh) = DtSet.Tables("DData").Rows.Item(DbCh - 1).ItemArray    'DRow.ItemArray         'DataBaseChを配列番号として配列に、配列として格納する
                            Next jloop

                            '---

                            GetCh = GrpProp.DrawInfo(iloop).DrawData(0).DataCh
                            For DataLoop = 0 To GrpProp.DataCount - 1                                                                           'データ系列数分のループ ↓
                                GrpProp.DataTime(DataLoop) = Date.Parse(RowDatas(GetCh).GetValue(0))                                            'データの測定時刻       2015/05/14 Kino Change 復活
                                For jloop = 0 To (GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum - 1)   '深度数分のループ       1データ2線対応
                                    GetCh = GrpProp.DrawInfo(iloop).DrawData(jloop).DataCh
                                    GrpProp.DrawInfo(iloop).DrawData(jloop).SensorSymbol = RowDatas(GetCh).GetValue(2).ToString                 '●計器記号
                                    'Next jloop
                                    'For jloop = 0 To GrpProp.DrawInfo(iloop).DataCount - 1                                                          ' 2015/05/14 Kino Add  2017/03/02 Kino Changed  警報値の読込が最深部だけになってしまうのを修正
                                    If GrpProp.DrawInfo(iloop).AddDataNum >= 2 AndAlso jloop >= GrpProp.DrawInfo(iloop).DataCount Then Continue For ' 2017/03/02 Kino Add 2系列データ表示時のエラー対策
                                    For kloop = 0 To 5                                                                                          '警報値分のループ（上限3つ下限3つ）
                                        GrpProp.DrawInfo(iloop).DrawData(jloop).AlertData(kloop) = CType(RowDatas(GetCh).GetValue(kloop + 4).ToString, Single) '●警報値    2016/04/22 Kino Changed  3 -> 4  SQLでフィールド追加のため
                                    Next kloop
                                Next jloop
                            Next DataLoop
                            'DbDa.Dispose()
                        End Using
                    End Using
                    '---------------

                    '■■最新データの場合
                    If Me.RdBFromNewest.Checked = True Then                                                                                     ' 2015/05/14 Kino Add　　Moveed from LowerStatement▲
                        ''警報値、計器記号、測定時刻の読込み 上下限警報値1～3・データ・測定時刻・計器記号 取得
                        ''For iloop = 0 To GrpProp.GraphCount - 1                                                                               'グラフ個数分のループ 2015/05/25 Kino Changed 個別のループを大きな１つのループにした
                        For DataLoop = 0 To GrpProp.DataCount - 1                                                                           'データ系列数分のループ ↓
                            GrpProp.DataTime(DataLoop) = Date.Parse(RowDatas(GetCh).GetValue(0))                                            'データの測定時刻       2015/05/14 Kino Change 復活
                            For jloop = 0 To (GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum - 1)   '深度数分のループ       1データ2線対応
                                GetCh = GrpProp.DrawInfo(iloop).DrawData(jloop).DataCh
                                GrpProp.DrawInfo(iloop).DrawDispData(DataLoop, jloop) = RowDatas(GetCh).GetValue(10)                         '●変位データ           2015/05/14 Kino Changed 復活       2016/04/22 Kino Changed  9 -> 10  SQLでフィールド追加のため
                            Next jloop
                        Next DataLoop
                        ' ''next iloop
                        ' ''Array.Clear(RowDatas, 0, RowDatas.Length)                                                                         ' 2015/05/25 Kino Changed  Moveed to lower statement
                        'DtSet.Dispose()
                        'DbCon.Dispose()

                        intRet = 1

                    Else
                        '■■過去データの場合

                        ''上の警報値読込みの分をDispose
                        'DtSet.Dispose()
                        'DbCon.Dispose()

                        If strSqlDate.Length = 0 Then
                            intRet = 0
                            Return intRet
                            ''Exit Function
                        Else
                            strSqlDate = strSqlDate.Substring(0, strSqlDate.Length - 1) + ")" '' 2011/04/06 Kino Add If...
                        End If

                        ''For iloop = 0 To GrpProp.GraphCount - 1                                                             '■■■■グラフ個数分のループ 2015/05/25 Kino Changed 個別のループを大きな１つのループにした
                        Dim NewDataCount As Integer = GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum - 1
                        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Server.MapPath(siteDirectory + "\App_Data\" + DataFileNames(GrpProp.DrawInfo(iloop).DataFileNo).FileName) + ";" + "Jet OLEDB:Engine Type= 5")

                        strSQL = clsconvChData.GetSQLString_Bunpu(Short.Parse(NewDataCount), GrpProp.DrawInfo(iloop).DrawData, Now(), Now(), strSqlDate, False)

                        '' データセット
                        'DtSet = New DataSet("DData")
                        Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                            Using DtSet As New DataSet("DData")
                                DbDa.Fill(DtSet, "DData")                                                                       '指定日時のレコードを取得

                                DbCon.Dispose()

                                DsetCount = DtSet.Tables("DData").Rows.Count                                                        'データ数(日付数)

                                DataLoop = 0
                                For Each DTR As DataRow In DtSet.Tables("DData").Rows
                                    GrpProp.DataTime(DataLoop) = DTR.Item(0)                       'データの測定時刻 
                                    For jloop = 0 To NewDataCount '- 1                                                              '2010/08/25 Kino Changed 
                                        GrpProp.DrawInfo(iloop).DrawDispData(DataLoop, jloop) = Single.Parse(DTR.Item(jloop + 1))   '●変位データ
                                    Next jloop
                                    DataLoop += 1
                                Next
                                ''Next iloop

                                'DtSet.Dispose()
                                'DbDa.Dispose()
                            End Using
                        End Using
                        intRet = 1
                    End If

                    Array.Clear(RowDatas, 0, RowDatas.Length)                                                               ' 2015/05/25 Kino Add
                End Using
            Next iloop

            clsconvChData = Nothing

            'DbCon.Dispose()                                                                                                 ' 2015/05/25 Kino Add
            'DbCon = Nothing                                                                                                 ' 2015/05/25 Kino Add

        End If

        Return intRet

    End Function

    Protected Function GraphLineInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, Optional ByVal postBackFlg As Boolean = False) As Integer
        ''
        '' 項目名におけるページに表示する個々のグラフ情報　_2
        ''
        '' Dim DbDr As OleDb.OleDbDataReader
        '' Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String
        Dim iloop As Integer = 0
        Dim intRet As Integer = 0

        GraphLineInfoFromDB = 0

        strSQL = ("Select * From [" + GrpProp.WindowTitle + "データ情報] ORDER BY グラフNo ASC;")
        Using DbDa As New OleDb.OleDbDataAdapter(strSQL, dbCon)
            Using DtSet As New DataSet("DData")
                DbDa.Fill(DtSet, "DData")
                '' DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
                ''DbDr = DbCom.ExecuteReader

                Try
                    For Each DTR As DataRow In DtSet.Tables("DData").Rows                                   'グラフ数でループ
                        If iloop = GrpProp.GraphCount Then                                                  ' 2012/01/18 Kino Add
                            Exit For
                        End If

                        If DTR.ItemArray.GetValue(2).ToString.Length <> 0 Then
                            GrpProp.DrawInfo(iloop).DirectionalTitle = DTR.ItemArray.GetValue(2).ToString   '方向性タイトル
                        Else
                            GrpProp.DrawInfo(iloop).DirectionalTitle = ""                                   '方向性タイトルなし
                        End If
                        With DTR.ItemArray
                            GrpProp.DrawInfo(iloop).DefaultSet = Boolean.Parse(.GetValue(3))                '既定値かどうか                     Cnt
                            GrpProp.DrawInfo(iloop).DefaultValue = Integer.Parse(.GetValue(4))              '既定値の場合のリストインデックス   Cnt
                            GrpProp.DrawInfo(iloop).DataTitle = .GetValue(5).ToString                       'データタイトル
                            GrpProp.DrawInfo(iloop).DataCount = Short.Parse(.GetValue(6))                   '深度データ数
                            GrpProp.DrawInfo(iloop).Max = Single.Parse(.GetValue(7))                        'スケール最大値
                            GrpProp.DrawInfo(iloop).Min = Single.Parse(.GetValue(8))                        'スケール最小値
                            GrpProp.DrawInfo(iloop).UnitMajor = Single.Parse(.GetValue(9))                  '主メモリ間隔    
                            GrpProp.DrawInfo(iloop).UnitMinor = Single.Parse(.GetValue(10))                 '副メモリ間隔
                            GrpProp.DrawInfo(iloop).CalcFromDeepest = Boolean.Parse(.GetValue(11))          '最深部から累積
                            GrpProp.DrawInfo(iloop).LineWidth = Short.Parse(.GetValue(12))                  'データ線幅
                            GrpProp.DrawInfo(iloop).LineType = Short.Parse(.GetValue(13))                   'データ線種
                            GrpProp.DrawInfo(iloop).Marker = Boolean.Parse(.GetValue(14))                   'マーカ表示
                            GrpProp.DrawInfo(iloop).DataLabel = Boolean.Parse(.GetValue(15))                'データラベル表示
                            GrpProp.DrawInfo(iloop).DepthDatas = .GetValue(16).ToString                     '深度データ文字列
                            GrpProp.DrawInfo(iloop).DispDatas = .GetValue(17).ToString                      '描画データチャンネル文字列
                            GrpProp.DrawInfo(iloop).FixPointDepth = Single.Parse(.GetValue(18))             '不動点深度
                            GrpProp.DrawInfo(iloop).DataUnit = .GetValue(19).ToString                       'データの単位
                            GrpProp.DrawInfo(iloop).DataFileNo = Integer.Parse(.GetValue(20))               'データファイル番号             
                            GrpProp.DrawInfo(iloop).AlertArea = Boolean.Parse(.GetValue(21))                '警報領域表示フラグ
                            GrpProp.DrawInfo(iloop).AddDataNum = Short.Parse(.GetValue(22))                 'IN-OUTなど1日付あたり2系列のデータが必要な場合の数
                            GrpProp.DrawInfo(iloop).LineExp = .GetValue(23).ToString                        ' ↑の系列説明文字列　カンマ区切り
                            If DtSet.Tables("DData").Columns.Count >= 25 Then                               ' フィールド数チェック
                                GrpProp.DrawInfo(iloop).Reversed = Boolean.Parse(.GetValue(24))             '横軸反転
                            Else
                                GrpProp.DrawInfo(iloop).Reversed = False
                            End If
                        End With
                        iloop += 1
                    Next
                    intRet = 1
                    ''With DbDr
                    ''    If .HasRows = True Then
                    ''        For iloop = 0 To GrpProp.GraphCount - 1                                     'グラフ数でループ
                    ''            .Read()

                    ''            If .Item(2).ToString.Length <> 0 Then
                    ''                GrpProp.DrawInfo(iloop).DirectionalTitle = .GetString(2)            '方向性タイトル
                    ''            Else
                    ''                GrpProp.DrawInfo(iloop).DirectionalTitle = ""                       '方向性タイトルなし
                    ''            End If
                    ''            GrpProp.DrawInfo(iloop).DefaultSet = .GetBoolean(3)                     '既定値かどうか                     Cnt
                    ''            GrpProp.DrawInfo(iloop).DefaultValue = .GetInt32(4)                     '既定値の場合のリストインデックス   Cnt
                    ''            GrpProp.DrawInfo(iloop).DataTitle = .GetString(5)                       'データタイトル
                    ''            GrpProp.DrawInfo(iloop).DataCount = .GetInt32(6)                        '深度データ数
                    ''            GrpProp.DrawInfo(iloop).Max = .GetFloat(7)                              'スケール最大値
                    ''            GrpProp.DrawInfo(iloop).Min = .GetFloat(8)                              'スケール最小値
                    ''            GrpProp.DrawInfo(iloop).UnitMajor = .GetInt32(9)                        '主メモリ間隔    
                    ''            GrpProp.DrawInfo(iloop).UnitMinor = .GetInt32(10)                       '副メモリ間隔
                    ''            GrpProp.DrawInfo(iloop).CalcFromDeepest = .GetBoolean(11)               '最深部から累積
                    ''            GrpProp.DrawInfo(iloop).LineWidth = .GetInt16(12)                       'データ線幅
                    ''            GrpProp.DrawInfo(iloop).LineType = .GetInt16(13)                        'データ線種
                    ''            GrpProp.DrawInfo(iloop).Marker = .GetBoolean(14)                        'マーカ表示
                    ''            GrpProp.DrawInfo(iloop).DataLabel = .GetBoolean(15)                     'データラベル表示
                    ''            GrpProp.DrawInfo(iloop).DepthDatas = .GetString(16)                     '深度データ文字列
                    ''            GrpProp.DrawInfo(iloop).DispDatas = .GetString(17)                      '描画データチャンネル文字列
                    ''            GrpProp.DrawInfo(iloop).FixPointDepth = .GetFloat(18)                   '不動点深度
                    ''            GrpProp.DrawInfo(iloop).DataUnit = .GetString(19)                       'データの単位
                    ''            GrpProp.DrawInfo(iloop).DataFileNo = .GetInt16(20)                      'データファイル番号             
                    ''            GrpProp.DrawInfo(iloop).AlertArea = .GetBoolean(21)                     '警報領域表示フラグ
                    ''            GrpProp.DrawInfo(iloop).AddDataNum = .GetInt16(22)                      'IN-OUTなど1日付あたり2系列のデータが必要な場合の数
                    ''            GrpProp.DrawInfo(iloop).LineExp = .GetString(23)                        ' ↑の系列説明文字列　カンマ区切り
                    ''        Next
                    ''        intRet = 1
                    ''    End If
                    ''End With

                    For iloop = 0 To GrpProp.GraphCount - 1                                             '個々のグラフの深度データ数で配列を指定 ここの他に[ReadViewState]でも同じことをしている(再描画の場合の対応)
                        ''If GrpProp.DrawInfo(iloop).FixPointDepth = -9999 Then                         ' 2012/07/20 Kino Changed 不動点の扱いを変えたので、同じでよくなった
                        ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum - 1)    '-9999の場合は不動点がないので、１つ少なめでよい 0～10 等　IN-OUTなどの場合は系列数倍とする
                        ''Else
                        ''    ' ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount)       '不動点のデータがあるので、１つ多めでよい 0～10 等
                        ''    '' ReDim GrpProp.DrawInfo(iloop).DrawData((GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum) _
                        ''    ''                                    + (GrpProp.DrawInfo(iloop).AddDataNum - 1))       '不動点のデータがあるので、１つ多めでよい 0～10 等 2012/07/19 2系列データの場合の対応(2倍+1)
                        ''    ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum - 1)    '-9999の場合は不動点がないので、１つ少なめでよい 0～10 等　IN-OUTなどの場合は系列数倍とする
                        ''End If
                    Next iloop

                Catch ex As Exception

                Finally
                    'DbDa.Dispose()
                    'DtSet.Dispose()
                    ''DbDr.Close()
                    ''DbCom.Dispose()
                End Try

            End Using
        End Using

        Return intRet

    End Function

    Protected Function ReadGraphInfoFromDB(ByVal dbCon As OleDb.OleDbConnection, ByVal graphType As String, Optional ByVal PostBackFlg As Boolean = False) As Integer
        ''    Protected Function ReadGraphInfoFromDB(ByVal grpDataFile As String, ByVal graphType As String, Optional ByVal PostBackFlg As Boolean = False) As Integer
        ''
        '' グラフの描画情報を読み込む　_1
        ''
        Dim strSQL As String
        'Dim iloop As Integer
        ''Dim DbCom As New OleDb.OleDbCommand
        ''Dim DbDr As OleDb.OleDbDataReader
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim intRet As Integer = 0
        Dim iloop As Integer

        ReDim GrpProp.DrawSizeColorInf(15)                          'グラフの各部分のフォントサイズなどは固定の配列

        strSQL = ("Select * From メニュー基本情報 Where (項目名 = '" + GrpProp.WindowTitle + "' And 種別 = '" + graphType + "')")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")
        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader

        With DtSet.Tables("DData")
            If .Rows.Count > 0 Then
                GrpProp.GraphCount = Integer.Parse(.Rows(0).Item(4))                'グラフの数
                GrpProp.GraphBoxCount = Integer.Parse(.Rows(0).Item(5))             'グラフ枠の数(パネル)
                GrpProp.GraphTitle = .Rows(0).Item(6).ToString                      'グラフのタイトル
                GrpProp.TitlePosition = Short.Parse(.Rows(0).Item(7))               'タイトルの位置(上or下)         'Cnt
                GrpProp.DateFormat = .Rows(0).Item(8).ToString                      '日付の表示フォーマット         'Cnt
                GrpProp.PaperOrientaion = Integer.Parse(.Rows(0).Item(9))           '紙の向き                       'Cnt
                GrpProp.EnableLegend = Boolean.Parse(.Rows(0).Item(26))             '凡例の有無                     'Cnt
                GrpProp.ConstructionStatus = Boolean.Parse(.Rows(0).Item(27))       '施工情報表示の有無             
                GrpProp.ConstructionStatusID = Integer.Parse(.Rows(0).Item(28))     '施工情報ID
                GrpProp.SoilStatus = Boolean.Parse(.Rows(0).Item(29))               '土質情報表示の有無
                GrpProp.SoilStatusID = Integer.Parse(.Rows(0).Item(30))             '土質情報ID
                GrpProp.TopPosition = Single.Parse(.Rows(0).Item(31))               '天端位置深度
                GrpProp.GraphYScale = Single.Parse(.Rows(0).Item(32))               '描画深度
                GrpProp.GridMajorSpaceY = Single.Parse(.Rows(0).Item(33))           '深度主メモリスケール間隔
                GrpProp.GridMinorSpaceY = Single.Parse(.Rows(0).Item(34))           '深度副メモリスケール間隔
                GrpProp.PointYScaleShow = Boolean.Parse(.Rows(0).Item(35))          'ポイント深度表示の有無
                GrpProp.PointYScale = Single.Parse(.Rows(0).Item(36))               'ポイント深度
                GrpProp.PointName = .Rows(0).Item(37).ToString                      'ポイント名称
                GrpProp.AlertZoneShow = Boolean.Parse(.Rows(0).Item(38))            '警報値表示の有無               'Cnt
                GrpProp.URL = .Rows(0).Item(39).ToString                            'グラフ表示ファイル名
                GrpProp.MissingDataContinuous = Boolean.Parse(.Rows(0).Item(40))    '欠測データの連結               'Cnt
                GrpProp.ShowGL2Legend = Boolean.Parse(.Rows(0).Item(41))            '凡例へのGL表示の有無           '2009/09/15 Kino Add
                For iloop = 10 To 25
                    GrpProp.DrawSizeColorInf(iloop - 10) = Integer.Parse(.Rows(0).Item(iloop))  'グラフの各部分のフォントサイズなど(詳細は下を参照)
                Next iloop
            End If
        End With

        ''With DbDr
        ''    If .HasRows = True Then
        ''        .Read()                                             '１レコード分読み込み
        ''        GrpProp.GraphCount = .GetInt32(4)                   'グラフの数
        ''        GrpProp.GraphBoxCount = .GetInt32(5)                'グラフ枠の数(パネル)
        ''        GrpProp.GraphTitle = .GetString(6)                  'グラフのタイトル
        ''        GrpProp.TitlePosition = CType(.GetInt32(7), Short)  'タイトルの位置(上or下)         'Cnt
        ''        GrpProp.DateFormat = .GetString(8)                  '日付の表示フォーマット         'Cnt
        ''        GrpProp.PaperOrientaion = .GetInt32(9)              '紙の向き                       'Cnt
        ''        GrpProp.EnableLegend = .GetBoolean(26)              '凡例の有無                     'Cnt
        ''        GrpProp.ConstructionStatus = .GetBoolean(27)        '施工情報表示の有無             
        ''        GrpProp.ConstructionStatusID = .GetInt16(28)        '施工情報ID
        ''        GrpProp.SoilStatus = .GetBoolean(29)                '土質情報表示の有無
        ''        GrpProp.SoilStatusID = .GetInt16(30)                '土質情報ID
        ''        GrpProp.TopPosition = .GetFloat(31)                 '天端位置深度
        ''        GrpProp.GraphYScale = .GetFloat(32)                 '描画深度
        ''        GrpProp.GridMajorSpaceY = .GetFloat(33)             '深度主メモリスケール間隔
        ''        GrpProp.GridMinorSpaceY = .GetFloat(34)             '深度副メモリスケール間隔
        ''        GrpProp.PointYScaleShow = .GetBoolean(35)           'ポイント深度表示の有無
        ''        GrpProp.PointYScale = .GetFloat(36)                 'ポイント深度
        ''        GrpProp.PointName = .GetString(37)                  'ポイント名称
        ''        GrpProp.AlertZoneShow = .GetBoolean(38)             '警報値表示の有無               'Cnt
        ''        ''GrpProp.DataFileNo = .GetInt16(39)                  'データファイル番号           '各グラフ設定に移行
        ''        GrpProp.URL = .GetString(39)                        'グラフ表示ファイル名
        ''        GrpProp.MissingDataContinuous = .GetBoolean(40)     '欠測データの連結               'Cnt
        ''        GrpProp.ShowGL2Legend = .GetBoolean(41)             '凡例へのGL表示の有無           '2009/09/15 Kino Add
        ''        For iloop = 10 To 25
        ''            GrpProp.DrawSizeColorInf(iloop - 10) = .GetInt32(iloop)     'グラフの各部分のフォントサイズなど(詳細は下を参照)
        ''        Next iloop
        ''    End If
        ''End With

        ReDim GrpProp.DrawInfo(GrpProp.GraphCount - 1)          '個々のグラフの情報を格納する配列設定(グラフ個数分)

        '' 配列番号における変数の内容                           '11 24:Y1軸スケールの文字色　
        ''0 10:グラフタイトルサイズ             8 18:Y軸スケールサイズ
        ''1 11:グラフタイトル色                 9 19:Y軸タイトル色
        ''2 12:サブタイトルサイズ              10 20:目盛り線幅
        ''3 13:サブタイトル色                  11 21:目盛り線色
        ''4 14:X軸タイトルサイズ               12 22:補助メモリ線幅
        ''5 15:X軸スケールサイズ               13 23:補助目盛り線色
        ''6 16:X軸タイトル色                   14 24:単位タイトルサイズ
        ''7 17:Y軸タイトルサイズ               15 25:単位タイトル色
        intRet = 1

        Return intRet

    End Function

    Protected Sub SetViewState()
        ''
        '' 変数から取得した内容をビューステートに格納する     変数　→　ビューステート
        ''
        Dim iloop As Integer
        Dim strCount As String

        ViewState("GTitle") = GrpProp.GraphTitle                    'グラフのタイトル
        ViewState("GBoxCount") = GrpProp.GraphBoxCount              'グラフ枠の数(パネル)
        ViewState("GCount") = GrpProp.GraphCount                    'グラフの数(True Web Chartコントロール)
        ViewState("WinTitle") = GrpProp.WindowTitle                 'グラフタイトル(ウィンドウタイトル)
        ViewState("ContStatus") = GrpProp.ConstructionStatus        '施工情報表示有無
        ViewState("ContID") = GrpProp.ConstructionStatusID          '施工情報ID
        ViewState("SoilStatus") = GrpProp.SoilStatus                '土質情報表示の有無
        ViewState("SoilID") = GrpProp.SoilStatusID                  '土質情報ID
        ViewState("TopPos") = GrpProp.TopPosition                   '天端位置深度
        ViewState("YScale") = GrpProp.GraphYScale                   '描画深度
        ViewState("YMajor") = GrpProp.GridMajorSpaceY               '深度主メモリスケール間隔
        ViewState("YMinor") = GrpProp.GridMinorSpaceY               '深度副メモリスケール間隔
        ViewState("PointYShow") = GrpProp.PointYScaleShow           'ポイント深度表示の有無
        ViewState("PointYScale") = GrpProp.PointYScale              'ポイント深度
        ViewState("PointYName") = GrpProp.PointName                 'ポイント名称
        ViewState("aspxFile") = GrpProp.URL                         'グラフ表示ファイル名称
        ViewState("AtUpDt") = TmrState                              'タイマーの状態
        ViewState("GL2Lgd") = GrpProp.ShowGL2Legend                 '凡例にGL表示の有無

        For iloop = 0 To GrpProp.GraphCount - 1
            strCount = iloop.ToString
            ViewState("DirectionTitle" + strCount) = GrpProp.DrawInfo(iloop).DirectionalTitle               '方向性ヘッダー
            ViewState("DataTitle" + strCount) = GrpProp.DrawInfo(iloop).DataTitle                           'データタイトル
            ViewState("DataCount" + strCount) = GrpProp.DrawInfo(iloop).DataCount                           '深度データ数
            ViewState("XMax" + strCount) = GrpProp.DrawInfo(iloop).Max                                      'スケール最大値
            ViewState("XMin" + strCount) = GrpProp.DrawInfo(iloop).Min                                      'スケール最小値
            ViewState("UtMajor" + strCount) = GrpProp.DrawInfo(iloop).UnitMajor                             '主メモリ間隔    
            ViewState("UtMinor" + strCount) = GrpProp.DrawInfo(iloop).UnitMinor                             '副メモリ間隔
            ViewState("Deepest" + strCount) = GrpProp.DrawInfo(iloop).CalcFromDeepest                       '最深部から累積
            ViewState("LineWid" + strCount) = GrpProp.DrawInfo(iloop).LineWidth                             'データ線幅
            ViewState("LineTyp" + strCount) = GrpProp.DrawInfo(iloop).LineType                              'データ線種
            ViewState("Marker" + strCount) = GrpProp.DrawInfo(iloop).Marker                                 'マーカ表示
            ViewState("DatLabel" + strCount) = GrpProp.DrawInfo(iloop).DataLabel                            'データラベル表示
            ViewState("DepDatStr" + strCount) = GrpProp.DrawInfo(iloop).DepthDatas                          '深度データ文字列
            ViewState("DispDatStr" + strCount) = GrpProp.DrawInfo(iloop).DispDatas                          '描画データチャンネル文字列
            ViewState("FixDepth" + strCount) = GrpProp.DrawInfo(iloop).FixPointDepth                        '不動点深度
            ViewState("DatUt" + strCount) = GrpProp.DrawInfo(iloop).DataUnit                                'データの単位
            ViewState("DatFlNo" + strCount) = GrpProp.DrawInfo(iloop).DataFileNo                            'データファイル番号  
            ViewState("ShowAlert" + strCount) = GrpProp.DrawInfo(iloop).AlertArea                           '警報値エリア表示
            ViewState("AddDataNum" + strCount) = GrpProp.DrawInfo(iloop).AddDataNum                         'IN-OUTなど1日付あたり2系列のデータが必要な場合の数
            ViewState("LineExp" + strCount) = GrpProp.DrawInfo(iloop).LineExp                               ' ↑の系列説明文字列　カンマ区切り
            ViewState("Reverse" + strCount) = GrpProp.DrawInfo(iloop).Reversed                              '横軸反転
        Next iloop

        For iloop = 10 To 25
            ViewState("Color" + (iloop - 10).ToString) = GrpProp.DrawSizeColorInf(iloop - 10)   'グラフの各部分のフォントサイズなど
        Next iloop

    End Sub

    Protected Sub ReadViewState()
        ''
        '' ビューステートから取得した内容を変数へ格納する　　　ビューステート　→　変数
        ''
        Dim iloop As Integer
        Dim strCount As String

        ReDim GrpProp.DrawSizeColorInf(15)

        GrpProp.GraphTitle = CType(ViewState("GTitle"), String)                                     'グラフのタイトル
        GrpProp.GraphBoxCount = CType(ViewState("GBoxCount"), Integer)                              'グラフ枠の数(パネル)
        GrpProp.GraphCount = CType(ViewState("GCount"), Integer)                                    'グラフの数(True Web Chartコントロール)
        GrpProp.WindowTitle = CType(ViewState("WinTitle"), String)                                  'グラフタイトル(ウィンドウタイトル)
        GrpProp.ConstructionStatus = CType(ViewState("ContStatus"), Boolean)                        '施工情報表示有無
        GrpProp.ConstructionStatusID = CType(ViewState("ContID"), Integer)                          '施工情報ID
        GrpProp.SoilStatus = CType(ViewState("SoilStatus"), Boolean)                                '土質情報表示の有無
        GrpProp.SoilStatusID = CType(ViewState("SoilID"), Integer)                                  '土質情報ID
        GrpProp.TopPosition = CType(ViewState("TopPos"), Single)                                    '天端位置深度
        GrpProp.GraphYScale = CType(ViewState("YScale"), Single)                                    '描画深度
        GrpProp.GridMajorSpaceY = CType(ViewState("YMajor"), Single)                                '深度主メモリスケール間隔
        GrpProp.GridMinorSpaceY = CType(ViewState("YMinor"), Single)                                '深度副メモリスケール間隔
        GrpProp.PointYScaleShow = CType(ViewState("PointYShow"), Boolean)                           'ポイント深度表示の有無
        GrpProp.PointYScale = CType(ViewState("PointYScale"), Single)                               'ポイント深度
        GrpProp.PointName = CType(ViewState("PointYName"), String)                                  'ポイント名称
        GrpProp.URL = CType(ViewState("aspxFile"), String)                                          'グラフ表示ファイル名称
        TmrState = ViewState("AtUpDt")                                                              'タイマーの起動状態
        GrpProp.ShowGL2Legend = CType(ViewState("GL2Lgd"), Boolean)                                 '凡例にGL表示の有無

        ReDim GrpProp.DrawInfo(GrpProp.GraphCount - 1)

        For iloop = 0 To GrpProp.GraphCount - 1
            strCount = iloop.ToString
            GrpProp.DrawInfo(iloop).DirectionalTitle = CType(ViewState("DirectionTitle" + strCount), String)        '方向性ヘッダー
            GrpProp.DrawInfo(iloop).DataTitle = CType(ViewState("DataTitle" + strCount), String)                    'データタイトル
            GrpProp.DrawInfo(iloop).DataCount = CType(ViewState("DataCount" + strCount), Short)                     '深度データ数
            GrpProp.DrawInfo(iloop).Max = CType(ViewState("XMax" + strCount), Single)                               'スケール最大値
            GrpProp.DrawInfo(iloop).Min = CType(ViewState("XMin" + strCount), Single)                               'スケール最小値
            GrpProp.DrawInfo(iloop).UnitMajor = CType(ViewState("UtMajor" + strCount), Single)                      '主メモリ間隔    
            GrpProp.DrawInfo(iloop).UnitMinor = CType(ViewState("UtMinor" + strCount), Single)                      '副メモリ間隔
            GrpProp.DrawInfo(iloop).CalcFromDeepest = CType(ViewState("Deepest" + strCount), Boolean)               '最深部から累積
            GrpProp.DrawInfo(iloop).LineWidth = CType(ViewState("LineWid" + strCount), Short)                       'データ線幅
            GrpProp.DrawInfo(iloop).LineType = CType(ViewState("LineTyp" + strCount), Short)                        'データ線種
            GrpProp.DrawInfo(iloop).Marker = CType(ViewState("Marker" + strCount), Boolean)                         'マーカ表示
            GrpProp.DrawInfo(iloop).DataLabel = CType(ViewState("DatLabel" + strCount), Boolean)                    'データラベル表示
            GrpProp.DrawInfo(iloop).DepthDatas = CType(ViewState("DepDatStr" + strCount), String)                   '深度データ文字列
            GrpProp.DrawInfo(iloop).DispDatas = CType(ViewState("DispDatStr" + strCount), String)                   '描画データチャンネル文字列
            GrpProp.DrawInfo(iloop).FixPointDepth = CType(ViewState("FixDepth" + strCount), Single)                 '不動点深度
            GrpProp.DrawInfo(iloop).DataUnit = CType(ViewState("DatUt" + strCount), String)                         'データの単位
            GrpProp.DrawInfo(iloop).DataFileNo = CType(ViewState("DatFlNo" + strCount), Integer)                    'データファイル番号  
            GrpProp.DrawInfo(iloop).AlertArea = CType(ViewState("ShowAlert" + strCount), Boolean)                   '警報値エリア表示
            GrpProp.DrawInfo(iloop).AddDataNum = CType(ViewState("AddDataNum" + strCount), Short)                   'IN-OUTなど1日付あたり2系列のデータが必要な場合の数
            GrpProp.DrawInfo(iloop).LineExp = CType(ViewState("LineExp" + strCount), String)                        ' ↑の系列説明文字列　カンマ区切り
            GrpProp.DrawInfo(iloop).Reversed = CType(ViewState("Reverse" + strCount), String)                       '横軸反転

        Next iloop

        ''For iloop = 0 To GrpProp.GraphCount - 1                                                     '個々のグラフの深度データ数で配列を指定
        ''    ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount)               '不動点のデータがあるので、１つ多めでよい 0～10 等
        ''Next iloop
        For iloop = 0 To GrpProp.GraphCount - 1                                             '個々のグラフの深度データ数で配列を指定 ここの他に[GraphLineInfoFromDB]でも同じことをしている(PostBackでない場合)
            ''If GrpProp.DrawInfo(iloop).FixPointDepth = -9999 Then                                     ' 2012/07/20 Kino Changed 不動点の取り扱いを変更したので同じでよくなった
            ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum - 1)    '-9999の場合は不動点がないので、１つ少なめでよい 0～10 等　IN-OUTなどの場合は系列数倍とする
            ''Else
            ''    ' ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount)       '不動点のデータがあるので、１つ多めでよい 0～10 等
            ''    '' ReDim GrpProp.DrawInfo(iloop).DrawData((GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum) _
            ''    ''                                    + (GrpProp.DrawInfo(iloop).AddDataNum - 1))       '不動点のデータがあるので、１つ多めでよい 0～10 等 2012/07/19 2系列データの場合の対応(2倍+1)
            ''    ReDim GrpProp.DrawInfo(iloop).DrawData(GrpProp.DrawInfo(iloop).DataCount * GrpProp.DrawInfo(iloop).AddDataNum - 1)    '-9999の場合は不動点がないので、１つ少なめでよい 0～10 等　IN-OUTなどの場合は系列数倍とする
            ''End If
        Next iloop

        For iloop = 10 To 25
            GrpProp.DrawSizeColorInf(iloop - 10) = CType(ViewState("Color" + (iloop - 10).ToString), Integer)       'グラフの各部分のフォントサイズなど
        Next iloop

    End Sub


    Protected Function ReadFromControl() As Integer
        ''
        '' コントロールに配置された値を読んで、変数へ格納する
        ''
        ''Dim iloop As Integer
        Dim retVal As Integer = 0

        ' ''オブジェクトの指定: グラフコントロール：スケール設定リストボックス : スケール既定・入力　Privateとした
        ''defVal(0) = Me.DdlScale1 : scaleDef(0) = Me.RBList1 : scaleMin(0) = Me.TxtMin1 : scaleMax(0) = Me.TxtMax1
        ''defVal(1) = Me.DdlScale2 : scaleDef(1) = Me.RBList2 : scaleMin(1) = Me.TxtMin2 : scaleMax(1) = Me.TxtMax2
        ''defVal(2) = Me.DdlScale3 : scaleDef(2) = Me.RBList3 : scaleMin(2) = Me.TxtMin3 : scaleMax(2) = Me.TxtMax3
        ''defVal(3) = Me.DdlScale4 : scaleDef(3) = Me.RBList4 : scaleMin(3) = Me.TxtMin4 : scaleMax(3) = Me.TxtMax4

        ''■表示一般設定
        GrpProp.DateFormat = Me.DdlDateFormat.SelectedValue                                     '日付フォーマット
        GrpProp.EnableLegend = Me.DdlEnableLegend.SelectedIndex                                 '凡例表示　0:しない 1:する
        GrpProp.PaperOrientaion = Me.DdlPaperOrientation.SelectedIndex                          '用紙の向き
        GrpProp.TitlePosition = Me.DdlTitlePosition.SelectedIndex                               'グラフタイトル位置
        GrpProp.MissingDataContinuous = Me.DdlContinous.SelectedIndex                           '欠測データの連結
        GrpProp.AlertZoneShow = Me.DDLPaintWarningValue.SelectedIndex                           '警報値表示

        ''■データスケール設定                                                                  '2009/05/30 Kino Changed 不要のためコメント
        ''For iloop = 0 To GrpProp.GraphCount - 1
        ''    scaleDef(iloop).SelectedIndex = (CInt(GrpProp.DrawInfo(iloop).DefaultSet) + 1)  '既定か入力か
        ''    If GrpProp.DrawInfo(iloop).DefaultSet = True Then
        ''        Defval(iloop).SelectedIndex = GrpProp.DrawInfo(iloop).DefaultValue              '既定値の場合のリストボックスの項目インデックス
        ''    End If
        ''    scaleMin(iloop).Text = GrpProp.DrawInfo(iloop).Min
        ''    scaleMax(iloop).Text = GrpProp.DrawInfo(iloop).Max
        ''Next iloop
        Return retVal

    End Function

    Protected Sub GetMostUpdate()
        ''
        '' 使用しているデータファイルの中から、一番最新のデータ日付を取得する
        ''
        Dim iloop As Integer
        Dim dteSession As Date

        For iloop = 0 To GrpProp.DrawInfo.Length - 1
            dteSession = CType(Session.Item("LastUpdate" + GrpProp.DrawInfo(iloop).DataFileNo.ToString), Date)
            If GrpProp.LastUpdate < dteSession Then
                GrpProp.LastUpdate = dteSession
            End If
        Next

    End Sub

    Protected Sub SetObj2Arg()
        ''
        '' オブジェクトを変数へ設定
        ''
        Dim iloop As Integer

        'オブジェクトの指定: グラフコントロール：スケール設定リストボックス : スケール既定・入力
        For iloop = 0 To 6
            TxtDates(iloop) = CType(FindControl("TxtDate" + iloop.ToString), TextBox)
            imgDates(iloop) = CType(FindControl("imgCalTxtDate" + iloop.ToString), System.Web.UI.WebControls.Image)
        Next

        For iloop = 0 To 3
            Grps(iloop) = CType(FindControl("WC" + (iloop + 1).ToString), C1WebChart)
            GrpBox(iloop) = CType(FindControl("PnlGraph" + (iloop + 1).ToString), Panel)
            GrpTitleLabel(iloop) = CType(FindControl("LblGraphTitle" + (iloop + 1).ToString), System.Web.UI.WebControls.Label)
            SetPanel(iloop) = CType(FindControl("Pnl" + (iloop + 1).ToString), Object)      'Panel)
            LabelNo(iloop) = CType(FindControl("LblNo" + (iloop + 1).ToString), System.Web.UI.WebControls.Label)
            Defval(iloop) = CType(FindControl("DdlScale" + (iloop + 1).ToString), DropDownList)
            scaleDef(iloop) = CType(FindControl("RBList" + (iloop + 1).ToString), RadioButtonList)
            scaleMin(iloop) = CType(FindControl("TxtMin" + (iloop + 1).ToString), TextBox)
            scaleMax(iloop) = CType(FindControl("TxtMax" + (iloop + 1).ToString), TextBox)
        Next

        ''TxtDates(0) = Me.TxtDate0 : imgDates(0) = Me.ImgDate0 'imgGetTime(0) = Me.ImgBtn0 : DDLTimes(0) = Me.DDLTime0
        ''TxtDates(1) = Me.TxtDate1 : imgDates(1) = Me.ImgDate1 'imgGetTime(1) = Me.ImgBtn1 : DDLTimes(1) = Me.DDLTime1
        ''TxtDates(2) = Me.TxtDate2 : imgDates(2) = Me.ImgDate2 'imgGetTime(2) = Me.ImgBtn2 : DDLTimes(2) = Me.DDLTime2
        ''TxtDates(3) = Me.TxtDate3 : imgDates(3) = Me.ImgDate3 'imgGetTime(3) = Me.ImgBtn3 : DDLTimes(3) = Me.DDLTime3
        ''TxtDates(4) = Me.TxtDate4 : imgDates(4) = Me.ImgDate4 'imgGetTime(4) = Me.ImgBtn4 : DDLTimes(4) = Me.DDLTime4
        ''TxtDates(5) = Me.TxtDate5 : imgDates(5) = Me.ImgDate5 'imgGetTime(5) = Me.ImgBtn5 : DDLTimes(5) = Me.DDLTime5
        ''TxtDates(6) = Me.TxtDate6 : imgDates(6) = Me.ImgDate6 'imgGetTime(6) = Me.ImgBtn6 : DDLTimes(6) = Me.DDLTime6

    End Sub


    Protected Sub SetCalenderFunction()
        ''
        '' テキストボックスとイメージにカレンダー機能を割り当てる
        ''
        Dim iloop As Integer

        For iloop = 0 To 6
            TxtDates(iloop).Attributes.Clear()
            imgDates(iloop).Attributes.Clear()

            TxtDates(iloop).Visible = True
            imgDates(iloop).Visible = True

            imgDates(iloop).Attributes.Add("onclick", "DropCalendar(this);")
        Next
        'imgGetTime(iloop).Visible = True
        'DDLTimes(iloop).Visible = True
        ''rangeValid(iloop).Visible = True
        ''TxtDates(iloop).Attributes.Add("onblur", "validateDate(this);")
    End Sub

    Public Sub SetBIND2Controls(ByVal dbCon As OleDb.OleDbConnection)
        ''
        ''ドロップダウンリストボックスにおける各設定のデータセットバインド
        ''
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
        ''Dim Defval(11) As DropDownList
        Dim iloop As Integer

        'オブジェクトの指定
        ''Defval(0) = Me.DdlScale1          ''モジュール内変数とした
        ''Defval(1) = Me.DdlScale2
        ''Defval(2) = Me.DdlScale3
        ''Defval(3) = Me.DdlScale4

        strSQL = "SELECT * FROM set_スケール WHERE 有効 = True ORDER BY ID"
        DtSet = New DataSet("DData")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        For iloop = 0 To 3          '0 To 3
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

    Public Sub SetInfo2FormControl()
        ''
        '' DBから読み込んだ内容をフォームのコントロールに情報を記載する
        ''
        Dim iloop As Integer

        ' ''オブジェクトの指定: グラフコントロール：スケール設定リストボックス : スケール既定・入力　Privateとした
        ''defVal(0) = Me.DdlScale1 : scaleDef(0) = Me.RBList1 : scaleMin(0) = Me.TxtMin1 : scaleMax(0) = Me.TxtMax1
        ''defVal(1) = Me.DdlScale2 : scaleDef(1) = Me.RBList2 : scaleMin(1) = Me.TxtMin2 : scaleMax(1) = Me.TxtMax2
        ''defVal(2) = Me.DdlScale3 : scaleDef(2) = Me.RBList3 : scaleMin(2) = Me.TxtMin3 : scaleMax(2) = Me.TxtMax3
        ''defVal(3) = Me.DdlScale4 : scaleDef(3) = Me.RBList4 : scaleMin(3) = Me.TxtMin4 : scaleMax(3) = Me.TxtMax4

        ''■表示一般設定
        Me.DdlDateFormat.SelectedValue = GrpProp.DateFormat.ToString                            '日付フォーマット
        Me.DdlEnableLegend.SelectedIndex = Math.Abs(CInt(GrpProp.EnableLegend))                 '凡例表示　0:しない 1:する
        Me.DdlPaperOrientation.SelectedIndex = GrpProp.PaperOrientaion                          '用紙の向き
        Me.DdlTitlePosition.SelectedIndex = GrpProp.TitlePosition                               'グラフタイトル位置
        Me.DdlContinous.SelectedIndex = Math.Abs(CInt(GrpProp.MissingDataContinuous))           '欠測データの連結
        Me.DDLPaintWarningValue.SelectedIndex = Math.Abs(CInt(GrpProp.AlertZoneShow))           '警報値表示

        ''■データスケール設定
        For iloop = 0 To GrpProp.GraphCount - 1
            scaleDef(iloop).SelectedIndex = (CInt(GrpProp.DrawInfo(iloop).DefaultSet) + 1)  '既定か入力か
            If GrpProp.DrawInfo(iloop).DefaultSet = True Then
                defVal(iloop).SelectedIndex = GrpProp.DrawInfo(iloop).DefaultValue              '既定値の場合のリストボックスの項目インデックス
            End If
            scaleMin(iloop).Text = GrpProp.DrawInfo(iloop).Min
            scaleMax(iloop).Text = GrpProp.DrawInfo(iloop).Max
        Next iloop

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

    ''    System.Diagnostics.Debuglo.WriteLine(TextObj.Text)

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


    Protected Sub C1WebCalendar_DisplayDateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles C1WebCalendar.DisplayDateChanged
        ''
        '' カレンダーコントロールの選択 年／月 を変えた時のイベント
        ''

    End Sub

    Protected Sub C1WebCalendar_SelectedDatesChanged(sender As Object, e As System.EventArgs) Handles C1WebCalendar.SelectedDatesChanged

        ''
        '' カレンダーコントロールから日付を選択した時に該当日付の測定時刻をリストボックスに記載する
        ''
        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim iloop As Integer
        Dim SelDate As Date
        Dim strSQL As String
        Dim siteDirectory As String = CType(Session.Item("SD"), String)                     '現場ディレクトリ

        ''siteDirectory = "totsuka"               '◆◆◆◆◆ デバッグ用
        SelDate = C1WebCalendar.SelectedDate
        C1WebCalendar.DisplayDate = SelDate

        ''Me.LblStatus.Text = "選択されている日付：" + SelDate
        Using DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Server.MapPath(siteDirectory + "\App_Data\" + DataFileNames(GrpProp.DrawInfo(iloop).DataFileNo).FileName) + ";" + "Jet OLEDB:Engine Type= 5")
            ''''DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=I:\WebDataSystem2\totsuka\App_Data\totsuka_CalculatedData.mdb" & ";" & "Jet OLEDB:Engine Type= 5"       '◆◆◆◆◆ デバッグ用
            ''DbCon.Open()

            ''strSQL = "SELECT DISTINCT FORMAT(日付, 'HH:mm:ss') AS 測定時刻,日付 FROM 日付 WHERE (日付 BETWEEN #" & SelDate.ToString("yyyy/MM/dd 0:00:00") & "# AND #" & SelDate.ToString("yyyy/MM/dd 23:59:59") & "#) ORDER BY 日付 ASC"
            strSQL = ("SELECT DISTINCT FORMAT(日付, 'HH:mm') AS 測定時刻,日付.日付 FROM 日付 WHERE (日付 BETWEEN #" + SelDate.ToString("yyyy/MM/dd 0:00:00") + "# AND #" + SelDate.ToString("yyyy/MM/dd 23:59:59") & "#) ORDER BY 日付 ASC")

            Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                Using DtSet As New DataSet("DData")
                    DbDa.Fill(DtSet, "DData")

                    ''DbCon.Close()
                    'DbCon.Dispose()

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

                    'DbCom.Dispose()
                    'DbDa.Dispose()
                End Using
            End Using
        End Using

    End Sub

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
End Class

