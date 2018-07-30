<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Graph_TimeChart_SP01.aspx.vb" Inherits="Graph_TimeChartSP01" Culture="ja-JP" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="C1.Web.UI.Controls.3" Namespace="C1.Web.UI.Controls.C1Calendar" TagPrefix="cc1" %>
<%@ Register assembly="C1.Web.C1WebChart.4" namespace="C1.Web.C1WebChart" tagprefix="C1WebChart" %>
<%@ OutputCache Duration="1" VaryByParam="None" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
	<title id="MyTitle" runat="server">経時変化図</title>
	<meta charset="utf-8" />
	<meta http-equiv="X-UA-Compatible" content="IE=9" />
	<meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
	<link href="CSS/stl_graph_table_sp01.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_sexybuttons.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_calendar.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_ToolTip.css" rel="stylesheet" type="text/css" />
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
	<script src="js/updCheckSub.min.js" type="text/javascript"></script>
	<script src="js/calendar.min.js" type="text/javascript"></script>
	<script src="js/grp.min.js" type="text/javascript"></script>
	<script src="js/yanagawaTop.min.js" type="text/javascript" ></script>
	<script src="js/tippy.all.min.js" type="text/javascript"></script>
	<script src="js/ToolTip.min.js" type="text/javascript"></script>
	<script src="js/Simple2D.min.js" type="text/javascript"></script>
</head>
<body>
	<form id="GraphTimeChartFrm" runat="server">
	<div id="FrmGraphTimeChart" lang="ja">
		<input id="nwdt" runat="server" type="hidden"/>
		<input id="nwdtno" runat="server" type="hidden"/>
		<asp:Button style="display:none; visibility:hidden" id="btnSubmit" runat="server" EnableViewState="False"></asp:Button>
		<asp:toolkitscriptmanager ID="AJAX_TKSM" runat="server" EnableScriptGlobalization="True" EnableScriptLocalization="True" EnablePageMethods="True" AsyncPostBackTimeout="180"></asp:toolkitscriptmanager>
		<asp:Panel ID="PnlNoPrint" runat="server" CssClass="dspOnly" Width="656px" EnableTheming="True" Wrap="False" BorderStyle="None" HorizontalAlign="Center">
			<asp:Panel ID="PnlButtons" runat="server" BackColor="#FFF8DC" BorderColor="Olive" BorderStyle="Ridge" EnableTheming="True" Height="37px" Width="650px" Font-Size="2pt" BorderWidth="2px" HorizontalAlign="Left" EnableViewState="False" Wrap="False">
				<div class="rg"><img id="imgloader" src="./img/comm.gif" alt="" class="asyncloader" /></div>
				<asp:ImageButton ID="ImgBtnPrint" runat="server" EnableViewState="False" AlternateText="Print" ImageUrl="~/img/print.png" OnClientClick="window.print();return false;" CausesValidation="False" BorderStyle="None" CssClass="BTNHEADER_PRINT" EnableTheming="False" ToolTip="「印刷」画面を表示します。" />
				<asp:ImageButton ID="ImbtnRedrawGraph" runat="server" EnableViewState="False" AlternateText="Redraw" ImageUrl="~/img/redraw.png" BorderStyle="None" CssClass="BTNHEADER_REDRAW" ToolTip="設定した内容でグラフを再描画します。" EnableTheming="False" />
				<asp:ImageButton ID="ImgBtnClose" runat="server" EnableViewState="False" AlternateText="Close" ImageUrl="~/img/close.png" OnClientClick="window.close();return false;" CausesValidation="False" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" BorderStyle="None" CssClass="BTNHEADER_CLOSE" EnableTheming="False" />
			</asp:Panel>
			<asp:Panel ID="Ph1" runat="server" CssClass="accordionHeader" EnableViewState="False">
				●グラフ設定
				<asp:ImageButton ID="ImgCollExp" runat="server" EnableTheming="False" 
					EnableViewState="False" BorderStyle="None" CssClass="collapseImgButton" 
					ImageUrl="~/img/el.png" />
				<asp:Label ID="LblColl1" runat="server" Font-Size="Small" ForeColor="Gold" BorderStyle="None" />
			</asp:Panel>
			<asp:Panel ID="PnlGraphSet" runat="server" BackColor="White" BorderStyle="None" CssClass="collapsePanel" EnableTheming="False" Width="654px" HorizontalAlign="Center" Wrap="False">
				<asp:Label ID="LblLastUpdate" runat="server" EnableViewState="False"></asp:Label>
				<table style="margin: 5px auto 10px auto; border: 0px solid #996600; padding: 1px; width: 630px; border-spacing: 0px; font-size: 10pt; border-collapse: collapse; text-align: left;">
					<tr>
						<td class="TABLEHEADER">表示期間設定</td>
					</tr>
					<tr>
						<td colspan="1" class="tableContent">
							●表示方法を指定してください。<br />
							&nbsp; &nbsp;「最新から」を選択した場合は、指定された期間遡った日付の0時から表示します。<br />
							&nbsp; &nbsp;「指定期間」を選択した場合は、指定された期間を表示します。<br />
							<div id="DataSpan" class="Term">
								<table id="TABLE5" class="tableShowSpanInner">
									<tr>
										<td class="a">■表示方法</td>
										<td class="b">
											<asp:RadioButton ID="RdBFromNewest" runat="server" BorderStyle="None" EnableTheming="False" GroupName="StartDateSet" Text="最新から" cssClass="radioShow" />
											<asp:DropDownList ID="DDLRange" runat="server" EnableTheming="False" Font-Size="10pt" Width="107px">
											</asp:DropDownList>
										</td>
									</tr>
									<tr>
										<td class="a"></td>
										<td class="b">
											<asp:RadioButton ID="RdBDsignatedDate" runat="server" BorderStyle="None" EnableTheming="False" GroupName="StartDateSet" Text="指定期間" cssClass="radioShow" />
											<asp:TextBox ID="TxtStartDate" runat="server" CausesValidation="True" CssClass="TEXTBOXES textboxDate" EnableTheming="False" MaxLength="10" ToolTip="「指定日」の時に、データ表示開始日時を入力してください" ValidationGroup="EndDay"></asp:TextBox>
											<asp:Image ID="imgCalTxtStartDate" runat="server" EnableTheming="False" 
												ImageAlign="AbsMiddle" ImageUrl="~/img/Calendar.gif" 
												ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" />
											<asp:Label ID="Lbl2" runat="server" BorderStyle="None" CssClass="LABELS" EnableTheming="False" Font-Size="Small" Text="～" Width="22px"></asp:Label>
											<asp:TextBox ID="TxtEndDate" runat="server" CausesValidation="True" CssClass="TEXTBOXES textboxDate" EnableTheming="False" MaxLength="10" ToolTip="「指定日」の時に、データ表示終了日時を入力してください" ValidationGroup="EndDay"></asp:TextBox>
											<asp:Image ID="imgCalTxtEndDate" runat="server" EnableTheming="False" 
												ImageAlign="AbsMiddle" ImageUrl="~/img/Calendar.gif" 
												ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" />
										</td>
									</tr>
								</table>
							</div>
							<div id="thin0" class="Term2">
								<table id="TABLE6" class="tableShowSpanInner">
									<tr>
										<td colspan="3" style="text-align: left">■データ間引き（チェックが１つもない場合は全データを表示します）</td>
									</tr>
									<tr>
										<td class="a" rowspan="2">
										</td>
										<td class="b">
											<asp:CheckBox ID="ChbPartial" runat="server" BorderStyle="None" CssClass="LABELS_Left" EnableTheming="False" Text="間引きを行なう" Width="117px" /></td>
										<td class="c" style="text-align: right">
											<div class="clear">
												<button class="sexybutton sexysimple sexysmall" onclick="Select(true);return false;" style="text-align: right;"><span class="ok">全てチェック</span></button>
												<button class="sexybutton sexysimple sexysmall" onclick="Select(false);return false;" style="text-align: right; ">全てチェックを外す</button>
											</div>
										</td>
									</tr>
									<tr>
										<td class="d" colspan="2">
											<asp:CheckBoxList ID="CBLPartial" runat="server" BorderColor="Tan" BorderStyle="Ridge" BorderWidth="2px" EnableTheming="True" RepeatColumns="6" RepeatDirection="Horizontal" Width="400px">
												<asp:ListItem>00:00</asp:ListItem>
												<asp:ListItem>01:00</asp:ListItem>
												<asp:ListItem>02:00</asp:ListItem>
												<asp:ListItem>03:00</asp:ListItem>
												<asp:ListItem>04:00</asp:ListItem>
												<asp:ListItem>05:00</asp:ListItem>
												<asp:ListItem>06:00</asp:ListItem>
												<asp:ListItem>07:00</asp:ListItem>
												<asp:ListItem>08:00</asp:ListItem>
												<asp:ListItem>09:00</asp:ListItem>
												<asp:ListItem>10:00</asp:ListItem>
												<asp:ListItem>11:00</asp:ListItem>
												<asp:ListItem>12:00</asp:ListItem>
												<asp:ListItem>13:00</asp:ListItem>
												<asp:ListItem>14:00</asp:ListItem>
												<asp:ListItem>15:00</asp:ListItem>
												<asp:ListItem>16:00</asp:ListItem>
												<asp:ListItem>17:00</asp:ListItem>
												<asp:ListItem>18:00</asp:ListItem>
												<asp:ListItem>19:00</asp:ListItem>
												<asp:ListItem>20:00</asp:ListItem>
												<asp:ListItem>21:00</asp:ListItem>
												<asp:ListItem>22:00</asp:ListItem>
												<asp:ListItem>23:00</asp:ListItem>
											</asp:CheckBoxList>
										</td>
									</tr>
								</table>
							</div>
							<asp:MaskedEditExtender ID="MaskedEditExtenderStartDate" runat="server" AutoComplete="False" Mask="9999/99/99" MaskType="Date" TargetControlID="TxtStartDate" UserDateFormat="YearMonthDay" EnableViewState="False"></asp:MaskedEditExtender>
							<asp:MaskedEditExtender ID="MaskedEditExtenderEndDate" runat="server" AutoComplete="False" Mask="9999/99/99" MaskType="Date" TargetControlID="TxtEndDate" UserDateFormat="YearMonthDay" EnableViewState="False"></asp:MaskedEditExtender>
							<asp:RangeValidator ID="RngValidSt" runat="server" ControlToValidate="TxtStartDate" Display="None" EnableTheming="False" EnableViewState="False" ErrorMessage="開始日は1990年以降としてください。" MaximumValue="2050/12/31" MinimumValue="1990/01/01" SetFocusOnError="True" Type="Date"></asp:RangeValidator>
							<asp:RangeValidator ID="RngValidEd" runat="server" ControlToValidate="TxtEndDate" Display="None" EnableTheming="False" EnableViewState="False" ErrorMessage="終了日の指定は2050以前にしてください。" MaximumValue="2050/12/31" MinimumValue="1990/01/01" SetFocusOnError="True" Type="Date"></asp:RangeValidator>
							<asp:RegularExpressionValidator ID="RExValidSt" runat="server" ControlToValidate="TxtStartDate" Display="None" EnableTheming="True" ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))" ValidationGroup="EndDay"></asp:RegularExpressionValidator>
							<asp:RegularExpressionValidator ID="RExValidEd" runat="server" ControlToValidate="TxtEndDate" Display="None" EnableTheming="True" ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))" ValidationGroup="EndDay"></asp:RegularExpressionValidator>
							<asp:ValidatorCalloutExtender ID="ValidCoExRegSt" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="RExValidSt" Width="250px"></asp:ValidatorCalloutExtender>
							<asp:ValidatorCalloutExtender ID="ValidCoExRegEd" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="RExValidEd" Width="250px"></asp:ValidatorCalloutExtender>
							<asp:ValidatorCalloutExtender ID="ValidCoExSt" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="RngValidSt" Width="250px"></asp:ValidatorCalloutExtender>
							<asp:ValidatorCalloutExtender ID="ValidCoExEd" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="RngValidEd" Width="250px"></asp:ValidatorCalloutExtender>
						</td>
					</tr>                    
				</table>
				<table style="margin: 5px auto 10px auto; border: 0px solid #996600; padding: 1px; width: 630px; border-spacing: 0px; font-size: 10pt; border-collapse: collapse; text-align: left;">
					<tr>
						<td class="TABLEHEADER">表示一般設定</td>
					</tr>
					<tr>
						<td class="tableContent" style="padding-top: 10px; padding-bottom: 10px">
							<table id="showparameta" style="width: 100%; text-align: left; border-spacing: 2px;font-size:9pt;">
								<tr>
									<td style="width: 16%">■日付表示形式</td>
									<td style="width: 17%">
										<asp:DropDownList ID="DdlDateFormat" runat="server" Font-Size="9pt" Width="95px">
										</asp:DropDownList>
									</td>
									<td style="width: 21%">■グラフタイトル位置</td>
									<td style="width: 12%">
										<asp:DropDownList ID="DdlTitlePosition" runat="server" Font-Size="9pt" Width="60px" Enabled="False">
											<asp:ListItem Value="0">上</asp:ListItem>
											<asp:ListItem Value="1">下</asp:ListItem>
										</asp:DropDownList>
									</td>
									<td style="width: 18%">■警報値表示</td>
									<td>
										<asp:DropDownList ID="DDLPaintWarningValue" runat="server" Font-Size="9pt" Width="56px"></asp:DropDownList>
									</td>
								</tr>
								<tr>
									<td>■凡例表示</td>
									<td>
										<asp:DropDownList ID="DdlEnableLegend" runat="server" Font-Size="9pt" Width="95px">
										</asp:DropDownList>
									</td>
									<td>■欠測データの連結</td>
									<td>
										<asp:DropDownList ID="DdlContinous" runat="server" Font-Size="9pt" 
											ToolTip="欠測データを直線で補完して表示します" Width="60px">
										</asp:DropDownList>
									</td>
									<td><!--■グラフ自動更新--></td>
									<td>
										<asp:DropDownList ID="DDLAutoUpdate" runat="server" Font-Size="9pt" 
											Visible="False" Width="54px">
											<asp:ListItem Selected="True" Value="0">しない</asp:ListItem>
											<asp:ListItem Value="1">する</asp:ListItem>
										</asp:DropDownList>
									</td>
								</tr>
								<tr>
									<td>■用紙</td>
									<td>
										<asp:DropDownList ID="DdlPaperOrientation" runat="server" DataTextField="用紙方向"
											DataValueField="用紙方向" Font-Bold="False" Font-Size="9pt" Width="95px" Enabled="True">
										</asp:DropDownList>
										 <%--onclick="setCss();return false;"--%>
									</td>
									<td>■グラフ色指定</td>
									<td>
										<asp:DropDownList ID="DdlGraphColorType" runat="server" Font-Size="9pt" 
											ToolTip="モノクロプリンターへ印刷する場合は「グレー」を選択してください。" Width="60px">
											<asp:ListItem Selected="True" Value="0">カラー</asp:ListItem>
											<asp:ListItem Value="1">グレー</asp:ListItem>
										</asp:DropDownList>
									</td>
									<td></td>
									<td>
										<asp:TextBox ID="TxtXAxisAngle" runat="server" BorderStyle="Inset" CssClass="TEXTBOXES" Enabled="False" Font-Size="10pt" ToolTip="現在未対応" Visible="False" Width="46px">0</asp:TextBox>
									</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
				<table class="tableOutline" id="tableScale">
					<tr>
						<td colspan="3" class="TABLEHEADER tableShowSpan">データスケール設定</td>
					</tr>
					<tr>
						<td class="tableLabelNo" id="header">No.</td>
						<td class="tableScaleHead ScaleHeadOther">左 軸　<asp:LinkButton ID="lkBtnLeftAll" runat="server" CssClass="saveBtn">保存</asp:LinkButton></td>
						<td class="tableScaleHead ScaleHeadOther tableBorderLeft">右 軸　<asp:LinkButton ID="lkBtnRightAll" runat="server" CssClass="saveBtn">保存</asp:LinkButton></td>
					</tr>
					<tr id="r1" runat="server">
						<td class="tableLabelNo tableBorderTop">
							<asp:Label ID="LblNo1" runat="server" EnableViewState="False" Text="No.1"/>
							<div runat="server" id="svOut1" class="saveBtnOut"><asp:LinkButton ID="lkBtn1" runat="server" CssClass="saveBtn">保存</asp:LinkButton></div>
						</td>
						<td class="tableChartSet tableBorderTop">
							<asp:Panel ID="Pnl1" runat="server" CssClass="ChartSetPanel">
								<asp:RadioButton ID="RdbNo11" runat="server" GroupName="No1" Text="既定" BorderStyle="None" />
								<asp:DropDownList ID="DdlScale1" runat="server" Width="89px"></asp:DropDownList><br />
								<asp:RadioButton ID="RdbNo12" runat="server" GroupName="No1" Text="入力" BorderStyle="None" />
								<asp:Label ID="Label3" runat="server" EnableViewState="False" Text="最大" CssClass="ChartlblMax" />
								<asp:TextBox ID="TxtMax1" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
								<asp:Label ID="Label4" runat="server" Text="～最小" CssClass="ChartlblMin"></asp:Label>
								<asp:TextBox ID="TxtMin1" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
							</asp:Panel>
						</td>
						<td class="tableChartSet tableBorderTop tableBorderLeft">
							<asp:panel ID="PnlR1" runat="server" cssClass="ChartSetPanel">
								<asp:RadioButton ID="RdbNoR11" runat="server" GroupName="No1R" Text="既定" BorderStyle="None" />
								<asp:DropDownList ID="DdlScaleR1" runat="server" Width="89px"></asp:DropDownList><br />
								<asp:RadioButton ID="RdbNoR12" runat="server" GroupName="No1R" Text="入力" BorderStyle="None" />
								<asp:Label ID="Label19" runat="server" Text="最大" CssClass="ChartlblMax"></asp:Label>
								<asp:TextBox ID="TxtMaxR1" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
								<asp:Label ID="Label20" runat="server" Text="～最小" CssClass="ChartlblMin"></asp:Label>
								<asp:TextBox ID="TxtMinR1" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
							</asp:panel>
						</td>
					</tr>
					<tr id="r2" runat="server">
						<td class="tableLabelNo tableBorderTop">
							<asp:Label ID="LblNo2" runat="server" Text="No.2"></asp:Label>
							<div runat="server" id="svOut2" class="saveBtnOut"><asp:LinkButton ID="lkBtn2" runat="server" CssClass="saveBtn">保存</asp:LinkButton></div>
						</td>
						<td class="tableChartSet tableBorderTop">
							<asp:panel ID="Pnl2" runat="server" cssClass="ChartSetPanel">
								<asp:RadioButton ID="RdbNo21" runat="server" GroupName="No2" Text="既定" BorderStyle="None" />
								<asp:DropDownList ID="DdlScale2" runat="server" Width="89px"></asp:DropDownList><br />
								<asp:RadioButton ID="RdbNo22" runat="server" GroupName="No2" Text="入力" BorderStyle="None" />
								<asp:Label ID="Label5" runat="server" Text="最大" CssClass="ChartlblMax"></asp:Label>
								<asp:TextBox ID="TxtMax2" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
								<asp:Label ID="Label6" runat="server" Text="～最小" CssClass="ChartlblMin"></asp:Label>
								<asp:TextBox ID="TxtMin2" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
							</asp:panel>
						</td>
						<td class="tableChartSet tableBorderTop tableBorderLeft">
							<asp:panel ID="PnlR2" runat="server" cssClass="ChartSetPanel">
								<asp:RadioButton ID="RdbNoR21" runat="server" GroupName="No2R" Text="既定" BorderStyle="None" />
								<asp:DropDownList ID="DdlScaleR2" runat="server" Width="89px"></asp:DropDownList><br />
								<asp:RadioButton ID="RdbNoR22" runat="server" GroupName="No2R" Text="入力" BorderStyle="None" />
								<asp:Label ID="Label21" runat="server" Text="最大" CssClass="ChartlblMax"></asp:Label>
								<asp:TextBox ID="TxtMaxR2" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
								<asp:Label ID="Label22" runat="server" Text="～最小" CssClass="ChartlblMin"></asp:Label>
								<asp:TextBox ID="TxtMinR2" runat="server" CssClass="TEXTBOXES txtScale"></asp:TextBox>
							</asp:panel>
						</td>
					</tr>
				</table>
			</asp:Panel>
			<asp:CollapsiblePanelExtender ID="CollapsiblePanelExtender1" runat="server" CollapseControlID="Ph1" Collapsed="true" CollapsedText="設定表示" ExpandControlID="Ph1"
				ExpandedText="設定非表示" TargetControlID="PnlGraphSet" TextLabelID="LblColl1" Enabled="True" EnableViewState="False" CollapsedImage="~/img/el.png" ExpandedImage="~/img/cl.png" ImageControlID="ImgCollExp" SuppressPostBack="True"></asp:CollapsiblePanelExtender>
			<table id="TABLE4" class="graphChange">
				<tr>
					<td colspan="1" class="TABLEHEADER">グラフの切替え</td>
				</tr>
				<tr>
					<td colspan="1" class="tableContent" style="text-align: center;">
						<asp:DropDownList ID="DDLSelectGraph" runat="server" Font-Size="10pt" 
							Width="620px" AutoPostBack="True" BackColor="White" CssClass="imageSet"></asp:DropDownList>
					</td>
				</tr>
			</table>
		</asp:Panel>
		<div id="GraphPage">
<%--			<div id="picArea" class="grpDiv">
				<table id="picTable" class="grpTable">
					<tbody>
						<tr>
							<td id="leftCel" class="grpTabelCell">
								<asp:Literal ID="litoutSvg" runat="server"></asp:Literal>
							</td>
							<td id="rightCell" class="grpTabelCell">
								<asp:Image ID="contour" runat="server" EnableViewState="False" CssClass="topImage" AlternateText="Contour" ClientIDMode="AutoID" GenerateEmptyAlternateText="True" ViewStateMode="Disabled" />
								<asp:Literal ID="litContour" runat="server"></asp:Literal>
							</td>
						</tr>
					</tbody>
				</table>
			</div>--%>
			<%--			<asp:SliderExtender ID="SliderExtender1"
				runat="server" RaiseChangeOnlyOnMouseUp="False" TargetControlID="TextBox1" Steps="200" Decimals="2" BoundControlID="Label1" Minimum="224" Maximum="301.2" BehaviorID="sld">
			</asp:SliderExtender>
			<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
			<asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>	--%>	
			<asp:UpdateProgress ID="UpdProg" runat="server" EnableViewState="False">
				<ProgressTemplate>
					<br />
					描画中です。しばらくお待ちください。<br />
					<img src="img/loader.gif" alt="読み込み中" style="border-width: 0px;" />
				</ProgressTemplate>
			</asp:UpdateProgress>
			<asp:UpdatePanel ID="UpdatePanel1" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional" EnableViewState="False">
				<ContentTemplate>
					<div id="picArea" class="grpDiv">
						<table id="picTable" class="grpTable">
							<tbody>
								<tr>
									<td id="leftCel" class="grpTabelCell">
										<asp:Literal ID="litoutSvg" runat="server"></asp:Literal>
									</td>
									<td id="rightCell" class="grpTabelCell">
										<asp:Image ID="contour" runat="server" EnableViewState="False" CssClass="topImage" AlternateText="Contour" ClientIDMode="AutoID" GenerateEmptyAlternateText="True" ViewStateMode="Disabled" />
									</td>
								</tr>
							</tbody>
						</table>
					</div>

					<input id="conParam" type="hidden" runat="server" />
					<input id="conheight" type="hidden" runat="server" />
<%--					<div>
						<asp:Label ID="LblDateSpan" runat="server" CssClass="lblDateSpan" EnableTheming="True"></asp:Label>
					</div>--%>
					<div id="TitleUpperOut" runat="server" class="graphtitleOut"><asp:Label ID="LblTitleUpper" runat="server" CssClass="graphtitle" EnableTheming="True" EnableViewState="False"></asp:Label></div>
					<asp:Panel ID="PnlGraph1" runat="server" Height="153px" Width="650px" Visible="False" EnableTheming="True" EnableViewState="False" BorderStyle="None" BorderWidth="0px">
						<C1WebChart:C1WebChart ID="WC1" runat="server" AlternateText="Graph1" 
							CallbackWaitImageUrl="~/img/redraw.gif" EnableCallback="False" Enabled="False" 
							EnableTheming="True" EnableViewState="False" Height="153px" 
							LastDesignUpdate="636258897285917969" Visible="False" Width="650px" ImageQuality="90">
							<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20122.22206&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=General;AlignImage=Center;AlignVert=Top;BackColor=White;Border=None,Black,2;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;Wrap=True;Rounding=1 1 1 1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=White;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Bottom;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=Solid,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Far;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Center;Border=None,Transparent,1;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=Transparent;BackColor2=Transparent;Border=None,ControlDark,1;GradientStyle=None;HatchStyle=None;Opaque=False;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;LeftAxis&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;7.7E+30&quot; DefaultSet=&quot;True&quot;&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;RightAxis&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;7.7E+30&quot;&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;West&quot; LocationDefault=&quot;0, -1&quot;&gt;
	&lt;Text&gt;1&lt;/Text&gt;
  &lt;/Header&gt;
  &lt;Footer Compass=&quot;East&quot; Visible=&quot;False&quot; /&gt;
  &lt;Legend Compass=&quot;East&quot; LocationDefault=&quot;-1, 15&quot; Visible=&quot;True&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;0, 0&quot; SizeDefault=&quot;530, 153&quot; Depth=&quot;20&quot; Rotation=&quot;45&quot; Elevation=&quot;45&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;2&quot; Min=&quot;0&quot; AnnoFormat=&quot;DateManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;South&quot;&gt;
	  &lt;AnnoFormatString&gt;yyyy/MM/dd&lt;/AnnoFormatString&gt;
	  &lt;GridMajor Spacing=&quot;1&quot;&gt;
		&lt;Color&gt;Gray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Spacing=&quot;0.5&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;2&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0.5&quot; UnitMinor=&quot;0.25&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;Text&gt;Axis Y&lt;/Text&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;0.5&quot; /&gt;
	  &lt;GridMinor Spacing=&quot;0.25&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;30&quot; Min=&quot;-10&quot; Thickness=&quot;1&quot; UnitMajor=&quot;2&quot; UnitMinor=&quot;1&quot; AutoMajor=&quot;False&quot; AutoMinor=&quot;False&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;East&quot;&gt;
	  &lt;Text&gt;AxisY2&lt;/Text&gt;
	&lt;/Axis&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" /></C1WebChart:C1WebChart>
					</asp:Panel>
					<div id="TitleLowerOut" runat="server"><asp:Label ID="LblTitleLower" runat="server" CssClass="graphtitle" EnableTheming="True" EnableViewState="False" /></div>
					<asp:Panel ID="PnlGraph2" runat="server" Height="153px" Width="650px" Visible="False" EnableTheming="True" EnableViewState="False" BorderStyle="None" BorderWidth="0px">
						<C1WebChart:C1WebChart ID="WC2" runat="server" AlternateText="Graph2" 
							CallbackWaitImageUrl="~/img/redraw.gif" EnableCallback="False" Enabled="False" 
							EnableTheming="True" EnableViewState="False" Height="153px" 
							LastDesignUpdate="636257731943302421" 
							Visible="False" Width="650px" OnDrawDataSeries="WC1_DrawDataSeries" ImageQuality="90">
							<Serializer Value="&lt;?xml version=&quot;1.0&quot;?&gt;
&lt;Chart2DPropBag Version=&quot;4.0.20122.22206&quot;&gt;
  &lt;StyleCollection&gt;
	&lt;NamedStyle Name=&quot;Legend&quot; ParentName=&quot;Legend.default&quot; StyleData=&quot;AlignHorz=General;AlignImage=Center;AlignVert=Top;BackColor=White;Border=None,Black,2;ForeColor=Black;Opaque=True;Rotation=Rotate0;Wrap=True;Rounding=1 1 1 1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control&quot; ParentName=&quot;Control.default&quot; StyleData=&quot;BackColor=Violet;Border=None,ControlDark,1;Font=MS UI Gothic, 9pt;ForeColor=Black;Opaque=True;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Header&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignHorz=Center;AlignImage=Center;AlignVert=Center;Border=None,Transparent,1;Font=MS UI Gothic, 9.75pt;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Legend.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;Wrap=False;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisX&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Center;AlignVert=Bottom;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate0;&quot; /&gt;
	&lt;NamedStyle Name=&quot;PlotArea&quot; ParentName=&quot;Area&quot; StyleData=&quot;BackColor=Transparent;BackColor2=Transparent;Border=Solid,Black,1;ForeColor=Black;GradientStyle=None;HatchStyle=None;Opaque=True;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Near;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;AxisY2&quot; ParentName=&quot;Area&quot; StyleData=&quot;AlignHorz=Far;AlignVert=Center;Font=MS UI Gothic, 9pt;ForeColor=ControlDarkDark;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Footer&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Center;Border=None,Transparent,1;Rotation=Rotate270;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Control.default&quot; ParentName=&quot;&quot; StyleData=&quot;BackColor=Control;Border=None,Transparent,1;ForeColor=ControlText;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault&quot; ParentName=&quot;LabelStyleDefault.default&quot; StyleData=&quot;AlignVert=Center;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;AlignVert=Top;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;LabelStyleDefault.default&quot; ParentName=&quot;Control&quot; StyleData=&quot;BackColor=Transparent;Border=None,Transparent,1;&quot; /&gt;
	&lt;NamedStyle Name=&quot;Area&quot; ParentName=&quot;Area.default&quot; StyleData=&quot;AlignVert=Top;BackColor=SpringGreen;BackColor2=SpringGreen;Border=Solid,White,1;GradientStyle=None;HatchStyle=None;Opaque=True;Rotation=Rotate0;&quot; /&gt;
  &lt;/StyleCollection&gt;
  &lt;ChartGroupsCollection&gt;
	&lt;ChartGroup Name=&quot;LeftAxis&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;7.7E+30&quot; DefaultSet=&quot;True&quot;&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
	&lt;ChartGroup Name=&quot;RightAxis&quot; Use3D=&quot;False&quot;&gt;
	  &lt;DataSerializer Hole=&quot;7.7E+30&quot;&gt;
		&lt;Highlight /&gt;
	  &lt;/DataSerializer&gt;
	&lt;/ChartGroup&gt;
  &lt;/ChartGroupsCollection&gt;
  &lt;Header Compass=&quot;North&quot; LocationDefault=&quot;0, -1&quot;&gt;
	&lt;Text&gt;1&lt;/Text&gt;
  &lt;/Header&gt;
  &lt;Footer Compass=&quot;East&quot; Visible=&quot;False&quot; /&gt;
  &lt;Legend Compass=&quot;East&quot; LocationDefault=&quot;-1, 15&quot; Visible=&quot;True&quot; /&gt;
  &lt;ChartArea LocationDefault=&quot;0, 0&quot; SizeDefault=&quot;530, 153&quot; Depth=&quot;20&quot; Rotation=&quot;45&quot; Elevation=&quot;45&quot; PlotLocation=&quot;-1, -1&quot; PlotSize=&quot;-1, -1&quot;&gt;
	&lt;Margin /&gt;
  &lt;/ChartArea&gt;
  &lt;Axes&gt;
	&lt;Axis Max=&quot;2&quot; Min=&quot;0&quot; AnnoFormat=&quot;DateManual&quot; Thickness=&quot;1&quot; UnitMajor=&quot;1&quot; UnitMinor=&quot;0.5&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;South&quot;&gt;
	  &lt;AnnoFormatString&gt;yyyy/MM/dd&lt;/AnnoFormatString&gt;
	  &lt;GridMajor Spacing=&quot;1&quot;&gt;
		&lt;Color&gt;Gray&lt;/Color&gt;
	  &lt;/GridMajor&gt;
	  &lt;GridMinor Spacing=&quot;0.5&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;2&quot; Min=&quot;0&quot; Thickness=&quot;1&quot; UnitMajor=&quot;0.5&quot; UnitMinor=&quot;0.25&quot; AutoMajor=&quot;True&quot; AutoMinor=&quot;True&quot; AutoMax=&quot;True&quot; AutoMin=&quot;True&quot; _onTop=&quot;-1&quot; Compass=&quot;West&quot;&gt;
	  &lt;Text&gt;Axis Y&lt;/Text&gt;
	  &lt;GridMajor Visible=&quot;True&quot; Spacing=&quot;0.5&quot; /&gt;
	  &lt;GridMinor Spacing=&quot;0.25&quot; /&gt;
	&lt;/Axis&gt;
	&lt;Axis Max=&quot;30&quot; Min=&quot;-10&quot; Thickness=&quot;1&quot; UnitMajor=&quot;2&quot; UnitMinor=&quot;1&quot; AutoMajor=&quot;False&quot; AutoMinor=&quot;False&quot; AutoMax=&quot;False&quot; AutoMin=&quot;False&quot; _onTop=&quot;-1&quot; Compass=&quot;East&quot;&gt;
	  &lt;Text&gt;AxisY2&lt;/Text&gt;
	&lt;/Axis&gt;
  &lt;/Axes&gt;
  &lt;AutoLabelArrangement /&gt;
  &lt;VisualEffectsData&gt;45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group1=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1;Group0=45,1,0.6,0.1,0.5,0.9,0,0,0.15,0,0,1,0.5,-25,0,0,0,1,64,1&lt;/VisualEffectsData&gt;
&lt;/Chart2DPropBag&gt;" /></C1WebChart:C1WebChart>
					</asp:Panel>
				</ContentTemplate>
				<Triggers>
					<asp:AsyncPostBackTrigger ControlID="ImbtnRedrawGraph" EventName="Click" />
					<asp:AsyncPostBackTrigger ControlID="DDLSelectGraph" EventName="SelectedIndexChanged" />
				</Triggers>
			</asp:UpdatePanel>
		</div>
		<asp:Timer ID="Tmr_Update" runat="server" Enabled="False" />        
		<asp:RegularExpressionValidator ID="RExValidMax1" runat="server" ControlToValidate="TxtMax1" CssClass="BASICSET" Display="None" EnableTheming="True" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression='^[+\-]?[0-9]{1,6}\.?[0-9]{0,5}$'></asp:RegularExpressionValidator>
		<asp:RegularExpressionValidator ID="RExValidMax2" runat="server" ControlToValidate="TxtMax2" CssClass="BASICSET" Display="None" EnableTheming="True" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$"></asp:RegularExpressionValidator>
		<asp:RegularExpressionValidator ID="RExValidMin1" runat="server" ControlToValidate="TxtMin1" CssClass="BASICSET" Display="None" EnableTheming="True" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,5}$"></asp:RegularExpressionValidator>
		<asp:RegularExpressionValidator ID="RExValidMin2" runat="server" ControlToValidate="TxtMin2" CssClass="BASICSET" Display="None" EnableTheming="True" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$"></asp:RegularExpressionValidator>
		<asp:RegularExpressionValidator ID="RExValidMaxR1" runat="server" ControlToValidate="TxtMaxR1" CssClass="BASICSET" Display="None" EnableTheming="True" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$"></asp:RegularExpressionValidator>
		<asp:RegularExpressionValidator ID="RExValidMaxR2" runat="server" ControlToValidate="TxtMaxR2" CssClass="BASICSET" Display="None" EnableTheming="True" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$"></asp:RegularExpressionValidator>
		<asp:RegularExpressionValidator ID="RExValidMinR1" runat="server" ControlToValidate="TxtMinR1" CssClass="BASICSET" Display="None" EnableTheming="True" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$"></asp:RegularExpressionValidator>
		<asp:RegularExpressionValidator ID="RExValidMinR2" runat="server" ControlToValidate="TxtMinR2" CssClass="BASICSET" Display="None" EnableTheming="True" ErrorMessage="<b>入力に間違いがあります。</b><br /><br />数値(半角)のみとしてください。" SetFocusOnError="True" ValidationExpression="^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$"></asp:RegularExpressionValidator>
		<asp:validatorcalloutextender ID="COEMax1" runat="server" TargetControlID="RExValidMax1" HighlightCssClass="validatorCalloutHighligh" Width="250px"></asp:validatorcalloutextender>
		<asp:validatorcalloutextender ID="COEMax2" runat="server" TargetControlID="RExValidMax2" HighlightCssClass="validatorCalloutHighligh" Width="250px"></asp:validatorcalloutextender>
		<asp:validatorcalloutextender ID="COEMin1" runat="server" TargetControlID="RExValidMin1" HighlightCssClass="validatorCalloutHighligh" Width="250px"></asp:validatorcalloutextender>
		<asp:validatorcalloutextender ID="COEMin2" runat="server" TargetControlID="RExValidMin2" HighlightCssClass="validatorCalloutHighligh" Width="250px"></asp:validatorcalloutextender>
		<asp:validatorcalloutextender ID="COEMaxR1" runat="server" TargetControlID="RExValidMaxR1" HighlightCssClass="validatorCalloutHighligh" Width="250px"></asp:validatorcalloutextender>
		<asp:validatorcalloutextender ID="COEMaxR2" runat="server" TargetControlID="RExValidMaxR2" HighlightCssClass="validatorCalloutHighligh" Width="250px"></asp:validatorcalloutextender>
		<asp:validatorcalloutextender ID="COEMinR1" runat="server" TargetControlID="RExValidMinR1" HighlightCssClass="validatorCalloutHighligh" Width="250px"></asp:validatorcalloutextender>
		<asp:validatorcalloutextender ID="COEMinR2" runat="server" TargetControlID="RExValidMinR2" HighlightCssClass="validatorCalloutHighligh" Width="250px"></asp:validatorcalloutextender>
		<asp:updatepanelanimationextender ID="UpdPnlAnimEx" runat="server" Enabled="True" TargetControlID="UpdatePanel1">
			<Animations>
				<OnUpdating>
					<FadeOut Duration="0.3" Fps="25" minimumOpacity="0.1" maximumOpacity="1" />
				</OnUpdating>
				<OnUpdated>
					<FadeIn Duration="0.3" Fps="25" minimumOpacity="0.1" maximumOpacity="1" />
				</OnUpdated>
			</Animations>      
		</asp:updatepanelanimationextender>
	</div>
	<cc1:C1Calendar ID="C1WebCalendar" runat="server" height="230px" width="248px" 
		CalendarTitle="－ {0:yyyy'年'M'月'} －" CultureInfo="ja-JP" 
		Easing="EaseOutQuart" 
		NavigationEffect="Auto" PopupMode="True" 
		UseEmbeddedVisualStyles="False" VisualStyle="OriginalCal" 
		VisualStylePath="~/VisualStyles" ClientOnAfterClose="CalendarClosed" 
		ToolTipDateFormat="yyyy/MM/dd" NextToolTip="1ヶ月先へ" PrevToolTip="1ヶ月前へ" 
		QuickNextToolTip="3ヶ月先へ" QuickPrevToolTip="3ヶ月前へ">
	</cc1:C1Calendar>	               
	</form>
</body>
</html>