//LZW Compression/Decompression for Strings
var LZW = {
    compress: function (uncompressed) {
        "use strict";
        // Build the dictionary.
        var i,
            dictionary = {},
            c,
            wc,
            w = "",
            result = [],
            dictSize = 256;
        for (i = 0; i < 256; i += 1) {
            dictionary[String.fromCharCode(i)] = i;
        }

        for (i = 0; i < uncompressed.length; i += 1) {
            c = uncompressed.charAt(i);
            wc = w + c;
            if (dictionary[wc]) {
                w = wc;
            } else {
                result.push(dictionary[w]);
                // Add wc to the dictionary.
                dictionary[wc] = dictSize++;
                w = String(c);
            }
        }

        // Output the code for w.
        if (w != "") {
            result.push(dictionary[w]);
        }
        return result;
    },
    decompress: function (compressed) {
        "use strict";
        // Build the dictionary.
        var i,
            dictionary = [],
            w,
            result,
            k,
            entry = "",
            dictSize = 256;
        for (i = 0; i < 256; i += 1) {
            dictionary[i] = String.fromCharCode(i);
        }

        w = String.fromCharCode(compressed[0]);
        result = w;
        for (i = 1; i < compressed.length; i += 1) {
            k = compressed[i];
            if (dictionary[k]) {
                entry = dictionary[k];
            } else {
                if (k == dictSize) {
                    entry = w + w.charAt(0);
                } else {
                    return null;
                }
            }

            result += entry;

            // Add w+entry[0] to the dictionary.
            dictionary[dictSize++] = w + entry.charAt(0);

            w = entry;
        }
        return result;
    }
};

var InterLocutorMouse = jVizzop.zs_Class.create({
    initialize: function (parentdiv) {
        var self = this;
        self._parentdiv = parentdiv;
        self.createMouseDiv();
        //msgbox.interlocutor_mouse.moveTo(v);
    },
    createMouseDiv: function () {
        var self = this;
        try {
            self._mousediv = jVizzop('<div></div>')
                    .addClass('interlocutorMouse')
                    .hide()
                    .appendTo(jVizzop(self._parentdiv));
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    moveTo: function (x, y) {
        //vizzoplib.log(x + ',' + y);
        var self = this;
        try {
            self._mousediv
                .show()
                .css({
                    'top': y + 'px',
                    'left': x + 'px'
                });
        } catch (err) {
            vizzoplib.log(err);
        }
    }
});

var Audio = jVizzop.zs_Class.create({
    initialize: function (audiofile_mp3, audiofile_ogg, loop) {
        var self = this;
        self._audiofile_mp3 = audiofile_mp3;
        self._audiofile_ogg = audiofile_ogg;
        self._loop = loop;
        self.createAudioElement();
    },
    createAudioElement: function () {
        var self = this;
        try {
            self._audioelement = jVizzop('<audio></audio>')
                    .attr('autobuffer', 'true')
                    .attr('preload', 'auto')
                    .appendTo(jVizzop('body'));
            try {
                self._audiosrc_mp3 = jVizzop('<source/>')
                    .attr('src', self._audiofile_mp3)
                    .appendTo(self._audioelement);
            } catch (err) { }
            try {
                self._audiosrc_ogg = jVizzop('<source/>')
                    .attr('src', self._audiofile_ogg)
                    .appendTo(self._audioelement);
            } catch (err) { }
            if (self._loop == true) {
                jVizzop(self._audioelement).bind('ended', function () {
                    this.currentTime = 0;
                    this.play();
                });
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    Play: function () {
        var self = this;
        try {
            self._audioelement[0].play();
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    Stop: function () {
        var self = this;
        try {
            if (self._loop == true) {
                jVizzop(self._audioelement).unbind('ended');
            }
            self._audioelement[0].pause();
            if (self._loop == true) {
                jVizzop(self._audioelement).bind('ended', function () {
                    this.currentTime = 0;
                    this.play();
                });
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    }
});

var Tracking = jVizzop.zs_Class.create({
    initialize: function (username, password, domain) {
        var self = this;
        try {
            this._username = username;
            this._password = password;
            this._domain = domain;
            this._id = null;
            this._cue = [];
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    TrackPageExit: function (url) {
        var self = this;
        try {
            if (vizzop.WSchat != null) {
                vizzop.WSchat.close();
            }
            if (vizzop.Tracking_InCourse != null) {
                return;
            }
            var msg = {
                'username': self._username,
                'password': self._password,
                'domain': self._domain,
                'url': url,
                'trackID': self._id
            };
            url = vizzop.mainURL + "/RealTime/TrackPageExit";
            vizzop.Tracking_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    vizzop.Tracking_InCourse = null;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.Tracking_InCourse = null;
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    TrackPageView: function (url, referrer) {
        var self = this;
        try {
            if (vizzop.Tracking_InCourse != null) {
                return;
            }
            var msg = {
                'username': self._username,
                'password': self._password,
                'domain': self._domain,
                'url': url,
                'referrer': referrer,
                'trackID': self._id
            };
            url = vizzop.mainURL + "/RealTime/TrackPageView";
            vizzop.Tracking_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    vizzop.Tracking_InCourse = null;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.Tracking_InCourse = null;
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    }
});

var Daemon = jVizzop.zs_Class.create({
    initialize: function () {
        /*
        var eo = "profiling";
        console.profile([eo]);
        */
        var self = this;
        try {
            var l = vizzoplib.getLocation(document.URL);

            self.audioNewMessage = new Audio(
                vizzop.mainURL + "/vizzop/data/vizzop_newmsg.mp3",
                vizzop.mainURL + "/vizzop/data/vizzop_newmsg.ogg",
                false);
            self.audioNewAction = new Audio(
                vizzop.mainURL + "/vizzop/data/vizzop_newaction.mp3",
                vizzop.mainURL + "/vizzop/data/vizzop_newaction.ogg",
                false);
            self.audioRinging = new Audio(
                vizzop.mainURL + "/vizzop/data/vizzop_ringing.mp3",
                vizzop.mainURL + "/vizzop/data/vizzop_ringing.ogg",
                true);

            self.controlBox = new ControlBox();
            self.chatlistBox = new ChatListBox(jVizzop('#chatlist'));

            self.openWebSockets();

            if (jVizzop("[rel='icon']").length > 0) {
                vizzop.OriginalFavicon = jVizzop("[rel='icon']").attr("href");
            }

            var name_mecookie = vizzop.ApiKey + "_me";
            var name_comsessionidcookie = vizzop.ApiKey + "_commsessionid";
            var disclaimer_idcookie = vizzop.ApiKey + "_disclaimer";
            switch (vizzop.mode) {
                case 'agent':
                    vizzoplib.deleteCookie(name_mecookie);
                    if ((vizzop.me == null) || (vizzop.me.Password == null)) {
                        self.loginBox = new MessageBox();
                        self.loginBox.fillBox_Login();
                    }
                    self.chatlistBox._boxtitletext.text(LLang('support_agents', null));
                    self.chatlistBox._box.show();
                    self.controlBox._box.show();
                    vizzop.RunningDaemon = window.setInterval(function () { vizzop.Daemon.checkAgent(); }, vizzop.DaemonTiming);
                    break;
                case 'client':
                    if (jVizzop.cookie(name_mecookie) != null) {
                        if (jVizzop.cookie(name_mecookie) != "") {
                            vizzop.me = jVizzop.secureEvalJSON(jVizzop.cookie(name_mecookie));
                        }
                    }
                    vizzoplib.setCookie(name_mecookie, jVizzop.toJSON(vizzop.me), 300);
                    self.clientmessagebox = new ClientMessageBox();
                    //self.controlBox.hideBox();
                    //self.chatlistBox.hideBox();
                    /*
                    if (vizzop.ShowHelpButton == false) {
                        self.clientmessagebox.hideBox();
                    }*/
                    if (vizzop.ShowHelpButton != false) {
                        self.clientmessagebox._box.show();
                    }

                    var disclaimer_accepted = null;
                    if (jVizzop.cookie(disclaimer_idcookie) != null) {
                        if (jVizzop.cookie(disclaimer_idcookie) != "") {
                            disclaimer_accepted = true;
                        }
                    }
                    if ((vizzop.ShowDisclaimer != false) && (disclaimer_accepted == null)) {
                        self.disclaimerbox = new DisclaimerBox();
                        self.disclaimerbox.ShowDisclaimer();
                        vizzoplib.setCookie(disclaimer_idcookie, true, 300);
                    }

                    self._commsessionid = null;
                    if (typeof jVizzop.cookie(name_comsessionidcookie) != "undefined") {
                        /*Creo que aqui abajo fallaba... igualandolo a ""*/
                        if (jVizzop.cookie(name_comsessionidcookie) != "") {
                            self._commsessionid = jVizzop.cookie(name_comsessionidcookie);
                            self.clientmessagebox._commsessionid = self._commsessionid;
                        }
                    }
                    self.checkCommSessions();
                    vizzop.RunningDaemon = window.setInterval(function () { vizzop.Daemon.checkClient(); }, vizzop.DaemonTiming);
                    break;
                case 'meeting':
                    if (jVizzop.cookie(name_mecookie) != null) {
                        if (jVizzop.cookie(name_mecookie) != "") {
                            vizzop.me = jVizzop.secureEvalJSON(jVizzop.cookie(name_mecookie));
                        }
                    }
                    self.meetingmessagebox = new MeetingMesageBox();
                    vizzop.RunningDaemon = window.setInterval(function () { vizzop.Daemon.checkMeeting(); }, vizzop.DaemonTiming);
                    break;
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    openWebSockets: function () {
        var self = this;
        vizzop.WSchat = null;
        vizzop.WSscreen = null;
        try {
            if ((typeof (WebSocket) === "function") && (vizzop.AllowSockets === true)) {
                var url = vizzop.wsURL + "/vizzop/Socket.ashx";
                vizzop.WSchat = new WebSocket(url);
                vizzop.WSchat.onopen = function () {
                    //vizzoplib.log("Socket Connected");
                };
                vizzop.WSchat.onmessage = function (evt) {
                    //vizzoplib.log(evt);
                    var json = jVizzop.parseJSON(evt.data);
                    //vizzoplib.log(json);
                    self.parseNewMessages(json);
                };
                vizzop.WSchat.onerror = function (evt) {
                    //vizzoplib.log(evt);
                };
                vizzop.WSchat.onclose = function () {
                    //vizzoplib.log("Socket Closed");
                };


                vizzop.WSscreen = new WebSocket(url);
                vizzop.WSscreen.onopen = function () {
                    //vizzoplib.log("Socket Connected");
                };
                vizzop.WSscreen.onmessage = function (evt) {
                    //vizzoplib.log(evt);
                    var json = jVizzop.parseJSON(evt.data);
                    //vizzoplib.log(json);
                    self.parseNewMessages(json);
                };
                vizzop.WSscreen.onerror = function (evt) {
                    //vizzoplib.log(evt);
                };
                vizzop.WSscreen.onclose = function () {
                    //vizzoplib.log("Socket Closed");
                };
            }
        } catch (ex) {
            vizzoplib.log(ex)
        }
    },
    checkAgent: function () {
        var self = this;
        try {
            //vizzoplib.log("cargando porque " + vizzop.CommRequest_InCourse);
            jVizzop(self.controlBox._box).show();

            //eso es 100 milisegundos (una decima de segundo)
            if ((vizzop.DaemonTiming_Steps % 1) == 0) {
                self.sendNewMessages();
                self.checkNewMessages();
                self.checkCommSessions();
                jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                    if ((typeof foundbox._interlocutor != "undefined") && (foundbox._interlocutor != null)) {
                        if (foundbox._interlocutor.UserName != "undefined") {
                            foundbox.loadScreen();
                        }
                    }
                });
            }

            //eso es un segundico
            if ((vizzop.DaemonTiming_Steps % 10) == 0) {
                self.checkExternal();
                self.sync_ChatList();
                /*
                var d = new Date();
                vizzoplib.log("cada segundo" + d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds() + ":" + d.getMilliseconds());
                */
            }

            //eso son 10 segundicos
            if ((vizzop.DaemonTiming_Steps % 100) == 0) {
                vizzop.DaemonTiming_Steps = new Number(0);
            }

            vizzop.DaemonTiming_Steps++;
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    checkClient: function () {
        var self = this;
        try {
            //Si no tenemos converser.. lo pedimos
            if ((vizzop.me == null) && (vizzop.CommRequest_InCourse == null)) {
                self.getNewConverser();
                //return;
            }
            if (vizzop.me != null) {
                if (vizzop.me.Business == null) {
                    self.getNewConverser();
                    return;
                }
            }
            if ((vizzop.Tracking == null) && (vizzop.me != null)) {
                vizzop.Tracking = new Tracking(vizzop.me.UserName, vizzop.me.Password, vizzop.me.Business.Domain);
                jVizzop(window).unload(function () {
                    vizzop.Tracking.TrackPageExit(document.URL);
                });
                self.checkExternal();

                if (vizzop.AllowScreenCaptures == true) {
                    self.activateMutationObserver();
                }

                vizzop.HtmlSend_ForceCheckSendHtml = true;
            }
            //Si ya tenemos datos como para trackear y checkear mensajes.. trackeamos nuestra posicion y checkeamos etc
            if (vizzop.me != null) {

                //eso es 100 milisegundos (una decima de segundo)
                if ((vizzop.DaemonTiming_Steps % 1) == 0) {
                    self.checkNewMessages();
                    self.sendNewMessages();
                    if (vizzop.AllowScreenCaptures == true) {
                        self.checkSendHtml();
                    }
                }

                //esto medio segundo
                if ((vizzop.DaemonTiming_Steps % 5) == 0) {
                }

                //Eso es un segundo
                if ((vizzop.DaemonTiming_Steps % 10) == 0) {
                    self.checkExternal();
                }

                //Esto es 10 segundos
                if ((vizzop.DaemonTiming_Steps % 100) == 0) {
                    //vizzop.HtmlSend_ForceCheckSendHtml = true;
                    /*
                    var d = new Date();
                    vizzoplib.log("cada 10 segundos" + d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds() + ":" + d.getMilliseconds());
                    */
                }

            }

            //Si tenemos un commsessionid y full ename y no hay requests en marcha... buscamos al operador ;)
            if ((self._commsessionid != null) && (vizzop.me != null) && (vizzop.CommRequest_InCourse == null)) {
                self.AskForAgentsByCommSessionId();
            }

            //eso son 10 segundicos
            if ((vizzop.DaemonTiming_Steps % 100) == 0) {
                vizzop.DaemonTiming_Steps = new Number(0);
            }

            vizzop.DaemonTiming_Steps++;

        } catch (err) {
            vizzoplib.log(err);
        }
    },
    checkMeeting: function () {
        var self = this;
        try {
            //Si no tenemos converser.. lo pedimos
            if ((vizzop.me == null) && (vizzop.CommRequest_InCourse == null)) {
                self.getNewConverser();
                //return;
            }
            if (vizzop.me != null) {
                if (vizzop.me.Business == null) {
                    self.getNewConverser();
                    return;
                }
                if ((vizzop.DaemonTiming_Steps % 1) == 0) {
                    self.checkNewMessages();
                    self.sendNewMessages();
                }
                if ((vizzop.DaemonTiming_Steps % 100) == 0) {
                    self.checkExternal();
                }
            }
            if (vizzop.DaemonTiming_Steps > new Number(10000)) {
                vizzop.DaemonTiming_Steps = new Number(0);
            } else {
                vizzop.DaemonTiming_Steps++;
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    deactivateMutationObserver: function () {
        var self = this;
        try {
            vizzop.HtmlSend_ForceCheckSendHtml = null;
            if (MutationObserver) {
                if (vizzop.MutationObserver != null) {
                    vizzop.MutationObserver.disconnect();
                }
            } else { // for a Lot of browsers
                try {
                    document.body.removeEventListener('DOMSubtreeModified');
                } catch (err) {
                    try {
                        if (document.onpropertychange) { // for IE 5.5+
                            document.onpropertychange = null;
                        }
                    } catch (err) { }
                }
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    activateMutationObserver: function () {
        var self = this;
        self.deactivateMutationObserver();
        try {
            if (MutationObserver) {
                //
                var MutationList = [];
                vizzop.MutationObserver = new MutationObserver(function (mutations) {
                    try {
                        /*
                        mutations.forEach(function (mutation) {
                            MutationList.push(mutation);
                        });
                        console.log(MutationList);
                        vizzop.HtmlSend_ForceCheckSendHtml = true;
                        */
                        jVizzop(vizzop).trigger("mutated");
                    } catch (ex) {
                        vizzoplib.log(ex);
                    }
                });

                vizzop.MutationObserver.observe(document.body,
                {  // options:
                    subtree: true,  // observe the subtree rooted at myNode
                    childList: true,  // include information childNode insertion/removals
                    attributes: true,  // include information about changes to attributes within the subtree
                    characterData: true
                });
            } else { // for a Lot of browsers
                try {
                    document.body.addEventListener('DOMSubtreeModified', function (e) {
                        jVizzop(vizzop).trigger("mutated");
                        //vizzop.HtmlSend_ForceCheckSendHtml = true;
                    }, false);
                } catch (err) {
                    try {
                        if (document.onpropertychange) { // for IE 5.5+
                            document.onpropertychange = function (e) {
                                /*vizzop.HtmlSend_ForceCheckSendHtml = true;*/
                                jVizzop(vizzop).trigger("mutated");
                            };
                        }
                    } catch (err) { }
                }
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    setConverserStatus: function (status) {
        var self = this;
        try {
            var msg = {
                'username': vizzop.me.UserName,
                'password': vizzop.me.Password,
                'domain': vizzop.me.Business.Domain,
                'status': status
            };
            var url = vizzop.mainURL + "/Converser/ChangeStatus";
            var changequery = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    //var link_id = "chatWith_" + vizzop.me.UserName;
                    //jVizzop(self.chatlistBox._boxList).find('#' + link_id).remove();
                    //self.sync_ChatList();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    sync_ChatList: function () {
        var self = this;
        if (vizzop.ChatListCheck_InCourse != null) {
            return;
        }
        try {
            var msg = {
                'username': vizzop.me.UserName,
                'password': vizzop.me.Password,
                'domain': vizzop.me.Business.Domain
            };
            var url = vizzop.mainURL + "/Converser/GetConversersToChat";
            vizzop.ChatListCheck_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    //self.chatlistBox._boxList.empty();
                    if (data == false) {
                        vizzop.ChatListCheck_InCourse = null;
                        return;
                    }

                    jVizzop.each(data, function (i, v) {
                        /*
                        if (v.UserName == vizzop.me.UserName) {
                            return;
                        }
                        */
                        // Busquemos si está ya en la lista...
                        //Y si no... lo añadimos
                        var link_id = "chatWith_" + v.UserName;

                        var icon_img = 'vizzop-icon-user vizzop-icon-green';
                        if (v.Active + '' == 'false') {
                            icon_img = 'vizzop-icon-remove-sign vizzop-icon-red';
                        }

                        var link = jVizzop(self.chatlistBox._boxList).find('#' + link_id);
                        if (link.length > 0) {
                            if ((link.attr('data-active') + '') != (v.Active + '')) {
                                var icon = jVizzop(link).find('i[data-option="status-icon"]');
                                icon.removeClass();
                                icon.addClass(icon_img);
                                var setvisible = jVizzop(link).find('li[data-option="set-visible"]');
                                var setinvisible = jVizzop(link).find('li[data-option="set-invisible"]');
                                if (v.Active + '' == 'false') {
                                    setvisible.show();
                                    setinvisible.hide();
                                } else {
                                    setvisible.hide();
                                    setinvisible.show();
                                }
                            }
                        }
                        if ((link != null) && (link.length > 0)) {
                            //Saltamos al siguiente.. porque ya existía con la misma info!!
                            return true;
                        }
                        link = jVizzop('<span></span>')
                            .attr('id', link_id)
                            .addClass('item')
                            .attr('data-linked', v.UserName)
                            .attr('data-active', v.Active)
                            .appendTo(jVizzop(self.chatlistBox._boxList))
                            .click(function (event) {
                                try {
                                    if (v.UserName == vizzop.me.UserName) {
                                        return false;
                                    }
                                    jVizzop.each(vizzop.Boxes, function (_index, __foundbox) {
                                        if (__foundbox._interlocutor != null) {
                                            if (__foundbox._interlocutor.UserName == v.UserName) {
                                                __foundbox.bringtofrontBox();
                                                v.Active = false;
                                                return false;
                                            }
                                        }
                                    });
                                    if (v.Active == true) {
                                        var agentmessagebox = new AgentMessageBox();
                                        //var client = { "UserName": v.UserName, "FullName": v.FullName, "Domain": v.Business.Domain };
                                        agentmessagebox._interlocutor = v;
                                        agentmessagebox._apikey = vizzop.ApiKey;
                                        if (v.FullName != "") {
                                            agentmessagebox._statustext = LLang('chat_with', [v.FullName]);
                                        }
                                        agentmessagebox.fillBox_RequestStartChat();
                                        self.sync_ChatList();
                                    }
                                    return false;
                                } catch (err) {
                                    vizzoplib.log(err);
                                }
                            });


                        jVizzop.each(vizzop.Boxes, function (_index, __foundbox) {
                            if (__foundbox._interlocutor) {
                                if (__foundbox._interlocutor.UserName == v.UserName) {
                                    icon_img = 'vizzop-icon-comment vizzop-icon-green';
                                }
                            }
                        });

                        var title = jVizzop('<span></span>');

                        if (v.UserName == vizzop.me.UserName) {
                            var dropstatusgroup = jVizzop('<div class="btn-group"></div>')
                                .appendTo(title);
                            var dropstatus = jVizzop('<a class="btn dropdown-toggle" data-toggle="dropdown" href="#"></a>')
                                .appendTo(dropstatusgroup)
                                .dropdown();

                            var icon = jVizzop('<i></i>')
                                .attr('data-option', 'status-icon')
                                .addClass(icon_img)
                                .appendTo(dropstatus);
                            var text = jVizzop('<span></span>')
                                .text(v.FullName)
                                .appendTo(dropstatus);
                            var yo = jVizzop('<b></b>')
                                .text(" (" + LLang("yo", null) + ") ")
                                .appendTo(dropstatus);
                            var caret = jVizzop('<span></span>')
                                .addClass('caret')
                                .appendTo(dropstatus);
                            var dropstatusopts = jVizzop('<ul class="dropdown-menu" role="menu" aria-labelledby="dLabel"></ul>')
                                .appendTo(dropstatusgroup);

                            var setvisible = jVizzop('<li><i class="vizzop-icon-user vizzop-icon-green"></i>Connected</li>')
                                .attr('data-option', 'set-visible')
                                .appendTo(dropstatusopts)
                                .click(function (event) {
                                    try {
                                        jVizzop(event.target).dropdown('toggle');
                                        setinvisible.show();
                                        setvisible.hide();
                                        icon.removeClass();
                                        icon.addClass('vizzop-icon-user vizzop-icon-green');
                                        link.attr('data-active', true);
                                        self.setConverserStatus(true);
                                        return false;
                                    } catch (err) {
                                        vizzoplib.log(err);
                                    }
                                })
                                .hide();

                            var setinvisible = jVizzop('<li><i class="vizzop-icon-remove-sign vizzop-icon-red"></i>Invisible</li>')
                                .attr('data-option', 'set-invisible')
                                .appendTo(dropstatusopts)
                                .click(function (event) {
                                    try {
                                        jVizzop(event.target).dropdown('toggle');
                                        setinvisible.hide();
                                        setvisible.show();
                                        icon.removeClass();
                                        icon.addClass('vizzop-icon-remove-sign vizzop-icon-red');
                                        link.attr('data-active', false);
                                        self.setConverserStatus(false);
                                        return false;
                                    } catch (err) {
                                        vizzoplib.log(err);
                                    }
                                })
                                .hide();

                            if (v.Active + '' == 'false') {
                                setvisible.show();
                            } else {
                                setinvisible.show();
                            }

                        } else {
                            title.text(v.FullName);
                        }

                        //jVizzop('.dropdown-toggle').dropdown();

                        if (v.UserName != vizzop.me.UserName) {
                            var icon = jVizzop('<i></i>')
                                .addClass(icon_img)
                                .appendTo(link);
                        } else {
                            link.addClass('me');
                        }

                        var text = jVizzop('<span></span>')
                            .addClass('item_text')
                            .append(title)
                            .appendTo(link);
                        /*
                        var hr = jVizzop('<hr/>')
                        .appendTo(jVizzop(self.chatlistBox._boxList));
                        */
                    });
                    vizzop.ChatListCheck_InCourse = null;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    self.chatlistBox._boxList.empty();
                    vizzop.ChatListCheck_InCourse = null;
                }
            });
        } catch (err) {
            vizzoplib.log(err);
            self.chatlistBox._boxList.empty();
            vizzop.ChatListCheck_InCourse = null;
        }
    },
    sync_ControlList: function () {
        var self = this;
        try {
            jVizzop(self.controlBox._boxList).empty();
            jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                // Las modales no las metemos ahi..
                if (_foundbox._nolist == true) { return; }
                // Busquemos si está ya en la lista...
                var id = jVizzop(_foundbox._box).attr('id');
                var link_id = "linkTo_" + jVizzop(_foundbox._box).attr('id');
                var but = jVizzop('#' + link_id);
                var icon_img = 'vizzop-icon-list-alt';
                var title = _foundbox._title;
                switch (_foundbox._status) {
                    case 'start_TextChat':
                    case 'start_VideoChat':
                    case 'start_Screenshare':
                        //title = LLang('chat_with', [title]);
                        icon_img = 'vizzop-icon-user';
                        break;
                    default:
                        break;
                }

                if (but.length == 0) {
                    var link = jVizzop('<span></span>')
                        .attr('id', link_id)
                        .addClass('btn')
                        .attr('data-linked', id)
                        .appendTo(jVizzop(self.controlBox._boxList))
                        .click(function (event) {
                            try {
                                jVizzop.each(vizzop.Boxes, function (_index, __foundbox) {
                                    if (__foundbox._id == link.attr('data-linked')) {
                                        __foundbox.bringtofrontBox();
                                    }
                                });
                                return false;
                            } catch (err) {
                                vizzoplib.log(err);
                            }
                        });
                    var icon = jVizzop('<i></i>')
                        .addClass(icon_img)
                        .appendTo(link);
                    var text = jVizzop('<span></span>')
                        .addClass('button_text')
                        .text(title)
                        .appendTo(link);

                }
            });
            //self.controlBox.positionBox(null, 'fast');
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    loadFaqBox: function (faqid) {
        var self = this;
        try {
            //Busquemos la box 
            var box = null;
            jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                if (foundbox._faqid) {
                    if (foundbox._faqid == faqid) {
                        box = foundbox;
                    }
                }
            });
            //Que sigue sin haber... la creamos leñe!!
            if (box == null) {
                box = new FaqBox(faqid);
            }
            //self.GetAllDetailsFromCommSession(commsessionid);
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    loadLocalizeBox: function (locid) {
        var self = this;
        try {
            //Busquemos la box 
            var box = null;
            jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                if (foundbox._locid) {
                    if (foundbox._locid == locid) {
                        box = foundbox;
                    }
                }
            });
            //Que sigue sin haber... la creamos leñe!!
            if (box == null) {
                box = new LocalizeBox(locid);
            }
            //self.GetAllDetailsFromCommSession(commsessionid);
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    loadTicketBox: function (commsessionid) {
        var self = this;
        try {
            //Busquemos la box 
            var box = null;
            jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                if (foundbox._commsessionid) {
                    if (foundbox._commsessionid == commsessionid) {
                        box = foundbox;
                    }
                }
            });
            //Que sigue sin haber... la creamos leñe!!
            if (box == null) {
                box = new TicketMessageBox(commsessionid);
            }
            var msg = {
                'UserName': vizzop.me.UserName,
                'Password': vizzop.me.Password,
                'Domain': vizzop.me.Business.Domain,
                'commsessionid': commsessionid,
                'block': true
            };
            var url = vizzop.mainURL + "/Comm/GetAllDetailsFromCommSession";
            var post = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    var msgbox = null;
                    jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                        if (_foundbox._commsessionid) {
                            if (_foundbox._commsessionid == commsessionid) {
                                msgbox = _foundbox;
                            }
                        }
                    });
                    if (msgbox == null) {
                        return;
                    }
                    if ((data == false) || (data == null)) {
                        //vizzoplib.log("MsgRequest data is " + vizzop.MsgCheck_InCourse);
                        msgbox.destroyBox();
                        return;
                    }
                    msgbox._interlocutor = data.Session.Client;
                    msgbox.fillBox_ShowMessageHistory();
                    var status_text = "";
                    switch (data.Session.Status) {
                        case 0:
                            status_text = LLang('waiting_approval', null);
                            jVizzop(msgbox._boxtextchat.inputZone)
                                .show();
                            jVizzop(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", false);
                            jVizzop(msgbox.commentandclose)
                                .attr("disabled", false);
                            jVizzop(msgbox.justcomment)
                                .attr("disabled", false);
                            jVizzop(msgbox.justclose)
                                .attr("disabled", false);
                            break;
                        case 1:
                            status_text = LLang('supporting', null);
                            jVizzop(msgbox._boxtextchat.inputZone)
                                .show();
                            jVizzop(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", false);
                            jVizzop(msgbox.commentandclose)
                                .attr("disabled", false);
                            jVizzop(msgbox.justcomment)
                                .attr("disabled", false);
                            jVizzop(msgbox.justclose)
                                .attr("disabled", false);
                            break;
                        case 2:
                            status_text = LLang('closed_ticket', null);
                            jVizzop(msgbox._reopenbutton).show();
                            jVizzop(msgbox._boxtextchat.inputZone)
                                .hide();
                            jVizzop(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", true);
                            jVizzop(msgbox.commentandclose)
                                .attr("disabled", true);
                            jVizzop(msgbox.justcomment)
                                .attr("disabled", true);
                            jVizzop(msgbox.justclose)
                                .attr("disabled", true);
                            break;
                        case 3:
                            status_text = LLang('closed_ticket', null);
                            jVizzop(msgbox._reopenbutton).show();
                            jVizzop(msgbox._boxtextchat.inputZone)
                                .hide();
                            jVizzop(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", true);
                            jVizzop(msgbox.commentandclose)
                                .attr("disabled", true);
                            jVizzop(msgbox.justcomment)
                                .attr("disabled", true);
                            jVizzop(msgbox.justclose)
                                .attr("disabled", true);
                            break;
                    }
                    jVizzop(msgbox._topstatustext).text(status_text);

                    jVizzop.each(data.Session.Messages, function (i, v) {
                        if ((v.Content != null) && (v.Content != "")) {
                            var newmsg = new Message(v.From.FullName, v.To.FullName, null, v.Content, msgbox);
                            newmsg._from_username = v.From.UserName;
                            newmsg._from_domain = v.From.Business.Domain;
                            newmsg._to_username = v.To.UserName;
                            newmsg._to_domain = v.To.Business.Domain;
                            newmsg._status = "sent";
                            newmsg._timestamp = vizzoplib.parseJsonDate(v.TimeStamp);
                            //msgbox._interlocutor = v.To;
                            if (v.From.UserName == data.Session.Client.UserName) {
                                newmsg._from += " (client) ";
                            } else {
                                newmsg._from += " (support agent) ";
                            }
                            newmsg.AddMsgToChat(newmsg);
                            newmsg.MarkAsOk();
                        }
                    });

                    if (data.Session.LockedBy != null) {
                        if (data.Session.LockedBy.ID != vizzop.me.ID) {
                            jVizzop(msgbox._lockedby)
                                .text(LLang('locked_by', [data.Session.LockedBy.FullName]));
                            jVizzop(msgbox._lockedbydiv)
                                .show();
                            jVizzop(msgbox._boxtextchat.inputZone)
                                .hide();
                            jVizzop(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", true);
                            jVizzop(msgbox.commentandclose)
                                .attr("disabled", true);
                            jVizzop(msgbox.justcomment)
                                .attr("disabled", true);
                            jVizzop(msgbox.justclose)
                                .attr("disabled", true);
                        }
                    }

                    msgbox.positionBox(function () { msgbox.show(); }, 'fast');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    GetOpenTokSessionID: function (commsessionid) {
        var self = this;
        try {
            var msg = {
                'UserName': vizzop.me.UserName,
                'Password': vizzop.me.Password,
                'Domain': vizzop.me.Business.Domain,
                'commsessionid': commsessionid
            };
            var url = vizzop.mainURL + "/Comm/GetOpenTokSessionID";
            var post = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data == false) {
                        return;
                    }
                    var msgbox = null;
                    jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                        if (_foundbox._commsessionid) {
                            if (_foundbox._commsessionid == commsessionid) {
                                _foundbox._OpenTokSessionID = data.OpenTokSessionID;
                                _foundbox._OpenTokToken = data.OpenTokToken;
                            }
                        }
                    });
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    GetAllMessagesFromInterlocutor: function (interlocutor_username, interlocutor_domain) {
        var self = this;
        try {
            //vizzoplib.log(interlocutor);
            var msg = {
                'UserName': vizzop.me.UserName,
                'Password': vizzop.me.Password,
                'Domain': vizzop.me.Business.Domain,
                'Interlocutor_UserName': interlocutor_username,
                'Interlocutor_Domain': interlocutor_domain
            };
            var url = vizzop.mainURL + "/Messages/GetAllMessagesFromInterlocutor";
            var post = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data == false) {
                        //vizzoplib.log("MsgRequest data is " + vizzop.MsgCheck_InCourse);
                        return;
                    }
                    var msgbox = null;
                    jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                        if (_foundbox._interlocutor != null) {
                            try {
                                if (_foundbox._interlocutor.UserName == interlocutor) {
                                    msgbox = _foundbox;
                                }
                            } catch (_err) {
                            }
                        }
                    });
                    if (msgbox == null) {
                        return;
                    }
                    jVizzop.each(data, function (i, v) {
                        var newmsg = new Message(v.From.FullName, v.To.FullName, null, v.Content, msgbox);
                        newmsg._from_username = v.From.UserName;
                        newmsg._from_domain = v.From.Business.Domain;
                        newmsg._to_username = v.To.UserName;
                        newmsg._to_domain = v.To.Business.Domain;
                        newmsg._status = "sent";
                        newmsg._timestamp = vizzoplib.parseJsonDate(v.TimeStamp);
                        //msgbox._interlocutor = v.To;

                        newmsg.AddMsgToChat(newmsg);
                    });
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                }
            });
        } catch (err) {
            vizzoplib.log("Error GetAllMessagesFromInterlocutor" + "/" + err);
        }
    },
    sendNewMessages: function () {
        var self = this;
        try {
            if (vizzop.MsgCue.length > 0) {
                var msg = vizzop.MsgCue.shift();
                msg.Send();
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    parseNewMessages: function (data) {
        var self = this;
        try {

            if ((data == null) || (data == false)) {
                return;
            }

            jVizzop.each(data, function (i, v) {
                try {
                    if ((v.From !== null) && (v.To !== null)) {
                        var commsessionid = null;
                        if (v.CommSession != null) {
                            commsessionid = v.CommSession.ID;
                        }
                        var timestamprecipientaccepted = new Date();
                        var audit = {
                            'timestamp': vizzoplib.parseJsonDate(v.TimeStamp),
                            'timestampsendersending': vizzoplib.parseJsonDate(v.TimeStampSenderSending),
                            'timestampsrvaccepted': vizzoplib.parseJsonDate(v.TimeStampSrvAccepted),
                            'timestampsrvsending': vizzoplib.parseJsonDate(v.TimeStampSrvSending),
                            'timestamprecipientaccepted': timestamprecipientaccepted.toJSON(),
                            'from': v.From.UserName + '@' + v.From.Business.Domain,
                            'to': v.To.UserName + '@' + v.To.Business.Domain,
                            'mainurl': vizzop.mainURL,
                            'messagetype': v.MessageType,
                            'commsessionid': commsessionid,
                            'subject': v.Subject,
                            'content': v.Content
                        }
                        if (audit.commsessionid != 0) {
                            //vizzoplib.log(audit);
                        }
                        vizzop.MsgCueAudit.push(audit);
                    }
                    if (v.Subject == '$#_forcestartsession') {
                        window.location.href = vizzop.mainURL + "/Account/LogOff";
                    }
                    var msgbox = null;

                    if (v.CommSession != null) {
                        if (v.CommSession.ID != 0) {
                            jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                                if (_foundbox._commsessionid) {
                                    if (_foundbox._commsessionid == v.CommSession.ID) {
                                        msgbox = _foundbox;
                                    }
                                }
                            });
                            if (msgbox == null) {
                                jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                                    if ((_foundbox._status == "fillBox_helpStandby") ||
                                (_foundbox._status == "fillBox_helpQuestChat") ||
                                (_foundbox._status == "fillBox_FindingSupport_WithName") ||
                                (_foundbox._status == "fillBox_FindingSupport_WithoutName") ||
                                (_foundbox._status == "fillBox_LeaveMessage")) {
                                        msgbox = _foundbox;
                                    }
                                });
                            }

                            if (msgbox == null) {
                                if (v.Subject.indexOf('$#_') > -1) {
                                    switch (v.Subject) {
                                        case '$#_closesession':
                                        case '$#_ask4screenshare':
                                        case '$#_currentdimensions':
                                        case '$#_cancelscreenshare':
                                        case '$#_ask4video':
                                        case '$#_cancelvideo':
                                        case '$#_inputfocus_in':
                                        case '$#_inputfocus_out':
                                        case '$#_notavailable':
                                        case '$#_available':
                                            return false;
                                    }
                                }
                                switch (vizzop.mode) {
                                    case 'client':
                                        msgbox = new ClientMessageBox();
                                        break;
                                    case 'agent':
                                        msgbox = new AgentMessageBox();
                                        break;
                                }
                            }

                            if (v.From.UserName + v.From.Business.Domain != vizzop.me.UserName + vizzop.me.Business.Domain) {
                                msgbox._interlocutor = v.From;
                            }

                            msgbox._apikey = vizzop.ApiKey;
                            msgbox._commsessionid = v.CommSession.ID;
                            if (v.Subject.indexOf('$#_') == -1) {
                                msgbox.start_TextChat();
                                if (vizzop.mode == 'agent') {
                                    msgbox.start_Screenshare();
                                }
                            }
                        }
                    }

                    if (msgbox == null) {
                        jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                            if (_foundbox._interlocutor != null) {
                                if (v.From.UserName + v.From.Business.Domain == _foundbox._interlocutor.UserName + _foundbox._interlocutor.Business.Domain) {
                                    msgbox = _foundbox;
                                }
                            }
                        });
                    }

                    if (msgbox == null) {
                        jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                            if ((_foundbox._status == "fillBox_helpStandby") ||
                        (_foundbox._status == "fillBox_helpQuestChat") ||
                        (_foundbox._status == "fillBox_FindingSupport_WithName") ||
                        (_foundbox._status == "fillBox_FindingSupport_WithoutName") ||
                        (_foundbox._status == "fillBox_LeaveMessage")) {
                                msgbox = _foundbox;
                            }
                        });
                    }

                    if (v.Subject.indexOf('$#_') > -1) {
                        switch (v.Subject) {
                            case '$#_trackid':
                                vizzop.Tracking._id = v.Content;
                                break;
                            case '$#_startsession':
                                //alert("eo");
                                msgbox._commsessionid = v.Content;
                                msgbox._interlocutor = v.From;
                                msgbox._apikey = vizzop.ApiKey;
                                break;
                            case '$#_closesession':
                                msgbox.closeSession();
                                break;
                            case '$#_activeagents':
                                if (vizzop.AllowChat != true) {
                                    vizzop.AllowChat = true;
                                    if (msgbox != null) {
                                        msgbox.checkBubble();
                                    }
                                }
                                break;
                            case '$#_noactiveagents':
                                if (vizzop.AllowChat != false) {
                                    vizzop.AllowChat = false;
                                    if (msgbox != null) {
                                        msgbox.checkBubble();
                                    }
                                }
                                break;
                            case '$#_ask4screenshare':
                                if (msgbox._sharingscreen == null) {
                                    msgbox.askforscreenshare();
                                }
                                break;
                            case '$#_updatescreen':
                                jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                                    if ((typeof foundbox._interlocutor != "undefined") && (foundbox._interlocutor != null)) {
                                        if (foundbox._interlocutor.UserName != "undefined") {
                                            if (v.From.UserName + v.From.Business.Domain == foundbox._interlocutor.UserName + foundbox._interlocutor.Business.Domain) {
                                                foundbox.loadScreen();
                                            }
                                        }
                                    }
                                });
                                break;
                            case '$#_triggerclick':
                                jVizzop('*[vizzop-id="' + v.Content + '"]').trigger('click');
                                break;
                            case '$#_currentdimensions':
                                var dimensions_arr = v.Content.split(",");
                                var width = dimensions_arr[0];
                                var height = dimensions_arr[1];
                                //vizzoplib.log(msgbox);
                                msgbox._interlocutor_iframe_width = width;
                                msgbox._interlocutor_iframe_height = height;
                                if (jVizzop(msgbox._box).hasClass('screenshare') == true) {
                                    msgbox._interlocutor_iframe.attr('width', width);
                                    msgbox._interlocutor_iframe.attr('height', height);
                                    //vizzoplib.log(msgbox._interlocutor_iframe);
                                }
                                break;
                            case '$#_cancelscreenshare':
                                msgbox.cancel_Screenshare();
                                break;
                            case '$#_ask4video':
                                if (msgbox._OpenTokStreams == null) {
                                    self._OpenTokSessionID = v.Content;
                                    msgbox.askforvideo();
                                }
                                break;
                            case '$#_cancelvideo':
                                if (msgbox._boxvideochat.is(':visible')) {
                                    msgbox._boxtitletext.attr({
                                        'rel': 'popover',
                                        'title': 'Information',
                                        'data-content': '<div>' + LLang('interlocutor_ended_video', [msgbox._interlocutor.FullName]) + '</div><div class=actions id=popover_action><button class="btn" id=popover_action_ok>Ok</button></div>'
                                    });
                                    msgbox._boxtitletext.popover({
                                        placement: 'bottom',
                                        html: 'true',
                                        trigger: 'manual'
                                    });
                                    jVizzop(msgbox._boxtitletext).popover('show');
                                    jVizzop('#popover_action_ok').click(function (event) {
                                        jVizzop(msgbox._boxtitletext).popover('hide');
                                        return false;
                                    });
                                }
                                msgbox.cancel_VideoChat(true);
                                break;
                            case '$#_inputfocus_in':
                                if (typeof msgbox._boxtextchat != "undefined") {
                                    msgbox._boxtextchat.infoZone.text(LLang("interlocutor_writing", [v.From.FullName]));
                                    if (vizzop.writingTimeout != null) {
                                        clearTimeout(vizzop.writingTimeout);
                                        vizzop.writingTimeout = null;
                                    }
                                    vizzop.writingTimeout = setTimeout(function () {
                                        msgbox._boxtextchat.infoZone.text('');
                                    }, 1000);
                                }
                                break;
                            case '$#_inputfocus_out':
                                if (typeof msgbox._boxtextchat != "undefined") {
                                    msgbox._boxtextchat.infoZone.text('');
                                    if (vizzop.writingTimeout != null) {
                                        clearTimeout(vizzop.writingTimeout);
                                        vizzop.writingTimeout = null;
                                    }
                                }
                                break;
                            case '$#_notavailable':
                                msgbox.inform_notavailable();
                                break;
                            case '$#_available':
                                msgbox.inform_available();
                                break;
                            case '$#_mousemove':
                                if (msgbox != null) {
                                    var arr = v.Content.split('@');
                                    var orig_size_arr = arr[0].split(',');
                                    var orig_width = new Number(orig_size_arr[0]);
                                    var orig_height = new Number(orig_size_arr[1]);
                                    var orig_mouse_arr = arr[1].split(',');
                                    var orig_x = new Number(orig_mouse_arr[0]);
                                    var orig_y = new Number(orig_mouse_arr[1]);
                                    var local_width = msgbox._boxscreenshare.find("img").outerWidth();
                                    var local_height = msgbox._boxscreenshare.find("img").outerHeight();
                                    var x_ratio = local_width / orig_width;
                                    var y_ratio = local_height / orig_height;
                                    var margin_left = new Number(msgbox._boxscreenshare.css('marginLeft').replace(/[^-\d\.]/g, ''));
                                    var margin_top = new Number(msgbox._boxscreenshare.css('marginTop').replace(/[^-\d\.]/g, ''));
                                    //vizzoplib.log(orig_x);
                                    var x = new Number(orig_x * x_ratio);
                                    //vizzoplib.log(x);
                                    x = x + margin_left - (msgbox.interlocutor_mouse._mousediv.outerWidth() / 2);
                                    //vizzoplib.log(orig_y)
                                    var y = new Number(orig_y * y_ratio);
                                    //vizzoplib.log(y);
                                    y = y + margin_top - (msgbox.interlocutor_mouse._mousediv.outerWidth() / 2);

                                    msgbox.interlocutor_mouse.moveTo(Math.round(x), Math.round(y));
                                }
                                break;
                        }
                    } else {
                        var newmsg = new Message(v.From.FullName, v.To.FullName, null, v.Content, msgbox, v.CommSession.ID, v.ID);
                        newmsg._from_username = v.From.UserName;
                        newmsg._from_domain = v.From.Business.Domain;
                        newmsg._to_username = v.To.UserName;
                        newmsg._to_domain = v.To.Business.Domain;
                        newmsg._status = "sent";
                        newmsg._timestamp = vizzoplib.parseJsonDate(v.TimeStamp);
                        newmsg._old = v.Status;

                        msgbox._interlocutor = v.From;
                        msgbox._commsessionid = v.CommSession.ID;
                        msgbox._col1.css('display', 'inline-block');
                        msgbox.inform_available();
                        newmsg.AddMsgToChat(newmsg);
                        //self.checkSendHtml(0.1);
                    }
                } catch (_err) {
                    vizzoplib.log("parseNewMessages " + _err + " : " + v.Subject + " // " + v.Content);
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    checkExternal: function () {
        var self = this;
        //TrackPageView(document.URL, document.referrer);
        try {
            if ((vizzop.MsgCheckExternal_InCourse != null) || (vizzop.me == null)) {
                return;
            }
            var _trackID = null;
            if (vizzop.Tracking != null) {
                _trackID = vizzop.Tracking._id;
            }
            var _commmSessionID = null;
            if (self._commsessionid != null) {
                _commmSessionID = self._commsessionid;
            }

            var MsgCueAudit = null;
            if ((vizzop.AuditMessages !== false) && (vizzop.MsgCueAudit.length > 0)) {
                MsgCueAudit = JSON.stringify(vizzop.MsgCueAudit);
            }
            vizzop.MsgCueAudit = [];

            var msg = {
                'UserName': vizzop.me.UserName,
                'Password': vizzop.me.Password,
                'Domain': vizzop.me.Business.Domain,
                'MsgLastID': vizzop.MsgLastID,
                'url': document.URL,
                'referrer': document.referrer,
                'CommSessionID': _commmSessionID,
                'SessionID': vizzop.SessionID,
                'trackID': _trackID,
                'MsgCueAudit': MsgCueAudit
            };
            var url = vizzop.mainURL + "/Messages/CheckExternal";
            vizzop.MsgCheckExternal_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    self.parseNewMessages(data);
                    vizzop.MsgCheckExternal_InCourse = null;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.MsgCheckExternal_InCourse = null;
                }
            });
        } catch (err) {
            vizzoplib.log(err);
            vizzop.MsgCheckExternal_InCourse = null;
        }
    },
    checkNewMessages: function () {
        var self = this;
        try {
            if ((vizzop.MsgCheck_InCourse != null) || (vizzop.me == null)) {
                return false;
            }
            var msg = {
                'UserName': vizzop.me.UserName,
                'Password': vizzop.me.Password,
                'Domain': vizzop.me.Business.Domain
            };
            var url = vizzop.mainURL + "/Messages/CheckNew";
            vizzop.MsgCheck_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data, textStatus, jqXHR) {
                    self.parseNewMessages(data);
                    vizzop.MsgCheck_InCourse = null;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.MsgCheck_InCourse = null;
                }
            });
        } catch (err) {
            vizzoplib.log(err);
            vizzop.MsgCheck_InCourse = null;
        }
    },
    sendHtml: function () {
        var self = this;
        try {

            if (vizzop.HtmlSend_InCourse != null) {
                //console.log(vizzop.HtmlSend_InCourse);
                return;
            }

            if (vizzop.HtmlSend_Data.length == 0) {
                //console.log(vizzop.HtmlSend_Data);
                return;
            }
            vizzop.HtmlSend_InCourse = true;

            var listeners_list = '';

            jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                if ((typeof foundbox._interlocutor != "undefined") && (foundbox._interlocutor != null)) {
                    if (foundbox._interlocutor.UserName != "undefined") {
                        listeners_list = listeners_list + foundbox._interlocutor.UserName + '@' + foundbox._interlocutor.Business.Domain + ',';
                    }
                }
            });

            if (listeners_list.length > 0) {
                listeners_list = listeners_list.slice(0, -1);
            }


            //solo mandamos lo que de tiempo a enviarse (lo ultimo que haya)
            /*
            while (vizzop.HtmlSend_Data.length > 1) {
                vizzop.HtmlSend_Data.pop();
            }
            */
            /*
            if (vizzop.HtmlSend_Data[0].length == 1) {
                console.log(vizzop.HtmlSend_Data[0]);
            }
            */
            var msg = {
                'username': vizzop.me.UserName,
                'password': vizzop.me.Password,
                'domain': vizzop.me.Business.Domain,
                'data': vizzop.HtmlSend_Data,
                'listeners': listeners_list,
                'messagetype': 'Screen'
            };
            //vizzop.HtmlSend_Data.shift();
            vizzop.HtmlSend_Data = [];
            //console.log(msg.data);
            var ws = vizzop.WSscreen;
            if (ws != null) {
                if (ws.readyState === undefined || ws.readyState > 1) {
                    vizzop.Daemon.openWebSockets();
                }
                if (ws.readyState == WebSocket.OPEN) {
                    //console.vizzoplib.log(msg);
                    var stringify = JSONVIZZOP.stringify(msg);
                    //vizzop.HtmlSend_Data.shift();
                    //vizzop.HtmlSend_Data = [];
                    //console.log(vizzop.HtmlSend_Data);
                    ws.send(stringify);
                }
                vizzop.HtmlSend_InCourse = null;
            } else {
                var url = vizzop.mainURL + "/RealTime/TrackScreen";
                msg.data = JSONVIZZOP.stringify(msg.data);

                //vizzop.HtmlSend_Data.shift();
                //vizzop.HtmlSend_Data = [];
                vizzop.HtmlSend_InCourse = jVizzop.ajax({
                    url: url,
                    type: "POST",
                    data: msg,
                    beforeSend: function (xhr) {
                        //console.vizzoplib.log(msg);
                        //vizzop.Daemon.audioNewAction.Play();
                    },
                    success: function (data) {
                        //vizzoplib.log("sent");
                        //vizzoplib.log("k");
                        vizzop.HtmlSend_InCourse = null;
                        //vizzoplib.log("l");
                        //vizzop.Daemon.audioNewAction.Play();
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //it allways gives an error
                        //vizzoplib.logAjax(url, msg, jqXHR);
                        //vizzop.Daemon.audioNewAction.Play();
                        vizzop.HtmlSend_InCourse = null;
                    }
                });
            }
            //vizzop.HtmlSend_InCourse = null;
        } catch (err) {
            vizzoplib.log(err);
            //vizzop.HtmlSend_Data = [];
            vizzop.HtmlSend_InCourse = null;
        }
    },
    sendChangesHtml: function () {
        var self = this;
        try {
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    checkSendHtml: function (quality) {
        var self = this;
        try {
            if ((vizzop.HtmlSend_ForceCheckSendHtml == null) || (vizzop.HtmlSend_IsCapturing == true)) {
                //Por si acaso...
                self.sendHtml();
                return;
            }
            vizzop.HtmlSend_IsCapturing = true;
            self.deactivateMutationObserver();
            vizzop.HtmlSend_ForceCheckSendHtml = null;
            var st = jVizzop(window).scrollTop();
            var sl = jVizzop(window).scrollLeft();
            var w;

            DateUTC = new Date();
            DateUTC.setHours(DateUTC.getHours() - DateUTC.getTimezoneOffset() / 60);

            var toSend = {
                'mx': vizzop.mouseXPos,
                'my': vizzop.mouseYPos,
                'st': jVizzop(window).scrollTop(),
                'sl': jVizzop(window).scrollLeft(),
                'w': jVizzop(window).width(),
                'h': jVizzop(window).height(),
                'url': document.URL,
                'date': DateUTC,
                'img': null,
                'blob': screenshotPage()
            }

            self.activateMutationObserver();

            var enviarTienes = false;
            if (vizzop.HtmlSend_LastHtmlSent == null) {
                enviarTienes = true;
            } else {
                var last = vizzop.HtmlSend_LastHtmlSent;
                var blobsSonIguales = false;
                if ((toSend.blob.length == 1) && (toSend.blob[0][0] == '0')) {
                    //console.log("blobs son iguales");
                    blobsSonIguales = true;
                }
                if ((last.mx != toSend.mx) ||
                    (last.my != toSend.my) ||
                    (last.st != toSend.st) ||
                    (last.sl != toSend.sl) ||
                    (last.w != toSend.w) ||
                    (last.h != toSend.h) ||
                    (last.url != toSend.url) ||
                    (last.img != toSend.img) ||
                    (blobsSonIguales == false)) {
                    enviarTienes = true;
                }
            }

            if (enviarTienes == true) {
                //console.log("envia");
                vizzop.HtmlSend_LastHtmlSent = toSend;
                //Lo metemos al final porque iremos enviando en orden por aquello de los diffs....
                vizzop.HtmlSend_Data.push(toSend);
            }

            vizzop.HtmlSend_IsCapturing = false;
            self.sendHtml();

        } catch (err) {
            vizzoplib.log(err);
            vizzop.HtmlSend_IsCapturing = false;
            self.activateMutationObserver();
        }
    },
    getCurrentHtml: function () {
        var self = this;
        try {
            //self._sharingscreen = true;

            jVizzop.each(jVizzop('*').get(), function (idx, val) {
                if (jVizzop(val).attr('zen-id')) {
                } else {
                    jVizzop(val).attr('zen-id', vizzoplib.randomnumber());
                }
            });

            var html = "<html><head>" + jVizzop(document.head).html() + "</head><body>" + jVizzop(document.body).html() + "</body></html>";

            var arr_current_html = LZW.compress(html);
            html = "";
            jVizzop(arr_current_html).each(function (idx, val) {
                var val_ = val + "_";
                html += val_;
            });
            html = html.substring(0, html.length - 1);

            return html;
        } catch (err) {
            vizzoplib.log(err);
            return null;
        }
    },
    checkCommSessions: function () {
        var self = this;
        if ((vizzop.CommRequest_InCourse != null) || (vizzop.me == null)) {
            return false;
        }
        try {
            //Primero miramos las sesiones por aprobar..
            var msg = {
                'username': vizzop.me.UserName,
                'password': vizzop.me.Password,
                'domain': vizzop.me.Business.Domain
            };
            var url = vizzop.mainURL + "/Comm/GetCommSessions";
            vizzop.CommRequest_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data != null) {
                        if (data != false) {
                            switch (vizzop.mode) {
                                case 'agent':
                                    jVizzop.each(data, function (i, v) {
                                        if (v.SessionType == "chat") {
                                            var found = false;
                                            var apikey = v.ApiKey;
                                            var usernameclient = v.Client.UserName;
                                            jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                                                if (typeof foundbox._interlocutor != "undefined") {
                                                    if (foundbox._commsessionid == v.ID) {
                                                        found = true;
                                                        return;
                                                    }
                                                }
                                            });
                                            if (found == false) {
                                                var agentmessagebox = new AgentMessageBox();
                                                agentmessagebox._interlocutor = v.Client;
                                                if (agentmessagebox._interlocutor.FullName == null) {
                                                    agentmessagebox._interlocutor.FullName = LLang('anon_client', null);
                                                }
                                                agentmessagebox._apikey = apikey;
                                                agentmessagebox._commsessionid = v.ID;
                                                self.GetOpenTokSessionID(v.ID);
                                                if (v.Status == 0) {
                                                    agentmessagebox.fillBox_SupportYesNo();
                                                } else if (v.Status == 1) {
                                                    agentmessagebox.start_TextChat();
                                                    self.parseNewMessages(v.Messages);
                                                }
                                            }
                                        } else {
                                            //alert("There is a new ticket, please check Customer Support Panel");
                                        }
                                    });
                                    break;
                                case 'client':
                                    jVizzop.each(data, function (i, v) {
                                        self.parseNewMessages(v.Messages);
                                    });
                                    break;
                            }
                        }
                    }

                    vizzop.CommRequest_InCourse = null;

                    if (vizzop.MsgCheck_InCourse == true) {
                        vizzop.MsgCheck_InCourse = null;
                    }
                    if (vizzop.MsgCheckExternal_InCourse == true) {
                        vizzop.MsgCheckExternal_InCourse = null;
                    }

                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            vizzoplib.log(err);
            vizzop.CommRequest_InCourse = null;
        }
    },
    getNewConverser: function () {
        var self = this;
        try {
            if (vizzop.CommRequest_InCourse != null) {
                return;
            }
            var msg = {
                'apikey': vizzop.ApiKey
            };
            var url = vizzop.mainURL + "/Converser/GetNew";
            vizzop.CommRequest_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                success: function (data) {
                    if (data == null) {
                        vizzop.CommRequest_InCourse = null;
                        return;
                    }
                    vizzop.me = data;

                    var name_mecookie = vizzop.ApiKey + "_me";
                    var name_comsessionidcookie = vizzop.ApiKey + "_commsessionid";

                    vizzoplib.setCookie(name_mecookie, jVizzop.toJSON(vizzop.me), 300);
                    vizzop.CommRequest_InCourse = null;

                    if (vizzop.MsgCheck_InCourse == true) {
                        vizzop.MsgCheck_InCourse = null;
                    }
                    if (vizzop.MsgCheckExternal_InCourse == true) {
                        vizzop.MsgCheckExternal_InCourse = null;
                        //self.checkExternal();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    Requestcommsessionid: function () {
        var self = this;

        if (vizzop.CommRequest_InCourse != null) {
            return false;
        }

        try {
            var msg = {
                'apikey': vizzop.ApiKey,
                'username': vizzop.me.UserName,
                'password': vizzop.me.Password,
                'fullname': vizzop.me.FullName,
                'domain': vizzop.me.Business.Domain
            };
            var url = vizzop.mainURL + "/Comm/Requestcommsessionid";
            vizzop.CommRequest_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                success: function (data) {
                    if (data != false) {
                        self._commsessionid = data;
                    }
                    vizzop.CommRequest_InCourse = null;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            var responseText = jVizzop.parseJSON(jqXHR.responseText);
            vizzoplib.log("Error Requestcommsessionid" + "/" + err);
            vizzop.CommRequest_InCourse = null;
        }
    },
    AskForAgentsByCommSessionId: function () {
        var self = this;
        try {
            //Busquemos la box que transformar en chat... esto es por si nos da por cambiar al operador... esto se encargaría de re-asignar!!!
            var box = null;
            jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                if (foundbox._commsessionid) {
                    if (foundbox._commsessionid == self._commsessionid) {
                        box = foundbox;
                    }
                }
            });
            //Que no hay box.. buscamos una que no se esté usando...
            jVizzop.each(vizzop.Boxes, function (index, foundbox) {
                if ((foundbox._status == 'fillBox_helpStandby') || (foundbox._status == 'fillBox_FindingSupport_WithName')) {
                    box = foundbox;
                }
            });
            //Que sigue sin haber... la creamos leñe!!
            if (box == null) {
                box = new ClientMessageBox();
            }
            //Le asignamos el commsessionid...
            box._commsessionid = self._commsessionid;
            var msg = {
                'requestcommsessionid': self._commsessionid,
                'username': vizzop.me.UserName,
                'password': vizzop.me.Password,
                'domain': vizzop.me.Business.Domain
            };
            var url = vizzop.mainURL + "/Comm/AskForAgentsByCommSessionId";
            vizzop.CommRequest_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    try {
                        if (data == false) {
                            //Si hay false es que la sesion todavia no está aceptada ni cerrada... o sea que sigue en el aire!!
                            vizzop.CommRequest_InCourse = null;
                            return;
                        }
                        if (data == "leave_message") {
                            //Si hay leave_message es que ningun agente quiere o puede responder... deje un mensaje al oir la señal
                            vizzop.CommRequest_InCourse = null;
                            self._commsessionid = null;
                            box.fillBox_LeaveMessage(LLang('checked_leave_message', null));
                            return;
                        }
                        //En caso contrario hay un converser con el que chatear.... Transformando!!! :)
                        box._interlocutor = data;

                        /*
                        //Vamos a dejar esto en waiting for hasta que el operador diga algo y entonces mostramos el tema!!!
                        box.start_TextChat();
                        */

                        //Ala, lo ponemos  a cero y cargamos la cookie (que no borraremos hasta que no cerremos el soporte)
                        var name_mecookie = vizzop.ApiKey + "_me";
                        var name_comsessionidcookie = vizzop.ApiKey + "_commsessionid";
                        vizzoplib.setCookie(name_mecookie, jVizzop.toJSON(vizzop.me), 300);
                        vizzoplib.setCookie(name_comsessionidcookie, self._commsessionid);

                        //Lo dejamos bloqueado con esta petición... dado que ya tenemos lo que buscábamos
                        vizzop.CommRequest_InCourse = true;
                        self.GetAllMessagesFromInterlocutor(data.UserName, data.Business.Domain);
                        self.GetOpenTokSessionID(self._commsessionid);
                    } catch (err) {
                        vizzoplib.log(err);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    blinkFavIcon: function () {
        var self = this;
        try {

            if (jVizzop("[rel='icon']").length > 0) {

                var icon = vizzop.mainURL + "/vizzop/img/favicon_newmsg_on.png";
                var icon_off = vizzop.mainURL + "/vizzop/img/favicon_newmsg_off.png";


                jVizzop("[rel='icon']")
                    .delay(1000)
                    .attr("href", icon_off)
                    .delay(1000)
                    .attr("href", icon)
                    .delay(1000)
                    .attr("href", icon_off)
                    .delay(1000)
                    .attr("href", vizzop.OriginalFavicon)

                /*
                    .queue(function (next) {
                        self.blinkFavIcon();
                    });
                */
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    }
});
