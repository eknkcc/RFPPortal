using Helpers.Models.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PagedList.Core;
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
                AuthController authCont = new AuthController();
                AjaxResponse response = authCont.GetUserInfo(AuthKey);
                if (response.Success == true)
                {
                    var userObj = response.Content as User;
                    HttpContext.Session.SetString("AuthKey", userObj.AuthKey);
                    HttpContext.Session.SetInt32("UserId", userObj.UserId);
                    HttpContext.Session.SetString("UserType", userObj.UserType);
                    HttpContext.Session.SetString("NameSurname", userObj.NameSurname);
                    HttpContext.Session.SetString("Email", userObj.Email);
                    HttpContext.Session.SetString("Username", userObj.UserName);
                }
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
                AuthController authCont = new AuthController();
                AjaxResponse response = authCont.GetUserInfo(AuthKey);
                if (response.Success == true)
                {
                    var userObj = response.Content as User;
                    HttpContext.Session.SetString("AuthKey", userObj.AuthKey);
                    HttpContext.Session.SetInt32("UserId", userObj.UserId);
                    HttpContext.Session.SetString("UserType", userObj.UserType);
                    HttpContext.Session.SetString("NameSurname", userObj.NameSurname);
                    HttpContext.Session.SetString("Email", userObj.Email);
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

        [HttpPost("RegisterPublicUser", Name = "RegisterPublicUser")]
        public JsonResult RegisterPublicUser([FromBody] RegisterModel registerInput)
        {
            try
            {
                AuthController authCont = new AuthController();
                registerInput.ip = Utility.IpHelper.GetClientIpAddress(HttpContext);
                registerInput.port = Utility.IpHelper.GetClientIpAddress(HttpContext);
                AjaxResponse response = authCont.RegisterPublic(registerInput);
                if (response.Success == true)
                {
                    dynamic content = response.Content;
                    string AuthKey = content.AuthKey.ToString();
                    AjaxResponse response2 = authCont.GetUserInfo(AuthKey);
                    dynamic d = response2.Content;
                    User userObj = d.User as User;
                    HttpContext.Session.SetString("AuthKey", userObj.AuthKey);
                    HttpContext.Session.SetInt32("UserId", userObj.UserId);
                    HttpContext.Session.SetString("UserType", userObj.UserType);
                    HttpContext.Session.SetString("NameSurname", userObj.NameSurname);
                    HttpContext.Session.SetString("Email", userObj.Email);
                    HttpContext.Session.SetString("Username", userObj.UserName);
                }
                return Json(response);
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return Json(new AjaxResponse() { Success = false, Message = "Unexpected error" });
            }
        }

        [HttpPost("SaveBid", Name = "SaveBid")]
        public JsonResult SaveBid([FromBody] NewBidModel NewBid)
        {
            try
            {
                BidController cont = new BidController();
                AjaxResponse responseFinal = cont.SubmitBid(new RfpBid() { Amount = NewBid.Amount, CreateDate = DateTime.Now, Note = NewBid.Note, Time = NewBid.TimeFrame, UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId")), RfpID = NewBid.RfpID });
                return Json(responseFinal);

            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return Json(new AjaxResponse() { Success = false, Message = "Unexpected error" });
            }
        }

        [HttpGet]
        public JsonResult GetUserFromBidCode(string BidCode)
        {

            return Json("");
        }

    }
}
