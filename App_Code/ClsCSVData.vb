Option Explicit On
Option Strict On

Imports System.IO
Imports System.Diagnostics

Public Class ClsCSVData

    ''' <summary>
    ''' 高頻度データのCSVファイル読み込みと、そのデータを戻り値に格納
    ''' </summary>
    ''' <param name="csvPath">CSVファイルのフルパス</param>
    ''' <param name="SensorNo">センサー台数</param>
    ''' <returns>読込んだデータ配列</returns>
    ''' <remarks>ほぼ鵜泊対応の仕様</remarks>
    Public Function ReadDataFromCSVatUdomari(ByVal csvPath As String, ByVal SensorNo As Integer) As Single(,)

        Dim arrRet As Single(,) = Nothing
        Dim ContentPre As String = Nothing
        Dim strTemp As String = Nothing

        If IO.File.Exists(csvPath) = True Then

            Using sr As New System.IO.StreamReader(csvPath, System.Text.Encoding.GetEncoding("shift_jis"))      ' 波形データのファイルがShift-JIS
                ContentPre = sr.ReadToEnd()
            End Using

            strTemp = ContentPre.Replace(Convert.ToChar(&HA), "")                   ' LFを除去
            strTemp = strTemp.TrimEnd(Convert.ToChar(&HD))                          ' 最後のCRを除去   

            Try
                Dim RowData() As String
                RowData = strTemp.Split(Convert.ToChar(&HD))                        ' CRで分割

                Dim RowLoop As Integer
                Dim ColLoop As Integer
                Dim Col() As String
                Dim ColSt As Integer
                Dim ColEd As Integer
                Dim offset As Integer
                Dim colCount As Integer

                If SensorNo > 0 Then
                    ColSt = ((SensorNo - 1) * 3 + 1)
                    ColEd = ColSt + 2
                    colCount = 3
                Else
                    ColSt = 1
                    ColEd = CType(HttpContext.Current.Session.Item("SensorCount"), Integer)
                    colCount = ColEd - 1
                End If
                offset = (ColSt - 1)

                ''ReDim arrRet(RowData.Length - 4, SensorCount)
                ReDim arrRet(RowData.Length - 3, colCount)

                '指定センサーのみ
                '記号行と単位行があるのでインデックスの２(3行目から)から取得している
                For RowLoop = 2 To RowData.Length - 1
                    Col = RowData(RowLoop).Split(Convert.ToChar(","))
                    ''For ColLoop = 0 To col.Length - 1

                    arrRet(RowLoop - 2, 0) = Convert.ToSingle(Col(0))
                    For ColLoop = ColSt To ColEd
                        arrRet(RowLoop - 2, (ColLoop - offset)) = Convert.ToSingle(Col(ColLoop))        '配列に格納
                    Next
                Next

            Catch ex As Exception

                ''Array.Clear(arrRet, 0, arrRet.Length)                                                 ' 配列の中身をクリアする
                Erase arrRet                                                                            ' 配列そのものをクリアする

            End Try

        End If

        Return arrRet

    End Function

    ''' <summary>
    ''' Ajaxで非同期呼び出しされる　　波形データを読み込んでJSONに成形
    ''' </summary>
    ''' <param name="csvPath">波形CSVファイルのパス</param>
    ''' <param name="SensorNo">センサー記号</param>
    ''' <returns>JSON形式に成形した文字列</returns>
    ''' <remarks></remarks>
    Public Function ReadDataFromCSVatUdomariStr(ByVal csvPath As String, ByVal SensorNo As Integer) As String

        Dim strRet As String = "Error"

        Try
            Dim datas As Single(,) = ReadDataFromCSVatUdomari(csvPath, SensorNo)
            Dim iloop As Integer
            Dim sb As New StringBuilder

            If datas IsNot Nothing Then

                sb.Append("[")
                For iloop = 0 To datas.GetLength(0) - 1
                    ''sb.Append(String.Format("[{0},{1}],", datas(iloop, 0), datas(iloop, 1)))
                    sb.Append(String.Format("[{0},{1},{2},", datas(iloop, 0), datas(iloop, 1), datas(iloop, 2)))
                    sb.Append(String.Format("{0}],", datas(iloop, 3)))
                Next iloop

                strRet = sb.ToString
                strRet = strRet.TrimEnd(Convert.ToChar(",")) & "]"

                sb.Length = 0

            End If

        Catch ex As Exception

        End Try

        Return strRet

    End Function


    ''' <summary>
    ''' 波形データCSVを丸々読んで返す
    ''' </summary>
    ''' <param name="csvPath">波形データCSVのパス</param>
    ''' <returns>波形データそのもの</returns>
    ''' <remarks></remarks>
    Public Function ReadDataFromCSVUdomariStrAll(ByVal csvPath As String) As String

        Dim strRet As String = "Error"

        If IO.File.Exists(csvPath) = True Then
            Try
                Using sr As New System.IO.StreamReader(csvPath, System.Text.Encoding.GetEncoding("shift_jis"))
                    strRet = sr.ReadToEnd()
                End Using
            Catch ex As Exception

            End Try
        End If

        Return strRet

    End Function

End Class
