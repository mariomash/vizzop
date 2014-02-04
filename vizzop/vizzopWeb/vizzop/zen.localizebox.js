var LocalizeBox = jVizzop.zs_Class.create(Box, {
    // constructor
    initialize: function (_locid) {
        /// <summary>Creates a Loclization Editor Box </summary>
        /// <param name="_locid" type="String" optional="true">The ID of the textstring</param>
        var self = this;
        self.base('initialize');
        self._type = 'LocalizeBox';

        /// <field name='_locid' type='String'>The Localize ID.</field>
        self._locid = _locid;
        self._data = null;
        self.fillBox_Loading();
        if (self._locid != null) {
            self.loadLocDetails();
        } else {
            self.fillBox_ShowLocEditor();
        }
    },
    loadLocDetails: function () {
        var self = this;
        try {
            if (self._locid == null) {
                return false;
            }
            //se envia en UTC
            var msg = {
                'id': self._locid
            };
            //vizzoplib.log("Por Enviar: " + self._content);
            var url = vizzop.mainURL + "/Localization/GetTextStringDetails";
            var req = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "json",
                success: function (data) {
                    self._data = data;
                    self.fillBox_ShowLocEditor(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    fillBox_ShowLocEditor: function (values) {
        var self = this;
        try {
            jVizzop.getJSON("/Localization/GetIsoCodes", null, function (data) {

                self._preferredubication = 'center';
                self._preferredwidth = '600px';
                self._preferredheight = 'auto';
                self._status = 'fillBox_ShowLocEditor';
                try { jVizzop(self._handle).remove(); } catch (err) { }
                self._boxinner.show();
                self.hideBox();
                self._boxinfo.empty();
                self._nolist = false;

                jVizzop(self._boxtitletext).html('<i class="icon-comment icon-white"></i>&nbsp;<span style="display: inline-block; vertical-align: middle; cursor: move;">Localization editor</span>');
                self._title = self.locid;

                self._boxtextloc = jVizzop('<div></div>')
                    .css('margin-top', '10px')
                    .addClass('boxquestion')
                    .appendTo(self._boxinfo);

                self._boxtextloc.langZone = jVizzop('<div></div>')
                    .appendTo(self._boxtextloc);

                self._boxtextloc.languagelabel = jVizzop('<label></label>')
                    .attr({ 'style': 'display : inline-block !important; vertical-align: top !important;; margin: 8px 4px !important;width: 100px; text-align: right;' })
                    .html('Language ')
                    .appendTo(self._boxtextloc.langZone);

                self._boxtextloc.languageselect = jVizzop('<select/>')
                    .attr({
                        'style': 'display: inline-block; vertical-align: top; height: auto !important; margin: 4px !important;'
                    })
                    .addClass('input-medium')
                    .change(function () { })
                    .appendTo(self._boxtextloc.langZone);

                self._boxtextloc.refZone = jVizzop('<div></div>')
                    .appendTo(self._boxtextloc);

                self._boxtextloc.reflabel = jVizzop('<label></label>')
                    .attr({ 'style': 'display : inline-block !important; vertical-align: top !important;; margin: 8px 4px !important;width: 100px; text-align: right;' })
                    .text('Reference')
                    .appendTo(self._boxtextloc.refZone);

                self._boxtextloc.refinput = jVizzop('<input/>')
                    .attr({
                        'style': 'display: inline-block; vertical-align: top; height: auto !important; margin: 4px !important;'
                    })
                    .attr('type', 'text')
                    .attr('placeholder', 'write the reference here...')
                    .appendTo(self._boxtextloc.refZone);


                self._boxtextloc.textZone = jVizzop('<div></div>')
                    .appendTo(self._boxtextloc);

                self._boxtextloc.textlabel = jVizzop('<label></label>')
                    .attr({ 'style': 'display : inline-block !important; vertical-align: top !important;; margin: 8px 4px !important; width: 100px; text-align: right;' })
                    .text('Text')
                    .appendTo(self._boxtextloc.textZone);

                self._boxtextloc.textinput = jVizzop('<textarea/>')
                            .attr('placeholder', 'write the text here...')
                            .addClass('hint')
                            .attr({
                                'style': 'display: inline-block; vertical-align: top; height: auto !important; margin: 4px !important;height: 180px !important; width: 450px !important;'
                            })
                            .appendTo(self._boxtextloc.textZone);

                self._boxtextloc.buttonsZone = jVizzop('<div></div>')
                    .addClass('zs_buttonszone form-actions')
                    .appendTo(self._boxtextloc);

                self._savebutton = jVizzop('<span></span>')
                    .text("save changes")
                    .addClass(
                    'btn btn-primary'
                    )
                    .click(function (e) {
                        e.preventDefault();
                        jVizzop(self._loading).show();
                        jVizzop(self._savebutton).hide();
                        jVizzop(self._deletebutton).hide();
                        self.SaveChanges();
                        return false;
                    })
                    .appendTo(self._boxtextloc.buttonsZone);

                self._deletebutton = jVizzop('<span></span>')
                    .html("delete")
                    .addClass(
                    'btn btn-danger'
                    )
                    .hide()
                    .click(function (e) {
                        e.preventDefault();
                        if (confirm('Are you sure you wish to remove it?')) {
                            jVizzop(self._loading).show();
                            jVizzop(self._savebutton).hide();
                            jVizzop(self._deletebutton).hide();
                            self.deleteLoc();
                        }
                        return false;
                    })
                    .appendTo(self._boxtextloc.buttonsZone);
                //vizzoplib.log(vizzop.CommRequest_InCourse);

                self._loading = jVizzop('<div></div>')
                    .css({ 'text-align': 'center', 'padding': '5px' })
                    .appendTo(self._boxtextloc.buttonsZone)
                    .hide();

                self._loadingimg = jVizzop('<img src="' + vizzop.mainURL + '/Content/images/loading.gif"/>')
                    .css({ 'margin': '8px 5px', 'vertical-align': 'middle' })
                    .appendTo(self._loading);

                self._closebutton
                    .show()
                    .click(function (e) {
                        self.destroyBox();
                        return false;
                    });

                jVizzop.each(data, function (index, item) {
                    jVizzop("<option></option>")
                        .text(item.Name + ' (' + item.IsoCode + ')')
                        .val(item.IsoCode)
                        .appendTo(self._boxtextloc.languageselect);
                });


                if (values != null) {
                    jVizzop(self._boxtextloc.languageselect).find('option[value="' + values.IsoCode + '"]').prop('selected', true);
                    self._boxtextloc.refinput.val(values.Ref);
                    self._boxtextloc.textinput.val(values.Text);
                    self._deletebutton.css('display', 'inline-block');
                }

                self.positionBox(function () { jVizzop(self._box).show(); }, 'fast');
            });


        } catch (err) {
            vizzoplib.log(err);
        }
    },
    SaveChanges: function () {
        var self = this;
        try {

            //se envia en UTC
            var msg = {
                'reference': self._boxtextloc.refinput.val(),
                'text': self._boxtextloc.textinput.val(),
                'langisocode': jVizzop(self._boxtextloc.languageselect).find(":selected").val(),
                'id': self._locid
            };
            //vizzoplib.log("Por Enviar: " + self._content);
            var url = vizzop.mainURL + "/Localization/SaveTextString";
            var req = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "json",
                success: function (data) {
                    jVizzop(self._loading).hide();
                    jVizzop(self._savebutton).show();
                    if (data == false) {
                        alert("error saving TextString");
                    } else {
                        self._locid = data;
                        self.loadLocDetails();
                        loc_oTable.loc_fnReloadAjax(getLocalizationURL(), function () { });
                        alert("Changes Saved ");
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    jVizzop(self._loading).hide();
                    jVizzop(self._savebutton).show();
                    alert("error saving TextString");
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    deleteLoc: function () {
        var self = this;
        try {
            //se envia en UTC
            var msg = {
                'id': self._locid
            };
            //vizzoplib.log("Por Enviar: " + self._content);
            var url = vizzop.mainURL + "/Localization/DeleteTextString";
            var req = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "json",
                success: function (data) {
                    if (data == true) {
                        self.destroyBox();
                        loc_oTable.loc_fnReloadAjax(getLocalizationURL(), function () { });
                    } else if ((data == false) || (data == null)) {
                        alert("error deleting");
                    } else {
                        alert("correctly Deleted");
                        self.loadLocDetails();
                        loc_oTable.loc_fnReloadAjax(getLocalizationURL(), function () { });
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    alert("error deleting");
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    }
});
