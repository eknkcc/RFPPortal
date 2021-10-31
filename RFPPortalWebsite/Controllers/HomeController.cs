using Helpers.Models.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PagedList.Core;
using RFPPortalWebsite.Methods;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.ViewModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [Route("Rfps")]
        [Route("Rfps/{Page}")]
        [Route("Rfps/{Page}/{AuthKey}")]
        public IActionResult Rfps(int Page = 1, string AuthKey = "")
        {
            if (!string.IsNullOrEmpty(AuthKey))
            {
                SignIn(AuthKey);
            }

            PagedList.Core.IPagedList<Rfp> model = new PagedList<Rfp>(null, 1, 1);

            model = Methods.RfpMethods.GetRfpsByStatusPaged("", Page, 5);

            return View(model);
        }

        [Route("RFP-Detail/{BidID}")]
        [Route("RFP-Detail/{BidID}/{AuthKey}")]
        public IActionResult RFP_Detail(int BidID, string AuthKey)
        {
            if (!string.IsNullOrEmpty(AuthKey))
            {
                SignIn(AuthKey);
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

        [Route("SignIn/{AuthKey}")]
        public IActionResult SignIn(string AuthKey)
        {
            User usr = Methods.AuthMethods.GetUserInfo(AuthKey);
            if(usr.UserId > 0)
            {
                HttpContext.Session.SetString("AuthKey", usr.AuthKey);
                HttpContext.Session.SetInt32("UserId", usr.UserId);
                HttpContext.Session.SetString("UserType", usr.UserType);
                HttpContext.Session.SetString("NameSurname", usr.NameSurname);
                HttpContext.Session.SetString("Email", usr.Email);
                HttpContext.Session.SetString("Username", usr.UserName);
                return Json(new AjaxResponse() { Success = true, Message = "Sign in successful.", Content = new { User = usr } });
            }

            return Json(new AjaxResponse() { Success = false, Message = "Sign in failed." });
        }

        public IActionResult Unauthorized()
        {
            return View();
        }

        //[HttpPost("SaveBid", Name = "SaveBid")]
        //[PublicUserAuthorization]
        //public JsonResult SaveBid([FromBody] NewBidModel NewBid)
        //{
        //    try
        //    {
        //        RfpBid bid = BidMethods.SubmitBid(new RfpBid() { Amount = NewBid.Amount, CreateDate = DateTime.Now, Note = NewBid.Note, Time = NewBid.TimeFrame, UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId")), RfpID = NewBid.RfpID });
        //        return Json(bid);
        //    }
        //    catch (Exception ex)
        //    {
        //        Program.monitizer.AddException(ex, LogTypes.ApplicationError);
        //        return Json(new AjaxResponse() { Success = false, Message = "Unexpected error" });
        //    }
        //}


    }
}
