<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Graph_DepthDistributionChart.aspx.vb" Inherits="Graph_DepthDistributionChart" Trace="false" TraceMode="SortByTime"%>
<%@ Register assembly="C1.Web.C1WebChart.4" namespace="C1.Web.C1WebChart" tagprefix="C1WebChart" %>
<%@ Register assembly="C1.Web.Wijmo.Controls.4" namespace="C1.Web.Wijmo.Controls.C1Calendar" tagprefix="wijmo" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
	<title id="MyTitle" runat="server">深度分布図</title>
	<meta charset="utf-8" />
	<meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
	<meta http-equiv="X-UA-Compatible" content="IE=9" />
	<link href="CSS/stl_graph_table.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_calendar.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_ToolTip.css" rel="stylesheet" type="text/css" />
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
	<script src="js/addclear.min.js" type="text/javascript"></script>
	<script src="js/updCheckSub.min.js" type="text/javascript"></script>
	<script src="js/tippy.all.min.js" type="text/javascript"></script>
	<script src="js/grp_Dep.min.js" type="text/javascript"></script>
	<script src="js/ToolTip.min.js" type="text/javascript"></script>
	<script src="js/Simple2D.min.js" type="text/javascript"></script>
</head>
<body oncontextmenu="return false">
	<form id="DepthDistributionChartFrm" runat="server">
		<div id="FrmDepthDistributionChart" lang="ja" title="深度分布図">
			<input id="nwdt" runat="server" type="hidden"/>
			<input id="nwdtno" runat="server" type="hidden"/>
		<asp:Button style="display:none; visibility:hidden" id="btnSubmit" runat="server" EnableViewState="False"></asp:Button>
			<asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" EnablePageMethods="True" EnableScriptGlobalization="True" EnableScriptLocalization="True" AsyncPostBackTimeout="180"></asp:ToolkitScriptManager>
			<asp:Panel ID="PnlNoPrint" runat="server" CssClass="dspOnly" EnableTheming="True" Width="660px" Wrap="False">
				<asp:Panel ID="PnlButtons" runat="server" CssClass="pnlButtons">
					<div class="rg"><img id="imgloader" src="./img/comm.gif" alt="" class="asyncloader" /></div>
					<asp:ImageButton ID="ImgBtnPrint" runat="server" AlternateText="Print" ImageUrl="~/img/print.png" OnClientClick="window.print();return false;" TabIndex="10" CssClass="BTNHEADER_PRINT" ToolTip="「印刷」画面を表示します。" />
					<asp:ImageButton ID="ImbtnRedrawGraph" runat="server" AlternateText="Redraw" EnableViewState="False" ImageUrl="~/img/redraw.png" CssClass="BTNHEADER_REDRAW" ToolTip="設定した内容で再描画します。" />
					<asp:ImageButton ID="ImgBtnClose" runat="server" AlternateText="Close" CausesValidation="False" EnableViewState="False" ImageUrl="~/img/close.png" OnClientClick="window.close();return false;" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" CssClass="BTNHEADER_CLOSE" />
				</asp:Panel>
				<asp:Panel ID="Ph1" runat="server" CssClass="accordionHeader" EnableViewState="False">
					●グラフ設定
					<asp:ImageButton ID="ImgCollExp" runat="server" EnableTheming="False" EnableViewState="False" CssClass="imgCollaps" />
					<asp:Label ID="LblColl1" runat="server" cssclass="lblCollaps" ></asp:Label>
				</asp:Panel>
				<asp:Panel ID="PnlGraphSet" runat="server" Width="654px" EnableTheming="False" 
					CssClass="collapsePanel">
					<asp:Label ID="LblLastUpdate" cssclass="lblUpdate"  runat="server" ></asp:Label>
					<table id="TABLE1" class="tableOutline unselectable">
						<tr>
							<td class="TABLEHEADER">表示期間設定</td>
						</tr>
						<tr>
							<td colspan="1" class="tableContent">
								●表示方法を指定してください。<br />
								&nbsp;&nbsp;「最新」を選択した場合は、サーバに収録された最新のデータを表示します。<br />
								&nbsp;&nbsp;「指定日時」を選択した場合は、指定された日時のデータを表示します。<br /><br />
								■表示方法
								<table class="tableSelDate">
									<tr class="depthShowRow">
										<td class="depthShowCol1">
											<asp:RadioButton ID="RdBFromNewest" runat="server" EnableTheming="True" GroupName="SelectOutputType" Text="最新 " Width="88px" CssClass="LABELS_Left" Checked="True" /></td>
										<td colspan="2">
											サーバ内の最新データを表示します
											<%--<asp:Label ID="LblNewestDate" runat="server" EnableTheming="True">サーバ内の最新データを表示します</asp:Label>--%></td>
									</tr>
									<tr class="depthShowRow">
										<td class="depthShowCol1">
											<asp:RadioButton ID="RdBDsignatedDate" runat="server" EnableTheming="True" GroupName="SelectOutputType" Text="指定日時" Width="88px" TabIndex="1" CssClass="LABELS_Left" /></td>
										<td class="depthShowCol2">
											<asp:TextBox ID="TxtDate0" runat="server" CssClass="dateTextBoxes" EnableTheming="True"
												MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="2"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate0" runat="server" EnableTheming="True" 
												ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" TabIndex="9" 
												EnableViewState="False" ImageAlign="AbsMiddle" /></td>
										<td class="depthShowCol3">
											<asp:RegularExpressionValidator ID="REValid0" runat="server"
												ControlToValidate="TxtDate0" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" TabIndex="9" CssClass="validColor" ForeColor="">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid0" runat="server" ControlToValidate="TxtDate0"
												Display="Dynamic" ErrorMessage="本日より過去2年間までです" SetFocusOnError="True"
												ValidationGroup="SetDay" CssClass="validColor" ForeColor=""></asp:RangeValidator></td>
									</tr>
									<tr class="depthShowRow">
										<td class="depthShowCol1"></td>
										<td class="depthShowCol2">
											<asp:TextBox ID="TxtDate1" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True"
												MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="3"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate1" runat="server" EnableTheming="True" 
												ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" 
												EnableViewState="False" TabIndex="10" ImageAlign="AbsMiddle" /></td>
										<td class="depthShowCol3">
											<asp:RegularExpressionValidator ID="REValid1" runat="server"
												ControlToValidate="TxtDate1" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" CssClass="validColor" ForeColor="">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid1" runat="server" ControlToValidate="TxtDate1" Display="Dynamic" ErrorMessage="本日より過去2年間までです" SetFocusOnError="True"
												ValidationGroup="SetDay" CssClass="validColor" ForeColor=""></asp:RangeValidator></td>
									</tr>
									<tr class="depthShowRow">
										<td class="depthShowCol1"></td>
										<td class="depthShowCol2">
											<asp:TextBox ID="TxtDate2" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True"
												MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="4"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate2" runat="server" EnableTheming="True" 
												ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" 
												EnableViewState="False" TabIndex="11" ImageAlign="AbsMiddle" /></td>
										<td class="depthShowCol3">
											<asp:RegularExpressionValidator ID="REValid2" runat="server" ControlToValidate="TxtDate2" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="" CssClass="validColor">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid2" runat="server" ControlToValidate="TxtDate2" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="" SetFocusOnError="True"
													ValidationGroup="SetDay" CssClass="validColor"></asp:RangeValidator></td>
									</tr>
									<tr class="depthShowRow">
										<td class="depthShowCol1"></td>
										<td class="depthShowCol2">
											<asp:TextBox ID="TxtDate3" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True"
												MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="5"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate3" runat="server" EnableTheming="True" 
												ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" 
												EnableViewState="False" TabIndex="12" ImageAlign="AbsMiddle" /></td>
										<td class="depthShowCol3">
											<asp:RegularExpressionValidator ID="REValid3" runat="server" ControlToValidate="TxtDate3" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="" CssClass="validColor">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid3" runat="server" ControlToValidate="TxtDate3" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="" SetFocusOnError="True"
												ValidationGroup="SetDay" CssClass="validColor"></asp:RangeValidator></td>
									</tr>
									<tr class="depthShowRow">
										<td class="depthShowCol1"></td>
										<td class="depthShowCol2">
											<asp:TextBox ID="TxtDate4" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True"
												MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="6"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate4" runat="server" EnableTheming="True" 
												ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" 
												EnableViewState="False" TabIndex="13" ImageAlign="AbsMiddle" /></td>
										<td class="depthShowCol3">
											<asp:RegularExpressionValidator ID="REValid4" runat="server" ControlToValidate="TxtDate4" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="" CssClass="validColor">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid4" runat="server" ControlToValidate="TxtDate4" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="" SetFocusOnError="True"
												ValidationGroup="SetDay" CssClass="validColor"></asp:RangeValidator></td>
									</tr>
									<tr class="depthShowRow">
										<td class="depthShowCol1"></td>
										<td class="depthShowCol2">
											<asp:TextBox ID="TxtDate5" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True"
												MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="7"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate5" runat="server" EnableTheming="True" 
												ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" 
												EnableViewState="False" TabIndex="14" ImageAlign="AbsMiddle" /></td>
										<td class="depthShowCol3">
											<asp:RegularExpressionValidator ID="REValid5" runat="server" ControlToValidate="TxtDate5" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="" CssClass="validColor">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid5" runat="server" ControlToValidate="TxtDate5" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="" SetFocusOnError="True"
												ValidationGroup="SetDay" CssClass="validColor"></asp:RangeValidator></td>
									</tr>
									<tr class="depthShowRow">
										<td class="depthShowCol1"></td>
										<td class="depthShowCol2">
											<asp:TextBox ID="TxtDate6" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True"
												MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="8"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate6" runat="server" EnableTheming="True" 
												ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" 
												EnableViewState="False" TabIndex="15" ImageAlign="AbsMiddle" /></td>
										<td class="depthShowCol3">
											<asp:RegularExpressionValidator ID="REValid6" runat="server" ControlToValidate="TxtDate6" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="" CssClass="validColor">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid6" runat="server" ControlToValidate="TxtDate6" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="" SetFocusOnError="True"
												ValidationGroup="SetDay" CssClass="validColor"></asp:RangeValidator></td>
									</tr>
								</table>
							</td>
						</tr>
					</table>
					<table style="margin: 5px auto 10px auto; border: 0px solid #996600; padding: 1px; width: 630px; border-spacing: 0px; font-size: 10pt; border-collapse: collapse; text-align: left;">
						<tr>
							<td class="TABLEHEADER">表示一般設定</td>
						</tr>
						<tr>
							<td colspan="1" class="tableContent">
								<div id="geneR1" class="geneConRow">
									<div id="Label13" class="geneConCol1">■日付表示形式</div>
									<asp:DropDownList ID="DdlDateFormat" runat="server" CssClass="geneConCol2" TabIndex="16"></asp:DropDownList>
									<div id="Label17" class="geneConCol3">■グラフタイトル位置</div>
									<asp:DropDownList ID="DdlTitlePosition" runat="server" CssClass="geneConCol4" TabIndex="19">
										<asp:ListItem Value="0">上</asp:ListItem>
										<asp:ListItem Selected="True" Value="1">下</asp:ListItem>
									</asp:DropDownList>
									<div id="LblAlert" class="geneConCol5">■警報値表示</div>
									<asp:DropDownList ID="DDLPaintWarningValue" runat="server" CssClass="geneConCol6" TabIndex="22"></asp:DropDownList>
								</div>
								<div id="geneR2" class="geneConRow">
									<div id="Label14" class="geneConCol1">■凡例表示</div>
									<asp:DropDownList ID="DdlEnableLegend" runat="server" CssClass="geneConCol2" TabIndex="17"></asp:DropDownList>
									<div id="Label16" class="geneConCol3">■欠測データの連結</div>
									<asp:DropDownList ID="DdlContinous" runat="server" CssClass="geneConCol4" TabIndex="20"></asp:DropDownList>
									<div id="Label2" class="geneConCol5 hiddenBlock" visible="false">■施工情報表示</div>
									<asp:DropDownList ID="DDLConstState" runat="server" CssClass="geneConCol6" Visible="False" TabIndex="23"></asp:DropDownList>
								</div>
								<div id="geneR3" class="geneConRow">
									<div id="Label15" class="geneConCol1">■用紙の向き</div>
									<asp:DropDownList ID="DdlPaperOrientation" runat="server" DataTextField="用紙方向" DataValueField="用紙方向" CssClass="geneConCol2" TabIndex="18"></asp:DropDownList>
									<div id="Label32" class="geneConCol3">■グラフ色指定</div>
									<asp:DropDownList ID="DdlGraphColorType" runat="server" ToolTip="モノクロプリンターへ印刷する場合は「グレー」を選択してください。" CssClass="geneConCol4" TabIndex="21">
										<asp:ListItem Selected="True" Value="0">カラー</asp:ListItem>
										<asp:ListItem Value="1">グレー</asp:ListItem>
									</asp:DropDownList>
									<div id="Label18" class="geneConCol5 hiddenBlock" visible="false">■グラフ自動更新</div>
									<asp:DropDownList ID="DDLAutoUpdate" runat="server" cssclass="geneConCol6" Visible="False" TabIndex="24">
										<asp:ListItem Selected="True" Value="0">しない</asp:ListItem>
										<asp:ListItem Value="1">する</asp:ListItem>
									</asp:DropDownList>
								</div>
							</td>
						</tr>
					</table>
					<table id="TABLE3" class="tableOutline unselectable">
						<tr>
							<td class="TABLEHEADER tableShowSpan" colspan="2">データスケール設定</td>
						</tr>
						<tr>
							<td class="tableLabelNo">No.</td>
							<td class="tableScaleHead ScaleHeadDepth">データ 軸</td>
						</tr>
						<tr id="r1" runat="server">
							<td class="tableLabelNo tableBorderTop">
								<asp:Label id="LblNo1" runat="server" EnableViewState="False" Text="No.1" />
							</td>
							<td class="tableScaleBody tableBorderTop">
								<div id="Pnl1" runat="server">
									<div class="divFloat">
										<asp:RadioButtonList ID="RBList1" runat="server" CssClass = "rbLists">
											<asp:ListItem Value="0">既定</asp:ListItem>
											<asp:ListItem Value="1">入力</asp:ListItem>
										</asp:RadioButtonList>
									</div>
									<div class="divFloat tableScaleBody2" >
										<asp:DropDownList ID="DdlScale1" runat="server" Width="89px"></asp:DropDownList><br />
										<asp:Label ID="Label3" runat="server" EnableViewState="False" Text="最大" CssClass="ChartlblMax"></asp:Label>
										<asp:TextBox ID="TxtMax1" runat="server" CssClass="txtScale"></asp:TextBox>
										<asp:Label ID="Label4" runat="server" EnableViewState="False" Text="～最小" CssClass="ChartlblMin"></asp:Label>
										<asp:TextBox ID="TxtMin1" runat="server" CssClass="txtScale"></asp:TextBox>
									</div>
								</div>
							</td>
						</tr>
						<tr id="r2" runat="server">
							<td class="tableLabelNo tableBorderTop">
								<asp:Label ID="LblNo2" runat="server" EnableViewState="False" Text="No.2"></asp:Label>
							</td>
							<td class="tableScaleBody tableBorderTop">
								<div id="Pnl2" runat="server">
									<div class="divFloat">                                                
										<asp:RadioButtonList ID="RBList2" runat="server" CssClass = "rbLists">
											<asp:ListItem Value="0">既定</asp:ListItem>
											<asp:ListItem Value="1">入力</asp:ListItem>
										</asp:RadioButtonList>
									</div>
									<div class="divFloat tableScaleBody2" >
										<asp:DropDownList ID="DdlScale2" runat="server" Width="89px"></asp:DropDownList><br />
										<asp:Label ID="Label5" runat="server" EnableViewState="False" Text="最大" CssClass="ChartlblMax"></asp:Label>
										<asp:TextBox ID="TxtMax2" runat="server" CssClass="txtScale"></asp:TextBox>
										<asp:Label ID="Label6" runat="server" EnableViewState="False" Text="～最小" CssClass="ChartlblMin"></asp:Label>
										<asp:TextBox ID="TxtMin2" runat="server" CssClass="txtScale"></asp:TextBox>
									</div>
								</div>
							</td>
						</tr>
						<tr id="r3" runat="server">
							<td class="tableLabelNo tableBorderTop">
								<asp:Label ID="LblNo3" runat="server" EnableViewState="False" Text="No.3"></asp:Label>
							</td>
							<td class="tableScaleBody tableBorderTop">
								<div id="Pnl3" runat="server">
									<div class="divFloat">
										<asp:RadioButtonList ID="RBList3" runat="server" CssClass = "rbLists">
											<asp:ListItem Value="0">既定</asp:ListItem>
											<asp:ListItem Value="1">入力</asp:ListItem>
										</asp:RadioButtonList>
									</div>
									<div class="divFloat tableScaleBody2" >
										<asp:DropDownList ID="DdlScale3" runat="server" Width="89px"></asp:DropDownList><br />
										<asp:Label ID="Label7" runat="server" EnableViewState="False" Text="最大" CssClass="ChartlblMax"></asp:Label>
										<asp:TextBox ID="TxtMax3" runat="server" CssClass="txtScale"></asp:TextBox>
										<asp:Label ID="Label8" runat="server" EnableViewState="False" Text="～最小" CssClass="ChartlblMin"></asp:Label>
										<asp:TextBox ID="TxtMin3" runat="server" CssClass="txtScale"></asp:TextBox>
									</div>
								</div>
							</td>
						</tr>
						<tr id="r4" runat="server">
							<td class="tableLabelNo tableBorderTop">
								<asp:Label ID="LblNo4" runat="server" EnableViewState="False" Text="No.4"></asp:Label>
							</td>
							<td class="tableScaleBody tableBorderTop">
								<div id="Pnl4" runat="server">
									<div class="divFloat">
										<asp:RadioButtonList ID="RBList4" runat="server" CssClass = "rbLists">
											<asp:ListItem Value="0">既定</asp:ListItem>
											<asp:ListItem Value="1">入力</asp:ListItem>
										</asp:RadioButtonList>
									</div>
									<div class="divFloat tableScaleBody2" >
										<asp:DropDownList ID="DdlScale4" runat="server" Width="89px"></asp:DropDownList><br />
										<asp:Label ID="Label9" runat="server" EnableViewState="False" Text="最大" CssClass="ChartlblMax"></asp:Label>
										<asp:TextBox ID="TxtMax4" runat="server" CssClass="txtScale"></asp:TextBox>
										<asp:Label ID="Label0" runat="server" EnableViewState="False" Text="～最小" CssClass="ChartlblMin"></asp:Label>
										<asp:TextBox ID="TxtMin4" runat="server" CssClass="txtScale"></asp:TextBox>
									</div>
								</div>
							</td>
						</tr>
					</table>
					<asp:MaskedEditExtender ID="MEdExDt0" runat="server" AutoComplete="False" Mask="9999/99/99 99:99" MaskType="DateTime" TargetControlID="TxtDate0" UserDateFormat="YearMonthDay" UserTimeFormat="TwentyFourHour" Century="2000" MessageValidatorTip="False"></asp:MaskedEditExtender>
					<asp:MaskedEditExtender ID="MEdExDt1" runat="server" AutoComplete="False" Mask="9999/99/99 99:99" MaskType="DateTime" TargetControlID="TxtDate1" UserDateFormat="YearMonthDay" UserTimeFormat="TwentyFourHour" Century="2000" MessageValidatorTip="False"></asp:MaskedEditExtender>
					<asp:MaskedEditExtender ID="MEdExDt2" runat="server" AutoComplete="False" Mask="9999/99/99 99:99" MaskType="DateTime" TargetControlID="TxtDate2" UserDateFormat="YearMonthDay" UserTimeFormat="TwentyFourHour" Century="2000" MessageValidatorTip="False"></asp:MaskedEditExtender>
					<asp:MaskedEditExtender ID="MEdExDt3" runat="server" AutoComplete="False" Mask="9999/99/99 99:99" MaskType="DateTime" TargetControlID="TxtDate3" UserDateFormat="YearMonthDay" UserTimeFormat="TwentyFourHour" Century="2000" MessageValidatorTip="False"></asp:MaskedEditExtender>
					<asp:MaskedEditExtender ID="MEdExDt4" runat="server" AutoComplete="False" Mask="9999/99/99 99:99" MaskType="DateTime" TargetControlID="TxtDate4" UserDateFormat="YearMonthDay" UserTimeFormat="TwentyFourHour" Century="2000" MessageValidatorTip="False"></asp:MaskedEditExtender>
					<asp:MaskedEditExtender ID="MEdExDt5" runat="server" AutoComplete="False" Mask="9999/99/99 99:99" MaskType="DateTime" TargetControlID="TxtDate5" UserDateFormat="YearMonthDay" UserTimeFormat="TwentyFourHour" Century="2000" MessageValidatorTip="False"></asp:MaskedEditExtender>
					<asp:MaskedEditExtender ID="MEdExDt6" runat="server" AutoComplete="False" Mask="9999/99/99 99:99" MaskType="DateTime" TargetControlID="TxtDate6" UserDateFormat="YearMonthDay" UserTimeFormat="TwentyFourHour" Century="2000" MessageValidatorTip="False"></asp:MaskedEditExtender>
					<asp:ValidatorCalloutExtender ID="VCoEx0" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="REValid0" Width="250px"></asp:ValidatorCalloutExtender>
					<asp:ValidatorCalloutExtender ID="VCoEx1" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="REValid1" Width="250px"></asp:ValidatorCalloutExtender>
					<asp:ValidatorCalloutExtender ID="VCoEx2" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="REValid2" Width="250px"></asp:ValidatorCalloutExtender>
					<asp:ValidatorCalloutExtender ID="VCoEx3" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="REValid3" Width="250px"></asp:ValidatorCalloutExtender>
					<asp:ValidatorCalloutExtender ID="VCoEx4" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="REValid4" Width="250px"></asp:ValidatorCalloutExtender>
					<asp:ValidatorCalloutExtender ID="VCoEx5" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="REValid5" Width="250px"></asp:ValidatorCalloutExtender>
					<asp:ValidatorCalloutExtender ID="VCoEx6" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="REValid6" Width="250px"></asp:ValidatorCalloutExtender>
					<asp:PopupControlExtender ID="pce0" runat="server" TargetControlID="imgCalTxtDate0" CommitProperty="value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetX="5" DynamicContextKey="0" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
						<Animations>
							<OnShow>
							  <Sequence>
								<HideAction Visible="true" />
								<FadeIn Duration="0.2" Fps="25" />
							  </Sequence>
							</OnShow>
							<OnHide>
							  <FadeOut Duration="0.1" Fps="25" />
							</OnHide></Animations>
					</asp:PopupControlExtender>
					<asp:PopupControlExtender ID="pce1" runat="server" TargetControlID="imgCalTxtDate1" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetY="5" DynamicContextKey="1" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
						<Animations>
							<OnShow>
							  <Sequence>
								<HideAction Visible="true" />
								<FadeIn Duration="0.2" Fps="25" />
							  </Sequence>
							</OnShow>
							<OnHide>
							  <FadeOut Duration="0.1" Fps="25" />
							</OnHide></Animations>
					</asp:PopupControlExtender>
					<asp:PopupControlExtender ID="pce2" runat="server" TargetControlID="imgCalTxtDate2" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetY="5" DynamicContextKey="2" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
						<Animations>
							<OnShow>
							  <Sequence>
								<HideAction Visible="true" />
								<FadeIn Duration="0.2" Fps="25" />
							  </Sequence>
							</OnShow>
							<OnHide>
							  <FadeOut Duration="0.1" Fps="25" />
							</OnHide></Animations>
					</asp:PopupControlExtender>
					<asp:PopupControlExtender ID="pce3" runat="server" TargetControlID="imgCalTxtDate3" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetY="5" DynamicContextKey="3" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
						<Animations>
							<OnShow>
							  <Sequence>
								<HideAction Visible="true" />
								<FadeIn Duration="0.2" Fps="25" />
							  </Sequence>
							</OnShow>
							<OnHide>
							  <FadeOut Duration="0.1" Fps="25" />
							</OnHide></Animations>
					</asp:PopupControlExtender>
					<asp:PopupControlExtender ID="pce4" runat="server" TargetControlID="imgCalTxtDate4" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetY="5" DynamicContextKey="4" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
						<Animations>
							<OnShow>
							  <Sequence>
								<HideAction Visible="true" />
								<FadeIn Duration="0.2" Fps="25" />
							  </Sequence>
							</OnShow>
							<OnHide>
							  <FadeOut Duration="0.1" Fps="25" />
							</OnHide></Animations>
					</asp:PopupControlExtender>
					<asp:PopupControlExtender ID="pce5" runat="server" TargetControlID="imgCalTxtDate5" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetY="5" DynamicContextKey="5" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
						<Animations>
							<OnShow>
							  <Sequence>
								<HideAction Visible="true" />
								<FadeIn Duration="0.2" Fps="25" />
							  </Sequence>
							</OnShow>
							<OnHide>
							  <FadeOut Duration="0.1" Fps="25" />
							</OnHide></Animations>
					</asp:PopupControlExtender>
					<asp:PopupControlExtender ID="pce6" runat="server" TargetControlID="imgCalTxtDate6" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetY="5" DynamicContextKey="6" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
						<Animations>
							<OnShow>
							  <Sequence>
								<HideAction Visible="true" />
								<FadeIn Duration="0.2" Fps="25" />
							  </Sequence>
							</OnShow>
							<OnHide>
							  <FadeOut Duration="0.1" Fps="25" />
							</OnHide></Animations>
					</asp:PopupControlExtender>
				</asp:Panel>
				<asp:CollapsiblePanelExtender ID="CollapsiblePanelExtender1" runat="server" CollapseControlID="Ph1" Collapsed="true" CollapsedImage="~/img/el.png" CollapsedText="設定表示" Enabled="True" EnableViewState="False" ExpandControlID="Ph1" ExpandedImage="~/img/cl.png" ExpandedText="設定非表示" ImageControlID="ImgCollExp" TargetControlID="PnlGraphSet" TextLabelID="LblColl1">
				</asp:CollapsiblePanelExtender>
			</asp:Panel>
			<asp:UpdateProgress ID="UpdProg" runat="server" EnableViewState="False" AssociatedUpdatePanelID="UpdPanel2">
				<ProgressTemplate>
					<br />
					描画中です。しばらくお待ちください。<br />
					<img src="img/loader.gif" alt= "読み込み中" />
				</ProgressTemplate>
			</asp:UpdateProgress>
			<asp:UpdatePanel ID="UpdPanel2" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional" EnableViewState="False">
				<ContentTemplate>
					<div runat="server" id="GraphOuter"> 
						<asp:Label ID="LblTitleUpper" runat="server" BorderStyle="None" CssClass="LABELS" EnableTheming="False" EnableViewState="False" Width="470px" Visible="False" />
						<asp:Literal ID="LitUpper" runat="server" EnableViewState="False" Mode="PassThrough" />
						<table id="Table_Graph" lang="ja" style="padding: 0px; border-spacing: 0px;">
							<tr>
								<td style="font-size: 1pt; vertical-align: top; text-align: center; height: 16px;"></td>
								<td style="font-size: 1pt; vertical-align: middle; font-family: 'MS UI Gothic'; text-align: center; height: 16px;">
									<asp:Label ID="LblGraphTitle5" runat="server" EnableTheming="False" EnableViewState="False" Font-Size="11pt" Text="施工" Width="50px" BorderStyle="None"></asp:Label></td>
								<td style="font-size: 1pt; vertical-align: middle; font-family: 'MS UI Gothic'; text-align: center; height: 16px;">
									<asp:Label ID="LblGraphTitle1" runat="server" EnableTheming="False" 
										EnableViewState="False" Font-Size="11pt" Text="変形 (mm)" BorderStyle="None"></asp:Label></td>
								<td style="font-size: 1pt; vertical-align: middle; font-family: 'MS UI Gothic'; text-align: center; height: 16px;">
									<asp:Label ID="LblGraphTitle2" runat="server" EnableTheming="False" 
										EnableViewState="False" Font-Size="11pt" Text="変形 (mm)" BorderStyle="None"></asp:Label></td>
								<td style="font-size: 1pt; vertical-align: middle; font-family: 'MS UI Gothic'; text-align: center; height: 16px;">
									<asp:Label ID="LblGraphTitle3" runat="server" EnableTheming="False" 
										EnableViewState="False" Font-Size="11pt" Text="変形 (mm)" BorderStyle="None"></asp:Label></td>
								<td style="font-size: 1pt; vertical-align: middle; font-family: 'MS UI Gothic'; text-align: center; height: 16px;">
									<asp:Label ID="LblGraphTitle4" runat="server" EnableTheming="False" 
										EnableViewState="False" Font-Size="11pt" Text="変形 (mm)" BorderStyle="None"></asp:Label></td>
							</tr>
							<tr>
								<td style="font-size: 1pt; vertical-align: top; font-family: 'MS UI Gothic'; text-align: center;">
									<asp:Panel ID="PnlGraph0" runat="server" EnableViewState="False" HorizontalAlign="Left">
									<C1WebChart:C1WebChart ID="WC0" runat="server" 
											CallbackWaitImageUrl="~/img/ajax-loader_w.gif" EnableCallback="False" 
											Enabled="False" EnableTheming="False" EnableViewState="False" Height="582px" 
											Width="120px" LastDesignUpdate="636023057297928906">
										<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20122.22206&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Right;AlignVert=Center;BackColor=White;Font=MS UI Gothic, 9.75pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;Wrap=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;BackColor=Azure;BackColor2=SkyBlue;Border=None,SkyBlue,1;Font=MS UI Gothic, 9pt;GradientStyle=Vertical;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Top;Font=MS UI Gothic, 9pt;ForeColor=White;Rotation=Rotate0;VerticalText=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=None,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;VerticalText=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=Transparent;BackColor2=Transparent;Border=None,ControlDark,1;GradientStyle=None;HatchStyle=None;Opaque=False;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;LeftAxis&quot; Stacked=&quot;True&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;9.9E+30&quot; DefaultSet=&quot;True&quot;&gt;
		&lt;DataSeriesCollection&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Color=&quot;Transparent&quot; /&gt;
			&lt;SymbolStyle Color=&quot;Transparent&quot; Shape=&quot;Tri&quot; /&gt;
			&lt;SeriesLabel&gt;series 0&lt;/SeriesLabel&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		&lt;/DataSeriesCollection&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;RightAxis&quot; Stacked=&quot;True&quot; ChartType=&quot;Bar&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;DataSeriesCollection&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Transparent&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkBlue&quot; Shape=&quot;Cross&quot; /&gt;
			&lt;SeriesLabel&gt;AC1&lt;/SeriesLabel&gt;
			&lt;X&gt;0&lt;/X&gt;
			&lt;Y&gt;0&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle GradientStyle=&quot;Vertical&quot; /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		&lt;/DataSeriesCollection&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;North&quot; LocationDefault=&quot;18, 10&quot;&gt;
	&lt;Text&gt;TPm   GLm   土質&lt;/Text&gt;
  &lt;/Header&gt;
  &lt;Footer Compass=&quot;South&quot; Visible=&quot;False&quot; /&gt;
  &lt;Legend Compass=&quot;South&quot; LocationDefault=&quot;130, 850&quot; SizeDefault=&quot;500, 60&quot; Visible=&quot;False&quot; Orientation=&quot;VerticalVariableItemHeight&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;20, 7&quot; SizeDefault=&quot;120, 459&quot; Depth=&quot;20&quot; Rotation=&quot;45&quot; Elevation=&quot;45&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;0.1&quot; Min=&quot;-0.05&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;0&quot; Reversed=&quot;True&quot; UnitMajor=&quot;0.1&quot; UnitMinor=&quot;0.05&quot; AutoMajor=&quot;False&quot; AutoMinor=&quot;False&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; TickMajor=&quot;None&quot; TickMinor=&quot;None&quot; _onTop=&quot;1&quot; Compass=&quot;North&quot;&gt;
	  &lt;Text&gt;　

&lt;/Text&gt;
	  &lt;AnnoFormatString&gt;　&lt;/AnnoFormatString&gt;
	  &lt;GridMajor Spacing=&quot;0.1&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;White&lt;/Color&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Spacing=&quot;2&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMinor&gt;
	  &lt;ValueLabels&gt;
		&lt;ValueLabel Appearance=&quot;TriangleMarker&quot; Text=&quot;a&quot; /&gt;
		&lt;ValueLabel Text=&quot;b&quot; /&gt;
	  &lt;/ValueLabels&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;1&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0.1&quot; UnitMinor=&quot;0.05&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot; _Origin=&quot;0&quot; _AutoOrigin=&quot;False&quot;&gt;
	  &lt;GridMajor Spacing=&quot;1&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Spacing=&quot;0.5&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMinor&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;2&quot; Min=&quot;-20&quot; Thickness=&quot;1&quot; UnitMajor=&quot;2&quot; UnitMinor=&quot;1&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;2&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMinor&gt;
	&lt;/Axis&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" />
									</C1WebChart:C1WebChart>
									</asp:Panel>
								</td>
								<td style="font-size: 1pt; vertical-align: top; font-family: 'MS UI Gothic'; text-align: center;">
									<asp:Panel ID="PnlGraph5" runat="server" EnableViewState="False">
									<asp:Image ID="ImgConstructState" runat="server" /></asp:Panel>
								</td>
								<td style="font-size: 1pt; vertical-align: top; font-family: 'MS UI Gothic'; text-align: center;">
									<asp:Panel ID="PnlGraph1" runat="server" EnableViewState="False">
										<C1WebChart:C1WebChart ID="WC1" runat="server" 
											CallbackWaitImageUrl="~/img/ajax-loader_w.gif" EnableCallback="False" 
											Enabled="False" EnableTheming="False" EnableViewState="False" Height="582px" 
											Width="277px" LastDesignUpdate="636022266794111328">
											<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20122.22206&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Right;AlignVert=Center;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;Wrap=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;BackColor=Azure;BackColor2=SkyBlue;Border=None,SkyBlue,1;Font=MS UI Gothic, 11pt;GradientStyle=Vertical;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=Solid,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=Transparent;BackColor2=Transparent;Border=None,ControlDark,1;GradientStyle=None;HatchStyle=None;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Top;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;LeftAxis&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;DataSeriesCollection&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkGreen&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkGreen&quot; Shape=&quot;Dot&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 10:00&lt;/SeriesLabel&gt;
			&lt;X&gt;10;9;8;7;6;5;4;3;2;1;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;RoyalBlue&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;Cornsilk&quot; OutlineColor=&quot;RoyalBlue&quot; Shape=&quot;Tri&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 11:00&lt;/SeriesLabel&gt;
			&lt;X&gt;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;2.5;1.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Magenta&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;Magenta&quot; Shape=&quot;Diamond&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 12:00&lt;/SeriesLabel&gt;
			&lt;X&gt;11;10;9;8;7;6;5;4;3;2;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkGoldenrod&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkGoldenrod&quot; Shape=&quot;Box&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 13:00&lt;/SeriesLabel&gt;
			&lt;X&gt;11.5;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;2.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Gray&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;Gray&quot; Shape=&quot;InvertedTri&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 14:00&lt;/SeriesLabel&gt;
			&lt;X&gt;12;11;10;9;8;7;6;5;4;3;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;YellowGreen&quot; /&gt;
			&lt;SymbolStyle Color=&quot;White&quot; OutlineColor=&quot;YellowGreen&quot; Shape=&quot;Star&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 15:00&lt;/SeriesLabel&gt;
			&lt;X&gt;12.5;11.5;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkBlue&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkBlue&quot; Shape=&quot;Cross&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 16:00&lt;/SeriesLabel&gt;
			&lt;X&gt;13;12;11;10;9;8;7;6;5;4;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		&lt;/DataSeriesCollection&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;RightAxis&quot; Use3D=&quot;False&quot; Visible=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;9.9E+30&quot; DefaultSet=&quot;True&quot;&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;North&quot; LocationDefault=&quot;340, 10&quot; Visible=&quot;False&quot; /&gt;
  &lt;Footer Compass=&quot;North&quot; LocationDefault=&quot;305, 35&quot; Visible=&quot;False&quot; /&gt;
  &lt;Legend Compass=&quot;South&quot; LocationDefault=&quot;-1, 457&quot; Visible=&quot;True&quot; Orientation=&quot;Vertical&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;0, 0&quot; SizeDefault=&quot;250, 470&quot; Depth=&quot;20&quot; Rotation=&quot;45&quot; Elevation=&quot;45&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin Left=&quot;0&quot; Right=&quot;0&quot; /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;100&quot; Min=&quot;-100&quot; Thickness=&quot;1&quot; UnitMajor=&quot;50&quot; UnitMinor=&quot;10&quot; AutoMajor=&quot;False&quot; AutoMinor=&quot;False&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;North&quot;&gt;
	  &lt;Text&gt;山側　　　　　　　　　　　　谷川

&lt;/Text&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;50&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;DimGray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; Spacing=&quot;10&quot; AutoSpace=&quot;False&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;-20&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;AnnoFormatString&gt; ; &lt;/AnnoFormatString&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;5&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;DimGray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; Spacing=&quot;1&quot; AutoSpace=&quot;False&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;-20&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot; Visible=&quot;False&quot;&gt;
	  &lt;AnnoFormatString&gt; ; &lt;/AnnoFormatString&gt;
	  &lt;GridMajor Spacing=&quot;1&quot;&gt;
		&lt;Pattern&gt;Solid&lt;/Pattern&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor AutoSpace=&quot;False&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMinor&gt;
	&lt;/Axis&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" />
										</C1WebChart:C1WebChart>
									</asp:Panel>
								</td>
								<td style="font-size: 1pt; vertical-align: top; font-family: 'MS UI Gothic'; text-align: center;">
									<asp:Panel ID="PnlGraph2" runat="server" EnableViewState="False">
										<C1WebChart:C1WebChart ID="WC2" runat="server" 
											CallbackWaitImageUrl="~/img/ajax-loader_w.gif" EnableCallback="False" 
											Enabled="False" EnableTheming="False" EnableViewState="False" Height="582px" 
											Width="277px" LastDesignUpdate="635083043191520384">
											<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20122.22206&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Right;AlignVert=Center;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;Wrap=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;BackColor=Azure;BackColor2=SkyBlue;Border=None,SkyBlue,1;Font=MS UI Gothic, 11pt;GradientStyle=Vertical;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Top;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=Solid,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=Transparent;BackColor2=Transparent;Border=None,ControlDark,1;GradientStyle=None;HatchStyle=None;Opaque=False;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;LeftAxis&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;DataSeriesCollection&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkGreen&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkGreen&quot; Shape=&quot;Dot&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 10:00&lt;/SeriesLabel&gt;
			&lt;X&gt;10;9;8;7;6;5;4;3;2;1;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;RoyalBlue&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;Cornsilk&quot; OutlineColor=&quot;RoyalBlue&quot; Shape=&quot;Tri&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 11:00&lt;/SeriesLabel&gt;
			&lt;X&gt;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;2.5;1.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Magenta&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;Magenta&quot; Shape=&quot;Diamond&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 12:00&lt;/SeriesLabel&gt;
			&lt;X&gt;11;10;9;8;7;6;5;4;3;2;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkGoldenrod&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkGoldenrod&quot; Shape=&quot;Box&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 13:00&lt;/SeriesLabel&gt;
			&lt;X&gt;11.5;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;2.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Gray&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;Gray&quot; Shape=&quot;InvertedTri&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 14:00&lt;/SeriesLabel&gt;
			&lt;X&gt;12;11;10;9;8;7;6;5;4;3;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;YellowGreen&quot; /&gt;
			&lt;SymbolStyle Color=&quot;White&quot; OutlineColor=&quot;YellowGreen&quot; Shape=&quot;Star&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 15:00&lt;/SeriesLabel&gt;
			&lt;X&gt;12.5;11.5;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkBlue&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkBlue&quot; Shape=&quot;Cross&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 16:00&lt;/SeriesLabel&gt;
			&lt;X&gt;13;12;11;10;9;8;7;6;5;4;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		&lt;/DataSeriesCollection&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;RightAxis&quot; Use3D=&quot;False&quot; Visible=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;9.9E+30&quot; DefaultSet=&quot;True&quot;&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;North&quot; LocationDefault=&quot;-1, 5&quot; Visible=&quot;False&quot; /&gt;
  &lt;Footer Compass=&quot;North&quot; LocationDefault=&quot;305, 35&quot; Visible=&quot;False&quot; /&gt;
  &lt;Legend Compass=&quot;South&quot; LocationDefault=&quot;-1, 457&quot; Visible=&quot;True&quot; Orientation=&quot;Vertical&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;0, 0&quot; SizeDefault=&quot;250, 470&quot; Depth=&quot;20&quot; Rotation=&quot;45&quot; Elevation=&quot;45&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin Left=&quot;0&quot; Right=&quot;0&quot; /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;100&quot; Min=&quot;-100&quot; Thickness=&quot;1&quot; UnitMajor=&quot;50&quot; UnitMinor=&quot;10&quot; AutoMajor=&quot;False&quot; AutoMinor=&quot;False&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;North&quot;&gt;
	  &lt;Text&gt;山側　　　　　　　　　　　　谷川

&lt;/Text&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;50&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;DimGray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; Spacing=&quot;10&quot; AutoSpace=&quot;False&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;-20&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;AnnoFormatString&gt; ; &lt;/AnnoFormatString&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;5&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;DimGray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; Spacing=&quot;1&quot; AutoSpace=&quot;False&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;-20&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot; Visible=&quot;False&quot;&gt;
	  &lt;AnnoFormatString&gt; ; &lt;/AnnoFormatString&gt;
	  &lt;GridMajor Spacing=&quot;1&quot;&gt;
		&lt;Pattern&gt;Solid&lt;/Pattern&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor AutoSpace=&quot;False&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMinor&gt;
	&lt;/Axis&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" />
										</C1WebChart:C1WebChart>
									</asp:Panel>
								</td>
								<td style="font-size: 1pt; vertical-align: top; font-family: 'MS UI Gothic'; text-align: center;">
									<asp:Panel ID="PnlGraph3" runat="server" EnableViewState="False">
										<C1WebChart:C1WebChart ID="WC3" runat="server" 
											CallbackWaitImageUrl="~/img/ajax-loader_w.gif" EnableCallback="False" 
											Enabled="False" EnableTheming="False" EnableViewState="False" Height="582px" 
											Width="277px" LastDesignUpdate="635083043203120400">
											<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20122.22206&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Right;AlignVert=Center;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;Wrap=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;BackColor=Azure;BackColor2=SkyBlue;Border=None,SkyBlue,1;Font=MS UI Gothic, 11pt;GradientStyle=Vertical;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Top;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=Solid,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=Transparent;BackColor2=Transparent;Border=None,ControlDark,1;GradientStyle=None;HatchStyle=None;Opaque=False;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;LeftAxis&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;DataSeriesCollection&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkGreen&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkGreen&quot; Shape=&quot;Dot&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 10:00&lt;/SeriesLabel&gt;
			&lt;X&gt;10;9;8;7;6;5;4;3;2;1;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;RoyalBlue&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;Cornsilk&quot; OutlineColor=&quot;RoyalBlue&quot; Shape=&quot;Tri&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 11:00&lt;/SeriesLabel&gt;
			&lt;X&gt;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;2.5;1.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Magenta&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;Magenta&quot; Shape=&quot;Diamond&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 12:00&lt;/SeriesLabel&gt;
			&lt;X&gt;11;10;9;8;7;6;5;4;3;2;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkGoldenrod&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkGoldenrod&quot; Shape=&quot;Box&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 13:00&lt;/SeriesLabel&gt;
			&lt;X&gt;11.5;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;2.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Gray&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;Gray&quot; Shape=&quot;InvertedTri&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 14:00&lt;/SeriesLabel&gt;
			&lt;X&gt;12;11;10;9;8;7;6;5;4;3;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;YellowGreen&quot; /&gt;
			&lt;SymbolStyle Color=&quot;White&quot; OutlineColor=&quot;YellowGreen&quot; Shape=&quot;Star&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 15:00&lt;/SeriesLabel&gt;
			&lt;X&gt;12.5;11.5;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkBlue&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkBlue&quot; Shape=&quot;Cross&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 16:00&lt;/SeriesLabel&gt;
			&lt;X&gt;13;12;11;10;9;8;7;6;5;4;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		&lt;/DataSeriesCollection&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;RightAxis&quot; Use3D=&quot;False&quot; Visible=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;9.9E+30&quot; DefaultSet=&quot;True&quot;&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;North&quot; LocationDefault=&quot;340, 10&quot; Visible=&quot;False&quot; /&gt;
  &lt;Footer Compass=&quot;North&quot; LocationDefault=&quot;305, 35&quot; Visible=&quot;False&quot; /&gt;
  &lt;Legend Compass=&quot;South&quot; LocationDefault=&quot;-1, 457&quot; Visible=&quot;True&quot; Orientation=&quot;Vertical&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;0, 0&quot; SizeDefault=&quot;250, 470&quot; Depth=&quot;20&quot; Rotation=&quot;45&quot; Elevation=&quot;45&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin Left=&quot;0&quot; Right=&quot;0&quot; /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;20&quot; Min=&quot;-20&quot; Thickness=&quot;1&quot; UnitMajor=&quot;10&quot; UnitMinor=&quot;5&quot; AutoMajor=&quot;False&quot; AutoMinor=&quot;False&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;North&quot;&gt;
	  &lt;Text&gt;山側　　　　　　　　　　　　谷川

&lt;/Text&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;10&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;DimGray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; Spacing=&quot;5&quot; AutoSpace=&quot;False&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;-20&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;AnnoFormatString&gt; ; &lt;/AnnoFormatString&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;5&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;DimGray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; Spacing=&quot;1&quot; AutoSpace=&quot;False&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;-20&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot; Visible=&quot;False&quot;&gt;
	  &lt;AnnoFormatString&gt; ; &lt;/AnnoFormatString&gt;
	  &lt;GridMajor Spacing=&quot;1&quot;&gt;
		&lt;Pattern&gt;Solid&lt;/Pattern&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor AutoSpace=&quot;False&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMinor&gt;
	&lt;/Axis&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" />
										</C1WebChart:C1WebChart>
									</asp:Panel>
								</td>
								<td style="font-size: 1pt; vertical-align: top; font-family: 'MS UI Gothic'; text-align: center;">
									<asp:Panel ID="PnlGraph4" runat="server" EnableViewState="False">
										<C1WebChart:C1WebChart ID="WC4" runat="server" 
											CallbackWaitImageUrl="~/img/ajax-loader_w.gif" EnableCallback="False" 
											Enabled="False" EnableTheming="False" EnableViewState="False" Height="579px" 
											Width="193px" LastDesignUpdate="635083043218820422">
											<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20122.22206&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Right;AlignVert=Center;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;Wrap=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=White;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;BackColor=Azure;BackColor2=SkyBlue;Border=None,SkyBlue,1;Font=MS UI Gothic, 11pt;GradientStyle=Vertical;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Top;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=Solid,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=Transparent;BackColor2=Transparent;Border=None,ControlDark,1;GradientStyle=None;HatchStyle=None;Opaque=False;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;LeftAxis&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;DataSeriesCollection&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkGreen&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkGreen&quot; Shape=&quot;Dot&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 10:00&lt;/SeriesLabel&gt;
			&lt;X&gt;10;9;8;7;6;5;4;3;2;1;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;RoyalBlue&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;Cornsilk&quot; OutlineColor=&quot;RoyalBlue&quot; Shape=&quot;Tri&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 11:00&lt;/SeriesLabel&gt;
			&lt;X&gt;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;2.5;1.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Magenta&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;Magenta&quot; Shape=&quot;Diamond&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 12:00&lt;/SeriesLabel&gt;
			&lt;X&gt;11;10;9;8;7;6;5;4;3;2;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkGoldenrod&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkGoldenrod&quot; Shape=&quot;Box&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 13:00&lt;/SeriesLabel&gt;
			&lt;X&gt;11.5;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;2.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;Gray&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;Gray&quot; Shape=&quot;InvertedTri&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 14:00&lt;/SeriesLabel&gt;
			&lt;X&gt;12;11;10;9;8;7;6;5;4;3;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;YellowGreen&quot; /&gt;
			&lt;SymbolStyle Color=&quot;White&quot; OutlineColor=&quot;YellowGreen&quot; Shape=&quot;Star&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 15:00&lt;/SeriesLabel&gt;
			&lt;X&gt;12.5;11.5;10.5;9.5;8.5;7.5;6.5;5.5;4.5;3.5;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		  &lt;DataSeriesSerializer&gt;
			&lt;LineStyle Thickness=&quot;2&quot; Color=&quot;DarkBlue&quot; /&gt;
			&lt;SymbolStyle OutlineWidth=&quot;2&quot; Color=&quot;White&quot; OutlineColor=&quot;DarkBlue&quot; Shape=&quot;Cross&quot; /&gt;
			&lt;SeriesLabel&gt;2008/10/10 16:00&lt;/SeriesLabel&gt;
			&lt;X&gt;13;12;11;10;9;8;7;6;5;4;0&lt;/X&gt;
			&lt;Y&gt;0;-2;-4;-6;-8;-10;-12;-14;-16;-18;-20&lt;/Y&gt;
			&lt;DataTypes&gt;Double;Double;Double;Double;Double&lt;/DataTypes&gt;
			&lt;FillStyle /&gt;
			&lt;Histogram /&gt;
		  &lt;/DataSeriesSerializer&gt;
		&lt;/DataSeriesCollection&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;RightAxis&quot; Use3D=&quot;False&quot; Visible=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;9.9E+30&quot; DefaultSet=&quot;True&quot;&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;North&quot; LocationDefault=&quot;340, 10&quot; Visible=&quot;False&quot; /&gt;
  &lt;Footer Compass=&quot;North&quot; LocationDefault=&quot;305, 35&quot; Visible=&quot;False&quot; /&gt;
  &lt;Legend Compass=&quot;South&quot; LocationDefault=&quot;-1, 457&quot; Visible=&quot;True&quot; Orientation=&quot;Vertical&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;0, 0&quot; SizeDefault=&quot;250, 470&quot; Depth=&quot;20&quot; Rotation=&quot;45&quot; Elevation=&quot;45&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin Left=&quot;0&quot; Right=&quot;0&quot; /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;100&quot; Min=&quot;-100&quot; Thickness=&quot;1&quot; UnitMajor=&quot;50&quot; UnitMinor=&quot;10&quot; AutoMajor=&quot;False&quot; AutoMinor=&quot;False&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;North&quot;&gt;
	  &lt;Text&gt;山側　　　　　　　　　　　　谷川

&lt;/Text&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;50&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;DimGray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; Spacing=&quot;10&quot; AutoSpace=&quot;False&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;-20&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;AnnoFormatString&gt; ; &lt;/AnnoFormatString&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;5&quot; AutoSpace=&quot;False&quot;&gt;
		&lt;Color&gt;DimGray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Visible=&quot;True&quot; Spacing=&quot;1&quot; AutoSpace=&quot;False&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;-20&quot; AnnoFormat=&quot;NumericManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot; Visible=&quot;False&quot;&gt;
	  &lt;AnnoFormatString&gt; ; &lt;/AnnoFormatString&gt;
	  &lt;GridMajor Spacing=&quot;1&quot;&gt;
		&lt;Pattern&gt;Solid&lt;/Pattern&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor AutoSpace=&quot;False&quot;&gt;
		&lt;Pattern&gt;None&lt;/Pattern&gt;
	  &lt;/GridMinor&gt;
	&lt;/Axis&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" />
										</C1WebChart:C1WebChart>
									</asp:Panel>
								</td>
							</tr>
						</table>
						<asp:Literal ID="LitLower" runat="server" EnableViewState="False" Mode="PassThrough"></asp:Literal>
						<asp:Label ID="LblTitleLower" runat="server" BorderColor="Transparent" CssClass="LABELS" EnableTheming="False" EnableViewState="False" Width="285px" BorderStyle="None"></asp:Label>
					</div>
				</ContentTemplate>
				<Triggers>
					<asp:AsyncPostBackTrigger ControlID="ImbtnRedrawGraph" EventName="Click" />
				</Triggers>
			</asp:UpdatePanel>
			<asp:Timer ID="Tmr_Update" runat="server" Enabled="False"></asp:Timer>
			<asp:Panel ID="PnlCalendar" runat="server" CssClass="PanelPadding" EnableTheming="True">
				<asp:UpdatePanel ID="UpdP" runat="server" UpdateMode="Conditional" RenderMode="Inline">
					<ContentTemplate>
						<table id="PopUpCalendar" class="popupTable" >
							<tr>
								<td style="text-align: center; " colspan="2">
									<asp:Label ID="LblExp" runat="server" BackColor="OldLace" BorderColor="SaddleBrown" BorderStyle="Groove" BorderWidth="2px" 
										CssClass="TOPBOTTOMMargin" Font-Size="8pt" Height="40px" Text="カレンダーから日付を選択すると、右のボックスに測定時刻を表示します。その中からグラフに表示する時刻を選択して下さい。" 
										ToolTip="この手順で選択してください。">
									</asp:Label>
								</td>
							</tr>
							<tr>
								<td class="style1">
									<wijmo:C1Calendar ID="C1WebCalendar" runat="server" AutoHide="False" AutoPostBack="True" Height="210px" NavButtons="Quick" NextTooltip="1ヶ月先へ移動" 
										PrevTooltip="1ヶ月前へ移動" QuickNavStep="3" QuickNextTooltip="3か月先へ移動" QuickPrevTooltip="3か月前へ移動" ShowOtherMonthDays="False" 
										TitleFormat="- yyyy年 MMMM -" ToolTipFormat="yyyy年MM月dd日" Width="235px">
										<SelectionMode Days="False" />
									</wijmo:C1Calendar>
									<asp:TextBox ID="txtID" runat="server" Height="5px" style="display:none;" Width="5px"></asp:TextBox>
								</td>
								<td style="vertical-align: top; text-align: left;">
									<asp:ListBox ID="LstBTime" runat="server" AutoPostBack="True" 
										DataTextFormatString="HH:mm" OnSelectedIndexChanged="LstBTime_SelectedIndexChanged" ToolTip="測定時刻を表示します。" CssClass="lstTime">
									</asp:ListBox>
								</td>
							</tr>
							<tr>
								<td class="style2" colspan="2" >
									<input id="btnCancel" onclick="BtnCancel();return false;" type="button" value="キャンセル" style="width: 89px; margin:auto;" />
								</td>
							</tr>
						</table>
					</ContentTemplate>
				</asp:UpdatePanel>
			</asp:Panel>
		</div>
	</form>
</body>
</html>