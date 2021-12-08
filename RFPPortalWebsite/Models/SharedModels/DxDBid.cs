using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.SharedModels
{
    public class DxDBid
    {
        public string name { get; set; }
        public string forum { get; set; }
        public string email { get; set; }
        public string delivery_date { get; set; }
        public int amount_of_bid { get; set; }
        public string additional_note { get; set; }
    }
}
