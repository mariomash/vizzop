var args = require('system').args;

var mainURL = null;

if (args.length === 1) {
    console.log('Try to pass some arguments when invoking this script!');
} else {
    mainURL = args[1];
    UserName = args[2];
    Domain = args[3];
}

var page = require('webpage').create();
var wrapper = 'wrapper.html';

page.onCallback = function (param) {
    var date = new Date();
    //console.log(date + ' ' + param.command);
    if (param.command == 'exit') {
        phantom.exit();
    } else if (param.command == 'iframeloaded') {

        /*
        var bb = page.evaluate(function () {
            return document.getElementById("WrapperIframe");
        });
        */
        //console.log(param.Height);

        /*
                page.viewportSize = {
                    width: param.Width,
                    height: param.Height
                };
                */
        /*
        page.clipRect = {
            top: param.ScrollTop,
            left: param.ScrollLeft,
            width: param.Width,
            height: param.Width
        };
        */
        /*
        page.evaluate(function (param) {


            window.callPhantom({
                'command': 'render',
                'filename': "captures/" + param.filename
            });

        }, param);
        */

        param.filename = "captures/" + param.filename;

        page.render(param.filename);
        console.log(date + ' rendered: ' + param.filename);
        page.evaluate(function () {
            OrdersRequest_InCourse = null;
        });

    } else {
        console.log(date + ' ' + param.log);
    }
}

var counter = 0;
page.onLoadFinished = function (status) {
    //console.log(status);
    if (status == 'success') {
        if (counter > 0) {
            return;
        }
        counter++;

        page.evaluate(function (page, mainURL, UserName, Domain) {

            var timer = window.setInterval(function () { GetOrders(page, mainURL, UserName, Domain); }, 1);

        }, page, mainURL, UserName, Domain);
    }
}

page.open(wrapper);