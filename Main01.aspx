<%@ Page Title="" Language="VB" MasterPageFile="~/MasterPage01.master" AutoEventWireup="false" CodeFile="Main01.aspx.vb" Inherits="Main01" Trace="false" TraceMode="SortByTime"%>
<asp:Content ID="CntHead" ContentPlaceHolderID="head" Runat="Server"></asp:Content>
<asp:Content ID="CntPlHolder" ContentPlaceHolderID="CntPlHolder" Runat="Server">
	<asp:Literal id="dcss" runat="server" />
	<script type="text/javascript">
		this.window.name = "dabrostop";
	</script>
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
	<script src="js/main.min.js" type="text/javascript"></script>
	<script src="js/jquery.dimensions.min.js" type="text/javascript" ></script>
	<script src="js/tippy.all.min.js" type="text/javascript"></script>
	<script src="js/updCheck.min.js" type="text/javascript"></script>
	<script src="js/ToolTip.min.js" type="text/javascript"></script>
	<link href="css/stl_ToolTip.css" rel="stylesheet" type="text/css" />
	<div id="MainContent" runat="server">
		<table id="TableAlarm" contenteditable="false" class="TableAlarm" >
			<tbody>
				<tr>
					<td colspan="1" rowspan="1" tabindex="0">
						<asp:Table ID="Table_AlarmStatus" runat="server" EnableTheming="False" EnableViewState="False" CssClass="statusTable">
							<asp:TableRow ID="Contents" runat="server" BorderWidth="1px" EnableTheming="False" EnableViewState="False" HorizontalAlign="Center" TableSection="TableHeader" VerticalAlign="Middle">
								<asp:TableCell ID="Col_00" runat="server" BorderWidth="1px" ColumnSpan="1" RowSpan="1" Width="140px" Wrap="False" BorderColor="Silver">名　称</asp:TableCell>
								<asp:TableCell ID="Col_01" runat="server" BorderWidth="1px" ColumnSpan="1" RowSpan="1" Width="210px" Wrap="False" BorderColor="Silver">測定日時</asp:TableCell>
								<asp:TableCell ID="Col_02" runat="server" BorderWidth="1px" ColumnSpan="1" RowSpan="1" Width="140px" Wrap="False" BorderColor="Silver">最新データ警報状況</asp:TableCell>
							</asp:TableRow>
						</asp:Table>
					</td>
					<td colspan="1" rowspan="1" style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px; margin: 0px; padding-top: 0px; width: 37px; height:auto;" tabindex="0">
						<asp:Image ID="ImgAlert" runat="server" ImageUrl="~/img/Alert.gif" Visible="False" EnableViewState="False" AlternateText="警報発生中" ToolTip="警報が発生しています" CssClass="NoClass" />
					</td>
	<%--            <td colspan="1" rowspan="1" style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px;
					margin: 0px; width: 37px; padding-top: 0px; height: 41px" tabindex="0">
					<asp:Literal ID="LitAlm" runat="server" EnableViewState="False" Mode="PassThrough" Visible="False"></asp:Literal>
				</td>
	--%>
				</tr>
			</tbody>
		</table>
		<asp:Image ID="Image2" runat="server" EnableViewState="False" CssClass="topImage" />
		<asp:Literal ID="litoutimg" runat="server"></asp:Literal>
		<asp:Panel ID="PnlCoordinate" runat="server" EnableTheming="False" EnableViewState="False" Visible="False" >
		<div id="mpxy"></div>
		</asp:Panel>
		<asp:Table ID="tblLastUpdate" runat="server" ToolTip="サーバに保持しているデータの保存期間を表示します。" EnableTheming="False" EnableViewState="False" CssClass="periodTable">
		</asp:Table>
		<p id="back-top"><a href="#top">Back to Top</a></p>
		<asp:Timer ID="TmrCheckNewData" runat="server" Enabled="False"></asp:Timer>
		<input id="nwdt" runat="server" type="hidden"/>
		<input id="nwdtno" type="hidden" runat="server" />
		<%--<form action="" method="post">--%>
		    <asp:TextBox ID="mnust" runat="server" CssClass="invisible" Text=""></asp:TextBox>
		    <%--<input id="mnust" type="hidden"/>--%>
		    <asp:Button id="btnSubmit" runat="server" EnableViewState="False" CssClass="invisible"></asp:Button>
		<%--</form>--%>
	</div>
</asp:Content>

