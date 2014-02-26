
// Message Class
var Message = jVizzop.zs_Class.create({
    // constructor
    initialize: function (from, to, subject, content, box, commsessionid, clientid) {
        var self = this;
        this._from = from;
        this._to = to;
        this._subject = subject;
        if (this._subject == null) {
            this._subject = "SupportMsg";
        }
        this._content = content;
        this._timestamp = new Date();
        this._old = null;
        this._status = null;
        this._commsessionid = commsessionid;
        this._clientid = clientid;
        if (this._clientid == null) {
            this._clientid = vizzoplib.randomnumber();
        }
        this._box = box;
    },
    // methods
    AddMsgToChat: function () {
        var self = this;
        //vizzoplib.log(self);
        try {
            if ((self._content == null) || (self._content == "null")) {
                return;
            }

            var msg_mine = false;
            if (self._from_username + "@" + self._from_domain == vizzop.me.UserName + "@" + vizzop.me.Business.Domain) {
                msg_mine = true;
            }

            self.divmsg = jVizzop('<span></span>');
            if (msg_mine == true) {
                self.divmsg.addClass('message mine');
            } else {
                self.divmsg.addClass('message interlocutor');
            }

            if (self._old == "1") {
                self.divmsg.addClass('dimmed');
            }

            var index = new Number(0);
            jVizzop.each(self._box.Messages, function (i, v) {
                if (v._clientid == self._clientid) {
                    index = i;
                    return;
                }
            });
            if (index == 0) {
                index = self._box.Messages.length;
            }

            var show = true;

            if (self._box._type == "MessageBox") {
                if (self._box.Messages.length > 0) {
                    if (self._box._boxtextchat != null) {
                        if (jVizzop(self._box._boxtextchat.msgZone).find('.message').length > 0) {
                            if (self._box.Messages[index - 1]) {
                                if (self._box.Messages[index - 1]._from == self._from) {
                                    show = false;
                                }
                            }
                        }
                    }
                }
            }


            var timestamp_toshow = self._timestamp.toTimeString().substring(0, (self._timestamp.toTimeString().indexOf(' ') - 3));                      
         
            if (self._box._type == "TicketBox") {
                timestamp_toshow = self._timestamp.toString();
            }

            if (show == true) {
                var divdate = jVizzop('<span></span>')
                        .addClass('timestamp')
                        .text(timestamp_toshow)
                        .appendTo(self.divmsg);
                var name_from = self._from;
                //console.vizzoplib.log(self._from_username + " " + vizzop.me.UserName)
                if (msg_mine == true) {
                    name_from = LLang("yo", null);
                } else {
                    if ((self._from == null) || (self._from == "")) {
                        name_from = LLang("anon_client", null);
                    }
                }
                var divfrom = jVizzop('<span></span>')
                        .addClass('from')
                        .html(name_from)
                        .appendTo(self.divmsg);
            }

            var arr_content = self._content.split("\n");
            //var content = self._content.replace(/\n/g, "<br/>");

            self.divmsg_contents = jVizzop('<span></span>')
                    .addClass('contents')
                    .appendTo(self.divmsg);

            jVizzop(arr_content).each(function (_i, _v) {
                var content = jVizzop('<p>')
                    .text(_v)
                    .appendTo(self.divmsg_contents);
            });

            var exists_yet = false;
            jVizzop.each(self._box.Messages, function (i, v) {
                if (v._clientid == self._clientid) {
                    exists_yet = true;
                    return;
                }
            });
            if (exists_yet == false) {

                var inserted = false;

                jVizzop.each(self._box.Messages, function (i, v) {
                    if (v._timestamp > self._timestamp) {
                        self.divmsg.insertBefore(jVizzop(v.divmsg));
                        inserted = true;
                        return false;
                    }
                });

                if (inserted == false) {
                    self.divmsg.appendTo(self._box._boxtextchat.msgZone);
                }

                if (msg_mine == false) {
                    vizzop.Daemon.audioNewMessage.Play();
                    //jVizzop(self._box._boxstatus).html(self._box._statustext);
                }

                vizzop.Messages.push(self);
                self._box.Messages.push(self);
                if (self._box._box.hasClass('focused') == false) {
                    self._box.unread_elements = self._box.unread_elements + 1;
                }
                try {
                    self._box.process_alerts();
                } catch (_err) {
                    vizzoplib.log(_err);
                }
            }

            try {
                var objDiv = jVizzop(self._box._boxtextchat.msgZone);
                objDiv.scrollTop(objDiv[0].scrollHeight);
                //objDiv.scrollTop = objDiv.outerScrollHeight();
                //objDiv.attr({ scrollTop: objDiv.attr("scrollHeight") });
            } catch (_err) {
                vizzoplib.log(_err);
            }

            try {
                if (vizzop.mode == 'client') {
                    if (msg_mine == false) {
                        jVizzop(self._box._boxtextchat.photoZone.name)
                            .text(self._from);
                        jVizzop(self._box._boxtextchat.photoZone)
                            .show()
                        self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
                    }
                }
            } catch (_err) {
                vizzoplib.log(_err);
            }

        } catch (err) {
            vizzoplib.log("AddMsgToChat " + err);
        }
    },
    MarkAsError: function () {
        var self = this;
        try {
            self.msg_error = jVizzop('<span></span>')
                        .css({ 'float': 'right' })
                        .prependTo(jVizzop(self.divmsg_contents));

            var msg_error_text = jVizzop('<span></span>')
                        .addClass('label ' + 'label-important')
            //.text(LLang('error_sending', null))
                        .html('<i class="vizzop-icon-exclamation-sign vizzop-icon-white"></i>')
                        .appendTo(jVizzop(self.msg_error));

            var msg_error_a = jVizzop('<a/>')
                        .addClass('resend')
                        .text(LLang('resend', null))
                        .click(function (event) {
                            jVizzop(self.msg_error).remove();
                            vizzop.MsgCue.push(self);
                            return false;
                        })
                        .appendTo(jVizzop(self.msg_error));
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    MarkAsOk: function () {
        var self = this;
        try {
            self.msg_ok = jVizzop('<span></span>')
                        .prependTo(jVizzop(self.divmsg_contents));

            
            var msg_ok_text = jVizzop('<i></i>')
                        .addClass('vizzop-icon-ok')
                        .css({
                            'float': 'right'
                        })
                        .appendTo(jVizzop(self.msg_ok));
                        

        } catch (err) {
            vizzoplib.log("MarkAsOk " + err);
        }
    },
    Send: function () {
        var self = this;
        try {
            //se envia en UTC
            var TimeStampSenderSending = new Date();
            var msg = {
                'From': self._from,
                'From_FullName': self._from_fullname,
                'To': self._to,
                'Subject': self._subject,
                'Content': self._content,
                '_clientid': self._clientid,
                '_status': self._status,
                'TimeStamp': self._timestamp.toJSON(),
                'TimeStampSenderSending': TimeStampSenderSending.toJSON(),
                'commsessionid': self._commsessionid,
                'SetTicketState': self._selfticketstate,
                'messagetype': 'Plain'
            };

            if (vizzop.WSchat != null) {
                if (vizzop.WSchat.readyState === undefined || vizzop.WSchat.readyState > 1) {
                    vizzop.Daemon.openWebSockets();
                }
                if (vizzop.WSchat.readyState == WebSocket.OPEN) {
                    vizzop.WSchat.send(JSONVIZZOP.stringify(msg));
                    self._status = "sent";
                } else {
                    self._status = "error";
                }
                jVizzop(self).trigger("sent", self);
            } else {
                var url = vizzop.mainURL + "/vizzop/send.ashx";
                var req = jVizzop.ajax({
                    url: url,
                    type: "POST",
                    data: msg,
                    dataType: "jsonp",
                    success: function (data) {
                        if (data == true) {
                            self._status = "sent";
                        } else {
                            self._status = "error";
                        }
                        jVizzop(self).trigger("sent", self);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        vizzoplib.logAjax(url, msg, jqXHR);
                        self._status = "error";
                        vizzop.MsgCue.splice(0, 1, self);
                        jVizzop(self).trigger("sent", self);
                    }
                });
            }
        } catch (err) {
            vizzoplib.log("Msg no enviado. Error: " + err);
            self._status = "error";
            vizzop.MsgCue.splice(0, 1, self);
            jVizzop(self).trigger("sent", self);
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
