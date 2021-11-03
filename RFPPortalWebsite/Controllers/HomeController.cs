using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PagedList.Core;
using RFPPortalWebsite.Methods;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
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
                model = Methods.RfpMethods.GetRfpsByTypePaged(null, Page, 5);
            }
            else
            {
                model = Methods.RfpMethods.GetRfpsByTypePaged(Models.Constants.Enums.RfpStatusTypes.Public, Page, 5);
            }

            ViewBag.PageTitle = "Request For Proposals";
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
                return Json(new AjaxResponse() { Success = true, Message = "Proposal submitted successfully." });
            }

            return Json(new AjaxResponse() { Success = false, Message = "Submission failed." });
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
            ViewBag.PageTitle = "RFP Detail";
            return View(model);
        }

        [UserAuthorization]
        [Route("My-Bids")]
        public IActionResult My_Bids()
        {
            ViewBag.PageTitle = "User's Bids";
            return View();
        }


        #region Login & Register Methods

        /// <summary>
        ///  User login function
        /// </summary>
        /// <param name="email">User's email or username</param>
        /// <param name="password">User's password</param>
        /// <param name="usercode">Captcha code (Needed after 3 failed requests)</param>
        /// <returns></returns>
        [Route("SignIn")]
        public IActionResult SignIn(string email, string pass)
        {
            User usr = Methods.AuthMethods.UserSignIn(email, pass);
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

        /// <summary>
        /// Completes user registration from activation link in the confirmation email
        /// </summary>
        /// <param name="str">Encrypted user information in the registration email</param>
        /// <returns></returns>
        [Route("RegisterCompleteView")]
        public IActionResult RegisterCompleteView(string str)
        {
            AjaxResponse resp = new AjaxResponse();

            User usr = AuthMethods.RegisterComplete(str);

            if (usr.UserId > 0)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "User activation successful.";
            }
            else
            {
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Invalid user activation request.";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// User logout function
        /// </summary>
        /// <returns></returns>
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        #endregion

    }
}
