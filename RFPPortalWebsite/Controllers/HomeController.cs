using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PagedList.Core;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Methods;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.ViewModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Rfps");
        }

        public IActionResult Unauthorized()
        {
            return View();
        }

        [Route("Rfps")]
        [Route("Rfps/{Page}")]
        public IActionResult Rfps(int Page = 1)
        {
            PagedList.Core.IPagedList<Rfp> model = new PagedList<Rfp>(null, 1, 1);

            if (HttpContext.Session.GetString("UserType") == Models.Constants.Enums.UserIdentityType.Internal.ToString() || HttpContext.Session.GetString("UserType") == Models.Constants.Enums.UserIdentityType.Admin.ToString())
            {
                model = Methods.RfpMethods.GetRfpsByStatusPaged(null, Page, 5);
            }
            else
            {
                model = Methods.RfpMethods.GetRfpsByStatusPaged(Models.Constants.Enums.RfpStatusTypes.Public.ToString(), Page, 5);
            }

            ViewBag.PageTitle = "DEVxDAO - Request for Proposals Portal";
            return View(model);
        }

        [AdminUserAuthorization]
        [Route("Rfp-Form")]
        public IActionResult Rfp_Form()
        {
            ViewBag.PageTitle = "RFP Form";
            return View();
        }

        [AdminUserAuthorization]
        [Route("SubmitForm")]
        public IActionResult SubmitForm(Rfp model)
        {
            model.UserId = (int)HttpContext.Session.GetInt32("UserId");
            model.CreateDate = DateTime.Now;
            model.Status = Models.Constants.Enums.RfpStatusTypes.Internal.ToString();

            Rfp usr = Methods.RfpMethods.SubmitRfpForm(model);
            if (usr.RfpID > 0)
            {
                return Json(new SimpleResponse() { Success = true, Message = "Proposal submitted successfully." });
            }

            return Json(new SimpleResponse() { Success = false, Message = "Submission failed." });
        }

        [Route("RFP-Detail/{BidID}")]
        public IActionResult RFP_Detail(int BidID)
        {
            RfpController cont = new RfpController();
            Models.ViewModels.RfpDetailModel model = new Models.ViewModels.RfpDetailModel();
            try
            {
                model.RfpDeatil = cont.GetRfpById(BidID);
                model.BidList = cont.GetRfpBidsByRfpId(BidID);

                if (model.RfpDeatil.Status == Models.Constants.Enums.RfpStatusTypes.Internal.ToString() && HttpContext.Session.GetString("UserType") == Models.Constants.Enums.UserIdentityType.Public.ToString())
                {
                    return RedirectToAction("Unauthorized");
                }
            }
            catch (Exception)
            {
                return View(new List<Rfp>());
            }
            ViewBag.PageTitle = "RFP Detail";
            return View(model);
        }

        [UserAuthorization]
        [Route("My-Bids")]
        public IActionResult My_Bids()
        {
            ViewBag.PageTitle = "My Bids";

            List<MyBidsModel> model = Methods.BidMethods.GetUserBids(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")));

            return View(model);
        }

    }
}
