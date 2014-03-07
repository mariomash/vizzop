var vizzoplib = {
    prepareScreenShot: function () {
        try {

            //Le ponemos a todo un ID...
            jVizzop.each(jVizzop('*').get(), function (idx, val) {
                if (jVizzop(val).attr('vizzop-id')) {
                } else {
                    //Vamos a asegurarnos de que no hay mas elementos como este...
                    var new_id = null;
                    while (new_id == null) {
                        new_id = vizzoplib.randomnumber();

                        if (vizzop.IsInFrame == true) {
                            new_id = window.frameElement.getAttribute("id") + '_' + new_id;
                        }

                        var attrToFind = "[vizzop-id='" + new_id + "']";
                        if (jVizzop(attrToFind).length > 0) {
                            new_id = null;
                        }
                    }
                    jVizzop(val).attr('vizzop-id', new_id);
                }
                if (jVizzop(val).is(":focus")) {
                    var attrToFind = "[vizzop-id='" + jVizzop(val).attr('vizzop-id') + "']";
                    var elem = jVizzop(vizzop.screenshot).find(attrToFind);
                    jVizzop(elem).attr('style', 'border: solid 2px blue !important; background-color: solid 2px #aaaaff !important;');
                }
            });

            var screenshot = document.documentElement.cloneNode(true);

            var attrToFind = "[vizzop-id='" + jVizzop(':focus').attr('vizzop-id') + "']";
            var elem = jVizzop(screenshot).find(attrToFind);
            jVizzop(elem).attr('style', 'border: solid 2px blue !important; background-color: solid 2px #aaaaff !important; box-shadow: 0 0 5px rgba(0, 0, 255, 1) !important;');

            //Vamos a recorrer todo metiendole el focus
            /*
            jVizzop.each(jVizzop('*').get(), function (idx, val) {
                var attrToFind = "[vizzop-id='" + jVizzop(val).attr('vizzop-id') + "']";
                var elem = jVizzop(screenshot).find(attrToFind);
                if (jVizzop(val).is(":focus")) {
                    jVizzop(elem).attr('style', 'border: solid 2px blue !important; background-color: solid 2px #aaaaff !important; box-shadow: 0 0 5px rgba(0, 0, 255, 1) !important;');
                    return false;
                }
                //jVizzop(elem).makeAbsolute();
            });
            */

            //Vamos a recorrer todo quitándonos todo lo que esté pasado el scroll..
            /*
            jVizzop.each(jVizzop('body').find('*').get(), function (idx, val) {
                setZeroTimeout(function (val) {
                    console.log(val);
                    return function () {
                        if (vizzoplib.isElementVisible(jVizzop(val)) == false) {
                            console.log("este está más abajo " + jVizzop(val).attr('class'));
                            var attrToFind = "[vizzop-id='" + jVizzop(val).attr('vizzop-id') + "']";
                            var elem = jVizzop(screenshot).find(attrToFind);
                            jVizzop(elem).remove();
                        }
                        //doSomethingHeavy(val);
                    }
                }(idx), 3);
            });
            */
            //console.log(jVizzop(screenshot)[0].innerHTML);

            /*
            jVizzop(screenshot).find('img').each(function () {
                jVizzop(this).attr('src', this.src);
            });
            jVizzop(screenshot).find('link').each(function () {
                jVizzop(this).attr('src', this.src);
            });
            */

            jVizzop(screenshot).find('script').each(function () {
                jVizzop(this).remove();
            });

            /*
            if (vizzop.IsInFrame == true) {
                jVizzop(screenshot).find('body').css('width', window.frameElement.getAttribute("width"));
                jVizzop(screenshot).find('body').css('height', window.frameElement.getAttribute("height"));
            }
            */

            jVizzop(screenshot).find('iframe').each(function () {
                //jVizzop(this).empty();
                var contents = jVizzop(this).attr('value');
                if (contents == null) {
                    jVizzop(this).remove();
                    return true;
                    //contents = "";
                }
                jVizzop(this).removeAttr('value');
                if (vizzop.IsInFrame == true) {
                    jVizzop(this).attr('src', 'data:text/html;charset=utf-8,' + contents);
                } else {
                    //Pero que sucio eres PhantomJS... tengo que crear un DIV para hacer de wrapper pfffff
                    var wrapper = jVizzop('<div></div>')
                        .attr('style', 'width: ' + jVizzop(this).attr('width') + 'px !important; height: ' + jVizzop(this).attr('height') + 'px !important; overflow: hidden !important;')
                        .insertBefore(this);
                    var new_iframe = jVizzop('<iframe></iframe>')
                        .attr('src', 'data:text/html;charset=utf-8,' + contents)
                        .appendTo(wrapper);
                }
                jVizzop(this).hide();
            });

            jVizzop(screenshot).find('noscript').each(function () {
                jVizzop(this).remove();
            });

            //urlsToAbsolute(document.scripts);
            // 2. Duplicate entire document.
            jVizzop('input').each(function () {
                var attrToFind = "[vizzop-id='" + jVizzop(this).attr('vizzop-id') + "']";
                jVizzop(screenshot).find(attrToFind).attr('value', jVizzop(this).val());
            });
            jVizzop('textarea').each(function () {
                var attrToFind = "[vizzop-id='" + jVizzop(this).attr('vizzop-id') + "']";
                jVizzop(screenshot).find(attrToFind).attr('value', jVizzop(this).val());
            });
            jVizzop('select').each(function () {
                var attrToFind = "[vizzop-id='" + jVizzop(this).attr('vizzop-id') + "']";
                jVizzop(screenshot).find(attrToFind).attr('value', jVizzop(this).val());
            });

            // Use <base> to make anchors and other relative links absolute.
            var b = document.createElement('base');
            b.href = document.location.protocol + '//' + location.host;
            var head = screenshot.querySelector('head');
            head.insertBefore(b, head.firstChild);
            // 3. Screenshot should be readyonly, no scrolling, and no selections.
            /*
            screenshot.style.pointerEvents = 'none';
            screenshot.style.overflow = 'hidden';
            screenshot.style.webkitUserSelect = 'none';
            screenshot.style.mozUserSelect = 'none';
            screenshot.style.msUserSelect = 'none';
            screenshot.style.oUserSelect = 'none';
            screenshot.style.userSelect = 'none';
            */
            // 4. Preserve current x,y scroll position of this page. See addOnPageLoad_().
            /*
            screenshot.dataset.scrollX = window.scrollX;
            screenshot.dataset.scrollY = window.scrollY;
            */
            // 4.5. When the screenshot loads (e.g. as ablob URL, as iframe.src, etc.),
            // scroll it to the same location of this page. Do this by appending a
            // window.onDOMContentLoaded listener which pulls out the saved scrollX/Y
            // state from the DOM.
            /*
            var script = document.createElement('script');
            script.textContent = '(' + addOnPageLoad_.toString() + ')();'; // self calling.
            screenshot.querySelector('body').appendChild(script);
            */
            // 5. Create a new .html file from the cloned content.
            return screenshot;
        } catch (err) {
            vizzoplib.log(err);
            return null;
        }
    },
    screenshotPage: function () {
        try {
            // 1. Rewrite current doc's imgs, css, and script URLs to be absolute before
            // we duplicate. This ensures no broken links when viewing the duplicate.
            //urlsToAbsolute(document.images);
            //urlsToAbsolute(document.querySelectorAll("link[rel='stylesheet']"));

            vizzop.screenshot = vizzoplib.prepareScreenShot();
            var current_html = vizzop.screenshot.outerHTML;

            //current_html = escape(current_html);

            if (vizzop.HtmlSend_LastHtmlContents == null) {
                vizzop.HtmlSend_LastHtmlContents = "";
            }

            var objdiff = new diff_match_patch();
            //console.log(vizzop.HtmlSend_LastHtmlContents);
            //console.log(current_html);
            var diffresult = objdiff.diff_main(vizzop.HtmlSend_LastHtmlContents, current_html);

            for (var i in diffresult) {
                var elem = diffresult[i];
                //console.log(elem[1]);
                //console.log(elem[1].length);
                if (elem[0] == 0) {
                    //Sustituimos el texto por el número de caracteres que hay que saltarse...
                    elem[1] = elem[1].length;
                } else if (elem[0] == -1) {
                    //Sustituimos el texto por el número de caracteres que hay que eliminar...
                    elem[1] = elem[1].length;
                }
            }

            //console.log(diffresult);

            vizzop.HtmlSend_LastHtmlContents = current_html;

            return diffresult;
        } catch (err) {
            vizzoplib.log(err);
            return null;
        }
    },
    isElementVisibleHelper: function (el) {
        try {
            var rect = el.getBoundingClientRect();
            return (rect.top <= (window.innerHeight || document.documentElement.clientHeight) &&
                rect.left <= (window.innerWidth || document.documentElement.clientWidth));
            /*
            return (
                rect.top >= 0 &&
                rect.left >= 0 &&
                rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) && 
                rect.right <= (window.innerWidth || document.documentElement.clientWidth) 
            );
            */
            /*or $(window).height() */
            /*or $(window).width() */
        } catch (err) {
            return true;
        }
    },
    isElementVisible: function (el) {
        try {
            var visible = vizzoplib.isElementVisibleHelper(el[0]);
            return visible;
        } catch (err) {
            return true;
        }
    },
    getLocation: function (href) {
        var l = document.createElement("a");
        l.href = href;
        return l;
    },
    ResizeWidthLikeImg: function (box, imgSrc, fnCallback) {
        var newImg = new Image();
        newImg.onload = function () {
            var height = newImg.height;
            var width = newImg.width;
            var ratio = width / height;
            var browserwidth = jVizzop(window).width() - 40;
            var newwidth = ratio * jVizzop(box._boxscreenshare).outerHeight();
            //console.log(jVizzop(box._boxscreenshare).outerHeight());
            if ((newwidth + jVizzop(box._col1).outerWidth() + jVizzop(box._col2).outerWidth()) > browserwidth) {
                newwidth = browserwidth - jVizzop(box._col1).outerWidth() - jVizzop(box._col2).outerWidth();
            }
            fnCallback(newwidth, newImg);
        }
        newImg.src = imgSrc; // this must be done AFTER setting onload
    },
    parseJsonDate: function (jsonDate) {
        var offset = new Date().getTimezoneOffset() * 60000;
        var parts = /\/Date\((-?\d+)([+-]\d{2})?(\d{2})?.*/.exec(jsonDate);

        if (parts[2] == undefined)
            parts[2] = 0;

        if (parts[3] == undefined)
            parts[3] = 0;

        //+ offset

        return new Date(+parts[1] + parts[2] * 3600000 + parts[3] * 60000);
    },
    LoadCss: function (url) {
        var fileref = document.createElement('link');
        fileref.setAttribute('rel', 'stylesheet');
        fileref.setAttribute('type', 'text/css');
        fileref.setAttribute('href', url);
        if (typeof fileref != 'undefined') {
            document.getElementsByTagName('head')[0].appendChild(fileref);
        }
    },
    LoadJS: function (url) {
        var fileref = document.createElement('script');
        fileref.setAttribute('type', 'text/javascript');
        fileref.setAttribute('src', url);

        if (typeof fileref != 'undefined') {
            document.getElementsByTagName('head')[0].appendChild(fileref);
        }
    },
    randomnumber: function () {
        return Math.floor(Math.random() * 10000001);
    },
    logAjax: function (url, msg, jqXHR) {
        vizzoplib.log("Error '" + url + "'/sent:" + JSON.stringify(msg) + "/recieved:" + JSON.stringify(jqXHR));
    },
    log: function (sText) {
        if (vizzop.isDebug == true) {
            try {
                console.info(new Date().getTime() + ' ' + sText);
            } catch (err) { }
            return false;
        } else {
            try {

                var strDate = new Date().getTime();

                var msg = {
                    'text': strDate + ' ' + sText + "/Apikey:" + vizzop.ApiKey + "/ClientIP:" + vizzop.clientIP
                };

                var request = jVizzop.ajax({
                    url: vizzop.mainURL + "/log/SaveFromAjax",
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
    },
    setCookie: function (c_name, value, exdays) {
        var exdate = new Date();
        exdate.setDate(exdate.getDate() + exdays);
        var c_value = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString() + "; path=/");
        document.cookie = c_name + "=" + c_value;
    },
    deleteCookie: function (c_name) {
        document.cookie = c_name + '=; expires=Thu, 01-Jan-70 00:00:01 GMT;';
    },
    ReBindForms: function () {
        try {
            //Vamos a ir poniendo los clicks como tocan...
            jVizzop('*').on('focus.vizzop', function () {
                try {
                    var name = jVizzop(this).attr("id");
                    if (typeof name === "undefined") {
                        name = jVizzop(this).attr("name");
                    }
                    var url = document.URL + '/focus_' + jVizzop(this).attr("name");
                    if (vizzop.IsInFrame == true) {
                        var data = {
                            mode: 'event',
                            url: url,
                            referrer: document.referrer
                        }
                        top.postMessage(JSON.stringify(data), "http://vizzop.com");
                    } else {
                        vizzop.Tracking.TrackPageView(url, document.referrer);
                    }
                } catch (err) {

                }
            }, 0);

            jVizzop('input[type="submit"], button, img, a').on('click.vizzop', function () {
                try {
                    var name = jVizzop(this).attr("id");
                    if (typeof name === "undefined") {
                        name = jVizzop(this).attr("name");
                    }
                    var url = document.URL + '/click_' + jVizzop(this).attr("name");
                    if (vizzop.IsInFrame == true) {
                        var data = {
                            mode: 'event',
                            url: url,
                            referrer: document.referrer
                        }
                        top.postMessage(JSON.stringify(data), "http://vizzop.com");
                    } else {
                        vizzop.Tracking.TrackPageView(url, document.referrer);
                    }
                } catch (err) {

                }
            }, 0);

            jVizzop('input, textarea, select').on('change.vizzop', function () {
                try {
                    vizzop.HtmlSend_ForceSendComplete = true;
                    var name = jVizzop(this).attr("id");
                    if (typeof name === "undefined") {
                        name = jVizzop(this).attr("name");
                    }
                    var url = document.URL + '/change_' + jVizzop(this).attr("name");
                    if (vizzop.IsInFrame == true) {
                        var data = {
                            mode: 'event',
                            url: url,
                            referrer: document.referrer
                        }
                        top.postMessage(JSON.stringify(data), "http://vizzop.com");
                    } else {
                        if (vizzop.Tracking != null) {
                            vizzop.Tracking.TrackPageView(url, document.referrer);
                        }
                    }
                } catch (err) { }
            }, 100);
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    dataURLToBlob: function (dataURL) {
        var BASE64_MARKER = ';base64,';
        if (dataURL.indexOf(BASE64_MARKER) == -1) {
            var parts = dataURL.split(',');
            var contentType = parts[0].split(':')[1];
            var raw = parts[1];

            return new Blob([raw], { type: contentType });
        }

        var parts = dataURL.split(BASE64_MARKER);
        var contentType = parts[0].split(':')[1];
        var raw = window.atob(parts[1]);
        var rawLength = raw.length;

        var uInt8Array = new Uint8Array(rawLength);

        for (var i = 0; i < rawLength; ++i) {
            uInt8Array[i] = raw.charCodeAt(i);
        }

        return new Blob([uInt8Array], { type: contentType });
    },
    PageisInIframe: function () {
        try {
            return window.self !== window.top;
        } catch (err) {
            return true;
        }
    },
    ReceivedMessageFromIframe: function (evt) {
        try {
            //console.log(evt);
            var json = jVizzop.parseJSON(evt.data);
            //console.log(unescape(json.html));
            if (json.vizzop) {
                if (json.vizzop == true) {
                    switch (json.mode) {
                        case 'html':
                            jVizzop('#' + json.id).attr('value', unescape(json.html)); //escape()
                            //console.log(jVizzop('#' + json.id));
                            jVizzop(vizzop).trigger("mutated");
                            break;
                        case 'event':
                            vizzop.Tracking.TrackPageView(json.url, json.referrer);
                            break;
                    }
                }
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    }
}

String.prototype.dateFromJSON = function () {
    return eval(this.replace(/\/Date\((\d+)\)\//gi, "new Date($1)"));
};

/**
 * If the browser is capable, tries zero timeout via postMessage (setTimeout can't go faster than 10ms).
 * Otherwise, it falls back to setTimeout(fn, delay) (which is the same as setTimeout(fn, 10) if under 10).
 * @function
 * @param {Function} fn
 * @param {int} delay
 * @example setZeroTimeout(function () { $.ajax('about:blank'); }, 0);
 */

var setZeroTimeout = (function (w) {
    if (w.postMessage) {
        var timeouts = [],
        msg_name = 'asc0tmot',

        // Like setTimeout, but only takes a function argument.  There's
        // no time argument (always zero) and no arguments (you have to
        // use a closure).
        _postTimeout = function (fn) {
            timeouts.push(fn);
            postMessage(msg_name, '*');
        },

        _handleMessage = function (event) {
            if (event.source == w && event.data == msg_name) {
                if (event.stopPropagation) {
                    event.stopPropagation();
                }
                if (timeouts.length) {
                    try {
                        timeouts.shift()();
                    } catch (e) {
                        // Throw in an asynchronous closure to prevent setZeroTimeout from hanging due to error
                        setTimeout((function (e) {
                            return function () {
                                throw e.stack || e;
                            };
                        }(e)), 0);
                    }
                }
                if (timeouts.length) { // more left?
                    postMessage(msg_name, '*');
                }
            }
        };

        if (w.addEventListener) {
            addEventListener('message', _handleMessage, true);
            return _postTimeout;
        } else if (w.attachEvent) {
            attachEvent('onmessage', _handleMessage);
            return _postTimeout;
        }
    }

    return setTimeout;
}(window));

var MutationObserver = window.MutationObserver || window.WebKitMutationObserver || window.MozMutationObserver;

(function (jVizzop) {
    var on = jVizzop.fn.on, timer;
    jVizzop.fn.on = function () {
        var args = Array.apply(null, arguments);
        var last = args[args.length - 1];

        if (isNaN(last) || (last === 1 && args.pop())) return on.apply(this, args);

        var delay = args.pop();
        var fn = args.pop();

        args.push(function () {
            var self = this, params = arguments;
            clearTimeout(timer);
            timer = setTimeout(function () {
                fn.apply(self, params);
            }, delay);
        });

        return on.apply(this, args);
    };
}(this.jVizzop || this.Zepto));

jVizzop.fn.outerScrollHeight = function (includeMargin) {
    var element = this[0];
    var jElement = jVizzop(element);
    var totalHeight = element.scrollHeight; //includes padding
    //totalHeight += parseInt(jElement.css("border-top-width"), 10) + parseInt(jElement.css("border-bottom-width"), 10);
    //if(includeMargin) totalHeight += parseInt(jElement.css("margin-top"), 10) + parseInt(jElement.css("margin-bottom"), 10);
    totalHeight += jElement.outerHeight(includeMargin) - jElement.innerHeight();
    return totalHeight;
};

jVizzop.fn.outerScrollWidth = function (includeMargin) {
    var element = this[0];
    var jElement = jVizzop(element);
    var totalWidth = element.scrollWidth; //includes padding
    //totalWidth += parseInt(jElement.css("border-left-width"), 10) + parseInt(jElement.css("border-right-width"), 10);
    //if(includeMargin) totalWidth += parseInt(jElement.css("margin-left"), 10) + parseInt(jElement.css("margin-right"), 10);
    totalWidth += jElement.outerWidth(includeMargin) - jElement.innerWidth();
    return totalWidth;
};

jVizzop.fn.makeAbsolute = function (rebase) {
    return this.each(function () {
        var el = jVizzop(this);
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

function LLang(ref, args) {
    var returnValue = "";
    try {
        jVizzop.each(vizzop.langStrings, function (i, v) {
            if (v.Ref == ref) {
                returnValue = v.Text;
                return;
            }
        });
        if (args != null) {
            jVizzop.each(args, function (i, v) {
                var toreplace = "%" + i;
                returnValue = returnValue.replace(eval("/%" + i + "/g"), v);
            });
        }
    } catch (err) {
        vizzoplib.log(err);
    }
    if (returnValue == "") {
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

function _arrayBufferToBase64(buffer) {
    var binary = ''
    var bytes = new Uint8Array(buffer)
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i])
    }
    return window.btoa(binary);
}

/**
*
*  Base64 encode / decode
*  http://www.webtoolkit.info/
*
**/
var Base64 = {
    // private property
    _keyStr: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=",
    // public method for encoding
    encode: function (input) {
        var output = "";
        var chr1, chr2, chr3, enc1, enc2, enc3, enc4;
        var i = 0;

        input = Base64._utf8_encode(input);

        while (i < input.length) {

            chr1 = input.charCodeAt(i++);
            chr2 = input.charCodeAt(i++);
            chr3 = input.charCodeAt(i++);

            enc1 = chr1 >> 2;
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
            enc4 = chr3 & 63;

            if (isNaN(chr2)) {
                enc3 = enc4 = 64;
            } else if (isNaN(chr3)) {
                enc4 = 64;
            }

            output = output +
            this._keyStr.charAt(enc1) + this._keyStr.charAt(enc2) +
            this._keyStr.charAt(enc3) + this._keyStr.charAt(enc4);

        }

        return output;
    },
    // public method for decoding
    decode: function (input) {
        var output = "";
        var chr1, chr2, chr3;
        var enc1, enc2, enc3, enc4;
        var i = 0;

        input = input.replace(/[^A-Za-z0-9\+\/\=]/g, "");

        while (i < input.length) {

            enc1 = this._keyStr.indexOf(input.charAt(i++));
            enc2 = this._keyStr.indexOf(input.charAt(i++));
            enc3 = this._keyStr.indexOf(input.charAt(i++));
            enc4 = this._keyStr.indexOf(input.charAt(i++));

            chr1 = (enc1 << 2) | (enc2 >> 4);
            chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
            chr3 = ((enc3 & 3) << 6) | enc4;

            output = output + String.fromCharCode(chr1);

            if (enc3 != 64) {
                output = output + String.fromCharCode(chr2);
            }
            if (enc4 != 64) {
                output = output + String.fromCharCode(chr3);
            }

        }

        output = Base64._utf8_decode(output);

        return output;

    },
    // private method for UTF-8 encoding
    _utf8_encode: function (string) {
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
    },
    // private method for UTF-8 decoding
    _utf8_decode: function (utftext) {
        var string = "";
        var i = 0;
        var c = c1 = c2 = 0;

        while (i < utftext.length) {

            c = utftext.charCodeAt(i);

            if (c < 128) {
                string += String.fromCharCode(c);
                i++;
            }
            else if ((c > 191) && (c < 224)) {
                c2 = utftext.charCodeAt(i + 1);
                string += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
                i += 2;
            }
            else {
                c2 = utftext.charCodeAt(i + 1);
                c3 = utftext.charCodeAt(i + 2);
                string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
                i += 3;
            }

        }

        return string;
    }
}

jVizzop.eachCallback = function (arr, process, callback) {
    var cnt = 0;
    function work() {
        var item = arr[cnt];
        process.apply(item);
        callback.apply(item, [cnt]);
        cnt += 1;
        if (cnt < arr.length) {
            setTimeout(work, 1);
        }
    }
    setTimeout(work, 1);
};

jVizzop.fn.eachCallback = function (process, callback) {
    var cnt = 0;
    var jq = this;
    function work() {
        var item = jq.get(cnt);
        process.apply(item);
        callback.apply(item, [cnt]);
        cnt += 1;
        if (cnt < jq.length) {
            setTimeout(work, 1);
        }
    }
    setTimeout(work, 1);
};

jVizzop(document).bind('ready.vizzop', function () {

    //vizzoplib.log("LOADED VIZZOP");

    vizzop.IsInFrame = vizzoplib.PageisInIframe();

    vizzoplib.ReBindForms();

    if (vizzop.me) {
        vizzop.me.LastActive = vizzop.me.LastActive.dateFromJSON();
    }

    jVizzop(vizzop).on('mutated', function (e) {
        try {
            vizzop.HtmlSend_ForceSendHtml = true;
        } catch (err) {

        }
    }, 250);

    jVizzop(document).on('mousemove.vizzop', function (e) {
        try {
            vizzop.mouseXPos = e.pageX;
            vizzop.mouseYPos = e.pageY;
            if (vizzop.AllowCaptureMouse == true) {
                jVizzop(vizzop).trigger("mutated");
            }
        } catch (err) {

        }
    }, 0);

    jVizzop(window).on('scroll.vizzop', function (e) {
        try {
            jVizzop(vizzop).trigger("mutated");
        } catch (err) {

        }
    }, 0);

    jVizzop(window).on('resize.vizzop', function (e) {
        try {
            jVizzop(vizzop).trigger("mutated");
        } catch (err) {

        }
    }, 0);

    jVizzop('input').each(function () {
        var attrToFind = "[vizzop-id='" + jVizzop(this).attr('vizzop-id') + "']";
        jVizzop(vizzop.screenshot).find(attrToFind).attr('value', jVizzop(this).val());
    });

    jVizzop('textarea').each(function () {
        var attrToFind = "[vizzop-id='" + jVizzop(this).attr('vizzop-id') + "']";
        jVizzop(vizzop.screenshot).find(attrToFind).attr('value', jVizzop(this).val());
    });

    jVizzop('select').each(function () {
        var attrToFind = "[vizzop-id='" + jVizzop(this).attr('vizzop-id') + "']";
        jVizzop(vizzop.screenshot).find(attrToFind).attr('value', jVizzop(this).val());
    });

    if (vizzop.IsInFrame == false) {
        // Respecto a guardar los contenidos de un iframe y saltarse el cross-domain: se registra el Listener de evento "message" (en el jsapi) y cuando te llega uno de "vizzop" se mete $(message.data.id).attr('src', 'data:' + escape(message.data.html));
        if (window.addEventListener) {
            window.addEventListener("message", vizzoplib.ReceivedMessageFromIframe, false);
        } else {
            window.attachEvent("onmessage", vizzoplib.ReceivedMessageFromIframe);
        }
    } else {
        vizzoplib.ReBindForms();
        // window.frameElement Gets IFrame element which document inside
        var id = window.frameElement.getAttribute("id");
        var screenshot = vizzoplib.prepareScreenShot();
        var current_html = screenshot.outerHTML;
        //console.log(current_html);
        //document.documentElement.outerHTML
        var data = {
            mode: 'html',
            vizzop: true,
            id: id,
            html: escape(current_html)
        }
        //console.log(data);
        top.postMessage(JSON.stringify(data), "*");
    }

    vizzop.Daemon = new Daemon();

});
