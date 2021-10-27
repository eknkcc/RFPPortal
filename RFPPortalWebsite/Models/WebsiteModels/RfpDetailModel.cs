using RFPPortalWebsite.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.WebsiteModels
{
    public class RfpDetailModel
    {
        public List<RfpBid> BidList { get; set; } = new List<RfpBid>();
        public Rfp RfpDeatil { get; set; }

    }
}
