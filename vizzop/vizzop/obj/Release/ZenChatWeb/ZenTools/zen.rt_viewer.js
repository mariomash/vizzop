
var rt_update_seconds;
var ajaxrequest = null;

function rt_Countdown() {
    if (rt_update_seconds > -1) {
        rt_update_seconds--;
        setTimeout(rt_Countdown, 1000); return;
    }
    rt_update(Agent_UserName, Agent_Password, UserName, ID);
    rt_update_seconds = new Number(1);
    setTimeout(rt_Countdown, 1000);
}

function rt_update(Agent_UserName, Agent_Password, UserName, ID) {
    if (ajaxrequest !== null) {
        return;
    }
    try {
        var msg = {
            'Agent_UserName': Agent_UserName,
            'Agent_Password': Agent_Password,
            'UserName': UserName,
            'ID': ID
        };
        ajaxrequest = jQuery.ajax({
            url: mainURL + "/RealTime/CheckRealtimeChanged",
            type: "POST",
            data: msg,
            dataType: "jsonp",
            beforeSend: function (xhr) {
            },
            success: function (data) {
                ajaxrequest = null;
                if (data === true) {
                    var url = mainURL + '/ZenTools/RemoteView.ashx?Agent_UserName=' + Agent_UserName + '&Agent_Password=' + Agent_Password + '&UserName=' + UserName;
                    window.location.href = url;
                    //console.log(window.location.href);
                }
            }
        });
        ajaxrequest.fail(function (jqXHR, textStatus) {
            ajaxrequest = null;
        });
    } catch (err) {
        ajaxrequest = null;
    }
}

$(document).ready(function () {
    rt_update_seconds = new Number(1);
    setTimeout(rt_Countdown, 1000);
});