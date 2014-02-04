
function getLocation(href) {
    var l = document.createElement("a");
    l.href = href;
    return l;
};

//var zentools = null;

String.prototype.dateFromJSON = function () {
    return eval(this.replace(/\/Date\((\d+)\)\//gi, "new Date($1)"));
};

var MutationObserver = window.MutationObserver || window.WebKitMutationObserver || window.MozMutationObserver;

function isDOMAttrModifiedSupported() {
    var p = document.createElement('p');
    var flag = false;

    if (p.addEventListener) p.addEventListener('DOMAttrModified', function () {
        flag = true
    }, false);
    else if (p.attachEvent) p.attachEvent('onDOMAttrModified', function () {
        flag = true
    });
    else return false;

    p.setAttribute('id', 'target');

    return flag;
}

/*
jQuery.fn.DOMchange = function (callback) {
if (MutationObserver) {
var options = {
subtree: false,
attributes: true
};

var observer = new MutationObserver(function (mutations) {
mutations.forEach(function (e) {
callback.call(e.target, e.attributeName);
});
});

return this.each(function () {
observer.observe(this, options);
});

} else if (isDOMAttrModifiedSupported()) {
return this.on('DOMAttrModified', function (e) {
callback.call(this, e.attrName);
});
} else if ('onpropertychange' in document.body) {
return this.on('propertychange', function (e) {
callback.call(this, window.event.propertyName);
});
}
}
*/

//
//  ON DOCUMENT CHANGE
//  Makes it possible to execute code when the DOM changes.
//
//  Licensed under the terms of the MIT license.
//  (c) 2010 Balázs Galambosi
//

/*
(function (window) {

    var last = +new Date();
    var delay = 100; // default delay

    // Manage event queue
    var stack = [];

    function callback() {
        var now = +new Date();
        if (now - last > delay) {
            for (var i = 0; i < stack.length; i++) {
                stack[i]();
            }
            last = now;
        }
    }

    // Public interface
    var onDomChange = function (fn, newdelay) {
        if (newdelay)
            delay = newdelay;
        stack.push(fn);
    };

    // Naive approach for compatibility
    function naive() {

        var last = document.getElementsByTagName('*');
        var lastlen = last.length;
        var timer = setTimeout(function check() {

            // get current state of the document
            var current = document.getElementsByTagName('*');
            var len = current.length;

            // if the length is different
            // it's fairly obvious
            if (len !== lastlen) {
                // just make sure the loop finishes early
                last = [];
            }

            // go check every element in order
            for (var i = 0; i < len; i++) {
                if (current[i] !== last[i]) {
                    callback();
                    last = current;
                    lastlen = len;
                    break;
                }
            }

            // over, and over, and over again
            setTimeout(check, delay);

        }, delay);
    }

    //
    //  Check for mutation events support
    //

    var support = {};

    var el = document.documentElement;
    var remain = 3;

    // callback for the tests
    function decide() {
        if (MutationObserver) {

            var options = {
                subtree: false,
                attributes: true
            };

            var observer = new MutationObserver(function (mutations) {
                mutations.forEach(function (e) {
                    //callback.call(e.target, e.attributeName);
                    callback.call(e);
                });
            });

            return this.each(function () {
                observer.observe(this, options);
            });

        } else if (support.DOMNodeInserted) {
            window.addEventListener("DOMContentLoaded", function () {
                if (support.DOMSubtreeModified) { // for FF 3+, Chrome
                    el.addEventListener('DOMSubtreeModified', callback, false);
                } else { // for FF 2, Safari, Opera 9.6+
                    el.addEventListener('DOMNodeInserted', callback, false);
                    el.addEventListener('DOMNodeRemoved', callback, false);
                }
            }, false);
        } else if (document.onpropertychange) { // for IE 5.5+
            document.onpropertychange = callback;
        } else { // fallback
            naive();
        }
    }

    // checks a particular event
    function test(event) {
        el.addEventListener(event, function fn() {
            support[event] = true;
            el.removeEventListener(event, fn, false);
            if (--remain === 0) decide();
        }, false);
    }

    // attach test events
    if (window.addEventListener) {
        test('DOMSubtreeModified');
        test('DOMNodeInserted');
        test('DOMNodeRemoved');
    } else {
        decide();
    }

    // do the dummy test
    var dummy = document.createElement("div");
    el.appendChild(dummy);
    el.removeChild(dummy);

    // expose
    window.onDomChange = onDomChange;

})(window);
*/

jQuery(document).bind('ready.js', function () {
    //log("[LOADED] ZEN Support");
    if (zentools.me) {
        zentools.me.LastActive = zentools.me.LastActive.dateFromJSON();
    }

    jQuery(document).mousemove(function (e) {
        zentools.mouseXPos = e.pageX;
        zentools.mouseYPos = e.pageY;
    });

    jQuery(window).scroll(function () {
    });

    jQuery(window).resize(function () {
    });
});

jQuery.fn.outerScrollHeight = function (includeMargin) {
    var element = this[0];
    var jElement = jQuery(element);
    var totalHeight = element.scrollHeight; //includes padding
    //totalHeight += parseInt(jElement.css("border-top-width"), 10) + parseInt(jElement.css("border-bottom-width"), 10);
    //if(includeMargin) totalHeight += parseInt(jElement.css("margin-top"), 10) + parseInt(jElement.css("margin-bottom"), 10);
    totalHeight += jElement.outerHeight(includeMargin) - jElement.innerHeight();
    return totalHeight;
};

jQuery.fn.outerScrollWidth = function (includeMargin) {
    var element = this[0];
    var jElement = jQuery(element);
    var totalWidth = element.scrollWidth; //includes padding
    //totalWidth += parseInt(jElement.css("border-left-width"), 10) + parseInt(jElement.css("border-right-width"), 10);
    //if(includeMargin) totalWidth += parseInt(jElement.css("margin-left"), 10) + parseInt(jElement.css("margin-right"), 10);
    totalWidth += jElement.outerWidth(includeMargin) - jElement.innerWidth();
    return totalWidth;
};

jQuery.fn.makeAbsolute = function (rebase) {
    return this.each(function () {
        var el = jQuery(this);
        var pos = el.position();
        el.css({
            position: "absolute",
            marginLeft: 0, marginTop: 0,
            top: pos.top, left: pos.left
        });
        if (rebase) {
            el.remove().appendTo("body");
        }
    });
}

/*
function get_time_zone_offset() {
    var current_date = new Date();
    return -current_date.getTimezoneOffset() / 60;
}
*/

function parseJsonDate(jsonDate) {
    var offset = new Date().getTimezoneOffset() * 60000;
    var parts = /\/Date\((-?\d+)([+-]\d{2})?(\d{2})?.*/.exec(jsonDate);

    if (parts[2] === undefined)
        parts[2] = 0;

    if (parts[3] === undefined)
        parts[3] = 0;

    //+ offset

    return new Date(+parts[1] + parts[2] * 3600000 + parts[3] * 60000);
};

function LoadCss(url) {
    var fileref = document.createElement('link');
    fileref.setAttribute('rel', 'stylesheet');
    fileref.setAttribute('type', 'text/css');
    fileref.setAttribute('href', url);

    if (typeof fileref !== 'undefined') {
        document.getElementsByTagName('head')[0].appendChild(fileref);
    }
}

function LoadJS(url) {
    var fileref = document.createElement('script');
    fileref.setAttribute('type', 'text/javascript');
    fileref.setAttribute('src', url);

    if (typeof fileref !== 'undefined') {
        document.getElementsByTagName('head')[0].appendChild(fileref);
    }
}

function randomnumber() {
    return Math.floor(Math.random() * 10000001);
}

function log(sText) {
    if (zentools.isDebug === true) {
        try {
            console.info(sText);
        } catch (err) { }
        return false;
    } else {
        try {

            var msg = {
                'text': zentools.clientIP + ": " + zentools.ApiKey + ": " + sText
            };

            var request = jQuery.ajax({
                url: zentools.mainURL + "/log/SaveFromAjax",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) { }
            });
            request.fail(function (jqXHR, textStatus) {
            });
        } catch (err) { }
    }
}

function LLang(ref, args) {
    var returnValue = "";
    try {
        jQuery.each(zentools.langStrings, function (i, v) {
            if (v.Ref === ref) {
                returnValue = v.Text;
                return;
            }
        });
        if (args !== null) {
            jQuery.each(args, function (i, v) {
                var toreplace = "%" + i;
                returnValue = returnValue.replace(eval("/%" + i + "/g"), v);
            });
        }
    } catch (err) {
        log(err);
    }
    if (returnValue === "") {
        returnValue = ref;
    }
    return returnValue;
}

// Added to make dates format to ISO8601
Date.prototype.toJSON = function (key) {
    function f(n) {
        // Format integers to have at least two digits.
        return n < 10 ? '0' + n : n;
    }

    return this.getUTCFullYear() + '-' +
         f(this.getUTCMonth() + 1) + '-' +
         f(this.getUTCDate()) + 'T' +
         f(this.getUTCHours()) + ':' +
         f(this.getUTCMinutes()) + ':' +
         f(this.getUTCSeconds()) + '.' +
         f(this.getUTCMilliseconds()) + 'Z';
};

/*
Date.prototype.toUTCArray = function () {
    var D = this;
    return [D.getUTCFullYear(), D.getUTCMonth(), D.getUTCDate(), D.getUTCHours(),
    D.getUTCMinutes(), D.getUTCSeconds()];
}

Date.prototype.toISO = function () {
    var tem, A = this.toUTCArray(), i = 0;
    A[1] += 1;
    while (i++ < 7) {
        tem = A[i];
        if (tem < 10) A[i] = '0' + tem;
    }
    return A.splice(0, 3).join('-') + 'T' + A.join(':');
}
*/

/**
*
*  MD5 (Message-Digest Algorithm)
*  http://www.webtoolkit.info/
*
**/

var MD5 = function (string) {

    function RotateLeft(lValue, iShiftBits) {
        return (lValue << iShiftBits) | (lValue >>> (32 - iShiftBits));
    }

    function AddUnsigned(lX, lY) {
        var lX4, lY4, lX8, lY8, lResult;
        lX8 = (lX & 0x80000000);
        lY8 = (lY & 0x80000000);
        lX4 = (lX & 0x40000000);
        lY4 = (lY & 0x40000000);
        lResult = (lX & 0x3FFFFFFF) + (lY & 0x3FFFFFFF);
        if (lX4 & lY4) {
            return (lResult ^ 0x80000000 ^ lX8 ^ lY8);
        }
        if (lX4 | lY4) {
            if (lResult & 0x40000000) {
                return (lResult ^ 0xC0000000 ^ lX8 ^ lY8);
            } else {
                return (lResult ^ 0x40000000 ^ lX8 ^ lY8);
            }
        } else {
            return (lResult ^ lX8 ^ lY8);
        }
    }

    function F(x, y, z) { return (x & y) | ((~x) & z); }

    function G(x, y, z) { return (x & z) | (y & (~z)); }

    function H(x, y, z) { return (x ^ y ^ z); }

    function I(x, y, z) { return (y ^ (x | (~z))); }

    function FF(a, b, c, d, x, s, ac) {
        a = AddUnsigned(a, AddUnsigned(AddUnsigned(F(b, c, d), x), ac));
        return AddUnsigned(RotateLeft(a, s), b);
    }

    function GG(a, b, c, d, x, s, ac) {
        a = AddUnsigned(a, AddUnsigned(AddUnsigned(G(b, c, d), x), ac));
        return AddUnsigned(RotateLeft(a, s), b);
    }

    function HH(a, b, c, d, x, s, ac) {
        a = AddUnsigned(a, AddUnsigned(AddUnsigned(H(b, c, d), x), ac));
        return AddUnsigned(RotateLeft(a, s), b);
    }

    function II(a, b, c, d, x, s, ac) {
        a = AddUnsigned(a, AddUnsigned(AddUnsigned(I(b, c, d), x), ac));
        return AddUnsigned(RotateLeft(a, s), b);
    }

    function ConvertToWordArray(string) {
        var lWordCount;
        var lMessageLength = string.length;
        var lNumberOfWords_temp1 = lMessageLength + 8;
        var lNumberOfWords_temp2 = (lNumberOfWords_temp1 - (lNumberOfWords_temp1 % 64)) / 64;
        var lNumberOfWords = (lNumberOfWords_temp2 + 1) * 16;
        var lWordArray = Array(lNumberOfWords - 1);
        var lBytePosition = 0;
        var lByteCount = 0;
        while (lByteCount < lMessageLength) {
            lWordCount = (lByteCount - (lByteCount % 4)) / 4;
            lBytePosition = (lByteCount % 4) * 8;
            lWordArray[lWordCount] = (lWordArray[lWordCount] | (string.charCodeAt(lByteCount) << lBytePosition));
            lByteCount++;
        }
        lWordCount = (lByteCount - (lByteCount % 4)) / 4;
        lBytePosition = (lByteCount % 4) * 8;
        lWordArray[lWordCount] = lWordArray[lWordCount] | (0x80 << lBytePosition);
        lWordArray[lNumberOfWords - 2] = lMessageLength << 3;
        lWordArray[lNumberOfWords - 1] = lMessageLength >>> 29;
        return lWordArray;
    }

    function WordToHex(lValue) {
        var WordToHexValue = "", WordToHexValue_temp = "", lByte, lCount;
        for (lCount = 0; lCount <= 3; lCount++) {
            lByte = (lValue >>> (lCount * 8)) & 255;
            WordToHexValue_temp = "0" + lByte.toString(16);
            WordToHexValue = WordToHexValue + WordToHexValue_temp.substr(WordToHexValue_temp.length - 2, 2);
        }
        return WordToHexValue;
    }

    function Utf8Encode(string) {
        string = string.replace(/\r\n/g, "\n");
        var utftext = "";

        for (var n = 0; n < string.length; n++) {

            var c = string.charCodeAt(n);

            if (c < 128) {
                utftext += String.fromCharCode(c);
            }
            else if ((c > 127) && (c < 2048)) {
                utftext += String.fromCharCode((c >> 6) | 192);
                utftext += String.fromCharCode((c & 63) | 128);
            }
            else {
                utftext += String.fromCharCode((c >> 12) | 224);
                utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                utftext += String.fromCharCode((c & 63) | 128);
            }

        }

        return utftext;
    }

    var x = Array();
    var k, AA, BB, CC, DD, a, b, c, d;
    var S11 = 7, S12 = 12, S13 = 17, S14 = 22;
    var S21 = 5, S22 = 9, S23 = 14, S24 = 20;
    var S31 = 4, S32 = 11, S33 = 16, S34 = 23;
    var S41 = 6, S42 = 10, S43 = 15, S44 = 21;

    string = Utf8Encode(string);

    x = ConvertToWordArray(string);

    a = 0x67452301;
    b = 0xEFCDAB89;
    c = 0x98BADCFE;
    d = 0x10325476;

    for (k = 0; k < x.length; k += 16) {
        AA = a;
        BB = b;
        CC = c;
        DD = d;
        a = FF(a, b, c, d, x[k + 0], S11, 0xD76AA478);
        d = FF(d, a, b, c, x[k + 1], S12, 0xE8C7B756);
        c = FF(c, d, a, b, x[k + 2], S13, 0x242070DB);
        b = FF(b, c, d, a, x[k + 3], S14, 0xC1BDCEEE);
        a = FF(a, b, c, d, x[k + 4], S11, 0xF57C0FAF);
        d = FF(d, a, b, c, x[k + 5], S12, 0x4787C62A);
        c = FF(c, d, a, b, x[k + 6], S13, 0xA8304613);
        b = FF(b, c, d, a, x[k + 7], S14, 0xFD469501);
        a = FF(a, b, c, d, x[k + 8], S11, 0x698098D8);
        d = FF(d, a, b, c, x[k + 9], S12, 0x8B44F7AF);
        c = FF(c, d, a, b, x[k + 10], S13, 0xFFFF5BB1);
        b = FF(b, c, d, a, x[k + 11], S14, 0x895CD7BE);
        a = FF(a, b, c, d, x[k + 12], S11, 0x6B901122);
        d = FF(d, a, b, c, x[k + 13], S12, 0xFD987193);
        c = FF(c, d, a, b, x[k + 14], S13, 0xA679438E);
        b = FF(b, c, d, a, x[k + 15], S14, 0x49B40821);
        a = GG(a, b, c, d, x[k + 1], S21, 0xF61E2562);
        d = GG(d, a, b, c, x[k + 6], S22, 0xC040B340);
        c = GG(c, d, a, b, x[k + 11], S23, 0x265E5A51);
        b = GG(b, c, d, a, x[k + 0], S24, 0xE9B6C7AA);
        a = GG(a, b, c, d, x[k + 5], S21, 0xD62F105D);
        d = GG(d, a, b, c, x[k + 10], S22, 0x2441453);
        c = GG(c, d, a, b, x[k + 15], S23, 0xD8A1E681);
        b = GG(b, c, d, a, x[k + 4], S24, 0xE7D3FBC8);
        a = GG(a, b, c, d, x[k + 9], S21, 0x21E1CDE6);
        d = GG(d, a, b, c, x[k + 14], S22, 0xC33707D6);
        c = GG(c, d, a, b, x[k + 3], S23, 0xF4D50D87);
        b = GG(b, c, d, a, x[k + 8], S24, 0x455A14ED);
        a = GG(a, b, c, d, x[k + 13], S21, 0xA9E3E905);
        d = GG(d, a, b, c, x[k + 2], S22, 0xFCEFA3F8);
        c = GG(c, d, a, b, x[k + 7], S23, 0x676F02D9);
        b = GG(b, c, d, a, x[k + 12], S24, 0x8D2A4C8A);
        a = HH(a, b, c, d, x[k + 5], S31, 0xFFFA3942);
        d = HH(d, a, b, c, x[k + 8], S32, 0x8771F681);
        c = HH(c, d, a, b, x[k + 11], S33, 0x6D9D6122);
        b = HH(b, c, d, a, x[k + 14], S34, 0xFDE5380C);
        a = HH(a, b, c, d, x[k + 1], S31, 0xA4BEEA44);
        d = HH(d, a, b, c, x[k + 4], S32, 0x4BDECFA9);
        c = HH(c, d, a, b, x[k + 7], S33, 0xF6BB4B60);
        b = HH(b, c, d, a, x[k + 10], S34, 0xBEBFBC70);
        a = HH(a, b, c, d, x[k + 13], S31, 0x289B7EC6);
        d = HH(d, a, b, c, x[k + 0], S32, 0xEAA127FA);
        c = HH(c, d, a, b, x[k + 3], S33, 0xD4EF3085);
        b = HH(b, c, d, a, x[k + 6], S34, 0x4881D05);
        a = HH(a, b, c, d, x[k + 9], S31, 0xD9D4D039);
        d = HH(d, a, b, c, x[k + 12], S32, 0xE6DB99E5);
        c = HH(c, d, a, b, x[k + 15], S33, 0x1FA27CF8);
        b = HH(b, c, d, a, x[k + 2], S34, 0xC4AC5665);
        a = II(a, b, c, d, x[k + 0], S41, 0xF4292244);
        d = II(d, a, b, c, x[k + 7], S42, 0x432AFF97);
        c = II(c, d, a, b, x[k + 14], S43, 0xAB9423A7);
        b = II(b, c, d, a, x[k + 5], S44, 0xFC93A039);
        a = II(a, b, c, d, x[k + 12], S41, 0x655B59C3);
        d = II(d, a, b, c, x[k + 3], S42, 0x8F0CCC92);
        c = II(c, d, a, b, x[k + 10], S43, 0xFFEFF47D);
        b = II(b, c, d, a, x[k + 1], S44, 0x85845DD1);
        a = II(a, b, c, d, x[k + 8], S41, 0x6FA87E4F);
        d = II(d, a, b, c, x[k + 15], S42, 0xFE2CE6E0);
        c = II(c, d, a, b, x[k + 6], S43, 0xA3014314);
        b = II(b, c, d, a, x[k + 13], S44, 0x4E0811A1);
        a = II(a, b, c, d, x[k + 4], S41, 0xF7537E82);
        d = II(d, a, b, c, x[k + 11], S42, 0xBD3AF235);
        c = II(c, d, a, b, x[k + 2], S43, 0x2AD7D2BB);
        b = II(b, c, d, a, x[k + 9], S44, 0xEB86D391);
        a = AddUnsigned(a, AA);
        b = AddUnsigned(b, BB);
        c = AddUnsigned(c, CC);
        d = AddUnsigned(d, DD);
    }

    var temp = WordToHex(a) + WordToHex(b) + WordToHex(c) + WordToHex(d);

    return temp.toLowerCase();
}

function setCookie(c_name, value, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var c_value = escape(value) + ((exdays === null) ? "" : "; expires=" + exdate.toUTCString() + "; path=/");
    document.cookie = c_name + "=" + c_value;
}
function deleteCookie(c_name) {
    document.cookie = c_name + '=; expires=Thu, 01-Jan-70 00:00:01 GMT;';
}