Option Explicit On
Option Strict On
Imports System.Data
Imports System.Data.OleDb

Partial Class _Default
    Inherits System.Web.UI.Page

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

    ''' <summary>
    ''' ページの初期設定　情報をDBから読み込んで、ユーザーコントロールの動的配置
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

        Dim RqQuesNo As Integer = CType(Request.Item("QN"), Integer)                      ' アンケート番号
        Dim FilePath As String = Server.MapPath("Ques\QuesItem.mdb")
        'Dim DbCon As New OleDbConnection

        If RqQuesNo = 0 Then RqQuesNo = 1

        Using DbCon As New OleDbConnection

            If IO.File.Exists(FilePath) = True Then

                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & FilePath & ";" & "Jet OLEDB:Engine Type= 5")

                ' 挨拶文
                Dim GreetingsWords As String = ReadGreetingsWord(DbCon)

                If GreetingsWords IsNot Nothing Then
                    Me.exp.InnerHtml = GreetingsWords
                End If

                Dim ItemData() As Array
                Dim Title As String
                Dim Supplement As String
                Dim QuesNo As Integer
                Dim ItemCount As Integer
                Dim iloop As Integer
                Dim ItemList() As String
                Dim ItemNo As Integer = 0

                ItemData = CType(ReadItemDatas(DbCon, RqQuesNo), Array())                               ' 関数の戻り値をArray型に変換して格納する

                If ItemData.Count > 0 Then

                    For Each datas As Array In ItemData
                        QuesNo = Convert.ToInt32(datas.GetValue(2))
                        Title = datas.GetValue(3).ToString
                        Supplement = datas.GetValue(4).ToString
                        ItemCount = Convert.ToInt32(datas.GetValue(5))
                        ReDim ItemList(ItemCount - 1)

                        For iloop = 0 To ItemCount - 1
                            ItemList(iloop) = datas.GetValue(6 + iloop).ToString
                        Next iloop

                        Dim QuesItem As Question = CType(Page.LoadControl("~/usercontrol/question.ascx"), Question)     ' ユーザーコントロール定義

                        With QuesItem                                                                       '   パラメータの設定
                            .ID = ("Q" & ItemNo)
                            .ListCount = ItemCount
                            .TitleWord = Title
                            .SupplementWord = Supplement
                            .ItemWords = ItemList
                        End With
                        dynamicitems.Controls.Add(QuesItem)                                                 ' ユーザーコントロールの生成
                        QuesItem.Dispose()

                        ''Dim valid As New RequiredFieldValidator
                        ''valid.ControlToValidate = String.Format("Q{0}$RBLPerform", ItemNo)
                        ''valid.ErrorMessage = "どれか１つを選択してください"
                        ''valid.ForeColor = Drawing.Color.Red
                        ''valid.ToolTip = "どれか１つを選択してください"
                        ''valid.SetFocusOnError = True
                        ''valid.Text = "●"
                        ''valid.ID = ("rfv" & ItemNo.ToString)

                        ''dynamicitems.Controls.AddAt(1, valid)
                        ''valid.Dispose()

                        ItemNo += 1
                    Next

                    Session.Add("itmCnt", ItemNo)
                    Session.Add("QueNo", QuesNo)
                End If

            End If

            'DbCon.Dispose()

        End Using

    End Sub

    ''' <summary>
    ''' アンケート項目の読み込み
    ''' </summary>
    ''' <param name="DbCon"></param>
    ''' <returns>DBから読み込んだ情報</returns>
    ''' <remarks></remarks>
    Private Function ReadItemDatas(ByVal DbCon As OleDb.OleDbConnection, ByVal RqQuesNo As Integer) As Array

        Dim DataArray() As Array
        'Dim DbDa As OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String = ""
        Dim iloop As Integer

        strSQL = ("SELECT * FROM ITEMS WHERE 表示期限 >=#" & DateTime.Now.ToString & "# AND QuestionNo=" & RqQuesNo.ToString)
        Using DbDa As New OleDbDataAdapter(strSQL, DbCon)
            Using DtSet As New DataSet("DData")
                DbDa.Fill(DtSet, "DData")

                Dim DsetCount As Integer = DtSet.Tables("DData").Rows.Count - 1

                ReDim DataArray(DsetCount)
                If DsetCount <> -1 Then                                                         '指定日付範囲でデータがある場合
                    iloop = 0
                    ''Dim stpw As Stopwatch = Stopwatch.StartNew
                    For Each DTR As DataRow In DtSet.Tables("DData").Rows
                        DataArray(iloop) = DTR.ItemArray
                        iloop += 1
                    Next
                End If

                'DbDa.Dispose()
                'DtSet.Dispose()
            End Using
        End Using

        Return DataArray

    End Function


    ''' <summary>
    ''' 挨拶文の読み込み
    ''' </summary>
    ''' <param name="DbCon"></param>
    ''' <returns>挨拶文</returns>
    ''' <remarks></remarks>
    Private Function ReadGreetingsWord(ByVal DbCon As OleDb.OleDbConnection) As String

        Dim strWords As String = Nothing
        'Dim DbDa As OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String = ""
        Dim DsetCount As Integer
        Dim GreetingsWord As String = Nothing

        strSQL = ("SELECT [表示期限],[挨拶文] FROM GREETINGS WHERE 表示期限 >=#" & DateTime.Now.ToString & "#")
        Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
            Using DtSet As New DataSet("DData")
                DbDa.Fill(DtSet, "DData")

                DsetCount = DtSet.Tables("DData").Rows.Count - 1

                Try
                    If DsetCount >= 0 Then
                        GreetingsWord = DtSet.Tables("DData").Rows(DsetCount).Item("挨拶文").ToString
                    End If

                    strWords = GreetingsWord.ToString

                Catch ex As Exception

                    'Finally
                    '    DbDa.Dispose()
                    '    DtSet.Dispose()
                End Try
            End Using
        End Using
        Return strWords

    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"

        If User.Identity.IsAuthenticated = False Then
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer
        LoginStatus = CType(Session.Item("LgSt"), Integer)          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            Response.Write(strScript)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする

        Dim FilePath As String = Server.MapPath("Ques\QuesResult.mdb")

        If IO.File.Exists(FilePath) = False Then                        ' アンケート結果の保存DBが無い場合は作成する
            Dim blnRet As Boolean = MakeDBFiles(FilePath)
        End If

    End Sub

    Private Function MakeDBFiles(ByVal FilePath As String) As Boolean

            Dim blnRet As Boolean
        'Dim cn As OleDb.OleDbConnection
            Dim strSQL As String = Nothing

            blnRet = MakeDatabase(FilePath)                             ' DBファイル(のみ)作成
            If blnRet = False Then
                Threading.Thread.Sleep(300)
                blnRet = MakeDatabase(FilePath)                         ' 1回だけリトライ
            End If

            If blnRet = True Then                                       ' 入れ物ができたら、テーブルを作成する

            strSQL = "CREATE TABLE [RESULTS]" & _
                        "(ID COUNTER PRIMARY KEY,SendDate DATETIME,QuestionNo SMALLINT,UserID TEXT(16),SiteFolder TEXT(32),RemoteADDRESS TEXT(64)," & _
                        "RemoteHOST TEXT(255),SiteNameDB TEXT(50),SiteNameWEB TEXT(128),StaffWEB TEXT(32),Comment MEMO,Age SMALLINT," & _
                        "Result1 SMALLINT DEFAULT 0,Result2 SMALLINT DEFAULT 0,Result3 SMALLINT DEFAULT 0,Result4 SMALLINT DEFAULT 0,Result5 SMALLINT DEFAULT 0," & _
                        "Result6 SMALLINT DEFAULT 0,Result7 SMALLINT DEFAULT 0,Result8 SMALLINT DEFAULT 0,Result9 SMALLINT DEFAULT 0,Result10 SMALLINT DEFAULT 0," & _
                        "Result11 SMALLINT DEFAULT 0,Result12 SMALLINT DEFAULT 0,Result13 SMALLINT DEFAULT 0,Result14 SMALLINT DEFAULT 0,Result15 SMALLINT DEFAULT 0," & _
                        "Result16 SMALLINT DEFAULT 0,Result17 SMALLINT DEFAULT 0,Result18 SMALLINT DEFAULT 0,Result19 SMALLINT DEFAULT 0,Result20 SMALLINT DEFAULT 0)"
                '                           ↑これは、SQLビューではエラーになる。コードで確認すること！

                'strSQL = "CREATE TABLE [UserInfo](HashValue TEXT(64) PRIMARY KEY NOT NULL,UserName TEXT(32),Passwords TEXT(32),LimitDate DATETIME,IPAddress TEXT(16))"

                Dim connectionString As String = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FilePath)

            Using cn As New OleDb.OleDbConnection
                'cn = New OleDbConnection(connectionString)
                cn.ConnectionString = connectionString

                If strSQL IsNot Nothing Then
                    Using oleCmd As OleDbCommand = New OleDbCommand(strSQL, cn)
                        cn.Open()
                        oleCmd.ExecuteNonQuery()                                                    ' テーブルの作成
                        cn.Close()
                        blnRet = True
                    End Using
                Else

                End If
            End Using
            'cn.Dispose()

        End If

        Return blnRet

    End Function


    ''' <summary>
    ''' アンケート結果を保存するデータベースファイルの作成
    ''' </summary>
    ''' <returns>成否　True:成功　False:失敗</returns>
    ''' <remarks>アクセスのファイルは、これ以外で作成する方法なし・・・</remarks>
    Private Function MakeDatabase(ByVal FilePath As String) As Boolean

        Dim blnRet As Boolean = False
        Dim con As ADODB.Connection
        Dim cat As ADOX.Catalog

        Try

            cat = CType(Microsoft.VisualBasic.CreateObject("ADOX.Catalog"), ADOX.Catalog)
            cat.Create("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FilePath) '& ";Jet OLEDB:Engine Type= 5;Jet OLEDB:Encrypt Database=True")

            'コネクションを開放する これをやらないと、ldbファイルが残ってしまう　プログラム終了しないと消せない
            con = CType(cat.ActiveConnection, ADODB.Connection)
            con.Close()

            blnRet = True

        Catch ex As Exception

        Finally
            cat = Nothing
            con = Nothing

        End Try

        Return blnRet

    End Function

    ''' <summary>
    ''' 送信されたデータから、結果をデータベースに保存する
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub SendData_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles SendData.Click

        Dim ItemCount As Integer = Convert.ToInt32(Session.Item("itmCnt"))
        Dim QuesNo As Integer = Convert.ToInt32(Session.Item("QueNo"))

        Dim iloop As Integer
        Dim ListName As String = Nothing
        'Dim QuesItem As Question
        Dim RBL As RadioButtonList
        Dim ResultData(ItemCount - 1) As Integer
        Dim RequestComment As String = "未記入"
        Dim StaffName As String = "未記入"
        Dim StaffAge As Integer = 0
        Dim GenbaName As String = "未記入"
        Dim strTemp As String = Nothing

        strTemp = Me.FeedbackText.Text
        If strTemp.Length > 0 Then
            RequestComment = Me.FeedbackText.Text                       ' コメント
        End If

        strTemp = Me.custname.Value
        If strTemp.Length > 0 Then
            StaffName = Me.custname.Value                               ' 氏名
        End If

        StaffAge = Convert.ToInt32(Me.ddlAge.SelectedValue)             ' 年齢

        strTemp = Me.ConstSite.Value
        If strTemp.Length > 0 Then
            GenbaName = Me.ConstSite.Value                              ' 現場名
        End If

        'ラジオボタンリストの選択内容を取得する
        For iloop = 0 To ItemCount - 1
            ListName = String.Format("Q{0}$RBLPerform", iloop)
            'QuesItem = CType(FindControl("Q" & iloop.ToString), Question)
            RBL = CType(FindControl(ListName), RadioButtonList)
            'Diagnostics.Debug.WriteLine(ListName & "," & RBL.Items.Count - RBL.SelectedIndex)

            ResultData(iloop) = (RBL.Items.Count - RBL.SelectedIndex)                 ' 一番左を最大値とするため、差分を取る
        Next

        ' データベースに保存
        Dim blnRet As Boolean = SaveResult(QuesNo, ResultData, StaffName, StaffAge, GenbaName, RequestComment)

        If blnRet = True Then
            Response.Redirect("~/thanks.aspx")                                              ' 御礼表示
        Else
            Response.Redirect("~/questionnaire.aspx")                                              ' 御礼表    
        End If

    End Sub

    ''' <summary>
    ''' アンケート結果の保存
    ''' </summary>
    ''' <param name="QuesNo">アンケート番号</param>
    ''' <param name="ResultData">アンケートの結果(ラジオボタンの番号の配列で渡す）</param>
    ''' <returns>成否　True:成功　False:失敗</returns>
    ''' <remarks></remarks>
    Private Function SaveResult(ByVal QuesNo As Integer, ByVal ResultData() As Integer, ByVal StaffName As String, ByVal StaffAge As Integer, ByVal GenbaName As String, ByVal RequestComment As String) As Boolean

        Dim blnRet As Boolean = False
        Dim FilePath As String = Server.MapPath("Ques\QuesResult.mdb")
        Dim RegistDate As DateTime = DateTime.Now()

        If IO.File.Exists(FilePath) = False Then                        ' アンケート結果の保存DBが無い場合は作成する
            blnRet = MakeDBFiles(FilePath)
        End If

        'Dim cn As OleDbConnection = New OleDbConnection
        'Dim dataAdp As OleDbDataAdapter = Nothing
        'Dim dSet As New DataSet("RESULT")
        'Dim cdteBuild As OleDbCommandBuilder = Nothing
        Dim DTable As DataTable = Nothing
        Dim strTemp As String = Nothing
        Dim sb As New Text.StringBuilder
        Dim iloop As Integer
        Dim ShortSiteName As String = CType(Session.Item("SSN"), String)
        Dim SiteDirectory As String = CType(Session.Item("SD"), String)
        Dim UserName As String = CType(Session.Item("UN"), String)
        'セッション変数に保存しないようにしたため変更 2018/03/30 Kino
        ''Dim SessionInf As String = CType(Session("RemoteInfo"), String)
        Dim clsBr As New ClsHttpCapData
        Dim SessionInf As String = clsBr.GetRemoteInfo(Request, Server.MapPath(""))
        clsBr = Nothing
        Dim remInf() As String
        Dim strResult As String = String.Empty

        If UserName Is Nothing Then
            UserName = "不明"
        End If

        If SiteDirectory Is Nothing Then
            SiteDirectory = "不明"
        End If

        If ShortSiteName Is Nothing Then
            ShortSiteName = "不明"
        End If

        Using cn As New OleDbConnection

            Try
                If SessionInf IsNot Nothing Then                                        ' リファラー情報
                    remInf = SessionInf.Split(","c)
                Else
                    ReDim remInf(1)
                    remInf(0) = "None"
                    remInf(1) = "None"
                End If

                For iloop = 1 To ResultData.Length
                    strTemp &= ("Result" & iloop.ToString & ",")                        ' Result*フィールドを作成
                    strResult &= (ResultData(iloop - 1).ToString & ",")                 ' 結果のデータを作成
                Next
                If strTemp IsNot Nothing Then                                           ' 2018/06/18 Kino Changed
                    strTemp = strTemp.TrimEnd(Convert.ToChar(","))
                    strTemp = strTemp.TrimEnd(Convert.ToChar(","))
                End If

                cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FilePath)
                Using dataAdp As New OleDbDataAdapter("SELECT * FROM RESULTS WHERE SendDate >= #" & DateTime.Now.AddDays(-1).ToShortDateString & "# ORDER BY ID ASC", cn)
                    '                                                                    ↑　[=>] はNG　[>=] はOK
                    Using dSet As New DataSet("RESULT")

                        dataAdp.Fill(dSet, "RESULT")
                        Using cdteBuild As New OleDbCommandBuilder(dataAdp)
                            DTable = dSet.Tables("RESULT")

                            ' 更新クエリの作成
                            Dim addQuery As String = Nothing
                            If strTemp IsNot Nothing Then                                           ' 2018/06/18 Kino Add
                                addQuery = "INSERT INTO RESULTS (SendDate,QuestionNo,UserID,SiteFolder,RemoteADDRESS,RemoteHOST,SiteNameDB,SiteNameWEB,StaffWEB,Comment,Age," & strTemp & ") VALUES ("
                            Else
                                addQuery = "INSERT INTO RESULTS (SendDate,QuestionNo,UserID,SiteFolder,RemoteADDRESS,RemoteHOST,SiteNameDB,SiteNameWEB,StaffWEB,Comment,Age) VALUES ("
                            End If
                            sb.Append(addQuery)
                            sb.Append("#" & RegistDate & "#")
                            sb.Append(",")
                            sb.Append(QuesNo.ToString)
                            sb.Append(",")
                            sb.Append("'" & UserName & "'")
                            sb.Append(",")
                            sb.Append("'" & SiteDirectory & "'")
                            sb.Append(",")
                            sb.Append("'" & remInf(0) & "'")
                            sb.Append(",")
                            sb.Append("'" & remInf(1) & "'")
                            sb.Append(",")
                            sb.Append("'" & ShortSiteName & "'")
                            sb.Append(",")
                            sb.Append("'" & GenbaName & "'")
                            sb.Append(",")
                            sb.Append("'" & StaffName & "'")
                            sb.Append(",")
                            sb.Append("'" & RequestComment & "'")
                            sb.Append(",")
                            sb.Append("'" & StaffAge & "'")
                            If strTemp IsNot Nothing Then                                           ' 2018/06/18 Kino Add
                                sb.Append(",")
                                sb.Append(strResult)
                            End If
                            sb.Append(")")

                            Dim strSQL As String = sb.ToString
                            sb.Length = 0

                            ' 追加処理
                            Dim com As New OleDb.OleDbCommand
                            cn.Open()
                            com.Connection = cn
                            com.CommandText = strSQL
                            com.ExecuteNonQuery()

                            blnRet = True
                        End Using
                    End Using
                End Using
            Catch ex As Exception

                blnRet = False

            Finally
                DTable.Dispose()
                cn.Close()

            End Try

        End Using

        Return blnRet

    End Function

End Class
