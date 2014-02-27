
var Box = jVizzop.zs_Class.create({
    // constructor
    initialize: function () {
        /// <summary>Creates a Base Box </summary>
        var self = this;
        self.createBox();

        /// <field name='_type' type='String'>The Type of Box.</field>
        self._type = 'Box';

        vizzop.Boxes.push(this);
        if (vizzop.Daemon != null) {
            vizzop.Daemon.sync_ControlList();
        }
    },
    createBox: function () {
        try {
            var self = this;

            self._preferredubication = 'center';
            self._preferredpinned = true;
            self._preferredstacked = null;
            var randomid = "box_" + vizzoplib.randomnumber();
            while (jVizzop.each(vizzop.Boxes, function (index, _box) {
				if (jVizzop(_box).attr('id') == randomid) { return true; }
            }) == true) {
                randomid = "box_" + randomnumber;
            }

            self._id = randomid;

            //Por defecto se muestra...
            self._nolist == false;

            self._box = jVizzop('<div></div>')
				.addClass('zs_dialog')
				.attr('id', randomid)
				.click(function (event) {
				    self.bringtofrontBox();
				})
				.appendTo(jVizzop('body'));

            //Inner Box
            self._boxinner = jVizzop('<div></div>')
					.addClass('zs_dialog_inner')
					.appendTo(self._box);

            self._title = LLang('sup_box_title', [vizzop.webname]);

            //Title Bar
            self._boxtitle = jVizzop('<div></div>')
				.addClass('zs_dialog_title')
                .appendTo(self._boxinner);
            self._boxtitletext = jVizzop('<span></span>')
					.addClass('title')
					.text(LLang('sup_box_title', [vizzop.webname]))
					.appendTo(self._boxtitle);
            /*
			
					.css({ 'cursor': 'move' })
			*/

            self._closebutton = jVizzop('<span></span>')
					.addClass('zs_dialogclosebutton')
					.html('<i class="vizzop-icon-remove vizzop-icon-white"></i>')
					.click(function (event) {
					    //self.destroyBox();
					    return false;
					})
					.hover(
					function (event) {
					    //jVizzop(this).html('<i class="vizzop-icon-remove vizzop-icon-white"></i>');
					},
					function (event) {
					    //jVizzop(this).html('<i class="vizzop-icon-remove"></i>');
					}
					)
					.appendTo(self._boxtitle);
            /*
			self._togglebarbutton = jVizzop('<span></span>')
					.addClass('zs_dialogtogglebarbutton')
					.html('<i class="vizzop-icon-minus"></i>')
					.click(function (event) {
						jVizzop(self._boxinfo).toggle();
						return false;
					})
					.hover(
					function (event) {
						jVizzop(this).html('<i class="vizzop-icon-minus vizzop-icon-white"></i>');
					},
					function (event) {
						jVizzop(this).html('<i class="vizzop-icon-minus"></i>');
					}
					)
					.appendTo(self._boxtitle);
			*/

            //Info Box
            self._boxinfo = jVizzop('<div></div>')
				.addClass('zs_boxinfo')
				.appendTo(self._boxinner);

            //Statusbar
            self._boxstatus = jVizzop('<div></div>')
				.addClass('boxstatus')
				.hide()
				.appendTo(self._boxinner);

            //events and so..
            self._boxtitle.click(function (event) {
                self.bringtofrontBox();
            });
            self._boxinfo.click(function (event) {
                self.bringtofrontBox();
            });
            self._boxstatus.click(function (event) {
                self.bringtofrontBox();
            });

            /**/
            self._boxtitle.dblclick(function (event) {
                jVizzop(self._boxinfo).toggle();
                //jVizzop(self._boxstatus).toggle();
                if (self._minimized == null) {
                    self._minimized = true;
                } else {
                    self._minimized = null;
                }
            });


            self._box.draggable({
                handle: self._boxtitle,
                start: function (event, ui) {
                    jVizzop(self._box).css({ 'bottom': 'auto', 'right': 'auto' });
                },
                drag: function (event, ui) {
                    self.checkSafePosition()
                },
                stop: function (event, ui) {
                    self._preferredubication = null;
                    self.checkSafePosition()
                }
            });

        } catch (err) {
            vizzoplib.log(err);
        }
    },
    fillBox_Loading: function () {
        try {
            var self = this;
            self._preferredubication = 'center';
            self._preferredwidth = '300px';
            self._preferredheight = 'auto';
            self._status = 'fillBox_Loading';
            try { jVizzop(self._handle).remove(); } catch (err) { }
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

            self._loading = jVizzop('<center></center>')
				.css({ 'margin': '40px' })
				.appendTo(self._boxinfo);

            var loading = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
				.css({})
				.appendTo(self._loading);

            self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');

        } catch (err) {
            vizzoplib.log(err);
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
                'top': ((jVizzop(window).height() / 2) - (jVizzop(box).outerHeight() / 2)) + 'px',
                'left': ((jVizzop(window).width() / 2) - (jVizzop(box).outerWidth() / 2)) + 'px'
            });
    },
    bringtofrontBox: function () {
        var self = this;
        try {
            if (vizzop.Boxes.length < 2) {
                return false;
            }
            if (self._preferredstacked != null) {
                return false;
            }
            var max_zindex = new Number(999999);
            jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                jVizzop(_foundbox._box).removeClass('focused');
                jVizzop(_foundbox._box).css({ 'z-index': max_zindex });
            });

            jVizzop.each(vizzop.Boxes, function (index, _foundbox) {
                if (_foundbox._type == "ControlBox") {
                    jVizzop(_foundbox._box).css({ 'z-index': (max_zindex + new Number(1)) });
                }
            });

            jVizzop(self._box).css({ 'z-index': (max_zindex + new Number(2)) });

            if (vizzop.Daemon != null) {
                //Botones de la lista...
                jVizzop.each(jVizzop(vizzop.Daemon.controlBox._boxList).find('.vizzop-btn'), function (index, _foundbutton) {
                    jVizzop(_foundbutton).removeClass('active');
                });
                jVizzop('#linkTo_' + self._id).addClass('active');
            }
            //Borramos los mensajes... porque los hemos leido no???
            self.unread_elements = new Number(0);
            //self._box.process_alerts();

            jVizzop(self._box).addClass('focused');
            //Hagamos que esto sea un poquito cómodo...
            if ((self._type == "MessageBox") && (self._status != "fillBox_LeaveMessage")) {
                var to_focus = jVizzop(self._box).find('.getfocus');
                if (to_focus.length == 0) {
                    to_focus = jVizzop(self._box).find('textarea');
                }
                if (to_focus.length == 0) {
                    to_focus = jVizzop(self._box).find('input');
                }
                if (to_focus.length == 0) {
                    to_focus = jVizzop(self._box).find('.vizzop-btn');
                }
                if (to_focus.length == 0) {
                    to_focus = jVizzop(self._box);
                }
                to_focus[0].focus();
            }

        } catch (err) {
            vizzoplib.log(err);
        }
    },
    destroyBox: function () {
        var self = this;
        try {
            jVizzop(self._box).remove();
            vizzop.Boxes = jVizzop.grep(vizzop.Boxes, function (value) {
                return value != self;
            });
            vizzop.Daemon.sync_ControlList();
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    positionBox: function (callback, speed) {
        var self = this;
        try {
            if (self._minimized != null) {
                return;
            }
            if (self._preferredubication != null) {
                self.bringtofrontBox();
            }

            if (vizzop.Daemon != null) {
                // && (vizzop.mode == 'agent')
                vizzop.Daemon.sync_ControlList();
            }

            var box = self._box;
            //box.unbind('mouseenter mouseleave click');
            jVizzop(window).unbind('resize.' + self._id);
            jVizzop(window).unbind('scroll.' + self._id);

            if (self._preferredstacked != null) {
                //vizzoplib.log(self._preferredstacked);
                jVizzop(box)
					.css({
					    'top': 'auto',
					    'right': 'auto',
					    'bottom': 'auto',
					    'left': 'auto',
					    'position': 'relative'
					})
					.appendTo(self._preferredstacked)
					//.unbind('click')
					.draggable("destroy")
					.addClass('stacked')
				.show();
                //.fadeIn();
                return;
            }
            /* 
			else {
			self._boxtitle.show();
			}
			*/

            if (self._preferredpinned == true) {
                jVizzop(window).bind('resize.' + self._id, function () {
                    self.positionBox(null, 0);
                });
                /*
				jVizzop(window).bind('scroll.' + self._id, function () {
				self.positionBox(null, 0);
				});
				*/
            }

            box.css({ 'width': self._preferredwidth, 'height': self._preferredheight });
            //console.vizzoplib.log(jVizzop(box).outerHeight());
            var pubication = self._preferredubication;
            switch (pubication) {
                case 'top':
                    box
						.css({
						    'top': 0 + 'px',
						    'right': 0 + 'px',
						    'bottom': 'auto',
						    'left': 0 + 'px',
						    'position': 'fixed'
						});
                    //.fadeIn();
                    break;
                case 'bottom':
                    box
						.css({
						    'top': 'auto',
						    'right': 'auto',
						    'bottom': 0 + 'px',
						    'left': 0 + 'px',
						    'position': 'fixed'
						});
                    //.fadeIn();
                    break;
                case 'maximized':
                    box
						.makeAbsolute()
						.css({
						    'top': '0px',
						    'right': '0px',
						    'bottom': '0px',
						    'left': '0px',
						    'position': 'fixed'
						});
                    //.fadeIn();
                    break;
                case 'center':
                    var top = new Number(20);
                    if (jVizzop(window).height() > jVizzop(box).outerHeight() + top) {
                        top = Math.floor(((jVizzop(window).height() / 2) - (jVizzop(box).outerHeight() / 2)));
                    }
                    var left = new Number(20);
                    if (jVizzop(window).width() > jVizzop(box).outerWidth() + left) {
                        left = Math.floor(((jVizzop(window).width() / 2) - (jVizzop(box).outerWidth() / 2)))
                    }
                    /*
					if (top == 0) {
						top = 20;
					}*/
                    box
						.makeAbsolute()
						.css({
						    'top': top + 'px',
						    'right': 'auto',
						    'bottom': 'auto',
						    'left': left + 'px',
						    'position': 'fixed'
						});
                    //.fadeIn();
                    break;
                case 'standbychat':
                    var bottom = new Number(0);
                    var right = new Number(10);

                    box
						.makeAbsolute()
						.css({
						    'top': 'auto',
						    'right': right + 'px',
						    'bottom': bottom + 'px',
						    'left': 'auto',
						    'position': 'fixed'
						});
                    break;
                case 'right':
                    box
						.makeAbsolute()
						.css({
						    'top': ((jVizzop(window).height() / 2) - (jVizzop(box).outerHeight() / 2)) + 'px',
						    'right': 5 + 'px',
						    'bottom': 'auto',
						    'left': 'auto',
						    'position': 'fixed'
						});
                    //.fadeIn();
                    break;
                case 'left':
                    box
						.makeAbsolute()
						.css({
						    'top': ((jVizzop(window).height() / 2) - (jVizzop(box).outerHeight() / 2)) + 'px',
						    'right': 'auto',
						    'bottom': 'auto',
						    'left': 0 + 'px',
						    'position': 'fixed'
						});
                    //.fadeIn();
                    break;
                case 'bottomright':

                    var bottom = new Number(4);
                    var right = 7;

                    if (vizzop.Boxes.length > 1) {
                        var i = new Number(0);
                        jVizzop.each(vizzop.Boxes, function (_index, _foundbox) {
                            if ((box._id != _foundbox._id) && (_foundbox._type == "MessageBox")) {
                                right = ((jVizzop(box).outerWidth() + new Number(3)) * i) + 7;
                                //left = left - (jVizzop(box).outerWidth() * i);
                                if (right > jVizzop(document).outerWidth()) {
                                    right = 7;
                                };
                                i++;
                            } else if (_foundbox._type == "ControlBox") {
                                if (vizzop.mode == "agent") {
                                    bottom = bottom + jVizzop(_foundbox._box).outerHeight();
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
						});
                    //.fadeIn();

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
						});
                    //.fadeIn();
                    break;
            }
            if (callback != null) {
                callback();
            }

            self.checkSafePosition();

        } catch (err) {
            vizzoplib.log(err);
        }
    },
    checkSafePosition: function () {
        var self = this;
        try {
            var box = self._box;

            var position = jVizzop(box).position();
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
            if ((position.left + jVizzop(box).outerWidth()) > jVizzop(window).width()) {
                box
					.makeAbsolute()
					.css({
					    'left': (jVizzop(window).width() - jVizzop(box).outerWidth()) + 'px'
					});
            }
            /*
			if ((position.top + jVizzop(box).outerHeight()) > jVizzop(window).height()) {
				box
					.makeAbsolute()
					.css({
						'top': (jVizzop(window).height() - jVizzop(box).outerHeight()) + 'px'
					});
			*/
            box.css({
                'position': 'fixed'
            });
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
