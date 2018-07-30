﻿Imports System.Data
Imports System.Data.OleDb
Imports System.Drawing

''' <summary>
''' 簗川ダム専用トップ画面
''' </summary>
''' <remarks></remarks>
Partial Class Main02
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

        ''Dim sw As New IO.StreamWriter(Server.MapPath("~/log/errorSummary_" & date.now.tostring("yyMM") & ".log"), True, Encoding.GetEncoding("Shift_JIS"))

        Dim ex As Exception = Server.GetLastError().GetBaseException()

        Call WriteErrorLog(MyBase.GetType.BaseType.FullName, ex.Message.ToString, ex.StackTrace.ToString)

    End Sub

    Protected Sub WriteErrorLog(ByVal ProcName As String, Optional ByVal exMes As String = "None", Optional ByVal exStackTrace As String = "None")

        Dim sw As IO.StreamWriter = Nothing

        Try
            sw = New IO.StreamWriter(Server.MapPath("~/log/errorSummary_" & Date.Now.ToString("yyMM") & ".log"), True, Encoding.GetEncoding("Shift_JIS"))
            Dim sb As New StringBuilder
            sb.Append(DateTime.Now.ToString)
            sb.Append(ControlChars.Tab)
            sb.Append(ProcName)
            sb.Append(ControlChars.Tab)
            sb.Append(CType(Session.Item("UN"), String))
            sb.Append(ControlChars.Tab)
            Dim strTemp As String = Nothing
            If strTemp IsNot Nothing Then
                sb.Append(Session.Item("SD").ToString)
            Else
                sb.Append("---")
            End If
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

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Page.Request("__EVENTTARGET") IsNot Nothing AndAlso Page.Request("__EVENTTARGET").ToString = "ctl00$LoginStatus1$ctl00" Then                   ' 2011/05/25 Kino Add ログアウトが押された時は処理しない 
            ' デバッグ用のためコメント
            ''Dim sw As New IO.StreamWriter(Server.MapPath("~/log/errorSummary_" & date.now.tostring("yyMM") & ".log"), True, Encoding.GetEncoding("Shift_JIS"))
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
            Call WriteErrorLog(MyBase.GetType.BaseType.FullName & " User.Identity not Found")
            Response.Redirect("Login.aspx", False)
            Exit Sub
        End If

        Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)
        'ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            System.Threading.Thread.Sleep(5000)                                                     ' 2010/06/01 Kino Add 再取得確認
            LoginStatus = CType(Session.Item("LgSt"), Integer)
            If LoginStatus = 0 Then
                Call WriteErrorLog(MyBase.GetType.BaseType.FullName & " LoginStatus not Found")

                Response.Redirect("Login.aspx", False)
                Exit Sub
            End If
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする

        ''Dim lblExistDataFile As Label = CType(Page.Master.FindControl("lblExistDataFile"), Label)         '2015/11/25 Kino Changed 使用していないためコメント
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
        Dim strParm() As String                                                                     ' 2011/07/29 Kino Add

        SiteName = CType(Session.Item("SN"), String)
        SiteDirectory = CType(Session.Item("SD"), String)      '現場ディレクトリ

        ''Dim imagePath As String = Server.MapPath(SiteDirectory & "\img\")                         ’ Httpハンドラで使用していたが、不要となったためコメント 2018/05/30 Kino
        ''Session.Add("imgPath", imagePath)
        Response.BufferOutput = True

        Dim ctrlname As String = Page.Request.Params.Get("__EVENTTARGET")
        Dim OpenString As String = Nothing
        '' 初回の読み込み時だけ、スライドインを有効にする(タイマー稼働時はスクリプトを吐き出さない)  2018/05/30 Kino Changed クライアントサイドで実施する
        ''If ctrlname <> "ctl00$CntplHolder$TmrCheckNewData" AndAlso Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
        ''    'OpenString = "<script type='text/javascript'>"
        ''    'OpenString += "var ft = 300;"
        ''    ''Header
        ''    'OpenString += "$(function () {"
        ''    'OpenString += "$('#FrmMasterPage:not(body#FrmMasterPage)').css({ display: 'block', marginTop: -60, opacity: '0' });"
        ''    'OpenString += "$('#FrmMasterPage:not(body#FrmMasterPage)').animate({ marginTop: '0px', opacity: '1' }, ft);"
        ''    'OpenString += "$('#FrmMasterPage').css({ display: 'block', opacity: '0' });"
        ''    'OpenString += "$('#FrmMasterPage').animate({ opacity: '1' }, ft);"
        ''    ''Menu    alert(-$('#ctl00_ColL').width()).show
        ''    'OpenString += "$('#ctl00_ColL:not(body#ctl00_ColL)').css({ display: 'block', marginLeft: -$('#ctl00_ColL').width(), opacity: '0' });"
        ''    'OpenString += "$('#ctl00_ColL:not(body#ctl00_ColL)').animate({ marginLeft: '0px', opacity: '1' }, ft);"
        ''    'OpenString += "$('#ctl00_ColL').css({ display: 'block', opacity: '0' });"
        ''    'OpenString += "$('#ctl00_ColL').animate({ opacity: '1' }, ft);"
        ''    'OpenString += "$('#ctl00_MyAccordion').css({ marginLeft: '0px'});"
        ''    'OpenString += "});"
        ''    'OpenString += "<" + "/SCRIPT>"
        ''    'Page.ClientScript.RegisterStartupScript(Me.GetType(), "Main02", OpenString)
        ''    '' ctl00$CntplHolder$TmrCheckNewData
        ''Else
        ''    OpenString = "<script type='text/javascript'>"                                      ' 2015/12/28 Kino Add  非ポストバックのときは、マージンを0pxとしておくことでメニューを表示する
        ''    ''OpenString += "$('#ctl00_MyAccordion').css({ marginLeft: '0px'});"
        ''    OpenString &= "$('#ctl00_MyAccordion').css({ marginLeft: -$('#ctl00_ColL').width()});"
        ''    OpenString &= "<" & "/SCRIPT>"
        ''End If
        ''Page.ClientScript.RegisterStartupScript(Me.GetType(), "Main02", OpenString)

        If IsPostBack = False Then
            ''Call CheckAutoUpdate(SiteDirectory)                                                 '自動更新設定の読み込み  2018/05/30 Kino Changed 非同期更新確認に移行
            Me.mnust.Text = "1"                                                                 ' 2018/04/16 Kino Add
        End If

        '2018/05/30 Kino Changed 非同期更新確認に移行
        ''Dim ErrorRetry As Integer = CType(Session.Item("Redraw"), Integer)                      ' 2011/05/25 Kino Add 読み込みエラーが発生していた場合は１がたつ
        ''If ErrorRetry = 1 Then
        ''    Call CheckAutoUpdate(SiteDirectory)                                                 '自動更新設定の再読み込み
        ''End If

        DataFile = Server.MapPath(SiteDirectory & "\App_Data\MenuInfo.mdb")

        '============= トップ画面にWebカメラの画像を表示する　チルト／パン 対応　2015/12/15 Kino Changed
        Dim iframeSrc As String = Nothing                                                       ' 2015/12/15 Kino Add
        imageFile = getMainpageInf(systemName, DataFile, iframeSrc)                             ' メインページの画像ファイルとシステム名称を取得    2015/12/15 Kino Changed  Add iframeSrc

        Page.Header.Title = SiteName + systemName.ToString                                      'タイトルバーのタイトル設定 2011/07/29 Kino Changed ファイルから読み込みする

        Dim clsCont As New ClsContour(IO.Path.Combine(Server.MapPath(SiteDirectory), "App_Data", "ConstructionInfo.mdb"))
        Dim param() As String = Nothing
        strParm = clsCont.getConHeightInf("メイン")

        If strParm(20) = -1 OrElse strParm(21) = -1 Then
            ' 画像の幅と高さの取得その２
            Dim imagew, imageh As Integer
            Dim fs As System.IO.FileStream
            ' Specify a valid picture file path on your computer.
            fs = New System.IO.FileStream(IO.Path.Combine(Server.MapPath(SiteDirectory), "img", imageFile), IO.FileMode.Open, IO.FileAccess.Read)
            imagew = System.Drawing.Image.FromStream(fs).Width
            imageh = System.Drawing.Image.FromStream(fs).Height
            fs.Close()

            If strParm(20) = "-1" Then strParm(20) = imagew.ToString
            If strParm(21) = "-1" Then strParm(21) = imageh.ToString
        End If

        ' -------------------------------- 打設高さ表示(SVG)
        Dim sb As New StringBuilder
        sb.Append(String.Format("<svg id='svgPic' xmlns='http://www.w3.org/2000/svg' width='{0}' height='{1}' version='1.1'>", strParm(22), strParm(23)))
        ''sb.Append(String.Format("<image xlink:href='{0}'", "./" & SiteDirectory & "/img/" & imageFile.ToString))
        sb.Append("<image xlink:href='./api/thumbnail.ashx'")
        sb.Append(String.Format(" x='{0}' y='{1}'", strParm(18), strParm(19)))
        sb.Append(String.Format(" height='{0}' width='{1}' class='{2}'/>", strParm(21), strParm(20), strParm(9)))
        sb.Append(String.Format("<line id='conline' x1='0' y1='0' x2='0' y2='0' class='{0}'/>", strParm(10)))
        sb.Append(String.Format("<rect id='outl' x='0' y='0' width='0' height='0' rx='{0}' ry='{1}' class='{2}'/>", strParm(14), strParm(14), strParm(12)))
        sb.Append(String.Format("<text id='word' x='0' y='0' class='{0}' dominant-baseline='central'>{1}</text></svg>", strParm(11), strParm(6)))
        Me.litoutimg.Text = sb.ToString                                                                  ' SVGの定義を記載
        sb.Length = 0

        Page.Header.Title = (SiteName & systemName.ToString)         '' "－ インターネットデータ閲覧サービス"                    'タイトルバーのタイトル設定 2011/07/29 Kino Changed ファイルから読み込みする

        Dim strRet As String = GetOldDataTerm(DataFile)
        Session.Item("OldTerm") = strRet
        ''intRet = ClsReadDataFile.GetDataFileNames(DataFile, DataFileNames)       'データファイル名、共通情報ファイル名、識別名を取得
        lngRet = GetOldAndNewestDate(Server.MapPath(SiteDirectory + "\App_Data\"))

        If lngRet = -1 Then                                                         ' 2011/05/23 Kino Add データが読めなければセッションエラーとする

            Call WriteErrorLog(MyBase.GetType.BaseType.FullName, "最新・古日付取得読込エラー(１分後再読み込み)")

            ''Response.Redirect("sessionerror.aspx")
            Session.Add("Redraw", 1)

            Dim LbArt As Label = CType(Master.FindControl("LblAlert"), Label)           '2017/02/24 Kino Add 場所を変えてマスターページ内の空きスペースに入れた
            LbArt.Text = "データ読込ができなかったため1分後に更新を行います"
            LbArt.BackColor = Color.PaleGreen
            LbArt.ForeColor = Color.Blue
            LbArt.Visible = True
            Me.TmrCheckNewData.Interval = 60000
            Me.TmrCheckNewData.Enabled = True

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
            ''lblExistDataFile.Text = "0"                                                                               '2015/11/25 Kino Changed 使用していないためコメント

            If System.IO.File.Exists(Server.MapPath(SiteDirectory + "\App_Data\" + "MainDataSummary.show")) = True Then                'データ表示フラグファイルがあった場合、データを読み込む
                Dim txtFile As New IO.StreamReader(Server.MapPath(SiteDirectory + "\App_Data\" + "MainDataSummary.show"), System.Text.Encoding.Default) '.GetEncoding("shift_jis"))
                DataSummaryName = txtFile.ReadLine
                txtFile.Close()
                Dim strFileNo As String = Nothing
                lngRet = ReadDataSummary(DataSummaryName, SiteDirectory, AlertCount, strFileNo, Convert.ToInt32(strParm(24)))                                   'メイン画面へのデータ一覧表示     ' 2018/03/19 Kino Changed 更新チェック機能追加に伴う変更
                If lngRet = -1 Then
                    Call NoDatasOperation() '' 2015/11/25 Kino Changed 使用していないためコメント lblExistDataFile)
                    Me.nwdt.Value = "NC"                                         ' 更新は不要だが、セッション維持のため
                Else
                    Dim fileNoSPLT() As String = strFileNo.Split(Convert.ToChar(","))
                    Dim fileLoop As Integer
                    Dim fileDate As String = Nothing
                    For fileLoop = 0 To (fileNoSPLT.Length - 1)
                        fileDate &= String.Format("{0},", DataFileNames(Convert.ToInt32(fileNoSPLT(fileLoop))).NewestDate.ToString)
                    Next
                    fileDate = fileDate.TrimEnd(Convert.ToChar(","))
                    Me.nwdt.Value = fileDate
                    Me.nwdtno.Value = strFileNo
                End If
            Else
                Me.nwdtno.Value = "0"
                Me.nwdt.Value = "NC"
            End If
            If System.IO.File.Exists(Server.MapPath(SiteDirectory + "\App_Data\" + "MainDataSummaryCoord.show")) = True Then
                OpenString = "<script type='text/javascript' src='js/mousepoint.js'></script>"
                Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "メイン", OpenString)
                Me.PnlCoordinate.Visible = True
            End If
        Else
            Call NoDatasOperation()                                                                                     ' '2015/11/25 Kino Changed 使用していないため引数を削除 lblExistDataFile)

        End If

        ''コンクリート打設高さ関連の情報をページに隠す
        Me.conParam.Value = (String.Format("{0},{1},{2},", strParm(7), strParm(8), strParm(16)) & String.Format("{0},{1},{2}", strParm(15), strParm(17), strParm(13)))
        clsCont.setDBPath = Server.MapPath(SiteDirectory) & "\App_Data\" & DataFileNames(0).CommonInf.ToString
        Me.conheight.Value = clsCont.ReadDataSummary4Yanagawa(Convert.ToInt32(strParm(24)), 19)
        ''Me.conheight.Value = "285.438" '   "292.941"    '"260.706"   '"232.02"       

    End Sub

    Protected Function getMainpageInf(ByRef systemName As String, ByVal dataFile As String, ByRef iframeSrc As String) As String        ' 2015/12/15 Kino Changed   Add iframeSrc

        Dim DbCon As New OleDb.OleDbConnection
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strRet As String = "None"
        ''Dim Dt As DataTable
        ''Dim tableNames As String = ""

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + dataFile + ";Jet OLEDB:Engine Type= 5;Jet OLEDB:Database Locking Mode=1")

        ' テーブル一覧を取得して存在していなかったら読まないようにするつもりだったが、すべてのDBへテーブルをエクスポートしたので未使用とする  2011/07/29 Kino Add
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
            If DtSet.Tables("DData").Columns.Count = 4 Then                                                                         '2015/12/15 Kino Add
                iframeSrc = DtSet.Tables("DData").Rows(0).Item(3)
            End If
        End If

        DbCon.Dispose()

        Return strRet

    End Function

    Protected Sub NoDatasOperation()
        '' Protected Sub NoDatasOperation(ByVal lblExistDataFile As Label)             '2015/11/25 Kino Changed 使用していないため引数をなしに

        Dim HeaderRow As New TableRow()
        Dim HeaderCell As New TableCell()

        Me.tblLastUpdate.Rows.Clear()
        HeaderCell.Text = " データ未収録"
        HeaderCell.ColumnSpan = 4
        HeaderRow.Cells.Add(HeaderCell)
        HeaderRow.TableSection = TableRowSection.TableHeader
        Me.tblLastUpdate.Rows.Add(HeaderRow)

        '' ''Me.LblLastUPDate.Text = " データ未収録"         ' 2011/03/24 Kino Changed  Tableへ移行
        '' lblExistDataFile.Text = "99"                                                 ' 2015/11/25 Kino Changed 使用していないためコメント

    End Sub

    Protected Function GetOldDataTerm(ByVal dataFile As String) As String

        Dim DbCon As New OleDb.OleDbConnection
        ' Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strRet As String = "None"

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & dataFile & ";Jet OLEDB:Engine Type= 5")
        ''DbCon.Open()

        '' 過去データ表示制限の読み込み
        DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM 過去データ制限 ORDER BY ID ASC", DbCon)
        DbDa.Fill(DtSet, "DData")

        ''DbCon.Close()

        If DtSet.Tables("DData").Rows(0).Item(1) = True Then
            strRet = DtSet.Tables("DData").Rows(0).Item(2).to
        End If
        DbCon.Dispose()

        Return strRet

    End Function

    ''' <summary>
    ''' データ一覧の設定を読み込み、データ一覧を表示する(最新データのみ)
    ''' </summary>
    ''' <param name="strGraphName"></param>
    ''' <param name="SiteDirectory"></param>
    ''' <param name="AlertCount"></param>
    ''' <param name="ConHeightCh"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function ReadDataSummary(ByVal strGraphName As String, ByVal SiteDirectory As String, ByVal AlertCount As Integer, ByRef strFileNo As String, Optional ByVal ConHeightCh As Integer = -1) As Long

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
        ''If LblAlert.Visible = True Then
        ''    PlusOffset = 25.5       '27.0           '35   '23.0   2009/07/01 Kino Changed       2012/02/22 Kino Changed　27.0->25.5
        ''Else
        ''    PlusOffset = -1.5           '3.0
        ''End If

        retryCount = 0
        Do                                                                                                                                  '' 2010/11/18 Kino Add　データ読み込みリトライ
            lngRet = clsSummary.ReadSummaryInfo(strGraphName, DataFilePath, ShowDatas, ShowDataInf, lblOffset, DataFileCount, PicFile, fieldCount)   '' 最新データを読み込む(DataSummaryGraphInfo.mdb) 2012/02/10 Kino Changed  Add FieldCount
            ' lngRet = clsSummary.ReadCommonInfData(DataFilePath, 1, DataFileCount, ShowDatas, DataFileNames)                                 '' 表示するデータを読み込む(***_CommonInf.mdb)
            If lngRet = 0 Then
                Dim fileCount As Integer                                                                                                    ' 2018/03/ Kino Add
                For fileCount = 0 To (ShowDatas.Length - 1)
                    strFileNo &= String.Format("{0},", ShowDatas(fileCount).DataFileNo)
                Next fileCount
                strFileNo = strFileNo.TrimEnd(Convert.ToChar(","))
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

        If lngRet = 0 Then                                                                                                       '' 2010/11/18 Kino Add データが読めなければ書かない・・・
            Dim ColumR As Panel = CType(Master.FindControl("pnlColR"), Panel)
            'Call clsSummary.DynamicMakeLabels(Me.Form, ShowDatas, ShowDataInf, lblOffset, DataFileCount, AlertCount, AlertInfo, _          ' 2018/06/04 Kino Changed ラベルを張り付ける親パーツを指定
            Call clsSummary.DynamicMakeLabels(ColumR, ShowDatas, ShowDataInf, lblOffset, DataFileCount, AlertCount, AlertInfo, _
                                                AlertJudgeNo, DataFileNames, DataFilePath, fieldCount, PlusOffset)     'データラベルを作成 2012/02/10 Kino Changed  Add fieldCount
        Else

        End If

        ''If PicFile.Length <> 0 Then                                                                                                 '画像ファイルが設定されていたら、
        ''    Me.Image2.ImageUrl = "thumbnail.ashx?img=" + PicFile
        ''    ''Me.MainPic.Src = ("~/" + SiteDirectory + "/img/" + PicFile)                                                               '       それを設定する
        ''End If

        If ConHeightCh > 0 Then                                                                                                           ' 簗川用
            Dim RefNo As Integer = clsSummary.GetDBChRecordNo(0, ShowDatas, ConHeightCh)
            'Dim ConHeight As Single = ShowDatas(0).DataSet.
            Dim ConHeight As Single = Convert.ToSingle(ShowDatas(0).DataSet(RefNo).GetValue(19))
            Me.conheight.Value = ConHeight.ToString
        End If

        clsSummary = Nothing
        Return lngRet

    End Function


    ''' <summary>
    ''' CommonInf.mdbから、指定チャンネルのデータのみを取得する
    ''' </summary>
    ''' <param name="SiteDirectory"></param>
    ''' <param name="ConHeightCh"></param>
    ''' <returns>取得したデータを文字列として戻す</returns>
    ''' <remarks></remarks>
    Protected Function ReadDataSummary4Yanagawa(ByVal SiteDirectory As String, ByVal ConHeightCh As Integer, FieldNo As Integer) As String

        Dim DataFilePath As String
        Dim DBFilePath As String
        Dim DbCon As New OleDb.OleDbConnection
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As DataSet
        Dim strSQL As String
        Dim strRet As String = Nothing

        DataFilePath = Server.MapPath(SiteDirectory)
        DBFilePath = (DataFilePath & "\App_Data\" & DataFileNames(0).CommonInf.ToString)

        DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" & DBFilePath & ";Jet OLEDB:Engine Type= 5")
        strSQL = String.Format("SELECT * FROM 共通情報 WHERE DataBaseCh IN({0}) ORDER BY DataBaseCh", ConHeightCh)

        DtSet = New DataSet("DrawData")
        DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
        DbDa.Fill(DtSet, "DrawData")

        If DtSet.Tables("DrawData").Rows.Count > 0 Then
            strRet = DtSet.Tables("DrawData").Rows(0).Item(FieldNo).ToString
        End If

        Return strRet

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

    ''' <summary>
    ''' 色番号(Integer)を3原色の要素に分解
    ''' </summary>
    ''' <param name="baseColor">色番号</param>
    ''' <param name="R">赤色要素データ</param>
    ''' <param name="G">緑色要素データ</param>
    ''' <param name="B">青色要素データ</param>
    ''' <remarks></remarks>
    Private Sub getColSeparate(ByVal baseColor As Integer, ByRef R As Integer, ByRef G As Integer, ByRef B As Integer)

        R = Color.FromArgb(baseColor).R
        G = Color.FromArgb(baseColor).G
        B = Color.FromArgb(baseColor).B

    End Sub

    Protected Function CheckAlertStatus(ByVal SiteDirectory As String) As Integer
        ''
        '' 最新データにおける警報状況を確認して表示する
        ''
        'Dim DbCon As New OleDb.OleDbConnection
        ''Dim DbDr As OleDb.OleDbDataReader
        ''Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String
        Dim DataFile As String
        Dim AlertShowEnable As Boolean
        Dim clsSummary As New ClsReadDataFile
        Dim clsAlert As New ClsReadDataFile
        Dim iloop As Integer
        Dim jloop As Integer
        Dim dataFolderPath As String = Server.MapPath(SiteDirectory + "\App_Data\")                                         '' 2010/10/07 KinoAdd
        Dim AlertCheck As Integer = 0               '2009/07/27 Kino Add
        Dim alertCol() As Integer = {16, 15, 14, 0, 11, 12, 13}                                                             ' 2015/11/25 Kino Add
        Dim cssText() As String = Nothing                                                                                   ' 2018/03/12 Kino Add

        Using DbCon As New OleDbConnection

            ''警報表示の有効無効を取得----
            DataFile = IO.Path.Combine(dataFolderPath, "MenuInfo.mdb")                                                          ' 2017/02/24 Kino Changed  Combine Method

            DbCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + DataFile + ";Jet OLEDB:Engine Type= 5"
            ''DbCon.Open()

            strSQL = "SELECT 警報表示有効 FROM 警報設定"
            'DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
            Using DbDa As New OleDb.OleDbDataAdapter(strSQL, DbCon)
                Using DtSet As New DataSet("DData")
                    DbDa.Fill(DtSet, "DData")

                    If DtSet.Tables("DData").Rows.Count > 0 Then
                        AlertShowEnable = DtSet.Tables("DData").Rows(0).Item(0)
                    End If
                End Using
            End Using
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
            Using DbDa As New OleDb.OleDbDataAdapter("SELECT * FROM 警報判定フラグ ORDER BY ChNo ASC", DbCon)                 '0より大きい次数のみを取得する       'WHERE 警報比較次数>0
                Using DtSet As New DataSet("DData")
                    'DtSet = New DataSet
                    ' ''Dim DbDa As OleDb.OleDbDataAdapter
                    ' ''Dim DtSet As New DataSet("DData")

                    '' 過去データ表示制限の読み込み
                    'DbDa = New OleDb.OleDbDataAdapter("SELECT * FROM 警報判定フラグ ORDER BY ChNo ASC", DbCon)                 '0より大きい次数のみを取得する       'WHERE 警報比較次数>0
                    DbDa.Fill(DtSet, "DData")

                    ''DbCon.Close()
                    '            DbCon.Dispose()

                    Dim DsetCount As Integer = DtSet.Tables("DData").Rows.Count - 1
                    Dim FieldCount As Integer = DtSet.Tables("DData").Rows(iloop).Table.Columns.Count - 2
                    ReDim AlertJudgeNo(DtSet.Tables("DData").Rows(DsetCount).Item(0), FieldCount)
                    For jloop = 0 To FieldCount
                        For iloop = 0 To DsetCount
                            'AlertJudgeNo(iloop, 0) = DtSet.Tables("DData").Rows(iloop).Item(0)
                            AlertJudgeNo(DtSet.Tables("DData").Rows(iloop).Item(0), jloop) = DtSet.Tables("DData").Rows(iloop).Item(1)                         'チャンネル番号を配列番号として値を格納
                        Next
                    Next jloop

                End Using
            End Using
        End Using
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
            Dim ErrorRetry As Integer = CType(Session.Item("Redraw"), Integer)                      ' 2011/05/25 Kino Add 読み込みエラーが発生していた場合は１がたつ
            Dim retryCount As Integer                                                                ' 2011/05/27 Kino Add
            Dim alertSensorTips(DataFileNames.Length - 1) As String                                 ' 2015/11/25 Kino Add
            Dim clsData As New ClsReadDataFile                                                      ' 2015/11/25 Kino Add

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
                        'alertSensorTips(jloop) = "<dl class='alerttable'><dt>記号</dt><dd class='col1'>測定値</dd><dd class='col2'>レベル</dd><dd class='col3'>管理値</dd><dd class='cl'></dd><hr class='hrpad'>"    '2015/11/25 Kino Add 開始タグ
                        Dim strTable As String = ""
                        ''strTable = "<table class='alertOutTable'><tbody><tr><td>"
                        ''alertSensorTips(jloop) = (strTable & "<dl class='alerttable'><dt class='dth'>記号</dt><dd class='col1h'>測定値</dd><dd class='col2h'>レベル</dd><dd class='col3h'>管理値</dd><dd class='cl'></dd>")     '<hr class='hrpad'>"    '2015/11/25 Kino Add 開始タグ

                        alertSensorTips(jloop) = "<div class='alertOutTable'><dl class='alerttable'><dt class='dth'>記号</dt><dd class='col1h'>測定値</dd><dd class='col2h'>レベル</dd><dd class='col3h'>管理値</dd><dd class='cl'></dd>"     '<hr class='hrpad'>"    '2015/11/25 Kino Add 開始タグ

                        Dim TempCountS As Integer
                        If TempCount > 90 Then TempCountS = 45 Else TempCountS = TempCount
                        For iloop = 0 To TempCountS
                            TempAlertLevel = LatestData(iloop).GetValue(17)                 '警報レベル

                            Try                                                                                     ' 2018/03/02 Kino Add　例外処理　警報判定フラグのレコード数が足りない場合の処理
                                'If Math.Abs(AlertLevel(jloop)) < Math.Abs(TempAlertLevel) And _
                                '        Math.Abs(TempAlertLevel) >= AlertJudgeNo(LatestData(iloop).GetValue(0), jloop) And _
                                '        AlertJudgeNo(LatestData(iloop).GetValue(0), jloop) <> 0 Then  '各データファイルごとの最大警報レベルを取得し
                                If AlertJudgeNo.GetLength(0) >= Convert.ToInt32(LatestData(iloop).GetValue(0)) AndAlso Math.Abs(TempAlertLevel) >= AlertJudgeNo(Convert.ToInt32(LatestData(iloop).GetValue(0)), jloop) AndAlso _
                                    AlertJudgeNo(Convert.ToInt32(LatestData(iloop).GetValue(0)), jloop) <> 0 Then  '各データファイルごとの最大警報レベルを取得し

                                    AlertLevel(jloop) = TempAlertLevel                          '                                           それを格納する
                                    AlertDate(jloop) = Convert.ToDateTime(LatestData(iloop).GetValue(18))           '警報発生時刻
                                    PreLevel = TempAlertLevel
                                    AlertStatus += 1                                                                '０なら警報なし、１以上なら警報あり
                                Else
                                    AlertLevel(jloop) = TempAlertLevel                          '                                           それを格納する
                                    AlertDate(jloop) = Convert.ToDateTime(LatestData(iloop).GetValue(18))           '警報発生時刻
                                    PreLevel = TempAlertLevel
                                    AlertStatus += 1                                                                '０なら警報なし、１以上なら警報あり

                                End If
                            Catch ex As Exception

                            End Try

                            Dim val(3) As String                                                                                ' 2015/11/25 Kino Add　管理値超過センサー一覧ポップアップ
                            Dim sngFormat As String
                            val(2) = AlertInfo(TempAlertLevel + 3).Words
                            val(3) = LatestData(iloop).GetValue(alertCol(TempAlertLevel + 3))
                            val(0) = LatestData(iloop).GetValue(3).ToString.TrimEnd

                            Dim inttemp As Integer
                            If val(3).IndexOf("."c) = -1 Then
                                inttemp = 1
                            Else
                                inttemp = (val(3).Length - val(3).IndexOf("."c) - 1)
                            End If

                            sngFormat = "0."
                            sngFormat = sngFormat.PadRight((inttemp + 2I), "0"c)
                            val(1) = clsData.trunc_round(Convert.ToSingle(LatestData(iloop).GetValue(19)), sngFormat, True)     ' 管理値と同じフォーマットにして、切り捨て
                            alertSensorTips(jloop) &= String.Format("<dt>{0}</dt><dd class='col1'>{1}</dd><dd class='col2'>{2}</dd><dd class='col3'>[{3}]</dd>", val)

                            ' 2列に分かれる場合には、一度tdを閉じて、2列目にヘッダーを入れる
                            If iloop = 45 Then
                                '' alertSensorTips(jloop) &= "</td><td><dl class='alerttable'><dt class='dth'>記号</dt><dd class='col1h'>測定値</dd><dd class='col2h'>レベル</dd><dd class='col3h'>管理値</dd><dd class='cl'></dd>"
                                alertSensorTips(jloop) &= "</div><div class='alertOutTable'><dl class='alerttable'><dt class='dth'>記号</dt><dd class='col1h'>測定値</dd><dd class='col2h'>レベル</dd><dd class='col3h'>管理値</dd><dd class='cl'></dd>"
                            End If
                        Next iloop

                        ''If TempCount > 45 Then
                        ''    'alertSensorTips(jloop) &= "<dt>表示領域 超過</dt><dd class='col1'>：</dd><dd class='col2'>：</dd><dd class='col3'>：</dd>"
                        ''    '' alertSensorTips(jloop) &= "</td></tr></tbody></table>"
                        alertSensorTips(jloop) &= "</div>"
                        ''End If

                    Else
                        AlertLevel(jloop) = 0                              '行が存在しなければ警報レベルは０とする
                        ''AlertDate(jloop) = ClsReadDataFile.dteLastUpdate.ToString("yyyy年 MM月 dd日 HH時 mm分")
                        AlertDate(jloop) = DataFileNames(jloop).NewestDate.ToString("yyyy年 MM月 dd日 HH時 mm分")               '' 2010/10/07 Kino Changed DataFileNames(0) -> DataFileNames(jloop)
                    End If
                End If

                alertSensorTips(jloop) &= "</dl>"                                                   '2015/11/25 Kino Add 終了タグ
            Next jloop

            AlertState(0) = "名　称"
            AlertState(1) = "名　称<img src = 'img/Alert.gif' height='13'/>"                           '回転灯 AnimationGIFファイル指定

            ''----------------------------動的CSSを生成                                             ' 2018/03/12 Kino Add
            Dim cssPath As String = IO.Path.Combine(Server.MapPath("css"), "stl_blinkbase.css")
            Dim alt0FlgPath As String = IO.Path.Combine(Server.MapPath(SiteDirectory), "App_Data\alert0.flg")
            Dim sb As New StringBuilder
            Dim cssTemp As String = Nothing
            Dim R As Integer
            Dim G As Integer
            Dim B As Integer
            Dim AlertLoop As Integer

            Call getColSeparate(AlertInfo(3).BackColor, R, G, B)
            Dim NormColor As String = String.Format("rgb({0},{1},{2})", R, G, B)

            cssText = clsAlert.ReadTextFile(cssPath)

            If cssText IsNot Nothing Then
                sb.AppendLine("<style type='text/css'>")
                sb.AppendLine("<!--")

                For AlertLoop = 3 To 6                                                                  ' 3:正常->No0　4:上限1次->No1　5:上限2次->No2　6:上限3次->No3
                    If (AlertLoop = 3 AndAlso IO.File.Exists(alt0FlgPath) = True) OrElse AlertLoop >= 4 Then
                        Call getColSeparate(AlertInfo(AlertLoop).BackColor, R, G, B)
                        Dim htmlColor As String = String.Format("rgb({0},{1},{2})", R, G, B)
                        cssTemp = String.Join(Environment.NewLine, cssText).Replace("@@ALT", htmlColor)
                        cssTemp = cssTemp.Replace("@@NOR", NormColor)
                        cssTemp = cssTemp.Replace("@@NO", (AlertLoop - 3).ToString)
                        sb.AppendLine(cssTemp)
                    End If
                Next
                sb.AppendLine("-->")
                sb.AppendLine("</style>")

                cssTemp = sb.ToString
                sb.Length = 0

                Me.dcss.Text = cssTemp
            End If

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
                        AddCel.Text = iloop
                        AddCel.RowSpan = 1
                        AddCel.BorderWidth = 1
                        AddCel.ColumnSpan = 1
                        AddCel.ID = "tAlm" & (jloop - 1).ToString & iloop.ToString                  ' 2015/11/19 Kino Add
                        AddRow.Cells.Add(AddCel)
                    Next iloop
                    .Rows.Add(AddRow)
                    AddRow.HorizontalAlign = HorizontalAlign.Center
                    AddRow.VerticalAlign = VerticalAlign.Middle
                    'AddRow.ToolTip = "警報なし"

                    'ヘッダを除く１行１列目
                    If DataFileNames.Length = 1 Then
                        .Rows(1).Cells(0).Text = DataFileNames(0).ZoneName                                      '名称
                    Else
                        .Rows(1).Cells(0).Text = "全項目"                                                                       '名称
                    End If
                    '１行２列目
                    For iloop = 0 To AlertDate.Length - 1                                                                               '' 2010/10/07 Kino Add 日付けが入っている最初の配列を探す
                        If AlertDate(iloop).Year <> 1 Then                                                                              '' ファイルがまだ存在しない場合に日付けがないことになるのでその対策
                            .Rows(1).Cells(1).Text = AlertDate(iloop).ToString("yyyy年 MM月 dd日 HH時 mm分")                    '最新データ日時
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
                    '.Rows(1).Cells(2).BorderColor = Color.Silver                Else
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
                                AddCel.ID = "tAlm" & (jloop).ToString & iloop.ToString                  ' 2015/11/19 Kino Add
                                'AddCel.CssClass = "NoClass"
                                AddRow.Cells.Add(AddCel)
                                If iloop = 2 AndAlso alertSensorTips(jloop).Length <> 0 Then            ' 2015/11/25 Kino Add
                                    AddCel.ToolTip = alertSensorTips(jloop).ToString
                                End If
                                'AddRow.BorderColor = Color.Silver
                            Next iloop

                            RowCount += 1

                            .Rows.Add(AddRow)
                            AddRow.HorizontalAlign = HorizontalAlign.Center
                            AddRow.VerticalAlign = VerticalAlign.Middle
                            'AddRow.ID = ("Alert" & jloop.ToString("00"))
                            AddRow.CssClass = "alertRow"
                            AddRow.ToolTip = ""     '【管理基準値超過】<br>　尿前雨量：注意体制<br>　尿前雨量：注意体制<br>　尿前雨量：注意体制<br>　尿前雨量：注意体制<br>　尿前雨量：注意体制<br>　尿前雨量：注意体制"

                            '' 警報発生状況表示
                            ''データを記載する
                            'ヘッダを除く１行１列目
                            .Rows(RowCount).Cells(0).Text = DataFileNames(jloop).ZoneName                                       '名称
                            '１行２列目
                            .Rows(RowCount).Cells(1).Text = AlertDate(jloop).ToString("yyyy年 MM月 dd日 HH時 mm分")                              '最新データ日時
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

        'ViewState("SelectedAccordionIndex") = CType(Page.Master.FindControl("hdninput"), TextBox).Text
        'CType(Page.Master.FindControl("MyAccordion"), AjaxControlToolkit.Accordion).SelectedIndex = CType(ViewState("SelectedAccordionIndex"), Integer)

        ''Call DataCollectCheck()     'strOldestDate, strNewestDate)                ' 2011/05/23 Kino Changed

    End Sub

    Protected Sub DataCollectCheck()        'ByRef strOldestDate As String, ByRef strNewestDate As String)
        ''
        '' データの収録状況の確認と表示
        ''
        Dim DiffDay As Integer
        ''Dim SiteDirectoryFull As String
        Dim dteTemp() As Date
        Dim iloop As Integer
        Dim FileCount As Integer
        Dim strTemp As String = ""
        ''Dim strTerm As String = ""
        Dim siteName As String = ""
        Dim completeSite As Boolean
        Dim MaxDate As DateTime = New DateTime(2000, 1, 1, 0, 0, 0)                 ' 2018/03/05 Kino Add

        siteName = Session.Item("SN")
        If siteName.Contains("【完了現場】") = True Then completeSite = True
        ''SiteDirectoryFull = Session.Item("FSD")    '現場ディレクトリ
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

        Dim LbArt As Label = CType(Master.FindControl("LblAlert"), Label)           '2017/02/24 Kino Add 場所を変えてマスターページ内の空きスペースに入れた

        For iloop = 0 To FileCount

            'dteTemp(iloop) = DataFileNames(iloop).NewestDate           ' 2012/10/22 Kino Changed データ収録期間の表示順を変更するための変更
            dteTemp(iloop) = sortedDataFileNames(iloop).NewestDate

            If MaxDate < dteTemp(iloop) Then                                        ' 2018/03/05 Kino Add
                MaxDate = dteTemp(iloop)
            End If

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
                    LbArt.Visible = True '文字列変数の中身があれば、表示する。 2009/01/28 Kino
                    'Me.LitSpace1.Text = "<br /><img src='./img/space_M.gif'>"
                Else
                    LbArt.Visible = False
                    'Me.LitSpace1.Text = vbNullString
                End If

            Else
                LbArt.Visible = False
                'Me.LitSpace1.Text = vbNullString                  '"<br />"   2009/05/29 Kino Changed Comment
            End If

            ''strTerm = strTerm + (DataFileNames(iloop).ZoneName + ":" + DataFileNames(iloop).OldestDate.ToString("yyyy/MM/dd HH:mm") + " ～ " + DataFileNames(iloop).NewestDate.ToString("yyyy/MM/dd HH:mm ") + "<br>")

            ''データが更新されていたら・・・                                                       ’ 2015/11/25 Kino Changed 判断文の中身がなかったのでコメント
            ''If dteTemp(iloop) < DataFileNames(iloop).NewestDate Then
            'If dteTemp(iloop) < sortedDataFileNames(iloop).NewestDate Then                  ' 2012/10/22 Kino Changed データ収録期間の表示順を変更するための変更

            '    ''Dim lblExistDataFile As Label = CType(Page.Master.FindControl("lblExistDataFile"), Label)
            '    ''lblExistDataFile.Text = "1"
            '    ''lblExistDataFile = Nothing
            '    ''ClsReadDataFile.dteLastUpdate = Date.Parse(strNewestDate)
            'End If

            If iloop = 0 Then
                Dim HeaderRow As New TableRow()
                Dim HeaderCell As New TableCell()
                HeaderCell.Text = "【データ収録期間】"
                HeaderCell.ColumnSpan = 4
                HeaderCell.CssClass = "priodCol1"
                HeaderRow.Cells.Add(HeaderCell)
                'Me.tblLastUpdate.Rows(0).Cells(0).Text = "【データ収録期間】"
                HeaderRow.TableSection = TableRowSection.TableHeader
                Me.tblLastUpdate.Rows.Add(HeaderRow)
            End If

            With Me.tblLastUpdate

                Dim Datas As New TableRow()
                Dim strCell As String = ""
                'Datas.ControlStyle.CssClass = "periodCol"
                For jloop As Integer = 0 To 3
                    Dim dataCell As New TableCell()
                    dataCell.Wrap = False
                    dataCell.ColumnSpan = 1
                    dataCell.ID = "tblRC" & iloop.ToString & jloop.ToString            ' 2015/11/19 Kino Add
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
            LbArt.BackColor = Color.PaleGreen
            LbArt.ForeColor = Color.Blue
            Me.nwdt.Value = "NC"                                    ' 2018/03/05 Kino Add
        End If

        LbArt.Text = strTemp
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
        ''Dim reTryCount As Integer                                                                               ' 2011/05/23 Kino Add
        Dim lngRet As Long = 0

        SiteDirectory = Session.Item("SD")                                                                      '現場ディレクトリ
        DataFile = Server.MapPath(SiteDirectory + "\App_Data\MenuInfo.mdb")
        intRet = clsNewDate.GetDataFileNames(DataFile, DataFileNames)                                           'データファイル名、共通情報ファイル名、識別名を取得

        '' 上の関数で取得するようになしたためコメント
        ''Do                                                                                                      ' 2011/05/23 Kino Add リトライ処理
        ''    intRet = clsNewDate.GetDataLastUpdate(Server.MapPath(SiteDirectory + "\App_Data\"), DataFileNames)      'データファイルにおける最新・最古データ日時を取得

        ''    If intRet = -1 Then                                                                                 ' 2011/05/23 Kino Add 読み込みリトライ
        ''        reTryCount += 1
        ''        System.Threading.Thread.Sleep(2000)
        ''        If reTryCount >= 15 Then
        ''            lngRet = -1
        ''            Exit Do
        ''        End If
        ''    Else
        ''        Exit Do
        ''    End If
        ''Loop

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

    Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)

        Page.ClientScript.RegisterForEventValidation(btnSubmit.UniqueID, String.Empty)
        MyBase.Render(writer)

        ''writer.Write("<a  id=""" & Me.UniqueID & """ href=""javascript:" & Page.GetPostBackEventReference(Me) & """>")
        ''writer.Write(" " & Me.UniqueID & "</a>")

    End Sub

End Class
