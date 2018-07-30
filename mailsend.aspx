<%@ Page Language="VB" AutoEventWireup="false" CodeFile="mailsend.aspx.vb" Inherits="mailsend" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>メール送信フォーム</title>
    <link href="CSS/stl_mailsend.css" rel="stylesheet" type="text/css" />
    <script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
    <script src="js/mailsend.js" type="text/javascript"></script>
</head>
<body>
    <form id="MailForm" runat="server">
        <div id="outerframe" class="paper">
            <div id="formtitle" runat="server" class ="formTitle" style="min-width:300px;width: 32%">メール送信フォーム</div>
            <div id="exp" runat="server">
            日頃、弊社製品をご愛顧いただきましてまことにありがとうございます。心より御礼申し上げます。<br /><br />
            皆様方の貴重なご意見を今後の品質向上に役立たせて参りたいと考えております。<br /><br />
            つきましては、お手数をお掛けし、はなはだ恐縮ではございますがDABROSのご利用について、ご意見、ご要望・不具合等がございましたらご記入いただけますと幸いに存じます。<br /><br />
            </div>
            <div id ="Mark" class="indispensable" style="display:none;">
                ※のついている項目は、必ずご記入下さい。
            </div>
            <table id="mail" class="mailcontent">
                <tr>
                    <td class="items">
                        現場名
                    </td>
                    <td class="inpval-tb">
                        <asp:TextBox ID="ConstructionSite" runat="server" CssClass="inpval" 
                            MaxLength="30"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class="items">
                        ご担当者名
                    </td>
                    <td class="inpval-tb">
                        <asp:TextBox ID="Name" runat="server" CssClass="inpval" MaxLength="16"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class="items">
                        E-Mailアドレス
                    </td>
                    <td class="inpval-tb">
                        <asp:TextBox ID="EMailAddress" runat="server" CssClass="inpval" MaxLength="60"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class="items">
                        種別
                    </td>
                    <td class="inpval-tb">
                        <asp:RadioButtonList ID="RdbList_Type" runat="server" 
                            RepeatDirection="Horizontal">
                            <asp:ListItem Selected="True" Value="0" class="radiobtn">ご意見・ご要望</asp:ListItem>
                            <asp:ListItem Value="1" class="radiobtn">不具合</asp:ListItem>
                        </asp:RadioButtonList>

                    </td>
                </tr>
                <tr>
                    <td class="items dipsSet">
                        ご意見・ご要望など<br />(400文字程度としてください)
                        <div id="wordct" class="wordct"></div>
                    </td>
                    <td class="inpval-tb">
                        <asp:TextBox ID="RqText" runat="server" CssClass="inpvalbig" MaxLength="400" TextMode="MultiLine" onInput="alertValue(this);"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2"><a href="./privacy.aspx" target="_blank">プライバシーポリシー</a>に、ご同意いただいた方のみ送信できます。</td>
                </tr>
                
            </table>
            
            <div id ="btnSend" class="mailbtns">
                <div id="SendData" onclick="sendEMail();">同意して送信</div>
                <div id="clear" onclick="clearitems();">クリア</div>
                <div id="cancel" onclick="cancel();">キャンセル</div>
            </div>
            <div class="mailcontent">　</div>
            <asp:Button id="btnSubmit" runat="server" EnableViewState="False" CssClass="invisible"></asp:Button>
        </div>
        <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="CustomValidator"></asp:CustomValidator>
    </form>
</body>
</html>
