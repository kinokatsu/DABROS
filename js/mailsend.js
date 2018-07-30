/*ブラウザを閉じたり、ログアウト、現場切替時に確認表示とグラフなどを閉じる*/
$(window).on("beforeunload", function (e) {
    return "メールの送信をやめて、入力フォームを閉じますか？\n\n";
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
//クリアボタン
function clearitems() {
    var result = confirm('入力した内容を削除しますか？');

    if (result) {
        $("#ConstructionSite").val("");
        $("#Name").val("");
        $("#EMailAddress").val("");
        $("#RqText").val("");
    }
    return false;
}
//キャンセルボタン
function cancel() {
    /*window.open(location, '_self').close();*/
    window.close();
    return false;
}
//同意して送信ボタン
function sendEMail($this) {
    var genba = $("#ConstructionSite").val();
    var name = $("#Name").val();
    var Eadd = $("#EMailAddress").val();

    if (genba === "") {
        alert("現場名を入力してください。");
        return;
    };
    if (name === "") {
        alert("ご担当者名を入力してください。");
        return;
    };
    if (Eadd === "") {
        alert("E-Mailアドレスを入力してください。");
        return;
    };
    $(window).off("beforeunload");
    $("#btnSubmit").click();
    $(window).on("beforeunload");
    /*return false;*/
}
//テキストボックスの文字数
function alertValue($this) {
    var wc;
    wc = $this.value.length;
    $("#wordct").val(wc);
    document.getElementById("wordct").innerHTML = "（現在の文字数:" + wc + "）";
    /* $this.nextSibling.innerHTML = $this.value; */
}