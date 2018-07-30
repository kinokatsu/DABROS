<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Login.aspx.vb" Inherits="Login" Culture="ja-JP" Trace="false" TraceMode="SortByTime"%>
<!DOCTYPE html>
<html lang="ja-JP" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<title>インターネットデータ閲覧サービス DABROS</title>
	<meta charset="utf-8" />      
	<meta name="viewport" content="width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no" />
	<meta http-equiv="X-UA-Compatible" content="IE=9" />
	<link rel="SHORTCUT ICON" href="~/favicon.ico" />
	<link href="CSS/stl_startpage.css" rel="stylesheet" type="text/css" />
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
	<script src="js/m.js" type="text/javascript"></script>
	<script type="text/javascript">
		var ft = 500;
		$(function() {
			// ページ表示時のフェードイン、ページ遷移時のフェードアウト　読み込み時は白いページにする
			$('head').append('<style type="text/css">#LoginFrm{display:none;}</style>');
			$('#LoginFrm:not(body#LoginFrm)').css({ display: 'block', marginTop: -$(window).height()*0.5, opacity: '0' });
			$('#LoginFrm:not(body#LoginFrm)').animate({ marginTop: '0px', opacity: '1' }, ft);
			$('body#LoginFrm').css({ display: 'block', opacity: '0' });
			$('body#LoginFrm').animate({ opacity: '1' }, ft);
			windowFade();
			// フォーカスセット
			document.all('Login1_UserName').focus();
			// 文字変換全角->半角 自動変換
			$("#Login1_UserName").change(function (e) {
				var str = $(this).val();
				if (str == "") {
					e.preventDefault()
					//return false;
				} else {
					str = str.replace(/[Ａ-Ｚａ-ｚ０-９－！”＃＄％＆’（）＝＜＞，．？＿［］｛｝＠＾～￥]/g, function (s) {
						return String.fromCharCode(s.charCodeAt(0) - 65248);
					});
					$(this).val(str);
				}
			});
		});       
//		function doexchg() {
//			var data;
//			var textbox;
//			var un = document.getElementById("UserName");
//			//throw new Error("エラー発生"); 
//			textbox = document.getElementById('Login1_Password');
//			//if( textbox.value != null || textbox.value != undefined ){
//			if (textbox.value != "") {

//				if (un.startswith("IoT")) {
//				
//				} else {

//				 }
//				data = textbox.value;
//				var pd = data.trim();
//				var phash = MD5_hexhash(pd);
//				textbox.value = phash;

//				// 遷移時フェードアウト
//				//$('#LoginFrm').animate({ opacity: 0.0 }, 300);
//				// ページが上にスライドアウト
//				$('#LoginFrm').animate({ marginTop: '-=' + $(window).height() + 'px', opacity: '0' }, ft, function () {
//	//				setTimeout(function () {
//	//					$('#LoginFrm').css({ marginTop: '0', opacity: '1' })
//	//				}, 3000);
//				});
//			}
//			return false;
//		}
//		// StringクラスにTrimメソッドの追加
//		String.prototype.trim = function () {
//			return this.replace(/^[ ]+|[ ]+$\r\n/g, '');
//		}
		// お気に入り追加
		var bookmarkurl = window.location.href;
		var bookmarktitle = document.title;
		function setBookMark() {
			if (document.all) {
				window.external.AddFavorite(bookmarkurl, bookmarktitle);
			} else if (window.sidebar && window.sidebar.addPanel) {
				window.sidebar.addPanel(bookmarktitle, bookmarkurl, "");
			} else {
				alert("このブラウザへのお気に入り追加ボタンは、Google Chrome/Safari等には対応しておりません。\nGoogle Chrome/Safariの場合、CtrlキーとDキーを同時に押してください。\nその他の場合はご利用のブラウザからお気に入りへ追加下さい。");
			}
		}
		function windowFade() {
			$('#LoginFrm').fadeIn(500);
		};
	</script>
</head>
<body oncontextmenu="return false" id="LoginForm" class="LABELS_Center">
	<form id="LoginFrm" runat="server" autocomplete="new-password">
		<div id="FrmLogin" class="loginform">
			<table id="TABLE1">
				<tr id="TitleLogo">
					<td style="border-style: none; vertical-align: middle; text-align: center; background-color: #4169E1;" contenteditable="false" class="loginform">
						<img src="~/img/Logo2.png" alt="Logo" contenteditable="false" id="ImgLogo" runat="server" enableviewstate="false" style="vertical-align: middle; text-align: center; border-style: none" />
					</td>
				</tr>
				<tr id="LoginCell">
					<td style="border-style: none; height: 100px; vertical-align: top;" >
						<asp:Login ID="Login1" runat="server" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderPadding="4"
							BorderStyle="Solid" BorderWidth="1px" DisplayRememberMe="False" EnableTheming="True"
							FailureText="ログインに失敗しました。ユーザー名 または パスワードが間違っていることが考えられます。大文字小文字などをご確認ください。"
							Font-Size="12pt" ForeColor="Black" ToolTip="ユーザ名とパスワードを入力して「ログイン」ボタンを押してください。"
							Width="338px" DestinationPageUrl="~/SiteSelect.aspx" EnableViewState="False" >
						<TextBoxStyle BackColor="Ivory" BorderStyle="Ridge" Font-Size="0.8em" />
						<LoginButtonStyle BackColor="LightYellow" BorderColor="#507CD1" BorderWidth="2px" Font-Bold="True" Font-Names="MS UI Gothic" Font-Size="1em" ForeColor="#284E98" />
						<InstructionTextStyle Font-Italic="True" ForeColor="Black" />
						<TitleTextStyle BackColor="RoyalBlue" Font-Bold="True" Font-Size="1.1em" ForeColor="White" />
						<ValidatorTextStyle Font-Bold="True" />
						<FailureTextStyle Font-Bold="False" Font-Names="MS UI Gothic" HorizontalAlign="Left" Font-Size="11pt" />
							<LayoutTemplate>
								<table class="logintableinside">
									<tr id="Title">
										<td colspan="2" style="font-weight: bold; font-size: 1.1em; color: white; background-color: #4169E1; height: 22px;">ログイン</td>
									</tr>
									<tr id="UsernameRow">
										<td class="logintextheadcell">ユーザー名:</td>
										<td class="logintextboxcell">
											<asp:TextBox ID="UserName" runat="server" autofocus="true" placeholder="ユーザー名を入力" CssClass="logintextbox" autocomplete="new-password"></asp:TextBox>
											<asp:RequiredFieldValidator ID="UserNameRequired" runat="server" 
												ControlToValidate="UserName" ErrorMessage="ユーザー名が必要です。" Font-Bold="True" 
												ToolTip="ユーザー名が必要です。" ValidationGroup="Login1" BorderStyle="None" 
												EnableClientScript="True">*</asp:RequiredFieldValidator>
										</td>
									</tr>
									<tr id="PasswordRow">
										<td class="logintextheadcell">パスワード:</td>
										<td class="logintextboxcell">
											<asp:TextBox ID="Password" runat="server" TextMode="Password" placeholder="パスワードを入力" CssClass="logintextbox" autocomplete="new-password"></asp:TextBox>
											<asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" ErrorMessage="パスワードが必要です。" Font-Bold="True" ToolTip="パスワードが必要です。" ValidationGroup="Login1" BorderStyle="None" EnableClientScript="True">*</asp:RequiredFieldValidator>
										</td>
									</tr>
									<tr id="FailureRow">
										<td colspan="2" style="font-weight: normal; font-size: 10pt; color: red; text-align: left; vertical-align: middle;">
											<asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
										</td>
									</tr>
									<tr id="LoginButtonRow">
										<td colspan="2" style="padding: 5px 10px 5px 5px; height: 40px; text-align: right; vertical-align: middle;">
											<asp:Button ID="LoginButton" runat="server" BackColor="LightYellow" 
												BorderColor="#507CD1" BorderWidth="2px" CommandName="Login" Font-Bold="False"
												Font-Size="1em" ForeColor="#284E98" OnClientClick="doexchg();" Text="ログイン" ValidationGroup="Login1" 
												style="outline:none;"/>
										</td>
									</tr>
								</table>
							</LayoutTemplate>
						</asp:Login>
						<asp:Label ID="LblQuery" runat="server" BackColor="LightGoldenrodYellow" Font-Bold="False"
							Font-Size="10pt" ForeColor="Red" Text="「ユーザー名」または「パスワード」が不明の場合は、担当までご連絡ください。"
							Visible="False" CssClass="NOTICE_Error" BorderStyle="None" Width="338px"></asp:Label>
					</td>
				</tr>
			</table>
		</div>
		<asp:Panel ID="PnlNotice01" runat="server" CssClass="NOTICE">
			<span style="font-weight:bold">当サイトでは､JavaScript、cookieを使用しております｡</span>
		</asp:Panel>
		<asp:Panel ID="TB_ExpBors" runat="server" CssClass="browser">
			<ul>
				<li>Chrome,FireFox,Opera,Safari(Mac)，Microsoft Edgeの最新版をご利用ください。</li>
				<li>Internet Explorer(IE)は11をご利用ください。それ以外では、正常に表示されない場合があります。</li>
			</ul>
		</asp:Panel>
		<asp:Label ID="lblCopyright" runat="server" Text="Copyright &copy; since 2013 SAKATA DENKI Co.,Ltd. All Rights Reserved." CssClass="copyR"></asp:Label>
		<div class="copyR" id="guideline1">お客様にあらかじめ通知することなくサービスの内容や仕様を変更する場合がございます。</div>
	</form>
</body>
</html>
