<%@ WebService Language="VB" Class="getDateTime" %>
'' ----------------------------------------------------------------------------------
'' 鵜泊における開発
'' AjaxのCacadingDropDown使用のためのWebサービス
'' CSVDate.mdb ファイルにおける日付の選択　　「年月」→「日」→「時分」の３段階
'' ----------------------------------------------------------------------------------
Imports AjaxControlToolkit
Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Collections.Generic
''Imports System.Collections.Specialized
Imports System.Data
''Imports System.Data.Common
Imports System.Web.Script.Services


<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ScriptService()> _
Public Class getDateTime
    Inherits System.Web.Services.WebService

    <WebMethod(EnableSession:=True, _
        CacheDuration:=0, _
        Description:="年月、日、時分を取得する")> _
    Public Function GetDate(ByVal knownCategoryValues As String, ByVal category As String) As CascadingDropDownNameValue()

        Dim retVal As New List(Of AjaxControlToolkit.CascadingDropDownNameValue)
        Dim sd As String = Context.Session("SD")
        Dim CSVDatePath As String = Context.Server.MapPath("UdomariTunnel\App_Data\CSVDate.mdb")
        CSVDatePath = CSVDatePath.Replace("\api", "")                                                   ' /apiフォルダに振り分けたため、それを除去する
        Dim strSQL As String = Nothing
        'Dim DbCon As New OleDb.OleDbConnection
        'Dim DbCom As New OleDb.OleDbCommand
        'Dim DbDa As New OleDb.OleDbDataAdapter
        'Dim DtSet As New DataSet("DateList")
        ''Dim DTbl As DataTable
        Dim selDate As DateTime

        Using DbCon As New OleDb.OleDbConnection

            Using DbCom As New OleDb.OleDbCommand
                
                Select Case category
                    Case "YM"       ' 年月
                        DbCom.CommandText = "SELECT DISTINCT FORMAT(日付, 'yyyy年 MM月') AS CSV日付 FROM CSV ORDER BY FORMAT(日付, 'yyyy年 MM月') ASC"
            
                    Case "D"        ' 日
                        Dim kv As StringDictionary = CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)
                        Dim getYM As String = kv.Item("YM")
                        selDate = DateTime.Parse(getYM)
                
                        DbCom.CommandText = "SELECT DISTINCT FORMAT(日付,'dd日') FROM CSV WHERE YEAR(日付)=YEAR(@SelDate) AND MONTH(日付)=MONTH(@SelDate) ORDER BY FORMAT(日付,'dd日') ASC"
            
                    Case "HM"       ' 時分
                        Dim kv As StringDictionary = CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)
                        Dim YM As String = kv.Item("YM")
                        Dim Dt As String = kv.Item("D")
                        selDate = DateTime.Parse(YM & Dt)
                
                        DbCom.CommandText = "SELECT FORMAT(日付,'HH:mm:ss') FROM CSV WHERE (YEAR(日付)=YEAR(@SelDate) AND MONTH(日付)=MONTH(@SelDate) AND DAY(日付)=DAY(@SelDate)) ORDER BY FORMAT(日付,'HH:mm:ss') ASC"
        
                End Select

                Try
      
                    DbCon.ConnectionString = ("Provider=Microsoft.Jet.OLEDB.4.0;" & "Data Source=" & CSVDatePath & ";Jet OLEDB:Engine Type= 5")
                    DbCom.Connection = DbCon
                    DbCom.Parameters.Add(New OleDb.OleDbParameter("@SelDate", System.Data.OleDb.OleDbType.Char))
                    DbCom.Parameters("@SelDate").Value = selDate
        
                    Using DbDa As New OleDb.OleDbDataAdapter(DbCom)
                        Using DtSet As New DataSet("DateList")
                        
                            DbDa.Fill(DtSet, "DateList")
                            ''DTbl = DtSet.Tables("DateList")

                            Dim DsetCount As Integer = DtSet.Tables("DateList").Rows.Count - 1
                            Dim iloop As Integer

                            If DsetCount <> -1 Then                                                         '指定日付範囲でデータがある場合
                                iloop = 0
                                For Each DTR As DataRow In DtSet.Tables("DateList").Rows
                                    Dim cv As New CascadingDropDownNameValue(DTR.ItemArray.GetValue(0), DTR.ItemArray.GetValue(0))
                                    retVal.Add(cv)
                                Next
                            End If
                        End Using
                    End Using
                        
                Catch ex As Exception

            
                    'Finally
                    '    ''DTbl.Dispose()
                    '    DbDa.Dispose()
                    '    DbCom.Dispose()
                    '    DbCon.Dispose()
            
                End Try
        
            End Using
        End Using
            
        Return retVal.ToArray
    End Function

End Class

