Option Explicit On
Option Strict On

Imports System.Data
Imports System.Diagnostics
Imports System.Data.OleDb

Public Class ClsContour

    Private _DBPath As String

    Public Property setDBPath As String
        Get
            Return _DBPath
        End Get
        Set(value As String)
            _DBPath = value
        End Set
    End Property

    Public Sub New(Optional ByVal PicDBPath As String = Nothing)

        _DBPath = PicDBPath

    End Sub

    ''' <summary>
    ''' コンター図ファイル一覧データベースファイルから、現在時刻より古い直近のファイル名を取得する
    ''' </summary>
    ''' <returns>取得したファイル名</returns>
    ''' <remarks></remarks>
    Public Function getFileNameFromDate(LastDate As DateTime) As String

        Dim strRet As String = Nothing
        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbDa As OleDb.OleDbDataAdapter = Nothing
        'Dim DtSet As New DataSet("DData")
        Dim strSQL As String = Nothing

        If IO.File.Exists(_DBPath) = True Then

            Try

                Using DbCon As New OleDbConnection

                    DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & _DBPath & ";Jet OLEDB:Engine Type= 5")

                    strSQL = "SELECT ShowDate,FileName FROM ContourDate WHERE ShowDate=(SELECT MAX(ShowDate) FROM ContourDate WHERE ShowDate <=#" & LastDate & "#);"

                    Using DbDa As New OleDbDataAdapter(strSQL, DbCon)
                        'DbDa = New OleDbDataAdapter(strSQL, DbCon)
                        Using DtSet As New DataSet

                            DbDa.Fill(DtSet, "DData")

                            If DtSet.Tables("DData").Rows.Count = 1 Then
                                strRet = DtSet.Tables("DData").Rows(0).Item(1).ToString
                            End If

                        End Using
                    End Using
                End Using
            Catch ex As Exception

            Finally
                'DtSet.Dispose()
                'DbDa.Dispose()
                'DbCon.Dispose()

            End Try
        End If

        Return strRet

    End Function

    ''' <summary>
    ''' コンクリート打設位置表示用情報の取得
    ''' </summary>
    ''' <returns>DBから読み込んだ情報を文字列配列として戻す</returns>
    ''' <remarks></remarks>
    Public Function getConHeightInf(conName As String) As String()

        Dim strRet As String() = Nothing
        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbDa As OleDb.OleDbDataAdapter = Nothing
        'Dim DtSet As New DataSet("DData")
        'Dim DTable As DataTable = Nothing
        Dim strSQL As String = Nothing

        If IO.File.Exists(_DBPath) = True Then

            Try

                Using DbCon As New OleDbConnection
                    DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & _DBPath & ";Jet OLEDB:Engine Type= 5")

                    strSQL = String.Format("SELECT * FROM メニュー基本情報 WHERE 項目名='{0}'", conName)

                    Using DbDa As New OleDbDataAdapter(strSQL, DbCon)
                        'DbDa = New OleDb.OleDbDataAdapter(strSQL, DbCon)

                        Using DtSet As New DataSet("DData")

                            DbDa.Fill(DtSet, "DData")

                            ReDim strRet(DtSet.Tables("DData").Columns.Count - 1)

                            Dim DTable As DataTable

                            DTable = DtSet.Tables(0)

                            If DtSet.Tables("DData").Rows.Count = 1 Then
                                Dim iloop As Integer
                                For iloop = 0 To DTable.Columns.Count - 1
                                    strRet(iloop) = DTable.Rows(0).Item(iloop).ToString
                                Next
                                ''Dim colCount As Integer
                                ''For Each DTC As DataColumn In DtSet.Tables("DData").Columns
                                ''    strRet(colCount) = DTC.Table
                                ''    colCount += 1
                                ''Next


                            End If
                            DTable.Dispose()
                        End Using
                    End Using
                End Using

            Catch ex As Exception

                'Finally
                '    DTable.Dispose()
                '    DtSet.Dispose()
                '    DbDa.Dispose()
                '    DbCon.Dispose()

            End Try
        End If

        Return strRet

    End Function

    ''' <summary>
    ''' CommonInf.mdbから、指定チャンネルの指定列データのみを取得する
    ''' </summary>
    ''' <param name="ConHeightCh"></param>
    ''' <returns>取得したデータを文字列として戻す</returns>
    ''' <remarks></remarks>
    Public Function ReadDataSummary4Yanagawa(ByVal ConHeightCh As Integer, FieldNo As Integer) As String

        ''Dim DataFilePath As String
        ''Dim DBFilePath As String
        'Dim DbCon As New OleDbConnection
        'Dim DbDa As OleDbDataAdapter
        'Dim DtSet As DataSet
        Dim strSQL As String
        Dim strRet As String = Nothing

        Using DbCon As New OleDbConnection

            DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" & setDBPath & ";Jet OLEDB:Engine Type= 5")
            strSQL = String.Format("SELECT * FROM 共通情報 WHERE DataBaseCh IN({0}) ORDER BY DataBaseCh", ConHeightCh)

            Using DbDa As New OleDbDataAdapter(strSQL, DbCon)

                Using DtSet As New DataSet("DrawData")

                    'DtSet = New DataSet("DrawData")
                    'DbDa = New OleDbDataAdapter(strSQL, DbCon)
                    DbDa.Fill(DtSet, "DrawData")

                    If DtSet.Tables("DrawData").Rows.Count > 0 Then
                        strRet = DtSet.Tables("DrawData").Rows(0).Item(FieldNo).ToString
                    End If

                End Using
            End Using
        End Using

        Return strRet

    End Function

End Class
