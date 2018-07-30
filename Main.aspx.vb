Imports System.Data
Imports System.Drawing

''' <summary>
''' 初期型DABROSトップ画面　未調整のため表示不可
''' </summary>
''' <remarks></remarks>
Partial Class Main

    Inherits System.Web.UI.Page

    Protected DataLabel As System.Web.UI.WebControls.Label          'ラベルを宣言する
    Private DataFileNames() As ClsReadDataFile.DataFileInf
    Private AlertInfo(6) As ClsReadDataFile.AlertInf
    Private AlertJudgeNo(,) As Short                                ''警報判定次数指定 2010/06/03 Kino Changed 2次元配列化

    ''Private AlertData(6) As ClsReadDataFile.AlertInf                '警報表示情報

    Protected Sub Page_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed

        Session.Abandon()                       '現在のセッションに破棄のマークを付けます（セッションを破棄する）
        ''Session.Clear()
        Response.Cookies.Clear()
        ''Response.Cookies.Add(New HttpCookie("ASP.NET_SessionId", ""))

    End Sub

    Protected Sub Page_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error

        ''Dim sw As New IO.StreamWriter(Server.MapPath("~/errorSummary.log"), True, Encoding.GetEncoding("Shift_JIS"))

        Dim ex As Exception = Server.GetLastError().GetBaseException()
        ''Dim UserName As String = CType(Session.Item("UN"), String)
        Call WriteErrorLog(MyBase.GetType.BaseType.FullName, ex.Message.ToString, ex.StackTrace.ToString)

    End Sub

    Protected Sub WriteErrorLog(ByVal ProcName As String, Optional ByVal exMes As String = "None", Optional ByVal exStackTrace As String = "None")

        Dim sw As IO.StreamWriter = Nothing
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

        Try
            sw = New IO.StreamWriter(Server.MapPath("~/log/errorSummary.log"), True, Encoding.GetEncoding("Shift_JIS"))
            Dim sb As New StringBuilder
            sb.Append(DateTime.Now.ToString)
            sb.Append(ControlChars.Tab)
            sb.Append(ProcName)
            sb.Append(ControlChars.Tab)
            sb.Append(userName)
            sb.Append(ControlChars.Tab)
            sb.Append(siteDirectory)
            sb.Append(ControlChars.Tab)
            sb.Append(exMes)
            sb.Append(ControlChars.Tab)
            sb.Append(exStackTrace)
            sw.WriteLine(sb.ToString)
        Catch

        Finally
            sw.Dispose()
        End Try

    End Sub

    Protected Overridable Sub OnAuthenticate(ByVal e As AuthenticateEventArgs)
        e.Authenticated = True
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Page.Request("__EVENTTARGET") IsNot Nothing AndAlso Page.Request("__EVENTTARGET").ToString = "ctl00$LoginStatus1$ctl00" Then                   ' 2011/05/25 Kino Add ログアウトが押された時は処理しない 
            ' デバッグ用のためコメント
            ''Dim sw As New IO.StreamWriter(Server.MapPath("~/errorSummary.log"), True, Encoding.GetEncoding("Shift_JIS"))
            ''Dim sb As New StringBuilder
            ''sb.Append(DateTime.Now.ToString)
            ''sb.Append(ControlChars.Tab)
            ''sb.Append(CType(Session.Item("UN"), String))
            ''sb.Append(ControlChars.Tab)
            ''sb.Append("ログアウトボタン押下によりログイン画面へ遷移:" & Page.Request("__EVENTTARGET").ToString)
            ''sw.WriteLine(sb.ToString)
            ''sw.Dispose()
            Exit Sub
        End If

        If User.Identity.IsAuthenticated = False Then

            Dim userName As String = CType(Session.Item("UN"), String)                      ' 2013/04/10 Kino Add IsAuthenticatedがFalseでもセッション変数の内容が参照できれば、再度Trueとする・・・できるか？
            If userName IsNot Nothing AndAlso userName.Length > 0 Then
                Dim authArg As New System.Web.UI.WebControls.AuthenticateEventArgs
                authArg.Authenticated = True
                OnAuthenticate(authArg)
            Else
                Call WriteErrorLog(MyBase.GetType.BaseType.FullName & " User.Identity not Found")
                Response.Redirect("Login.aspx")
                Exit Sub
            End If

        End If
        'FormsAuthentication.GetAuthCookie(CType(Session.Item("UN"), String), False)

        Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)
        'ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            System.Threading.Thread.Sleep(5000)                                                     ' 2010/06/01 Kino Add 再取得確認
            LoginStatus = CType(Session.Item("LgSt"), Integer)
            If LoginStatus = 0 Then
                Call WriteErrorLog(MyBase.GetType.BaseType.FullName & " LoginStatus not Found")

                Response.Redirect("Login.aspx")
                Exit Sub
            End If
        End If

        Dim lblExistDataFile As Label = CType(Page.Master.FindControl("lblExistDataFile"), Label)
        Dim SiteName As String
        Dim SiteDirectory As String
        Dim DataFile As String
        Dim strOldestDate As String = ""
        Dim strNewestDate As String = ""
        Dim DataSummaryName As String = ""
        Dim AlertCount As Integer
        Dim lngRet As Long = 1
        Dim imageFile As String = ""                                                                ' 2011/07/29 Kino Add
        Dim systemName As String = ""                                                               ' 2011/07/29 Kino Add

        SiteName = CType(Session.Item("SN"), String)
        SiteDirectory = CType(Session.Item("SD"), String)      '現場ディレクトリ

        ''Dim imagePath As String = Server.MapPath(SiteDirectory & "\img\")                         ’ Httpハンドラで使用していたが、不要となったためコメント 2018/05/30 Kino
        ''Session.Add("imgPath", imagePath)
        Response.BufferOutput = True

        If IsPostBack = False Then
            Call CheckAutoUpdate(SiteDirectory)                                                 '自動更新設定の読み込み

            ''Else
            ''    Dim dataLabels As Label
            ''    For Each dataLabels In Me.Controls
            ''        Me.Controls.Remove(dataLabels)
            ''    Next

            'Else
            '    Dim EventID As String = Page.Request.Params.Get("__EVENTTARGET")                        ''最終的に不要のためコメント 2010/04/12 
            '    If EventID <> Nothing Then                     'ログアウトボタンを押した時にエラーがでるため、処理をここで辞める(ラベルのIDを共通化した結果)
            '        If Page.Request.Params.Get("__EVENTTARGET").IndexOf("LoginStatus1") <> -1 Then
            '            Exit Sub
            '        End If
            '    End If

        End If

        Dim ErrorRetry As Integer = CType(Session.Item("Redraw"), Integer)                      ' 2011/05/25 Kino Add 読み込みエラーが発生していた場合は１がたつ
        If ErrorRetry = 1 Then
            Call CheckAutoUpdate(SiteDirectory)                                                 '自動更新設定の再読み込み
        End If

        DataFile = Server.MapPath(SiteDirectory + "\App_Data\MenuInfo.mdb")

        imageFile = getMainpageInf(systemName, DataFile)                                        ' メインページの画像ファイルとシステム名称を取得

        Page.Header.Title = SiteName + systemName.ToString         '' "－ インターネットデータ閲覧サービス"                    'タイトルバーのタイトル設定 2011/07/29 Kino Changed ファイルから読み込みする
        Me.Image2.ImageUrl = "~/api/thumbnail.ashx?img=" + imageFile.ToString   'MainPic.png"                  '平面図を表示       'データ一覧を出さない場合はファイル固定とする 2011/07/29 Kino Changed ファイルから読み込みする

        Dim strRet As String = GetOldDataTerm(DataFile)
        Session.Item("OldTerm") = strRet
        ''intRet = ClsReadDataFile.GetDataFileNames(DataFile, DataFileNames)       'データファイル名、共通情報ファイル名、識別名を取得
        lngRet = GetOldAndNewestDate(Server.MapPath(SiteDirectory + "\App_Data\"))

        If lngRet = -1 Then                                                         ' 2011/05/23 Kino Add データが読めなければセッションエラーとする

            Call WriteErrorLog(MyBase.GetType.BaseType.FullName, "最新・古日付取得読込エラー(１分後再読み込み)")

            ''Response.Redirect("sessionerror.aspx")
            Session.Add("Redraw", 1)
            Me.LblAlert.Text = "データ読込ができなかったため1分後に更新を行います"
            Me.LblAlert.BackColor = Color.PaleGreen
            Me.LblAlert.ForeColor = Color.Blue
            Me.LblAlert.Visible = True
            Me.TmrCheckNewData.Interval = 60000

            ''Dim strTemp As String = Request.UrlReferrer.AbsoluteUri.ToString()
            ''Response.Redirect(strTemp)

            ''Dim btncancel As Button = CType(FindControl("ctl00$CntplHolder$BtnCan"), Button)              ' 2011/07/29 Kino これではダメだった・・・　orz
            ''btncancel.Attributes.Add("onclick", "CancelAsyncPostBackA();")
            ''Call BtnCan_Click(sender, e)
            Exit Sub
        End If

        'データファイルへのパスを取得する
        '' ''DataFile = Server.MapPath(SiteDirectory + "\App_Data\" + SiteDirectory + "_CalculatedData.mdb")            '' 2010/01/15 Kino Changed これは不要ではないか？？？　でコメント
        AlertCount = CheckAlertStatus(SiteDirectory)                                            '警報発生状況確認

        '' 2010/06/03 Kino Changed 警報音をストリーミング再生だとどうもレスポンスが悪いのでコメント
        ''If Me.ImgAlert.Visible = True Then                                                                 '2010/06/03 Kino Add 
        ''    Me.LitAlm.Visible = True
        ''    Me.LitAlm.Text = "<EMBED src='./snd/AlmSnd.mp3' type='application/x-mplayer2' autostart='true' width='50' height='45' PlayCount='0'>"
        ''Else
        ''    Me.LitAlm.Text = Nothing
        ''    Me.LitAlm.Visible = False
        ''    '' ''==== フォームのサイズを調整する ====
        ''    ''If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
        ''    ''    Dim OpenString As String
        ''    ''    OpenString = "<SCRIPT LANGUAGE='javascript'>window.open('Alert.html',null,'width=50,Height=100,top=0,left=0,menubar=no,toolbar=no,scrollbars=no,status=no,resizable=no,location=no,directories=no,titlebar=no');</SCRIPT>"
        ''    ''    Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "メイン", OpenString)
        ''    ''End If
        ''    '' ''====================================
        ''End If

        lngRet = 1
        ''データファイルの最終日を取得して画面へ表示する
        If System.IO.File.Exists(DataFile) = True Then

            Call DataCollectCheck()     'strOldestDate, strNewestDate)                                                 'データの収録状況確認
            ''ClsReadDataFile.dteLastUpdate = Date.Parse(strNewestDate)
            lblExistDataFile.Text = "0"

            If System.IO.File.Exists(Server.MapPath(SiteDirectory + "\App_Data\" + "MainDataSummary.show")) = True Then                'データ表示フラグファイルがあった場合、データを読み込む
                Dim txtFile As New IO.StreamReader(Server.MapPath(SiteDirectory + "\App_Data\" + "MainDataSummary.show"), System.Text.Encoding.Default) '.GetEncoding("shift_jis"))
                DataSummaryName = txtFile.ReadLine
                txtFile.Close()
                lngRet = ReadDataSummary(DataSummaryName, SiteDirectory, AlertCount)                                        'メイン画面へのデータ一覧表示
                If lngRet = -1 Then Call NoDatasOperation(lblExistDataFile)
            End If
            If System.IO.File.Exists(Server.MapPath(SiteDirectory + "\App_Data\" + "MainDataSummaryCoord.show")) = True Then
                '' Dim OpenString As String = "<script type='text/javascript'>window.document.onmousemove = getMouseXY;function getMouseXY(evt){if (window.createPopup){x = event.x + document.body.scrollLeft;y = event.y + document.body.scrollTop;}else{x = evt.pageX;y = evt.pageY;}document.getElementById('result1').innerHTML = '(x,y) = '+x + ', '+y;if (document.all){cx = event.offsetX;cy = event.offsetY;}else{cx = evt.layerX;cy = evt.layerY;}document.getElementById('result2').innerHTML = '(offsetX,offsetY) = '+cx + ', '+cy;window.status = '(offsetX,offsetY) = '+cx + ', '+cy;}</script>"
                ''Dim OpenString As String = "<script type='text/javascript'>window.document.onmousemove = getMouseXY;function getMouseXY(evt){if (window.createPopup){x = event.x + document.body.scrollLeft;y = event.y + document.body.scrollTop;}else{x = evt.pageX;y = evt.pageY;}document.getElementById('result1').innerHTML = '(Top,Left) = '+y + ', '+x;if (document.all){cx = event.offsetX;cy = event.offsetY;}else{cx = evt.layerX;cy = evt.layerY;}document.getElementById('result2').innerHTML = '(offset Top,offset Left) = '+cy + ', '+cx;window.status = '(offset Top,offset Left) = '+cy + ', '+cx;}</script>" ' 2012/05/07 Kino Changed  X,Y入替
                Dim OpenString As String = "<script type='text/javascript' src='js/mousepoint.js'></script>"
                Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "メイン", OpenString)
                Me.PnlCoordinate.Visible = True
            End If
        Else
            Call NoDatasOperation(lblExistDataFile)

        End If

    End Sub

    Protected Function getMainpageInf(ByRef systemName As String, ByVal dataFile As String) As String

        Dim DbCon As New OleDb.OleDbConnection
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strRet As String = "None"
        ''Dim Dt As DataTable
        ''Dim tableNames As String = ""

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + dataFile + ";Jet OLEDB:Engine Type= 5;Jet OLEDB:Database Locking Mode=1")

        ' テーブル一覧を取得して存在していなかったら読まないようにするつもりだったが、すべてのDBへテーブルをエクスポートしたので未仕様とする  2011/07/29 Kino Add
        ''DbCon.Open()
        ' ''Dt = DbCon.GetOleDbSchemaTable(OleDb.OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, "TABLE"})
        ''Dt = DbCon.GetSchema("Tables")
        ''For iloop As Integer = 0 To Dt.Rows.Count - 1
        ''    tableNames += ("@" & Dt.Rows(iloop)("TABLE_NAME").ToString & "@")
        ''Next
        ''DbCon.Close()

        '' メインページの画像ファイル名とシステム名称の読み込み
        DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM メインページ ORDER BY ID ASC", DbCon)
        DbDa.Fill(DtSet, "DData")
        If DtSet.Tables("DData").Rows.Count > 0 Then
            strRet = DtSet.Tables("DData").Rows(0).Item(1)
            systemName = DtSet.Tables("DData").Rows(0).Item(2)
        End If

        DbCon.Dispose()

        Return strRet

    End Function

    Protected Sub NoDatasOperation(ByVal lblExistDataFile As Label)

        Dim HeaderRow As New TableRow()
        Dim HeaderCell As New TableCell()

        Me.tblLastUpdate.Rows.Clear()
        HeaderCell.Text = " データ未収録"
        HeaderCell.ColumnSpan = 4
        HeaderRow.Cells.Add(HeaderCell)
        HeaderRow.TableSection = TableRowSection.TableHeader
        Me.tblLastUpdate.Rows.Add(HeaderRow)

        '' ''Me.LblLastUPDate.Text = " データ未収録"         ' 2011/03/24 Kino Changed  Tableへ移行
        lblExistDataFile.Text = "99"

    End Sub

    Protected Function GetOldDataTerm(ByVal dataFile As String) As String

        Dim DbCon As New OleDb.OleDbConnection
        ' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strRet As String = "None"

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + dataFile + ";Jet OLEDB:Engine Type= 5")
        ''DbCon.Open()

        '' 過去データ表示制限の読み込み
        DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM 過去データ制限 ORDER BY ID ASC", DbCon)
        DbDa.Fill(DtSet, "DData")

        ''DbCon.Close()

        If DtSet.Tables("DData").Rows(0).Item(1) = True Then
            strRet = DtSet.Tables("DData").Rows(0).Item(2)
        End If
        DbCon.Dispose()

        Return strRet

    End Function

    Protected Function ReadDataSummary(ByVal strGraphName As String, ByVal SiteDirectory As String, ByVal AlertCount As Integer) As Long
        ''
        '' データ一覧の設定を読み込み、データ一覧を表示する(最新データのみ)
        ''
        Dim DataFilePath As String
        Dim ShowDatas() As ShowDataSet = {}
        Dim ShowDataInf() As Array = {}
        Dim lblOffset As OffsetCoordinates
        Dim DataFileCount As Integer
        Dim PicFile As String = ""
        Dim PlusOffset As Single = 0
        Dim clsSummary As New ClsDataSummary
        Dim lngRet As Long = 0
        Dim retryCount As Integer = 0
        Dim fieldCount As Integer = 0                                                                                                       ' 2012/02/10 Kino Add

        DataFilePath = Server.MapPath(SiteDirectory)
        If LblAlert.Visible = True Then
            PlusOffset = 0.0       '27.0           '35   '23.0   2009/07/01 Kino Changed       2012/02/22 Kino Changed　27.0->25.5
        Else
            PlusOffset = -25.5           '3.0
        End If

        retryCount = 0
        Do                                                                                                                                  '' 2010/11/18 Kino Add　データ読み込みリトライ
            lngRet = clsSummary.ReadSummaryInfo(strGraphName, DataFilePath, ShowDatas, ShowDataInf, lblOffset, DataFileCount, PicFile, fieldCount)   '' 最新データを読み込む(DataSummaryGraphInfo.mdb) 2012/02/10 Kino Changed  Add FieldCount
            ' lngRet = clsSummary.ReadCommonInfData(DataFilePath, 1, DataFileCount, ShowDatas, DataFileNames)                                 '' 表示するデータを読み込む(***_CommonInf.mdb)
            If lngRet = 0 Then
                Exit Do
            ElseIf lngRet = -1 Then
                Exit Do
            Else
                System.Threading.Thread.Sleep(2000)
                retryCount += 1
                If retryCount >= 20 Then
                    Exit Do
                End If
            End If
        Loop

        retryCount = 0
        Do                                                                                                                                  '' 2010/11/18 Kino Add　データ読み込みリトライ
            lngRet = clsSummary.ReadCommonInfData(DataFilePath, 1, DataFileCount, ShowDatas, DataFileNames)                                 '' 表示するデータを読み込む(***_CommonInf.mdb)
            If lngRet = 0 Then
                Exit Do
            ElseIf lngRet = -1 Then
                Exit Do
            Else
                System.Threading.Thread.Sleep(2000)
                retryCount += 1
                If retryCount >= 20 Then
                    Exit Do
                End If
            End If
        Loop

        If lngRet = 0 Then
            Dim ColumR As Panel = CType(Master.FindControl("pnlColR"), Panel)                                                                   ' 2018/06/04 Kino Add 親パーツ指定のため
            Call clsSummary.DynamicMakeLabels(ColumR, ShowDatas, ShowDataInf, lblOffset, DataFileCount, AlertCount, AlertInfo, _
                                                AlertJudgeNo, DataFileNames, DataFilePath, fieldCount, PlusOffset)     'データラベルを作成 2012/02/10 Kino Changed  Add fieldCount
        Else

        End If

        ''If PicFile.Length <> 0 Then                                                                                                 '画像ファイルが設定されていたら、
        ''    Me.Image2.ImageUrl = "thumbnail.ashx?img=" + PicFile
        ''    ''Me.MainPic.Src = ("~/" + SiteDirectory + "/img/" + PicFile)                                                               '       それを設定する
        ''End If

        clsSummary = Nothing
        Return lngRet

    End Function


    Protected Sub CheckAutoUpdate(ByVal SiteDirectory As String)

        Dim DataFile As String
        Dim DbCon As New OleDb.OleDbConnection
        ''Dim DbDr As OleDb.OleDbDataReader
        ''Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("UPDATE")
        Dim strSQL As String = ""
        Dim intInterval As Integer = 600000
        Dim blnAutoUpdate As Boolean = False

        DataFile = Server.MapPath(SiteDirectory + "\App_Data\MenuInfo.mdb")

        If System.IO.File.Exists(DataFile) = True Then
            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + DataFile + ";" + "Jet OLEDB:Engine Type= 5"
            ''DbCon.Open()

            strSQL = "SELECT [自動更新有効],[更新間隔(ms)] FROM 自動更新 WHERE 自動更新項目 = 'メイン'"
            DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
            DbDa.Fill(DtSet, "UPDATE")

            If DtSet.Tables("UPDATE").Rows.Count > 0 Then
                blnAutoUpdate = DtSet.Tables("UPDATE").Rows(0).Item(0)
                intInterval = DtSet.Tables("UPDATE").Rows(0).Item(1)
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
            DbDa.Dispose()
            DtSet.Dispose()
            DbCon.Dispose()

        End If

        'タイマーの有効／無効を設定
        Me.TmrCheckNewData.Enabled = blnAutoUpdate
        Me.TmrCheckNewData.Interval = intInterval

    End Sub

    Protected Function CheckAlertStatus(ByVal SiteDirectory As String) As Integer
        ''
        '' 最新データにおける警報状況を確認して表示する
        ''
        'Dim DbCon As New OleDb.OleDbConnection
        ''Dim DbDr As OleDb.OleDbDataReader
        ''Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strSQL As String
        Dim DataFile As String
        Dim AlertShowEnable As Boolean
        Dim clsSummary As New ClsReadDataFile
        Dim clsAlert As New ClsReadDataFile
        Dim iloop As Integer
        Dim jloop As Integer
        Dim dataFolderPath As String = Server.MapPath(SiteDirectory + "\App_Data\")                                         '' 2010/10/07 KinoAdd
        Dim AlertCheck As Integer = 0               '2009/07/27 Kino Add

        ''警報表示の有効無効を取得----
        DataFile = (dataFolderPath + "MenuInfo.mdb")

        Using DbCon As New OleDb.OleDbConnection

            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + DataFile + ";Jet OLEDB:Engine Type= 5"
            ''DbCon.Open()

            strSQL = "SELECT 警報表示有効 FROM 警報設定"

            'DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
            Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)

                DbDa.Fill(DtSet, "DData")

                If DtSet.Tables("DData").Rows.Count > 0 Then
                    AlertShowEnable = DtSet.Tables("DData").Rows(0).Item(0)
                End If

                ''DbCom = New OleDb.OleDbCommand(strSQL, DbCon)
                ''DbDr = DbCom.ExecuteReader

                ''If DbDr.HasRows = True Then
                ''    DbDr.Read()
                ''    AlertShowEnable = DbDr.GetBoolean(0)                '表示の有効無効を取得
                ''End If

                'DbDr.Close()
                'DbCon.Close()
                ''----------------------------

                '' 警報判定情報を取得する
                'DbDa = New OleDb.OleDbDataAdapter
                Using DbDa2 As New OleDb.OleDbDataAdapter("SELECT * FROM 警報判定フラグ ORDER BY ChNo ASC", DbCon)
                    'DtSet = New DataSet
                    Using DtSet2 As New DataSet
                        ''Dim DbDa As OleDb.OleDbDataAdapter
                        ''Dim DtSet As New DataSet("DData")

                        '' 過去データ表示制限の読み込み
                        'DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM 警報判定フラグ ORDER BY ChNo ASC", DbCon)                 '0より大きい次数のみを取得する       'WHERE 警報比較次数>0
                        DbDa2.Fill(DtSet2, "DData")
                    End Using
                End Using

                ''DbCon.Close()
                ''            DbCon.Dispose()
            End Using
        End Using

        Dim DsetCount As Integer = DtSet.Tables("DData").Rows.Count - 1
        Dim FieldCount As Integer = DtSet.Tables("DData").Rows(iloop).Table.Columns.Count - 2
        ReDim AlertJudgeNo(DtSet.Tables("DData").Rows(DsetCount).Item(0), FieldCount)
        For jloop = 0 To FieldCount
            For iloop = 0 To DsetCount
                'AlertJudgeNo(iloop, 0) = DtSet.Tables("DData").Rows(iloop).Item(0)
                AlertJudgeNo(DtSet.Tables("DData").Rows(iloop).Item(0), jloop) = DtSet.Tables("DData").Rows(iloop).Item(1)                         'チャンネル番号を配列番号として値を格納
            Next
        Next jloop
        ''----------------------------
        Me.Table_AlarmStatus.Visible = AlertShowEnable          '警報表示テーブルの表示設定         警報判定設定がなければ、表示しない
        'Me.imgSpace2.Visible = AlertShowEnable                  'スペーサー用GIF画像表示           警報表示のところで再設定しているためコメント
        ''If AlertShowEnable = False Then
        ''    Me.LitSpace1.Visible = False
        ''    Me.LitSpace1.Text = vbNullString
        ''Else
        ''    Me.LitSpace1.Visible = False
        ''    Me.LitSpace1.Text = "<br /><img src='./img/space_M.gif'><br />"   '"<br /><br />"
        ''End If
        ''Me.LitSpace2.Text = "<img src='./img/space_M.gif'><br />"   ' "<br />"
        ''Me.LitSpace3.Text = "<br /><img src='./img/space_M.gif'><br />"   ' "<br />"

        If AlertShowEnable = True Then

            Dim AlertDate(DataFileNames.Length - 1) As Date
            Dim AlertLevel(DataFileNames.Length - 1) As Integer
            Dim AlertState(1) As String
            Dim TempCount As Integer = 0
            Dim PreLevel As Integer = 0
            Dim TempAlertLevel As Integer = 0
            Dim strCount As String
            Dim LatestData() As Array = {}
            Dim AlertStatus As Integer
            ''Dim AlertCheck As Integer               '2009/07/27 Kino Add
            Dim ErrorRetry As Integer = CType(Session.Item("Redraw"), Integer)                      ' 2011/05/25 Kino Add 読み込みエラーが発生していた場合は１がたつ
            Dim retryCount As Integer                                                                ' 2011/05/27 Kino Add

            If IsPostBack = False Or ErrorRetry = 1 Then                                            ' 2011/05/25 Kino Add 　ErrorRetry....
                Session.Item("Redraw") = 0

                ''警報レベル表示設定を取得--------
                DataFile = Server.MapPath(SiteDirectory + "\App_Data\MenuInfo.mdb")
                TempCount = clsSummary.GetAlertDisplayInf(DataFile, AlertInfo)

                '' 変数から取得した警報表示内容をビューステートに格納する     変数　→　ビューステート
                ViewState("AlertSetCount") = TempCount - 1
                For iloop = 0 To TempCount - 1
                    strCount = iloop.ToString
                    ViewState("AlertLevels" + strCount) = AlertInfo(iloop).Levels                   ''clsAlert.AlertData(iloop).Levels
                    ViewState("AlertWords" + strCount) = AlertInfo(iloop).Words                     ''clsAlert.AlertData(iloop).Words
                    ViewState("BackColors" + strCount) = AlertInfo(iloop).BackColor
                    ViewState("ForeColors" + strCount) = AlertInfo(iloop).ForeColor
                Next iloop

            Else                                                                'ポストバックの時は、ビューステートから読み込む

                TempCount = CType(ViewState("AlertSetCount"), Integer)
                For iloop = 0 To TempCount
                    strCount = iloop.ToString
                    AlertInfo(iloop).Levels = CType(ViewState("AlertLevels" + strCount), Short)
                    AlertInfo(iloop).Words = CType(ViewState("AlertWords" + strCount), String)
                    AlertInfo(iloop).BackColor = CType(ViewState("BackColors" + strCount), Integer)
                    AlertInfo(iloop).ForeColor = CType(ViewState("ForeColors" + strCount), Integer)
                Next iloop
            End If

            ''----------------------------警報発生状況を確認
            AlertStatus = 0                                                         '2009/07/27 Kino Add 初期化
            Dim setDataFileNo As Integer = 0                                        '2010/10/07 Kino Add
            For jloop = 0 To DataFileNames.Length - 1
                retryCount = 0                                                      ' 2011/05/27 Kino Add 初期化
                If DataFileNames(jloop).CommonInf <> Nothing And System.IO.File.Exists(dataFolderPath + DataFileNames(jloop).CommonInf) = True Then                      '' 2010/10/06 Kino Add ファイル存在チェックを追加
                    DataFile = (dataFolderPath + DataFileNames(jloop).CommonInf)      'SiteDirectory & "_CommonInf.mdb")
                    PreLevel = 0
                    strSQL = "SELECT * From 共通情報 WHERE 警報判定結果 <> 0"

                    Do                                                                                                  ' 2011/05/27 Kino Add 読み込みリトライ
                        TempCount = 0
                        TempCount = clsSummary.GetLatestDataFromCommonInf(DataFile, LatestData, strSQL)                 'CommonInf.mdbの読込　この中でエラーが出ている
                        If TempCount = -2 Then
                            retryCount += 1
                            System.Threading.Thread.Sleep(2000)
                            If retryCount >= 15 Then
                                TempCount = -1
                                Exit Do
                            End If
                        Else
                            Exit Do
                        End If
                    Loop

                    ''AlertStatus = 0                                                         '2009/07/27 Kino Add 初期化
                    If TempCount >= 0 Then                                                  '行が存在した場合は、警報情報を比較して最大次数の警報値を取得する
                        For iloop = 0 To TempCount
                            TempAlertLevel = LatestData(iloop).GetValue(17)                 '警報レベル
                            If Math.Abs(AlertLevel(jloop)) < Math.Abs(TempAlertLevel) And _
                                    Math.Abs(TempAlertLevel) >= AlertJudgeNo(LatestData(iloop).GetValue(0), jloop) And _
                                    AlertJudgeNo(LatestData(iloop).GetValue(0), jloop) <> 0 Then  '各データファイルごとの最大警報レベルを取得し

                                AlertLevel(jloop) = TempAlertLevel                          '                                           それを格納する
                                AlertDate(jloop) = LatestData(iloop).GetValue(18)           '警報発生時刻
                                PreLevel = TempAlertLevel
                                AlertStatus += 1                                                    '０なら警報なし、１以上なら警報あり
                            Else
                                AlertDate(jloop) = DataFileNames(jloop).NewestDate.ToString("yyyy年 MM月 dd日 HH時 mm分")       '' 2010/10/07 Kino Changed DataFileNames(0) -> DataFileNames(jloop)
                            End If
                        Next iloop

                    Else
                        AlertLevel(jloop) = 0                              '行が存在しなければ警報レベルは０とする
                        ''AlertDate(jloop) = ClsReadDataFile.dteLastUpdate.ToString("yyyy年 MM月 dd日 HH時 mm分")
                        AlertDate(jloop) = DataFileNames(jloop).NewestDate.ToString("yyyy年 MM月 dd日 HH時 mm分")               '' 2010/10/07 Kino Changed DataFileNames(0) -> DataFileNames(jloop)
                    End If
                End If
            Next jloop

            AlertState(0) = "名　称"
            AlertState(1) = "名　称<img src = 'img/Alert.gif' height='13'/>"                           '回転灯 AnimationGIFファイル指定

            ''----------------------------警報発生状況を表示

            With Me.Table_AlarmStatus

                Dim RowCount As Integer = 0
                AlertCheck = 0
                If AlertStatus = 0 Then

                    ''警報が発生していないとき
                    Dim AddRow As New TableRow()
                    '１行追加
                    For iloop = 0 To 2
                        Dim AddCel As New TableCell()
                        Try
                            AddCel.Text = iloop
                            AddCel.RowSpan = 1
                            AddCel.BorderWidth = 1
                            AddCel.ColumnSpan = 1
                            AddRow.Cells.Add(AddCel)
                        Catch ex As Exception
                        Finally
                            AddCel.Dispose()
                        End Try
                    Next iloop
                    .Rows.Add(AddRow)
                    AddRow.HorizontalAlign = HorizontalAlign.Center
                    AddRow.VerticalAlign = VerticalAlign.Middle

                    'ヘッダを除く１行１列目
                    If DataFileNames.Length = 1 Then
                        .Rows(1).Cells(0).Text = DataFileNames(0).ZoneName                                      '名称
                    Else
                        .Rows(1).Cells(0).Text = "全項目"                                                                       '名称
                    End If
                    '１行２列目
                    For iloop = 0 To AlertDate.Length - 1                                                                               '' 2010/10/07 Kino Add 日付けが入っている最初の配列を探す
                        If AlertDate(iloop).Year <> 1 Then                                                                              '' ファイルがまだ存在しない場合に日付けがないことになるのでその対策
                            .Rows(1).Cells(1).Text = Date.Parse(AlertDate(iloop)).ToString("yyyy年 MM月 dd日 HH時 mm分")                    '最新データ日時
                            Exit For
                        Else
                            .Rows(1).Cells(1).Text = "データ未収録"                                                             ' 2012/01/18 Kino Add
                        End If
                    Next iloop
                    '１行３列目
                    .Rows(1).Cells(2).Text = AlertInfo(3).Words                                                                 '警報レベル
                    .Rows(1).Cells(2).Font.Bold = True
                    .Rows(1).Cells(2).BackColor = Color.FromArgb(AlertInfo(3).BackColor)                                        '背景色
                    .Rows(1).Cells(2).ForeColor = Color.FromArgb(AlertInfo(3).ForeColor)                                        '文字色
                    '.Rows(1).Cells(2).BorderColor = Color.Silver
                    AlertCheck = 1                                                                                              '2009/07/22 Kino Add　 表示行数
                    .Rows(1).Cells(0).Attributes.Add("Class", "statusTableCell")
                    .Rows(1).Cells(1).Attributes.Add("Class", "statusTableCell")
                    .Rows(1).Cells(2).Attributes.Add("Class", "statusTableCell")
                    '.Rows(1).Cells(0).BorderColor = Color.Silver
                    '.Rows(1).Cells(1).BorderColor = Color.Silver
                    '.Rows(1).Cells(2).BorderColor = Color.Silver
                Else

                    ''警報が発生している場合
                    For jloop = 0 To DataFileNames.Length - 1
                        If AlertStatus = 0 And jloop > 0 Then Exit For
                        System.Windows.Forms.Application.DoEvents()

                        If AlertLevel(jloop) <> 0 Then
                            System.Windows.Forms.Application.DoEvents()
                            Dim AddRow As New TableRow()
                            '１行追加
                            For iloop = 0 To 2
                                Dim AddCel As New TableCell()
                                AddCel.Text = iloop
                                AddCel.RowSpan = 1
                                AddCel.BorderWidth = 1
                                AddCel.ColumnSpan = 1
                                AddRow.Cells.Add(AddCel)
                                'AddRow.BorderColor = Color.Silver
                            Next iloop

                            RowCount += 1

                            .Rows.Add(AddRow)
                            AddRow.HorizontalAlign = HorizontalAlign.Center
                            AddRow.VerticalAlign = VerticalAlign.Middle

                            '' 警報発生状況表示
                            ''データを記載する
                            'ヘッダを除く１行１列目
                            .Rows(RowCount).Cells(0).Text = DataFileNames(jloop).ZoneName                                       '名称
                            '１行２列目
                            .Rows(RowCount).Cells(1).Text = Date.Parse(AlertDate(jloop)).ToString("yyyy年 MM月 dd日 HH時 mm分")                 '最新データ日時
                            '１行３列目
                            .Rows(RowCount).Cells(2).Text = AlertInfo(AlertLevel(jloop) + 3).Words                                              '警報レベル
                            .Rows(RowCount).Cells(2).Font.Bold = True
                            .Rows(RowCount).Cells(2).BackColor = Color.FromArgb(AlertInfo(AlertLevel(jloop) + 3).BackColor)                     '背景色
                            .Rows(RowCount).Cells(2).ForeColor = Color.FromArgb(AlertInfo(AlertLevel(jloop) + 3).ForeColor)                     '文字色
                            '.Rows(RowCount).Cells(0).BorderColor = Color.Silver                                                                 ' 2012/02/22 Kino Changed  1 -> RowCount
                            '.Rows(RowCount).Cells(1).BorderColor = Color.Silver                                                                 ' 2012/02/22 Kino Changed  1 -> RowCount
                            '.Rows(RowCount).Cells(2).BorderColor = Color.Silver                                                                 ' 2012/02/22 Kino Changed  1 -> RowCount
                            .Rows(RowCount).Cells(0).Attributes.Add("Class", "statusTableCell")
                            .Rows(RowCount).Cells(1).Attributes.Add("Class", "statusTableCell")
                            .Rows(RowCount).Cells(2).Attributes.Add("Class", "statusTableCell")
                            AlertCheck += 1                                                                                                     '2009/07/22 Kino Add 表示行数
                        End If

                    Next jloop
                End If

                If AlertStatus <> 0 Then                                                                                                '警報が発生しているようであれば、
                    ''    .Rows(0).Cells(0).Text = AlertState(1)                                                                              '   左上角に回転灯表示
                    Me.ImgAlert.Visible = True
                End If

            End With

            clsSummary = Nothing
            clsAlert = Nothing

        End If

        Return AlertCheck       'AlertStatus                '2009/07/27 Kino Changed

    End Function


    Protected Sub TmrCheckNewData_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles TmrCheckNewData.Tick
        ''
        '' データの最終更新日を確認する
        ''
        Dim strNewestDate As String = ""
        Dim strOldestDate As String = ""

        ''Call DataCollectCheck()     'strOldestDate, strNewestDate)                ' 2011/05/23 Kino Changed

    End Sub

    Protected Sub DataCollectCheck()        'ByRef strOldestDate As String, ByRef strNewestDate As String)
        ''
        '' データの収録状況の確認と表示
        ''
        Dim DiffDay As Integer
        Dim SiteDirectoryFull As String
        Dim dteTemp() As Date
        Dim iloop As Integer
        Dim FileCount As Integer
        Dim strTemp As String = ""
        ''Dim strTerm As String = ""
        Dim siteName As String = ""
        Dim completeSite As Boolean

        siteName = Session.Item("SN")
        If siteName.Contains("【完了現場】") = True Then completeSite = True
        SiteDirectoryFull = Session.Item("FSD")    '現場ディレクトリ
        ''strTerm = "【データ収録期間】<br>"
        ''dteTemp = ClsReadDataFile .dteLastUpdate

        FileCount = DataFileNames.Length - 1
        ReDim dteTemp(FileCount)

        'データファイルへのパスを取得する
        ''Dim DataFile As String = Server.MapPath(SiteDirectory & "\App_Data\" & SiteDirectory & "_CalculatedData.mdb")
        ''Call GetOldAndNewestDate(SiteDirectoryFull)

        '====================   ' 2012/10/22 Kino Add データ収録期間の表示順を変更するために追加
        Dim sortedDataFileNames(FileCount) As ClsReadDataFile.DataFileInf
        For iloop = 0 To FileCount
            sortedDataFileNames(DataFileNames(iloop).showIndex) = DataFileNames(iloop)
        Next iloop
        '====================

        With Me.tblLastUpdate
            .Rows.Clear()
        End With

        For iloop = 0 To FileCount

            'dteTemp(iloop) = DataFileNames(iloop).NewestDate           ' 2012/10/22 Kino Changed データ収録期間の表示順を変更するための変更
            dteTemp(iloop) = sortedDataFileNames(iloop).NewestDate

            'DiffDay = Int32.Parse(DateDiff(DateInterval.Day, DataFileNames(iloop).NewestDate, Now()))  ' 2012/10/22 Kino Changed データ収録期間の表示順を変更するための変更
            DiffDay = Int32.Parse(DateDiff(DateInterval.Day, sortedDataFileNames(iloop).NewestDate, Now()))
            If DiffDay >= 2 Then                                                                    '最新データと今日の日付が2日以上開いていたらアラートを表示する
                'If DataFileNames(iloop).OverDayCheck = True Then                                    '日付チェックフラグがあれば、表示する 2009/01/28 Kino
                '    strTemp = strTemp + ("【" + DataFileNames(iloop).ZoneName + "】: 最終更新日から " + DiffDay.ToString + "日経過しています ")    '+ "<br>"
                If sortedDataFileNames(iloop).OverDayCheck = True Then                                    '日付チェックフラグがあれば、表示する 2009/01/28 Kino     ' 2012/10/22 Kino Changed データ収録期間の表示順を変更するための変更
                    strTemp = strTemp + ("【" + sortedDataFileNames(iloop).ZoneName + "】: 最終更新日から " + DiffDay.ToString + "日経過しています ")    '+ "<br>"  ' 2012/10/22 Kino Changed データ収録期間の表示順を変更するための変更
                    ''                Me.LblAlert.Text = " 最終更新日から " + DiffDay.ToString + "日経過しています "
                End If

                If strTemp.Length <> 0 Then
                    Me.LblAlert.Visible = True '文字列変数の中身があれば、表示する。 2009/01/28 Kino
                    'Me.LitSpace1.Text = "<br /><img src='./img/space_M.gif'>"
                Else
                    Me.LblAlert.Visible = False
                    'Me.LitSpace1.Text = vbNullString
                End If

            Else
                Me.LblAlert.Visible = False
                'Me.LitSpace1.Text = vbNullString                  '"<br />"   2009/05/29 Kino Changed Comment
            End If

            ''strTerm = strTerm + (DataFileNames(iloop).ZoneName + ":" + DataFileNames(iloop).OldestDate.ToString("yyyy/MM/dd HH:mm") + " ～ " + DataFileNames(iloop).NewestDate.ToString("yyyy/MM/dd HH:mm ") + "<br>")

            'データが更新されていたら・・・
            'If dteTemp(iloop) < DataFileNames(iloop).NewestDate Then
            If dteTemp(iloop) < sortedDataFileNames(iloop).NewestDate Then                  ' 2012/10/22 Kino Changed データ収録期間の表示順を変更するための変更
                ''Dim lblExistDataFile As Label = CType(Page.Master.FindControl("lblExistDataFile"), Label)
                ''lblExistDataFile.Text = "1"
                ''lblExistDataFile = Nothing
                ''ClsReadDataFile.dteLastUpdate = Date.Parse(strNewestDate)
            End If

            If iloop = 0 Then
                Dim HeaderRow As New TableRow()
                Dim HeaderCell As New TableCell()
                HeaderCell.Text = "【データ収録期間】"
                HeaderCell.ColumnSpan = 4
                HeaderRow.Cells.Add(HeaderCell)
                'Me.tblLastUpdate.Rows(0).Cells(0).Text = "【データ収録期間】"
                HeaderRow.TableSection = TableRowSection.TableHeader
                Me.tblLastUpdate.Rows.Add(HeaderRow)
            End If

            With Me.tblLastUpdate

                Dim Datas As New TableRow()
                Dim strCell As String = ""
                For jloop As Integer = 0 To 3
                    Dim dataCell As New TableCell()
                    dataCell.Wrap = False
                    dataCell.ColumnSpan = 1
                    ''dataCell.Attributes.Add("padding-right", "10px")
                    dataCell.Style("padding-right") = "10px"                    '' これでないとFireFoxやChromeで正常に表示出来ない
                    Select Case jloop                                           ' 2012/10/22 Kino Changed データ収録期間の表示順を変更するための変更
                        Case 0
                            strCell = sortedDataFileNames(iloop).ZoneName
                        Case 1
                            strCell = sortedDataFileNames(iloop).OldestDate.ToString("yyyy/MM/dd HH:mm")
                        Case 2
                            strCell = "～"
                        Case 3
                            strCell = sortedDataFileNames(iloop).NewestDate.ToString("yyyy/MM/dd HH:mm ")
                    End Select
                    ''Select Case jloop
                    ''    Case 0
                    ''        strCell = DataFileNames(iloop).ZoneName
                    ''    Case 1
                    ''        strCell = DataFileNames(iloop).OldestDate.ToString("yyyy/MM/dd HH:mm")
                    ''    Case 2
                    ''        strCell = "～"
                    ''    Case 3
                    ''        strCell = DataFileNames(iloop).NewestDate.ToString("yyyy/MM/dd HH:mm ")
                    ''End Select
                    dataCell.Text = strCell
                    dataCell.CssClass = "priodCol1"
                    Datas.Cells.Add(dataCell)
                Next

                ''.Rows(iloop + 0).Cells(0).Text = DataFileNames(iloop).ZoneName
                ''.Rows(iloop + 0).Cells(1).Text = "　" + DataFileNames(iloop).OldestDate.ToString("yyyy/MM/dd HH:mm")
                ''.Rows(iloop + 0).Cells(2).Text = "　～"
                ''.Rows(iloop + 0).Cells(3).Text = "　" + DataFileNames(iloop).NewestDate.ToString("yyyy/MM/dd HH:mm ")
                Datas.TableSection = TableRowSection.TableBody
                .Rows.Add(Datas)
            End With
        Next iloop

        If completeSite = True Then
            strTemp = "本現場の計測は完了しています。"
            Me.LblAlert.BackColor = Color.PaleGreen
            Me.LblAlert.ForeColor = Color.Blue
        End If

        Me.LblAlert.Text = strTemp
        '' ''Me.LblLastUPDate.Text = strTerm                        ' 2011/03/24 Kino Changed  Tableへ移行

    End Sub

    Protected Function GetOldAndNewestDate(ByVal SiteDirectoryFull As String) As Long 'ByVal DataFile As String, Optional ByRef strOldestDate As String = "", Optional ByRef strNewestDate As String = "")
        ''
        '' データベースの最新と最古のデータを取得してセッションに保存する
        ''

        Dim intRet As Integer
        Dim iloop As Integer
        Dim DataFile As String
        Dim SiteDirectory As String
        Dim clsNewDate As New ClsReadDataFile
        Dim reTryCount As Integer                                                                               ' 2011/05/23 Kino Add
        Dim lngRet As Long = 0

        SiteDirectory = Session.Item("SD")                                                                      '現場ディレクトリ
        DataFile = Server.MapPath(SiteDirectory + "\App_Data\MenuInfo.mdb")
        intRet = clsNewDate.GetDataFileNames(DataFile, DataFileNames)                                           'データファイル名、共通情報ファイル名、識別名を取得

        Do                                                                                                      ' 2011/05/23 Kino Add リトライ処理
            intRet = clsNewDate.GetDataLastUpdate(Server.MapPath(SiteDirectory + "\App_Data\"), DataFileNames)      'データファイルにおける最新・最古データ日時を取得

            If intRet = -1 Then                                                                                 ' 2011/05/23 Kino Add 読み込みリトライ
                reTryCount += 1
                System.Threading.Thread.Sleep(2000)
                If reTryCount >= 15 Then
                    lngRet = -1
                    Exit Do
                End If
            Else
                Exit Do
            End If
        Loop

        For iloop = 0 To intRet

            Session.Item("LastUpdate" + iloop.ToString) = DataFileNames(iloop).NewestDate.ToString("yyyy/MM/dd HH:mm")
            Session.Item("OldestDate" + iloop.ToString) = DataFileNames(iloop).OldestDate.ToString("yyyy/MM/dd HH:mm")

            ''Session.Item("LastUpdate" & iloop.ToString) = DataFileNames(iloop).NewestDate.ToString("yyyy/MM/dd HH:mm")
            ''Session.Item("OldestDate" & iloop.ToString) = DataFileNames(iloop).OldestDate.ToString("yyyy/MM/dd HH:mm")

            'strOldestDate = OldestDate.ToString("yyyy/MM/dd HH:mm")
            'strNewestDate = NewestDate.ToString("yyyy/MM/dd HH:mm")
        Next iloop

        clsNewDate = Nothing
        Return lngRet                                                                                           ' 2011/05/23 Kino Add 戻り値設定

    End Function

End Class
