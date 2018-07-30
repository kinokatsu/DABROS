// This application jscript code is browser dependent.
// The code emitted by the chart and called by ShowDataCoords
// is browser independent.

// client_x calculates the left relative to the page of
// the passed object
function client_left(o)
{
    if (typeof(o) != "object" || o == null)
        return 0;
    else
        return o.offsetLeft - o.scrollLeft + client_left(o.offsetParent);
}

// client_y calculates the top relative to the page of
// the passed object
function client_top(o)
{
    if (typeof(o) != "object" || o == null)
        return 0;
    else
        return o.offsetTop - o.scrollTop + client_top(o.offsetParent);
}

// convert the event coordinates to chart image coordinates
// and finally convert to data coordinates.  popup the result.
function ShowDataCoords(chart, e) {
  var x = e.clientX - client_left(chart);
  var y = e.clientY - client_top(chart);
  
  // the conversion routine emitted by the chart is the concatenation
  // of the chart id and chart group name and the function name.
  // Note that the ChartGroup name can be changed.
  var dc = C1WebChart1_Group1_CoordToDataCoord(x,y);
  var vstr = '(' + dc.x + ',' + dc.y + ')';
  alert(vstr);
}
