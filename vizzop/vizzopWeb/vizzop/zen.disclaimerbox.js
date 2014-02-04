
var DisclaimerBox = jVizzop.zs_Class.create(Box, {
    // constructor
    initialize: function () {
        var self = this;
        self.base('initialize');
        self._type = 'DisclaimerBox';
    },
    ShowDisclaimer: function () {
        var self = this;
        try {
            self._preferredubication = 'top';
            self._preferredwidth = 'auto';
            self._preferredheight = 'auto';
            self._status = 'ShowDisclaimer';
            try { jVizzop(self._handle).remove(); } catch (err) { }
            self._boxtitle.hide();
            self._boxinfo.attr('style', '');
            self._boxinner.show();
            self.hideBox();
            self._boxinfo.empty();

            self._nolist = true;

            /*
            <div class="alert error">
                <a class="close" data-dismiss="alert">×</a>
            @Html.Raw(ViewBag.errors)
            </div>
            */

            self._box.addClass('disclaimer')

            var msgbox = jVizzop('<div></div>')
                    .addClass('alert alert-info')
                    .attr('style', 'text-align: center !important; margin: 0 !important; ')
                    .appendTo(self._boxinfo);

            var text = jVizzop('<span></span>')
                    .addClass('disclaimertext')
                    .text(LLang('law_disclaimer', null))
                    .appendTo(msgbox);

            var text = jVizzop('<a></a>')
                    .addClass('btn btn-primary btn-large')
                    .attr('data-dismiss', 'alert')
                    .text('OK')
                    .appendTo(msgbox);

            self.positionBox(function () { jVizzop(self._box).fadeIn(); }, 'fast');
        } catch (err) {
            vizzoplib.log("ShowDisclaimer " + err);
        }
    }
},
{
    // properties
    getset: [
        ['Box', '_box']
    ]
});
