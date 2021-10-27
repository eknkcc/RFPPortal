using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RFPPortalWebsite.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            RfpController cont = new RfpController();
            List<Rfp> model = new List<Rfp>();
            try
            {
                model = cont.GetRfpsByStatus("");
            }
            catch (Exception)
            {
                return View(new List<Rfp>());
            }

            return View(model);

        }

        [Route("RFP-Detail/{BidID}")]
        public IActionResult RFP_Detail(int BidID)
        {
            RfpController cont = new RfpController();
            Models.WebsiteModels.RfpDetailModel model = new Models.WebsiteModels.RfpDetailModel();
            try
            {
                model.RfpDeatil = cont.GetRfpById(BidID);
                model.BidList = cont.GetRfpBidsByRfpId(BidID);
            }
            catch (Exception)
            {
                return View(new List<Rfp>());
            }
            return View(model);
        }

        public IActionResult Unauthorized()
        {
            return View();
        }

    }
}
