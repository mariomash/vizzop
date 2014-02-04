using System.Linq;
using System.Web.Mvc;
//using System.Web.Mvc.Resources;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{
    public class MeetingController : Controller
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Index()
        {
            var _business = (from m in db.Businesses
                             where m.Email == "customer.service@vizzop.com"
                             select m).FirstOrDefault();
            ViewBag.business = _business;
            ViewBag.ApiKey = _business.ApiKey;
            ViewBag.BusinessName = _business.BusinessName;
            return View();
        }

    }
}
