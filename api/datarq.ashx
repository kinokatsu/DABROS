﻿<%@ WebHandler Language="VB" Class="datarq" %>
''httpによって最新値一覧を取得するためのハンドラー　OTPのクライアントソフト用

''Imports System
''Imports System.Web
Imports System.Data
Imports System.Xml.Serialization
Imports Newtonsoft.Json

Public Class datarq : Implements IHttpHandler
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        
        Dim currentContext As HttpContext = HttpContext.Current
        Dim retString As String = Nothing
        Dim uid As String = CType(currentContext.Request.QueryString("ui"), String)
        Dim dataType As String = CType(currentContext.Request.QueryString("ty"), String).ToUpper
        Dim rqDate As String = CType(currentContext.Request.QueryString("dt"), String)
        Dim ContentType As String = CType(currentContext.Request.QueryString("ct"), String)

        ''Dim userInfo As String = uid.Substring(0, uid.Length - 3)                    ' ユーザー情報など   この方法は永久に認証できてしまうので時分から計算する方法に変更
        ''Dim deCodeKey As Integer = CType(uid.Substring(uid.Length - 3), Integer)     ' 複合化キー
        Dim userInfo As String = uid.Substring(0, uid.Length)                    ' ユーザー情報など
        Dim deCodeKey As Integer
        Dim dataAcc As New ClsCheckUser
        Dim minLoop As Double
        Dim nowDateTime As DateTime = DateTime.Now
        Dim calcDateTime As DateTime
        Dim mi As Integer
        Dim ho As Integer
        Dim plusminus As Integer = 1
        Dim preCalc As Integer
        Dim deCodedStr As String = Nothing

        For minLoop = 0 To 20                                                           ' 複合化キーをループでゴールシークする  
            preCalc = (minLoop * plusminus + preCalc)                                   '  暗号化キーを時、分で計算するため、時計の誤差を±20分として複合化文字列に「@OK」があることを見つける
            calcDateTime = nowDateTime.AddMinutes(preCalc)
            mi = (calcDateTime.Minute + 1)
            ho = (calcDateTime.Hour + 1)
            ''(ho * 2.5 + Math.Sin((mi + ho) * 5 * 180/Math.PI) ^ 2 * 1990 / 2.9 + 100) * 1.04      100-895内に納めないと複合化できない
            ''180/Math.PI=57.295779513082323   　1990/2.9=686.2068
            deCodeKey = CType((ho * 2.5 + Math.Sin((mi + ho) * 5 * 57.2957) ^ 2 * 686.2068 + 100) * 1.04, Integer)          ' 演算式の根拠は適当に数値がばらけるようにしただけで、値そのものに意味はない
            plusminus *= -1
            deCodedStr = dataAcc.EncryptText(userInfo, False, deCodeKey)    ' User@Pass@folder@OK に複合化される
            If deCodedStr.EndsWith("@OK") = True Then                       ' @OKで終わっていれば、Hit!
                Exit For
            End If
        Next
        
        Dim usr() As String = deCodedStr.Split("@"c)                                ' 0:User 1:Pass 2:Folder
        ''Dim getDate As DateTime
        If usr.Length = 4 AndAlso usr(3) = "OK" Then
        
            Dim MD5Pass As String = dataAcc.MAKE_MD5(usr(1))                            ' パスワードをMD5にエンコード
            Dim siteNo As String = Nothing
            Dim userLevel As Integer
            Dim authLimitDate As DateTime
            Dim userGroup As String = Nothing
            Dim showName As String = Nothing
            Dim remoteADDR As String = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
            Dim RemoteHOST As String = System.Net.Dns.GetHostEntry(remoteADDR).HostName
            Dim RemoteInf As String = remoteADDR & "," & RemoteHOST & ",datarq 1.00 Pg,Windows"
            Dim siteFolder As String = IO.Path.Combine(context.Server.MapPath(usr(2)), "App_Data")      ' 現場フォルダ
            siteFolder = siteFolder.Replace("\api", "")                                                 ' /apiフォルダに振り分けたため、それを除去する            

            ' 認証処理(アカウントとパスワードの合致を判定)
            Dim auth As Boolean = dataAcc.SessionData_Check(usr(0), MD5Pass, siteNo, userLevel, authLimitDate, RemoteInf, userGroup, showName)

            dataAcc = Nothing
            If auth = True Then                                                         ' 認証OKの場合
                Dim dataFileAccess As New ClsReadDataFile
                Dim commonFiles() As ClsReadDataFile.DataFileInf = Nothing
                Dim fileLoop As Integer
                Dim rowLoop As Integer
                Dim sb As New StringBuilder
                Dim rowCount As Integer = 1
                Dim strTemp as String = ("センサー記号,単位,設置位置,管理値超過レベル,測定日時,測定データ" & Convert.ToChar(13) & Convert.ToChar(10))
                Dim dat As New dataProp.AllData

                sb.Append(strTemp)                                                      ' csv用の前準備

                If rqDate <> "0" Then
                    '' ---- 文字列から日時に変換するメソッドを記述予定 ----
                    ''Dim blnRet As Boolean = Date.TryParse(rqDate, getDate)
                    retString = "Error:Query parameter error"
                Else
                
                    Dim intRet As Integer = dataFileAccess.GetDataFileNames(IO.Path.Combine(siteFolder, "MenuInfo.mdb"), commonFiles)      ' 登録commonFileをすべて取得
                    Dim newestData() As Array = Nothing
                    Dim intTemp As Integer = 0
                    Dim strSQL As String
                
                    strSQL = "SELECT 計器記号,出力単位,設置位置,警報判定結果,警報判定時刻,最新データ FROM 共通情報 ORDER BY DataBaseCh ASC"
                    dat.NewestDatas = New Generic.List(Of dataProp.DataEntry)

                    Try
                        
                        For fileLoop = 0 To commonFiles.Length - 1
                            intTemp = dataFileAccess.GetLatestDataFromCommonInf(IO.Path.Combine(siteFolder, commonFiles(fileLoop).CommonInf), newestData, strSQL)   '*CommonFileからデータを取得する

                            Select Case dataType
                        
                                Case "XM", "JS"     '●XML or JSON シリアライズするもとのデータは共用できる
                                    For rowLoop = 0 To newestData.Length - 1
                                        Dim tempDat As New dataProp.DataEntry()
                                        tempDat.No = rowCount
                                        tempDat.SensorID = newestData(rowLoop).GetValue(0)
                                        tempDat.SensorUnit = newestData(rowLoop).GetValue(1)
                                        tempDat.Location = newestData(rowLoop).GetValue(2)
                                        tempDat.Level = newestData(rowLoop).GetValue(3)
                                        tempDat.MeasureDate = newestData(rowLoop).GetValue(4)
                                        tempDat.Data = newestData(rowLoop).GetValue(5)

                                        dat.NewestDatas.Add(tempDat)
                                        rowCount += 1

                                        tempDat = Nothing
                                    Next

                                Case "CS"           '●CSV
                                    Dim colLoop As Integer
                                    Dim temp(newestData(0).GetLength(0) - 1) As String
                                    For rowLoop = 0 To newestData.Length - 1
                                        For colLoop = 0 To newestData(0).GetLength(0) - 1
                                            temp(colLoop) = newestData(rowLoop).GetValue(colLoop)
                                        Next
                                        sb.Append(String.Join(",", temp))
                                        sb.Append(Convert.ToChar(13) & Convert.ToChar(10))
                                    Next

                                Case Else
                                    retString = "Error:Query parameter error"
                            End Select

                        Next

                    Catch ex As Exception

                    End Try

                    If intTemp > 0 Then
                        Select Case dataType                                    ' CSVはそのまま出す
                            Case "CS"

                                'CSVデータを生成する
                                retString = ("<pre>" & sb.ToString & "</pre>")
                                sb.Length = 0

                            Case "XM"

                                'XMLデータを生成する
                                Try

                                    Dim serializer1 As New XmlSerializer(GetType(dataProp.AllData))
                                    Dim ns As New XmlSerializerNamespaces           ' 名前空間が不要の場合はこれを入れる(<item xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"> のような記述がなくなる)
                                    ns.Add(String.Empty, String.Empty)

                                    ''メモリーストリーム()
                                    Using DatMS As New IO.MemoryStream
                                        Dim msData As Byte()
                                        Dim streamWriter As New IO.StreamWriter(DatMS, System.Text.Encoding.UTF8)
                                        serializer1.Serialize(streamWriter, dat, ns)
                                        msData = DatMS.ToArray
                                        retString = System.Text.Encoding.UTF8.GetString(msData)
                                        retString = retString.Replace("encoding=""utf-8""", "encoding=""UTF-8""")  ' まぁなくてもよいだろう・・

                                        ''Dim DatSW As New IO.StringWriter
                                        ''serializer1.Serialize(DatSW, dat, ns)                                     ' これだと<?xml version="1.0" encoding="utf-16"?>になってしまう
                                        ''retString = DatSW.ToString
                                    End Using
                                    
                                Catch ex As Exception

                                End Try

                                ''Dim seri As New System.Runtime.Serialization.DataContractSerializer(GetType(NewDatas()))
                                ''Dim sett As New System.Xml.XmlWriterSettings()
                                ''sett.Encoding = New System.Text.UTF8Encoding(False)
                                ''Dim datsb As New StringBuilder
                                ''Dim s As System.Xml.XmlWriter = System.Xml.XmlWriter.Create(dat, sett)
                                ' ''seri.WriteObject(s, dat)
                                ' ''serializer1.Serialize(s, dat)
                                ''s.WriteString(retString)

                                ''Dim mems As New IO.MemoryStream
                                ''Dim bytes() As Byte
                                ''serializer1.Serialize(mems, dat)
                                ''bytes = mems.ToArray()
                                ''mems.Close()

                            Case "JS"
                                'JSONデータを生成する
                                Dim jsonObj As New Generic.List(Of dataProp.AllData)
                                jsonObj.Add(dat)
                                Dim jsonStr As String = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj)

                                If jsonStr.StartsWith("[") = True Then                              ' "["で始まっている場合はそれを削除
                                    jsonStr = jsonStr.Substring(1)
                                End If
                                If jsonStr.EndsWith("]") = True Then                                ' "]"で終わっている場合はそれを削除
                                    jsonStr = jsonStr.Substring(0, jsonStr.Length - 1)
                                End If

                                retString = jsonStr.ToString

                        End Select

                    ElseIf intTemp = 2 Then

                        retString = "Error:Can not read datas."

                    End If

                End If
                'http://localhost:27607/WebDataSystem2/datarq.ashx?ui=339290333340383373382381377373340351373382381377373371381378366373290333276&ty=cs&dt=0&ct=h
                'http://localhost:27607/WebDataSystem2/datarq.ashx?ui=339290333340383373382381377373340351373382381377373371381378366373290333276&ty=xm&dt=0&ct=h
                'http://localhost:27607/WebDataSystem2/datarq.ashx?ui=368257366375348342349350346342375380342349350346342336350345333342257366&ty=js&dt=0&ct=h
            Else

                retString = "Error:Unauthorized"
            
            End If

        Else

            retString = "Error:Query parameter error"

        End If
            
        ''System.Diagnostics.Debug.WriteLine(context.Request.HttpMethod)

        If ContentType = "h" Then
            context.Response.ContentType = "text/html"
            If dataType = "JS" Then
                context.Response.ContentType = "application/json"               'RFC 4627
            ElseIf dataType = "XM" Then
                context.Response.ContentType = "application/xml"                'RFC2376
            End If
        Else
            context.Response.ContentType = "text/palin"
        End If

        If retString.StartsWith("Error:") = True Then
            context.Response.StatusCode = Net.HttpStatusCode.InternalServerError
        Else
            context.Response.StatusCode = Net.HttpStatusCode.OK
        End If

        retString = retString.Replace("空き", "")                               ' 単位が設定されていない場合に空白とする

        context.Response.Charset = "UTF-8"
        context.Response.Write(retString)

        context.Response.Flush()
        context.Response.SuppressContent = True                             ' 余計なごみ(HTML関連)がついてしまうのを回避する　Flushと一緒に利用

    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class

Namespace dataProp

    Public Class DataEntry

        Private _No As String
        Private _SensorID As String
        Private _SensorUnit As String
        Private _Location As String
        Private _Level As String
        Private _MeasureDate As String
        Private _Data As String

        Public Property No() As String
            Get
                Return _No
            End Get
            Set(ByVal value As String)
                _No = value
            End Set
        End Property
        Public Property SensorID() As String
            Get
                Return _SensorID
            End Get
            Set(ByVal value As String)
                _SensorID = value
            End Set
        End Property

        Public Property SensorUnit() As String
            Get
                Return _SensorUnit
            End Get
            Set(ByVal value As String)
                _SensorUnit = value
            End Set
        End Property

        Public Property Location() As String
            Get
                Return _Location
            End Get
            Set(ByVal value As String)
                _Location = value
            End Set
        End Property
        Public Property Level() As String
            Get
                Return _Level
            End Get
            Set(ByVal value As String)
                _Level = value
            End Set
        End Property
        Public Property MeasureDate() As String
            Get
                Return _MeasureDate
            End Get
            Set(ByVal value As String)
                _MeasureDate = value
            End Set
        End Property
        Public Property Data() As String
            Get
                Return _Data
            End Get
            Set(ByVal value As String)
                _Data = value
            End Set
        End Property

    End Class

    Public Class AllData

        Private Shared _XMLDatas As System.Collections.Generic.List(Of DataEntry)

        Public Property NewestDatas() As System.Collections.Generic.List(Of DataEntry)
            Get
                Return _XMLDatas
            End Get
            Set(ByVal value As System.Collections.Generic.List(Of DataEntry))
                _XMLDatas = value
            End Set
        End Property

    End Class

End Namespace