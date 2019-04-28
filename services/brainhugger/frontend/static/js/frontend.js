const alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
let timerId = -1;
const Mode = Object.freeze({
    "work": 1, "login": 2, "registration": 3
});

let token = genRandString(20);
let interval = 100;

let loginFrom = $("#login-form");
let workPage = $("#brainhugger");
let loginLabel = $("#login-label");
let registerLabel = $("#register-label");
let loginButton = $("#login-btn");
let registerButton = $("#register-btn");
let registerHead = $("#reg-head");
let loginHead = $("#login-head");
let passwordField = $("#password-fld");
let idField = $("#id-fld");
let bhSource = $("#bh-source");
let bhStdout = $("#bh-stdout");
let logoutButton = $("#logout-btn");


function genRandString(length) {
    let res = "";
    for (let i = 0; i < length; i++)
        res += alpha.charAt(Math.floor(Math.random() * alpha.length));
    return res;
}

function hideObj(obj) {
    obj.css("display", "none");
}

function showObj(obj) {
    obj.css("display", "block");
}

function clearCookies() {
    document.cookie = "uid=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    document.cookie = "secret=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
}

function runTask() {
    let data = JSON.stringify({
        "source": bhSource.val(),
        "stdinb64": btoa($("#bh-stdin").val()),
        "token": token,
    });
    $("#noTrespassingOuterBarG").css("display", "block");
    $.ajax({
        type: "POST",
        url: "/run_task",
        data: data,
        success: function (data, status, obj) {
            // show bar
            unsetError();
            bhStdout.text("");
            interval = 100;
            timerId = setInterval(checkTask, interval, JSON.parse(data).taskId);
        },
        error: function(data, status, obj) {
            if (data.status / 100 === 4) {
                clearCookies();
                location.reload();
            }
            // hide bar
        },
        dataType: "text",
    });
}

function checkTask(taskId) {
    $.ajax({
        type: "GET",
        url: "/task_info/" + taskId.toString() + "?token=" + token,
        success: function (data, status, obj) {
            let task = JSON.parse(data);
            if (task.Status === 0) {
                bhStdout.text(atob(task.Stdoutb64));
                // hide bar
                clearInterval(timerId);
            } else if (task.Status === 2) {
                setError();
                bhStdout.text(task.Error);
                // hide bar
                clearInterval(timerId);
            }
        },
        dataType: "text",
    });
    clearInterval(timerId);
    interval = interval * 2;
    timerId = setInterval(checkTask, interval, taskId);
}

function setError() {
    bhStdout.removeClass("st30");
    bhStdout.addClass("bh-text-err");
}

function unsetError() {
    bhStdout.removeClass("bh-text-err");
    bhStdout.addClass("st30");
}

function getUid() {
    let res = undefined;
    document.cookie.split("; ").forEach(function (rawCookie) {
        if (rawCookie.startsWith("uid=")) {
            res = parseInt(rawCookie.substring(4));
        }
    });
    return res;
}

function setInterface(mode) {
    if (mode === Mode.work) {
        hideObj(loginFrom);
        showObj(workPage);
        bhSource.text("User id=" + getUid() + "! Hug your mind with it!\n++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.");
    } else {
        hideObj(workPage);
        showObj(loginFrom);
        if (mode === Mode.login) {
            showObj(loginLabel);
            showObj(loginButton);
            showObj(idField);
            hideObj(registerHead);
            showObj(loginHead);
            hideObj(registerLabel);
            hideObj(registerButton);
        } else {
            showObj(registerLabel);
            showObj(registerButton);
            showObj(registerHead);
            hideObj(loginLabel);
            hideObj(loginButton);
            hideObj(idField);
            hideObj(loginHead);
        }
    }
}

$("#run-btn").click(function() {
    runTask();
});

$("#reg-link").click(function () {
    setInterface(Mode.registration);
});

$("#login-link").click(function () {
    setInterface(Mode.login);
});

registerButton.click(
    function () {
        let password = passwordField.val();
        if (password === "") {
            alert("Password is empty!");
            return;
        }
        let data = JSON.stringify({
            "password": password,
        });
        $.ajax({
            type: "POST",
            url: "/register",
            data: data,
            success: function (data, status, obj) {
                setInterface(Mode.work);
            },
            error: function (data, status, obj) {
                console.log("error");
                console.log(data, status, obj);
            },
            dataType: "text",
        });
    }
);

loginButton.click(
    function () {
        let userId = parseInt(idField.val());
        let password = passwordField.val();
        if (userId === 0 || password === "") {
            alert("Empty user id and/or password.");
            return;
        }
        let data = JSON.stringify({
            "userId": userId,
            "password": password,
        });
        $.ajax({
            type: "POST",
            url: "/login",
            data: data,
            success: function (data, status, obj) {
                setInterface(Mode.work);
            },
            error: function (data, status, obj) {
                alert("Invalid userId and/or password.")
            },
            dataType: "text",
        });
    }
);

function addSymbol(c) {
    bhSource.text(bhSource.val() + c);
}

let id2ops = {
    "inc": "+",
    "phl": "+",
    "pvl": "+",
    "dec": "-",
    "mns": "-",
    "rht": ">",
    "grt": ">",
    "lbr": "[",
    "lfb": "[",
    "scn": ",",
    "col": ",",
    "prt": ".",
    "dot": ".",
    "rbr": "]",
    "rhb": "]",
    "lft": "<",
    "lst": "<",
};

["#backspace-btn", "#backspace-ar1-btn", "#backspace-ar2-btn"].forEach(function (btnId) {
    $(btnId).click(function () {
        bhSource.text(bhSource.val().substring(0, bhSource.val().length - 1));
    })
});

["#del-btn", "#del1-btn", "#del2-btn", "#del3-btn"].forEach(function (btnId) {
    $(btnId).click(function () {
        bhSource.text("");
    })
});

["#hug-btn", "#hb1", "#hb2", "#hb3", "#hb4", "#hb5"].forEach(function (btnId) {
    $(btnId).click(function () {
        runTask();
    })
});

Object.keys(id2ops).forEach(function (buttonIdPregix) {
    $("#" + buttonIdPregix + "-b").click(function (event) {
        let idPref = event.target.id.substr(0, 3);
        bhSource.text(bhSource.val() + id2ops[idPref]);
    })
});

logoutButton.click(function () {
    clearCookies();
    location.reload();
});

if (getUid() !== undefined) {
    setInterface(Mode.work);
} else {
    setInterface(Mode.login);
}
