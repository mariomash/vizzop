
var MessageBox = jQuery.zs_Class.create(Box, {
    // constructor
    initialize: function () {
        var self = this;
        self.base('initialize');
        self._type = 'MessageBox';
        self._interlocutor = null;
        /*
        self._box._closebutton.click(function (event) {
        self.confirm_endsession();
        return false;
        });
        */
        self.Messages = [];
        self.unread_elements = new Number(0);
    },
    // methods
    process_alerts: function () {
        var self = this;
        try {
            jQuery(self._unread_elements_div).remove();
            if (self.unread_elements > 0) {
                self._unread_elements_div = jQuery('<span></span>')
					.addClass('label label-warning')
					.text(self.unread_elements)
					.insertBefore(jQuery('#linkTo_' + self._id).find('i')[0]);
            }
        } catch (err) {
            log(err);
        }
    },
    askforscreenshare: function () {
        var self = this;
        self._boxtitletext.attr({
            'rel': 'popover',
            'title': 'Information',
            'data-content': '<div>' + LLang('share_question', [self._interlocutor.FullName]) + '</div><div class=actions id=popover_action><button class="btn btn-primary" id=popover_action_ok>' + LLang('ok', null) + '</button><button class="btn" id=popover_action_no>' + LLang('no', null) + '</button></div>'
        });
        self._boxtitletext.popover({
            placement: 'bottom',
            html: 'true',
            trigger: 'manual'
        });
        jQuery(self._boxtitletext).popover('show');
        jQuery('#popover_action_ok').click(function (event) {
            self.start_Screenshare();
            jQuery(self._boxtitletext).popover('hide');
            //self.send_current_html();
            return false;
        });
        $('#popover_action_no').click(function (event) {
            var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_cancelscreenshare', null, self, self._commsessionid);
            zentools.MsgCue.push(newmsg);
            jQuery(self._boxtitletext).popover('hide');
            return false;
        });
    },
    cancel_Screenshare: function () {
        try {
            var self = this;
            if (jQuery(self._box).hasClass('screenshare') === true) {
                self._status = 'cancel_Screenshare';
                self._preferredwidth = 'auto';
                self._preferredheight = 'auto';

                self._preferredubication = 'bottomright';
                jQuery(self._interlocutor_iframe).remove();

                self.button_CancelScreenSharing.hide();
                self.button_AddScreenSharing
                    .css({ display: 'inline-block' });

                jQuery(self._box).removeClass('screenshare');
                self.positionBox(null, 'fast');
            }
        } catch (err) { log(err); }
    },
    start_Screenshare: function () {
        try {
            var self = this;
            if (jQuery(self._box).hasClass('screenshare') === false) {
                self._status = 'start_Screenshare';
                self._preferredwidth = 'auto';
                self._preferredheight = 'auto';
                //self._preferredubication = 'center';
                self._preferredubication = 'topleft';

                /*
                self._col0 = jQuery('<span></span>')
                .addClass('col0')
                .insertAfter(self._col1);
                */
                /*
                .css({ display: 'inline-block' })
                */
                /*
                self._boxscreenshare = jQuery('<div></div>')
                .addClass('boxscreenshare')
                .appendTo(self._col0);
                */
                /*
                if ((self._interlocutor_iframe_width === null) || (self._interlocutor_iframe_height === null)) {
                self._interlocutor_iframe_width = 722;
                self._interlocutor_iframe_height = 517;
                }
                */
                self._interlocutor_iframe = jQuery('<iframe />')
						.attr('name', 'interlocutor_iframe')
						.attr('id', 'interlocutor_iframe')
						.attr('frameborder', '0')
						.addClass('interlocutor_iframe')
						.addClass('col0')
						.insertAfter(self._col1);

                /*
                .attr('scrolling', 'no')
                .attr('width', self._interlocutor_iframe_width)
                .attr('height', self._interlocutor_iframe_height)
                */

                var d = self._interlocutor_iframe[0].contentWindow.document; // contentWindow works in IE7 and FF
                d.open(); d.close(); // must open and close document object to start using it!

                //self._interlocutor.UserName
                jQuery("document", d).html('<head></head><body></body>');
                jQuery("html", d)
					.css({ 'background-color': '#eeeeee', 'background-image': 'none' });
                jQuery("body", d)
					.css({ 'background-color': '#eeeeee', 'background-image': 'none', 'text-align': 'center' })
					.append(jQuery('<h3></h3>').text(LLang('waiting_screenview', null)).css({ 'margin': '180px auto 0px' }))
					.append(jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>').css({ 'margin': '30px 5px', 'vertical-align': 'middle' }));

                jQuery("head", d)
					.append(jQuery('<link/>').attr('rel', 'stylesheet').attr('type', 'text/css').attr('href', zentools.mainURL + '/Content/Site.css'))
					.append(jQuery('<link/>').attr('rel', 'stylesheet').attr('type', 'text/css').attr('href', zentools.mainURL + '/Scripts/bootstrap/css/bootstrap.css'))
					.append(jQuery('<link/>').attr('rel', 'stylesheet').attr('type', 'text/css').attr('href', zentools.mainURL + '/ZenTools/css/zen-site.css'));

                self._info_iframe = jQuery('<span></span>')
					.addClass('infoscreenshare')
					.text(LLang('shareframe_description', null))
					.insertAfter(self._col1);

                self.button_AddScreenSharing.hide();
                self.button_CancelScreenSharing.show();
                self.loadScreen();

                /*
                var url = zentools.mainURL + '/ZenTools/RemoteView.ashx?Agent_UserName=' + zentools.me.UserName + '&Agent_Password=' + zentools.me.Password + '&UserName=' + self._interlocutor.UserName;

                self._interlocutor_iframe.attr('src', url);

                jQuery(self._interlocutor_iframe).load(function () {
                jQuery(self._interlocutor_iframe).contents().find('*').each(function (idx, val) {
                if (jQuery(val).attr('zenus-trigger')) {
                } else {
                jQuery(val).click(function (event) {
                var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_triggerclick', jQuery(event.target).attr('zenus-id'));
                zentools.MsgCue.push(newmsg);
                return false;
                });
                jQuery(val).attr('zenus-trigger', true);
                }
                });
                });
                */

                jQuery(self._box).addClass('screenshare');
                self.positionBox(null, 'fast');
            }
        } catch (err) { log(err); }
    },
    loadScreen: function () {
        var self = this;
        try {
            //Primero miramos las sesiones por aprobar..
            if (self._lastScreenID === null) {
                self._lastScreenID = 0;
            }
            if (self._lastScreenMD5 === null) {
                self._lastScreenMD5 = "";
            }
            var msg = {
                'Agent_UserName': zentools.me.UserName,
                'Agent_Password': zentools.me.Password,
                'Agent_Domain': zentools.me.Business.Domain,
                'UserName': self._interlocutor.UserName,
                'Domain': self._interlocutor.Business.Domain,
                'ID': self._lastScreenID,
                'MD5': self._lastScreenMD5
            };
            var post = jQuery.ajax({
                url: zentools.mainURL + "/RealTime/GetScreen",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (self._interlocutor_iframe[0].contentWindow === null) {
                        return false;
                    }
                    if (data !== null) {
                        if (data !== false) {
                            switch (data.CaptureType) {
                                case 'key':
                                    var d = self._interlocutor_iframe[0].contentWindow.document; // contentWindow works in IE7 and FF
                                    //d.open(); d.close(); // must open and close document object to start using it!
                                    //jQuery("document", d).html(data.data);
                                    d.open();
                                    d.write(data.Data);
                                    d.close();
                                    break;
                                case 'change':
                                    var arr_data = data.Data.split(":");
                                    var characts = "'[zen-id=\"" + arr_data[0] + "\"]'";
                                    var new_contents = LZW.decompress(arr_data[1].split("_"));
                                    if ((new_contents !== "null") && (new_contents !== null)) {
                                        var elem = jQuery(characts);
                                        log(elem + ":::" + elem.html() + " -> " + new_contents);
                                        elem.html(new_contents);
                                    }
                                    break;
                            }
                            /*
                            self._interlocutor_iframe
                            .attr('width', data.Width)
                            .attr('height', data.Height);
                            */
                            self._lastScreenID = data.ID;
                            self._lastScreenMD5 = data.MD5;
                        }
                    }
                    self.loadScreen();
                },
                error: function (jqXHR, textStatus) {
                    log(jqXHR);
                    log("Error GetScreen: " + textStatus);
                    self.loadScreen();
                }
            });
        } catch (err) {
            log(err);
            self.loadScreen();
        }
    },
    askforvideo: function () {
        var self = this;
        self._boxtitletext.attr({
            'rel': 'popover',
            'title': 'Information',
            'data-content': '<div>' + LLang('video_question', [self._interlocutor.FullName]) + '</div><div class=actions id=popover_action><button class="btn btn-primary" id=popover_action_ok>' + LLang('ok', null) + '</button><button class="btn" id=popover_action_no>' + LLang('no', null) + '</button></div>'
        });
        self._boxtitletext.popover({
            placement: 'bottom',
            html: 'true',
            trigger: 'manual'
        });
        jQuery(self._boxtitletext).popover('show');
        jQuery('#popover_action_ok').click(function (event) {
            self.start_VideoChat();
            jQuery(self._boxtitletext).popover('hide');
            return false;
        });
        jQuery('#popover_action_no').click(function (event) {
            self.cancel_VideoChat();
            jQuery(self._boxtitletext).popover('hide');
            return false;
        });
    },
    start_VideoChat: function () {
        var self = this;
        try {
            if (jQuery(self._box).hasClass('videochat') === false) {
                self._status = 'start_VideoChat';
                //self._preferredwidth = '260px';
                self._preferredwidth = 'auto';
                self._preferredheight = 'auto';
                if (jQuery(self._box).hasClass('screenshare') === false) {
                    self._preferredubication = 'bottomright';
                }
                self.hideBox();

                self._boxvideochat.empty();
                //loading video
                self._boxvideochat._videoZone = jQuery('<span></span>')
						.addClass('chat_videoZone')
						.appendTo(self._boxvideochat);

                self.button_AddVideo.hide();
                self.button_CancelVideo.css({ display: 'inline-block' });
                self._boxvideochat.show();

                self._loadingimg = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
						.css({ 'margin-top': '60px' })
						.css({ 'margin-bottom': '10px' })
						.appendTo(self._boxvideochat._videoZone);
                self._loadingvideotext = jQuery('<div></div>')
					.html(LLang('loading_video', null))
					.appendTo(self._boxvideochat._videoZone);

                try {
                    jQuery(self._boxtextchat.msgZone).scrollTop(jQuery(self._boxtextchat.msgZone).outerScrollHeight());
                    //jQuery(box._boxtextchat.msgZone).attr({ scrollTop: jQuery(box._boxtextchat.msgZone).attr("scrollHeight") });
                } catch (err) {
                    log(err);
                }

                if ((self._OpenTokSessionID === null) || (self._OpenTokToken === null)) {
                    /* Si no tenemos el sessionID lo pedimos.. y si se le pasa null trae los dos, y si se le pasa un ID adecuado trae solo un TOKEN que valga ;) */
                    try {
                        var msg = {
                            'UserName': zentools.me.UserName,
                            'Password': zentools.me.Password,
                            'Domain': zentools.me.Business.Domain,
                            'commsessionid': self._commsessionid
                        };
                        var ajaxrequest = jQuery.ajax({
                            url: zentools.mainURL + "/Comm/GetOpenTokSessionID",
                            type: "POST",
                            data: msg,
                            dataType: "jsonp",
                            beforeSend: function (xhr) {
                            },
                            success: function (data) {
                                if (data === false) {
                                    self.cancel_VideoChat(true);
                                    return;
                                }
                                self._OpenTokSessionID = data.OpenTokSessionID;
                                self._OpenTokToken = data.OpenTokToken;
                                self.start_VideoChatOpenTok();
                            },
                            error: function (jqXHR, textStatus) {
                                log("Error start_VideoChat: " + textStatus);
                                self.cancel_VideoChat(true);
                                self.button_AddVideo.hide();
                                self.positionBox(null, 'fast');
                                self.cancel_VideoChat();
                            }
                        });
                    } catch (err) {
                        log(err);
                        self._boxvideochat.hide();
                        self.button_AddVideo.hide();
                        self.positionBox(null, 'fast');
                        self.cancel_VideoChat();
                    }
                } else {
                    self.start_VideoChatOpenTok();
                }

                jQuery(self._box).addClass('videochat');
                self.positionBox(null, 'fast');
            }
        } catch (err) {
            log(err);
            self.cancel_VideoChat();
            self.button_AddVideo.hide();
            self.positionBox(null, 'fast');
        }
    },
    addStream: function (stream) {
        var self = this;
        try {
            // Check if this is the stream that I am publishing, and if so do not publish.
            if (stream.connection.connectionId === self._OpenTokSession.connection.connectionId) {
                return;
            }
            //self._boxvideochat._videoZone.empty();
            self._boxvideochat.videoChat_interlocutor_Placemark = jQuery('<span></span>')
												.addClass('videochat_interlocutor')
												.appendTo(self._boxvideochat._videoZone);
            self._boxvideochat.videoChat_interlocutor = jQuery('<div></div>')
												.attr('id', 'videoChat_interlocutor')
												.appendTo(self._boxvideochat.videoChat_interlocutor_Placemark);
            self._OpenTokSubscribers[stream.streamId] = self._OpenTokSession.subscribe(stream, 'videoChat_interlocutor');
        } catch (err) {
            log(err);
        }
    },
    // Called when user wants to start publishing to the session
    startPublishing: function () {
        try {
            var self = this;
            if (!self._OpenTokPublisher) {
                self._boxvideochat.videoChat_me_Placemark = jQuery('<span></span>')
										.addClass('videochat_beforeallow')
										.appendTo(self._boxvideochat._videoZone);
                self._boxvideochat.videoChat_me = jQuery('<div></div>')
										.attr('id', 'videoChat_me')
										.appendTo(self._boxvideochat.videoChat_me_Placemark);
                self._OpenTokPublisher = self._OpenTokSession.publish('videoChat_me'); // Pass the replacement div id to the publish method
                self._OpenTokPublisher.addEventListener('accessAllowed', function (event) {
                    try {
                        self._boxvideochat.videoChat_me_Placemark.removeClass('videochat_beforeallow');
                        self._boxvideochat.videoChat_me_Placemark.addClass('videochat_me');
                        //log(zentools.opentok_videostreams);
                        //try { self._boxvideochat.videoChat_interlocutor_Placemark.remove(); } catch (err) { }
                        // Subscribe to the newly created streams
                        //self._OpenTok
                        /*
                        for (var i = 0; i < event.streams.length; i++) {
                        self.addStream(self._OpenTokStreams[i]);
                        }
                        */
                        var _el = jQuery('#' + self._OpenTokPublisher.id);
                        _el.attr({
                            'width': '120',
                            'height': '90'
                        });
                        var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_ask4video', self._commsessionid, self, self._commsessionid);
                        zentools.MsgCue.push(newmsg);
                        var newmsg_ = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_ask4video', self._commsessionid, self, self._commsessionid);
                        zentools.MsgCue.push(newmsg_);
                    } catch (err) {
                        log(err);
                        self.cancel_VideoChat();
                    }
                });
            }
        } catch (err) {
            log(err);
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
                log("Exception: " + event.code + "::" + event.message);
                self.cancel_VideoChat();
            });
            if (TB.checkSystemRequirements() !== TB.HAS_REQUIREMENTS) {
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
                self._OpenTokSession.connect(zentools.opentok_apiKey, self._OpenTokToken);
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

            self.positionBox(null, 'fast');
        } catch (err) {
            log(err);
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
            jQuery(self._box).removeClass('videochat');
            self._boxvideochat.empty();
            self.button_CancelVideo.hide();
            self.button_AddVideo.css({ display: 'inline-block' });
            try { self._handle.hide(); } catch (err) { }
            self._boxinner.show();
            self.positionBox(null, 0);
            if (dont_send_msg === null) {
                var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_cancelvideo', self._commsessionid, self, self._commsessionid);
                zentools.MsgCue.push(newmsg);
            }
        } catch (err) {
            log("start_TextChat: " + err);
        }
    },
    start_TextChat: function () {
        try {
            var self = this;
            if (jQuery(self._box).hasClass('chat') === false) {
                self._preferredubication = 'bottomright';
                self._preferredwidth = '260px';
                self._preferredheight = 'auto';
                self._status = 'start_TextChat';
                try { jQuery(self._handle).remove(); } catch (err) { }
                self._boxinner.show();
                self.hideBox();
                self._boxinfo.empty();

                self._nolist = false;

                jQuery(self._boxtitletext).html('<i class="icon-user"></i>&nbsp;<span style="display: inline-block; vertical-align: middle">' + LLang('chat_with', [self._interlocutor.FullName]) + "</span>");
                self._title = LLang('chat_with', [self._interlocutor.FullName]);

                //status text
                //alert(self._statustext);
                jQuery(self._boxstatus).html(self._statustext);


                //Header Box
                self._boxheader = jQuery('<div></div>')
						.addClass('boxheader')
						.appendTo(self._boxinfo);

                self._col1 = jQuery('<span></span>')
						.addClass('col1')
						.appendTo(self._boxinfo);

                /*
						
                .css({ display: 'inline-block' })
                */

                self._boxvideochat = jQuery('<div></div>')
						.addClass('boxvideochat')
						.appendTo(self._col1)
						.hide();

                self._boxtextchat = jQuery('<div></div>')
						.addClass('boxtextchat')
						.appendTo(self._col1);

                //Añadimos botones si soporta Video Chat
                var playerVersion = swfobject.getFlashPlayerVersion();
                if (playerVersion.major >= 10) {
                    self.button_AddVideo = jQuery('<button></button>')
							.html('<i class="icon-facetime-video"></i>&nbsp;' + LLang('add_video', null)) /*LLang('add_video', null)*/
							.addClass('btn small')
							.css({ display: 'inline-block' })
							.click(function (event) {
							    self.start_VideoChat();
							    return false;
							})
							.appendTo(self._boxheader);
                    self.button_CancelVideo = jQuery('<button></button>')
							.text(LLang('cancel_video', null))
							.addClass('btn small')
							.click(function (event) {
							    self.cancel_VideoChat();
							    return false;
							})
							.hide()
							.appendTo(self._boxheader);
                }


                //Añadimos resto de botones
                if (zentools.mode === 'agent') {
                    self.button_AddScreenSharing = jQuery('<button></button>')
						.html('<i class="icon-eye-open"></i>&nbsp;' + LLang('screen_view', null))
						.addClass('btn small')
						.css({ display: 'inline-block' })
						.click(function (event) {
						    self.start_Screenshare();
						    /*
						    var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_ask4screenshare', null, self, self._commsessionid);
						    zentools.MsgCue.push(newmsg);
						    */
						    return false;
						})
						.appendTo(self._boxheader);

                    self.button_CancelScreenSharing = jQuery('<button></button>')
						.text(LLang('cancel_screen_share', null))
						.addClass('btn small')
						.css({ display: 'inline-block' })
						.click(function (event) {
						    self.cancel_Screenshare();
						    return false;
						})
						.hide()
						.appendTo(self._boxheader);
                }

                self._boxtextchat.msgZone = jQuery('<span></span>')
						.addClass('chat_msgZone')
						.appendTo(self._boxtextchat);

                self._boxtextchat.infoZone = jQuery('<span></span>')
						.addClass('chat_infoZone')
						.appendTo(self._boxtextchat);

                /*
                self._boxtextchat.separator = jQuery('<hr/>')
                .addClass('hr')
                .appendTo(self._boxtextchat);
                */

                self._boxtextchat.inputZone = jQuery('<span></span>')
						.addClass('chat_inputZone')
						.appendTo(self._boxtextchat);

                self._boxtextchat.inputTextArea = jQuery('<textarea/>')
						.attr('id', 'zs_message')
						.attr('name', 'zs_message')
						.addClass('chat_inputTextArea hint')
						.focus(function (event) {
						    var target = jQuery(event.target);
						    if (target.hasClass('hint') === true) {
						        target.val('');
						        target.removeClass('hint');
						    } else {
						        target.select();
						    }
						})
						.focusin(function () {
						    var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_inputfocus_in', self._commsessionid, self, self._commsessionid);
						    newmsg._from_fullname = zentools.me.FullName;
						    new_Cue = [];
						    jQuery.each(zentools.MsgCue, function (i, v) {
						        if ((v._subject !== '$#_inputfocus_in') && (v._subject !== '$#_inputfocus_out')) {
						            new_Cue.push(v);
						        }
						    });
						    zentools.MsgCue = new_Cue;
						    zentools.MsgCue.push(newmsg);
						})
						.focusout(function () {
						    var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_inputfocus_out', self._commsessionid, self, self._commsessionid);
						    newmsg._from_fullname = zentools.me.FullName;
						    new_Cue = [];
						    jQuery.each(zentools.MsgCue, function (i, v) {
						        if ((v._subject !== '$#_inputfocus_in') && (v._subject !== '$#_inputfocus_out')) {
						            new_Cue.push(v);
						        }
						    });
						    zentools.MsgCue = new_Cue;
						    zentools.MsgCue.push(newmsg);
						})
						.appendTo(self._boxtextchat.inputZone);

                self._boxtextchat.inputTextArea.focus();

                self._boxtextchat.inputTextArea.keypress(function (e) {
                    if (e.keyCode === 13) {
                        e.preventDefault();
                        if (jQuery.trim(jQuery(this).val()) !== '') {
                            if (self._interlocutor.UserName !== null) {
                                var msg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, null, jQuery(this).val(), self, self._commsessionid);
                                msg._from_username = zentools.me.UserName;
                                msg._from_fullname = zentools.me.FullName;
                                msg._to_username = self._interlocutor.UserName;
                                //log(msg);
                                jQuery(msg).bind('sent', function (e, value) {
                                    if (value._status === "error") {
                                        msg.MarkAsError();
                                    } else {
                                        msg.MarkAsOk();
                                    }
                                });
                                zentools.MsgCue.push(msg);
                                zentools.Daemon.sendNewMessages();
                                msg.AddMsgToChat(msg);
                                jQuery(this).val(null);
                            }
                        }
                        if (jQuery.trim(jQuery(this).val()) === '') {
                            jQuery(this).val(null);
                        }
                    }
                });

                jQuery.each(self.Messages, function (i, v) {
                    v.AddMsgToChat(v);
                    return;
                });

                self._closebutton
				.show()
				.click(function (event) {
				    if (typeof self._commsessionid === "undefined") {
				        self.closeSession();
				    } else {
				        self.askforendsession();
				    }
				    return false;
				});

                self.positionBox(null, 'fast');

                jQuery(self._box).addClass('chat');
            }
        } catch (err) {
            log("start_TextChat: " + err);
        }
    },
    ConverserUpdateName: function (FullName) {
        var self = this;
        try {
            var data_to_send = {
                'UserName': zentools.me.UserName,
                'FullName': FullName,
                'Domain': zentools.me.Business.Domain,
                'Password': zentools.me.Password
            };
            var request = jQuery.ajax({
                url: zentools.mainURL + "/Converser/ChangeName",
                type: "POST",
                data: data_to_send,
                dataType: "jsonp",
                success: function (data) {
                    if (data === null) {
                        jQuery(self).trigger("ConverserUpdateNameFinished", null);
                        return;
                    } else {
                        if (data === false) {
                            jQuery(self).trigger("ConverserUpdateNameFinished", null);
                            return;
                        } else {
                            jQuery(self).trigger("ConverserUpdateNameFinished", data);
                            return;
                        }
                    }
                },
                error: function (jqXHR, textStatus) {
                    log("Error ConverserUpdateName: " + textStatus);
                    jQuery(self).trigger("ConverserUpdateNameFinished", null);
                    return;
                }
            });
        } catch (err) {
            log("ConverserUpdateName " + err);
        }
    },
    inform_notavailable: function () {
        var self = this;
        try {
            if (jQuery(self._box).hasClass('chat') === false) {
                return false;
            }
            if (typeof self._boxtextchat !== "undefined") {
                jQuery(self._boxtextchat.inputTextArea)
					.addClass('disabled')
					.attr('disabled', 'disabled');
                jQuery(self.button_CancelVideo.hide())
					.addClass('disabled')
					.attr('disabled', 'disabled');
                jQuery(self.button_AddVideo.show())
					.addClass('disabled')
					.attr('disabled', 'disabled');
                jQuery(self._boxtextchat.infoZone)
					.text(LLang('interlocutor_not_available', [self._interlocutor.FullName]))
					.addClass('warning');
                jQuery(self.button_AddScreenSharing)
					.addClass('disabled')
					.attr('disabled', 'disabled');
                jQuery(self.button_CancelScreenSharing)
					.addClass('disabled')
					.attr('disabled', 'disabled');
                jQuery(self._boxtextchat.msgZone).focus();
            }
        } catch (err) {
            log("inform_notavailable " + err);
        }
    },
    inform_available: function () {
        var self = this;
        try {
            if (jQuery(self._box).hasClass('chat') === false) {
                return false;
            }
            if (typeof self._boxtextchat !== "undefined") {
                if (jQuery(self._boxtextchat.inputTextArea).hasClass('disabled') === true) {
                    jQuery(self._boxtextchat.inputTextArea)
                        .removeClass('disabled')
                        .removeAttr('disabled');
                    jQuery(self.button_CancelVideo.hide())
                        .removeClass('disabled')
                        .removeAttr('disabled');
                    jQuery(self.button_AddVideo.show())
                        .removeClass('disabled')
                        .removeAttr('disabled');
                    jQuery(self._boxtextchat.infoZone)
                        .text("")
                        .removeClass('warning');
                    jQuery(self.button_AddScreenSharing)
                        .removeClass('disabled')
                        .removeAttr('disabled');
                    jQuery(self.button_CancelScreenSharing)
                        .removeClass('disabled')
                        .removeAttr('disabled');
                    jQuery(self._boxtextchat.inputTextArea)
                        .focus();
                }
            }
        } catch (err) {
            log("inform_available " + err);
        }
    },
    closeSession: function () {
        alert("cerrando");
        var self = this;
        try {
            if (self._OpenTokSession !== null) {
                self.cancel_VideoChat();
            }
            if (zentools.mode === 'client') {
                self._commsessionid = null;
                zentools.Daemon._commsessionid = null;
                zentools.CommRequest_InCourse = null;

                var name_mecookie = zentools.ApiKey + "_me";
                var name_comsessionidcookie = zentools.ApiKey + "_commsessionid";
                deleteCookie(name_comsessionidcookie);
                deleteCookie(name_mecookie);
                self.destroyBox();
                zentools.Daemon.clientmessagebox = new ClientMessageBox();
                zentools.Daemon.clientmessagebox.fillBox_helpStandby();
            } else {
                self.destroyBox();
            }
            /*
            if (typeof self._boxtextchat !== "undefined") {
            jQuery(self._boxtextchat.inputTextArea)
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jQuery(self.button_CancelVideo.hide())
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jQuery(self.button_AddVideo.show())
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jQuery(self._boxtextchat.infoZone)
            .text(LLang('interlocutor_closed_chat', [self._interlocutor.FullName]))
            .addClass('warning');
            jQuery(self.button_AddScreenSharing)
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jQuery(self.button_CancelScreenSharing)
            .addClass('disabled')
            .attr('disabled', 'disabled');
            jQuery(self._boxtextchat.msgZone).focus();
            }
            self.destroyBox();
            */
        } catch (_err) {
            log("closeSession: " + _err);
        }
    },
    doFinalizeCommRequest: function () {
        var self = this;
        try {
            var data_to_send = {
                'commsessionid': self._commsessionid,
                'username': zentools.me.UserName,
                'password': zentools.me.Password,
                'domain': zentools.me.Business.Domain
            };
            var request = jQuery.ajax({
                url: zentools.mainURL + "/Comm/FinalizeCommRequest",
                type: "POST",
                data: data_to_send,
                dataType: "jsonp",
                success: function (data) {
                    //alert(data);
                    //log(self._status);
                    if (data === false) {
                        jQuery(self.loading).hide();
                        jQuery(self.buttonyes)
							.removeClass('disabled')
							.show();
                        jQuery(self.buttonno)
							.removeClass('disabled')
							.show();
                    }
                },
                error: function (jqXHR, textStatus) {
                    log("Error doFinalizeCommRequest: " + textStatus);
                    jQuery(self.loading).hide();
                    jQuery(self.buttonyes)
								.removeClass('disabled')
								.show();
                    jQuery(self.buttonno)
								.removeClass('disabled')
								.show();
                }
            });
        } catch (err) {
            log("doFinalizeCommRequest " + err);
            jQuery(self.loading).hide();
            jQuery(self.buttonyes)
				.removeClass('disabled')
				.show();
            jQuery(self.buttonno)
				.removeClass('disabled')
				.show();
        }
    },
    checkLogin: function () {
        var self = this;
        try {
            var data = {
                'username': jQuery(self.inputUser).val(),
                'password': jQuery(self.inputPassword).val()
            };
            var request = jQuery.ajax({
                url: zentools.mainURL + "/Converser/checkLogin",
                type: "POST",
                data: data,
                dataType: "jsonp",
                success: function (data) {
                    if (data !== null) {
                        if (data !== false) {
                            zentools.me = data;
                            setCookie("me", jQuery.toJSON(zentools.me), 1);
                            jQuery(self._box).remove();
                            self.destroyBox();
                        } else {
                            jQuery(self.inputUser)
								.removeClass('disabled')
								.removeAttr("disabled");
                            jQuery(self.inputPassword)
								.removeClass('disabled')
								.removeAttr("disabled");
                            jQuery(self.buttoninputLogin)
								.removeClass('disabled')
								.show();
                            jQuery(self.loading).hide();
                            jQuery(self.alert)
								.html(LLang('error_login', null))
								.fadeIn();
                        }
                    }
                },
                error: function (jqXHR, textStatus) {
                    log("Error checkLogin: " + textStatus);
                    jQuery(self.inputUser)
					.removeClass('disabled')
					.removeAttr("disabled");
                    jQuery(self.inputPassword)
					.removeClass('disabled')
					.removeAttr("disabled");
                    jQuery(self.buttoninputLogin)
					.removeClass('disabled')
					.show();
                    jQuery(self.loading).hide();
                    jQuery(self.alert)
					.html(LLang('error_login', null))
					.fadeIn();
                    return false;
                }
            });
        } catch (err) {
            log("checkLogin " + err);
        }
    },
    askforendsession: function () {
        var self = this;
        try {
            self._boxtitletext.attr({
                'rel': 'popover',
                'title': 'Information',
                'data-content': '<div>' + LLang('end_session', null) + '</div><div class=actions id=popover_action><button class="btn btn-primary" id=popover_action_ok>' + LLang('ok', null) + '</button><button class="btn" id=popover_action_no>' + LLang('no', null) + '</button></div>'
            });
            self._boxtitletext.popover({
                placement: 'bottom',
                html: 'true',
                trigger: 'manual'
            });
            jQuery(self._boxtitletext).popover('show');
            jQuery('#popover_action_ok').click(function (event) {
                self.doFinalizeCommRequest();
                jQuery(self._boxtitletext).popover('hide');
                self._boxtitletext.attr({
                    'rel': 'popover',
                    'title': 'Information',
                    'data-content': '<div>' + LLang('wait_end_session', null) + '</div><div class=actions id=popover_action><img style="margin: 8px 5px; vertical-align: middle; text-align: center;" src="' + zentools.mainURL + '/Content/images/loading.gif"/></div>'
                });
                self._boxtitletext.popover({
                    placement: 'bottom',
                    html: 'true',
                    trigger: 'manual'
                });
                jQuery(self._boxtitletext).popover('show');
                return false;
            });
            jQuery('#popover_action_no').click(function (event) {
                jQuery(self._boxtitletext).popover('hide');
                return false;
            });
        } catch (err) {
            log("askforendsession " + err);
        }
    },
    fillBox_RequestStartChat: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_RequestStartChat';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jQuery('<div></div>')
				.addClass('boxinfocontents')
				.appendTo(self._boxinfo);

            msgbox.html(null);

            var text = jQuery('<h5></h5>')
				.text(LLang('waiting_startchat', [zentools.me.FullName]))
				.appendTo(msgbox);

            var loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
				.css({ 'margin': '8px 5px' })
				.appendTo(msgbox);

            self.positionBox(null, 'fast');

            self.RequestStartChat();

        } catch (err) {
            log("fillBox_RequestStartChat " + err);
        }
    },
    fillBox_RequestScreenView: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_RequestScreenView';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jQuery('<div></div>')
				.addClass('boxinfocontents')
				.appendTo(self._boxinfo);

            msgbox.html(null);

            var text = jQuery('<h5></h5>')
				.text(LLang('waiting_screenview', [zentools.me.FullName]))
				.appendTo(msgbox);

            var loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
				.css({ 'margin': '8px 5px' })
				.appendTo(msgbox);

            self.positionBox(null, 'fast');

            self.RequestScreenView();

        } catch (err) {
            log("fillBox_RequestScreenView " + err);
        }
    },
    fillBox_CancelChat: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_CancelChat';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jQuery('<div></div>')
				.addClass('boxinfocontents')
				.appendTo(self._boxinfo);

            msgbox.html(null);

            var text = jQuery('<h5></h5>')
				.text(LLang('chat_cancelled', [zentools.me.FullName]))
				.appendTo(msgbox);

            self.positionBox(null, 'fast');

        } catch (err) {
            log("fillBox_CancelChat " + err);
        }
    },
    RequestScreenView: function () {
        var self = this;
        try {
            var msg = {
                'apikey': zentools.ApiKey,
                'agent_username': zentools.me.UserName,
                'agent_password': zentools.me.Password,
                'agent_domain': zentools.me.Business.Domain,
                'username': self._interlocutor.UserName,
                'domain': self._interlocutor.Business.Domain,
                'password': self._interlocutor.Password
            };
            zentools.CommRequest_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Comm/Requestcommsessionid",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                success: function (data) {
                    if (data !== false) {
                        self._commsessionid = data;
                        var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_startsession', self._commsessionid, self, self._commsessionid);
                        newmsg._from_fullname = zentools.me.FullName;
                        jQuery(newmsg).bind('sent', function (e, value) {
                            if (value._status === "error") {
                                newmsg.MarkAsError();
                                self.destroyBox();
                            } else {
                                self.start_TextChat();
                                self.start_Screenshare();
                            }
                        });
                        zentools.MsgCue.push(newmsg);
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
    RequestStartChat: function () {
        var self = this;
        try {
            var msg = {
                'apikey': zentools.ApiKey,
                'agent_username': zentools.me.UserName,
                'agent_password': zentools.me.Password,
                'agent_domain': zentools.me.Business.Domain,
                'username': self._interlocutor.UserName,
                'domain': self._interlocutor.Business.Domain,
                'password': self._interlocutor.Password
            };
            zentools.CommRequest_InCourse = jQuery.ajax({
                url: zentools.mainURL + "/Comm/Requestcommsessionid",
                type: "POST",
                data: msg,
                dataType: "jsonp",
                success: function (data) {
                    if (data !== false) {
                        self._commsessionid = data;
                        var newmsg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, '$#_startsession', self._commsessionid, self, self._commsessionid);
                        newmsg._from_fullname = zentools.me.FullName;
                        zentools.MsgCue.push(newmsg);
                        jQuery(newmsg).bind('sent', function (e, value) {
                            if (value._status === "error") {
                                newmsg.MarkAsError();
                                self.destroyBox();
                            } else {
                                self.start_TextChat();
                            }
                        });
                    } else {
                        self.destroyBox();
                    }
                    zentools.CommRequest_InCourse = null;
                },
                error: function (jqXHR, textStatus) {
                    zentools.CommRequest_InCourse = null;
                    log("Error Requestcommsessionid: " + textStatus);
                }
            });
        } catch (err) {
            zentools.CommRequest_InCourse = null;
            log("Error RequestStartChat: " + err);
        }
    },
    fillBox_Login: function () {
        var self = this;
        try {
            self._preferredubication = 'center';
            self._preferredwidth = '280px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_Login';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jQuery('<div></div>')
					.addClass('boxinfocontents')
					.appendTo(self._boxinfo);

            var title_text = jQuery('<h4></h4>')
					.text(LLang('login_title', null))
					.appendTo(msgbox);

            msgbox.append(jQuery('<br/>'));

            self.alert = jQuery('<div></div>')
					.addClass('alert')
					.appendTo(msgbox)
					.hide();

            var form = jQuery('<form/>')
					.addClass('form-stacked')
					.css({ "margin": "0 auto", "width": "250px" })
					.appendTo(msgbox);

            var fieldset = jQuery('<fieldset/>')
					.appendTo(form);

            var divclear1 = jQuery('<div/>')
					.addClass('clearfix')
					.appendTo(fieldset);
            var divlabel1 = jQuery('<label/>')
					.attr('for', 'user')
					.text(LLang('user', null))
					.appendTo(divclear1);
            var divinput1 = jQuery('<div/>')
					.addClass('input-append')
					.appendTo(divclear1);
            self.inputUser = jQuery('<input/>')
					.attr('id', 'user')
					.attr('name', 'user')
					.attr('type', 'text')
					.attr('placeholder', LLang('write_user', null))
					.addClass('input-small hint')
					.focus(function (event) {
					    var target = jQuery(event.target);
					    if (target.hasClass('hint') === true) {
					        target.val('');
					        target.removeClass('hint');
					    }
					})
					.keypress(function (e) {
					    try {
					        if (e.keyCode === 13) {
					            e.preventDefault();
					            if (jQuery(self.inputUser).val() !== "") {
					                jQuery(self.inputPassword).focus();
					                jQuery(self.inputPassword).select();
					            }
					        }
					    } catch (err) {
					        log(err);
					    }
					})
					.appendTo(divinput1);

            var divclear2 = jQuery('<div/>')
					.addClass('clearfix')
					.appendTo(fieldset);
            var divlabel2 = jQuery('<label/>')
					.attr('for', 'password')
					.text(LLang('password', null))
					.appendTo(divclear2);
            var divinput2 = jQuery('<div/>')
					.addClass('input')
					.appendTo(divclear2);
            self.inputPassword = jQuery('<input/>')
					.attr('id', 'password')
					.attr('name', 'password')
					.attr('type', 'password')
					.attr('placeholder', LLang('write_password', null))
					.addClass('input-medium hint')
					.focus(function (event) {
					    var target = jQuery(event.target);
					    if (target.hasClass('hint') === true) {
					        target.val('');
					        target.removeClass('hint');
					    }
					})
					.keypress(function (e) {
					    try {
					        if (e.keyCode === 13) {
					            e.preventDefault();
					            if ((jQuery(self.inputUser).val() !== "") && (jQuery(self.inputPassword).val() !== "")) {
					                //zentools.me.FullName = jQuery(inputHelp).val();
					                jQuery(self.inputUser)
										.addClass('disabled')
										.attr('disabled', 'disabled');
					                jQuery(self.inputPassword)
										.addClass('disabled')
										.attr('disabled', 'disabled');
					                jQuery(self.buttoninputLogin)
										.addClass('disabled')
										.hide();
					                jQuery(self.loading).show();
					                jQuery(self.alert).hide();
					                self.checkLogin();
					            }
					        }
					    } catch (err) {
					        log(err);
					    }
					})
					.appendTo(divinput2);

            var divclear3 = jQuery('<div/>')
					.addClass('clearfix')
					.appendTo(fieldset);
            var divlabel3 = jQuery('<label/>')
					.attr('for', 'login')
					.text('')
					.appendTo(divclear3);
            var divinput3 = jQuery('<div/>')
					.addClass('input')
					.appendTo(divclear3);
            self.buttoninputLogin = jQuery('<button></button>')
					.attr('id', 'login')
					.attr('name', 'login')
					.text(LLang('login', null))
					.addClass('btn btn-primary')
					.click(function (event) {
					    try {
					        if ((jQuery(self.inputUser).val() !== "") && (jQuery(self.inputPassword).val() !== "")) {
					            //zentools.me.FullName = jQuery(inputHelp).val();
					            jQuery(self.inputUser)
									.addClass('disabled')
									.attr('disabled', 'disabled');
					            jQuery(self.inputPassword)
									.addClass('disabled')
									.attr('disabled', 'disabled');
					            jQuery(self.buttoninputLogin)
									.addClass('disabled')
									.hide();
					            jQuery(self.loading).show();
					            jQuery(self.alert).hide();
					            self.checkLogin();
					        }
					    } catch (err) {
					        log(err);
					    }
					    return false;
					})
					.appendTo(divinput3);

            self.loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
					.css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
					.appendTo(divinput3)
					.hide();

            self._closebutton.hide();

            self.positionBox(null, 'fast');
        } catch (err) {
            log("fillBox_Login " + err);
        }
    }
},
	{
	    // properties
	    getset: [
			['Box', '_box']
	    ]
	});
