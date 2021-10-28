﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Models.ViewModels
{
    public class NewBidModel
    {
        public double Amount { get; set; }
        public string Note { get; set; }
        public string TimeFrame { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
