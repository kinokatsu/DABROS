<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Table_TimeSeries.aspx.vb" Inherits="Table_TimeSeries" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="C1.Web.UI.Controls.3" Namespace="C1.Web.UI.Controls.C1Calendar" TagPrefix="cc1" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
	<title id="MyTitle" runat="server">経時表</title>
	<meta charset="utf-8" />
	<meta name="viewport" content="width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no" />
	<meta http-equiv="X-UA-Compatible" content="IE=9" />
	<link href="CSS/stl_sexybuttons.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_graph_table.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_calendar.css" rel="stylesheet" type="text/css" />
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
	<script src="js/updCheckSub.min.js" type="text/javascript"></script>
	<script src="js/calendar.min.js" type="text/javascript"></script>
	<script src="js/grp.min.js" type="text/javascript"></script>
	<asp:Literal ID="dcss" runat="server"></asp:Literal>
</head>
<body>
	<form id="TableTimeSeriesFrm" runat="server">
	<div id="TableTimeSeries" lang="ja" title="経時表">
		<input id="nwdt" runat="server" type="hidden"/>
		<input id="nwdtno" runat="server" type="hidden"/>
		<asp:Button style="display: none; visibility: hidden" id="btnSubmit" runat="server" EnableViewState="False"></asp:Button>
		<asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="180" EnableScriptGlobalization="True" EnablePageMethods="True"></asp:ToolkitScriptManager>
		<asp:Panel ID="PnlNoPrint" runat="server" CssClass="dspOnly" Width="656px" EnableTheming="True" Wrap="False" HorizontalAlign="Center" BorderStyle="None">
			<asp:Panel ID="PnlButtons" runat="server" BackColor="Cornsilk" BorderColor="Olive" BorderStyle="Ridge" EnableTheming="False" Height="37px" Width="650px" Font-Size="2pt" BorderWidth="2px" HorizontalAlign="Left" EnableViewState="False" Wrap="False">
				<div class="rg"><img id="imgloader" src="./img/comm.gif" alt="" class="asyncloader" /></div>
				<asp:ImageButton ID="ImgBtnPrint" runat="server" EnableViewState="False" AlternateText="Print" ImageUrl="~/img/print.png" OnClientClick="window.print();return false;" CausesValidation="False" BorderStyle="None" CssClass="BTNHEADER_PRINT" ToolTip="「印刷」画面を表示します。" />
				<asp:ImageButton ID="ImbtnRedrawGraph" runat="server" EnableViewState="False" AlternateText="Redraw" ImageUrl="~/img/redraw.png" ToolTip="設定した内容でグラフを再描画します。" BorderStyle="None" CssClass="BTNHEADER_REDRAW" />
				<asp:ImageButton ID="ImgBtnClose" runat="server" EnableViewState="False" AlternateText="Close" ImageUrl="~/img/close.png" OnClientClick="window.close();return false;" CausesValidation="False" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" BorderStyle="None" CssClass="BTNHEADER_CLOSE" />
			</asp:Panel>
			<asp:Panel ID="Ph1" runat="server" CssClass="accordionHeader" EnableViewState="false">
				●作表設定
				<asp:ImageButton ID="ImgCollExp" runat="server" EnableTheming="False" EnableViewState="False" BorderStyle="None" CssClass="collapseImgButton"/>
				<asp:Label ID="LblColl1" runat="server" Font-Size="Small" ForeColor="Gold" 
					EnableTheming="True" BorderStyle="None"></asp:Label>
			</asp:Panel>
			<asp:Panel ID="PnlGraphSet" runat="server" BackColor="Transparent" 
				CssClass="collapsePanel" EnableTheming="False" Width="654px" 
				HorizontalAlign="Center" Wrap="False" BorderStyle="None">
				<asp:Label ID="LblLastUpdate" runat="server"  EnableTheming="True"></asp:Label>
				<table style="margin: 5px auto 5px auto; border: 0px solid #996600; padding: 1px; width: 630px; border-spacing: 0px; font-size: 10pt; border-collapse: collapse; text-align: left;">
					<tr>
						<td class="TABLEHEADER">
							表示期間設定</td>
					</tr>
					<tr>
						<td colspan="1" class="tableContent">
							●表示方法を指定してください。<br />
							&nbsp;&nbsp;
										「最新から」を選択した場合は、指定された期間遡った日付の0時から表示します。<br />
							&nbsp;&nbsp;
										「指定期間」を選択した場合は、指定された期間を表示します。<br />
							<br />
							<div id="DataSpan" class="Term">
							  <table id="TABLE5">
								<tr>
								  <td class="a">
									  ■表示方法</td>
									<td class="b">
										<asp:RadioButton ID="RdBFromNewest" runat="server" BorderStyle="None" 
											EnableTheming="False" GroupName="StartDateSet" Text="最新から" Width="84px" />
										&nbsp;<asp:DropDownList ID="DDLRange" runat="server" EnableTheming="False" 
											Font-Size="10pt" Width="107px">
										</asp:DropDownList>
									</td>
								</tr>
								<tr>
								  <td class="a"></td>
								  <td class="b">
										<asp:RadioButton ID="RdBDsignatedDate" runat="server" BorderStyle="None" 
											EnableTheming="False" GroupName="StartDateSet" Text="指定期間" Width="88px" />
										<asp:TextBox ID="TxtStartDate" runat="server" CausesValidation="True" 
											CssClass="TEXTBOXES" EnableTheming="False" MaxLength="10" 
											ToolTip="「指定日」の時に、データ表示開始日時を入力してください" ValidationGroup="EndDay" Width="100px"></asp:TextBox>
										<asp:Image ID="imgCalTxtStartDate" runat="server" EnableTheming="False" 
											ImageAlign="AbsMiddle" ImageUrl="~/img/Calendar.gif" 
											ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" />
										<asp:Label ID="Lbl2" runat="server" BorderStyle="None" CssClass="LABELS" 
											EnableTheming="False" EnableViewState="False" Font-Size="Small" Text="～" 
											Width="22px"></asp:Label>
										<asp:TextBox ID="TxtEndDate" runat="server" CausesValidation="True" 
											CssClass="TEXTBOXES" EnableTheming="False" MaxLength="10" 
											ToolTip="「指定日」の時に、データ表示終了日時を入力してください" ValidationGroup="EndDay" Width="100px"></asp:TextBox>
										<asp:Image ID="imgCalTxtEndDate" runat="server" EnableTheming="False" 
											ImageAlign="AbsMiddle" ImageUrl="~/img/Calendar.gif" 
											ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" />
									</td>
								</tr>
							  </table>
							</div>
							<br />
							<div id="thin0" class="Term2">
								<table id="TABLE6" style="margin-bottom: 10px">
									<tr>
										<td colspan="3" style="text-align: left">
											■データ間引き（チェックが１つもない場合は全データを表示します） 
										</td>
									</tr>
									<tr>
										<td class="a" rowspan="2"></td>
										<td class="b">
											<asp:CheckBox ID="ChbPartial" runat="server" BorderStyle="None" 
												CssClass="LABELS_Left" EnableTheming="False" Text="間引きを行なう" Width="117px" />
										</td>
										<td class="c" style="text-align: right">
											<div class="clear">
												<button class="sexybutton sexysimple sexysmall" 
													onclick="Select(true);return false;" style="text-align: right;">
												<span class="ok">全てチェック</span></button>
												<button class="sexybutton sexysimple sexysmall" 
													onclick="Select(false);return false;" style="text-align: right; ">
												全てチェックを外す</button>
											</div>
										</td>
									</tr>
									<tr>
										<td class="d" colspan="2">
											<asp:CheckBoxList ID="CBLPartial" runat="server" BorderColor="Tan" 
												BorderStyle="Ridge" BorderWidth="2px" EnableTheming="True" RepeatColumns="6" 
												RepeatDirection="Horizontal" Width="400px">
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
							<asp:RangeValidator ID="RngValidSt" runat="server" ControlToValidate="TxtStartDate"
									Display="None" EnableTheming="False" ErrorMessage="開始日はデータの収録期間外もしくは日付として正しくありません。修正してください。" MaximumValue="2099/12/31"
									MinimumValue="1990/01/01" SetFocusOnError="True" Type="Date"> </asp:RangeValidator>
							<asp:ValidatorCalloutExtender ID="RngValidSt_ValidatorCalloutExtender" 
								runat="server" Enabled="True" HighlightCssClass="validatorCalloutHighlight" 
								PopupPosition="BottomRight" TargetControlID="RngValidSt" Width="250px">
							</asp:ValidatorCalloutExtender>
							<asp:RangeValidator ID="RngValidEd" runat="server" 
								ControlToValidate="TxtEndDate" Display="None" 
								ErrorMessage="終了日はデータの収録期間外もしくは日付として正しくありません。修正してください。" MaximumValue="2050/12/31" 
								MinimumValue="1990/01/01" SetFocusOnError="True" Type="Date"> </asp:RangeValidator>
							<asp:ValidatorCalloutExtender ID="RngValidEd_ValidatorCalloutExtender" 
								runat="server" BehaviorID="ValidCoExRngEd" Enabled="True" 
								HighlightCssClass="validatorCalloutHighlight" TargetControlID="RngValidEd" 
								Width="250px" PopupPosition="BottomLeft">
							</asp:ValidatorCalloutExtender>
							<asp:RegularExpressionValidator ID="RExValidSt" runat="server" 
								ControlToValidate="TxtStartDate" Display="None" EnableTheming="True" 
								ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" 
								ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))" ValidationGroup="EndDay"> </asp:RegularExpressionValidator>
							<asp:ValidatorCalloutExtender ID="RExValidSt_ValidatorCalloutExtender" 
								runat="server" Enabled="True" HighlightCssClass="validatorCalloutHighlight" 
								PopupPosition="BottomRight" TargetControlID="RExValidSt" Width="250px">
							</asp:ValidatorCalloutExtender>
							<asp:RegularExpressionValidator ID="RExValidEd" runat="server" 
								ControlToValidate="TxtEndDate" Display="None" EnableTheming="True" 
								ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" 
								ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))" ValidationGroup="EndDay"> </asp:RegularExpressionValidator>
							<asp:ValidatorCalloutExtender ID="RExValidEd_ValidatorCalloutExtender" 
								runat="server" CssClass="validatorCalloutHighlight" Enabled="True" 
								PopupPosition="BottomLeft" TargetControlID="RExValidEd" Width="250px">
							</asp:ValidatorCalloutExtender>
							<asp:MaskedEditExtender ID="MaskEdExSt" runat="server" AutoComplete="False" 
								EnableViewState="False" Mask="9999/99/99" MaskType="Date" 
								TargetControlID="TxtStartDate" UserDateFormat="YearMonthDay">
							</asp:MaskedEditExtender>
							<asp:MaskedEditExtender ID="MaskEdExEd" runat="server" AutoComplete="False" 
								EnableViewState="False" Mask="9999/99/99" MaskType="Date" 
								TargetControlID="TxtEndDate" UserDateFormat="YearMonthDay">
							</asp:MaskedEditExtender>
						</td>
					</tr>
				</table>
			</asp:Panel>
		</asp:Panel>
		</div>
	<asp:collapsiblepanelextender ID="CollapsiblePanelExtender1" runat="server"
			CollapseControlID="Ph1" Collapsed="True" CollapsedText="設定表示" ExpandControlID="Ph1"
			ExpandedText="設定非表示" TargetControlID="PnlGraphSet" TextLabelID="LblColl1" 
	Enabled="True" EnableViewState="False" CollapsedImage="~/img/el.png" 
	ExpandedImage="~/img/cl.png" ImageControlID="ImgCollExp" 
	SuppressPostBack="True">
	</asp:collapsiblepanelextender>
		<asp:Panel ID="PnlTable" runat="server" BorderStyle="None">
			<asp:Label ID="LblTitleUpper" runat="server" BorderStyle="None" CssClass="LABELS" EnableTheming="False" EnableViewState="False" Width="650px"></asp:Label>
			<asp:Label ID="LblDateSpan" runat="server" BackColor="Transparent" BorderColor="Transparent" BorderStyle="None" CssClass="LABELS_Right" EnableTheming="False" EnableViewState="False" Width="650px" Font-Size="9pt"></asp:Label>
			<asp:GridView ID="GrdTimeSeries" runat="server" BackColor="White" BorderColor="Black"
				BorderStyle="Solid" BorderWidth="1px" CellPadding="2"
				ForeColor="Black" Height="912px" Width="640px" AllowPaging="True" EmptyDataText="---" Font-Names="MS UI Gothic" Font-Size="9pt" PageSize="30" EnableViewState="False" ShowFooter="True" UseAccessibleHeader="False" DataMember="DefaultView" EnableSortingAndPagingCallbacks="True" EnableTheming="False" AllowSorting="True">
				<FooterStyle BackColor="#CCCC99" HorizontalAlign="Center" VerticalAlign="Middle" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px" />
				<RowStyle BackColor="#F7F7DE" Height="12px" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px" />
				<PagerStyle BackColor="#F7F7DE" ForeColor="SaddleBrown" HorizontalAlign="Center" VerticalAlign="Middle" Font-Names="MS UI Gothic" Font-Overline="False" Font-Bold="False" Font-Size="12pt" Font-Underline="False" BorderWidth="1px" />
				<SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" BorderWidth="1px" />
				<HeaderStyle BackColor="Wheat" Font-Bold="False" ForeColor="Black" HorizontalAlign="Center" VerticalAlign="Middle" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" />
				<AlternatingRowStyle BackColor="White" BorderWidth="1px" />
					<PagerSettings Mode="NumericFirstLast" PageButtonCount="15" />
					<EmptyDataRowStyle BorderWidth="1px" />
					<EditRowStyle BorderWidth="1px" />
			</asp:GridView>
			<asp:Label ID="LblTitleLower" runat="server" BorderStyle="None" Width="650px" BackColor="Transparent" BorderColor="Transparent" CssClass="LABELS" EnableTheming="False" EnableViewState="False" Visible="False" /></asp:Panel>
			<asp:AccessDataSource ID="AccessDataSource1" runat="server"></asp:AccessDataSource>
		<asp:Timer ID="Tmr_Update" runat="server" Enabled="False">
		</asp:Timer>
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
