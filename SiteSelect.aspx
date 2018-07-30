<%@ Page Language="VB" AutoEventWireup="false" CodeFile="SiteSelect.aspx.vb" Inherits="SiteSelect" Trace="false" TraceMode="SortByTime"%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
	<title id="MyTitle" runat="server">現場選択</title>
	<meta name="viewport" content="width=device-width, initial-scale=0.7, minimum-scale=0.7, maximum-scale=5.0, user-scalable=yes" />
	<meta charset="utf-8" />
	<meta http-equiv="X-UA-Compatible" content="IE=9" />
	<link rel="SHORTCUT ICON" href="~/favicon.ico" />
	<link href="CSS/stl_startpage.css" rel="stylesheet" type="text/css" />
	<script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
	<script type="text/javascript" src="js/GenbaSearch.min.js"></script>
	<script type="text/javascript" src="js/jquery.easing.1.3.min.js"></script>
	<script type="text/javascript" >
		var ft = 500;	    
		$(function () {
			$('#SiteSelectFrm:not(body#SiteSelectFrm)').css({ display: 'block', marginTop: -$(window).height() * 0.5, opacity: '0' });
			$('#SiteSelectFrm:not(body#SiteSelectFrm)').animate({ marginTop: '0px', opacity: '1' }, ft);
			$('#SiteSelectFrm').css({ display: 'block', opacity: '0' });
			$('#SiteSelectFrm').animate({ opacity: '1' }, ft);
			windowFade();
			document.all('ImageButton1').focus();       // フォーカスセット
		});   
		// 現場名の点滅
//		$(function () {
//			//$('.dmclass').parent().addClass('ShowAreaAlert');
//			setInterval(function () {
//				$('.ShowAreaAlert').fadeOut(500, function () { $(this).fadeIn(500) });
//			}, 1000);
//		});
		// ページ表示時のフェードイン、ページ遷移時のフェードアウト　読み込み時は白いページにする
		$('head').append('<style type="text/css">#SiteSelectFrm{display:none;}</style>');
		function windowFade() {
			$('#SiteSelectFrm').fadeIn(500);
			$('#ImageButton1').click(function () {
				var $btn = $(this);
				//$('#SiteSelectFrm').animate({ opacity: 0.1 }, 300);
				// ページが上にスライドアウト
				$('#SiteSelectFrm').animate({ marginTop: '-=' + $(window).height() + 'px', opacity: '0' }, ft, function () {
					//					setTimeout(function () {
					//						$('#SiteSelectFrm').css({ marginTop: '0', opacity: '1' })
					//					}, 10000);
				});
				$btn.unbind('click').click();
				//return false;
			});
		};
		window.onunload = function () { windowFade(); };
		window.onunload = function () { windowFade(); };
		function imgbtnClick() {
		    $('#ImageButton1').click();
		};
	</script>
</head>
<body style="text-align: center">
	<form id="SiteSelectFrm" runat="server">
	<div style="margin: auto; text-align: center; font-size: 11pt;" id="FrmSiteSelect">
		<asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" 
			EnablePageMethods="True" EnableScriptGlobalization="True">
		</asp:ToolkitScriptManager>
		<div style="margin: auto; width: 90%">
			<div id="sitename" style="margin: auto; width: 100%; height: 27px; text-align: center;" class="headBlock" >
				<asp:Label ID="lblSiteName" runat="server" Font-Bold="True" Font-Size="12pt" ForeColor="White" BorderStyle="None"></asp:Label>
			</div>
			<div id="sitelist" class="siteListButton">
				<asp:RadioButtonList ID="RBLConstructionList" runat="server" BorderColor="#C0C0FF" BorderStyle="Double" BorderWidth="4px" CellPadding="0" 
					CssClass="allSiteList" Font-Size="11pt" ToolTip="一覧から選択してください" Width="100%">
				</asp:RadioButtonList>
				<asp:ImageButton ID="ImageButton1" runat="server" AlternateText="決 定" EnableTheming="True" EnableViewState="False" ImageUrl="~/img/decision.png" BorderStyle="None" 
					style="outline:none;" CssClass="imgbutton" />
			</div>
			<div id="Notes">
				<div style="text-decoration:underline; text-align: left;">お知らせ</div>
				<asp:GridView ID="GrdView" runat="server" BackColor="White"
					BorderColor="#DEDFDE" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" DataSourceID="AccessDataSource1"
					ForeColor="Black" GridLines="Vertical" Width="100%" AllowPaging="True" 
					AllowSorting="True" EmptyDataText="---" Font-Size="9pt" ToolTip="お知らせ" 
					CssClass="allSiteList">
					<FooterStyle BackColor="#CCCC99" />
					<RowStyle BackColor="#F7F7DE" BorderStyle="Solid" BorderWidth="1px" BorderColor="Silver" />
					<PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Center" VerticalAlign="Top" BorderStyle="Solid" BorderWidth="1px" BorderColor="Silver" />
					<SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
					<HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" BorderStyle="Solid" BorderWidth="1px" BorderColor="Silver" />
					<AlternatingRowStyle BackColor="White" />
					<PagerTemplate>
						&nbsp;<asp:ImageButton ID="ImgBtnFirst" runat="server" AlternateText="最初" ImageUrl="~/img/First.png" CommandArgument="First" CommandName="Page" ToolTip="最初のページへ" />
						<asp:ImageButton ID="ImgBtnPrev" runat="server" AlternateText="前" ImageUrl="~/img/prev.png" CommandArgument="Prev" CommandName="Page" ToolTip="前のページへ" />
						[ <%= GrdView.PageIndex+1 %> / <%= GrdView.PageCount %> ]
						<asp:ImageButton ID="ImgBtnNext" runat="server" AlternateText="次"  ImageUrl="~/img/Next.png" CommandArgument="Next" CommandName="Page" ToolTip="次のページへ" />
						<asp:ImageButton ID="ImgBtnLast" runat="server" AlternateText="最後" ImageUrl="~/img/Last.png" CommandArgument="Last" CommandName="Page" ToolTip="最後のページへ" />
					</PagerTemplate>
					<EditRowStyle HorizontalAlign="Left" VerticalAlign="Middle" />
				</asp:GridView>
				<asp:AccessDataSource ID="AccessDataSource1" runat="server"></asp:AccessDataSource>
			</div>
			<div id="gsearchbox" class="searchbox" runat="server">
				<div id="search" action="javascript:searchWord()">現場名：
					<input id="q" class="sbtn" autocomplete="on" placeholder="現場名の一部を入力" />
					<input type="button" class ="sbtn" onclick="searchWord()" value="検索" />
					<input id="decision" type="button" class ="sbtn" value="決定" onclick="imgbtnClick();return false;" />
				</div>
			</div>
			<div id="Operation" class="accordionDiv">
				<asp:Panel ID="Ph1" runat="server" CssClass="accordionHeader" EnableTheming="False">ご利用方法 
					<asp:ImageButton ID="ImgCollExp" runat="server" EnableTheming="False" EnableViewState="False" ImageAlign="AbsMiddle" BorderStyle="None" CssClass="leftspace10" />
					<asp:Label ID="LblColl1" runat="server" Font-Size="Small" ForeColor="Gold" 
						BorderStyle="None" EnableTheming="True" Width="100px"></asp:Label>
				</asp:Panel>
				<asp:Panel ID="Pc1" runat="server" EnableTheming="True" CssClass="collapsePanel">
このサービスの取り扱いについてご説明いたします。<br />
					<ul>
						<li>対応ブラウザー<br />
							Chrome, FireFox, Opera, Safari など<br />Internet Explorer 10以降(表示に異常が見られる場合は、互換モードでご利用ください)<br /><br /></li>
						<li>ポップアップブロックについて<br />
						ポップアップ ブロックは、ポップアップを制限またはブロックできるようにする機能です。各グラフや表は別ウィンドウに表示しますので、ポップアップブロックは解除するようにしてください。解除方法は以下を参照してください。<br />
							<a href="https://support.mozilla.org/ja/kb/pop-blocker-settings-exceptions-troubleshooting" target="_blank">FireFoxヘルプ</a><br />
							<a href="https://support.google.com/chrome/answer/95472?hl=ja&co=GENIE.Platform%3DDesktop" target="_blank">Chromeヘルプ</a><br />
							<a href="https://support.microsoft.com/ja-jp/help/17479/windows-internet-explorer-11-change-security-privacy-settings" target="_blank">Internet Explorer ポップアップ ブロック</a><br />
							<a href="http://help.opera.com/Windows/9.02/ja/general.html" target="_blank">Opera</a><br />
							<a href="https://support.apple.com/ja-jp/guide/safari/block-pop-ups-and-unnecessary-content-sfri40696/mac" target="_blank">Safari</a><br /><br />
						</li>
						<li>お知らせ<br />
						お知らせには、現場における注記すべき事象やサーバメンテナンスなどについて記載しています。<br /><br />
						</li>
						<li>表、グラフについて<br />
						各表やグラフの初期設定はサーバ内にのみ保存していますので、毎回同じ状態で表示します。個別に設定を変更してもサーバ内の設定は変更されません。<br />
						トップページのメニューから該当の項目をクリックすることで、表示します。
						</li>
						<%--<li>携帯電話(フィーチャーフォン)からの閲覧はオプション設定となります。弊社営業までご連絡ください。データの表示は数値のみとなります。 ※スマートフォンからの閲覧はパソコンと同様に行えます。<br /><br />
						</li>
						<li>表示に関して異常が見られる場合は、ログイン画面下部のリンクからメールを送信してください。</li>--%>
					</ul>
				</asp:Panel>
				<asp:CollapsiblePanelExtender ID="CollapsiblePanelExtender1" runat="server" 
					CollapseControlID="Ph1" Collapsed="True" CollapsedImage="~/img/el.png" 
					Enabled="True" ExpandControlID="Ph1" ExpandedImage="~/img/cl.png" 
					ExpandedText="非表示" ImageControlID="ImgCollExp" SuppressPostBack="True" 
					TargetControlID="Pc1" TextLabelID="LblColl1" CollapsedText="表示">
				</asp:CollapsiblePanelExtender>
			</div>		
		</div>
	</div>
	</form>
</body>
</html>
