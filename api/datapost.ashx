<%@ WebHandler Language="VB" Class="ReceiveData" %>
'' HTTP POST で送られてきたデータをCSVファイルで保存する
'' 風向風速計関連で、現場からHTTPでデータを送ってきた場合の処理を行うためのハンドラー
''Imports System
''Imports System.Web
Imports System.Xml.Serialization
Imports Newtonsoft.Json

Public Class ReceiveData : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim currentContext As HttpContext = HttpContext.Current
        Dim StrRet As String = "Bad Request"
        Dim CodeRet As Integer = 400
        Dim dataType As String = Convert.ToString(currentContext.Request.QueryString("ty")).ToUpper
        Dim cookie As String = Convert.ToString(currentContext.Request.QueryString("AspxAutoDetectCookieSupport"))
        Dim WriteData As String = Nothing
        Dim dataAcc As New ClsCheckUser
        Dim loopoffset As Integer = 5
        
        context.Response.ContentType = "text/plain"
        If cookie Is Nothing Then                                                                               ' クッキーレスセッションを有効にするおまじないが入っているとずれるので調整
            loopoffset = 4
        End If

        Try
            If context.Request.HttpMethod = "POST" Then
                CodeRet = 200
                StrRet = "Successful"
            
                Dim chgKey As Integer = Convert.ToInt32(context.Request.Params.Get("ch"))
                Dim GenbaFolder As String = context.Request.Params.Get("sd").ToString                           ' 現場フォルダ名
                Dim DataCount As String = context.Request.Params.Get("dc").ToString                             ' データ数
                Dim loopCount As Integer
                Dim DataLoop As Integer
                Dim loopindex As Integer

                If DataCount IsNot Nothing AndAlso DataCount.Length > 0 AndAlso DataCount <= 30 Then            ' 送信できるパラメータの数は30までとする
                    loopCount = Convert.ToInt32(DataCount) - 1
                    Dim recData(loopCount) As String
                    Dim keyData(loopCount) As String
                    Dim dict As New System.Collections.Generic.Dictionary(Of String, String)
                    Dim al As New System.Collections.ArrayList()
                    ''Dim array As System.Collections.Generic.IDictionary(Of String, String)()                  '配列にする場合　とりあえずとっておくだけ

                    Dim genbaName As String = (context.Request.Params.Get("sd")).ToString
                    Dim decryptGenbaFolder As String = Nothing
                
                    For DataLoop = 0 To loopCount
                        loopindex = DataLoop + loopoffset
                        keyData(DataLoop) = context.Request.Params.GetKey(loopindex).ToString
                        recData(DataLoop) = context.Request.Params.Get(loopindex).ToString   ' "dt" & DataLoop.ToString).ToString       ' データ数分を取得して格納
                        al.Add(New KeyAndVal(keyData(DataLoop), recData(DataLoop)))
                        dict.Add(keyData(DataLoop), recData(DataLoop))
                    Next
                
                    ''Dim writeData As String = String.Join(",", keyData) & Environment.NewLine
                    ''writeData &= String.Join(",", recData)
                    decryptGenbaFolder = dataAcc.EncryptText(GenbaFolder, False, chgKey)                           ' 現場フォルダ名をデクリプト
                    Dim FileName As String = (decryptGenbaFolder & "_NewestData")
                    Dim FilePath As String = context.Server.MapPath(decryptGenbaFolder & "\App_Data\" & FileName)
                    Dim FilePath2 As String = context.Server.MapPath("OutData\" & FileName)
                    ''ReDim Array(1)                                                                            '配列にする場合　とりあえずとっておくだけ
                    ''array(0) = dict
                    ''array(1) = dict
                    ''Dim json As String = JsonConvert.SerializeObject(array)                               ' JSONにシリアライズ

                    Try
                        If dataType.ToUpper = "X" Then

                            FilePath &= ".xml"
                            ''Dim ns As New XmlSerializerNamespaces           ' 名前空間が不要の場合はこれを入れる(<item xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"> のような記述がなくなる)
                            ''ns.Add(String.Empty, String.Empty)
                            Dim et As Type() = New Type() {GetType(KeyAndVal)}                                                          'ArrayListに追加されているオブジェクトの型の配列を作成
                            Dim serializer As New System.Xml.Serialization.XmlSerializer(GetType(System.Collections.ArrayList), et)     'ArrayListに追加されているオブジェクトを指定してXMLファイルに保存する
                            Using sw As New System.IO.StreamWriter(FilePath, False, System.Text.Encoding.UTF8)
                                serializer.Serialize(sw, al)    ', ns)
                            End Using
                            Using sw As New System.IO.StreamWriter(FilePath, False, System.Text.Encoding.UTF8)
                                serializer.Serialize(sw, al)    ', ns)
                            End Using
                            

                        Else

                            FilePath &= ".json"
                            FilePath2 &= ".json"
                            WriteData = Newtonsoft.Json.JsonConvert.SerializeObject(dict)                                      ' JSONにシリアライズ                                    
                            If WriteData IsNot Nothing Then
                                Using sw As New IO.StreamWriter(FilePath, False, System.Text.Encoding.UTF8)
                                    sw.WriteLine(WriteData)
                                End Using
                                Using sw As New IO.StreamWriter(FilePath2, False, System.Text.Encoding.UTF8)
                                    sw.WriteLine(WriteData)
                                End Using
                            End If

                        End If
                    Catch ex As Exception
                        StrRet = "Not Found " & ex.Message
                        CodeRet = 404
                    End Try
                Else
                    StrRet = "Bad Request"
                    CodeRet = 400
                
                End If

            End If

        Catch ex As Exception
            Diagnostics.Debug.WriteLine(ex.Message)
            StrRet = "Not Acceptable"
            CodeRet = 406

        End Try

        context.Response.Write(StrRet)                                                                      ' HTTPのレスポンスを返す
        context.Response.StatusCode = CodeRet

        context.Response.Flush()
        context.Response.SuppressContent = True                             ' 余計なごみ(HTML関連)がついてしまうのを回避する　Flushと一緒に利用

        dataAcc = Nothing

    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Class KeyAndVal
        Public key As String
        Public Value As String

        Public Sub New()
            key = ""
            Value = ""
        End Sub
        Public Sub New(ByVal name As String, ByVal msg As String)
            key = name
            Value = msg
        End Sub
    End Class

End Class
