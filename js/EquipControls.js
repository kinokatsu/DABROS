//定義
var timerchk;
var compFlg;
//var counter;
var frame;
var wsize;
var genbaState;
//yyyymmddhhnn のデータが日時として正しいか確認 NG:False
function checkDate(dateStr) {
    var yy = dateStr.substr(0,4);
    var mm = dateStr.substr(4,2);
    var dd = dateStr.substr(6,2);
    var hh = dateStr.substr(8,2);
    var nn = dateStr.substr(10,2);
    dt=new Date(yy,mm-1,dd,hh,nn,'00');
    return(dt.getFullYear()==yy && dt.getMonth()==mm-1 && dt.getDate()==dd &&
            dt.getHours()==hh && dt.getMinutes()==nn);
}
//----接点状態読込(GetStatusContact)
function GSc(ID) {
    setCheck("000000")          // 状態リセット
    if (ID=="Undefind"){
        $('#lnkBtn_submit').hide()
     }else {
        showframe("現場機器の状況を取得中・・");
        $("#preloadimg1st").show();
        startOpe("RqGSc:" , ID);
        //counter = 1;
    }
}
//----接点状態設定(SetContactStatus)
function SCs() {
    var LogID = $("#DDLMeasPoint").val();
    var st =getCheckState()
    showframe("現場機器の制御と応答の待機中・・");
    $("#preloadimg1st").show();
    startOpe("RqSCs:",LogID ,st);
    //counter = 8;
}
//---------------------------------↓ 非同期通信
// 接点状態取得
function startOpe(sendComm,ID,st){
    $("#order").html("");
    $("#resSite").text("");
    //console.log("startOpe      " + " event " + Date());
    compFlg=0;
    var cmd
    if (st=="") {
        cmd = sendComm + ID
    } else {
        cmd = sendComm + ID + ":" + st
    }
    CommandSend(cmd);
    timerchk = setTimeout(function() {CommandReceive(ID);}, 5000);
}
// ----応答の受信
function CommandReceive(ID) {
    //console.log("CommandReceive" + " event" + Date());
    //window.scrollTo(0,0);           // TOPへ戻す
    //$('body,html').animate({ scrollTop: 0 }, 1);
    Control.SiteEquip.ControlEquip.Confirm(ID, onComplete, onError, "#resSite"); 
}
// ----命令ファイルの作成
function CommandSend(sendcom) {
    //console.log("CommandSend   " + " event " + Date());
    // Add() は引数を 1 つとるため、 "yamada" は GetSample() に渡されます。
    // (以下で指定した引数の頭から順に、作成した WebMethod の引数分だけ渡されます。)
    Control.SiteEquip.ControlEquip.Add(sendcom, onComplete, onError, "#order");
}
// ----関数の呼び出しが成功した場合(応答受信)の後処理　//
function onComplete(result, context)
{
    //console.log("comgFlg=     "+compFlg + " event " + Date());
    if(context=="#order"){
        $("#resSite").text("現場からの応答を記載します"); //[" + counter + "]");
    }
    if(result.indexOf("RCVNG")!=-1){
        $("#resSite").text("");
    }
    var resText;

    resText = result.replace("[RCVNG]","");   // + addCount;
    resText = resText.replace("[RCV]","");
    resText = resText.replace("[CMDNG]","");

    // 受信文字列チェックとタイマー停止
    //console.log("result="+result + " event " + Date());

    if(result.indexOf("RCVNG")!=-1){
        //if(counter==0){
            dellPreloader()
            $(context).html("現場から応答がありませんでした");
            $("#lnkBtn_submit").show();
        //} else if (context!="#order") {
            //console.log("setTimeout 20sec" + " event " + Date());
            //timerchk = setTimeout("CommandReceive();",15000);
        //}
    } else if(result.indexOf("RCV")!=-1) {

        if(resText.indexOf("|")!=-1){
            var rep;
            rep = resText.split("|");
            // =======================================
            // 描画処理を追加 測定時刻、収録時刻の場合
            // =======================================
            genbaState=rep[2];
            if(resText.indexOf("GCs")!=-1|resText.indexOf("SCs")!=-1){
                setCheck(genbaState);
                $("#lnkBtn_submit").show();
//            } else if(resText.indexOf("aaa")!=-1) {
//                setCheck(genbaState);
            }
            $(context).html(rep[0]);

        } else {
            $(context).html(resText);
        }
        dellPreloader();
        compFlg=1;

        // clearInterval(timerchk);
        clearTimeout(timerchk);
        //console.log("comgFlg=     "+compFlg + " event " + Date());
        if(result.indexOf("正常")==-1){
            $("#lnkBtn_submit").show();
        }
    } else if(result.indexOf("CMDNG")!=-1) {
        $(context).html(resText);
        dellPreloader();
    }
}

//---- 関数の呼び出しが失敗した場合
function onError(error)
{
    alert(error.get_message());
    //if(counter!=0 || compFlg!=1){
    $('#lnkBtn_submit').hide();
    $("#resSite").html("エラーが発生したため、処理を行えませんでした。");
    dellPreloader();
}
function dellPreloader() {
    $("#preloadimg").hide();
    if ($("#blockframe")[0]) {
        frame.parentNode.removeChild(frame);        // ボックス消去
    }
    $("#preloadimg1st").hide();
}
//---------------------------------↑
// IE8向けのラベル チェック属性
$(function(){
    //checkedだったら最初からチェックする
    $('div.checkbox-group input').each(function(){
        //if ($(this).attr('checked') == 'checked') {
        if (this.checked==true) {
            $(this).next().addClass('checked');
        }
    });
    //クリックした要素にクラス割り当てる
    $('div.checkbox-group label').click(function(){
        if ($(this).hasClass('checked')) {
            $(this)
                .removeClass('checked')
                //.prev('input').prop({'checked':false});
                .prev("input").attr("checked",true);
                //.prev('input').removeAttr('checked')

        } else {
            $(this)
                .addClass('checked')
                //.prev('input').prop({'checked':'checked'});
                .prev("input").attr("checked",false);
        }
    });
});
//チェック状態を取得して 0,1で返す
function getCheckState() {
    var digit="";
    var i;
    var list = $("#chbContact input");
    for (i=0;i<list.length;i++) {
        if (list[i].checked) {
            digit = digit + "1";
        } else {
            digit = digit + "0";
        }
    }
    var addDigit;
    if ($("#ChbDisable").attr("checked")) {
        addDigit=1
    } else {
        addDigit=0
    }
    digit += addDigit
    return [digit]      //,checkCount];
};
//All unSelect
//function allReset(hm) {
//    if (hm==0) {
//        var list = $('#chbHour input');
//    }else{
//        var list = $('#chbMin input');
//    }
//    $(list).each(function(){
//        this.checked=false;
//        $(this).next().removeClass('checked');
//    })
//}
//All Select
//function allSet(hm) {
//    if (hm==0) {
//        var list = $('#chbHour input');
//    }else{
//        var list = $('#chbMin input');
//    }
//    $(list).each(function(){
//        this.checked=true;
//        $(this).next().addClass('checked');
//    })
//}
// 指定文字を指定個数並べる
function stringRepaet(hm,setVal,strLen) {
    var strRet="";
    var i;
    for (i=0;i<=strLen;i++) {
        strRet+=setVal;
    }
    return strRet;
}
//指定のIndexにチェック・未チェックをセットする
function setCheck(setVal) {
    var setValLen;
    var checkVal=0;
    var strLen=0;
    var i;
    var list = $("#chbContact input");

    //文字列が１つだったら、それを指定個数並べる
//    setValLen=String(setVal);
//    if (setValLen.length==1) {
//        setVal=stringRepaet(hm,setVal,strLen)
//    }
    for (i=0;i<5;i++) {
        checkVal=parseInt(setVal.charAt(i));
        list[i].checked=Boolean(checkVal);
        $(list[i]).next().removeClass("checked");
        if (checkVal==0) {
            $(list[i]).next().removeClass("checked");
        } else {
            $(list[i]).next().addClass("checked");
        }
    }
    //$("#ChbDisable").checked = setVal.charAt(5)
    if (setVal.charAt(5)==0) {
        $("#ChbDisable").attr("checked",false);
        //$('#ChbDisable').prop("checked",true);
    } else {
        $("#ChbDisable").attr("checked",true);
    }
}
//設定補助
function setInterval(hm) {
    var st;
    var interv;
    var list;
    var flg = 0;
    var maxVal=0;
    if (hm==0) {
        st = parseInt($("#stHour").val());
        interv = parseInt($("#intervH").val());
        maxVal = parseInt(23);
        list = $("#chbHour input");
    }else{
        st = parseInt($("#stMin").val());
        interv = parseInt($("#intervM").val());
        maxVal = parseInt(59);
        list = $("#chbMin input");
    }
    flg = isNumeric(st);　
    flg +=isNumeric(interv);
//    if (isFinite(st)==false){
//        flg = 1;
//    } else if (isFinite(interv)==false){
//        flg = 1;
//    }
    if (flg){
        jAlert('未入力、もしくは数字以外が入力されています。確認してください。','入力値を確認してください'); // 数字以外が入力された場合は警告ダイアログを表示
        return false;
    }
    flg=0;
    flg = isRange(st,0,maxVal)
    flg += isRange(interv,1,maxVal)
    if (flg){
        jAlert('範囲外の数字が入力されています(マイナスの数値など)。確認してください。','入力値を確認してください'); // 数字以外が入力された場合は警告ダイアログを表示
        return false;
    }

    //allReset(hm);
    setCheck(hm,0);
    vai i;
    for (i=st;i<list.length;i+=interv) {
        list[i].checked=true;
        $(list[i]).next().addClass('checked');
    }
//    while(i<=maxIndex) {
//        list[i].checked=true;
//        i = i +interv;
//    }
}
// 数値であるか判定する
function isNumeric(checkVal){
    var flg=0;
    if (isFinite(checkVal)==false){
        flg = 1;
    }
    return flg;
}
// 指定範囲に入っているか判定する
function isRange(checkVal,MinVal,MaxVal){
    var flg=0;
    if (checkVal>MaxVal || checkVal<MinVal) {
        flg=1;
    }
    return flg;
}
//操作不可用のボックス表示
function showframe(setVal) {
    var getisze;
    getbrsize= function(node) {
        return { dwidth : node.clientWidth, dheight : node.clientHeight, width : node.scrollWidth, height : node.scrollHeight };
    };
    wsize = getbrsize(document.documentElement);
    frame = document.body.appendChild(document.createElement("div"));
    if (frame.style.setExpression) { //IE
      frame.style.position = "absolute";
//      frame.style.setExpression('top', 'y=(document.documentElement.scrollTop)+"px"');
//      frame.style.setExpression('left', 'x=(document.documentElement.scrollLeft)+"px"');
    } else {
      frame.style.position = "fixed";
    }
    frame.className = "blockframe";
    frame.id="frameArea";
    frame.style.left = "0px";
    frame.style.top = "0px";
    frame.style.width = wsize.dwidth + "px";
    frame.style.height = (wsize.dheight + 200) + "px";
    frame.style.filter = "alpha(opacity=50)";
    var divNode = document.createElement("div");
    frame.appendChild(divNode);
    divNode.id="blockframe";
    divNode.className="innerframe";
    divNode.style.position = "fixed";
    divNode.style.left = "0px";
    divNode.style.top = "0px";
    divNode.style.width = wsize.dwidth + "px";
    divNode.style.height = wsize.dheight + "px";
    divNode.style.filter = "alpha(opacity=60)";
    var imgNode = document.createElement("img");
    imgNode.id="aniload2nd";
    imgNode.src="img/loader4.gif";
    divNode.appendChild(imgNode);
    var SpanNode = document.createElement("span");
    var textNode = document.createTextNode(setVal);
    SpanNode.appendChild(textNode);
    divNode.appendChild(SpanNode);
};
//    window.onresize = function() {
    $(window).resize(function(){
        var wsize = getbrsize(document.documentElement);
        frame.style.width = wsize.dwidth + "px";
        frame.style.height = (wsize.dheight + 200) + "px";
        if ($("#blockframe")[0]) {
            $("#blockframe").width(wsize.dwidth);
            $("#blockframe").height(wsize.dheight + 200);
        }
//    }
    });
// 入力文字制限
function ime(){
    // 数字入力対応（入力可能文字：0～9 , 0.1 , -1など）
    $(".numeric").numeric();
     // 整数入力対応（入力可能文字：0～9 , -1など）
    $(".integer").numeric({
        decimal: false
    });
    // 正の数値（入力可能文字：0～9 , 0.1など）
    $(".positive").numeric({
        negative: false
    });
    // 正の整数（入力可能文字：0～9のみ）
    $(".positive-integer").numeric({
        decimal: false,
        negative: false
    });
    // 全角文字入力対応
    $(".antirc").change(function() {
        $(this).keyup();
    });
    // *「右クリック貼り付け」対応
    $(".antirc").change(function() {
        $(this).keyup();
    });
};
