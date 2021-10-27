using Helpers.Models.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.Constants;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Controllers
{
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
                using (rfpdb_context db = new rfpdb_context())
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        model = db.Rfps.Where(x => x.Status == status).ToList();
                    }
                    else
                    {
                        model = db.Rfps.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return model;
        }

        /// <summary>
        ///  Returns object of RFP bids for given RFP by identity.
        /// </summary>
        /// <param name="rfpid">RFP identity (Rfps table primary key)</param>
        /// <returns>RFP Bid Object</returns>
        [Route("GetRfpById")]
        [HttpGet]
        public Rfp GetRfpById(int rfpid)
        {
            Rfp model = new Rfp();

            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    model = db.Rfps.SingleOrDefault(x => x.RfpID == rfpid);
                }
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
        /// <returns>RFP List</returns>
        [Route("GetRfpsByStatusPaged")]
        [HttpGet]
        public PaginationEntity<Rfp> GetRfpsByStatusPaged(string status, int page = 1, int pageCount = 30)
        {
            PaginationEntity<Rfp> res = new PaginationEntity<Rfp>();

            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        IPagedList<Rfp> lst = db.Rfps.Where(x => x.Status == status).OrderByDescending(x => x.RfpID).ToPagedList(page, pageCount);

                        res.Items = lst;
                        res.MetaData = new PaginationMetaData() { Count = lst.Count, FirstItemOnPage = lst.FirstItemOnPage, HasNextPage = lst.HasNextPage, HasPreviousPage = lst.HasPreviousPage, IsFirstPage = lst.IsFirstPage, IsLastPage = lst.IsLastPage, LastItemOnPage = lst.LastItemOnPage, PageCount = lst.PageCount, PageNumber = lst.PageNumber, PageSize = lst.PageSize, TotalItemCount = lst.TotalItemCount };
                    }
                    else
                    {
                        IPagedList<Rfp> lst = db.Rfps.OrderByDescending(x => x.RfpID).ToPagedList(page, pageCount);

                        res.Items = lst;
                        res.MetaData = new PaginationMetaData() { Count = lst.Count, FirstItemOnPage = lst.FirstItemOnPage, HasNextPage = lst.HasNextPage, HasPreviousPage = lst.HasPreviousPage, IsFirstPage = lst.IsFirstPage, IsLastPage = lst.IsLastPage, LastItemOnPage = lst.LastItemOnPage, PageCount = lst.PageCount, PageNumber = lst.PageNumber, PageSize = lst.PageSize, TotalItemCount = lst.TotalItemCount };
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return res;
        }

        /// <summary>
        ///  Returns list of RFP bids for given RFP by identity.
        /// </summary>
        /// <param name="rfpid">RFP identity (Rfps table primary key)</param>
        /// <returns>RFP Bid List</returns>
        [Route("GetRfpBidsByRfpId")]
        [HttpGet]
        public List<RfpBid> GetRfpBidsByRfpId(int rfpid)
        {
            List<RfpBid> model = new List<RfpBid>();

            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {

                    model = db.RfpBids.Where(x => x.RfpID == rfpid).ToList();
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return model;
        }

        /// <summary>
        ///  Post RFP to database
        /// </summary>
        /// <param name="model">Rfp model</param>
        /// <returns></returns>
        [Route("SubmitRfpForm")]
        [HttpPost]
        public AjaxResponse SubmitRfpForm(Rfp model)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    model.CreateDate = DateTime.Now;
                    db.Rfps.Add(model);
                    db.SaveChanges();

                    return new AjaxResponse() { Success = true, Message = "Rfp form succesfully posted.", Content = model };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new AjaxResponse() { Success = false, Message = "An error occured while proccesing your request." };
            }
        }

        /// <summary>
        ///  Changes the status of the given Rfp record
        /// </summary>
        /// <param name="model">Rfp model</param>
        /// <returns></returns>
        [Route("ChangeRfpStatus")]
        [HttpPut]
        public AjaxResponse ChangeRfpStatus(Rfp model)
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

                    try
                    {
                        Enums.RfpStatusTypes type = (Enums.RfpStatusTypes)Enum.Parse(typeof(Enums.RfpStatusTypes), model.Status);
                    }
                    catch
                    {
                        return new AjaxResponse() { Success = false, Message = "Invalid status. Please post a valid status. Valid status codes: Pending, Active, Waiting, Completed" };
                    }

                    rfp.Status = model.Status;

                    db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();

                    return new AjaxResponse() { Success = true, Message = "Rfp status succesfully updated.", Content = model };
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
