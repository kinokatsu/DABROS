Imports System.Data
Imports System.Drawing
''Imports System.Windows.Forms

Partial Class Figure_DataSummary
    Inherits System.Web.UI.Page

    'Protected DataLabel As System.Web.UI.WebControls.Label

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Call ClsDataSummary.DynamicMakeLabels(Me.form1) ', DataLabel)
        'Call DynamicMakeLabels()

    End Sub


    Protected Sub DynamicMakeLabels()
        ''
        '' ラベルのの動的生成
        ''

        Dim iloop As Integer

        For iloop = 0 To 10
            Dim DataLabel As System.Web.UI.WebControls.Label = New System.Web.UI.WebControls.Label
            With DataLabel
                .CssClass = "DataLBLCenter"
                .ID = "Dat" & iloop
                .Text = iloop * 2.21
                .Font.Size = 13
                .BorderStyle = WebControls.BorderStyle.Ridge
                .BorderWidth = 2
                .EnableTheming = False
                .EnableViewState = False
                .ToolTip = "LABEL" & iloop
                .Attributes.Add("class", "LBL_MAIN")
                .Style("z-index") = 100
                .Style("Position") = "Absolute"
                .Style("left") = 20 * iloop & "px"
                .Style("top") = 20 * iloop & "px"
                .Style("Width") = 60 & "px"
                Me.Controls.Add(DataLabel)
                .Width = 20
            End With
        Next
    End Sub

End Class
