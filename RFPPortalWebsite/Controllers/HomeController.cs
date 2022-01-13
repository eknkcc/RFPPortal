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
using Stripe;
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

            model = Methods.RfpMethods.GetRfpsByStatusPaged(null, Page, 5);

            ViewBag.PageTitle = "DEVxDAO - Request for Proposals Portal";
            return View(model);
        }

        [UserAuthorization]
        [Route("Rfp-Form")]
        public IActionResult Rfp_Form()
        {
            if(HttpContext.Session.GetString("RfpFormKey") == null)
            {
                if(HttpContext.Session.GetString("UserType") != Models.Constants.Enums.UserIdentityType.Admin.ToString())
                    return View("Unauthorized");
                else
                    return View();
            }
            else
            {
                ViewBag.PageTitle = "RFP Form";
                return View();
            }
       
        }

        [UserAuthorization]
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

        [UserAuthorization]
        [HttpPost]
        public IActionResult DosFeeProcess(string stripeToken, string stripeEmail, int rfpid, int rfpbidid)
        {
            try
            {
                Dictionary<string, string> Metadata = new Dictionary<string, string>();
                Metadata.Add("Product", "RFP Bid DoS Fee");
                Metadata.Add("Quantity", "1");
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt64(Program._settings.DosFee * 100),
                    Currency = "USD",
                    Description = "Rfp bid DoS fee payment. RfpID: " + rfpid + ", RfpBidId: " + rfpbidid,
                    Source = stripeToken,
                    ReceiptEmail = stripeEmail,
                    Metadata = Metadata
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Paid)
                {
                    using (rfpdb_context db = new rfpdb_context())
                    {
                        var bid = db.RfpBids.Find(rfpbidid);
                        bid.DosPaid = true;
                        db.SaveChanges();
                    }

                    Program.monitizer.AddUserLog(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")), UserLogType.Request, "DoS fee payment result. Sucessful.  RfpID: " + rfpid + ", RfpBidId: " + rfpbidid);

                    TempData["toastr-message"] = "Payment succesful. Your bid is now validated.";
                    TempData["toastr-type"] = "success";
                }
                else
                {
                    Program.monitizer.AddUserLog(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")), UserLogType.Request, "DoS fee payment result. " + charge.FailureMessage + "  RfpID: " + rfpid + ", RfpBidId: " + rfpbidid);

                    TempData["toastr-message"] = "Payment failed. Err: " + charge.FailureMessage;
                    TempData["toastr-type"] = "error";
                }

            }
            catch (StripeException ex)
            {
                TempData["toastr-message"] = "Payment failed. Err: " + ex.Message;
                TempData["toastr-type"] = "error";

                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
            }

            return Redirect("../RFP-Detail/" + rfpid);
        }

        [UserAuthorization]
        [HttpGet]
        public SimpleResponse CheckRFPFormKey(string key)
        {
            SimpleResponse res = new SimpleResponse();
            if (key == Program._settings.RfpKey)
            {
                try
                {
                    HttpContext.Session.SetString("RfpFormKey", key);
                    res.Success = true;
                }
                catch (Exception)
                {
                    res.Success = false;
                    res.Message = "An error occurred during the process. Please try again later.";
                }
            }
            else
            {
                res.Success = false;
                res.Message = "Invalid key. Please try again.";
            }

            return res;
        }
    }
}
