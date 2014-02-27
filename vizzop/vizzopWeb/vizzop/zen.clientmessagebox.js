
var ClientMessageBox = jVizzop.zs_Class.create(MessageBox, {
    // constructor
    initialize: function () {
        var self = this;
        self.base('initialize');
        self.fillBox_helpStandby();
    },
    fillBox_FindingSupport_WithoutName: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_FindingSupport_WithoutName';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jVizzop('<div></div>')
                .addClass('zs_boxinfocontents')
                .appendTo(self._boxinfo);

            var text = jVizzop('<p></p>')
                .text(LLang('name_question', null))
                .appendTo(msgbox);

            msgbox.append(jVizzop('<br/>'));

            var divclear1 = jVizzop('<div/>')
                .appendTo(msgbox);
            var divinput1 = jVizzop('<div/>')
                .addClass(
                'input'
                )
                .appendTo(divclear1);

            jVizzop(self).bind('ConverserUpdateNameFinished');
            jVizzop(self).bind('ConverserUpdateNameFinished', function (e, value) {
                if (value == null) {
                    self.fillBox_FindingSupport_WithoutName();
                } else {
                    vizzop.me = value;
                    var name_mecookie = vizzop.ApiKey + "_me";
                    var name_comsessionidcookie = vizzop.ApiKey + "_commsessionid";
                    vizzoplib.setCookie(name_mecookie, jVizzop.toJSON(vizzop.me), 1);
                    self.fillBox_FindingSupport_WithName();
                }
            });

            var inputHelp = jVizzop('<input/>')
                .attr('id', 'client_name')
                .attr('name', 'client_name')
                .attr('type', 'text')
                .attr('maxlength', '25')
                .attr('placeholder', LLang('write_name', null))
                .addClass(
                'input-medium hint'
            )
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
                            if (jVizzop(inputHelp).val() != "") {
                                jVizzop(inputHelp)
                                    .addClass('disabled')
                                    .attr('disabled', 'disabled');
                                jVizzop(buttoninputHelp)
                                    .addClass('disabled')
                                    .hide();
                                jVizzop(loading).show();
                                self.ConverserUpdateName(jVizzop(inputHelp).val());
                            }
                        }
                    } catch (err) {
                        vizzoplib.log(err);
                    }
                })
                .appendTo(divinput1);

            var buttoninputHelp = jVizzop('<button></button>')
                .text(LLang('ok', null))
                .addClass('vizzop-btn vizzop-btn-primary')
                .click(function (event) {
                    try {
                        if (jVizzop(inputHelp).val() != "") {
                            jVizzop(inputHelp)
                                .addClass('disabled')
                                .attr('disabled', 'disabled');
                            jVizzop(buttoninputHelp)
                                .addClass('disabled')
                                .hide();
                            jVizzop(loading).show();
                            self.ConverserUpdateName(jVizzop(inputHelp).val());
                        }
                    } catch (err) {
                        vizzoplib.log(err);
                    }
                    return false;
                })
                .appendTo(divinput1);

            var loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
                .appendTo(divinput1)
                .hide();

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    fillBox_FindingSupport_WithName: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_FindingSupport_WithName';
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
                .text(LLang('waiting_support', [vizzop.me.FullName]))
                .appendTo(msgbox);

            var loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px' })
                .appendTo(msgbox);

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');

            /*
            */

            self._commsessionid = null;
            vizzop.CommRequest_InCourse = null;
            vizzop.Daemon.Requestcommsessionid();

        } catch (err) {
            vizzoplib.log(err);
        }
    },
    fillBox_helpYesNo: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_helpYesNo';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            var msgbox = jVizzop('<div></div>')
                .addClass('zs_boxinfocontents')
                .appendTo(self._boxinfo);

            var text = jVizzop('<h5></h5>')
                .text(LLang('can_we_assist', null))
                .appendTo(msgbox);

            msgbox.append(jVizzop('<br/>'));

            var buttonYes = jVizzop('<button></button>')
                .text(LLang('yes', null))
                .addClass('vizzop-btn vizzop-btn-primary')
                .click(function (event) {
                    var box = self._box;
                    var pwidth = box.width();
                    var pheight = box.height();
                    var msgbox = self._boxmsg;
                    msgbox.html(null);
                    box.makeAbsolute();
                    box.animate({
                        opacity: 0,
                        'top': '+=' + (pheight / 2),
                        'left': '+=' + (pwidth / 2),
                        'width': (pheight / 2) + 'px',
                        'height': (pwidth / 2) + 'px'
                    }, 'fast', function () {
                        self.fillBox_helpQuestChat();
                    });
                    return false;
                })
                .appendTo(msgbox);

            var buttonNo = jVizzop('<button></button>')
                .text(LLang('no', null))
                .addClass('vizzop-btn')
                .click(function (event) {
                    self.fillBox_helpStandby();
                    return false;
                })
                .appendTo(msgbox);

        } catch (err) {
            vizzoplib.log(err);
        }
    },
    checkBubble: function () {
        var self = this;
        try {
            if (vizzop.AllowChat == true) {
                self._bubbletext
                    .text(LLang('agents_available', null));
                self._bubble
                    .css({
                        'display': 'block'
                    });
            } else {
                self._bubbletext
                    .text(LLang('leave_us_a_messsage', null));
                self._bubble
                    .hide();
            }
        } catch (ex) {
            vizzoplib.log(err);
        }
    },
    fillBox_helpStandby: function () {
        try {
            var self = this;
            self._preferredubication = 'standbychat';
            self._preferredwidth = 'auto';
            self._preferredheight = 'auto';
            self._status = 'fillBox_helpStandby';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self.hideBox();
            self._boxinfo.empty();
            var box = self._box;
            // No va a la lista de boxes..
            self._nolist = true;
            self._boxinner.hide();
            //Handle Bar
            self._handle = jVizzop('<span></span>')
                .addClass('zs_dialoghandle')
                .appendTo(box);

            if (vizzop.WidgetBg != null) {
                self._handle.attr('style', 'background-color : ' + vizzop.WidgetBg + ' !important; color: ' + vizzop.WidgetFg + ' !important; border: solid 1px ' + vizzop.WidgetBorder + ' !important;')
            }

            //Bubble Information
            /*
            self._bubble = jVizzop('<span></span>')
                .addClass('zs_bubble')
                */
            self._bubble = jVizzop('<div></div>')
                .addClass('vizzop-popover top in zs_bubble')
                .html('<div class="vizzop-arrow"></div>')
                .appendTo(self._handle)
                .hide();
            self._bubbleinner = jVizzop('<div></div>')
                .addClass('vizzop-popover-inner')
                .appendTo(self._bubble)
            self._bubbletext = jVizzop('<div></div>')
                .addClass('vizzop-popover-content')
                .appendTo(self._bubbleinner);
            self.checkBubble();
            /*<img src="' + vizzop.mainURL + '/Content/images/help_25.png"/>')*/
            self._helpimg = jVizzop('<i class="vizzop-icon-comment"></i>')
                .addClass('zs_hoverbarimg')
                .appendTo(self._handle);
            self._hoverbartext = jVizzop('<span></span>')
                .addClass('zs_hoverbartext')
                .text(LLang('click_support', null))
                .appendTo(self._handle);
            if ((vizzop.WidgetText != '') && (vizzop.WidgetText != null)) {
                self._hoverbartext.text(vizzop.WidgetText);
            }
            self._cleardiv = jVizzop('<div></div>')
                .addClass('zs_dialoghandlepubli')
                .html('powered by <a href="https://vizzop.com" target="blank">vizzop</a>')
                .appendTo(self._handle);

            if (vizzop.ShowHelpButton == true) {
                self.positionBox(function () {
                    self._box.unbind('mouseenter mouseleave click');
                    self._box.click(
                        function () {
                            self.fillBox_helpQuestChat();
                        }
                    );
                    self._box.show();
                }, 'fast');
            }
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    fillBox_LeaveMessage: function (text_leave) {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '420px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_LeaveMessage';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jVizzop('<div></div>')
                .addClass('zs_boxinfocontents form-horizontal')
                .appendTo(self._boxinfo);

            var alert = jVizzop('<div></div')
                .addClass('alert')
                .appendTo(msgbox)
                .hide();

            var alertspan = jVizzop('<span></span')
                .text(LLang('error', null))
                .appendTo(alert);

            var close = jVizzop('<a></a>')
                .addClass('close')
                .attr('data-dismiss', 'alert')
                .text('x')
                .appendTo(alert);


            var Business = jVizzop('<h1></h1>')
                .text(vizzop.webname)
                //.attr('style', 'color: ' + vizzop.WidgetBg + ' !important;')
                .appendTo(msgbox);

            if (text_leave == null) {
                text_leave = LLang('leave_message', null);
            }
            var text = jVizzop('<div></div>')
                .addClass('text-info')
                .html(text_leave)
                .appendTo(msgbox);

            msgbox.append(jVizzop('<hr/>'));

            var textresult = jVizzop('<h3></h3>')
                .appendTo(msgbox);

            var form = jVizzop('<form></form>')
                .attr('id', 'leave_message')
                .attr('name', 'leave_message')
                .css({ 'margin': '0' })
                .appendTo(msgbox);

            var fieldset = jVizzop('<fieldset></fieldset>')
                .appendTo(form);

            var divclear1 = jVizzop('<div/>')
                .appendTo(fieldset);
            var divinput1 = jVizzop('<div/>')
                .addClass('clearfix')
                .appendTo(divclear1);
            var divlabel1 = jVizzop('<label/>')
                .text(LLang('name', null))
                .attr('for', 'leave_message_client_name')
                .appendTo(divinput1);
            var client_name = jVizzop('<input/>')
                .attr('id', 'leave_message_client_name')
                .attr('name', 'leave_message_client_name')
                .attr('type', 'text')
                .attr('required', 'required')
                .attr('maxlength', '25')
                .attr('placeholder', LLang('write_name', null))
                .addClass('input-medium hint')
                .focus(function (event) {
                    var target = jVizzop(event.target);
                    if (target.hasClass('hint') == true) {
                        target.val('');
                        target.removeClass('hint');
                    }
                })
                .appendTo(divinput1);

            var divclear2 = jVizzop('<div/>')
                .appendTo(fieldset);
            var divinput2 = jVizzop('<div/>')
                .addClass('clearfix')
                .appendTo(divclear2);
            var divlabel2 = jVizzop('<label/>')
                .text(LLang('email', null))
                .attr('for', 'leave_message_client_email')
                .appendTo(divinput2);
            var client_email = jVizzop('<input/>')
                .attr('id', 'leave_message_client_email')
                .attr('name', 'leave_message_client_email')
                .attr('type', 'text')
                .attr('required', 'required')
                .attr('maxlength', '50')
                .attr('placeholder', LLang('write_email', null))
                .addClass('input-large hint h5-email')
                .focus(function (event) {
                    var target = jVizzop(event.target);
                    if (target.hasClass('hint') == true) {
                        target.val('');
                        target.removeClass('hint');
                    }
                })
                .blur(function () {
                    /*
                    if (client_email.val() == client_confirm_email.val()) {
                    client_confirm_email.addClass('ui-state-error');
                    }
                    */
                })
                .appendTo(divinput2);

            var divclear3 = jVizzop('<div/>')
                .appendTo(fieldset);
            var divinput3 = jVizzop('<div/>')
                .addClass('clearfix')
                .appendTo(divclear3);
            var divlabel3 = jVizzop('<label/>')
                .text(LLang('repeat_email', null))
                .attr('for', 'leave_message_client_confirmemail')
                .appendTo(divinput3);
            var client_confirm_email = jVizzop('<input/>')
                .attr('id', 'leave_message_client_confirm_email')
                .attr('name', 'leave_message_client_confirm_email')
                .attr('type', 'text')
                .attr('required', 'required')
                .attr('data-equals', 'client_email')
                .attr('maxlength', '50')
                .attr('placeholder', LLang('confirm_email', null))
                .addClass('input-large hint h5-email')
                .focus(function (event) {
                    var target = jVizzop(event.target);
                    if (target.hasClass('hint') == true) {
                        target.val('');
                        target.removeClass('hint');
                    }
                })
                .blur(function () {
                    if (client_email.val() != client_confirm_email.val()) {
                        client_confirm_email.addClass('ui-state-error');
                    }
                })
                .appendTo(divinput3);

            var divclear4 = jVizzop('<div/>')
                .appendTo(fieldset);
            var divinput4 = jVizzop('<div/>')
                .addClass('clearfix')
                .appendTo(divclear4);
            var divlabel4 = jVizzop('<label/>')
                .text(LLang('message', null))
                .attr('for', 'leave_message_client_message')
                .appendTo(divinput4);
            var client_message = jVizzop('<textarea/>')
                .attr('cols', '40')
                .attr('rows', '4')
                .attr('required', 'required')
                .attr('id', 'leave_message_client_message')
                .attr('name', 'leave_message_client_message')
                .attr('placeholder', LLang('client_message', null))
                .addClass('hint')
                .css({ 'width': '370px', 'height': '100px' })
                .focus(function (event) {
                    var target = jVizzop(event.target);
                    if (target.hasClass('hint') == true) {
                        target.val('');
                        target.removeClass('hint');
                    }
                })
                .appendTo(divinput4);

            var divclear5 = jVizzop('<div/>')
                .appendTo(fieldset);
            var divinput5 = jVizzop('<div/>')
                .addClass('actions')
                .appendTo(divclear5);
            var divlabel5 = jVizzop('<label/>')
                .attr('for', 'client_message')
                .appendTo(divinput5);

            var buttoninputHelp = jVizzop('<button></button>')
                .text(LLang('send', null))
                .addClass('vizzop-btn vizzop-btn-primary vizzop-btn-large')
                .attr('type', 'submit')
                .click(function (event) {
                    try {
                        event.preventDefault();
                        client_name.trigger('validate');
                        client_email.trigger('validate');
                        client_confirm_email.trigger('validate');
                        client_message.trigger('validate');
                        if (client_email.val() != client_confirm_email.val()) {
                            client_confirm_email.addClass('ui-state-error');
                        }
                        if (jVizzop("#leave_message .ui-state-error").length == 0) {
                            jVizzop(buttoninputHelp)
                            .addClass('disabled')
                            .hide();
                            jVizzop(loading).show();

                            var data_to_send = {
                                'apikey': vizzop.ApiKey,
                                'FullName': client_name.val(),
                                'email': client_email.val(),
                                'content': client_message.val()
                            };
                            var request = jVizzop.ajax({
                                url: vizzop.mainURL + "/Comm/RequestcommsessionidWithMessage",
                                type: "POST",
                                data: data_to_send,
                                dataType: "jsonp",
                                success: function (data) {
                                    //vizzoplib.log(self._status);
                                    if (data == null) {
                                        jVizzop(loading).hide();
                                        jVizzop(buttoninputHelp).show();
                                        jVizzop(alert).show();
                                        jVizzop(_okbutton).show();
                                        self._preferredwidth = '300px';
                                        self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
                                        return;
                                    } else {
                                        if (data == false) {
                                            jVizzop(loading).hide();
                                            jVizzop(buttoninputHelp).show();
                                            jVizzop(alert).show();
                                        } else {
                                            jVizzop(divclear1).hide();
                                            jVizzop(divclear2).hide();
                                            jVizzop(divclear3).hide();
                                            jVizzop(divclear4).hide();
                                            jVizzop(divclear5).hide();
                                            jVizzop(text).hide();
                                            jVizzop(loading).hide();
                                            jVizzop(textresult).text(LLang('support_message_sent', data));
                                            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
                                        }
                                    }
                                }
                            });
                            request.fail(function (jqXHR, textStatus) {
                                vizzoplib.log("Error RequestcommsessionidWithMessage: " + textStatus);
                                jVizzop(loading).hide();
                                jVizzop(buttoninputHelp).show();
                                jVizzop(alert).show();
                                self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
                            });

                        }
                    } catch (err) {
                        vizzoplib.log("Error RequestcommsessionidWithMessage: " + err);
                        jVizzop(loading).hide();
                        jVizzop(buttoninputHelp).show();
                        jVizzop(alert).show();
                    }
                    return false;
                })
                .appendTo(divinput5);

            var loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
                .appendTo(divinput5)
                .hide();

            var _okbutton = jVizzop('<button></button>')
                .text(LLang('ok', null))
                .addClass('vizzop-btn vizzop-btn-primary')
                .click(function (event) {
                    self.fillBox_helpStandby();
                    //self.destroyBox();
                    return false;
                })
                .appendTo(msgbox)
                .hide();

            var clear = jVizzop('<div></div>')
                .text('')
                .css('clear', 'both')
                .appendTo(msgbox);

            form.h5Validate();

            self._closebutton
                    .click(function (event) {
                        self.fillBox_helpStandby();
                        //self.destroyBox();
                        return false;
                    });

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    fillBox_helpQuestChat: function () {
        var self = this;

        try {

            //SSi no hay agentes logged... que deje el mensaje solo
            if (vizzop.AllowChat == false) {
                self.fillBox_LeaveMessage();
                return false;
            }

            self._preferredubication = 'center';
            self._preferredwidth = '420px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_helpQuestChat';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jVizzop('<div></div>')
                .addClass('zs_boxinfocontents')
                .appendTo(self._boxinfo);

            var Business = jVizzop('<h1></h1>')
                .text(vizzop.webname)
                //.attr('style', 'color: ' + vizzop.WidgetBg + ' !important;')
                .appendTo(msgbox);

            var text = jVizzop('<h3></h3>')
                .text(LLang('help_method_question', null))
                .appendTo(msgbox);

            msgbox.append(jVizzop('<br/>'));


            var opt_A = jVizzop('<span></span>')
                .text(LLang('a_option', null))
                .addClass('option')
                .appendTo(msgbox);

            var opt_A_contents = jVizzop('<span></span>')
                .addClass('optioncontents')
                .appendTo(msgbox);

            var buttonChat = jVizzop('<button></button>')
                .text(LLang('start_chat', null))
                .addClass('vizzop-btn vizzop-btn-primary vizzop-btn-large')
                .click(function (event) {
                    var name_mecookie = vizzop.ApiKey + "_me";
                    vizzoplib.deleteCookie(name_mecookie);
                    vizzop.me.FullName = null;
                    self._commsessionid = null;
                    if (vizzop.me.FullName == null) {
                        self.fillBox_FindingSupport_WithoutName();
                    } else {
                        self.fillBox_FindingSupport_WithName();
                    }
                    return false;
                })
                .appendTo(opt_A_contents);

            msgbox.append(jVizzop('<hr/>'));

            var opt_B = jVizzop('<span></span>')
                .text(LLang('b_option', null))
                .addClass('option')
                .appendTo(msgbox);

            var opt_B_contents = jVizzop('<span></span>')
                .addClass('optioncontents')
                .appendTo(msgbox);

            var buttoninputHelp = jVizzop('<button></button>')
                .text(LLang('send_message', null))
                .addClass('vizzop-btn vizzop-btn-primary vizzop-btn-large')
                .css({ 'margin-top': '5px' })
                .click(function (event) {
                    self.fillBox_LeaveMessage();
                    return false;
                })
                .appendTo(opt_B_contents);

            self._closebutton
                    .click(function (event) {
                        self.fillBox_helpStandby();
                        /*self.destroyBox();*/
                        return false;
                    })

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');

        } catch (err) {
            vizzoplib.log(err);
        }
    }
});
