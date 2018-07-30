<%@ Page Language="VB" AutoEventWireup="false" CodeFile="AsyncTest2.aspx.vb" Inherits="AsyncTest2" %>
<%@ Register Src="UserControl/wds.ascx" TagName="wds" TagPrefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>非同期通信テスト Handler</title>
    <link href="CSS/stl_AsyncCom.css" rel="stylesheet" type="text/css" />
    <link href="CSS/stl_wd.css" rel="stylesheet" type="text/css" />
    <link href="CSS/stl_jquery.alerts.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="js/jquery-3.0.0.min.js"></script>
    <script type="text/javascript" src="js/jQueryRotate.js"></script>
<%--    <script type="text/javascript" src="js/tweenjs-NEXT.min.js"></script>
    <script type="text/javascript" src="js/plugin/RotationPlugin.js"></script>
    <script type="https://code.createjs.com/1.0.0/createjs.min.js"></script>--%>
    <script type="text/javascript">
        function showClock1() {
           var nowTime = new Date();
            var year = nowTime.getFullYear(); // 年
            var month = nowTime.getMonth()+1; // 月
            var date = nowTime.getDate(); // 日
            var nowHour = nowTime.getHours();
            var nowMin = nowTime.getMinutes();
            var nowSec = nowTime.getSeconds();
            // 数値が1桁の場合、頭に0を付けて2桁で表示する指定
            nowHour = ("0" + nowHour).substr(-2)
            nowMin = ("0" + nowMin).substr(-2)
            nowSec = ("0" + nowSec).substr(-2)
            var msg = "時刻：" +  year + '/' + month + '/' + date + ' ' + nowHour + ":" + nowMin + ":" + nowSec;
           document.getElementById("Clock").innerHTML = msg;
        }
        function btn() {
            $('#measbtn').trigger('click');
        }
        setInterval('showClock1()',1000);
        setInterval('btn()',100000);
    </script>

    <script type="text/javascript">
        $(document).ready(function () {
        // $(window).load(function(){
            function addMessage(msg) {
                // メッセージの先頭に時刻を追加する（mm:ss.fff）
                var dt = new Date();
                var hour = ("0" + dt.getHours().toString()).substr(-2);
                var min = ("0" + dt.getMinutes().toString()).substr(-2);
                var sec = ("0" + dt.getSeconds().toString()).substr(-2);
                var mSec = ("00" + dt.getMilliseconds().toString()).substr(-3);
                var time = hour + ":" + min + ":" + sec + "." + mSec;
                var s = time + " " + msg + "\n";
                $("#result").text($("#result").text() + s);
            }

            // ボタンのクリックイベントハンドラー
            $("#measbtn").click(function (e) {
                $("#result").text(""); // テキストエリアをクリア
        //        addMessage("クリックイベントハンドラーの先頭");

                $.ajax({
                    type: "GET", // 省略可（省略時は"GET"）
                    dataType: "json", // 省略可（省略時はMIMEタイプから推定） IISにMIMEの追加必要(IIS6)
                    scriptCharset: 'utf-8',
                    url: "./dataget_async.ashx?ty=j",
                    cache: false, // 省略可（省略時はtrue）
                    timeout: 10000 // 省略可
                })
                // 成功時
                .done(function (data, textStatus, jqXHR) {
                    addMessage(textStatus + ": status=" + jqXHR.status);
                    // 「dataType: "text"」と指定すると、dataはプレーンテキスト
                    addMessage(data.meastime + "," + data.WindDir + "," + data.WindSpd + "," + data.WindSpdMax);
                    // JSONデータをオブジェクトに配置
                    for (var key in data){
                    //addMessage(key);
                        $("#" + key).text(data[key])
                    }
                    //animate1();
                    var prerot = parseFloat($("#prerot").val());
                    var rot=data["WindDir"];
                    imgrot(prerot,rot);
                    $("#prerot").val(rot);
                    addMessage(rot + "," + prerot);
                })

                // 失敗時
                .fail(function (jqXHR, textStatus, errorThrown) {
                  addMessage("fail! " + jqXHR.statusText + ", status=" + jqXHR.status);
                });

    //        addMessage("クリックイベントハンドラーの末尾");
            });
        });

        function imgrot(prerot,rot){
                // ボタンをクリックした時
            var calrot = parseFloat(rot-prerot);
            var judgerot= Math.sin(calrot *(Math.PI/180));
            var nowrot;
            rot = parseFloat(rot);

//            createjs.RotationPlugin.install(); //なぜかうまく動かない
//            var imgArr = document.getElementById("img");
//            var tweentest = new createjs.Tween.get(imgArr)
//            tweentest.to({rotation: rot},1000,createjs.Ease.cubicInOut);
            console.log("rot:" + rot + "  prerot:" + prerot + "  calrot:" + calrot + " judgerot:" + judgerot);

             if (judgerot < 0 ) {
                 console.log("Left");
                 var dif = (prerot-rot)
                 if (dif < 0) {
                     nowrot = rot - 360;
                 } else {
                    if (prerot > rot) {
                        nowrot=rot;
                    } else {
                            nowrot = rot - 360;
                    };
                };
             } else {
                 console.log("Right");
                 if (prerot > 180.1 && rot >= 0) {
                    nowrot=360+rot;
                 } else {
                    nowrot = rot;
                 };
             };
            console.log("nowrot:" + nowrot);

////            $("#img").rotate({
////                duration: 1000,
////                angle: prerot,
////                animateTo: parseFloat(nowrot),
////                easing: $.easing.easeInOutExpo
////            });

            $({deg:prerot}).animate({deg:nowrot}, {
                duration:1000,
                // 途中経過
                progress:function() {
                    $('img.wdimg').css({
                            transform:'rotate(' + this.deg + 'deg)'
                    });
                },
            });
        };

        onload = function() {
            draw();
        };
        function draw() {
            /* canvas要素のノードオブジェクト */
            var canvas = document.getElementById('canvassample');
            /* canvas要素の存在チェックとCanvas未対応ブラウザの対処 */
            if ( ! canvas || ! canvas.getContext ) {
            return false;
            }
            /* 2Dコンテキスト */
            var ctx = canvas.getContext('2d');
            //  /* 四角を描く */
            //  ctx.beginPath();
            //  ctx.moveTo(0, 0);
            //  ctx.lineTo(120, 0);
            //  ctx.lineTo(120, 120);
            //  ctx.lineTo(0, 120);
            //  ctx.closePath();
            //  ctx.stroke();
            /* 円を描く */
            ctx.beginPath();
            ctx.lineWidth = 2;
            ctx.strokeStyle = 'rgb(11, 98, 185)'; // 水色
            ctx.fillStyle = 'rgb(153, 201, 249)'; // 水色
            ctx.arc(61, 61, 60, 0, Math.PI*2, false);
            ctx.fill();
            ctx.stroke();
        };
    </script>
    <link href="./CSS/stl_wd.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <h1 class="txtShadow">非同期通信テストページ [Handler]</h1>
        <div hidefocus="true" id="ControlSitePCSet_Meas" lang="ja" title="AsyncTest">
            <div id = "Clock" class="boxborder wid365 timebox">時刻：</div>
            <div id = "lbl0" class="boxborder wid100 timebox">測定時刻：</div>
                <div id = "meastime" class = "boxborder LABELS_Left wid260 datalabel">--</div>
            <div id = "lbl1" class="boxborder wid100 timebox">方　　位：</div>
                <div id = "WindDir" class = "boxborder LABELS_Left wid260 datalabel">データが入ります。</div>
            <div id = "lbl2" class="boxborder wid100 timebox">風　　速：</div>
                <div id = "WindSpd" class = "boxborder LABELS_Left wid260 datalabel">データが入ります。</div>
            <div id = "lbl3" class="boxborder wid100 timebox">最大風速：</div>
                <div id = "WindSpdMax" class = "boxborder LABELS_Left wid260 datalabel">データが入ります。</div>
            <br />
            <div id="outbox" class="wid365 timebox">
                <div id="measbtn" class="meas">
                    <div id="btn" class="btnBox"><a href="javascript:void(0);">非同期 更新</a></div> 
                </div>
            </div>
<table id="wdtb" class="outtbl">
        <tbody>
                <tr>
                        <td></td>
                        <td>北</td>
                        <td></td>
                </tr>
                <tr>
                        <td>西</td>
                        <td>
                    <%--<div class="wds">
                        <img id="img" class="wdimg" src="./img/AniArrow1.gif" width="90px">
                    </div>--%>
                        </td>
                        <td>東</td>
                </tr>
                <tr>
                        <td></td>
                        <td>南</td>
                        <td>
                        <canvas id="canvassample" width="122" height="122"></canvas>
                        </td>
                </tr>
        </tbody>
</table>
            <p>処理結果<br />
                <textarea id="result" rows="10" cols="80"></textarea>
            </p>
            <input type="hidden" id="prerot" name="prerot" value="0" /><br />
            <uc1:wds ID="Wds1" runat="server" />
        </div>
    </form>
</body>
</html>