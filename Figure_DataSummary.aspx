<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Figure_DataSummary.aspx.vb" Inherits="Figure_DataSummary" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>無題のページ</title>
    <%--<meta http-equiv="X-UA-Compatible" content="IE=7" />--%>
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Image ID="Image1" runat="server" ImageUrl="~/totsuka/img/MainPic.png" Style="z-index: 100;
            left: 0px; position: absolute; top: 0px; background-image: url(totsuka/img/MainPic.png);" />
        <asp:Label ID="Label2" runat="server" BorderStyle="Ridge" BorderWidth="2px"
            Style="z-index: 101; left: 87px; position: absolute; top: 136px" Text="22.01"
            Width="11px" CssClass="DataLBLCenter" Height="15px" EnableTheming="False" EnableViewState="False"></asp:Label>
        <asp:Label ID="Label1" runat="server" BorderStyle="Ridge" BorderWidth="2px" CssClass="DataLBLCenter"
            EnableTheming="False" EnableViewState="False" Height="15px" Style="z-index: 101;
            left: 149px; position: absolute; top: 136px" Text="22.01" Width="11px"></asp:Label>
        <asp:TextBox ID="TextBox1" runat="server" BorderStyle="Ridge"
            BorderWidth="2px" CssClass="DataLBLCenter"
            Height="15px" Style="z-index: 102; left: 117px; position: absolute;
            top: 157px" Width="29px" EnableTheming="False" EnableViewState="False">22.01</asp:TextBox>
        &nbsp;</div>
    </form>
</body>
</html>
