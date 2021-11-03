using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.ViewModels
{
    public class RegisterModel
    {
        public string UserName { get; set; }
        public string NameSurname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
    }
}
