﻿using System;

namespace RFPPortalWebsite.Models.SharedModels
{   
    /// <summary>
    ///  Simple http response class
    /// </summary>
    [Serializable]
    public class AjaxResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }
    }
}
