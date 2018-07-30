//var j = jQuery.noConflict();
jQd = jQuery;
jQd(document).ready(function () {
    var i;
    if (document.getElementById('RdBFromNewest').checked) {
        for (i = 0; i <= 6; i++) {
            document.getElementById(['TxtDate' + i]).disabled = true;
            document.getElementById(['imgCalTxtDate' + i]).style.visibility = "hidden";
        }
    }
    else {
        for (i = 0; i <= 6; i++) {
            document.getElementById(['TxtDate' + i]).disabled = false;
            document.getElementById(['imgCalTxtDate' + i]).style.visibility = "visible";
        }
    }
    //    $("#PnlCalendar").hide()
    //$("#PnlCalendar").attr('display', 'none');      // 初期はこちらの方法でないとダメみたい・・・　orz
    RegisterPopupBehavior();                    // イベント付加
    //jQd('input[id^="TxtDate"]').addClear();        // TextBoxのクリア
    //$(':input[id^=TxtDate]').addClear();        // TextBoxのクリア
});
//function setToolTip(pos, arr, ofst) {
//    tippy('area[title],img[title]', {
//        placement: pos,
//        animation: 'shift-away',
//        duration: 300,
//        arrow: arr,
//        arrowType: 'round',
//        arrowTransform: 'scaleX(0.7) scale(1.2)',
//        theme: 'tooltip bordered-theme',
//        inertia: true,
//        followCursor: true,
//        performance: true,
//        offset: ofst
//    });
//}
//    $(function(){
//        // default settings
//        $.Tooltip.defaults = $.extend( $.Tooltip.defaults, {
//            delay      : 1,
//            showURL    : false,
//            showBody   : " - ",
//            left       : -100,
//            track      : true
//        });
//        $("area[title],img[title]").Tooltip({
//            delay      : 1,
//            showURL    : false,
//            left       : -100,
//            track      : true
//        });
//    });

//ラジオボタン変更イベント
function ChangeSelState(obj) {
    if (obj.id == "RdBFromNewest"){
        var i;
        for (i=0;i<=6;i++) {
        document.getElementById(['TxtDate'+i]).disabled=true;
        document.getElementById(['imgCalTxtDate' + i]).style.visibility = "hidden";
        }
    }    
    else if (obj.id == "RdBDsignatedDate"){
        for (i=0;i<=6;i++) {
        document.getElementById(['TxtDate'+i]).disabled=false;
        document.getElementById(['imgCalTxtDate' + i]).style.visibility = "visible";
        }
    }
}
function PrintPreview() {
    if(window.ActiveXObject == null || document.body.insertAdjacentHTML == null) return;
    var sWebBrowserCode = '<object width="0" height="0" classid="CLSID:8856F961-340A-11D0-A96B-00C04FD705A2"></object>'; 
    document.body.insertAdjacentHTML('beforeEnd', sWebBrowserCode); 
    var objWebBrowser = document.body.lastChild;
    if(objWebBrowser == null) return;
    objWebBrowser.ExecWB(7, 0);
    document.body.removeChild(objWebBrowser);
}
// 日付の書式を調整する関数　yyyy/MM/dd
function DateToString(objDate){
    var result = "";
    var temp;

    // 年はそのまま4桁。
    result += objDate.getFullYear();
    result += "/";

    // 月は 2 桁に調整。月は 0 から始まるので +1 とする。
    // 調整は文字列として頭に "0" を付加。後ろ2文字を取得。　0サプレスしない
    temp = "0" + (objDate.getMonth() + 1);
    temp = temp.substr(temp.length - 2, temp.length);
    result += temp;
    result += "/";

    // 日も月と同じ調整
    temp = "0" + objDate.getDate();
    temp = temp.substr(temp.length - 2, temp.length);
    result += temp;

    // result 取得した値を返す。
    return result;
}
// キャンセルボタンを押したときに、Popupを消す
function BtnCancel() {
    var nu = document.getElementById("txtID").value;
    pceObj = "pce" + nu;
    jQd.find(pceObj).hidePopup();
    comboEnable(true);      // Disabled
    //Sys.Extended.UI.PopupControlBehavior.__VisiblePopup.hidePopup();
    //AjaxControlToolkit.PopupControlBehavior.__VisiblePopup.hidePopup();
    }
//　イメージボタンにPopupControlExtenderを割り当てているが、テキストボックスに値を入力 //
function inData(e) {
    //var str = e.id;
    var nu = document.getElementById("txtID").value;
    //var nu = str.substr(str.length-1,1);
    TxtObj = "TxtDate" + nu;
    var textbox = document.getElementById(TxtObj);
    textbox.value = e;  //.Value;
    comboEnable(true);
}
// PopupContorolExtender に表示前イベントを付加
function RegisterPopupBehavior(){
  $find('pce0')._popupBehavior.add_showing(onPopupShowing);
  $find('pce1')._popupBehavior.add_showing(onPopupShowing);
  $find('pce2')._popupBehavior.add_showing(onPopupShowing);
  $find('pce3')._popupBehavior.add_showing(onPopupShowing);
  $find('pce4')._popupBehavior.add_showing(onPopupShowing);
  $find('pce5')._popupBehavior.add_showing(onPopupShowing);
  $find('pce6')._popupBehavior.add_showing(onPopupShowing);
  $find('pce0')._popupBehavior.add_shown(onPopupShown);
  $find('pce1')._popupBehavior.add_shown(onPopupShown);
  $find('pce2')._popupBehavior.add_shown(onPopupShown);
  $find('pce3')._popupBehavior.add_shown(onPopupShown);
  $find('pce4')._popupBehavior.add_shown(onPopupShown);
  $find('pce5')._popupBehavior.add_shown(onPopupShown);
  $find('pce6')._popupBehavior.add_shown(onPopupShown);
  $find('pce0')._popupBehavior.add_hidden(onPopupHidden);
  $find('pce1')._popupBehavior.add_hidden(onPopupHidden);
  $find('pce2')._popupBehavior.add_hidden(onPopupHidden);
  $find('pce3')._popupBehavior.add_hidden(onPopupHidden);
  $find('pce4')._popupBehavior.add_hidden(onPopupHidden);
  $find('pce5')._popupBehavior.add_hidden(onPopupHidden);
  $find('pce6')._popupBehavior.add_hidden(onPopupHidden);
}
// コンボボックスのEnable/Disableを切り替える
function comboEnable(Enafal){
//    var ddlobj;
//    var c1obj;
    // PopupContorolExtenderが消えた後もカレンダーがあった位置で認識をしてしまうため、カレンダの表示／非表示を切り替える
//    c1obj = document.getElementById("C1WebCalendar");
//    ddlobj = document.getElementById("LstBTime");       // 2009/07/06 Kino Add
//    pnlCal = document.getElementById("PnlCalendar");    // 2015/05/25 Kino Add
    if (Enafal === true){
        $("#PnlCalendar").hide()
//        c1obj.style.visibility = "hidden";
//        ddlobj.style.visibility = "hidden";             // 2009/07/06 Kino Add
//        pnlCal.style.visibility = "hidden";             // 2015/05/25 Kino Add
    }
    else {
        $("#PnlCalendar").show();
//        c1obj.style.visibility = "visible";
//        ddlobj.style.visibility = "visible";            // 2009/07/06 Kino Add
//        pnlCal.style.visibility = "visible";            // 2015/05/25 Kino Add 
    }
}
// PopupContorolExtenderの表示中イベント
function onPopupShowing(sender, eventArgs){
    //comboEnable(false);     // Eabled
    var num = sender._id.substring(3,4);
    var ddlobj;
    var textbox=document.getElementById("TxtDate" + num);
    if (textbox.value !== ""){
        var da = new Date(textbox.value);
        //C1WebCalendar_Client.UnSelectAll();
        $("#C1WebCalendar").c1calendar("unSelectAll");
        //C1WebCalendar_Client.DisplayDate = da;
        $("#C1WebCalendar").c1calendar({ displayDate: da });
        //C1WebCalendar_Client.SelectDate(da);
        $("#C1WebCalendar").c1calendar("selectDate", da);
        //C1WebCalendar_Client.Refresh();
        $("#C1WebCalendar").c1calendar("refresh");
        ddlobj = document.getElementById("LstBTime");           //リストボックスの時刻を選択
        curHour = to2String(da.getHours());
        curMinute =  to2String(da.getMinutes());
        DateString = curHour + ":" + curMinute      //+ ":" + curSecond;   // 時刻文字列を作成
        comboEnable(false);
    }
}
// 時刻の０付加
function to2String(value){
    label=""+value;
    if(label.length<2){
        label="0"+label;
    }
    return label;
}
// PopupContorolExtenderが表示された後のイベント
function onPopupShown(sender, eventArgs){
    comboEnable(false);     // Eabled
}
// PopupContorolExtenderが消えた後のイベント
function onPopupHidden(sender, eventArgs){
    comboEnable(true);      // Disabled
}
// コンフリクト対策
jQuery.noConflict();
(function ($) {
    jQd(function () {
        jQd('input[id^=TxtDate]').addClear();        // TextBoxのクリア
        // jQd(':input[id^=TxtDate]').addClear();        // TextBoxのクリア
    });
})(jQuery);
// グラフ設定の不要枠を削除する
function removeRow(st)
{
    var searchID;
    for (i=st;i<=4;i++) {
        searchID ="r" + String(i);
        // 検索するID要素がわかっているなら、jQueryよりこちらの方が高速らしい
        if (document.getElementById(searchID) != "") {
            $("#" + searchID).remove();
        }
    }
}
function showalert(msg) {
    alert(msg);
}