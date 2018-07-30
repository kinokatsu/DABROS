'Imports Microsoft.VisualBasic

Public Class ClsGraphCommon

    Public Sub SetSelectDateScript(ByVal FormObj As System.Web.UI.HtmlControls.HtmlForm)
        ''
        '' コントロールのイベントにJavaScriptの関数を割り当てる
        ''
        ''Dim TxtSt As TextBox = CType(FormObj.FindControl("TxtStartDate"), TextBox)
        ''Dim TxtEd As TextBox = CType(FormObj.FindControl("TxtEndDate"), TextBox)
        Dim ImgSt As Image = CType(FormObj.FindControl("imgCalTxtStartDate"), Image)
        Dim ImgEd As Image = CType(FormObj.FindControl("imgCalTxtEndDate"), Image)
        Dim RdbDsign As RadioButton = CType(FormObj.FindControl("RdBDsignatedDate"), RadioButton)
        Dim RdbNewest As RadioButton = CType(FormObj.FindControl("RdbFromNewest"), RadioButton)

        'カレンダー表示の設定
        ''TxtSt.Attributes.Add("onblur", "validateDate(this);")
        ''TxtEd.Attributes.Add("onblur", "validateDate(this);")
        If ImgSt IsNot Nothing Then
            ImgSt.Attributes.Add("onclick", "DropCalendar(this);")        ' 2012/11/02 Kino Changed jQuery
        End If
        If ImgEd IsNot Nothing Then
            ImgEd.Attributes.Add("onclick", "DropCalendar(this);")
        End If

        ''UpdatePanelからJavascriptへ制御を移行した
        If RdbDsign IsNot Nothing Then
            RdbDsign.Attributes.Add("onclick", "ChangeSelState(this);")
        End If

        If RdbNewest IsNot Nothing Then
            RdbNewest.Attributes.Add("onclick", "ChangeSelState(this);")
        End If

    End Sub

    Public Sub CheckAutoUpdate(ByVal FormObj As System.Web.UI.HtmlControls.HtmlForm, ByVal SiteDirectory As String, ByVal GrpType As String)
        ''
        '' タイマーの設定情報読み込みと設定
        ''

        Dim tmrobj As Timer = CType(FormObj.FindControl("Tmr_Update"), Timer)
        Dim intInterval As Integer '= 600000
        Dim blnAutoUpdate As Boolean '= False
        Dim clsUpdate As New ClsReadDataFile

        clsUpdate.CheckAutoUpdate(SiteDirectory, GrpType, intInterval, blnAutoUpdate)

        'タイマーの有効／無効を設定
        tmrobj.Enabled = blnAutoUpdate
        tmrobj.Interval = intInterval

        clsUpdate = Nothing

    End Sub

    Public Sub SetSelectDateScrpt_Depth(ByVal FormObj As System.Web.UI.HtmlControls.HtmlForm, Optional ByVal TxtCount As Integer = 0)
        ''
        '' コントロールのイベントにJavaScriptの関数を割り当てる　深度分布図用
        ''
        ''Dim TxtSt As TextBox = CType(FormObj.FindControl("TxtStartDate"), TextBox)
        ''Dim TxtEd As TextBox = CType(FormObj.FindControl("TxtEndDate"), TextBox)
        ''Dim ImgSt As Image = CType(FormObj.FindControl("ImgStDate"), Image)
        ''Dim ImgEd As Image = CType(FormObj.FindControl("ImgEdDate"), Image)
        Dim RdbDsign As RadioButton = CType(FormObj.FindControl("RdBDsignatedDate"), RadioButton)
        Dim RdbNewest As RadioButton = CType(FormObj.FindControl("RdbFromNewest"), RadioButton)
        ''Dim TextDate As TextBox
        ''Dim ImageBt As Image
        ''Dim iloop As Integer

        RdbDsign.Attributes.Add("onclick", "ChangeSelState(this);")
        RdbNewest.Attributes.Add("onclick", "ChangeSelState(this);")

        ''For iloop = 0 To TxtCount
        ''    TextDate = FormObj.FindControl("TxtDate" & iloop.ToString)
        ''    ImageBt = FormObj.FindControl("imgDate" & iloop.ToString)

        ''    TextDate.Attributes.Add("onblur", "validateDate(this);")

        ''    TextDate.Attributes.Add("onblur", "validateDate(" & "TxtDate" & iloop.ToString & ");")
        ''    ImageBt.Attributes.Add("onclick", "DropCalendar(this);")
        ''Next

    End Sub

    Public Sub writeErrorLog(ByVal ex As Exception, ByVal FilePath As String, ByVal UN As String, ByVal SD As String, ByVal procName As String)

        If ex IsNot Nothing Then
            Using sw As New IO.StreamWriter(FilePath, True, Encoding.GetEncoding("Shift_JIS"))
                Dim sb As New StringBuilder
                sb.Append(DateTime.Now.ToString)
                sb.Append(ControlChars.Tab)
                sb.Append(MyBase.GetType.BaseType.FullName)
                sb.Append(ControlChars.Tab)
                sb.Append(UN)
                sb.Append(ControlChars.Tab)
                sb.Append(SD)
                sb.Append(ControlChars.Tab)
                sb.Append(ex.Message.ToString)
                sb.Append(ControlChars.Tab)
                sb.Append(ex.StackTrace.ToString)
                sb.Append(procName)
                sw.WriteLine(sb.ToString)
            End Using                                                   ' これでテキストファイルが閉じられる
        End If

    End Sub

End Class
