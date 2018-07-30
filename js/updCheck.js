/* データの更新チェック トップ画面用 */
/* eslint-disable no-console */

var svrdt;
var svrno;
var checkInterval;
var timer1;
var targetBtnMainObj;
var targetBtnReloadObj;
var targetInpObj1;
var targetInpObj2;
var ptyp;   /* 0:非ポストバック  1:ポストバック */
var jQ = jQuery;    /* c1.calenderで、旧jQueryを使用しているため、別名を定義する*/
var maInt = 30000;
/*var longInt = 181000;*/
var NCInt = 1500000;
var ajaxTO = 2000;
$(document).ready(function () {
    /* 1.ページの読み込みが始まる 2.HTMLの読み込みが終わる 3.$(document).readyが実行 4.画像など含めすべてのコンテンツの読み込みが終わる 5.$(window).loadが実行 */
    /* $(window).on("load", function () {   */
    /* window.console.log = function(i){return;};  運用時に有効にするとログをはかない */
    setObject(); /* オブジェクトの設定とタイマーの設定 */
    console.log(document.URL);
    console.log("Load - " + formatDate(new Date(), "yyyy/MM/dd HH:mm:ss"));

    // consoleが使えない場合は空のオブジェクトを設定しておく
    if (typeof window.console === "undefined") {
        window.console = {};
    }
    // console.logがメソッドでない場合は空のメソッドを用意する
    if ( typeof window.console.log !== "function" ){
        window.console.log = function(){};
    }
    // console.logがメソッドでない場合は空のメソッドを用意する
    if( typeof window.console.log !== "function" ){
        window.console.log = function(){};
    }
});
/* オブジェクトの設定とタイマー時間の処理 */
function setObject() {
    var dt = document.URL;
    if (dt.includes("main") === true) {
        targetBtnMainObj = $("#ctl00_CntplHolder_btnSubmit");
        targetBtnReloadObj = $("#ctl00_CntplHolder_btnSubmit");
        targetInpObj1 = $("#ctl00_CntplHolder_nwdt");
        targetInpObj2 = $("#ctl00_CntplHolder_nwdtno");
        checkInterval = maInt;
        ptyp = -1;
    /*} else {*/
    }
    svrdt = targetInpObj1.val();                /*表示している最新日付*/
    svrno = targetInpObj2.val();                /*表示しているデータのファイル番号*/
    if (svrdt.includes("NC") === true) {
        checkInterval=NCInt;                    /*NC の場合は、セッション維持だけなので25分間隔 認証チケットが30分のようだ・・・ */
    }
    timer1=setInterval(dtchk,checkInterval);
}
/* 非同期によるサーバーの最新日時の取得 */
var dtchk = function() {
    $("#imgloader").show();
    svrdt = targetInpObj1.val();                /*表示している最新日付*/
    svrno = targetInpObj2.val();                /*表示しているデータのファイル番号*/
    /* addMessage("イベントの先頭"); */
    console.log("------");
    console.log("ajax---- " + formatDate(new Date(),"yyyy/MM/ss HH:mm:ss") + " -> " + svrno);
    console.log("window.name = " + window.name);	/* デバッグ用 */
    jQ.ajax({
        async: true,
        type: "GET",            /* 省略可（省略時は"GET"）*/
        dataType: "text",       /* 省略可（省略時はMIMEタイプから推定） IISにMIMEの追加必要(IIS6) */
        scriptCharset: "utf-8",
        url: "./api/UpdCheck.ashx",
        data: "i=" + svrno,     /* クエリ */
        cache: false,           /* 省略可（省略時はtrue）*/
        timeout: ajaxTO         /* 省略可 */
    })
    /* 成功時 */
    .done(function (data, textStatus, jqXHR) {
        console.log("textStatus- " + formatDate(new Date(),"yyyy/MM/ss HH:mm:ss") + " -> " + textStatus);
        console.log("jqXHR- " + formatDate(new Date(),"yyyy/MM/ss HH:mm:ss") + " -> " + jqXHR);
        console.log("GetDate- " + formatDate(new Date(),"yyyy/MM/ss HH:mm:ss") + " -> " + data);
        if (data.includes("DataEr") === true) {
            console.log("DataError - SubButton Click - " + formatDate(new Date(),"yyyy/MM/ss HH:mm:ss"));
            clearInterval(timer1);
            targetBtnReloadObj.click();
        } else {
            if (compareDate(data)) {
                console.log("Update - MainButton Click - " + formatDate(new Date(),"yyyy/MM/ss HH:mm:ss"));
                if (ptyp === 0) {
                    clearInterval(timer1);      /* 非ポストバックの時のみクリアする */
                }
                targetBtnMainObj.click();
                /* document.forms[0].submit()   これでもポストバックできるようだ・・・*/
            }
        }
        $("#imgloader").hide();
    })
    /* 失敗時 */
    .fail(function (jqXHR, textStatus, errorThrown) {
        console.log("fail! " + jqXHR.statusText + ", status=" + jqXHR.status + ", Response=" + jqXHR.responseText);
        var res=jqXHR.responseText;
        if (res == null) {              /* エラーの場合は、undefinedになるので、判定する */
            $("#imgloader").hide();
        } else {
            /* セッションが切れている場合の処理 */
            if (res.includes("Unauthorized") === true && document.URL.includes("main") === true) {
                /* トップ画面のみの処理 */
                clearInterval(timer1);
                if (ptyp === -1) {
                    alert("セッションが切断されたため、更新ができませんでした。\n\nお手数をおかけいたしますが、再度ログインしてください。");
                    targetBtnMainObj.click();
                }
            } else {
                $("#imgloader").hide();
            }
        }
    });
};
/* 日付の大小チェック 複数ファイルの場合はdteは「,」区切りになる */
function compareDate(dte) {
    var src=0;
    var dst=0;
    var sDate;
    var getDate;
    var svrDate=svrdt.split(",");       /*保管日付*/
    var chkDate=dte.split(",");         /*取得日付(引数)*/
    var result = false;
    var i;
    console.log("URL -- " + document.URL);
    for (i=0; i < chkDate.length; i++){
        if (svrdt.includes("NC") === true) {
            console.log("No check site.");
            console.log("-"+formatDate(new Date(),"yyyy/MM/dd HH:mm:ss"));
            break;
        }
        /* 保管日付けのチェック*/
        var dtms=svrDate[i].split(" ");
        if (CheckDate(dtms[0]) && CheckTime(dtms[1])) {
            sDate = new Date(svrDate[i]);
            src=1;
        }
        /*取得日付のチェック*/
        var dtmg=chkDate[i].split(" ");
        if (CheckDate(dtmg[0]) && CheckTime(dtmg[1])) {
            getDate = new Date(chkDate[i]);
            dst=1;
        }
        /*日付けが正常なら、比較する*/
        if (src === 1 && dst === 1){
            console.log(i + " " + checkInterval);
            /*console.log("now----"+formatDate(new Date(),"yyyy/MM/dd HH:mm:ss"));*/
            console.log("Hidden-"+formatDate(sDate,"yyyy/MM/dd HH:mm:ss"));
            console.log("Get----"+formatDate(getDate,"yyyy/MM/dd HH:mm:ss"));
            if (sDate < getDate) {
                result=true;
                targetInpObj1.val([dte]);
                break;
                /*continue;*/
            }
        }
    }
    /* if (result === true) {
    setUpdTimer(checkInterval);
    /* clearInterval(timer1);      クリアしないとずっと残る postbackの場合readyが呼ばれないので、毎回再セットする*/
    /* timer1=setInterval("dtchk()",checkInterval); */
    /* } */
    return result;
}
/* 月日のチェック */
function CheckDate(YMD) {
    var arr = YMD.split("/");
    if (arr.length !== 3) {return false;}

    //const date = new Date(arr[0], arr[1] - 1, arr[2]);   //これだとAjaxMinimiferが通らない
    var date = new Date(arr[0], arr[1] - 1, arr[2]);
    if (arr[0] !== String(date.getFullYear()) || arr[1] !== ("0" + (date.getMonth() + 1)).slice(-2) || arr[2] !== ("0" + date.getDate()).slice(-2)) {
        return false;
    } else {
        return true;
    }
}
/* 時刻のチェック */
function CheckTime(HMS) {
    return HMS.match(/^([01]?[0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])$/) !== null;
}
/* 結果の格納 */
function addMessage(msg) {
    // メッセージの先頭に時刻を追加する（mm:ss.fff）
    var dt = new Date();
    var hour = ("0" + dt.getHours().toString()).substr(-2);
    var min = ("0" + dt.getMinutes().toString()).substr(-2);
    var sec = ("0" + dt.getSeconds().toString()).substr(-2);
    //var mSec = ("00" + dt.getMilliseconds().toString()).substr(-3);
    var time = hour + ":" + min + ":" + sec;     //+ "." + mSec;
    var s = time + " " + msg + "\n";
    /*$("#result").text(s);*/
}
/* 日付けフォーマット */
function formatDate(date, format) {
    format = format.replace(/yyyy/g, date.getFullYear());
    format = format.replace(/MM/g, ("0" + (date.getMonth() + 1)).slice(-2));
    format = format.replace(/dd/g, ("0" + date.getDate()).slice(-2));
    format = format.replace(/HH/g, ("0" + date.getHours()).slice(-2));
    format = format.replace(/mm/g, ("0" + date.getMinutes()).slice(-2));
    format = format.replace(/ss/g, ("0" + date.getSeconds()).slice(-2));
    format = format.replace(/SSS/g, ("00" + date.getMilliseconds()).slice(-3));
    return format;
}
function alertDialog(_options){
    var default_option = {
        title:"",
        body:"",
        close: function (){ return true; }
    };
    var options = $.extend(default_option, _options, {});
    var dom = $("<div />", { title: options.title, html: options.body });
    dom.dialog({
        modal: true,
        close: function(){
            var dom = $(this);
            dom.dialog("destroy");
            dom.remove();
            options.close();
        },
        buttons: [{
            text: "Ok",
            click: function(){
                $(this).dialog("close");
            }
        }
      ]
    });
}