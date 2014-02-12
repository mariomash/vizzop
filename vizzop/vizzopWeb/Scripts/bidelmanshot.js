
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
    /*
    exports.screenshotPage = screenshotPage;
    exports.doScreenshot = doScreenshot;
    */
})(window);