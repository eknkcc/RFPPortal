using Helpers.Models.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.Constants;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Controllers
{
    /// <summary>
    ///  BidController contains Bid CRUD operations.
    ///  SubmitBid and DeleteBid methods can be accesed by PublicUserAuthorization.
    ///  User must be signed in with AuthKey and user session must be created before calling SubmitBid and DeleteBid methods.
    ///  ChooseWinningBid method can only be accessed by third party admin in ip whitelist.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        /// <summary>
        ///  Post RFP Bid to database
        /// </summary>
        /// <param name="model">RfpBid model</param>
        /// <returns></returns>
        [Route("SubmitBid")]
        [HttpPost]
        [PublicUserAuthorization]
        public AjaxResponse SubmitBid(RfpBid model)
        {
            try
            {
                //Check if user is trying to submit bid for another user
                if (HttpContext.Session.GetInt32("UserID") != model.UserId)
                {
                    return new AjaxResponse() { Success = false, Message = "User identity mismatch in the request." };
                }

                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfp = db.Rfps.Find(model.RfpID);

                    //Check if RfpID is a valid identity.
                    if (rfp == null || rfp.RfpID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpID. Please post an existing RfpID." };
                    }

                    //Check if Rfp status is active.
                    if (rfp.Status != Enums.RfpStatusTypes.Active.ToString())
                    {
                        return new AjaxResponse() { Success = false, Message = "Rfp status must be 'Active' in order to post bid. Current Rfp status: " + rfp.Status };
                    }

                    //Check if user already has an existing bid
                    if (db.RfpBids.Count(x => x.UserId == model.UserId && x.RfpID == model.RfpID) > 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Bid already exists for given UserID. Please delete your bid first." };
                    }

                    //Post bid to database
                    model.CreateDate = DateTime.Now;
                    db.RfpBids.Add(model);
                    db.SaveChanges();

                    //Log
                    Program.monitizer.AddUserLog(Convert.ToInt32(HttpContext.Session.GetInt32("UserID")), UserLogType.Request, "User submitted a new bid for RFP: " + model.RfpID);

                    return new AjaxResponse() { Success = true, Message = "Rfp bid succesfully posted.", Content = model };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new AjaxResponse() { Success = false, Message = "An error occured while proccesing your request." };
            }
        }

        /// <summary>
        ///  Delete RFP Bid record from database
        /// </summary>
        /// <param name="rfpbidid">RfpBid identity</param>
        /// <returns></returns>
        [Route("DeleteBid")]
        [HttpDelete]
        [PublicUserAuthorization]
        public AjaxResponse DeleteBid([FromQuery] int RfpBidID)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfpbid = db.RfpBids.Find(RfpBidID);

                    //Check if user is trying to delete bid for another user
                    if (HttpContext.Session.GetInt32("UserID") != rfpbid.UserId)
                    {
                        return new AjaxResponse() { Success = false, Message = "User identity mismatch in the request." };
                    }

                    //Check if bid identity is valid
                    if (rfpbid == null || rfpbid.RfpBidID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpBidID. Please post an existing RfoBidID." };
                    }

                    //Delete bid from database
                    db.RfpBids.Remove(rfpbid);
                    db.SaveChanges();

                    //Log
                    Program.monitizer.AddUserLog(Convert.ToInt32(HttpContext.Session.GetInt32("UserID")), UserLogType.Request, "User deleted bid for RFP: " + rfpbid.RfpID);

                    return new AjaxResponse() { Success = true, Message = "Rfp bid succesfully deleted." };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new AjaxResponse() { Success = false, Message = "An error occured while proccesing your request." };
            }
        }

        /// <summary>
        ///  Choose winning bid from Rfp Bids with RfpBidID
        /// </summary>
        /// <param name="rfpbidid">Identity of the RfpBid</param>
        /// <returns></returns>
        [Route("ChooseWinningBid")]
        [HttpPut]
        [IpWhitelistAuthorization]
        public AjaxResponse ChooseWinningBid([FromQuery] int RfpBidID)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Check if bid identity is valid
                    var rfpbid = db.RfpBids.Find(RfpBidID);
                    if (rfpbid == null || rfpbid.RfpID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpBidID. Please post an existing RfpBidID." };
                    }

                    //Check if RFP identity is valid
                    var rfp = db.Rfps.Find(rfpbid.RfpID);
                    if (rfp == null || rfp.RfpID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpID. Please post an existing RfpID." };
                    }

                    //Rfp winner bid database update
                    rfp.WinnerRfpBidID = RfpBidID;
                    rfp.Status = Enums.RfpStatusTypes.Completed.ToString();

                    db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();

                    //Log
                    Program.monitizer.AddUserLog(Convert.ToInt32(HttpContext.Session.GetInt32("UserID")), UserLogType.Request, "Admin choose winning bid for RFP: " + rfpbid.RfpID + ", RfpBid: " + RfpBidID);

                    return new AjaxResponse() { Success = true, Message = "Rfp winning bid and status succesfully updated.", Content = rfp };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new AjaxResponse() { Success = false, Message = "An error occured while proccesing your request." };
            }
        }

    }
}
