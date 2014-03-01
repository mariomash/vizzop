var TicketMessageBox = jVizzop.zs_Class.create(MessageBox, {
    // constructor
    initialize: function (_commsessionid) {
        var self = this;
        self.base('initialize');
        self._type = 'TicketBox';
        self._commsessionid = _commsessionid;
        self.fillBox_Loading();
    },
    fillBox_ShowMessageHistory: function () {
        var self = this;
        try {
            self._preferredubication = 'center';
            self._preferredwidth = '600px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_ShowMessageHistory';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();
            self.Messages = [];
            self._nolist = false;

            jVizzop(self._boxtitletext)
                .html('<i class="vizzop-icon-user vizzop-icon-white"></i>&nbsp;<span style="display: inline-block; vertical-align: middle">' + self._interlocutor.FullName + "</span>");
            self._title = self._interlocutor.FullName;

            self._boxheader = jVizzop('<div></div>')
                        .addClass('boxheader')
                        .appendTo(self._boxinfo);

            self._topstatusdiv = jVizzop('<div></div>')
                .css({ 'display': 'block', 'border-radius': '0', 'text-align': 'left', 'float': 'left', 'line-height': '32px', 'margin-left': '8px' })
                .appendTo(self._boxheader);

            var statusicon = jVizzop('<i></i>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle' }).addClass(
                'vizzop-icon-flag'
                )
                .appendTo(self._topstatusdiv);

            self._topstatustext = jVizzop('<span></span>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle', 'margin-left': '5px', 'margin-right': '5px' })
                .appendTo(self._topstatusdiv);

            self._topstatusloading = jVizzop('<span><img src="' + vizzop.mainURL + '/Content/images/loading.gif"/></span>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle' })
                .appendTo(self._topstatusdiv)
                .hide();

            self._lockedbydiv = jVizzop('<div></div>')
                .css({ 'display': 'block', 'border-radius': '0', 'text-align': 'right' })
                .hide()
                .appendTo(self._boxheader);

            var lockicon = jVizzop('<i></i>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle' }).addClass(
                'vizzop-icon-lock'
                )
                .appendTo(self._lockedbydiv);

            self._lockedby = jVizzop('<span></span>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle', 'margin-left': '5px', 'margin-right': '5px' })
                .appendTo(self._lockedbydiv);

            self._steallock = jVizzop('<span></span>')
                .addClass(
                'vizzop-btn vizzop-btn-warning'
                )
                .css({ 'vertical-align': 'middle' })
                .text(LLang('steal_lock', null))
                .click(function (e) {
                    e.preventDefault();
                    return false;
                })
                .appendTo(self._lockedbydiv);

            var div = jVizzop('<div></div>')
                .css({ 'clear': 'both' })
                .appendTo(self._boxheader);

            self._boxtextchat = jVizzop('<div></div>')
                .addClass('boxtextticket')
                .appendTo(self._boxinfo);

            var clear = jVizzop('<div></div>')
                .css({ 'clear': 'both' })
                .appendTo(self._boxtextchat);

            self._boxtextchat.msgZone = jVizzop('<span></span>')
                .addClass('chat_msgZone')
                .appendTo(self._boxtextchat);

            /*
            self._boxtextchat.infoZone = jVizzop('<span></span>')
            .addClass('chat_infoZone')
            .appendTo(self._boxtextchat);
            */

            self._boxtextchat.inputZone = jVizzop('<span></span>')
                .addClass('ticket_inputZone')
                .appendTo(self._boxtextchat);

            self._boxtextchat.inputTextArea = jVizzop('<textarea/>')
                        .attr('id', 'zs_message')
                        .attr('name', 'zs_message')
                        .addClass('ticket_inputTextArea hint')
                        .css({
                            'display': 'inline-block',
                            'vertical-align': 'top'
                        })
                        .focus(function (event) {
                            var target = jVizzop(event.target);
                            if (target.hasClass('hint') == true) {
                                target.val('');
                                target.removeClass('hint');
                            } else {
                                target.select();
                            }
                        })
                        .appendTo(self._boxtextchat.inputZone);

            self._boxtextchat.inputTextArea.focus();

            var ticketbuttons = jVizzop('<span></span>')
                .css({
                    'display': 'inline-block',
                    'vertical-align': 'top',
                    'margin-left': '10px',
                    'width': '150px'
                })
                .appendTo(self._boxtextchat.inputZone);

            self.commentandclose = jVizzop('<button></button>')
                .text("comment & close ticket")
                .attr('style', 'display: block; margin-top: 5px; width: 100% !important;')
                .addClass(
                'vizzop-btn vizzop-btn-primary'
                )
                .click(function (e) {
                    e.preventDefault();
                    jVizzop(self._loading).show();
                    jVizzop(self._boxtextchat.inputZone).hide();
                    if ((jVizzop.trim(jVizzop(self._boxtextchat.inputTextArea).val()) != '') && (jVizzop(self.commentandclose).attr('disabled') != true)) {
                        if (self._interlocutor.UserName != null) {
                            var msg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, self._interlocutor[0].UserName + '@' + self._interlocutor[0].Business.Domain, 'ticket', jVizzop(self._boxtextchat.inputTextArea).val(), self, self._commsessionid);
                            msg._commsessionid = self._commsessionid;
                            msg._selfticketstate = 'close';
                            jVizzop(msg).bind('sent', function (e, value) {
                                if (value._status == "error") {
                                    msg.MarkAsError();
                                    jVizzop(self._loading).hide();
                                    jVizzop(self._boxtextchat.inputZone).show();
                                } else {
                                    msg.MarkAsOk();
                                    vizzop.Daemon.loadTicketBox(self._commsessionid);
                                    try {
                                        Comm_updatetable();
                                    } catch (_err) { }
                                }
                            });
                            msg.Send(self);
                            //msg.AddMsgToChat(msg);
                            jVizzop(self._boxtextchat.inputTextArea).val('');
                        }
                    }
                    if (jVizzop.trim(jVizzop(this).val()) == '') {
                        jVizzop(this).val(null);
                    }
                    return false;
                })
                .appendTo(ticketbuttons);
            //vizzoplib.log(vizzop.CommRequest_InCourse);

            self.justcomment = jVizzop('<button></button>')
                .text("just comment")
                .attr('style', 'display: block; margin-top: 5px; width: 100% !important;')
                .addClass(
                'vizzop-btn'
                )
                .css({})
                .click(function (e) {
                    e.preventDefault();
                    jVizzop(self._loading).show();
                    jVizzop(self._boxtextchat.inputZone).hide();
                    if ((jVizzop.trim(jVizzop(self._boxtextchat.inputTextArea).val()) != '') && (jVizzop(self.justcomment).attr('disabled') != true)) {
                        if (self._interlocutor.UserName != null) {
                            var msg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, self._interlocutor[0].UserName + '@' + self._interlocutor[0].Business.Domain, null, jVizzop(self._boxtextchat.inputTextArea).val(), self, self._commsessionid);
                            msg._commsessionid = self._commsessionid;
                            msg._selfticketstate = 'open';
                            jVizzop(msg).bind('sent', function (e, value) {
                                if (value._status == "error") {
                                    msg.MarkAsError();
                                    jVizzop(self._loading).hide();
                                    jVizzop(self._boxtextchat.inputZone).show();
                                } else {
                                    msg.MarkAsOk();
                                    vizzop.Daemon.loadTicketBox(self._commsessionid);
                                    try {
                                        Comm_updatetable();
                                    } catch (_err) { }
                                }
                            });
                            msg.Send(self);
                            msg.AddMsgToChat(msg);
                            jVizzop(self._boxtextchat.inputTextArea).val('');
                        }
                    }
                    if (jVizzop.trim(jVizzop(this).val()) == '') {
                        jVizzop(this).val(null);
                    }
                    return false;
                })
                .appendTo(ticketbuttons);

            self.justclose = jVizzop('<button></button>')
                .text("just close")
                .addClass(
                'vizzop-btn'
                )
                .attr('style', 'display: block; margin-top: 5px; width: 100% !important;')
                .click(function (e) {
                    e.preventDefault();
                    jVizzop(self._loading).show();
                    jVizzop(self._boxtextchat.inputZone).hide();
                    if (jVizzop(self.justclose).attr('disabled') != true) {
                        if (self._interlocutor.UserName != null) {
                            var msg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, self._interlocutor[0].UserName + '@' + self._interlocutor[0].Business.Domain, null, null, self, self._commsessionid);
                            msg._commsessionid = self._commsessionid;
                            msg._selfticketstate = 'close';
                            jVizzop(msg).bind('sent', function (e, value) {
                                if (value._status == "error") {
                                    msg.MarkAsError();
                                    jVizzop(self._loading).hide();
                                    jVizzop(self._boxtextchat.inputZone).show();
                                } else {
                                    msg.MarkAsOk();
                                    vizzop.Daemon.loadTicketBox(self._commsessionid);
                                    try {
                                        Comm_updatetable();
                                    } catch (_err) { }
                                }
                            });
                            msg.Send(self);
                        }
                    }
                    return false;
                })
                .appendTo(ticketbuttons);

            jVizzop.each(self.Messages, function (i, v) {
                v.AddMsgToChat(v);
                return;
            });

            self._reopenbutton = jVizzop('<div></div>')
                .addClass('vizzop-btn vizzop-btn-warning')
                .text(LLang('reopen_ticket', null))
                .click(function (e) {
                    e.preventDefault();
                    self._reopenbutton.hide();
                    self._topstatusloading.show();
                    var msg = new Message(vizzop.me.UserName + '@' + vizzop.me.Business.Domain, self._interlocutor[0].UserName + '@' + self._interlocutor[0].Business.Domain, null, null, self, self._commsessionid);
                    msg._commsessionid = self._commsessionid;
                    msg._selfticketstate = 'open';
                    jVizzop(msg).bind('sent', function (e, value) {
                        if (value._status == "error") {
                            msg.MarkAsError();
                            jVizzop(self._topstatusloading).hide();
                            jVizzop(self._reopenbutton).show();
                        } else {
                            msg.MarkAsOk();
                            vizzop.Daemon.loadTicketBox(self._commsessionid);
                        }
                    });
                    msg.Send(self);
                    msg.AddMsgToChat(msg);
                    return false;
                })
                .appendTo(self._topstatusdiv)
                .hide();

            self._loading = jVizzop('<div></div>')
                .css({ 'text-align': 'center', 'padding': '5px' })
                .appendTo(self._boxtextchat)
                .hide();

            self._loadingimg = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
                .appendTo(self._loading);

            self._closebutton
                .show()
                .click(function (e) {
                    self.destroyBox();
                    return false;
                });

            self.positionBox(function() { jVizzop(self._box).show(); }, 'fast');

        } catch (err) {
            vizzoplib.log(err);
        }
    }
});
