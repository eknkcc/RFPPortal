using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.SharedModels
{
    public class DxDSurvey
    {
        public string job_title { get; set; }
        public string job_description { get; set; }
        public int total_price { get; set; }
        public string job_start_date { get; set; }
        public string job_end_date { get; set; }
        public int survey_hours { get; set; }
        public List<DxDBid> bids { get; set; } = new List<DxDBid>();
    }
}
