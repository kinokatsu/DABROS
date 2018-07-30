/*jQuery(document).ready(function () {  こっちは、UpdatePanelでは実行されない*/
function pageLoad() {
    // ToolTip トップ画面用
    tippy("SPAN,.NoClass,.periodTable", {
        placement: "bottom",
        animation: "shift-away",
        duration: 300,
        arrow: false,
        theme: "tooltip",
        inertia: false,
        distance: 10,
        followCursor: true,
        performance: true
    });
    tippy(".statusTableCell", {
        placement: "bottom",
        animation: "shift-away",
        duration: 300,
        arrow: false,
        theme: "alerttip",
        inertia: true
    });
    var dt = document.URL;
    if (dt.includes("Graph_Depth") === true) {
        setToolTip("right", true, 0);
    } else if (dt.includes("Graph_Disp") === true || dt.includes("Graph_Time") === true) {
        setToolTip("bottom", true, 20);
    }
}
//深度分布図・変位分布図用
function setToolTip(pos, arr, ofst) {
    tippy("area[title],img[title]", {
        /*tippy('area[title]' ,{*/
        placement: pos,
        animation: "shift-away",
        duration: 300,
        arrow: false,
        arrowType: "round",
        arrowTransform: "scaleX(0.7) scale(1.2)",
        theme: "tooltipdist",   /*'tooltipdist bordered-theme',*/
        inertia: true,
        followCursor: true,
        performance: true,
        offset: ofst,
        distance: 15
    });
}

