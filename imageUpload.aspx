<%@ Page Language="VB" AutoEventWireup="false" CodeFile="imageUpload.aspx.vb" Inherits="imageUpload" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
    <title id="MyTitle" runat="server">画像ファイル送信</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <%--<link href="CSS/stl_graph_table.css" rel="stylesheet" type="text/css" />--%>
    <link href="CSS/stl_fileupload.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server" method="post" enctype="multipart/form-data">
    <div title="画像ファイル送信">
        <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="180" EnablePageMethods="True" EnableScriptGlobalization="True" EnableScriptLocalization="True"></asp:ToolkitScriptManager>
        <asp:Panel ID="PanelHead" runat="server" CssClass="noaccordionHeader" EnableViewState="False">●画像ファイルアップロード</asp:Panel>
        <asp:Panel ID="PnlUpload" runat="server" CssClass="panelpad">
            現場図面などの画像ファイルをサーバへ送信します。<br />
            同名のファイルは上書きします。元に戻せません。<br />
            <ul class="list">
                <li>4MB以上のファイルは送信できません。</li>
                <li>ファイル形式はPNGもしくはJPEGのみとなります。</li>
            </ul>
            ●送信するファイルを選択してください。<br />
            <asp:Panel ID="Panel2" runat="server" CssClass="uploadpanel">
                <asp:FileUpload ID="FlUp" runat="server" CssClass="uploadpart" />
                <asp:Label ID="LblError" runat="server" CssClass="alertlabel" EnableViewState="False"></asp:Label>
            </asp:Panel>
            ●入れ替えるファイルを選択<br />
            <div id="imagelist" class="imglist" >
                <div id="colleft" class="colleft">
                    <asp:ListBox ID="lstImageFiles" runat="server" CssClass="imglistbox"></asp:ListBox><br />
                    <asp:Button ID="BtnFileUpload" runat="server" Text="ファイル送信" />
                </div>
                <div id="colright" class="colright">
                    <asp:Literal ID="LitExp" runat="server" ></asp:Literal>                　
                </div>
            </div>
            <asp:Label ID="LblMsg" runat="server" EnableViewState="False" CssClass="msglabel"></asp:Label>
        </asp:Panel>
    </div>
    </form>
</body>
</html>
