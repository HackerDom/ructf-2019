const alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
let timerId = -1;
const Mode = Object.freeze({
    "work": 1, "login": 2, "registration": 3
});

let token = genRandString(20);
let interval = 100;

let loginFrom = $("#login-form");
let workPage = $("#work-page");
let loginLabel = $("#login-label");
let registerLabel = $("#register-label");
let loginButton = $("#login-btn");
let registerButton = $("#register-btn");
let registerHead = $("#reg-head");
let loginHead = $("#login-head");
let passwordField = $("#password-fld");
let idField = $("#id-fld");

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
        "source": $("#src-fld").val(),
        "stdin": $("#stdin-fld").val(),
        "token": token,
    });
    $("#noTrespassingOuterBarG").css("display", "block");
    $.ajax({
        type: "POST",
        url: "/run_task",
        data: data,
        success: function (data, status, obj) {
            $("#error-out-fld").text("");
            $("#stdout-out-fld").text("");
            $("#stdout-div").css("display", "none");
            $("#error-div").css("display", "none");
            interval = 100;
            timerId = setInterval(checkTask, interval, JSON.parse(data).taskId);
        },
        error: function(data, status, obj) {
            if (data.status / 100 === 4) {
                clearCookies();
                location.reload();
            }
            $("#noTrespassingOuterBarG").css("display", "none");
            $("#error-out-fld").text("Could not connect to server.");
            $("#error-div").css("display", "block");
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
                $("#stdout-div").css("display", "block");
                $("#stdout-out-fld").text(task.Stdout);
                $("#noTrespassingOuterBarG").css("display", "none");
                clearInterval(timerId);
            } else if (task.Status === 2) {
                $("#error-out-fld").text("Executing error: " + task.Error);
                $("#error-div").css("display", "block");
                $("#noTrespassingOuterBarG").css("display", "none");
                clearInterval(timerId);
            }
        },
        dataType: "text",
    });
    clearInterval(timerId);
    interval = interval * 2;
    timerId = setInterval(checkTask, interval, taskId);
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
        $("#prompt-header").text("Hello, user with id=" + getUid() + "! Execute your BrainHug code here!");
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
        password = passwordField.val();
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
        let userId = idField.val();
        let password = passwordField.val();
        if (userId === "" || password === "") {
            alert("Empty user id and/or password.");
            return;
        }
        $.ajax({
            type: "GET",
            url: "/check?userid=" + userId + "&password=" + password,
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

if (getUid() !== undefined) {
    setInterface(Mode.work);
} else {
    setInterface(Mode.login);
}
