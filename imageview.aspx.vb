Imports AjaxControlToolkit
Imports System.IO
Imports System.Data

Partial Class imageview
    Inherits System.Web.UI.Page

    Public Structure ImageViewInfo
        Dim PointName As String
        Dim DefaultPoint As Boolean
        Dim FilePreFix As String
    End Structure

    Public Structure ImageSize
        Dim Height As Integer
        Dim Width As Integer
    End Structure

    Private imgInfo() As ImageViewInfo
    ''Private MaxPicCount As Integer
    Private thumbnailPage As Integer
    Private ImageInf As ImageSize
    Private thumbnailInf As ImageSize
    Private NowPrefix As String

    Protected Sub Page_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error

        ' 2011/05/30 Kino Add

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
        ''
        '' ページを閉じるときにデータオブジェクトとグリッドをDisposeする
        ''
        'Me.ViewState.Clear()

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        ''■デバッグのためコメント
        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"
        If User.Identity.IsAuthenticated = False Then
            'Response.Redirect("Login.aspx")
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer = CType(Session.Item("LgSt"), Integer)          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            Response.Redirect("sessionerror.aspx", False)
            ''Response.Write(strScript)
            Exit Sub
        End If
        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする

        Dim siteName As String
        Dim siteDirectory As String
        Dim imgDataFile As String
        Dim intRet As Integer
        Dim clsNewDate As New ClsReadDataFile

        siteName = CType(Session.Item("SN"), String)                '現場名
        siteDirectory = CType(Session.Item("SD"), String)           '現場ディレクトリ
        ''imgInfo.GraphID = CType(Request.Item("GNa"), String)    '表タイトル（ウィンドウタイトル）
        imgDataFile = IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", "ImageView.mdb")
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)

        '' ''intRet = clsNewDate.GetDataFileNames(Server.MapPath(siteDirectory & "\App_Data\MenuInfo.mdb"), DataFileNames)       'データファイル名、共通情報ファイル名、識別名を取得

        ' 2017/03/14 Kino Changed 不要のためコメント
        ''If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
        ''    ''==== フォームのサイズを調整する ====
        ''    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
        ''        Dim OpenString As String

        ''        OpenString = "<SCRIPT LANGUAGE='javascript'>"
        ''        OpenString += ("window.resizeTo(" + wid + "," + Hei + ");")
        ''        OpenString += "<" + "/SCRIPT>"
        ''        Page.ClientScript.RegisterStartupScript(Me.GetType(), "経時変化図", OpenString)
        ''    End If
        ''    ''====================================
        ''End If

        Dim imgFile() As String = {}
        Dim imgFileDate() As Date = {}

        If IsPostBack = False Then      'ポストバックではないときにはデータベースから設定を読み込む

            Using DbCon As New OleDb.OleDbConnection
                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & imgDataFile & ";" & "Jet OLEDB:Engine Type= 5")
                ''DbCon.Open()

                intRet = ReadimageInfoFromDB(DbCon)               'グラフイメージ情報を取得
                NowPrefix = SetBIND2Controls(DbCon)
                ''DbCon.Close()
                thumbnailPage = 0
                intRet = GetImageFiles(imgFile, imgFileDate, NowPrefix, siteDirectory)
                If intRet <> -1 Then                                                        '2010/04/14 Kino Add ファイルがない場合の対応
                    Call SetThumbnail(siteDirectory, imgFile, imgFileDate, ImageInf)
                End If
                Call setViewState()
            End Using
        Else

            Call readViewState()
            NowPrefix = Me.DDLPointName.SelectedValue
            Dim opeID As String = ToolkitScriptManager1.AsyncPostBackSourceElementID
            ''Page.Request.Params.Get("__EVENTTARGET")
            Dim selItem As Integer = Me.DDLDateTime.SelectedIndex
            If opeID = "DDLDateTime" Or opeID = "DDLPointName" Then
                intRet = GetImageFiles(imgFile, imgFileDate, NowPrefix, siteDirectory)
                If intRet <> -1 Then                                                        ''2010/04/14 Kino Add ファイルがなかった場合の対応
                    If opeID = "DDLDateTime" Then thumbnailPage = selItem \ 5
                    If opeID = "DDLPointName" Then
                        thumbnailPage = 0
                        Dim strTemp As String = Me.DDLDateTime.SelectedValue
                        Me.Image1.ImageUrl = "~/api/thumbnail.ashx?img=" + strTemp + "&h=" + ImageInf.Height.ToString & "&w=" + ImageInf.Width.ToString
                        Me.Image1.ToolTip = "撮影日時：" + Me.DDLDateTime.SelectedItem.Text
                        Me.lblPhotoDate.Text = "【 設置箇所：" + Me.DDLPointName.SelectedItem.Text + "　　撮影日時：" + Me.DDLDateTime.SelectedItem.Text + " 】"
                    Else
                        Me.DDLDateTime.SelectedIndex = selItem
                    End If
                    Call SetThumbnail(siteDirectory, imgFile, imgFileDate, ImageInf)
                    ViewState("idxPg") = thumbnailPage
                End If
            End If
            'If ToolkitScriptManager1.AsyncPostBackSourceElementID = "DDLPointName" Then        'ポストバックがどのコントロールから発生したのかで判断(カレンダーのイベントではここに入らない!!)
            '    intRet = GetImageFiles(imgFile, imgFileDate, NowPrefix, siteDirectory)
            '    Call SetThumbnail(siteDirectory, imgFile, imgFileDate, ImageInf)
            '    Dim strTemp As String = Me.DDLDateTime.SelectedValue
            '    Me.Image1.ImageUrl = "makeThumbnail.ashx?img=" + strTemp + "&h=" + ImageInf.Height.ToString & "&w=" + ImageInf.Width.ToString
            '    Me.Image1.ToolTip = Me.DDLDateTime.SelectedItem.Text
            'End If

        End If

        Dim PointName As String = Me.DDLPointName.SelectedItem.Text
        Me.MyTitle.Text = "撮影画像表示" & " [" & PointName & "] - " & siteName

    End Sub

    Protected Function GetImageFiles(ByRef imgFile() As String, ByRef imgFileDate() As Date, ByVal NowPrefix As String, ByVal SiteDirectory As String) As Integer
        ''
        '' 規定フォーマットのファイル名を持ったJPEGファイル一覧を取得
        ''

        Dim FileName As String
        Dim ImageFolder As String = Server.MapPath(SiteDirectory + "\img\")
        Dim strText As New StringBuilder
        Dim strDate As New StringBuilder
        Dim FileCount As Integer
        ''Dim lngTemp() As Long = {}
        Dim iloop As Integer
        Dim clsDateExcahge As New ClsReadDataFile
        Dim strTempo As String

        '' 指定ディレクトリにあるファイルを全て取得する

        For Each FileName In Directory.GetFiles(ImageFolder, NowPrefix + "*_00.jpg")
            If FileName.Length <> 0 Then
                strText.Append(FileName.Substring(ImageFolder.Length) + ",")
            End If
        Next

        If strText.Length <> 0 Then                                         '2010/04/14 Kino Add ファイルがない場合の対応
            strTempo = strText.ToString.Substring(0, strText.Length - 1)
            imgFile = strTempo.Split(","c)                           'カンマ区切りで配列に
        End If

        FileCount = imgFile.Length - 1
        ''ReDim lngTemp(FileCount)
        ''For iloop = 0 To FileCount                                      '一度Longにする
        ''    lngTemp(iloop) = Long.Parse(imgFile(iloop))
        ''Next
        ''Array.Sort(lngTemp)                                             '降順でソートしてから
        ''Array.Reverse(lngTemp)                                          '昇順でソートしないと並ばない・・・

        Array.Sort(imgFile)
        Array.Reverse(imgFile)

        ReDim imgFileDate(FileCount)

        DDLDateTime.Items.Clear()
        For iloop = 0 To FileCount
            ''imgFile(iloop) = NowPrefix + lngTemp(iloop).ToString + "_00.jpg"
            ''imgFileDate(iloop) = Date.Parse(clsDateExcahge.exchange2Date(lngTemp(iloop).ToString))
            imgFileDate(iloop) = Convert.ToDateTime(clsDateExcahge.exchange2Date(imgFile(iloop).Substring(NowPrefix.Length, imgFile(iloop).IndexOf(".jpg") - NowPrefix.Length - 3)))
            Me.DDLDateTime.Items.Add(imgFileDate(iloop).ToString("yyyy/MM/dd HH:mm"))
            Me.DDLDateTime.Items(iloop).Value = imgFile(iloop).ToString
        Next

        ''FileCount = imgFile.Length - 1
        ''If FileCount > MaxPicCount Then
        ''    For iloop = FileCount To (MaxPicCount + 1) Step -1
        ''        File.Delete(ImageFolder + imgFile(iloop))
        ''    Next
        ''    ReDim Preserve imgFile(MaxPicCount)
        ''End If

        clsDateExcahge = Nothing
        strText = Nothing
        strDate = Nothing
        Return FileCount                                '最後にCrLfが余計に入るので１つ減らす　そして配列は０からなのでさらに１つ減らす

    End Function


    Protected Function SetThumbnail(ByVal siteDirectory As String, ByVal imgFile() As String, ByVal imgFileDate() As Date, ByVal imageInf As ImageSize) As Boolean
        ''
        '' サムネイルを表示する
        ''

        Dim setBtn(4) As ImageButton
        Dim setLbl(4) As Label
        Dim iloop As Integer
        Dim stIndex As Integer
        Dim edIndex As Integer
        Dim objIndex As Integer = 0
        Dim maxIndex As Integer = imgFile.Length - 1
        Dim imagePath As String = IO.Path.Combine(Server.MapPath(siteDirectory), "img")
        ''Session.Add("imgPath", imagePath)
        Response.BufferOutput = True
        For iloop = 0 To 4
            setBtn(iloop) = CType(Form.FindControl("ImgBtn" + iloop.ToString), ImageButton)
            setLbl(iloop) = CType(Form.FindControl("LblPhotoDate" + iloop.ToString), Label)
        Next iloop

        stIndex = thumbnailPage * 5
        edIndex = stIndex + 4
        ''If edIndex > maxIndex Then
        ''    'edIndex = maxIndex
        ''End If
        If ToolkitScriptManager1.AsyncPostBackSourceElementID = "BtnNext" Or ToolkitScriptManager1.AsyncPostBackSourceElementID = "BtnPrev" Then
            Me.DDLDateTime.SelectedIndex = stIndex
        End If

        objIndex = Me.DDLDateTime.SelectedIndex
        Me.Image1.ImageUrl = String.Format("~/api/thumbnail.ashx?img={0}&h={1}&w={2}", imgFile(objIndex), imageInf.Height, imageInf.Width)         '"~/thumbnail.ashx?img=" + imgFile(objIndex).ToString + "&h=" + imageInf.Height.ToString + "&w=" + imageInf.Width.ToString
        Me.Image1.AlternateText = String.Format("撮影日時：{0}", imgFileDate(objIndex).ToString("yyyy/MM/dd HH:mm"))     '"撮影日時：" + imgFileDate(objIndex).ToString("yyyy/MM/dd HH:mm")
        Me.Image1.ToolTip = String.Format("撮影日時：{0}", imgFileDate(objIndex).ToString("yyyy/MM/dd HH:mm"))            '"撮影日時：" + imgFileDate(objIndex).ToString("yyyy/MM/dd HH:mm")
        Me.lblPhotoDate.Text = String.Format("【 設置個所：{0}　　撮影日時：{1} 】", Me.DDLPointName.SelectedItem.Text, imgFileDate(objIndex).ToString("yyyy/MM/dd HH:mm"))        '"【 設置箇所：" + Me.DDLPointName.SelectedItem.Text + "　　" + Me.Image1.ToolTip + " 】"
        Me.PnlSelect.Width = imageInf.Width
        Me.PnlPhotoDate.Width = imageInf.Width

        objIndex = 0
        For iloop = stIndex To edIndex
            setBtn(objIndex).ImageUrl = ""
            If imgFile.Length - 1 < iloop Then
                setBtn(objIndex).Enabled = False
                setBtn(objIndex).ImageUrl = "~/api/thumbnail.ashx?img=None" + "&h=" + thumbnailInf.Height.ToString + "&w=" + thumbnailInf.Width.ToString
                setBtn(objIndex).ToolTip = "---"
                setLbl(objIndex).Text = "---"
            Else
                setBtn(objIndex).Enabled = True
                If File.Exists(IO.Path.Combine(imagePath, imgFile(iloop).ToString)) = True Then
                    setBtn(objIndex).ImageUrl = String.Format("~/api/thumbnail.ashx?img={0}&h={1}&w={2}", imgFile(iloop), thumbnailInf.Height, thumbnailInf.Width)        '"~/thumbnail.ashx?img=" + imgFile(iloop) + "&h=" + thumbnailInf.Height.ToString + "&w=" + thumbnailInf.Width.ToString
                    setBtn(objIndex).ToolTip = String.Format("撮影日時：{0}", imgFileDate(iloop).ToString("yyyy/MM/dd HH:mm"))     '"撮影日時：" + imgFileDate(iloop).ToString("yyyy/MM/dd HH:mm")
                    setBtn(objIndex).CommandArgument = imgFile(iloop)
                End If
                setLbl(objIndex).Text = imgFileDate(iloop).ToString("yyyy/MM/dd HH:mm")
            End If
            setLbl(objIndex).Width = thumbnailInf.Width
            objIndex += 1
        Next iloop

        setBtn = Nothing
        setLbl = Nothing

        ''ashxへ移行したので、不要　参考に取っておく
        'Dim instance As System.Drawing.Image = Image.FromFile(Server.MapPath(siteDirectory & "\img\C1_200910280925_00.jpg"))
        'Dim thumbWidth As Integer = 120
        'Dim thumbHeight As Integer = 97
        ' ''Dim callback As GetThumbnailImageAbort
        ' ''Dim callbackData As IntPtr
        ' ''Dim returnValue As System.Drawing.Image
        ' ''callback = New Image.GetThumbnailImageAbort(AddressOf DummyCallbac)
        ' ''returnValue = instance.GetThumbnailImage(thumbWidth, thumbHeight, callback, callbackData)

        ' ''returnValue.Save(Server.MapPath(siteDirectory & "\img\01.png"))
        ' ''Me.ImgBtn0.ImageUrl = "~/" & siteDirectory & "/img/01.png"

        'Dim thumbnail As Bitmap = New Bitmap(thumbWidth, thumbHeight)
        'Using g As Graphics = Graphics.FromImage(thumbnail)
        '    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        '    g.DrawImage(instance, 0, 0, thumbWidth, thumbHeight)
        'End Using

        'instance.Save(Server.MapPath(siteDirectory & "\img\01.png"))
        'Me.ImgBtn1.ImageUrl = "~/" & siteDirectory & "/img/02.png"

        ''  上の一環
        ''Shared Function DummyCallbac() As Boolean
        ''    Return False
        ''End Function
        Return True

    End Function


    Protected Function ReadimageInfoFromDB(ByVal dbCon As OleDb.OleDbConnection) As Integer
        '
        ' 描画情報を読み込む
        '
        'Dim DbDa As New OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String

        ReadimageInfoFromDB = 0
        strSQL = "Select * From 写真表示情報 ORDER BY ID"
        Using DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
            Using DtSet As New DataSet("DData")

                DbDa.Fill(DtSet, "DData")

                With DtSet.Tables("DData").Rows(0)
                    ''MaxPicCount = Integer.Parse(.Item(1))               '最大保存写真枚数
                    thumbnailInf.Width = Convert.ToInt32(.Item(2))        'サムネイルの幅
                    thumbnailInf.Height = Convert.ToInt32(.Item(3))       'サムネイルの高さ
                    ImageInf.Width = Convert.ToInt32(.Item(4))            '表示画像の幅
                    ImageInf.Height = Convert.ToInt32(.Item(5))           '表示画像の高さ
                End With
                'DbDa.Dispose()
                'DtSet.Dispose()
            End Using
        End Using

        Me.Image1.Width = ImageInf.Width
        Me.Image1.Height = ImageInf.Height
        Dim iloop As Integer
        For iloop = 0 To 4
            CType(Form.FindControl("ImgBtn" + iloop.ToString), ImageButton).Width = thumbnailInf.Width
            CType(Form.FindControl("ImgBtn" + iloop.ToString), ImageButton).Height = thumbnailInf.Height
        Next iloop
        Me.Pnlthum.Width = (thumbnailInf.Width + 10)

    End Function

    'Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
    '    CType(Form.FindControl("ImgBtn4"), ImageButton).ImageUrl = "~/img/ajax-loader.gif"
    'End Sub

    Protected Sub ImgBtn0_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
        ''
        ''イメージボタンクリックイベント
        ''

        Dim FileName As String
        Dim setBtn As ImageButton
        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ
        Dim clsDateExcahge As New ClsReadDataFile
        Dim imagePath As String = IO.Path.Combine(Server.MapPath(siteDirectory), "img")
        Dim photoDate As String

        setBtn = CType(Form.FindControl(CType(sender, ImageButton).ID), ImageButton)
        FileName = setBtn.CommandArgument

        Response.BufferOutput = True
        ''Session.Add("imgPath", Server.MapPath(siteDirectory & "\img\"))

        If File.Exists(IO.Path.Combine(imagePath, FileName)) = True Then
            ''Me.Image1.ImageUrl = "~/thumbnail.ashx?img=" + FileName + "&h=" + ImageInf.Height.ToString & "&w=" + ImageInf.Width.ToString
            Me.Image1.ImageUrl = String.Format("~/api/thumbnail.ashx?img={0}&h={1}&w={2}", FileName, ImageInf.Height, ImageInf.Width)
            photoDate = Convert.ToDateTime(clsDateExcahge.exchange2Date(FileName.Substring(NowPrefix.Length, FileName.IndexOf(".jpg") - NowPrefix.Length - 3))).ToString("yyyy/MM/dd HH:mm")
            Me.Image1.ToolTip = String.Format("撮影日時：{0}", photoDate)
            Me.lblPhotoDate.Text = String.Format("【 設置個所：{0}　　撮影日時：{1} 】", Me.DDLPointName.SelectedItem.Text, photoDate)
            Me.DDLDateTime.SelectedItem.Text = photoDate
        End If

        clsDateExcahge = Nothing
        setBtn = Nothing

    End Sub

    Protected Sub ImgBtnClose_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImgBtnClose.Click
        ''
        '' 閉じるボタンイベント
        ''
        ''Session.Remove("imgPath")

    End Sub

    Public Function SetBIND2Controls(ByVal dbCon As OleDb.OleDbConnection) As String
        ''
        ''ドロップダウンリストボックスにおける各設定のデータセットバインド
        ''
        Dim DbCom As New OleDb.OleDbCommand
        Dim DbDa As OleDb.OleDbDataAdapter
        Dim DtSet As New DataSet("DData")
        Dim strSQL As String
        Dim strTemp As String = ""

        ''■■日付範囲
        strSQL = "SELECT * FROM メニュー基本情報 ORDER BY ID"
        '' 現場名称の読込
        DbDa = New OleDb.OleDbDataAdapter(strSQL, dbCon)
        DbDa.Fill(DtSet, "DData")

        With Me.DDLPointName
            .DataSource = DtSet.Tables("DData")
            .DataMember = DtSet.DataSetName
            .DataTextField = "測点名称"
            .DataValueField = "ファイル名プリフィックス"
            .DataBind()
        End With

        For Each DTR As DataRow In DtSet.Tables("DData").Rows
            If Convert.ToBoolean(DTR.Item(1)) = True Then
                Me.DDLPointName.SelectedIndex = Convert.ToInt32(DTR.Item(0))
                strTemp = DTR.Item(3).ToString
                Exit For
            End If
        Next

        DbDa.Dispose()
        DbCom.Dispose()
        DtSet.Dispose()

        If strTemp.Length = 0 Then
            strTemp = Me.DDLPointName.Items(0).Value
        End If

        Return strTemp

    End Function

    Protected Sub BtnPrev_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        ''
        '' 画像切り替えボタン
        ''
        Dim intRet As Integer
        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ

        Dim imgFile() As String = {}
        Dim imgFileDate() As Date = {}

        intRet = GetImageFiles(imgFile, imgFileDate, NowPrefix, siteDirectory)
        If intRet <> -1 Then                                                        '2010/04/14 Kino Add ファイルがなかった場合の対応
            thumbnailPage = CType(ViewState("idxPg"), Integer)

            If CType(sender, Button).ID = "BtnPrev" Then
                thumbnailPage -= 1
                If thumbnailPage <= 0 Then thumbnailPage = 0
            Else
                thumbnailPage += 1
                If thumbnailPage * 5 > intRet Then
                    thumbnailPage -= 1
                End If
            End If

            ViewState("idxPg") = thumbnailPage
            Call SetThumbnail(siteDirectory, imgFile, imgFileDate, ImageInf)
            ''Me.lblPhotoDate.Text = "【 測点：" + Me.DDLPointName.SelectedItem.Text + "　　撮影日時：" + Me.DDLDateTime.SelectedItem.Text + " 】"
        End If

    End Sub

#Region "IntegerSort"
    ''integerの配列ソートなら使える
    ''Public Sub DimSort(ByRef minds() As Integer)
    ''    Dim front() As Integer = Nothing
    ''    Dim rear() As Integer = Nothing
    ''    Dim base As Integer
    ''    Dim forI As Integer

    ''    If minds.Length = 2 Then
    ''        If minds(1) < minds(0) Then
    ''            Array.Reverse(minds)
    ''        End If
    ''    ElseIf minds.Length > 2 Then
    ''        '平均値を基準とする
    ''        base = Average(minds)

    ''        '基準の前後に分割
    ''        For forI = 0 To minds.Length - 1
    ''            If minds(forI) <= base Then
    ''                If front Is Nothing Then
    ''                    ReDim front(0)
    ''                Else
    ''                    ReDim Preserve front(front.Length)
    ''                End If
    ''                front(front.Length - 1) = minds(forI)
    ''            Else
    ''                If rear Is Nothing Then
    ''                    ReDim rear(0)
    ''                Else
    ''                    ReDim Preserve rear(rear.Length)
    ''                End If
    ''                rear(rear.Length - 1) = minds(forI)
    ''            End If
    ''        Next

    ''        If rear Is Nothing Then
    ''            '全部同じ値
    ''            Exit Sub
    ''        End If

    ''        'さらに斬り分ける
    ''        Call DimSort(front)
    ''        Call DimSort(rear)

    ''        'くっつける
    ''        Array.Copy(front, 0, minds, 0, front.Length)
    ''        Array.Copy(rear, 0, minds, front.Length, rear.Length)
    ''    End If
    ''End Sub

    ''Public Function Average(ByVal DataArray() As Integer) As Integer
    ''    Dim ArraySum As Integer
    ''    Dim forI As Integer

    ''    For forI = 0 To DataArray.Length - 1
    ''        ArraySum += DataArray(forI)
    ''    Next
    ''    ArraySum \= DataArray.Length

    ''    Return ArraySum
    ''End Function
#End Region

    Protected Sub DDLDateTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        ''
        '' ドロップダウンリストから表示日付を選択した時のイベント
        ''
        Dim strTemp As String = Me.DDLDateTime.SelectedValue

        Me.Image1.ImageUrl = String.Format("~/api/thumbnail.ashx?img={0}&h={1}&w={2}", strTemp, ImageInf.Height, ImageInf.Width)            '"~/thumbnail.ashx?img=" + strTemp + "&h=" + ImageInf.Height.ToString & "&w=" + ImageInf.Width.ToString
        Me.Image1.ToolTip = String.Format("撮影日時：{0}", Me.DDLDateTime.SelectedItem.Text)                                             '"撮影日時：" + Me.DDLDateTime.SelectedItem.Text
        Me.lblPhotoDate.Text = String.Format("【 設置個所：{0}　　撮影日時：{1} 】", Me.DDLPointName.SelectedItem.Text, Me.DDLDateTime.SelectedItem.Text)       '"【 測点：" + Me.DDLPointName.SelectedItem.Text + "　　撮影日時：" + Me.DDLDateTime.SelectedItem.Text + " 】"

    End Sub

    Protected Sub DDLPointName_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DDLPointName.TextChanged
        ''
        '' ドロップダウンリストから測点名称を選択した時のイベント
        ''
        NowPrefix = Me.DDLPointName.SelectedValue

    End Sub


    Protected Sub setViewState()
        ''
        '' 変数から取得した内容をビューステートに格納する     変数　→　ビューステート
        ''
        ViewState("thumbW") = thumbnailInf.Width
        ViewState("thumbH") = thumbnailInf.Height
        ViewState("ImageW") = ImageInf.Width
        ViewState("ImageH") = ImageInf.Height

    End Sub

    Protected Sub readViewState()
        ''
        '' ビューステートから取得した内容を変数へ格納する　　　ビューステート　→　変数
        ''
        thumbnailInf.Width = CType(ViewState("thumbW"), Integer)
        thumbnailInf.Height = CType(ViewState("thumbH"), Integer)
        ImageInf.Width = CType(ViewState("ImageW"), Integer)
        ImageInf.Height = CType(ViewState("ImageH"), Integer)
        thumbnailPage = CType(ViewState("idxPg"), Integer)

    End Sub

End Class