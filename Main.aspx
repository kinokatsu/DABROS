<%@ Page Language="VB" MasterPageFile="~/MasterPage.master" AutoEventWireup="false" CodeFile="Main.aspx.vb" Inherits="Main" Culture="Auto" Debug="true" validateRequest="false" %>
<asp:Content ID="CntHead" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="CntPlHolder" ContentPlaceHolderID="CntPlHolder" Runat="Server">
	<script src="js/jquery.tooltip.js" type="text/javascript" ></script>
	<script type="text/javascript">
	$( function(){
		// default settings
		$.Tooltip.defaults = $.extend( $.Tooltip.defaults, {
					delay      : 1,
			showURL    : false,
			showBody   : " - ",
			left       : -100,
			track      : true
		});
		// initialize
		//$( '#list1 a' ).Tooltip();

		// with different css
		//$( '#list2 span' ).Tooltip({
		//    extraClass: 'darktip'
		// with different css
//        $( ' span' ).Tooltip({
//            //extraClass: 'darktip'
//        });       // 2012/02/22 Changed
		// with different css   
		$( ".DataLBLRight,.DataLBLRightB,.DataLBLCenter,.DataLBLLeft,.NoClass,.periodTable" ).Tooltip({
			//extraClass: 'darktip'
		});         // 2012/02/22 Add
	});
	</script>
	<div id="MainContent">
		<asp:Label ID="LblAlert" runat="server" BorderStyle="Ridge" BorderWidth="2px" 
			Font-Bold="False" Font-Names="MS UI Gothic" Font-Size="11pt" ForeColor="LemonChiffon" BackColor="Red" BorderColor="MediumBlue" Visible="False" CssClass="NowrapLabel"></asp:Label>
		<table contenteditable="true" id="TableAlarm" class="TableAlarm">
			<tr>
				<td colspan="1" rowspan="1" tabindex="0">
					<asp:Table ID="Table_AlarmStatus" runat="server" EnableTheming="True" EnableViewState="False" BorderColor="Silver" BorderStyle="Ridge" BorderWidth="3px" CellPadding="1" CellSpacing="1" Font-Names="MS UI Gothic" Font-Size="11pt" ToolTip="警報の発生状況を表示します。" BackColor="White" CssClass="statusTable">
						<asp:TableRow ID="Contents" runat="server" BorderWidth="1px" EnableTheming="False"
							EnableViewState="False" HorizontalAlign="Center" TableSection="TableHeader" VerticalAlign="Middle">
							<asp:TableCell ID="Col_00" runat="server" BorderWidth="1px" ColumnSpan="1" RowSpan="1"
								Width="140px" Wrap="False" BorderColor="Silver">名　称</asp:TableCell>
							<asp:TableCell ID="Col_01" runat="server" BorderWidth="1px" ColumnSpan="1" RowSpan="1"
								Width="210px" Wrap="False" BorderColor="Silver">測定日時</asp:TableCell>
							<asp:TableCell ID="Col_02" runat="server" BorderWidth="1px" ColumnSpan="1" RowSpan="1"
								Width="140px" Wrap="False" BorderColor="Silver">最新データ警報状況</asp:TableCell>
						</asp:TableRow>
					</asp:Table>
				</td>
				<td colspan="1" rowspan="1" style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px;
					margin: 0px; padding-top: 0px; width: 37px;" tabindex="0">
					<asp:Image ID="ImgAlert" runat="server" ImageUrl="~/img/Alert.gif" Visible="False" EnableViewState="False" AlternateText="警報発生中" ToolTip="警報が発生しています" CssClass="NoClass" /></td>
			</tr>
		</table>
		<asp:Image ID="Image2" runat="server" EnableViewState="False" CssClass="topImage" />
		<asp:Panel ID="PnlCoordinate" runat="server" EnableTheming="False" EnableViewState="False" Visible="False" >
			<div id="result1" hidefocus="hideFocus"></div>
			<div id="result2" hidefocus="hideFocus"></div>
			<div id="mpxy" hidefocus="hideFocus"> </div>
		</asp:Panel>
	  <asp:Table ID="tblLastUpdate" runat="server" BackColor="LightCyan" BorderStyle="Ridge"
		BorderWidth="3px" ToolTip="サーバに保持しているデータの保存期間を表示します。" Font-Size="10pt" CellPadding="0" CellSpacing="0" EnableTheming="False" EnableViewState="False" CssClass="periodTable">
	  </asp:Table>
		<asp:Label ID="LblLastUPDate" runat="server" EnableTheming="True" BorderStyle="Ridge" Font-Bold="False" Font-Names="ＭＳ ゴシック" Font-Size="11pt" ForeColor="Black" BorderWidth="2px" BackColor="LightCyan" ToolTip="サーバに保持しているデータの保存期間を表示します。" Visible="False" CssClass="NoClass"></asp:Label><asp:Timer ID="TmrCheckNewData" runat="server" Enabled="False"></asp:Timer>
	</div>
</asp:Content>

