using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.DbModels
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NameSurname { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public DateTime CreateDate { get; set; }
        public string AuthKey { get; set; }
        public string ThirdPartyKey { get; set; }
        public string ThirdPartyType { get; set; }
    }
}
