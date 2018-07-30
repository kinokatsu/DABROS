
Partial Class UserContorl_wds
    Inherits System.Web.UI.UserControl

Private _NEWSFONTSIZE As Single
Private _FULLSIZE As Integer
Private _IMGSIZE As Integer
Private _IMGSPEED As Integer

    Public WriteOnly Property imgSpeed() As Integer
        Set(ByVal value As Integer)
            _IMGSPEED = value
        End Set
    End Property

    Public WriteOnly Property nsewFontSize() As Single
        Set(ByVal value As Single)
            _NEWSFONTSIZE = value
        End Set
    End Property

    Public WriteOnly Property fullSize() As Integer
        Set(ByVal value As Integer)
            _FULLSIZE = value
        End Set
    End Property

    Public WriteOnly Property imageSize() As Integer
        Set(ByVal value As Integer)
            _IMGSIZE = value
        End Set
    End Property

    Private Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim showimg As String = Nothing
        Dim imgWidth As Integer = _FULLSIZE * 0.8

        wind.Style("width") = String.Format("{0}px", _FULLSIZE)
        wind.Style("height") = String.Format("{0}px", _FULLSIZE)

        nor.Style("font-size") = String.Format("{0}pt", _NEWSFONTSIZE)
        eas.Style("font-size") = String.Format("{0}pt", _NEWSFONTSIZE)
        sou.Style("font-size") = String.Format("{0}pt", _NEWSFONTSIZE)
        wes.Style("font-size") = String.Format("{0}pt", _NEWSFONTSIZE)

        showimg &= String.Format("src='./img/AniArrow{0}.gif' Width='{1}px'>", _IMGSPEED, imgWidth)

        Lit2.Text = "<img id='img' class='wdimg' " & showimg

    End Sub



End Class
