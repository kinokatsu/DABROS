Imports System.Data
Imports System.Data.OleDb
Imports System.Diagnostics

Public Class ClsReadDataFile

    Public Structure AlertInf
        Dim Levels As Short                                         '警報レベル　通常 -3～3
        Dim BackColor As Integer                                    '　　背景色
        Dim ForeColor As Integer                                    '　　文字色
        Dim Words As String                                         '　　警報表示文字列
        Dim alpha As Integer                                        '    アルファ値      '2014/05/16 KinoAdd
    End Structure

    Public Structure DataFileInf                                    'データファイルの設定
        Dim ZoneName As String                                      'データファイルの識別名
        Dim FileName As String                                      'データファイルのファイル名
        Dim CommonInf As String                                     '共通情報のファイル名
        Dim OldestDate As Date                                      '最古日付
        Dim NewestDate As Date                                      '最新日付
        Dim OverDayCheck As Boolean                                 'データ最新日付と現在日時の比較を行なうフラグ　2009/01/28 Kino
        Dim MostOldestDate As Date                                  'すべてのデータファイルにおける最古の日付   2010/05/24 Kino Add
        Dim showIndex As Short                                      ' 2012/10/22 Kino Add メイン画面下部のデータ収録期間をソートするためのフィールド
        ''Dim SitePath As String                                      '現場サイトのフォルダ
    End Structure

    Public Structure DataInfo                                       '■分布図用 構造体
        Dim DataCh As Integer                                       'データチャンネル
        Dim DepthData As Single                                     '深度データ
        ''Dim DispData() As Single                                    '変位データ       '別変数とした
        Dim AlertData() As Single                                   '警報値データ
        Dim SensorSymbol As String                                  '計器記号
    End Structure

    Public Structure siteInfo
        Dim No As Short                                             ' 現場番号
        Dim Name As String                                          ' 現場名称
        Dim Folder As String                                        ' 現場データフォルダ
        Dim shortName As String                                     ' 現場省略名称
        Dim menuType As String      'Short                                       ' メニュータイプ
        Dim subSite As String                                       ' サブ項目用タイトル
        Dim showAlertFlg As Short                                   ' 赤点滅表示　有効／無効
        Dim alertMaxLevel() As Short                                ' 現場における最高警報次数(ファイルごと)
        Dim allSensorSymbol() As String                             ' 現場における警報値超過センサー(ファイルごと)
    End Structure


    ''''' <summary>
    ''''' 警報表示の設定を格納する配列構造体
    ''''' </summary>
    ''Public AlertData(6) As AlertInf                          '警報表示情報
    ''''' <summary>
    ''''' 現場で使用するデータファイルの配列構造体
    ''''' </summary>
    ''Public Shared DataFileNames() As DataFileInf                    'データファイル名を格納(複数対応のため)
    ''Public Shared dteLastUpdate As Date                             '最終更新日
    ''Public Shared intAlertCount As Integer

    Dim siteInf As Object

    ''' <summary>
    ''' 表示指定されている全現場の各*CommonInf.mdbを全て取得し、その中に記載されている警報超過センサーを取得する
    ''' </summary>
    ''' <param name="siteDirectory"></param>
    ''' <param name="siteInf"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getSiteAlertStatus(ByVal siteDirectory As String, ByRef siteInf() As siteInfo) As Integer

        Dim iloop As Integer
        Dim jloop As Integer
        Dim menuFilePath As String
        Dim DataFileNames() As DataFileInf = Nothing
        Dim strRet As String = Nothing
        Dim overSensorSymbol As String = Nothing
        Dim maxLevel As Integer = 0
        Dim intRet As Integer

        For iloop = 0 To siteInf.Length - 1
            If siteInf(iloop).Folder.IndexOf("SP") = -1 Then

                menuFilePath = IO.Path.Combine(siteDirectory, siteInf(iloop).Folder, "App_Data\Menuinfo.mdb")
                If IO.File.Exists(menuFilePath) = True AndAlso siteInf(iloop).Name.Contains("完了現場") = False Then                                                         ' 2015/12/23 Kino Add
                    intRet = GetDataFileNames(menuFilePath, DataFileNames)                                          ' 処理するファイルを取得

                    ReDim siteInf(iloop).allSensorSymbol(intRet)                                                    ' *CommonInf.mdbのファイル数で配列指定
                    ReDim siteInf(iloop).alertMaxLevel(intRet)

                    If siteInf(iloop).showAlertFlg <> 0 Then

                        For jloop = 0 To intRet
                            maxLevel = getAlertInfo(siteDirectory & siteInf(iloop).Folder & "\App_Data\" & DataFileNames(jloop).CommonInf, overSensorSymbol)

                            siteInf(iloop).allSensorSymbol(jloop) = overSensorSymbol
                            siteInf(iloop).alertMaxLevel(jloop) = maxLevel
                        Next
                    End If
                Else
                    ReDim siteInf(iloop).allSensorSymbol(0)                                                             ' 2015/12/23 Kino Add
                    ReDim siteInf(iloop).alertMaxLevel(0)
                End If
            End If
        Next

        Return intRet

    End Function

    ''' <summary>
    ''' 指定した*commoninf.mdbファイルを検索して警報値を超過しているセンサーを取得
    ''' </summary>
    ''' <param name="commonInfPath">*commoninf.mdbファイルのフルパス</param>
    ''' <param name="alertSensor">警報値を超過したセンサーをカンマ区切りで格納(記号:次数　→　例:　A-1:-1,A-2:1・・・) </param>
    ''' <returns>最大警報次数</returns>
    ''' <remarks></remarks>
    Public Function getAlertInfo(commonInfPath As String, ByRef alertSensor As String) As Integer

        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbDa As OleDb.OleDbDataAdapter = Nothing
        'Dim DtSet As New DataSet("DData")
        Dim intRet As Integer = 0
        Dim strSQL As String = Nothing
        Dim overSensorSymbol As New StringBuilder
        Dim maxLevel As Short
        Dim tempLevel As Short

        If IO.File.Exists(commonInfPath) = True Then

            Try

                Using DbCon As New OleDbConnection
                    DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & commonInfPath & ";Jet OLEDB:Engine Type= 5")
                    strSQL = "SELECT [計器記号],[警報判定結果],[最新データ] FROM 共通情報 WHERE 警報判定結果<>0 ORDER BY DatabaseCh"

                    Using DbDa As New OleDbDataAdapter(strSQL, DbCon)
                        'DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)

                        Using DtSet As New DataSet("DData")
                            DbDa.Fill(DtSet, "DData")

                            For Each DTR As DataRow In DtSet.Tables("DData").Rows
                                overSensorSymbol.Append(DTR.Item(0).ToString).Append(":").Append(DTR.Item(1).ToString).Append(",")      ' 警報超過センサーの一覧取得

                                tempLevel = Math.Abs(CType(DTR.Item(1), Short))                                                         ' 最大次数の取得
                                If maxLevel < tempLevel Then
                                    maxLevel = tempLevel
                                    intRet = CType(DTR.Item(1), Short)                                                                  ' 最大次数の上下限保持するためにここで入力
                                End If
                            Next

                            If overSensorSymbol.Length > 0 Then
                                alertSensor = overSensorSymbol.ToString.Substring(0, overSensorSymbol.Length - 1)
                            Else
                                alertSensor = Nothing
                                intRet = 0
                            End If
                            overSensorSymbol.Clear()
                        End Using
                    End Using
                End Using
            Catch ex As Exception

            Finally
                ''DbDa.Dispose()
                ''DtSet.Dispose()
                ''DbCon.Dispose()
                overSensorSymbol.Length = 0

            End Try

        Else
            intRet = -99
        End If

        Return intRet

    End Function

    ''' <summary>
    ''' データファイル名を取得しDataFileNamesに格納する
    ''' </summary>
    ''' <param name="DataFile">データファイルの設定が記録されているデータベースのPath[MenuInf.mdb]</param>
    Public Function GetDataFileNames(ByVal dataFile As String, ByRef DataFileNames() As DataFileInf) As Integer
        ''HttpContext.Current.Session["セッション変数名"] 

        'Dim DbCon As New OleDb.OleDbConnection
        ''Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim iloop As Integer
        Dim DsetCount As Integer = 0
        Dim dteTemp As DateTime = DateTime.Now                              ' 2016/02/18 Kino Add

        Using DbCon As New OleDbConnection

            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + dataFile + ";" + "Jet OLEDB:Engine Type= 5")
            ''DbCon.Open()                                                      ' 2011/03/15 Kino Changed 

            '' 現場名称の読込
            Using DbDa As New OleDbDataAdapter("SELECT * FROM データファイル名設定 ORDER BY ID ASC", DbCon)
                'DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM データファイル名設定 ORDER BY ID ASC", DbCon)

                Using DtSet As New DataSet("DData")
                    DbDa.Fill(DtSet, "DData")

                    ''DbCon.Close()                                                     ' 2011/03/15 Kino Changed 
                    ''DbCon.Dispose()

                    DsetCount = DtSet.Tables("DData").Rows.Count - 1                    ' 行数-1(配列インデックスが0から始まることを考慮した値)
                    ReDim DataFileNames(DsetCount)

                    For Each DTR As DataRow In DtSet.Tables("DData").Rows
                        DataFileNames(iloop).ZoneName = Trim(DTR.Item(1))
                        DataFileNames(iloop).FileName = Trim(DTR.Item(2))
                        ''If IsDBNull(DTR.Item(3)) = False Then                         'ヌル値チェック                    ' 2016/02/18 Kino Changed   もういらないだろう・・
                        DataFileNames(iloop).CommonInf = Trim(DTR.Item(3))
                        ''End If
                        ''DataFileNames(iloop).OverDayCheck = DtSet.Tables("DData").Rows(iloop).Item(4)
                        ''DataFileNames(iloop).OverDayCheck = DtSet.Tables("DData").Rows(iloop).Item(4)
                        ''If DtSet.Tables("DData").Columns.Count > 5 Then                                                 ' 2012/10/22 Kino Add 表示順を読込
                        ''    DataFileNames(iloop).showIndex = DtSet.Tables("DData").Rows(iloop).Item(5)
                        ''Else
                        ''    DataFileNames(iloop).showIndex = iloop
                        ''End If
                        ''iloop += 1
                        DataFileNames(iloop).OverDayCheck = DtSet.Tables("DData").Rows(iloop).Item(4)
                        If DtSet.Tables("DData").Columns.Count > 5 AndAlso IsDBNull(DTR.Item(5)) = False Then           ' 2012/10/22 Kino Add 表示順を読込  2015/12/17 Kino Changed ヌルチェック
                            DataFileNames(iloop).showIndex = DtSet.Tables("DData").Rows(iloop).Item(5)
                            If iloop >= 1 AndAlso DataFileNames(iloop - 1).showIndex = DataFileNames(iloop).showIndex Then      '2015/12/17 Kino Add  インデックスが変わらなければ+1する
                                DataFileNames(iloop).showIndex = (DataFileNames(iloop - 1).showIndex + 1)
                            End If
                        Else
                            DataFileNames(iloop).showIndex = iloop
                        End If

                        DataFileNames(iloop).OldestDate = DateTime.Parse(DTR.Item(6))                                   ' 2016/02/18 Kino Add データ最古日付
                        DataFileNames(iloop).NewestDate = DateTime.Parse(DTR.Item(7))                                   ' 2016/02/18 Kino Add データ最新日付

                        If dteTemp >= DataFileNames(iloop).OldestDate Then                                              ' 全てのファイルにおける最古日付を取得する
                            dteTemp = DataFileNames(iloop).OldestDate
                        End If

                        iloop += 1
                    Next

                    For iloop = 0 To (iloop - 1)                                                                        '2016/02/18 Kino Add 複数ファイルの場合においても最古のデータ
                        DataFileNames(iloop).MostOldestDate = dteTemp
                    Next
                    ''For iloop = 0 To DsetCount
                    ''    DataFileNames(iloop).ZoneName = Trim(DtSet.Tables("DData").Rows(iloop).Item(1))
                    ''    DataFileNames(iloop).FileName = Trim(DtSet.Tables("DData").Rows(iloop).Item(2))
                    ''    If IsDBNull(DtSet.Tables("DData").Rows(iloop).Item(3)) = False Then                         'ヌル値チェック
                    ''        DataFileNames(iloop).CommonInf = Trim(DtSet.Tables("DData").Rows(iloop).Item(3))
                    ''    End If
                    ''    DataFileNames(iloop).OverDayCheck = DtSet.Tables("DData").Rows(iloop).Item(4)
                    ''Next iloop

                    'DbDa.Dispose()
                    ' ''DbCom.Dispose()
                    'DbCon.Dispose()

                    'DbDa = Nothing
                    ' ''DbCom = Nothing
                    'DtSet = Nothing
                    'DbCon = Nothing
                End Using
            End Using
        End Using

        Return DsetCount

    End Function

    ''' <summary>
    ''' データ線色情報を読み込む
    ''' </summary>
    ''' <returns>色情報設定数(レコード数)</returns>
    Public Function GetLineColor(ByVal DBPath As String, ByRef LineColor() As String) As Integer

        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        Dim iloop As Integer
        Dim DsetCount As Integer = 0

        If System.IO.File.Exists(DBPath) = True Then
            Using DbCon As New OleDbConnection

                DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + DBPath + ";" + "Jet OLEDB:Engine Type= 5"
                ''DbCon.Open()                                                              ' 2011/03/15 Kino Changed 

                ''テーブルオープン
                Using DbDa As New OleDb.OleDbDataAdapter("Select * From データ線色 Order by データNo ASC;", DbCon)

                    'DbDa = New OleDb.OleDbDataAdapter("Select * From データ線色 Order by データNo ASC;", DbCon)

                    Using DtSet As New DataSet("DData")
                        DbDa.Fill(DtSet, "DData")

                        ''DbCon.Close()                                                             ' 2011/03/15 Kino Changed 
                        ''DbCon.Dispose()

                        DsetCount = DtSet.Tables("DData").Rows.Count - 1

                        For Each DTR As DataRow In DtSet.Tables("DData").Rows
                            LineColor(iloop) = DTR.Item(1)
                            iloop += 1
                        Next
                        ''For iloop = 0 To DsetCount
                        ''    LineColor(iloop) = DtSet.Tables("DData").Rows(iloop).Item(1)
                        ''Next

                        'DbDa.Dispose()
                    End Using
                End Using
            End Using
        End If

        'DbCon.Dispose()
        'DbCom.Dispose()
        'DbDa = Nothing
        'DbCom = Nothing
        'DbCon = Nothing

        Return DsetCount

    End Function

    ''' <summary>
    ''' マーカ情報を読み込む
    ''' </summary>
    ''' <returns>マーカ設定数(レコード数)</returns>
    Public Function GetMarkerNumber(ByVal DBPath As String, ByRef MarkerNum() As Integer) As Integer

        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        Dim iloop As Integer
        Dim DsetCount As Integer = 0

        If System.IO.File.Exists(DBPath) = True Then

            Using DbCon As New OleDbConnection
                DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + DBPath + ";" + "Jet OLEDB:Engine Type= 5"
                ''DbCon.Open()                                                                      ' 2011/03/15 Kino Changed 

                ''テーブルオープン
                Using DbDa As New OleDb.OleDbDataAdapter("Select * From マーカ種別 Order by データ番号 ASC", DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter("Select * From マーカ種別 Order by データ番号 ASC", DbCon)

                    Using DtSet As New DataSet("DData")

                        DbDa.Fill(DtSet, "DData")

                        ''DbCon.Close()                                                                     ' 2011/03/15 Kino Changed 
                        ''DbCon.Dispose()

                        DsetCount = DtSet.Tables("DData").Rows.Count - 1

                        For Each DTR As DataRow In DtSet.Tables("DData").Rows
                            If iloop = MarkerNum.Length Then Exit For '                                     '2017/04/07 Kino Add 
                            MarkerNum(iloop) = DTR.Item(2)
                            iloop += 1
                        Next
                        ''For iloop = 0 To DsetCount
                        ''    MarkerNum(iloop) = DtSet.Tables("DData").Rows(iloop).Item(2)
                        ''Next

                        ''DbCon.Dispose()
                        ''DbDa.Dispose()
                    End Using
                End Using
            End Using
        End If

        ''DbCon.Dispose()
        ''DbCom.Dispose()

        ''DbDa = Nothing
        ''DbCom = Nothing
        ''DbCon = Nothing

        Return DsetCount

    End Function

    ''' <summary>
    ''' データファイルの最古・最新日時を取得する　複数データファイル対応 DataFileNames()取得済みを必須とする
    ''' </summary>
    ''' <returns>データファイル数</returns>
    Public Function GetDataLastUpdate(ByVal dataFile As String, ByRef DataFileNames() As DataFileInf) As Integer
        'Public Function GetDataLastUpdate(ByVal siteDirectory As String, ByRef DataFileNames() As DataFileInf) As Integer

        'Dim DbCon As New OleDb.OleDbConnection
        ''Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As New OleDb.OleDbDataAdapter
        Dim iloop As Integer
        Dim FileCount As Integer = DataFileNames.Length - 1
        Dim dteTemp As Date = Now.Date
        'Dim DtSet As New DataSet
        Dim DsetCount As Integer                                                                                ' 2016/02/18 Kino Add

        Try                                                                                 ' 2011/05/23 Kino Add エラートラップ
            Using DbCon As New OleDbConnection
                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & dataFile & ";" & "Jet OLEDB:Engine Type= 5")

                '' 現場名称の読込
                Using DbDa As New OleDbDataAdapter("SELECT データファイル名,OldestDate,NewestDate FROM データファイル名設定 ORDER BY ID ASC", DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter("SELECT データファイル名,OldestDate,NewestDate FROM データファイル名設定 ORDER BY ID ASC", DbCon) '2016/02/18 Kino Changed

                    Using DtSet As New DataSet("DData")
                        DbDa.Fill(DtSet, "DData")

                        DsetCount = DtSet.Tables("DData").Rows.Count - 1

                        For Each DTR As DataRow In DtSet.Tables("DData").Rows
                            DataFileNames(iloop).OldestDate = DateTime.Parse(DTR.Item(1))                                   ' 2016/02/18 Kino Add　データ最古日付
                            DataFileNames(iloop).NewestDate = DateTime.Parse(DTR.Item(2))                                   ' 2016/02/18 Kino Add　データ最新日付

                            If dteTemp >= DataFileNames(iloop).OldestDate Then                                              ' 全てのファイルにおける最古日付を取得する
                                dteTemp = DataFileNames(iloop).OldestDate
                            End If
                            iloop += 1
                        Next

                        '' データファイルから取得する方法は廃止　　MenuInfo.mdbに格納する仕組みとしたので、そこから読むように変更
                        ''For iloop = 0 To FileCount                                                       'データファイル数でループ
                        ''    If System.IO.File.Exists(siteDirectory + DataFileNames(iloop).FileName) = True Then

                        ''        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + siteDirectory + DataFileNames(iloop).FileName + ";" + "Jet OLEDB:Engine Type= 5")
                        ''        ''DbCon.Open()                                                          ' 2011/03/15 Kino Changed 

                        ''        DtSet = New DataSet("DData")                                                                                   ''データセットを宣言（ここでやらないとデータセットの内容が前のと同じになる）

                        ''        ''最古最新日付取得
                        ''        DbDa = New OleDb.OleDbDataAdapter("Select MIN(日付) As MinDate, MAX(日付) As MaxDate From 日付", DbCon)
                        ''        DbDa.Fill(DtSet, "DData")

                        ''        ''DbCon.Close()                                                         ' 2011/03/15 Kino Changed 
                        ''        ''DbCon.Dispose()

                        ''        If IsDBNull(DtSet.Tables("DData").Rows(0).Item(0)) = False Then                                                         'データDBはあるが日付けがない場合の対策
                        ''            DataFileNames(iloop).OldestDate = Trim(DtSet.Tables("DData").Rows(0).Item(0))   'DbDr.GetDateTime(0)                '最古日付
                        ''            DataFileNames(iloop).NewestDate = Trim(DtSet.Tables("DData").Rows(0).Item(1))   'DbDr.GetDateTime(1)                '最新日付

                        ''            If dteTemp >= DataFileNames(iloop).OldestDate Then                                                                  '2010/05/24 Kino Add 複数ファイルにおける最古を探す
                        ''                dteTemp = DataFileNames(iloop).OldestDate
                        ''            End If
                        ''        End If

                        ''    End If
                        ''Next iloop

                        For iloop = 0 To FileCount                                                                                                  '2010/05/24 Kino Add 複数ファイルの場合においても最古のデータ
                            DataFileNames(iloop).MostOldestDate = dteTemp
                        Next
                    End Using
                End Using
            End Using

        Catch ex As Exception

            FileCount = -1

            ''Finally

            ''    DbDa.Dispose()
            ''    DtSet.Dispose()
            ''    DbCon.Dispose()

            ''    DbDa = Nothing
            ''    DtSet = Nothing
            ''    DbCon = Nothing
        End Try


        ''DbCom.Dispose()
        ''DbCon.Dispose()                                           ' 2016/06/22 Kino Changed コメント

        ''DbDa = Nothing
        ''DbCom = Nothing
        ''DbCon = Nothing

        Return FileCount

    End Function

    ''' <summary>
    ''' CSVデータファイルの最新日時を取得する
    ''' </summary>
    ''' <returns>データファイル数</returns>
    Public Function GetHSSDataLastUpdate(ByVal dataFile As String) As DateTime

        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbDa As New OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet
        Dim getDate As DateTime

        Try

            Using DbCon As New OleDb.OleDbConnection

                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & dataFile & ";" & "Jet OLEDB:Engine Type= 5")

                '' 現場名称の読込
                Using DbDa As New OleDb.OleDbDataAdapter("SELECT MAX(日付) FROM CSV", DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter("SELECT MAX(日付) FROM CSV", DbCon)

                    Using DtSet As New DataSet("DData")

                        DbDa.Fill(DtSet, "DData")

                        If DtSet.Tables("DData").Rows.Count > 0 Then

                            getDate = DtSet.Tables("DData").Rows(0).Item(0)

                        End If

                        ''For Each DTR As DataRow In DtSet.Tables("DData").Rows
                        ''    DataFileNames(iloop).OldestDate = DateTime.Parse(DTR.Item(1))                                   ' 2016/02/18 Kino Add　データ最古日付
                        ''    DataFileNames(iloop).NewestDate = DateTime.Parse(DTR.Item(2))                                   ' 2016/02/18 Kino Add　データ最新日付

                        ''    If dteTemp >= DataFileNames(iloop).OldestDate Then                                              ' 全てのファイルにおける最古日付を取得する
                        ''        dteTemp = DataFileNames(iloop).OldestDate
                        ''    End If
                        ''    iloop += 1
                        ''Next
                    End Using
                End Using
            End Using
        Catch ex As Exception

            Return getDate

            'Finally

            '    DbDa.Dispose()
            '    DtSet.Dispose()
            '    DbCon.Dispose()

            '    DbDa = Nothing
            '    DtSet = Nothing
            '    DbCon = Nothing

        End Try

        Return getDate

    End Function

    ''' <summary>
    ''' 共通情報の最大チャンネル数を取得する　複数データファイル対応 DataFileNames()取得済みを必須とする
    ''' </summary>
    ''' <returns>最大チャンネル番号</returns>
    Public Function GetMaxChNo(ByVal DataFile As String) As Integer

        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        Dim MaxCh As Integer

        If System.IO.File.Exists(DataFile) = True Then

            Using DbCon As New OleDb.OleDbConnection

                DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + DataFile + ";" + "Jet OLEDB:Engine Type= 5"
                ''DbCon.Open()                                                                          ' 2011/03/15 Kino Changed 

                'Dim DtSet As New DataSet("DData")                                                                                   ''データセットを宣言（ここでやらないとデータセットの内容が前のと同じになる）

                ''最古最新日付取得
                Using DbDa As New OleDb.OleDbDataAdapter("Select MAX(DataBaseCh) As MAXCh From 共通情報", DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter("Select MAX(DataBaseCh) As MAXCh From 共通情報", DbCon)

                    Using DtSet As New DataSet("DData")

                        DbDa.Fill(DtSet, "DData")

                        ''DbCon.Close()                                                                         ' 2011/03/15 Kino Changed 
                        ''DbCon.Dispose()

                        MaxCh = Trim(DtSet.Tables("DData").Rows(0).Item(0))   'DbDr.GetDateTime(0)                '最古日付

                        'DbDa.Dispose()

                        'DtSet = Nothing
                    End Using
                End Using
            End Using
        End If

        'DbCom.Dispose()
        'DbCon.Dispose()
        'DbDa = Nothing
        'DbCom = Nothing
        'DbCon = Nothing

        Return MaxCh

    End Function

    ''' <summary>
    ''' 期間データを取得するためのSQLクエリ文字列を作成する
    ''' </summary>
    ''' <param name="dataCount">データ数(読込むフィールド数)</param>
    ''' <param name="dataCh">データのフィールド名</param>
    ''' <param name="stDate">開始日付</param>
    ''' <param name="edDate">終了日付</param>
    ''' <param name="timeCond">検索条件式(時間)</param>
    ''' <param name="strOption">検索条件式(任意オプション)</param>
    ''' <param name="anotherName">フィールドを開くときの別名</param>
    ''' <param name="intTOPflg">SELECT句の直後に指定する条件</param>
    Public Function GetSQLString(ByVal dataCount As Short, ByVal dataCh() As Integer, ByVal stDate As Date, _
                                        ByVal edDate As Date, Optional ByVal timeCond As String = "", _
                                        Optional ByVal strOption As String = "", Optional ByVal anotherName As String = "", _
                                        Optional ByVal intTOPflg As Integer = 0) As String
        Dim iloop As Integer
        Dim TimeSet() As String
        Dim Joint As String = ""
        Dim dmdm As Integer         ''String = ""
        Dim Table As String = ""
        Dim j1 As String = ""
        Dim j2 As String = "日付"
        Dim w1 As String = ""
        Dim w2 As String = ""
        Dim FieldName As String = ""
        Dim Condition As String = ""
        Dim Work As String = ""
        Dim strData As String = ""
        Dim AnthName() As String = {}
        Dim strORDER As String = ""

        If intTOPflg = 9999 Then
            FieldName = "DISTINCT FORMAT(日付, 'HH:mm') AS 測定時刻, 日付"
            strORDER = " ORDER BY 日付.日付 ASC"
        ElseIf intTOPflg > 0 Then
            FieldName = ("TOP " + intTOPflg.ToString + " 日付.日付")
            strORDER = " ORDER BY 日付.日付 DESC"
        ElseIf intTOPflg = -1 Then
            FieldName = "FORMAT(日付.日付,'yyyy/MM/dd HH:mm:ss')"
            strORDER = " ORDER BY 日付.日付 ASC"
        Else
            FieldName = "日付.日付"
            strORDER = " ORDER BY 日付.日付 ASC"
        End If

        '別名オプションが合った場合
        If anotherName.Length <> 0 Then
            AnthName = anotherName.Split(",")
            For iloop = 0 To AnthName.Length - 1
                AnthName(iloop) = (" AS " + AnthName(iloop))
            Next
        Else
            ReDim AnthName(dataCount)
            For iloop = 0 To dataCount
                AnthName(iloop) = ""
            Next
        End If

        If intTOPflg = 9999 Then
            Joint = "日付"
        Else
            ''テーブル名の取得
            '' フィールド一覧作成
            For iloop = 0 To dataCount
                ''dmdm = Int((dataCh(iloop) - 1) / 100) * 100
                ''dmdm = Int((dataCh(iloop) - 1) * 0.01) * 100
                dmdm = Math.Truncate((dataCh(iloop) - 1) * 0.01) * 100                                              ' 2016/06/15 Kino Changed   Int -> Math.Truncate

                ''Table = "C" & Right$("00000" & ((dmdm + 1).ToString).Trim, 4) & "_C" & Right$("00000" & ((dmdm + 100).ToString).Trim, 4)
                If dmdm >= 0 Then                                                                                   ' 2016/06/15 Kino Add データ本数が実際の設定より多くしてしまった場合の対策
                    Table = ("C" + (dmdm + 1).ToString("0000") + "_C" + (dmdm + 100).ToString("0000"))

                    ''If repData Is Nothing = False AndAlso repData(iloop).Length <> 0 Then                       ' 2013/01/28 Kino Add 現状のMDBではReplaceは使用できない

                    ''    Dim jloop As Integer
                    ''    allRep = repData(iloop).Split(",")              'ex (.0: 　,.1: 弱,.2: 強)
                    ''    Dim partRep() As String = Nothing
                    ''    Dim TBfldName As String
                    ''    Dim ChTemp As String = Nothing
                    ''    Dim ChInf As String = Nothing
                    ''    TBfldName = (Table + ".CH" + (dataCh(iloop).ToString).Trim)
                    ''    For jloop = 0 To allRep.Length - 1
                    ''        partRep = allRep(jloop).Split(":")
                    ''        If ChInf Is Nothing Then
                    ''            ChTemp = TBfldName
                    ''        Else
                    ''            ChTemp = ChInf
                    ''        End If
                    ''        ChInf = ("Replace(" & ChTemp & ",""" & partRep(0) & """,""" & partRep(1) & """)")
                    ''    Next
                    ''    FieldName += ("," & ChInf & AnthName(iloop))
                    ''Else

                    FieldName += ("," + Table + ".CH" + (dataCh(iloop).ToString).Trim + AnthName(iloop))

                    ''End If
                End If
            Next

            Dim PreTbl As String = ""                                                               '2010/05/24 Kino Add 処理したテーブル名
            '' テーブル一覧作成
            ''For iloop = Int((dataCh(0) - 1) * 0.01) + 1 To Int((dataCh(dataCount) - 1) * 0.01) + 1                '' 2009/08/06 Kino Changed Int((dataCh(dataCount)) * 0.01) + 1 -> Int((dataCh(dataCount) - 1) * 0.01) + 1
            For iloop = 0 To dataCh.Length - 1                                                                          '' 2010/05/24 Kino Changed チャンネル番号でループ
                ''Table = "C" & Right$("00000" & Trim$(Str$(iloop * 100 - 99)), 4) & "_C" & Right$("00000" & Trim$(Str$(iloop * 100)), 4)
                ''dmdm = Int((dataCh(iloop) - 1) * 0.01) * 100                                                        ''2010/05/24 Kino Add チャンネル番号からテーブル名を計算
                dmdm = Math.Truncate((dataCh(iloop) - 1) * 0.01) * 100                                              ' 2016/06/15 Kino Changed   Int -> Math.Truncate

                If dmdm >= 0 AndAlso PreTbl.IndexOf("@" & dmdm.ToString & "@") = -1 Then                             ' 2016/06/15 Kino Changed   Add dmdm>0 AndAlso   データ本数が実際の設定より多くしてしまった場合の対策
                    ''Table = ("C" + (iloop * 100 - 99).ToString("0000") + "_C" + (iloop * 100).ToString("0000"))
                    Table = ("C" + (dmdm + 1).ToString("0000") + "_C" + (dmdm + 100).ToString("0000"))              ''2010/05/24 Kino Changed 使用しているテーブルだけにする
                    j1 += w1
                    j2 += w2 & (" INNER JOIN " + Table + " ON 日付.日付ID=" + Table + ".日付ID")
                    w1 = "(" : w2 = ")"
                    PreTbl += "@" + dmdm.ToString + "@,"                                                            ''2010/05/24 Kino Add 処理したテーブルを保持
                End If
            Next
            Joint = j1 & j2
        End If

        '' 条件文作成
        If timeCond.Length <> 0 Then
            TimeSet = timeCond.Split(",")
            ''strData &= "(日付 LIKE "
            strData += "("
            ''For iloop = 0 To TimeSet.Length - 1           '2009/10/19 Kino Changed 簡素化
            ''    ''strDat= "'" & TimeSet(iloop).Substring(0, 2) & ":" & TimeSet(iloop).Substring(2, 2) & "' or "
            ''    strData += ("HOUR(日付)=" + TimeSet(iloop) + " or ")
            ''Next iloop
            ''strData = (strData.Substring(0, strData.Length - 4) + ") and minute(日付)=0")
            strData = "(HOUR(日付) IN (" + timeCond + ") AND MINUTE(日付)=0)"
            Condition = ("(日付.日付 BETWEEN #" + stDate.ToString + "# AND #" + edDate.ToString + "#) and " + strData)     '& " ORDER BY 日付 ASC" 'ORDER BY 日付 ASC"       
        Else
            Condition = ("(日付.日付 BETWEEN #" + stDate.ToString + "# AND #" + edDate.ToString + "#)")                    '" ORDER BY 日付 ASC" 'ORDER BY 日付 ASC"       
        End If

        If strOption.Length <> 0 Then
            Condition += (" and " + strOption)
        End If

        Condition &= strORDER

        Work = ("SELECT " + FieldName + " FROM " + Joint + " WHERE " + Condition & ";")

        Return Work

        'SELECT 日付.日付,C0001_C0100.[Ch1], C0001_C0100.[Ch2],C0001_C0100.[Ch1], C0001_C0100.[Ch3],C0001_C0100.[Ch1], C0001_C0100.[Ch4] FROM 日付 INNER JOIN C0001_C0100 ON 日付.日付ID=C0001_C0100.日付ID WHERE (日付.日付 BETWEEN #2013/04/01# AND #2013/04/15 23:59#);
        '実行結果 →  日付、Expr1001、Ch2、Expr1003、Ch3、Ch1、Ch4
    End Function


    Public Function GetSQLString_Bunpu(ByVal dataCount As Short, ByVal dataCh() As DataInfo, ByVal stDate As Date, ByVal edDate As Date, _
                                       Optional ByVal strOption As String = "", Optional ByVal fromNewest As Boolean = True) As String
        ''
        '' 分布図用
        ''
        Dim iloop As Integer
        Dim Joint As String = ""
        Dim dmdm As Integer         ''String = ""
        Dim Table As String = ""
        Dim j1 As String = ""
        Dim j2 As String = "日付"
        Dim w1 As String = ""
        Dim w2 As String = ""
        Dim FieldName As String = ""
        Dim Condition As String = ""
        Dim Work As String = ""
        Dim strData As String = ""
        Dim AnthName() As String = {}
        Dim strORDER As String = ""

        If fromNewest = True Then           '最新データの場合は「共通情報テーブル」を読む

            FieldName = "FORMAT(警報判定時刻,'yyyy/MM/dd HH:mm:ss'), DataBaseCh,計器記号,設置位置,警報値上限1,警報値上限2,警報値上限3,警報値下限1,警報値下限2,警報値下限3,最新データ"
            strORDER = " ORDER BY DataBaseCh"
            Joint = "共通情報"
            ''Condition = "DataBaseCh IN("
            ''For iloop = 0 To dataCount
            ''    Condition &= dataCh(iloop).DataCh & ","
            ''Next iloop
            ''Condition = Condition.Substring(0, Condition.Length - 1) & ")"

        Else
            ''テーブル名の取得
            '' フィールド一覧作成
            FieldName = "日付.日付"
            Condition = " WHERE "
            For iloop = 0 To dataCount  '- 1                                                                                '2010/08/25 Kino Changed
                ''dmdm = Int((dataCh(iloop).DataCh - 1) * 0.01) * 100
                dmdm = Math.Truncate((dataCh(iloop).DataCh - 1) * 0.01) * 100                                                               ' 2016/04/26 Kino Changed Int -> Math.Truncate
                Table = ("C" + (dmdm + 1).ToString("0000") + "_C" + (dmdm + 100).ToString("0000"))
                FieldName += ("," + Table + ".CH" + (dataCh(iloop).DataCh.ToString).Trim)        '+ AnthName(iloop)
            Next

            '' テーブル一覧作成
            For iloop = Math.Truncate((dataCh(0).DataCh - 1) * 0.01) + 1 To Math.Truncate((dataCh(dataCount - 1).DataCh) * 0.01) + 1    '使用しているテーブルの数分のループ 2016/04/27 Kino Changed Convert.ToInt32 -> Math.Truncate
                ''For iloop = Int((dataCh(0).DataCh - 1) * 0.01) + 1 To Int((dataCh(dataCount - 1).DataCh) * 0.01) + 1            '使用しているテーブルの数分のループ
                Table = ("C" + (iloop * 100 - 99).ToString("0000") + "_C" + (iloop * 100).ToString("0000"))
                j1 += w1
                j2 += w2 + (" INNER JOIN " + Table + " ON 日付.日付ID=" + Table + ".日付ID")
                w1 = "(" : w2 = ")"
            Next
            Joint = j1 + j2
        End If

        If strOption.Length <> 0 Then
            Condition += ("日付 " + strOption + " ORDER BY 日付")                                                           '' 2011/04/14 Kino Changed  Add ORDER...
        End If

        Condition += strORDER

        Work = ("SELECT " + FieldName + " FROM " + Joint + Condition + ";")

        Return Work

    End Function

    ''' <summary>
    ''' SQLによりDBを開き、配列にデータを格納して戻す(とりあえず経時変化図用に作成)
    ''' </summary>
    ''' <param name="DataFilePath">データファイルのパス</param>
    ''' <param name="DataCount">データのフィールド名</param>
    ''' <param name="DataCh">データのフィールド名</param>
    ''' <param name="StDate">開始日付</param>
    ''' <param name="EdDate">終了日付</param>
    ''' <param name="Datas">読込んだデータ</param>
    ''' <param name="TimeCond">検索条件式(時間)</param>
    Public Function GetDrawData(ByVal dataFilePath As String, ByVal dataCount As Short, ByVal dataCh() As Integer, _
                                        ByVal stDate As Date, ByVal edDate As Date, ByRef datas() As Array, _
                                        Optional ByVal timeCond As String = "") As Integer

        'Dim DbCon As New OleDb.OleDbConnection
        ''Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As New OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        ''Dim DRow As DataRow
        Dim iloop As Integer
        Dim strSQL As String = ""
        'Dim DTbl As DataTable
        Erase datas                                                                         ' 2016/06/15 Kino Add 一度内容をクリアする

        strSQL = GetSQLString(dataCount, dataCh, stDate, edDate, timeCond)

        Using DbCon As New OleDbConnection
            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + dataFilePath + ";Jet OLEDB:Engine Type= 5")
            ''DbCon.Open()                                                                  ' 2011/03/15 Kino Changed                 

            Try                                                                                 ' 2011/06/07 Kino Add リトライ処理追加
                '' 現場名称の読込
                Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)

                    Using DtSet As New DataSet("DData")

                        DbDa.Fill(DtSet, "DData")
                        'DTbl = DtSet.Tables("DData")

                        ''DbCon.Close()                                                                 ' 2011/03/15 Kino Changed 
                        ''DbCon.Dispose()

                        ''-----------
                        Dim DsetCount As Integer = DtSet.Tables("DData").Rows.Count - 1

                        ReDim datas(DsetCount)
                        If DsetCount <> -1 Then                                                         '指定日付範囲でデータがある場合
                            iloop = 0
                            ''Dim stpw As Stopwatch = Stopwatch.StartNew
                            For Each DTR As DataRow In DtSet.Tables("DData").Rows
                                ''DRow = DTb
                                datas(iloop) = DTR.ItemArray        'DRow.ItemArray
                                iloop += 1
                            Next
                        End If

                        ''stpw.Stop()
                        ''Console.WriteLine("処理は {0} ミリ秒かかりました。", stpw.ElapsedMilliseconds)

                        ''-----------
                        ''stpw = Stopwatch.StartNew             '2009/07/17 Kino Changed　↑ こちらの方が若干速いので変更
                        ''For iloop = 0 To DsetCount
                        ''    DRow = DtSet.Tables("DData").Rows.Item(iloop)
                        ''    datas(iloop) = DRow.ItemArray
                        ''    'For jloop = 0 To DataCount
                        ''    '    strData = strData & DRow(jloop) & ","
                        ''    'Next jloop
                        ''    'strData = strData & Environment.NewLine
                        ''Next iloop
                        ''stpw.Stop()
                        ''Console.WriteLine("処理は {0} ミリ秒かかりました。", stpw.ElapsedMilliseconds)
                        ''-----------

                        Return DsetCount

                        'If DbDr.HasRows = True Then

                        '    SiteData = SiteNo.Split(",")
                        '    DbDa = New OleDb.OleDbDataAdapter("Select * From ConstructionSite Where No = " & SiteData(iLoop).ToString, DbCon)
                        '    DbDa.Fill(DtSet, "ConstructionSite")

                        '    If DtSet.Tables("ConstructionSite").Rows.Count <> 0 Then
                        '        For iLoop = 0 To UBound(SiteData) - 1
                        '            'Me.ConstructionList.Items.Add(DtSet.Tables("ConstructionSite").Rows(iLoop).Item("SiteName").ToString)
                        '            'Me.ConstructionList.Items(iLoop).Value = DtSet.Tables("ConstructionSite").Rows(iLoop).Item("Address").ToString
                        '        Next
                        '    End If
                        '    Me.ConstructionList.Items(0).Selected = True
                        'End If
                        '' ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. ☆ .. 読込完了
                    End Using
                End Using
            Catch ex As Exception

                Return -1

            Finally

                'DbDa.Dispose()
                ' ''DbCom.Dispose()
                'DtSet.Dispose()
                'DbCon.Dispose()

            End Try
        End Using

    End Function

    ''' <summary>
    ''' 全最新データを読み込む  ***_CommonInf.mdb
    ''' </summary>
    ''' <param name="DataFile">データファイルのパス</param>
    ''' <param name="LatestDatas">データの日付[戻り値]</param>
    ''' <param name="strSQL">SQLクエリ(省略可)</param>
    Public Function GetLatestDataFromCommonInf(ByVal dataFile As String, ByRef latestDatas() As Array, Optional ByVal strSQL As String = "") As Integer

        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbDa As New OleDb.OleDbDataAdapter
        Dim DsetCount As Integer
        'Dim DtSet As New DataSet("DData")
        Dim iloop As Integer
        Dim SQLSet As String
        'Dim DbCom As New OleDb.OleDbCommand
        ''Dim DRow As DataRow

        '' ''警報レベルの絶対値の最大値を取得
        '' ''strSQL = "SELECT * From 共通情報 WHERE 警報判定結果 = (SELECT MAX(ABS(警報判定結果)) FROM 共通情報)"
        '' ''strSQL = "SELECT MAX(ABS(警報判定結果)) AS 警報レベル From 共通情報 WHERE 警報判定結果 <> 0"
        '' ''strSQL = "SELECT MAX(ABS(警報判定結果)) AS 警報レベル,警報判定時刻 From 共通情報 ORDER BY 警報判定時刻 ASC"
        Try                                                                         ' 2011/05/27 Kino Add エラートラップ　ここのDBがらみでエラーが出ているらしいので対応

            Using DbCon As New OleDbConnection
                DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & dataFile & ";Jet OLEDB:Engine Type= 5"
                ''DbCon.Open()                                                  ' 2011/03/15 Kino Changed 

                If strSQL.Length = 0 Then
                    SQLSet = "SELECT * FROM 共通情報 ORDER BY DataBaseCh "
                Else
                    SQLSet = strSQL                                             'strSQL = "SELECT * From 共通情報 WHERE 警報判定結果 <> 0"
                End If

                Using DbDa As New OleDb.OleDbDataAdapter(SQLSet, DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter(SQLSet, DbCon)
                    Using DtSet As New DataSet("DData")

                        DbDa.Fill(DtSet, "DData")
                        ''DbCon.Close()                                                   '2009/08/28 Kino Add  ' 2011/03/15 Kino Changed 

                        DsetCount = DtSet.Tables("DData").Rows.Count - 1
                        ReDim latestDatas(DsetCount)
                        iloop = 0
                        For Each DTR As DataRow In DtSet.Tables("DData").Rows
                            ''DRow = DTR                                                '１レコード分を取得
                            latestDatas(iloop) = DTR.ItemArray   ' DRow.ItemArray       '配列に、配列として格納する
                            iloop += 1
                        Next

                        ''For iloop = 0 To DsetCount
                        ''    DRow = DtSet.Tables("DData").Rows.Item(iloop)               '１レコード分を取得
                        ''    latestDatas(iloop) = DRow.ItemArray                         '配列に、配列として格納する
                        ''Next iloop
                    End Using
                End Using
            End Using

        Catch ex As Exception

            DsetCount = -2                                                      '戻り値を-1とする(エラーが発生したということ)

        Finally

            ' ''DbCon.Close()
            'DbCon.Dispose()

            'DbDa.Dispose()
            ''DbCom.Dispose()
            'DtSet.Dispose()
            'DbCon.Dispose()

        End Try

        Return DsetCount                                                    '2009/08/28 Kino Changed Moved Here

    End Function


    ''' <summary>
    ''' 配列内のデータの平均、最大値、最小値を求める
    ''' </summary>
    ''' <param name="DataCount">データの列数</param>
    ''' <param name="DsetCount">データの行数</param>
    ''' <param name="Datas">演算するデータの配列</param>
    ''' <param name="MaxMinData">演算した結果の2次元配列</param>
    Public Function GetSum(ByVal dataCount As Integer, ByVal dsetCount As Integer, ByVal datas() As Array, ByRef maxMinData(,) As Single) As Integer

        Dim SumValue As Double
        Dim DatCount As Integer
        Dim iloop As Integer
        Dim jloop As Integer
        Dim MaxTemp As Single
        Dim MinTemp As Single

        For iloop = 1 To dataCount                                  '列ループ

            SumValue = 0
            DatCount = 0                                            '初期値リセット
            MaxTemp = -9.9E+30
            MinTemp = 9.9E+30
            For Each DA As Array In datas
                If DA.GetValue(iloop) < 1.1E+30 Then
                    SumValue += DA.GetValue(iloop)
                    DatCount += 1
                    If MaxTemp < DA.GetValue(iloop) Then MaxTemp = DA.GetValue(iloop)
                    If MinTemp > DA.GetValue(iloop) Then MinTemp = DA.GetValue(iloop)
                End If

                jloop += 1
            Next
            ''For jloop = 0 To dsetCount                              '行ループ
            ''    If datas(jloop).GetValue(iloop) < 1.1E+30 Then
            ''        SumValue += datas(jloop).GetValue(iloop)
            ''        DatCount += 1
            ''        If MaxTemp < datas(jloop).GetValue(iloop) Then MaxTemp = datas(jloop).GetValue(iloop)
            ''        If MinTemp > datas(jloop).GetValue(iloop) Then MinTemp = datas(jloop).GetValue(iloop)
            ''    End If
            ''Next jloop

            maxMinData(iloop - 1, 0) = SumValue                     '総和
            maxMinData(iloop - 1, 1) = Convert.ToSingle(DatCount)	'データ数
            If DatCount <> 0 Then
                maxMinData(iloop - 1, 2) = SumValue / DatCount      '平均値
            Else
                maxMinData(iloop - 1, 2) = 7.7E+30
            End If
            If MaxTemp > -1.1E+20 Then
                maxMinData(iloop - 1, 3) = MaxTemp                  '最大値
            Else
                maxMinData(iloop - 1, 3) = 7.7E+30
            End If
            If MinTemp < 1.1E+20 Then
                maxMinData(iloop - 1, 4) = MinTemp                  '最小値
            Else
                maxMinData(iloop - 1, 4) = 7.7E+30
            End If

        Next iloop

        Return 1

    End Function

    ''' <summary>
    ''' 最新データ日付から指定期間遡った日付を計算する
    ''' </summary>
    ''' <param name="fromNewestFld">最新から、もしくは指定期間として計算する判別値</param>
    ''' <param name="lastUpdate">最新データ日付</param>
    ''' <param name="edDay">指定日付</param>
    ''' <param name="OutputKind">グラフ種別</param>
    ''' <returns>計算結果の日時</returns>
    Public Function CalcEndDate(ByVal fromNewestFld As Boolean, ByVal lastUpdate As Date, _
                                        ByVal edDay As String, ByVal outputKind As Integer) As Date

        Dim endDate As Date

        '' 最終日を計算する
        If fromNewestFld = True Then
            '最新から
            endDate = lastUpdate
        Else
            '指定期間
            endDate = Date.Parse(edDay)
        End If

        Select Case outputKind
            Case 0      '経時変化図
                endDate = Date.Parse(endDate.AddDays(1).ToString("yyyy/MM/dd 00:00"))       '最新データの次の日の0時までとする
            Case 1, 3      '経時表,相関図
                endDate = Date.Parse(endDate.ToString("yyyy/MM/dd 23:59:59"))               '最新データがある日の23:59:59までとする
            Case 2
                endDate = endDate                                                           'そのままの時間を使う　期間が時間指定の場合に使用する
        End Select

        Return endDate              '計算した最終日を返す

    End Function

    ''' <summary>
    ''' 指定日から、指定日前の日付を計算する
    ''' </summary>
    ''' <returns>計算結果の日時</returns>
    ''' <param name="stDay">a</param>
    ''' <param name="edDay">指定最終日</param>
    ''' <param name="DateType">期間種別</param>
    ''' <param name="intInterval">間隔</param>
    ''' <param name="OutputKind">グラフ種別</param>
    Public Function CalcStartDate(ByVal stDay As String, ByVal edDay As Date, ByVal dateType As String, _
                                        ByVal intInterval As Integer, ByVal outputKind As Integer) As Date
        Dim dteTemp As Date
        Dim startDate As Date

        Select Case outputKind
            Case 0      '経時変化図
                startDate = edDay.AddDays(-1)                                   '計算された最終日付の前の日の0時を基準とする            
            Case 1      '経時表
                startDate = Date.Parse(edDay.ToString("yyyy/MM/dd 0:00"))       '計算された最終日付の0時を基準とする
            Case 2
                startDate = edDay                                               'そのままの時間を使う　期間が時間指定の場合に使用する
        End Select

        Select Case dateType
            Case "H"    '時
                dteTemp = startDate.AddHours(-intInterval)
            Case "D"    '日
                dteTemp = startDate.AddDays(-intInterval)
            Case "M"    '月
                dteTemp = startDate.AddMonths(-intInterval)
            Case "Y"    '年
                dteTemp = startDate.AddYears(-intInterval)
            Case "W"    '週
                dteTemp = startDate.AddYears(-intInterval * 7)
            Case Else
                dteTemp = startDate
        End Select

        Return dteTemp

    End Function

    ''' <summary>
    ''' 今日を起点にして過去の指定期間を計算して返す
    ''' </summary>
    ''' <param name="OldTerm">期間文字列</param>
    Public Function CalcOldDateLimit(ByVal OldTerm As String) As Date

        Dim Interval As Double
        Dim DateType As String
        Dim dteTemp As Date
        Dim EdDate As Date

        EdDate = Date.Parse(Date.Now().ToString("yyyy/MM/dd 0:00"))

        If OldTerm = "None" Then
            dteTemp = Date.Parse(EdDate).AddYears(-50)
        Else
            Interval = Integer.Parse(OldTerm.Substring(0, 2))
            DateType = OldTerm.Substring(2, 1)

            Select Case DateType
                Case "Y"
                    dteTemp = EdDate.AddYears(-Interval)
                Case "M"
                    dteTemp = EdDate.AddMonths(-Interval)
                Case "D"
                    dteTemp = EdDate.AddDays(-Interval)
            End Select
        End If

        dteTemp = dteTemp.AddDays(-1)                                    '2009/10/22 Kino Changed 計算によって１日ずれるので対策用

        Return dteTemp

    End Function

    ''' <summary>
    ''' 警報表示情報を読込みAlertDataに格納する
    ''' </summary>
    ''' <param name="DataFile">データベースのパス</param>
    Public Function GetAlertDisplayInf(ByVal dataFile As String, ByVal AlertDatas() As AlertInf) As Integer

        'Dim DbCon As New OleDb.OleDbConnection
        ' Dim DbDr As OleDb.OleDbDataReader
        ' Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")

        Dim strSQL As String
        Dim TempCount As Integer

        ''警報レベル表示設定を取得--------

        Using DbCon As New OleDbConnection
            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + dataFile + ";Jet OLEDB:Engine Type= 5"
            '' DbCon.Open()                                                          ' 2011/03/15 Kino Changed 

            strSQL = "SELECT * FROM 警報表示情報 ORDER BY 警報レベル ASC"

            Using DbDa As New OleDbDataAdapter(strSQL, DbCon)
                'DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)

                Using DtSet As New DataSet("DData")

                    DbDa.Fill(DtSet, "DData")

                    '' DbCom = New OleDb.OleDbCommand(strSQL, DbCon)
                    '' DbDr = DbCom.ExecuteReader

                    For Each DTR As DataRow In DtSet.Tables("DData").Rows
                        If TempCount > 6 Then Exit For
                        AlertDatas(TempCount).Levels = DTR.Item(1)
                        AlertDatas(TempCount).Words = DTR.Item(2)
                        AlertDatas(TempCount).BackColor = DTR.Item(3)
                        AlertDatas(TempCount).ForeColor = DTR.Item(4)
                        AlertDatas(TempCount).alpha = DTR.Item(5)
                        TempCount += 1
                    Next

                    ''With DbDr
                    ''    If .HasRows = True Then
                    ''        Do While .Read()
                    ''            If TempCount > 6 Then Exit Do
                    ''            AlertDatas(TempCount).Levels = .GetInt16(1)
                    ''            AlertDatas(TempCount).Words = .GetString(2)
                    ''            AlertDatas(TempCount).BackColor = .GetInt32(3)
                    ''            AlertDatas(TempCount).ForeColor = .GetInt32(4)
                    ''            TempCount += 1
                    ''        Loop
                    ''    End If
                    ''End With

                    ''DbDr.Close()
                    ''DbCon.Close()                                                         ' 2011/03/15 Kino Changed 
                    'DbDa.Dispose()
                    'DtSet.Dispose()
                    'DbCon.Dispose()
                    ''DbCom.Dispose()
                End Using
            End Using
        End Using

        Return TempCount

    End Function

    Private Sub DatabaseOptimize(ByVal srcDBPath As String)
        ''
        '' データベースを最適化する
        ''

        Dim jro As JRO.JetEngine
        Dim strTemp As String

        strTemp = srcDBPath.Substring(0, srcDBPath.LastIndexOf(".")) & "_Temp.mdb"

        jro = New JRO.JetEngine
        jro.CompactDatabase("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & srcDBPath, _
                            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & strTemp)

        jro = Nothing

        If IO.File.Exists(strTemp) = True Then
            'System.Windows.Forms.Application.DoEvents()
            System.Threading.Thread.Sleep(20)
            IO.File.Delete(srcDBPath)                       'ソースファイル削除

            'System.Windows.Forms.Application.DoEvents()
            System.Threading.Thread.Sleep(20)
            IO.File.Move(strTemp, srcDBPath)                'リネーム
        End If

    End Sub
    ''' <summary>
    ''' 自動更新のフラグを取得
    ''' </summary>
    ''' <param name="DataFile">データベースファイル名</param>
    ''' <param name="itemName">自動更新状態を確認する項目名</param>
    ''' <param name="intInterval">更新間隔(戻り値)</param>
    ''' <param name="blnAutoUpdate">自動更新状態(戻り値)</param>
    Public Sub CheckAutoUpdate(ByVal DataFile As String, ByVal itemName As String, ByRef intInterval As Integer, ByRef blnAutoUpdate As Boolean)

        ''Dim DataFile As String
        'Dim DbCon As New OleDb.OleDbConnection
        ''Dim DbDr As OleDb.OleDbDataReader
        ''Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String = ""

        blnAutoUpdate = False
        intInterval = 600000

        ''DataFile = Server.MapPath(SiteDirectory + "\App_Data\MenuInfo.mdb")

        If System.IO.File.Exists(DataFile) = True Then

            Using DbCon As New OleDbConnection
                DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DataFile & ";" & "Jet OLEDB:Engine Type= 5"
                ''DbCon.Open()

                strSQL = ("SELECT [自動更新有効],[更新間隔(ms)] FROM 自動更新 WHERE 自動更新項目 = '" & itemName & "'")

                Using DbDa As New OleDbDataAdapter(strSQL, DbCon)
                    'DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)

                    Using DtSet As New DataSet("DData")

                        DbDa.Fill(DtSet, "DData")

                        If DtSet.Tables("DData").Rows.Count > 0 Then
                            blnAutoUpdate = DtSet.Tables("DData").Rows(0).Item(0)
                            intInterval = DtSet.Tables("DData").Rows(0).Item(1)
                        Else
                            blnAutoUpdate = False
                            intInterval = 60000
                        End If

                        ''DbCom.Connection = DbCon
                        ''DbCom.CommandText = strSQL
                        ''DbDr = DbCom.ExecuteReader

                        ''If DbDr.HasRows = True Then
                        ''    DbDr.Read()
                        ''    blnAutoUpdate = DbDr.GetBoolean(2)
                        ''    intInterval = DbDr.GetInt32(3)
                        ''End If

                        ''DbDr.Close()                            'データリーダを閉じる
                        ''DbCom.Dispose()
                        ''DbCon.Close()
                        'DbDa.Dispose()
                        'DtSet.Dispose()
                        'DbCon.Dispose()
                    End Using
                End Using
            End Using
        End If

        ' ''タイマーの有効／無効を設定
        ''Me.Tmr_GraphUpdate.Enabled = blnAutoUpdate
        ''Me.Tmr_GraphUpdate.Interval = intInterval

    End Sub
    ''' <summary>
    ''' チャンネルの指定文字列を配列に入れる
    ''' </summary>
    ''' <param name="outCh">文字列を変換したチャンネルを入れた配列(文字列)</param>
    ''' <param name="strDatas">チャンネルを指定した文字列</param>
    ''' <param name="ascending">ソートフラグ</param>
    ''' <returns>カンマ区切りの文字列(ソートフラグによらない)</returns>
    ''' <remarks>第1引数のみソートフラグによるソートを行う。戻り値のStringは第2引数の順序のまま</remarks>    
    Public Function GetOutputChannel(ByRef outCh() As Integer, ByVal strDatas As String, Optional ByVal ascending As Short = 1) As String
        ''
        '' 設定情報文字列から出力チャンネルを配列に格納しソートする(0:そのまま　1:昇順　2:降順)       
        ''
        Dim jloop As Integer
        Dim kloop As Integer
        Dim strTemp() As String = Nothing
        Dim strTemp2() As String = Nothing
        Dim st As Integer = 0
        Dim ed As Integer = 0
        Dim dummyCount As Integer = 0
        Dim OutChTemp() As String
        Dim strOutCh As String = ""
        Dim strTempOutCh() As String = Nothing
        Dim strText As New System.Text.StringBuilder()

        '出力チャンネルチェック
        strTemp = strDatas.Split(",")

        If strTemp.Length = 1 Then                                      '●カンマがなかった場合

            strTemp2 = strTemp(0).Split("-")
            If strTemp2.Length = 1 Then                                 '   ■- がなかった場合
                ReDim outCh(0)
                outCh(0) = strTemp2(0)                                  '       1chのみ
                'strOutCh = (strTemp2(0) + ",")
                strText.Append(strTemp2(0) & ",")
            Else                                                        '   ■- があった場合
                st = Integer.Parse(strTemp2(0))
                ed = Integer.Parse(strTemp2(1))
                ReDim outCh((ed - st))
                For jloop = st To ed                                    '   30-40　なら　30～40 を配列に入れる
                    'outCh(dummyCount) = jloop
                    dummyCount += 1
                    strText.Append(jloop.ToString & ",")
                    'strOutCh += (jloop.ToString + ",")
                Next
            End If

            If strText.Length <> 0 Then
                strText.Remove(strText.Length - 1, 1)
            End If

            'If strOutCh.Length <> 0 Then
            '    strOutCh = strOutCh.Substring(0, strOutCh.Length - 1)
            'End If

        Else                                                            '●カンマで区切られていた場合

            For jloop = 0 To strTemp.Length - 1

                strTemp2 = strTemp(jloop).Split("-")
                If strTemp2.Length = 1 Then                             '   ■- がなかった場合
                    strText.Append(strTemp2(0) & ",")
                    'strOutCh += (strTemp2(0) + ",")

                Else                                                    '   ■- があった場合
                    st = Integer.Parse(strTemp2(0))
                    ed = Integer.Parse(strTemp2(1))

                    For kloop = st To ed                                '   30-40　なら　30～40 を配列に入れる
                        strText.Append(kloop.ToString & ",")
                        'strOutCh += (kloop.ToString + ",")
                    Next
                End If

            Next
            'If strOutCh.Length <> 0 Then
            '    strOutCh = strOutCh.Substring(0, strOutCh.Length - 1)
            'End If

            If strText.Length <> 0 Then
                strText.Remove(strText.Length - 1, 1)
            End If

            ''OutChTemp = strOutCh.Split(",")                             '文字列を配列に変換
            ''ReDim outCh(OutChTemp.Length - 1)
            ''For jloop = 0 To OutChTemp.Length - 1                       'stringの配列をintegerの配列に入れなおす
            ''    outCh(jloop) = Integer.Parse(OutChTemp(jloop))
            ''Next

            ''Select Case ascending                                       '0なら何もしない
            ''    Case 1
            ''        Array.Sort(outCh)                                   '配列を昇順でソート(デフォルト)
            ''    Case 2
            ''        Array.Reverse(outCh)                                '配列を降順でソート
            ''End Select

            'If ascending = 1 Then
            '    Array.Sort(outCh)                                       '配列を昇順でソート(デフォルト)
            'Else
            '    Array.Reverse(outCh)                                    '配列を降順でソート
            'End If

        End If

        strOutCh = strText.ToString
        OutChTemp = strOutCh.Split(",")                             '文字列を配列に変換
        ReDim outCh(OutChTemp.Length - 1)
        For jloop = 0 To OutChTemp.Length - 1                       'stringの配列をintegerの配列に入れなおす
            outCh(jloop) = Integer.Parse(OutChTemp(jloop))
        Next

        If ascending > 0 Then
            Select Case ascending                                       '0なら何もしない
                Case 1
                    Array.Sort(outCh)                                   '配列を昇順でソート(デフォルト)
                Case 2
                    Array.Reverse(outCh)                                '配列を降順でソート
            End Select

            ''strText.Length = 0                                        ' これはやめた・・・
            ''For jloop = 0 To outCh.Length - 1                           'ソート済みの文字列を作成するためにループする
            ''    strText.Append(outCh(jloop).ToString + ",")
            ''Next

            ''If strText.Length <> 0 Then                                 '最後の不要なカンマを取り除く
            ''    strText.Remove(strText.Length - 1, 1)
            ''End If
            ''strOutCh = strText.ToString

        End If


        Return strOutCh

    End Function
    ''' <summary>
    ''' 四捨五入もしくは切捨てを行う
    ''' </summary>
    ''' <param name="calcData">演算結果数値</param>
    ''' <param name="DataFormat">フォーマット</param>
    ''' <param name="TruncFlg">切捨てフラグ</param>
    ''' <param name="addSign">+符号をつけるかどうか</param>
    ''' <returns>四捨五入もしくは切捨てした後の数値を成形した文字列</returns>
    Public Function trunc_round(ByVal calcData As Single, ByVal DataFormat As String, ByVal TruncFlg As Boolean, Optional ByVal addSign As Boolean = False) As String
        Dim sngTemp As Single
        Dim calcTemp As Long
        Dim format_DecimalPoint As Integer
        Dim decCount As Short
        Dim strRet As String = ""

        format_DecimalPoint = DataFormat.IndexOf(".")                           'フォーマットの小数点の位置を探す
        If format_DecimalPoint <> -1 Then                                       'フォーマットの小数点以降の文字数をカウント
            decCount = DataFormat.Substring(format_DecimalPoint + 1).Length
        Else
            decCount = 0
        End If

        If calcData > 1.1E+30 Then                                                          ' 2012/02/24 Kino Add 異常値判定を追加
            strRet = "--"
        Else

            If TruncFlg = True Then                             '切捨てのとき
                Try                                                                         ' 2012/02/24 Kino Add 例外処理を追加
                    sngTemp = calcData * (10 ^ decCount)                                    '10の小数点以下の数乗して整数値とする
                    calcTemp = Decimal.Truncate(sngTemp)                                    '小数点以下を切捨て
                    If addSign = False Then
                        strRet = (calcTemp * (0.1 ^ decCount)).ToString(DataFormat)
                    Else
                        strRet = (calcTemp * (0.1 ^ decCount)).ToString("+" + DataFormat + ";-" + DataFormat + ";+" + DataFormat)   '表示用の成形
                    End If
                Catch ex As Exception
                    strRet = "--"
                End Try

            Else                                                '四捨五入のとき

                Try                                                                         ' 2012/02/24 Kino Add 例外処理を追加
                    sngTemp = Math.Round(calcData, decCount, MidpointRounding.AwayFromZero)
                    If addSign = False Then
                        strRet = sngTemp.ToString(DataFormat)
                    Else
                        strRet = sngTemp.ToString("+" + DataFormat + ";-" + DataFormat + ";+" + DataFormat)
                    End If
                Catch ex As Exception
                    strRet = "--"
                End Try

            End If
        End If

        Return strRet

    End Function

    ''' <summary>
    ''' 文字列の日付をDate型に変換する
    ''' </summary>
    ''' <param name="strDate">「：／」抜きの文字列日付</param>
    ''' <returns>日付型に変換した日付</returns>
    Public Function exchange2Date(ByVal strDate As String) As Date

        Dim strTemp As String
        Dim dtRet As Date = Date.MinValue

        If strDate.Length = 12 Then             ' yyyyMMddhhmm の場合
            strTemp = strDate.Substring(0, 4) + "/" + strDate.Substring(4, 2) + "/" + strDate.Substring(6, 2) + " " + strDate.Substring(8, 2) + ":" + strDate.Substring(10, 2)
            If DateTime.TryParse(strTemp, dtRet) = True Then
                ''Return dtRet
            Else
                dtRet = Date.MinValue
                ''Return Date.MinValue
            End If
        Else

        End If

        Return dtRet

    End Function


    ''' <summary>
    ''' データベース内に指定テーブルが存在するか確認する
    ''' </summary>
    ''' <param name="dbcon">データベースコネクション</param>
    ''' <param name="tableName">検索するテーブル名</param>
    ''' <returns>存在する場合はTrue</returns>
    ''' <remarks></remarks>
    Public Function isExistTable(ByVal dbcon As OleDb.OleDbConnection, ByVal tableName As String) As Boolean

        Dim Dt As DataTable
        Dim iloop As Integer
        Dim tableNames As String = Nothing
        Dim blnRet As Boolean = False

        With dbcon
            ' テーブル一覧を取得
            .Open()
            'Dt = DbCon.GetOleDbSchemaTable(OleDb.OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, "TABLE"})
            Dt = .GetSchema("Tables")
            For iloop = 0 To Dt.Rows.Count - 1
                tableNames += ("@" & Dt.Rows(iloop)("TABLE_NAME").ToString & "@")
            Next
            .Close()
        End With

        If tableNames.IndexOf(tableName) <> -1 Then
            blnRet = True
        End If

        Return blnRet

    End Function

    ''' <summary>
    ''' 色番号から、16進数表示に変換のための処理
    ''' </summary>
    ''' <param name="baseColor">色番号</param>
    ''' <returns>16進数</returns>    
    ''' <remarks>2016/10/06 Kino Add</remarks>
    Public Function getColSeparate(ByVal baseColor As Integer) As String

        Dim strRet As String = Nothing
        Dim R As Integer
        Dim G As Integer
        Dim B As Integer
        Dim strTemp As String

        R = System.Drawing.Color.FromArgb(baseColor).R
        G = System.Drawing.Color.FromArgb(baseColor).G
        B = System.Drawing.Color.FromArgb(baseColor).B

        strTemp = System.Drawing.Color.FromArgb(255, R, G, B).Name     ' 透過はしない
        strRet = "#" & strTemp.Substring(2)

        Return strRet

    End Function

    ''' <summary>
    ''' テキストファイルの内容の全読込
    ''' </summary>
    ''' <returns>テキストファイルの内容を配列にして返す</returns>
    ''' <remarks></remarks>
    Public Function ReadTextFile(ByVal FilePath As String) As String()

        Dim strRet() As String = Nothing

        Try
            If IO.File.Exists(FilePath) = True Then
                Dim enc As System.Text.Encoding = System.Text.Encoding.GetEncoding("shift_jis")
                'テキストファイルの中身をすべて読み込む
                Dim str As String = IO.File.ReadAllText(FilePath, enc)

                Dim temp As String = str.Replace(Convert.ToChar(&HA), "")
                temp = temp.TrimEnd(Convert.ToChar(&HD))
                Dim strTemp() As String = temp.Split(Convert.ToChar(&HD))

                strRet = strTemp
            End If
        Catch ex As Exception
            strRet(0) = "File Read Error"
        End Try

        Return strRet

    End Function

End Class

