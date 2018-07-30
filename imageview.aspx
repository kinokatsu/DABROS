<%@ Page Language="VB" AutoEventWireup="false" CodeFile="imageview.aspx.vb" Inherits="imageview" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
    <title id="MyTitle" runat="server">撮影画像表示</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <link href="CSS/stl_imageview.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="ImageViewForm" runat="server">
    <div id="ImageView" lang="ja" title="撮影画像">
        <asp:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server" EnablePageMethods="True" EnableScriptGlobalization="True" EnableScriptLocalization="True"></asp:toolkitscriptmanager>
        <asp:Panel ID="PnlNoPrint" runat="server" CssClass="dspOnly" Width="656px" EnableTheming="True" Wrap="False" EnableViewState="False" BorderStyle="None">
            <asp:Panel ID="PnlButtons" runat="server" BackColor="Cornsilk" BorderColor="Olive" BorderStyle="Ridge" EnableTheming="False" Height="37px" Width="650px" Font-Size="2pt" BorderWidth="2px" HorizontalAlign="Left" EnableViewState="False" Wrap="False">
                <asp:ImageButton ID="ImgBtnPrint" runat="server" EnableViewState="False" AlternateText="Print" ImageUrl="~/img/print.png" OnClientClick="window.print();return false;" CausesValidation="False" BorderStyle="None" CssClass="BTNHEADER_PRINT" ToolTip="「印刷」画面を表示します。" />
                <asp:ImageButton ID="ImbtnRedrawGraph" runat="server" EnableViewState="False" AlternateText="Reload" ImageUrl="~/img/reload.png" BorderStyle="None" CssClass="BTNHEADER_REDRAW" />
                <asp:ImageButton ID="ImgBtnClose" runat="server" EnableViewState="False" AlternateText="Close" ImageUrl="~/img/close.png" CausesValidation="False" OnClientClick="javascript:window.close();return false;" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" BorderStyle="None" CssClass="BTNHEADER_CLOSE"/>
            </asp:Panel>
            <asp:Panel ID="Ph1" runat="server" CssClass="noaccordionHeader" Width="648px" BorderWidth="2px" EnableViewState="False" BackColor="#0000EC">
                ●撮影画像表示
            </asp:Panel>
        </asp:Panel>
        <asp:UpdatePanel ID="PudPnl" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <table>
                    <tr>
                        <td style="width: 100px; vertical-align: top;" rowspan="2">
                            <asp:Panel ID="Pnlthum" runat="server" HorizontalAlign="Center" BorderStyle="Ridge" CssClass="dspOnly" BorderWidth="4px" BorderColor="#909090">
                                <asp:Button ID="BtnPrev" runat="server" Text="新しい日時へ" OnClick="BtnPrev_Click" EnableViewState="False" ToolTip="撮影時刻が現在時刻に近い5画像を表示します。" CssClass="GoBackBtn" />
                                <div id="PnlthumImg">
                                    <asp:ImageButton ID="ImgBtn0" runat="server" OnClick="ImgBtn0_Click" CssClass="imgBtn" />
                                    <asp:Label ID="LblPhotoDate0" runat="server" Text="-" CssClass="photolabel"></asp:Label>
                                    <asp:ImageButton ID="ImgBtn1" runat="server" OnClick="ImgBtn0_Click" CssClass="imgBtn" />
                                    <asp:Label ID="LblPhotoDate1" runat="server" Text="-" CssClass="photolabel"></asp:Label>
                                    <asp:ImageButton ID="ImgBtn2" runat="server" OnClick="ImgBtn0_Click" CssClass="imgBtn" />
                                    <asp:Label ID="LblPhotoDate2" runat="server" Text="-" CssClass="photolabel"></asp:Label>
                                    <asp:ImageButton ID="ImgBtn3" runat="server" OnClick="ImgBtn0_Click" CssClass="imgBtn" />
                                    <asp:Label ID="LblPhotoDate3" runat="server" Text="-" CssClass="photolabel"></asp:Label>
                                    <asp:ImageButton ID="ImgBtn4" runat="server" OnClick="ImgBtn0_Click" CssClass="imgBtn" />
                                    <asp:Label ID="LblPhotoDate4" runat="server" Text="-" CssClass="photolabel"></asp:Label>
                                </div>
                                <asp:Button ID="BtnNext" runat="server" Text="過去の日時へ" OnClick="BtnPrev_Click" EnableViewState="False" ToolTip="撮影時刻が古い時刻に向かって5画像を表示します。" CssClass="GoBackBtn" />
                            </asp:Panel>
                        </td>
                        <td style="vertical-align: top;">
                            <asp:Image ID="Image1" runat="server" GenerateEmptyAlternateText="True" CssClass="imgBtnLoaderBase" />
                            <asp:Panel ID="PnlPhotoDate" runat="server" CssClass="photolabelPanel">
                                <asp:Label ID="lblPhotoDate" runat="server" CssClass="BASICSET VertBottom photolabelPanel" ></asp:Label>
                            </asp:Panel>
                        </td>
                    </tr>
                    <tr>
                        <td style="vertical-align: bottom; text-align: center; height: 50px;">
                            <asp:Panel ID="PnlSelect" runat="server" BorderStyle="None" CssClass="dspOnly,VertBottom" HorizontalAlign="Left" Width="600px">
                                <asp:Label ID="Label1" runat="server" BorderStyle="None"  CssClass="dspOnly,VertBottom" EnableViewState="False" Font-Size="11pt" Text="設置個所："></asp:Label>
                                <asp:DropDownList ID="DDLPointName" runat="server" AutoPostBack="True" CssClass="dspOnly,VertBottom" Font-Size="10pt" Width="168px"></asp:DropDownList>
                                <asp:Label ID="Label2" runat="server" BorderStyle="None" CssClass="dspOnly,VertBottom" EnableViewState="False" Font-Size="11pt" Text="撮影日時："></asp:Label>
                                <asp:DropDownList ID="DDLDateTime" runat="server" AutoPostBack="True" CssClass="dspOnly,VertBottom" Font-Size="10pt" OnTextChanged="DDLDateTime_TextChanged" Width="141px"></asp:DropDownList>
                            </asp:Panel>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
<%--        <asp:updatepanelanimationextender ID="UpdPnlAnimEx" runat="server" TargetControlID="PudPnl" Enabled="True">
        <Animations>
            <OnUpdating>
              <FadeOut Duration="0.5" Fps="10" minimumOpacity="0.5" maximumOpacity="1" />
            </OnUpdating>
            <OnUpdated>
              <FadeIn Duration="0.5" Fps="25" minimumOpacity="0.5" maximumOpacity="1" />
            </OnUpdated></Animations>
        </asp:updatepanelanimationextender>--%>
        </div>
    </form>
</body>
</html>
