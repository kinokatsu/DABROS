<%@ Page Title="" Language="VB" MasterPageFile="~/MasterPageRv.master" AutoEventWireup="false" CodeFile="MainRiver.aspx.vb" Inherits="MainRiver" Trace="false" TraceMode="SortByTime"%>
<%@ MasterType VirtualPath="~/MasterPageRv.master" %>
<asp:Content ID="CntHead" ContentPlaceHolderID="head" Runat="Server"></asp:Content>
<asp:Content ID="CntPlHolder" ContentPlaceHolderID="CntPlHolder" Runat="Server">
	<asp:Literal id="dcss" runat="server" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge"/>
	<link href="css/stl_WaterLevel.css" rel="stylesheet" type="text/css" />
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
<%--	<!--[if IE]><script type="text/javascript" src="js/tippy.v142.min.js"></script><![endif]-->
	<!--[if !IE]>--><script src="js/tippy.all.min.js" type="text/javascript"></script><!--<![endif]-->--%>
	<script src="js/tippy.all.min.js" type="text/javascript"></script>
	<%--<script src="js/updCheck.min.js" type="text/javascript"></script>--%>
	<script src="js/ScrollViewer.js" type="text/javascript"></script>
	<script src="js/river.js" type="text/javascript"></script>
	<div id="MainContent" runat="server">
		<div runat="server" id="map" class="maparea">
			<div runat="server" id="innerMap" class="inmap">
				<asp:Image ID="topimg" runat="server" EnableViewState="False" CssClass="areamap"/>
				<asp:BulletedList ID="MesPoint" runat="server" BorderStyle="None" EnableTheming="False" EnableViewState="False">
				</asp:BulletedList>
			</div>
		</div>
		<asp:Literal ID="litoutimg" runat="server"></asp:Literal>
		<asp:Panel ID="PnlCoordinate" runat="server" EnableTheming="False" EnableViewState="False" Visible="False" >
		<div id="mpxy"></div>
		</asp:Panel>
		<p id="back-top"><a href="#top">Back to Top</a></p>
		<asp:Timer ID="TmrCheckNewData" runat="server" Enabled="False"></asp:Timer>
		<input id="nwdt" type="hidden" runat="server" />
		<input id="nwdtno" type="hidden" runat="server" />
		<input id="akey" type="hidden" runat="server" />
		<input id="abox" type="hidden" runat="server" />
		<div id="alertSt" class="alertstatus invisible">警戒状況</div>
		<asp:Button id="btnSubmit" runat="server" EnableViewState="False" CssClass="invisible"></asp:Button>
	</div>
</asp:Content>

