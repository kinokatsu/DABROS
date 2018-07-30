//2017.02.21 csv作成関数で変数のクリア方法を変更 delete -> void 0
var chart1;
var chart2;
var chart3;
var loaded;
var opeTime;
function pageLoad() {
	/*theCanvas = document.getElementById('canvasjs-chart-canvas');*/
//	window.addEventListener('resize',canvas_resize,false);
	/*canvas_resize();*/
	test();

//	$("#pre").hide();
//	var SensorNo = $("#DDLSensor").val();
//	$("#csv_part").val(SensorNo + "のみ");
//	//$("#csv_part").text(SensorNo + "のみ");

//	//センサー記号が選択されたら描画
//	$('#DDLSensor').change(function() {
//		getWaveData();
//	});
//	//日付けが選択されたら描画
//	$('#DDLHMin').change(function() {
//		getWaveData();
//	});
//	$.fn.extend({
//		rawclick : function(){
//			this.each(function(i, e){
//				e.click();
//			});
//		}
//	});
}

function canvas_resize(){
//	var outbox = $('#grp');
//	var theCanvas = $('#leftCol');
//	/*var style = theCanvas.currentStyle || document.defaultView.getComputedStyle(theCanvas, '')*/
//	var windowInnerWidth=outbox.innerWidth;
//	var windowInnerHeight=outbox.innerHeight;
////	theCanvas.setAttribute('width',(windowInnerWidth*0.5)+"px");
////	theCanvas.setAttribute('height',windowInnerHeight+"px");
//	outbox.css("width","680px");
//	theCanvas.css("width","340px");        /*(windowInnerWidth*0.5)+"px");*/
//	/*theCanvas.css('height',windowInnerHeight+"px");*/

	/*強制的にウィンドウサイズを調整して印刷幅に合わせる・・・*/
	//this.window.resizeTo(757,800);
}

function PrintPreview() {
	/*canvas_resize();*/
	window.print();
//	if(window.ActiveXObject === null || document.body.insertAdjacentHTML === null) return;
//	var sWebBrowserCode = '<object width="0" height="0" classid="CLSID:8856F961-340A-11D0-A96B-00C04FD705A2"></object>';
//	document.body.insertAdjacentHTML('beforeEnd', sWebBrowserCode); 
//	var objWebBrowser = document.body.lastChild;
//	if(objWebBrowser === null) return;
//	objWebBrowser.ExecWB(7, 1);
//	document.body.removeChild(objWebBrowser);
	}
function CheckBrowserReadyState() {
	if(window.document.readyState !== null && window.document.readyState != 'complete') {
		return false; //処理続行NG¥n
		}
	else{
		return true; //処理続行OK¥n 
		}
	}
function CheckRegExp() {
	var strSrc = "";
	var rObj = new RegExp("^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$");
	for (i=1;i<=6;i++) {
		if (document.getElementById(['TxtMax'+i]) !== undefined){
			strSrc = document.getElementById(['TxtMax'+i]).value;
			if (strSrc.match("^[+\-]?[0-9]{1,6}\.?[0-9]{0,2}$")){}
			else{
				//alert("【最大値 No." + i + "】に入力に間違いがあります。　数値(半角)のみとしてください。");
				showAlert("入力エラー","【最大値 No." + i + "】に入力に間違いがあります。　数値(半角)のみとしてください。");
				CancelPostBack();
			}
		}
	}
}

function CancelPostBack()
{
	var prm = Sys.WebForms.PageRequestManager.getInstance();
	prm.abortPostBack();
}

//スケールの設定を取得
function getMaxMin() {

	var rdb;
	var max;
	var min;
	rdb=$("input[name='No1']:checked").val();
	if (rdb == "RdbNo11")
	{
		var cmb;
		cmb=$("#DdlScale1").val();
		if (cmb == 9999)
		{
			max=null;
			min=null;
		}else{
			max=cmb;
			min=-cmb;
		}
	}
	else
	{
		max = $("#TxtMax1").val();
		min = $("#TxtMin1").val();
	}
	return [max,min];
}
//グラフのスケールを設定
function setScale(){

	var arr = getMaxMin();

	chart1.axisY[0].set("maximum",arr[0]);
	chart1.axisY[0].set("minimum",arr[1]);
	chart2.axisY[0].set("maximum",arr[0]);
	chart2.axisY[0].set("minimum",arr[1]);
	chart3.axisY[0].set("maximum",arr[0]);
	chart3.axisY[0].set("minimum",arr[1]);
	$(".canvasjs-chart-toolbar").css("display", "none");
}

//選択された日時を取得
function getDate() {
	var ym = $("#DDLYMon").val();
	var dt = $("#DDLDay").val();
	var hms = $("#DDLHMin").val();

	var csv = -1;
	if  (hms !== '' && dt!=='' && hms !== '') {
		csv = ym.replace("年 ","").replace("月","")+dt.replace("日","_");       // yyyyMMdd
		csv += hms.replace(/:/g,"");                                                    // 正規表現で記述しないと２つ目は無視される
		return [csv,ym + " " + dt + " " + hms];
	}
	else
	{
		//alert("日付け指定を見直してください。");
		showAlert("日付選択エラー","日付け指定を見直してください。");
		return [-1,-1];
	}
}

//アラート表示(jQuery-Confirm用)
function showAlert(ttl,cont){
		$.alert({title:ttl,
			content:cont,
			closeIcon: true,
			animation:'zoom',
			closeAnimation:'zoom',
			boxWidth:'300px',
			useBootstrap:false,
			type:'red',
			bgOpacity:0.7,
			buttons: {
				okay: {
					text: 'OK',
					btnClass: 'btn-primary'
				}
			}
		});
}

//データCSVファイルダウンロード
function downloadCSV(DLType){

var SensorNo;

	var parm;
	var genHandler="";
	var fileName="";
	var headerText="";
	var charSet;
	var dataSet;

	SensorNo = $("#DDLSensor").val();
	csv = getDate();

	if (csv[0]==-1){
		return false;
	}

	buttonEnable("true");
	//delayEnable(0);

	if (DLType=="p"){
		//$("#pre").show();
		$("#csv_part").addClass("bpic");
		param={dt:csv[0],sn:SensorNo};
		genHandler="./api/datarq_wv.ashx";
		fileName=csv[0]+ "_" + SensorNo + ".csv";
		var senNo= SensorNo.replace("No.","");
		headerText = csv[1] + ',' + SensorNo + '\n' + 'time,';
		headerText += ("N@-X,N@-Y,N@-Z").replace(/@/g,senNo) + "\n"; // 正規表現で記述しないと２つ目は無視される
		headerText += "sec,kine,kine,kine\n"
		charSet='shift-jis';
		dataSet="json"
	}else{
		$("#csv_all").addClass("bpic");
		//$("#pre2").show();
		param={dt:csv[0]};
		//param={dt:csv[0],sn:0};
		genHandler="./api/datarq_wv_all.ashx";
		fileName=csv[0] + ".csv";
		headerText = "";
		charSet='shift-jis';
		dataSet="text"
	}

	var makeCSV="";
	makeCSV = headerText;
	$.ajax({
		type: "GET",        // 省略可（省略時は"GET"）
		dataType: dataSet,  // 省略可（省略時はMIMEタイプから推定） IISにMIMEの追加必要(IIS6)
		scriptCharset: charSet,
		data: param,
		url: genHandler,
		cache: false,       // 省略可（省略時はtrue）
		timeout: 15000      // 省略可
	}).done(function (data, textStatus, jqXHR) {                    // 成功時 この書き方は、Ver1.8以降
		if (DLType=="p"){
			$.each(data, function(key, value){
				makeCSV += value[0]+","+value[1]+","+value[2]+","+value[3]+"\n";
			});
		}else{
			makeCSV = data;
		}

		delayEnable(0);

		// このやり方だとUTF-8になる→Excelで開くと日本語化ける→BOM付きにするとUTF-8で開くことができる
		var bom = new Uint8Array([0xEF, 0xBB, 0xBF]);
		var blob = new Blob([ bom, makeCSV ], { "type" : "text/csv" });

		if (window.navigator.msSaveBlob) { 
			window.navigator.msSaveBlob(blob, fileName);        //"test.txt");

			// msSaveOrOpenBlobの場合はファイルを保存せずに開ける
			window.navigator.msSaveOrOpenBlob(blob, fileName);      //"test.txt");
		} else {
			var userAgent = window.navigator.userAgent.toLowerCase();
			var appVersion = window.navigator.appVersion.toLowerCase();
			var ua = navigator.userAgent.toLowerCase();

			var url = window.URL;   // || window.weblitURL;
			var blobURL = url.createObjectURL(blob);

			if (userAgent.indexOf('firefox') != -1 || ua.indexOf('opera') != -1) {
				//FireFox,Opera向け
				document.getElementById("download").download=fileName;
				document.getElementById("download").href = blobURL;     //window.URL.createObjectURL(blob);
				//$("download").click();
				document.getElementById("download").click();            //jQueryのclick()ではNG
			}else{
				var a = document.createElement('a');
				a.download = fileName       //"test.txt";     //fileName;
				a.href = blobURL;
				a.click();
			}
		}
	})
	// 失敗時
	.fail(function (jqXHR, textStatus, errorThrown) {
		showAlert("データ読込異常発生","エラーが発生しました。初期状態に戻します。");
		$("#ImbtnReloadGraph").click();
		//addMessage("fail! " + jqXHR.statusText + ", status=" + jqXHR.status);
	});

	$("#csv_part").removeClass("bpic");
	$("#csv_all").removeClass("bpic");
//    $("#pre").hide();
//    $("#pre2").hide();

	makecsv = void 0;     //変数の初期化　メモリ解放 varで定義した変数はdeleteは使えない
}
//ボタンの有効／無効　切り替え
function buttonEnable(prop){
	$("#csv_part").prop("disabled", prop);
	$("#csv_all").prop("disabled", prop);
	$("#drawGraph").prop("disabled", prop);
}

//非同期によるデータ受信とグラフオブジェクトの定義
function getWaveData() {
	//var startTime = new Date();
	var SensorNo;
	SensorNo = $("#DDLSensor").val();           //センサー記号の取得

	showDate = getDate();                       //表示日時の取得　0:「/」「:」抜き　　1:年月日時分秒
	if (showDate[0]==-1){return;}

	buttonEnable(true);
	//delayEnable(0);
	$("#csv_part").val(SensorNo + "のみ");
	//$("#csv_part").text(SensorNo + "のみ");

//    chart1.data[0].remove();  //なくてよさそう
//    chart2.data[0].remove();
//    chart3.data[0].remove();

	var arr = getMaxMin();
	var dataPoints1 = [];                       // 配列のクリア
	var dataPoints2 = [];
	var dataPoints3 = [];

	//window.alert($.fn.jquery);    //jQueryヴァージョンチェックの方法
	//非同期処理で指定データを取得する
	$.ajax({
		type: "GET", // 省略可（省略時は"GET"）
		dataType: "json", // 省略可（省略時はMIMEタイプから推定） IISにMIMEの追加必要(IIS6)
		scriptCharset: 'shift-jis',
		data: {dt:showDate[0],
			   sn:SensorNo},
		url: "./datarq_wv.ashx",
		cache: false, // 省略可（省略時はtrue）
		timeout: 10000 // 省略可
	}).done(function (data, textStatus, jqXHR) {                    // 成功時 この書き方は、Ver1.8以降
		$.each(data, function(key, value){
			dataPoints1.push({x: value[0], y: value[1]});           //JSONのデータをpushして配列変数に格納
			dataPoints2.push({x: value[0], y: value[2]});
			dataPoints3.push({x: value[0], y: value[3]});
		});

		//var endTime = new Date();
		delayEnable(0);

		//chart1 = new $('#cCtnr1').CanvasJSChart({
		chart1 = new CanvasJS.Chart("cCtnr1",{
			zoomEnabled:true,zoomType:'xy',theme:'theme1',animationEnabled:true,animationDuration:400,axisY2:{includeZero:false,lineThickness:2},
			axisX: {interval:5,minimum:0,maximum:60,labelFontSize:16,gridDashType: "shortDot",gridThickness: 1},
			axisX2:{lineThickness:2},
			axisY:{titleFontSize:16,minimum:arr[1],maximum:arr[0],title:'Ｘ成分 速度(kine)',titleFontFamily: 'meiryo',labelFontSize:16,gridDashType: "shortDot",gridThickness:1},
			axisY2:{includeZero:false ,lineThickness:2},
			toolTip:{fontSize:13,borderColor: "black",content:"経過時間: {x}(sec)</br>速　　度: {y}(kine)",fontStyle:'italic',fontFamily:'meiryo',backgroundColor:'rgba(240,240,240,.8)'},
			data: [{
			type: "line",dataPoints : dataPoints1},
			{type:'line',axisYType:'secondary'},{type:'line',axisXType:'secondary'}],
			rangeChanged: syncHandler
		});
		chart1.render();

		//chart2 = new $('#cCtnr2').CanvasJSChart({
		chart2 = new CanvasJS.Chart("cCtnr2",{
			zoomEnabled:true,zoomType:'xy',animationEnabled:true,animationDuration:350,axisY2:{includeZero:false,lineThickness:2},
			axisX: {interval:5,minimum:0,maximum:60,labelFontSize:16,gridDashType: "shortDot",gridThickness: 1},
			axisX2:{lineThickness:2},
			axisY:{titleFontSize:16,minimum:arr[1],maximum:arr[0],title:'Ｙ成分 速度(kine)',titleFontFamily: 'meiryo',labelFontSize:16,gridDashType: "shortDot",gridThickness: 1},
			axisY2:{includeZero:false ,lineThickness:2},
			toolTip:{fontSize:13,borderColor: "black",content:"経過時間: {x}(sec)</br>速　　度: {y}(kine)",fontStyle:'italic',fontFamily:'meiryo',backgroundColor:'rgba(240,240,240,.8)'},
			data: [{
			type: "line",
				dataPoints : dataPoints2},
			{type:'line',axisYType:'secondary'},{type:'line',axisXType:'secondary'}],
			rangeChanged: syncHandler
		});
		chart2.render();

		//chart3 = new $('#cCtnr3').CanvasJSChart({
		chart3 = new CanvasJS.Chart("cCtnr3",{
			zoomEnabled:true,zoomType:'xy',animationEnabled:true,animationDuration:300,axisY2:{includeZero:false,lineThickness:2},
			axisX: {interval:5,minimum:0,maximum:60,labelFontSize:16,gridDashType: "shortDot",gridThickness: 1},
			axisX2:{lineThickness:2},
			axisY:{titleFontSize:16,minimum:arr[1],maximum:arr[0],title:'Ｚ成分 速度(kine)',titleFontFamily: "meiryo",labelFontSize:16,gridDashType: "shortDot",gridThickness: 1},
			axisY2:{includeZero:false ,lineThickness:2},
			toolTip:{fontSize:13,borderColor: "black",content:"経過時間: {x}(sec)</br>速　　度: {y}(kine)",fontStyle:'italic',fontFamily:'meiryo',backgroundColor:'rgba(240,240,240,.8)'},
			data: [{
			type: "line",
				dataPoints : dataPoints3},
			{type:'line',axisYType:'secondary'},{type:'line',axisXType:'secondary'}],
			rangeChanged: syncHandler
		}); 
		chart3.render();

		$("#LblTitleLower").text("【" + SensorNo + " 測点 振動波形データ】");           //ページタイトル
		$("#LblDateSpan").text("データ日時：" + showDate[1]);                           //出力日時

		$('.pic').off('click');                                                         // onイベントは追加型のため、その前に削除しておく
		setPicDL();                                                                     // 画像ファイル保存ボタンのイベント定義

	})
	// 失敗時
	.fail(function (jqXHR, textStatus, errorThrown) {
		//alert("エラーが発生しました。再読み込みを実行します。");
		showAlert("データ読込異常発生","エラーが発生しました。初期状態に戻します。");
		$("#ImbtnReloadGraph").click();
		//addMessage("fail! " + jqXHR.statusText + ", status=" + jqXHR.status);
	});
	dataPoints1.length=0;       //配列の初期化　メモリ解放
	dataPoints2.length=0;
	dataPoints1.length=0;
	//buttonEnable(false);
}

//タイマーを使用してボタンを有効化する（最低900ms + 引数ms）
function delayEnable(waitTime){
	//this.bar = waitTime === undefined ? 0 : waitTime;
	setTimeout(function(a){buttonEnable(a)}, (700+waitTime), false);
}
// CanvasJSのサンプルから　こちらは、エラーを検知しないため、↑に変更
//    $.getJSON("./datarq_wv.ashx?dt=" + csv + "&sn=" + SensorNo, function(data){
////        alert("Data Loaded: " + data);
//        $.each(data, function(key, value){
//            dataPoints1.push({x: value[0], y: value[1]});
//            dataPoints2.push({x: value[0], y: value[2]});
//            dataPoints3.push({x: value[0], y: value[3]});
//        });
//        chart1 = new $('#cCtnr1').CanvasJSChart({
//            axisX: {interval:5,minimum:0,maximum:60},
//            data: [{
//            type: "line",
//                dataPoints : dataPoints1,
//            }]
//        });
//        chart2 = new $('#cCtnr2').CanvasJSChart({
//            axisX: {interval:5,minimum:0,maximum:60},
//            data: [{
//            type: "line",
//                dataPoints : dataPoints2,
//            }]
//        });
//        chart3 = new $('#cCtnr3').CanvasJSChart({
//            axisX: {interval:5,minimum:0,maximum:60},
//            data: [{
//            type: "line",
//                dataPoints : dataPoints3,
//            }]
//        });
//    //chart.render();
//    //updateChart();
//    });
//function isValidDate(s) {
//    var matches = /^(\d+)\/(\d+)\/(\d+)$/.exec(s);
//    if(!matches) {
//        return false;
//    }
//    var y = parseInt(matches[1]);
//    var m = parseInt(matches[2]);
//    var d = parseInt(matches[3]);
//    if(m < 1 || m > 12 || d < 1 || d > 31) {
//        return false;
//    }
//    var dt = new Date(y, m - 1, d, 0, 0, 0, 0);
//    if(dt.getFullYear() != y
//    || dt.getMonth() != m - 1
//    || dt.getDate() != d)
//    {
//        return false;
//    }
//    return true;
//}
//function isValidDate(date_str) {
//    var date = new Date(date_str),
//    date_reg = /^(\d{4}|\d{2})(?:\x2d|\u002f)(\d{2}|\d)(?:\x2d|\u002f)(\d{2}|\d)/,

//    valid = function() {
//        var date_strs = date_str.match(date_reg),
//        addDateStr = {};
//        addDateStr.date = function() {
//            return (date.getFullYear() - 0) + (date.getMonth() - 0) + (date.getDate() - 0);
//        };
//        addDateStr.str = function() {
//            return (date_strs[1] - 0) + (date_strs[2] - 1) + (date_strs[3] - 0);
//        };
//        if (isNaN(date.getTime()) || !date_strs) return false;
//            return addDateStr.date() === addDateStr.str();
//        };
//    if (Object.prototype.toString.call(date) !== "[object Date]") return false;
//    return valid();
//}

function setPicDL() {
//    if (loaded == true){return;}
	// 画像ファイル保存ボタンのイベント定義
	$('.pic').on('click',function(){
//        $('#dom')[0].addEventListener('click', fnc);
//        $('#dom').off('click');
//        $('#dom').on('click', fnc);

		var showDate = getDate();                       //表示日時の取得　0:「/」「:」抜き　　1:年月日時分秒
		var type = $(this).data('dir');
		if (showDate[0]==-1){return;}
		var SensorNo = $("#DDLSensor").val();           //センサー記号の取得
		var chartsv;
		var fn;
		//押されたボタンで処理を分ける
		switch(type){
			case '_X':
				chartsv=chart1
				break;
			case '_Y':
				chartsv=chart2
				break;
			case '_Z':
				chartsv=chart3
				break;
		}
		//chart.set("height", 422);
		fn = showDate[0] + type + "_" + SensorNo;
		chartsv.exportChart({format: "png",fileName:fn});
		//chart.set("height", 211);
//  	    loaded = true
	});
}
//↑Optimize
////    $("#saveZ").click(function(){
////        chart3=$('#cCtnr3').CanvasJSChart();
////        var csv = getDate();
////        var fn = csv[0] +'_Z';
////        chart3.exportChart({format: "png",fileName:fn});
////    });
//};


//--------------- Syncing the charts ---------------//　グラフ間のズームやパンの同期
//var charts0 = []        //[chart1, chart2, chart3]; // add all charts (with axes) to be synced
var charts0 = [chart1, chart2, chart3]; // add all charts (with axes) to be synced
function syncHandler(e) {
	for (var i = 0; i < charts0.length; i++) {
//        var chart = charts0[i];
		eval("var chart=chart" + (i + 1) + ";")

		 if (!chart.options.axisX)
		   chart.options.axisX = {};

		 if (!chart.options.axisY)
		   chart.options.axisY = {};

		 if (e.trigger === "reset") {

		   chart.options.axisX.viewportMinimum = chart.options.axisX.viewportMaximum = null;
		   chart.options.axisY.viewportMinimum = chart.options.axisY.viewportMaximum = null;
		   chart.render();

		 } else if (chart !== e.chart) {

		   chart.options.axisX.viewportMinimum = e.axisX[0].viewportMinimum;
		   chart.options.axisX.viewportMaximum = e.axisX[0].viewportMaximum;

		   chart.options.axisY.viewportMinimum = e.axisY[0].viewportMinimum;
		   chart.options.axisY.viewportMaximum = e.axisY[0].viewportMaximum;

		   chart.render();
		}
	}
}


function test(){
	// 横線
	var stripLines = [];
	// 管理値
	stripLines.push({value:16,thickness:3,showOnTop: true,color:"rgba(234,234,0,1)",labelAlign: "far",label:"Alert Level 1",labelFontColor:"rgba(0,0,0,0.3)"});
	stripLines.push({value:18,thickness:3,showOnTop: true,color:"rgba(255,128,128,1)",labelAlign: "far",label:"Alert Level 2",labelFontColor:"rgba(0,0,0,0.3)"});
	stripLines.push({value:20,thickness:3,showOnTop: true,color:"rgba(255,0,0,1)",labelAlign: "far",label:"Alert Level 3",labelFontColor:"rgba(0,0,0,0.3)"});
	stripLines.push({value:22,thickness:3,showOnTop: true,color:"rgba(255,0,255,1)",labelAlign: "far",label:"Alert Level 4",labelFontColor:"rgba(0,0,0,0.3)"});

	/* ---- 河川断面図 ---- */
	var chart = new CanvasJS.Chart("chartContainer", {
		animationEnabled: true,
		animationDuration: 500,
		zoomEnabled: false,
		toolTip: {
			enabled: false,
		},
		axisX: {
			valueFormatString: "",
			minimum: 0,
			maximum: 10,
			interval: 0,
			labelFontSize: 0,
			tickThickness: 0,
			lineThickness:0,
		},
		axisX2:{
			lineThickness:0,
		},
		axisY: {
			margin:10,
			title: "",
			titleFontSize: 18,
			minimum: 0,
			maximum: 25,
			interval: 5,
			gridThickness: 0,
			stripLines: stripLines,
			includeZero: true,
			gridDashType:"shortDot",
				labelFormatter: function(e){
					/*return " " + e.value;*/
					return " ";
				},
			titleFontFamily: "meiryo",
			lineThickness:0,
			tickLength:0,
		},
		axisY2: {
			includeZero:false ,
			lineThickness:0,
			tickLength:0,
				labelFormatter: function(){
					return " ";
				},
			minimum: 0,
			maximum: 25,
			interval: 5,
			gridThickness: 0,
		},
		data: [
			{
			/***右軸用ダミー***/
			name:"dummy",
			type: "line",
			showInLegend: false,
			dataPoints: [],
			axisYType: "secondary",
			axisXType: "secondary",
			},
			/***水位データ表示***/
			{name: "Water Head",
			showInLegend: false,
			type: "area",
			color: "rgba(119,187,255,0.7)",
			markerType: "none",
			markerSize: 0,
			titleFontFamily: "meiryo",
			toolTipContent: function(){
				return " ";
			},
			dataPoints: [
				{ x: 0.0, y: 12.8 },
				{ x: 5, y: 12.8 ,indexLabelFormatter: function (e) { return "水位 :" + e.dataPoint.y + "(m)";}},
				{ x: 10, y: 12.8 }
			]
			},
			/***河川断面塗り***/
			{name: "River Shape",
			 markerType: "none",
			 showInLegend: false,
			 legendMarkerType: "none",
			 type: "area",
			 color: "rgba(207,187,139,1)",
			 markerSize: 0,
			 fillOpacity: 1,
			 toolTipContent: function(){
				return " ";
			 },
			 dataPoints: [
				{ x: 0.0, y: 22 },
				{ x: 1, y: 22 },
				{ x: 2.5, y: 5 },
				{ x: 7.5, y: 5 },
				{ x: 9, y: 22 },
				{ x: 10, y: 22 },
			 ]
			},
			/***河川断面線***/
			{name: "River Shape Line",
			 markerType: "none",
			 showInLegend: false,
			 legendMarkerType: "none",
			 type: "line",
			 color: "rgba(177,147,73,1)",
			 markerSize: 0,
			 fillOpacity: 1,
			 toolTipContent: function(){
				return " ";
			 },
			 dataPoints: [
				{ x: 0, y: 22 },
				{ x: 1, y: 22 },
				{ x: 2.5, y: 5 },
				{ x: 7.5, y: 5 },
				{ x: 9, y: 22 },
				{ x: 10, y: 22 },
			 ],
			},
		]
	});
	chart.render();
	window.matchMedia("print").addListener(() => {
		chart.render();
	});
}
