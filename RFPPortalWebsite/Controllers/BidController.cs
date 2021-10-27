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
        public AjaxResponse SubmitBid(RfpBid model)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfp = db.Rfps.Find(model.RfpID);

                    if (rfp == null || rfp.RfpID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpID. Please post an existing RfpID." };
                    }

                    if (rfp.Status != Enums.RfpStatusTypes.Active.ToString())
                    {
                        return new AjaxResponse() { Success = false, Message = "Rfp status must be 'Active' in order to post bid. Current Rfp status: " + rfp.Status };
                    }

                    if (db.RfpBids.Count(x => x.UserId == model.UserId && x.RfpID == model.RfpID) > 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Bid already exists for given UserID. Please delete your bid first." };
                    }

                    model.CreateDate = DateTime.Now;
                    db.RfpBids.Add(model);
                    db.SaveChanges();

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
        public AjaxResponse DeleteBid([FromQuery] int RfpBidID)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfpbid = db.RfpBids.Find(RfpBidID);

                    if (rfpbid == null || rfpbid.RfpBidID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpBidID. Please post an existing RfoBidID." };
                    }

                    db.RfpBids.Remove(rfpbid);
                    db.SaveChanges();

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
        [AdminAuthorization]
        public AjaxResponse ChooseWinningBid([FromQuery] int RfpBidID)
        {

            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfpbid = db.RfpBids.Find(RfpBidID);
                    if (rfpbid == null || rfpbid.RfpID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpBidID. Please post an existing RfpBidID." };
                    }

                    var rfp = db.Rfps.Find(rfpbid.RfpID);
                    if (rfp == null || rfp.RfpID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpID. Please post an existing RfpID." };
                    }

                    rfp.WinnerRfpBidID = RfpBidID;
                    rfp.Status = Enums.RfpStatusTypes.Completed.ToString();

                    db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();

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
