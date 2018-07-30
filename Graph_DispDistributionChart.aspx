<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Graph_DispDistributionChart.aspx.vb" Inherits="DisplacementDistributionChart" Trace="false" TraceMode="SortByTime"%>
<%@ Register Assembly="C1.Web.C1WebChart.4" Namespace="C1.Web.C1WebChart" TagPrefix="C1WebChart" %>
<%@ Register assembly="C1.Web.Wijmo.Controls.4" namespace="C1.Web.Wijmo.Controls.C1Calendar" tagprefix="wijmo" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
	<title id="MyTitle" runat="server">変位分布図</title>
	<meta charset="utf-8" />
	<%--<meta http-equiv="X-UA-Compatible" content="IE=7" />--%>
	<meta name="viewport" content="width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no" />
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
	<form id="DispDistributionChartFrm" runat="server">
		<div id="FrmDispDistributionChart" lang="ja">
			<input id="nwdt" runat="server" type="hidden"/>
			<input id="nwdtno" runat="server" type="hidden"/>
		<asp:Button style="display:none; visibility:hidden" id="btnSubmit" runat="server" EnableViewState="False"></asp:Button>
			<asp:ToolkitScriptManager id="ToolkitScriptManager1" runat="server" EnableScriptLocalization="True" EnableScriptGlobalization="True"></asp:ToolkitScriptManager>
			<asp:Panel id="PnlNoPrint" runat="server" Wrap="False" Width="660px" EnableTheming="True" CssClass="dspOnly">
				<asp:Panel id="PnlButtons" runat="server" CssClass="pnlButtons">
					<div class="rg"><img id="imgloader" src="./img/comm.gif" alt="" class="asyncloader" /></div>
					<asp:ImageButton ID="ImgBtnPrint" runat="server" AlternateText="Print" ImageUrl="~/img/print.png" OnClientClick="window.print();return false;" TabIndex="10" CssClass="BTNHEADER_PRINT" ToolTip="「印刷」画面を表示します。" />
					<asp:ImageButton ID="ImbtnRedrawGraph" runat="server" AlternateText="Redraw" EnableViewState="False" ImageUrl="~/img/redraw.png" CssClass="BTNHEADER_REDRAW" ToolTip="設定した内容で再描画します。" />
					<asp:ImageButton ID="ImgBtnClose" runat="server" AlternateText="Close" CausesValidation="False" EnableViewState="False" ImageUrl="~/img/close.png" OnClientClick="window.close();return false;" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" CssClass="BTNHEADER_CLOSE" />
				</asp:Panel>
				<asp:Panel id="Ph1" runat="server" CssClass="accordionHeader" EnableViewState="False">
							●グラフ設定
					<asp:ImageButton ID="ImgCollExp" runat="server" EnableTheming="False" EnableViewState="False" cssclass="imgCollaps" />
					<asp:Label ID="LblColl1" runat="server" cssclass="lblCollaps" />
				</asp:Panel>
				<asp:Panel id="PnlGraphSet" runat="server" Width="654px" EnableTheming="True" 
					CssClass="collapsePanel">
					<asp:Label ID="LblLastUpdate" cssclass="lblUpdate" runat="server"></asp:Label>
					<table id="TABLE1" class="tableOutline unselectable">
						<tr>
							<td class="TABLEHEADER tableShowSpan" colspan="1">表示期間設定</td>
						</tr>
						<tr>
							<td colspan="1" class="tableShowType">
								●表示方法を指定してください。<br />
									「最新」を選択した場合は、サーバに収録された最新のデータを表示します。<br />
									「指定日時」を選択した場合は、指定された日時のデータを表示します。<br /> 
								<br />
								■表示方法
								<table class="tableSelDate">
									<tr class="depthShowRow">
										<td class="selDateCol1">
											<asp:RadioButton ID="RdBFromNewest" runat="server" EnableTheming="True" GroupName="SelectOutputType" Text="最新　　" cssClass="radioShow" Checked="True" /></td>
										<td colspan="2">
											サーバ内の最新データを表示します
										</td>
									</tr>
									<tr class="depthShowRow">
										<td class="selDateCol1">
											<asp:RadioButton ID="RdBDsignatedDate" runat="server" EnableTheming="True" GroupName="SelectOutputType" Text="指定日時" cssClass="radioShow" TabIndex="1" /></td>
										<td class="selDateCol2">
											<asp:TextBox ID="TxtDate0" runat="server" CssClass="dateTextBoxes" EnableTheming="True" MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="2"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate0" runat="server" EnableTheming="True" ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" TabIndex="9" EnableViewState="False" ImageAlign="AbsMiddle" /></td>
										<td class="selDateCol3">
											<asp:RegularExpressionValidator ID="REValid0" runat="server" BackColor="LightCoral"
												ControlToValidate="TxtDate0" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												ForeColor="MediumBlue" SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" TabIndex="9">日時を確認して下さい
											</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid0" runat="server" BackColor="LightCoral" ControlToValidate="TxtDate0" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="MediumBlue" SetFocusOnError="True"
												ValidationGroup="SetDay" Width="169px">
											</asp:RangeValidator>
										</td>
									</tr>
									<tr class="depthShowRow">
										<td class="selDateCol1"></td>
										<td class="selDateCol2">
											<asp:TextBox ID="TxtDate1" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True" MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="3"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate1" runat="server" EnableTheming="True" ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" EnableViewState="False" TabIndex="10" ImageAlign="AbsMiddle" />
										</td>
										<td class="selDateCol3">
											<asp:RegularExpressionValidator ID="REValid1" runat="server" BackColor="LightCoral"
												ControlToValidate="TxtDate1" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="MediumBlue">日時を確認して下さい
											</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid1" runat="server" BackColor="LightCoral" ControlToValidate="TxtDate1" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="MediumBlue" SetFocusOnError="True" ValidationGroup="SetDay" Width="169px">
											</asp:RangeValidator>
										</td>
									</tr>
									<tr class="depthShowRow">
										<td class="selDateCol1"></td>
										<td class="selDateCol2">
											<asp:TextBox ID="TxtDate2" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True" MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="4"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate2" runat="server" EnableTheming="True" ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" EnableViewState="False" TabIndex="11" ImageAlign="AbsMiddle" />
										</td>
										<td class="selDateCol3">
											<asp:RegularExpressionValidator ID="REValid2" runat="server" BackColor="LightCoral"
												ControlToValidate="TxtDate2" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="MediumBlue">日時を確認して下さい
											</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid2" runat="server" BackColor="LightCoral" ControlToValidate="TxtDate2" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="MediumBlue" SetFocusOnError="True"
												ValidationGroup="SetDay" Width="169px">
											</asp:RangeValidator>
										</td>
									</tr>
									<tr class="depthShowRow">
										<td class="selDateCol1"></td>
										<td class="selDateCol2">
											<asp:TextBox ID="TxtDate3" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True" MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="5"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate3" runat="server" EnableTheming="True" ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" EnableViewState="False" TabIndex="12" ImageAlign="AbsMiddle" />
										</td>
										<td class="selDateCol3">
											<asp:RegularExpressionValidator ID="REValid3" runat="server" BackColor="LightCoral"
												ControlToValidate="TxtDate3" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="MediumBlue">日時を確認して下さい
											</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid3" runat="server" BackColor="LightCoral" ControlToValidate="TxtDate3" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="MediumBlue" SetFocusOnError="True"
												ValidationGroup="SetDay" Width="169px"></asp:RangeValidator>
										</td>
									</tr>
									<tr class="depthShowRow">
										<td class="selDateCol1"></td>
										<td class="selDateCol2">
											<asp:TextBox ID="TxtDate4" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True" MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="6"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate4" runat="server" EnableTheming="True" ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" EnableViewState="False" TabIndex="13" ImageAlign="AbsMiddle" />
										</td>
										<td class="selDateCol3">
											<asp:RegularExpressionValidator ID="REValid4" runat="server" BackColor="LightCoral"
												ControlToValidate="TxtDate4" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="MediumBlue">日時を確認して下さい
											</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid4" runat="server" BackColor="LightCoral" ControlToValidate="TxtDate4" Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="MediumBlue" SetFocusOnError="True"
												ValidationGroup="SetDay" Width="169px">
											</asp:RangeValidator>
										</td>
									</tr>
									<tr class="depthShowRow">
										<td class="selDateCol1"></td>
										<td class="selDateCol2">
											<asp:TextBox ID="TxtDate5" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True" MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="7"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate5" runat="server" EnableTheming="True" ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" EnableViewState="False" TabIndex="14" ImageAlign="AbsMiddle" /></td>
										<td class="selDateCol3">
											<asp:RegularExpressionValidator ID="REValid5" runat="server" BackColor="LightCoral"
												ControlToValidate="TxtDate5" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="MediumBlue">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid5" runat="server" BackColor="LightCoral" ControlToValidate="TxtDate5"
												Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="MediumBlue" SetFocusOnError="True"
												ValidationGroup="SetDay" Width="169px"></asp:RangeValidator></td>
									</tr>
									<tr class="depthShowRow">
										<td class="selDateCol1"></td>
										<td class="selDateCol2">
											<asp:TextBox ID="TxtDate6" runat="server" CausesValidation="True" CssClass="dateTextBoxes" EnableTheming="True" MaxLength="16" ToolTip="「指定日時」の時に、データ表示日時を入力してください" ValidationGroup="SetDay" TabIndex="8"></asp:TextBox>
											<asp:Image ID="imgCalTxtDate6" runat="server" EnableTheming="True" ImageUrl="~/img/Calendar.gif" ToolTip="ここをクリックするとカレンダーを表示します" EnableViewState="False" TabIndex="15" ImageAlign="AbsMiddle" /></td>
										<td class="selDateCol3">
											<asp:RegularExpressionValidator ID="REValid6" runat="server" BackColor="LightCoral"
												ControlToValidate="TxtDate6" Display="Dynamic" ErrorMessage="指定日時は正しくありません。再入力してください。"
												SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29)).(2[0-3]|[01]?[0-9])\:[0-5]\d"
												ValidationGroup="SetDay" ForeColor="MediumBlue">日時を確認して下さい</asp:RegularExpressionValidator>
											<asp:RangeValidator ID="RngValid6" runat="server" BackColor="LightCoral" ControlToValidate="TxtDate6"
												Display="Dynamic" ErrorMessage="本日より過去2年間までです" ForeColor="MediumBlue" SetFocusOnError="True"
												ValidationGroup="SetDay" Width="169px"></asp:RangeValidator></td>
									</tr>
								</table>
							</td>
						</tr>
					</table>
					<table id="TABLE2" class="tableOutline">
						<tr>
							<td class="TABLEHEADER tableShowSpan" colspan="1">表示一般設定</td>
						</tr>
						<tr>
							<td colspan="1" style="font-size: 9pt; width: 633px; height: 28px; background-color: #FFF8DC; text-align: left;">
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
						<tr id="r5" runat="server">
							<td class="tableLabelNo tableBorderTop">
								<asp:Label ID="LblNo5" runat="server" EnableViewState="False" Text="No.5"></asp:Label>
							</td>
							<td class="tableScaleBody tableBorderTop">
								<div id="Pnl5" runat="server">
									<div class="divFloat">
										<asp:RadioButtonList ID="RBList5" runat="server" CssClass = "rbLists">
											<asp:ListItem Value="0">既定</asp:ListItem>
											<asp:ListItem Value="1">入力</asp:ListItem>
										</asp:RadioButtonList>
									</div>
									<div class="divFloat tableScaleBody2" >
										<asp:DropDownList ID="DdlScale5" runat="server" Width="89px"></asp:DropDownList><br />
										<asp:Label ID="Label11" runat="server" EnableViewState="False" Text="最大" CssClass="ChartlblMax"></asp:Label>
										<asp:TextBox ID="TxtMax5" runat="server" CssClass="txtScale"></asp:TextBox>
										<asp:Label ID="Label12" runat="server" EnableViewState="False" Text="～最小" CssClass="ChartlblMin"></asp:Label>
										<asp:TextBox ID="TxtMin5" runat="server" CssClass="txtScale"></asp:TextBox>
									</div>
								</div>
							</td>
						</tr>
						<tr id="r6" runat="server">
							<td class="tableLabelNo tableBorderTop">
								<asp:Label ID="LblNo6" runat="server" EnableViewState="False" Text="No.6"></asp:Label>
							</td>
							<td class="tableScaleBody tableBorderTop">
								<div id="Pnl6" runat="server">
									<div class="divFloat">
										<asp:RadioButtonList ID="RBList6" runat="server" CssClass = "rbLists">
											<asp:ListItem Value="0">既定</asp:ListItem>
											<asp:ListItem Value="1">入力</asp:ListItem>
										</asp:RadioButtonList>
									</div>
									<div class="divFloat tableScaleBody2" >
										<asp:DropDownList ID="DdlScale6" runat="server" Width="89px"></asp:DropDownList><br />
										<asp:Label ID="Label19" runat="server" EnableViewState="False" Text="最大" CssClass="ChartlblMax"></asp:Label>
										<asp:TextBox ID="TxtMax6" runat="server" CssClass="txtScale"></asp:TextBox>
										<asp:Label ID="Label20" runat="server" EnableViewState="False" Text="～最小" CssClass="ChartlblMin"></asp:Label>
										<asp:TextBox ID="TxtMin6" runat="server" CssClass="txtScale"></asp:TextBox>
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
					<asp:PopupControlExtender ID="pce0" runat="server" TargetControlID="imgCalTxtDate0" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetX="5" DynamicContextKey="0" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
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
					<asp:PopupControlExtender ID="pce3" runat="server" TargetControlID="imgCalTxtDate3" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetX="5" DynamicContextKey="3" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
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
					<asp:PopupControlExtender ID="pce4" runat="server" TargetControlID="imgCalTxtDate4" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetX="5" DynamicContextKey="4" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
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
					<asp:PopupControlExtender ID="pce5" runat="server" TargetControlID="imgCalTxtDate5" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetX="5" DynamicContextKey="5" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
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
					<asp:PopupControlExtender ID="pce6" runat="server" TargetControlID="imgCalTxtDate6" CommitProperty="Value" CommitScript="inData(e);" PopupControlID="PnlCalendar" Position="Right" OffsetX="5" DynamicContextKey="6" DynamicControlID="txtID" DynamicServiceMethod="GetDynamicContent">
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
				<asp:CollapsiblePanelExtender id="CollapsiblePanelExtender1" runat="server" EnableViewState="False" TextLabelID="LblColl1" TargetControlID="PnlGraphSet" ImageControlID="ImgCollExp" ExpandedText="設定非表示" ExpandedImage="~/img/cl.png" ExpandControlID="Ph1" Enabled="True" CollapsedText="設定表示" CollapsedImage="~/img/el.png" Collapsed="true" CollapseControlID="Ph1">
				</asp:CollapsiblePanelExtender>
			</asp:Panel>
			<asp:UpdateProgress id="UpdProg" runat="server" DisplayAfter="200" AssociatedUpdatePanelID="UpdPanel2">
				<ProgressTemplate>
					<br />
					描画中です。しばらくお待ちください。<br />
					<img src="img/loader.gif" alt= "読み込み中" />
				</ProgressTemplate>
			</asp:UpdateProgress>
			<asp:UpdatePanel id="UpdPanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False">
				<ContentTemplate>
					<div id="GraphOuter" runat="server">
						<asp:Label id="LblTitleUpper" runat="server" Width="470px" EnableTheming="False" CssClass="titleUpper" EnableViewState="False" Visible="False" />
						<asp:Panel id="PnlGraph1" runat="server" EnableTheming="False" EnableViewState="False">
							<C1WebChart:C1WebChart id="WC1" runat="server" Width="650px" 
								EnableTheming="True" Height="228px" Enabled="False" Visible="False" 
								CallbackWaitImageUrl="" LastDesignUpdate="636274113259702578">
									<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20122.22206&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=General;AlignImage=Center;AlignVert=Top;BackColor=White;BackColor2=;ForeColor=Black;GradientStyle=None;Opaque=True;Rotation=Rotate0;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=Transparent;BackColor2=Transparent;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;GradientStyle=None;HatchStyle=None;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=White;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;BackColor=Azure;BackColor2=SkyBlue;Border=None,SkyBlue,1;Font=MS UI Gothic, 9.75pt;GradientStyle=Vertical;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Far;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Center;BackColor=Transparent;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Opaque=False;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=Solid,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Bottom;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;Group1&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;Group2&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;East&quot; /&gt;
  &lt;Footer Compass=&quot;East&quot; /&gt;
  &lt;Legend Compass=&quot;East&quot; Visible=&quot;False&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;-1, -1&quot; SizeDefault=&quot;-1, -1&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;South&quot; /&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;GridMajor Thickness=&quot;0&quot; /&gt;
	  &lt;GridMinor Thickness=&quot;0&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; Compass=&quot;East&quot; /&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" />
							</C1WebChart:C1WebChart>
						</asp:Panel>
						<asp:Panel id="PnlGraph2" runat="server" EnableTheming="False" EnableViewState="False">
							<C1WebChart:C1WebChart id="WC2" runat="server" Width="650px" EnableTheming="True" Height="228px" Enabled="False" Visible="False" CallbackWaitImageUrl="">
								<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;2.0.20083.19012&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;GradientStyle=None;Border=Solid,Black,1;BackColor=Transparent;ForeColor=Black;HatchStyle=None;BackColor2=Transparent;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;Border=None,Transparent,1;ForeColor=ControlText;BackColor=Control;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;BackColor2=Transparent;Border=None,ControlDark,1;Opaque=False;GradientStyle=None;BackColor=Transparent;Font=MS UI Gothic, 9pt;HatchStyle=None;Rotation=Rotate0;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate270;AlignVert=Center;Font=MS UI Gothic, 9pt;AlignHorz=Far;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Wrap=False;Border=None,Transparent,1;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Border=None,Transparent,1;BackColor=Transparent;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Center;Border=None,Transparent,1;Rotation=Rotate270;BackColor=Transparent;Font=MS UI Gothic, 9.75pt;Opaque=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Border=None,Transparent,1;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate270;AlignVert=Center;Font=MS UI Gothic, 9pt;AlignHorz=Near;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate0;AlignVert=Bottom;Font=MS UI Gothic, 9pt;AlignHorz=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;Rotation=Rotate0;BackColor=White;ForeColor=Black;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;BackColor2=SkyBlue;Border=None,SkyBlue,1;AlignVert=Center;Opaque=False;GradientStyle=Vertical;BackColor=Azure;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;AlignImage=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;BackColor2=;Opaque=True;AlignImage=Center;GradientStyle=None;Rotation=Rotate0;BackColor=White;Wrap=False;ForeColor=Black;AlignHorz=General;AlignVert=Top;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;Group1&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;Group2&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;East&quot; /&gt;
  &lt;Footer Compass=&quot;East&quot; /&gt;
  &lt;Legend Compass=&quot;East&quot; Visible=&quot;False&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;-1, -1&quot; SizeDefault=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;South&quot; /&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;GridMajor Thickness=&quot;0&quot; /&gt;
	  &lt;GridMinor Thickness=&quot;0&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; Compass=&quot;East&quot; /&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
&lt;/Chart2DPropBag&gt;" />
							</C1WebChart:C1WebChart>
						</asp:Panel>
						<asp:Panel id="PnlGraph3" runat="server" EnableTheming="False" EnableViewState="False">
							<C1WebChart:C1WebChart id="WC3" runat="server" Width="650px" EnableTheming="True" Height="228px" Enabled="False" Visible="False" CallbackWaitImageUrl="">
								<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;2.0.20083.19012&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;GradientStyle=None;Border=Solid,Black,1;BackColor=Transparent;ForeColor=Black;HatchStyle=None;BackColor2=Transparent;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;Border=None,Transparent,1;ForeColor=ControlText;BackColor=Control;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;BackColor2=Transparent;Border=None,ControlDark,1;Opaque=False;GradientStyle=None;BackColor=Transparent;Font=MS UI Gothic, 9pt;HatchStyle=None;Rotation=Rotate0;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate270;AlignVert=Center;Font=MS UI Gothic, 9pt;AlignHorz=Far;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Wrap=False;Border=None,Transparent,1;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Border=None,Transparent,1;BackColor=Transparent;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Center;Border=None,Transparent,1;Rotation=Rotate270;BackColor=Transparent;Font=MS UI Gothic, 9.75pt;Opaque=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Border=None,Transparent,1;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate270;AlignVert=Center;Font=MS UI Gothic, 9pt;AlignHorz=Near;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate0;AlignVert=Bottom;Font=MS UI Gothic, 9pt;AlignHorz=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;Rotation=Rotate0;BackColor=White;ForeColor=Black;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;BackColor2=SkyBlue;Border=None,SkyBlue,1;AlignVert=Center;Opaque=False;GradientStyle=Vertical;BackColor=Azure;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;AlignImage=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;BackColor2=;Opaque=True;AlignImage=Center;GradientStyle=None;Rotation=Rotate0;BackColor=White;Wrap=False;ForeColor=Black;AlignHorz=General;AlignVert=Top;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;Group1&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;Group2&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;East&quot; /&gt;
  &lt;Footer Compass=&quot;East&quot; /&gt;
  &lt;Legend Compass=&quot;East&quot; Visible=&quot;False&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;-1, -1&quot; SizeDefault=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;South&quot; /&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;GridMajor Thickness=&quot;0&quot; /&gt;
	  &lt;GridMinor Thickness=&quot;0&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; Compass=&quot;East&quot; /&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
&lt;/Chart2DPropBag&gt;" />
							</C1WebChart:C1WebChart>
						</asp:Panel>
						<asp:Panel id="PnlGraph4" runat="server" EnableTheming="False" EnableViewState="False">
							<C1WebChart:C1WebChart id="WC4" runat="server" Width="650px" EnableTheming="True" Height="228px" Enabled="False" Visible="False" CallbackWaitImageUrl="">
								<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;2.0.20083.19012&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;GradientStyle=None;Border=Solid,Black,1;BackColor=Transparent;ForeColor=Black;HatchStyle=None;BackColor2=Transparent;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;Border=None,Transparent,1;ForeColor=ControlText;BackColor=Control;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;BackColor2=Transparent;Border=None,ControlDark,1;Opaque=False;GradientStyle=None;BackColor=Transparent;Font=MS UI Gothic, 9pt;HatchStyle=None;Rotation=Rotate0;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate270;AlignVert=Center;Font=MS UI Gothic, 9pt;AlignHorz=Far;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Wrap=False;Border=None,Transparent,1;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Border=None,Transparent,1;BackColor=Transparent;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Center;Border=None,Transparent,1;Rotation=Rotate270;BackColor=Transparent;Font=MS UI Gothic, 9.75pt;Opaque=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Border=None,Transparent,1;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate270;AlignVert=Center;Font=MS UI Gothic, 9pt;AlignHorz=Near;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate0;AlignVert=Bottom;Font=MS UI Gothic, 9pt;AlignHorz=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;Rotation=Rotate0;BackColor=White;ForeColor=Black;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;BackColor2=SkyBlue;Border=None,SkyBlue,1;AlignVert=Center;Opaque=False;GradientStyle=Vertical;BackColor=Azure;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;AlignImage=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;BackColor2=;Opaque=True;AlignImage=Center;GradientStyle=None;Rotation=Rotate0;BackColor=White;Wrap=False;ForeColor=Black;AlignHorz=General;AlignVert=Top;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;Group1&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;Group2&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;East&quot; /&gt;
  &lt;Footer Compass=&quot;East&quot; /&gt;
  &lt;Legend Compass=&quot;East&quot; Visible=&quot;False&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;-1, -1&quot; SizeDefault=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;South&quot; /&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;GridMajor Thickness=&quot;0&quot; /&gt;
	  &lt;GridMinor Thickness=&quot;0&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; Compass=&quot;East&quot; /&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
&lt;/Chart2DPropBag&gt;" />
							</C1WebChart:C1WebChart>
						</asp:Panel>
						<asp:Panel id="PnlGraph5" runat="server" EnableTheming="False" EnableViewState="False">
							<C1WebChart:C1WebChart id="WC5" runat="server" Width="650px" EnableTheming="True" Height="228px" Enabled="False" Visible="False" CallbackWaitImageUrl="">
								<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;2.0.20083.19012&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;GradientStyle=None;Border=Solid,Black,1;BackColor=Transparent;ForeColor=Black;HatchStyle=None;BackColor2=Transparent;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;Border=None,Transparent,1;ForeColor=ControlText;BackColor=Control;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;BackColor2=Transparent;Border=None,ControlDark,1;Opaque=False;GradientStyle=None;BackColor=Transparent;Font=MS UI Gothic, 9pt;HatchStyle=None;Rotation=Rotate0;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate270;AlignVert=Center;Font=MS UI Gothic, 9pt;AlignHorz=Far;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Wrap=False;Border=None,Transparent,1;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Border=None,Transparent,1;BackColor=Transparent;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Center;Border=None,Transparent,1;Rotation=Rotate270;BackColor=Transparent;Font=MS UI Gothic, 9.75pt;Opaque=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;Border=None,Transparent,1;AlignVert=Top;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate270;AlignVert=Center;Font=MS UI Gothic, 9pt;AlignHorz=Near;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;ForeColor=ControlDarkDark;Rotation=Rotate0;AlignVert=Bottom;Font=MS UI Gothic, 9pt;AlignHorz=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;Rotation=Rotate0;BackColor=White;ForeColor=Black;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;BackColor2=SkyBlue;Border=None,SkyBlue,1;AlignVert=Center;Opaque=False;GradientStyle=Vertical;BackColor=Azure;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;AlignImage=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;BackColor2=;Opaque=True;AlignImage=Center;GradientStyle=None;Rotation=Rotate0;BackColor=White;Wrap=False;ForeColor=Black;AlignHorz=General;AlignVert=Top;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;Group1&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;Group2&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;East&quot; /&gt;
  &lt;Footer Compass=&quot;East&quot; /&gt;
  &lt;Legend Compass=&quot;East&quot; Visible=&quot;False&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;-1, -1&quot; SizeDefault=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;South&quot; /&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;GridMajor Thickness=&quot;0&quot; /&gt;
	  &lt;GridMinor Thickness=&quot;0&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; Compass=&quot;East&quot; /&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
&lt;/Chart2DPropBag&gt;" />
							</C1WebChart:C1WebChart>
						</asp:Panel>
						<asp:Panel id="PnlGraph6" runat="server" EnableTheming="False" EnableViewState="False">
							<C1WebChart:C1WebChart id="WC6" runat="server" Width="650px" EnableTheming="True" Height="228px" Enabled="False" Visible="False" CallbackWaitImageUrl="" LastDesignUpdate="636270209451328125">
								<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20112.21131&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=General;AlignImage=Center;AlignVert=Top;BackColor=White;BackColor2=;ForeColor=Black;GradientStyle=None;Opaque=True;Rotation=Rotate0;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=Transparent;BackColor2=Transparent;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;GradientStyle=None;HatchStyle=None;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=White;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;BackColor=Azure;BackColor2=SkyBlue;Border=None,SkyBlue,1;Font=MS UI Gothic, 9.75pt;GradientStyle=Vertical;Opaque=False;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Far;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Center;BackColor=Transparent;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Opaque=False;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=Solid,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Bottom;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;Group1&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;Group2&quot;&gt;
	  &lt;DataSerializer&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;East&quot; /&gt;
  &lt;Footer Compass=&quot;East&quot; /&gt;
  &lt;Legend Compass=&quot;East&quot; Visible=&quot;False&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;-1, -1&quot; SizeDefault=&quot;-1, -1&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;South&quot; /&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;GridMajor Thickness=&quot;0&quot; /&gt;
	  &lt;GridMinor Thickness=&quot;0&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;0&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0&quot; UnitMinor=&quot;0&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; Compass=&quot;East&quot; /&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" />
							</C1WebChart:C1WebChart>
						</asp:Panel>
						<asp:Label id="LblTitleLower" runat="server" Width="285px" EnableTheming="False" CssClass="titleLower" EnableViewState="False" BorderStyle="None" BorderColor="Transparent" />
					</div>
				</ContentTemplate>
				<Triggers>
					<asp:AsyncPostBackTrigger ControlID="ImbtnRedrawGraph" EventName="Click" />
				</Triggers>
			</asp:UpdatePanel>
			<asp:Timer id="Tmr_Update" runat="server" Enabled="False"></asp:Timer> 
			<asp:updatepanelanimationextender id="UpdatePanelAnimationExtender1" runat="server" TargetControlID="UpdPanel2">
				<Animations>
					<OnUpdating>
						<FadeOut Duration="0.3" Fps="25" minimumOpacity="0.1" maximumOpacity="1" />
					</OnUpdating>
					<OnUpdated>
						<FadeIn Duration="0.3" Fps="25" minimumOpacity="0.1" maximumOpacity="1" />
					</OnUpdated>
				</Animations>
			</asp:updatepanelanimationextender>
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
