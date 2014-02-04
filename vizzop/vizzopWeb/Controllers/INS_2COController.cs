using System.Web.Mvc;

namespace vizzopWeb.Controllers
{
    public class INS_2COController : Controller
    {
        private Utils utils = new Utils();

        //
        // GET: /INS_2CO/
        [HttpPost]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Index(FormCollection collection)
        {
            utils.GrabaLog(Utils.NivelLog.error, utils.SacaParamsContext(HttpContext));
            return null;
        }

    }
}
