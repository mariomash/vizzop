
// Copyright 2012 Eric Bidelman
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Author: Eric Bidelman (ebidel@)
(function (exports) {
    function escape(s) {
        var n = s;
        n = n.replace(/&/g, "&amp;");
        n = n.replace(/</g, "&lt;");
        n = n.replace(/>/g, "&gt;");
        n = n.replace(/"/g, "&quot;");

        return n;
    }

    function urlsToAbsolute(nodeList) {
        if (!nodeList.length) {
            return [];
        }
        var attrName = 'href';
        if (nodeList[0].__proto__ === HTMLImageElement.prototype ||
        nodeList[0].__proto__ === HTMLScriptElement.prototype) {
            attrName = 'src';
        }
        nodeList = [].map.call(nodeList, function (el, i) {
            var attr = el.getAttribute(attrName);
            // If no src/href is present, disregard.
            if (!attr) {
                return;
            }
            var absURL = /^(https?|data):/i.test(attr);
            if (absURL) {
                return el;
            } else {
                // Set the src/href attribute to an absolute version.
                /*
                if (attr.indexOf('/') != 0) { // src="images/test.jpg"
                    el.setAttribute(attrName, document.location.origin + document.location.pathname + attr);
                } else if (attr.match(/^\/\//)) { // src="//static.server/test.jpg"
                    el.setAttribute(attrName, document.location.protocol + attr);
                } else {
                    el.setAttribute(attrName, document.location.origin + attr);
                }
                */
                // Set the src/href attribute to an absolute version. Accessing
                // el['src']/el['href], the browser will stringify an absolute URL, but
                // we still need to explicitly set the attribute on the duplicate.
                return el;
            }
        });
        return nodeList;
    }
    // TODO: current limitation is css background images are not included.
    function screenshotPage() {
        try {
            // 1. Rewrite current doc's imgs, css, and script URLs to be absolute before
            // we duplicate. This ensures no broken links when viewing the duplicate.
            //urlsToAbsolute(document.images);
            //urlsToAbsolute(document.querySelectorAll("link[rel='stylesheet']"));

            jVizzop.each(jVizzop('*').get(), function (idx, val) {
                if (jVizzop(val).attr('vizzop-id')) {
                } else {
                    //Vamos a asegurarnos de que no hay mas elementos como este...
                    var new_id = null;
                    while (new_id == null) {
                        new_id = vizzoplib.randomnumber();
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

            vizzop.screenshot = document.documentElement.cloneNode(true);

            jVizzop.each(jVizzop('*').get(), function (idx, val) {
                if (jVizzop(val).is(":focus")) {
                    var attrToFind = "[vizzop-id='" + jVizzop(val).attr('vizzop-id') + "']";
                    var elem = jVizzop(vizzop.screenshot).find(attrToFind);
                    jVizzop(elem).attr('style', 'border: solid 2px blue !important; background-color: solid 2px #aaaaff !important; box-shadow: 0 0 5px rgba(0, 0, 255, 1) !important;');
                }
            });

            /*
            jVizzop(vizzop.screenshot).find('iframe').each(function (idx, val) {
                var attrToFind = "[vizzop-id='" + jVizzop(val).attr('vizzop-id') + "']";
                var elem = jVizzop(attrToFind);
                console.log(elem.contentWindow.document);
                var iframe_html = elem.contents().find("html").html();
                jVizzop(this).attr('src', 'data:text/html;charset=utf-8,' + escape(iframe_html));
            });
            */

            jVizzop(vizzop.screenshot).find('img').each(function () {
                jVizzop(this).attr('src', this.src);
            });
            jVizzop(vizzop.screenshot).find('link').each(function () {
                jVizzop(this).attr('src', this.src);
            });
            jVizzop(vizzop.screenshot).find('script').each(function () {
                jVizzop(this).remove();
            });

            //urlsToAbsolute(document.scripts);
            // 2. Duplicate entire document.
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

            // Use <base> to make anchors and other relative links absolute.
            var b = document.createElement('base');
            b.href = document.location.protocol + '//' + location.host;
            var head = vizzop.screenshot.querySelector('head');
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
            var current_html = vizzop.screenshot.outerHTML;

            //console.log(current_html);

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

            //var html = '<html>' + jVizzop(vizzop.screenshot).html() + '</html>';
            //diffresult = Base64.encode(diffresult);
            //html = LZW.compress(html);
            /*
            var arr_current_html = LZW.compress(html);
            html = "";
            jVizzop(arr_current_html).each(function (idx, val) {
                var val_ = val + "_";
                html += val_;
            });
            html = html.substring(0, html.length - 1);
            */
            return diffresult;
            //var blob = new Blob([diffresult], { type: 'text/html' });
            //var blob = new Blob([diffresult], { type: 'text/plain' });
            //return blob;
        } catch (err) {
            vizzoplib.log(err);
            return null;
        }
    }
    // NOTE: Not to be invoked directly. When the screenshot loads, it should scroll
    // to the same x,y location of this page.
    function addOnPageLoad_() {
        window.addEventListener('DOMContentLoaded', function (e) {
            var scrollX = document.documentElement.dataset.scrollX || 0;
            var scrollY = document.documentElement.dataset.scrollY || 0;
            window.scrollTo(scrollX, scrollY);
        });
    }
    function doScreenshot() {
        window.URL = window.URL || window.webkitURL;
        window.open(window.URL.createObjectURL(screenshotPage()));
    }
    exports.screenshotPage = screenshotPage;
    exports.doScreenshot = doScreenshot;
})(window);