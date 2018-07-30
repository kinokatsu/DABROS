Option Explicit On
Option Strict On

''Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.OleDb
Imports System.Web
Imports System.Drawing

Public Structure ShowDataSet
    ''' <summary>
    ''' 表示するラベルの数
    ''' </summary>
    Dim DataCount As Integer
    ''' <summary>
    ''' 表示するデータのファイル番号
    ''' </summary>
    Dim DataFileNo As Integer
    ''' <summary>
    ''' 表示するデータベースのチャンネル番号最大値
    ''' </summary>
    Dim MaxCh As Integer
    ''' <summary>
    ''' 表示するデータベースのチャンネル番号最小値
    ''' </summary>
    Dim MinCh As Integer
    ''' <summary>
    ''' データを読み込むときに使用するSQLクエリ文字列（チャンネル番号）
    ''' </summary>
    Dim SQLCh As String
    ''' <summary>
    ''' CommonInf.mdbから読み込んだデータ
    ''' </summary>
    Dim DataSet() As Array
End Structure

Public Structure OffsetCoordinates
    ''' <summary>
    ''' Leftに対するオフセット値
    ''' </summary>
    Dim X As Single                             'Ｘ座標(Left)
    ''' <summary>
    ''' Topに対するオフセット値
    ''' </summary>
    Dim Y As Single                             'Ｙ座標(Top)
End Structure


Public Class ClsDataSummary
    ''' <summary>
    ''' データ一覧用設定の読込
    ''' </summary>
    ''' <param name="strGraphName">グラフの識別名称</param>
    ''' <param name="SiteDirectory">現場のフォルダ</param>
    ''' <param name="ShowDatas">設定から抽出した値を格納する配列構造体</param>
    ''' <param name="ShowDataInf">データ一覧で表示するための設定</param>
    ''' <param name="LabelOffset">画像に対してラベルの位置を調整するためのオフセット値</param>
    ''' <param name="DataFileCount">データ一覧に表示に使用するデータファイル数</param>
    Public Function ReadSummaryInfo(ByVal strGraphName As String, ByVal siteDirectory As String, _
                                        ByRef showDatas() As ShowDataSet, ByRef showDataInf() As Array, ByRef labelOffset As OffsetCoordinates, _
                                        ByRef dataFileCount As Integer, ByRef picFile As String, ByRef FieldCount As Integer) As Long
        ''
        '' ラベル設定情報、データの読込
        ''
        Dim DBFilePath As String
        'Dim DbCon As New OleDbConnection
        'Dim DbCom As New OleDbCommand
        'Dim DbDa As New OleDbDataAdapter
        'Dim DtSet As New DataSet
        ''Dim DRow As DataRow
        Dim iloop As Integer
        Dim Work As String = ""
        Dim DateFormat As String
        Dim DatePosition As Integer
        Dim intTemp As Integer
        Dim lngTemp As Long = -1        ''戻り値初期値設定 0:正常　-1:ファイルなし 　それ以外:読み込みエラー
        ''Dim ShowDataInf() As Array = {}
        'Dim jloop As Integer
        ''Dim LabelOffsetX As Integer
        ''Dim LabelOffsetY As Integer
        ''Dim ShowDatas() As ShowDataSet
        ''Dim DataFileCount As Integer

        '' ===============【 設定情報を読み込む 】===============(DataSummaryGraphInfo.mdb)
        DBFilePath = IO.Path.Combine(siteDirectory, "App_Data", "DataSummaryGraphInfo.mdb")
        '' DbCon.ConnectionString = ("Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + DBFilePath + ";" + "Jet OLEDB:Engine Type= 5;Jet OLEDB:Database Locking Mode=1") '' Access2007 用
        Using DbCon As New OleDbConnection

            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DBFilePath & ";" & "Jet OLEDB:Engine Type= 5;Jet OLEDB:Database Locking Mode=1")
            ''DbCon.Open()                                                                                              ' 2011/03/15 Kino Changed
            Try
                Work = ("SELECT * FROM メニュー基本情報 WHERE 項目名='" & strGraphName & "'")
                '' 現場名称の読込
                Using DbDa As New OleDbDataAdapter(Work, DbCon)
                    'DbDa = New OleDbDataAdapter(Work, DbCon)
                    Using DtSet As New DataSet("InfData")
                        'DtSet = New DataSet("InfData")
                        DbDa.Fill(DtSet, "InfData")
                        ''DbCon.Close()                                                               '' 2009/08/28 Kino Add        ' 2011/03/15 Kino Changed

                        If DtSet.Tables("InfData").Rows.Count <> 0 Then
                            DateFormat = DtSet.Tables("InfData").Rows(0).Item(6).ToString                   '日付フォーマット
                            DatePosition = Convert.ToInt32(DtSet.Tables("InfData").Rows(0).Item(7))         '日付表示位置
                            labelOffset.X = Convert.ToSingle(DtSet.Tables("InfData").Rows(0).Item(13))      'オフセット値X
                            labelOffset.Y = Convert.ToSingle(DtSet.Tables("InfData").Rows(0).Item(14))      'オフセット値Y
                            If IsDBNull(DtSet.Tables("InfData").Rows(0).Item(15)) = False Then
                                picFile = DtSet.Tables("InfData").Rows(0).Item(15).ToString                 '画像ファイル名
                            Else
                                picFile = ""
                            End If
                        Else
                            lngTemp = -1                                                                        ' 2012/02/10 Kino Add
                            Return lngTemp                                                                      ' 2012/02/10 Kino Add Finallyは通るが最後のReturnは通らない
                            ' Exit Function                                                                     ' 2012/02/10 Kino Changed
                        End If
                        DtSet.Clear()
                    End Using
                End Using
                '' ===============【 表示チャンネル数を取得し配列を宣言する 】===============(DataSummaryGraphInfo.mdb)
                Work = ("SELECT COUNT(ChNo) As DatCount,データファイルNo,MAX(ChNo) AS ChMax,MIN(ChNo) AS ChMin FROM " + strGraphName + "_描画情報 GROUP BY データファイルNo")
                Using DbDa As New OleDbDataAdapter(Work, DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter(Work, DbCon)
                    Using DtSet As New DataSet("ChannelCount")
                        'DtSet = New DataSet("ChannelCount")
                        DbDa.Fill(DtSet, "ChannelCount")
                        dataFileCount = DtSet.Tables("ChannelCount").Rows.Count

                        ReDim showDatas(dataFileCount - 1)
                        '' 2009/07/17 Kino Changed 高速化
                        ''For iloop = 0 To dataFileCount - 1                                                      'データファイル毎に表示チャンネル数を取得する
                        ''    showDatas(iloop).DataCount = DtSet.Tables("ChannelCount").Rows(iloop).Item(0)       'チャンネル数を取得
                        ''    showDatas(iloop).DataFileNo = DtSet.Tables("ChannelCount").Rows(iloop).Item(1)      'ファイル番号を取得
                        ''    showDatas(iloop).MaxCh = DtSet.Tables("ChannelCount").Rows(iloop).Item(2)           'チャンネルの最大値を取得
                        ''    showDatas(iloop).MinCh = DtSet.Tables("ChannelCount").Rows(iloop).Item(3)           'チャンネルの最小値を取得
                        ''    ReDim showDatas(iloop).DataSet(showDatas(iloop).DataCount - 1)                      'CommonInfから読み込むデータの配列宣言
                        ''Next iloop
                        iloop = 0
                        For Each DTR As DataRow In DtSet.Tables("ChannelCount").Rows
                            showDatas(iloop).DataCount = Convert.ToInt32(DTR.Item(0))                           'チャンネル数を取得
                            showDatas(iloop).DataFileNo = Convert.ToInt32(DTR.Item(1))                          'ファイル番号を取得
                            showDatas(iloop).MaxCh = Convert.ToInt32(DTR.Item(2))                               'チャンネルの最大値を取得
                            showDatas(iloop).MinCh = Convert.ToInt32(DTR.Item(3))                               'チャンネルの最小値を取得
                            ReDim showDatas(iloop).DataSet(showDatas(iloop).DataCount - 1)                      'CommonInfから読み込むデータの配列宣言
                            iloop += 1
                        Next
                        DtSet.Clear()
                    End Using
                End Using
                '' ===============【 表示チャンネル情報を読込、表示チャンネル番号を取得し配列に格納する 】===============(DataSummaryGraphInfo.mdb)
                Work = String.Format("SELECT * FROM {0}_描画情報 ORDER BY データファイルNo,ChNo", strGraphName)
                Using DbDa As New OleDbDataAdapter(Work, DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter(Work, DbCon)
                    Using DtSet As New DataSet("DrawDataInf")
                        'DtSet = New DataSet("DrawDataInf")
                        DbDa.Fill(DtSet, "DrawDataInf")

                        Dim DsetCount As Integer = DtSet.Tables("DrawDataInf").Rows.Count - 1
                        ReDim showDataInf(DsetCount)
                        '' 2009/07/17 Kino Changed 高速化
                        ''For iloop = 0 To DsetCount
                        ''    DRow = DtSet.Tables("DrawDataInf").Rows.Item(iloop)
                        ''    showDataInf(iloop) = DRow.ItemArray
                        ''    intTemp = DtSet.Tables("DrawDataInf").Rows(iloop).Item(13)                                                      'ファイル番号を取得
                        ''    showDatas(intTemp).SQLCh = showDatas(intTemp).SQLCh & DtSet.Tables("DrawDataInf").Rows(iloop).Item(2) & ","     'チャンネル番号を格納
                        ''Next iloop
                        iloop = 0
                        FieldCount = DtSet.Tables("DrawDataInf").Rows(0).ItemArray.Length - 1               ' 2017/02/24 Kino Changed LongLength -> Length
                        For Each DTR As DataRow In DtSet.Tables("DrawDataInf").Rows
                            ''DRow = DtSet.Tables("DrawDataInf").Rows.Item(iloop)
                            showDataInf(iloop) = DTR.ItemArray
                            intTemp = Convert.ToInt32(DTR.Item(13))                                                                             'ファイル番号を取得
                            showDatas(intTemp).SQLCh = showDatas(intTemp).SQLCh & DTR.Item(2).ToString & ","                                    'チャンネル番号を格納
                            iloop += 1
                        Next

                        For iloop = 0 To dataFileCount - 1
                            showDatas(intTemp).SQLCh = showDatas(intTemp).SQLCh.Replace(",0", "")
                            showDatas(intTemp).SQLCh.TrimEnd(Convert.ToChar(","))                                                           ' 2017/02/24 Kino Changed  From Lower statement
                            ''If showDatas(intTemp).SQLCh.EndsWith(",") = True Then
                            ''    showDatas(iloop).SQLCh = Left(showDatas(iloop).SQLCh, showDatas(iloop).SQLCh.ToString.Length - 1)           '最後のカンマを取る
                            ''End If
                        Next
                        lngTemp = 0                                                                                                         ' 2012/02/10 Kino Add
                    End Using
                End Using
            Catch ex As Exception
                lngTemp = -1                                                                                                        ' 2012/02/10 Kino Add
                'Finally
                ''DbCon.Close()                                                                                                     ' 2011/03/15 Kino Changed

                'DbDa.Dispose()
                'DbCom.Dispose()
                'DtSet.Dispose()
                'DbCon.Dispose()

            End Try

        End Using

        Return lngTemp                                                                                                          ' 2012/02/10 Kino Add

    End Function

    ''' <summary>
    ''' ***_CommonInfからデータの読込
    ''' </summary>
    ''' <param name="SiteDirectory">現場のフォルダ</param>
    ''' <param name="NewestFlg">最新データフラグ　1:最新　0:過去データ</param>
    ''' <param name="DataFileCount">データ一覧に表示に使用するデータファイル数</param>
    ''' <param name="ShowDatas">読み込んだデータを格納する配列構造体</param>
    ''' <returns>データ読み込みの可否</returns>
    ''' <remarks>2010/11/18 Function化して戻り値を設定</remarks>
    Public Function ReadCommonInfData(ByVal siteDirectory As String, ByVal newestFlg As Integer, ByVal dataFileCount As Integer, ByRef showDatas() As ShowDataSet, ByRef DataFileNames() As ClsReadDataFile.DataFileInf) As Long
        ''
        '' ***_CommonInf.mdb の指定チャンネルのデータを読込
        ''
        Dim DBFilePath As String
        'Dim DbCon As New OleDb.OleDbConnection
        ''Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As DataSet
        ''Dim DRow As DataRow
        ''Dim DataCount As Integer
        Dim iloop As Integer
        Dim jloop As Integer
        Dim work As String
        Dim lngTemp As Long = -1        ''戻り値初期値設定 0:正常　-1:ファイルなし 　それ以外:読み込みエラー
        Dim reTryCount As Integer = 0

        Dim gotDatas(dataFileCount - 1) As Boolean                                                          '' 2010/11/19 Kino Add データ読み込みフラグ初期化
        For jloop = 0 To dataFileCount - 1
            gotDatas(jloop) = True
        Next

ReLoadDatas:
        Dim retryFlg As Integer = 0
        '' ===============【 表示データを読込む 】===============(***_CommonInf.mdb)
        For jloop = 0 To dataFileCount - 1
            If DataFileNames(showDatas(jloop).DataFileNo).CommonInf.Length <> 0 And gotDatas(jloop) = True Then     '' 2010/11/19 Kino Changed   Add   And....

                DBFilePath = IO.Path.Combine(siteDirectory.ToString, "App_Data", DataFileNames(showDatas(jloop).DataFileNo).CommonInf.ToString)

                If IO.File.Exists(DBFilePath) = True Then
                    Dim dmy As Long = 1
                    If newestFlg = 1 Then
                        dmy = 1
                        If IO.File.Exists(DBFilePath) = True Then
                            Try                                                                                 '' 2010/11/18 Kino Add
                                ''最新データを表示　読込データ：現場フォルダ_CommonInf.mdb
                                Using DbCon As New OleDb.OleDbConnection

                                    DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DBFilePath.ToString & ";Jet OLEDB:Engine Type= 5")
                                    ''DbCon.Open()                                                                  ' 2011/03/15 Kino Changed
                                    work = "SELECT * FROM 共通情報 WHERE DataBaseCh IN(" + showDatas(jloop).SQLCh.ToString + ") ORDER BY DataBaseCh "

                                    Using DbDa As New OleDb.OleDbDataAdapter(work, DbCon)
                                        'DtSet = New DataSet("DrawData")
                                        Using DtSet As New DataSet("DrawData")
                                            'DbDa = New OleDb.OleDbDataAdapter(work, DbCon)
                                            DbDa.Fill(DtSet, "DrawData")
                                            ''DbCon.Close()                                                                 ' 2011/03/15 Kino Changed
                                            '' 2009/07/17 Kino Chaneged 高速化
                                            ''DataCount = DtSet.Tables("DrawData").Rows.Count - 1
                                            ''For iloop = 0 To DataCount
                                            ''    DRow = DtSet.Tables("DrawData").Rows.Item(iloop)
                                            ''    showDatas(jloop).DataSet(iloop) = DRow.ItemArray
                                            ''Next iloop
                                            iloop = 0
                                            For Each DTR As DataRow In DtSet.Tables("DrawData").Rows
                                                showDatas(jloop).DataSet(iloop) = DTR.ItemArray
                                                iloop += 1
                                            Next
                                            gotDatas(jloop) = False                                                         '' 2010/11/19 Kino Add　データは読み込めたので再読み込みしないようにフラグを外す
                                            lngTemp = 0
                                        End Using
                                    End Using
                                End Using
                            Catch ex As Exception
                                retryFlg += 1                                                                   '' 2010/11/19 Kino Add データが読み込めなかったのでカウントアップ
                                lngTemp = 1
                                Dim serverPath As String = siteDirectory.Substring(0, siteDirectory.LastIndexOf("\"))                                                                               ' 2015/05/16 Kino Add
                                ''Dim sw As New IO.StreamWriter(siteDirectory.ToString + "\log\errorSummary_" & Date.Now.ToString("yyMM") & ".log", True, Encoding.GetEncoding("Shift_JIS"))        ' 2015/05/16 Kino Changed
                                Using sw As New IO.StreamWriter(serverPath + "\log\errorSummary_" & Date.Now.ToString("yyMM") & ".log", True, Encoding.GetEncoding("Shift_JIS"))                      ' 2015/05/16 Kino Add
                                    If ex IsNot Nothing Then
                                        Dim sb As New StringBuilder
                                        sb.Append(DateTime.Now.ToString)
                                        sb.Append(ControlChars.Tab)
                                        sb.Append(ex.Source)
                                        sb.Append(ControlChars.Tab)
                                        sb.Append(ex.Message)
                                        sb.Append(ControlChars.Tab)
                                        sb.Append(jloop.ToString & " " & reTryCount.ToString)
                                        sw.WriteLine(sb.ToString)
                                    End If
                                    'sw.Dispose()
                                End Using
                            End Try
                        End If
                    Else
                        ''過去データ表示 　読込データ：現場フォルダ_CalculatedData.mdb              この機能は予定未定の延期 2008/07/29

                    End If
                End If
            End If
        Next jloop

        ' 2011/05/20 Kino Changed もしかしてこれがダメか？
        ''If retryFlg <> 0 And reTryCount < 3 Then                                                                               '' 2010/11/19 Kino Add データ再読み込み
        ''    reTryCount += 1
        ''    GoTo ReLoadDatas
        ''End If

        ''DbCon.Dispose()
        ''DbCom.Dispose()
        ''DbCon.Dispose()

        Return lngTemp                                                                          '' 2010/11/18 Kino Add

    End Function

    ''' <summary>
    ''' データ一覧用のラベルを作成しプロパティを設定System.Web.UI.HtmlControls.HtmlGenericControl
    ''' </summary>
    ''' <param name="FormObj">ラベルを表示するフォームオブジェクト</param>
    ''' <param name="ShowDatas">設定から抽出した値とデータ ***Commoninf.mdb</param>
    ''' <param name="ShowDataInf">読み込んだ設定 DataSummaryGraphInfo.mdb</param>
    ''' <param name="LabelOffset">画像に対してラベルの位置を調整するためのオフセット値</param>
    ''' <param name="DataFileCount">データ一覧に表示に使用するデータファイル数</param>
    Public Sub DynamicMakeLabels(ByVal FormObj As Panel, ByVal showDatas() As ShowDataSet, ByVal showDataInf() As Array, _
                                        ByVal labelOffset As OffsetCoordinates, ByVal dataFileCount As Integer, ByVal alertCount As Integer, _
                                        ByVal AlertInfo() As ClsReadDataFile.AlertInf, ByVal alertJudgeNo(,) As Short, ByVal DataFileNeme() As ClsReadDataFile.DataFileInf, _
                                        ByVal DataFilePath As String, ByVal fieldCount As Integer, Optional ByVal plusOffset As Single = 0)
        '    Public Sub DynamicMakeLabels(ByVal FormObj As System.Web.UI.HtmlControls.HtmlForm, ByVal showDatas() As ShowDataSet, ByVal showDataInf() As Array, _
        ''
        '' ラベルを動的に作成する　データ一覧用
        ''

        Dim iloop As Integer
        Dim jloop As Integer
        Dim DataFormat As String
        Dim CalcedData As Single
        Dim LabelData As String = ""
        Dim PreFix As String = ""
        Dim AlertLevel As Integer
        Dim DummyCount As Integer = 0                                                   ' 2012/02/22 Kino Changed  Add =0
        Dim PicTop As Single = 0
        Dim ShowCh As Integer
        Dim ShowRec As Integer
        Dim CharactorFlg As Integer
        Dim ShowData As String = ""
        Dim AlertWord As String = ""
        Dim clsAlert As New ClsReadDataFile
        Dim changeValue As String
        Dim SelCol As Integer
        Dim br As String = "<br>"                   'jQuery の ToolTip用
        Dim follState() As String = {"正常", "転倒済み", "転倒", "○", "×"}            '' 転倒センサ用
        Dim healthStatus As Short = 0                                                   '' 転倒センサ用
        Dim sufix As String = ""
        Dim IDPrefix As String                                                          ' 2015/11/19 Kino Add 非同期DOM対応用
        Dim RGBColor(3) As String                                                       '' 2016/06/28 Kino Add 背景だけ透過するCSSを作成するための色変数
        Dim LeftOffset As Integer = Convert.ToInt32(FormObj.Style("margin-left").Replace("px", ""))
        If plusOffset <> 0 Then
            PicTop = plusOffset
        End If
        If alertCount <> 0 Then
            ''PicTop = PicTop + 21 * (alertCount - 1)                                                       ''警報状況の表示分をオフセット
            PicTop = Convert.ToSingle(PicTop + (21 * (alertCount - 1) * 0.98))                              ''警報状況の表示分をオフセット
        End If

        For jloop = 0 To dataFileCount - 1
            For iloop = 0 To showDatas(jloop).DataCount - 1
                If Convert.ToBoolean(showDataInf(DummyCount).GetValue(15)) = True And
                    IO.File.Exists(IO.Path.Combine(DataFilePath, "App_Data", DataFileNeme(Convert.ToInt32(showDataInf(DummyCount).GetValue(13))).CommonInf)) = True Then      '表示がTrueなら処理を行なう
                    ShowCh = CType(showDataInf(DummyCount).GetValue(2), Integer)                             '表示するデータベースチャンネル
                    ShowRec = GetDBChRecordNo(jloop, showDatas, ShowCh)                                 '表示するデータベースチャンネルをデータの中のレコードから検索しレコード番号を取得
                    'Dim DataLabel As New System.Web.UI.WebControls.Label
                    Using DataLabel As New System.Web.UI.WebControls.Label
                        DataFormat = showDataInf(DummyCount).GetValue(14).ToString
                        If showDatas(jloop).DataSet(ShowRec) Is Nothing Then GoTo NextStep
                        AlertLevel = Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(17))
                        AlertWord = ""
                        If AlertLevel <> 0 Then
                            Try                                                                             ' 2018/03/02 Kino Add 例外処理追加　警報判定フラグのレコード数が足りない場合の処理
                                If alertJudgeNo(ShowCh, jloop) = 0 Then
                                    AlertLevel = 0
                                    AlertWord = "警報判定除外"
                                ElseIf alertJudgeNo(ShowCh, jloop) > Math.Abs(AlertLevel) Then
                                    AlertLevel = 0
                                    AlertWord = "警報判定" + alertJudgeNo(ShowCh, jloop).ToString + "次以降"
                                End If
                            Catch ex As Exception

                            End Try
                        End If

                        SelCol = Convert.ToInt32(showDataInf(DummyCount).GetValue(8))
                        sufix = ""
                        With DataLabel
                            Select Case SelCol          'showDataInf(DummyCount).GetValue(8)
                                Case 0                                                                      '●任意文字列指定
                                    LabelData = showDataInf(DummyCount).GetValue(10).ToString
                                    .Text = LabelData
                                    CharactorFlg = 1                                                        '   文字列フラグを立てる
                                Case 3 To 6, 18                                                             '●計器記号、出力、種別、ブロックタイトル、測定時刻
                                    LabelData = showDatas(jloop).DataSet(ShowRec).GetValue(Convert.ToInt32(showDataInf(DummyCount).GetValue(8))).ToString
                                    .Text = LabelData
                                    CharactorFlg = 1                                                        '   文字列フラグを立てる
                                Case 19                                                                     '●測定値
                                    CalcedData = Convert.ToSingle(showDatas(jloop).DataSet(ShowRec).GetValue(Convert.ToInt32(showDataInf(DummyCount).GetValue(8))))
                                    ''If CalcedData >= 0 Then PreFix = "+" Else PreFix = "-" '±の表記を指定
                                    If CalcedData > 1.1E+30 Then
                                        .Text = "**"
                                        ShowData = "異常値もしくは欠測"
                                        AlertWord = "－"
                                    Else
                                        ''.Text = PreFix & Math.Abs(CalcedData).ToString(DataFormat )
                                        changeValue = clsAlert.trunc_round(CalcedData, DataFormat, Convert.ToBoolean(showDataInf(DummyCount).GetValue(18)))        '' 四捨五入／切捨て処理と符合付加を行う 2009/06/11 Kino Changed
                                        .Text = PreFix & changeValue    'CalcedData.ToString("+" & DataFormat & ";-" & DataFormat) '& ";+" & DataFormat)
                                        ShowData = (clsAlert.trunc_round(CalcedData, (DataFormat & "00"), Convert.ToBoolean(showDataInf(DummyCount).GetValue(18))) & " " & showDatas(jloop).DataSet(ShowRec).GetValue(4).ToString)  ' 2011/05/25 Kino Changed
                                        ''ShowData = CalcedData.ToString(DataFormat & "00 ") & showDatas(jloop).DataSet(ShowRec).GetValue(4)
                                        If AlertWord.Length = 0 Then
                                            AlertWord = AlertInfo(AlertLevel + 3).Words                                     'clsAlert.AlertData(AlertLevel + 3).Words
                                        End If
                                    End If
                                    CharactorFlg = 0
                                Case 20
                                    LabelData = showDatas(jloop).DataSet(ShowRec).GetValue(3).ToString                      '● 転倒センサ　識別記号
                                    .Text = LabelData
                                    CharactorFlg = 2
                                Case 21                                                                                     '● 転倒センサ 健全性確認
                                    If Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(19)) > 1 Then showDatas(jloop).DataSet(ShowRec).SetValue(1, 19) ' 2012/03/12 Kino Add 米坂線対応(複合転倒判定対応_転倒済みの判定は不可)
                                    Dim refCh As Short = Convert.ToInt16(showDatas(jloop).DataSet(ShowRec).GetValue(19))
                                    If refCh >= 0 And refCh <= 1 Then
                                        LabelData = follState(refCh + 3)
                                        .Text = LabelData
                                    End If
                                    CharactorFlg = 3
                                Case Else                                                                   '●上記以外
                                    CalcedData = Convert.ToSingle(showDatas(jloop).DataSet(ShowRec).GetValue(Convert.ToInt32(showDataInf(DummyCount).GetValue(8))))
                                    ''If CalcedData >= 0 Then PreFix = "+" Else PreFix = "-" '±の表記を指定
                                    ''.Text = PreFix + Math.Abs(CalcedData).ToString(DataFormat)
                                    .Text = (PreFix + CalcedData.ToString("+" + DataFormat + ";-" + DataFormat + ";+" + DataFormat))
                                    ShowData = CalcedData.ToString(DataFormat + "00 ") & showDatas(jloop).DataSet(ShowRec).GetValue(4).ToString
                                    AlertWord = "－"
                                    CharactorFlg = 0
                            End Select

                            IDPrefix = showDataInf(DummyCount).GetValue(1).ToString                         ' 2015/11/19 Kino Changed DOM対応でIDにサフィックスを付与(Ch)　DOMに関係ないものは今まで通り
                            If IDPrefix.IndexOf("NoTip") > -1 AndAlso SelCol = 0 Then
                                .ID = (showDataInf(DummyCount).GetValue(1).ToString & DummyCount.ToString)
                            Else
                                .ID = (showDataInf(DummyCount).GetValue(1).ToString & DummyCount.ToString)  '& "_F" & jloop.ToString & "_Ch" & ShowCh.ToString)
                            End If

                            If showDataInf(DummyCount).GetValue(1).ToString.IndexOf("NoTip") <> -1 Then     ' 2012/02/22 Kino Add
                                CharactorFlg = 4
                                AlertLevel = 0
                            End If

                            .Font.Size = CType(showDataInf(DummyCount).GetValue(7), Web.UI.WebControls.FontSize)
                            .BorderStyle = CType(showDataInf(DummyCount).GetValue(11), Web.UI.WebControls.BorderStyle)
                            If fieldCount = 18 Then                                                                     ' 2012/02/10 Kino Add
                                .BorderColor = Color.FromArgb(-4144960)
                            Else
                                .BorderColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(19)))                     '' これにすると新しいBrowserにもBorderStyleが対応
                            End If
                            .BorderWidth = CInt(showDataInf(DummyCount).GetValue(12))
                            ''.EnableTheming = False                                                                '' 2009/06/24 Kino Changed
                            .EnableViewState = False

                            RGBColor(0) = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17))).R.ToString               '' 2016/06/28 Kino Add 透過背景を作成するための文字列
                            RGBColor(1) = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17))).G.ToString
                            RGBColor(2) = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17))).B.ToString
                            RGBColor(3) = (Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17))).A / 255).ToString("0.00")

                            Select Case CharactorFlg
                                Case 0, 2, 3
                                    If AlertInfo(AlertLevel + 3).BackColor = 0 Then                                     '' 警報判定しない場合(MenuInfo.mdb)
                                        .BackColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17)))
                                        AlertWord = "-"
                                    Else
                                        .BackColor = Color.FromArgb(AlertInfo(AlertLevel + 3).BackColor)                    ''clsAlert.AlertData(AlertLevel + 3).BackColor)
                                    End If

                                    If AlertInfo(AlertLevel + 3).ForeColor = 0 Then                                     '' 警報判定しない場合(MenuInfo.mdb)
                                        .ForeColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(16)))
                                    Else
                                        .ForeColor = Color.FromArgb(AlertInfo(AlertLevel + 3).ForeColor)                    ''clsAlert.AlertData(AlertLevel + 3).ForeColor)
                                    End If
                                    Select Case CharactorFlg
                                        Case 0                  ' 通常センサ
                                            .ToolTip = (
                                                String.Format("計器記号： {0}<br>", showDatas(jloop).DataSet(ShowRec).GetValue(3).ToString) &
                                                String.Format("数値詳細： {0}<br>", ShowData) &
                                                String.Format("警報判定： 【{0}】<br><br>", AlertWord) &
                                                String.Format("測定時刻： {0}", showDatas(jloop).DataSet(ShowRec).GetValue(18))
                                                )
                                        Case 2                  ' 転倒センサ
                                            If Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(19)) > 2 Then showDatas(jloop).DataSet(ShowRec).SetValue(2, 19) ' 2012/03/12 Kino Add 米坂線対応(複合転倒判定対応_転倒済みの判定は不可)
                                            .ToolTip = (
                                                String.Format("識別記号: {0}<br>", showDatas(jloop).DataSet(ShowRec).GetValue(3)) &
                                                String.Format("状　　態: {0}<br>", follState(Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(19)))) &
                                                String.Format("通知時刻: {0}", showDatas(jloop).DataSet(ShowRec).GetValue(18))
                                                )
                                        Case 3                  ' 転倒センサ 健全性
                                            If Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(19)) > 1 Then showDatas(jloop).DataSet(ShowRec).SetValue(1, 19) ' 2012/03/12 Kino Add 米坂線対応(複合転倒判定対応_転倒済みの判定は不可)
                                            LabelData = LabelData.Replace("×", "通知異常")
                                            LabelData = LabelData.Replace("○", "正常")
                                            .ToolTip = (
                                                String.Format("識別記号： {0}<br>", showDatas(jloop).DataSet(ShowRec).GetValue(3)) &
                                                String.Format("健全性　： {0}<br>", LabelData) &
                                                String.Format("通知時刻： {0}", showDatas(jloop).DataSet(ShowRec).GetValue(18))
                                            )

                                        Case 4          'NoTips
                                            .ToolTip = ""
                                    End Select
                                Case 1
                                    '.BackColor = Color.FromArgb(showDataInf(DummyCount).GetValue(17))                          '2016/06/28 Kino Changed 
                                    .Style("background-color") = String.Format("rgba({0},{1},{2},{3})", RGBColor)               ' 2016/06/28 Kino Add
                                    .ForeColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(16)))
                                    .ToolTip = showDataInf(DummyCount).GetValue(10).ToString
                                Case Else                                                                                       ' 文字列等
                                    ''If Convert.ToInt32(showDataInf(DummyCount).GetValue(17)) < 999999999 Then                       ' 2014/07/07 Kino Add 999999999 は 背景透明で枠だけ
                                    ''    .BackColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17)))                           ' 2012/02/22 Kino Add
                                    ''End If
                                    '.BackColor = Color.FromArgb(showDataInf(DummyCount).GetValue(17))                           ' 2012/02/22 Kino Add  '2016/06/28 Kino Changed 
                                    .Style("background-color") = String.Format("rgba({0},{1},{2},{3})", RGBColor)               ' 2016/06/28 Kino Add
                                    .ForeColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(16)))
                                    .ToolTip = ""
                            End Select

                            ''System.Windows.Forms.Application.DoEvents()                                                       ' 2012/02/22 Kino Changed
                            ''.Attributes.Add("class", "LBL_MAIN")      '↓
                            '' '' '' '' ''.Style("z-index") = 3
                            ''Unit.Parse(CType(CType(menuProp._Width, Integer) + 15, String)).ToString

                            .Style("Position") = "Absolute"
                            If Convert.ToInt32(showDataInf(DummyCount).GetValue(6)) <> 0 Then
                                .Style("width") = (showDataInf(DummyCount).GetValue(6).ToString + "px")
                            End If
                            If Convert.ToInt32(showDataInf(DummyCount).GetValue(5)) <> 0 Then
                                .Style("height") = (showDataInf(DummyCount).GetValue(5).ToString + "px")
                                If showDataInf(DummyCount).GetValue(9).ToString.IndexOf("M") > 0 Then
                                    .Style("line-height") = (showDataInf(DummyCount).GetValue(5).ToString + "px")               ' 2015/05/14 Kino Add 縦位置センターのおまじない
                                End If
                            End If
                            ''.Style("left") = (Convert.ToInt32(showDataInf(DummyCount).GetValue(4)) + labelOffset.X).ToString + "px"
                            .Style("left") = Unit.Parse((Convert.ToInt32(showDataInf(DummyCount).GetValue(4)) + labelOffset.X - LeftOffset).ToString).ToString
                            .Style("top") = Unit.Parse((Convert.ToInt32(showDataInf(DummyCount).GetValue(3)) + labelOffset.Y + PicTop).ToString).ToString
                            If AlertLevel <> 0 And CharactorFlg = 0 Then
                                ''.Style("text-decoration") = "blink"                                             'ブラウザによっては点滅する   2015/05/14 Kino Changed 既に点滅はしないのでコメント
                                '.Font.Bold = True
                                ''Dim anm As AjaxControlToolkit.AnimationExtender = CType(FormObj.FindControl("AnmEx"), AjaxControlToolkit.AnimationExtender) 動的にAnimationExtenderを設定できると思ったけど駄目だった・・・
                                ''anm.TargetControlID = "ctl00_" & (showDataInf(iloop).GetValue(1) & iloop).ToString
                                sufix = "B"                                                                     ' LCe,LLe,LRi を最後に設定する必要があるが、これで警報時は太字になる
                            End If
                            .Attributes.Add("class", showDataInf(DummyCount).GetValue(9).ToString + sufix)

                            If IO.File.Exists(IO.Path.Combine(DataFilePath, "App_Data\BlinkOn.flg")) = True AndAlso SelCol = 19 Then        ' 2018/03/12 Kino Add
                                sufix &= String.Format(" blinkNo{0}", Math.Abs(AlertLevel))
                            End If
                            ''.style("filter") = "mask(color=white)".ToString 'ColorTranslator.ToHtml(Color.FromArgb(ClsReadDataFile.AlertData(AlertLevel + 3).BackColor))  ''(Color.FromArgb(ClsReadDataFile.AlertData(AlertLevel + 3).BackColor)).ToKnownColor  ''ColorTranslator.FromHtml("0x" & Microsoft.VisualBasic.Right(Hex(ClsReadDataFile.AlertData(AlertLevel + 3).BackColor), 6)).ToString
                            ''.Style("font-family") = "'MS UI Gothic'"
                            ''Select Case showDataInf(iloop).GetValue(9).ToString
                            ''    Case "DataLBLLeft"
                            ''        .Style("TEXT-ALIGN") = "left"
                            ''    Case "DataLBLRight"
                            ''        .Style("TEXT-ALIGN") = "right"
                            ''    Case Else
                            ''        .Style("TEXT-ALIGN") = "center"
                            ''End Select
                            ''.Style("vertical-align") = "middle"
                            ''.Style("padding-left") = "2px"
                            ''.Style("padding-right") = "2px"
                            ''.Style("padding-bottom") = "1px"
                            ''.Style("padding-top") = "1px"
                            ''.Style("white-space") = "nowrap"

                            ''.CssClass = showDataInf(DummyCount).GetValue(9).ToString
                            FormObj.Controls.Add(DataLabel)
                        End With
                    End Using
                End If
NextStep:
                DummyCount += 1
            Next iloop
        Next jloop

        clsAlert = Nothing

    End Sub

    Public Function GetDBChRecordNo(ByVal FileNo As Integer, ByVal ShowDatas() As ShowDataSet, ByVal SearchCh As Integer) As Integer

        Dim iloop As Integer
        Dim intTemp As Integer = 0

        If SearchCh = 0 Then
            Return intTemp
            Exit Function '文字ラベルの場合は検索しない
        End If


        ''For iloop = 0 To ShowDatas(FileNo).DataSet.Length - 1
        ''    If ShowDatas(FileNo).DataSet(iloop).GetValue(0) = SearchCh Then
        ''        intTemp = iloop
        ''        Exit For
        ''    End If
        ''Next iloop
        '' 2009/07/17 Kino Add 高速化
        iloop = 0
        For Each DA As Array In ShowDatas(FileNo).DataSet
            If DA Is Nothing = False Then                                       '' 2010/10/06 Kino Add ファイルが存在しない場合の対応
                If Convert.ToInt32(DA.GetValue(0)) = SearchCh Then
                    intTemp = iloop
                    Exit For
                End If
                iloop += 1
            End If
        Next
        Return intTemp
    End Function

End Class
