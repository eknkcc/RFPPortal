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
        public SimpleResponse SubmitBid([FromBody] RfpBid model)
        {
            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfp = db.Rfps.Find(model.RfpID);

                    //Check if RfpID is a valid identity.
                    if (rfp == null || rfp.RfpID <= 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Invalid RfpID. Please post an existing RfpID." };
                    }

                    //Check if Rfp status is active.
                    if (rfp.Status != Enums.RfpStatusTypes.Public.ToString() && rfp.Status != Enums.RfpStatusTypes.Internal.ToString())
                    {
                        return new SimpleResponse() { Success = false, Message = "Rfp status must be 'Active' in order to post bid. Current Rfp status: " + rfp.Status };
                    }

                    //Check if user already has an existing bid
                    if (db.RfpBids.Count(x => x.UserId == HttpContext.Session.GetInt32("UserId") && x.RfpID == model.RfpID) > 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Bid already exists for given UserId. Please delete your bid first." };
                    }

                    //Check if public user trying to post bid on internal bid
                    if (rfp.Status == Enums.RfpStatusTypes.Internal.ToString() && HttpContext.Session.GetString("UserType") == Enums.UserIdentityType.Public.ToString())
                    {
                        return new SimpleResponse() { Success = false, Message = "Public users can't bid to internal bidding." };
                    }
                }

                model.UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                RfpBid bidResult = Methods.BidMethods.SubmitBid(model);

                if (bidResult.RfpBidID > 0)
                {
                    //Log
                    Program.monitizer.AddUserLog(model.UserId, UserLogType.Request, "User submitted a new bid for RFP: " + model.RfpID);

                    return new SimpleResponse() { Success = true, Message = "Bid submitted successfully.", Content = bidResult };
                }
                else
                {
                    return new SimpleResponse() { Success = false, Message = "An error occured while proccesing your request." };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new SimpleResponse() { Success = false, Message = "An error occured while proccesing your request." };
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
        public SimpleResponse DeleteBid([FromQuery] int RfpBidID)
        {
            try
            {
                //Validations
                RfpBid rfpbid = new RfpBid();
                Rfp rfp = new Rfp();

                using (rfpdb_context db = new rfpdb_context())
                {
                    rfpbid = db.RfpBids.Find(RfpBidID);
                    rfp = db.Rfps.Find(rfpbid.RfpID);

                    //Check if user is trying to delete bid for another user
                    if (HttpContext.Session.GetInt32("UserId") != rfpbid.UserId)
                    {
                        return new SimpleResponse() { Success = false, Message = "User identity mismatch in the request." };
                    }

                    //Check if bid identity is valid
                    if (rfpbid == null || rfpbid.RfpBidID <= 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Invalid RfpBidID. Please post an existing RfoBidID." };
                    }

                    //Check if user trying to delete a bid in an completed bidding.
                    if (rfp.Status != Enums.RfpStatusTypes.Internal.ToString() && rfp.Status != Enums.RfpStatusTypes.Public.ToString())
                    {
                        return new SimpleResponse() { Success = false, Message = "You can't delete the bid from completed bidding." };
                    }
                }

                //Delete bid
                bool result = Methods.BidMethods.DeleteBid(RfpBidID);
                if (result == true)
                {
                    //Log
                    Program.monitizer.AddUserLog(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")), UserLogType.Request, "User deleted bid for RFP: " + rfpbid.RfpID);

                    return new SimpleResponse() { Success = true, Message = "Rfp bid succesfully deleted." };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new SimpleResponse() { Success = false, Message = "An error occured while proccesing your request." };
        }

        /// <summary>
        ///  Edit RFP Bid record in database
        /// </summary>
        /// <param name="rfpbidid">RfpBid identity</param>
        /// <returns></returns>
        [Route("EditBid")]
        [HttpPost]
        [PublicUserAuthorization]
        public SimpleResponse EditBid([FromBody] RfpBid model)
        {
            try
            {
                //Validations
                RfpBid rfpbid = new RfpBid();
                Rfp rfp = new Rfp();

                using (rfpdb_context db = new rfpdb_context())
                {
                    rfpbid = db.RfpBids.Find(model.RfpBidID);
                    rfp = db.Rfps.Find(rfpbid.RfpID);

                    //Check if user is trying to edit bid for another user
                    if (HttpContext.Session.GetInt32("UserId") != rfpbid.UserId)
                    {
                        return new SimpleResponse() { Success = false, Message = "User identity mismatch in the request." };
                    }

                    //Check if bid identity is valid
                    if (rfpbid == null || rfpbid.RfpBidID <= 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Invalid RfpBidID. Please post an existing RfoBidID." };
                    }

                    //Check if user trying to delete a bid in an completed bidding.
                    if (rfp.Status != Enums.RfpStatusTypes.Internal.ToString() && rfp.Status != Enums.RfpStatusTypes.Public.ToString())
                    {
                        return new SimpleResponse() { Success = false, Message = "You can't delete the bid from completed bidding." };
                    }
                }

                //Edit bid
                var result = Methods.BidMethods.EditBid(model);
                if (result.RfpBidID > 0 )
                {
                    //Log
                    Program.monitizer.AddUserLog(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")), UserLogType.Request, "User deleted bid for RFP: " + rfpbid.RfpID);

                    return new SimpleResponse() { Success = true, Message = "Rfp bid succesfully edited." };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new SimpleResponse() { Success = false, Message = "An error occured while proccesing your request." };
        }

        /// <summary>
        ///  Choose winning bid from Rfp Bids with RfpBidID
        /// </summary>
        /// <param name="rfpbidid">Identity of the RfpBid</param>
        /// <returns></returns>
        [Route("ChooseWinningBid")]
        [HttpPut]
        [IpWhitelistAuthorization]
        public SimpleResponse ChooseWinningBid([FromQuery] int RfpBidID)
        {
            try
            {
                //Validations
                RfpBid rfpbid = new RfpBid();
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Check if bid identity is valid
                    rfpbid = db.RfpBids.Find(RfpBidID);
                    if (rfpbid == null || rfpbid.RfpID <= 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Invalid RfpBidID. Please post an existing RfpBidID." };
                    }

                    //Check if RFP identity is valid
                    var rfp = db.Rfps.Find(rfpbid.RfpID);
                    if (rfp == null || rfp.RfpID <= 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Invalid RfpID. Please post an existing RfpID." };
                    }
                }

                //Update RFP's winning bid
                bool result = Methods.BidMethods.ChooseWinningBid(rfpbid);
                if (result == true)
                {
                    //Log
                    Program.monitizer.AddUserLog(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")), UserLogType.Request, "Admin choose winning bid for RFP: " + rfpbid.RfpID + ", RfpBid: " + RfpBidID);

                    return new SimpleResponse() { Success = true, Message = "Rfp winning bid and status succesfully updated." };
                }

            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new SimpleResponse() { Success = false, Message = "An error occured while proccesing your request." };
        }

    }
}
