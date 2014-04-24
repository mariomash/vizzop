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
        public async Task<ActionResult> GetCapture(string UserName, string Domain, string WindowName, string GUID, string callback)
        {
            try
            {
                WebLocation weblocation = await Task.Factory.StartNew(
                    () => utils.BuscaNuevasCapturas(
                        UserName,
                        Domain,
                        WindowName,
                        GUID));

                if (weblocation == null)
                {
                    return Json(false);
                }
                else
                {
                    var CompleteHtml = utils.ScrubHTML(weblocation.CompleteHtml);

                    var toReturn = new
                    {
                        Html = CompleteHtml,
                        Blob = weblocation.ScreenCapture.Blob,
                        UserName = UserName,
                        Domain = Domain,
                        GUID = weblocation.ScreenCapture.GUID,
                        WindowName = weblocation.ScreenCapture.WindowName,
                        Width = weblocation.ScreenCapture.Width,
                        Height = weblocation.ScreenCapture.Height,
                        ScrollTop = weblocation.ScreenCapture.ScrollTop,
                        ScrollLeft = weblocation.ScreenCapture.ScrollLeft
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
