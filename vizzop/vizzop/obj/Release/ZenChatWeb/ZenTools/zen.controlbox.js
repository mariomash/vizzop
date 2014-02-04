
var ChatListBox = jQuery.zs_Class.create(Box, {
    initialize: function (div_to_stack_on) {
        var self = this;
        try {
            self.base('initialize');
            self._type = 'ChatListBox';
            self._preferredstacked = div_to_stack_on;
            self.fillBox_ChatList();
        } catch (err) {
            log(err);
        }
    },
    fillBox_ChatList: function () {
        var self = this;
        try {
            self._preferredubication = 'right';
            self._preferredwidth = '250px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_ChatList';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = true;
            jQuery(self._box).addClass('chatlistBox');
            jQuery(self._closebutton).hide();

            var msgbox = jQuery('<div></div>')
					.addClass('boxinfocontents')
					.appendTo(self._boxinfo);

            self._boxList = jQuery('<div></div>')
					.appendTo(msgbox);

            self.positionBox(null, 'fast');

            /*
            jQuery(self._box).css({
            'width': (jQuery(self._box).outerWidth() - jQuery(self._box._boxtitle).outerWidth()) + 'px'
            });
            jQuery(window).unbind('resize.chatlistBox');
            jQuery(window).bind('resize.chatlistBox', function () {
            jQuery(self._box).css({
            'width': (jQuery(self._box).outerWidth() - jQuery(self._box._boxtitle).outerWidth()) + 'px'
            });
            });
            */
        } catch (err) {
            log(err);
        }
    }
});

var ControlBox = jQuery.zs_Class.create(Box, {
    // constructor
    initialize: function () {
        var self = this;
        try {
            self.base('initialize');
            self._type = 'ControlBox';
            self.fillBox_Control();
        } catch (err) {
            log(err);
        }
    },
    // methods
    fillBox_Control: function () {
        try {
            var self = this;
            self._preferredubication = 'bottomleft';
            self._preferredwidth = '100%';
            //self._preferredheight = '190px';
            /*
            self._preferredheight = 'auto';
            */
            self._status = 'fillBox_Control';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = true;
            jQuery(self._box).addClass('controlBox');
            jQuery(self._closebutton).hide();
            jQuery(self._box).unbind('click');
            jQuery(self._box).draggable("destroy");

            var msgbox = jQuery('<div></div>')
					.addClass('boxinfocontents')
					.appendTo(self._boxinfo);

            self._boxList = jQuery('<div></div>')
					.appendTo(msgbox);

            self.positionBox(null, 'fast');

            jQuery(self._box).css({
                'width': (jQuery(self._box).outerWidth() - jQuery(self._box._boxtitle).outerWidth()) + 'px'
            });
            jQuery(window).unbind('resize.controlBox');
            jQuery(window).bind('resize.controlBox', function () {
                jQuery(self._box).css({
                    'width': (jQuery(self._box).outerWidth() - jQuery(self._box._boxtitle).outerWidth()) + 'px'
                });
            });

        } catch (err) {
            log(err);
        }
    }
},
	{
	    // properties
	    getset: [
			['Box', '_box']
		]
	});
