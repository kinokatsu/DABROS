<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Table_Summary.aspx.vb" Inherits="Table_Summary" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="C1.Web.UI.Controls.3" Namespace="C1.Web.UI.Controls.C1Calendar" TagPrefix="cc1" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
	<title id="MyTitle" runat="server">集計表</title>
	<meta charset="utf-8" />    
	<meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
	<meta http-equiv="X-UA-Compatible" content="IE=9" />
	<link href="CSS/stl_graph_table.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_calendar.css" rel="stylesheet" type="text/css" />
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
	<script src="js/calendar.min.js" type="text/javascript"></script>
	<script src="js/summary.min.js" type="text/javascript"></script>
	<script src="js/updCheckSub.min.js" type="text/javascript"></script>
</head>
<body>
	<form id="TableSummaryFrm" runat="server">
	<div id="TableSummary" lang="ja" title="集計表">
		<asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="180" EnablePageMethods="True" EnableScriptGlobalization="True"></asp:ToolkitScriptManager>
		<input id="nwdt" runat="server" type="hidden"/>
		<input id="nwdtno" runat="server" type="hidden"/>
		<asp:Button style="display: none; visibility: hidden" id="btnSubmit" runat="server" EnableViewState="False"></asp:Button>
		<asp:Panel ID="PnlNoPrint" runat="server" CssClass="dspOnly" EnableTheming="True" Width="656px" Wrap="False" BorderStyle="None" HorizontalAlign="Center">
			<asp:Panel ID="PnlButtons" runat="server" BackColor="Cornsilk" BorderColor="Olive" BorderStyle="Ridge" BorderWidth="2px" EnableTheming="False" EnableViewState="False" Height="37px" HorizontalAlign="Left" Width="650px" Wrap="False">
				<div class="rg"><img id="imgloader" src="./img/comm.gif" alt="" class="asyncloader" /></div>
				<asp:ImageButton ID="ImgBtnPrint" runat="server" AlternateText="Print" CausesValidation="False" EnableViewState="False" ImageUrl="~/img/print.png" OnClientClick="window.print();return false;" BorderStyle="None" CssClass="BTNHEADER_PRINT" ToolTip="「印刷」画面を表示します。" />
				<asp:ImageButton ID="ImbtnRedrawGraph" runat="server" AlternateText="Redraw" EnableViewState="False" ImageUrl="~/img/redraw.png" ToolTip="設定した内容でグラフを再描画します。" BorderStyle="None" CssClass="BTNHEADER_REDRAW" />
				<asp:ImageButton ID="ImgBtnClose" runat="server" AlternateText="Close" CausesValidation="False" EnableViewState="False" ImageUrl="~/img/close.png" OnClientClick="window.close();return false;" ToolTip="印刷プレビュー後などで、このボタンでウィンドウを閉じない場合は、右上の×ボタンで閉じてください。" BorderStyle="None" CssClass="BTNHEADER_CLOSE" />
			</asp:Panel>
			<asp:Panel ID="Ph1" runat="server" CssClass="accordionHeader" EnableViewState="False">
				●集計表設定
				<asp:ImageButton ID="ImgCollExp" runat="server" EnableTheming="False" EnableViewState="False" Height="13px" ImageAlign="AbsMiddle" Width="13px" BorderStyle="None" />
				<asp:Label ID="LblColl1" runat="server" EnableTheming="False" Font-Size="Small" ForeColor="Gold" BorderStyle="None"></asp:Label>
			</asp:Panel>
			<asp:Panel ID="PnlGraphSet" runat="server" BackColor="Transparent" CssClass="collapsePanel" EnableTheming="False" HorizontalAlign="Center" Width="654px" Wrap="False" BorderStyle="None">
				<asp:Label ID="LblLastUpdate" runat="server" EnableViewState="False"></asp:Label>
				<table id="Table2" class="tableOutline" cellpadding="1" cellspacing="0" unselectable="on">
					<tr>
						<td class="TABLEHEADER tableShowSpan" colspan="2">表示設定</td>
					</tr>
					<tr>
						<td class="tableContent" colspan="2">
							<ul class="ulset">
								<li class="liset">集計の方法を指定してください。表示初期は日報を表示しています。<br />１日の集計を行なう場合は「日報」を、１ヶ月の集計を行なう場合は「月報」を選択してください。<br />開始時刻を変更した場合は、指定した時刻を元に１日／１月さかのぼって集計を行ないます。<br />変動値は、「終了値-開始値」のデータになります。 <br />平均値は、表示期間の測定データにおける平均値になります。
								</li>
							</ul>
						</td>
					</tr>
					<tr>
						<td colspan="1" class="tableSummaryColHead">■集計の種類</td>
						<td colspan="1" class="tableSummaryCol1">
							<asp:RadioButtonList ID="RdBtnList" runat="server" RepeatDirection="Horizontal" EnableTheming="True" RepeatLayout="Flow" CssClass="divFloat">
								<asp:ListItem Selected="True" Value="0">日報</asp:ListItem>
								<asp:ListItem Value="1">月報</asp:ListItem>
							</asp:RadioButtonList>
						</td>
					</tr>
					<tr>
						<td colspan="1" class="tableSummaryColHead">■開始日付</td>
						<td colspan="1" class="tableSummaryCol1">
							<asp:TextBox ID="TxtStartDate" runat="server" CssClass="TEXTBOXES" EnableTheming="False" MaxLength="10" ToolTip="「指定日」の時に、データ表示開始日時を入力してください" Width="100px" ValidationGroup="DateCheck" BorderStyle="Inset" CausesValidation="True"></asp:TextBox>
							<asp:Image ID="imgCalTxtStartDate" runat="server" EnableTheming="False" ImageUrl="~/img/Calendar.gif" ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" ImageAlign="AbsMiddle" />
						</td>
					</tr>
					<tr>
						<td colspan="1" class="tableSummaryColHead">■開始時刻</td>
						<td colspan="1" class="tableSummaryCol1">
							<asp:DropDownList ID="DDLhour" runat="server" Width="107px" EnableTheming="True" Font-Size="10pt">
							</asp:DropDownList>
						</td>
					</tr>
				</table>
				<asp:RangeValidator ID="RngValidSt" runat="server" ControlToValidate="TxtStartDate" Display="None" EnableTheming="True" ErrorMessage="開始日は1990年以降としてください。" MaximumValue="2050/12/31" MinimumValue="1990/01/01" SetFocusOnError="True" Type="Date"></asp:RangeValidator>
				<asp:RegularExpressionValidator ID="RExValidSt" runat="server" ControlToValidate="TxtStartDate" Display="None" EnableTheming="True" ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))" ValidationGroup="EndDay"></asp:RegularExpressionValidator>
				<asp:MaskedEditExtender ID="MaskedEditExtenderStartDate" runat="server" AutoComplete="False" EnableViewState="False" Mask="9999/99/99" MaskType="Date" TargetControlID="TxtStartDate" UserDateFormat="YearMonthDay"></asp:MaskedEditExtender>
				<asp:ValidatorCalloutExtender ID="ValidCoExSt" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="RExValidSt" Width="250px" PopupPosition="BottomRight"></asp:ValidatorCalloutExtender>
				<asp:ValidatorCalloutExtender ID="ValidCoExRngSt" runat="server" HighlightCssClass="validatorCalloutHighlight" TargetControlID="RngValidSt" Width="250px" PopupPosition="BottomRight"></asp:ValidatorCalloutExtender>
			</asp:Panel>
			<asp:CollapsiblePanelExtender ID="CollapsiblePanelExtender1" runat="server"
				CollapseControlID="Ph1" Collapsed="true" CollapsedImage="~/img/el.png" CollapsedText="設定表示"
				Enabled="True" EnableViewState="False" ExpandControlID="Ph1" ExpandedImage="~/img/cl.png"
				ExpandedText="設定非表示" ImageControlID="ImgCollExp" SuppressPostBack="True" TargetControlID="PnlGraphSet"
				TextLabelID="LblColl1">
			</asp:CollapsiblePanelExtender>
		</asp:Panel>
		<asp:Panel ID="PnlTable" runat="server" BorderStyle="None">
			<div runat="server" id="TableOuter">
				<asp:Label ID="LblTitleUpper" runat="server" CssClass="titleUpper" EnableTheming="True"></asp:Label>
				<div><asp:Label ID="LblDateSpan" runat="server" CssClass="LABELS_Right" EnableTheming="False" EnableViewState="False" Width="650px" Font-Size="9pt"></asp:Label></div>
				<asp:GridView
					ID="GrdSummary" runat="server" AllowPaging="True" BackColor="White"
					BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" CellPadding="3" DataMember="DefaultView" EmptyDataText="---" EnableSortingAndPagingCallbacks="True"
					EnableTheming="False" Font-Names="MS UI Gothic" Font-Size="9pt"
					ForeColor="Black" Height="907px" PageSize="30" UseAccessibleHeader="False"
					Width="652px" EnableViewState="False">
					<FooterStyle BackColor="#CCCC99" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px"
						HorizontalAlign="Center" VerticalAlign="Middle" />
					<RowStyle BackColor="#F7F7DE" BorderColor="Gray" BorderStyle="Solid" Height="12px" BorderWidth="1px" />
					<PagerStyle BackColor="#F7F7DE" Font-Bold="False" Font-Names="MS UI Gothic" Font-Overline="False"
						Font-Size="12pt" Font-Underline="False" ForeColor="SaddleBrown" HorizontalAlign="Center"
						VerticalAlign="Middle" BorderWidth="1px" />
					<SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" BorderWidth="1px" />
					<HeaderStyle BackColor="Wheat" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"
						Font-Bold="False" ForeColor="Black" HorizontalAlign="Center" VerticalAlign="Middle" />
					<AlternatingRowStyle BackColor="White" BorderWidth="1px" />
					<PagerSettings Mode="NumericFirstLast" />
					<EmptyDataRowStyle HorizontalAlign="Center" VerticalAlign="Middle" />
					<EditRowStyle BorderWidth="1px" />
				</asp:GridView>
				<%--<asp:Label ID="lblCalcExp" runat="server" CssClass="LABELS_Right" Font-Names="MS UI Gothic" Font-Size="9pt" Width="649px"></asp:Label>--%>
				<asp:Label ID="LblTitleLower" runat="server" CssClass="titleLower"/>
			</div>
		</asp:Panel>
	</div>
	<cc1:c1calendar ID="C1WebCalendar" runat="server" height="230px" width="248px" 
		CalendarTitle="－ {0:yyyy'年'M'月'} －" CultureInfo="ja-JP" 
		Easing="EaseOutQuart" 
		NavigationEffect="Auto" PopupMode="True" 
		UseEmbeddedVisualStyles="False" VisualStyle="OriginalCal" 
		VisualStylePath="~/VisualStyles" ClientOnAfterClose="CalendarClosed" 
		ToolTipDateFormat="yyyy/MM/dd" NextToolTip="1ヶ月先へ" PrevToolTip="1ヶ月前へ" 
	QuickNextToolTip="3ヶ月先へ" QuickPrevToolTip="3ヶ月前へ">
	</cc1:c1calendar>
	</form>
</body>
</html>
