Imports System.Data
Imports System.Drawing

Partial Class Graph_Image
    Inherits System.Web.UI.Page

    Private DataFileNames() As ClsReadDataFile.DataFileInf
    Public Structure GraphImageInfo
        Dim ContentsTitle As String             'タイトル
        Dim GraphID As String                   'グラフの識別名
        Dim GraphTitle As String                'グラフタイトル
        Dim TitleSize As Integer                'グラフタイトルサイズ
        Dim TitlePosition As Integer            'グラフタイトルの位置
        Dim LastUpdate As Date                  'データの最終更新日
        Dim ImagePath As String                 '画像のファイルパス
        Dim DataFileNo As Integer               'グラフのデータファイル番号
        Dim ShowDateTime As Boolean             '最新データ日時の表示の有無
    End Structure

    Private imgInfo As GraphImageInfo

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
        ''
        '' ページを閉じるときにデータオブジェクトとグリッドをDisposeする
        ''
        Me.ViewState.Clear()

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"
        If User.Identity.IsAuthenticated = False Then
            'Response.Redirect("Login.aspx")
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer = Session.Item("LgSt")          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            Response.Redirect("sessionerror.aspx", False)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする
        Response.Cache.SetNoStore()
        Response.Cache.SetExpires(DateTime.Now.AddDays(-1))

        Dim siteName As String
        Dim siteDirectory As String
        Dim imgDataFile As String
        Dim intRet As Integer
        Dim clsNewDate As New ClsReadDataFile

        siteName = CType(Session.Item("SN"), String)                '現場名
        siteDirectory = CType(Session.Item("SD"), String)           '現場ディレクトリ
        imgInfo.GraphID = CType(Request.Item("GNa"), String)    '表タイトル（ウィンドウタイトル）
        imgDataFile = IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", "ImageGraph.mdb")
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)

        intRet = clsNewDate.GetDataFileNames(IO.Path.Combine(Server.MapPath(siteDirectory), "App_Data", "MenuInfo.mdb"), DataFileNames)       'データファイル名、共通情報ファイル名、識別名を取得

        ''        If IsPostBack = False Then      'ポストバックではないときにはデータベースから設定を読み込む

        ' 2017/03/14 Kino Changed 不要のためコメント
        ''If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add
        ''    ''==== フォームのサイズを調整する ====
        ''    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
        ''        Dim OpenString As String

        ''        OpenString = "<SCRIPT LANGUAGE='javascript'>"
        ''        OpenString &= "window.resizeTo(" & wid & "," & Hei & ");"
        ''        OpenString &= "<" & "/SCRIPT>"

        ''        'Dim instance As ClientScriptManager = Page.ClientScript                            'この方法から↓の方法へ変更した
        ''        'instance.RegisterClientScriptBlock(Me.GetType(), "経時変化図", OpenString)
        ''        Page.ClientScript.RegisterStartupScript(Me.GetType(), "経時変化図", OpenString)
        ''    End If
        ''End If

        '初回読み込みはデータベースから行なう
        Using DbCon As New OleDb.OleDbConnection
            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & imgDataFile & ";" & "Jet OLEDB:Engine Type= 5")
            ''DbCon.Open()

            intRet = ReadimageInfoFromDB(DbCon)               'グラフイメージ情報を取得

            ''DbCon.Close()
            'DbCon.Dispose()
        End Using

        '自動更新設定の読み込み
        Call CheckAutoUpdate(siteDirectory)

        ''Else                            'ポストバックの時はビューステートから設定を読み込む
        ' ''Call ReadViewState()
        '' ''Call GetMostUpdate()
        ''End If

        Dim imgFileWebPath As String = ("~/" & siteDirectory & "/img/" & imgInfo.ImagePath)

        'グラフタイトル 表示位置、フォントサイズ設定
        Dim titlePos As String = imgInfo.TitlePosition.ToString("00")                                           ' 2012/08/22 Kino Add
        Dim objLabel As System.Web.UI.WebControls.Label
        ' If imgInfo.TitlePosition = 0 Then    
        If titlePos.Substring(1, 1) = "0" Then                                                                  ' 上
            ''Me.LitUpper.Text = "<img src='./img/space_S.gif'><br />"    '" <br /><br />"
            ''Me.LitLower.Text = ""
            objLabel = Me.LblTitleUpper
            Me.LblTitleUpper.Visible = True
            Me.LblTitleLower.Visible = False
            Controls.Remove(Me.LblTitleLower)
        Else                                                                                                    ' 下
            ''Me.LitUpper.Text = ""
            ''Me.LitLower.Text = "<img src='./img/space_S.gif'><br />"    ' "<br /><br />"
            objLabel = Me.LblTitleLower
            Me.LblTitleUpper.Visible = False
            Me.LblTitleLower.Visible = True
            Controls.Remove(Me.LblTitleUpper)
        End If

        Select Case titlePos.Substring(0, 1)                                                                    ' 2012/08/22 Kino Add
            Case "0"    ' 中
                objLabel.Attributes.Add("Class", "LABELS")
            Case "1"    ' 左
                objLabel.Attributes.Add("Class", "LABELS_Left")
            Case "2"    ' 右
                objLabel.Attributes.Add("Class", "LABELS_Right")
        End Select

        objLabel.Text = imgInfo.GraphTitle
        objLabel.Font.Size = imgInfo.TitleSize

        If imgInfo.ShowDateTime = True Then
            imgInfo.LastUpdate = CType(Session.Item("LastUpdate" & imgInfo.DataFileNo.ToString), String)          '最新データ
            Me.LblLastUpdate.Text = "最新データ日時：" & imgInfo.LastUpdate.ToString("yyyy/MM/dd HH:mm")
        Else
            Me.LblLastUpdate.Enabled = False
            Me.LblLastUpdate.Visible = False
            Controls.Remove(Me.LblLastUpdate)
        End If

        Me.MyTitle.Text = imgInfo.GraphTitle & " [" & imgInfo.ContentsTitle & "] - " & siteName
        Dim img As Drawing.Image
        img = Drawing.Image.FromFile(Server.MapPath(imgFileWebPath))                                                '画像の横幅を取得
        objLabel.Width = img.Width
        img.Dispose()

        ''Dim imagePath As String = Server.MapPath(SiteDirectory & "\img\")                         ’ Httpハンドラで使用していたが、不要となったためコメント 2018/05/30 Kino
        ''Session.Add("imgPath", imagePath)
        Response.BufferOutput = True

        Me.ImgGraph.ImageUrl = "~/api/thumbnail.ashx?img=" & imgInfo.ImagePath                                            '2009/11/09 Kino Changed 画像のパスが見えなくなるのでこっちがよい
        ''Me.ImgGraph.ImageUrl = "~/" & siteDirectory & imgInfo.ImagePath                   'Server.MapPath(siteDirectory & imgInfo.ImagePath)

        Me.lblCaption.Text = "●" & imgInfo.ContentsTitle

    End Sub


    Protected Function ReadimageInfoFromDB(ByVal dbCon As OleDb.OleDbConnection) As Integer
        ''
        '' 表の描画情報を読み込む
        ''
        Dim strSQL As String
        ''Dim DbCom As New OleDb.OleDbCommand
        ''Dim DbDr As OleDb.OleDbDataReader
        'Dim DbDa As OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DData")

        ReadimageInfoFromDB = 0

        strSQL = "Select * From メニュー基本情報 Where (項目名 = '" & imgInfo.GraphID & "')"              ''And 種別 = " & GraphType & ")"
        Using DbDa As New OleDb.OleDbDataAdapter(strSQL, dbCon)
            Using DtSet As New DataSet("DData")
                DbDa.Fill(DtSet, "DData")
                If DtSet.Tables("DData").Rows.Count > 0 Then
                    With DtSet.Tables("DData")
                        imgInfo.ContentsTitle = .Rows(0).Item(2)                '見出しタイトル
                        imgInfo.GraphTitle = .Rows(0).Item(3)                   'グラフタイトル
                        imgInfo.TitleSize = .Rows(0).Item(4)                    'グラフタイトルサイズ
                        imgInfo.TitlePosition = .Rows(0).Item(5)                'グラフタイトル位置
                        imgInfo.ImagePath = .Rows(0).Item(6)                    'イメージパス
                        imgInfo.DataFileNo = .Rows(0).Item(8)                   'データファイル番号
                        imgInfo.ShowDateTime = .Rows(0).Item(9)                 '最新データ日時の表示
                    End With
                End If
            End Using
        End Using
        ''DbCom = New OleDb.OleDbCommand(strSQL, dbCon)
        ''DbDr = DbCom.ExecuteReader
        ''With DbDr
        ''    If .HasRows = True Then
        ''        .Read()                                             '１レコード分読み込み
        ''        imgInfo.ContentsTitle = .GetString(2)               '見出しタイトル
        ''        imgInfo.GraphTitle = .GetString(3)                  'グラフタイトル
        ''        imgInfo.TitleSize = .GetInt32(4)                    'グラフタイトルサイズ
        ''        imgInfo.TitlePosition = .GetInt32(5)                'グラフタイトル位置
        ''        imgInfo.ImagePath = .GetString(6)                   'イメージパス
        ''        imgInfo.DataFileNo = .GetInt16(8)                   'データファイル番号
        ''        imgInfo.ShowDateTime = .GetBoolean(9)               '最新データ日時の表示
        ''    End If
        '' ''End With
        ''DbDr.Close()
        ''DbCom.Dispose()
        'DbDa.Dispose()
        'DtSet.Dispose()
        ReadimageInfoFromDB = 1

    End Function


    Protected Sub CheckAutoUpdate(ByVal SiteDirectory As String)

        Dim DataFile As String
        Dim intInterval As Integer '= 600000
        Dim blnAutoUpdate As Boolean '= False
        Dim clsUpdate As New ClsReadDataFile

        DataFile = Server.MapPath(SiteDirectory & "\App_Data\MenuInfo.mdb")
        clsUpdate.CheckAutoUpdate(DataFile, imgInfo.ContentsTitle, intInterval, blnAutoUpdate)

        'タイマーの有効／無効を設定
        Me.Tmr_Update.Enabled = blnAutoUpdate
        Me.Tmr_Update.Interval = intInterval

    End Sub

End Class