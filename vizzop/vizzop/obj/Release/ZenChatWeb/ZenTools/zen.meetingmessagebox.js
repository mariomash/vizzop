
var MeetingMessageBox = jQuery.zs_Class.create(MessageBox, {
    // constructor
    initialize: function () {
        var self = this;
        self.base('initialize');
        if (zentools.me.FullName === null) {
            self.fillBox_LoadingMeeting_WithoutName();
        } else {
            self.fillBox_LoadingMeeting_WithName();
        }
    },
    fillBox_LoadingMeeting_WithoutName: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_FindingMeeting_WithoutName';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = false;

            var msgbox = jQuery('<div></div>')
				.addClass('boxinfocontents')
				.appendTo(self._boxinfo);

            var text = jQuery('<p></p>')
				.text(LLang('meeting_name_question', null))
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
                    self.fillBox_LoadingMeeting_WithoutName();
                } else {
                    zentools.me = value;
                    var name_mecookie = zentools.ApiKey + "_me";
                    setCookie(name_mecookie, jQuery.toJSON(zentools.me), 1);
                    self.fillBox_LoadingMeeting_WithName();
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
				            //self.ConverserUpdateName(jQuery(inputHelp).val());
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
    fillBox_LoadingMeeting_WithName: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_FindingMeeting_WithName';
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
				.text(LLang('waiting_meeting', [zentools.me.FullName]))
				.appendTo(msgbox);

            var loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
				.css({ 'margin': '8px 5px' })
				.appendTo(msgbox);

            self.positionBox(null, 'fast');

            //zentools.Daemon.Requestcommsessionid();

        } catch (err) {
            log(err);
        }
    }
});
