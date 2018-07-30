Option Strict On
Option Explicit On

Imports System.Data
Imports System.Data.OleDb

''' <summary>
''' コードビハインドは、初期設定をデータベースから読み込むことがメインで、操作に関するところは、Javascriptで実施
''' そのため、汎用性を持たせて作ってあったが、不要なところはコメントした
''' </summary>
''' <remarks></remarks>
Partial Class Graph_HSSTimeChart

    Inherits System.Web.UI.Page

    'グラフデータの情報
    Private Structure LineProperties
        Dim DataCount As Short                  'データ系列数
        Dim DataCh As String                    '描画データ表示チャンネル番号
        Dim Symbol As String                    '記号
        Dim LineColor As String                 '線色
        Dim LineType As String                  '線種
        Dim LineWidth As String                 '線幅
        Dim MarkSize As Short                   'マーカサイズ
        Dim Marker As String                    'マーカ
'        Dim MarkOutlineWidth As Short           'マーカアウトライン幅　これが0でない場合は白抜きとする　　0なら塗りつぶし
    End Structure

    '軸の情報
    Private Structure AxisPropaerties
        Dim DefaultSet As Boolean               'スケール既定値か入力か
        Dim DefaultValue As Integer             '既定値の場合の値
        'Dim UnitTitle As String                '単位タイトル
        Dim YTitle As String                    'Y軸タイトル
        Dim YTitleSize As Integer               'Y軸タイトルサイズ
        Dim YTitleColor As Integer              'Y軸タイトル色
        Dim YTickLabelSize As Integer           'Y軸スケール文字サイズ
        Dim YTickLabelColor As Integer          'Y軸スケール文字色
        Dim YMax As Single                      'Y軸最大値
        Dim YMin As Single                      'Y軸最小値
        Dim YUnitMajor As Single                'Y軸主メモリ間隔
        Dim YUnitMinor As Single                'Y軸副メモリ間隔
        Dim ChartType As String                 'グラフ種別
        Dim XTitle As String                    'X軸タイトル
        Dim XTitleSize As Integer               'X軸タイトルサイズ
        Dim XTitleColor As Integer              'X軸タイトル色
        Dim XTickLabelSize As Integer           'X軸スケール文字サイズ
        Dim XTickLabelColor As Integer          'X軸スケール文字色
        Dim XMax As Single                      'X軸最大値
        Dim XMin As Single                      'X軸最小値
        Dim XUnitMajor As Single                'X軸主メモリ間隔
        Dim XUnitMinor As Single                'X軸副メモリ間隔
        Dim Reversed As Boolean                 '軸反転     False:しない　　True:する
        Dim LogAxis As Boolean                  'Y軸(データ軸)の対数スケール　False:しない　True:する      
        Dim YLabelFormat As String              'Y軸スケールのフォーマット文字列                           
        Dim XLabelFormat As String              'X軸スケールのフォーマット文字列                           
        Dim XLabelAngle As Integer              'X軸ラベルの回転角度
        Dim IndividualArertValue As String      '個別警報値
        Dim ShowAlert As Boolean                '警報表示
        Dim AlertType As Integer                '警報表示種別
        Dim AlertLineWidth As Integer           '警報線幅

    End Structure

    'グラフ描画における個々のグラフ情報
    Private Structure GrpPersonalizedInfo
        Dim LeftAxisData As LineProperties      'グラフ描画における線情報
        Dim LeftAxis As AxisPropaerties         '左軸設定   
        'Dim RightAxisData() As LineProperties   '右軸グラフ描画における線情報
        'Dim SubTitle As String                  'サブタイトル
        'Dim EnableRightAxis As Boolean          '右軸有効
        'Dim RightAxis As AxisPropaerties        '右軸設定
        'Dim DataFileNo As Integer               'データファイル番号
    End Structure

    'グラフ描画全般の情報
    Private Structure GrpInfo
        Dim LineInfo() As GrpPersonalizedInfo   'グラフにおける線の情報

        Dim DrawSizeColorInf() As Integer       '全般
        '--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--

        Dim StartDate As Date                   'グラフ描画開始日
        Dim EndDate As Date                     'グラフ描画終了日
        Dim LastUpdate As Date                  'データの最終更新日
        Dim WindowTitle As String               'タイトルバーのタイトル

        Dim GraphCount As Integer               '描画グラフ数
        Dim GraphBoxCount As Integer            'グラフの枠数
        Dim GraphTitle As String                'グラフタイトル（ページタイトル　　以降　グラフ->ページ
        Dim TitlePosition As Short              'グラフタイトルの位置(上or下)
        Dim TimeScale As String                 'X軸表示期間
        Dim DateFormat As String                '日付フォーマット
        'Dim XAxisTileAngle As Integer           'X軸タイトルの角度
        Dim PaperOrientaion As Integer          '用紙の向き
        Dim GraphTitleSize As Integer           'グラフタイトルサイズ
        Dim GraphTitleColor As Integer          'グラフタイトルカラー
        Dim LegendSize As Integer               '凡例の文字サイズ
        Dim LegendColor As Integer              '凡例の文字サイズ
        Dim TickLength As Integer               '目盛り線長さ
        Dim TickColor As Integer                '目盛り線色
        Dim ShowLegend As Boolean               '凡例の表示／非表示  1/0
        Dim MissingDataContinuous As Integer    '欠測データの連結
        Dim ShowAlarmArea As Integer            '警報値の表示
        Dim thinoutEnable As Boolean            '間引きチェックの有無
        Dim thinoutCh As String                 '間引きチャンネル情報
        Dim CSVFilePath As String               ' CSVファイルのパス
        Dim SensorCount As Integer              ' センサー台数

    End Structure

    Private GrpProp As GrpInfo                  'グラフ描画に関する情報

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

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"

        If User.Identity.IsAuthenticated = False Then
            'Response.Redirect("Login.aspx")
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer
        LoginStatus = CType(Session.Item("LgSt"), Integer)          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            'Response.Redirect("Login.aspx")
            Response.Write(strScript)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする

        Dim siteName As String = Nothing
        Dim siteDirectory As String = Nothing
        Dim grpDataFile As String = Nothing
        Dim intRet As Integer

        siteName = CType(Session.Item("SN"), String)                            '現場名
        siteDirectory = CType(Session.Item("SD"), String)                       '現場ディレクトリ
        grpDataFile = Server.MapPath(siteDirectory & "\App_Data\HSSTimeChartGraphInfo.mdb")
        GrpProp.WindowTitle = Request.Item("GNa").ToString                      'グラフタイトル（ウィンドウタイトル）

        'Dim GrpName() As String = Me.DDLSelectGraph.Text.Split(Convert.ToChar(","))

        Dim LastUpdate As DateTime = GetMaxDate("CSVDate.mdb")                               ' CSVデータの最新値を取得
        If LastUpdate.Year > 2000 Then
            Me.LblLastUpdate.Text = String.Format("最新データ日時：{0}", LastUpdate.ToString("yyyy/MM/dd HH:mm:ss"))
        Else
            Me.LblLastUpdate.Text = String.Format("最新データ日時：{0}", "--")
        End If

        Using DbCon As New OleDbConnection

            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & grpDataFile & ";" & "Jet OLEDB:Engine Type= 5")

            intRet = ReadGraphInfoFromDB(DbCon, "高頻度履歴図")                     'グラフ情報を取得
            intRet = GraphLineInfoFromDB(DbCon)                                     'グラフの軸タイトル、最大・最小スケールなどを取得
            Dim strRet As String = ReadCSVFilePath(DbCon)                           'CSV保存フォルダの取得
            If strRet IsNot Nothing Then
                GrpProp.CSVFilePath = IO.Path.Combine(strRet, siteDirectory)
                Session.Add("csvpath", GrpProp.CSVFilePath)
            End If

            If IO.Directory.Exists(GrpProp.CSVFilePath) = True Then

                Call SetBIND2Controls(DbCon)

            End If

            If IsPostBack = False Then
                intRet = ReadSensorInfoFromDB(DbCon)                                    ' センサー番号を取得し、DDLに登録する
            Else

            End If
        End Using

        ''Me.litG1.Text = "<button id='printChart' class='prn' width='32' width='22'><img src='./img/printer.png' alt='印刷' align='middle'></button><div id='cCtnr1' class='wavegraph'></div>"                             'style='height: 200px; width: 100%;'></div>"
        Me.litG1.Text = "<div id='cCtnr1' class='wavegraph'></div>"                             'style='height: 200px; width: 100%;'></div>"
        Me.litG2.Text = "<div id='cCtnr2' class='wavegraph'></div>"
        Me.litG3.Text = "<div id='cCtnr3' class='wavegraph'></div>"

        ''Dim OpenString As String
        'If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then

        '    OpenString = "" '"<script type='text/javascript'>window.onload = function() {var chart = new CanvasJS.Chart('cCtnr1', {title: {text: 'Line Chart'},axisX: {interval: 10},data: [{type: 'line',dataPoints: [  { x: 10, y: 45 },  { x: 20, y: 14 },  { x: 30, y: 20 },  { x: 40, y: 60 },  { x: 50, y: 50 },  { x: 60, y: 80 },  { x: 70, y: 40 },  { x: 80, y: 60 },  { x: 90, y: 10 },  { x: 100, y: 50 },  { x: 110, y: 40 },  { x: 120, y: 14 },  { x: 130, y: 70 },  { x: 140, y: 40 },  { x: 150, y: 90 },]}]});chart.render();"
        '    OpenString &= "var chart1 = new CanvasJS.Chart('cCtnr2', {title: {text: 'Line Chart'},axisX: {interval: 10},data: [{type: 'line',dataPoints: [  { x: 10, y: 45 },  { x: 20, y: 14 },  { x: 30, y: 20 },  { x: 40, y: 60 },  { x: 50, y: 50 },  { x: 60, y: 80 },  { x: 70, y: 40 },  { x: 80, y: 60 },  { x: 90, y: 10 },  { x: 100, y: 50 },  { x: 110, y: 40 },  { x: 120, y: 14 },  { x: 130, y: 70 },  { x: 140, y: 40 },  { x: 150, y: 90 },]}]});chart1.render();"
        '    OpenString &= "var chart2 = new CanvasJS.Chart('cCtnr3', {title: {text: 'Line Chart'},axisX: {interval: 10},data: [{type: 'line',dataPoints: [  { x: 10, y: 45 },  { x: 20, y: 14 },  { x: 30, y: 20 },  { x: 40, y: 60 },  { x: 50, y: 50 },  { x: 60, y: 80 },  { x: 70, y: 40 },  { x: 80, y: 60 },  { x: 90, y: 10 },  { x: 100, y: 50 },  { x: 110, y: 40 },  { x: 120, y: 14 },  { x: 130, y: 70 },  { x: 140, y: 40 },  { x: 150, y: 90 },]}]});chart2.render();}</script>"

        '    Page.ClientScript.RegisterStartupScript(Me.GetType(), "高頻度履歴図", OpenString)
        'End If

        'Me.LblTitleLower.Text = "【振動波形データ】"
        Me.LblTitleLower.Font.Size = GrpProp.GraphTitleSize
        Me.LblTitleLower.ForeColor = System.Drawing.Color.FromArgb(GrpProp.GraphTitleColor)

    End Sub

    ''' <summary>
    ''' CSVの保存ルートパスを取得
    ''' </summary>
    ''' <param name="DbCon">OleDbConnection</param>
    ''' <returns>成否　　成功：グラフ数　否：0</returns>
    ''' <remarks></remarks>
    Protected Function ReadCSVFilePath(ByVal dbcon As OleDbConnection) As String

        Dim strRet As String = Nothing
        Dim strSQL As String
        Dim DbDa As OleDbDataAdapter = Nothing
        Dim DtSet As New DataSet("DData")
        Dim GraphCount As Integer = 0

        strSQL = ("SELECT * FROM CSVFolder ORDER BY ID ASC")


        Try
            DbDa = New OleDb.OleDbDataAdapter(strSQL, dbcon)
            DbDa.Fill(DtSet, "DData")

            If DtSet.Tables("DData").Rows.Count > 0 Then
                strRet = DtSet.Tables("DData").Rows(0).Item(1).ToString
            End If

        Catch ex As Exception

        Finally
            DbDa.Dispose()
            DtSet.Dispose()
        End Try

        Return strRet

    End Function

    Public Sub SetBIND2Controls(ByVal dbCon As OleDb.OleDbConnection)
        ''
        ''ドロップダウンリストボックスにおける各設定のデータセットバインド
        ''
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String

        ''■■スケール設定
        Dim Defval(2) As DropDownList
        Dim iloop As Integer

        'オブジェクトの指定
        Defval(0) = Me.DdlScale1
        ''Defval(1) = Me.DdlScale2
        ''Defval(2) = Me.DdlScale3


        strSQL = "SELECT * FROM set_スケール WHERE 有効 = True ORDER BY ID"
        'DtSet = New DataSet("DData")
        Using DbDa As New OleDb.OleDbDataAdapter(strSQL, dbCon)
            Using DtSet As New DataSet("DData")

                DbDa.Fill(DtSet, "DData")

                For iloop = 0 To 0
                    With Defval(iloop)
                        .DataSource = DtSet.Tables("DData")
                        .DataMember = DtSet.DataSetName
                        .DataTextField = "表示"
                        .DataValueField = "値"
                        .DataBind()
                    End With
                Next iloop

                'DbDa.Dispose()
                DtSet.Dispose()

            End Using
        End Using

        Me.TxtMax1.Text = GrpProp.LineInfo(0).LeftAxis.YMax.ToString
        Me.DdlScale1.SelectedIndex = GrpProp.LineInfo(0).LeftAxis.DefaultValue
        ''Me.TxtMax2.Text = GrpProp.LineInfo(1).LeftAxis.YMax.ToString
        ''Me.TxtMax3.Text = GrpProp.LineInfo(2).LeftAxis.YMax.ToString

        Me.TxtMin1.Text = GrpProp.LineInfo(0).LeftAxis.YMin.ToString
        ''Me.TxtMin2.Text = GrpProp.LineInfo(1).LeftAxis.YMin.ToString
        ''Me.TxtMin3.Text = GrpProp.LineInfo(2).LeftAxis.YMin.ToString

        If GrpProp.LineInfo(0).LeftAxis.DefaultSet = True Then
            Me.RdbNo11.Checked = True
            Me.RdbNo12.Checked = False
        Else
            Me.RdbNo11.Checked = False
            Me.RdbNo12.Checked = True
        End If

        ''If GrpProp.LineInfo(1).LeftAxis.DefaultSet = True Then
        ''    Me.RdbNo21.Checked = True
        ''    Me.RdbNo22.Checked = False
        ''Else
        ''    Me.RdbNo21.Checked = False
        ''    Me.RdbNo22.Checked = True
        ''End If

        ''If GrpProp.LineInfo(2).LeftAxis.DefaultSet = True Then
        ''    Me.RdbNo31.Checked = True
        ''    Me.RdbNo32.Checked = False
        ''Else
        ''    Me.RdbNo31.Checked = False
        ''    Me.RdbNo32.Checked = True
        ''End If

    End Sub

    ''' <summary>
    ''' データ描画に関する情報の取得
    ''' </summary>
    ''' <param name="DbCon">OleDbConnection</param>
    ''' <returns>成否　　成功：グラフ数　否：0</returns>
    ''' <remarks></remarks>
    Protected Function GraphLineInfoFromDB(ByVal DbCon As OleDbConnection) As Integer

        Dim intRet As Integer = 0
        Dim strSQL As String
        'Dim DbDa As OleDbDataAdapter = Nothing
        'Dim DtSet As New DataSet("DData")
        Dim GraphCount As Integer = 0

        strSQL = ("SELECT * FROM グラフ情報 WHERE " & String.Format("項目名 = '{0}' ORDER BY グラフNo ASC", GrpProp.WindowTitle))


        Try
            Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                Using DtSet As New DataSet("DData")

                    DbDa.Fill(DtSet, "DData")

                    For Each DTR As DataRow In DtSet.Tables("DData").Rows

                        GrpProp.LineInfo(GraphCount).LeftAxis.DefaultSet = Convert.ToBoolean(DTR.Item(3))           'スケール規定
                        GrpProp.LineInfo(GraphCount).LeftAxis.DefaultValue = Convert.ToInt32(DTR.Item(4))           'スケール規定値番号
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.YTitle = DTR.Item(5).ToString                         'Y軸タイトル
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.YTitleSize = Convert.ToInt32(DTR.Item(6))             'Y軸タイトルサイズ
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.YTitleColor = Convert.ToInt32(DTR.Item(7))            'Y軸タイトル色
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.YTickLabelSize = Convert.ToInt32(DTR.Item(8))         'Y軸スケール文字サイズ
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.YTickLabelColor = Convert.ToInt32(DTR.Item(9))        'Y軸スケール文字色
                        GrpProp.LineInfo(GraphCount).LeftAxis.YMax = Convert.ToSingle(DTR.Item(10))                 'Y軸最大値
                        GrpProp.LineInfo(GraphCount).LeftAxis.YMin = Convert.ToSingle(DTR.Item(11))                 'Y軸最大小
                        GrpProp.LineInfo(GraphCount).LeftAxis.YUnitMajor = Convert.ToSingle(DTR.Item(12))           'Y軸主メモリ間隔
                        GrpProp.LineInfo(GraphCount).LeftAxis.YUnitMinor = Convert.ToSingle(DTR.Item(13))           'Y軸副メモリ間隔
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.ChartType = DTR.Item(16).ToString                     'グラフ種別
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XTitle = DTR.Item(17).ToString                        'X軸タイトル
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XTitleSize = Convert.ToInt32(DTR.Item(18))            'X軸タイトルサイズ
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XTitleColor = Convert.ToInt32(DTR.Item(19))           'X軸タイトル色
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XTickLabelSize = Convert.ToInt32(DTR.Item(20))        'X軸スケール文字サイズ
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XTickLabelColor = Convert.ToInt32(DTR.Item(21))       'X軸スケール文字色
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.Reversed = Convert.ToBoolean(DTR.Item(22))            '軸反転     False:しない　　True:する
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.LogAxis = Convert.ToBoolean(DTR.Item(23))             'Y軸(データ軸)の対数スケール　False:しない　True:する     
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.YLabelFormat = DTR.Item(24).ToString                  'Y軸スケールのフォーマット文字列
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XLabelFormat = DTR.Item(25).ToString                  'X軸スケールのフォーマット文字列
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XLabelAngle = Convert.ToInt32(DTR.Item(26))           'X軸ラベルの回転角度
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.IndividualArertValue = DTR.Item(27).ToString          '個別警報値
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XMax = Convert.ToSingle(DTR.Item(28))                 'X軸最大値
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XMin = Convert.ToSingle(DTR.Item(29))                 'X軸最小値
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XUnitMajor = Convert.ToSingle(DTR.Item(30))           'X軸主メモリ間隔
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.XUnitMinor = Convert.ToSingle(DTR.Item(31))           'X軸副メモリ間隔
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.ShowAlert = Convert.ToBoolean(DTR.Item(32))           '警報表示
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.AlertType = Convert.ToInt32(DTR.Item(33))             '警報表示種別
                        ''GrpProp.LineInfo(GraphCount).LeftAxis.AlertLineWidth = Convert.ToInt32(DTR.Item(34))        '警報線幅

                        ''GrpProp.LineInfo(GraphCount).LeftAxisData.DataCount = Convert.ToInt16(DTR.Item(14))         'データ系列数
                        ''GrpProp.LineInfo(GraphCount).LeftAxisData.DataCh = DTR.Item(15).ToString                    '描画データ表示チャンネル番号   カンマ区切りでデータ系列数分入る
                        ''GrpProp.LineInfo(GraphCount).LeftAxisData.Symbol = DTR.Item(35).ToString                    '記号
                        ''GrpProp.LineInfo(GraphCount).LeftAxisData.LineColor = DTR.Item(36).ToString                 '線色
                        ''GrpProp.LineInfo(GraphCount).LeftAxisData.LineType = DTR.Item(37).ToString                  '線種
                        ''GrpProp.LineInfo(GraphCount).LeftAxisData.LineWidth = DTR.Item(38).ToString                 '線幅
                        ''GrpProp.LineInfo(GraphCount).LeftAxisData.MarkSize = Convert.ToInt16(DTR.Item(39))          'マーカサイズ
                        ''GrpProp.LineInfo(GraphCount).LeftAxisData.Marker = DTR.Item(40).ToString                    'マーカ種別

                        GraphCount += 1
                    Next

                    intRet = GraphCount
                End Using
            End Using
        Catch ex As Exception
            'Finally
            '    DbDa.Dispose()
            '    DtSet.Dispose()
        End Try

        Return intRet

    End Function

    ''' <summary>
    ''' センサー番号のDropDownListboxに、センサー番号を登録する
    ''' </summary>
    ''' <param name="DbCon">OleDbConnection</param>
    ''' <returns>成否　　成功：センサー台数　否：0</returns>
    ''' <remarks></remarks>
    Protected Function ReadSensorInfoFromDB(ByVal DbCon As OleDbConnection) As Integer

        Dim intRet As Integer = 0
        Dim strSQL As String
        Dim DbDa As OleDbDataAdapter = Nothing
        Dim DtSet As New DataSet("DData")

        strSQL = ("SELECT ID,センサー記号 FROM センサー情報 WHERE 有効 = true ORDER BY ID")

        Try
            DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
            DbDa.Fill(DtSet, "DData")

            Me.DDLSensor.Items.Clear()

            For Each DTR As DataRow In DtSet.Tables("DData").Rows
                Me.DDLSensor.Items.Add(DTR.Item(1).ToString)
            Next

            intRet = DtSet.Tables("DData").Rows.Count
            Session.Add("SensorCount", intRet)

        Catch ex As Exception

        Finally
            DbDa.Dispose()
            DtSet.Dispose()

        End Try

        Return intRet

    End Function

    ''' <summary>
    ''' データベースから、ページにおけるグラフの情報を取得する
    ''' </summary>
    ''' <param name="DbCon">OleDbConnection</param>
    ''' <param name="GraphType">グラフ種別</param>
    ''' <returns>成否　　成功：センサー台数　否：0</returns>
    ''' <remarks></remarks>
    Protected Function ReadGraphInfoFromDB(ByVal DbCon As OleDbConnection, ByVal GraphType As String) As Integer

        Dim intRet As Integer = 0
        Dim strSQL As String
        'Dim DbDa As OleDbDataAdapter = Nothing
        'Dim DtSet As New DataSet("DData")
        ''Dim iloop As Integer
        ''ReDim GrpProp.DrawSizeColorInf(23)                      'グラフの各部分のフォントサイズなどは固定の配列

        strSQL = ("SELECT * FROM メニュー基本情報 WHERE " & String.Format("(項目名 = '{0}' AND 種別= '{1}')", GrpProp.WindowTitle, GraphType))

        Try
            Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                Using DtSet As New DataSet("DData")
                    DbDa.Fill(DtSet, "DData")

                    With DtSet.Tables("DData")
                        If .Rows.Count > 0 Then
                            GrpProp.GraphCount = Convert.ToInt32(.Rows(0).Item(4))              'グラフの数
                            ''GrpProp.GraphBoxCount = Convert.ToInt32(.Rows(0).Item(5))           'グラフ枠の数(パネル)
                            GrpProp.GraphTitle = .Rows(0).Item(6).ToString                      'グラフのタイトル（ページタイトル　以降 グラフ->ページ）
                            ''GrpProp.TitlePosition = Convert.ToInt16(.Rows(0).Item(7))           'タイトルの位置(上or下)
                            ''GrpProp.TimeScale = .Rows(0).Item(8).ToString                       '横軸の時間軸スケール
                            ''GrpProp.DateFormat = .Rows(0).Item(10).ToString                     '日付の表示フォーマット
                            ' ''GrpProp.XAxisTileAngle = Convert.ToInt32(.Rows(0).Item(11))         '横軸のメモリラベルの角度
                            ''GrpProp.PaperOrientaion = Convert.ToInt32(.Rows(0).Item(11))        '紙の向き
                            GrpProp.GraphTitleSize = Convert.ToInt32(.Rows(0).Item(12))         'グラフタイトルサイズ
                            GrpProp.GraphTitleColor = Convert.ToInt32(.Rows(0).Item(13))        'グラフタイトル色
                            ''GrpProp.LegendSize = Convert.ToInt32(.Rows(0).Item(14))             '凡例フォントサイズ
                            ''GrpProp.LegendColor = Convert.ToInt32(.Rows(0).Item(15))            '凡例フォント色
                            ''GrpProp.TickLength = Convert.ToInt32(.Rows(0).Item(16))             '目盛り線長さ
                            ''GrpProp.TickColor = Convert.ToInt32(.Rows(0).Item(17))              '目盛り線色
                            ''GrpProp.ShowLegend = Convert.ToBoolean(.Rows(0).Item(18))           '凡例の有無
                            ''GrpProp.MissingDataContinuous = Convert.ToInt32(.Rows(0).Item(19))  '欠測データの連結
                            ''GrpProp.ShowAlarmArea = Convert.ToInt32(.Rows(0).Item(20))          '警報値表示
                            ''GrpProp.thinoutEnable = Convert.ToBoolean(.Rows(0).Item(21))        '間引きチェックの有無
                            ''GrpProp.thinoutCh = .Rows(0).Item(22).ToString                      '間引きチャンネル情報 カンマ区切り

                            ''For iloop = 13 To 35
                            ''    GrpProp.DrawSizeColorInf(iloop - 13) = Convert.ToInt32(.Rows(0).Item(iloop))  'グラフの各部分のフォントサイズなど(詳細は下を参照)
                            ''Next iloop
                        End If
                    End With

                    ReDim GrpProp.LineInfo(GrpProp.GraphCount - 1)          '個々のグラフの情報を格納する配列設定(グラフ個数分)

                    intRet = 1
                End Using
            End Using
        Catch ex As Exception

            'Finally
            '    DbDa.Dispose()
            '    DtSet.Dispose()
        End Try

        Return intRet

    End Function



    ''' <summary>
    ''' C指定されたデータベースの最新日時を取得
    ''' </summary>
    ''' <param name="FileName">データベースのファイル名</param>    
    ''' <returns>最新日時</returns>
    ''' <remarks>「日付」フィールドの最大値を取得する</remarks>
    Protected Function GetMaxDate(ByVal FileName As String) As DateTime

        Dim clsDate As New ClsReadDataFile
        Dim DBPath As String = Nothing
        Dim siteDirectory As String
        Dim LastUpdate As DateTime

        Try
            siteDirectory = Session.Item("SD").ToString

            DBPath = Server.MapPath(siteDirectory & "\App_Data\" & FileName)

            LastUpdate = clsDate.GetHSSDataLastUpdate(DBPath)

        Catch ex As Exception

        End Try

        Return LastUpdate

    End Function

    Protected Sub DDLHMin_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DDLHMin.SelectedIndexChanged

    End Sub

    ''''' <summary>
    ''''' [イベント]　ボタン押下　「波形表示」
    ''''' </summary>
    ''''' <param name="sender"></param>
    ''''' <param name="e"></param>
    ''''' <remarks></remarks>
    ''Protected Sub btnDrawData_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDrawData.Click

    ''    Dim DataRead As New ClsCSVData
    ''    Dim YM As String = Me.DDLYMon.Text
    ''    Dim DT As String = Me.DDLDay.Text
    ''    Dim HM As String = Me.DDLHMin.Text
    ''    Dim SiteDirectory As String = Nothing
    ''    Dim sb0 As New StringBuilder
    ''    Dim sb1 As New StringBuilder
    ''    Dim sb2 As New StringBuilder
    ''    Dim DataString0 As String = Nothing
    ''    Dim DataString1 As String = Nothing
    ''    Dim DataString2 As String = Nothing

    ''    SiteDirectory = CType(Session.Item("SD"), String)                       '現場ディレクトリ

    ''    ''Me.LblTitleLower.Text = String.Format("【{0} 測点 振動波形データ】", Me.DDLSensor.Text)
    ''    ''Me.LblDateSpan.InnerText = String.Format("データ日時：{0} {1} {2}", YM, DT, HM)


    ''    Dim selDate As DateTime = DateTime.Parse(YM & DT & HM)
    ''    Dim getDate As String = selDate.ToString("yyyyMMdd_HHmmss'.csv'")
    ''    Dim csvPath As String = IO.Path.Combine(GrpProp.CSVFilePath, getDate)

    ''    Dim iloop As Integer
    ''    Dim strTemp As String = Me.DDLSensor.Text.Replace("No.", "")
    ''    Dim sensorNo As Integer = Convert.ToInt32(strTemp)
    ''    Dim datas As Single(,) = DataRead.ReadDataFromCSVatUdomari(csvPath, sensorNo)           '(Me.DDLSensor.Items.Count * 3))        '全データいらないので

    ''    For iloop = 0 To datas.GetLength(0) - 1
    ''        sb0.Append(String.Format("{{x:{0},y:{1}}},", datas(iloop, 0), datas(iloop, 1)))
    ''        sb1.Append(String.Format("{{x:{0},y:{1}}},", datas(iloop, 0), datas(iloop, 2)))
    ''        sb2.Append(String.Format("{{x:{0},y:{1}}},", datas(iloop, 0), datas(iloop, 3)))
    ''    Next iloop

    ''    DataString0 = sb0.ToString
    ''    DataString1 = sb1.ToString
    ''    DataString2 = sb2.ToString

    ''    sb0.Length = 0                                                           'メモリ解放
    ''    sb1.Length = 0                                                           'メモリ解放
    ''    sb2.Length = 0                                                           'メモリ解放

    ''    DataString0 = DataString0.TrimEnd(Convert.ToChar(","))
    ''    DataString1 = DataString1.TrimEnd(Convert.ToChar(","))
    ''    DataString2 = DataString2.TrimEnd(Convert.ToChar(","))

    ''    DataRead = Nothing

    ''    Dim OpenString As String
    ''    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
    ''        Dim os As New StringBuilder
    ''        ''os.Append("<script type='text/javascript'>window.onload = function() {")
    ''        ''os.Append("var chart  = new CanvasJS.Chart('cCtnr1',{zoomEnabled:true,zoomType:'xy',animationEnabled:true,axisY2:{includeZero:false,lineThickness:2},axisX: {interval:10},data: [{type: 'line',dataPoints: [")

    ''        os.Append("<script type='text/javascript'>$(function() {")
    ''        os.Append("$('#cCtnr1').CanvasJSChart({zoomEnabled:true,zoomType:'xy',animationEnabled:true,axisY2:{includeZero:false,lineThickness:2},axisX: {interval:5,minimum:0,maximum:60},data: [{type: 'line',dataPoints: [")
    ''        os.Append(DataString0)
    ''        os.Append("]},{type:'line',axisYType:'secondary'}]});")        'chart.render();")

    ''        os.Append("$('#cCtnr2').CanvasJSChart({zoomEnabled:true,zoomType:'xy',animationEnabled:true,axisY2:{includeZero:false ,lineThickness:2},axisX: {interval:5,minimum:0,maximum:60},data: [{type: 'line',dataPoints: [")
    ''        os.Append(DataString1)
    ''        os.Append("]},{type:'line',axisYType:'secondary'}]});")      'chart1.render();")

    ''        os.Append("$('#cCtnr3').CanvasJSChart({zoomEnabled:true,zoomType:'xy',animationEnabled:true,axisY2:{includeZero:false,lineThickness:2},axisX: {interval:5,minimum:0,maximum:60},data: [{type: 'line',dataPoints: [")
    ''        os.Append(DataString2)
    ''        os.Append("]},{type:'line',axisYType:'secondary'}]});});</script>")         'chart2.render();}</script>")

    ''        OpenString = os.ToString
    ''        os.Length = 0
    ''        Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "高頻度履歴図", OpenString)
    ''    End If

    ''End Sub

End Class

