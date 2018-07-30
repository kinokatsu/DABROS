<%@ Page Language="VB" AutoEventWireup="false" CodeFile="TEXT_CSVDownload.aspx.vb" Inherits="TEXT_CSVDownload" %>
<%@ Register Assembly="C1.Web.UI.Controls.3" Namespace="C1.Web.UI.Controls.C1Calendar" TagPrefix="cc1" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
	<title>テキストファイルダウンロード</title>
	<meta charset="utf-8" />
	<meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
	<meta http-equiv="X-UA-Compatible" content="IE=9" />
	<link href="CSS/stl_sexybuttons.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_calendar.css" rel="stylesheet" type="text/css" />
	<link href="CSS/stl_graph_table.css" rel="stylesheet" type="text/css" />
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
	<script src="js/grp.min.js" type="text/javascript"></script>
	<script src="js/calendar.min.js" type="text/javascript"></script>
</head>
<body>
	<form id="form1" runat="server">
		<asp:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server" EnableScriptGlobalization="True" EnableScriptLocalization="True" EnablePageMethods="True" AsyncPostBackTimeout="180"></asp:toolkitscriptmanager>
		<asp:Panel ID="PanelHead" runat="server" CssClass="noaccordionHeader" EnableTheming="True" EnableViewState="False" Width="648px">
			●テキストファイルダウンロード
		</asp:Panel>
		<asp:Panel ID="PnlGraphSet" runat="server" BackColor="White" BorderStyle="None" EnableTheming="True" Font-Size="10pt" HorizontalAlign="Center" Width="654px" Wrap="False" CssClass="text-align:Center;">
			<asp:Label ID="LblLastUpdate" runat="server"  EnableTheming="True" Width="625px"></asp:Label>
			<table id="TABLE1" style="margin: 5px auto 5px auto; border: 0px solid #996600; padding: 1px; width: 630px; border-spacing: 0px; font-size: 10pt; border-collapse: collapse; position: relative; text-align: center; top: 0px; left: 0px; height: 612px;">
				<tr>
					<td class="TABLEHEADER">
						出力設定
					</td>
				</tr>
				<tr>
					<td class="tableContent" colspan="1">
						<ul class="ulset">
							<li>
								表示方法を指定してください。<br /> 最新から」を選択した場合は、指定された期間遡った日付の0時から表示します。<br />「指定期間」を選択した場合は、指定された期間を表示します。 
							</li>
						</ul>
						<div id="DataSpan" class="Term">
							<table id="TABLE5" style="font-size: 10pt; margin-bottom: 10px; margin-top: 10px;">
								<tr>
									<td class="a">
										■表示方法</td>
									<td class="b">
										<asp:RadioButton ID="RdBFromNewest" runat="server" BorderStyle="None" EnableTheming="False" GroupName="StartDateSet" Text="最新から" Width="84px" />
									</td>
									<td class="c">
										<asp:DropDownList ID="DDLRange" runat="server" EnableTheming="False" Font-Size="10pt" Width="105px"></asp:DropDownList>
									</td>
								</tr>
								<tr>
									<td class="a">
									</td>
									<td class="b">
										<asp:RadioButton ID="RdBDsignatedDate" runat="server" BorderStyle="None" EnableTheming="False" GroupName="StartDateSet" Text="指定期間" Width="88px" />
									</td>
									<td class="c">
										<asp:TextBox ID="TxtStartDate" runat="server" CausesValidation="True" CssClass="TEXTBOXES" EnableTheming="False" MaxLength="10" ToolTip="「指定日」の時に、データ表示開始日時を入力してください" ValidationGroup="EndDay" Width="100px"></asp:TextBox>
										<asp:Image ID="imgCalTxtStartDate" runat="server" EnableTheming="False" ImageAlign="AbsMiddle" ImageUrl="~/img/Calendar.gif" ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" />
										<asp:Label ID="Lbl2" runat="server" BorderStyle="None" CssClass="LABELS" EnableTheming="False" EnableViewState="False" Font-Size="Small" Text="～" Width="22px"></asp:Label>
										<asp:TextBox ID="TxtEndDate" runat="server" CausesValidation="True" CssClass="TEXTBOXES" EnableTheming="False" MaxLength="10" ToolTip="「指定日」の時に、データ表示終了日時を入力してください" ValidationGroup="EndDay" Width="100px"></asp:TextBox>
										<asp:Image ID="imgCalTxtEndDate" runat="server" EnableTheming="False" ImageAlign="AbsMiddle" ImageUrl="~/img/Calendar.gif" ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" />
										<%--									    <wijmo:C1InputDate ID="C1TxtStartInputDate" runat="server" 
											Date="10/19/2012 14:24:00" ShowTrigger="True" StartYear="2000" Width="120px">
										</wijmo:C1InputDate>--%>
									</td>
								</tr>
							</table>
						</div>
						<div id="thin0" class="Term2">
							<table id="TABLE6" style="font-size: 10pt; margin-bottom: 10px;">
								<tr>
									<td colspan="3" style="text-align: left">
										■データ間引き（チェックが１つもない場合は全データを表示します） 
									</td>
								</tr>
								<tr>
									<td class="a" rowspan="2">
									</td>
									<td class="b">
										<asp:CheckBox ID="ChbPartial" runat="server" BorderStyle="None" CssClass="LABELS_Left" EnableTheming="False" Text="間引きを行なう" Width="117px" />
									</td>
									<td class="c" style="text-align: right">
										<div class="clear">
											<button class="sexybutton sexysimple sexysmall" onclick="Select(true);return false;" style="text-align: right;">
											<span class="ok">全てチェック</span>
											</button>
											<button class="sexybutton sexysimple sexysmall" onclick="Select(false);return false;" style="text-align: right; ">
											全てチェックを外す</button>
										</div>
									</td>
								</tr>
								<tr>
									<td class="d" colspan="2">
										<asp:CheckBoxList ID="CBLPartial" runat="server" BorderColor="Tan" 
											BorderStyle="Ridge" BorderWidth="2px" EnableTheming="True" Font-Size="10pt" 
											RepeatColumns="6" RepeatDirection="Horizontal" Width="400px">
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
						<asp:Label ID="Label5" runat="server" EnableTheming="False" EnableViewState="False" Height="25px" Text="■ダウンロード項目" Width="618px"></asp:Label>
						<br />
						 テキストファイル（CSV形式）のダウンロードを行なう項目をチェックしてください。
						<asp:Panel ID="PnlDLContents" runat="server" BorderStyle="None" Width="622px">
							<table style="width: 622px; border-collapse: collapse;">
								<tr>
									<td rowspan="2" style="width: 100px">
									</td>
									<td>
										<table id="TABLE2" 
											style="border-style: none; border-width: 2px; border-color: #996600; width: 520px; font-size: 10pt; border-spacing: 0px; border-collapse: collapse;">
											<tr>
												<td class="tableContent" style="vertical-align: middle; background-color: #ffcc66; text-align: left; width: 400px;">
													項 目
												</td>
												<td class="tableContent" style="vertical-align: middle; background-color: #ffcc66; text-align: left; width: 110px;">
													ダウンロード方法
												</td>
											</tr>
											<tr>
												<td class="tableContent" style="vertical-align: top; text-align: left; width: 400px;">
													<asp:CheckBoxList ID="CBLDownloadList" runat="server" CellPadding="1" CellSpacing="2" Font-Size="10pt" Width="400px"></asp:CheckBoxList>
												</td>
												<td class="tableContent" colspan="1" style="vertical-align: top; text-align: left; width: 110px;">
													<asp:RadioButtonList ID="RBLFileFormat" runat="server" Font-Size="10pt" Width="100px">
														<asp:ListItem Selected="True" Value="#TRUE#">圧縮する</asp:ListItem>
														<asp:ListItem Value="#FALSE#">圧縮しない</asp:ListItem>
													</asp:RadioButtonList>
												</td>
											</tr>
										</table>
									</td>
								</tr>
								<tr>
									<td>
										<div>
											<ul class="LABELS_Left ulset2">
												<li class="liset">「圧縮する」の場合、ZIP形式の圧縮ファイルとなります。</li>
												<li class="liset">「圧縮しない」の場合、選択項目の全てが１つのテキストファイルに保存され<br />るため、期間によっては膨大な行数になり、Excel等で開けない場合があります。</li>
											</ul>
										</div>
									</td>
								</tr>
							</table>
							<asp:RegularExpressionValidator ID="RExValidEd" runat="server" 
								ControlToValidate="TxtEndDate" Display="None" EnableTheming="True" 
								ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" 
								ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))" 
								ValidationGroup="EndDay"></asp:RegularExpressionValidator>
							<asp:ValidatorCalloutExtender ID="RExValidEd_ValidatorCalloutExtender" 
								runat="server" HighlightCssClass="validatorCalloutHighlight" 
								TargetControlID="RExValidEd" Width="250px" PopupPosition="BottomLeft">
							</asp:ValidatorCalloutExtender>
							<asp:RegularExpressionValidator ID="RExValidSt" runat="server" 
								ControlToValidate="TxtStartDate" Display="None" EnableTheming="True" 
								ErrorMessage="指定日時は正しくありません。再入力してください。" SetFocusOnError="True" 
								ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))" 
								ValidationGroup="EndDay"></asp:RegularExpressionValidator>
							<asp:ValidatorCalloutExtender ID="RExValidSt_ValidatorCalloutExtender" 
								runat="server" HighlightCssClass="validatorCalloutHighlight" 
								TargetControlID="RExValidSt" Width="250px" PopupPosition="BottomRight">
							</asp:ValidatorCalloutExtender>
							<asp:RangeValidator ID="RngValidSt" runat="server" 
								ControlToValidate="TxtStartDate" Display="None" EnableTheming="True" 
								ErrorMessage="開始日はデータの収録期間外もしくは日付として正しくありません。修正してください。" 
								MaximumValue="2999/12/31" MinimumValue="1900/01/01" SetFocusOnError="True" 
								Type="Date"></asp:RangeValidator>
							<asp:ValidatorCalloutExtender ID="RngValidSt_ValidatorCalloutExtender" 
								runat="server" Enabled="True" HighlightCssClass="validatorCalloutHighlight" 
								TargetControlID="RngValidSt" Width="250px" PopupPosition="BottomRight">
							</asp:ValidatorCalloutExtender>
							<asp:RangeValidator ID="RngValidEd" runat="server" 
								ControlToValidate="TxtEndDate" Display="None" EnableTheming="True" 
								ErrorMessage="終了日はデータの収録期間外もしくは日付として正しくありません。修正してください。" 
								MaximumValue="2999/12/31" MinimumValue="1900/01/01" SetFocusOnError="True" 
								Type="Date"></asp:RangeValidator>
							<asp:ValidatorCalloutExtender ID="RngValidEd_ValidatorCalloutExtender" 
								runat="server" 
								TargetControlID="RngValidEd" Width="250px" HighlightCssClass="validatorCalloutHighlight" 
								PopupPosition="BottomLeft">
							</asp:ValidatorCalloutExtender>
							<asp:MaskedEditExtender ID="MaskedEditExtenderEndDate" runat="server" 
								AutoComplete="False" Mask="9999/99/99" MaskType="Date" 
								TargetControlID="TxtEndDate" UserDateFormat="YearMonthDay">
							</asp:MaskedEditExtender>
							<asp:MaskedEditExtender ID="MaskedEditExtenderStartDate" runat="server" 
								AutoComplete="False" Mask="9999/99/99" MaskType="Date" 
								TargetControlID="TxtStartDate" UserDateFormat="YearMonthDay">
							</asp:MaskedEditExtender>
						</asp:Panel>
						<asp:Panel ID="PnlDownload" runat="server" EnableViewState="False" 
							Height="60px" HorizontalAlign="Center" Width="622px">
							<asp:ImageButton ID="ImBDownload" runat="server" AlternateText="ダウンロード" 
								BorderStyle="None" CssClass="BTNHEADER_DL" EnableTheming="False" 
								EnableViewState="False" ImageUrl="~/img/download.png" />
							<asp:AnimationExtender ID="ImBDownload_AnimationExtender" runat="server" 
								Enabled="True" TargetControlID="ImBDownload">
								<Animations>
								<OnClick>
									<Sequence>
									<EnableAction Enabled="False" />
									<FadeOut AnimationTarget="PnlDownload" Duration="4" Fps="20" maximumOpacity="1.0" minimumOpacity="0.2" />
									<EnableAction Enabled="True" />
									<FadeIn AnimationTarget="PnlDownload" Duration="4" Fps="20" maximumOpacity="1.0" minimumOpacity="0.2" />
									</Sequence>
								</OnClick>
								</Animations>
							</asp:AnimationExtender>
							<asp:ImageButton ID="ImBClose" runat="server" AlternateText="閉じる" 
								BorderStyle="None" CssClass="BTNHEADER_REDRAW" EnableTheming="False" 
								EnableViewState="False" ImageUrl="~/img/dl_close.png" 
								OnClientClick="window.close();return false;" />
						</asp:Panel>
					</td>
				</tr>
			</table>
		</asp:Panel>                   
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
