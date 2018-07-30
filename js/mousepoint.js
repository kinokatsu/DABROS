function displayMouseXY( evt ) {
	var X = Y = 0;
	if ( document.all ) {
		X = event.x
		Y = event.y
		document.all["mpxy"].innerText = "X= " + X + "  Y= " + Y;
		document.all["mpxy"].style.left = X - document.documentElement.scrollLeft + 8;
		document.all["mpxy"].style.top = Y - document.documentElement.scrollTop + 16;
	} else if ( document.layers ) {
		X = evt.x
		Y = evt.y
		with(document.layers["mpxy"]) {
			document.open();
			document.write( "<SMALL>" + X + "," + Y + "</SMALL>" );
			document.close();
			left = X + 8;
			top = Y + 16;
		}
	} else if ( document.getElementById ) {
		X = evt.pageX;
		Y = evt.pageY;
		document.getElementById('mpxy').childNodes.item(0).nodeValue = "X= " + X + "  Y= " + Y;
		document.getElementById('mpxy').style.left = X + 8;
		document.getElementById('mpxy').style.top = Y + 16;
	}
}

if ( document.all ) document.onmousemove = displayMouseXY;
if ( document.getElementById ) document.onmousemove = displayMouseXY;
if ( document.layers ) {
	window.onmousemove = displayMouseXY;
	window.captureEvents(Event.MOUSEMOVE);
}
