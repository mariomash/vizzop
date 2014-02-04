
var ChatListBox = jVizzop.zs_Class.create(Box, {
    initialize: function (div_to_stack_on) {
        var self = this;
        try {
            self.base('initialize');
            self._type = 'ChatListBox';
            self._preferredstacked = div_to_stack_on;
            self.fillBox_ChatList();
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    fillBox_ChatList: function () {
        var self = this;
        try {
            self._preferredubication = 'right';
            self._preferredwidth = '250px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_ChatList';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = true;
            jVizzop(self._box).addClass('chatlistBox');
            jVizzop(self._closebutton).hide();


            self._boxinfo.attr('style', '');


            var msgbox = jVizzop('<div></div>')
					.addClass('zs_boxinfocontents')
					.appendTo(self._boxinfo);

            self._boxList = jVizzop('<div></div>')
					.appendTo(msgbox);

            self.positionBox(null, 'fast');

            /*
            jVizzop(self._box).css({
            'width': (jVizzop(self._box).outerWidth() - jVizzop(self._box._boxtitle).outerWidth()) + 'px'
            });
            jVizzop(window).unbind('resize.chatlistBox');
            jVizzop(window).bind('resize.chatlistBox', function () {
            jVizzop(self._box).css({
            'width': (jVizzop(self._box).outerWidth() - jVizzop(self._box._boxtitle).outerWidth()) + 'px'
            });
            });
            */
        } catch (err) {
            vizzoplib.log(err);
        }
    }
});

var ControlBox = jVizzop.zs_Class.create(Box, {
    // constructor
    initialize: function () {
        var self = this;
        try {
            self.base('initialize');
            self._type = 'ControlBox';
            self.fillBox_Control();
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    // methods
    fillBox_Control: function () {
        try {
            var self = this;
            self._preferredubication = 'bottom';
            self._preferredwidth = '100%';
            //self._preferredheight = '190px';
            /*
            self._preferredheight = 'auto';
            */
            self._status = 'fillBox_Control';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = true;
            jVizzop(self._box).addClass('controlBox');
            jVizzop(self._closebutton).hide();

            self._boxinfo.attr('style', '');

            jVizzop(self._box).unbind('click');
            jVizzop(self._box).draggable("destroy");

            var msgbox = jVizzop('<div></div>')
					.addClass('zs_boxinfocontents')
					.appendTo(self._boxinfo);

            self._boxList = jVizzop('<div></div>')
					.appendTo(msgbox);

            self.positionBox(null, 'fast');
            /*
            jVizzop(self._box).css({
                'width': (jVizzop(self._box).outerWidth() - jVizzop(self._box._boxtitle).outerWidth()) + 'px'
            });
            jVizzop(window).unbind('resize.controlBox');
            jVizzop(window).bind('resize.controlBox', function () {
                jVizzop(self._box).css({
                    'width': (jVizzop(self._box).outerWidth() - jVizzop(self._box._boxtitle).outerWidth()) + 'px'
                });
            });*/
        } catch (err) {
            vizzoplib.log(err);
        }
    }
},
	{
	    // properties
	    getset: [
			['Box', '_box']
	    ]
	});
