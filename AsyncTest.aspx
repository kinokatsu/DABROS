<%@ Page Language="VB" AutoEventWireup="false" CodeFile="AsyncTest.aspx.vb" Inherits="AsyncTest" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>非同期通信テスト Ajax</title>
    <link href="CSS/stl_AsyncCom.css" rel="stylesheet" type="text/css" />
    <link href="CSS/stl_jquery.alerts.css" rel="stylesheet" type="text/css" />    
    <script type="text/javascript" src="js/jquery-3.0.0.min.js"></script>
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
        if(nowHour < 10) { nowHour = "0" + nowHour; }
        if(nowMin < 10) { nowMin = "0" + nowMin; }
        if(nowSec < 10) { nowSec = "0" + nowSec; }

        var msg = "時刻：" +  year + '/' + month + '/' + date + ' ' + nowHour + ":" + nowMin + ":" + nowSec;
       document.getElementById("Clock").innerHTML = msg;
    }
    function btn() {
        $('#measbtn').trigger('click');
    }
    setInterval('showClock1()',1000);
    setInterval('btn()',1000);
    </script>

    <script type="text/javascript">
    $(document).ready(function () {
    // $(window).load(function(){
      function addMessage(msg) {
        // メッセージの先頭に時刻を追加する（mm:ss.fff）
        var dt = new Date();
        var hour = ("0" + dt.getHours().toString()).slice(-2);
        var min = ("0" + dt.getMinutes().toString()).slice(-2);
        var sec = ("0" + dt.getSeconds().toString()).slice(-2);
        var mSec = ("00" + dt.getMilliseconds().toString()).slice(-3);
        var time = hour + ":" + min + ":" + sec + "." + mSec;
        var s = time + " " + msg + "\n";
        $("#result").text($("#result").text() + s);
      }

      // ボタンのクリックイベントハンドラー
      $("#measbtn").click(function (e) {
        $("#result").text(""); // テキストエリアをクリア
        addMessage("クリックイベントハンドラーの先頭");

        $.ajax({
          type: "GET", // 省略可（省略時は"GET"）
          dataType: "json", // 省略可（省略時はMIMEタイプから推定） IISにMIMEの追加必要(IIS6)
          scriptCharset: 'utf-8',
          //url: "./OutData/katayanagi_NewestData.json",
          url: "./OutData/katayanagi_NewestData.json",
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
        })

        // 失敗時
        .fail(function (jqXHR, textStatus, errorThrown) {
          addMessage("fail! " + jqXHR.statusText + ", status=" + jqXHR.status);
        });

        addMessage("クリックイベントハンドラーの末尾");
      });
    });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1 class="txtShadow">非同期通信テストページ [AJax/jQuery]</h1>
        <div hidefocus="hideFocus" id="ControlSitePCSet_Meas" lang="ja" title="AsyncTest">
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
            <p>処理結果<br />
                <textarea id="result" rows="10" cols="80"></textarea>
            </p>
        </div>
    </form>
</body>
</html>
