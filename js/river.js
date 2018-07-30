/*jQuery(document).ready(function () {  こっちは、UpdatePanelでは実行されない*/
var ajaxTO = 2000;
function pageLoad() {
    this.window.name = "dabrosRivertop";

    /*liオブジェクトにクリックイベントを付ける*/
    $("li[id^=obsArr]").click(function () {
        var eid = $(this).attr("eqid");
        GraphOpen(eid);
    });
//    $("div[id^=btn]").click(function () {
//        var enlarge = $(this).attr("fn");
//        $("#innerMap").css({ "transform-origin": "center", "transform": "scale(" + enlarge + ")" });        /*"top left"*/
//    }).on("transitionend", function () {
//        $(this).css({
//            transform: ""
//        }).off("transitionend");
//    });
    /*マップのドラッグ移動プラグイン呼出し*/
    var viewer = new ScrollViewer("#ctl00_CntplHolder_map","#ctl00_CntplHolder_topimg");

    /*トップへ戻る*/　
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
    /*起動時のフェードイン呼出し*/
    windowFade();
    /*dtchk();*/

    var mousewheelevent = 'onwheel' in document ? 'wheel' : 'onmousewheel' in document ? 'mousewheel' : 'DOMMouseScroll';
    /*DOMMouseScroll*/
    $("#lowertd").on(mousewheelevent,function(e){
        /*var num = parseInt($('.wheel').text());*/
        var dNum = parseFloat($("#ctl00_lblNum").text());
        e.preventDefault();
        var delta = e.originalEvent.deltaY ? -(e.originalEvent.deltaY) : e.originalEvent.wheelDelta ? e.originalEvent.wheelDelta : -(e.originalEvent.detail);
        if (delta < 0){
            dNum = dNum - 0.025;
            /*$('.wheel').html(dNum)*/
            if (dNum < 0.5){dNum = 0.5;}
        } else {
            dNum = dNum + 0.025;
            if (dNum > 1){dNum = 1;}
            /*$('.wheel').html(uNum)*/
        }
        $("#ctl00_txtSlider").val(dNum+"").change();
        $("#ctl00_lblNum").text(dNum+"");
        /*スライダーのポインターを強制的に移動*/
        var loc_left = ((dNum - 0.5) / 0.025) * 3;
        $(".ajax__slider_h_handle").css("left",loc_left+"px");
    });

}
/*スライダーによる縮小表示*/
$("#ctl00_txtSlider").change(function() {
    var enlarge = $(this).val();
    $("#ctl00_CntplHolder_innerMap").css({ "transform-origin": "top left", "transform": "scale(" + enlarge + ")" });
}).on("transitionend", function () {
    $(this).css({
        transform: ""
    }).off("transitionend");
});


/*読み込み完了後に、個別にテーマを指定するようにした*/
window.onload = function () {
    // ここに読み込み完了時に実行する内容を記述
    //    Array.from(document.querySelectorAll('[data-tippy]'), el => console.log(el.id))
    //    Array.from(document.querySelectorAll("li[title]"), function (el) {
    //        console.log(el.id);
    //        el._tippy.options.theme = "WLCommon tooltipWL2";
    //    });
    var ret = setToolTip();
};
/*tooltipの設定*/
function setToolTip (){
    var alertstate=0;
    var userAgent = window.navigator.userAgent.toLowerCase();
    $("li[id^=obsArr]").each(function () {
        var mid = $(this).attr("id");       /*DOMのID*/
        var ALv = $(this).attr("alv");      /*管理値レベル*/
        if (alertstate === 0 && ALv > 0) {  /*管理値を超過していた場合は、警戒状況を表示する*/
            alertstate = ALv;
            $("#alertSt").removeClass('invisible');
        }
        /*alert(eid);*/
        // if(userAgent.indexOf('msie') != 1 || userAgent.indexOf('trident') != 1 ) {
        //     /*IEの場合*/
        //     tippy("#" + mid, {
        //         position: "top",
        //         animation: "shift",
        //         duration: 300,
        //         html: true,
        //         arrow: false,
        //         animateFill: false,
        //         theme: "WLCommon tooltipWL" + ALv,
        //         inertia: true,
        //         followCursor: false,
        //         performance: true,
        //         offset: 20,
        //         distance: 15
        //     });
        // } else {
            /*IE以外*/
            tippy.one("#" + mid, {
                placement: "top",
                animation: "shift-away",
                duration: 300,
                arrow: false,
                animateFill: false,
                theme: "WLCommon tooltipWL" + ALv,
                inertia: true,
                followCursor: false,
                performance: true,
                offset: 20,
                distance: 15
            });
        // }
    });
    if (alertstate === 0) {
        $("#alertSt").addClass("invisible");     /*警戒値を超えてない場合は、警戒状況を非表示にする*/
        }
}

/*グラフウィンドウの表示*/
function GraphOpen(eid) {
    /*window.open("Graph_WLTimeChart.aspx?id=" + eid, "rivGr", "width=1000, height=890, menubar=no, toolbar=no, scrollbars=yes");*/
    /*window.open("WaterLevel/"+eid, "rivGr", "width=1000, height=890, menubar=no, toolbar=no, scrollbars=yes");*/
    window.open(eid + "/WaterLevel" , "rivGr", "width=1000, height=890, menubar=no, toolbar=no, scrollbars=yes");
}
/*ブラウザを閉じたり、ログアウト、現場切替時に確認表示とグラフなどを閉じる*/
$(window).on("beforeunload", function (e) {
    return "現場を切り替えますか？\n\n表示していた図や表を閉じてください。\n\n（閉じない場合は、エラーが表示される場合があります）\n\n";
});
/* submit時は無効*/
$("input[type=submit]").on("click", function () {
    $(window).off("beforeunload");
});
/* F5キー押下時は無効*/
$(window).keydown(function (e) {
    if (e.keyCode === 116) $(window).off("beforeunload");
});
// ページ表示時のフェードイン、ページ遷移時のフェードアウト 読み込み時は白いページにする
$("head").append("<style type='text/css'>#ctl00_CntplHolder_MainContent{display:none;}</style>");
function windowFade() {
    $("#ctl00_CntplHolder_MainContent").delay(300).fadeIn("500");
}


/* 非同期によるサーバーの最新日時の取得 */
var dtchk = function () {
    $.ajax({
        async: true,
        type: "POST",            /* 省略可（省略時は"GET"）*/
        dataType: "json",       /* 省略可（省略時はMIMEタイプから推定） IISにMIMEの追加必要(IIS6) */
        scriptCharset: "utf-8",
        url: "./api/datarq_WL.ashx",
        data: JSON.stringify([{"age":18,"name":"mogeri",}]),
        cache: false,           /* 省略可（省略時はtrue）*/
        timeout: ajaxTO         /* 省略可 */
    })
    /* 成功時 */
    .done(function (data, textStatus, jqXHR) {
        console.log("textStatus- " + formatDate(new Date(), "yyyy/MM/ss HH:mm:ss") + " -> " + textStatus);
        console.log("jqXHR- " + formatDate(new Date(), "yyyy/MM/ss HH:mm:ss") + " -> " + jqXHR);
        console.log("GetDate- " + formatDate(new Date(), "yyyy/MM/ss HH:mm:ss") + " -> " + data);
        if (data.includes("DataEr") === true) {
            console.log("DataError - SubButton Click - " + formatDate(new Date(), "yyyy/MM/ss HH:mm:ss"));
            clearInterval(timer1);
            targetBtnReloadObj.click();
        } else {
            if (compareDate(data)) {
                console.log("Update - MainButton Click - " + formatDate(new Date(), "yyyy/MM/ss HH:mm:ss"));
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
        var res = jqXHR.responseText;
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