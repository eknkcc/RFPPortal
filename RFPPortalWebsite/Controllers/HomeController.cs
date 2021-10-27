using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
        [Route("RFP_Detail/{BidID}")]
        public IActionResult RFP_Detail(int BidID)
        {
            return View();
        }


    }
}
