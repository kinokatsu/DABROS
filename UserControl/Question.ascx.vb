Option Explicit On
Option Strict On

Partial Class Question
    Inherits System.Web.UI.UserControl

    Private _QTITLE As String
    Private _SUPPLE As String
    Private _LISTCOUNT As Integer
    Private _LISTITEMS() As String

    ''' <summary>
    ''' ラジオボタンリストのボタン数
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ListCount() As Integer
        Get
            Return _LISTCOUNT
        End Get
        Set(ByVal value As Integer)
            _LISTCOUNT = value
            ReDim _LISTITEMS(_LISTCOUNT - 1)
        End Set
    End Property

    ''' <summary>
    ''' アンケート項目のタイトル
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TitleWord() As String
        Get
            Return _QTITLE
        End Get
        Set(ByVal value As String)
            _QTITLE = value
            Me.title.InnerText = ("●" & value)
        End Set
    End Property

    ''' <summary>
    ''' アンケート項目の説明
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SupplementWord() As String
        Get
            Return _SUPPLE
        End Get
        Set(ByVal value As String)
            _SUPPLE = value
            Me.supplementation.InnerText = value
        End Set
    End Property

    ''' <summary>
    ''' ラジオボタンリストのボタン表示文字列設定
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    Public WriteOnly Property ItemWords() As String()
        Set(ByVal value As String())
            _LISTITEMS = value
            Dim iloop As Integer

            For iloop = 0 To (_LISTCOUNT - 1)
                Me.RBLPerform.Items.Add(value(iloop))
            Next

        End Set
    End Property

End Class
