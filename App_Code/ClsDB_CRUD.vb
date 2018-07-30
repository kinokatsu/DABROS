Option Explicit On
Option Strict On
Imports System.Data
Imports System.Data.OleDb
Imports System.Diagnostics
'ADOXとADODBを参(Microsoft ActiveX Data Objects 2.8 LibraryとMicrosoft ADO Ext. 2.8 for DDL and Security)
'Windows Server 2003ではこのバージョン以上だとエラーがでる。DBのファイル作成時のみしか使用しないため、
'DBファイルを事前に用意して、それをコピーする方法に移行し、参照をやめる。
'文字列フィールドはテキスト型 (Text) であるメモ型 (Memo) や文字型 (Character) がある。　CHARにすると、指定のサイズで文字列が生成される(空白をパディング)

Public Class DataUpd
    Property LoggerID As String
    Property FieldName As String()
    Property SensorSymbol As String()
    Property SensorUnit As String()
    Property AlertInfo As New List(Of Single())
    Property AlertStatus As Integer()
    Property AlertOn As Boolean = False
    Property UpdDate As New List(Of DateTime)
    Property UpdDatas As New List(Of Single())
    Property StandAloneOpedFlg As Integer = 0
End Class

Public Class DB_CRUD

    'Shared connection As OleDbConnection = Nothing
    Private connection As OleDbConnection = Nothing

    Dim _readClientData() As Array

    'Public Property deleteClientHandle As IntPtr
    Public Property dbFilePath As String
    Public Property exError As Exception = Nothing
    Public Property exMessage As String
    Public Property TBLName As String
    Public Property QueryString As String
    Public Property SaveData As String()
    Public Property SaveChData As String()
    Public Property TableCount As Integer = 1
    Public Property RecordExist As Boolean = False
    Public Property RecordExistValue As Integer
    Private AddRecordID As Integer

    ''' <summary>
    ''' 読み込んだテーブルのレコードを返すプロパティ
    ''' </summary>
    ''' <value>－</value>
    ''' <returns>レコードのデータ</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property readClientData() As Array
        Get
            readClientData = _readClientData
        End Get
    End Property

    ''' <summary>
    ''' テーブルのレコードを格納した変数のクリア
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub clearData()

        If _readClientData IsNot Nothing Then
            Array.Clear(_readClientData, 0, _readClientData.Length)
            exError = Nothing
        End If

    End Sub

    ''' <summary>
    ''' 標準システム向けのデータファイルにおけるテーブル名を作成する
    ''' </summary>
    ''' <param name="TblNo">テーブル番号</param>
    ''' <returns>テーブル名</returns>
    ''' <remarks></remarks>
    Private Function MakeTableName(TblNo As Integer) As String

        Dim strRet As String = Nothing
        Dim StCh As String = Nothing
        Dim EdCh As String = Nothing
        Dim TableNo As Integer

        TableNo = (TblNo - 1) * 100
        StCh = (TableNo + 1).ToString.PadLeft(4, "0"c)
        EdCh = (TableNo + 100).ToString.PadLeft(4, "0"c)
        strRet = String.Format("C{0}_C{1}", StCh, EdCh)

        Return strRet

    End Function

    ''' <summary>
    ''' テーブルを作成（日付のテーブル）
    ''' </summary>
    ''' <param name="DBPath">データベースファイルのパス</param>
    ''' <returns> テーブル作成の成否 </returns>
    ''' <remarks></remarks>
    Private Function makeTable0(DBPath As String) As Integer

        Dim IntRet As Integer = -1
        'Dim cn As OleDbConnection
        Dim TableLoop As Integer
        Dim StrSQL As String = Nothing
        Dim StCh As String = Nothing
        Dim EdCh As String = Nothing

        Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & DBPath.ToString)

        Using cn As New OleDbConnection
            'cn = New OleDbConnection(connectionString)
            cn.ConnectionString = connectionString

            For TableLoop = 1 To TableCount

                StrSQL = Nothing
                'SQL作成
                StrSQL = "CREATE TABLE [日付] (日付ID IDENTITY(1,1) PRIMARY KEY,日付 DATETIME)"

                If StrSQL IsNot Nothing Then
                    Try
                        'Dim oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                        Using oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                            cn.Open()
                            oleCmd.ExecuteNonQuery()
                            IntRet = 0
                        End Using
                    Catch ex As Exception
                        exError = ex
                    Finally
                        cn.Close()
                    End Try
                Else

                End If

            Next
            'cn.Dispose()
        End Using

        Return IntRet

    End Function

    ''' <summary>
    ''' テーブルを作成（データのテーブル）
    ''' </summary>
    ''' <param name="DBPath">データベースファイルのパス</param>
    ''' <returns> テーブル作成の成否 </returns>
    ''' <remarks></remarks>
    Private Function makeTable1(DBPath As String) As Integer

        Dim IntRet As Integer = -1
        'Dim cn As OleDbConnection
        Dim TableLoop As Integer
        Dim FieldLoop As Integer
        Dim FieldChSt As Integer
        Dim FieldChEd As Integer
        Dim StrSQL As String = Nothing
        Dim TBLoop As Integer

        Dim TblName As String

        Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & DBPath.ToString)

        Using cn As New OleDbConnection
            'cn = New OleDbConnection(connectionString)
            cn.ConnectionString = connectionString

            For TableLoop = 1 To TableCount
                TblName = MakeTableName(TableLoop)
                TBLoop = (TableCount - 1) * 100
                FieldChSt = (TBLoop + 1)
                FieldChEd = (TBLoop + 100)
                Dim ChField As String = Nothing

                StrSQL = Nothing
                'SQL作成
                StrSQL = String.Format("CREATE TABLE [{0}] (日付ID INTEGER4 PRIMARY KEY NOT NULL,", TblName)

                For FieldLoop = FieldChSt To FieldChEd
                    ChField &= String.Format("Ch{0} REAL DEFAULT 7.7E+30,", FieldLoop)
                Next

                ChField = ChField.TrimEnd(Convert.ToChar(","))

                StrSQL &= (ChField & ")")

                If StrSQL IsNot Nothing Then
                    Try
                        Using oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                            'Dim oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                            cn.Open()
                            oleCmd.ExecuteNonQuery()
                            IntRet = 0
                        End Using
                    Catch ex As Exception
                        exError = ex
                    Finally
                        cn.Close()
                    End Try
                Else

                End If

            Next

            'cn.Dispose()
        End Using

        Return IntRet

    End Function

    ''' <summary>
    ''' 汎用のテーブル作成メソッド
    ''' </summary>
    ''' <param name="DBPath">データベースファイルのパス</param>
    ''' <returns> テーブル作成の成否 </returns>
    ''' <remarks></remarks>
    Public Function makeTableGeneral(DBPath As String, Query As String) As Integer

        Dim IntRet As Integer = -1
        'Dim cn As OleDbConnection
        Dim StrSQL As String = Nothing
        Dim StCh As String = Nothing
        Dim EdCh As String = Nothing

        Try
            Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & DBPath.ToString)

            Using cn As New OleDbConnection

                'cn = New OleDbConnection(connectionString)
                cn.ConnectionString = connectionString

                'SQL作成
                StrSQL = Query
                '        "CREATE TABLE [共通情報] (DataBaseCh INTEGER2 PRIMARY KEY NOT NULL,BlockNo INTEGER2,現場DBCh INTEGER2," &
                '"計器記号 TEXT(32),出力単位 TEXT(32),種別 TEXT(64),タイトル TEXT(128),設置位置 REAL," &
                '"初期値 REAL,係数 REAL,オフセット REAL,警報値上限1 REAL,警報値上限2 REAL,警報値上限3 REAL," &
                '"警報値下限1 REAL,警報値下限2 REAL,警報値下限3 REAL,警報判定結果 INTEGER2,警報判定時刻 DATETIME,最新データ REAL)"

                If StrSQL IsNot Nothing Then
                    Try
                        'Dim oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                        Using oleCmd As New OleDbCommand(StrSQL, cn)
                            cn.Open()
                            oleCmd.ExecuteNonQuery()
                            cn.Close()
                        End Using
                        IntRet = 0
                    Catch ex As Exception
                        exError = ex
                    End Try
                Else

                End If
            End Using

        Catch ex As Exception
            exError = ex
        End Try

        Return IntRet

    End Function

    ''' <summary>
    ''' データベースのファイルのみ作成(入れ物だけ。中身は別途)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>アクセスのファイルは、これ以外で作成する方法なし・・・
    ''' 古いOSで参照によって動かない場合があるので、ソースファイルを用意するようにした</remarks>
    Public Function MakeDatabaseFile(FilePath As String) As Integer        ', Optional ByVal PassWd As String = Nothing) As Integer

        Dim intRet As Integer = -1
        Dim SrcDBPath As String = IO.Path.Combine(System.Windows.Forms.Application.ExecutablePath, "Stored", "source.mdb")        ' 元のDBファイルパス  ★ 要確認

        Try
            IO.File.Copy(SrcDBPath, FilePath, False)
            intRet = 0
        Catch ex As Exception

        End Try

        ''Dim con As ADODB.Connection
        ''Dim cat As New ADOX.Catalog

        ''Try
        ''    cat = CType(Microsoft.VisualBasic.CreateObject("ADOX.Catalog"), ADOX.Catalog)
        ''    ''If PassWd Is Nothing Then
        ''    cat.Create("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FilePath & ";Jet OLEDB:Engine Type= 5") '& ";Jet OLEDB:Engine Type= 5;Jet OLEDB:Encrypt Database=True")
        ''    ''Else
        ''    ''    cat.Create(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Database Password={1}", FilePath, PassWd)) '& ";Jet OLEDB:Engine Type= 5;Jet OLEDB:Encrypt Database=True;Database Password="")
        ''    ''End If
        ''    'コネクションを開放する。これをやらないと、ldbファイルが残ってしまう　プログラム終了しないと消せない
        ''    con = CType(cat.ActiveConnection, ADODB.Connection)
        ''    con.Close()
        ''    intRet = 0
        ''Catch ex As Exception
        ''    exError = ex
        ''Finally
        ''    cat = Nothing
        ''    con = Nothing
        ''End Try

        Return intRet

    End Function

    ''' <summary>
    ''' 標準システムと同じ形式のデータファイルを作成するメインメソッド（R値保存用）
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MakeStandardSystemDBFile() As Integer

        Dim intMK0 As Integer
        Dim intMK1 As Integer

        If IO.File.Exists(dbFilePath) = False Then

            Dim intRet As Integer = MakeDatabaseFile(dbFilePath)                                        ' DBの入れ物を作成

            exError = Nothing
            Threading.Thread.Sleep(1000)

            If intRet = 0 Then
                intMK0 = makeTable0(dbFilePath)
                intMK1 = makeTable1(dbFilePath)
            End If

        End If

        Return (intMK0 + intMK1)

    End Function

    ''' <summary>
    ''' 標準システムと同じ形式のデータファイルを作成するメインメソッド（DABROS保存用）
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MakeDABROSDBFile() As Integer

        Dim intMK0 As Integer
        Dim intMK1 As Integer
        Dim intMK2 As Integer

        If IO.File.Exists(dbFilePath) = False Then

            Dim intRet As Integer = MakeDatabaseFile(dbFilePath)                            ' DBの入れ物を作成

            exError = Nothing
            Threading.Thread.Sleep(100)

            If intRet = 0 Then
                intMK0 = makeTable0(dbFilePath)
                intMK1 = makeTable1(dbFilePath)
                'intMK2 = makeTable2(dbFilePath)
            End If

        End If

        Return ((intMK0 * 100) + (intMK1 * 10) + intMK2)

    End Function

    ''' <summary>
    ''' ACCESS MDBファイルのパスワード変更
    ''' </summary>
    ''' <param name="DBPath">MDBファイルPATH</param>
    ''' <param name="oldPassWd">旧パスワード</param>
    ''' <param name="newPassWd">新パスワード</param>
    ''' <remarks></remarks>
    Public Function SetPassword2DB(ByVal DBPath As String, ByVal oldPassWd As String, ByVal newPassWd As String) As Integer

        Dim intRet As Integer = -1
        'Dim dbCon As OleDbConnection = New OleDbConnection()
        Using dbCon As New OleDbConnection

            Try

                ' ACCESS MDB オープン
                Dim cst As String = Nothing
                cst = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DBPath & ";")

                If oldPassWd <> "" Then
                    cst &= String.Format("Jet OLEDB:Database Password={0};", oldPassWd)
                End If
                cst &= "Mode=Share Exclusive;"
                dbCon.ConnectionString = cst
                dbCon.Open()

                ' 旧パスワード用のSQL生成
                If oldPassWd = "" Then
                    oldPassWd = "NULL"
                Else
                    oldPassWd = String.Format("[{0}]", oldPassWd)
                End If

                ' 新パスワード用のSQL生成
                If newPassWd = "" Then
                    newPassWd = "NULL"
                Else
                    newPassWd = String.Format("[{0}]", newPassWd)           '"[" & newPassWd & "]"
                End If

                ' ACCESS MDB パスワード変更
                Dim sql As String = String.Format("ALTER DATABASE PASSWORD {0} {1}", newPassWd, oldPassWd)
                'Dim dbCmd As OleDbCommand = New OleDbCommand(sql, dbCon)
                Using dbCmd As OleDbCommand = New OleDbCommand(sql, dbCon)
                    dbCmd.ExecuteNonQuery()
                End Using
                ' '' ACCESS MDB クローズ
                ''dbCon.Close()
                ''dbCon.Dispose()
                intRet = 0

            Catch ex As Exception
                exError = ex
                ''Finally
                ''    ' ACCESS MDB クローズ
                ''    dbCon.Close()
                ''    dbCon.Dispose()
            End Try

        End Using

        Return intRet

    End Function

    ''' <summary>
    ''' 現場情報のデータファイルを作成するメインメソッド（各種テーブルあり）@現場情報　　例外処理時のステートメントを追加する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MakeFieldInfoDBFile() As Integer

        Dim intRet As Integer = -1
        'Dim cn As New OleDbConnection

        If IO.File.Exists(dbFilePath) = False Then

            Try

                Dim intRet2 As Integer = MakeDatabaseFile(dbFilePath)                                        ' DBの入れ物を作成

                Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath.ToString)
                Using cn As New OleDbConnection
                    cn.ConnectionString = connectionString
                    'cn = New OleDbConnection(connectionString)

                    cn.Open()
                    intRet = makeTable_CalcParam(cn, 100)               ' 演算パラメータ用テーブル作成
                    intRet = makeTable_SensorParam(cn, 100)             ' センサー情報テーブル作成
                    intRet = makeTable_Commons(cn, "CREATE TABLE [FieldInfo] (FieldID IDENTITY(1,1) PRIMARY KEY NOT NULL,現場名 TEXT(64),FolderName TEXT(128),Complete YESNO)")     ' 現場情報テーブル作成
                    intRet = makeTable_Commons(cn, "CREATE TABLE [LoggerInfo] (RecordID IDENTITY(1,1) PRIMARY KEY NOT NULL,FieldID INTEGER4,LoggerID TEXT(8)," &
                                               "センサー台数 INTEGER2,Complete YESNO,MeasHour TEXT(6),MeasMinute TEXT(15),SendHour TEXT(6),SendMinute TEXT(15)," &
                                               "AutoTimeAdjust YESNO,StoredState INTEGER2,iSENSORID TEXT(10),TOAMIID INTEGER2,MeasHigh INTEGER2 ,MeasLow INTEGER2," &
                                               "NewestDate DATETIME,ReceivedDate DATETIME,IntervalMode INTEGER2,AlertCallOff_DelayTime INTEGER2,Calloff_Time DATETIME," &
                                               "ModeChangeJudgeCh INTEGER2,ForceCommand INTEGER2,UpdateMonitoring YESNO)")                                   ' ロガー情報テーブル作成
                    intRet = makeTable_Commons(cn, "CREATE TABLE [ParamTypes] (ID INTEGER2 PRIMARY KEY NOT NULL,種別 TEXT(32))")                                                    ' 種別情報テーブル
                    intRet = makeTable_Commons(cn, "CREATE TABLE [CalcTypes] (ID INTEGER2 PRIMARY KEY NOT NULL,種別 TEXT(16),単位 TEXT(32))")                                        ' 演算番号情報テーブル
                    intRet = makeTable_Commons(cn, "CREATE TABLE [MeasInterval] (ID INTEGER2 PRIMARY KEY NOT NULL,間隔 INTEGER2 NOT NULL)")                                         ' 測定間隔(モード)情報テーブル
                    intRet = 0
                    cn.Close()
                End Using
            Catch ex As Exception
                exError = ex
                'Finally
                '    If cn.State = ConnectionState.Open Then
                '        cn.Close()
                '        cn.Dispose()
                '    End If
            End Try
        End If

        Return intRet

    End Function


    ''' <summary>
    ''' テーブルを作成（ParamTypes（種別）のテーブル）@現場情報
    ''' </summary>
    ''' <param name="cn">データベースコネクション</param>
    ''' <param name="SQL">クエリ文字列</param>
    ''' <returns> テーブル作成の成否 </returns>
    ''' <remarks></remarks>
    Private Function makeTable_Commons(cn As OleDbConnection, SQL As String) As Integer

        Dim IntRet As Integer = -1
        Dim StrSQL As String = Nothing

        StrSQL = Nothing
        'SQL作成
        StrSQL = SQL

        If StrSQL IsNot Nothing Then
            Try
                Using oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                    'Dim oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                    oleCmd.ExecuteNonQuery()
                    IntRet = 0
                End Using
            Catch ex As Exception
                exError = ex
            End Try
        Else

        End If

        Return IntRet

    End Function

    ''' <summary>
    ''' テーブルを作成（SensorParams（単位や記号）のテーブル）@現場情報
    ''' </summary>
    ''' <returns> テーブル作成の成否 </returns>
    ''' <remarks></remarks>
    Private Function makeTable_SensorParam(cn As OleDbConnection, ChCount As Integer) As Integer

        Dim IntRet As Integer = -1
        Dim FieldLoop As Integer
        Dim StrSQL As String = Nothing
        Dim ChField As String = Nothing

        'SQL作成
        StrSQL = "CREATE TABLE [SensorParams] (RecordID IDENTITY(1,1) PRIMARY KEY NOT NULL,ロガーID TEXT(8),Disable YESNO,Type INTEGER2,SetTime DATETIME,"

        For FieldLoop = 1 To ChCount
            ChField &= String.Format("Ch{0} TEXT(16) DEFAULT Undefined,", FieldLoop)
        Next

        ChField = ChField.TrimEnd(Convert.ToChar(","))

        StrSQL &= (ChField & ")")

        If StrSQL IsNot Nothing Then
            Try
                Using oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                    'Dim oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                    oleCmd.ExecuteNonQuery()
                    IntRet = 0
                End Using
            Catch ex As Exception
                exError = ex
            End Try
        Else

        End If

        Return IntRet

    End Function

    ''' <summary>
    ''' テーブルを作成（CalcParams（初期値や係数）のテーブル）@現場情報
    ''' </summary>
    ''' <returns> テーブル作成の成否 </returns>
    ''' <remarks></remarks>
    Private Function makeTable_CalcParam(cn As OleDbConnection, ChCount As Integer) As Integer

        Dim IntRet As Integer = -1
        Dim FieldLoop As Integer
        Dim StrSQL As String = Nothing
        Dim ChField As String = Nothing
        Dim sb As New StringBuilder

        'SQL作成
        StrSQL = "CREATE TABLE [CalcParams] (RecordID IDENTITY(1,1) PRIMARY KEY NOT NULL,ロガーID TEXT(8),Disable YESNO,Type INTEGER2,SetTime DATETIME,"

        For FieldLoop = 1 To ChCount
            sb.Append(String.Format("Ch{0} REAL DEFAULT 7.7E+30,", FieldLoop))
            'ChField &= String.Format("Ch{0} REAL DEFAULT 7.7E+30,", FieldLoop)
        Next

        ChField = sb.ToString
        ChField = ChField.TrimEnd(Convert.ToChar(","))

        sb.Length = 0

        StrSQL &= (ChField & ")")

        If StrSQL IsNot Nothing Then
            Try
                'Dim oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                Using oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                    oleCmd.ExecuteNonQuery()
                    IntRet = 0
                End Using

            Catch ex As Exception
                exError = ex

            End Try
        Else

        End If

        Return IntRet

    End Function

    ''' <summary>
    ''' Toami関連の設定ファイルを作成するメインメソッド（各種テーブルあり）@iSensor　　例外処理時のステートメントを追加する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MakeToamiInfoDBFile() As Integer

        Dim intRet As Integer = -1

        'Dim cn As New OleDbConnection
        Using cn As New OleDbConnection

            If IO.File.Exists(dbFilePath) = False Then

                Try
                    Dim intRet2 As Integer = MakeDatabaseFile(dbFilePath)                                        ' DBの入れ物を作成
                    Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath.ToString)
                    'cn = New OleDbConnection(connectionString)
                    cn.ConnectionString = connectionString
                    cn.Open()

                    intRet = makeTable_Commons(cn, "CREATE TABLE [ToamiInfo] (ToamiID IDENTITY(1,1) PRIMARY KEY NOT NULL,HOST TEXT(32) ,Toamiパス TEXT(64) ,GWName TEXT(32) ,AppKey TEXT(64) ,GWName以降パス TEXT(64),LoggerID TEXT(8),NewstSendTime DATETIME,Complete YESNO)")       ' 共通情報

                Catch ex As Exception
                    exError = ex
                Finally
                    If cn.State = ConnectionState.Open Then
                        cn.Close()
                        ''cn.Dispose()
                    End If
                End Try

            End If

        End Using

        Return intRet

    End Function

    ''' <summary>
    ''' iSensor関連の設定ファイルを作成するメインメソッド（各種テーブルあり）@iSensor　　例外処理時のステートメントを追加する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MakeiSensorInfoDBFile() As Integer

        Dim intRet As Integer = -1
        'Dim cn As New OleDbConnection

        If IO.File.Exists(dbFilePath) = False Then

            Try

                Dim intRet2 As Integer = MakeDatabaseFile(dbFilePath)                                        ' DBの入れ物を作成
                Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath.ToString)

                Using cn As New OleDbConnection
                    'cn = New OleDbConnection(connectionString)
                    cn.ConnectionString = connectionString

                    cn.Open()
                    intRet = makeTable_Commons(cn, "CREATE TABLE [MailServerInfo] (ID IDENTITY(1,1) PRIMARY KEY NOT NULL,ServerName TEXT(64),SMTP TEXT(32),POP TEXT(32)," &
                                               "SMTP_Port INTEGER2,POP_Port INTEGER2,SMTP_Auth YESNO,PopBeforeSMTP YESNO,SSLTLS使用 YESNO,メール残す YESNO,アカウント TEXT(64)," &
                                               "Pass TEXT(64),SenderName TEXT(32),MailAddress TEXT(64),Complete YESNO)")                                  ' メールサーバー関連情報

                    intRet = makeTable_Commons(cn, "CREATE TABLE [SensorInfo] (ID IDENTITY(1,1) PRIMARY KEY NOT NULL,現場ID INTEGER2,個別番号 TEXT(10),センサータイプ TEXT(8)," &
                                                               "データ個数 INTEGER2,通常送信間隔 INTEGER2,イベント送信間隔 INTEGER2,MailServerID INTEGER2,BaseTime DATETIME," &
                                                               "EventStatus INTEGER2,NewestDate DATETIME,SaveStartCH INTEGER2,NewestData REAL,ALERT_ON REAL DEFAULT 7.7E+30," &
                                                               " ALERT_OFF REAL DEFAULT 7.7E+30,ALERT_JudgeCh TEXT(8),SaveDataInf TEXT(16),Complete YESNO)")           ' センサー情報

                    'intRet = makeTable_Commons(cn, "CREATE TABLE [SensorInfo] (iSENSORID IDENTITY(1,1) PRIMARY KEY NOT NULL,現場ID INTEGER2,個別番号 TEXT(10),センサータイプ TEXT(8)," &
                    '                                           "データ個数 INTEGER2,通常送信間隔 INTEGER2,イベント送信間隔 INTEGER2,ServerID INTEGER4,BaseTime DATETIME,Account TEXT(32)," &
                    '                                           "Pass_word TEXT(32),Address TEXT(64),SenderName TEXT(32),Mail_Delete YESNO,EventStatus YESNO,NewestDate DATETIME)")           ' センサー情報

                    intRet = 0
                    cn.Close()
                End Using
            Catch ex As Exception
                exError = ex
                'Finally
                '    If cn.State = ConnectionState.Open Then
                '        cn.Close()
                '        cn.Dispose()
                '    End If
            End Try
        End If

        Return intRet

    End Function

    ''' <summary>
    ''' iSensor関連の設定ファイルを作成するメインメソッド（各種テーブルあり）@iSensor　　例外処理時のステートメントを追加する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MakeDataCombineInfoDBFile() As Integer

        Dim intRet As Integer = -1
        'Dim cn As New OleDbConnection

        If IO.File.Exists(dbFilePath) = False Then

            Try

                Dim intRet2 As Integer = MakeDatabaseFile(dbFilePath)                                        ' DBの入れ物を作成
                Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath.ToString)
                Using cn As New OleDbConnection
                    'cn = New OleDbConnection(connectionString)
                    cn.ConnectionString = connectionString

                    cn.Open()
                    intRet = makeTable_Commons(cn, "CREATE TABLE [SendInfo] (ID IDENTITY(1,1) PRIMARY KEY NOT NULL,LoggerID TEXT(10)," &
                                               "BaseTime DATETIME,SendInterval INTEGER2,NewestDate DATETIME,CombineChInfo TEXT(255)," &
                                               "iSE_SaveCh TEXT(64),Complete YESNO,WebSave YESNO,SaveStartCh INTEGER2,MailSend YESNO," &
                                               "ContactControl YESNO,WebFolder TEXT(128),FTPNo INTEGER2,Con_IP TEXT(16), Con_Port INTEGER2," &
                                               "Toami YESNO)")           ' センサー情報
                    intRet = 0
                    cn.Close()
                End Using
            Catch ex As Exception
                exError = ex
                'Finally
                '    If cn.State = ConnectionState.Open Then
                '        cn.Close()
                '        cn.Dispose()
                '    End If
            End Try
        End If

        Return intRet

    End Function

    ''' <summary>
    ''' センサーデータの保存ファイルを作成するメインメソッド（各種テーブルあり）
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MakeCombineDataDBFile(TableName() As String) As Integer

        Dim intRet As Integer = -1
        'Dim cn As New OleDbConnection
        Dim TableLoop As Integer

        '        If IO.File.Exists(dbFilePath) = False Then

        Try

            If IO.File.Exists(dbFilePath) = False Then
                Dim intRet2 As Integer = MakeDatabaseFile(dbFilePath)                                        ' DBの入れ物を作成
            End If
            Using cn As New OleDbConnection
                Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath.ToString)
                'cn = New OleDbConnection(connectionString)
                cn.ConnectionString = connectionString
                cn.Open()

                For TableLoop = 0 To TableName.Length - 1
                    If isExistTable(cn, TableName(TableLoop)) = False Then               ' テーブルが存在しない場合には作成する
                        intRet = makeTable4CombineData(cn, TableName(TableLoop))
                    End If
                Next

                intRet = 0
                cn.Close()
            End Using
        Catch ex As Exception
            exError = ex
            'Finally
            '    If cn.State = ConnectionState.Open Then
            '        cn.Close()
            '        cn.Dispose()
            '    End If
        End Try
        'End If

        Return intRet

    End Function

    ''' <summary>
    ''' テーブルを作成（データのテーブル）
    ''' </summary>
    ''' <param name="cn">データベースのコネクション</param>
    ''' <param name="TableName">テーブル名</param>
    ''' <returns> テーブル作成の成否 </returns>
    ''' <remarks></remarks>
    Private Function makeTable4CombineData(cn As OleDbConnection, TableName As String) As Integer

        Dim IntRet As Integer = -1
        Dim FieldLoop As Integer
        Dim StrSQL As String = Nothing
        Dim ChField As String = Nothing
        Dim sb As New StringBuilder

        StrSQL = Nothing
        'SQL作成
        StrSQL = "CREATE TABLE [" & TableName & "] (ID IDENTITY(1,1) PRIMARY KEY NOT NULL,日付 DATETIME,"

        For FieldLoop = 1 To 100
            sb.Append(String.Format("Ch{0} REAL DEFAULT 7.7E+30,", FieldLoop))
            'ChField &= String.Format("Ch{0} REAL DEFAULT 7.7E+30,", FieldLoop)
        Next

        ChField = sb.ToString
        ChField = ChField.TrimEnd(Convert.ToChar(","))

        sb.Length = 0

        StrSQL &= (ChField & ")")

        If StrSQL IsNot Nothing Then
            Try
                'Dim oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                Using oleCmd As OleDbCommand = New OleDbCommand(StrSQL, cn)
                    oleCmd.ExecuteNonQuery()
                    IntRet = 0
                End Using
            Catch ex As Exception
                exError = ex
            End Try
        Else

        End If

        Return IntRet

    End Function

    ''' <summary>
    ''' テーブルの存在確認(クエリー版)
    ''' </summary>
    ''' <param name="DBFilePath">DBファイルパス</param>
    ''' <param name="TableName">確認するテーブル名</param>
    ''' <returns>テーブルの確認結果　あり:テーブル名　なし:Nothing</returns>
    ''' <remarks></remarks>
    Public Overloads Function isExistTable(DBFilePath As String, TableName As String) As String

        ' この方法だと、権限がないとかでMSysObjectsが読めない　Acccess2010では、変更できない？　ので、変更
        ' ''Dim strSQL As String = "SELECT MSysObjects.Type, MSysObjects.Name, MSysObjects.Flags FROM MSysObjects WHERE MSysObjects.Type=1 AND MSysObjects.Flags=0 ORDER BY MSysObjects.Type, MSysObjects.Name"
        ''Dim strSQL As String = "MSysObjects.Name FROM MSysObjects WHERE MSysObjects.Type=1 AND MSysObjects.Flags=0 ORDER BY MSysObjects.Type, MSysObjects.Name"
        ' ''Dim strSQL As String = "SELECT MSysObjects.Type, MSysObjects.Name, MSysObjects.Flags FROM MSysObjects ORDER BY MSysObjects.Type, MSysObjects.Name"
        ' ''Dim strSQL As String = "SELECT Name FROM MsysObjects WHERE Type = 1 AND Flags = 0"
        ''Dim dSet As New DataSet("TBNames")
        ''Dim cn As OleDbConnection = New OleDbConnection
        ''Dim dataAdp As New OleDbDataAdapter

        ''cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & DBFilePath)

        ''Try
        ''    dataAdp = New OleDbDataAdapter(strSQL, cn)    ' DB接続
        ''    dataAdp.Fill(dSet, "TBNames")

        ''    For Each DtRow As DataRow In dSet.Tables("TBNames").Rows
        ''        Dim TbNames As String = DtRow.Item(0).ToString
        ''        If TbNames = TableName Then
        ''            strRet = TbNames
        ''            Exit For
        ''        End If
        ''    Next
        ''Catch ex As Exception
        ''    strRet = ex.Message
        ''End Try

        Dim strRet As String = Nothing
        'Dim cn As OleDbConnection = New OleDbConnection
        Using cn As OleDbConnection = New OleDbConnection
            cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & DBFilePath)

            Try
                cn.Open()

                Dim schemaTable As DataTable = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, "TABLE"})
                Dim row As DataRow

                For Each row In schemaTable.Rows
                    Dim column As DataColumn = schemaTable.Columns.Item("TABLE_NAME")
                    'Console.WriteLine(row.Item(column))
                    If row.Item(column).ToString = TableName Then
                        strRet = TableName
                        Exit For
                    End If
                Next

                cn.Close()

            Catch ex As Exception
                strRet = ex.Message
            End Try
        End Using

        Return strRet

    End Function

    ''' <summary>
    ''' テーブルの存在確認
    ''' </summary>
    ''' <param name="cn">コネクションオブジェクト</param>
    ''' <param name="TableName">確認するテーブル名</param>
    ''' <returns>テーブルの確認結果　True:あり　False:なし</returns>
    ''' <remarks></remarks>
    Public Overloads Function isExistTable(cn As OleDbConnection, TableName As String) As Boolean

        Dim blnRet As Boolean = False
        'Dim dSet As New DataSet
        Dim CloseFlg As Integer = 0

        Try
            Dim row As DataRow
            If cn.State <> ConnectionState.Open Then                ' 2018/01/17 Kino Add オープンされていないとダメなので、チェックする
                cn.Open()
                CloseFlg = 1
            End If

            'Dim dataTable As DataTable = cn.GetSchema(Odbc.OdbcMetaDataCollectionNames.Tables)         ' ODBC
            Dim dataTable As DataTable = cn.GetSchema(OleDbMetaDataCollectionNames.Tables)              ' OLEDB
            'Dim dataTable As DataTable = cn.GetSchema("Tables")                                        ' OLEDB こっちでもよいようだ・・

            For Each row In dataTable.Rows
                Dim column As DataColumn = dataTable.Columns.Item("TABLE_NAME")
                'Console.WriteLine(row.Item(column))
                If row.Item(column).ToString = TableName Then
                    blnRet = True
                    Exit For
                End If
            Next

            If CloseFlg = 1 Then
                cn.Close()
                CloseFlg = 0
            End If

        Catch ex As Exception
            exError = ex
        End Try

        ''Dim iloop As Integer
        ''For iloop = 0 To dataTable.Rows.Count - 1
        ''    Dim TBName As String = dataTable.Rows(iloop)("TABLE_NAME").ToString
        ''Next

        Return blnRet

    End Function

    ''' <summary>
    ''' 指定ファイルのデータベースファイルからレコードを読み込む
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getDBDatas()

        'Dim cn As OleDbConnection = New OleDbConnection
        'Dim dataAdp As OleDbDataAdapter = Nothing
        'Dim dSet As New DataSet("DBData")
        'Dim cdteBuild As OleDbCommandBuilder = Nothing
        Dim DTable As DataTable = Nothing
        Dim iloop As Integer
        Dim recCount As Integer

        exError = Nothing                                   ' プロパティを初期化する(例外メッセージを削除する)

        Try
            Using cn As OleDbConnection = New OleDbConnection
                cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath)

                Using dataAdp As New OleDbDataAdapter(QueryString, cn)    ' DB接続
                    'dataAdp = New OleDbDataAdapter(QueryString, cn)    ' DB接続
                    Using dSet As New DataSet("DBData")
                        dataAdp.Fill(dSet, "DBData")                                                                ' データセット

                        Using cdteBuild As New OleDbCommandBuilder(dataAdp)                                                ' これが必要　
                            'cdteBuild = New OleDbCommandBuilder(dataAdp)                                                ' これが必要　
                            DTable = dSet.Tables("DBData")                                                              ' データテーブルに格納

                            recCount = DTable.Rows.Count - 1
                            ReDim _readClientData(recCount)

                            If recCount >= 0 Then                                                                       ' レコードがあったら処理する
                                For Each dtRow As DataRow In DTable.Rows
                                    _readClientData(iloop) = dtRow.ItemArray                                            ' 1レコード丸読み
                                    iloop += 1
                                Next
                            End If
                        End Using
                    End Using
                End Using
            End Using
        Catch ex As Exception
            exError = ex

            'Finally

            'If DTable IsNot Nothing Then
            '    DTable.Dispose()
            'End If

            'If cdteBuild IsNot Nothing Then
            '    cdteBuild.Dispose()
            'End If
            'If dSet IsNot Nothing Then
            '    dSet.Dispose()
            'End If
            'If dataAdp IsNot Nothing Then
            '    dataAdp.Dispose()
            'End If
            'cn.Dispose()
        End Try

    End Sub



    ''' <summary>
    ''' クエリ渡して、テーブルを指定し、複数フィールドのデータを更新する [原則、データ用DB保存]
    ''' </summary>
    ''' <param name="DataCollection">保存するデータのフィールド</param>
    ''' <returns>成否 0:成功　-1:失敗</returns>
    ''' <remarks></remarks>
    Public Overloads Function UpdateDataField(DataCollection As DataUpd) As Integer

        Dim intRet As Integer = -1
        'Dim cn As OleDbConnection = New OleDbConnection
        'Dim dataAdp As OleDbDataAdapter = Nothing
        'Dim dSet As DataSet = Nothing
        Dim FieldLoop As Integer
        Dim DataLoop As Integer                                     ' 測定日時数分のループ
        Dim strSQL As String = Nothing
        Dim SelCh As String = ""

        exError = Nothing

        'SQL文作成用フィールド名文字列の作成
        SelCh = "ID,日付,"
        For FieldLoop = 0 To DataCollection.FieldName.Count - 1
            SelCh &= String.Format("{0},", DataCollection.FieldName(FieldLoop))
        Next
        SelCh = SelCh.TrimEnd(Convert.ToChar(","))

        Try
            Using cn As OleDbConnection = New OleDbConnection
                cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath)

                For DataLoop = 0 To DataCollection.UpdDate.Count - 1
                    strSQL = String.Format("SELECT {0} FROM {1} WHERE 日付=#{2}#", SelCh, DataCollection.LoggerID, DataCollection.UpdDate(DataLoop))           ' クエリの作成

                    Using dataAdp As New OleDbDataAdapter(strSQL, cn)
                        'dataAdp = New OleDbDataAdapter(strSQL, cn)

                        Using dSet As New DataSet("Data")
                            'dSet = New DataSet
                            dataAdp.Fill(dSet, "Data")

                            If dSet.Tables("Data").Rows.Count > 0 Then
                                Using cmdbuilder As New OleDbCommandBuilder
                                    'Dim cmdbuilder As New OleDbCommandBuilder

                                    For FieldLoop = 0 To DataCollection.FieldName.Count - 1
                                        dSet.Tables("Data").Rows(0)(DataCollection.FieldName(FieldLoop)) = Convert.ToSingle(DataCollection.UpdDatas(DataLoop).GetValue(FieldLoop))
                                        cmdbuilder.DataAdapter = dataAdp
                                    Next FieldLoop

                                    dataAdp.Update(dSet, "Data")

                                    'cmdbuilder.Dispose()
                                    RecordExistValue = Convert.ToInt32(dSet.Tables("Data").Rows(0)(0))
                                    intRet = 0
                                End Using
                            Else
                                Dim dt As New DataTable
                                Try
                                    dt = dSet.Tables("Data")

                                    Dim dr As DataRow = dt.NewRow()
                                    dr("日付") = DataCollection.UpdDate(DataLoop)                                                                             ' 日付
                                    For FieldLoop = 0 To DataCollection.FieldName.Count - 1
                                        dr(DataCollection.FieldName(FieldLoop)) = Convert.ToSingle(DataCollection.UpdDatas(DataLoop).GetValue(FieldLoop))     ' 測定値
                                    Next
                                    dt.Rows.Add(dr)

                                    Using ComBuild As OleDbCommandBuilder = New OleDbCommandBuilder(dataAdp)
                                        'Dim ComBuild As OleDbCommandBuilder = New OleDbCommandBuilder(dataAdp)
                                        dataAdp.Update(dSet, "Data")
                                    End Using
                                Catch ex As Exception
                                Finally
                                    dt.dispose()
                                End Try
                            End If
                        End Using
                    End Using
                Next
                intRet = 0
            End Using
        Catch ex As Exception
            exError = ex
            Debug.WriteLine(String.Format("{0},{1},{2}", DateTime.Now, ex.Message, ex.StackTrace))
            'Finally
            '    If dSet IsNot Nothing Then
            '        dSet.Dispose()
            '    End If
            '    If dataAdp IsNot Nothing Then
            '        dataAdp.Dispose()
            '    End If
            '    cn.Dispose()

        End Try


        Return intRet

    End Function

    ''' <summary>
    ''' クエリ文字列のみを指定して、実行（更新、追加、削除、読込）する
    ''' </summary>
    ''' <param name="strSQL"></param>
    ''' <returns></returns>
    ''' <remarks>変更項目が多い場合には、これを使用する。OleDbCommandBuilder では、エラー（式が複雑すぎます。）になる場合に使用する。</remarks>
    Public Overloads Function UpdateDataField(strSQL As String) As Integer

        Dim intRet As Integer = -1
        'Dim cn As OleDbConnection = New OleDbConnection
        exError = Nothing

        Using cn As New OleDbConnection

            cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath)
            cn.Open()

            'Dim cmd As OleDbCommand = cn.CreateCommand
            Using cmd As OleDbCommand = cn.CreateCommand

                'Dim tranz As OleDbTransaction = cn.BeginTransaction
                Using tranz As OleDbTransaction = cn.BeginTransaction

                    cmd.Connection = cn
                    cmd.Transaction = tranz

                    Try
                        cmd.CommandText = strSQL
                        cmd.ExecuteNonQuery()

                        tranz.Commit()
                        intRet = 0

                    Catch ex As Exception
                        exError = ex
                        Debug.WriteLine(String.Format("{0},{1},{2} ロールバックします。", DateTime.Now, ex.Message, ex.StackTrace))
                        tranz.Rollback()
                        'Finally
                        '    tranz.Dispose()
                        '    cmd.Dispose()
                        'cn.Close()
                        '    cn.Dispose()
                    End Try
                End Using
            End Using

            cn.Close()

        End Using

        Return intRet

    End Function

    ''' <summary>
    ''' クエリ渡して、テーブルを指定し、複数フィールドのデータを更新する
    ''' </summary>
    ''' <param name="FieldName">フィールド名の配列</param>
    ''' <param name="strSQL">DB接続用クエリ</param>
    ''' <param name="UpdData">更新するデータの配列</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function UpdateDataField(FieldName() As String, strSQL As String, UpdData() As Object, FieldIndexName As String) As Integer

        Dim intRet As Integer = -1
        'Dim cn As OleDbConnection = New OleDbConnection
        'Dim dataAdp As OleDbDataAdapter = Nothing
        'Dim dSet As New DataSet
        Dim FieldLoop As Integer

        If FieldName.Length <> UpdData.Length Then
            intRet = -9999
            Return intRet
        End If

        Try
            Using cn As OleDbConnection = New OleDbConnection
                cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath)

                Using dataAdp As New OleDbDataAdapter(strSQL, cn)
                    'dataAdp = New OleDbDataAdapter(strSQL, cn)
                    AddHandler dataAdp.RowUpdated, New OleDbRowUpdatedEventHandler(AddressOf OnRowUpdated)          ' オートインクリメントで生成された新レコードのIDを取得するためのイベント定義

                    Using dSet As New DataSet("Data")
                        connection = cn
                        dataAdp.Fill(dSet, "Data")

                        If dSet.Tables("Data").Rows.Count > 0 Then
                            'Dim cmdbuilder As New OleDbCommandBuilder
                            Using cmdbuilder As New OleDbCommandBuilder


                                For FieldLoop = 0 To FieldName.Length - 1
                                    dSet.Tables("Data").Rows(0)(FieldName(FieldLoop)) = UpdData(FieldLoop)
                                Next FieldLoop

                                cmdbuilder.DataAdapter = dataAdp
                                dataAdp.Update(dSet, "Data")

                                'cmdbuilder.Dispose()
                                RecordExistValue = Convert.ToInt32(dSet.Tables("Data").Rows(0)(0))
                                intRet = 0
                            End Using
                        Else
                            Dim dt As New DataTable
                            Try
                                dt = dSet.Tables("Data")

                                Dim dr As DataRow = dt.NewRow()
                                For FieldLoop = 0 To FieldName.Length - 1
                                    dr(FieldName(FieldLoop)) = UpdData(FieldLoop)
                                Next
                                dt.Rows.Add(dr)

                                'Dim ComBuild As OleDbCommandBuilder = New OleDbCommandBuilder(dataAdp)
                                Using ComBuild As OleDbCommandBuilder = New OleDbCommandBuilder(dataAdp)
                                    dataAdp.Update(dSet, "Data")
                                    intRet = AddRecordID
                                End Using

                            Catch ex As Exception

                            Finally
                                dt.Dispose()
                            End Try

                            ''Dim strTemp() As String = strSQL.Split(Convert.ToChar(" "))                                     ' テーブル名を検索する
                            ''Dim indexLoop As Integer
                            ''For indexLoop = 0 To strTemp.Length - 1
                            ''    If strTemp(indexLoop) = "FROM" Then
                            ''        indexLoop += 1
                            ''        Exit For
                            ''    End If
                            ''Next

                            ''Dim idDataAdp As OleDbDataAdapter = Nothing                                                     '追加したレコードのServerIDを取得する
                            ''Dim idDataset As DataSet = Nothing
                            ''idDataAdp = New OleDbDataAdapter(String.Format("SELECT {0} FROM {1}", FieldIndexName, strTemp(indexLoop)), cn)
                            ''idDataset = New DataSet
                            ''idDataAdp.Fill(idDataset, "ID")
                            ' ''intRet = DirectCast(idDataset.Tables("ID").Rows(idDataset.Tables("ID").Rows.Count - 1)(0), Int32)                               'レコードのServerIDを返す
                            ''intRet = Convert.ToInt32(idDataset.Tables("ID").Rows(idDataset.Tables("ID").Rows.Count - 1)(0))                               'レコードのServerIDを返す

                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            exError = ex
            Debug.WriteLine(String.Format("{0},{1},{2}", DateTime.Now, ex.Message, ex.StackTrace))
            intRet = -1
            'Finally
            '    RemoveHandler dataAdp.RowUpdated, New OleDbRowUpdatedEventHandler(AddressOf OnRowUpdated)
            '    dSet.Dispose()
            '    If dataAdp IsNot Nothing Then
            '        dataAdp.Dispose()
            '    End If
            '    cn.Dispose()

        End Try

        Return intRet

    End Function


    ''' <summary>
    ''' [イベント] データが追加されたとき　　
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>オートインクリメントで追加されたIDを取得する</remarks>
    Private Sub OnRowUpdated(ByVal sender As Object, ByVal e As OleDbRowUpdatedEventArgs)

        Try
            If e.StatementType = StatementType.Insert Then
                ' Retrieve the Autonumber and store it in the CategoryID column.
                Using cmdNewID As New OleDbCommand("SELECT @@IDENTITY", connection)
                    AddRecordID = CInt(cmdNewID.ExecuteScalar)
                    'e.Row("RecordID") = CInt(cmdNewID.ExecuteScalar)
                    e.Status = UpdateStatus.SkipCurrentRow
                End Using
            End If
        Catch ex As Exception
            exError = ex
        End Try

    End Sub

    ''' <summary>
    ''' クエリ渡して、テーブルを指定し、1フィールドのデータを更新する
    ''' </summary>
    ''' <param name="FieldName">フィールド名</param>
    ''' <param name="strSQL">DB接続用クエリ</param>
    ''' <param name="updData">更新するデータ</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function UpdateDataField(FieldName As String, strSQL As String, updData As Object, FieldIndexName As String) As Integer

        Dim intRet As Integer = -1
        'Dim cn As OleDbConnection = New OleDbConnection
        'Dim dataAdp As OleDbDataAdapter = Nothing
        'Dim dSet As New DataSet

        Try
            Using cn As OleDbConnection = New OleDbConnection
                cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath)
                Using dataAdp As New OleDbDataAdapter(strSQL, cn)
                    'dataAdp = New OleDbDataAdapter(strSQL, cn)
                    AddHandler dataAdp.RowUpdated, New OleDbRowUpdatedEventHandler(AddressOf OnRowUpdated)          ' オートインクリメントで生成された新レコードのIDを取得するためのイベント定義

                    connection = cn
                    Using dSet As New DataSet

                        dataAdp.Fill(dSet, "Data")

                        If dSet.Tables("Data").Rows.Count > 0 Then

                            If updData.ToString = "COM_Del" Then                        ' 該当レコードがあって、Com_Delならレコードを削除する
                                dSet.Tables("Data").Rows(0).Delete()                    ' レコード削除
                            Else
                                dSet.Tables("Data").Rows(0)(FieldName) = updData
                            End If

                            'Dim cmdbuilder As New OleDbCommandBuilder
                            Using cmdbuilder As New OleDbCommandBuilder

                                cmdbuilder.DataAdapter = dataAdp

                                dataAdp.Update(dSet, "Data")

                                'cmdbuilder.Dispose()
                                RecordExist = True
                                RecordExistValue = Convert.ToInt32(dSet.Tables("Data").Rows(0)(0))
                                intRet = 0
                            End Using
                        Else
                            If updData.ToString <> "COM_Del" Then                       '該当レコードがなくて、Com_Delなら何もしない（更新するデータがないから）

                                Dim dt As New DataTable
                                dt = dSet.Tables("Data")

                                Dim dr As DataRow = dt.NewRow()
                                dr(FieldName) = updData
                                dt.Rows.Add(dr)

                                'Dim ComBuild As OleDbCommandBuilder = New OleDbCommandBuilder(dataAdp)
                                Using ComBuild As OleDbCommandBuilder = New OleDbCommandBuilder(dataAdp)
                                    dataAdp.Update(dSet, "Data")
                                    intRet = AddRecordID
                                End Using
                                ''Dim strTemp() As String = strSQL.Split(Convert.ToChar(" "))                                     ' テーブル名を検索する
                                ''Dim indexLoop As Integer
                                ''For indexLoop = 0 To strTemp.Length - 1
                                ''    If strTemp(indexLoop) = "FROM" Then
                                ''        indexLoop += 1
                                ''        Exit For
                                ''    End If
                                ''Next
                                ''Dim idDataAdp As OleDbDataAdapter = Nothing                                                     '追加したレコードのServerIDを取得する
                                ''Dim idDataset As DataSet = Nothing
                                ''idDataAdp = New OleDbDataAdapter(String.Format("SELECT {0} FROM {1}", FieldIndexName, strTemp(indexLoop)), cn)
                                ''idDataset = New DataSet
                                ''idDataAdp.Fill(idDataset, "ID")
                                ''intRet = DirectCast(idDataset.Tables("ID").Rows(0)(0), Int32)                                   'レコードのIDを返す
                            End If
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Debug.WriteLine(String.Format("{0},{1},{2}", DateTime.Now, ex.Message, ex.StackTrace))
            exError = ex
            ''Finally
            ''    RemoveHandler dataAdp.RowUpdated, New OleDbRowUpdatedEventHandler(AddressOf OnRowUpdated)
            ''    dSet.Dispose()
            ''    dataAdp.Dispose()
            ''    cn.Dispose()

        End Try

        Return intRet

    End Function


    ''' <summary>
    ''' 計測管理システム用のデータベースに保存するメソッド
    ''' </summary>
    ''' <param name="DataDate"></param>
    ''' <returns>書き込み処理の成否　成功：0　失敗：-1</returns>
    ''' <remarks></remarks>
    Public Function AddData(DataDate As DateTime) As Integer

        Dim intRet As Integer = -1
        'Dim cn As OleDbConnection = New OleDbConnection
        'Dim dateAdp As OleDbDataAdapter
        'Dim dSet As New DataSet("日付")
        'Dim cdteBuild As OleDbCommandBuilder
        'Dim DTable As DataTable
        Dim GetIndexDataRow() As DataRow
        Dim DRow As DataRow
        'Dim idDataAdp As OleDbDataAdapter = Nothing
        'Dim idDataset As DataSet = Nothing
        Dim GetDateIndex As Integer
        Dim jloop As Integer
        Dim strSQL As String = Nothing
        Dim iloop As Integer
        'Dim dcom As OleDbCommand

        Dim TableName(TableCount - 1) As String
        Dim StrSQLCH(TableCount - 1) As String
        Dim strSQLData(TableCount - 1) As String

        Dim tmpTalble As String = Nothing
        Dim tmpCh As String = Nothing
        Dim tmpData As String = Nothing
        Dim Counter As Integer = 0

        For iloop = 0 To SaveData.Length - 1
            Counter = Convert.ToInt32(Convert.ToInt32(SaveChData(iloop)) * 0.01)            ' 0,1,2... TableNameのインデックスと同じになる
            'StrSQLCH(Counter) &= String.Format("Ch{0},", SaveChData(iloop))
            'strSQLData(Counter) &= String.Format("{0},", SaveData(iloop))
            strSQLData(Counter) &= String.Format("Ch{0} = {1},", SaveChData(iloop), SaveData(iloop))
        Next

        For iloop = 0 To (TableCount - 1)
            'If StrSQLCH(iloop).Trim.EndsWith(",") Then
            '    StrSQLCH(iloop) = StrSQLCH(iloop).Remove(StrSQLCH(iloop).Length - 1, 1)
            'End If
            TableName(iloop) = MakeTableName(iloop + 1)
            If strSQLData(iloop).Trim.EndsWith(",") Then
                strSQLData(iloop) = strSQLData(iloop).Remove(strSQLData(iloop).Length - 1, 1)
            End If
        Next

        Try
            Using cn As New OleDbConnection

                cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & dbFilePath)

                Using dateAdp As New OleDbDataAdapter("SELECT * FROM 日付 WHERE 日付 >= #" & DataDate.AddDays(-3) & "# ORDER BY 日付ID ASC", cn)      ' 転送されてきたデータの最古日付以降を読み込む　
                    'dateAdp = New OleDbDataAdapter("SELECT * FROM 日付 WHERE 日付 >= #" & DataDate.AddDays(-3) & "# ORDER BY 日付ID ASC", cn)      ' 転送されてきたデータの最古日付以降を読み込む　

                    Using dSet As New DataSet("日付")

                        dateAdp.Fill(dSet, "日付")

                        Using cdteBuild As New OleDbCommandBuilder(dateAdp)              'これがないと、UPDATEで落ちる・・(--#)　
                            'cdteBuild = New OleDbCommandBuilder(dateAdp)              'これがないと、UPDATEで落ちる・・(--#)　

                            Using DTable As New DataTable("日付")
                                'DTable = dSet.Tables("日付")

                                '「日付」のテーブルから日付IDの検索
                                GetIndexDataRow = dSet.Tables("日付").Select("日付 = #" & DataDate.ToString("yyyy/MM/d HH:mm:ss") & "#")   ''日付で検索しIDを取得　無ければLength=0となる

                                If GetIndexDataRow.Length = 0 Then
                                    ' 日付が存在しなかった場合
                                    DRow = DTable.NewRow()
                                    DRow("日付") = DataDate
                                    DTable.Rows.Add(DRow)
                                    dateAdp.Update(DTable)

                                    Using idDataAdp As New OleDbDataAdapter("SELECT 日付ID FROM 日付 WHERE 日付 = #" & DataDate.ToString & "#", cn)

                                        'idDataAdp = New OleDbDataAdapter("SELECT 日付ID FROM 日付 WHERE 日付 = #" & DataDate.ToString & "#", cn)                                    '' データアダプタへ変更
                                        Using idDataset = New DataSet("日付ID")
                                            idDataAdp.Fill(idDataset, "日付ID")
                                            GetDateIndex = DirectCast(idDataset.Tables("日付ID").Rows(0)(0), Int32)                                                   ' 2016/02/24 Kino Changed CType

                                            cn.Open()
                                            '先にデータテーブル数分　各テーブルにレコードを追加して、日付IDのフィールドにIDを書き込む
                                            For jloop = 1 To TableCount
                                                strSQL = Nothing
                                                'TableName(jloop) = MakeTableName(jloop)
                                                strSQL = String.Format("INSERT INTO {0} (日付ID) VALUES({1})", TableName(jloop - 1), GetDateIndex)
                                                Using dcom As New OleDbCommand(strSQL, cn)
                                                    dcom.ExecuteNonQuery()
                                                    'dcom.Dispose()
                                                End Using
                                            Next jloop
                                            cn.Close()
                                        End Using
                                    End Using
                                Else
                                    ''日付が存在したら、対象測定日付の日付IDを取得
                                    GetDateIndex = CInt(GetIndexDataRow(0).ItemArray(0))
                                End If

                                If GetDateIndex > 0 Then
                                    cn.Open()
                                    For jloop = 0 To (TableCount - 1)
                                        Dim DataSQL As String = Nothing
                                        strSQL = Nothing
                                        strSQL = String.Format("UPDATE {0} SET {1} WHERE 日付ID={2}", TableName(jloop), strSQLData(jloop), GetDateIndex)
                                        Using dcom As New OleDbCommand(strSQL, cn)
                                            dcom.ExecuteNonQuery()
                                            'dcom.Dispose()
                                        End Using
                                    Next jloop
                                    cn.Close()
                                End If

                                intRet = 0

                            End Using
                        End Using
                    End Using
                End Using
            End Using
        Catch ex As Exception
            exError = ex
            exMessage = String.Format("データ保存時にエラーが発生しました。 {0} {1}", ex.Message, ex.StackTrace)
            'Finally
            '    dcom = Nothing
            '    DTable = Nothing
            '    cn.Close()
        End Try

        Return intRet

    End Function

End Class

