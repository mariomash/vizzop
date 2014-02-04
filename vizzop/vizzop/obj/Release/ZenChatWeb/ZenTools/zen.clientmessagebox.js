
var ClientMessageBox = jQuery.zs_Class.create(MessageBox, {
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
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jQuery('<div></div>')
                .addClass('boxinfocontents')
                .appendTo(self._boxinfo);

            var text = jQuery('<p></p>')
                .text(LLang('name_question', null))
                .appendTo(msgbox);

            msgbox.append(jQuery('<br/>'));

            var divclear1 = jQuery('<div/>')
                .appendTo(msgbox);
            var divinput1 = jQuery('<div/>')
                .addClass(
                'input'
                )
                .appendTo(divclear1);

            jQuery(self).bind('ConverserUpdateNameFinished');
            jQuery(self).bind('ConverserUpdateNameFinished', function (e, value) {
                if (value === null) {
                    self.fillBox_FindingSupport_WithoutName();
                } else {
                    zentools.me = value;
                    var name_mecookie = zentools.ApiKey + "_me";
                    var name_comsessionidcookie = zentools.ApiKey + "_commsessionid";
                    setCookie(name_mecookie, jQuery.toJSON(zentools.me), 1);
                    self.fillBox_FindingSupport_WithName();
                }
            });

            var inputHelp = jQuery('<input/>')
                .attr('id', 'client_name')
                .attr('name', 'client_name')
                .attr('type', 'text')
                .attr('maxlength', '25')
                .attr('placeholder', LLang('write_name', null))
                .addClass(
                'input-medium hint'
            )
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
                            if (jQuery(inputHelp).val() !== "") {
                                jQuery(inputHelp)
                                    .addClass('disabled')
                                    .attr('disabled', 'disabled');
                                jQuery(buttoninputHelp)
                                    .addClass('disabled')
                                    .hide();
                                jQuery(loading).show();
                                self.ConverserUpdateName(jQuery(inputHelp).val());
                            }
                        }
                    } catch (err) {
                        log(err);
                    }
                })
                .appendTo(divinput1);

            var buttoninputHelp = jQuery('<button></button>')
                .text(LLang('ok', null))
                .addClass('btn btn-primary')
                .click(function (event) {
                    try {
                        if (jQuery(inputHelp).val() !== "") {
                            jQuery(inputHelp)
                                .addClass('disabled')
                                .attr('disabled', 'disabled');
                            jQuery(buttoninputHelp)
                                .addClass('disabled')
                                .hide();
                            jQuery(loading).show();
                            self.ConverserUpdateName(jQuery(inputHelp).val());
                        }
                    } catch (err) {
                        log(err);
                    }
                    return false;
                })
                .appendTo(divinput1);

            var loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
                .appendTo(divinput1)
                .hide();

            self.positionBox(null, 'fast');
        } catch (err) {
            log(err);
        }
    },
    fillBox_FindingSupport_WithName: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_FindingSupport_WithName';
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
                .text(LLang('waiting_support', [zentools.me.FullName]))
                .appendTo(msgbox);

            var loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px' })
                .appendTo(msgbox);

            self.positionBox(null, 'fast');

            zentools.Daemon.Requestcommsessionid();

        } catch (err) {
            log(err);
        }
    },
    fillBox_helpYesNo: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_helpYesNo';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            var msgbox = jQuery('<div></div>')
                .addClass('boxinfocontents')
                .appendTo(self._boxinfo);

            var text = jQuery('<h5></h5>')
                .text(LLang('can_we_assist', null))
                .appendTo(msgbox);

            msgbox.append(jQuery('<br/>'));

            var buttonYes = jQuery('<button></button>')
                .text(LLang('yes', null))
                .addClass('btn btn-primary')
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

            var buttonNo = jQuery('<button></button>')
                .text(LLang('no', null))
                .addClass('btn')
                .click(function (event) {
                    self.fillBox_helpStandby();
                    return false;
                })
                .appendTo(msgbox);

        } catch (err) {
            log(err);
        }
    },
    fillBox_helpStandby: function () {
        try {
            var self = this;
            self._preferredubication = 'standbychat';
            self._preferredwidth = 'auto';
            self._preferredheight = 'auto';
            self._status = 'fillBox_helpStandby';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self.hideBox();
            self._boxinfo.empty();
            var box = self._box;
            // No va a la lista de boxes..
            self._nolist = true;
            self._boxinner.hide();
            //Handle Bar
            self._handle = jQuery('<span></span>')
                .addClass('zs_dialoghandle')
                .appendTo(box);
            /*<img src="' + zentools.mainURL + '/Content/images/help_25.png"/>')*/
            self._helpimg = jQuery('<i class="icon-comment icon-white"></i>')
                .addClass('zs_hoverbarimg')
                .appendTo(self._handle);
            self._hoverbartext = jQuery('<span></span>')
                .addClass('zs_hoverbartext')
                .text(LLang('click_support', null))
                .appendTo(self._handle);

            if (zentools.ShowHelpButton === true) {
                self.positionBox(function () {
                    self._box.unbind('mouseenter mouseleave click');
                    /*
                    self._box.hover(
                    function () {
                    self.fillBox_helpQuestChat();
                    },
                    function () {
                    //self.fillBox_helpStandby();
                    }
                    );
                    */
                    self._box.click(
                        function () {
                            self.fillBox_helpQuestChat();
                        }
                    );
                }, 'fast');
            }
        } catch (err) {
            log(err);
        }
    },
    fillBox_LeaveMessage: function (text_leave) {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '580px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_LeaveMessage';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jQuery('<div></div>')
                .addClass('boxinfocontents form-horizontal')
                .appendTo(self._boxinfo);

            var alert = jQuery('<div></div')
                .addClass('alert')
                .appendTo(msgbox)
                .hide();

            var alertspan = jQuery('<span></span')
                .text(LLang('error', null))
                .appendTo(alert);

            var close = jQuery('<a></a>')
                .addClass('close')
                .attr('data-dismiss', 'alert')
                .text('x')
                .appendTo(alert);

            if (text_leave === null) {
                text_leave = LLang('leave_message', null);
            }
            var text = jQuery('<p></p>')
                .html(text_leave)
                .appendTo(msgbox);

            msgbox.append(jQuery('<br/>'));

            var form = jQuery('<form></form>')
                .attr('id', 'leave_message')
                .attr('name', 'leave_message')
                .css({ 'margin': '0' })
                .appendTo(msgbox);

            var fieldset = jQuery('<fieldset></fieldset>')
                .appendTo(form);

            var divclear1 = jQuery('<div/>')
                .appendTo(fieldset);
            var divinput1 = jQuery('<div/>')
                .addClass('clearfix')
                .appendTo(divclear1);
            var divlabel1 = jQuery('<label/>')
                .text(LLang('name', null))
                .attr('for', 'leave_message_client_name')
                .appendTo(divinput1);
            var client_name = jQuery('<input/>')
                .attr('id', 'leave_message_client_name')
                .attr('name', 'leave_message_client_name')
                .attr('type', 'text')
                .attr('required', 'required')
                .attr('maxlength', '25')
                .attr('placeholder', LLang('write_name', null))
                .addClass('input-medium hint')
                .focus(function (event) {
                    var target = jQuery(event.target);
                    if (target.hasClass('hint') === true) {
                        target.val('');
                        target.removeClass('hint');
                    }
                })
                .appendTo(divinput1);

            var divclear2 = jQuery('<div/>')
                .appendTo(fieldset);
            var divinput2 = jQuery('<div/>')
                .addClass('clearfix')
                .appendTo(divclear2);
            var divlabel2 = jQuery('<label/>')
                .text(LLang('email', null))
                .attr('for', 'leave_message_client_email')
                .appendTo(divinput2);
            var client_email = jQuery('<input/>')
                .attr('id', 'leave_message_client_email')
                .attr('name', 'leave_message_client_email')
                .attr('type', 'text')
                .attr('required', 'required')
                .attr('maxlength', '50')
                .attr('placeholder', LLang('write_email', null))
                .addClass('input-large hint h5-email')
                .focus(function (event) {
                    var target = jQuery(event.target);
                    if (target.hasClass('hint') === true) {
                        target.val('');
                        target.removeClass('hint');
                    }
                })
                .blur(function () {
                    /*
                    if (client_email.val() === client_confirm_email.val()) {
                    client_confirm_email.addClass('ui-state-error');
                    }
                    */
                })
                .appendTo(divinput2);

            var divclear3 = jQuery('<div/>')
                .appendTo(fieldset);
            var divinput3 = jQuery('<div/>')
                .addClass('clearfix')
                .appendTo(divclear3);
            var divlabel3 = jQuery('<label/>')
                .text(LLang('repeat_email', null))
                .attr('for', 'leave_message_client_confirmemail')
                .appendTo(divinput3);
            var client_confirm_email = jQuery('<input/>')
                .attr('id', 'leave_message_client_confirm_email')
                .attr('name', 'leave_message_client_confirm_email')
                .attr('type', 'text')
                .attr('required', 'required')
                .attr('data-equals', 'client_email')
                .attr('maxlength', '50')
                .attr('placeholder', LLang('confirm_email', null))
                .addClass('input-large hint h5-email')
                .focus(function (event) {
                    var target = jQuery(event.target);
                    if (target.hasClass('hint') === true) {
                        target.val('');
                        target.removeClass('hint');
                    }
                })
                .blur(function () {
                    if (client_email.val() !== client_confirm_email.val()) {
                        client_confirm_email.addClass('ui-state-error');
                    }
                })
                .appendTo(divinput3);

            var divclear4 = jQuery('<div/>')
                .appendTo(fieldset);
            var divinput4 = jQuery('<div/>')
                .addClass('clearfix')
                .appendTo(divclear4);
            var divlabel4 = jQuery('<label/>')
                .text(LLang('message', null))
                .attr('for', 'leave_message_client_message')
                .appendTo(divinput4);
            var client_message = jQuery('<textarea/>')
                .attr('cols', '40')
                .attr('rows', '4')
                .attr('required', 'required')
                .attr('id', 'leave_message_client_message')
                .attr('name', 'leave_message_client_message')
                .attr('placeholder', LLang('client_message', null))
                .addClass('hint')
                .css({ 'width': '370px', 'height': '100px' })
                .focus(function (event) {
                    var target = jQuery(event.target);
                    if (target.hasClass('hint') === true) {
                        target.val('');
                        target.removeClass('hint');
                    }
                })
                .appendTo(divinput4);

            var divclear5 = jQuery('<div/>')
                .appendTo(fieldset);
            var divinput5 = jQuery('<div/>')
                .addClass('actions')
                .appendTo(divclear5);
            var divlabel5 = jQuery('<label/>')
                .attr('for', 'client_message')
                .appendTo(divinput5);

            var buttoninputHelp = jQuery('<button></button>')
                .text(LLang('send', null))
                .addClass('btn btn-primary btn-large')
                .attr('type', 'submit')
                .click(function (event) {
                    try {
                        event.preventDefault();
                        client_name.trigger('validate');
                        client_email.trigger('validate');
                        client_confirm_email.trigger('validate');
                        client_message.trigger('validate');
                        if (client_email.val() !== client_confirm_email.val()) {
                            client_confirm_email.addClass('ui-state-error');
                        }
                        if (jQuery("#leave_message .ui-state-error").length === 0) {
                            jQuery(buttoninputHelp)
                            .addClass('disabled')
                            .hide();
                            jQuery(loading).show();

                            var data_to_send = {
                                'apikey': zentools.ApiKey,
                                'FullName': client_name.val(),
                                'email': client_email.val(),
                                'content': client_message.val()
                            };
                            var request = jQuery.ajax({
                                url: zentools.mainURL + "/Comm/RequestcommsessionidWithMessage",
                                type: "POST",
                                data: data_to_send,
                                dataType: "jsonp",
                                success: function (data) {
                                    //log(self._status);
                                    if (data === null) {
                                        jQuery(loading).hide();
                                        jQuery(buttoninputHelp).show();
                                        jQuery(alert).show();
                                        jQuery(_okbutton).show();
                                        self._preferredwidth = '300px';
                                        self.positionBox(null, 'fast');
                                        return;
                                    } else {
                                        if (data === false) {
                                            jQuery(loading).hide();
                                            jQuery(buttoninputHelp).show();
                                            jQuery(alert).show();
                                        } else {
                                            jQuery(divclear1).hide();
                                            jQuery(divclear2).hide();
                                            jQuery(divclear3).hide();
                                            jQuery(divclear4).hide();
                                            jQuery(divclear5).hide();
                                            jQuery(loading).hide();
                                            jQuery(text).text(LLang('support_message_sent', data));
                                            self.positionBox(null, 'fast');
                                        }
                                    }
                                }
                            });
                            request.fail(function (jqXHR, textStatus) {
                                log("Error RequestcommsessionidWithMessage: " + textStatus);
                                jQuery(loading).hide();
                                jQuery(buttoninputHelp).show();
                                jQuery(alert).show();
                                self.positionBox(null, 'fast');
                            });

                        }
                    } catch (err) {
                        log("Error RequestcommsessionidWithMessage: " + err);
                        jQuery(loading).hide();
                        jQuery(buttoninputHelp).show();
                        jQuery(alert).show();
                    }
                    return false;
                })
                .appendTo(divinput5);

            var loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
                .css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
                .appendTo(divinput5)
                .hide();

            var _okbutton = jQuery('<button></button>')
                .text(LLang('ok', null))
                .addClass('btn btn-primary')
                .click(function (event) {
                    self.fillBox_helpStandby();
                    //self.destroyBox();
                    return false;
                })
                .appendTo(msgbox)
                .hide();

            form.h5Validate();

            self._closebutton
                    .click(function (event) {
                        self.fillBox_helpStandby();
                        //self.destroyBox();
                        return false;
                    });

            self.positionBox(null, 'fast');
        } catch (err) {
            log(err);
        }
    },
    fillBox_helpQuestChat: function () {
        var self = this;

        try {

            //SSi no hay agentes logged... que deje el mensaje solo
            if (zentools.AllowChat === false) {
                self.fillBox_LeaveMessage();
                return false;
            }

            self._preferredubication = 'center';
            self._preferredwidth = '420px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_helpQuestChat';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jQuery('<div></div>')
                .addClass('boxinfocontents')
                .appendTo(self._boxinfo);

            var text = jQuery('<h5></h5>')
                .text(LLang('help_method_question', null))
                .appendTo(msgbox);

            msgbox.append(jQuery('<br/>'));


            var opt_A = jQuery('<span></span>')
                .text(LLang('a_option', null))
                .addClass('option')
                .appendTo(msgbox);

            var opt_A_contents = jQuery('<span></span>')
                .addClass('optioncontents')
                .appendTo(msgbox);

            var buttonChat = jQuery('<button></button>')
                .text(LLang('start_chat', null))
                .addClass('btn btn-primary')
                .click(function (event) {
                    if (zentools.me.FullName === null) {
                        self.fillBox_FindingSupport_WithoutName();
                    } else {
                        self.fillBox_FindingSupport_WithName();
                    }
                    return false;
                })
                .appendTo(opt_A_contents);

            msgbox.append(jQuery('<hr/>'));

            var opt_B = jQuery('<span></span>')
                .text(LLang('b_option', null))
                .addClass('option')
                .appendTo(msgbox);

            var opt_B_contents = jQuery('<span></span>')
                .addClass('optioncontents')
                .appendTo(msgbox);

            var buttoninputHelp = jQuery('<button></button>')
                .text(LLang('send_message', null))
                .addClass('btn btn-primary')
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

            self.positionBox(null, 'fast');
            self.positionBox(null, 'fast');

        } catch (err) {
            log(err);
        }
    }
});
