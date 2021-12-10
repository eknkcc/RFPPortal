using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.SharedModels
{
    public class DxDSurveyResponse
    {
        public bool success { get; set; }
        public Survey survey { get; set; }

        public class SurveyRfpBid
        {
            public int id { get; set; }
            public int survey_id { get; set; }
            public int bid { get; set; }
            public string name { get; set; }
            public string forum { get; set; }
            public string email { get; set; }
            public DateTime delivery_date { get; set; }
            public int amount_of_bid { get; set; }
            public string additional_note { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }

        public class Survey
        {
            public int id { get; set; }
            public int number_response { get; set; }
            public string time_unit { get; set; }
            public int time { get; set; }
            public DateTime end_time { get; set; }
            public string status { get; set; }
            public int user_responded { get; set; }
            public object proposal_win { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public int downvote { get; set; }
            public string type { get; set; }
            public string job_title { get; set; }
            public string job_description { get; set; }
            public int total_price { get; set; }
            public string job_start_date { get; set; }
            public string job_end_date { get; set; }
            public List<SurveyRfpBid> survey_rfp_bids { get; set; }
            public SurveyRfpBid survey_rfp_win { get; set; }

        }


    }
}
