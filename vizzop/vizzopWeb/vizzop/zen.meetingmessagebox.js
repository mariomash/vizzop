
var MeetingMessageBox = jVizzop.zs_Class.create(MessageBox, {
	// constructor
	initialize: function () {
		var self = this;
		self.base('initialize');
		if (vizzop.me.FullName == null) {
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
			try { jVizzop(self._handle).remove(); } catch (err) { }
			self._boxinner.show();
			self.hideBox();
			self._boxinfo.empty();

			self._nolist = false;

			var msgbox = jVizzop('<div></div>')
				.addClass('zs_boxinfocontents')
				.appendTo(self._boxinfo);

			var text = jVizzop('<p></p>')
				.text(LLang('meeting_name_question', null))
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
					self.fillBox_LoadingMeeting_WithoutName();
				} else {
					vizzop.me = value;
					var name_mecookie = vizzop.ApiKey + "_me";
					vizzoplib.setCookie(name_mecookie, jVizzop.toJSON(vizzop.me), 1);
					self.fillBox_LoadingMeeting_WithName();
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
							//self.ConverserUpdateName(jVizzop(inputHelp).val());
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

			self.positionBox(function() { jVizzop(self._box).show(); }, 'fast');
		} catch (err) {
			vizzoplib.log(err);
		}
	},
	fillBox_LoadingMeeting_WithName: function () {
		try {
			var self = this;
			self._preferredubication = 'center';
			self._preferredwidth = '380px';
			self._preferredheight = 'auto';
			self._status = 'fillBox_FindingMeeting_WithName';
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
				.text(LLang('waiting_meeting', [vizzop.me.FullName]))
				.appendTo(msgbox);

			var loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
				.css({ 'margin': '8px 5px' })
				.appendTo(msgbox);

			self.positionBox(function() { jVizzop(self._box).show(); }, 'fast');

			//vizzop.Daemon.Requestcommsessionid();

		} catch (err) {
			vizzoplib.log(err);
		}
	}
});
