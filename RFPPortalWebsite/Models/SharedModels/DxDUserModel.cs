using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.SharedModels
{
    public class DxDUserModel
    {
        public bool success { get; set; }
        public DxDUser User { get; set; }

        public class DxDUser
        {
            public string user_id { get; set; }
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string forum_name { get; set; }
        }

    }

}
