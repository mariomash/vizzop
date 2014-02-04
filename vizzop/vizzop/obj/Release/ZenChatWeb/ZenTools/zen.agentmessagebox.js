
var AgentMessageBox = jQuery.zs_Class.create(MessageBox, {
    // constructor
    initialize: function () {
        var self = this;
        this.base('initialize');
        //self.fillBox_helpYesNo();
    },
    fillBox_SupportYesNo: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '380px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_SupportYesNo';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            var msgbox = jQuery('<div></div>')
                .addClass('boxinfocontents')
				.appendTo(self._boxinfo);

            //self._interlocutor.UserName
            self._text = jQuery('<h5></h5>')
				.text(LLang('support_question', null))
				.appendTo(msgbox);

            msgbox.append(jQuery('<br/>'));

            self.loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
					.css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
					.appendTo(msgbox)
					.hide();

            self.buttonYes = jQuery('<button></button>')
				.text(LLang('yes', null))
				.addClass('btn btn-primary')
				.click(function (event) {
				    self.buttonYes.hide();
				    self.buttonNo.hide();
				    self.loading.show();
				    self.approveCommRequest();
				    return false;
				})
				.appendTo(msgbox);

            self.buttonNo = jQuery('<button></button>')
				.text(LLang('no', null))
				.addClass('btn')
				.click(function (event) {
				    self.buttonYes.hide();
				    self.buttonNo.hide();
				    self.loading.show();
				    self.denyCommRequest();
				    return false;
				})
				.appendTo(msgbox);

            self._closebutton.hide();

            self.positionBox(null, 'fast');

        } catch (err) {
            log(err);
        }
    },
    approveCommRequest: function () {
        var self = this;
        var msg = {
            'commsessionid': self._commsessionid,
            'username': zentools.me.UserName,
            'password': zentools.me.Password,
            'domain': zentools.me.Business.Domain
        };
        var request = jQuery.ajax({
            url: zentools.mainURL + "/Comm/ApproveCommRequest",
            type: "POST",
            data: msg,
            dataType: "jsonp",
            success: function (data) {
                //log(data);
                if (data === true) {
                    self.start_TextChat();
                    zentools.CommRequest_InCourse = null;
                    //log(zentools.CommRequest_InCourse);
                } else {
                    //jQuery(self._box).remove();
                    self._text.text(LLang('already_approved', null));
                    self.buttonYes
                        .text(LLang('ok', null))
                        .show();
                    self.buttonNo.hide();
                    self.loading.hide();
                    zentools.CommRequest_InCourse = null;
                }
            }
        });
        request.fail(function (jqXHR, textStatus) {
            log("Error approveCommRequest: " + textStatus);
            self.buttonYes.show();
            self.buttonNo.show();
            self.loading.hide();
            zentools.CommRequest_InCourse = null;
        });
    },
    denyCommRequest: function () {
        var self = this;
        var msg = {
            'commsessionid': self._commsessionid,
            'username': zentools.me.UserName,
            'password': zentools.me.Password,
            'domain': zentools.me.Business.Domain
        };
        var request = jQuery.ajax({
            url: zentools.mainURL + "/Comm/DenyCommRequest",
            type: "POST",
            data: msg,
            dataType: "jsonp"
        });
        request.done(function (msg) {
            jQuery(self._box).remove();
            zentools.CommRequest_InCourse = null;
        });
        request.fail(function (jqXHR, textStatus) {
            log("Error denyCommRequest: " + textStatus);
            jQuery(self._box).remove();
            zentools.CommRequest_InCourse = null;
        });
    }
});
