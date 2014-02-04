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
        if (w !== "") {
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
                if (k === dictSize) {
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

var Audio = jQuery.zs_Class.create({
    initialize: function (audiofile_mp3, audiofile_ogg) {
        var self = this;
        self._audiofile_mp3 = audiofile_mp3;
        self._audiofile_ogg = audiofile_ogg;
        self.createAudioElement();
    },
    createAudioElement: function () {
        var self = this;
        try {
            self._audioelement = jQuery('<audio></audio>')
                    .attr('autobuffer', 'true')
                    .attr('preload', 'auto')
                    .appendTo(jQuery('body'));
            try {
                self._audiosrc_mp3 = jQuery('<source/>')
                    .attr('src', self._audiofile_mp3)
                    .appendTo(self._audioelement);
            } catch (err) {
                log(err);
            }
            try {
                self._audiosrc_ogg = jQuery('<source/>')
                    .attr('src', self._audiofile_ogg)
                    .appendTo(self._audioelement);
            } catch (err) {
                log(err);
            }
        } catch (err) {
            log(err);
        }
    },
    Play: function (url) {
        var self = this;
        try {
            self._audioelement[0].play();
        } catch (err) {
            log(err);
        }
    }
});

var Tracking = jQuery.zs_Class.create({
    initialize: function (username, password, domain) {
        var self = this;
        try {
            this._username = username;
            this._password = password;
            this._domain = domain;
            this._id = null;
            this._cue = [];
        } catch (err) {
            log(err);
        }
    },
    TrackPageExit: function (url) {
        var self = this;
        try {
            if (zentools.Tracking_InCourse !== null) {
                return;
            }
            var msg = {
                'username': self._username,
                'password': self._password,
                'domain': self._domain,
                'url': url,
                'trackID': self._id
            };
            zentools.Tracking_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/RealTime/TrackPageExit",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    zentools.Tracking_InCourse = null;
                },
                error: function (jqXHR, textStatus) {
                    log("Error TrackPageExit: " + textStatus);
                    zentools.Tracking_InCourse = null;
                }
            });
        } catch (err) {
            log(err);
        }
    },
    TrackPageView: function (url, referrer) {
        var self = this;
        try {
            if (zentools.Tracking_InCourse !== null) {
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
            zentools.Tracking_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/RealTime/TrackPageView",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    zentools.Tracking_InCourse = null;
                },
                error: function (jqXHR, textStatus) {
                    log("Error TrackPageView: " + textStatus);
                    zentools.Tracking_InCourse = null;
                }
            });
        } catch (err) {
            log(err);
        }
    }
});

var Daemon = jQuery.zs_Class.create({
    initialize: function () {
        var self = this;
        try {
            var l = getLocation(document.URL);
            zentools.trackHostName = l.hostname;
            zentools.trackPathName = l.pathname;
            self.audioNewMessage = new Audio(zentools.mainURL + "/ZenTools/data/zentools_newmsg.mp3", zentools.mainURL + "/ZenTools/data/zentools_newmsg.ogg");
            self.controlBox = new ControlBox();
            var div = jQuery('#chatlist');
            self.chatlistBox = new ChatListBox(div);
            var name_mecookie = zentools.ApiKey + "_me";
            var name_comsessionidcookie = zentools.ApiKey + "_commsessionid";
            switch (zentools.mode) {
                case 'agent':
                    deleteCookie(name_mecookie);
                    if ((zentools.me === null) || (zentools.me.Password === null)) {
                        self.loginBox = new MessageBox();
                        self.loginBox.fillBox_Login();
                    }
                    self.chatlistBox._boxtitletext.text(LLang('support_agents', null));
                    zentools.RunningDaemon = window.setInterval(function () { zentools.Daemon.checkAgent(); }, zentools.DaemonTiming);
                    break;
                case 'client':
                    if (jQuery.cookie(name_mecookie) !== null) {
                        if (jQuery.cookie(name_mecookie) !== "") {
                            zentools.me = jQuery.secureEvalJSON(jQuery.cookie(name_mecookie));
                        }
                    }
                    self.clientmessagebox = new ClientMessageBox();
                    self.controlBox.hideBox();
                    self.chatlistBox.hideBox();
                    if (zentools.ShowHelpButton === false) {
                        self.clientmessagebox.hideBox();
                    }
                    if (jQuery.cookie(name_comsessionidcookie) !== null) {
                        self._commsessionid = jQuery.cookie(name_comsessionidcookie);
                        self.clientmessagebox._commsessionid = self._commsessionid;
                    } else {
                        self._commsessionid = null;
                    }
                    self.checkCommSessions();
                    zentools.RunningDaemon = window.setInterval(function () { zentools.Daemon.checkClient(); }, zentools.DaemonTiming);
                    break;
                case 'meeting':
                    if (jQuery.cookie(name_mecookie) !== null) {
                        if (jQuery.cookie(name_mecookie) !== "") {
                            zentools.me = jQuery.secureEvalJSON(jQuery.cookie(name_mecookie));
                        }
                    }
                    self.meetingmessagebox = new MeetingMesageBox();
                    zentools.RunningDaemon = window.setInterval(function () { zentools.Daemon.checkMeeting(); }, zentools.DaemonTiming);
                    break;
            }
        } catch (err) {
            log(err);
        }
    },
    checkAgent: function () {
        var self = this;
        try {
            //log("cargando porque " + zentools.CommRequest_InCourse);
            jQuery(self.controlBox._box).show();

            if ((zentools.DaemonTiming_Steps % 1) === 0) {
                self.sendNewMessages();
                self.checkNewMessages();
                self.checkCommSessions();
            }

            if ((zentools.DaemonTiming_Steps % 500) === 0) {
                self.checkExternal();
                self.sync_ChatList();
            }

            if (zentools.DaemonTiming_Steps > new Number(10000)) {
                zentools.DaemonTiming_Steps = new Number(0);
            } else {
                zentools.DaemonTiming_Steps++;
            }
        } catch (err) {
            log(err);
        }
    },
    checkClient: function () {
        var self = this;
        try {
            //Si no tenemos converser.. lo pedimos
            if ((zentools.me === null) && (zentools.CommRequest_InCourse === null)) {
                self.getNewConverser();
                //return;
            }
            if (zentools.me !== null) {
                if (zentools.me.Business === null) {
                    self.getNewConverser();
                    return;
                }
            }
            if ((zentools.Tracking === null) && (zentools.me !== null)) {
                zentools.Tracking = new Tracking(zentools.me.UserName, zentools.me.Password, zentools.me.Business.Domain);
                jQuery(window).unload(function () {
                    zentools.Tracking.TrackPageExit(zentools.trackHostName + zentools.trackPathName);
                });
                self.addKeyFrametoHtmlSendCue();
                if (MutationObserver) {
                    var MutationList = [];
                    var observer = new MutationObserver(function (mutations) {
                        try {
                            mutations.forEach(function (mutation) {
                                MutationList.push(mutation);
                                //log(mutation);
                            });
                            zentools.HtmlSend_ChangesMade = true;
                            //log(MutationList);
                        } catch (ex) {
                            log(ex);
                        }
                    });
                    observer.observe(document.body,
                    {  // options:
                        subtree: true,  // observe the subtree rooted at myNode
                        childList: true,  // include information childNode insertion/removals
                        attributes: true,  // include information about changes to attributes within the subtree
                        characterData: true
                    });
                } else { // for a Lot
                    try {
                        document.body.addEventListener('DOMSubtreeModified', function (e) {
                            zentools.HtmlSend_ChangesMade = true;
                        }, false);
                    } catch (err) {
                        try {
                            if (document.onpropertychange) { // for IE 5.5+
                                document.onpropertychange = function (e) {
                                    zentools.HtmlSend_ChangesMade = true;
                                };
                            }
                        } catch (err) { }
                    }
                }
            }
            //Si ya tenemos datos como para trackear y checkear mensajes.. trackeamos nuestra posicion y checkeamos
            if (zentools.me !== null) {
                if ((zentools.DaemonTiming_Steps % 1) === 0) {
                    self.checkNewMessages();
                    self.sendNewMessages();
                    self.sendHtml();
                }
                if ((zentools.DaemonTiming_Steps % 500) === 0) {
                    self.checkExternal();
                }
                if ((zentools.DaemonTiming_Steps % 500) === 0) {
                    self.checkKeyFrameNeeded();
                }
            }
            //Si tenemos un commsessionid y full name y no hay requests en marcha... buscamos al operador ;)
            if ((self._commsessionid !== null) && (zentools.me !== null) && (zentools.CommRequest_InCourse === null)) {
                self.AskForAgentsByCommSessionId();
            }
            //jQuery(self.clientmessagebox._box).hide();
            if (zentools.DaemonTiming_Steps > new Number(10000)) {
                zentools.DaemonTiming_Steps = new Number(0);
            } else {
                zentools.DaemonTiming_Steps++;
            }
        } catch (err) {
            log(err);
        }
    },
    checkMeeting: function () {
        var self = this;
        try {
            //Si no tenemos converser.. lo pedimos
            if ((zentools.me === null) && (zentools.CommRequest_InCourse === null)) {
                self.getNewConverser();
                //return;
            }
            if (zentools.me !== null) {
                if (zentools.me.Business === null) {
                    self.getNewConverser();
                    return;
                }
                if ((zentools.DaemonTiming_Steps % 1) === 0) {
                    self.checkNewMessages();
                    self.sendNewMessages();
                }
                if ((zentools.DaemonTiming_Steps % 100) === 0) {
                    self.checkExternal();
                }
            }
            if (zentools.DaemonTiming_Steps > new Number(10000)) {
                zentools.DaemonTiming_Steps = new Number(0);
            } else {
                zentools.DaemonTiming_Steps++;
            }
        } catch (err) {
            log(err);
        }
    },
    sync_ChatList: function () {
        var self = this;
        if (zentools.ChatListCheck_InCourse !== null) {
            return;
        }
        try {
            var msg = {
                'username': zentools.me.UserName,
                'password': zentools.me.Password,
                'domain': zentools.me.Business.Domain
            };
            zentools.ChatListCheck_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Converser/GetConversersToChat",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    self.chatlistBox._boxList.empty();
                    if (data === false) {
                        //log("MsgRequest data is " + zentools.MsgCheck_InCourse);
                        zentools.ChatListCheck_InCourse = null;
                        return;
                    }

                    jQuery.each(data, function (i, v) {
                        if (v.UserName === zentools.me.UserName) {
                            return;
                        }
                        // Busquemos si está ya en la lista...
                        //Y si no... lo añadimos
                        var link_id = "chatWith_" + v.UserName;
                        var icon_img = 'icon-user icon-green';
                        if (v.Active === false) {
                            icon_img = 'icon-remove-sign icon-red';
                        }
                        jQuery.each(zentools.Boxes, function (_index, __foundbox) {
                            if (__foundbox._interlocutor) {
                                if (__foundbox._interlocutor.UserName === v.UserName) {
                                    icon_img = 'icon-comment icon-green';
                                }
                            }
                        });
                        var title = v.FullName;
                        var link = jQuery('<span></span>')
                            .attr('id', link_id)
                            .addClass('item')
                            .attr('data-linked', v.UserName)
                            .attr('data-active', v.Active)
                            .appendTo(jQuery(self.chatlistBox._boxList))
                            .click(function (event) {
                                try {
                                    if (v.UserName === zentools.me.UserName) {
                                        return false;
                                    }
                                    jQuery.each(zentools.Boxes, function (_index, __foundbox) {
                                        if (__foundbox._interlocutor !== null) {
                                            if (__foundbox._interlocutor.UserName === v.UserName) {
                                                __foundbox.bringtofrontBox();
                                                v.Active = false;
                                                return false;
                                            }
                                        }
                                    });
                                    if (v.Active === true) {
                                        var agentmessagebox = new AgentMessageBox();
                                        //var client = { "UserName": v.UserName, "FullName": v.FullName, "Domain": v.Business.Domain };
                                        agentmessagebox._interlocutor = v;
                                        agentmessagebox._apikey = zentools.ApiKey;
                                        agentmessagebox._statustext = LLang('chat_with', [v.FullName]);
                                        agentmessagebox.fillBox_RequestStartChat();
                                        self.sync_ChatList();
                                    }
                                    return false;
                                } catch (err) {
                                    log(err);
                                }
                            });
                        var icon = jQuery('<i></i>')
                            .addClass(icon_img)
                            .appendTo(link);
                        var text = jQuery('<span></span>')
                            .addClass('item_text')
                            .text(title)
                            .appendTo(link);
                        /*
                        var hr = jQuery('<hr/>')
                        .appendTo(jQuery(self.chatlistBox._boxList));
                        */
                    });
                    zentools.ChatListCheck_InCourse = null;
                },
                error: function (jqXHR, textStatus) {
                    if (textStatus === null) {
                        textStatus = "";
                    }
                    log("Error sync_ChatList: " + textStatus);
                    self.chatlistBox._boxList.empty();
                    zentools.ChatListCheck_InCourse = null;
                }
            });
        } catch (err) {
            log(err);
            self.chatlistBox._boxList.empty();
            zentools.ChatListCheck_InCourse = null;
        }
    },
    sync_ControlList: function () {
        var self = this;
        try {
            jQuery(self.controlBox._boxList).empty();
            jQuery.each(zentools.Boxes, function (index, _foundbox) {
                // Las modales no las metemos ahi..
                if (_foundbox._nolist === true) { return; }
                // Busquemos si está ya en la lista...
                var id = jQuery(_foundbox._box).attr('id');
                var link_id = "linkTo_" + jQuery(_foundbox._box).attr('id');
                var but = jQuery('#' + link_id);
                var icon_img = 'icon-list-alt';
                var title = _foundbox._title;
                switch (_foundbox._status) {
                    case 'start_TextChat':
                    case 'start_VideoChat':
                    case 'start_Screenshare':
                        //title = LLang('chat_with', [title]);
                        icon_img = 'icon-user';
                        break;
                    default:
                        break;
                }

                if (but.length === 0) {
                    var link = jQuery('<span></span>')
                        .attr('id', link_id)
                        .addClass('btn')
                        .attr('data-linked', id)
                        .appendTo(jQuery(self.controlBox._boxList))
                        .click(function (event) {
                            try {
                                jQuery.each(zentools.Boxes, function (_index, __foundbox) {
                                    if (__foundbox._id === link.attr('data-linked')) {
                                        __foundbox.bringtofrontBox();
                                    }
                                });
                                return false;
                            } catch (err) {
                                log(err);
                            }
                        });
                    var icon = jQuery('<i></i>')
                        .addClass(icon_img)
                        .appendTo(link);
                    var text = jQuery('<span></span>')
                        .addClass('button_text')
                        .text(title)
                        .appendTo(link);

                }
            });
            //self.controlBox.positionBox(null, 'fast');
        } catch (err) {
            log(err);
        }
    },
    loadTicketBox: function (commsessionid) {
        var self = this;
        try {
            //Busquemos la box 
            var box = null;
            jQuery.each(zentools.Boxes, function (index, foundbox) {
                if (foundbox._commsessionid) {
                    if (foundbox._commsessionid === commsessionid) {
                        box = foundbox;
                    }
                }
            });
            //Que sigue sin haber... la creamos leñe!!
            if (box === null) {
                box = new TicketMessageBox(commsessionid);
            }
            self.GetAllDetailsFromCommSession(commsessionid);
        } catch (err) {
            log(err);
        }
    },
    GetOpenTokSessionID: function (commsessionid) {
        var self = this;
        try {
            var msg = {
                'UserName': zentools.me.UserName,
                'Password': zentools.me.Password,
                'Domain': zentools.me.Business.Domain,
                'commsessionid': commsessionid
            };
            var post = jQuery.ajax({
                url: zentools.mainURL + "/Comm/GetOpenTokSessionID",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data === false) {
                        return;
                    }
                    var msgbox = null;
                    jQuery.each(zentools.Boxes, function (index, _foundbox) {
                        if (_foundbox._commsessionid) {
                            if (_foundbox._commsessionid === commsessionid) {
                                _foundbox._OpenTokSessionID = data.OpenTokSessionID;
                                _foundbox._OpenTokToken = data.OpenTokToken;
                            }
                        }
                    });
                },
                error: function (jqXHR, textStatus) {
                    log("Error GetOpenTokSessionID: " + textStatus);
                }
            });
        } catch (err) {
            log(err);
        }
    },
    GetAllMessagesFromCommSession: function (commsessionid) {
        var self = this;
        try {
            var msg = {
                'UserName': zentools.me.UserName,
                'Password': zentools.me.Password,
                'Domain': zentools.me.Business.Domain,
                'commsessionid': commsessionid
            };
            var post = jQuery.ajax({
                url: zentools.mainURL + "/Messages/GetAllMessagesFromCommSession",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data === false) {
                        //log("MsgRequest data is " + zentools.MsgCheck_InCourse);
                        return;
                    }
                    var msgbox = null;
                    jQuery.each(zentools.Boxes, function (index, _foundbox) {
                        if (_foundbox._commsessionid) {
                            if (_foundbox._commsessionid === commsessionid) {
                                msgbox = _foundbox;
                            }
                        }
                    });
                    if (msgbox === null) {
                        return;
                    }
                    jQuery.each(data, function (i, v) {
                        var newmsg = new Message(v.From.FullName, v.To.FullName, null, v.Content, msgbox, commsessionid, v.ID);
                        newmsg._from_username = v.From.UserName;
                        newmsg._from_domain = v.From.Business.Domain;
                        newmsg._to_username = v.To.UserName;
                        newmsg._to_domain = v.To.Business.Domain;
                        newmsg._status = "sent";
                        newmsg._old = v.Status;
                        newmsg._timestamp = parseJsonDate(v.TimeStamp);
                        //msgbox._interlocutor = v.To;
                        newmsg.AddMsgToChat(newmsg);
                    });
                },
                error: function (jqXHR, textStatus) {
                    log("Error GetAllMessagesFromCommSession: " + textStatus);
                }
            });
        } catch (err) {
            log("Error GetAllMessagesFromCommSession: " + err);
        }
    },
    GetAllMessagesFromInterlocutor: function (interlocutor_username, interlocutor_domain) {
        var self = this;
        try {
            //log(interlocutor);
            var msg = {
                'UserName': zentools.me.UserName,
                'Password': zentools.me.Password,
                'Domain': zentools.me.Business.Domain,
                'Interlocutor_UserName': interlocutor_username,
                'Interlocutor_Domain': interlocutor_domain
            };
            var post = jQuery.ajax({
                url: zentools.mainURL + "/Messages/GetAllMessagesFromInterlocutor",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data === false) {
                        //log("MsgRequest data is " + zentools.MsgCheck_InCourse);
                        return;
                    }
                    var msgbox = null;
                    jQuery.each(zentools.Boxes, function (index, _foundbox) {
                        if (_foundbox._interlocutor) {
                            if (_foundbox._interlocutor.UserName === interlocutor) {
                                msgbox = _foundbox;
                            }
                        }
                    });
                    if (msgbox === null) {
                        return;
                    }
                    jQuery.each(data, function (i, v) {
                        var newmsg = new Message(v.From.FullName, v.To.FullName, null, v.Content, msgbox);
                        newmsg._from_username = v.From.UserName;
                        newmsg._from_domain = v.From.Business.Domain;
                        newmsg._to_username = v.To.UserName;
                        newmsg._to_domain = v.To.Business.Domain;
                        newmsg._status = "sent";
                        newmsg._timestamp = parseJsonDate(v.TimeStamp);
                        //msgbox._interlocutor = v.To;
                        newmsg.AddMsgToChat(newmsg);
                    });
                },
                error: function (jqXHR, textStatus) {
                    log("Error GetAllMessagesFromInterlocutor: " + textStatus);
                }
            });
        } catch (err) {
            log("Error GetAllMessagesFromInterlocutor: " + err);
        }
    },
    GetAllDetailsFromCommSession: function (commsessionid) {
        var self = this;
        try {
            var msg = {
                'UserName': zentools.me.UserName,
                'Password': zentools.me.Password,
                'Domain': zentools.me.Business.Domain,
                'commsessionid': commsessionid,
                'block': true
            };
            var post = jQuery.ajax({
                url: zentools.mainURL + "/Comm/GetAllDetailsFromCommSession",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    var msgbox = null;
                    jQuery.each(zentools.Boxes, function (index, _foundbox) {
                        if (_foundbox._commsessionid) {
                            if (_foundbox._commsessionid === commsessionid) {
                                msgbox = _foundbox;
                            }
                        }
                    });
                    //log(msgbox);
                    if (msgbox === null) {
                        return;
                    }
                    //log(data);
                    if ((data === false) || (data === null)) {
                        //log(data);
                        //log("MsgRequest data is " + zentools.MsgCheck_InCourse);
                        msgbox.destroyBox();
                        return;
                    }
                    msgbox._interlocutor = data.Client;
                    msgbox.fillBox_ShowMessageHistory();
                    var status_text = "";
                    switch (data.Status) {
                        case 0:
                            status_text = LLang('waiting_approval', null);
                            jQuery(msgbox._boxtextchat.inputZone)
                                .show();
                            jQuery(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", false);
                            jQuery(msgbox.commentandclose)
                                .attr("disabled", false);
                            jQuery(msgbox.justcomment)
                                .attr("disabled", false);
                            jQuery(msgbox.justclose)
                                .attr("disabled", false);
                            break;
                        case 1:
                            status_text = LLang('supporting', null);
                            jQuery(msgbox._boxtextchat.inputZone)
                                .show();
                            jQuery(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", false);
                            jQuery(msgbox.commentandclose)
                                .attr("disabled", false);
                            jQuery(msgbox.justcomment)
                                .attr("disabled", false);
                            jQuery(msgbox.justclose)
                                .attr("disabled", false);
                            break;
                        case 2:
                            status_text = LLang('closed_ticket', null);
                            jQuery(msgbox._reopenbutton).show();
                            jQuery(msgbox._boxtextchat.inputZone)
                                .hide();
                            jQuery(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", true);
                            jQuery(msgbox.commentandclose)
                                .attr("disabled", true);
                            jQuery(msgbox.justcomment)
                                .attr("disabled", true);
                            jQuery(msgbox.justclose)
                                .attr("disabled", true);
                            break;
                        case 3:
                            status_text = LLang('closed_ticket', null);
                            jQuery(msgbox._reopenbutton).show();
                            jQuery(msgbox._boxtextchat.inputZone)
                                .hide();
                            jQuery(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", true);
                            jQuery(msgbox.commentandclose)
                                .attr("disabled", true);
                            jQuery(msgbox.justcomment)
                                .attr("disabled", true);
                            jQuery(msgbox.justclose)
                                .attr("disabled", true);
                            break;
                    }
                    jQuery(msgbox._topstatustext).text(status_text);

                    jQuery.each(data.Messages, function (i, v) {
                        var newmsg = new Message(v.From.FullName, v.To.FullName, null, v.Content, msgbox);
                        newmsg._from_username = v.From.UserName;
                        newmsg._from_domain = v.From.Business.Domain;
                        newmsg._to_username = v.To.UserName;
                        newmsg._to_domain = v.To.Business.Domain;
                        newmsg._status = "sent";
                        newmsg._timestamp = parseJsonDate(v.TimeStamp);
                        //msgbox._interlocutor = v.To;
                        if (v.From.UserName === data.Client.UserName) {
                            newmsg._from += " (client) ";
                        } else {
                            newmsg._from += " (support agent) ";
                        }
                        newmsg.AddMsgToChat(newmsg);
                        newmsg.MarkAsOk();
                    });

                    if (data.LockedBy !== null) {
                        //log(data.LockedBy);
                        if (data.LockedBy.ID !== zentools.me.ID) {
                            jQuery(msgbox._lockedby)
                                .text(LLang('locked_by', [data.LockedBy.FullName]));
                            jQuery(msgbox._lockedbydiv)
                                .show();
                            jQuery(msgbox._boxtextchat.inputZone)
                                .hide();
                            jQuery(msgbox._boxtextchat.inputTextArea)
                                .attr("disabled", true);
                            jQuery(msgbox.commentandclose)
                                .attr("disabled", true);
                            jQuery(msgbox.justcomment)
                                .attr("disabled", true);
                            jQuery(msgbox.justclose)
                                .attr("disabled", true);
                        }
                    }

                    msgbox.positionBox(null, 'fast');
                },
                error: function (jqXHR, textStatus) {
                    log("Error GetAllDetailsFromCommSession: " + textStatus);
                }
            });
        } catch (err) {
            log("Error GetAllDetailsFromCommSession: " + err);
        }
    },
    sendNewMessages: function () {
        var self = this;
        try {
            if (zentools.MsgCue.length > 0) {
                var msg = zentools.MsgCue.shift();
                msg.Send();
            }
        } catch (err) {
            log(err);
        }
    },
    parseNewMessages: function (data) {
        var self = this;
        try {

            if ((data === null) || (data === false)) {
                return;
            }

            jQuery.each(data, function (i, v) {
                //log("parsing " + data);
                //log(v);
                try {

                    if (v.Subject === '$#_forcestartsession') {
                        window.location.href = zentools.mainURL + "/Account/LogOff";
                    }
                    var msgbox = null;

                    if (v.CommSession !== null) {
                        if (v.CommSession.ID !== 0) {
                            jQuery.each(zentools.Boxes, function (index, _foundbox) {
                                if (_foundbox._commsessionid) {
                                    if (_foundbox._commsessionid === v.CommSession.ID) {
                                        msgbox = _foundbox;
                                    }
                                }
                            });

                            if (msgbox === null) {
                                jQuery.each(zentools.Boxes, function (index, _foundbox) {
                                    if ((_foundbox._status === "fillBox_helpStandby") ||
                                (_foundbox._status === "fillBox_helpQuestChat") ||
                                (_foundbox._status === "fillBox_FindingSupport_WithName") ||
                                (_foundbox._status === "fillBox_FindingSupport_WithoutName") ||
                                (_foundbox._status === "fillBox_LeaveMessage")) {
                                        msgbox = _foundbox;
                                    }
                                });
                            }

                            if (msgbox === null) {
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
                                switch (zentools.mode) {
                                    case 'client':
                                        msgbox = new ClientMessageBox();
                                        break;
                                    case 'agent':
                                        msgbox = new AgentMessageBox();
                                        break;
                                }
                            }

                            switch (zentools.mode) {
                                case 'agent':
                                    msgbox._statustext = LLang('chat_with', [v.From.FullName + " : " + v.From.LangISO]);
                                    if (v.From.Agent === null) {
                                        if (msgbox.button_AddScreenSharing !== null) {
                                            msgbox.button_AddScreenSharing.hide();
                                        }
                                    }
                                    break;
                                case 'client':
                                    msgbox._statustext = LLang('adv_statusbar', null);
                                    break;
                            }

                            if (v.From.UserName + v.From.Business.Domain !== zentools.me.UserName + zentools.me.Business.Domain) {
                                msgbox._interlocutor = v.From;
                            }

                            msgbox._apikey = zentools.ApiKey;
                            msgbox._commsessionid = v.CommSession.ID;
                            if (v.Subject.indexOf('$#_') === -1) {
                                msgbox.start_TextChat();
                            }
                        }
                    }

                    if (v.Subject.indexOf('$#_') > -1) {
                        switch (v.Subject) {
                            case '$#_trackid':
                                zentools.Tracking._id = v.Content;
                                break;
                            case '$#_startsession':
                                //alert("eo");
                                msgbox._commsessionid = v.Content;
                                msgbox._interlocutor = v.From;
                                msgbox._apikey = zentools.ApiKey;
                                break;
                            case '$#_closesession':
                                msgbox.closeSession();
                                break;
                            case '$#_activeagents':
                                zentools.AllowChat = true;
                                break;
                            case '$#_noactiveagents':
                                zentools.AllowChat = false;
                                break;
                            case '$#_ask4screenshare':
                                if (msgbox._sharingscreen === null) {
                                    msgbox.askforscreenshare();
                                }
                                break;
                            case '$#_triggerclick':
                                jQuery('*[zen-id="' + v.Content + '"]').trigger('click');
                                break;

                            case '$#_currentdimensions':
                                var dimensions_arr = v.Content.split(",");
                                var width = dimensions_arr[0];
                                var height = dimensions_arr[1];
                                //log(msgbox);
                                msgbox._interlocutor_iframe_width = width;
                                msgbox._interlocutor_iframe_height = height;
                                if (jQuery(msgbox._box).hasClass('screenshare') === true) {
                                    msgbox._interlocutor_iframe.attr('width', width);
                                    msgbox._interlocutor_iframe.attr('height', height);
                                    //log(msgbox._interlocutor_iframe);
                                }
                                break;
                            case '$#_cancelscreenshare':
                                msgbox.cancel_Screenshare();
                                break;
                            case '$#_ask4video':
                                if (msgbox._OpenTokStreams === null) {
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
                                    jQuery(msgbox._boxtitletext).popover('show');
                                    jQuery('#popover_action_ok').click(function (event) {
                                        jQuery(msgbox._boxtitletext).popover('hide');
                                        return false;
                                    });
                                }
                                msgbox.cancel_VideoChat(true);
                                break;
                            case '$#_inputfocus_in':
                                if (typeof msgbox._boxtextchat !== "undefined") {
                                    msgbox._boxtextchat.infoZone.text(LLang("interlocutor_writing", [v.From.FullName]));
                                }
                                break;
                            case '$#_inputfocus_out':
                                if (typeof msgbox._boxtextchat !== "undefined") {
                                    msgbox._boxtextchat.infoZone.text('');
                                }
                                break;
                            case '$#_notavailable':
                                msgbox.inform_notavailable();
                                break;
                            case '$#_available':
                                msgbox.inform_available();
                                break;
                        }
                    } else {
                        var newmsg = new Message(v.From.FullName, v.To.FullName, null, v.Content, msgbox, v.CommSession.ID, v.ID);
                        newmsg._from_username = v.From.UserName;
                        newmsg._from_domain = v.From.Business.Domain;
                        newmsg._to_username = v.To.UserName;
                        newmsg._to_domain = v.To.Business.Domain;
                        newmsg._status = "sent";
                        newmsg._timestamp = parseJsonDate(v.TimeStamp);
                        newmsg._old = v.Status;
                        //log(v.From);
                        msgbox._interlocutor = v.From;
                        msgbox._commsessionid = v.CommSession.ID;
                        msgbox.inform_available();
                        newmsg.AddMsgToChat(newmsg);
                    }
                } catch (_err) {
                    log("parseNewMessages " + _err);
                }
            });
        } catch (err) {
            log(err);
        }
    },
    checkExternal: function () {
        var self = this;
        //TrackPageView(document.URL, document.referrer);
        try {
            if ((zentools.MsgCheckExternal_InCourse !== null) || (zentools.me === null)) {
                return;
            }
            var _trackID = null;
            if (zentools.Tracking !== null) {
                _trackID = zentools.Tracking._id;
            }
            var _commmSessionID = null;
            if (self._commsessionid !== null) {
                _commmSessionID = self._commsessionid;
            }
            //document.URL
            var msg = {
                'UserName': zentools.me.UserName,
                'Password': zentools.me.Password,
                'Domain': zentools.me.Business.Domain,
                'MsgLastID': zentools.MsgLastID,
                'url': zentools.trackHostName + zentools.trackPathName,
                'referrer': document.referrer,
                'CommSessionID': _commmSessionID,
                'SessionID': zentools.SessionID,
                'trackID': _trackID
            };
            zentools.MsgCheckExternal_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Messages/CheckExternal",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    self.parseNewMessages(data);
                    zentools.MsgCheckExternal_InCourse = null;
                },
                error: function (jqXHR, textStatus) {
                    log("Error checkExternal: " + textStatus);
                    zentools.MsgCheckExternal_InCourse = null;
                }
            });
        } catch (err) {
            log(err);
            zentools.MsgCheckExternal_InCourse = null;
        }
    },
    checkNewMessages: function () {
        var self = this;
        try {
            if ((zentools.MsgCheck_InCourse !== null) || (zentools.me === null)) {
                return false;
            }
            var msg = {
                'UserName': zentools.me.UserName,
                'Password': zentools.me.Password,
                'Domain': zentools.me.Business.Domain
            };
            zentools.MsgCheck_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Messages/CheckNew",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    self.parseNewMessages(data);
                    zentools.MsgCheck_InCourse = null;
                },
                error: function (jqXHR, textStatus) {
                    log("Error checkNewMessages: " + textStatus);
                    zentools.MsgCheck_InCourse = null;
                }
            });
        } catch (err) {
            log(err);
            zentools.MsgCheck_InCourse = null;
        }
    },
    getCurrentHtml: function () {
        var self = this;
        try {
            //self._sharingscreen = true;

            jQuery.each(jQuery('*').get(), function (idx, val) {
                if (jQuery(val).attr('zen-id')) {
                } else {
                    jQuery(val).attr('zen-id', randomnumber());
                }
            });

            var html = "<html><head>" + jQuery(document.head).html() + "</head><body>" + jQuery(document.body).html() + "</body></html>";

            var arr_current_html = LZW.compress(html);
            html = "";
            jQuery(arr_current_html).each(function (idx, val) {
                var val_ = val + "_";
                html += val_;
            });
            html = html.substring(0, html.length - 1);

            return html;
        } catch (err) {
            log(err);
            return null;
        }
    },
    checkKeyFrameNeeded: function () {
        var self = this;
        try {
            if (zentools.HtmlSend_ChangesMade !== null) {
                self.addKeyFrametoHtmlSendCue();
                zentools.HtmlSend_ChangesMade = null;
            } else if ((zentools.DaemonTiming_Steps % 50) === 0) {
                self.addKeyFrametoHtmlSendCue();
            }
        } catch (err) {
            log(err);
        }
    },
    addKeyFrametoHtmlSendCue: function () {
        var self = this;
        try {
            zentools.HtmlSendCue = [];
            var html = self.getCurrentHtml();
            var md5 = MD5(html);

            if (self._current_md5 === null) {
                self._current_md5 = "";
            }

            if (self._current_md5 !== md5) {
                self._current_md5 = md5;
                var v = {
                    'type': 'key',
                    'data': html,
                    'md5': md5
                };
                zentools.HtmlSendCue.push(v);
            }

        } catch (err) {
            log('addKeyFrametoHtmlSendCue: ' + err);
        }
    },
    addChangeFrametoHtmlSendCue: function (e) {
        var self = this;
        try {
            jQuery.each(jQuery('*').get(), function (idx, val) {
                if (jQuery(val).attr('zen-id')) {
                } else {
                    jQuery(val).attr('zen-id', randomnumber());
                }
            });
            var to_compress = jQuery(e.target).html();
            if (to_compress === "") { return; }
            var arr_current_html = LZW.compress(to_compress);
            html = "";
            jQuery(arr_current_html).each(function (idx, val) {
                var val_ = val + "_";
                html += val_;
            });
            html = html.substring(0, html.length - 1);
            html = jQuery(e.target).attr('zen-id') + ":" + html;
            var md5 = MD5(html);
            var v = {
                'type': 'change',
                'data': html,
                'md5': md5
            };
            //log(v);
            zentools.HtmlSendCue.push(v);
        } catch (err) {
            log('addChangeFrametoHtmlSendCue: ' + err);
        }
    },
    sendHtml: function () {
        var self = this;
        try {
            if (zentools.HtmlSend_InCourse !== null) {
                return;
            }
            if (zentools.HtmlSendCue.length < 1) {
                return;
            }

            var url = zentools.mainURL + "/RealTime/TrackScreen";
            var width = jQuery(window).width();
            var height = jQuery(window).height();
            var current_dimensions = width + ',' + height;
            var to_send = zentools.HtmlSendCue[0];
            var v = {
                'username': zentools.me.UserName,
                'password': zentools.me.Password,
                'domain': zentools.me.Business.Domain,
                'dimensions': current_dimensions,
                'data': to_send.data,
                'type': to_send.type,
                'md5': to_send.md5
            };
            zentools.HtmlSend_InCourse = jQuery.ajax({
                url: url,
                type: "POST",
                data: v,
                dataType: "json",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    zentools.HtmlSendCue.shift();
                    zentools.HtmlSend_InCourse = null;
                },
                error: function (jqXHR, textStatus) {
                    zentools.HtmlSendCue.shift();
                    zentools.HtmlSend_InCourse = null;
                }
            });
        } catch (err) {
            zentools.HtmlSendCue.shift();
            zentools.HtmlSend_InCourse = null;
        }
    },
    checkCommSessions: function () {
        var self = this;
        if ((zentools.CommRequest_InCourse !== null) || (zentools.me === null)) {
            return false;
        }
        try {
            //Primero miramos las sesiones por aprobar..
            var msg = {
                'username': zentools.me.UserName,
                'password': zentools.me.Password,
                'domain': zentools.me.Business.Domain
            };
            zentools.CommRequest_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Comm/GetCommSessions",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data !== null) {
                        if (data !== false) {
                            switch (zentools.mode) {
                                case 'agent':
                                    jQuery.each(data, function (i, v) {
                                        if (v.SessionType === "chat") {
                                            var found = false;
                                            var apikey = v.ApiKey;
                                            var usernameclient = v.Client.UserName;
                                            jQuery.each(zentools.Boxes, function (index, foundbox) {
                                                if (typeof foundbox._interlocutor !== "undefined") {
                                                    if (foundbox._commsessionid === v.ID) {
                                                        found = true;
                                                        return;
                                                    }
                                                }
                                            });
                                            if (found === false) {
                                                var agentmessagebox = new AgentMessageBox();
                                                //log(v.Client);
                                                agentmessagebox._interlocutor = v.Client;
                                                if (agentmessagebox._interlocutor.FullName === null) {
                                                    agentmessagebox._interlocutor.FullName = LLang('anon_client', null);
                                                }
                                                agentmessagebox._apikey = apikey;
                                                agentmessagebox._commsessionid = v.ID;
                                                self.GetOpenTokSessionID(v.ID);
                                                if (v.Status === 0) {
                                                    agentmessagebox.fillBox_SupportYesNo();
                                                } else if (v.Status === 1) {
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
                                    jQuery.each(data, function (i, v) {
                                        self.parseNewMessages(v.Messages);
                                    });
                                    break;
                            }
                        }
                    }

                    zentools.CommRequest_InCourse = null;

                    if (zentools.MsgCheck_InCourse === true) {
                        zentools.MsgCheck_InCourse = null;
                    }
                    if (zentools.MsgCheckExternal_InCourse === true) {
                        zentools.MsgCheckExternal_InCourse = null;
                    }

                },
                error: function (jqXHR, textStatus) {
                    log("Error checkCommSessions: " + jqXHR + " : " + textStatus);
                    zentools.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            log(err);
            zentools.CommRequest_InCourse = null;
        }
    },
    getNewConverser: function () {
        var self = this;
        try {
            if (zentools.CommRequest_InCourse !== null) {
                return;
            }
            var msg = {
                'apikey': zentools.ApiKey
            };
            zentools.CommRequest_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Converser/GetNew",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                success: function (data) {
                    if (data === null) {
                        zentools.CommRequest_InCourse = null;
                        return;
                    }
                    zentools.me = data;

                    var name_mecookie = zentools.ApiKey + "_me";
                    var name_comsessionidcookie = zentools.ApiKey + "_commsessionid";

                    setCookie(name_mecookie, jQuery.toJSON(zentools.me), 1);
                    zentools.CommRequest_InCourse = null;

                    if (zentools.MsgCheck_InCourse === true) {
                        zentools.MsgCheck_InCourse = null;
                    }
                    if (zentools.MsgCheckExternal_InCourse === true) {
                        zentools.MsgCheckExternal_InCourse = null;
                        //self.checkExternal();
                    }
                },
                error: function (jqXHR, textStatus) {
                    log("Error getNewConverser: " + textStatus);
                    zentools.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            log(err);
        }
    },
    Requestcommsessionid: function () {
        var self = this;
        if (zentools.CommRequest_InCourse !== null) {
            return false;
        }
        try {
            var msg = {
                'apikey': zentools.ApiKey,
                'username': zentools.me.UserName,
                'password': zentools.me.Password,
                'fullname': zentools.me.FullName,
                'domain': zentools.me.Business.Domain
            };
            zentools.CommRequest_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Comm/Requestcommsessionid",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                success: function (data) {
                    if (data !== false) {
                        self._commsessionid = data;
                    }
                    zentools.CommRequest_InCourse = null;
                },
                error: function (jqXHR, textStatus) {
                    log("Error Requestcommsessionid: " + textStatus);
                    zentools.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            log("Error Requestcommsessionid: " + textStatus);
            zentools.CommRequest_InCourse = null;
        }
    },
    AskForAgentsByCommSessionId: function () {
        var self = this;
        try {
            //Busquemos la box que transformar en chat... esto es por si nos da por cambiar al operador... esto se encargaría de re-asignar!!!
            var box = null;
            jQuery.each(zentools.Boxes, function (index, foundbox) {
                if (foundbox._commsessionid) {
                    if (foundbox._commsessionid === self._commsessionid) {
                        box = foundbox;
                    }
                }
            });
            //Que no hay box.. buscamos una que no se esté usando...
            jQuery.each(zentools.Boxes, function (index, foundbox) {
                if ((foundbox._status === 'fillBox_helpStandby') || (foundbox._status === 'fillBox_FindingSupport_WithName')) {
                    box = foundbox;
                }
            });
            //Que sigue sin haber... la creamos leñe!!
            if (box === null) {
                box = new ClientMessageBox();
            }
            //Le asignamos el commsessionid...
            box._commsessionid = self._commsessionid;
            var msg = {
                'requestcommsessionid': self._commsessionid,
                'username': zentools.me.UserName,
                'password': zentools.me.Password,
                'domain': zentools.me.Business.Domain
            };
            zentools.CommRequest_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Comm/AskForAgentsByCommSessionId",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    try {
                        if (data === false) {
                            //Si hay false es que la sesion todavia no está aceptada ni cerrada... o sea que sigue en el aire!!
                            zentools.CommRequest_InCourse = null;
                            return;
                        }
                        if (data === "leave_message") {
                            //Si hay leave_message es que ningun agente quiere o puede responder... deje un mensaje al oir la señal
                            zentools.CommRequest_InCourse = null;
                            self._commsessionid = null;
                            box.fillBox_LeaveMessage(LLang('checked_leave_message', null));
                            return;
                        }
                        //En caso contrario hay un converser con el que chatear.... Transformando!!! :)
                        box._interlocutor = data;
                        box.start_TextChat();

                        //Ala, lo ponemos  a cero y cargamos la cookie (que no borraremos hasta que no cerremos el soporte)
                        var name_mecookie = zentools.ApiKey + "_me";
                        var name_comsessionidcookie = zentools.ApiKey + "_commsessionid";
                        setCookie(name_mecookie, jQuery.toJSON(zentools.me), 1);
                        setCookie(name_comsessionidcookie, self._commsessionid);

                        //Lo dejamos bloqueado con esta petición... dado que ya tenemos lo que buscábamos
                        zentools.CommRequest_InCourse = true;
                        self.GetAllMessagesFromInterlocutor(data.UserName, data.Business.Domain);
                        self.GetOpenTokSessionID(self._commsessionid);
                    } catch (err) {
                        log(err);
                    }
                },
                error: function (jqXHR, textStatus) {
                    log("Error ClientDaemon.check: " + textStatus);
                    zentools.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            log(err);
        }
    }
});
