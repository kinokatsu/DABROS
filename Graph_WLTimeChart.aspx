<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Graph_WLTimeChart.aspx.vb" Inherits="Graph_WLTimeChart"  EnableEventValidation="False" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<% Response.Expires = -1 
Response.AddHeader ("Cache-Control", "No-Cache")
Response.AddHeader ("Pragma", "No-Cache")
%>
<%@ OutputCache Duration=1 VaryByParam="None" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
	<head runat="server">
		<title id="MyTitle" runat="server">水位観測情報</title>
		<%--<meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />--%>
		<meta http-equiv="X-UA-Compatible" content="IE=edge"/>
        <link href="CSS/stl_graph_WL.css" rel="stylesheet" type="text/css" />
		<link href="CSS/jquery-confirm.css" rel="stylesheet" type="text/css"/>
		<link href="CSS/stl_print.css" rel="stylesheet" type="text/css" media="print" />
		<script type="text/javascript" src="<%# Page.ResolveUrl("~/js/jquery-3.0.0.min.js") %>"></script>
		<script type="text/javascript" src="<%# Page.ResolveUrl("~/js/jquery.canvasjs.min.js") %>"></script>
		<script type="text/javascript" src="<%# Page.ResolveUrl("~/js/grpWL.js") %>"></script>
		<script type="text/javascript" src="<%# Page.ResolveUrl("~/js/jquery-confirm.js") %>"></script>
		<script type="text/javascript" src="<%# Page.ResolveUrl("~/js/grpWL.js") %>"></script>	
	</head>
	<body>
		<form id="GraphTimeChartFrm" runat="server">
			<div id="FrmGraphHSSTimeChart" lang="ja">
				<asp:ToolkitScriptManager id="AJAX_TKSM" runat="server" EnableScriptGlobalization="True" EnableScriptLocalization="True" EnablePageMethods="True" AsyncPostBackTimeout="180"></asp:ToolkitScriptManager> 
				<asp:Panel id="PnlNoPrint" runat="server" CssClass="dspOnly" Width="656px" EnableTheming="True" Wrap="False" BorderStyle="None" HorizontalAlign="Center">
					<asp:Panel id="PnlButtons" runat="server" CssClass="pnlButtons">
						<asp:ImageButton ID="ImgBtnPrint" runat="server" AlternateText="Print" ImageUrl="~/img/print.png" OnClientClick="PrintPreview();return false;" CausesValidation="False" CssClass="BTNHEADER_PRINT"/>
						<asp:ImageButton ID="ImbtnReloadGraph" runat="server" AlternateText="Reload" ImageUrl="~/img/reload1.png" CausesValidation="False" CssClass="BTNHEADER_REDRAW" ToolTip="初期設定を読み込み直します。"/>
						<asp:ImageButton ID="ImgBtnClose" runat="server" AlternateText="Close" ImageUrl="~/img/close.png" OnClientClick="window.close();return false;" CausesValidation="False" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" CssClass="BTNHEADER_CLOSE"/>
					</asp:Panel>
					<asp:Panel id="Ph1" runat="server" CssClass="accordionHeader">
						●表示設定
						<asp:ImageButton ID="ImgCollExp" runat="server" EnableTheming="False" cssclass="imgCollaps" />
						<asp:Label ID="LblColl1" runat="server" cssclass="lblCollaps"></asp:Label>
					</asp:Panel>
					<asp:Panel id="PnlGraphSet" runat="server" CssClass="collapsePanel" Width="654px" EnableTheming="True">
						<asp:Label id="LblLastUpdate" runat="server" cssclass="lblUpdate"></asp:Label>
						<table id="Table3" class="tableOutline" >
							<tbody>
								<tr>
									<td class="TABLEHEADER tableShowSpan td_span">データスケール設定</td>
								</tr>
								<tr id="r1" runat="server">
									<td class="tableChartSet">
										<asp:panel id="Pnl1" runat="server" Width="135px" cssClass="ChartSetPanel">
											<asp:RadioButton id="RdbNo11" runat="server" BorderStyle="None" Text="既定" GroupName="No1"></asp:RadioButton>
											<asp:DropDownList id="DdlScale1" runat="server" Width="89px"></asp:DropDownList>
											<br />
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
						<table id="TABLE1" class="tableOutline">
							<tbody>
								<tr>
									<td class="TABLEHEADER tableShowSpan">表示期間設定</td>
								</tr>
								<tr>
									<td class="tableShowType">●表示する期間を、開始日、終了日から選択してください。
										<div id="DataSpan">
											<asp:Label ID="stDate" runat="server" BorderStyle="None" CssClass="LABELS" Text="開始日"></asp:Label>
											<asp:TextBox ID="TxtStartDate" runat="server" CausesValidation="True" CssClass="TEXTBOXES textboxDate" EnableTheming="False" MaxLength="10" ToolTip="「指定日」の時に、データ表示開始日時を入力してください" ValidationGroup="EndDay"></asp:TextBox>
											<asp:Image ID="imgCalTxtStartDate" runat="server" EnableTheming="False" ImageAlign="AbsMiddle" ImageUrl="~/img/Calendar.gif" CssClass="imgCal" ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" />
											<asp:Label ID="Lbl2" runat="server" BorderStyle="None" CssClass="LABELS" EnableTheming="False" Font-Size="Small" Text="～ 終了日"></asp:Label>
											<asp:TextBox ID="TxtEndDate" runat="server" CausesValidation="True" CssClass="TEXTBOXES textboxDate" EnableTheming="False" MaxLength="10" ToolTip="「指定日」の時に、データ表示終了日時を入力してください" ValidationGroup="EndDay"></asp:TextBox>
											<asp:Image ID="imgCalTxtEndDate" runat="server" EnableTheming="False" ImageAlign="AbsMiddle" ImageUrl="~/img/Calendar.gif" CssClass="imgCal" ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" />
											<input id="btnUpdate" type="button" class="btnpic" value="更新" onclick="getWLData();return false;"  title="設定された期間でグラフを再描画します。"/>
										</div>
									</td>
								</tr>
							</tbody>
						</table>
					</asp:Panel>
					<asp:CollapsiblePanelExtender id="CollapsiblePanelExtender1" runat="server" CollapseControlID="Ph1" Collapsed="true" CollapsedText="設定表示" ExpandControlID="Ph1" ExpandedText="設定非表示" TextLabelID="LblColl1" CollapsedImage="~/img/el.png" ExpandedImage="~/img/cl.png" ImageControlID="ImgCollExp" SuppressPostBack="True" TargetControlID="PnlGraphSet" Enabled="True"></asp:CollapsiblePanelExtender>
				</asp:Panel>
				<article>
				<section  class="print_pages_port">
					<div id="grp" class="GraphOuterPortrait BoxShadow MarginAuto">
						<div class="GrpDateFrame">
							<div id="LblDateSpan" runat="server" EnableTheming="True" class="GrpDate"></div>
						</div>
						<div id="containerUpper" class ="ConUpper" >
							<div id="leftCol" class="con_common con_left" runat="server">
								<div id="chartContainer" class="graphConteiner">
									<asp:Literal ID="litG1" runat="server"></asp:Literal>
								</div>
							</div>
							<div id="RightCol" class="con_common con_right">
								<div id="TableContainer" style="height: 100%; width:100%">
									<table id='tbl3' class="condTable">
										<thead class="cond_Top">
											<tr>
												<td colspan='2'>▽▽4観測所 □□4川</td>
											</tr>
										</thead>
										<tbody>
											<tr>
												<td class="cond_col1">観測局名</td>
												<td class="cond_col2">▽▽4観測所</td>
											</tr>
											<tr>
												<td class="cond_col1">水系名</td>
												<td>〇〇4水系</td>
											</tr>
											<tr>
												<td class="cond_col1">河川名</td>
												<td>□□4川</td>
											</tr>
											<tr>
												<td class="cond_col1">測定日時</td>
												<td id='mesdt3'>2018/07/10 10:00</td>
											</tr>
											<tr>
												<td class="cond_col1">水位(m)</td>
												<td id='WL3'>10.40</td>
											</tr>
											<tr>
												<td class="cond_col1">水位変化</td>
												<td id='wlst3'>↑</td>
											</tr>
											<tr>
												<td class="cond_col1">観測開始水位(m)</td>
												<td>15.00</td>
											</tr>
											<tr>
												<td class="cond_col1">装置温度(℃)</td>
												<td id='temp3'>24.00</td>
											</tr>
											<tr>
												<td class="cond_col1">電池電圧(V)</td>
												<td id='volt3'>13.00</td>
											</tr>
										</tbody>
									</table>
								</div>
							</div>
						</div>
						<div id="containerLower" class="timechart float_Clear" runat="server">
							<asp:Literal ID="litG2" runat="server"></asp:Literal>                    
						</div>
						<asp:Label id="LblTitleLower" runat="server" CssClass="titleLower"></asp:Label>
					</div>
				</section>
				</article>
				<asp:RegularExpressionValidator id="RExValidMax1" runat="server" CssClass="BASICSET" EnableTheming="True" ControlToValidate="TxtMax1" Display="None" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,6}$"></asp:RegularExpressionValidator> 
				<asp:RegularExpressionValidator id="RExValidMin1" runat="server" CssClass="BASICSET" EnableTheming="True" ControlToValidate="TxtMin1" Display="None" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,6}$"></asp:RegularExpressionValidator> 
				<asp:ValidatorCalloutExtender id="COEMax1" runat="server" Width="250px" TargetControlID="RExValidMax1" HighlightCssClass="validatorCalloutHighligh"></asp:ValidatorCalloutExtender> 
				<asp:ValidatorCalloutExtender id="COEMin1" runat="server" Width="250px" TargetControlID="RExValidMin1" HighlightCssClass="validatorCalloutHighligh"></asp:ValidatorCalloutExtender> 
			</div>
		</form>
	</body>
</html>