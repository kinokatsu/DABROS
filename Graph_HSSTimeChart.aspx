<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Graph_HSSTimeChart.aspx.vb" Inherits="Graph_HSSTimeChart"  EnableEventValidation="False" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<% Response.Expires = -1 
Response.AddHeader ("Cache-Control", "No-Cache")
Response.AddHeader ("Pragma", "No-Cache")
%>
<%@ OutputCache Duration=1 VaryByParam="None" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title id="MyTitle" runat="server">高頻度データグラフ</title>
        <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
        <link href="CSS/stl_graph_wave.css" rel="stylesheet" type="text/css" />
        <link href="CSS/jquery-confirm.css" rel="stylesheet" type="text/css"/>
        <script type="text/javascript" src="js/jquery-3.0.0.min.js"></script>
        <script type="text/javascript" src="js/jquery.canvasjs.min.js"></script>
        <script type="text/javascript" src="js/grpWave.js"></script>
        <script type="text/javascript" src="js/jquery-confirm.js"></script>
    </head>
    <body>
        <form id="GraphTimeChartFrm" runat="server">
            <div id="FrmGraphHSSTimeChart" lang="ja">
                <asp:ToolkitScriptManager id="AJAX_TKSM" runat="server" EnableScriptGlobalization="True" EnableScriptLocalization="True" EnablePageMethods="True" AsyncPostBackTimeout="180"></asp:ToolkitScriptManager> 
                <asp:Panel id="PnlNoPrint" runat="server" CssClass="dspOnly" Width="656px" EnableTheming="True" Wrap="False" BorderStyle="None" HorizontalAlign="Center">
                    <asp:Panel id="PnlButtons" runat="server" CssClass="pnlButtons">
                        <asp:ImageButton ID="ImgBtnPrint" runat="server" AlternateText="Print" ImageUrl="~/img/print.png" OnClientClick="window.print();return false;" CausesValidation="False" CssClass="BTNHEADER_PRINT"/>
                        <asp:ImageButton ID="ImbtnReloadGraph" runat="server" AlternateText="Reload" ImageUrl="~/img/reload1.png" CausesValidation="False" CssClass="BTNHEADER_REDRAW" ToolTip="初期設定を読み込み直します。"/>
                        <asp:ImageButton ID="ImgBtnClose" runat="server" AlternateText="Close" ImageUrl="~/img/close.png" OnClientClick="window.close();return false;" CausesValidation="False" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" CssClass="BTNHEADER_CLOSE"/>
                    </asp:Panel>
                    <asp:Panel id="Ph1" runat="server" CssClass="accordionHeader">
                        ●グラフ設定
                        <asp:ImageButton ID="ImgCollExp" runat="server" EnableTheming="False" cssclass="imgCollaps" />
                        <asp:Label ID="LblColl1" runat="server" cssclass="lblCollaps"></asp:Label>
                    </asp:Panel>
                    <asp:Panel id="PnlGraphSet" runat="server" CssClass="collapsePanel" Width="654px" EnableTheming="True">
                        <asp:Label id="LblLastUpdate" runat="server" cssclass="lblUpdate"></asp:Label>
                        <table id="Table3" class="tableOutline" cellSpacing=0 cellPadding=1 >
                            <tbody>
                                <tr>
                                    <td class="TABLEHEADER tableShowSpan" colSpan=3>データスケール設定</td>
                                </tr>
                                <tr>
                                    <td class="tableLabelNo">No.</td>
                                    <td class="tableScaleHead ScaleHeadOther" colSpan=2>左 軸</td>
                                </tr>
                                <tr id="r1" runat="server">
                                    <td class="tableLabelNo tableBorderTop">
                                        <asp:Label id="LblNo1" runat="server" Text="No.1"></asp:Label>
                                        <div id="svOut1" class="saveBtnOut" runat="server">
                                            <asp:LinkButton id="lkBtn1" runat="server" CssClass="saveBtn">保存</asp:LinkButton>
                                        </div>
                                    </td>
                                    <td class="tableChartSet tableBorderTop" colSpan=2>
                                        <asp:panel id="Pnl1" runat="server" Width="135px" cssClass="ChartSetPanel">
                                            <asp:RadioButton id="RdbNo11" runat="server" BorderStyle="None" Text="既定" GroupName="No1"></asp:RadioButton>
                                            <asp:DropDownList id="DdlScale1" runat="server" Width="89px"></asp:DropDownList>
                                            <BR />
                                            <asp:RadioButton id="RdbNo12" runat="server" BorderStyle="None" Text="入力" GroupName="No1"></asp:RadioButton>
                                            <asp:Label id="Label3" runat="server" CssClass="ChartlblMax" Text="最大"></asp:Label>
                                            <asp:TextBox id="TxtMax1" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
                                            <asp:Label id="Label4" runat="server" CssClass="ChartlblMin" Text="～最小"></asp:Label>
                                            <asp:TextBox id="TxtMin1" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
                                            <input id="btnSetScale" type="button" value="スケール変更" onclick="setScale();return false;"/>
                                        </asp:panel>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </asp:Panel>
                    <asp:CollapsiblePanelExtender id="CollapsiblePanelExtender1" runat="server" CollapseControlID="Ph1" Collapsed="true" CollapsedText="設定表示" ExpandControlID="Ph1" ExpandedText="設定非表示" TextLabelID="LblColl1" CollapsedImage="~/img/el.png" ExpandedImage="~/img/cl.png" ImageControlID="ImgCollExp" SuppressPostBack="True" TargetControlID="PnlGraphSet" Enabled="True"></asp:CollapsiblePanelExtender>
                    <table id="TABLE1" class="tableOutline" cellSpacing=0 cellPadding=1>
                        <tbody>
                            <tr>
                                <td class="TABLEHEADER tableShowSpan">表示日時設定</td>
                            </tr>
                            <tr>
                                <td class="tableShowType">●表示する日時を、左から順に「年月」「日」、「時刻」もしくは「センサー番号」を選択してください。
                                    <div id="DataSpan" class="Term">
                                        <table id="TABLE5" class="tableShowSpanInner">
                                            <tr>
                                                <th class="th1">年 月</th>
                                                <th class="th1">日</th>
                                                <th class="th1">時刻</th>
                                                <th class="th1">センサー番号</th>
                                                <th></th>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <asp:DropDownList id="DDLYMon" runat="server" CssClass="ccdDrop"></asp:DropDownList>
                                                </td>
                                                <td>
                                                    <asp:DropDownList id="DDLDay" runat="server" CssClass="ccdDrop"></asp:DropDownList>
                                                </td>
                                                <td>
                                                    <asp:DropDownList id="DDLHMin" runat="server" CssClass="ccdDrop" ToolTip="「時刻」もしくは「センサー番号」選択すると、波形を描画します。"></asp:DropDownList>
                                                </td>
                                                <td>
                                                    <asp:DropDownList id="DDLSensor" runat="server" CssClass="ccdDrop" ToolTip="「時刻」もしくは「センサー番号」選択すると、波形を描画します。"></asp:DropDownList>
                                                </td>
                                                <td>
                                                    <input id="drawGraph" type="button" value="再描画" onclick="getWaveData();return false;" title="波形を再描画します。"/>
                                                </td>
                                            </tr>
                                        </table>
                                        <div id="option" class="opt">
                                            ■画像ファイル保存　<input id="saveX" type="button" class="pic" data-dir="_X" value="Ｘ"/>
                                            <input id="saveY" type="button" class="pic" data-dir="_Y" value="Ｙ"/>
                                            <input id="saveZ" type="button" class="pic" data-dir="_Z" value="Ｚ"/>
                                            　　■CSVダウンロード　<span id="pre" class="pre"><img src="./img/loader_s.gif" alt=""/></span>
                                            <input id="csv_part" type="button" value="No.*のみ" onclick="downloadCSV('p');return false;" href="#"/>
                                            <a href="#" id="download" download="sensor.csv" class="ffdl">ダウンロード</a>
                                            <span id="pre2" class="pre"><img src="./img/loader_s.gif" alt=""/></span>
                                            <input id="csv_all" type="button" value="全センサー" onclick="downloadCSV('a');return false;" href="#"/>
                                        </div>
                                        <asp:CascadingDropDown id="ccdYM" runat="server" TargetControlID="DDLYMon" ServiceMethod="GetDate" PromptText="「年月」を選択してください。" Category="YM" LoadingText="読込中..." ServicePath="api/getDateTime.asmx"></asp:CascadingDropDown>
                                        <asp:CascadingDropDown id="ccdDay" runat="server" TargetControlID="DDLDay" ServiceMethod="GetDate" PromptText="「日」を選択してください。" Category="D" LoadingText="読込中..." ServicePath="api/getDateTime.asmx" ParentControlID="DDLYMon"></asp:CascadingDropDown>
                                        <asp:CascadingDropDown id="ccdHM" runat="server" TargetControlID="DDLHMin" ServiceMethod="GetDate" PromptText="「時:分:秒」を選択してください。" Category="HM" LoadingText="読込中..." ServicePath="api/getDateTime.asmx" ParentControlID="DDLDay"></asp:CascadingDropDown>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </asp:Panel> 
                <div id="grp" class="GraphOuterLandscape">
                    <div class="GrpDateFrame">
                        <div id="LblDateSpan" runat="server" EnableTheming="True" class="GrpDate"></div>
                    </div>
                    <asp:Literal ID="litG1" runat="server"></asp:Literal>
                    <asp:Literal ID="litG2" runat="server"></asp:Literal>
                    <asp:Literal ID="litG3" runat="server"></asp:Literal>
                    <asp:Label id="LblTitleLower" runat="server" CssClass="titleLower"></asp:Label>
                </div>
                <asp:RegularExpressionValidator id="RExValidMax1" runat="server" CssClass="BASICSET" EnableTheming="True" ControlToValidate="TxtMax1" Display="None" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,6}$"></asp:RegularExpressionValidator> 
                <asp:RegularExpressionValidator id="RExValidMin1" runat="server" CssClass="BASICSET" EnableTheming="True" ControlToValidate="TxtMin1" Display="None" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,6}$"></asp:RegularExpressionValidator> 
                <asp:ValidatorCalloutExtender id="COEMax1" runat="server" Width="250px" TargetControlID="RExValidMax1" HighlightCssClass="validatorCalloutHighligh"></asp:ValidatorCalloutExtender> 
                <asp:ValidatorCalloutExtender id="COEMin1" runat="server" Width="250px" TargetControlID="RExValidMin1" HighlightCssClass="validatorCalloutHighligh"></asp:ValidatorCalloutExtender> 
            </div>
        </form>
    </body>
</html>