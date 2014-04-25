using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{
    public class PhantomController : Controller
    {

        Utils utils = new Utils();

        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public async Task<ActionResult> GetCaptureToRender(string callback)
        {
            try
            {

                WebLocation wl = await Task.Factory.StartNew(() => utils.BuscaNuevasWebLocations());

                if (wl == null)
                {
                    return Json(false);
                }
                else
                {
                    var CompleteHtml = utils.ScrubHTML(wl.CompleteHtml);

                    var toReturn = new
                    {
                        Html = CompleteHtml,
                        Blob = wl.ScreenCapture.Blob,
                        UserName = wl.UserName,
                        Domain = wl.Domain,
                        GUID = wl.ScreenCapture.GUID,
                        WindowName = wl.ScreenCapture.WindowName,
                        Width = wl.ScreenCapture.Width,
                        Height = wl.ScreenCapture.Height,
                        ScrollTop = wl.ScreenCapture.ScrollTop,
                        ScrollLeft = wl.ScreenCapture.ScrollLeft
                    };

                    return Json(toReturn);
                }

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
            }
        }

    }
}
