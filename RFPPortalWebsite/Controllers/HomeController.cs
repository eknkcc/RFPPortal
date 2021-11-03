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
        public IActionResult Rfps(int Page = 1)
        {
           
            PagedList.Core.IPagedList<Rfp> model = new PagedList<Rfp>(null, 1, 1);
            model = Methods.RfpMethods.GetRfpsByStatusPaged("", Page, 5);

            ViewBag.Message = "Request For Proposals";
            return View(model);
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
            ViewBag.Message = "RFP Detail";
            return View(model);
        }

        [Route("SignIn")]
        public IActionResult SignIn(string email,string pass)
        {
            User usr = Methods.AuthMethods.GetUserInfo(email, pass);
            if (usr.UserId > 0)
            {
                HttpContext.Session.SetInt32("UserId", usr.UserId);
                HttpContext.Session.SetString("UserType", usr.UserType);
                HttpContext.Session.SetString("NameSurname", usr.NameSurname);
                HttpContext.Session.SetString("Email", usr.Email);
                HttpContext.Session.SetString("Username", usr.UserName);
                return Json(new AjaxResponse() { Success = true, Message = "Sign in successful.", Content = new { User = usr } });
            }

            return Json(new AjaxResponse() { Success = false, Message = "Sign in failed." });
        }

        [Route("SignUp")]
        public IActionResult SignUp(RegisterModel rgstr)
        {
            if (rgstr.Password != rgstr.Password)
            {
                return Json(new AjaxResponse { Success = false, Message = "Passwords are not compatible." });
            }
            AuthController cont = new AuthController();
            AjaxResponse resp = new AjaxResponse();
            try
            {
                resp = cont.RegisterUser(rgstr);
            }
            catch 
            {
                return Json(new AjaxResponse() { Success = true, Message = "Sign up successful." });
            }
            return Json(resp);
        }

        [AdminUserAuthorization]
        [Route("Rfp-Form")]
        public IActionResult Rfp_Form()
        {
            ViewBag.Message = "RFP Form";
            return View();
        }
        [UserAuthorization]
        [Route("Bidded-Rfps")]
        public IActionResult Bidded_Rfps()
        {
            ViewBag.Message = "Bidded RFPs";
            return View();
        }
        [AdminUserAuthorization]
        [Route("SubmitForm")]
        public IActionResult SubmitForm(Rfp model)
        {
            model.UserId = (int)HttpContext.Session.GetInt32("UserId");
            model.CreateDate = DateTime.Now;
            model.Status = Models.Constants.Enums.RfpStatusTypes.Active.ToString();

            Rfp usr = Methods.RfpMethods.SubmitRfpForm(model);
            if (usr.RfpID > 0)
            {
                return Json(new AjaxResponse() { Success = true, Message = "Proposal submitted successfully." });
            }

            return Json(new AjaxResponse() { Success = false, Message = "Submission failed." });
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
