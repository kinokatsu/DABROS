//C1Calendar Ver.
var TxtDataObj = "";
// ポップアップ
function DropCalendar(txt) {
    TxtDataObj = txt.id.replace("imgCal", "");
    var calendar = Sys.Application.findComponent("C1WebCalendar");
    var input = document.getElementById(TxtDataObj);
    calendar.unSelectAll();
    if (input.value != "") {
        var d = new Date(input.value);
        calendar.selectDate(d);
        calendar.set_displayDate(d);
        calendar.refresh();
    }
    calendar.popupBeside(input, C1.Web.UI.PositioningMode.bottomLeft);
}
// 日付選択後
function CalendarClosed() {
    var calendar = Sys.Application.findComponent("C1WebCalendar");
    var input = document.getElementById(TxtDataObj);

    var selDate = new Date(calendar.get_selectedDate().toDateString());
    var month = "0" + (selDate.getMonth() + 1);
    month = month.substr(month.length - 2, month.length);
    var dateNum = "0" + selDate.getDate();
    dateNum = dateNum.substr(dateNum.length - 2, dateNum.length);
    input.value = selDate.getFullYear() + "/" + month + "/" + dateNum;
}
