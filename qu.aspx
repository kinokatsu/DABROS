<%@ Page Language="VB" AutoEventWireup="true" CodeFile="qu.aspx.vb" Inherits="_Default" EnableSessionState="True" Trace="false" TraceMode="SortByTime" %>
<%@ Register Src="UserControl/question.ascx" TagPrefix="uc" TagName="ucQu" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
	<head runat="server">
		<title>アンケートフォーム</title>
		<link href="CSS/stl_Qu.css" rel="stylesheet" type="text/css" />
		<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
		<script type="text/javascript">
			$(function(){
				//ラジオボタン
				var radio = $('div.radio-group');
				$('input', radio).css({'opacity': '0'});
				$('label', radio).click(function() {
					$(this).parent().parent().each(function() {
						$('label',this).removeClass('checked');	
					});
					$(this).addClass('checked');
				});
			});
		</script>
	</head>
	<body>
		<form id="Questionnarie" runat="server">
			<div id="outerframe" class="paper"><div id="QName" runat="server" class ="formTitle">計測管理システム・DABROSアンケートフォーム</div>
			<div id="exp" runat="server"></div>
			<asp:PlaceHolder ID="dynamicitems" runat="server"></asp:PlaceHolder> 
				<div id="requestitems" class="Items">
					<div id="rqtitle" class="Title">●ご意見、ご要望</div>
					<div id="rqsupplementation" class="supplementation">お気づきの点や今後のご要望などががございましたら、お聞かせください（400文字以内）。</div>
					<div id="Feedback" class="RBL">
						<asp:TextBox ID="FeedbackText" runat="server" BorderStyle="Ridge" BorderWidth="2px" Columns="90" Rows="10" CssClass="FeedbackText" TextMode="MultiLine" MaxLength="400"></asp:TextBox> 
					</div>
				</div>
				<div id="fixitems" class="Items">
					<div id="customer" class="Title">●お客様情報</div>
						<ul>
							<li class="customer">
								<label for ="cust">お名前</label>
								<input id="custname" type="text" name="custname" maxlength="16" class="customerName" runat="server"/>
								<span class="addComment">差し支えなければ、ご担当者様のお名前をご記入ください。</span>
							</li>
							<li class="customer">
								<label for ="age">年齢</label>
								<asp:DropDownList ID="ddlAge" runat="server" CssClass="age">
									<asp:ListItem Value="0">選択してください</asp:ListItem>
									<asp:ListItem Value="1">10代</asp:ListItem>
									<asp:ListItem Value="2">20代</asp:ListItem>
									<asp:ListItem Value="3">30代</asp:ListItem>
									<asp:ListItem Value="4">40代</asp:ListItem>
									<asp:ListItem Value="5">50代</asp:ListItem>
									<asp:ListItem Value="6">60代</asp:ListItem>
									<asp:ListItem Value="7">70代以上</asp:ListItem>                            
								</asp:DropDownList>
								<span class="addComment">差し支えなければ、ご担当者様の年齢を選択してください。</span>
							</li>
							<li class="customer">
								<label for ="cust">現場名称</label>
								<input id="ConstSite" type="text" name="ConstSite" class="customerName" runat="server"/>
							</li>
						</ul>
					<div class="sendbtn">
						<asp:LinkButton ID="SendData" runat="server">送 信</asp:LinkButton> 
						<span class="addComment">　（確認画面が出ずに送信されます）</span>
					</div>
				</div>
			</div>                
		</form>
	</body>
</html>
