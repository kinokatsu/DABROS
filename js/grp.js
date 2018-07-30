$(document).ready(function () {
	//function pageLoad() {
	if (document.getElementById('RdBFromNewest').checked) {
		document.getElementById('TxtStartDate').disabled = true;
		document.getElementById('TxtEndDate').disabled = true;
		document.getElementById('imgCalTxtStartDate').style.visibility = "hidden";
		document.getElementById('imgCalTxtEndDate').style.visibility = "hidden";
		document.getElementById('DDLRange').disabled = false;
	} else {
		document.getElementById('TxtStartDate').disabled = false;
		document.getElementById('TxtEndDate').disabled = false;
		document.getElementById('imgCalTxtStartDate').style.visibility = "visible";
		document.getElementById('imgCalTxtEndDate').style.visibility = "visible";
		document.getElementById('DDLRange').disabled = true;
	}
	setCss();
});

// コードビハインドでCSSをセットし直してもNGのため、クライアントサイドで処理する・・・ orz
function setCss() {
	var selIndex = $('#DdlPaperOrientation').val();
	if (selIndex == 0) {
		$('#GraphPage').removeClass('GraphOuterLandscape');
		$('#GraphPage').addClass('GraphOuterPortrait');
	} else {
		$('#GraphPage').removeClass('GraphOuterPortrait');
		$('#GraphPage').addClass('GraphOuterLandscape');
	}
}

function PrintPreview() {
	if(window.ActiveXObject == null || document.body.insertAdjacentHTML == null) return;
	var sWebBrowserCode = '<object width="0" height="0" classid="CLSID:8856F961-340A-11D0-A96B-00C04FD705A2"></object>'; 
	document.body.insertAdjacentHTML('beforeEnd', sWebBrowserCode); 
	var objWebBrowser = document.body.lastChild;
	if(objWebBrowser == null) return;
	objWebBrowser.ExecWB(7, 1);
	document.body.removeChild(objWebBrowser);
	}

// 間引き情報のチェック付け外し処理//
function Select(Select){
  for (i=0;i<=23;i++)
	document.forms[0].elements["CBLPartial$"+i].checked=Select; return false;
}

// 「最初から」もしくは「指定期間」選択によるオブジェクトの表示・非表示
function ChangeSelState(obj) {
		if (obj.id == "RdBDsignatedDate") {
			document.getElementById('TxtStartDate').disabled=false;
			document.getElementById('TxtEndDate').disabled=false;
			document.getElementById('imgCalTxtStartDate').style.visibility = "visible";
			document.getElementById('imgCalTxtEndDate').style.visibility = "visible";    
			document.getElementById('DDLRange').disabled=true;
			}
		else if (obj.id == "RdBFromNewest") {
			document.getElementById('TxtStartDate').disabled=true;
			document.getElementById('TxtEndDate').disabled=true;
			document.getElementById('imgCalTxtStartDate').style.visibility = "hidden";
			document.getElementById('imgCalTxtEndDate').style.visibility = "hidden";
			document.getElementById('DDLRange').disabled = false;
		}
	}

// 表のマウスオーバーで、マウスのある行の背景色を変更する
function setBg(tr, color) {
  tr.style.backgroundColor = color;
}

// ブラウザーの状態チェック?
function CheckBrowserReadyState() {
	if(window.document.readyState != null && window.document.readyState != 'complete') {
		return false; //処理続行NG¥n
		}
	else{
		return true; //処理続行OK¥n 
		}
}

// 正規表現関係?
function CheckRegExp() {
	var strSrc = "";
	var rObj = new RegExp("^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$");
	for (i=1;i<=6;i++) {
		if (document.getElementById(['TxtMax'+i]) != undefined){
			strSrc = document.getElementById(['TxtMax'+i]).value;
			if (strSrc.match("^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$")){}
			else{
				alert("【左軸 最大値 No." + i + "】に入力に間違いがあります。　数値(半角)のみとしてください。");
				CancelPostBack();
			}
		}
	}
}

// ポストバックをキャンセル
function CancelPostBack()
{
	var prm = Sys.WebForms.PageRequestManager.getInstance();
	prm.abortPostBack();
}
// グラフ設定の不要枠を削除する
function removeRow(st) {
	var searchID;
	for (i = st; i <= 6; i++) {
		searchID = "No" + String(i);
		// 検索するID要素がわかっているなら、jQueryよりこちらの方が高速らしい
		if (document.getElementById(searchID) != "") {
			$("#" + searchID).remove();
		}
	}
}