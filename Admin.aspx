<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Admin.aspx.vb" Inherits="Admin" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
    <title>管理者ページ</title>
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <meta charset="utf-8" />
    <link href="CSS/stl_keisokuweb.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="AdminFrm" runat="server">
    <div style="text-align: left" title="管理者用画面" id="FrmAdmin">
        <asp:Label ID="Title" runat="server" Font-Italic="True" Font-Size="30pt"
            ForeColor="White" Text="管理者用画面" Width="255px" CssClass="headBlock"></asp:Label><br />
        <br />
        <asp:Panel ID="Panel1" runat="server" Width="800px" EnableTheming="False" EnableViewState="False">
            &nbsp;<asp:CheckBoxList ID="ConstructionList" runat="server" Width="795px" RepeatLayout="Flow" Font-Size="11pt" CellSpacing="1" EnableTheming="False">
        </asp:CheckBoxList><br />
            <br />
        <asp:Label ID="ListNothing" runat="server" ForeColor="Red" Text="登録している現場が有りません。"
            Width="344px"></asp:Label><br />
            <br />
            <asp:Label ID="LabelSiteName" runat="server" CssClass="LABELS" Text="現場名" Width="112px"></asp:Label><asp:TextBox
                ID="TxtNewSite" runat="server" MaxLength="50" ValidationGroup="NewSite"></asp:TextBox><asp:RequiredFieldValidator
                    ID="RequiredFieldValidator4" runat="server" ControlToValidate="TxtNewSite" Display="Dynamic"
                    ErrorMessage="RequiredFieldValidator" SetFocusOnError="True" ValidationGroup="NewSite">必ず入力してください</asp:RequiredFieldValidator><br />
            <asp:Label ID="Label2" runat="server" CssClass="LABELS" Text="フォルダ名" Width="112px"></asp:Label><asp:TextBox
                ID="TxtFolderName" runat="server" MaxLength="50" ValidationGroup="NewSite"></asp:TextBox><asp:RequiredFieldValidator
                    ID="RequiredFieldValidator5" runat="server" ControlToValidate="TxtFolderName"
                    Display="Dynamic" ErrorMessage="RequiredFieldValidator" SetFocusOnError="True" ValidationGroup="NewSite">必ず入力してください</asp:RequiredFieldValidator><br />
            <asp:Label ID="lblAlert" runat="server" BorderStyle="Ridge" Font-Bold="True" Font-Size="12pt"
                ForeColor="Red" Text="既に同名の現場またはフォルダが登録されています。違う名称で登録してください。" Visible="False"
                Width="354px" EnableTheming="False" EnableViewState="False"></asp:Label><br />
            <br />
            <asp:Button ID="BtnSiteAdd" runat="server" Text="新規現場追加" Width="328px" EnableTheming="False" ValidationGroup="NewSite" /><br />
            <br />
            <hr />
            <br />
        <asp:Label ID="NewUserMsg" runat="server" Text="新規ユーザー名称とパスワードを入力して下さい" Width="426px"></asp:Label><br />
            <br />
        <asp:Label ID="Spc1" runat="server" Text="ユーザー名称" Width="112px" CssClass="LABELS"></asp:Label><asp:TextBox
            ID="NewUserName" runat="server" MaxLength="50" ValidationGroup="NewUser"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="NewUserName"
            ErrorMessage="RequiredFieldValidator" Display="Dynamic" SetFocusOnError="True" ValidationGroup="NewUser">必ず入力してください</asp:RequiredFieldValidator><br />
        <asp:Label ID="Spc2" runat="server" Text="パスワード" Width="112px" CssClass="LABELS"></asp:Label><asp:TextBox
            ID="NewUserPassword" runat="server" MaxLength="40" TextMode="Password" ValidationGroup="NewUser"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="NewUserPassword"
            ErrorMessage="RequiredFieldValidator" Display="Dynamic" SetFocusOnError="True" ValidationGroup="NewUser">必ず入力してください</asp:RequiredFieldValidator><br />
            <asp:Label ID="Label1" runat="server" Text="客先名" Width="112px" CssClass="LABELS"></asp:Label><asp:TextBox
                ID="NewCustomerName" runat="server" MaxLength="50" ValidationGroup="NewUser"></asp:TextBox><asp:RequiredFieldValidator
                    ID="RequiredFieldValidator3" runat="server" ControlToValidate="NewCustomerName"
                    Display="Dynamic" ErrorMessage="RequiredFieldValidator" SetFocusOnError="True" ValidationGroup="NewUser">必ず入力してください</asp:RequiredFieldValidator><br />
                <asp:Label ID="LblNewUserAlert" runat="server" BorderStyle="Ridge" Font-Bold="True"
                    Font-Size="12pt" ForeColor="Red" Visible="False" Width="354px" EnableTheming="False" EnableViewState="False"></asp:Label><br />
            <br />
        <asp:Button ID="NewUser" runat="server" Text="新規ユーザー追加" Width="328px" EnableTheming="False" ValidationGroup="NewUser" /><br />
            </asp:Panel>
                &nbsp;<br />
        <asp:LoginStatus ID="LoginStatus1" runat="server" EnableTheming="False" EnableViewState="False"
            LogoutPageUrl="~/Login.aspx" LogoutAction="Redirect" Font-Names="MS UI Gothic" Font-Size="12pt" />
        <br />
        <br />
        <asp:Button ID="Button1" runat="server" CausesValidation="False" EnableTheming="False"
            EnableViewState="False" Text="アクセス履歴表示" UseSubmitBehavior="False" /></div>
    </form>
</body>
</html>
