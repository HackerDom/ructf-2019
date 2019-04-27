'use strict';

const stylesCount = 4;
const height = 30;
const width = 50;
const freq = 0.1;
const exponent = 2;
const size = 20;
const delta = 10;
const stylesMap = {0: "#352D64", 1: "#6C2D6A", 2: "#933B92", 3: "#D35092", 4: "#EE82EE"};
const beaconStyle = "#ff3041";
const selectedBeaconStyle = "#f9e902";
const selectedCellStyle = "#002eff";
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
    document.getElementById("invite-button").onclick = function(e) {
        let inviteForm = document.getElementById("write-invite-form");
        inviteForm.classList.remove("hidden");
        e.target.classList.add("hidden");
    };
    document.getElementById("go-to-profile").onclick = function() {
        if (mapStateObject["selected"]) {
            undoSelected(mapStateObject, ctx);
        }
        mapStateObject["selected"] = undefined;
        viewProfile(mapStateObject, ctx);
    };
    document.getElementById("go-to-latest").onclick = function() {
        if (mapStateObject["selected"]) {
            undoSelected(mapStateObject, ctx);
        }
        mapStateObject["selected"] = undefined;
        viewLatest(mapStateObject, ctx);
    };
}

function addFormsListener(mapStateObject, ctx) {
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

            let coords = shiftCoords(mapStateObject["centerX"], mapStateObject["centerY"],
                                 width/2, height/2,
                                 mapStateObject["selected"]["x"], mapStateObject["selected"]["y"]);

            renderRect(coords[0]*size + 1, coords[1]*size + 1, size - 2, selectedBeaconStyle, ctx);

            viewBeacon(beacon);
        }
        beaconAddFormElement.reset();
    });

    let inviteFormElement = document.getElementById("write-invite-form");
    inviteFormElement.addEventListener("submit", function(event) {
        event.preventDefault();
        var form = new FormData(document.forms.beaconInvite);
        let openedBeacon = getBeaconByInvite(form);
        if (openedBeacon) {
            let beacon = {"id": openedBeacon.id, "coord_x": openedBeacon.x, "coord_y": openedBeacon.y}
            mapStateObject["selected"] = {"beacon": beacon, "x": openedBeacon.x, "y": openedBeacon.y}

            mapStateObject["centerX"] = openedBeacon.x;
            mapStateObject["centerY"] = openedBeacon.y;
            renderFullMap(mapStateObject, ctx);
        }
        inviteFormElement.reset();
        inviteFormElement.classList.add("hidden");
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
    imgDiv.classList.add("img-photo");

    let imgLabel = document.createElement("div");
    imgLabel.classList.add("img-label-container");
    imgLabel.classList.add("img-label");

    let nameLabel = document.createElement("div");
    nameLabel.innerHTML = "name: " + photo["name"];
    imgLabel.appendChild(nameLabel);

    let deviceLabel = document.createElement("div");
    imgLabel.appendChild(deviceLabel);

    let dateLabel = document.createElement("div");
    imgLabel.appendChild(dateLabel);

    let img = document.createElement("img");
    img.classList.add("img-little");
    img.setAttribute("src", "/Photo/GetPhoto/" + photo["id"]);

    let imgB = document.createElement("img");
    imgB.classList.add("img-big");
    imgB.setAttribute("src", "/Photo/GetPhoto/" + photo["id"]);

    img.onload = function() {
        setDeviceModel(img, deviceLabel, dateLabel);
    }
    imgDiv.appendChild(img);
    imgDiv.appendChild(imgLabel);
    imgDiv.appendChild(imgB);

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

    let inviteForm = document.getElementById("write-invite-form");
    inviteForm.classList.add("hidden");
    let beaconAdditional = document.getElementById("beacon-additional");
    beaconAdditional.classList.add("hidden");

    let beaconPhotosElement = document.getElementById("beacon-photos");
    beaconPhotosElement.innerHTML = "";
    let beaconAddPhotosFormElement = document.getElementById("beacon-add-photo-form")

    let beaconNameElement = document.getElementById("beacon-name");
    beaconNameElement.innerHTML = beaconInfo.name;

    let beaconCommentElement = document.getElementById("beacon-comment");
    beaconCommentElement.innerHTML = beaconInfo.comment;

    if (beaconInfo.invite) {
        beaconAdditional.innerHTML =
        "This beacon is private.<br/> If you want share it to other people, send them this code: </br><span class=\"bold\">" +
            beaconInfo.invite + "</span>."
        beaconAdditional.classList.remove("hidden");
    }

    if (beaconInfo.is_private) {
        beaconAddPhotosFormElement.classList.add("hidden");
        beaconPhotosElement.innerHTML = "You could not access to this beacon. If you want, you can ask " +
            beaconInfo.creator + " to share it."
    } else {
        beaconAddPhotosFormElement.classList.remove("hidden");
        beaconInfo.photos.forEach(function(photo) {
            addPhotoRender(photo);
        });
    }
}

function viewProfile(mapStateObject, ctx) {
    hideSidebarsCards();
    showSidebarsCard("profile");
    let profileBeaconsElement = document.getElementById("profile-beacons");
    profileBeaconsElement.innerHTML = "";

    let inviteButtonElement = document.getElementById("invite-button");
    inviteButtonElement.classList.remove("hidden");

    let inviteFormElement = document.getElementById("write-invite-form");
    inviteFormElement.classList.add("hidden");

    let userBeacons = getUserBeacons();
    userBeacons.forEach(function(beacon) {
        let beaconDiv = document.createElement("div");
        beaconDiv.classList.add("profile-beacon");

        let buttonGoToBeacon = document.createElement("button");
        buttonGoToBeacon.innerHTML = "Go to beacon";
        buttonGoToBeacon.classList.add("button-button");
        buttonGoToBeacon.addEventListener('click', function(event) {
            let selected = {"id": beacon.id, "coord_x": beacon.x, "coord_y": beacon.y}

            mapStateObject["beacons"] = [];
            mapStateObject["selected"] = {"x": beacon.x, "y": beacon.y, "beacon": selected};
            mapStateObject["centerX"] = beacon.x;
            mapStateObject["centerY"] = beacon.y;
            renderFullMap(mapStateObject, ctx);
        });

        let beaconNameDiv = document.createElement("div");
        beaconNameDiv.innerHTML = beacon.name;
        beaconDiv.appendChild(beaconNameDiv);
        beaconDiv.appendChild(buttonGoToBeacon);

        profileBeaconsElement.appendChild(beaconDiv);
    });
    if (userBeacons.length == 0) {
        profileBeaconsElement.innerHTML = "You have no beacons yet."
    }
}

function viewLatest(mapStateObject, ctx) {
    let latestPhotosElement = document.getElementById("latest-photos");
    latestPhotosElement.innerHTML = "";
    hideSidebarsCards();
    showSidebarsCard("latest");

    let latest = getLatest();
    latest.forEach(function(photo) {
        let imgDiv = document.createElement("div");
        imgDiv.classList.add("latest-img");

        let buttonGoToBeacon = document.createElement("button");
        buttonGoToBeacon.innerHTML = "Go to beacon";
        buttonGoToBeacon.classList.add("button-button");
        buttonGoToBeacon.classList.add("button-latest");
        buttonGoToBeacon.addEventListener('click', function(event) {
            let beacon = getBeacon(photo["beaconId"]);
            let selected = {"id": photo["beaconId"], "coord_x": beacon.x, "coord_y": beacon.y}

            mapStateObject["beacons"] = [];
            mapStateObject["selected"] = {"x": beacon.x, "y": beacon.y, "beacon": selected};
            mapStateObject["centerX"] = beacon.x;
            mapStateObject["centerY"] = beacon.y;
            renderFullMap(mapStateObject, ctx);
        });

        let img = document.createElement("img");
        img.classList.add("img-little");
        img.setAttribute("src", "/Photo/GetPhoto/" + photo["id"]);

        let imgB = document.createElement("img");
        imgB.classList.add("img-big");
        imgB.setAttribute("src", "/Photo/GetPhoto/" + photo["id"]);

        imgDiv.appendChild(img);
        imgDiv.appendChild(imgB);
        imgDiv.appendChild(buttonGoToBeacon);

        latestPhotosElement.appendChild(imgDiv);
    });
    if (latest.length == 0) {
        latestPhotosElement.innerHTML = "There is nothing here yet."
    }
}

function setDeviceModel(imgElement, deviceModelElement, sizeElement) {
    EXIF.getData(imgElement, function() {
        var make = EXIF.getTag(this, "Make");
        var model = EXIF.getTag(this, "Model");
        var date = EXIF.getTag(this, "DateTime");
        if (make && model) {
            deviceModelElement.innerHTML = `device: ${make} ${model}`;
        }
        if (date) {
            sizeElement.innerHTML = `date: ${date}`;
        }
    });
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
        let response = JSON.parse(xhr.responseText);
        if (response.error) {
            showError(response.error);
        } else {
            return response["upserted_id"];
        }
    }
}

function getLatest() {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", "/Photo/GetLatest", false);
    xhr.send();
    if (xhr.status != 200) {
        showError("Could not get latest photos. Try again.");
    } else {
        return JSON.parse(xhr.responseText)["latest_photos"];
    }
}

function getUserBeacons() {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", "/Beacon/GetUserBeacons", false);
    xhr.send();
    if (xhr.status != 200) {
        showError("Could not get beacons. Try again.");
    } else {
        return JSON.parse(xhr.responseText)["beacons"];
    }
}

function getBeaconByInvite(formData) {
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/Beacon/Invite", false);
    xhr.send(formData);
    if (xhr.status != 200) {
        showError("Could not find beacon for this code. Try again.");
    } else {
        let response = JSON.parse(xhr.responseText);
        if (response.error) {
            showError(response.error);
        } else {
            return response;
        }
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
    addFormsListener(mapStateObject, ctx);
    viewProfile(mapStateObject, ctx);
}