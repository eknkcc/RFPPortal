using PagedList.Core;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Methods
{
    /// <summary>
    ///  Rfp methods bussiness layer
    /// </summary>
    public class RfpMethods
    {
        /// <summary>
        ///  Returns list of RFPs in the database by status.
        ///  Returns all records in the database if status parameter is null or empty
        /// </summary>
        /// <param name="status">Status of the RFP</param>
        /// <returns>RFP List</returns>
        public static List<Rfp> GetRfpsByStatus(string status)
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
        ///  Returns object of RFP for given identity.
        /// </summary>
        /// <param name="rfpid">RFP identity (Rfps table primary key)</param>
        /// <returns>Rfp single object</returns>
        public static Rfp GetRfpById(int rfpid)
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
        /// <returns>RFP List with pagination entity</returns>
        public static PagedList.Core.IPagedList<Rfp> GetRfpsByStatusPaged(string status, int page = 1, int pageCount = 30)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        IPagedList<Rfp> lst = db.Rfps.Where(x => x.Status == status).OrderByDescending(x => x.RfpID).ToPagedList(page, pageCount);
                        return lst;
                    }
                    else
                    {
                        IPagedList<Rfp> lst = db.Rfps.OrderByDescending(x => x.RfpID).ToPagedList(page, pageCount);
                        return lst;
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new PagedList<Rfp>(null, 1, 1);
        }

        /// <summary>
        ///  Returns list of RFPs in the database by type (internal or public) with pagination.
        ///  Returns all paginated records in the database if type parameter is null or empty.
        /// </summary>
        /// <param name="type">Type of the RFP (Internal or Public)</param>
        /// <returns>RFP List with pagination entity</returns>
        public static PagedList.Core.IPagedList<Rfp> GetRfpsByTypePaged(Models.Constants.Enums.RfpStatusTypes? type, int page = 1, int pageCount = 30)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    if(type == null)
                    {
                        IPagedList<Rfp> lst = db.Rfps.OrderByDescending(x => x.RfpID).ToPagedList(page, pageCount);
                        return lst;
                    }
                    else if (type == RfpStatusTypes.Internal)
                    {
                        var dt = DateTime.Now.AddDays(-Program._settings.InternalBiddingDays);

                        IPagedList<Rfp> lst = db.Rfps.Where(x => DateTime.Now < x.CreateDate.AddDays(Program._settings.InternalBiddingDays)).OrderByDescending(x => x.RfpID).ToPagedList(page, pageCount);
                        return lst;
                    }
                    else if (type == RfpStatusTypes.Public)
                    {
                        var dt = DateTime.Now.AddDays(-Program._settings.InternalBiddingDays);
                        IPagedList<Rfp> lst = db.Rfps.Where(x => dt > x.CreateDate).OrderByDescending(x => x.RfpID).ToPagedList(page, pageCount);
                        
                        return lst;
                    }
                }
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
        public static List<RfpBidWUser> GetRfpBidsByRfpId(int rfpid)
        {
            List<RfpBidWUser> model = new List<RfpBidWUser>();

            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    model = (from bid in db.RfpBids
                             join user in db.Users on bid.UserId equals user.UserId
                             where bid.RfpID == rfpid
                             select new RfpBidWUser
                             {
                                 Bid = bid,
                                 Username = user.UserName
                             }).ToList();
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
        ///  This method can only be accessed by third party admin in ip whitelist.
        /// </summary>
        /// <param name="model">Rfp model</param>
        /// <returns>Submitted RFP</returns>
        public static Rfp SubmitRfpForm(Rfp model)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    model.CreateDate = DateTime.Now;
                    db.Rfps.Add(model);
                    db.SaveChanges();

                    //Logging
                    Program.monitizer.AddUserLog(model.UserId, Models.Constants.Enums.UserLogType.Auth, "Post RFP successful. RfpID: " + model.RfpID);

                    return model;
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new Rfp();
        }

        /// <summary>
        ///  Changes the status of the given Rfp record
        ///  This method can only be accessed by third party admin in ip whitelist.
        /// </summary>
        /// <param name="model">Rfp model</param>
        /// <returns>Updated RFP</returns>
        public static Rfp ChangeRfpStatus(Rfp model)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfp = db.Rfps.Find(model.RfpID);
                    rfp.Status = model.Status;
                    db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();

                    //Logging
                    Program.monitizer.AddUserLog(model.UserId, Models.Constants.Enums.UserLogType.Auth, "Change RFP status successful. RfpID: " + model.RfpID+ " New Status: " + model.Status);

                    return rfp;
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return new Rfp();
        }
    }
}
