
var TicketMessageBox = jQuery.zs_Class.create(MessageBox, {
    // constructor
    initialize: function (_commsessionid) {
        var self = this;
        self.base('initialize');
        self._type = 'TicketBox';
        self._commsessionid = _commsessionid;
        self.fillBox_Loading();
    },
    fillBox_ShowMessageHistory: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '600px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_ShowMessageHistory';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();
            self.Messages = [];
            self._nolist = false;

            jQuery(self._boxtitletext)
                .html('<i class="icon-user"></i>&nbsp;<span style="display: inline-block; vertical-align: middle">' + self._interlocutor.FullName + "</span>");
            self._title = self._interlocutor.FullName;

            self._boxheader = jQuery('<div></div>')
                        .addClass('boxheader')
                        .appendTo(self._boxinfo);

            self._topstatusdiv = jQuery('<div></div>')
                .css({ 'display': 'block', 'border-radius': '0', 'text-align': 'left', 'float': 'left', 'line-height': '32px', 'margin-left': '8px' })
                .appendTo(self._boxheader);

            var statusicon = jQuery('<i></i>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle' }).addClass(
                'icon-flag'
                )
                .appendTo(self._topstatusdiv);

            self._topstatustext = jQuery('<span></span>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle', 'margin-left': '5px', 'margin-right': '5px' })
                .appendTo(self._topstatusdiv);

            self._topstatusloading = jQuery('<span><img src="' + zentools.mainURL + '/Content/images/loading.gif"/></span>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle' })
                .appendTo(self._topstatusdiv)
                .hide();

            self._lockedbydiv = jQuery('<div></div>')
                .css({ 'display': 'block', 'border-radius': '0', 'text-align': 'right' })
                .hide()
                .appendTo(self._boxheader);

            var lockicon = jQuery('<i></i>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle' }).addClass(
                'icon-lock'
                )
                .appendTo(self._lockedbydiv);

            self._lockedby = jQuery('<span></span>')
                .css({ 'display': 'inline-block', 'vertical-align': 'middle', 'margin-left': '5px', 'margin-right': '5px' })
                .appendTo(self._lockedbydiv);

            self._steallock = jQuery('<span></span>')
                .addClass(
                'btn btn-warning'
                )
                .css({ 'vertical-align': 'middle' })
                .text(LLang('steal_lock', null))
                .click(function (e) {
                    e.preventDefault();
                    return false;
                })
                .appendTo(self._lockedbydiv);

            var div = jQuery('<div></div>')
                .css({ 'clear': 'both' })
                .appendTo(self._boxheader);

            self._boxtextchat = jQuery('<div></div>')
                .addClass('boxtextticket')
                .appendTo(self._boxinfo);

            var clear = jQuery('<div></div>')
                .css({ 'clear': 'both' })
                .appendTo(self._boxtextchat);

            self._boxtextchat.msgZone = jQuery('<span></span>')
                .addClass('chat_msgZone')
                .appendTo(self._boxtextchat);

            /*
            self._boxtextchat.infoZone = jQuery('<span></span>')
            .addClass('chat_infoZone')
            .appendTo(self._boxtextchat);
            */

            self._boxtextchat.inputZone = jQuery('<span></span>')
                .addClass('ticket_inputZone')
                .appendTo(self._boxtextchat);

            self._boxtextchat.inputTextArea = jQuery('<textarea/>')
                        .attr('id', 'zs_message')
                        .attr('name', 'zs_message')
                        .addClass('ticket_inputTextArea hint')
                        .css({
                            'display': 'inline-block',
                            'vertical-align': 'top'
                        })
                        .focus(function (event) {
                            var target = jQuery(event.target);
                            if (target.hasClass('hint') === true) {
                                target.val('');
                                target.removeClass('hint');
                            } else {
                                target.select();
                            }
                        })
                        .appendTo(self._boxtextchat.inputZone);

            self._boxtextchat.inputTextArea.focus();

            var ticketbuttons = jQuery('<span></span>')
                .css({
                    'display': 'inline-block',
                    'vertical-align': 'top',
                    'margin-left': '10px',
                    'width': '150px'
                })
                .appendTo(self._boxtextchat.inputZone);

            self.commentandclose = jQuery('<button></button>')
                .text("comment & close ticket")
                .css({
                    'display': 'block',
                    'width': '100%'
                })
                .addClass(
                'btn btn-primary'
                )
                .click(function (e) {
                    e.preventDefault();
                    jQuery(self._loading).show();
                    jQuery(self._boxtextchat.inputZone).hide();
                    if ((jQuery.trim(jQuery(self._boxtextchat.inputTextArea).val()) !== '') && (jQuery(self.commentandclose).attr('disabled') !== true)) {
                        if (self._interlocutor.UserName !== null) {
                            var msg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, 'ticket', jQuery(self._boxtextchat.inputTextArea).val(), self, self._commsessionid);
                            msg._commsessionid = self._commsessionid;
                            msg._selfticketstate = 'close';
                            jQuery(msg).bind('sent', function (e, value) {
                                if (value._status === "error") {
                                    msg.MarkAsError();
                                    jQuery(self._loading).hide();
                                    jQuery(self._boxtextchat.inputZone).show();
                                } else {
                                    msg.MarkAsOk();
                                    zentools.Daemon.loadTicketBox(self._commsessionid);
                                }
                            });
                            msg.Send(self);
                            //msg.AddMsgToChat(msg);
                            jQuery(self._boxtextchat.inputTextArea).val('');
                        }
                    }
                    if (jQuery.trim(jQuery(this).val()) === '') {
                        jQuery(this).val(null);
                    }
                    return false;
                })
                .appendTo(ticketbuttons);
            //log(zentools.CommRequest_InCourse);

            self.justcomment = jQuery('<button></button>')
                .text("just comment")
                .css({
                    'display': 'block',
                    'margin-top': '5px',
                    'width': '100%'
                })
                .addClass(
                'btn'
                )
                .css({})
                .click(function (e) {
                    e.preventDefault();
                    jQuery(self._loading).show();
                    jQuery(self._boxtextchat.inputZone).hide();
                    if ((jQuery.trim(jQuery(self._boxtextchat.inputTextArea).val()) !== '') && (jQuery(self.justcomment).attr('disabled') !== true)) {
                        if (self._interlocutor.UserName !== null) {
                            var msg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, null, jQuery(self._boxtextchat.inputTextArea).val(), self, self._commsessionid);
                            msg._commsessionid = self._commsessionid;
                            msg._selfticketstate = 'open';
                            jQuery(msg).bind('sent', function (e, value) {
                                if (value._status === "error") {
                                    msg.MarkAsError();
                                    jQuery(self._loading).hide();
                                    jQuery(self._boxtextchat.inputZone).show();
                                } else {
                                    msg.MarkAsOk();
                                    zentools.Daemon.loadTicketBox(self._commsessionid);
                                }
                            });
                            msg.Send(self);
                            msg.AddMsgToChat(msg);
                            jQuery(self._boxtextchat.inputTextArea).val('');
                        }
                    }
                    if (jQuery.trim(jQuery(this).val()) === '') {
                        jQuery(this).val(null);
                    }
                    return false;
                })
                .appendTo(ticketbuttons);

            self.justclose = jQuery('<button></button>')
                .text("just close")
                .addClass(
                'btn'
                )
                .css({
                    'display': 'block',
                    'margin-top': '5px',
                    'width': '100%'
                })
                .click(function (e) {
                    e.preventDefault();
                    jQuery(self._loading).show();
                    jQuery(self._boxtextchat.inputZone).hide();
                    if (jQuery(self.justclose).attr('disabled') !== true) {
                        if (self._interlocutor.UserName !== null) {
                            var msg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, null, null, self, self._commsessionid);
                            msg._commsessionid = self._commsessionid;
                            msg._selfticketstate = 'close';
                            jQuery(msg).bind('sent', function (e, value) {
                                if (value._status === "error") {
                                    msg.MarkAsError();
                                    jQuery(self._loading).hide();
                                    jQuery(self._boxtextchat.inputZone).show();
                                } else {
                                    msg.MarkAsOk();
                                    zentools.Daemon.loadTicketBox(self._commsessionid);
                                }
                            });
                            msg.Send(self);
                        }
                    }
                    return false;
                })
                .appendTo(ticketbuttons);

            jQuery.each(self.Messages, function (i, v) {
                v.AddMsgToChat(v);
                return;
            });

            self._reopenbutton = jQuery('<div></div>')
                .addClass('btn btn-warning')
                .text(LLang('reopen_ticket', null))
                .click(function (e) {
                    e.preventDefault();
                    self._reopenbutton.hide();
                    self._topstatusloading.show();
                    var msg = new Message(zentools.me.UserName + '@' + zentools.me.Business.Domain, self._interlocutor.UserName + '@' + self._interlocutor.Business.Domain, null, null, self, self._commsessionid);
                    msg._commsessionid = self._commsessionid;
                    msg._selfticketstate = 'open';
                    jQuery(msg).bind('sent', function (e, value) {
                        if (value._status === "error") {
                            msg.MarkAsError();
                            jQuery(self._topstatusloading).hide();
                            jQuery(self._reopenbutton).show();
                        } else {
                            msg.MarkAsOk();
                            zentools.Daemon.loadTicketBox(self._commsessionid);
                        }
                    });
                    msg.Send(self);
                    msg.AddMsgToChat(msg);
                    return false;
                })
                .appendTo(self._topstatusdiv)
                .hide();

            self._loading = jQuery('<div></div>')
                .css({ 'text-align': 'center', 'padding': '5px' })
                .appendTo(self._boxtextchat)
                .hide();

            self._loadingimg = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
                .appendTo(self._loading);

            self._closebutton
                .show()
                .click(function (e) {
                    self.destroyBox();
                    return false;
                });

            self.positionBox(null, 'fast');

        } catch (err) {
            log(err);
        }
    }
});
