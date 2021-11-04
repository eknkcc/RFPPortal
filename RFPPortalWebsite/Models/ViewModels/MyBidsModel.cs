using RFPPortalWebsite.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.ViewModels
{
    public class MyBidsModel
    {
        public int RfpID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public RfpBid Bid { get; set; }
        public int? WinnerRfpBidID { get; set; }
    }
}
