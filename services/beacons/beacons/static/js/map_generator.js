'use strict';

const stylesCount = 4;
const height = 30;
const width = 50;
const freq = 0.1;
const exponent = 2;
const size = 20;
const delta = 10;
const stylesMap = {0: "#352D64", 1: "#6C2D6A", 2: "#933B92", 3: "#D35092", 4: "#EE82EE"};
const beaconStyle = "#C0C0C0";
const selectedBeaconStyle = "#D2691E";
const selectedCellStyle = "#800000";
let gen = undefined;

function noise(nx, ny) {
  return gen.noise2D(nx, ny) / 2 + 0.5;
}

function generateOneCell(x, y) {
    let nx = x/width - 0.5, ny = y/height - 0.5;
    // Можно поумножать на частоту (freq*nx, freq*ny)
    let e = noise(nx, ny);
    let exp = Math.pow(e, exponent);
    return Math.round(exp * stylesCount);
}

function generateMap(centerX, centerY) {
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
            elevation[elevationYIndex][elevationXIndex] = generateOneCell(x, y);
            elevationXIndex++;
        }
        elevationYIndex++;
        elevationXIndex = 0;
    }

	return elevation
}

function isBounded(centerX, centerY, x, y) {
    return x >= centerX - width / 2 && x <= centerX + width / 2 &&
           y >= centerY - height / 2 && y <= centerY + height / 2;
}

function shiftCoords(centerX, centerY, newCenterX, newCenterY, x, y) {
    let shiftedX = x - centerX + newCenterX;
    let shiftedY = y - centerY + newCenterY;
    return [shiftedX, shiftedY];
}

function renderRect(x, y, rectSize, style, ctx) {
    ctx.fillStyle = style;
    ctx.fillRect(x,y,rectSize,rectSize);
}

function renderMap(map, ctx) {
    for (let y = 0; y < height; y++) {
		for (let x = 0; x < width; x++) {
			let p = map[y][x];
			let style = stylesMap[p];
			renderRect(x*size, y*size, size, style, ctx);

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

        renderRect(coords[0]*size + 1, coords[1]*size + 1, size - 2, beaconStyle, ctx);
    });
}

function renderSelected(mapStateObject, ctx) {
    if (mapStateObject["selected"] &&
        isBounded(mapStateObject["centerX"], mapStateObject["centerY"],
                  mapStateObject["selected"]["x"], mapStateObject["selected"]["y"])) {
        let coords = shiftCoords(mapStateObject["centerX"], mapStateObject["centerY"],
                                 width/2, height/2,
                                 mapStateObject["selected"]["x"], mapStateObject["selected"]["y"]);
        if (mapStateObject["selected"]["beacon"]) {
            renderRect(coords[0]*size + 1, coords[1]*size + 1, size - 2, selectedBeaconStyle, ctx);
            viewBeacon(mapStateObject["selected"]["beacon"]);
        } else {
            renderRect(coords[0]*size + 1, coords[1]*size + 1, size - 2, selectedCellStyle, ctx);
            addBeacon();
        }
    }
}

function undoSelected(mapStateObject, ctx) {
    let x = mapStateObject["selected"]["x"];
    let y = mapStateObject["selected"]["y"]
    if (isBounded(mapStateObject["centerX"], mapStateObject["centerY"], x, y)) {
        let style = mapStateObject["selected"]["beacon"] ? beaconStyle : stylesMap[generateOneCell(x, y)];
        let coords = shiftCoords(mapStateObject["centerX"], mapStateObject["centerY"],
                                 width/2, height/2, x, y);
        renderRect(coords[0]*size+1, coords[1]*size+1, size-2, style, ctx);
    }
}

function renderFullMap(mapStateObject, ctx) {
    let map = generateMap(mapStateObject["centerX"], mapStateObject["centerY"]);
    renderMap(map, ctx);
    let beacons = getBeacons(mapStateObject["centerX"], mapStateObject["centerY"]);
    mapStateObject["beacons"] = beacons;
    renderBeacons(beacons, mapStateObject["centerX"], mapStateObject["centerY"], ctx);
    renderSelected(mapStateObject, ctx);
}

function hideSidebarsCards() {
    let elements = [].slice.call(document.getElementsByClassName("card"));
    elements.forEach(function(element) {
        if (!element.classList.contains("hidden")) {
            element.classList.add('hidden');
        }
    });
}

function showSidebarsCard(id) {
    let element = document.getElementById(id)
    if (element.classList.contains("hidden")) {
        element.classList.remove("hidden");
    }
}

function addButtonListeners(mapStateObject, ctx) {
    document.getElementById('button-left').onclick = function() {
        mapStateObject["centerX"] = mapStateObject["centerX"] - delta;
        renderFullMap(mapStateObject, ctx);
    };
    document.getElementById('button-up').onclick = function() {
        mapStateObject["centerY"] = mapStateObject["centerY"] - delta;
        renderFullMap(mapStateObject, ctx);
    };
    document.getElementById('button-right').onclick = function() {
        mapStateObject["centerX"] = mapStateObject["centerX"] + delta;
        renderFullMap(mapStateObject, ctx);
    };
    document.getElementById('button-down').onclick = function() {
        mapStateObject["centerY"] = mapStateObject["centerY"] + delta;
        renderFullMap(mapStateObject, ctx);
    };
}

function addFormsListener(mapStateObject) {
    let beaconAddPhotoInputElement = document.getElementById("beacon-add-photo-input");
    let beaconAddPhotoFormElement = document.getElementById("beacon-add-photo-form");
    beaconAddPhotoFormElement.addEventListener("submit", function(event) {
        event.preventDefault();
        if (beaconAddPhotoInputElement.files[0].size > 5000000) {
            showError("File should be less then 5 mg");
            return;
        }
        var form = new FormData(document.forms.beaconPhotos);
        let insertedPhoto = addPhoto(mapStateObject["selected"]["beacon"]["id"], form);
        if(insertedPhoto)
            addPhotoRender(insertedPhoto);
        beaconAddPhotoFormElement.reset();
    });

    let beaconAddFormElement = document.getElementById("beacon-add-form");
    beaconAddFormElement.addEventListener("submit", function(event) {
        event.preventDefault();
        let x = mapStateObject["selected"]["x"];
        let y = mapStateObject["selected"]["y"];
        var form = new FormData(document.forms.beacon);
        form.set("coord_x", x);
        form.set("coord_y", y);
        let insertedBeaconId = addBeaconToServer(form);
        if(insertedBeaconId) {
            let beacon = {"id": insertedBeaconId, "coord_x": x, "coord_y": y}
            mapStateObject["beacons"].push(beacon);
            mapStateObject["selected"]["beacon"] = beacon;
            viewBeacon(beacon);
        }
        beaconAddFormElement.reset();
    });
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

function addCanvasListener(mapStateObject, ctx) {
    let canvas = document.getElementById('canvas');

    canvas.addEventListener('click', function(event) {
        let canvasCoords = getCoords(canvas);
        let clickedX = event.pageX - canvasCoords[1];
        let clickedY = event.pageY - canvasCoords[0];

        let x = div(clickedX, size);
        let y = div(clickedY, size);

        if (mapStateObject["selected"]) {
            undoSelected(mapStateObject, ctx);
        }

        let realCoords = shiftCoords(width/2, height/2, mapStateObject["centerX"], mapStateObject["centerY"], x, y);

        let selectedBeacons = mapStateObject["beacons"].filter(function(beacon) {
            return realCoords[0] === beacon.coord_x && realCoords[1] === beacon.coord_y;
        });

        if (selectedBeacons.length > 0) {
            mapStateObject["selected"] = {"x": realCoords[0], "y": realCoords[1], "beacon": selectedBeacons[0]};
        } else {
            mapStateObject["selected"] = {"x": realCoords[0], "y": realCoords[1], "beacon": undefined};
        }
        renderSelected(mapStateObject, ctx);
    });
}

function addPhotoRender(photo) {
    let imgDiv = document.createElement("div");

    let img = document.createElement("img");
    img.setAttribute("src", "/Beacon/GetPhoto/" + photo["id"]);
    imgDiv.appendChild(img);

    let imgLabel = document.createElement("label");
    imgLabel.innerHTML = photo["name"];
    imgDiv.appendChild(imgLabel);

    let beaconPhotosElement = document.getElementById("beacon-photos");
    beaconPhotosElement.appendChild(imgDiv);
}

function addBeacon() {
    hideSidebarsCards();
    showSidebarsCard("add-beacon");
}

function viewBeacon(beacon) {
    hideSidebarsCards();
    showSidebarsCard("beacon");

    let beaconInfo = getBeacon(beacon.id);
    if (!beaconInfo) {
        return;
    }
    let beaconNameElement = document.getElementById("beacon-name");
    beaconNameElement.innerHTML = beaconInfo.name;

    let beaconCommentElement = document.getElementById("beacon-comment");
    beaconCommentElement.innerHTML = beaconInfo.Comment;

    let beaconPhotosElement = document.getElementById("beacon-photos");
    beaconPhotosElement.innerHTML = "";

    beaconInfo.photos.forEach(function(photo) {
        addPhotoRender(photo);
    });
}

function getExif() {
    var img1 = document.getElementById("img1");
    EXIF.getData(img1, function() {
        var make = EXIF.getTag(this, "Make");
        var model = EXIF.getTag(this, "Model");
        var makeAndModel = document.getElementById("makeAndModel");
        makeAndModel.innerHTML = `${make} ${model}`;
    });

//    var img2 = document.getElementById("img2");
//    EXIF.getData(img2, function() {
//        var allMetaData = EXIF.getAllTags(this);
//        var allMetaDataSpan = document.getElementById("allMetaDataSpan");
//        allMetaDataSpan.innerHTML = JSON.stringify(allMetaData, null, "\t");
//    });
}

function getBeacons(centerX, centerY) {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", "/GetBeacons?center_coord_x=" + centerX + "&center_coord_y=" + centerY, false);
    xhr.send();
    if (xhr.status != 200) {
        showError("Could not get beacons. Try again.");
    } else {
        return JSON.parse(xhr.responseText)["beacons"];
    }
}

function getBeacon(beacon) {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", "/Beacon/" + beacon, false);
    xhr.send();
    if (xhr.status != 200) {
        showError("Could not get beacon. Try again.");
    } else {
        return JSON.parse(xhr.responseText);
    }
}

function addPhoto(beaconId, formData) {
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/Beacon/AddPhoto/" + beaconId, false);
    xhr.send(formData);
    if (xhr.status != 200) {
        showError("Could not add photo. Try again.");
    } else {
        let response = JSON.parse(xhr.responseText);
        if (response.error) {
            showError(response.error);
        } else {
            return response;
        }
    }
}

function addBeaconToServer(formData) {
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/Beacon/Add", false);
    xhr.send(formData);
    if (xhr.status != 200) {
        showError("Could not add beacon. Try again.");
    } else {
        return JSON.parse(xhr.responseText)["inserted_id"];
    }
}

let errorTimer = undefined;

function showError(text) {
    if (errorTimer) {
        clearTimeout(errorTimer);
    }
    var elem = document.getElementById('error');
    elem.innerHTML = text;
    errorTimer = setTimeout(hideError, 2000)
}

function hideError() {
    var elem = document.getElementById('error');
    elem.innerHTML = "";
}

function init(centerXStr, centerYStr){
    gen = new SimplexNoise('fkjwes');//ololol
    let centerX = parseInt(centerXStr);
    let centerY = parseInt(centerYStr);

	var ctx = document.getElementById('canvas').getContext("2d");
	let mapStateObject = {"beacons": [], "selected": undefined, "centerX": centerX, "centerY": centerY};

	let beacons = renderFullMap(mapStateObject, ctx);
	let centerCoords = [centerX, centerY];
    addButtonListeners(mapStateObject, ctx);
    addCanvasListener(mapStateObject, ctx)
    addFormsListener(mapStateObject);
}