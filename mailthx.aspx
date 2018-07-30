<%@ Page Language="VB" AutoEventWireup="false" CodeFile="thanks.aspx.vb" Inherits="thanx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>御礼</title>
    <link href="CSS/stl_Qu.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
    <!--
        // 閉じる
        function quitBox(cmd) {
            open(location, '_self').close();
            return false;
        }
        // 戻る禁止
        history.pushState(null, null, null);
        window.addEventListener("popstate", function () {
            history.pushState(null, null, null);
        });
    -->
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="guide" class="thanx">
            お忙しいところ、メールの送付ありがとうございました。<br />
            ご送信いただきました内容は、お客様サービスの向上のために利用させていただきます。
            <br />
            <br />
            確認のメールを送付しております。<br />
            もし、届いていない場合は、しばらく様子を見ていただいて、それも届かない場合は、お手数ですが、再度メール送信フォームからお手続きをお願いいたします。<br />
            <br />
            今後とも弊社サービスをよろしくお願い申し上げます。<br />
            <br />
            <asp:LinkButton ID="LinkButton1" runat="server" OnClientClick="return quitBox('quit');">閉じる</asp:LinkButton>
        </div>
    </form>
</body>
</html>
