using RFPPortalWebsite.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.ViewModels
{
    public class RfpDetailModel
    {
        public List<RfpBidWUser> BidList { get; set; } = new List<RfpBidWUser>();
        public Rfp RfpDeatil { get; set; }
        public DateTime TimeRemaining { get; set; }
        public string BiddingType { get; set; }

    }
}
