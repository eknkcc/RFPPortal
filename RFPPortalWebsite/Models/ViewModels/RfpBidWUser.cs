using RFPPortalWebsite.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.ViewModels
{
    public class RfpBidWUser
    {
        public RfpBid Bid { get; set; }
        public string Username { get; set; }
        public string UserType { get; set; }

    }
}
