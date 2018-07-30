Imports System
Imports System.Data
Imports System.Security.Cryptography

Public Class ClsCheckUser

    Private Shadows _UserInformation_DatabaseFile As String
    Private Shadows _UserAnnouncement_DatabaseFile As String

    ''' <summary>
    ''' ユーザー名とパスワードのファイルを返すプロパティ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property UserInformation_DatabaseFile As String
        Get
            Return _UserInformation_DatabaseFile
        End Get
    End Property

    ''' <summary>
    ''' お知らせ情報のファイルを返すプロパティ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property UserAnnouncement_DatabaseFile As String
        Get
            Return _UserAnnouncement_DatabaseFile
        End Get
    End Property

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="WebType">システムタイプ</param>
    ''' <remarks>システムタイプを指定することで、基本ファイルのパスを指定する</remarks>
    Sub New(Optional ByVal WebType As String = Nothing)

        If WebType Is Nothing Then
            _UserInformation_DatabaseFile = "C:\chkdb\UserAndPassword.mdb"
            _UserAnnouncement_DatabaseFile = "C:\chkdb\UserInformations.mdb"

        ElseIf WebType = "iot" Then
            _UserInformation_DatabaseFile = "C:\chkdb\CustomerInf.mdb"
            _UserAnnouncement_DatabaseFile = "C:\chkdb\UserInformationsIoT.mdb"

        End If
    End Sub

    ''Public Const UserInformation_DatabaseFile As String = "C:\chkdb\UserAndPassword.mdb"　　↑のようにプロパティ化した
    ''Public Const UserAnnouncement_DatabaseFile As String = "C:\chkdb\UserInformations.mdb"
    '' *.vbからApplication変数やSession変数を利用するためには、「System.Web.HttpContext.Current」

    ''' <summary>
    ''' オートログインのユーザー名とハッシュキーの確認
    ''' </summary>
    ''' <param name="userName">ユーザー名</param>
    ''' <param name="HashValue">ハッシュキー</param>
    ''' <param name="passWord">取得したパスワードをMD5で暗号化した文字列</param>
    ''' <returns>ログイン成否　成功:True 失敗:False</returns>
    ''' <remarks></remarks>
    Public Function SessionDataOTP_Check(ByVal userName As String, ByVal HashValue As String, ByRef passWord As String) As Boolean

        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As New OleDb.OleDbDataAdapter
        'Dim cdteBuild As OleDb.OleDbCommandBuilder = Nothing
        'Dim DTable As DataTable = Nothing
        Dim existRow() As DataRow = Nothing
        ''Dim DRow As DataRow
        'Dim DtSet As New DataSet("UserCheck")
        Dim AccessLogFile As String = "C:\chkdb\AccessLog.mdb"
        Dim ReturnValue As Boolean = False
        Dim otpDBFile As String = IO.Path.Combine(IO.Path.GetDirectoryName(AccessLogFile), "otp.mdb")

        Using DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & otpDBFile & ";Jet OLEDB:Engine Type= 5")

            Using DbDa As New OleDb.OleDbDataAdapter(String.Format("SELECT * FROM UserInfo WHERE HashValue='{0}' AND UserName='{1}' And LimitDate>#{2}#", HashValue, userName, DateTime.Now), DbCon)
                'DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM UserInfo WHERE HashValue='" & HashValue & "' AND UserName='" & userName & "' And LimitDate>#" & DateTime.Now & "#", DbCon)

                Using DtSet As New DataSet

                    DbDa.Fill(DtSet, "UserCheck")

                    Using cdteBuild As New OleDb.OleDbCommandBuilder(DbDa)
                        'cdteBuild = New OleDb.OleDbCommandBuilder(DbDa)

                        'DTable = DtSet.Tables("UserCheck")

                        If DtSet.Tables("UserCheck").Rows.Count >= 1 Then                                      ' アカウントに該当のレコードがあれば、削除する
                            existRow = DtSet.Tables("UserCheck").Select("HashValue = '" & HashValue & "'")
                            passWord = existRow.GetValue(0).Item(2).ToString
                            passWord = MAKE_MD5(passWord)
                            existRow(0).Delete()
                            ReturnValue = True
                        End If

                        ''Dim RowsCount As Integer = existRow.GetUpperBound(0)            ' 検索結果が存在するか確認する　該当なしは -1

                        ''If RowsCount >= 0 Then
                        ''    DRow = existRow(0)
                        ''    DRow.Delete()
                        ''    ReturnValue = True
                        ''End If

                        cdteBuild.DataAdapter = DbDa
                        DbDa.Update(DtSet, "UserCheck")
                    End Using
                End Using
            End Using
        End Using

        'DTable.Dispose()
        'cdteBuild.Dispose()
        'DtSet.Dispose()
        'DbDa.Dispose()
        'DbCon.Dispose()

        Return ReturnValue

    End Function

    ''' <summary>
    ''' ユーザ名とパスワードの確認
    ''' </summary>
    ''' <param name="userName">ユーザ名</param>
    ''' <param name="userPass">パスワード</param>
    ''' <returns>ログインの認証結果</returns>
    Public Function SessionDataIoT_Check(ByVal userName As String, ByVal userPass As String, ByRef userLevel As Integer,
                                        ByRef AuthLimitDate As Date, ByRef userGroup As String,
                                        ByRef IDNum As Long, ByRef officeName As String, Optional ByVal hiddenFlg As Integer = 0) As Boolean

        Dim AccessLogFile As String = "C:\chkdb\AccessLog.mdb"
        Dim CustomerName As String = "Not Found"
        Dim ReturnValue As Boolean = False

        '' セッションにより、引き継がれたデータがデータベースと一致するかを確認
        If userName = "" Or userPass = "" Then
            Return False
            Exit Function
        End If

        Try                                                                             ' 2018/06/26 Kino Add   例外処理
            Using DbCon As New OleDb.OleDbConnection                                    '                       スコープを指定した

                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & _UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")

                Using DbCom As New OleDb.OleDbCommand                                   ' 2018/06/26 Kino Add　　　strComp　文字列比較(Arrg1,Arg2,Arg3)  Arg3:バイナリ比較(大文字小文字判別)

                    'DbCom.CommandText = "SELECT * FROM Customer WHERE StrComp([ユーザー名],@UID,0)=0 AND パスワード = @UPW"
                    DbCom.CommandText = "SELECT * FROM UserInformation WHERE StrComp([UserName],@UID,0)=0"           ' AND PassWord = @UPW"
                    DbCom.Connection = DbCon
                    DbCom.Parameters.Add(New OleDb.OleDbParameter("@UID", System.Data.OleDb.OleDbType.Char))
                    DbCom.Parameters("@UID").Value = userName
                    DbCom.Parameters.Add(New OleDb.OleDbParameter("@UPW", System.Data.OleDb.OleDbType.Char))
                    DbCom.Parameters("@UPW").Value = userPass

                    Using DbDa As New OleDb.OleDbDataAdapter(DbCom)

                        Using DtSet As New DataSet("User")
                            DbDa.Fill(DtSet, "User")

                            Dim comparer As StringComparer = StringComparer.OrdinalIgnoreCase
                            Dim DTable As DataTable = DtSet.Tables("User")

                            If DTable.Rows.Count > 0 Then
                                CustomerName = DTable.Rows(0).Item(4).ToString                                              '事務所名
                                userLevel = CType(DTable.Rows(0).Item(5), Integer)                                          'ユーザレベル
                                Dim tempLimitDate As DateTime = CType(DTable.Rows(0).Item(6), DateTime)
                                AuthLimitDate = CType(tempLimitDate.ToString("yyyy/MM/dd") & " 23:59:59", DateTime)         '認証期限       時刻をつけないと0:00になるため、最終時刻を指定　2011/10/03 Kino Add
                                userGroup = DTable.Rows(0).Item(7).ToString                                                 'ユーザグループ
                                IDNum = Convert.ToInt32(DTable.Rows(0).Item(10))                                            '識別番号
                                'AuthKey = DTable.Rows(0).Item(11).ToString                                                  '認証キー
                                ReturnValue = True                                                                          'レコードが存在するということは、アカウントとパスワードが一致するということで、ログインＯＫとする
                                If AuthLimitDate < DateTime.Now() Then                                                      '認証期限切れはFalse
                                    ReturnValue = False
                                End If
                                If userName = "sakataadmin" Then                                                            '管理者であるか確認
                                    CustomerName = "管理者"
                                End If
                                officeName = CustomerName
                                '/--------------- 2015/11/18 Kino Add  隠しフィールド対応　★ペンディング
                                'Dim remIP As Net.IPAddress = Nothing
                                'Dim refIP As Net.IPAddress = Nothing
                                'Dim blnRet As Boolean = Net.IPAddress.TryParse(HttpContext.Current.Request.ServerVariables("REMOTE_ADDR"), remIP)
                                'Dim AccessRestriction As Net.IPAddress = Nothing
                                'Dim refRet As Boolean
                                'If DtSet.Tables("User").Columns.Count > 9 Then                                      ' フィールドが存在していたら読み込む
                                '    refRet = Net.IPAddress.TryParse(DtSet.Tables("User").Rows(0).Item(9).ToString, refIP)
                                'End If
                                'If hiddenFlg = 1 AndAlso blnRet = True AndAlso remIP.Equals(refIP) = False Then
                                '    ReturnValue = False
                                'ElseIf hiddenFlg = 0 AndAlso refRet = True Then                                     ' 隠しフィールド用アカウントでのログインの場合は、通常のUIログインNG(隠しフィールド専用)とする
                                '    ReturnValue = False
                                'End If
                            Else
                                ReturnValue = False
                            End If
                        End Using
                    End Using
                End Using

            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine(ex)
        End Try

        If IO.File.Exists(AccessLogFile) = False Then                           'ログファイルチェック
            Call MakeDB_AccessLog(AccessLogFile)                                'ログファイル作成
        End If

        Try                                                                                                 ' 2012/08/03 Kino Add 例外処理追加
            Call WriteAccessLog(AccessLogFile, ReturnValue, userName, userPass, CustomerName)       ', RemoteInf)     'アクセスログ書き込み
        Catch ex As Exception
            Using sw As New IO.StreamWriter("C:\chkdb\writeAccessLogError.log", True, Encoding.GetEncoding("Shift_JIS"))
                sw.WriteLine(Now.ToString & ex.Message & "," & ex.StackTrace + Environment.NewLine)
            End Using
        End Try

        Return ReturnValue

    End Function

    ''' <summary>
    ''' ユーザ名とパスワードの確認
    ''' </summary>
    ''' <param name="userName">ユーザ名</param>
    ''' <param name="userPass">パスワード</param>
    ''' <param name="siteNo">現場番号(byRef)</param>
    ''' <returns>ログインの認証結果</returns>
    Public Function SessionData_Check(ByVal userName As String, ByVal userPass As String, ByRef siteNo As String, ByRef userLevel As Integer,
                                        ByRef AuthLimitDate As Date, ByRef userGroup As String,
                                        ByRef ShowName As String, Optional ByVal hiddenFlg As Integer = 0) As Boolean

        ''Dim DbDr As OleDb.OleDbDataReader
        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As New OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("User")
        Dim DecPW As String = "-"
        ''Dim strEncKey As String
        Dim AccessLogFile As String = "C:\chkdb\AccessLog.mdb"
        Dim CustomerName As String = "Not Found"
        Dim ReturnValue As Boolean = False
        'Dim clsAuth As New ClsCheckUser
        Dim strHash As String

        '' セッションにより、引き継がれたデータがデータベースと一致するかを確認
        If userName = "" Or userPass = "" Then
            ''MsgBox("ユーザー＆パスワードを入力して下さい。", MsgBoxStyle.OkOnly, "警告")
            Return False
            Exit Function
        End If

        Try                                                                             ' 2018/06/26 Kino Add   例外処理
            Using DbCon As New OleDb.OleDbConnection                                    '                       スコープを指定した

                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & _UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")
                ''DbCon.Open()                                                                                                          ' 2011/03/15 Kino Changed
                '' ユーザーとパスワードの照合
                ''strEncKey = Date.Now().Month & Date.Now().Day & Date.Now().DayOfWeek                   '複合化キーを作成
                ''DecPW = clsAuth.DecryptString(userPass, strEncKey)                 '暗号化されたパスワードの文字列を複合化する        不要ためコメント
                ''DecPW = userPass
                ''DbCom.CommandText = "SELECT * FROM UserInformation WHERE UserName = @UID"           ' AND PassWord = @UPW"

                Using DbCom As New OleDb.OleDbCommand                                   ' 2018/06/26 Kino Add

                    DbCom.CommandText = "SELECT * FROM UserInformation WHERE StrComp([UserName],@UID,0)=0"           ' AND PassWord = @UPW"
                    DbCom.Connection = DbCon
                    DbCom.Parameters.Add(New OleDb.OleDbParameter("@UID", System.Data.OleDb.OleDbType.Char))
                    DbCom.Parameters("@UID").Value = userName                               'フィールド名　UserName
                    '' ''DbCom.Parameters.Add(New OleDb.OleDbParameter("@UPW", System.Data.OleDb.OleDbType.VarWChar))       'MD5対応としたためコメント
                    '' ''DbCom.Parameters("@UPW").Value = DecPW                                  'フィールド名　PassWord
                    ''Dim Sql As String = "Select * From UserInformation Where UserName = '" & userName & "'"
                    ''DbCom = New OleDb.OleDbCommand(Sql, DbCon)

                    Using DbDa As New OleDb.OleDbDataAdapter(DbCom)
                        'DbDa = New OleDb.OleDbDataAdapter(DbCom)

                        Using DtSet As New DataSet("User")
                            DbDa.Fill(DtSet, "User")

                            ''DbCon.Close()                                                                                                         ' 2011/03/15 Kino Changed
                            ''DbCon.Dispose()

                            Dim comparer As StringComparer = StringComparer.OrdinalIgnoreCase

                            ''DataReaderからDataSetへ変更した。 2009/10/01 Kino
                            If DtSet.Tables("User").Rows.Count > 0 Then                                 'レコードが存在するかどうか
                                For Each DTR As DataRow In DtSet.Tables("User").Rows
                                    DecPW = DTR.Item(2).ToString
                                    strHash = MAKE_MD5(DecPW)
                                    'System.Windows.Forms.Application.DoEvents()
                                    If comparer.Compare(strHash, userPass) = 0 Then                             '■受信した文字列とDBのパスワードの一致を確認
                                        siteNo = DTR.Item(3).ToString                                           'サイト番号
                                        CustomerName = DTR.Item(4).ToString                                     '顧客名
                                        userLevel = CType(DTR.Item(5), Integer)                                 'ユーザレベル
                                        Dim tempLimitDate As DateTime = CType(DTR.Item(6), DateTime)
                                        AuthLimitDate = CType(tempLimitDate.ToString("yyyy/MM/dd") & " 23:59:59", DateTime)     '認証期限       時刻をつけないと0:00になるため、最終時刻を指定　2011/10/03 Kino Add
                                        userGroup = DTR.Item(7).ToString                                        'ユーザグループ
                                        ShowName = DTR.Item(8).ToString                                         '一覧表示タイトル
                                        ReturnValue = True
                                        If AuthLimitDate < DateTime.Now() Then                                  ' 2016/06/07 Kino Add 認証期限切れでもAuthorizedとなってしまうので修正
                                            ReturnValue = True
                                        End If
                                        If userName = "sakataadmin" Then                                        '管理者であるか確認
                                            CustomerName = "管理者"
                                        End If
                                        '/--------------- 2015/11/18 Kino Add  隠しフィールド対応
                                        Dim remIP As Net.IPAddress = Nothing
                                        Dim refIP As Net.IPAddress = Nothing
                                        Dim blnRet As Boolean = Net.IPAddress.TryParse(HttpContext.Current.Request.ServerVariables("REMOTE_ADDR"), remIP)
                                        Dim AccessRestriction As Net.IPAddress = Nothing
                                        Dim refRet As Boolean
                                        If DtSet.Tables("User").Columns.Count > 9 Then                                      ' フィールドが存在していたら読み込む
                                            refRet = Net.IPAddress.TryParse(DTR.Item(9).ToString, refIP)
                                        End If
                                        If hiddenFlg = 1 AndAlso blnRet = True AndAlso remIP.Equals(refIP) = False Then
                                            ReturnValue = False
                                        ElseIf hiddenFlg = 0 AndAlso refRet = True Then                                     ' 隠しフィールド用アカウントでのログインの場合は、通常のUIログインNG(隠しフィールド専用)とする
                                            ReturnValue = False
                                        End If
                                        '-------------------------------------------------------/
                                        Exit For
                                    Else
                                        ReturnValue = False
                                    End If
                                Next
                            Else
                                ReturnValue = False
                            End If
                        End Using
                    End Using
                End Using
                ''DbDr = DbCom.ExecuteReader
                ''If DbDr.HasRows = True Then                                                     'SQLを通した結果レコードがあれば、ユーザとパスワードが一致したデータがあったとして認証する
                ''    ''DecPW = EncryptText(UserPass, 1)
                ''    ''strEncKey = Now().Month & Now().Day & Now().DayOfWeek                     '暗号化キーを作成
                ''    ''DecPW = ClsCheckUser.DecryptString(userPass, strEncKey)                   'パスワードの文字列を暗号化する
                ''    ' Create a StringComparer an comare the hashes.
                ''    Dim comparer As StringComparer = StringComparer.OrdinalIgnoreCase

                ''    Do While DbDr.Read()
                ''        '' ''    If DbDr.GetString(2) = DecPW Then
                ''        ''DbDr.Read()
                ''        DecPW = DbDr.GetString(2)
                ''        strHash = MAKE_MD5(DecPW)
                ''        System.Windows.Forms.Application.DoEvents()

                ''        If comparer.Compare(strHash, userPass) = 0 Then                                 '■受信した文字列とDBのパスワードの一致を確認
                ''            siteNo = DbDr.GetString(3)                                                  'サイト番号
                ''            CustomerName = DbDr.GetString(4)                                            '顧客名
                ''            userLevel = DbDr.GetInt32(5)                                                'ユーザレベル
                ''            AuthLimitDate = DbDr.GetDateTime(6)                                         '認証期限
                ''            userGroup = DbDr.GetString(7)                                               'ユーザグループ
                ''            ShowName = DbDr.GetString(8)                                                '一覧表示タイトル
                ''            'DecPW = "-"
                ''            ReturnValue = True     ' SessionData_Check = True

                ''            If userName = "sakataadmin" Then                                            '管理者であるか確認
                ''                CustomerName = "管理者"
                ''            End If
                ''            Exit Do
                ''        Else
                ''            ReturnValue = False
                ''        End If
                ''        '' ''        Exit Do
                ''        '' ''    End If
                ''    Loop

                ''    ''DbDa = New OleDb.OleDbDataAdapter(Sql, DbCon)
                ''    ''DbDa.Fill(DtSet, "UserInformation")

                ''    ''If DtSet.Tables("UserInformation").Rows(0).Item("PassWord").ToString = DecPW Then
                ''    ''    SiteNo = DtSet.Tables("UserInformation").Rows(0).Item("SetPlaceNo").ToString
                ''    ''    SessionData_Check = True
                ''    ''    ''Else
                ''    ''    ''    MsgBox("登録されていないユーザー名　もしくは　パスワードが違います。", MsgBoxStyle.OkOnly, "警告")
                ''    ''End If
                ''    '' ''Else
                ''    ''    ''MsgBox("登録されていないユーザー名　もしくは　パスワードが違います。", MsgBoxStyle.OkOnly, "警告")
                ''Else
                ''    ReturnValue = False             'SessionData_Check = False

                ''    ''If userName = "sakataadmin" And DecPW = "softgr3270" Then
                ''    ''    CustomerName = "管理者"
                ''    ''    DecPW = "-"
                ''    ''    ReturnValue = True          'SessionData_Check = True
                ''    ''End If

                ''End If

            End Using
        Catch ex As Exception

        Finally
            ''DbDr.Close()
            'DbDa.Dispose()
            'DtSet.Dispose()
            'DbCom.Dispose()
            'DbCon.Close()
            'DbCon.Dispose()
        End Try

        If IO.File.Exists(AccessLogFile) = False Then                           'ログファイルチェック
            Call MakeDB_AccessLog(AccessLogFile)                                'ログファイル作成
        End If

        ''Call WriteAccessLog(AccessLogFile, ReturnValue, userName, DecPW, CustomerName, RemoteInf)     'アクセスログ書き込み
        Try                                                                                                 ' 2012/08/03 Kino Add 例外処理追加
            Call WriteAccessLog(AccessLogFile, ReturnValue, userName, userPass, CustomerName)       ', RemoteInf)     'アクセスログ書き込み
        Catch ex As Exception
            Using sw As New IO.StreamWriter("C:\chkdb\writeAccessLogError.log", True, Encoding.GetEncoding("Shift_JIS"))
                sw.WriteLine(Now.ToString & ex.Message & "," & ex.StackTrace + Environment.NewLine)
            End Using
        End Try

        Return ReturnValue

    End Function


    ''' <summary>
    ''' アクセスログへ書き出し
    ''' </summary>
    ''' <param name="DBPath">データベースのPath</param>
    ''' <param name="SessionDataCheck">認証状況</param>
    ''' <param name="UserName">ユーザ名</param>
    ''' <param name="PassWord">パスワード</param>
    ''' <param name="CustomerName">現場名</param>
    Public Sub WriteAccessLog(ByRef DBPath As String, ByRef SessionDataCheck As Boolean, ByRef UserName As String, _
                                ByRef PassWord As String, ByRef CustomerName As String)     ', ByVal RemoteInf As String)
        ''
        '' アクセスログの記録
        ''
        Dim strStatus As String
        'Dim cn As OleDb.OleDbConnection = New OleDb.OleDbConnection()
        'Dim dataAdp As OleDb.OleDbDataAdapter
        'Dim dSet As New DataSet
        ''Dim DTable As DataTable
        'Dim cdteBuild As New OleDb.OleDbCommandBuilder
        ''Dim strSQL As String
        ''Dim GetNo As Integer
        ''Dim dcom As OleDb.OleDbCommand
        ''Dim RemoteADDR = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")       'ページを変えた
        ''Dim RemoteHOST = System.Net.Dns.GetHostByAddress(RemoteADDR).HostName           'HttpContext.Current.Request.ServerVariables("REMOTE_HOST")
        ''Dim dq As String = Chr(34)      ' ダブルクォーテーション
        Dim clsBr As New ClsHttpCapData
        Dim RemoteInf As String = clsBr.GetRemoteInfo(HttpContext.Current.Request)
        clsBr = Nothing

        Dim RemInfo() As String

        If RemoteInf IsNot Nothing Then                                        '' 2011/04/05 Kino Changed 取得できていない場合があるのを対応
            RemInfo = RemoteInf.Split(","c)
            If RemInfo.Length < 4 Then
                ReDim Preserve RemInfo(3)
                RemInfo(3) = "disable"
            End If
        Else
            ReDim RemInfo(3)
            RemInfo(0) = "disable"
            RemInfo(1) = "disable"
            RemInfo(2) = "disable"
            RemInfo(3) = "disable"
        End If

        If SessionDataCheck = True Then
            strStatus = "Autholized"
        Else
            strStatus = "Unauthorized"
        End If

        Using cn As New OleDb.OleDbConnection

            cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & DBPath)

            Using dataAdp As New OleDb.OleDbDataAdapter("SELECT * FROM アクセスログ WHERE 日時 > #" & Date.Now.AddHours(-24).ToString & "#", cn)       ' 2017/05/08 Kino Changed  -1 -> -24
                'dataAdp = New OleDb.OleDbDataAdapter("SELECT * FROM アクセスログ WHERE 日時 > #" & Date.Now.AddHours(-24).ToString & "#", cn)       ' 2017/05/08 Kino Changed  -1 -> -24

                Using DSet As New DataSet

                    dataAdp.Fill(DSet, "Log")

                    Dim dr As DataRow
                    dr = DSet.Tables("Log").NewRow()
                    dr(1) = DateTime.Now()
                    dr(2) = UserName.ToString
                    dr(3) = PassWord.ToString
                    dr(4) = CustomerName.ToString
                    dr(5) = strStatus.ToString
                    dr(6) = RemInfo(0).ToString
                    dr(7) = RemInfo(1).ToString
                    dr(8) = RemInfo(3).ToString
                    dr(9) = RemInfo(2).ToString                 'ブラウザ
                    DSet.Tables("Log").Rows.Add(dr)
                    Using cdteBuild As New OleDb.OleDbCommandBuilder
                        cdteBuild.DataAdapter = dataAdp
                        dataAdp.Update(DSet, "Log")
                    End Using

                    ''cdteBuild = New OleDb.OleDbCommandBuilder(dateAdp)              'これがないと、UPDATEで落ちる・・(--#)　
                    ''DTable = dSet.Tables("Log")

                    ' ''最終レコードの日付IDの値を取得する　後データのテーブルに書き込むときにオートインクリメントの値を計算するために使う
                    ''If DTable.Rows.Count <> 0 Then
                    ''    GetNo = dSet.Tables("アクセスログ").Rows.Item(DTable.Rows.Count - 1)(0)
                    ''Else
                    ''    GetNo = 0
                    ''End If

                    ''GetNo += 1

                    ''strSQL = "INSERT INTO アクセスログ(日時,ユーザ名,パスワード,顧客名,ステータス,リモートアドレス,リモートホスト,プラットフォーム,ブラウザ) VALUES("
                    ''strSQL &= ("#" + DateTime.Now().ToString + "#," + dq + UserName + dq + "," + dq + PassWord + dq + "," + dq + CustomerName + dq + "," + dq + strStatus + dq + _
                    ''            "," + dq + RemInfo(0) + dq + _
                    ''            "," + dq + RemInfo(1) + dq + _
                    ''            "," + dq + RemInfo(3) + dq + _
                    ''            "," + dq + RemInfo(2) + dq + ")")

                    ''dcom = New OleDb.OleDbCommand(strSQL, cn)                                   'ログイン状況を記録
                    ''dcom.ExecuteNonQuery()
                    ''dcom.Dispose()

                    'cdteBuild.Dispose()
                    'DSet.Dispose()
                    'dataAdp.Dispose()
                    ''cn.Close()                                                                            ' 2011/03/15 Kino Changed
                    ''dSet.Dispose()
                    ''DTable.Dispose()
                    'cn.Dispose()
                    ''dSet = Nothing
                    ''DTable = Nothing
                    ''dcom = Nothing
                    ''cn = Nothing
                End Using
            End Using
        End Using

    End Sub

    ''' <summary>
    ''' アクセスログのデータベースファイル作成
    ''' </summary>
    ''' <param name="accessLogFile">データベースのPath</param>
    Public Sub MakeDB_AccessLog(ByVal accessLogFile As String)
        ''
        '' アクセスログ用データベースの作成
        ''

        Dim srcDB As String = "c:\chkdb\srcDB\BlankFile.mdb"
        Dim dstDB As String = "c:\chkdb\AccessLog.mdb"
        Dim clsDB As New DB_CRUD
        Dim SqlTable As String = Nothing

        FileCopy(srcDB, dstDB)

        Dim sb As New StringBuilder
        sb.Capacity = 200

        sb.Append("CREATE TABLE [アクセスログ] (")
        sb.Append("[No] IDENTITY(1,1) PRIMARY KEY,")                '' No の場合は [No] と書く　. は # で置き換える？　No.というフィールド名は、インポートするとNoに書き換えられ(ピリオドが取られる)、リンクテーブルにするとNo#と表示されるらしい
        sb.Append("日時 DATETIME,")
        sb.Append("ユーザ名 TEXT(32),")
        sb.Append("パスワード TEXT(32),")
        sb.Append("顧客名 TEXT(128),")
        sb.Append("ステータス TEXT(16),")
        sb.Append("リモートアドレス TEXT(64),")
        sb.Append("リモートホスト TEXT(255),")
        sb.Append("プラットフォーム TEXT(32),")
        sb.Append("ブラウザ TEXT(32))")

        SqlTable = sb.ToString
        sb.Length = 0

        Dim intRet As Integer = clsDB.makeTableGeneral(dstDB, SqlTable)

    End Sub

    ''' <summary>
    ''' ユーザ、パスワードを保存するデータベースの作成
    ''' </summary>
    Public Sub Login_DatabaseMake()
        ''
        '' データベースの作成
        ''

        Dim Db As New ADOX.Catalog
        Dim dtTbl As New ADOX.Table

        Db.Create("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" & _UserInformation_DatabaseFile & ";" & "Jet OLEDB:Engine Type= 5")
        'テーブル名を指定してテーブルを追加する

        dtTbl = New ADOX.Table
        With dtTbl
            .Name = "UserInformation"
            .Columns.Append("UserNo", ADOX.DataTypeEnum.adInteger)              '' ユーザー番号
            .Columns.Append("UserName", ADOX.DataTypeEnum.adVarWChar)           '' ユーザー名称
            .Columns.Append("PassWord", ADOX.DataTypeEnum.adVarWChar)           '' パスワード
            .Columns.Append("SetPlaceNo", ADOX.DataTypeEnum.adVarWChar)         '' 使用する現場番号
        End With
        Db.Tables.Append(dtTbl)

        dtTbl = New ADOX.Table
        With dtTbl
            .Name = "ConstructionSite"
            .Columns.Append("SiteNo", ADOX.DataTypeEnum.adInteger)              '' 現場番号
            .Columns.Append("SiteName", ADOX.DataTypeEnum.adVarWChar)           '' 現場名称
            .Columns.Append("Address", ADOX.DataTypeEnum.adVarWChar)            '' 接続アドレス
        End With
        Db.Tables.Append(dtTbl)

        'コネクションを開放する
        Dim con As ADODB.Connection = CType(Db.ActiveConnection, ADODB.Connection)
        con.Close()

        Db = Nothing

    End Sub

    ''' <summary>
    ''' 文字列のMD5ハッシュ値を計算する
    ''' </summary>
    ''' <param name="src_txt">ハッシュ化する文字列</param>
    ''' <returns>ハッシュ値</returns>
    Public Function MAKE_MD5(ByVal src_txt As String) As String

        If src_txt.Length = 0 Then
            Return "Nothing "
            Exit Function
        End If

        Dim MD5S As MD5 = Nothing
        Dim sBuilder As New StringBuilder()
        Try

            MD5S = MD5.Create
            Dim Data As Byte() = MD5S.ComputeHash(Encoding.Default.GetBytes(src_txt))
            Dim i As Integer
            For i = 0 To Data.Length - 1
                sBuilder.Append(Data(i).ToString("x2"))
            Next i

        Catch ex As Exception

        Finally
            MD5S.Dispose()
        End Try

        ''Dim md5 As New MD5CryptoServiceProvider

        ''Dim bytHashCode() As Byte
        ''Dim BytData As Byte
        ''Dim bytOriginalText() As Byte
        ''Dim MD5_STR As String = vbNullString
        ''
        ''If src_txt.Length = 0 Then
        ''    Return "Nothing "
        ''    Exit Function
        ''End If
        ''
        ''bytOriginalText = System.Text.Encoding.Default.GetBytes(src_txt)
        ''bytHashCode = md5.ComputeHash(bytOriginalText)

        ''For Each BytData In bytHashCode
        ''    'Microsoftのサンプル型
        ''    'MD5_STR &= BytData.ToString & " "

        ''    'サイトなどで使用される一般的なMD5書式
        ''    '16進表記でスペース区切りなし
        ''''    MD5_STR &= Hex(BytData.ToString)　　'←こっちは間違いか？
        ''    MD5_STR &= BytData.ToString("x2") 
        ''Next
        ''
        ''Return MD5_STR

        Return sBuilder.ToString

    End Function

    Public Function GetWord(ByVal srcText As String, ByVal ULNo() As String) As Integer

        Dim wordLength As Integer = srcText.Length
        Dim iloop As Integer

        For iloop = 1 To wordLength
            ULNo(iloop - 1) = srcText.Substring((wordLength - iloop), 1)
        Next iloop

        Return (wordLength - 1)

    End Function

    ''' <summary>
    ''' 文字列の簡易暗号化処理
    ''' </summary>
    ''' <param name="strData">元文字列</param>
    ''' <param name="Encodec">True:暗号化　False:複合化</param>
    ''' <param name="keyNo">暗号化キー(3桁でないと記号が複合化できない)</param>
    ''' <returns>暗号化もしくは複合化した文字列</returns>
    ''' <remarks></remarks>
    Public Function EncryptText(ByVal strData As String, ByVal Encodec As Boolean, ByVal keyNo As Integer) As String

        Dim iloop As Integer
        Dim WordCount As Integer
        Dim strTemp As String = ""
        Dim stepCount As Integer

        WordCount = strData.Length
        stepCount = keyNo.ToString.Length

        Try

            If Encodec = True Then
                For iloop = 1 To WordCount
                    strTemp &= (System.Convert.ToInt32(Convert.ToChar(strData.Substring(iloop - 1, 1))) Xor keyNo).ToString.PadLeft(stepCount, "0"c)
                Next iloop
            Else
                For iloop = 1 To WordCount Step stepCount
                    strTemp &= System.Convert.ToChar(Integer.Parse(strData.Substring(iloop - 1, stepCount)) Xor keyNo).ToString
                    Diagnostics.Debug.WriteLine(strTemp)
                Next iloop
            End If

        Catch ex As Exception
            strTemp = "ConversionError"
        End Try

        EncryptText = strTemp

    End Function

    ''' <summary>
    ''' SHA1 ハッシュ関数を使用して、ハッシュ メッセージ認証コード(HMAC)を計算する
    ''' </summary>
    ''' <param name="sourceText">演算元文字列</param>
    ''' <param name="keyText">演算キー</param>
    ''' <returns>HMACの演算結果</returns>
    ''' <remarks></remarks>
    Public Function calcHashHMACSHA1(ByVal sourceText As String, ByVal keyText As String) As String

        '' ''HMAC-SHA1を計算する文字列
        ' ''Dim s As String = "<1896.697170952@dbc.mtview.ca.us>tanstaaf"
        '' ''キーとする文字列
        ' ''Dim key As String = "password"
        Dim retString As String = "Failed"
        Dim hmac As System.Security.Cryptography.HMACSHA1 = Nothing

        Try

            '文字列をバイト型配列に変換する
            Dim data As Byte() = System.Text.Encoding.UTF8.GetBytes(sourceText)
            Dim keyData As Byte() = System.Text.Encoding.UTF8.GetBytes(keyText)

            'HMACSHA1オブジェクトの作成
            hmac = New System.Security.Cryptography.HMACSHA1(keyData)
            'ハッシュ値を計算
            Dim bs As Byte() = hmac.ComputeHash(data)
            'リソースを解放する
            hmac.Clear()

            'Byte型配列を16進数に変換
            Dim result As String = BitConverter.ToString(bs).ToLower().Replace("-", "")

            retString = result


        Catch ex As Exception

        Finally
            hmac.Dispose()

        End Try

        Return retString

    End Function

    ''' <summary>Determines if a password is sufficiently complex.</summary>
    ''' <param name="pwd">Password to validate</param>
    ''' <param name="minLength">Minimum number of password characters.</param>
    ''' <param name="numUpper">Minimum number of uppercase characters.</param>
    ''' <param name="numLower">Minimum number of lowercase characters.</param>
    ''' <param name="numNumbers">Minimum number of numeric characters.</param>
    ''' <param name="numSpecial">Minimum number of special characters.</param>
    ''' <returns>True if the password is sufficiently complex.</returns>
    Function ValidatePassword(ByVal pwd As String,
        Optional ByVal minLength As Integer = 8,
        Optional ByVal numUpper As Integer = 2,
        Optional ByVal numLower As Integer = 2,
        Optional ByVal numNumbers As Integer = 2,
        Optional ByVal numSpecial As Integer = 2) As Boolean

        ' Replace [A-Z] with \p{Lu}, to allow for Unicode uppercase letters.
        Dim upper As New System.Text.RegularExpressions.Regex("[A-Z]")
        Dim lower As New System.Text.RegularExpressions.Regex("[a-z]")
        Dim number As New System.Text.RegularExpressions.Regex("[0-9]")
        ' Special is "none of the above".
        Dim special As New System.Text.RegularExpressions.Regex("[^a-zA-Z0-9]")

        ' Check the length.
        If Len(pwd) < minLength Then Return False

        ' Check for minimum number of occurrences.
        If upper.Matches(pwd).Count < numUpper Then Return False
        If lower.Matches(pwd).Count < numLower Then Return False
        If number.Matches(pwd).Count < numNumbers Then Return False
        If special.Matches(pwd).Count < numSpecial Then Return False

        ' Passed all checks.
        Return True

    End Function

#Region "削除コード"

    '' ''Public Shared Function EncryptText(ByVal strData As String, ByVal Codec As Integer) As String
    '' ''    ''
    '' ''    '' 文字列の暗号化 Codec　0:暗号化　　1:複合化
    '' ''    ''

    '' ''    Dim iloop As Long
    '' ''    Dim WordCount As Long
    '' ''    Dim strTemp As String
    '' ''    Dim mstrKey As Integer

    '' ''    mstrKey = 0
    '' ''    For iloop = 0 To 7
    '' ''        mstrKey = mstrKey + Int16.Parse(Now().ToString("yyyyMMdd").Substring(iloop, 1))
    '' ''    Next iloop
    '' ''    mstrKey = mstrKey + 32

    '' ''    strTemp = ""
    '' ''    WordCount = strData.Length

    '' ''    If Codec = 0 Then
    '' ''        For iloop = 0 To WordCount - 1
    '' ''            strTemp = strTemp & CStr(Asc(strData.Substring(iloop, 1)) Xor mstrKey)
    '' ''        Next iloop
    '' ''    Else
    '' ''        For iloop = 0 To WordCount - 1 Step 2
    '' ''            strTemp = strTemp & Chr(strData.Substring(iloop, 2) Xor mstrKey)
    '' ''        Next iloop
    '' ''    End If

    '' ''    Return strTemp

    '' ''End Function

    ''未使用となったためコメントとして残しておく
    ''''''' <summary>
    ''''''' 文字列を暗号化する
    ''''''' </summary>
    ''''''' <param name="str">暗号化する文字列</param>
    ''''''' <param name="key">パスワード</param>
    ''''''' <returns>暗号化された文字列</returns>
    '' ''Public Function EncryptString(ByVal str As String, ByVal key As String) As String
    '' ''    '文字列をバイト型配列にする
    '' ''    Dim bytesIn As Byte() = System.Text.Encoding.UTF8.GetBytes(str)

    '' ''    'DESCryptoServiceProviderオブジェクトの作成
    '' ''    Dim des As New System.Security.Cryptography.DESCryptoServiceProvider

    '' ''    '共有キーと初期化ベクタを決定
    '' ''    'パスワードをバイト配列にする
    '' ''    Dim bytesKey As Byte() = System.Text.Encoding.UTF8.GetBytes(key)
    '' ''    '共有キーと初期化ベクタを設定
    '' ''    des.Key = ResizeBytesArray(bytesKey, des.Key.Length)
    '' ''    des.IV = ResizeBytesArray(bytesKey, des.IV.Length)

    '' ''    '暗号化されたデータを書き出すためのMemoryStream
    '' ''    Dim msOut As New System.IO.MemoryStream
    '' ''    'DES暗号化オブジェクトの作成
    '' ''    Dim desdecrypt As System.Security.Cryptography.ICryptoTransform = des.CreateEncryptor()
    '' ''    '書き込むためのCryptoStreamの作成
    '' ''    Dim cryptStreem As New System.Security.Cryptography.CryptoStream(msOut, desdecrypt, System.Security.Cryptography.CryptoStreamMode.Write)
    '' ''    '書き込む
    '' ''    cryptStreem.Write(bytesIn, 0, bytesIn.Length)
    '' ''    cryptStreem.FlushFinalBlock()
    '' ''    '暗号化されたデータを取得
    '' ''    Dim bytesOut As Byte() = msOut.ToArray()

    '' ''    '閉じる
    '' ''    cryptStreem.Close()
    '' ''    msOut.Close()

    '' ''    'Base64で文字列に変更して結果を返す
    '' ''    Return System.Convert.ToBase64String(bytesOut)

    '' ''End Function

    ''''''' <summary>
    ''''''' 暗号化された文字列を復号化する
    ''''''' </summary>
    ''''''' <param name="str">暗号化された文字列</param>
    ''''''' <param name="key">パスワード</param>
    ''''''' <returns>復号化された文字列</returns>
    '' ''Public Function DecryptString(ByVal str As String, ByVal key As String) As String
    '' ''    'DESCryptoServiceProviderオブジェクトの作成
    '' ''    Dim des As New System.Security.Cryptography.DESCryptoServiceProvider

    '' ''    '共有キーと初期化ベクタを決定
    '' ''    'パスワードをバイト配列にする
    '' ''    Dim bytesKey As Byte() = System.Text.Encoding.UTF8.GetBytes(key)
    '' ''    '共有キーと初期化ベクタを設定
    '' ''    des.Key = ResizeBytesArray(bytesKey, des.Key.Length)
    '' ''    des.IV = ResizeBytesArray(bytesKey, des.IV.Length)

    '' ''    'Base64で文字列をバイト配列に戻す
    '' ''    Dim bytesIn As Byte() = System.Convert.FromBase64String(str)
    '' ''    '暗号化されたデータを読み込むためのMemoryStream
    '' ''    Dim msIn As New System.IO.MemoryStream(bytesIn)
    '' ''    'DES復号化オブジェクトの作成
    '' ''    Dim desdecrypt As System.Security.Cryptography.ICryptoTransform = des.CreateDecryptor()
    '' ''    '読み込むためのCryptoStreamの作成
    '' ''    Dim cryptStreem As New System.Security.Cryptography.CryptoStream(msIn, desdecrypt, System.Security.Cryptography.CryptoStreamMode.Read)

    '' ''    '復号化されたデータを取得するためのStreamReader
    '' ''    Dim srOut As New System.IO.StreamReader(cryptStreem, System.Text.Encoding.UTF8)
    '' ''    '復号化されたデータを取得する
    '' ''    Dim result As String = srOut.ReadToEnd()

    '' ''    '閉じる
    '' ''    srOut.Close()
    '' ''    cryptStreem.Close()
    '' ''    msIn.Close()

    '' ''    Return result
    '' ''End Function

    ''''''' <summary>
    ''''''' 共有キー用に、バイト配列のサイズを変更する
    ''''''' </summary>
    ''''''' <param name="bytes">サイズを変更するバイト配列</param>
    ''''''' <param name="newSize">バイト配列の新しい大きさ</param>
    ''''''' <returns>サイズが変更されたバイト配列</returns>
    '' ''Private Function ResizeBytesArray(ByVal bytes() As Byte, ByVal newSize As Integer) As Byte()

    '' ''    Dim newBytes(newSize - 1) As Byte

    '' ''    If bytes.Length <= newSize Then

    '' ''        Dim i As Integer

    '' ''        For i = 0 To bytes.Length - 1
    '' ''            newBytes(i) = bytes(i)
    '' ''        Next i

    '' ''    Else

    '' ''        Dim pos As Integer = 0
    '' ''        Dim i As Integer
    '' ''        For i = 0 To bytes.Length - 1
    '' ''            newBytes(pos) = newBytes(pos) Xor bytes(i)
    '' ''            pos += 1
    '' ''            If pos >= newBytes.Length Then
    '' ''                pos = 0
    '' ''            End If
    '' ''        Next i

    '' ''    End If

    '' ''    Return newBytes
    '' ''End Function
#End Region

End Class

Public Class ClsCareerInfo

    ''' <summary>
    ''' 携帯キャリアチェック
    ''' </summary>
    ''' <param name="sAgent">UserAgentの戻り値</param>
    ''' <returns>キャリア名称</returns>
    ''' <remarks></remarks>
    Public Function Judge(ByVal sAgent As String) As String

        Dim strRet As String = "PC"

        'Docomo
        If Regex.IsMatch(sAgent, "DoCoMo") Then
            strRet = "DoCoMo"
        End If

        'AU
        If Regex.IsMatch(sAgent, "UP\.Browser") Then
            strRet = "AU"
        End If

        'SoftBank
        If Regex.IsMatch(sAgent, "J-PHONE|Vodafone|SoftBank|MOT|J-EMULATOR") Then
            strRet = "SoftBank"
        End If

        ' ''エミュレータ 
        ''If Regex.IsMatch(sAgent, "J-EMULATOR") Then
        ''    strRet = "EMULATOR"
        ''End If

        Return strRet

    End Function

End Class