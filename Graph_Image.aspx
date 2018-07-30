<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Graph_Image.aspx.vb" Inherits="Graph_Image" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
    <title id="MyTitle" runat="server">画像表示</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <meta http-equiv="X-UA-Compatible" content="IE=9" />
    <script src="js/grp.min.js" type="text/javascript"></script>
    <link href="CSS/stl_graph_table.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="GraphTimeChartFrm" runat="server">
    <div id="GraphImage" lang="ja" class="stl_graph_table.css" title="画像">
        <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" EnableScriptGlobalization="True" EnableScriptLocalization="True" EnablePageMethods="True"></asp:ToolkitScriptManager>
        <asp:Panel ID="PnlNoPrint" runat="server" CssClass="dspOnly" Width="656px" EnableTheming="True" Wrap="False" EnableViewState="False" BorderStyle="None">
            <asp:Panel ID="PnlButtons" runat="server" BackColor="Cornsilk" BorderColor="Olive" BorderStyle="Ridge" EnableTheming="True" Height="37px" Width="650px" Font-Size="2pt" BorderWidth="2px" HorizontalAlign="Left" EnableViewState="False" Wrap="False">
                <asp:ImageButton ID="ImgBtnPrint" runat="server" EnableViewState="False" AlternateText="Print" ImageUrl="~/img/print.png" OnClientClick="window.print();return false;" CausesValidation="False" BorderStyle="None" CssClass="BTNHEADER_PRINT" EnableTheming="False" ToolTip="「印刷」画面を表示します。" />
                <asp:ImageButton ID="ImbtnRedrawGraph" runat="server" EnableViewState="False" AlternateText="Redraw" ImageUrl="~/img/redraw.png" BorderStyle="None" CssClass="BTNHEADER_REDRAW" ToolTip="設定した内容でグラフを再描画します。" EnableTheming="False" />
                <asp:ImageButton ID="ImgBtnClose" runat="server" EnableViewState="False" AlternateText="Close" ImageUrl="~/img/close.png" OnClientClick="window.close();return false;" CausesValidation="False" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" BorderStyle="None" CssClass="BTNHEADER_CLOSE" EnableTheming="False" />
            </asp:Panel>
            <asp:Panel ID="Ph1" runat="server" CssClass="noaccordionHeader" Width="648px" BorderWidth="2px" EnableViewState="False" BorderStyle="Solid">
                <asp:Label ID="lblCaption" runat="server" EnableViewState="False" BorderWidth="0px" BorderStyle="None"></asp:Label>
            </asp:Panel>
            <asp:Panel ID="PnlGraphSet" runat="server" BackColor="Transparent" BorderStyle="None" CssClass="collapsePanel" EnableTheming="True" Width="654px" Font-Size="4pt" HorizontalAlign="Center" Wrap="False" EnableViewState="False">
                <br />
                <asp:Label ID="LblLastUpdate" runat="server" BackColor="#C0FFC0" BorderStyle="Ridge" BorderWidth="2px" CssClass="LABELS_Right" Font-Size="10pt" Width="625px" EnableTheming="False" EnableViewState="False"></asp:Label>
            </asp:Panel>
        </asp:Panel>
        <div>
            <asp:Label ID="LblTitleUpper" runat="server" BorderStyle="None" EnableTheming="False" EnableViewState="False" Width="650px" />
            <asp:Image ID="ImgGraph" runat="server" BorderStyle="None" CssClass="imageSet" />
            <asp:Label ID="LblTitleLower" runat="server" BorderStyle="None" EnableTheming="False" EnableViewState="False" Width="650px" />
        </div>
        <asp:Timer ID="Tmr_Update" runat="server" Enabled="False"></asp:Timer>
    </div>
</form>
</body>
</html>
