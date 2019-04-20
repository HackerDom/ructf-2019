'use strict';

const n = 4;
const height = 30;
const width = 50;
const freq = 0.1;
const exponent = 2;
const size = 20;
const delta = 10;

function GenerateMap(centerX, centerY) {
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

function ShiftCoords(centerX, centerY, newCenterX, newCenterY, x, y) {
    let shiftedX = x - centerX + newCenterX;
    let shiftedY = y - centerY + newCenterY;
    return [shiftedX, shiftedY];
}

function RenderMap(map, ctx) {
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

function RenderBeacons(beacons, centerX, centerY, ctx) {
    let map = document.getElementById('map');
    let canvas = document.getElementById('canvas');
    beacons.forEach(function(beacon) {
        let coords = ShiftCoords(centerX, centerY, width/2, height/2, beacon.coord_x, beacon.coord_y);

        ctx.fillStyle = "#C0C0C0";
        ctx.fillRect(coords[0]*size,coords[1]*size,size,size);
    });
}

function RenderFullMap(centerX, centerY, ctx) {
    let map = GenerateMap(centerX, centerY);
    RenderMap(map, ctx);
    let beacons = GetBeacons(centerX, centerY);
    RenderBeacons(beacons, centerX, centerY, ctx);
}

function GetBeacons(centerX, centerY) {
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

function addButtonListeners(centerX, centerY, ctx) {
    document.getElementById('button-left').onclick = function() {
        centerX = centerX - delta;
        RenderFullMap(centerX, centerY, ctx);
    };
    document.getElementById('button-up').onclick = function() {
        centerY = centerY - delta;
        RenderFullMap(centerX, centerY, ctx);
    };
    document.getElementById('button-right').onclick = function() {
        centerX = centerX + delta;
        RenderFullMap(centerX, centerY, ctx);
    };
    document.getElementById('button-down').onclick = function() {
        centerY = centerY + delta;
        RenderFullMap(centerX, centerY, ctx);
    };
}

function init(centerXStr, centerYStr){
    let centerX = parseInt(centerXStr);
    let centerY = parseInt(centerYStr);

	var ctx = document.getElementById('canvas').getContext("2d");

	RenderFullMap(centerX, centerY, ctx);
    addButtonListeners(centerX, centerY, ctx);

//	let map = GenerateMap(width/2, height/2);
//	PrintMap(map, ctx);
}