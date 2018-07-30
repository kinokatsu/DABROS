
Partial Class AsyncTest2
    Inherits System.Web.UI.Page

Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim strScript As String = "<html><head><title>未認証</title></head><body>アクセスを認証されていません</body></html>" + "<script language=javascript>alert('正規の手続きでログインをしてください');window.close();</script>"

        If User.Identity.IsAuthenticated = False Then
            'Response.Redirect("Login.aspx")
            Response.Write(strScript)
            Exit Sub
        End If

        Dim LoginStatus As Integer

        LoginStatus = Session.Item("LgSt")          'ログインステータス
        ' ''ログインしていない場合は、ログイン画面へ
        If LoginStatus = 0 Then
            Response.Redirect("sessionerror.aspx")
            Exit Sub
        End If

        Response.Cache.SetCacheability(HttpCacheability.NoCache)        ''キャッシュなしとする

        ' ユーザーコントロールの設定
        Wds1.fullSize = 100
        Wds1.nsewFontSize = 8
        Wds1.imgSpeed = 1

End Sub


End Class
