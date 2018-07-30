'Imports Microsoft.VisualBasic

Public Class ClsHttpCapData

    ''' <summary>
    ''' アクセスしてきたユーザーの情報を取得する（環境変数の取得とセッション変数への格納）
    ''' </summary>
    ''' <remarks></remarks>
    Public Function GetRemoteInfo(ByVal Request As System.Web.HttpRequest, Optional ByVal WebMapPath As String = Nothing) As String

        Dim strRet As String = Nothing
        Dim myBrowserCaps As System.Web.HttpBrowserCapabilities
        Dim RemoteADDR As String = Nothing
        Dim RemoteHOST As String = Nothing
        Dim strTemp As String = Nothing
        Dim ua As String = Nothing
        Dim browserName As String = Nothing
        Dim browserVer As String = Nothing
        Dim userAgent() As String = Nothing


        'Dim myBrowserCaps As System.Web.HttpBrowserCapabilities = Request.Browser           '' Javascript有効確認
        'Dim strBrowserType As String
        'Dim browserCheck As Integer = 0                                                     '' 2011/07/15 Kino Add

        'strBrowserType = (CType(myBrowserCaps, System.Web.Configuration.HttpCapabilitiesBase)).Browser.ToUpper()

        Try                                                                                     ' 2012/08/03 Kino Add 例外処理
            ua = Request.UserAgent.ToString                                                     ' 2011/07/27 Kino Add ↓
            userAgent = ua.Split(" ")
            myBrowserCaps = Request.Browser

            Try
                If ua.IndexOf("Firefox") > -1 Then
                    browserName = "Firefox"
                    browserVer = userAgent(6).Substring(userAgent(6).IndexOf("/") + 1)

                ElseIf ua.IndexOf("Chrome") > -1 Then
                    browserName = "Chrome"
                    browserVer = userAgent(8).Substring(userAgent(8).IndexOf("/") + 1)

                ElseIf ua.IndexOf("Safari") > -1 Then
                    browserName = "Safari"
                    browserVer = userAgent(8).Substring(userAgent(8).IndexOf("/") + 1)

                ElseIf ua.IndexOf("Opera") > -1 Then
                    browserName = "Opera"
                    browserVer = userAgent(7).Substring(userAgent(7).IndexOf("/") + 1)

                ElseIf ua.IndexOf("Sleipnir") > -1 Then
                    browserName = "Sleipnir"
                    Dim shtTemp As Short = userAgent(40).IndexOf("/") + 1
                    Dim shtTemp2 As Short = userAgent(40).IndexOf(")")
                    browserVer = userAgent(40).Substring(shtTemp, (shtTemp2 - shtTemp))

                Else        '  IEなど
                    browserName = myBrowserCaps.Browser.ToString
                    browserVer = myBrowserCaps.Version.ToString
                End If
            Catch
                browserVer = myBrowserCaps.Version.ToString
            End Try                                                                                 ' 2011/07/27 Kino Add ↑

            Try                                                                                     ' 2012/08/03 Kino Add 例外処理
                RemoteADDR = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
            Catch ex As Exception
                RemoteADDR = "Error"
            End Try

            Try                                                                                     ' 2012/08/03 Kino Add 例外処理
                RemoteHOST = System.Net.Dns.GetHostEntry(RemoteADDR).HostName
            Catch ex As Exception
                RemoteHOST = "Error"
            End Try

            'strTemp = RemoteADDR + "," + RemoteHOST + "," + browserName + " " + browserVer + " " + myBrowserCaps.Browsers.Item(2) + "," + myBrowserCaps.Platform                            ' 2011/07/27 Kino Add   
            Dim sb0 As New StringBuilder
            sb0.Capacity = 100
            sb0.Append(RemoteADDR & ",")
            sb0.Append(RemoteHOST & ",")
            sb0.Append(browserName & " ")
            sb0.Append(browserVer & " ")
            sb0.Append(myBrowserCaps.Browsers.Item(2) & ",")
            sb0.Append(myBrowserCaps.Platform)
            strTemp = sb0.ToString
            sb0.Length = 0

            strRet = strTemp

            'アクセステストの場合に稼働する-------------
            If WebMapPath IsNot Nothing Then
                Dim FilePath As String = IO.Path.Combine(WebMapPath, "accTestLogOut.flg")

                If IO.File.Exists(FilePath) = True Then
                    Dim strBrowserType As String = (DirectCast(myBrowserCaps, System.Web.Configuration.HttpCapabilitiesBase)).Browser.ToUpper()
                    FilePath = IO.Path.Combine(WebMapPath, "acctest.log")
                    Using sw As New IO.StreamWriter(FilePath, True, Encoding.GetEncoding("Shift_JIS"))
                        Dim sb As New StringBuilder
                        sb.Capacity = 300
                        sb.Append(DateTime.Now.ToString & ",")
                        sb.Append(Request.UserAgent & ",")
                        sb.Append(strBrowserType & ",")
                        sb.Append(Request.Browser.Id & ",")
                        sb.Append(Request.Browser.Type & ",")
                        sb.Append(Request.Browser.Capabilities.Item("deviceID") & ",")
                        sb.Append(myBrowserCaps.MobileDeviceModel.ToString & ",")
                        sb.Append(String.Format("H:{0},W:{1},", myBrowserCaps.ScreenPixelsHeight.ToString, myBrowserCaps.ScreenPixelsWidth.ToString))
                        Dim st As New Diagnostics.StackTrace
                        Dim className As String = st.GetFrame(2).GetMethod.ReflectedType.Name           ' 1:自分のプロシージャ　2:呼び出し元プロシージャ
                        Dim methodName As String = st.GetFrame(2).GetMethod.Name
                        sb.Append(String.Format("■{0},{1}.{2}", strTemp, className, methodName))

                        sw.WriteLine(sb.ToString)
                        sb.Length = 0
                        ' Now().ToString + "," + HttpContext.Current.Request.UserAgent + "," + strBrowserType + "," + 
                        ' HttpContext.Current.Request.Browser.Id +"," + HttpContext.Current.Request.Browser.Capabilities.Item("type") + "," + HttpContext.Current.Request.Browser.Capabilities.Item("deviceID") + "," + 
                        ' myBrowserCaps.MobileDeviceModel.ToString + ",H:" + myBrowserCaps.ScreenPixelsHeight.ToString + ",W:" + myBrowserCaps.ScreenPixelsWidth.ToString + "■" + strTemp + ",PreRenderComplete")
                    End Using
                End If
            End If
            '-------------------------------------------

        Catch ex As Exception

            If WebMapPath IsNot Nothing Then
                Dim FilePath As String = IO.Path.Combine(WebMapPath, "accTestLogOut.flg")
                If IO.File.Exists(FilePath) = True Then
                    FilePath = IO.Path.Combine(WebMapPath, "errorSummary.log")
                    Using sw As New IO.StreamWriter(FilePath, True, Encoding.GetEncoding("Shift_JIS"))
                        sw.WriteLine(String.Format("{0} {1}", DateTime.Now.ToString, ex.StackTrace))
                    End Using
                End If
            End If
        End Try

        Return strRet

    End Function


End Class
