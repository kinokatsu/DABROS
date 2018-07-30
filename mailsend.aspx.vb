
Partial Class mailsend
    Inherits System.Web.UI.Page

    ''' <summary>
    ''' [イベント]ページエラー発生時
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
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
            Dim strScript As String = "<html><head><title>タイムアウト</title></head><body>接続タイムアウトになりました。</body></html>" + "<script language=javascript>alert('画面を閉じて再表示してください。');window.close();</script>"
            Response.Write(strScript)
        End If

    End Sub

    ''' <summary>
    ''' [イベント]　送信ボタン　押下
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub btnSubmit_Click(sender As Object, e As System.EventArgs) Handles btnSubmit.Click

        If CustomValidator1.IsValid = True Then                 '検証結果がTrueの時は送信

            Dim strFilePath As String = "D:\MailFolder"
            Dim MailPath As String = ""
            Dim mes(2) As String
            Dim GenbaName As String = Me.ConstructionSite.Text.Trim
            Dim StaffName As String = Me.Name.Text.Trim
            Dim MailAdd As String = Me.EMailAddress.Text.Trim
            Dim Opinion As String = Me.RqText.Text.Trim
            Dim SendType As String = Nothing
            Dim strTemp As New StringBuilder

            If Me.RdbList_Type.SelectedValue = 0 Then
                SendType = "ご意見・ご要望"
            Else
                SendType = "不具合報告"
            End If

            '---送信者への確認メール
            Try
                MailPath = IO.Path.Combine(strFilePath, String.Format("{0}_FormMail_Confirm.mail", DateTime.Now().ToString("yyMMdd_HHmmss")))

                mes(0) = "subject:DABROS Mail（確認用）"
                mes(1) = String.Format("address:to:{0} 様:{1}", StaffName, MailAdd)

                strTemp.AppendLine("")
                strTemp.AppendLine("日頃、弊社製品をご愛顧いただきましてまことにありがとうございます。")
                strTemp.AppendLine("")
                strTemp.AppendLine(SendType & "の送信ありがとうございます。")
                strTemp.AppendLine("下記内容でメールを受け付けさせていただきました。")
                strTemp.AppendLine("")
                strTemp.AppendLine("皆様方の貴重なご意見を今後の品質向上に役立たせて参りたいと考えておりますので、今後ともよろしくお願いいたします。")
                strTemp.AppendLine("")
                strTemp.AppendLine("=== 内容 =========")
                strTemp.AppendLine("")
                strTemp.AppendLine("種別:" & SendType)
                strTemp.AppendLine(Opinion)

                mes(2) = ("body:" & strTemp.ToString)

                writeTextFile(MailPath, String.Join(Environment.NewLine, mes))
            Catch ex As Exception

            End Try


            '---坂田電機へのメール
            Try
                Dim userName As String = CType(Session.Item("UN"), String)
                Dim siteDirectory As String = CType(Session.Item("SD"), String)
                Dim ua As String = Request.UserAgent.ToString
                Dim RemoteADDR As String = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
                Dim RemoteHOST As String = System.Net.Dns.GetHostEntry(RemoteADDR).HostName
                Dim browser As String = Request.Browser.ToString
                Dim AddInfo As New StringBuilder

                MailPath = IO.Path.Combine(strFilePath, String.Format("{0}_FormMail.mail", DateTime.Now().ToString("yyMMdd_HHmmss")))

                mes(0) = "subject:DABROS Mail"
                mes(1) = String.Format("address:to:木ノ嶋:katsuyuki.kinoshima@sakatadenki.co.jp")

                AddInfo.AppendLine("")
                AddInfo.AppendLine("Rmote Address:" & RemoteADDR)
                AddInfo.AppendLine("Remote Host:" & RemoteHOST)
                AddInfo.AppendLine("User Name:" & userName)
                AddInfo.AppendLine("Site Directory:" & siteDirectory)
                AddInfo.AppendLine("種別:" & SendType)
                AddInfo.AppendLine("")
                AddInfo.AppendLine(Opinion)

                mes(2) = ("body:" & AddInfo.ToString)

                writeTextFile(MailPath, String.Join(Environment.NewLine, mes))

                AddInfo.Length = 0

            Catch ex As Exception

            End Try

            strTemp.Length = 0

            Response.Redirect("./mailthx.aspx", False)          ' 送信完了表示

        End If

    End Sub

    ''' <summary>
    ''' テキストファイルに保存する　　
    ''' </summary>
    ''' <param name="msg">ファイルに保存する文字列</param>
    ''' <remarks>文字列の最後に「，」があったら除去する</remarks>
    Private Sub writeTextFile(FilePath As String, Msg As String, Optional ByVal AppendFlg As Boolean = True)        'ByVal TimeStamp As DateTime)

        'Dim filePath As String = IO.Path.Combine(sysInf.systemPath, "StatusCheckTime.txt")

        Try
            Using sw As New IO.StreamWriter(FilePath, AppendFlg, System.Text.Encoding.GetEncoding("Shift_JIS"))
                Msg = Msg.TrimEnd(Convert.ToChar(","))
                sw.WriteLine(Msg)
            End Using
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' [イベント]ページロード時
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"

        If User.Identity.IsAuthenticated = False Then
            'Response.Redirect("Login.aspx",false)
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer
        LoginStatus = CType(Session.Item("LgSt"), Integer)          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            'Response.Redirect("Login.aspx",false)
            Response.Write(strScript)
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする
        Response.Cache.SetNoStore()
        Response.Cache.SetExpires(DateTime.Now.AddDays(-1))

        If IsPostBack = False Then
            Dim strDt As String = DateTime.Now.Ticks.ToString
            Session("sendtime") = strDt
            ViewState("sendtime") = strDt
        End If

    End Sub

    ''' <summary>
    ''' [イベント］Load イベント終了後の、検証処理
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="args"></param>
    ''' <remarks></remarks>
    Protected Sub CustomValidator1_ServerValidate(source As Object, args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles CustomValidator1.ServerValidate

        Me.Application.Lock()
        Dim befreStamp As String = TryCast(Me.Session("sendtime"), String)
        Dim stamp As String = DateTime.Now.Ticks.ToString()
        Me.Session("sendtime") = stamp
        Me.Application.UnLock()
        Dim vStamp As String = TryCast(Me.ViewState("sendtime"), String)

        If befreStamp Is Nothing OrElse vStamp Is Nothing OrElse befreStamp.CompareTo(vStamp) <> 0 Then
            args.IsValid = False
        Else
            args.IsValid = True
            Me.ViewState("sendtime") = stamp
        End If

    End Sub

End Class
