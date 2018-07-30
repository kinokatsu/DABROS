Imports System.IO
Imports System.Data

Partial Class imageUpload
    Inherits System.Web.UI.Page

    Dim mlngMaxFolderSize As Long
    Dim ULNo(5) As String
    Dim mainpicEnable As Boolean

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

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"

        ''認証されていない場合は、閉じる
        If User.Identity.IsAuthenticated = False Then
            Response.Write(strScript)
            'FormsAuthentication.RedirectFromLoginPage("vb", True)
            Exit Sub
        End If

        Dim LoginStatus As Integer = Session.Item("LgSt")          'ログインステータス

        ' ''ログインしていない場合は、閉じる
        If LoginStatus = 0 Then
            'FormsAuthentication.RedirectFromLoginPage("vb", True)
            Response.Redirect("sessionerror.aspx", False)
            ''Response.Write(strScript)
            Exit Sub
        End If

        ''実行権限がない場合は前のページに戻る
        Dim uLevel As Integer = CType(Session.Item("UL"), Integer)
        Dim userLevel As String = uLevel.ToString("000000")     'ユーザレベル
        Dim clsLevelCheck As New ClsCheckUser
        Dim strInt As Integer = clsLevelCheck.GetWord(userLevel, ULNo)
        '' Dim strScriptUL As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('実行する権限がありません');history.back();</script>" ';self.opener=self;setTimeout('self.close()',1);</script>"
        Dim strScriptUL As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"
        If ULNo(3) = "0" Then
            Response.Write(strScriptUL)
            Exit Sub
        End If

        '' ''■IE８対応　互換モード指定
        ''Dim xuac As HtmlMeta = New HtmlMeta()
        ''xuac.HttpEquiv = "X-UA-Compatible"
        ''xuac.Content = "IE=EmulateIE7"
        ''Header.Controls.AddAt(0, xuac)

        Response.Cache.SetCacheability(HttpCacheability.NoCache)                    ''キャッシュなしとする

        Dim siteName As String
        Dim siteDirectory As String = CType(Session.Item("SD"), String)             ''現場ディレクトリ
        Dim imgSendFile As String = Server.MapPath(siteDirectory & "\App_Data\ImageSend.mdb")
        'Dim mainpicEnable As Boolean
        Dim strTemp As String = ""

        siteName = CType(Session.Item("SN"), String)                                ''現場名
        Dim wid As String = CType(Request.Item("W"), String)
        Dim Hei As String = CType(Request.Item("H"), String)

        ''If IsPostBack = False Then                  'ポストバックではないとき

        ''If wid IsNot Nothing And Hei IsNot Nothing Then                                                                 ' 2012/08/20 Kino Add    
        ''    ''==== フォームのサイズを調整する ====
        ''    If Not Page.ClientScript.IsStartupScriptRegistered("javascript") Then
        ''        Dim OpenString As String

        ''        OpenString = "<SCRIPT LANGUAGE='javascript'>"
        ''        OpenString &= "window.resizeTo(" & wid & "," & Hei & ");"
        ''        OpenString &= "<" & "/SCRIPT>"
        ''        Page.ClientScript.RegisterStartupScript(Me.GetType(), "画像送信", OpenString)
        ''    End If
        ''End If

        If IsPostBack = False Then
            Using DbCon As New OleDb.OleDbConnection
                DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & imgSendFile & ";" & "Jet OLEDB:Engine Type= 5")
                'Dim DbDa As OleDb.OleDbDataAdapter
                'Dim DtSet As New DataSet("DData")
                Dim strSQL As String

                strSQL = ("SELECT * FROM メニュー基本情報")
                Using DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)
                    Using DtSet As New DataSet("DData")

                        DbDa.Fill(DtSet, "DData")
                        If DtSet.Tables("DData").Rows.Count <> 0 Then
                            mainpicEnable = Boolean.Parse(DtSet.Tables("DData").Rows(0).Item(1))
                            mlngMaxFolderSize = Long.Parse(DtSet.Tables("DData").Rows(0).Item(2))
                            strTemp = DtSet.Tables("DData").Rows(0).Item(3).ToString
                            ViewState("mainpicenb") = mainpicEnable
                        End If

                        'DbDa.Dispose()
                        'DtSet.Dispose()
                        'DbCon.Dispose()
                    End Using
                End Using
            End Using

            Me.lstImageFiles.Items.Clear()
            Dim imgPath As String = Server.MapPath(siteDirectory & "\img\")

            Dim di As New System.IO.DirectoryInfo(imgPath)
            Dim folderSize As Single = GetDirectorySize(di)
            folderSize *= 0.001                                                                         '' B -> kBに変換
            strTemp &= Environment.NewLine & String.Format("(現在のフォルダ容量は【{0}kB】です)", folderSize.ToString("#0.00"))

            If strTemp.Length <> 0 Then                                                                 '' 説明書き
                strTemp = strTemp.Replace(Environment.NewLine, "<br />")
                Me.LitExp.Text = strTemp
            End If

            Call SetImagefile2Listbox(imgPath)

            'Dim s As New System.Collections.Generic.List(Of String)()                                   '' フォルダ内の画像ファイル名を取得
            's.AddRange(System.IO.Directory.GetFiles(imgPath, "*.png"))
            's.AddRange(System.IO.Directory.GetFiles(imgPath, "*.jpg"))
            's.Sort()
            'Dim strFile As String() = s.ToArray()
            'Dim iloop As Integer
            ''Dim fileNames() As String
            ''strTemp = Nothing
            ''Dim ArrList As New ArrayList()

            'Me.lstImageFiles.Items.Add("新規ファイル")

            'For iloop = 0 To strFile.Length - 1                                                         ' 権限による除外ファイルを検出するため、コレクションに入れる
            '    If mainpicEnable = True Then
            '        '' 'strTemp &= strFile(iloop).Substring(strFile(iloop).LastIndexOf("\") + 1) & ","
            '        ''strTemp &= IO.Path.GetFileName(strFile(iloop)) & ","
            '        'ArrList.Add(IO.Path.GetFileName(strFile(iloop)))
            '        Me.lstImageFiles.Items.Add(IO.Path.GetFileName(strFile(iloop)))
            '    Else
            '        If strFile(iloop).ToLower.Contains("mainpic.png") = False Then                      ''メイン画像が許可されていない場合は意外とする
            '            ' ''strTemp &= strFile(iloop).Substring(strFile(iloop).LastIndexOf("\") + 1) & ","
            '            ''strTemp &= IO.Path.GetFileName(strFile(iloop)) & ","
            '            'ArrList.Add(IO.Path.GetFileName(strFile(iloop)))
            '            Me.lstImageFiles.Items.Add(IO.Path.GetFileName(strFile(iloop)))
            '        End If
            '    End If
            'Next

            'Me.lstImageFiles.Items.Add("新規ファイル")
            'If strTemp.Length <> 0 Then                                                                 '' 現在フォルダにあるファイル一覧を表示
            '    strTemp = strTemp.Substring(0, strTemp.Length - 1)
            '    fileNames = strTemp.Split(",")
            '    Array.Sort(fileNames)
            '    For iloop = 0 To fileNames.Length - 1
            '        Dim lstItem As New ListItem
            '        lstItem.Text = fileNames(iloop)
            '        Me.lstImageFiles.Items.Add(lstItem)
            '    Next
            'End If
        Else
            mainpicEnable = Convert.ToBoolean(ViewState("mainpicenb"))
        End If

        ' ''説明用ファイルを読込み
        ''Dim FilePath As String = Server.MapPath(siteDirectory & "\App_Data\imguplad.tmp")
        ''If IO.File.Exists(FilePath) = True Then
        ''    Dim TextFile As StreamReader
        ''    TextFile = New StreamReader(FilePath, System.Text.Encoding.GetEncoding("Shift_Jis"))
        ''    Dim strTemp As String = TextFile.ReadToEnd
        ''    TextFile.Close()

        ''    strTemp = strTemp.Replace(Environment.NewLine, "<br />")
        ''    Me.LitExp.Text = strTemp
        ''End If

        ''For Each FileName In Directory.GetFiles(imgPath, "*.png,*.jpg")           '指定フォルダ内の.pngファイルを検索する
        ''    If FileName.Length <> 0 Then
        ''        Dim lstItem As New ListItem
        ''        lstItem.Text = FileName.Substring(FileName.LastIndexOf("\") + 1)
        ''        lstItem.Value = dummy
        ''        Me.lstImageFiles.Items.Add(lstItem)
        ''        'Me.lstImageFiles.Items(dummy).Value = dummy
        ''        dummy += 1
        ''    End If
        ''Next
        ''Else
        Me.LblError.Text = ""
        'Me.LblMsg.Text = ""

        Me.BtnFileUpload.Attributes("OnClick") = "return confirm('ファイルを送信しますか？')"
        Me.MyTitle.Text = " [画像送信] - " & siteName

    End Sub


    Protected Sub BtnFileUpload_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnFileUpload.Click

        Dim siteDirectory As String = CType(Session.Item("SD"), String)           '現場ディレクトリ
        Dim FileName As String
        Dim strTemp As String
        Dim Ext As String
        Dim destFileName As String
        Dim FileSize As Long
        Dim saveFilePath As String

        Me.LblError.Text = ""
        Me.LblMsg.Text = ""

        strTemp = Me.FlUp.FileName                                  '送信するファイルパス

        If ULNo(1) = "0" And Me.FlUp.FileName.ToLower.Contains("mainpic.png") = True Then
            Me.LblError.Text = "その名称のファイルを送信する権限がありません。"
            Exit Sub
        End If

        FileSize = Me.FlUp.FileBytes.Length                         '送信するファイルのサイズ
        If Me.lstImageFiles.SelectedIndex <> -1 Then
            destFileName = Me.lstImageFiles.SelectedItem.Text       '保存するファイル名
            If destFileName = "新規ファイル" Then
                destFileName = ""
            End If
        Else
            destFileName = ""
        End If

        If FileSize > 4096000 Then
            Me.LblError.Text = "ファイルの容量が大きすぎます。送信できません。"
            Exit Sub
        End If

        If strTemp.Length = 0 Then
            Me.LblError.Text = "ファイルを指定してください。"
            Exit Sub
        End If

        If destFileName.Length <> 0 Then
            FileName = destFileName
        Else
            FileName = strTemp          '.Substring(strTemp.LastIndexOf("\"))
        End If

        Ext = FileName.Substring(FileName.LastIndexOf(".")).ToLower
        If Ext <> ".png" And Ext <> ".jpg" Then
            Me.LblError.Text = "画像ファイルは4MB以内のPNGもしくはJPEG形式としてください。"
            Exit Sub
        End If

        saveFilePath = IO.Path.Combine(Server.MapPath(siteDirectory), "img", FileName)
        Try
            If Me.FlUp.HasFile = True Then
                Me.FlUp.SaveAs(saveFilePath)
                'Me.LblMsg.Text = "　　「" + strTemp + "」 を 「" + FileName + "」 として送信しました。"
                Me.LblMsg.Text = String.Format("　「{0}」 を 「{1}」 として送信しました。", strTemp, FileName)
            End If
        Catch ex As Exception
            Me.LblMsg.Text = String.Format("　　{0} を送信できませんでした。", strTemp)
        End Try

        Dim imgPath As String = IO.Path.Combine(Server.MapPath(siteDirectory), "img")
        Dim di As New System.IO.DirectoryInfo(imgPath)
        Dim folderSize As Single = GetDirectorySize(di)

        If folderSize > 5120000 Then
            Me.LblMsg.Text = "フォルダの保存できる最大容量を超えたのでファイルを削除しました。<br />ファイルを送信したい場合は弊社担当者までご連絡ください"
            If IO.File.Exists(saveFilePath) = True Then
                IO.File.Delete(saveFilePath)
            End If
        End If

        ''画像追加になった場合に再取得する
        Call SetImagefile2Listbox(imgPath)

        ''If Me.lstImageFiles.SelectedIndex = -1 Or Me.lstImageFiles.SelectedIndex = 0 Then
        ''    Me.lstImageFiles.Items.Clear()

        ''    Dim s As New System.Collections.Generic.List(Of String)()
        ''    s.AddRange(System.IO.Directory.GetFiles(imgPath, "*.png"))
        ''    s.AddRange(System.IO.Directory.GetFiles(imgPath, "*.jpg"))
        ''    Dim strFile As String() = s.ToArray()

        ''    Dim iloop As Integer
        ''    For iloop = 0 To strFile.Length - 1
        ''        Dim lstItem As New ListItem
        ''        lstItem.Text = strFile(iloop).Substring(strFile(iloop).LastIndexOf("\") + 1)
        ''        lstItem.Value = iloop
        ''        Me.lstImageFiles.Items.Add(lstItem)
        ''    Next
        ''End If

    End Sub

    ''' <summary>
    ''' 指定フォルダ内の指定拡張子のファイルを取得して、ソート後にリストボックスに格納する　共通プロシージャー
    ''' </summary>
    ''' <param name="imgPath"></param>
    ''' <remarks></remarks>
    Private Sub SetImagefile2Listbox(imgPath As String)

        Dim s As New System.Collections.Generic.List(Of String)()                                   '' フォルダ内の画像ファイル名を取得
        s.AddRange(System.IO.Directory.GetFiles(imgPath, "*.png"))
        s.AddRange(System.IO.Directory.GetFiles(imgPath, "*.jpg"))
        s.Sort()

        Dim strFile As String() = s.ToArray()
        Dim iloop As Integer

        Me.lstImageFiles.Items.Clear()
        Me.lstImageFiles.Items.Add("新規ファイル")

        For iloop = 0 To strFile.Length - 1                                                         ' 権限による除外ファイルを検出するため、コレクションに入れる
            If mainpicEnable = True Then
                Me.lstImageFiles.Items.Add(IO.Path.GetFileName(strFile(iloop)))
            Else
                If strFile(iloop).ToLower.Contains("mainpic.png") = False Then                      ''メイン画像が許可されていない場合は意外とする
                    Me.lstImageFiles.Items.Add(IO.Path.GetFileName(strFile(iloop)))
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' フォルダのサイズを取得する
    ''' </summary>
    ''' <param name="dirInfo">サイズを取得するフォルダ</param>
    ''' <returns>フォルダのサイズ（バイト）</returns>
    Public Shared Function GetDirectorySize(ByVal dirInfo As DirectoryInfo) As Long

        Dim size As Long = 0

        'フォルダ内の全ファイルの合計サイズを計算する
        Dim fi As FileInfo
        For Each fi In dirInfo.GetFiles()
            size += fi.Length
        Next fi

        'サブフォルダのサイズを合計していく
        Dim di As DirectoryInfo
        For Each di In dirInfo.GetDirectories()
            size += GetDirectorySize(di)
        Next di

        '結果を返す
        Return size

    End Function

End Class
