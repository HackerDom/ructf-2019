'use strict';

const n = 4;
const height = 30;
const width = 50;
const freq = 0.1;
const exponent = 2;
const size = 20;
const delta = 10;

function generateMap(centerX, centerY) {
	let gen = new SimplexNoise('fkjwes');//ololol
    function noise(nx, ny) {
      return gen.noise2D(nx, ny) / 2 + 0.5;
    }

    let startX = centerX - width / 2;
    let startY = centerY - height / 2;

    let endX = startX + width;
    let endY = startY + height;

    let elevation = [];
    let elevationXIndex = 0;
    let elevationYIndex = 0;
    for (let y = startY; y < endY; y++) {
        elevation[elevationYIndex] = [];
        for (let x = startX; x < endX; x++) {
            let nx = x/width - 0.5, ny = y/height - 0.5;
            // Можно поумножать на частоту (freq*nx, freq*ny)
            let e = noise(nx, ny);
            let exp = Math.pow(e, exponent);
            let a = Math.round(exp * n);
            elevation[elevationYIndex][elevationXIndex] = a;
            elevationXIndex++;
        }
        elevationYIndex++;
        elevationXIndex = 0;
    }

	return elevation
}

function shiftCoords(centerX, centerY, newCenterX, newCenterY, x, y) {
    let shiftedX = x - centerX + newCenterX;
    let shiftedY = y - centerY + newCenterY;
    return [shiftedX, shiftedY];
}

function renderMap(map, ctx) {
    for (let y = 0; y < height; y++) {
		for (let x = 0; x < width; x++) {
			let p = map[y][x];
			let style = "#352D64";
			switch(p) {
				case 0:
					style = "#352D64";
					break;
				case 1:
					style = "#6C2D6A";
					break;
				case 2:
					style = "#933B92";
					break;
				case 3:
					style = "#D35092";
					break;
				case 4:
					style = "#EE82EE";
					break;
			}
			ctx.fillStyle = style;
			ctx.fillRect(x*size,y*size,size,size);

			ctx.rect(x*size,y*size,size,size);
		}
	}

	ctx.strokeStyle = "#E485B7";
	ctx.stroke();
}

function renderBeacons(beacons, centerX, centerY, ctx) {
    let canvas = document.getElementById('canvas');
    beacons.forEach(function(beacon) {
        let coords = shiftCoords(centerX, centerY, width/2, height/2, beacon.coord_x, beacon.coord_y);

        ctx.fillStyle = "#C0C0C0";
        ctx.fillRect(coords[0]*size,coords[1]*size,size,size);
    });
}

function renderFullMap(centerX, centerY, ctx) {
    let map = generateMap(centerX, centerY);
    renderMap(map, ctx);
    let beacons = getBeacons(centerX, centerY);
    renderBeacons(beacons, centerX, centerY, ctx);
    return {"beacons": beacons};
}

function getBeacons(centerX, centerY) {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", "/GetBeacons?center_coord_x=" + centerX + "&center_coord_y=" + centerY, false);
    xhr.send();
    if (xhr.status != 200) {
        var elem = document.getElementById('error');
        elem.innerHTML = "Could not get beacons. Try again.";
    } else {
        return JSON.parse(xhr.responseText)["beacons"];
    }
}

function addButtonListeners(centerCoords, beacons, ctx) {
    document.getElementById('button-left').onclick = function() {
        centerCoords[0] = centerCoords[0] - delta;
        beacons["beacons"] = renderFullMap(centerCoords[0], centerCoords[1], ctx)["beacons"];
    };
    document.getElementById('button-up').onclick = function() {
        centerCoords[1] = centerCoords[1] - delta;
        beacons["beacons"] = renderFullMap(centerCoords[0], centerCoords[1], ctx)["beacons"];
    };
    document.getElementById('button-right').onclick = function() {
        centerCoords[0] = centerCoords[0] + delta;
        beacons["beacons"] = renderFullMap(centerCoords[0], centerCoords[1], ctx)["beacons"];
    };
    document.getElementById('button-down').onclick = function() {
        centerCoords[1] = centerCoords[1] + delta;
        beacons["beacons"] = renderFullMap(centerCoords[0], centerCoords[1], ctx)["beacons"];
    };
}

function div(val, by){
    return (val - val % by) / by;
}

function getCoords(elem) {
    var box = elem.getBoundingClientRect();

    var body = document.body;
    var docEl = document.documentElement;
    var scrollTop = window.pageYOffset || docEl.scrollTop || body.scrollTop;
    var scrollLeft = window.pageXOffset || docEl.scrollLeft || body.scrollLeft;

    var clientTop = docEl.clientTop || body.clientTop || 0;
    var clientLeft = docEl.clientLeft || body.clientLeft || 0;

    var top = box.top + scrollTop - clientTop;
    var left = box.left + scrollLeft - clientLeft;

    return [top, left];
}

function addCanvasListener(centerCoords, beacons, ctx) {
    let canvas = document.getElementById('canvas');
    let canvasCoords = getCoords(canvas);

    canvas.addEventListener('click', function(event) {
        let clickedX = event.pageX - canvasCoords[1];
        let clickedY = event.pageY - canvasCoords[0];

        let x = div(clickedX, size);
        let y = div(clickedY, size);

        let selectedBeacons = beacons["beacons"].filter(function(beacon) {
            let beaconMapCoords = shiftCoords(centerCoords[0], centerCoords[1], width/2, height/2, beacon.coord_x, beacon.coord_y);
            return beaconMapCoords[0] === x && beaconMapCoords[1] === y;
        });

        if (selectedBeacons.length > 0) {
            ctx.fillStyle = "#D2691E";
            ctx.fillRect(x*size,y*size,size,size);
            viewBeacon(selectedBeacons[0]);
        } else {
            ctx.fillStyle = "#800000";
            ctx.fillRect(x*size,y*size,size,size);
            addBeacon(x, y, centerCoords)
        }
    });
}

function addBeacon(x, y, centerCoords) {
    console.log("create beacon!")
}

function viewBeacon(beacon) {
    console.log(beacon);
}

function init(centerXStr, centerYStr){
    let centerX = parseInt(centerXStr);
    let centerY = parseInt(centerYStr);

	var ctx = document.getElementById('canvas').getContext("2d");

	let beacons = renderFullMap(centerX, centerY, ctx);
	let centerCoords = [centerX, centerY];
    addButtonListeners(centerCoords, beacons, ctx);
    addCanvasListener(centerCoords, beacons, ctx)

}