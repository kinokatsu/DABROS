Imports System.IO

Public Class ClsDenyList

    ''' <summary>
    ''' ユーザーエジェントとアクセス拒否リストが合致しているか確認
    ''' </summary>
    ''' <param name="FilePath">グラフの識別名称</param>
    ''' <param name="UserAgents">現場のフォルダ</param>
    ''' <returns>拒否リストと合致したらTrue</returns>
    Public Function ReadDenyList(ByVal FilePath As String, ByVal UserAgents As String) As Boolean

        'Dim TextFile As IO.StreamReader
        Dim RoadedText As String
        Dim blnRet As Boolean

        If IO.File.Exists(FilePath) = False Then
            Return False
        End If

        Using TextFile As New IO.StreamReader(FilePath, System.Text.Encoding.Default)
            'TextFile = New IO.StreamReader(FilePath, System.Text.Encoding.Default)

            RoadedText = TextFile.ReadToEnd
            'TextFile.Close()
        End Using

        Dim UsrAgentList() As String = ReadTextFile(FilePath)       'RoadedText.Split(ControlChars.CrLf)
        For iloop As Integer = 0 To UsrAgentList.Length - 1
            If HttpContext.Current.Request.UserAgent.IndexOf(UsrAgentList(iloop)) <> -1 Then
                blnRet = True

                Dim txtPath As String = "Denied.list"
                Using txtfile As New IO.StreamWriter(txtPath, True)
                    'Dim txtfile As IO.StreamWriter
                    'Dim txtPath As String = "Denied.list"
                    'txtfile = IO.File.AppendText(txtPath)
                    txtfile.Write(Now().ToString & ",Access Deny," & HttpContext.Current.Request.UserAgent & vbCr)
                    'txtfile.Close()
                End Using

            End If
        Next iloop

        Return blnRet

    End Function

    Public Function CheckDenyIP(ByVal FilePath As String, ByVal IPAdd As String) As Boolean

        Dim blnRet As Boolean = False
        If IO.File.Exists(FilePath) = False Then
            Return False
        End If

        Dim DenyIPList() As String = ReadTextFile(FilePath)

        Dim intRet As Integer = Array.LastIndexOf(DenyIPList, IPAdd)

        If intRet >= 0 Then
            blnRet = True
        End If

        Return blnRet

    End Function

    ''' <summary>
    ''' テキストファイルの内容の全読込
    ''' </summary>
    ''' <returns>テキストファイルの内容を配列にして返す</returns>
    ''' <remarks></remarks>
    Private Function ReadTextFile(ByVal FilePath As String) As String()

        Dim strRet() As String = Nothing

        Try
            If IO.File.Exists(FilePath) = True Then
                Dim enc As System.Text.Encoding = System.Text.Encoding.GetEncoding("shift_jis")
                'テキストファイルの中身をすべて読み込む
                Dim str As String = IO.File.ReadAllText(FilePath, enc)

                Dim temp As String = str.Replace(Convert.ToChar(&HA), "")
                temp = temp.TrimEnd(Convert.ToChar(&HD))
                Dim strTemp() As String = temp.Split(Convert.ToChar(&HD))

                strRet = strTemp
            End If
        Catch ex As Exception
            strRet(0) = "File Read Error"
        End Try

        Return strRet

    End Function

End Class
