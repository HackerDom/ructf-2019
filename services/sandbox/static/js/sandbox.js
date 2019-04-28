
$("#create-btn").click(
    function () {
        var uuid = $("#uuid-fld").val();
        var mind = $("#mind-fld").val();
        if (!uuid || !mind) {
            alert("UUID or Mind is not set.");
            return;
        }
        if (!uuid.match(/[0-9a-fA-F]{32}/)) {
            alert("Invalid UUID format. Expected: [0-9a-fA-F]{32}");
            return;
        }
        $.ajax({
            type: "POST",
            url: `/create?uuid=${uuid}&mind=${mind}`,
            success: function (data, status, obj) {
                alert("Unit added!")
            },
            error: function (data, status, obj) {
                alert("Error: " + data.status)
            },
            dataType: "text",
        });
    }
);


$("#track-btn").click(
    function () {
        var uuid = $("#uuid-fld-track").val();
        if (!uuid) {
            alert("UUID is not set.");
            return;
        }
        if (!uuid.match(/[0-9a-fA-F]{32}/)) {
            alert("Invalid UUID format. Expected: [0-9a-fA-F]{32}");
            return;
        }
        $.ajax({
            type: "GET",
            url: `/track?uuid=${uuid}`,
            success: function (data, status, obj) {
                var stats = JSON.parse(data);
                alert(`Unit stats:\n \u2022 mind: ${stats.mind}\n\n \u2022 X: ${stats.posX}\n \u2022 Y: ${stats.posY}\n \u2022 Z: ${stats.posZ}\n\n \u2022 power: ${stats.power}`);
            },
            error: function (data, status, obj) {
                alert("Error: " + data.status)
            },
            dataType: "text",
        });
    }
);
