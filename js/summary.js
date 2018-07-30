function pageLoad() {
    if (document.getElementById("RdBtnList_0").checked) {
        document.getElementById('DDLhour').disabled=false;
    }
    else{
        document.getElementById('DDLhour').disabled=true;
    }
}  
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
function ChangeState(Num){
        if (Num == 0) {
            document.getElementById('DDLhour').disabled=false;
            }
        else{
            document.getElementById('DDLhour').disabled=true;
        }
    }
function DropCalendar(img)
    {
        var textbox=document.getElementById("TxtStartDate");
        var d = new Date(textbox.value);
        C1WebCalendar1_Client.UnSelectAll();
    if (d != "NaN/NaN/NaN")
        {
        C1WebCalendar1_Client.DisplayDate = d;
        C1WebCalendar1_Client.SelectDate(d);
        }        
        C1WebCalendar1_Client.PopupSetting.Dock = c1_dock_bottomleft;
        C1WebCalendar1_Client.PopupBeside(textbox);
    }
function Calendar_SelChange(calendar, seltype, seldates)
    {
        var textbox=document.getElementById("TxtStartDate");
        textbox.value = DateToString(calendar.SelectedDate);
    }
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
