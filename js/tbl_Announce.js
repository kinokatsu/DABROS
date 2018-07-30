function PrintPreview()
    {
    if(window.ActiveXObject == null || document.body.insertAdjacentHTML == null) return;
    var sWebBrowserCode = '<object width="0" height="0" classid="CLSID:8856F961-340A-11D0-A96B-00C04FD705A2"></object>';
    document.body.insertAdjacentHTML('beforeEnd', sWebBrowserCode);
    var objWebBrowser = document.body.lastChild;
    if(objWebBrowser == null) return;
    objWebBrowser.ExecWB(7, 0);
    document.body.removeChild(objWebBrowser);
    }

//var flag=0;
//var TxtDateObj="";
//function DropCalendar(img)
//    {
////    C1WebCalendar1_Client.MinDate = new Date("1900/1/1");
////    C1WebCalendar1_Client.MaxDate = new Date("2999/1/1");
//    if (img.id == "ImgLimitDate"){
//        TxtDateObj = "TxtLimitDate";
//        flag=1;}
//    else if (img.id == "ImgStDate"){
//        TxtDateObj = "TxtStDate";
//        flag=2;}

//    var textbox = document.getElementById(TxtDateObj);
//        var d = new Date(textbox.value);
//        C1WebCalendar1_Client.UnSelectAll();
//    if (d != "NaN/NaN/NaN")
//        {
//        C1WebCalendar1_Client.DisplayDate = d;
//        C1WebCalendar1_Client.SelectDate(d);
//        }
//        C1WebCalendar1_Client.PopupSetting.Dock = c1_dock_bottomleft;
//        C1WebCalendar1_Client.PopupBeside(document.getElementById(TxtDateObj));
//    }

//function Calendar_SelChange(calendar, seltype, seldates)
//    {
//        var textbox = document.getElementById(TxtDateObj);
//        textbox.value = DateToString(calendar.SelectedDate);
//    }

// 日付の書式を調整する関数　yyyy/MM/dd
function DateToString(objDate)
{
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
function setBg(tr, color) {
  tr.style.backgroundColor = color;
}
function NowDateTime(TxtObj) {
    var d;
    var hh;
    var mm;
    var strDt;
    d = new Date();
    hh = d.getHours();
    mm = d.getMinutes();
    strDt = DateToString(d)
    document.getElementById(TxtObj).value = strDt;
    document.getElementById('DDListHourSt').selectedIndex =hh;
    document.getElementById('DDListMinuteSt').selectedIndex =mm;
}
function btnAddClick(){
    document.getElementById('BtnAddRecord').click();
}
// クライアントスクリプトからASP.NETイベントハンドラの呼び出し
function __doPostBack(eventTarget, eventArgument) {
      var theform = document.ctrl2
      theform.__EVENTTARGET.value = eventTarget
      theform.__EVENTARGUMENT.value = eventArgument
      theform.submit()
}