var args = require('system').args;

var mainURL = null;
var logphantom = false;

if (args.length === 1) {
    //console.log('Try to pass some arguments when invoking this script!');
} else {
    mainURL = args[1];
    UserName = args[2];
    Domain = args[3];
    WindowName = args[4];
    if (args[5] === "true") {
        logphantom = true;
    }
}

var page = require('webpage').create();
var wrapper = 'wrapper.html';

//console.log(new Date());

page.onCallback = function (param) {
    var date = new Date();
    //console.log(date + ' ' + param.command);
    if (param.command === 'exit') {
        phantom.exit();
    } else if (param.command === 'render') {

        param.filename = "captures/" + param.filename;

        page.render(param.filename);
        console.log(date + ' rendered: ' + param.filename);

    } else {

        if (logphantom === true) {
            console.log(date + ' ' + param.log);
        }
    }
};

var counter = 0;
page.onLoadFinished = function (status) {
    //console.log(status);
    if (status === 'success') {
        if (counter > 0) {
            return;
        }
        counter++;

        if (logphantom === true) {
            page.onResourceRequested = function (request) {
                console.log('Request ' + JSON.stringify(request, undefined, 4));
            };
        }

        page.evaluate(function (page, mainURL, UserName, Domain, WindowName) {
            var timerorders = window.setInterval(function () { GetOrders(page, mainURL, UserName, Domain, WindowName); }, 1);
            var timerframes = window.setInterval(function () { CheckIfFrameLoaded(); }, 1);
        }, page, mainURL, UserName, Domain, WindowName);
    }
};

page.open(wrapper);
