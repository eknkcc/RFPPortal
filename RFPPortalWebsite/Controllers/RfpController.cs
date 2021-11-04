using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.Constants;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.ViewModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Controllers
{
    /// <summary>
    ///  RfpController contains Rfp CRUD operations.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class RfpController : ControllerBase
    {
        /// <summary>
        ///  Returns list of RFPs in the database by status.
        ///  Returns all records in the database if status parameter is null or empty
        /// </summary>
        /// <param name="status">Status of the RFP</param>
        /// <returns>RFP List</returns>
        [Route("GetRfpsByStatus")]
        [HttpGet]
        public List<Rfp> GetRfpsByStatus(string status)
        {
            List<Rfp> model = new List<Rfp>();

            try
            {
                model = Methods.RfpMethods.GetRfpsByStatus(status);
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return model;
        }

        /// <summary>
        ///  Returns object of RFP for given identity.
        /// </summary>
        /// <param name="rfpid">RFP identity (Rfps table primary key)</param>
        /// <returns>Rfp single object</returns>
        [Route("GetRfpById")]
        [HttpGet]
        public Rfp GetRfpById(int rfpid)
        {
            Rfp model = new Rfp();

            try
            {
                model = Methods.RfpMethods.GetRfpById(rfpid);
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return model;
        }

        /// <summary>
        ///  Returns list of RFPs in the database by status with pagination.
        ///  Returns all paginated records in the database if status parameter is null or empty.
        /// </summary>
        /// <param name="status">Status of the RFP</param>
        /// <returns>RFP List with pagination entity</returns>
        [Route("GetRfpsByStatusPaged")]
        [HttpGet]
        public PagedList.Core.IPagedList<Rfp> GetRfpsByStatusPaged(string status, int page = 1, int pageCount = 30)
        {
            try
            {
                IPagedList<Rfp> lst = Methods.RfpMethods.GetRfpsByStatusPaged(status, page, pageCount);

                return lst;
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new PagedList<Rfp>(null, 1, 1);
        }

        /// <summary>
        ///  Returns list of RFP bids for given RFP by identity.
        /// </summary>
        /// <param name="rfpid">RFP identity (Rfps table primary key)</param>
        /// <returns>Bid List for given RfpId</returns>
        [Route("GetRfpBidsByRfpId")]
        [HttpGet]
        [InternalUserAuthorization]
        public List<RfpBidWUser> GetRfpBidsByRfpId(int rfpid)
        {
            List<RfpBidWUser> model = new List<RfpBidWUser>();

            try
            {
                model = Methods.RfpMethods.GetRfpBidsByRfpId(rfpid);
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return model;
        }

        /// <summary>
        ///  Post RFP to database
        ///  This method can only be accessed by third party admin in ip whitelist.
        /// </summary>
        /// <param name="model">Rfp model</param>
        /// <returns>Submitted RFP</returns>
        [Route("SubmitRfpForm")]
        [HttpPost]
        [IpWhitelistAuthorization]
        public AjaxResponse SubmitRfpForm(Rfp model)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    model.CreateDate = DateTime.Now;
                    model = Methods.RfpMethods.SubmitRfpForm(model);

                    if (model.RfpID > 0)
                        return new AjaxResponse() { Success = true, Message = "Rfp form succesfully posted.", Content = model };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new AjaxResponse() { Success = false, Message = "An error occured while proccesing your request." };
        }

        /// <summary>
        ///  Changes the status of the given Rfp record
        ///  This method can only be accessed by third party admin in ip whitelist.
        /// </summary>
        /// <param name="model">Rfp model</param>
        /// <returns>Updated RFP</returns>
        [Route("ChangeRfpStatus")]
        [HttpPut]
        [IpWhitelistAuthorization]
        public AjaxResponse ChangeRfpStatus(Rfp model)
        {
            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfp = db.Rfps.Find(model.RfpID);

                    if (rfp == null || rfp.RfpID <= 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid RfpID. Please post an existing RfpID." };
                    }

                    try
                    {
                        Enums.RfpStatusTypes type = (Enums.RfpStatusTypes)Enum.Parse(typeof(Enums.RfpStatusTypes), model.Status);
                    }
                    catch
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid status. Please post a valid status. Valid status codes: Pending, Active, Waiting, Completed" };
                    }
                }

                model = Methods.RfpMethods.ChangeRfpStatus(model);

                return new AjaxResponse() { Success = true, Message = "Rfp status succesfully updated.", Content = model };

            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new AjaxResponse() { Success = false, Message = "An error occured while proccesing your request." };
        }

    }
}
