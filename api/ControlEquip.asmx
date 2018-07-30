<%@ WebService Language="VB" Class="Control.SiteEquip.ControlEquip" %>

Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Web.Script.Services
Imports System.IO
Imports System.Data
Imports System.Data.OleDb

Namespace Control.SiteEquip

    <WebService(Namespace:="http://tempuri2.org/")> _
    <WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
    <System.ComponentModel.ToolboxItem(False)> _
    <ScriptService()> _
    Public Class ControlEquip

        Inherits System.Web.Services.WebService

        ' ■■■■ 現場PC制御用ファイルの作成■======================================================
        <WebMethod(EnableSession:=True, CacheDuration:=0, Description:="現場機器制御用ファイルを作成する")> _
        Public Function Add(ByVal orderText As String) As String

            Dim sd As String = Context.Session("SD")
            Dim strTemp() As String = orderText.Split(":")
            Dim LoggerID As String = strTemp(1)
            Dim orderFileName As String = String.Format("{0}_{1}_.CCNW", LoggerID, DateTime.Now.ToString("yyyyMMdd-HHmmss"))
            Dim replyFileName As String = String.Format("{0}_{1}_.webrep", LoggerID, sd)
            Dim strStatus As String = "現場機器に要求を送信しました。"
            Dim sendOrder As String = Nothing
            Dim enc As Encoding = Encoding.GetEncoding("shift_jis")
            
            Dim orderFilePath As String = Path.Combine(Context.Server.MapPath("rqCom\"), orderFileName)
            Dim replyFilePath As String = Path.Combine(Context.Server.MapPath("rqCom\"), replyFileName)

            '前回の現場への要求ファイルが残っていた場合は削除する()
            Call delFile(orderFileName)

            '現場からの応答ファイルが残っていた場合は削除する()
            Call delFile(replyFilePath)

            Try
                ' 現場への要求ファイルを作成----------------------------------------------            
                Select Case strTemp(0)
                    Case "RqGSc"            ' 情報読み込み
                        Dim SB As New StringBuilder
                    
                        SB.AppendLine("0,0,0,0,0,0")
                        SB.AppendLine(DateTime.Now.ToString)
                        SB.AppendLine("-9999")
                        SB.AppendLine("0")
                        SB.AppendLine(sd)
                        ''strStatus = String.Format("GCc|{0}", GetLastStatus(LoggerID, sd))
                        sendOrder = SB.ToString
                        SB.Length = 0
                   
                    Case "RqSCs"
                        'Throw New Exception("Value1が9を超えました。")
                        If strTemp.Length = 3 Then
                            Dim SB As New StringBuilder
                            Dim order As String = strTemp(2)
                            Dim bit As String = strTemp(2).Substring(0, 5)
                            Dim DisableBit As String = strTemp(2).Substring(strTemp(2).Length - 1, 1)
                            Dim iloop As Integer
                            Dim sendBit As String = "0,"
                            For iloop = 0 To 4
                                sendBit &= String.Format("{0},", bit.Substring(iloop, 1))
                            Next
                            sendBit = sendBit.TrimEnd(Convert.ToChar(","))
                            SB.AppendLine(sendBit)
                            SB.AppendLine(DateTime.Now.ToString)
                            SB.AppendLine("0")
                            SB.AppendLine("0")
                            SB.AppendLine(sd)
                            ''strStatus = String.Format("GCc|{0}", GetLastStatus(LoggerID, sd))
                            sendOrder = SB.ToString
                            SB.Length = 0

                            'DBにDsiableを保存する
                            SetControlDisable(DisableBit, LoggerID, sd)

                        Else
                            orderFilePath = replyFilePath
                            sendOrder = "[CMDNG]設定異常です。コマンドを送信できません。"
                        End If
                        
                End Select

            Catch ex As Exception
                orderFilePath = replyFilePath
                sendOrder = "[CMDNG]設定異常です。コマンドを送信できません。"
            End Try
                
            File.WriteAllText(orderFilePath, sendOrder, enc)



            Return strStatus

        End Function
        '--------------------------------------------------------------------------------------------

        ' ■■■■現場からの応答ファイル確認と内容取得■=============================================
        <WebMethod(EnableSession:=True, CacheDuration:=0, Description:="現場からの応答ファイル存在確認と内容取得")> _
        Public Function Confirm(ByVal dmy As String) As String
            
            Dim LoggerID As String = dmy
            Dim sd As String = Context.Session("SD")
            Dim orderFileName As String = String.Format("{0}_{1}_.CCNW", LoggerID, DateTime.Now.ToString("yyyyMMdd-HHmmss"))
            Dim replyFileName As String = String.Format("{0}_{1}_.webrep", LoggerID, sd)
            
            Dim orderFilePath As String = Context.Server.MapPath("rqCom\" & orderFileName)
            Dim replyFilePath As String = Context.Server.MapPath("rqCom\" & replyFileName)
            Dim strRet As String = Nothing
            Dim repText As String = Nothing
            
            '現場への要求ファイルが残っていた場合は現場へ送信できていないということ
            If IO.File.Exists(orderFilePath) = True Then
                Return "現場に送信できなかった可能性があります"
            End If
            
            ' 現場からの応答ファイルを待機する----------------------------------------
            Dim sw As New System.Diagnostics.Stopwatch
            sw.Start()
            Do Until IO.File.Exists(replyFilePath) = True
                Threading.Thread.Sleep(500)

                If sw.Elapsed.Seconds >= 30 Then
                    Exit Do
                End If
            Loop
            sw.Stop()
            sw.Reset()
            
            ' 応答の後処理-----------------------------------------------------------
            If IO.File.Exists(replyFilePath) = True Then
            
                repText = makeResultInfo(replyFilePath)
                Call delFile(replyFilePath)

            Else

                repText = "[RCVNG]現場機器からの応答がありませんでした。"  '& DateTime.Now.Second.ToString("00")

            End If
            
            Return repText

            
        End Function
        '--------------------------------------------------------------------------------------------
        
        ' ■■■■応答ファイルの内容を読み込んで、処理して返す■=====================================
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
            ' 2行目：読込んだ結果など
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
                
                strTemp = rpCommand(1).Substring(iloop, 1)
                If strTemp = "F" Then                                   ' Failure
                    repText = "[RCVNG]"
                    comInfo = "通信異常"
                Else
                    repText = "[RCV]"
                    comInfo = "通信正常"
                End If
                repText = String.Format("{0}{1}<br />", repText, comInfo)

                If strTemp = "F" Then
                    repText &= "現場機器と接続できませんでした。通信および接点機器の確認をしてください。<br />"
                Else
                    Select Case rpCommand(0).ToUpper
                        Case "RPGCS"                ' 接点状態読み込み
                            repText &= "現場機器の状態を読み込み、接点状態の表示を更新しました。<br />"
                        Case "RPSCS"                '  収録時刻変更 
                            repText &= "現場機器の設定を変更しました。<br />"
                    End Select
                End If

                ' 測定時刻要求、収録時刻要求、ライン状態要求 の時は時刻情報を付与する()
                repText &= String.Format("|{0}|{1}", rpCommand(0), rpCommand(1))
            Else
                repText = "応答の取得ができませんでした。"
            End If

            Return repText

        End Function
        '--------------------------------------------------------------------------------------------

        ' ■■■■ファイルの存在を確認して、あれば削除する■=========================================
        Private Sub delFile(ByVal filePath As String)
            Try
                If File.Exists(filePath) = True Then
                    File.Delete(filePath)
                End If
            Catch ex As Exception
            End Try

        End Sub
        '--------------------------------------------------------------------------------------------
        
        
        ' ■■■■データベースに制御有効／無効 を保存する■==========================================
        Private Sub SetControlDisable(ByVal setFlag As Integer, ByVal LoggerID As String, ByVal SD As String)
            Dim cn As New OleDbConnection
            Dim dtSet As DataSet
            Dim da As OleDbDataAdapter = Nothing
            Dim DBPath As String = IO.Path.Combine(Context.Server.MapPath(SD), "App_Data\ContactControl.mdb")

            cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DBPath & ";" & "Jet OLEDB:Engine Type= 5")
            
            Try
                '' 現場情報の読込
                da = New OleDbDataAdapter(String.Format("SELECT * FROM ContactInfo WHERE LoggerID='{0}' ORDER BY ID ASC", LoggerID), cn)
                dtSet = New DataSet
                da.Fill(dtSet, "Loc")
                
                If dtSet.Tables("Loc").Rows.Count > 0 Then
                    Dim flg As Boolean = False
                    Dim cmdbuilder As New OleDbCommandBuilder
                    If setFlag = 1 Then
                        flg = True
                    End If
                    dtSet.Tables("Loc").Rows(0)("Disable") = flg
                    cmdbuilder.DataAdapter = da
                
                    da.Update(dtSet, "Loc")
                    
                    cmdbuilder.Dispose()

                End If

            Catch ex As Exception

            Finally
                dtSet = Nothing
                da.Dispose()
                cn.Dispose()
            End Try
            
        End Sub
        '--------------------------------------------------------------------------------------------
        
        
        ' ■■■■データベースから接点状態を取得して、整形して返す■=================================
        Private Function GetLastStatus(ByVal LoggerID As String, ByVal SD As String) As String
            
            Dim strRet As String = Nothing
            Dim cn As New OleDbConnection
            Dim da As OleDbDataAdapter = Nothing
            Dim dtSet As New DataSet("Loc")
            Dim DBPath As String = IO.Path.Combine(Context.Server.MapPath(SD), "App_Data\ContactControl.mdb")

            cn.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & DBPath & ";" & "Jet OLEDB:Engine Type= 5")

            Try
                '' 現場情報の読込
                da = New OleDbDataAdapter(String.Format("SELECT * FROM ContactInfo WHERE LoggerID='{0}' ORDER BY ID ASC", LoggerID), cn)
                da.Fill(dtSet, "Loc")

                For Each DTR As DataRow In dtSet.Tables("Loc").Rows
                    strRet = Convert.ToInt32(Convert.ToBoolean(DTR.Item(2)))
                    strRet &= Convert.ToInt32(Convert.ToBoolean(DTR.Item(3)))
                    strRet &= Convert.ToInt32(Convert.ToBoolean(DTR.Item(4)))
                    strRet &= Convert.ToInt32(Convert.ToBoolean(DTR.Item(5)))
                    strRet &= Convert.ToInt32(Convert.ToBoolean(DTR.Item(6)))
                    strRet &= Convert.ToInt32(Convert.ToBoolean(DTR.Item(9)))
                Next
            Catch ex As Exception

            Finally
                dtSet = Nothing
                da.Dispose()
                cn.Dispose()
            End Try

            Return strRet
            
        End Function
        '--------------------------------------------------------------------------------------------
        
        
    End Class

End Namespace
