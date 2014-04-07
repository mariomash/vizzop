
var MessageBox = jVizzop.zs_Class.create(Box, {
    // constructor
    initialize: function () {
        var self = this;
        self.base('initialize');
        self._type = 'MessageBox';
        self._interlocutor = [];
        self.Messages = [];
        self.unread_elements = new Number(0);
    },
    CheckIfInterlocutorIsInList: function (interlocutor) {
        var self = this;
        var found = false;
        try {
            jVizzop(self._interlocutor).each(function (index, item) {
                if ((item.UserName == interlocutor.UserName) && (item.Business.Domain == interlocutor.Business.Domain)) {
                    found = true;
                }
            });
        } catch (err) {
            vizzoplib.log("Error CheckIfInterlocutorIsInList" + "/" + err);
        }
        return found;
    },
    AddInterlocutor: function (interlocutor) {
        var self = this;
        try {
            if (self.CheckIfInterlocutorIsInList(interlocutor) == false) {
                self._interlocutor.push(interlocutor);
            }
        } catch (err) {
            vizzoplib.log("Error AddInterlocutor" + "/" + err);
        }
    },
    GetAllDetailsFromCommSession: function () {
        var self = this;
        try {
            var msg = {
                'UserName': vizzop.me.UserName,
                'Password': vizzop.me.Password,
                'Domain': vizzop.me.Business.Domain,
                'commsessionid': self._commsessionid,
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
                    //console.vizzoplib.log(data);
                    if (data == null) {
                        return;
                    }

                    if (vizzop.mode == 'agent') {
                        self.prepareChatInfoSection(data);
                    }

                    if (data.Session) {
                        if (data.Session.Messages != null) {
                            if (data.Session.Messages.length > 0) {
                                self._col1.css('display', 'inline-block');
                            }

                            /*
                            jVizzop.each(data.Session.Messages, function (i, v) {
                                if (v.Subject.indexOf('$#_') == -1) {
                                    var newmsg = new Message(v.From.FullName, v.To.FullName, null, v.Content, self, v.CommSession.ID, v.ID);
                                    newmsg._from_username = v.From.UserName;
                                    newmsg._from_domain = v.From.Business.Domain;
                                    newmsg._to_username = v.To.UserName;
                                    newmsg._to_domain = v.To.Business.Domain;
                                    newmsg._status = "sent";
                                    newmsg._old = v.Status;
                                    newmsg._timestamp = vizzoplib.parseJsonDate(v.TimeStamp);
                                    newmsg.AddMsgToChat(newmsg);
                                }
                            });
                            */

                            var arrMessages = [];
                            jVizzop.each(data.Session.Messages, function (_i, _v) {
                                if (_v.Subject.indexOf('$#_') == -1) {
                                    arrMessages.push(_v);
                                    /*
                                    if ((_v.To.UserName + _v.To.Business.Domain == vizzop.me.UserName + vizzop.me.Business.Domain) ||
                                        (_v.From.UserName + _v.From.Business.Domain == vizzop.me.UserName + vizzop.me.Business.Domain)) {
                                        arrMessages.push(_v);
                                    }
                                    */
                                }
                            });
                            vizzop.Daemon.parseNewMessages(arrMessages);
                        }
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                }
            });
        } catch (err) {
            vizzoplib.log("Error GetAllDetailsFromCommSession" + "/" + err);
        }
    },
    process_alerts: function () {
        var self = this;
        try {
            jVizzop(self._unread_elements_div).remove();
            if (self.unread_elements > 0) {
                self._unread_elements_div = jVizzop('<span></span>')
                    .addClass('label label-warning')
                    .text(self.unread_elements)
                    .insertBefore(jVizzop('#linkTo_' + self._id).find('i')[0]);
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    prepareChatInfoSection: function (data) {
        var self = this;
        try {

            var all_info = '';
            if (data.Session) {
                if (data.Session.Client != null) {
                    if (data.Session.Client.ID != null) {
                        all_info += '<dl><dt>Client ID</dt><dd>' + data.Session.Client.ID + '</dd></dl>';
                    }
                }
                if (data.Session.CreatedOn != null) {
                    var first_message_time = vizzoplib.parseJsonDate(data.Session.CreatedOn);
                    all_info += '<dl><dt>First Access</dt><dd>' + first_message_time.toDateString() + ' ' + first_message_time.toTimeString().substring(0, (first_message_time.toTimeString().indexOf(' ') - 3)) + '</dd></dl>';
                }
            }

            if (data.Location) {
                if (data.Location.Lang != null) {
                    all_info += '<dl><dt>Language</dt><dd>' + data.Location.Lang + '</dd></dl>';
                }
                if (data.Location.Url != null) {
                    all_info += '<dl><dt>URL</dt><dd>' + data.Location.Url + '</dd></dl>';
                }
                if (data.Location.Referrer != null) {
                    all_info += '<dl><dt>Referrer</dt><dd>' + data.Location.Referrer + '</dd></dl>';
                }
                if (data.Location.FirstViewedHuman != null) {
                    all_info += '<dl><dt>First Access</dt><dd>' + data.Location.FirstViewedHuman + '</dd></dl>';
                }
                if (data.Location.LastViewedHuman != null) {
                    all_info += '<dl><dt>Last Active</dt><dd>' + data.Location.LastViewedHuman + '</dd></dl>';
                }
                /*
                if (data.Location.IP != null) {
                    all_info += '<dl><dt>IP</dt><dd>' + data.Location.IP + '</dd></dl>';
                }
                */
                if (data.Location.UserAgent != null) {
                    var browser = data.Location.UserAgent.substring(data.Location.UserAgent.lastIndexOf(" ") + 1, data.Location.length);
                    all_info += '<dl><dt>Browser</dt><dd>' + browser + '</dd></dl>';
                    var OS = data.Location.UserAgent.substring(data.Location.UserAgent.indexOf("(") + 1, data.Location.UserAgent.indexOf(")"));
                    all_info += '<dl><dt>Operating System</dt><dd>' + OS + '</dd></dl>';
                }
            }
            self._col2.css('display', 'inline-block');
            self._chatinfo.html(all_info);

            self.checkSafePosition();
            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');


            var title = data.Session.Client.ID + ' - ';
            if (self._interlocutor[0] != null) {
                if (self._interlocutor[0].FullName != "") {
                    //title = LLang('chat_with', [self._interlocutor.FullName]);
                    title += self._interlocutor[0].FullName;
                } else {
                    title += LLang('anon_client', []);
                }
            }

            jVizzop(self._boxtitletext).html('<i class="vizzop-icon-user vizzop-icon-white"></i>&nbsp;<span style="display: inline-block; vertical-align: middle">' + title + "</span>");
            self._title = title;

        } catch (err) {
            vizzoplib.log("Error prepareChatInfoSection" + "/" + err);
            self._chatinfo.html('');
        }
    },
    askforscreenshare: function () {
        var self = this;
        try {
            self._boxtitletext.attr({
                'rel': 'popover',
                'title': 'Information',
                'data-content': '<div>' + LLang('share_question', [self._interlocutor.FullName]) + '</div><div class=actions id=popover_action><button class="vizzop-btn vizzop-btn-primary" id=popover_action_ok>' + LLang('ok', null) + '</button><button class="vizzop-btn" id=popover_action_no>' + LLang('no', null) + '</button></div>'
            });
            self._boxtitletext.popover({
                placement: 'bottom',
                html: 'true',
                trigger: 'manual'
            });
            jVizzop(self._boxtitletext).popover('show');
            jVizzop('#popover_action_ok').click(function (event) {
                self.start_Screenshare();
                jVizzop(self._boxtitletext).popover('hide');
                //self.send_current_html();
                return false;
            });
            jVizzop('#popover_action_no').click(function (event) {

                jVizzop.each(self._box._interlocutor, function (index, interlocutor) {
                    var newmsg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, interlocutor.UserName + '@' + interlocutor.Business.Domain, '$#_cancelscreenshare', null, self, self._commsessionid);
                    vizzop.MsgCue.push(newmsg);
                });
                jVizzop(self._boxtitletext).popover('hide');
                return false;
            });
        } catch (err) {
            vizzoplib.log("Error askforscreenshare" + "/" + err);
        }
    },
    cancel_Screenshare: function () {
        try {
            var self = this;
            if (jVizzop(self._box).hasClass('screenshare') == true) {
                self._status = 'cancel_Screenshare';
                self._preferredwidth = 'auto';
                self._preferredheight = 'auto';
                self._col0.css('display', 'inline-block');
                self._preferredubication = 'bottomright';
                jVizzop(self._interlocutor_image).remove();

                self.button_CancelScreenSharing.hide();
                self.button_AddScreenSharing
                    .css({ display: 'inline-block' });

                jVizzop(self._box).removeClass('screenshare');
                self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
            }
        } catch (err) { vizzoplib.log(err); }
    },
    start_Screenshare: function () {
        try {
            var self = this;
            if (jVizzop(self._box).hasClass('screenshare') == false) {
                self._status = 'start_Screenshare';
                self._preferredwidth = 'auto';
                self._preferredheight = 'auto';
                self._preferredubication = 'center';

                //self._col0.css('display', 'inline-block');

                /*Screen*/
                self._boxscreenshare = jVizzop('<div></div>')
                    .addClass('boxscreenshare')
                    .css('min-width', '200px')
                    .appendTo(self._col0);

                self.interlocutor_mouse = new InterLocutorMouse(self._col0);

                self.interlocutor_image = jVizzop('<center></center>')
                    .css('padding-top', '160px')
                    .addClass('loading_plain')
                    .append(jVizzop('<div></div>').text(LLang('loading', null)))
                    .append(jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>').css({ 'text-align': 'center', 'margin-top': '5px' }))
                    .appendTo(self._boxscreenshare);

                /*Info*/
                self._boxscreenpositiondiv = jVizzop('<div></div>')
                    .addClass('boxscreensharepositiondiv')
                    .appendTo(self._col0);

                self._boxscreenshareposition = jVizzop('<div></div>')
                    .addClass("boxscreenshareposition")
                    .appendTo(self._boxscreenpositiondiv);


                /*Slider*/
                self._boxscreensliderdiv = jVizzop('<div></div>')
                    .addClass('boxscreensharesliderdiv')
                    .appendTo(self._col0)
                    .hide();

                self._boxscreenshareendtitle = jVizzop('<div></div>')
                    .addClass('boxscreenshareendtitle')
                    .appendTo(self._boxscreensliderdiv);

                self._boxscreensharerangeend = jVizzop('<div></div>')
                    .addClass('sliderendtitle')
                    .text('auto')
                    .appendTo(self._boxscreenshareendtitle);

                self._boxscreensharestarttitle = jVizzop('<div></div>')
                    .addClass('boxscreensharestarttitle')
                    .appendTo(self._boxscreensliderdiv);

                self._boxscreensharerangestart = jVizzop('<div></div>')
                    .addClass('sliderstarttitle')
                    .text('')
                    .appendTo(self._boxscreensharestarttitle);

                self.slider_interlocutor_image = jVizzop('<input />')
                    .addClass('boxscreenshareslider')
                    .attr('type', 'range')
                    .attr('min', '1')
                    .attr('step', '1')
                    .attr('max', '1')
                    .attr('value', '1')
                    .change(function () {
                        if (self.slider_interlocutor_image.attr('value') == self.slider_interlocutor_image.attr('max')) {
                        } else {
                            var index = self.slider_interlocutor_image.attr('value');
                            self._boxscreenshareloadingtext
                                .text(LLang('loading', null) + " " + self.ScreenShots[index].CreatedOn);
                        }
                        /*
                        self._boxscreenshareposition
                                    .text(LLang('loading', null) + " " + );
                                    */
                        if (this.sliderTimeour) clearTimeout(this.sliderTimeour);
                        this.sliderTimeour = setTimeout(function () {
                            try {
                                self.loadingScreen.abort();
                            } catch (ex) { }
                            if (self.slider_interlocutor_image.attr('value') == self.slider_interlocutor_image.attr('max')) {
                                self.ScreenShots.pop();
                                self.slider_interlocutor_image.attr('max', self.ScreenShots.length);
                                self.slider_interlocutor_image.attr('value', self.ScreenShots.length);
                            }
                            self.loadingScreen = null;
                            self._boxscreenshareloading.show();
                            //your code here
                        }, 500);
                    })
                    .appendTo(self._boxscreensliderdiv);


                self._boxscreenshareloadingtext = jVizzop('<h2></h2>')
                    .text(LLang('loading', null));

                self._boxscreenshareloading = jVizzop('<div></div>')
                    .addClass('boxscreenshareloading') /*.addClass('loading')*/
                    .append(self._boxscreenshareloadingtext)
                    .append(jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>').css({ 'text-align': 'center', 'margin-top': '5px' }))
                    .hide()
                    .appendTo(self._col0);

                self.button_AddScreenSharing.hide();

                self.loadScreen();

                jVizzop(self._box).addClass('screenshare');
                self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
            }
        } catch (err) { vizzoplib.log(err); }
    },
    loadScreen: function () {
        var self = this;
        try {
            if (jVizzop(self._box).hasClass('screenshare') != true) {
                return false;
            }
            //console.vizzoplib.log(self.loadingScreen);
            if (self.loadingScreen != null) {
                return false;
            }

            var url = null;
            var msg = null;
            /*
            Si no tiene lista de imagenes primero cargamos la lista...
            */

            if (self.ScreenShots == null) {
                self.ScreenShots = [];
            }

            url = vizzop.mainURL + "/RealTime/GetScreen";

            var guid = null;
            if (self.ScreenShots.length > 0) {
                guid = self.ScreenShots[self.ScreenShots.length - 1].GUID;
            }
            //var height = self._boxscreenshare.outerHeight();
            var height = 485;
            msg = {
                'username': self._interlocutor[0].UserName,
                'domain': self._interlocutor[0].Business.Domain,
                'windowname': self._interlocutor[0].WindowName,
                'guid': guid,
                'height': height
            };
            self.loadingScreen = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    self._boxscreenshareloading.hide();

                    if (data != null) {
                        if (data != false) {
                            self._col0.css('display', 'inline-block');
                            if (self.slider_interlocutor_image.attr('value') == self.slider_interlocutor_image.attr('max')) {
                                //self._boxscreenshareposition.text("auto updating");
                                self.ScreenShots.push(data);
                                self.slider_interlocutor_image.attr('max', self.ScreenShots.length);
                                self.slider_interlocutor_image.attr('value', self.ScreenShots.length);
                            } else {
                                self.slider_interlocutor_image.attr('max', self.ScreenShots.length);
                            }
                            /*
                            if (self.ScreenShots.length > 1) {
                                self._boxscreensliderdiv.show();
                            }
                            */
                            self._boxscreenshareposition
                                .text(data.CreatedOn + " - " + data.Url);

                            vizzoplib.ResizeWidthLikeImg(self, data.Data, function resize(newwidth, newimg) {
                                jVizzop(newimg)
                                    .attr('style', 'width: ' + newwidth.toFixed(0) + 'px !important;')
                                    .addClass('interlocutor_img')
                                    .appendTo(self._boxscreenshare);
                                jVizzop(self._boxscreenshare).children().each(function (index, item) {
                                    if (item != newimg) {
                                        jVizzop(item).remove();
                                    }
                                });

                                self.checkSafePosition();

                                self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');

                                if (self.slider_interlocutor_image.attr('value') == self.slider_interlocutor_image.attr('max')) {
                                    self.loadingScreen = null;
                                }
                            });
                        } else {
                            if (self.slider_interlocutor_image.attr('value') == self.slider_interlocutor_image.attr('max')) {
                                self.loadingScreen = null;
                            }
                        }
                    } else {
                        if (self.slider_interlocutor_image.attr('value') == self.slider_interlocutor_image.attr('max')) {
                            self.loadingScreen = null;
                        }
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.log(url, msg, jqXHR);
                    if (self.slider_interlocutor_image.attr('value') == self.slider_interlocutor_image.attr('max')) {
                        self.loadingScreen = null;
                    }
                }
            });

        } catch (err) {
            vizzoplib.log(err);
            if (self.slider_interlocutor_image.attr('value') == self.slider_interlocutor_image.attr('max')) {
                self.loadingScreen = null;
            }
        }
    },
    askforvideo: function () {
        var self = this;
        try {
            self._boxtitletext.attr({
                'rel': 'popover',
                'title': 'Information',
                'data-content': '<div>' + LLang('video_question', [self._interlocutor.FullName]) + '</div><div class=actions id=popover_action><button class="vizzop-btn vizzop-btn-primary" id=popover_action_ok>' + LLang('ok', null) + '</button><button class="vizzop-btn" id=popover_action_no>' + LLang('no', null) + '</button></div>'
            });
            self._boxtitletext.popover({
                placement: 'bottom',
                html: 'true',
                trigger: 'manual'
            });
            jVizzop(self._boxtitletext).popover('show');
            jVizzop('#popover_action_ok').click(function (event) {
                self.start_VideoChat();
                jVizzop(self._boxtitletext).popover('hide');
                return false;
            });
            jVizzop('#popover_action_no').click(function (event) {
                self.cancel_VideoChat();
                jVizzop(self._boxtitletext).popover('hide');
                return false;
            });

        } catch (err) {
            vizzoplib.log(err);
        }
    },
    start_VideoChat: function () {
        var self = this;
        try {
            if (jVizzop(self._box).hasClass('videochat') == false) {
                self._status = 'start_VideoChat';
                //self._preferredwidth = '260px';
                self._preferredwidth = 'auto';
                self._preferredheight = 'auto';
                if (jVizzop(self._box).hasClass('screenshare') == false) {
                    self._preferredubication = 'bottomright';
                }
                self.hideBox();

                self._boxvideochat.empty();
                //loading video
                self._boxvideochat._videoZone = jVizzop('<span></span>')
                        .addClass('chat_videoZone')
                        .appendTo(self._boxvideochat);

                self.button_AddVideo.hide();
                self.button_CancelVideo.css({ display: 'inline-block' });
                self._boxvideochat.show();

                self._loadingimg = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                        .css({ 'margin-top': '60px' })
                        .css({ 'margin-bottom': '10px' })
                        .appendTo(self._boxvideochat._videoZone);
                self._loadingvideotext = jVizzop('<div></div>')
                    .html(LLang('loading_video', null))
                    .appendTo(self._boxvideochat._videoZone);

                try {
                    jVizzop(self._boxtextchat.msgZone).scrollTop(jVizzop(self._boxtextchat.msgZone).outerScrollHeight());
                    //jVizzop(box._boxtextchat.msgZone).attr({ scrollTop: jVizzop(box._boxtextchat.msgZone).attr("scrollHeight") });
                } catch (err) {
                    vizzoplib.log(err);
                }

                if ((self._OpenTokSessionID == null) || (self._OpenTokToken == null)) {
                    /* Si no tenemos el sessionID lo pedimos.. y si se le pasa null trae los dos, y si se le pasa un ID adecuado trae solo un TOKEN que valga ;) */
                    try {
                        var msg = {
                            'UserName': vizzop.me.UserName,
                            'Password': vizzop.me.Password,
                            'Domain': vizzop.me.Business.Domain,
                            'commsessionid': self._commsessionid
                        };
                        var url = vizzop.mainURL + "/Comm/GetOpenTokSessionID";
                        var ajaxrequest = jVizzop.ajax({
                            url: url,
                            type: "POST",
                            data: msg,
                            dataType: "jsonp",
                            beforeSend: function (xhr) {
                            },
                            success: function (data) {
                                if (data == false) {
                                    self.cancel_VideoChat(true);
                                    return;
                                }
                                self._OpenTokSessionID = data.OpenTokSessionID;
                                self._OpenTokToken = data.OpenTokToken;
                                self.start_VideoChatOpenTok();
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                vizzoplib.logAjax(url, msg, jqXHR);
                                self.cancel_VideoChat(true);
                                self.button_AddVideo.hide();
                                self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
                                self.cancel_VideoChat();
                            }
                        });
                    } catch (err) {
                        vizzoplib.log(err);
                        self._boxvideochat.hide();
                        self.button_AddVideo.hide();
                        self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
                        self.cancel_VideoChat();
                    }
                } else {
                    self.start_VideoChatOpenTok();
                }

                jVizzop(self._box).addClass('videochat');
                self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
            }
        } catch (err) {
            vizzoplib.log(err);
            self.cancel_VideoChat();
            self.button_AddVideo.hide();
            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
        }
    },
    addStream: function (stream) {
        var self = this;
        try {
            // Check if this is the stream that I am publishing, and if so do not publish.
            if (stream.connection.connectionId == self._OpenTokSession.connection.connectionId) {
                return;
            }
            //self._boxvideochat._videoZone.empty();
            self._boxvideochat.videoChat_interlocutor_Placemark = jVizzop('<span></span>')
                                                .addClass('videochat_interlocutor')
                                                .appendTo(self._boxvideochat._videoZone);
            self._boxvideochat.videoChat_interlocutor = jVizzop('<div></div>')
                                                .attr('id', 'videoChat_interlocutor')
                                                .appendTo(self._boxvideochat.videoChat_interlocutor_Placemark);
            self._OpenTokSubscribers[stream.streamId] = self._OpenTokSession.subscribe(stream, 'videoChat_interlocutor');
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    // Called when user wants to start publishing to the session
    startPublishing: function () {
        try {
            var self = this;
            if (!self._OpenTokPublisher) {
                self._boxvideochat.videoChat_me_Placemark = jVizzop('<span></span>')
                                        .addClass('videochat_beforeallow')
                                        .appendTo(self._boxvideochat._videoZone);
                self._boxvideochat.videoChat_me = jVizzop('<div></div>')
                                        .attr('id', 'videoChat_me')
                                        .appendTo(self._boxvideochat.videoChat_me_Placemark);
                self._OpenTokPublisher = self._OpenTokSession.publish('videoChat_me'); // Pass the replacement div id to the publish method
                self._OpenTokPublisher.addEventListener('accessAllowed', function (event) {
                    try {
                        self._boxvideochat.videoChat_me_Placemark.removeClass('videochat_beforeallow');
                        self._boxvideochat.videoChat_me_Placemark.addClass('videochat_me');
                        //vizzoplib.log(vizzop.opentok_videostreams);
                        //try { self._boxvideochat.videoChat_interlocutor_Placemark.remove(); } catch (err) { }
                        // Subscribe to the newly created streams
                        //self._OpenTok
                        /*
                        for (var i = 0; i < event.streams.length; i++) {
                        self.addStream(self._OpenTokStreams[i]);
                        }
                        */
                        var _el = jVizzop('#' + self._OpenTokPublisher.id);
                        _el.attr({
                            'width': '120',
                            'height': '90'
                        });
                        var newmsg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, self._interlocutor[0].UserName + '@' + self._interlocutor[0].Business.Domain, '$#_ask4video', self._commsessionid, self, self._commsessionid);
                        vizzop.MsgCue.push(newmsg);
                    } catch (err) {
                        vizzoplib.log(err);
                        self.cancel_VideoChat();
                    }
                });
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    start_VideoChatOpenTok: function () {
        var self = this;
        try {
            self._OpenTokSession = null; //TB.initSession();
            self._OpenTokPublisher = null;
            self._OpenTokSubscribers = {};
            self._OpenTokStreams = {};
            // Un-comment either of the following to set automatic logging and exception handling.
            // See the exceptionHandler() method below.
            TB.setLogLevel(TB.DEBUG);
            TB.addEventListener("exception", function (event) {
                vizzoplib.log("Exception: " + event.code + "::" + event.message);
                self.cancel_VideoChat();
            });
            if (TB.checkSystemRequirements() != TB.HAS_REQUIREMENTS) {
                alert("You don't have the minimum requirements to run this application."
                  + "Please upgrade to the latest version of Flash.");
            } else {
                self._OpenTokSession = TB.initSession(self._OpenTokSessionID); // Initialize session
                // Add event listeners to the session
                self._OpenTokSession.addEventListener('sessionConnected', function (event) {
                    // Subscribe to all streams currently in the Session
                    self._OpenTokStreams = event.streams;
                    //event.streams.length
                    for (var i = 0; i < event.streams.length; i++) {
                        self.addStream(event.streams[i]);
                    }
                    self.startPublishing();
                });
                self._OpenTokSession.addEventListener('sessionDisconnected', function (event) {
                    // This signals that the user was disconnected from the Session. Any subscribers and publishers
                    // will automatically be removed. This default behaviour can be prevented using event.preventDefault()
                    self._OpenTokSession = null;
                });
                self._OpenTokSession.addEventListener('connectionCreated', function (event) {
                    // This signals new connections have been created.
                });
                self._OpenTokSession.addEventListener('connectionDestroyed', function (event) {
                    // This signals that connections were destroyed
                });
                self._OpenTokSession.addEventListener('streamCreated', function (event) {
                    self._OpenTokStreams = event.streams;
                    // Subscribe to the newly created streams
                    for (var i = 0; i < event.streams.length; i++) {
                        self.addStream(event.streams[i]);
                    }
                });
                self._OpenTokSession.addEventListener('streamDestroyed', function (event) {
                    // This signals that a stream was destroyed. Any Subscribers will automatically be removed.
                    // This default behaviour can be prevented using event.preventDefault()
                });
                self._OpenTokSession.connect(vizzop.opentok_apiKey, self._OpenTokToken);
            }

            //--------------------------------------
            //  LINK CLICK HANDLERS
            //--------------------------------------

            /*
            If testing the app from the desktop, be sure to check the Flash Player Global Security setting
            to allow the page from communicating with SWF content loaded from the web. For more information,
            see http://www.tokbox.com/opentok/build/tutorials/helloworld.html#localTest
            */
            function connect() {
            }

            function disconnect() {
                self._OpenTokSession.disconnect();
            }


            function stopPublishing() {
                if (self._OpenTokPublisher) {
                    self._OpenTokSession.unpublish(self._OpenTokPublisher);
                }
                self._OpenTokPublisher = null;
            }

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
        } catch (err) {
            vizzoplib.log(err);
            self.cancel_VideoChat();
        }
    },
    cancel_VideoChat: function (dont_send_msg) {
        var self = this;
        try {
            if (self._OpenTokSession) {
                self._OpenTokSession.disconnect();
                self._opentok_publisher = null;
                self._opentok_videostreams = null;
                self._opentok_session = null;
                self._OpenTokSessionID = null;
            }
            self._boxvideochat.hide();
            jVizzop(self._box).removeClass('videochat');
            self._boxvideochat.empty();
            self.button_CancelVideo.hide();
            self.button_AddVideo.css({ display: 'inline-block' });
            try { self._handle.hide(); } catch (err) { }
            self._boxinner.show();
            self.positionBox(null, 0);
            if (dont_send_msg == null) {
                var newmsg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, self._interlocutor[0].UserName + '@' + self._interlocutor[0].Business.Domain, '$#_cancelvideo', self._commsessionid, self, self._commsessionid);
                vizzop.MsgCue.push(newmsg);
            }
        } catch (err) {
            vizzoplib.log("start_TextChat: " + err);
        }
    },
    start_TextChat: function () {
        try {
            var self = this;
            var name_acceptschatidcookie = vizzop.ApiKey + "_accepts_chat";
            if (jVizzop(self._box).hasClass('chat') == false) {

                if (vizzop.mode == 'client') {
                    if (jVizzop.cookie(name_acceptschatidcookie) == null) {
                        if (self._status != 'fillBox_FindingSupport_WithName') {
                            self._box
                                .unbind('mouseenter mouseleave click')
                                .click(function (event) {
                                    //Ahora mostramos todo
                                    //Marcamos esto como que acepta los chats... así cuando vaya  otras páginas siempre abrirá etc...
                                    vizzoplib.setCookie(name_acceptschatidcookie, self._commsessionid);
                                    try { jVizzop(self._handle).remove(); } catch (err) { }
                                    self._boxinfo
                                        .show();

                                    self._boxtitletext
                                        .show();

                                    self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
                                });
                            self._boxinfo.hide();
                            self._boxtitletext.hide();
                            self._bubbletext
                                .html(LLang('new_message_from_agent', null));
                            self._bubble.fadeOut(300, function () {
                                self._bubble.fadeIn(300, function () {
                                    self._bubble.fadeOut(300, function () {
                                        self._bubble.fadeIn(300, function () {
                                            self._bubble.fadeOut(300, function () {
                                                self._bubble.fadeIn(300, function () {
                                                });
                                            });
                                        });
                                    });
                                });
                            });
                        } else {
                            try { jVizzop(self._handle).remove(); } catch (err) { }
                            vizzoplib.setCookie(name_acceptschatidcookie, self._commsessionid);
                        }
                    } else {
                        try { jVizzop(self._handle).remove(); } catch (err) { }
                    }
                }

                self._preferredubication = 'bottomright';
                //self._preferredwidth = '260px';
                self._preferredwidth = 'auto';
                self._preferredheight = 'auto';
                self._status = 'start_TextChat';

                /*
                self.hideBox();
                */

                self._boxinner.show();
                self._boxinfo.empty();
                self._boxtitletext.empty();

                self._nolist = false;

                var title = "";
                if (self._interlocutor[0] != null) {
                    if (self._interlocutor[0].FullName != "") {
                        //title = LLang('chat_with', [self._interlocutor.FullName]);
                        title = self._interlocutor[0].FullName;
                    } else {
                        title = LLang('anon_client', []);
                    }
                }

                jVizzop(self._boxtitletext).html('<i class="vizzop-icon-user vizzop-icon-white"></i>&nbsp;<span style="display: inline-block; vertical-align: middle">' + title + "</span>");
                self._title = title;

                //status text
                //jVizzop(self._chatinfo).html(self._statustext);

                //Header Box
                self._boxheader = jVizzop('<div></div>')
                        .addClass('boxheader')
                        .hide()
                        .appendTo(self._boxinfo);

                self._boxcontents = jVizzop('<div></div>')
                    .css('white-space', 'nowrap')
                    .appendTo(self._boxinfo);

                //share
                self._col0 = jVizzop('<span></span>')
                        .addClass('col0')
                        .appendTo(self._boxcontents)
                        .hide();

                //Text
                self._col1 = jVizzop('<span></span>')
                        .addClass('col1')
                        .appendTo(self._boxcontents);

                self._boxtextchat = jVizzop('<div></div>')
                        .addClass('boxtextchat')
                        .appendTo(self._col1);

                //Video & Info
                self._col2 = jVizzop('<span></span>')
                        .addClass('col2')
                        .hide()
                        .appendTo(self._boxcontents);

                self._boxvideochat = jVizzop('<div></div>')
                        .addClass('boxvideochat')
                        .appendTo(self._col2)
                        .hide();


                self._convinfo = jVizzop('<div></div>')
                        .addClass('chatinfo')
                        .appendTo(self._col2);

                self._chatinfo = jVizzop('<div></div>')
                        .addClass('chatinfo')
                        .appendTo(self._col2);

                var details = jVizzop('<center></center>')
                    .addClass('loading_plain')
                    .append(jVizzop('<div></div>').text(LLang('loading', null)))
                    .append(jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>').css({ 'text-align': 'center', 'margin-top': '5px' }))
                    .appendTo(self._chatinfo);

                //console.vizzoplib.log(self._commsessionid);
                if (self._commsessionid != null) {
                    //self._col1.css('display', 'inline-block');
                    //self._col2.css('display', 'inline-block');
                    if (vizzop.mode == 'agent') {
                        self.GetAllDetailsFromCommSession();
                    }
                    //Añadimos botones si soporta Video Chat
                    /*
                    var playerVersion = swfobject.getFlashPlayerVersion();
                    if (playerVersion.major >= 10) {
                        self.button_AddVideo = jVizzop('<button></button>')
                            .html('<i class="vizzop-icon-facetime-video"></i>&nbsp;' + LLang('add_video', null))
                            .addClass('vizzop-btn small')
                            .css({ display: 'inline-block' })
                            .click(function (event) {
                                self.start_VideoChat();
                                return false;
                            })
                            .hide()
                            .appendTo(self._boxheader);
                        self.button_CancelVideo = jVizzop('<button></button>')
                            .text(LLang('cancel_video', null))
                            .addClass('vizzop-btn small')
                            .click(function (event) {
                                self.cancel_VideoChat();
                                return false;
                            })
                            .hide()
                            .appendTo(self._boxheader);
                    }
                    */
                }

                //Añadimos resto de botones
                if (vizzop.mode == 'agent') {
                    self.button_AddScreenSharing = jVizzop('<button></button>')
                        .html('<i class="vizzop-icon-eye-open"></i>&nbsp;' + LLang('screen_view', null))
                        .addClass('vizzop-btn small')
                        .css({ display: 'inline-block' })
                        .click(function (event) {
                            self.start_Screenshare();
                            return false;
                        })
                        .appendTo(self._boxheader);

                    self.button_CancelScreenSharing = jVizzop('<button></button>')
                        .text(LLang('cancel_screen_share', null))
                        .addClass('vizzop-btn small')
                        .css({ display: 'inline-block' })
                        .click(function (event) {
                            self.cancel_Screenshare();
                            return false;
                        })
                        .hide()
                        .appendTo(self._boxheader);
                }

                self._boxtextchat.photoZone = jVizzop('<span></span>')
                        .addClass('chat_photoZone')
                        .appendTo(self._boxtextchat)
                        .hide();

                self._boxtextchat.photoZone.photo = jVizzop('<span></span>')
                        .addClass('photoZone_photo')
                        .appendTo(self._boxtextchat.photoZone);

                self._boxtextchat.photoZone.name = jVizzop('<span></span>')
                        .addClass('photoZone_name')
                        .appendTo(self._boxtextchat.photoZone);

                self._boxtextchat.msgZone = jVizzop('<span></span>')
                        .addClass('chat_msgZone')
                        .appendTo(self._boxtextchat);

                self._boxtextchat.infoZone = jVizzop('<span></span>')
                        .addClass('chat_infoZone')
                        .appendTo(self._boxtextchat);

                self._boxtextchat.inputZone = jVizzop('<span></span>')
                        .addClass('chat_inputZone')
                        .appendTo(self._boxtextchat);

                self._boxtextchat.inputTextArea = jVizzop('<textarea/>')
                        .attr('id', 'zs_message')
                        .attr('name', 'zs_message')
                        .addClass('chat_inputTextArea hint')
                        .focus(function (event) {
                            var target = jVizzop(event.target);
                            if (target.hasClass('hint') == true) {
                                target.val('');
                                target.removeClass('hint');
                            } else {
                                target.select();
                            }
                        })
                        .focusin(function () {
                            /*
                                var newmsg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_inputfocus_in', self._commsessionid, self, self._commsessionid);
                                newmsg._from_fullname = vizzop.me.FullName;
                                new_Cue = [];
                                jVizzop.each(vizzop.MsgCue, function (i, v) {
                                    if ((v._subject != '$#_inputfocus_in') && (v._subject != '$#_inputfocus_out')) {
                                        new_Cue.push(v);
                                    }
                                });
                                vizzop.MsgCue = new_Cue;
                                vizzop.MsgCue.push(newmsg);
                                */
                        })
                        .focusout(function () {
                            /*
                            jVizzop.each(self._interlocutor, function (index, interlocutor) {
                                var newmsg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, interlocutor.UserName + '@' + interlocutor.Business.Domain, '$#_inputfocus_out', self._commsessionid, self, self._commsessionid);
                                newmsg._from_fullname = vizzop.me.FullName;
                                vizzop.MsgCue.push(newmsg);
                            });
                            */
                        })
                    .appendTo(self._boxtextchat.inputZone);

                self._boxtextchat.inputTextArea.focus();

                self._boxtextchat.inputTextArea.keypress(function (e) {
                    if (e.keyCode == 13) {
                        e.preventDefault();
                        var input = this;
                        var text = jVizzop.trim(jVizzop(input).val());
                        if (text != '') {
                            if ((typeof self._interlocutor != "undefined") && (self._interlocutor != null)) {
                                var msg = null;
                                var to_list = '';
                                jVizzop.each(self._interlocutor, function (index, interlocutor) {
                                    if (vizzop.me.UserName + '@' + vizzop.me.Business.Domain != interlocutor.UserName + '@' + interlocutor.Business.Domain) {
                                        to_list += ',' + interlocutor.FullName + '::' + interlocutor.UserName + '@' + interlocutor.Business.Domain
                                    }
                                });
                                if (to_list.length > 0) {
                                    to_list = to_list.slice(1);
                                }
                                msg = new Message(
                                    vizzop.me.UserName + '@' + vizzop.me.Business.Domain,
                                    to_list,
                                    null,
                                    text,
                                    self,
                                    self._commsessionid);
                                msg._from_username = vizzop.me.UserName;
                                msg._from_domain = vizzop.me.Business.Domain;
                                msg._from_fullname = vizzop.me.FullName;

                                /*
                                msg._to_username = to_list;
                                newmsg._to_domain = to_list;
                                */

                                jVizzop(msg).bind('sent', function (e, value) {
                                    if (value._status == "error") {
                                        msg.MarkAsError();
                                    } else {
                                        msg.MarkAsOk();
                                    }
                                });

                                vizzop.MsgCue.push(msg);
                                vizzop.Daemon.sendNewMessages();
                                msg.AddMsgToChat(msg);
                                jVizzop(input).val(null);
                            }
                        }
                        if (jVizzop.trim(jVizzop(this).val()) == '') {
                            jVizzop(this).val(null);
                        }
                    } else {

                        if ((typeof self._interlocutor != "undefined") && (self._interlocutor != null)) {

                            var to_list = '';
                            jVizzop.each(self._interlocutor, function (index, interlocutor) {
                                if (vizzop.me.UserName + '@' + vizzop.me.Business.Domain != interlocutor.UserName + '@' + interlocutor.Business.Domain) {
                                    to_list += ',' + interlocutor.FullName + '::' + interlocutor.UserName + '@' + interlocutor.Business.Domain
                                }
                            });
                            if (to_list.length > 0) {
                                to_list = to_list.slice(1);
                            }
                            var found = false;
                            jVizzop.each(vizzop.MsgCue, function (i, v) {
                                if (v._subject == '$#_inputfocus_in') {
                                    found = true;
                                    return false;
                                }
                            });
                            if (found == false) {
                                var newmsg = new Message(
                                    vizzop.me.UserName + '@' + vizzop.me.Business.Domain,
                                    to_list,
                                    '$#_inputfocus_in',
                                    self._commsessionid,
                                    self,
                                    self._commsessionid
                                    );
                                newmsg._from_fullname = vizzop.me.FullName;
                                vizzop.MsgCue.push(newmsg);
                            }
                        }

                    }
                });

                /*
                jVizzop.each(self.Messages, function (i, v) {
                    v.AddMsgToChat(v);
                    return;
                });
                */

                self._closebutton
                    .show()
                    .click(function (event) {
                        self.closeSession();
                        /*
                        if (typeof self._commsessionid == "undefined") {
                            self.closeSession();
                        } else {
                            //self.doFinalizeCommRequest();
                            //self.destroyBox();
                            self.askforendsession();
                        }
                        */
                        return false;
                    });


                self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');


                jVizzop(self._box).addClass('chat');
            }
        } catch (err) {
            vizzoplib.log("start_TextChat: " + err);
        }
    },
    ConverserUpdateName: function (FullName) {
        var self = this;
        try {
            var data_to_send = {
                'UserName': vizzop.me.UserName,
                'FullName': FullName,
                'Domain': vizzop.me.Business.Domain,
                'Password': vizzop.me.Password
            };
            var url = vizzop.mainURL + "/Converser/ChangeName";
            var request = jVizzop.ajax({
                url: url,
                type: "POST",
                data: data_to_send,
                dataType: "jsonp",
                success: function (data) {
                    if (data == null) {
                        jVizzop(self).trigger("ConverserUpdateNameFinished", null);
                        return;
                    } else {
                        if (data == false) {
                            jVizzop(self).trigger("ConverserUpdateNameFinished", null);
                            return;
                        } else {
                            jVizzop(self).trigger("ConverserUpdateNameFinished", data);
                            return;
                        }
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    jVizzop(self).trigger("ConverserUpdateNameFinished", null);
                    return;
                }
            });
        } catch (err) {
            vizzoplib.log("ConverserUpdateName " + err);
        }
    },
    inform_notavailable: function () {
        var self = this;
        try {
            if (jVizzop(self._box).hasClass('chat') == false) {
                return false;
            }
            if (typeof self._boxtextchat != "undefined") {
                jVizzop(self._boxtextchat.infoZone)
                    .text(LLang('interlocutor_not_available', [self._interlocutor.FullName]))
                    .addClass('warning');
                jVizzop(self._boxtextchat.inputTextArea)
                    .addClass('disabled')
                    .attr('disabled', 'disabled');
                if (self.button_CancelVideo != null) {
                    jVizzop(self.button_CancelVideo.hide())
                        .addClass('disabled')
                        .attr('disabled', 'disabled');
                }
                if (self.button_AddVideo != null) {
                    jVizzop(self.button_AddVideo.show())
                        .addClass('disabled')
                        .attr('disabled', 'disabled');
                }
                if (self.button_AddScreenSharing != null) {
                    jVizzop(self.button_AddScreenSharing)
                        .addClass('disabled')
                        .attr('disabled', 'disabled');
                }
                if (self.button_CancelScreenSharing != null) {
                    jVizzop(self.button_CancelScreenSharing)
                        .addClass('disabled')
                        .attr('disabled', 'disabled');
                    jVizzop(self._boxtextchat.msgZone).focus();
                }
            }
        } catch (err) {
            vizzoplib.log("inform_notavailable " + err);
        }
    },
    inform_available: function () {
        var self = this;
        try {
            if (jVizzop(self._box).hasClass('chat') == false) {
                return false;
            }
            if (typeof self._boxtextchat != "undefined") {
                if (jVizzop(self._boxtextchat.inputTextArea).hasClass('disabled') == true) {
                    jVizzop(self._boxtextchat.inputTextArea)
                        .removeClass('disabled')
                        .removeAttr('disabled');
                    jVizzop(self._boxtextchat.infoZone)
                        .text("")
                        .removeClass('warning');
                    if (self.button_CancelVideo != null) {
                        jVizzop(self.button_CancelVideo.hide())
                            .removeClass('disabled')
                            .removeAttr('disabled');
                    }
                    if (self.button_AddVideo != null) {
                        jVizzop(self.button_AddVideo.show())
                            .removeClass('disabled')
                            .removeAttr('disabled');
                    }
                    if (self.button_AddScreenSharing != null) {
                        jVizzop(self.button_AddScreenSharing)
                            .removeClass('disabled')
                            .removeAttr('disabled');
                    }
                    if (self.button_CancelScreenSharing != null) {
                        jVizzop(self.button_CancelScreenSharing)
                            .removeClass('disabled')
                            .removeAttr('disabled');
                    }
                    jVizzop(self._boxtextchat.inputTextArea)
                        .focus();
                }
            }
        } catch (err) {
            vizzoplib.log("inform_available " + err);
        }
    },
    closeSession: function () {
        var self = this;
        try {
            if (self._OpenTokSession != null) {
                self.cancel_VideoChat();
            }
            if (vizzop.mode == 'client') {
                self._commsessionid = null;
                vizzop.Daemon._commsessionid = null;
                vizzop.CommRequest_InCourse = null;

                var name_mecookie = vizzop.ApiKey + "_me";
                var name_comsessionidcookie = vizzop.ApiKey + "_commsessionid";
                vizzoplib.deleteCookie(name_comsessionidcookie);
                vizzoplib.deleteCookie(name_mecookie);
                self.destroyBox();
                vizzop.Daemon.clientmessagebox = new ClientMessageBox();
                vizzop.Daemon.clientmessagebox.fillBox_helpStandby();
            } else {
                self.destroyBox();
            }
            /*
            if (typeof self._boxtextchat != "undefined") {
            jVizzop(self._boxtextchat.inputTextArea)
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jVizzop(self.button_CancelVideo.hide())
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jVizzop(self.button_AddVideo.show())
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jVizzop(self._boxtextchat.infoZone)
            .text(LLang('interlocutor_closed_chat', [self._interlocutor.FullName]))
            .addClass('warning');
            jVizzop(self.button_AddScreenSharing)
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jVizzop(self.button_CancelScreenSharing)
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jVizzop(self._boxtextchat.msgZone).focus();
            }
            self.destroyBox();
            */
        } catch (_err) {
            vizzoplib.log("closeSession: " + _err);
        }
    },
    doFinalizeCommRequest: function () {
        var self = this;
        try {
            var data_to_send = {
                'commsessionid': self._commsessionid,
                'username': vizzop.me.UserName,
                'password': vizzop.me.Password,
                'domain': vizzop.me.Business.Domain
            };
            var url = vizzop.mainURL + "/Comm/FinalizeCommRequest";
            var request = jVizzop.ajax({
                url: url,
                type: "POST",
                data: data_to_send,
                dataType: "jsonp",
                success: function (data) {
                    //alert(data);
                    //vizzoplib.log(self._status);
                    if (data == false) {
                        jVizzop(self.loading).hide();
                        jVizzop(self.buttonyes)
                            .removeClass('disabled')
                            .show();
                        jVizzop(self.buttonno)
                            .removeClass('disabled')
                            .show();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    jVizzop(self.loading).hide();
                    jVizzop(self.buttonyes)
                                .removeClass('disabled')
                                .show();
                    jVizzop(self.buttonno)
                                .removeClass('disabled')
                                .show();
                }
            });
        } catch (err) {
            vizzoplib.log("doFinalizeCommRequest " + err);
            jVizzop(self.loading).hide();
            jVizzop(self.buttonyes)
                .removeClass('disabled')
                .show();
            jVizzop(self.buttonno)
                .removeClass('disabled')
                .show();
        }
    },
    checkLogin: function () {
        var self = this;
        try {
            var data = {
                'username': jVizzop(self.inputUser).val(),
                'password': jVizzop(self.inputPassword).val()
            };
            var url = vizzop.mainURL + "/Converser/checkLogin";
            var request = jVizzop.ajax({
                url: url,
                type: "POST",
                data: data,
                dataType: "jsonp",
                success: function (data) {
                    if (data != null) {
                        if (data != false) {
                            vizzop.me = data;
                            vizzoplib.setCookie("me", jVizzop.toJSON(vizzop.me), 1);
                            self.destroyBox();
                        } else {
                            jVizzop(self.inputUser)
                                .removeClass('disabled')
                                .removeAttr("disabled");
                            jVizzop(self.inputPassword)
                                .removeClass('disabled')
                                .removeAttr("disabled");
                            jVizzop(self.buttoninputLogin)
                                .removeClass('disabled')
                                .show();
                            jVizzop(self.loading).hide();
                            jVizzop(self.alert)
                                .html(LLang('error_login', null))
                                .fadeIn();
                        }
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    jVizzop(self.inputUser)
                    .removeClass('disabled')
                    .removeAttr("disabled");
                    jVizzop(self.inputPassword)
                    .removeClass('disabled')
                    .removeAttr("disabled");
                    jVizzop(self.buttoninputLogin)
                    .removeClass('disabled')
                    .show();
                    jVizzop(self.loading).hide();
                    jVizzop(self.alert)
                    .html(LLang('error_login', null))
                    .fadeIn();
                    return false;
                }
            });
        } catch (err) {
            vizzoplib.log("checkLogin " + err);
        }
    },
    askforendsession: function () {
        var self = this;
        try {
            self._boxtitletext.attr({
                'rel': 'popover',
                'title': 'Information',
                'data-content': '<div>' + LLang('end_session', null) + '</div><div class=actions id=popover_action><button class="vizzop-btn vizzop-btn-primary" id=popover_action_ok>' + LLang('ok', null) + '</button><button class="vizzop-btn" id=popover_action_no>' + LLang('no', null) + '</button></div>'
            });
            self._boxtitletext.popover({
                placement: 'bottom',
                html: 'true',
                trigger: 'manual'
            });
            jVizzop(self._boxtitletext).popover('show');
            jVizzop('#popover_action_ok').click(function (event) {
                self.doFinalizeCommRequest();
                jVizzop(self._boxtitletext).popover('hide');
                self._boxtitletext.attr({
                    'rel': 'popover',
                    'title': 'Information',
                    'data-content': '<div>' + LLang('wait_end_session', null) + '</div><div class=actions id=popover_action><img style="margin: 8px 5px; vertical-align: middle; text-align: center;" src="' + vizzop.mainURL + '/Content/images/loading.gif"/></div>'
                });
                self._boxtitletext.popover({
                    placement: 'bottom',
                    html: 'true',
                    trigger: 'manual'
                });
                jVizzop(self._boxtitletext).popover('show');
                return false;
            });
            jVizzop('#popover_action_no').click(function (event) {
                jVizzop(self._boxtitletext).popover('hide');
                return false;
            });
        } catch (err) {
            vizzoplib.log("askforendsession " + err);
        }
    },
    fillBox_RequestStartChat: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_RequestStartChat';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jVizzop('<div></div>')
                .addClass('zs_boxinfocontents')
                .appendTo(self._boxinfo);

            msgbox.html(null);

            var text = jVizzop('<h5></h5>')
                .text(LLang('waiting_startchat', [vizzop.me.FullName]))
                .appendTo(msgbox);

            var loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px' })
                .appendTo(msgbox);

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');

            self.RequestStartChat();

        } catch (err) {
            vizzoplib.log("fillBox_RequestStartChat " + err);
        }
    },
    fillBox_RequestScreenView: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_RequestScreenView';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jVizzop('<div></div>')
                .addClass('zs_boxinfocontents')
                .appendTo(self._boxinfo);

            msgbox.html(null);

            var text = jVizzop('<h5></h5>')
                .text(LLang('loading', []))
                .appendTo(msgbox);

            var loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px' })
                .appendTo(msgbox);

            self._closebutton
                .show()
                .click(function (event) {
                    self.destroyBox();
                    return false;
                });

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');

            self.RequestScreenView();

        } catch (err) {
            vizzoplib.log("fillBox_RequestScreenView " + err);
        }
    },
    fillBox_CancelChat: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_CancelChat';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jVizzop('<div></div>')
                .addClass('zs_boxinfocontents')
                .appendTo(self._boxinfo);

            msgbox.html(null);

            var text = jVizzop('<h5></h5>')
                .text(LLang('chat_cancelled', [vizzop.me.FullName]))
                .appendTo(msgbox);

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');

        } catch (err) {
            vizzoplib.log("fillBox_CancelChat " + err);
        }
    },
    RequestScreenView: function () {
        var self = this;
        try {
            var msg = {
                'apikey': vizzop.ApiKey,
                'agent_username': vizzop.me.UserName,
                'agent_password': vizzop.me.Password,
                'agent_domain': vizzop.me.Business.Domain,
                'username': self._interlocutor[0].UserName,
                'domain': self._interlocutor[0].Business.Domain,
                'password': self._interlocutor[0].Password
            };
            var url = vizzop.mainURL + "/Comm/Requestcommsessionid";
            //console.vizzoplib.log(msg);
            vizzop.CommRequest_InCourse = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                success: function (data) {
                    //console.vizzoplib.log(data);
                    if (data != false) {
                        self._commsessionid = data;
                        var newmsg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, self._interlocutor[0].UserName + '@' + self._interlocutor[0].Business.Domain, '$#_startsession', self._commsessionid, self, self._commsessionid);
                        newmsg._from_fullname = vizzop.me.FullName;
                        jVizzop(newmsg).bind('sent', function (e, value) {
                            if (value._status == "error") {
                                newmsg.MarkAsError();
                                self.destroyBox();
                            } else {
                                self.start_TextChat();
                                if (self._deferred != null) {
                                    self._col1.hide();
                                }
                                self.start_Screenshare();
                            }
                        });
                        vizzop.MsgCue.push(newmsg);
                    } else {
                        self.destroyBox();
                    }
                    vizzop.CommRequest_InCourse = null;
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
    RequestStartChat: function () {
        var self = this;
        try {
            var msg = {
                'apikey': vizzop.ApiKey,
                'agent_username': vizzop.me.UserName,
                'agent_password': vizzop.me.Password,
                'agent_domain': vizzop.me.Business.Domain,
                'username': self._interlocutor.UserName,
                'domain': self._interlocutor.Business.Domain,
                'password': self._interlocutor.Password
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
                        jVizzop.each(self._interlocutor, function (index, interlocutor) {
                            var newmsg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, interlocutor.UserName + '@' + interlocutor.Business.Domain, '$#_startsession', self._commsessionid, self, self._commsessionid);
                            newmsg._from_fullname = vizzop.me.FullName;
                            vizzop.MsgCue.push(newmsg);
                            jVizzop(newmsg).bind('sent', function (e, value) {
                                if (value._status == "error") {
                                    newmsg.MarkAsError();
                                    self.destroyBox();
                                } else {
                                    self.start_TextChat();
                                }
                            });
                        });
                    } else {
                        self.destroyBox();
                    }
                    vizzop.CommRequest_InCourse = null;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    vizzop.CommRequest_InCourse = null;
                }
            });
        } catch (err) {
            vizzop.CommRequest_InCourse = null;
            vizzoplib.log("Error RequestStartChat: " + err);
        }
    },
    fillBox_Login: function () {
        var self = this;
        try {
            self._preferredubication = 'center';
            self._preferredwidth = '280px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_Login';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jVizzop('<div></div>')
                    .addClass('zs_boxinfocontents')
                    .appendTo(self._boxinfo);

            var title_text = jVizzop('<h4></h4>')
                    .text(LLang('login_title', null))
                    .appendTo(msgbox);

            msgbox.append(jVizzop('<br/>'));

            self.alert = jVizzop('<div></div>')
                    .addClass('alert')
                    .appendTo(msgbox)
                    .hide();

            var form = jVizzop('<form/>')
                    .addClass('form-stacked')
                    .css({ "margin": "0 auto", "width": "250px" })
                    .appendTo(msgbox);

            var fieldset = jVizzop('<fieldset/>')
                    .appendTo(form);

            var divclear1 = jVizzop('<div/>')
                    .addClass('clearfix')
                    .appendTo(fieldset);
            var divlabel1 = jVizzop('<label/>')
                    .attr('for', 'user')
                    .text(LLang('user', null))
                    .appendTo(divclear1);
            var divinput1 = jVizzop('<div/>')
                    .addClass('input-append')
                    .appendTo(divclear1);
            self.inputUser = jVizzop('<input/>')
                    .attr('id', 'user')
                    .attr('name', 'user')
                    .attr('type', 'text')
                    .attr('placeholder', LLang('write_user', null))
                    .addClass('input-small hint')
                    .focus(function (event) {
                        var target = jVizzop(event.target);
                        if (target.hasClass('hint') == true) {
                            target.val('');
                            target.removeClass('hint');
                        }
                    })
                    .keypress(function (e) {
                        try {
                            if (e.keyCode == 13) {
                                e.preventDefault();
                                if (jVizzop(self.inputUser).val() != "") {
                                    jVizzop(self.inputPassword).focus();
                                    jVizzop(self.inputPassword).select();
                                }
                            }
                        } catch (err) {
                            vizzoplib.log(err);
                        }
                    })
                    .appendTo(divinput1);

            var divclear2 = jVizzop('<div/>')
                    .addClass('clearfix')
                    .appendTo(fieldset);
            var divlabel2 = jVizzop('<label/>')
                    .attr('for', 'password')
                    .text(LLang('password', null))
                    .appendTo(divclear2);
            var divinput2 = jVizzop('<div/>')
                    .addClass('input')
                    .appendTo(divclear2);
            self.inputPassword = jVizzop('<input/>')
                    .attr('id', 'password')
                    .attr('name', 'password')
                    .attr('type', 'password')
                    .attr('placeholder', LLang('write_password', null))
                    .addClass('input-medium hint')
                    .focus(function (event) {
                        var target = jVizzop(event.target);
                        if (target.hasClass('hint') == true) {
                            target.val('');
                            target.removeClass('hint');
                        }
                    })
                    .keypress(function (e) {
                        try {
                            if (e.keyCode == 13) {
                                e.preventDefault();
                                if ((jVizzop(self.inputUser).val() != "") && (jVizzop(self.inputPassword).val() != "")) {
                                    //vizzop.me.FullName = jVizzop(inputHelp).val();
                                    jVizzop(self.inputUser)
                                        .addClass('disabled')
                                        .attr('disabled', 'disabled');
                                    jVizzop(self.inputPassword)
                                        .addClass('disabled')
                                        .attr('disabled', 'disabled');
                                    jVizzop(self.buttoninputLogin)
                                        .addClass('disabled')
                                        .hide();
                                    jVizzop(self.loading).show();
                                    jVizzop(self.alert).hide();
                                    self.checkLogin();
                                }
                            }
                        } catch (err) {
                            vizzoplib.log(err);
                        }
                    })
                    .appendTo(divinput2);

            var divclear3 = jVizzop('<div/>')
                    .addClass('clearfix')
                    .appendTo(fieldset);
            var divlabel3 = jVizzop('<label/>')
                    .attr('for', 'login')
                    .text('')
                    .appendTo(divclear3);
            var divinput3 = jVizzop('<div/>')
                    .addClass('input')
                    .appendTo(divclear3);
            self.buttoninputLogin = jVizzop('<button></button>')
                    .attr('id', 'login')
                    .attr('name', 'login')
                    .text(LLang('login', null))
                    .addClass('vizzop-btn vizzop-btn-primary')
                    .click(function (event) {
                        try {
                            if ((jVizzop(self.inputUser).val() != "") && (jVizzop(self.inputPassword).val() != "")) {
                                //vizzop.me.FullName = jVizzop(inputHelp).val();
                                jVizzop(self.inputUser)
                                    .addClass('disabled')
                                    .attr('disabled', 'disabled');
                                jVizzop(self.inputPassword)
                                    .addClass('disabled')
                                    .attr('disabled', 'disabled');
                                jVizzop(self.buttoninputLogin)
                                    .addClass('disabled')
                                    .hide();
                                jVizzop(self.loading).show();
                                jVizzop(self.alert).hide();
                                self.checkLogin();
                            }
                        } catch (err) {
                            vizzoplib.log(err);
                        }
                        return false;
                    })
                    .appendTo(divinput3);

            self.loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                    .css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
                    .appendTo(divinput3)
                    .hide();

            self._closebutton.hide();

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
        } catch (err) {
            vizzoplib.log("fillBox_Login " + err);
        }
    }
},
{
    // properties
    getset: [
        ['Box', '_box']
    ]
});
