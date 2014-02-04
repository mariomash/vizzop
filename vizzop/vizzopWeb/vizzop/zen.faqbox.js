var FaqBox = jVizzop.zs_Class.create(Box, {
    // constructor
    initialize: function (_faqid) {
        /// <summary>Creates a Faq Editor Box </summary>
        /// <param name="_faqid" type="String" optional="true">The ID of the faq</param>
        var self = this;
        self.base('initialize');
        self._type = 'FaqBox';

        /// <field name='_faqid' type='String'>The FAQ ID.</field>
        self._faqid = _faqid;
        self._data = null;
        self.fillBox_Loading();
        if (self._faqid != null) {
            self.loadFaqDetails();
        } else {
            self.fillBox_ShowFaqEditor();
        }
    },
    loadFaqDetails: function () {
        var self = this;
        try {
            if (self._faqid == null) {
                return false;
            }
            //se envia en UTC
            var msg = {
                'id': self._faqid
            };
            //vizzoplib.log("Por Enviar: " + self._content);
            var url = vizzop.mainURL + "/Faqs/GetFaqDetails";
            var req = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "json",
                success: function (data) {
                    self._data = data;
                    self.fillBox_ShowFaqEditor(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    fillBox_ShowFaqEditor: function (values) {
        var self = this;
        try {
            jVizzop.getJSON("/Localization/GetIsoCodes", null, function (data) {

                self._preferredubication = 'center';
                self._preferredwidth = '400px';
                self._preferredheight = 'auto';
                self._status = 'fillBox_ShowFaqEditor';
                try { jVizzop(self._handle).remove(); } catch (err) { }
                self._boxinner.show();
                self.hideBox();
                self._boxinfo.empty();
                self._nolist = false;

                jVizzop(self._boxtitletext).html('<i class="icon-comment icon-white"></i>&nbsp;<span style="display: inline-block; vertical-align: middle; cursor: move;">FAQ editor</span>');
                self._title = self.faqid;

                self._boxheader = jVizzop('<div></div>')
                    .addClass('boxheader')
                    .hide()
                    .appendTo(self._boxinfo);

                self._topstatustext = jVizzop('<label></label>')
                    .attr({ 'style': 'display : inline-block !important; vertical-align: middle !important;; margin: 4px !important;' })
                    .html('&nbsp;&nbsp;Selected Language ')
                    .appendTo(self._boxheader);

                self._topstatusselect = jVizzop('<select/>')
                    .attr({
                        'style': 'display: inline-block; vertical-align: middle; height: auto !important; margin: 4px !important;'
                    })
                    .addClass('input-medium')
                    .change(function () {
                        var n = jVizzop(self._topstatusselect).val();
                        jVizzop(self._data.FaqDetails).each(function (index, item) {
                            if (item.LangISOCode == n) {
                                self._boxtextfaq.questionTextArea.val(item.Question);
                                self._boxtextfaq.answerTextArea.val(item.Answer);
                            }
                        });
                    })
                    .appendTo(self._boxheader);

                self._topstatusloading = jVizzop('<span><img src="' + vizzop.mainURL + '/Content/images/loading.gif"/></span>')
                    .css({ 'display': 'inline-block', 'vertical-align': 'middle' })
                    .appendTo(self._boxheader)
                    .hide();

                self._topstatuscreate = jVizzop('<span></span>')
                    .addClass(
                    'btn btn-primary pull-right'
                    )
                    .css({ 'vertical-align': 'middle' })
                    .html("<i class='icon-white icon-plus'></i> add language")
                    .click(function (e) {
                        e.preventDefault();

                        jVizzop('<option></option>')
                            .text('Start by choosing the language of the question...')
                            .val('null')
                            .insertBefore(jVizzop(self._boxtextfaq.languageSelect).find('option')[0]);

                        jVizzop('<option></option>')
                            .text('...')
                            .val('null')
                            .insertBefore(jVizzop(self._topstatusselect).find('option')[0]);

                        jVizzop(self._boxtextfaq.languageSelect).find('option').each(function (index, item) {
                            if (index == 0) {
                                jVizzop(item).prop('selected', true);
                            } else {
                                jVizzop(item).prop('selected', false);
                            }
                        });

                        jVizzop(self._topstatusselect).find('option').each(function (index, item) {
                            if (index == 0) {
                                jVizzop(item).prop('selected', true);
                            } else {
                                jVizzop(item).prop('selected', false);
                            }
                        });

                        self._boxtextfaq.questionTextArea.attr('disabled', 'disabled');
                        self._boxtextfaq.answerTextArea.attr('disabled', 'disabled');
                        self._savebutton.attr('disabled', 'disabled');
                        self._topstatusselect.attr('disabled', 'disabled');
                        self._topstatuscreate.hide();

                        jVizzop(self._boxtextfaq.langZone).show();
                        jVizzop(self._boxtextfaq.questionTextArea).val(null);
                        jVizzop(self._boxtextfaq.questionZone).hide();
                        jVizzop(self._boxtextfaq.answerZone).hide();
                        jVizzop(self._boxtextfaq.answerTextArea).val(null);
                        jVizzop(self._deletebutton).hide();
                        jVizzop(self._boxtextfaq.buttonsZone).hide();

                        return false;
                    })
                    .appendTo(self._boxheader);

                self._boxtextfaq = jVizzop('<div></div>')
                    .addClass('boxquestion')
                    .appendTo(self._boxinfo);

                var clear = jVizzop('<div></div>')
                    .css({ 'clear': 'both' })
                    .appendTo(self._boxtextfaq);
                /*
                var title = jVizzop('<h3>Frequently Asked Question Editor</h3>')
                    .attr({ 'style': 'margin-top: 20px !important' })
                    .appendTo(self._boxtextfaq);
                    */
                self._boxtextfaq.langZone = jVizzop('<div></div>')
                    .addClass('zs_langzone')
                    .hide()
                    .appendTo(self._boxtextfaq);

                self._boxtextfaq.langlabel = jVizzop('<label></label>')
                    .text('Language')
                    .attr('for', 'zs_language')
                    .appendTo(self._boxtextfaq.langZone);

                self._boxtextfaq.languageSelect = jVizzop('<select/>')
                    .css({
                        'display': 'block',
                        'vertical-align': 'top'
                    })
                    .html('<option value="null">Start by choosing the language of the question...</option>')
                    .change(function () {
                        var selected = jVizzop(self._boxtextfaq.languageSelect).find(":selected");
                        if (selected.val() != null) {
                            var first = jVizzop(self._boxtextfaq.langcluageSelect).find('option')[0];
                            if (jVizzop(first).val() == "null") {
                                jVizzop(first).remove();
                            }
                            self._boxtextfaq.questionTextArea.removeAttr('disabled');
                            self._boxtextfaq.answerTextArea.removeAttr('disabled');
                            self._savebutton.removeAttr('disabled');

                            jVizzop(self._boxtextfaq.questionZone).show();
                            jVizzop(self._boxtextfaq.answerZone).show();
                            jVizzop(self._boxtextfaq.buttonsZone).show();
                            jVizzop(self._deletebutton).hide();
                            self._preferredwidth = '600px';
                            self.positionBox(function() { jVizzop(self._box).show(); }, 'fast');                            //.each(function () {str += $(this).text() + " ";});
                        }
                    })
                    .appendTo(self._boxtextfaq.langZone);


                self._boxtextfaq.questionZone = jVizzop('<div></div>')
                    .addClass('zs_questionzone')
                    .hide()
                    .appendTo(self._boxtextfaq);

                self._boxtextfaq.questionlabel = jVizzop('<label></label>')
                    .text('Question')
                    .attr('for', 'zs_question')
                    .appendTo(self._boxtextfaq.questionZone);

                self._boxtextfaq.questionTextArea = jVizzop('<textarea/>')
                            .attr('disabled', 'disabled')
                            .attr('placeholder', 'write the question here...')
                            .addClass('hint')
                            .css({
                                'display': 'block',
                                'vertical-align': 'top'
                            })
                            .appendTo(self._boxtextfaq.questionZone);

                self._boxtextfaq.answerZone = jVizzop('<div></div>')
                    .addClass('zs_answerzone')
                    .hide()
                    .appendTo(self._boxtextfaq);

                self._boxtextfaq.answerlabel = jVizzop('<label></label>')
                    .text('Answer')
                    .attr('for', 'zs_answer')
                    .appendTo(self._boxtextfaq.answerZone);

                self._boxtextfaq.answerTextArea = jVizzop('<textarea/>')
                            .attr('disabled', 'disabled')
                            .attr('placeholder', 'write the answer here...')
                            .attr({ 'style': 'height: 90px !important' })
                            .addClass('hint zs_answerarea')
                            .appendTo(self._boxtextfaq.answerZone);

                //.attr('contentEditable', 'true')

                //jVizzop(self._boxtextfaq.answerTextArea).wysiwyg();

                //self._boxtextfaq.questionTextArea.focus();

                self._boxtextfaq.buttonsZone = jVizzop('<div></div>')
                    .addClass('zs_buttonszone form-actions')
                    .hide()
                    .appendTo(self._boxtextfaq);

                self._savebutton = jVizzop('<span></span>')
                    .text("save changes")
                    .attr('disabled', 'disabled')
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
                    .appendTo(self._boxtextfaq.buttonsZone);

                self._deletebutton = jVizzop('<span></span>')
                    .html("<i class='icon-white icon-trash'></i> delete this language")
                    .addClass(
                    'btn btn-danger'
                    )
                    .click(function (e) {
                        e.preventDefault();
                        if (confirm('Are you sure you wish to remove it?')) {
                            var n = jVizzop(self._topstatusselect).find('option:selected').val();
                            jVizzop(self._loading).show();
                            jVizzop(self._savebutton).hide();
                            jVizzop(self._deletebutton).hide();
                            self.deleteFaqDetails(n);
                        }
                        return false;
                    })
                    .hide()
                    .appendTo(self._boxtextfaq.buttonsZone);
                //vizzoplib.log(vizzop.CommRequest_InCourse);

                self._loading = jVizzop('<div></div>')
                    .css({ 'text-align': 'center', 'padding': '5px' })
                    .appendTo(self._boxtextfaq.buttonsZone)
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

                self.positionBox(function() { jVizzop(self._box).show(); }, 'fast');

                jVizzop.each(data, function (index, item) {
                    jVizzop("<option></option>")
                        .text(item.Name + ' (' + item.IsoCode + ')')
                        .val(item.IsoCode)
                        .appendTo(self._boxtextfaq.languageSelect);
                });

                if (values != null) {
                    jVizzop(self._topstatusselect).empty();

                    self._boxheader.show();
                    jVizzop(self._boxtextfaq.questionZone).show();
                    jVizzop(self._boxtextfaq.answerZone).show();
                    jVizzop(self._boxtextfaq.buttonsZone).show();

                    jVizzop(self._boxtextfaq.languageSelect).find('option[value="' + values.FaqDetails[0].LangISOCode + '"]').prop('selected', true);
                    var first = jVizzop(self._boxtextfaq.languageSelect).find('option')[0];
                    if (jVizzop(first).val() == "null") {
                        jVizzop(first).remove();
                    }
                    self._boxtextfaq.langZone.hide();
                    jVizzop(values.FaqDetails).each(function (index, item) {
                        var n = jVizzop(self._boxtextfaq.languageSelect).find('option[value="' + item.LangISOCode + '"]');
                        var option = jVizzop('<option></option>')
                            .text(n.text())
                            .val(n.val())
                            .appendTo(self._topstatusselect);
                        if (item.LangISOCode == values.FaqDetails[0].LangISOCode) {
                            option.prop('selected', true);
                        }
                    });

                    self._boxtextfaq.questionTextArea.removeAttr('disabled');
                    self._boxtextfaq.answerTextArea.removeAttr('disabled');
                    self._savebutton.removeAttr('disabled');
                    self._boxtextfaq.questionTextArea.val(values.FaqDetails[0].Question);
                    self._boxtextfaq.answerTextArea.val(values.FaqDetails[0].Answer);

                    self._boxheader.show();
                    jVizzop(self._deletebutton).css('display', 'inline-block');

                    self._preferredwidth = '600px';

                } else {
                    jVizzop(self._boxtextfaq.langZone).show();
                }

                self.positionBox(function() { jVizzop(self._box).show(); }, 'fast');
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
                'question': self._boxtextfaq.questionTextArea.val(),
                'answer': self._boxtextfaq.answerTextArea.val(),
                'langisocode': jVizzop(self._boxtextfaq.languageSelect).find(":selected").val(),
                'id': self._faqid
            };
            //vizzoplib.log("Por Enviar: " + self._content);
            var url = vizzop.mainURL + "/Faqs/SaveFaq";
            var req = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "json",
                success: function (data) {
                    jVizzop(self._loading).hide();
                    jVizzop(self._savebutton).show();
                    jVizzop(self._boxtextfaq.langZone).hide();
                    if (data == false) {
                        alert("error saving F.A.Q.");
                    } else {
                        self._faqid = data;
                        self.loadFaqDetails();
                        Faq_oTable.Faq_fnReloadAjax(getFaqsURL(), function () { });
                        alert("Changes Saved ");
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    vizzoplib.logAjax(url, msg, jqXHR);
                    jVizzop(self._loading).hide();
                    jVizzop(self._savebutton).show();
                    alert("error saving F.A.Q.");
                }
            });
        } catch (err) {
            vizzoplib.log(err);
        }
    },
    deleteFaqDetails: function (lang) {
        var self = this;
        try {
            //se envia en UTC
            var msg = {
                'id': self._faqid,
                'lang': lang
            };
            //vizzoplib.log("Por Enviar: " + self._content);
            var url = vizzop.mainURL + "/Faqs/DeleteFaqDetails";
            var req = jVizzop.ajax({
                url: url,
                type: "POST",
                data: msg,
                dataType: "json",
                success: function (data) {
                    if (data == true) {
                        self.destroyBox();
                        Faq_oTable.Faq_fnReloadAjax(getFaqsURL(), function () { });
                    } else if ((data == false) || (data == null)) {
                        alert("error deleting");
                    } else {
                        alert("correctly Deleted");
                        self.loadFaqDetails();
                        Faq_oTable.Faq_fnReloadAjax(getFaqsURL(), function () { });
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
