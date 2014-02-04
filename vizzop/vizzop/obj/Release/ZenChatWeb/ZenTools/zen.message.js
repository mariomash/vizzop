
// Message Class
var Message = jQuery.zs_Class.create({
    // constructor
    initialize: function (from, to, subject, content, box, commsessionid, clientid) {
        var self = this;
        this._from = from;
        this._to = to;
        this._subject = subject;
        if (this._subject === null) {
            this._subject = "SupportMsg";
        }
        this._content = content;
        this._timestamp = new Date();
        this._old = null;
        this._status = null;
        this._commsessionid = commsessionid;
        this._clientid = clientid;
        if (this._clientid === null) {
            this._clientid = randomnumber();
        }
        this._box = box;
    },
    // methods
    AddMsgToChat: function () {
        var self = this;
        //log(self);
        try {
            if ((self._content === null) || (self._content === "null")) {
                return;
            }

            var msg_mine = false;
            if (self._from_username + "@" + self._from_domain === zentools.me.UserName + "@" + zentools.me.Domain) {
                msg_mine = true;
            }

            self.divmsg = jQuery('<span></span>')
                    .addClass('message');

            if (self._old === "1") {
                self.divmsg.addClass('dimmed');
            }

            var index = new Number(0);
            jQuery.each(self._box.Messages, function (i, v) {
                if (v._clientid === self._clientid) {
                    index = i;
                    return;
                }
            });
            if (index === 0) {
                index = self._box.Messages.length;
            }

            var show = true;

            if ((self._box._type === "MessageBox") && (self._box.Messages.length > 0) && (jQuery(self._box._boxtextchat.msgZone).find('.message').length > 0)) {
                if (self._box.Messages[index - 1]._from === self._from) {
                    show = false;
                }
            }


            var timestamp_toshow = self._timestamp.toTimeString().substring(0, (self._timestamp.toTimeString().indexOf(' ') - 3));
            if (self._box._type === "TicketBox") {
                timestamp_toshow = self._timestamp.toString();
            }

            if (show === true) {
                var divdate = jQuery('<span></span>')
                        .addClass('timestamp')
                        .text(timestamp_toshow)
                        .appendTo(self.divmsg);
                var name_from = self._from;
                //console.log(self._from_username + " " + zentools.me.UserName)
                if (msg_mine === true) {
                    name_from = LLang("yo", null);
                } else {
                    if (self._from === null) {
                        name_from = LLang("anon_client", null);
                    }
                }
                var divfrom = jQuery('<span></span>')
                        .addClass('from')
                        .html(name_from)
                        .appendTo(self.divmsg);
            }

            var arr_content = self._content.split("\n");
            //var content = self._content.replace(/\n/g, "<br/>");

            self.divmsg_contents = jQuery('<span></span>')
                    .addClass('contents')
                    .appendTo(self.divmsg);

            $(arr_content).each(function (_i, _v) {
                var content = jQuery('<p>')
                    .text(_v)
                    .appendTo(self.divmsg_contents);
            });

            var exists_yet = false;
            jQuery.each(self._box.Messages, function (i, v) {
                if (v._clientid === self._clientid) {
                    exists_yet = true;
                    return;
                }
            });
            if (exists_yet === false) {

                var inserted = false;

                jQuery.each(self._box.Messages, function (i, v) {
                    if (v._timestamp > self._timestamp) {
                        self.divmsg.insertBefore(jQuery(v.divmsg));
                        inserted = true;
                        return false;
                    }
                });

                if (inserted === false) {
                    self.divmsg.appendTo(self._box._boxtextchat.msgZone);
                }

                if (msg_mine === false) {
                    zentools.Daemon.audioNewMessage.Play();
                    jQuery(self._box._boxstatus).html(self._box._statustext);
                }

                zentools.Messages.push(self);
                self._box.Messages.push(self);
                if (self._box._box.hasClass('focused') === false) {
                    self._box.unread_elements = self._box.unread_elements + 1;
                }
                try {
                    self._box.process_alerts();
                } catch (_err) {
                    log(_err);
                }
            }

            try {
                jQuery(self._box._boxtextchat.msgZone).scrollTop(jQuery(self._box._boxtextchat.msgZone).outerScrollHeight());
                //jQuery(self._box._boxtextchat.msgZone).attr({ scrollTop: jQuery(self._box._boxtextchat.msgZone).attr("scrollHeight") });
            } catch (_err) {
                log(_err);
            }
        } catch (err) {
            log("AddMsgToChat " + err);
        }
    },
    MarkAsError: function () {
        var self = this;
        try {
            self.msg_error = jQuery('<span></span>')
                        .css({ 'float': 'right' })
                        .prependTo(jQuery(self.divmsg_contents));

            var msg_error_text = jQuery('<span></span>')
                        .addClass('label ' + 'label-important')
            //.text(LLang('error_sending', null))
                        .html('<i class="icon-exclamation-sign icon-white"></i>')
                        .appendTo(jQuery(self.msg_error));

            var msg_error_a = jQuery('<a/>')
                        .addClass('resend')
                        .text(LLang('resend', null))
                        .click(function (event) {
                            jQuery(self.msg_error).remove();
                            zentools.MsgCue.push(self);
                            return false;
                        })
                        .appendTo(jQuery(self.msg_error));
        } catch (err) {
            log(err);
        }
    },
    MarkAsOk: function () {
        var self = this;
        try {
            self.msg_ok = jQuery('<span></span>')
                        .prependTo(jQuery(self.divmsg_contents));

            /*
            var msg_ok_text = jQuery('<i></i>')
                        .addClass('icon-ok')
                        .css({
                            'float': 'right'
                        })
                        .appendTo(jQuery(self.msg_ok));
                        */

        } catch (err) {
            log("MarkAsOk " + err);
        }
    },
    Send: function () {
        var self = this;
        try {
            //se envia en UTC
            var msg = {
                'From': self._from,
                'From_FullName': self._from_fullname,
                'To': self._to,
                'Subject': self._subject,
                'Content': self._content,
                '_clientid': self._clientid,
                '_status': self._status,
                'TimeStamp': self._timestamp.toJSON(),
                'commsessionid': self._commsessionid,
                'SetTicketState': self._selfticketstate
            };
            //log("Por Enviar: " + self._content);
            var url = zentools.mainURL + "/ZenTools/send.ashx";
            var req = jQuery.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "jsonp",
                success: function (data) {
                    if (data === true) {
                        self._status = "sent";
                    } else {
                        self._status = "error";
                    }
                    jQuery(self).trigger("sent", self);
                },
                error: function (jqXHR, textStatus) {
                    log("Msg no enviado. Error: " + textStatus);
                    self._status = "error";
                    zentools.MsgCue.splice(0, 1, self);
                    jQuery(self).trigger("sent", self);
                }
            });
        } catch (err) {
            log("Msg no enviado. Error: " + err);
            self._status = "error";
            zentools.MsgCue.splice(0, 1, self);
            jQuery(self).trigger("sent", self);
        }
    }
},
    {
        // properties
        getset: [
            ['From', '_from'],
            ['To', '_to'],
            ['Content', '_content'],
            ['Subject', '_subject']
        ]
    });
