// wijmo Calendar Ver.
var TxtDateObj = "";
$(document).ready(function () {
    // イメージのクリックイベントを指定
    $("img[id*='imgCal']").click(function () {
        DropCalendar(this);
    });
});

function initCalendar(){
    $("#C1WebCalendar").c1calendar(
            {
                culture: 'ja-JP',
                titleFormat: '- yyyy年MM月 -',
                //easing: 'easeInCubic',
                //duration: 250,
                //direction: 'horizontal',
                selectionMode: { day: true, days: false },
                weekDayFormat: 'short',
                allowQuickPick: true,
                quickNavStep: 6,
                quickNextTooltip: '６ヶ月先へ',
                quickPrevTooltip: '６ヶ月前へ',
                nextPreviewTooltip: '１ヶ月先へ',
                prevPreviewTooltip: '１ヶ月前へ',
                navButtons: 'quick',
                toolTipFormat: 'yyyy年MMMMdd日',
                popupMode: false,
                autoHide: false,
                showOtherMonthDays: true,
                selectedDatesChanged: function () {
                    //var selDate = new Date($(this).c1calendar("getSelectedDate").toLocaleDateString());
                    var selDate = new Date($(this).c1calendar("getSelectedDate").toDateString());
                    var month = selDate.getMonth() + 1;
                    if (!!selDate) $(TxtObjName).val(selDate.getFullYear() + "/" + month + "/" + selDate.getDate());
                    $("TxtObjName").val(selDate);
                }
//                selectedDatesChanged: function () {
//                    var selDate = new Date($(this).c1calendar("getSelectedDate").toDateString());
//                    var month = "0" + (selDate.getMonth() + 1);
//                    month = month.substr(month.length - 2, month.length);
//                    var dateNum = "0" + selDate.getDate();
//                    dateNum = dateNum.substr(dateNum.length - 2, dateNum.length);
//                    //if (!!selDate) $(TxtDateObj).val(selDate.getFullYear() + "/" + month + "/" + selDate.getDate());
//                    if (!!selDate) $(TxtDateObj).val(selDate.getFullYear() + "/" + month + "/" + dateNum);
//                    $("TxtDateObj").val(selDate);
            });
};

function DropCalendar(img) {
    //イメージのIDからテキストボックスのIDを生成
    TxtDateObj = img.id.replace("imgCal", "#");

    //カレンダーから全ての選択されている日付を解除
    $("#C1WebCalendar").c1calendar("unSelectAll");

    //現在入力されている日時を取得
    var getDate = $(TxtDateObj).val();
    if (getDate != "") {
        var d = new Date(getDate);
        $("#C1WebCalendar").c1calendar("selectDate", d);
        $("#C1WebCalendar").c1calendar({ displayDate: d });
    }

    //カレンダーをポップアップ
    $("#C1WebCalendar").c1calendar("popup", {
        of: $(TxtDateObj),
        offset: '0 2'
    });
}

Sys.Application.add_load(initCalendar);