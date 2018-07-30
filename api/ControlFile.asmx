<%@ WebService Language="VB" Class="Control.SitePC.ControlFile" %>

Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Web.Script.Services
Imports System.IO

Namespace Control.SitePC

    <WebService(Namespace:="http://tempuri.org/")> _
    <WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
    <System.ComponentModel.ToolboxItem(False)> _
    <ScriptService()> _
    Public Class ControlFile

        Inherits System.Web.Services.WebService
        'Public enc As Encoding = Encoding.GetEncoding("shift_jis")
        
        ' ■現場PC制御用ファイルの作成■=============================================================
        <WebMethod(EnableSession:=True, CacheDuration:=0, Description:="現場PC制御用ファイルを作成する")> _
        Public Function Add(ByVal orderText As String) As String

            Dim sd As String = Context.Session("SD")
            Dim orderFileName As String = sd & "_webComm.cmd"
            Dim replyFileName As String = sd & "_webRep.cmd"
            
            Dim orderFilePath As String = Context.Server.MapPath("rqCom\" & orderFileName)
            Dim replyFilePath As String = Context.Server.MapPath("rqCom\" & replyFileName)
            
            Dim enc As Encoding = Encoding.GetEncoding("shift_jis")
            Dim ts As TimeSpan = Nothing
            Dim stTime As DateTime = DateTime.Now
            Dim inCompleteFlg As Integer = 0
            Dim strMsg As String = "現場へ命令を送信しました。"
            Dim sendOrder As String = Nothing
            Dim orderCommand() As String = Nothing
            Dim checkResult As Boolean
            Dim repText As String = Nothing
            
            '前回の現場への要求ファイルが残っていた場合は削除する()
            Call delFile(orderFileName)

            '現場からの応答ファイルが残っていた場合は削除する()
            Call delFile(replyFilePath)

            ' 現場への要求ファイルを作成----------------------------------------------
            System.Windows.Forms.Application.DoEvents()
            sendOrder = orderText.Replace(":", Environment.NewLine)             ' 改行がはいらないかもなので、一応変換しておく
            orderCommand = orderText.Split(":")
            Select Case orderCommand(0)
                Case "RqCYW"        ' 収録時刻変更
                    Dim dat() As String = orderCommand(1).Split(","c)
                    checkResult = DateCheck(orderCommand(1))                    ' 開始,終了,間隔のチェック
                    If checkResult = False Then
                        repText = "[RCVNG]設定内容に間違いがあります。"
                    End If

                Case "RqPWt"        ' 測定時刻変更                              ' 測定時刻のチェック
                    checkResult = checkPWtData(orderCommand(1))
                    If checkResult = False Then
                        repText = "[RCVNG]設定内容に間違いがあります。"
                    End If

            End Select
            If repText IsNot Nothing Then Return repText
            
            File.WriteAllText(orderFilePath, sendOrder, enc)

            ' 現場への要求ファイルが無くなるのを確認する(無くなった＝送信した)
            Do While IO.File.Exists(orderFilePath) = True
                Threading.Thread.Sleep(100)
                ts = DateTime.Now.Subtract(stTime)

                If ts.TotalSeconds >= 5 Then
                    inCompleteFlg = 1
                    Exit Do
                End If
            Loop

            ' 要求ファイルが無くならなかったら、現場へ司令できていないはず・・
            If inCompleteFlg = 1 Then
                Call delFile(orderFilePath)
                strMsg = "現場へ送信できなかった可能性があります"
            End If

            Return strMsg

        End Function

        
        ' ■現場からの応答ファイル確認と内容取得■===================================================
        <WebMethod(EnableSession:=True, CacheDuration:=0, Description:="現場からの応答ファイル存在確認と内容取得")> _
        Public Function Confirm(ByVal dmy As String) As String
            
            Dim sd As String = Context.Session("SD")
            Dim orderFileName As String = sd & "_webComm.cmd"
            Dim replyFileName As String = sd & "_webRep.cmd"
            
            Dim orderFilePath As String = Context.Server.MapPath("rqCom\" & orderFileName)
            Dim replyFilePath As String = Context.Server.MapPath("rqCom\" & replyFileName)
            
            Dim ts As TimeSpan
            Dim stTime As DateTime = DateTime.Now
            Dim orderFlg As Integer = 0
            Dim strMsg As String = Nothing
            Dim repText As String = Nothing
            
            '現場への要求ファイルが残っていた場合は現場へ送信できていないということ
            If IO.File.Exists(orderFilePath) = True Then
                Return "現場に送信できなかった可能性があります"
            End If

            ' 現場からの応答ファイルを待機する----------------------------------------
            Do Until IO.File.Exists(replyFilePath) = True
                Threading.Thread.Sleep(100)
                ts = DateTime.Now.Subtract(stTime)

                If ts.Seconds >= 5 Then
                    Exit Do
                End If
            Loop

            ' 応答の後処理-----------------------------------------------------------
            If IO.File.Exists(replyFilePath) = True Then
            
                repText = makeResultInfo(replyFilePath)     ', addText)
                Call delFile(replyFilePath)

            Else

                repText = "現場からの応答確認中・・・"  '& DateTime.Now.Second.ToString("00")

            End If
            
            Return repText

            ''Dim result As String
            ''result = String.Format("{0}：{1}：{2}", "Hellow World", name, sd)
            ''Return result   '"Hello World:" & name & ":" & sd

        End Function
        
        ' ■ファイルの存在を確認して、あれば削除する■===============================================
        Private Sub delFile(ByVal filePath As String)
            Try
                If File.Exists(filePath) = True Then
                    File.Delete(filePath)
                End If
            Catch ex As Exception
            End Try

        End Sub

        ' ■応答ファイルの内容を読み込んで、処理して返す■===========================================
        Private Function makeResultInfo(ByVal replyFilePath As String) As String   ', ByVal addText As String) As String
            
            Dim strMsg As String = Nothing
            
            Dim rpCommand As String() = Nothing
            Dim enc As System.Text.Encoding = System.Text.Encoding.GetEncoding("shift_jis")
            Dim iloop As Integer
            Dim strTemp As String
            Dim intRet As Integer = 0
            Dim repText As String = Nothing
            Dim comInfo As String = Nothing
            Dim retryCount As Integer = 0
            Dim errorCount As Integer = 0
            ' 1行目：コマンド
            ' 2行目：通信結果
            ' 3行目：読込んだ結果など
            Do
                Try
                    rpCommand = System.IO.File.ReadAllLines(replyFilePath, enc)     '複数行の応答を配列に格納
                    Exit Do

                Catch ex As Exception

                    If retryCount = 10 Then Exit Do
                    retryCount += 1
                    Threading.Thread.Sleep(100)

                End Try
            Loop

            If rpCommand IsNot Nothing Then
                
                repText = "[RCV]"
                For iloop = 0 To rpCommand(1).Length - 1
                    strTemp = rpCommand(1).Substring(iloop, 1)
                    If strTemp <> "0" Then
                        intRet = Convert.ToInt32(strTemp)
                        If intRet > 0 Then errorCount += 1
                        Select Case intRet
                            Case 1
                                'strMsg &= ("No." & (iloop + 1).ToString & "：通信異常" & Environment.NewLine)
                                comInfo = "通信異常"
                            Case 2
                                'strMsg &= ("No." & (iloop + 1).ToString & "：測定中" & Environment.NewLine)
                                comInfo = "測定中"
                            Case 8
                                'strMsg &= ("No." & (iloop + 1).ToString & "：収録設定として、定期もしくは任意時刻が設定されています。" & Environment.NewLine)
                                comInfo = "収録設定として、定期もしくは任意時刻が設定されています。"
                            Case 9
                                'strMsg &= "定時収録時刻、設定中、およびロガー未登録 により測定実行できませんでした。" & Environment.NewLine
                                comInfo = "定時収録時刻、設定中、およびロガー未登録 により測定実行できませんでした。"
                        End Select
                    Else
                        'strMsg &= ("No." & (iloop + 1).ToString & "：正常" & Environment.NewLine)
                        comInfo = "正常"
                    End If

                    strMsg &= String.Format(" No.{0}：{1}", (iloop + 1).ToString.PadLeft(2, "0"c), comInfo & "<br />")

                Next

                Select Case rpCommand(0).ToUpper

                    ' ロガーか絡まないコマンド
                    Case "RPCYR", "RPPRT"       ' 収録時刻読込 測定時刻読込
                        repText &= "現場PCの設定を読み込みました。<br />"
                        strMsg = Nothing
                    Case "RPCYW"                '  収録時刻変更 
                        repText &= "現場PCの設定を変更しました。<br />"
                        strMsg = Nothing
                        
                        ' ロガーか関係するコマンド                        
                    Case "RPSWA", "RPPWT", "RPGWT"      ' 即時測定 測定時刻変更 時計合わせ
                        If errorCount > 0 Then
                            repText &= "一部のロガーに命令を出せませんでした。<br />"
                        Else
                            repText &= "ロガーに命令を送信しました。<br />"
                            If rpCommand(0).ToUpper = "RPSWA" Then
                                repText &= "数分後にデータが更新されます。<br />"
                            End If
                        End If
                        
                    Case Else

                End Select

                If strMsg IsNot Nothing Then
                    repText &= strMsg       '("ロガー：結果<br />----------------------<br />" & strMsg)
                End If
            
                ''If addText.Length > 0 Then
                ''    repText &= (addText)
                ''End If
             
                ' 測定時刻要求、収録時刻要求、ライン状態要求 の時は時刻情報を付与する()
                If rpCommand(0).ToUpper = "RPPRT" Or rpCommand(0).ToUpper = "RPCRO" Or rpCommand(0).ToUpper = "RPCYR" Or rpCommand(0).ToUpper = "RPCWO" Then
                    repText &= "|" & rpCommand(0) & rpCommand(2)
                End If
            Else
                
                repText = "応答の取得ができませんでした。"
                
            End If
                
            Return repText
            
        End Function

        ' ■文字列(記号なし日付)が日付として正常であるか確認して変換まで行う■==============
        Private Function DateCheck(ByVal checkCommand As String) As Boolean
            
            Dim blnRet As Boolean = False
            Dim comData() As String
            Dim convDate1 As DateTime
            Dim convDate2 As DateTime

            If checkCommand IsNot Nothing AndAlso checkCommand.Length >= 27 Then
                comData = checkCommand.Split(",")
                convDate1 = convertDate(comData(0))

                If IsDate(convDate1) = True Then
                
                    If comData(1) <> "999999999999" Then
                        convDate2 = convertDate(comData(1))
                        If convDate1 < convDate2 Then
                            blnRet = True
                        End If
                    Else
                        blnRet = True
                    End If
                        
                Else

                    blnRet = False

                End If

            End If

            Return blnRet
            
        End Function

        ' ■文字列(記号なし日付)を日付型に変換する■==============
        Private Function convertDate(ByVal dateString As String) As DateTime
            
            Dim strTemp As String
            Dim blnRet As Boolean
            Dim convDate As DateTime
            Dim retDate As DateTime = CType("1900/01/01 0:00:00", DateTime)
            
            If dateString IsNot Nothing AndAlso dateString.Length = 12 Then

                strTemp = dateString.Substring(0, 4) & "/" & _
                            dateString.Substring(4, 2) & "/" & _
                            dateString.Substring(6, 2) & " " & _
                            dateString.Substring(8, 2) & ":" & _
                            dateString.Substring(10, 2)
                
                blnRet = DateTime.TryParse(strTemp, convDate)
                If blnRet = False Then
                    retDate = CType("1900/01/01 0:00:00", DateTime)
                Else
                    retDate = convDate
                End If
                
            End If

            Return retDate

        End Function
        
        ' ■測定時刻設定内容のバリデーション■==============
        Private Function checkPWtData(ByVal orderCommand As String) As Boolean
            Dim hourData As String
            Dim minData As String
            Dim blnRet As Boolean = False
            
            If orderCommand IsNot Nothing AndAlso orderCommand.Length >= 84 Then

                hourData = orderCommand.Substring(0, 24)    '時データ
                minData = orderCommand.Substring(24)        '分データ
                
                If hourData.IndexOf("1") <> -1 AndAlso minData.IndexOf("1") <> -1 Then
                    
                    '時に0,1以外の文字があるか確認
                    Dim strTemp As String = hourData.Replace("0", "")
                    strTemp = strTemp.Replace("1", "")
                    If strTemp.Length = 0 Then
                        blnRet = True
                    End If
             
                    If blnRet = True Then
                        '分に0,1以外の文字があるか確認
                        strTemp = Nothing
                        strTemp = minData.Replace("0", "")
                        strTemp = strTemp.Replace("1", "")
                        If strTemp.Length = 0 Then
                            blnRet = True
                        Else
                            blnRet = False
                        End If
                    End If
                End If
                
            End If
            
            Return blnRet
            
        End Function
        
    End Class

End Namespace