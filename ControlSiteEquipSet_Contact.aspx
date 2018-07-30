<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ControlSiteEquipSet_Contact.aspx.vb" Inherits="Control_SiteEquipSetContact"%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
    <head runat="server">
    <title>現場機器制御-接点制御</title>
    <%--<meta http-equiv="X-UA-Compatible" content="IE=7" />--%>
    <meta name="viewport" content="width=device-width, initial-scale=1.0, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <%--<link href="CSS/stl_sexybuttons.css" rel="stylesheet" type="text/css" />--%>
    <link id="brocss" type="text/css" href="CSS/stl_pccontrol.css" rel="stylesheet" />
    <link href="CSS/stl_jquery.alerts.css" rel="stylesheet" type="text/css" />
    <%--<script type="text/javascript" src="js/jquery.js"></script>--%>
    <script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
    <script type="text/javascript" src="js/jquery.alerts.min.js"></script>
    <script type="text/javascript" src="js/EquipControls.js"></script>
    <script type="text/javascript" src="js/jquery.numeric.js"></script>
    <script type="text/javascript">
        function pageLoad() {
            $('#lnkBtn_submit').hide()
            //設置場所が選択されたら描画
            $('#DDLMeasPoint').change(function() {
                var LogID;
                LogID = $("#DDLMeasPoint").val();           //LoggerIDの取得
                GSc(LogID);
                ime();
            });
        }
    </script>
    </head>
    <body>
        <form id="ControlContact" runat="server">
            <div hidefocus="true" id="ControlSitePCSet_Meas" lang="ja" title="現場接点制御">
                <asp:ToolkitScriptManager id="ToolkitScriptManager1" runat="server" EnableScriptGlobalization="True" EnableScriptLocalization="True" AsyncPostBackTimeout="180" EnablePageMethods="True">
                    <Services>
                        <asp:ServiceReference Path="api/ControlEquip.asmx"></asp:ServiceReference>
                    </Services>
                </asp:ToolkitScriptManager>

                <asp:Panel id="PanelHead" runat="server" CssClass="noaccordionHeader" EnableTheming="True" EnableViewState="False">
                    ●現場機器制御-接点制御設定
                </asp:Panel>
                <asp:Panel id="PnlGraphSet" runat="server" CssClass="outerBox" EnableTheming="True">
                    <asp:Label id="LblLastUpdate1" runat="server" CssClass="Label_lastupdate"></asp:Label>
                    <div id="header" class="tableheader">接点制御　設定</div>
                    <div id="body" class="tablebody">●現場で稼働している接点制御装置に対して、接点の制御を行います。 
                        <ul style="LIST-STYLE-POSITION: outside" class="expUl">
                            <li>通信状況が悪いと、制御の時間が長くなる場合があります。</li>
                            <li>最初に、設置場所を選択してください。現場の機器より現状を取得して表示します（ON/OFF)。</li>
                            <li>次に、接点のON、OFFを設定してください。</li>
                            <li>一時的に、管理値による回転灯の制御をしない場合は、「管理値による制御を実施しない」にチェックを入れてください。</li>
                            <li>「接点状態 設定」ボタンを押すと、設定を反映します。</li>
                            <li>再度、状態取得を行う場合は、設置場所の項目から「選択してください」を選択してから、設置場所を選択してください。</li>
                        </ul><br />
                        <div id="explaination" class="imgExp alRight">
                            <img id="Img1" class="imgState" src="img/Select.jpg" alt="Select"/>：ON　
                            <img id="Img2" class="imgState" src="img/unSelect.jpg" alt="unSelect"/>：OFF</div>
                        <div id="CheckID" class="ItemsH">
                            <div id="titleContact" class="Title">接点状態</div>
                            <div id="LoggerPoint" class="ddllp">
                                ●設置場所を選択：<asp:DropDownList id="DDLMeasPoint" runat="server"></asp:DropDownList>
                            </div>
                            <div id="contactset" class="checkbox-group Equip">
                                <asp:CheckBoxList id="chbContact" runat="server" EnableViewState="true" CellPadding="0" CellSpacing="0" RepeatColumns="5" RepeatLayout="Flow" RepeatDirection="Horizontal">
                                    <asp:ListItem>DO1</asp:ListItem>
                                    <asp:ListItem>DO2</asp:ListItem>
                                    <asp:ListItem>DO3</asp:ListItem>
                                    <asp:ListItem>DO4</asp:ListItem>
                                    <asp:ListItem>DO5</asp:ListItem>
                                </asp:CheckBoxList>
                            </div>
                            <div id="ControlDisable" class="chkDisable">
                                <asp:CheckBox ID="ChbDisable" runat="server" CssClass="LABELS_Left" EnableTheming="False" Text="管理値による制御を実施しない" BorderStyle="None" />
                            </div>
                        </div>
                        <div id="measbtn" class="meas">
                            <div id="btn" class="btnBox">
                                <asp:LinkButton id="lnkBtn_submit" runat="server" BorderStyle="None" OnClientClick="SCs(); return false;" ToolTip="ボタンを押すと設定にしたがって接点制御命令を現場機器へ送信しします。">接点状態 設定</asp:LinkButton>
                            </div>
                            <div id="exp" class="exp">以下に、命令の送信状況と、現場からの応答を表示します</div>
                            <div id="resTL" class="resTL">
                                <div class="contentsVert">
                                    <span>命令送信結果</span>
                                </div>
                            </div>
                            <div id="resTR" class="resTR">
                                <div class="contentsVert">
                                    <span id="order" class="ifresult"></span>
                                </div>
                            </div>
                            <div id="resBL" class="resBL">
                                <div class="contentsVert">
                                    <p class="vertAlign">現場側結果</p>
                                </div>
                            </div>
                            <div id="resBR" class="resBR">
                                <div id="resSite" class="ifresult"></div>
                            </div>
                            <div id="dmy" class="dmy"></div>
                            <asp:LinkButton id="lnBtnClose" runat="server" CssClass="btnclose" BorderStyle="None" OnClientClick="javascript:window.close();return false;" ToolTip="このボタンで閉じてください。" CausesValidation="False">閉じる</asp:LinkButton>
                            <%--<asp:LinkButton ID="LinkButton1" runat="server" EnableViewState="False" >LinkButton</asp:LinkButton>--%>
                        </div>
                    </div>
                </asp:Panel> 
            </div>
            <%--<img id="preloadimg1st" runat="server" class="aniload1st" enableviewstate="true" src="img/loader4.gif" />--%>
            <%--<input id="fp" runat="server" class="fp" name="arg" type="hidden" value="RqPWt:" />--%>
        </form>
    </body>
</html>
