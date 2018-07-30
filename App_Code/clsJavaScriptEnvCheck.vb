Imports System.Data
Imports System.Configuration
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports System.Text

Public Class clsJavaScriptEnvCheck

    ''' <summary>
    ''' JavaScript有効可否情報を格納するHidden項目のコントロールID
    ''' </summary>
    Private Const _JS_ENV_CHECK_HIDDEN_VAL_ID As String = "id_js_env_check"

    ''' <summary>
    ''' Hidden項目のユニークID
    ''' </summary>
    Private Shared _hiddenUniqueID As String = ""

    ''Private Sub New()
    ''End Sub

    ''' <summary>
    ''' JavaScriptEnvCheckの初期化処理
    ''' ※PageのLoad処理でコールします。
    ''' </summary>
    ''' <param name="checkPage">JavaScript有効のチェックを行う対象Page</param>
    ''' <returns>true：初期化成功</returns>
    Public Shared Function Initialize(ByVal checkPage As System.Web.UI.Page) As Boolean

        If checkPage Is Nothing Then
            Return False
        End If

        'Hidden項目を生成し、該当Pageに埋め込む
        If checkPage.Form.FindControl(_JS_ENV_CHECK_HIDDEN_VAL_ID) Is Nothing Then

            Dim hidden As New HiddenField()
            Try
                hidden.ID = _JS_ENV_CHECK_HIDDEN_VAL_ID
                hidden.Value = "false"
                hidden.EnableViewState = False
                checkPage.Form.Controls.Add(hidden)

                'Hidden項目に値を設定するJavaScriptを埋め込む
                Dim scriptBuf As StringBuilder = New StringBuilder("<script language='JavaScript'>").Append("{").Append("var hidden = document.getElementById('").Append(hidden.ClientID).Append("');").Append("if(null!=hidden){hidden.value='true';}").Append("}").Append("</script>")

                ScriptManager.RegisterStartupScript(checkPage, checkPage.[GetType](), "checkJSEnv", scriptBuf.ToString(), False)

                'Hidden値取得の為にIDを保持しておく
                _hiddenUniqueID = hidden.UniqueID
            Catch ex As Exception

            Finally
                hidden.Dispose()
            End Try
        End If

        Return True

    End Function
    ''' <summary>
    ''' JavaScriptが有効になっているか判定するメソッド
    ''' </summary>
    ''' <param name="request">HttpRequest</param>
    ''' <returns>true：JavaScriptは有効</returns>
    Public Shared Function IsEnableJavaScript(ByVal request As HttpRequest) As Boolean

        'RequestよりHidden値を取得する
        Dim hiddenValue As String = request.Form(_hiddenUniqueID)

        If hiddenValue Is Nothing OrElse hiddenValue.Equals("") Then
            ''Throw New Exception("JavaScriptEnvCheck.Initialize()が実行されていません")
        End If

        If hiddenValue.Equals("true") Then
            Return True
        End If

        Return False
    End Function

End Class
