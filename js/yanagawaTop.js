//$(document).ready(function () {
//    /* 打設位置描画 */
//    setConHeight();
//});
// ページ読み込み後に実行（UpdatePanelがある場合は、これでないとイベント発生せず）
/* function pageLoad() {     /*sender, args) {*/
jQuery(document).ready(function () {
    setConHeight();
});

//打設高さ表示SVG制御
function setConHeight() {
    var getData = $("#ctl00_CntplHolder_conParam").val();        //各種パラメータの受け渡し
    if (getData === undefined) {                                // undefind判定
        getData = $("#conParam").val();                         //各種パラメータの受け渡し
    }
    var param = getData.split(",");
    var enlarge = param[2] - 0;
    var ELStart = param[3] - 0;
    var ZeroPoint = param[4] - 0;
    var wordVertOffset = param[5] - 0;
    var conH = $("#ctl00_CntplHolder_conheight").val();         //打設高さの受け渡し
    var Xoffset;
    if (conH === undefined) {
        conH = $("#conheight").val();                           //打設高さの受け渡し
        Xoffset = -45;  // -65;
    } else {
        Xoffset = 80;
    }
    var hei = conH - ELStart;
    var conline = document.getElementById("conline");
    var setHeight = ZeroPoint - (hei * enlarge);
    var x2Point = (setHeight + 141.61) / 1.3423 + Xoffset;        // 右端の位置は上に行くほど、左に寄る
    conline.setAttribute("y1", setHeight);
    conline.setAttribute("y2", setHeight);
    conline.setAttribute("x1", param[0]);
    conline.setAttribute("x2", x2Point);
    var txt = document.getElementById("word");
    var txtbox = txt.getBBox();
    var txtHeight = txtbox.height;
    var txtWidth = txtbox.width;
    var txttop = setHeight + wordVertOffset;
    txt.setAttribute("y", txttop);
    txt.setAttribute("x", x2Point - 0 + (txtWidth * 1.25 - txtWidth) * 0.5);
    var outline = document.getElementById("outl");
    var boxtop = (setHeight - txtHeight * 0.5);
    outline.setAttribute("y", boxtop);
    outline.setAttribute("x", x2Point);
    outline.setAttribute("width", txtWidth * 1.25);
    outline.setAttribute("height", txtHeight);
    if (param.length > 6) {
        var bx1 = document.getElementById("box1");
        var offY=10;
        var pox1 = (param[6] - param[8] * 0.5);
        var poy1 = (param[7] - param[8] * 0.5);
        bx1.setAttribute("x", pox1);
        bx1.setAttribute("y", poy1);
        bx1.setAttribute("width", param[8]);
        bx1.setAttribute("height", param[8]);
        if ((param[9] - 0) != -100) {
            var bx2 = document.getElementById("box2");
            var pox2 = (param[9] - param[11] * 0.5);
            var poy2 = (param[10] - param[11] * 0.5);
            bx2.setAttribute("x", pox2);
            bx2.setAttribute("y", poy2);
            bx2.setAttribute("width", param[11]);
            bx2.setAttribute("height", param[11]);
            var pl = document.getElementById("pathline");
            var strPath = "M " + param[6] + " " + poy1;
            strPath += " Q " + param[6] + " " + (poy1 - offY);
            strPath += " " + (parseFloat(param[6]) + parseFloat(param[9])) * 0.5 + " " + (poy1 - offY);
            strPath += " Q" + param[9] + " " + (poy1 - offY);
            strPath += " " + param[9] + " " + (poy1);
//            var strPath = "M" + param[6] + "," + poy1;
//            strPath += "V" + (poy1 - offY);
//            strPath += "H" + param[9];
//            strPath += "V" + (poy1);
            pl.setAttribute("d", strPath);
        }
    }
}

// Debug use
//function pageLoad() {
//    var slider = $find("sld");
////    slider.add_slideStart(function () {
////        var boundControl = $get(slider.get_BoundControlID());
////        boundControl.style.display = "block";
////    });
////    slider.add_slideEnd(function () {
////        $("#ctl00_CntplHolder_conheight").val($("#ctl00_CntplHolder_Label1").text());
////        setConHeight();
//////        var boundControl = $get(slider.get_BoundControlID());
//////        boundControl.style.display = "none";
////    });
//    slider.add_valueChanged(function () {
//        var getData = $("#ctl00_CntplHolder_conParam").val()
//        if (getData === undefined) {
//            $("#conheight").val($("#Label1").text());
//        } else {
//            $("#ctl00_CntplHolder_conheight").val($("#ctl00_CntplHolder_Label1").text());
//        }
//        setConHeight();
//    });
//}