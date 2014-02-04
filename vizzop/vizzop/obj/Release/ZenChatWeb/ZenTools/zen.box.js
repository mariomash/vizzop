
var Box = jQuery.zs_Class.create({
    // constructor
    initialize: function () {
        var self = this;
        self.createBox();
        self._type = 'Box';
        zentools.Boxes.push(this);
        if (zentools.Daemon !== null) {
            zentools.Daemon.sync_ControlList();
        }
    },
    createBox: function () {
        try {
            var self = this;

            self._preferredubication = 'center';
            self._preferredpinned = true;
            self._preferredstacked = null;
            var randomid = "box_" + randomnumber();
            while (jQuery.each(zentools.Boxes, function (index, _box) {
                if (jQuery(_box).attr('id') === randomid) {
                    return true;
            }
            }) === true) {
                randomid = "box_" + randomnumber;
            }

            self._id = randomid;

            //Por defecto se muestra...
            self._nolist === false;

            self._box = jQuery('<div></div>')
					.addClass('zs_dialog')
					.attr('id', randomid)
					.click(function (event) {
					    self.bringtofrontBox();
					})
					.appendTo(jQuery('body'));

            //Inner Box
            self._boxinner = jQuery('<div></div>')
					.addClass('zs_dialog_inner')
					.appendTo(self._box);

            self._title = LLang('sup_box_title', [zentools.webname]);

            //Title Bar
            self._boxtitle = jQuery('<div></div>')
					.addClass('zs_dialog_title')
					.appendTo(self._boxinner);
            self._boxtitletext = jQuery('<span></span>')
					.addClass('title')
					.text(LLang('sup_box_title', [zentools.webname]))
					.appendTo(self._boxtitle);
            self._closebutton = jQuery('<span></span>')
					.addClass('zs_dialogclosebutton')
					.html('<i class="icon-remove"></i>')
					.click(function (event) {
					    //self.destroyBox();
					    return false;
					})
                    .hover(
                    function (event) {
                        jQuery(this).html('<i class="icon-remove icon-white"></i>');
                    },
                    function (event) {
                        jQuery(this).html('<i class="icon-remove"></i>');
                    }
                    )
					.appendTo(self._boxtitle);

            self._togglebarbutton = jQuery('<span></span>')
					.addClass('zs_dialogtogglebarbutton')
					.html('<i class="icon-minus"></i>')
					.click(function (event) {
					    jQuery(self._boxinfo).toggle();
					    return false;
					})
                    .hover(
                    function (event) {
                        jQuery(this).html('<i class="icon-minus icon-white"></i>');
                    },
                    function (event) {
                        jQuery(this).html('<i class="icon-minus"></i>');
                    }
                    )
					.appendTo(self._boxtitle);

            //Info Box
            self._boxinfo = jQuery('<div></div>')
					.addClass('boxinfo')
					.appendTo(self._boxinner);

            //Statusbar
            self._boxstatus = jQuery('<div></div>')
                .addClass('boxstatus')
                .appendTo(self._boxinner);


            //events and so..

            self._boxtitle.click(function (event) {
                self.bringtofrontBox();
            });

            self._boxtitle.dblclick(function (event) {
                jQuery(self._boxinfo).toggle();
                jQuery(self._boxstatus).toggle();
            });

            self._boxinfo.click(function (event) {
                self.bringtofrontBox();
            });

            self._boxstatus.click(function (event) {
                self.bringtofrontBox();
            });

            self._box.draggable({
                handle: self._boxtitle,
                start: function (event, ui) {
                    jQuery(self._box).css({ 'bottom': 'auto', 'right': 'auto' });
                },
                drag: function (event, ui) {
                    self.checkSafePosition()
                },
                stop: function (event, ui) {
                    self.checkSafePosition()
                }
            });

        } catch (err) {
            log(err);
        }
    },
    fillBox_Loading: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '300px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_Loading';
            try { jQuery(self._handle).remove(); } catch (err) { }
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = true;

            self._closebutton
                .show()
                .click(function (event) {
                    self.destroyBox();
                    return false;
                })

            //zentools.mainURL + "/Messages/Send"
            self._loading = jQuery('<center></center>')
                .css({ 'margin': '40px' })
                .appendTo(self._boxinfo);

            var loading = jQuery('<img src="' + zentools.mainURL + '/Content/images/loading.gif"/>')
				.css({})
				.appendTo(self._loading);

            self.positionBox(null, 'fast');

        } catch (err) {
            log(err);
        }
    },
    hideBox: function () {
        var self = this;
        var box = self._box;
        box.unbind('mouseenter mouseleave click');
        box
				.hide()
				.makeAbsolute()
				.css({
				    'top': ((jQuery(window).height() / 2) - (jQuery(box).outerHeight() / 2)) + 'px',
				    'left': ((jQuery(window).width() / 2) - (jQuery(box).outerWidth() / 2)) + 'px'
				});
    },
    bringtofrontBox: function () {
        var self = this;
        try {
            if (zentools.Boxes.length < 2) {
                return false;
            }
            if (self._preferredstacked !== null) {
                return false;
            }
            var max_zindex = new Number(1000);
            var new_zindex = new Number(1000);
            jQuery.each(zentools.Boxes, function (index, _foundbox) {
                jQuery(_foundbox._box).removeClass('focused');
                new_zindex = new Number(jQuery(_foundbox._box).css('z-index'));
                if (new_zindex > max_zindex) {
                    max_zindex = new_zindex;
                }
            });
            jQuery.each(zentools.Boxes, function (index, _foundbox) {
                if (_foundbox._type === "ControlBox") {
                    jQuery(_foundbox._box).css({ 'z-index': max_zindex + 1 });
                }
            });
            jQuery(self._box).css({ 'z-index': max_zindex + 2 });

            if (zentools.Daemon !== null) {
                //Botones de la lista...
                jQuery.each(jQuery(zentools.Daemon.controlBox._boxList).find('.btn'), function (index, _foundbutton) {
                    jQuery(_foundbutton).removeClass('active');
                });
                jQuery('#linkTo_' + self._id).addClass('active');
            }
            //Borramos los mensajes... porque los hemos leido no???
            self.unread_elements = new Number(0);
            //self._box.process_alerts();

            jQuery(self._box).addClass('focused');
            //Hagamos que esto sea un poquito cómodo...
            if ((self._type === "MessageBox") && (self._status !== "fillBox_LeaveMessage")) {
                var to_focus = jQuery(self._box).find('.getfocus')[0];
                if (to_focus === null) {
                    to_focus = jQuery(self._box).find('textarea')[0];
                }
                if (to_focus === null) {
                    to_focus = jQuery(self._box).find('input')[0];
                }
                if (to_focus === null) {
                    to_focus = jQuery(self._box).find('.btn')[0];
                }
                if (to_focus === null) {
                    to_focus = jQuery(self._box);
                }
                to_focus.focus();
            }

        } catch (err) {
            log(err);
        }
    },
    destroyBox: function () {
        var self = this;
        try {
            jQuery(self._box).remove();
            zentools.Boxes = jQuery.grep(zentools.Boxes, function (value) {
                return value !== self;
            });
            zentools.Daemon.sync_ControlList();
        } catch (err) {
            log(err);
        }
    },
    positionBox: function (callback, speed) {
        var self = this;
        try {

            self.bringtofrontBox();

            if (zentools.Daemon !== null) {
                // && (zentools.mode === 'agent')
                zentools.Daemon.sync_ControlList();
            }

            var box = self._box;
            box.unbind('mouseenter mouseleave click');
            jQuery(window).unbind('resize.' + self._id);
            jQuery(window).unbind('scroll.' + self._id);

            if (self._preferredstacked !== null) {
                //log(self._preferredstacked);
                jQuery(box)
                    .css({
                        'top': 'auto',
                        'right': 'auto',
                        'bottom': 'auto',
                        'left': 'auto',
                        'position': 'relative'
                    })
                    .appendTo(self._preferredstacked)
                    .unbind('click')
                    .draggable("destroy")
                    .addClass('stacked')
                    .fadeIn();
                return;
            }
            /* 
            else {
            self._boxtitle.show();
            }
            */

            if (self._preferredpinned === true) {
                jQuery(window).bind('resize.' + self._id, function () {
                    self.positionBox(null, 0);
                });
                /*
                jQuery(window).bind('scroll.' + self._id, function () {
                self.positionBox(null, 0);
                });
                */
            }

            box.css({ 'width': self._preferredwidth, 'height': self._preferredheight });
            //console.log(jQuery(box).outerHeight());
            var pubication = self._preferredubication;
            switch (pubication) {
                case 'maximized':
                    box
						.makeAbsolute()
						.css({
						    'top': '0px',
						    'right': '0px',
						    'bottom': '0px',
						    'left': '0px',
						    'position': 'fixed'
						})
						.fadeIn();
                    break;
                case 'center':
                    box
						.makeAbsolute()
						.css({
						    'top': ((jQuery(window).height() / 2) - (jQuery(box).outerHeight() / 2)) + 'px',
						    'right': 'auto',
						    'bottom': 'auto',
						    'left': ((jQuery(window).width() / 2) - (jQuery(box).outerWidth() / 2)) + 'px',
						    'position': 'fixed'
						})
						.fadeIn();
                    break;
                case 'standbychat':
                    var bottom = new Number(0);
                    var right = new Number(100);

                    box
						.makeAbsolute()
						.css({
						    'top': 'auto',
						    'right': right + 'px',
						    'bottom': bottom + 'px',
						    'left': 'auto',
						    'position': 'fixed'
						})
                        .fadeIn();

                    break;
                case 'right':
                    box
						.makeAbsolute()
						.css({
						    'top': ((jQuery(window).height() / 2) - (jQuery(box).outerHeight() / 2)) + 'px',
						    'right': 5 + 'px',
						    'bottom': 'auto',
						    'left': 'auto',
						    'position': 'fixed'
						})
						.fadeIn();
                    break;
                case 'left':
                    box
						.makeAbsolute()
						.css({
						    'top': ((jQuery(window).height() / 2) - (jQuery(box).outerHeight() / 2)) + 'px',
						    'right': 'auto',
						    'bottom': 'auto',
						    'left': 0 + 'px',
						    'position': 'fixed'
						})
						.fadeIn();
                    break;
                case 'bottomright':

                    var bottom = new Number(4);
                    var right = 7;

                    if (zentools.Boxes.length > 1) {
                        var i = new Number(0);
                        jQuery.each(zentools.Boxes, function (_index, _foundbox) {
                            if ((box._id !== _foundbox._id) && (_foundbox._type === "MessageBox")) {
                                right = ((jQuery(box).outerWidth() + new Number(3)) * i) + 7;
                                //left = left - (jQuery(box).outerWidth() * i);
                                if (right > jQuery(document).outerWidth()) {
                                    right = 7;
                                };
                                i++;
                            } else if (_foundbox._type === "ControlBox") {
                                if (zentools.mode === "agent") {
                                    bottom = bottom + jQuery(_foundbox._box).outerHeight();
                                }
                            }
                        });
                    }

                    box
						.makeAbsolute()
						.css({
						    'top': 'auto',
						    'right': right + 'px',
						    'bottom': bottom + 'px',
						    'left': 'auto',
						    'position': 'fixed'
						})
                        .fadeIn();

                    break;
                case 'bottomleft':
                    box
						.makeAbsolute()
						.css({
						    'top': 'auto',
						    'right': 'auto',
						    'bottom': 0 + 'px',
						    'left': 0 + 'px',
						    'position': 'fixed'
						});
                    //.fadeIn();
                    break;
                case 'topleft':
                    box
						.makeAbsolute()
						.css({
						    'top': 0 + 'px',
						    'right': 'auto',
						    'bottom': 'auto',
						    'left': 0 + 'px',
						    'position': 'fixed'
						})
						.fadeIn();
                    break;
            }
            if (callback !== null) {
                callback();
            }

            self.checkSafePosition();

        } catch (err) {
            log(err);
        }
    },
    checkSafePosition: function () {
        var self = this;
        try {
            var box = self._box;

            var position = jQuery(box).position();
            if (position.left < 0) {
                box
                    .makeAbsolute()
                    .css({
                        'left': '0px'
                    });
            }
            if (position.top < 0) {
                box
                    .makeAbsolute()
                    .css({
                        'top': '0px'
                    });
            }
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
