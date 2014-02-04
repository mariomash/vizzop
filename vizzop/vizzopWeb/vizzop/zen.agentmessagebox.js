var AgentMessageBox = jVizzop.zs_Class.create(MessageBox, {
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
			try { jVizzop(self._handle).remove(); } catch (err) { }
			self._boxinner.show();
			self.hideBox();
			self._boxinfo.empty();

			var msgbox = jVizzop('<div></div>')
				.addClass('zs_boxinfocontents')
				.appendTo(self._boxinfo);

			//self._interlocutor.UserName
			self._text = jVizzop('<h5></h5>')
				.text(LLang('support_question', null))
				.appendTo(msgbox);

			msgbox.append(jVizzop('<br/>'));

			self.loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
					.css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
					.appendTo(msgbox)
					.hide();

			self.buttonYes = jVizzop('<button></button>')
				.text(LLang('yes', null))
				.addClass('btn btn-primary')
				.click(function (event) {
					vizzop.Daemon.audioRinging.Stop();
					self.buttonYes.hide();
					self.buttonNo.hide();
					self.loading.show();
					self.approveCommRequest();
					return false;
				})
				.appendTo(msgbox);

			self.buttonNo = jVizzop('<button></button>')
				.text(LLang('no', null))
				.addClass('btn')
				.click(function (event) {
					vizzop.Daemon.audioRinging.Stop();
					self.buttonYes.hide();
					self.buttonNo.hide();
					self.loading.show();
					self.denyCommRequest();
					return false;
				})
				.appendTo(msgbox);

			self._closebutton.hide();

			self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');

			vizzop.Daemon.audioRinging.Play();
			vizzop.Daemon.blinkFavIcon();

		} catch (err) {
			vizzoplib.log(err);
		}
	},
	approveCommRequest: function () {
		var self = this;
		var msg = {
			'commsessionid': self._commsessionid,
			'username': vizzop.me.UserName,
			'password': vizzop.me.Password,
			'domain': vizzop.me.Business.Domain
		};
		var url = vizzop.mainURL + "/Comm/ApproveCommRequest";
		var request = jVizzop.ajax({
			url: url,
			type: "POST",
			data: msg,
			dataType: "jsonp",
			success: function (data) {
				//vizzoplib.log(data);
				if (data == true) {
					self.start_TextChat();
					self.start_Screenshare();
					//self.fillBox_RequestScreenView();
					vizzop.CommRequest_InCourse = null;
					//vizzoplib.log(vizzop.CommRequest_InCourse);
				} else {
					//jVizzop(self._box).remove();
					self._text.text(LLang('already_approved', null));
					self.buttonYes
						.text(LLang('ok', null))
						.show()
						.click(function (event) {
							vizzop.Daemon.audioRinging.Stop();
							self.destroyBox();
							return false;
						});
					self.buttonNo.hide();
					self.loading.hide();
					vizzop.CommRequest_InCourse = null;
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				vizzoplib.logAjax(url, msg, jqXHR);
				self.buttonYes.show();
				self.buttonNo.show();
				self.loading.hide();
				vizzop.CommRequest_InCourse = null;
			}
		});
	},
	denyCommRequest: function () {
		var self = this;
		var msg = {
			'commsessionid': self._commsessionid,
			'username': vizzop.me.UserName,
			'password': vizzop.me.Password,
			'domain': vizzop.me.Business.Domain
		};
		var url = vizzop.mainURL + "/Comm/DenyCommRequest";
		var request = jVizzop.ajax({
			url: url,
			type: "POST",
			data: msg,
			dataType: "jsonp",
			success: function (data) {
				self.destroyBox();
				vizzop.CommRequest_InCourse = null;
			},
			error: function (jqXHR, textStatus, errorThrown) {
				vizzoplib.logAjax(url, msg, jqXHR);
				self.destroyBox();
				vizzop.CommRequest_InCourse = null;
			}
		});
	}
});
