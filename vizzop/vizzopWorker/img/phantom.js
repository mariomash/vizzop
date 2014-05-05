var args = require('system').args;

var mainURL = null;
var logphantom = false;

if (args.length === 1) {
    console.log('Try to pass some arguments when invoking this script!');
} else {
    mainURL = args[1];
    if (args[2] === "true") {
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

        console.log('rendered:' + param.filename);
        /*
        window.setTimeout(function () {
        }, 500);
        */
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

        /*
        if (logphantom === true) {
            page.onResourceRequested = function (request) {
                console.log('Request ' + JSON.stringify(request, undefined, 4));
            };
        }
        */

        page.evaluate(function (page, mainURL) {
            var timerorders = window.setInterval(function () { GetOrders(page, mainURL); }, 1);
            var timerframes = window.setInterval(function () { CheckIfFrameLoaded(); }, 1000);
        }, page, mainURL);
    }
};

page.open(wrapper);
