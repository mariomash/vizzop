using System.Linq;
using System.Web.Mvc;
//using System.Web.Mvc.Resources;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{
    public class MeetingController : Controller
    {
        
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Index()
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

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
