const alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
let timerId = -1;


function genRandString(length) {
    let res = "";
    for (let i = 0; i < length; i++)
        res += alpha.charAt(Math.floor(Math.random() * alpha.length));
    return res;
}

let token = genRandString(20);
let interval = 100;

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
            console.log(":" + data);
            let task = JSON.parse(data);
            console.log(task);
            if (task.Status === 0) {
                $("#stdout-div").css("display", "block");
                $("#stdout-out-fld").text(task.Stdout);
                $("#noTrespassingOuterBarG").css("display", "none");
                clearInterval(timerId);
            } else if (task.Status === 2) {
                $("#error-out-fld").text("Executing error: " + task.Error);
                $("#error-div").css("display", "block");
                clearInterval(timerId);
            }
        },
        dataType: "text",
    });
    clearInterval(timerId);
    interval = interval * 2;
    timerId = setInterval(checkTask, interval, taskId);
}

$("#run-btn").click(function() {
    runTask();
});
