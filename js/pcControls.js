//定義
var timerchk;
var compFlg;
var counter;
var frame;
var wsize;
var genbaState;
//即時測定
function GWt() {
    $("#lnkBtn_submit").hide();
    $("#preloadimg").show();
    startOpe("RqGWt:0");
    counter = 8;
}
//オン・オフライン読込
function CRO() {
    showframe("現場の設定を取得中・・");
    $("#preloadimg1st").show();
    startOpe("RqCRo:0");
    counter = 8;
}
//オン・オフライン設定
function CWO() {
    var sendVal;
    sendVal = $("#OnOff").val();
    if(genbaState.substr(5)==sendVal){
        jAlert('現場の設定と同じため、設定変更を行いません。','設定が変更されていません'); 
        return false;
    }
    if(sendVal==-1){
        jAlert('自動収録する／しないを選択してください。','設定を確認してください'); 
        return false;
    }
    $("#lnkBtn_submit").hide();
    $("#preloadimg").show();
    startOpe("RqCWo:" + sendVal);
    counter = 8;
}
//即時測定
function SWa() {
    //if (this.console && typeof console.log != "undefined"){
    //    console.log("SWa           " + " event " + Date());
    //}
    $("#lnkBtn_submit").hide();
    $("#preloadimg").show();
    startOpe("RqSWa:0");
    counter = 8;
}
//収録時刻読込
function CYR() {
    showframe("現場の設定を取得中・・");
    $("#preloadimg1st").show();
    startOpe("RqCYR:0");
    counter = 8;
}
//収録時刻設定
function CYW() {
    var setData;
    var stDate = $("#stDate").val();
    var edDate = $("#edDate").val();
    var inter = $("#interval").val();
    var sendVal;
    if (stDate.length < 12) {
        alert('「収録開始時刻」の文字数が足りていません。');
        $("#stDate").focus();
        return false;
    };
    if (checkDate(stDate)==false){
        $("#stDate").focus();
        alert('「収録開始時刻」の形式が間違えています。');
        return false;
    };
    if (checkDate(edDate)==false){
        $("#edDate").val('999999999999')        // 終了日付が異常なら、終了日時を設定しないことにする
    };    
    var flg = isRange(inter,5,99999);
    if (flg == 1) {
        alert('「収録間隔」の設定が間違えています。[5～99999]までの間で設定してください。');
        $("#interval").focus();
        return false;
    };
    sendVal=(stDate + "," + edDate + "," + inter);
    if(genbaState.substr(5)==sendVal){
        jAlert('現場の設定と同じため、設定変更を行いません。','変更されていません'); 
        return false;
    }
    $("#lnkBtn_submit").hide();
    $("#preloadimg").show();
    startOpe("RqCYW:" + sendVal);
    counter = 8;
}
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
//測定時刻読込
function PRt() {
    showframe("現場の設定を取得中・・");
    $("#preloadimg1st").show();
    startOpe("RqPRt:1");
    counter = 8;
}
//測定時刻変更
function PWt() {
    window.scrollTo(0,300);
    var minVal;
    var hourVal;
    var sendVal;
    hourVal=getCheckState(0);        // 時刻取得
    if (typeof hourVal[0] === "undefined") {
        jAlert('時刻が１つも選択されていません。','設定を確認してください');
        return false;
    }
    minVal=getCheckState(1);         // 分取得
    if (typeof minVal[0] === "undefined") {
        jAlert('分が１つも選択されていません。','設定を確認してください');
        return false;
    }
    if (minVal[1] > 12) {
        if(window.confirm('測定間隔を5分以下にすると、データを収録出来ない場合があります。設定しますか？')){        
        } else {
            return false;        
        }
    }
    sendVal=hourVal[0] + minVal[0];
    if(genbaState.substr(5)==sendVal){
        jAlert('現場の設定と同じため、設定変更を行いません。','変更されていません'); 
        return false;
    }
    $("#lnkBtn_submit").hide();
    $("#preloadimg").show();
    startOpe("RqPWt:" + sendVal);
    counter = 8;
}
//---------------------------------↓ 非同期通信
function startOpe(sendComm){
    $("#order").html("");
    $("#resSite").text("");
    //console.log("startOpe      " + " event " + Date());
    compFlg=0;
    //$("#preloadimg").show();
    CommandSend(sendComm);
    //timerchk = setTimeout("CommandReceive();" , 5000);
    timerchk = setTimeout(function() {CommandReceive();}, 5000);
}
// 応答の受信
function CommandReceive() {
    //console.log("CommandReceive" + " event" + Date());
    //window.scrollTo(0,0);           // TOPへ戻す
    //$('body,html').animate({ scrollTop: 0 }, 1); 
    Control.SitePC.ControlFile.Confirm("0", onComplete, onError, "#resSite"); 
}
// 命令ファイルの作成
function CommandSend(sendcom) {
    //console.log("CommandSend   " + " event " + Date());
    // Add() は引数を 1 つとるため、 "yamada" は GetSample() に渡されます。
    // (以下で指定した引数の頭から順に、作成した WebMethod の引数分だけ渡されます。)
    Control.SitePC.ControlFile.Add(sendcom, onComplete, onError, "#order");
}
// 関数の呼び出しが成功した場合(応答受信)の後処理
function onComplete(result, context)
{
    //console.log("comgFlg=     "+compFlg + " event " + Date());
    if(context=="#order"){
        $("#resSite").text("現場からの応答を記載します[" + counter + "]");
    }
    if(result.indexOf("RCVNG")!=-1){
        $("#resSite").text("");
    }
    var resText;
    var addCount;

    if(context!="#order" && result.indexOf("No")==-1){
        addCount="[" + counter + "]";
    } else {
        addCount=""
    }

    resText = result.replace("[RCVNG]","");   // + addCount;
    resText = resText.replace("[RCV]","");   // + addCount;

    if(resText.indexOf("|")!=-1){
        var rep;
        rep = resText.split("|");
        // =======================================       
        // 描画処理を追加 測定時刻、収録時刻の場合
        // =======================================
        genbaState=rep[1];
        if(resText.indexOf("PRt")!=-1){
            var hourVal;
            var minVal;
            //genbaState=rep[1];
            hourVal=rep[1].substr(5,24);
            minVal=rep[1].substr(29,60);
            setCheck(0,hourVal);
            setCheck(1,minVal);
        } else if(resText.indexOf("CYR")!=-1) {
            var dt=rep[1].substr(5).split(",");
            $("#stDate").val(dt[0]);
            $("#edDate").val(dt[1]);
            $("#interval").val(dt[2]);
            //genbaState=rep[1];
        } else if(resText.indexOf("CRo")!=-1||resText.indexOf("CWo")!=-1) {
            var state;
            var msg = "自動収録：する";
            var col = "#d2d2ff"
            state = rep[1].substr(5,1);
            if(state=="0"){
                msg = "自動収録：しない";
                col = "#ffa2a2";
            }
            $("#status").text(msg);
            $("#status").css("background-color", col)
        }
        $(context).html(rep[0]);
    
    } else {
        $(context).html(resText);
    }

    // 受信文字列チェックとタイマー停止
    //console.log("result="+result + " event " + Date());
    
    if(result.indexOf("RCV")!=-1){
        compFlg=1;
        $("#preloadimg").hide();
        if ($("#blockframe")[0]) {
            frame.parentNode.removeChild(frame);        // ボックス消去
        }
        $("#preloadimg1st").hide();        
        // clearInterval(timerchk);
        clearTimeout(timerchk);
        //console.log("comgFlg=     "+compFlg + " event " + Date());
        if(result.indexOf("正常")==-1){
            $("#lnkBtn_submit").show();
        }
    } else {
        if(counter==0){
            $("#preloadimg").hide();
            frame.parentNode.removeChild(frame);    // ボックス消去
            $("#preloadimg1st").hide();
            $(context).html("現場から応答がありませんでした");
            $("#lnkBtn_submit").show();
        } else if (context!="#order") {
            //console.log("setTimeout 20sec" + " event " + Date());
            timerchk = setTimeout("CommandReceive();",15000);        
        }
    }
    if(context!="#order"){
        counter--;
    }
}

// 関数の呼び出しが失敗した場合
function onError(error)
{
    alert(error.get_message());
    if(counter!=0 || compFlg!=1){
        timerchk = setTimeout("CommandReceive();",10000);
    }
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
                .prev('input').attr('checked',true);
                //.prev('input').removeAttr('checked')
                
        } else {
            $(this)
                .addClass('checked')
                //.prev('input').prop({'checked':'checked'});
                .prev('input').attr('checked',false);
        }
    });
});
//チェック状態を取得して 0,1で返す
function getCheckState(hm) {
    var digit="";
    var checkCount =0;
    var retVal;
    if (hm==0) {
        var list = $('#chbHour input');    
    }else{
        var list = $('#chbMin input');    
    }
    for (i=0;i<list.length;i++) {
        if (list[i].checked) {
            digit = digit + "1"     ;i + ",";  //list[i].id;
            checkCount++;
        } else {
            digit = digit + "0"
        }
    }
    //alert(index);
    if (checkCount==0) {
        digit=undefined;
    }
    return [digit,checkCount];
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
    for (i=0;i<=strLen;i++) {
        strRet+=setVal;
    }
    return strRet;
}
//指定のIndexにチェック・未チェックをセットする
function setCheck(hm,setVal) {
    var setValLen;
    var checkVal=0;
    var strLen=0;
    if (hm==0) {
        var list = $('#chbHour input');    
        strLen=23;
    }else{
        var list = $('#chbMin input');    
        strLen=59;
    }
    //文字列が１つだったら、それを指定個数並べる
    setValLen=String(setVal);
    if (setValLen.length==1) {
        setVal=stringRepaet(hm,setVal,strLen)
    }
    for (i=0;i<setVal.length;i++) {
        checkVal=parseInt(setVal.charAt(i));
        list[i].checked=Boolean(checkVal);
        if (checkVal==0) {
            $(list[i]).next().removeClass('checked');
        } else {
            $(list[i]).next().addClass('checked');
        }
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
        st = parseInt($('#stHour').val());  
        interv = parseInt($('#intervH').val());  
        maxVal = parseInt(23);  
        list = $('#chbHour input');
    }else{
        st = parseInt($('#stMin').val());  
        interv = parseInt($('#intervM').val());  
        maxVal = parseInt(59);  
        list = $('#chbMin input');
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
    getsize= function(node) {
        return { dwidth : node.clientWidth, dheight : node.clientHeight, width : node.scrollWidth, height : node.scrollHeight };
    };
    wsize = getsize(document.documentElement);
    frame = document.body.appendChild(document.createElement("div"));
    if (frame.style.setExpression) { //IE
      frame.style.position = "absolute";
//      frame.style.setExpression('top', 'y=(document.documentElement.scrollTop)+"px"');
//      frame.style.setExpression('left', 'x=(document.documentElement.scrollLeft)+"px"');
    } else {
      frame.style.position = "fixed";
    }
    frame.className = 'blockframe';
    frame.id="frameArea";
    frame.style.left = "0px";
    frame.style.top = "0px";
    frame.style.width = wsize.dwidth + "px";
    frame.style.height = (wsize.dheight + 200) + "px";
    frame.style.filter = "alpha(opacity=50)";
    var divNode = document.createElement('div');
    frame.appendChild(divNode);    
    divNode.id="blockframe";
    divNode.className='innerframe';
    divNode.style.position = "fixed";
    divNode.style.left = "0px";
    divNode.style.top = "0px";
    divNode.style.width = wsize.dwidth + "px";
    divNode.style.height = wsize.dheight + "px";
    divNode.style.filter = "alpha(opacity=60)";    
    var imgNode = document.createElement('img');
    imgNode.id="aniload2nd";
    imgNode.src="img/loader4.gif"
    divNode.appendChild(imgNode);
    var SpanNode = document.createElement('span');
    var textNode = document.createTextNode(setVal);
    SpanNode.appendChild(textNode);
    divNode.appendChild(SpanNode);
};
//    window.onresize = function() {
    $(window).resize(function(){
        var wsize = getsize(document.documentElement);
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
