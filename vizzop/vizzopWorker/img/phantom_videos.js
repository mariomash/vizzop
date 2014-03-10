var args = require('system').args;

var mainURL = null;

if (args.length === 1) {
    console.log('Try to pass some arguments when invoking this script!');
} else {
    mainURL = args[1];
    UserName = args[2];
    Domain = args[3];
    Password = args[4];
    GUID = args[5];
}

var page = require('webpage').create();
var wrapper = 'wrapper_videos.html';

page.onCallback = function (param) {
    var date = new Date();
    //console.log(date + ' ' + param.command);
    if (param.command == 'exit') {
        phantom.exit();
    } else if (param.command == 'iframeloaded') {

        param.filename = "captures_video/" + param.filename;

        page.render(param.filename);
        //console.log(date + ' rendered: ' + param.filename);
        phantom.exit();

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
        
        page.evaluate(function (page, mainURL, UserName, Domain, Password, GUID) {

            GetScreenCapture(page, mainURL, UserName, Domain, Password, GUID);

        }, page, mainURL, UserName, Domain, Password, GUID);
    }
}

page.open(wrapper);