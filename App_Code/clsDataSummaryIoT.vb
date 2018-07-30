Option Explicit On
Option Strict On
''Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.OleDb
Imports System.Web
Imports System.Drawing

''' <summary>
''' ロガー情報のフィールド
''' </summary>
''' <remarks></remarks>
Public Class LoggerInf
    ''' <summary>
    ''' 水系名
    ''' </summary>
    Shadows Property RiverSysName As String = Nothing
    ''' <summary>
    ''' 河川名称
    ''' </summary>
    Shadows Property RiverName As String = Nothing
    ''' <summary>
    ''' SIM電話番号
    ''' </summary>
    Shadows Property TelNo As String = Nothing
    ''' <summary>
    ''' ロガーID
    ''' </summary>
    Shadows Property LoggerID As String = Nothing
    ''' <summary>
    ''' 観測局番号
    ''' </summary>
    Shadows Property ObsSt_No As Integer
    ''' <summary>
    ''' 観測局名
    ''' </summary>
    Shadows Property ObsSt_Name As String = Nothing
    ''' <summary>
    ''' 水位計名
    ''' </summary>
    Shadows Property WLName As String = Nothing
    ''' <summary>
    ''' 緯度（縦位置）
    ''' </summary>
    Shadows Property latitude As Single
    ''' <summary>
    ''' 経度（横位置）
    ''' </summary>
    Shadows Property longitude As Single
    ''' <summary>
    ''' 都道府県コード
    ''' </summary>
    Shadows Property PrefCode As Integer
    ''' <summary>
    ''' 市町村コード
    ''' </summary>
    Shadows Property MuniCode As Integer
    ''' <summary>
    ''' 係数
    ''' </summary>
    Shadows Property CoeffVal As Single
    ''' <summary>
    ''' 初期値
    ''' </summary>
    Shadows Property InitVal As Single
    ''' <summary>
    ''' 設置標高
    ''' </summary>
    Shadows Property SetElevation As Single
    ''' <summary>
    ''' 堤防(天端)高さ
    ''' </summary>
    Shadows Property EmbankmentHeight As Single
    ''' <summary>
    ''' 観測開始水位
    ''' </summary>
    Shadows Property ObsStHeigth As Single
    ''' <summary>
    ''' 観測停止水位
    ''' </summary>
    Shadows Property ObsEdHeigth As Single
    ''' <summary>
    ''' 水位演算種別
    ''' </summary>
    Shadows Property WLCalcType As Integer
    ''' <summary>
    ''' 河川種別
    ''' </summary>
    Shadows Property RiverType As Integer
    ''' <summary>
    ''' 送信遅延時間(s)
    ''' </summary>
    Shadows Property DelayTime As Integer
    ''' <summary>
    ''' アラート表示フラグ
    ''' </summary>
    Shadows Property AlertEnable As Boolean
    ''' <summary>
    ''' メンテナンスフラグ
    ''' </summary>
    Shadows Property MainteState As Integer
    ''' <summary>
    ''' メール送信フラグ
    ''' </summary>
    Shadows Property MailSendEnable As Boolean
    ''' <summary>
    ''' 設定更新フラグ － サーバー->水位計へのコマンド送信
    ''' </summary>
    Shadows Property UpdateFlg As Boolean
    ''' <summary>
    ''' 現在のモード
    ''' </summary>
    Shadows Property ObsModeNo As Integer
    ''' <summary>
    ''' アクセスキー(API用)
    ''' </summary>
    Shadows Property AccKey As String = Nothing
    ''' <summary>
    ''' 認証キー（API用）
    ''' </summary>
    Shadows Property AuthKey As String = Nothing
    ''' <summary>
    ''' 画面への表示フラグ
    ''' </summary>
    Shadows Property ShowFlg As Boolean
End Class

''' <summary>
''' ポイント表示用の色設定フィールド
''' </summary>
''' <remarks></remarks>
Public Class AlertLevel
    ''Property LvNo As Integer
    Shadows Property LvName As String()
    Shadows Property Red As Integer()
    Shadows Property Green As Integer()
    Shadows Property Blue As Integer()
    Shadows Property Alpha As Single()
    Shadows Property AlphaArr As Single()

    Sub New()
        ReDim LvName(4)     ' 平常、LV1、Lv2、Lv3、Lv4
        ReDim Red(4)
        ReDim Green(4)
        ReDim Blue(4)
        ReDim Alpha(4)
        ReDim AlphaArr(4)
    End Sub
End Class

''' <summary>
''' データのしきい値超過情報フィールド
''' </summary>
''' <remarks></remarks>
Public Class AlertInf
    ''' <summary>
    ''' ロガーID
    ''' </summary>
    Shadows Property LoggerID As String = Nothing
    Shadows Property Levels() As New AlertLevel

    ''Sub New()
    ''    ReDim Levels(4)
    ''End Sub
End Class

''' <summary>
''' チャンネルごとのデータ格納フィールド
''' </summary>
''' <remarks></remarks>
Public Class ChData
    Shadows Property Ch As Single()

    Sub New()
        ReDim Ch(9)
    End Sub
End Class

''' <summary>
''' チャンネルごとの単位格納フィールド
''' </summary>
''' <remarks></remarks>
Public Class ChDataInf
    Shadows Property ChUnit As String()

    Sub New()
        ReDim ChUnit(9)
    End Sub
End Class

''' <summary>
''' 顧客関連情報（識別番号）フィールド
''' </summary>
''' <remarks></remarks>
Public Class AreaInf
    ''' <summary>
    ''' 緯度（縦位置）
    ''' </summary>
    Shadows Property latitude As Single
    ''' <summary>
    ''' 経度（横位置）
    ''' </summary>
    Shadows Property longitude As Single
    ''' <summary>
    ''' 表示倍率
    ''' </summary>
    Shadows Property enlarge As Single
    ''' <summary>
    ''' 識別番号における、認証キー
    ''' </summary>
    Shadows Property AuthKey As String
    ''' <summary>
    ''' メールアドレス
    ''' </summary>
    Shadows Property MailAddress As String
End Class

''' <summary>
''' ロガー全般のデータフィールド
''' </summary>
''' <remarks></remarks>
Public Class LoggerData
    Shadows Property LoggerID As String = Nothing
    Shadows Property MeasDate As DateTime
    Shadows Property Data As New ChData
    Shadows Property LevelState As Integer
    Shadows Property Mainte As Integer
    Shadows Property AlertJude() As New ChData
    Shadows Property DataUnit As New ChDataInf
End Class

Public Class clsDataSummaryIoT

    Property DBFilePath As String
    Property IDNumber As String
    Property Loggers As New List(Of LoggerInf)()
    Property Datas As New List(Of LoggerData)()
    Property AlertSet As New List(Of AlertInf)()
    Property CustomerInf As New List(Of AreaInf)

    ''' <summary>
    ''' 現場の顧客情報取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReadCutomerInf(UserName As String) As Integer

        Dim intRet As Integer = -1
        Dim DBR As New DB_CRUD

        Dim WLGFile As String = DBFilePath

        DBR.QueryString = String.Format("SELECT * FROM AreaInfo WHERE 識別番号={0} AND ユーザー名='{1}' ORDER BY ID ASC", IDNumber, UserName)
        DBR.dbFilePath = DBFilePath
        DBR.getDBDatas()
        If DBR.exError Is Nothing Then
            Dim strGenba() As Array = CType(DBR.readClientData, Array())

            If strGenba.Length > 0 Then
                Dim tmpLog As New AreaInf


                tmpLog.latitude = Convert.ToSingle(strGenba(0).GetValue(3))                                 ' 初期位置 緯度
                tmpLog.longitude = Convert.ToSingle(strGenba(0).GetValue(4))                                ' 初期位置 経度
                tmpLog.enlarge = Convert.ToSingle(strGenba(0).GetValue(5))                                  ' 初期表示倍率
                tmpLog.AuthKey = strGenba(0).GetValue(6).ToString                                           ' 認証キー
                tmpLog.MailAddress = strGenba(0).GetValue(7).ToString                                       ' メールアドレス

                CustomerInf.Add(tmpLog)

                intRet = strGenba.Length - 1
            Else
                intRet = -1
            End If
        Else
            intRet = -1
        End If

        DBR = Nothing

        Return intRet

    End Function

    ''' <summary>
    ''' 表示色情報の取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReadAlertInf() As Integer

        Dim intRet As Integer = -1
        Dim DBR As New DB_CRUD

        Dim WLGFile As String = DBFilePath
        Dim rowLoop As Integer
        Dim LevelLoop As Integer

        DBR.QueryString = String.Format("SELECT * FROM AlertParam WHERE 識別番号={0} ORDER BY ID ASC", IDNumber)
        DBR.dbFilePath = DBFilePath
        DBR.getDBDatas()
        If DBR.exError Is Nothing Then
            Dim strGenba() As Array = CType(DBR.readClientData, Array())

            'For rowLoop = 0 To strGenba.Length - 1
            rowLoop = 0                                                                                     ' 原則１行しかないはず
            Dim tmpLog As New AlertInf
            Dim offset As Integer

            For LevelLoop = 0 To 4                                                                          ' 平常、Lv1,2,3を取得する
                offset = LevelLoop * 7
                Dim LvNo As Integer = Convert.ToInt32(strGenba(rowLoop).GetValue(2 + offset))
                'tmpLog.Levels(LevelLoop).LvNo = Convert.ToInt32(strGenba(rowLoop).GetValue(2 + offset))    ' レベル
                tmpLog.Levels.LvName(LvNo) = strGenba(rowLoop).GetValue(3 + offset).ToString                ' レベル名称
                tmpLog.Levels.Red(LvNo) = Convert.ToInt32(strGenba(rowLoop).GetValue(4 + offset))           ' 赤
                tmpLog.Levels.Green(LvNo) = Convert.ToInt32(strGenba(rowLoop).GetValue(5 + offset))         ' 緑
                tmpLog.Levels.Blue(LvNo) = Convert.ToInt32(strGenba(rowLoop).GetValue(6 + offset))          ' 青
                tmpLog.Levels.Alpha(LvNo) = Convert.ToSingle(strGenba(rowLoop).GetValue(7 + offset))        ' 透明度
                tmpLog.Levels.AlphaArr(LvNo) = Convert.ToSingle(strGenba(rowLoop).GetValue(8 + offset))     ' 透明度（三角）
            Next

            AlertSet.Add(tmpLog)
            'Next

            intRet = strGenba.Length - 1
        Else

            Dim clsLogWrite As New ClsGraphCommon
            Dim logDir As String = IO.Path.GetDirectoryName(DBFilePath)
            clsLogWrite.writeErrorLog(DBR.exError, IO.Path.Combine(logDir, "errorSummary.log"), "IoT", "IoT", MyBase.GetType.BaseType.FullName)

            intRet = -1
        End If

        DBR = Nothing

        Return intRet
    End Function

    ''' <summary>
    ''' 指定識別番号のロガー情報を全読みする
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReadLoggerInf() As Integer

        Dim intRet As Integer = -1
        Dim DBR As New DB_CRUD

        Dim LoggInf As New LoggerInf
        Dim WLGFile As String = DBFilePath
        Dim rowLoop As Integer

        DBR.QueryString = String.Format("SELECT * FROM SiteParam WHERE 識別番号={0} ORDER BY ID ASC", IDNumber)
        DBR.dbFilePath = DBFilePath
        DBR.getDBDatas()

        If DBR.exError Is Nothing Then
            Dim strGenba() As Array = CType(DBR.readClientData, Array())
            For rowLoop = 0 To strGenba.Length - 1
                Dim tmpLog As New LoggerInf

                tmpLog.RiverSysName = strGenba(rowLoop).GetValue(2).ToString            ' 水系名
                tmpLog.RiverName = strGenba(rowLoop).GetValue(3).ToString               ' 河川名
                tmpLog.TelNo = strGenba(rowLoop).GetValue(4).ToString                   ' 電話番号
                tmpLog.LoggerID = strGenba(rowLoop).GetValue(5).ToString                ' ロガーID
                tmpLog.ObsSt_No = Convert.ToInt32(strGenba(rowLoop).GetValue(6))        ' 観測局番号
                tmpLog.ObsSt_Name = strGenba(rowLoop).GetValue(7).ToString              ' 観測局名
                tmpLog.WLName = strGenba(rowLoop).GetValue(8).ToString                  ' 水位計名
                tmpLog.latitude = Convert.ToSingle(strGenba(rowLoop).GetValue(9))       ' 緯度
                tmpLog.longitude = Convert.ToSingle(strGenba(rowLoop).GetValue(10))     ' 経度
                tmpLog.PrefCode = Convert.ToInt32(strGenba(rowLoop).GetValue(11))       ' 都道府県コード
                tmpLog.MuniCode = Convert.ToInt32(strGenba(rowLoop).GetValue(12))       ' 市町村コード
                tmpLog.CoeffVal = Convert.ToSingle(strGenba(rowLoop).GetValue(13))      ' 係数
                tmpLog.InitVal = Convert.ToSingle(strGenba(rowLoop).GetValue(14))       ' 初期値
                tmpLog.SetElevation = Convert.ToSingle(strGenba(rowLoop).GetValue(15))      ' 設置高
                tmpLog.EmbankmentHeight = Convert.ToSingle(strGenba(rowLoop).GetValue(16))  ' 基準高
                tmpLog.ObsStHeigth = Convert.ToSingle(strGenba(rowLoop).GetValue(17))   ' 観測開始水位
                tmpLog.ObsEdHeigth = Convert.ToSingle(strGenba(rowLoop).GetValue(18))   ' 観測停止水位
                tmpLog.WLCalcType = Convert.ToInt32(strGenba(rowLoop).GetValue(19))     ' 水位演算種別
                tmpLog.RiverType = Convert.ToInt32(strGenba(rowLoop).GetValue(20))      ' 河川種別
                tmpLog.DelayTime = Convert.ToInt32(strGenba(rowLoop).GetValue(21))      ' 送信遅延時間
                tmpLog.AlertEnable = Convert.ToBoolean(strGenba(rowLoop).GetValue(22))  ' アラート表示
                tmpLog.MainteState = Convert.ToInt32(strGenba(rowLoop).GetValue(23))    ' メンテナンスモード
                tmpLog.MailSendEnable = Convert.ToBoolean(strGenba(rowLoop).GetValue(24))   ' メール通知
                tmpLog.UpdateFlg = Convert.ToBoolean(strGenba(rowLoop).GetValue(25))    ' IoT水位計への設定送信フラグ
                tmpLog.ObsModeNo = Convert.ToInt32(strGenba(rowLoop).GetValue(26))      ' 現在の測定モード
                tmpLog.AccKey = strGenba(rowLoop).GetValue(27).ToString                 ' ロガーアクセスキー
                tmpLog.AuthKey = strGenba(rowLoop).GetValue(28).ToString                ' 認証キー
                tmpLog.ShowFlg = Convert.ToBoolean(strGenba(rowLoop).GetValue(29))      ' 表示フラグ

                Loggers.Add(tmpLog)
            Next

            intRet = strGenba.Length - 1
        Else
            intRet = -1
        End If

        DBR = Nothing

        Return intRet

    End Function

    ''' <summary>
    ''' 最新データ一覧から、識別番号が合致するデータを読み込む
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReadWLNewData() As Integer

        Dim intRet As Integer = -1
        Dim DBR As New DB_CRUD

        Dim WLGFile As String = DBFilePath
        Dim rowLoop As Integer
        Dim chLoop As Integer

        DBR.QueryString = String.Format("SELECT * FROM NewData WHERE 識別番号={0} ORDER BY ID ASC", IDNumber)
        DBR.dbFilePath = DBFilePath
        DBR.getDBDatas()

        If DBR.exError Is Nothing Then
            Dim strGenba() As Array = CType(DBR.readClientData, Array())
            For rowLoop = 0 To strGenba.Length - 1
                Dim tmpLog As New LoggerData

                tmpLog.LoggerID = strGenba(rowLoop).GetValue(2).ToString                    ' ロガーID
                tmpLog.MeasDate = Convert.ToDateTime(strGenba(rowLoop).GetValue(3))         ' 測定日時
                For chLoop = 0 To 9                                                         ' 測定データ
                    tmpLog.Data.Ch(chLoop) = Convert.ToSingle(strGenba(rowLoop).GetValue(chLoop + 4))
                Next
                tmpLog.LevelState = Convert.ToInt32(strGenba(rowLoop).GetValue(14))         ' 水位レベル状況
                tmpLog.Mainte = Convert.ToInt32(strGenba(rowLoop).GetValue(15))             ' メンテナンス状況　
                For chLoop = 0 To 0
                    tmpLog.AlertJude.Ch(chLoop) = Convert.ToInt32(strGenba(rowLoop).GetValue(chLoop + 16))    ' 管理値判定レベル
                Next

                Datas.Add(tmpLog)
            Next

            intRet = strGenba.Length - 1
        End If

        DBR = Nothing

        Return intRet

    End Function

    ''' <summary>
    ''' 測定値の単位情報の取得とリストへの格納
    ''' </summary>
    ''' <returns>成否　0:否　0以外:成功（レコード数）</returns>
    ''' <remarks></remarks>
    Public Function ReadUnitInf() As Integer

        Dim intRet As Integer = -1
        Dim DBR As New DB_CRUD

        Dim WLGFile As String = DBFilePath
        Dim rowLoop As Integer
        Dim chLoop As Integer

        DBR.QueryString = String.Format("SELECT * FROM Unit WHERE 識別番号={0} ORDER BY ID ASC", IDNumber)
        DBR.dbFilePath = DBFilePath
        DBR.getDBDatas()

        If DBR.exError Is Nothing Then
            Dim strGenba() As Array = CType(DBR.readClientData, Array())
            For rowLoop = 0 To strGenba.Length - 1
                Dim LoggerID As String = strGenba(rowLoop).GetValue(2).ToString
                Dim DataIndex As Integer = SearchLoggerIDFromNewData(LoggerID)               'ロガー情報からIDの該当するリストのインデックスを取得

                For chLoop = 0 To 9                                                         ' 測定データ
                    Datas(DataIndex).DataUnit.ChUnit(chLoop) = strGenba(rowLoop).GetValue(chLoop + 3).ToString
                Next chLoop
            Next

            intRet = strGenba.Length - 1
        End If

        Return intRet

    End Function


    ''' <summary>
    ''' 観測点情報フィールドから、該当のIDのインデックスを検索する
    ''' </summary>
    ''' <param name="LogId">検索するロガーID</param>
    ''' <returns>観測点情報の該当ロガーIDのインデックス</returns>
    ''' <remarks></remarks>
    Private Function SearchLoggerIDFromObserbationInfo(LogId As String) As Integer

        Dim intRet As Integer = -1

        intRet = Loggers.FindIndex(Function(s) s.LoggerID.StartsWith(LogId))

        Return intRet

    End Function

    ''' <summary>
    ''' 最新値データフィールドから、該当のIDのインデックスを検索する
    ''' </summary>
    ''' <param name="LogId">検索するロガーID</param>
    ''' <returns>観測点情報の該当ロガーIDのインデックス</returns>
    ''' <remarks></remarks>
    Private Function SearchLoggerIDFromNewData(LogId As String) As Integer

        Dim intRet As Integer = -1

        intRet = Datas.FindIndex(Function(s) s.LoggerID.StartsWith(LogId))

        Return intRet

    End Function

    ''' <summary>
    ''' データ一覧用のULを作成しプロパティを設定（マーカー作成）　System.Web.UI.HtmlControls.HtmlGenericControl
    ''' </summary>
    ''' <param name="FormObj">ラベルを表示するフォームオブジェクト</param>
    Public Sub DynamicMakeObject(ByVal FormObj As System.Web.UI.HtmlControls.HtmlGenericControl, ByVal UL As BulletedList, UserLevel As Integer)
        '    Public Sub DynamicMakeLabels(ByVal FormObj As System.Web.UI.HtmlControls.HtmlForm, ByVal showDatas() As ShowDataSet, ByVal showDataInf() As Array, _
        ''
        '' ULを動的に作成する　データ一覧用
        ''

        Dim PointLoop As Integer
        Dim obsCount As Integer = 0
        Dim tipTemp As String = "<tr><td>{0}</td><td>：</td><td>{1}</td></tr>"
        Dim tipTemp2 As String = "<tr><td>{0}</td><td>：</td><td id='{1}{2}'>{3}</td></tr>"
        Dim WLState() As String = {"↓", "→", "↑"}
        Dim WLArrState() As String = {"arrDown", "arrNor", "arrUp"}
        Dim mainteState() As String = {"", "メンテナンス中", "休止中", "欠測"}

        For PointLoop = 0 To Datas.Count - 1                                                ' ロガーの台数分ループする
            Dim obsIndex As Integer = SearchLoggerIDFromObserbationInfo(Datas(PointLoop).LoggerID)  'ロガー情報からIDの該当するリストのインデックスを取得

            If Loggers(obsIndex).ShowFlg = True Then                                        ' 表示フラグの確認

                Dim DataList As New ListItem

                DataList.Attributes.Add("id", String.Format("obsArr{0:d3}", obsCount))                                                      ' オブジェクトID
                DataList.Attributes.Add("eqid", Datas(obsCount).LoggerID)                                                                   ' ロガーID
                DataList.Attributes.Add("mesdt", Datas(PointLoop).MeasDate.ToString("yyyy/MM/dd HH:mm"))
                'DataList.Text = String.Format("{0} {1}", Loggers(obsIndex).ObsSt_Name, Loggers(obsIndex).RiverName)
                DataList.Attributes.Add("style", String.Format("top:{0};left:{1};z-index:{2};", Unit.Parse(Loggers(obsIndex).latitude.ToString).ToString,
                                                               Unit.Parse(Loggers(obsIndex).longitude.ToString).ToString, (10).ToString))   ' 表示位置指定
                DataList.Attributes.Add("alv", Datas(obsCount).AlertJude.Ch(0).ToString)                                                    ' AlartLevel

                Dim TriCss As String = String.Format("arr{0} ", Datas(obsCount).AlertJude.Ch(0))                                           ' ポイント矢印の色、向きなど
                TriCss &= WLArrState(Datas(PointLoop).LevelState)
                DataList.Attributes.Add("class", String.Format("liSet {0}", TriCss))                                                        ' cssとして、 liSet + αを記述
                'DataList.EnableViewState = False

                Dim sbtitle As New StringBuilder                                                                                            ' Tooltip情報テーブル作成
                sbtitle.Capacity = 1000
                sbtitle.Append(String.Format("<table id='tbl{0}'>", obsCount))
                sbtitle.Append(String.Format("<thead><tr><td colspan='3'>{0}</td></tr></thead><tbody>", String.Format("{0} {1}", Loggers(obsIndex).ObsSt_Name, Loggers(obsIndex).RiverName)))
                sbtitle.Append(String.Format(tipTemp, "観測局名", Loggers(obsIndex).ObsSt_Name))
                sbtitle.Append(String.Format(tipTemp, "水系名", Loggers(obsIndex).RiverSysName))
                sbtitle.Append(String.Format(tipTemp, "河川名", Loggers(obsIndex).RiverName))
                sbtitle.Append(String.Format(tipTemp2, "測定日時", "mesdt", obsCount, Datas(PointLoop).MeasDate.ToString("yyyy/MM/dd HH:mm")))

                If UserLevel >= 15 Or (Datas(PointLoop).Mainte = 0 And Loggers(obsIndex).MainteState = 0) Then
                    '管理者もしくは、それ以下の権限でメンテフラグがない場合は常にデータを表示する
                    If Datas(PointLoop).Data.Ch(0) < 1.1E+30 Then
                        sbtitle.Append(String.Format(tipTemp2, String.Format("水位({0})", Datas(PointLoop).DataUnit.ChUnit(0)), "WL", obsCount, Datas(PointLoop).Data.Ch(0).ToString("#0.00")))
                    Else
                        sbtitle.Append(String.Format(tipTemp2, String.Format("水位({0})", Datas(PointLoop).DataUnit.ChUnit(0)), "WL", obsCount, "異常値"))
                    End If
                    sbtitle.Append(String.Format(tipTemp2, "水位変化", "wlst", obsCount, WLState(Datas(PointLoop).LevelState)))
                Else
                    '管理者以下でメンテナンスフラグがあった場合は、データを表示しない
                    If Datas(PointLoop).Mainte > 0 OrElse Loggers(obsIndex).MainteState > 0 Then
                        Dim mainteFlg As Integer
                        If Datas(PointLoop).Mainte > 0 Then
                            mainteFlg = Datas(PointLoop).Mainte
                        Else
                            mainteFlg = Loggers(obsIndex).MainteState
                        End If
                        sbtitle.Append(String.Format(tipTemp2, String.Format("水位({0})", Datas(PointLoop).DataUnit.ChUnit(0)), "WL", obsCount, mainteState(mainteFlg)))
                        sbtitle.Append(String.Format(tipTemp2, "水位変化", "wlst", obsCount, "-"))

                    Else

                        'メンテフラグがない場合は、データ表示もしくは、欠測
                        If Datas(PointLoop).Data.Ch(0) < 1.1E+30 Then
                            sbtitle.Append(String.Format(tipTemp2, String.Format("水位({0})", Datas(PointLoop).DataUnit.ChUnit(0)), "WL", obsCount, Datas(PointLoop).Data.Ch(0).ToString("#0.00")))
                        Else
                            sbtitle.Append(String.Format(tipTemp2, String.Format("水位({0})", Datas(PointLoop).DataUnit.ChUnit(0)), "WL", obsCount, "欠測"))
                        End If
                        sbtitle.Append(String.Format(tipTemp2, "水位変化", "wlst", obsCount, WLState(Datas(PointLoop).LevelState)))
                    End If
                End If

                sbtitle.Append(String.Format(tipTemp, String.Format("観測開始水位({0})", Datas(PointLoop).DataUnit.ChUnit(0)), Loggers(obsIndex).ObsStHeigth.ToString("#0.00")))

                ' 管理者向け
                If UserLevel >= 15 Then
                    sbtitle.Append(String.Format(tipTemp2, String.Format("装置温度({0})", Datas(PointLoop).DataUnit.ChUnit(1)), "temp", obsCount, Datas(PointLoop).Data.Ch(1).ToString("#0.00")))
                    sbtitle.Append(String.Format(tipTemp2, String.Format("電池電圧({0})", Datas(PointLoop).DataUnit.ChUnit(2)), "volt", obsCount, Datas(PointLoop).Data.Ch(2).ToString("#0.00")))
                End If
                sbtitle.Append("</tbody></table>")

                DataList.Attributes.Add("title", sbtitle.ToString)
                sbtitle.Length = 0

                UL.Items.Add(DataList)

            End If
            obsCount += 1

        Next

        UL.EnableViewState = False


        'Dim LeftOffset As Integer = Convert.ToInt32(FormObj.Style("margin-left").Replace("px", ""))

        '        If alertCount <> 0 Then
        '            ''PicTop = PicTop + 21 * (alertCount - 1)                                                       ''警報状況の表示分をオフセット
        '            PicTop = Convert.ToSingle(PicTop + (21 * (alertCount - 1) * 0.98))                              ''警報状況の表示分をオフセット
        '        End If

        '        For jloop = 0 To dataFileCount - 1
        '            For iloop = 0 To showDatas(jloop).DataCount - 1
        '                If Convert.ToBoolean(showDataInf(DummyCount).GetValue(15)) = True And
        '                    IO.File.Exists(IO.Path.Combine(DataFilePath, "App_Data", DataFileNeme(Convert.ToInt32(showDataInf(DummyCount).GetValue(13))).CommonInf)) = True Then      '表示がTrueなら処理を行なう
        '                    ShowCh = CType(showDataInf(DummyCount).GetValue(2), Integer)                             '表示するデータベースチャンネル
        '                    ShowRec = GetDBChRecordNo(jloop, showDatas, ShowCh)                                 '表示するデータベースチャンネルをデータの中のレコードから検索しレコード番号を取得
        '                    'Dim DataLabel As New System.Web.UI.WebControls.Label
        '                    Using DataLabel As New System.Web.UI.WebControls.Label
        '                        DataFormat = showDataInf(DummyCount).GetValue(14).ToString
        '                        If showDatas(jloop).DataSet(ShowRec) Is Nothing Then GoTo NextStep
        '                        AlertLevel = Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(17))
        '                        AlertWord = ""
        '                        If AlertLevel <> 0 Then
        '                            Try                                                                             ' 2018/03/02 Kino Add 例外処理追加　警報判定フラグのレコード数が足りない場合の処理
        '                                If alertJudgeNo(ShowCh, jloop) = 0 Then
        '                                    AlertLevel = 0
        '                                    AlertWord = "警報判定除外"
        '                                ElseIf alertJudgeNo(ShowCh, jloop) > Math.Abs(AlertLevel) Then
        '                                    AlertLevel = 0
        '                                    AlertWord = "警報判定" + alertJudgeNo(ShowCh, jloop).ToString + "次以降"
        '                                End If
        '                            Catch ex As Exception

        '                            End Try
        '                        End If

        '                        SelCol = Convert.ToInt32(showDataInf(DummyCount).GetValue(8))
        '                        sufix = ""
        '                        With DataLabel
        '                            Select Case SelCol          'showDataInf(DummyCount).GetValue(8)
        '                                Case 0                                                                      '●任意文字列指定
        '                                    LabelData = showDataInf(DummyCount).GetValue(10).ToString
        '                                    .Text = LabelData
        '                                    CharactorFlg = 1                                                        '   文字列フラグを立てる
        '                                Case 3 To 6, 18                                                             '●計器記号、出力、種別、ブロックタイトル、測定時刻
        '                                    LabelData = showDatas(jloop).DataSet(ShowRec).GetValue(Convert.ToInt32(showDataInf(DummyCount).GetValue(8))).ToString
        '                                    .Text = LabelData
        '                                    CharactorFlg = 1                                                        '   文字列フラグを立てる
        '                                Case 19                                                                     '●測定値
        '                                    CalcedData = Convert.ToSingle(showDatas(jloop).DataSet(ShowRec).GetValue(Convert.ToInt32(showDataInf(DummyCount).GetValue(8))))
        '                                    If CalcedData > 1.1E+30 Then
        '                                        .Text = "**"
        '                                        ShowData = "異常値もしくは欠測"
        '                                        AlertWord = "－"
        '                                    Else
        '                                        changeValue = clsAlert.trunc_round(CalcedData, DataFormat, Convert.ToBoolean(showDataInf(DummyCount).GetValue(18)))        '' 四捨五入／切捨て処理と符合付加を行う 2009/06/11 Kino Changed
        '                                        .Text = PreFix & changeValue    'CalcedData.ToString("+" & DataFormat & ";-" & DataFormat) '& ";+" & DataFormat)
        '                                        ShowData = (clsAlert.trunc_round(CalcedData, (DataFormat & "00"), Convert.ToBoolean(showDataInf(DummyCount).GetValue(18))) & " " & showDatas(jloop).DataSet(ShowRec).GetValue(4).ToString)  ' 2011/05/25 Kino Changed
        '                                        If AlertWord.Length = 0 Then
        '                                            AlertWord = AlertInfo(AlertLevel + 3).Words                                     'clsAlert.AlertData(AlertLevel + 3).Words
        '                                        End If
        '                                    End If
        '                                    CharactorFlg = 0
        '                                Case 20
        '                                    LabelData = showDatas(jloop).DataSet(ShowRec).GetValue(3).ToString                      '● 転倒センサ　識別記号
        '                                    .Text = LabelData
        '                                    CharactorFlg = 2
        '                                Case 21                                                                                     '● 転倒センサ 健全性確認
        '                                    If Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(19)) > 1 Then showDatas(jloop).DataSet(ShowRec).SetValue(1, 19) ' 2012/03/12 Kino Add 米坂線対応(複合転倒判定対応_転倒済みの判定は不可)
        '                                    Dim refCh As Short = Convert.ToInt16(showDatas(jloop).DataSet(ShowRec).GetValue(19))
        '                                    If refCh >= 0 And refCh <= 1 Then
        '                                        LabelData = follState(refCh + 3)
        '                                        .Text = LabelData
        '                                    End If
        '                                    CharactorFlg = 3
        '                                Case Else                                                                   '●上記以外
        '                                    CalcedData = Convert.ToSingle(showDatas(jloop).DataSet(ShowRec).GetValue(Convert.ToInt32(showDataInf(DummyCount).GetValue(8))))
        '                                    .Text = (PreFix + CalcedData.ToString("+" + DataFormat + ";-" + DataFormat + ";+" + DataFormat))
        '                                    ShowData = CalcedData.ToString(DataFormat + "00 ") & showDatas(jloop).DataSet(ShowRec).GetValue(4).ToString
        '                                    AlertWord = "－"
        '                                    CharactorFlg = 0
        '                            End Select

        '                            IDPrefix = showDataInf(DummyCount).GetValue(1).ToString                         ' 2015/11/19 Kino Changed DOM対応でIDにサフィックスを付与(Ch)　DOMに関係ないものは今まで通り
        '                            If IDPrefix.IndexOf("NoTip") > -1 AndAlso SelCol = 0 Then
        '                                .ID = (showDataInf(DummyCount).GetValue(1).ToString & DummyCount.ToString)
        '                            Else
        '                                .ID = (showDataInf(DummyCount).GetValue(1).ToString & DummyCount.ToString)  '& "_F" & jloop.ToString & "_Ch" & ShowCh.ToString)
        '                            End If

        '                            If showDataInf(DummyCount).GetValue(1).ToString.IndexOf("NoTip") <> -1 Then     ' 2012/02/22 Kino Add
        '                                CharactorFlg = 4
        '                                AlertLevel = 0
        '                            End If

        '                            .Font.Size = CType(showDataInf(DummyCount).GetValue(7), Web.UI.WebControls.FontSize)
        '                            .BorderStyle = CType(showDataInf(DummyCount).GetValue(11), Web.UI.WebControls.BorderStyle)
        '                            If fieldCount = 18 Then                                                                     ' 2012/02/10 Kino Add
        '                                .BorderColor = Color.FromArgb(-4144960)
        '                            Else
        '                                .BorderColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(19)))                     '' これにすると新しいBrowserにもBorderStyleが対応
        '                            End If
        '                            .BorderWidth = CInt(showDataInf(DummyCount).GetValue(12))
        '                            .EnableViewState = False

        '                            RGBColor(0) = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17))).R.ToString               '' 2016/06/28 Kino Add 透過背景を作成するための文字列
        '                            RGBColor(1) = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17))).G.ToString
        '                            RGBColor(2) = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17))).B.ToString
        '                            RGBColor(3) = (Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17))).A / 255).ToString("0.00")

        '                            Select Case CharactorFlg
        '                                Case 0, 2, 3
        '                                    If AlertInfo(AlertLevel + 3).BackColor = 0 Then                                     '' 警報判定しない場合(MenuInfo.mdb)
        '                                        .BackColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(17)))
        '                                        AlertWord = "-"
        '                                    Else
        '                                        .BackColor = Color.FromArgb(AlertInfo(AlertLevel + 3).BackColor)                    ''clsAlert.AlertData(AlertLevel + 3).BackColor)
        '                                    End If

        '                                    If AlertInfo(AlertLevel + 3).ForeColor = 0 Then                                     '' 警報判定しない場合(MenuInfo.mdb)
        '                                        .ForeColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(16)))
        '                                    Else
        '                                        .ForeColor = Color.FromArgb(AlertInfo(AlertLevel + 3).ForeColor)                    ''clsAlert.AlertData(AlertLevel + 3).ForeColor)
        '                                    End If
        '                                    Select Case CharactorFlg
        '                                        Case 0                  ' 通常センサ
        '                                            .ToolTip = (
        '                                                String.Format("計器記号： {0}<br>", showDatas(jloop).DataSet(ShowRec).GetValue(3).ToString) &
        '                                                String.Format("数値詳細： {0}<br>", ShowData) &
        '                                                String.Format("警報判定： 【{0}】<br><br>", AlertWord) &
        '                                                String.Format("測定時刻： {0}", showDatas(jloop).DataSet(ShowRec).GetValue(18))
        '                                                )
        '                                        Case 2                  ' 転倒センサ
        '                                            If Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(19)) > 2 Then showDatas(jloop).DataSet(ShowRec).SetValue(2, 19) ' 2012/03/12 Kino Add 米坂線対応(複合転倒判定対応_転倒済みの判定は不可)
        '                                            .ToolTip = (
        '                                                String.Format("識別記号: {0}<br>", showDatas(jloop).DataSet(ShowRec).GetValue(3)) &
        '                                                String.Format("状　　態: {0}<br>", follState(Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(19)))) &
        '                                                String.Format("通知時刻: {0}", showDatas(jloop).DataSet(ShowRec).GetValue(18))
        '                                                )
        '                                        Case 3                  ' 転倒センサ 健全性
        '                                            If Convert.ToInt32(showDatas(jloop).DataSet(ShowRec).GetValue(19)) > 1 Then showDatas(jloop).DataSet(ShowRec).SetValue(1, 19) ' 2012/03/12 Kino Add 米坂線対応(複合転倒判定対応_転倒済みの判定は不可)
        '                                            LabelData = LabelData.Replace("×", "通知異常")
        '                                            LabelData = LabelData.Replace("○", "正常")
        '                                            .ToolTip = (
        '                                                String.Format("識別記号： {0}<br>", showDatas(jloop).DataSet(ShowRec).GetValue(3)) &
        '                                                String.Format("健全性　： {0}<br>", LabelData) &
        '                                                String.Format("通知時刻： {0}", showDatas(jloop).DataSet(ShowRec).GetValue(18))
        '                                            )

        '                                        Case 4          'NoTips
        '                                            .ToolTip = ""
        '                                    End Select
        '                                Case 1
        '                                    .Style("background-color") = String.Format("rgba({0},{1},{2},{3})", RGBColor)               ' 2016/06/28 Kino Add
        '                                    .ForeColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(16)))
        '                                    .ToolTip = showDataInf(DummyCount).GetValue(10).ToString
        '                                Case Else                                                                                       ' 文字列等
        '                                    .Style("background-color") = String.Format("rgba({0},{1},{2},{3})", RGBColor)               ' 2016/06/28 Kino Add
        '                                    .ForeColor = Color.FromArgb(Convert.ToInt32(showDataInf(DummyCount).GetValue(16)))
        '                                    .ToolTip = ""
        '                            End Select

        '                            .Style("Position") = "Absolute"
        '                            If Convert.ToInt32(showDataInf(DummyCount).GetValue(6)) <> 0 Then
        '                                .Style("width") = (showDataInf(DummyCount).GetValue(6).ToString + "px")
        '                            End If
        '                            If Convert.ToInt32(showDataInf(DummyCount).GetValue(5)) <> 0 Then
        '                                .Style("height") = (showDataInf(DummyCount).GetValue(5).ToString + "px")
        '                                If showDataInf(DummyCount).GetValue(9).ToString.IndexOf("M") > 0 Then
        '                                    .Style("line-height") = (showDataInf(DummyCount).GetValue(5).ToString + "px")               ' 2015/05/14 Kino Add 縦位置センターのおまじない
        '                                End If
        '                            End If
        '                            .Style("left") = Unit.Parse((Convert.ToInt32(showDataInf(DummyCount).GetValue(4)) + labelOffset.X - LeftOffset).ToString).ToString
        '                            .Style("top") = Unit.Parse((Convert.ToInt32(showDataInf(DummyCount).GetValue(3)) + labelOffset.Y + PicTop).ToString).ToString
        '                            If AlertLevel <> 0 And CharactorFlg = 0 Then
        '                                sufix = "B"                                                                     ' LCe,LLe,LRi を最後に設定する必要があるが、これで警報時は太字になる
        '                            End If
        '                            .Attributes.Add("class", showDataInf(DummyCount).GetValue(9).ToString + sufix)

        '                            If IO.File.Exists(IO.Path.Combine(DataFilePath, "App_Data\BlinkOn.flg")) = True AndAlso SelCol = 19 Then        ' 2018/03/12 Kino Add
        '                                sufix &= String.Format(" blinkNo{0}", Math.Abs(AlertLevel))
        '                            End If
        '                            FormObj.Controls.Add(DataLabel)
        '                        End With
        '                    End Using
        '                End If
        'NextStep:
        '                DummyCount += 1
        '            Next iloop
        '        Next jloop

        'clsAlert = Nothing

    End Sub


    ''' <summary>
    ''' 指定色の反対色、補色を求める
    ''' </summary>
    ''' <param name="srcColor">元の色</param>
    ''' <param name="InvertFlg">反対色：True　補色：False</param>
    ''' <returns>計算した色</returns>
    ''' <remarks></remarks>
    Public Function complementaryColors(ByVal srcColor As Color, Optional ByVal InvertFlg As Boolean = False) As Color

        Dim strRet As Color
        Dim sR As Integer = System.Drawing.Color.FromArgb(srcColor.R).ToArgb
        Dim sG As Integer = System.Drawing.Color.FromArgb(srcColor.G).ToArgb
        Dim sB As Integer = System.Drawing.Color.FromArgb(srcColor.B).ToArgb
        Dim sA As Integer = System.Drawing.Color.FromArgb(srcColor.A).ToArgb

        If InvertFlg = True Then                                                               ' 反転色

            strRet = Color.FromArgb(sA, (255 - sR), (255 - sG), (255 - sB))

        Else                                                                                    ' 補色

            Dim MaxVal As Integer = Math.Max(Math.Max(sR, sG), sB)
            Dim MinVal As Integer = Math.Min(Math.Min(sR, sG), sB)
            Dim TotalVal As Integer = (MaxVal + MinVal)

            strRet = Color.FromArgb((TotalVal - sR), (TotalVal - sG), (TotalVal - sB))

        End If

        Return strRet

    End Function


End Class


