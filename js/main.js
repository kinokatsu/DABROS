var openFlg;
var ColRMargine;
// ページ表示時のフェードイン、ページ遷移時のフェードアウト 読み込み時は白いページにする
$("head").append("<style type='text/css'>#MasterPageFrm{display:none;}</style>");
function windowFade() {
	$("#MainContent").fadeIn(500);
	/*var vs = document.getElementById("ctl00_TScMan1_HiddenField").value;
	alert(vs);
	$('#ImageButton1').click(function () {
		var $btn = $(this);
		$('#SiteSelectFrm').animate({ opacity: 0.1 }, 300);
		//$('#SiteSelectFrm').fadeOut(500);
		$btn.unbind('click').click();
	});*/
}
// フォーカスセット
window.onload = function () { windowFade(); };
window.onunload = function () { windowFade(); };

$(document).ready(function () {
	/* this.window.name = "dabrostop"; */
	// hide #back-top first
	ColRMargine = parseInt($("#ctl00_pnlColR").css("margin-left"), 10);
	openFlg = document.getElementById("ctl00_CntplHolder_mnust").value;
	if (openFlg === "0") {
		$("#ctl00_ColL").css({
			display: "block",
			opacity: "0"
		});
		/*$('#ctl00_MyAccordion').css({ marginLeft: -$('#ctl00_ColL').width() });*/
		closeMenu(300);
	} else {
		openMenu(300);
	}
	$("#back-top").hide();
	// fade in #back-top
	$(function () {
		$(window).scroll(function () {
			if ($(this).scrollTop() > 900) {
				$("#back-top").fadeIn();
			} else {
				$("#back-top").fadeOut();
			}
		});

		// scroll body to 0px on click
		$("#back-top a").click(function () {
			$("body,html").animate({
				scrollTop: 0
			}, 800);
			return false;
		});
	});
	/*ブラウザを閉じたり、ログアウト、現場切替時に確認表示とグラフなどを閉じる*/
	$(window).on("beforeunload", function (e) {
		//		var ar = ['経時変化図', '波形データ', '深度分布図', 'データ一覧', '変位分布図', '経時表', '集計表', 'CSVDL', '制御', 'MNG', '撮影画像', '参考図面'];
		//		/* 子ウィンドウから書き込まれたリストの分だけ処理する */
		//		if (ar.length > 0) {
		//			for (var i = 0; i < ar.length; i++) {
		//				var win;
		//				if (ar[i] !== "") {
		//					/* window.open('',ar[i]).close(); */
		//					/* win = window.open("",ar[i]); */
		//					/* win.close(); */

		//					if (/Chrome/i.test(navigator.userAgent)) {
		//						win = window.open('about:blank', ar[i]);
		//						win.close();
		//						/* window.close(); */
		//					} else {
		//						window.open('about:blank', ar[i]).close();
		//					}
		//				};
		//			}
		//		}
		return "現場を切り替えますか？\n\n表示していた図や表を閉じてください。\n\n（閉じない場合は、エラーが表示される場合があります）\n\n";
		/*return "現場を切り替えますか？\n\n表示していた図や表は閉じられます(閉じない場合は、手動で閉じでください)。\n\n\n";*/
	});
	/* submit時は無効*/
	$("input[type=submit]").on("click", function () {
		$(window).off("beforeunload");
	});
	/* F5キー押下時は無効*/
	$(window).keydown(function (e) {
		if (e.keyCode === 116) $(window).off("beforeunload");
	});
	/* menuのスライドイン・スライドアウト */
	$("#ctl00_MenuHead").on("click", function () {
		if (openFlg === "1") {
			/*alert('Close');*/
			openFlg = "0";
			closeMenu(300);
		} else {
			/*alert('Open');*/
			openFlg = "1";
			openMenu(300);
		}
		document.getElementById("ctl00_CntplHolder_mnust").value = openFlg;
	});
	window.name = "dabrostop";
});
function openMenu(ft) {
	$(function () {
		/*		$('#FrmMasterPage:not(body#FrmMasterPage)').css({
		display: 'block',
		marginTop: -60,
		opacity: '0'
		});
		$('#FrmMasterPage:not(body#FrmMasterPage)').animate({
		marginTop: '0px',
		opacity: '1'
		}, ft);
		$('#FrmMasterPage').css({
		display: 'block',
		opacity: '0'
		});
		$('#FrmMasterPage').animate({
		opacity: "1"
		}, ft);*/
		$("#ctl00_ColL:not(body#ctl00_ColL)").css({
			display: "block",
			marginLeft: -$("#ctl00_ColL").width(),
			opacity: "0"
		});
		$("#ctl00_ColL:not(body#ctl00_ColL)").animate({
			marginLeft: "0px",
			opacity: "1"
		}, ft);
		$("#ctl00_pnlColR").animate({
			marginLeft: ColRMargine,
			opacity: "1"
		}, ft);
		$("#ctl00_ColL").css({
			display: "block",
			opacity: "0"
		});
		$("#ctl00_ColL").animate({
			opacity: "1"
		}, ft);
		$("#ctl00_MyAccordion").css({
			marginLeft: "0px"
		});
		$("#ctl00_ImgCollExp").attr({
			title: "メニューを隠す"
		});
		$({ deg: 180 }).animate({ deg: 0 }, {
			duration: 300,
			// 途中経過
			progress: function () {
				$("#ctl00_ImgCollExp").css({
					transform: "rotate(" + this.deg + "deg)"
				});
			}
		});
	});
}
function closeMenu(ft) {
	$(function () {
/*		$("#FrmMasterPage:not(body#FrmMasterPage)").css({
			display: "block",
			marginTop: -60,
			opacity: "0"
		});
		$("#FrmMasterPage:not(body#FrmMasterPage)").animate({
			marginTop: "0px",
			opacity: "1"
		}, ft);
		$("#FrmMasterPage").css({
			display: "block",
			opacity: "0"
		});
		$("#FrmMasterPage").animate({
			opacity: "1"
		}, ft);*/
		$("#ctl00_ColL:not(body#ctl00_ColL)").animate({
			marginLeft: -$("#ctl00_ColL").width(),
			opacity: "1"
		}, ft);
		$("#ctl00_pnlColR").animate({
			marginLeft: 0,
			opacity: "1"
		}, ft);	
		$("#ctl00_ColL").css({
			display: "block",
			opacity: "0"
		});
		$("#ctl00_ColL").animate({
			opacity: "1"
		}, ft);
		$("#ctl00_MyAccordion").css({
			marginLeft: "0px"
		});
		$("#ctl00_ImgCollExp").attr({
			title: "メニューを表示する"
		});
		$({deg:0}).animate({deg:180}, {
			duration:300,
			// 途中経過
			progress:function() {
				$("#ctl00_ImgCollExp").css({
					transform:"rotate(" + this.deg + "deg)"
				});
			}
		});
	});
}
