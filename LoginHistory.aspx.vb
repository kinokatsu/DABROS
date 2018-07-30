
Partial Class LoginHistory
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        If User.Identity.IsAuthenticated = False Then Response.Redirect("~/Login.aspx", False)

    End Sub
End Class
