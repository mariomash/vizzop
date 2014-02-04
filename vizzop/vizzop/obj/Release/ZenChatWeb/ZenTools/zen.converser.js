
/*
// Converser Class
var Converser = jQuery.zs_Class.create({
    // constructor
    initialize: function (username, fullname) {
        this._username = username;
        this._fullname = fullname;
    },
    // methods
    Update: function () {
        var self = this;
        var msg = {
            'UserName': self._username,
            'FullName': self._fullname
        };
        var request = jQuery.ajax({
            url: zentools.mainURL + "/Converser/Edit",
            type: "POST",
            data: msg,
            dataType: "jsonp"
        });

        //TODO asegurarse de que esto furula
        request.done(function (msg) {
            if (msg === true) { } else { }
        });

        request.fail(function (jqXHR, textStatus) {
            log("Error Converser.Update: " + textStatus);
            self._sent = false;
        });
    }
});
*/
