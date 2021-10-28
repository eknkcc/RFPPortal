using Helpers.Models.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Utility;
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
            return RedirectToAction("Rfps");
        }

        [Route("Rfps")]
        [Route("Rfps/{AuthKey}")]
        public IActionResult Rfps(string AuthKey)
        {
            if (!string.IsNullOrEmpty(AuthKey))
            {
                AuthController authCont = new AuthController();
                AjaxResponse response = authCont.GetUserInfo(AuthKey);
                if (response.Success == true)
                {
                    var userObj = response.Content as User;
                    HttpContext.Session.SetInt32("UserId", userObj.UserId);
                    HttpContext.Session.SetString("UserType", userObj.UserType);
                    HttpContext.Session.SetString("Username", userObj.UserName);
                }
            }

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
        [Route("RFP-Detail/{BidID}/{AuthKey}")]
        public IActionResult RFP_Detail(int BidID, string AuthKey)
        {
            if (!string.IsNullOrEmpty(AuthKey))
            {
                AuthController authCont = new AuthController();
                AjaxResponse response = authCont.GetUserInfo(AuthKey);
                if (response.Success == true)
                {
                    var userObj = response.Content as User;
                    HttpContext.Session.SetInt32("UserId", userObj.UserId);
                    HttpContext.Session.SetString("UserType", userObj.UserType);
                    HttpContext.Session.SetString("Username", userObj.UserName);
                }
            }

            RfpController cont = new RfpController();
            Models.ViewModels.RfpDetailModel model = new Models.ViewModels.RfpDetailModel();
            try
            {
                model.RfpDeatil = cont.GetRfpById(BidID);
                model.BidList = cont.GetRfpBidsByRfpId(BidID);

                if (model.RfpDeatil.CreateDate.AddDays(Program._settings.InternalBiddingDays) >= DateTime.Now)
                {
                    model.BiddingType = "Internal Bidding";
                    model.TimeRemaining = model.RfpDeatil.CreateDate.AddDays(Program._settings.InternalBiddingDays);
                }
                else if (DateTime.Now >= model.RfpDeatil.CreateDate.AddDays(Program._settings.InternalBiddingDays) &&
                DateTime.Now <= model.RfpDeatil.CreateDate.AddDays(Program._settings.InternalBiddingDays + Program._settings.PublicBiddingDays))
                {
                    model.BiddingType = "Public Bidding";
                    model.TimeRemaining = model.RfpDeatil.CreateDate.AddDays(Program._settings.InternalBiddingDays + Program._settings.PublicBiddingDays);
                }
                else
                {
                    model.BiddingType = "Bidding Ended";
                }

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
