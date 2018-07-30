Imports System.Net

Partial Class Login
    Inherits System.Web.UI.Page

    '    Const UserInformation_DatabaseFile As String = "C:\chkdb\UserAndPassword.mdb"

    Protected Sub Login1_Authenticate(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.AuthenticateEventArgs) Handles Login1.Authenticate

        Dim UserName As String
        Dim PassWord As String = Nothing
        Dim blnLogin As Boolean = False
        Dim blnAutoLogin As Boolean = False
        ''        Dim ChkUser As New Class1
        Dim SiteNo As String = Nothing
        ''Dim strEncKey As String
        Dim userLevel As Integer
        Dim clsAuth As New ClsCheckUser
        Dim AuthLimitDate As Date
        Dim RemoteInf As String = Nothing
        Dim usergroup As String = ""
        Dim ShowName As String = ""

        Dim AuthKey As String = Nothing                                             ' IoT用
        Dim IDNum As Long = -1
        Dim officeName As String = Nothing
        Dim IoTFLg As Boolean = False                                               ' IoTフラグ True：IoT水位計　　Flase：それ以外


        If Login1.UserName.ToLower.StartsWith("iot") Then
            IoTFLg = True
        End If

        Dim clsBr As New ClsHttpCapData
        RemoteInf = clsBr.GetRemoteInfo(Request, Server.MapPath(""))
        clsBr = Nothing

        ' ''-----------------------/ クエリにパスワード記載対応(確認程度) 2015/02/02 /----------------------- 
        ''Dim un As String = Nothing
        ''Dim pw As String = Nothing
        ''un = CType(Request.Item("A1"), String)
        ''pw = CType(Request.Item("A2"), String)
        ' ''-------------------------------------------------------------------------------------------------
        '---------------------------------/ OTパスワード対応 2015/04/28 /---------------------------------
        Dim un1 As String
        Dim hashVal As String
        Dim siteFolder As String
        un1 = CType(Request.Item("P1"), String)
        hashVal = CType(Request.Item("P2"), String)
        siteFolder = CType(Request.Item("SF"), String)

        '---------------------------------/ 隠しフィールド対応 2015/11/19 /---------------------------------
        Dim hiddenFlg As Integer = 0
        'DBからun1とhashValをキーにレコードを取得する。そこからMD5化されたパスワードを取得する。
        'あとは、取得したパスワードとの比較処理は同じ。
        '-------------------------------------------------------------------------------------------------

        '' データベースファイルが無ければ作成する
        If System.IO.File.Exists(clsAuth.UserInformation_DatabaseFile) = False Then Call clsAuth.Login_DatabaseMake()

        ''If un IsNot Nothing AndAlso pw IsNot Nothing Then                                                   ' クエリにパスワード記載対応(確認程度) 2015/02/02
        ''    '● クエリにパスワードの場合　基本的には非対応
        ''    UserName = un                                                                                   ' クエリにパスワード記載対応(確認程度) 2015/02/02
        ''    PassWord = pw
        ''    blnAutoLogin = True                                                                             ' 2015/05/08 Kino Add ダミー

        If un1 Is Nothing And hashVal Is Nothing Then
            '●通常ログインの場合と隠しフィールドの場合
            UserName = Login1.UserName.ToString
            ''strEncKey = Now().Month & Now().Day & Now().DayOfWeek                               '暗号化キーを作成
            ''PassWord = clsAuth.EncryptString(Login1.Password.ToString, strEncKey)               'パスワードを暗号化　はサーバ内だけの処理でネットワークには平文で流れるのでコメント

            PassWord = Login1.Password.ToString
            blnAutoLogin = True                                                                             ' 2015/05/08 Kino Add ダミー

            '/---------------------------------  Hiddenフィールドに記載があった場合の処理 2015/11/18
            Dim hidePW As String = CType(Request.Form.Get("PD"), String)
            If PassWord.Length = 0 AndAlso hidePW IsNot Nothing Then
                hiddenFlg = 1
                PassWord = hidePW
            End If
            '--------------------------------------------------------------------------------------/

        Else
            '●ONE TIME PASSWORD の場合
            UserName = un1
            blnAutoLogin = clsAuth.SessionDataOTP_Check(un1, hashVal, PassWord)                                 '登録ユーザとパスワードの確認 2015/05/08 Kino Add

        End If

        If blnAutoLogin = True Then
            If IoTFLg = True Then
                blnLogin = clsAuth.SessionDataIoT_Check(UserName, PassWord, userLevel, AuthLimitDate, usergroup, IDNum, officeName, hiddenFlg)     ' IoT水位計用
            Else
                blnLogin = clsAuth.SessionData_Check(UserName, PassWord, SiteNo, userLevel, AuthLimitDate, usergroup, ShowName, hiddenFlg)                  '登録ユーザとパスワードの確認 2015/11/19 Kino Changed  Add hiddenFlg
            End If
        End If

        If blnLogin = False Then                                                            '登録ユーザがいない

            e.Authenticated = False
            Me.LblQuery.Text = "「ユーザー名」または「パスワード」をお忘れになられた場合は、計測担当者までご連絡ください。"
            Me.LblQuery.Visible = True

        Else                                                                                '登録ユーザがいた場合

            e.Authenticated = True
            Login1.RememberMeSet = False
            Me.LblQuery.Visible = True
            If userLevel = 999999 Then                                                        '管理者の場合

                Me.Login1.DestinationPageUrl = "~/Admin.aspx"

            Else                                                                            'それ以外のユーザの場合

                If IoTFLg = False Then
                    Dim strUL(5) As String                                                      'ユーザ権限で携帯のみの場合の対応
                    Dim intTemp As Integer = clsAuth.GetWord(userLevel.ToString, strUL)
                    If Convert.ToInt32(strUL(2)) = 1 And Convert.ToInt32(strUL(1)) = 0 And Convert.ToInt32(strUL(0)) = 0 Then
                        e.Authenticated = False
                        With Me.LblQuery
                            .Text = "そのユーザ名は携帯電話からのアクセスのみ利用可能です。"
                            .Font.Bold = True
                            .ForeColor = Drawing.Color.Blue
                            .Visible = True
                        End With
                        GoTo NoOpe
                    End If
                End If

                ''FormsAuthentication.RedirectFromLoginPage(UserName, False) フォーム認証の場合のクッキー処理
                If DateTime.Now() <= AuthLimitDate Then



                    e.Authenticated = True
                    Session.Add("UN", UserName)                         'ユーザ名
                    Session.Add("PW", PassWord)                         'パスワード
                    Session.Add("LgSt", 1)                              'ログインステータス
                    Session.Add("Br", "PC")                             'ブラウザ種類
                    Session.Add("UL", userLevel)                        'ユーザレベル
                    Session.Add("UG", usergroup)                        'ユーザグループ

                    If IoTFLg = False Then

                        Session.Add("SNo", SiteNo)                          '登録されている現場番号
                        Session.Add("TT", ShowName)                         '表示タイトル
                        Me.Login1.DestinationPageUrl = "~/SiteSelect.aspx"

                    Else

                        Session.Add("AKEY", AuthKey)                        '認証キー
                        Session.Add("IDNm", IDNum)                          '識別番号
                        Session.Add("SD", "IoT")                            '現場ディレクトリ
                        Session.Add("SN", officeName)                       '事務所名

                        Me.Login1.DestinationPageUrl = "~/MainRiver.aspx"

                    End If


                Else

                    e.Authenticated = False
                    Me.LblQuery.Text = "入力された「ユーザー名」は認証期限を過ぎております。引き続き閲覧される場合は、計測担当者までご連絡ください。"
                    Me.LblQuery.Visible = True

                End If



NoOpe:
                ''End Select
            End If

        End If
        ''Call Login_DatabaseComp(UserName, PassWord)

        clsAuth = Nothing

        ''Dim jsEnable As Boolean = clsJavaScriptEnvCheck.IsEnableJavaScript(Me.Request)          '' Javascript有効確認         ' 2012/08/31 Kino Changed htmlで対応

        ''If jsEnable = False AndAlso Me.PnlNotice01.Visible = False Then
        ''    Me.PnlNotice01.Visible = True
        ''End If

    End Sub

    Protected Sub Page_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error                  ' 2011/05/30 Kino Add

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

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ''
        '' ページが読み込まれたときに、ブラウザの種類を判別して、IEでなければモバイル用のログイン画面へ遷移させる
        ''

        If IsPostBack = False Then
            Dim myBrowserCaps As System.Web.HttpBrowserCapabilities = Request.Browser           '' Javascript有効確認
            Dim strBrowserType As String
            Dim browserCheck As Integer = 0                                                     '' 2011/07/15 Kino Add
            Dim MobileDev As Boolean = myBrowserCaps.IsMobileDevice                             '' 2018/06/27 Kino Add  モバイルデバイスの判定
            Dim ua As String = HttpContext.Current.Request.UserAgent.ToString

            strBrowserType = (CType(myBrowserCaps, System.Web.Configuration.HttpCapabilitiesBase)).Browser.ToUpper()
            browserCheck = strBrowserType.IndexOf("IE") + strBrowserType.IndexOf("NETSCAPE") + strBrowserType.IndexOf("MOZILLA") +
                            strBrowserType.IndexOf("FIREFOX") + strBrowserType.IndexOf("APPLEMAC-SAFARI") + strBrowserType.IndexOf("OPERA") +
                            strBrowserType.IndexOf("CHROME")

            Try
                If IO.File.Exists(Server.MapPath("~/accTestLogOut.flg")) = True Then                                                                 '2012/08/03 Kino Add アクセステストの場合に稼働する            
                    Using sw As New IO.StreamWriter(Server.MapPath("~/acctest.log"), True, Encoding.GetEncoding("Shift_JIS"))
                        sw.WriteLine(Now().ToString + "," + strBrowserType.ToString + "," + browserCheck.ToString + ",Page_Load")
                    End Using
                End If
            Catch ex As Exception

            End Try

            'スマートフォンの判定（タブレットを除く）
            If MobileDev = True Then
                If String.IsNullOrEmpty(ua) Then
                    MobileDev = False
                Else
                    MobileDev = ua.Contains("iPhone") OrElse (ua.Contains("Android") AndAlso ua.Contains("Mobile"))
                End If
            End If

            If browserCheck = -7 Then
                ''■携帯用ページへ遷移
                ''Dim wc As webclient = New webclient()
                ''Dim nvc As NameValueCollection = New NameValueCollection
                ''nvc.Add("BR", HttpUtility.UrlEncode(strBrowserType))
                ''nvc.Add("No", "")
                ''nvc.Add("TY", "")
                ''Request.ServerVariables("QUERY_STRING") 
                ''wc.QueryString = nvc
                ''KDDI-SH38 UP.Browser/6.2_7.2.7.1.K.3.330 (GUI) MMP/2.0
                'Dim BlNo As Integer = CType(Request.Item("BN"), Integer)
                ''Dim pointType As String = CType(Request.QueryString("TY"), String)
                ''If pointType = Nothing Then pointType = "N"
                Dim DataTime As String = CType(Request.QueryString("DT"), String)
                Dim ChNo As Integer = CType(Request.Item("CH"), Integer)
                If DataTime = Nothing Then DataTime = "0000000000"
                Dim txtPath As String = Server.MapPath("~/mobile.log")
                Dim DenyCheck As New ClsDenyList
                Dim DenyOpe As Boolean = DenyCheck.ReadDenyList(Server.MapPath("~/Deny.list"), HttpContext.Current.Request.UserAgent)   ' アクセス拒否リストに記載されているUserAgentの場合Googleへ飛ばす
                If DenyOpe = True Then
                    Response.Redirect("https://www.google.com/", False)
                    Exit Sub
                End If
                DenyCheck = Nothing
                ''Dim txtfile As IO.StreamWriter
                ''txtfile = IO.File.AppendText(txtPath)
                ''txtfile.Write(Now().ToString & "," & HttpContext.Current.Request.UserAgent & "," & strBrowserType & vbCr)
                ''txtfile.Close()
                ''Response.Redirect("mobile.aspx?BR=" + Server.UrlEncode(strBrowserType) + "&BN=" + Server.UrlEncode(BlNo) + "&TY=" + Server.UrlEncode(pointType) + "&DT=" + Server.UrlEncode(DataTime))
                ''Response.Redirect("mobile.aspx?BN=" + Server.UrlEncode(BlNo) + "&TY=" + Server.UrlEncode(pointType) + "&DT=" + Server.UrlEncode(DataTime))
                'Server.Transfer("mobile.aspx?CH=" + Server.UrlEncode(ChNo) + "&DT=" + Server.UrlEncode(DataTime))      ' 2012/10/18 Kino Changed 非対応のためコメント
                ''Response.Redirect("mobile.aspx?CH=" + Server.UrlEncode(ChNo) + "&DT=" + Server.UrlEncode(DataTime))
            End If
            ' アクセス元IP制限 2018/03/30 Kino Add
            Dim RefIP As String = Request.UserHostAddress   'HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
            Dim DenyIPCheck As New ClsDenyList
            Dim DenyIPOpe As Boolean = DenyIPCheck.CheckDenyIP(Server.MapPath("~/DenyIP.list"), RefIP)   ' アクセス拒否IPリストに記載されているUserAgentの場合Googleへ飛ばす
            If DenyIPOpe = True Then
                Response.Redirect("https://www.google.com/", False)
                Exit Sub
            End If
            DenyIPCheck = Nothing

            ''If strBrowserType <> "IE" Then Me.PnlFavorit.Visible = False ' 2011/05/24 Kino Add

            Dim SiteNo As Integer                   '現場番号
            Dim NoteView As Integer

            SiteNo = CType(Request.Item("SN"), Integer)                             'クエリ文字列で現場番号がきたら、セッション情報に保存する
            NoteView = CType(Request.Item("NV"), Integer)                           '現場選択で一度止まるかどうか
            Session.Add("AutoSiteNo", SiteNo)
            Session.Add("NoteView", NoteView)

            'パスワードとユーザ名のオートコンプリートを解除する
            Dim PWtxt As TextBox = CType(Me.Login1.FindControl("Password"), TextBox)
            PWtxt.Attributes.Add("autocomplete", "off")
            Dim UNtxt As TextBox = CType(Me.Login1.FindControl("UserName"), TextBox)
            UNtxt.Attributes.Add("autocomplete", "off")

            '/------------------------ Hiddenフィールドに記載があった場合の処理 2015/11/18
            Dim hideUN As String = CType(Request.Form.Get("UN"), String)            ' .ToString にしたが、パラメータがない場合エラーになるので、CTypeでキャストした
            Dim hidePW As String = CType(Request.Form.Get("PD"), String)
            Dim hidesiteFolder As String = CType(Request.Form.Get("SD"), String)
            '-------------------------------------------------------------------------------/

            '/----------------------- OTP対応   2015/02/02 
            Dim un As String = CType(Request.Item("P1"), String)
            Dim pw As String = CType(Request.Item("P2"), String)
            Dim siteFolder As String = CType(Request.Item("SF"), String)
            '------------------------------------------------------------------------/

            '/------------------------ Hiddenフィールドに記載があった場合の処理 2015/11/18 この順番には意味がある
            If un Is Nothing AndAlso hideUN IsNot Nothing Then un = hideUN
            If pw Is Nothing AndAlso hidePW IsNot Nothing Then pw = hidePW
            If siteFolder Is Nothing AndAlso hidesiteFolder IsNot Nothing Then siteFolder = hidesiteFolder
            '-----------------------------------------------------------------------------/

            ' セッション変数に保存しないようにする 2018/03/30 Kino
            ''If Session("RemoteInfo") Is Nothing Then                                                    ' 2017/01/12 Kino Changed RenderCompleteからこっちに移動
            ''    Call GetRemoteInfo()
            ''End If

            If un IsNot Nothing AndAlso pw IsNot Nothing AndAlso siteFolder IsNot Nothing Then

                Dim eTemp As New System.Web.UI.WebControls.AuthenticateEventArgs                        'クエリにパスワード記載対応(確認程度)
                Me.Login1.UserName = un.ToString
                Call Login1_Authenticate(Me.Login1, eTemp)

                If eTemp.Authenticated = True Then                                                      ' 2015/04/28 Kino Add ログインができていれば、認証チケットを生成して認証が通っているような振る舞いにする
                    ''デバッグ用
                    ''Using sw As New IO.StreamWriter(Server.MapPath("~/errorSummary.log"), True, Encoding.GetEncoding("Shift_JIS"))
                    ''    sw.WriteLine(Now.ToString & " Sessio Timeout=" & Session.Timeout)
                    ''End Using
                    Dim userData As String = "ApplicationSpecific data for this user."
                    Dim fromAuthTicket As New FormsAuthenticationTicket(1, un, DateTime.Now, DateTime.Now.AddMinutes(Session.Timeout), False, userData, FormsAuthentication.FormsCookiePath)
                    Dim cookie As HttpCookie = New HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(fromAuthTicket))
                    Response.Cookies.Add(cookie)

                    '' デバッグ用
                    ''Dim decode As FormsAuthenticationTicket
                    ''decode = FormsAuthentication.Decrypt(cookie.Value)
                    Response.Redirect("SiteSelect.aspx", False)
                End If

                Exit Sub
                'Protected Sub LoginButton_Click(ByVal sender As Object, ByVal e As System.EventArgs)
                'End Sub

            End If
            '------------------------------------------------------------------------------------------------

            ''JAVA Scriptの確認
            ''If myBrowserCaps.EcmaScriptVersion.Major >= 1 Then
            ''    Me.PnlNotice01.Visible = False
            ''    Me.ImgSpace.Visible = False
            ''End If

        End If

        'If IO.File.Exists(Server.MapPath("~/flg_js_show.flg")) = False Then
        '    Me.PnlNotice01.Visible = False
        'End If

        ''If IO.File.Exists(Server.MapPath("~/flg_css_show.flg")) = False Then
        ''    Me.PnlNotice02.Visible = False
        ''End If

        '' '' クッキーレスモードの確認
        ''If Session.IsCookieless = False And IO.File.Exists(Server.MapPath("~/cookie_show.flg")) = False Then
        ''    Me.PnlNotice03.Visible = False
        ''    'Me.ImgSpace2.Visible = False
        ''End If

        '' '' ブラウザ設定について
        ''If IO.File.Exists(Server.MapPath("~/flg_advice_show.flg")) = False Then
        ''    Me.Panel2.Visible = False
        ''End If

        '' '' お気に入り
        ''If IO.File.Exists(Server.MapPath("~/flg_favorit_show.flg")) = False Then
        ''    Me.PnlFavorit.Visible = False
        ''End If

        ''
        If IO.File.Exists(Server.MapPath("~/flg_notice_show.flg")) = False Then
            Me.TB_ExpBors.Visible = False
        End If

        ''Response.AddHeader("Expires", "Mon, 26 Jul 1997 05:00:00 GMT")
        ''Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate")
        ''Response.AddHeader("Pragma", "no-cache")
        ''<meta http-equiv="Expires" content="Mon, 26 Jul 1997 05:00:00 GMT" /> 
        ''<meta http-equiv="Cache-Control" content="no-store, no-cache,must-revalidate" /> 
        ''<meta http-equiv="Pragma" content="no-cache" /> 

        ''<configuration>
        ''   <system.web>7
        ''      <httpRuntime maxRequestLength="8096" />
        ''    </system.web>
        ''</configuration>

        Dim txtCopyright As String = Server.MapPath("~/flg_copyright.flg")
        If IO.File.Exists(txtCopyright) = True Then
            Me.lblCopyright.Text = "<br />DABROS Ver 2.10<br />Copyright &copy; since 2008 SAKATA DENKI Co.,Ltd. All Rights Reserved."
            Me.lblCopyright.Visible = True
        Else
            Me.lblCopyright.Visible = False
        End If

        'Page.SetFocus(Login1)       'Login1.Focus()    これは使えなくなったようだ・・


    End Sub

#Region "別プロシージャ化したため、RenderCompleteのタイミングではなく、Form_Loadから呼び出すことにした"

    ''Protected Sub Page_PreRenderComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRenderComplete

    ' 2017/01/12 Kino Changed 　ユーザー情報の取得を別プロシージャとした（他から呼び出せるように）
    ''
    '' ユーザ情報の取得　　画面のレンダリングが完了した後のイベント(画面表示前)
    ''
    ''Dim myBrowserCaps As System.Web.HttpBrowserCapabilities
    ''Dim RemoteADDR As String
    ''Dim RemoteHOST As String
    ''Dim strTemp As String
    ''Dim ua As String
    '' '' Dim userAgent As String = Request.Browser.Capabilities("").ToString()
    ''Dim browserName As String = ""
    ''Dim browserVer As String = ""
    ''Dim userAgent() As String = {}

    ''Try                                                                                     ' 2012/08/03 Kino Add 例外処理
    ''    ua = Request.UserAgent.ToString                                                     ' 2011/07/27 Kino Add ↓
    ''    userAgent = ua.Split(" ")
    ''    myBrowserCaps = Request.Browser

    ''    If IsPostBack = False Then

    ''        Try
    ''            If ua.IndexOf("Firefox") > -1 Then
    ''                browserName = "Firefox"
    ''                browserVer = userAgent(6).Substring(userAgent(6).IndexOf("/") + 1)

    ''            ElseIf ua.IndexOf("Chrome") > -1 Then
    ''                browserName = "Chrome"
    ''                browserVer = userAgent(8).Substring(userAgent(8).IndexOf("/") + 1)

    ''            ElseIf ua.IndexOf("Safari") > -1 Then
    ''                browserName = "Safari"
    ''                browserVer = userAgent(8).Substring(userAgent(8).IndexOf("/") + 1)

    ''            ElseIf ua.IndexOf("Opera") > -1 Then
    ''                browserName = "Opera"
    ''                browserVer = userAgent(7).Substring(userAgent(7).IndexOf("/") + 1)

    ''            ElseIf ua.IndexOf("Sleipnir") > -1 Then
    ''                browserName = "Sleipnir"
    ''                Dim shtTemp As Short = userAgent(40).IndexOf("/") + 1
    ''                Dim shtTemp2 As Short = userAgent(40).IndexOf(")")
    ''                browserVer = userAgent(40).Substring(shtTemp, (shtTemp2 - shtTemp))

    ''            Else        '  IEなど
    ''                browserName = myBrowserCaps.Browser.ToString
    ''                browserVer = myBrowserCaps.Version.ToString
    ''            End If
    ''        Catch
    ''            browserVer = myBrowserCaps.Version.ToString
    ''        End Try                                                                                 ' 2011/07/27 Kino Add ↑

    ''        Try                                                                                     ' 2012/08/03 Kino Add 例外処理
    ''            RemoteADDR = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
    ''        Catch ex As Exception
    ''            RemoteADDR = "Error"
    ''        End Try

    ''        Try                                                                                     ' 2012/08/03 Kino Add 例外処理
    ''            RemoteHOST = System.Net.Dns.GetHostEntry(RemoteADDR).HostName
    ''        Catch ex As Exception
    ''            RemoteHOST = "Error"
    ''        End Try

    ''        ' strTemp = RemoteADDR + "," + RemoteHOST + "," + myBrowserCaps.Browser + " " + myBrowserCaps.Version + " " + myBrowserCaps.Browsers.Item(2) + "," + myBrowserCaps.Platform     ' 2011/07/27 Kino Changed       
    ''        strTemp = RemoteADDR + "," + RemoteHOST + "," + browserName + " " + browserVer + " " + myBrowserCaps.Browsers.Item(2) + "," + myBrowserCaps.Platform                            ' 2011/07/27 Kino Add   

    ''        If IO.File.Exists(Server.MapPath("~/accTestLogOut.flg")) = True Then                                                                    '2012/08/03 Kino Add アクセステストの場合に稼働する
    ''            Dim strBrowserType As String = (DirectCast(myBrowserCaps, System.Web.Configuration.HttpCapabilitiesBase)).Browser.ToUpper()
    ''            Using sw As New IO.StreamWriter(Server.MapPath("~/acctest.log"), True, Encoding.GetEncoding("Shift_JIS"))
    ''                sw.WriteLine(Now().ToString + "," + HttpContext.Current.Request.UserAgent + "," + strBrowserType + "," + HttpContext.Current.Request.Browser.Id + "," + _
    ''                            HttpContext.Current.Request.Browser.Capabilities.Item("type") + "," + HttpContext.Current.Request.Browser.Capabilities.Item("deviceID") + "," + _
    ''                            myBrowserCaps.MobileDeviceModel.ToString + ",H:" + myBrowserCaps.ScreenPixelsHeight.ToString + ",W:" + myBrowserCaps.ScreenPixelsWidth.ToString _
    ''                             + "■" + strTemp + ",PreRenderComplete")
    ''            End Using
    ''        End If

    ''        Session.Add("RemoteInfo", strTemp)
    ''    End If

    ''Catch ex As Exception
    ''    If IO.File.Exists(Server.MapPath("~/accTestLogOut.flg")) = True Then                                                                        '2012/08/03 Kino Add 例外処理
    ''        Using sw As New IO.StreamWriter(Server.MapPath("~/errorSummary.log"), True, Encoding.GetEncoding("Shift_JIS"))
    ''            sw.WriteLine(Now.ToString + ex.StackTrace + vbCr)
    ''        End Using
    ''    End If

    ''End Try

    ''End Sub

#End Region

#Region "別クラスにして、セッション変数をやめたのでコメント - GetRemoteInfo"
    '''' <summary>
    '''' アクセスしてきたユーザーの情報を取得する（環境変数の取得とセッション変数への格納）
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub GetRemoteInfo()

    '    ''
    '    '' ユーザ情報の取得　　画面のレンダリングが完了した後のイベント(画面表示前)
    '    ''
    '    Dim myBrowserCaps As System.Web.HttpBrowserCapabilities
    '    Dim RemoteADDR As String
    '    Dim RemoteHOST As String
    '    Dim strTemp As String
    '    Dim ua As String
    '    '' Dim userAgent As String = Request.Browser.Capabilities("").ToString()
    '    Dim browserName As String = ""
    '    Dim browserVer As String = ""
    '    Dim userAgent() As String = {}

    '    Try                                                                                     ' 2012/08/03 Kino Add 例外処理
    '        ua = Request.UserAgent.ToString                                                     ' 2011/07/27 Kino Add ↓
    '        userAgent = ua.Split(" ")
    '        myBrowserCaps = Request.Browser

    '        If IsPostBack = False Then

    '            Try
    '                If ua.IndexOf("Firefox") > -1 Then
    '                    browserName = "Firefox"
    '                    browserVer = userAgent(6).Substring(userAgent(6).IndexOf("/") + 1)

    '                ElseIf ua.IndexOf("Chrome") > -1 Then
    '                    browserName = "Chrome"
    '                    browserVer = userAgent(8).Substring(userAgent(8).IndexOf("/") + 1)

    '                ElseIf ua.IndexOf("Safari") > -1 Then
    '                    browserName = "Safari"
    '                    browserVer = userAgent(8).Substring(userAgent(8).IndexOf("/") + 1)

    '                ElseIf ua.IndexOf("Opera") > -1 Then
    '                    browserName = "Opera"
    '                    browserVer = userAgent(7).Substring(userAgent(7).IndexOf("/") + 1)

    '                ElseIf ua.IndexOf("Sleipnir") > -1 Then
    '                    browserName = "Sleipnir"
    '                    Dim shtTemp As Short = userAgent(40).IndexOf("/") + 1
    '                    Dim shtTemp2 As Short = userAgent(40).IndexOf(")")
    '                    browserVer = userAgent(40).Substring(shtTemp, (shtTemp2 - shtTemp))

    '                Else        '  IEなど
    '                    browserName = myBrowserCaps.Browser.ToString
    '                    browserVer = myBrowserCaps.Version.ToString
    '                End If
    '            Catch
    '                browserVer = myBrowserCaps.Version.ToString
    '            End Try                                                                                 ' 2011/07/27 Kino Add ↑

    '            Try                                                                                     ' 2012/08/03 Kino Add 例外処理
    '                RemoteADDR = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
    '            Catch ex As Exception
    '                RemoteADDR = "Error"
    '            End Try

    '            Try                                                                                     ' 2012/08/03 Kino Add 例外処理
    '                RemoteHOST = System.Net.Dns.GetHostEntry(RemoteADDR).HostName
    '            Catch ex As Exception
    '                RemoteHOST = "Error"
    '            End Try

    '            ' strTemp = RemoteADDR + "," + RemoteHOST + "," + myBrowserCaps.Browser + " " + myBrowserCaps.Version + " " + myBrowserCaps.Browsers.Item(2) + "," + myBrowserCaps.Platform     ' 2011/07/27 Kino Changed       
    '            strTemp = RemoteADDR + "," + RemoteHOST + "," + browserName + " " + browserVer + " " + myBrowserCaps.Browsers.Item(2) + "," + myBrowserCaps.Platform                            ' 2011/07/27 Kino Add   

    '            If IO.File.Exists(Server.MapPath("~/accTestLogOut.flg")) = True Then                                                                    '2012/08/03 Kino Add アクセステストの場合に稼働する
    '                Dim strBrowserType As String = (DirectCast(myBrowserCaps, System.Web.Configuration.HttpCapabilitiesBase)).Browser.ToUpper()
    '                Using sw As New IO.StreamWriter(Server.MapPath("~/acctest.log"), True, Encoding.GetEncoding("Shift_JIS"))
    '                    sw.WriteLine(Now().ToString + "," + HttpContext.Current.Request.UserAgent + "," + strBrowserType + "," + HttpContext.Current.Request.Browser.Id + "," + _
    '                                HttpContext.Current.Request.Browser.Capabilities.Item("type") + "," + HttpContext.Current.Request.Browser.Capabilities.Item("deviceID") + "," + _
    '                                myBrowserCaps.MobileDeviceModel.ToString + ",H:" + myBrowserCaps.ScreenPixelsHeight.ToString + ",W:" + myBrowserCaps.ScreenPixelsWidth.ToString _
    '                                 + "■" + strTemp + ",PreRenderComplete")
    '                End Using
    '            End If

    '            Session.Add("RemoteInfo", strTemp)
    '        End If

    '    Catch ex As Exception
    '        If IO.File.Exists(Server.MapPath("~/accTestLogOut.flg")) = True Then                                                                        '2012/08/03 Kino Add 例外処理
    '            Using sw As New IO.StreamWriter(Server.MapPath("~/errorSummary.log"), True, Encoding.GetEncoding("Shift_JIS"))
    '                sw.WriteLine(Now.ToString + ex.StackTrace + vbCr)
    '            End Using
    '        End If

    '    End Try

    'End Sub
#End Region

    ''Protected Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click

    ''    Dim jsEnable As Boolean = clsJavaScriptEnvCheck.IsEnableJavaScript(Me.Request)

    ''    If jsEnable = True Then
    ''        Me.PnlNotice01.Visible = False
    ''        Me.ImgSpace.Visible = False
    ''    End If

    ''End Sub

End Class
