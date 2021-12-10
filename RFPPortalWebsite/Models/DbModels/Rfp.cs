using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.DbModels
{
    public class Rfp
    {
        [Key]
        public int RfpID { get; set; }
        public int UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public string Timeframe { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? WinnerRfpBidID { get; set; }
        public DateTime PublicBidEndDate { get; set; }
        public DateTime InternalBidEndDate { get; set; }
        public string Tags { get; set; }
        public int? InternalSurveyId { get; set; }
        public int? PublicSurveyId { get; set; }
    }
}
