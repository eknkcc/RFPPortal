using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.SharedModels
{
    public class DxDSurveyResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public class DxDSurveyItem
        {
            public int number_response { get; set; }
            public int downvote { get; set; }
            public int time { get; set; }
            public string time_unit { get; set; }
            public string job_title { get; set; }
            public string job_description { get; set; }
            public int total_price { get; set; }
            public string end_time { get; set; }
            public string job_start_date { get; set; }
            public string job_end_date { get; set; }
            public string status { get; set; }
            public int user_responded { get; set; }
            public string type { get; set; }
            public string updated_at { get; set; }
            public string created_at { get; set; }
            public int id { get; set; }
        }
    }
}
