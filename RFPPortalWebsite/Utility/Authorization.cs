using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Utility
{
    public class PublicUserAuthorization : ActionFilterAttribute
    {
        public PublicUserAuthorization()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                bool control = true;


                if (context.HttpContext.Session.GetInt32("UserID") == null)
                {
                    control = false;
                }

                if (!control)
                {
                    context.Result = new RedirectResult("../Public/Login");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Public/Login");
            }

        }
    }

    public class InternalUserAuthorization : ActionFilterAttribute
    {
        public InternalUserAuthorization()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                bool control = true;


                if (context.HttpContext.Session.GetInt32("UserID") == null)
                {
                    control = false;
                }

                if (!control)
                {
                    context.Result = new RedirectResult("../Public/Login");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Public/Login");
            }

        }
    }

    public class AdminAuthorization : ActionFilterAttribute
    {
        public AdminAuthorization()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                bool control = true;

                if (context.HttpContext.Session.GetInt32("UserID") == null)
                {
                    control = false;
                }

                if (!control)
                {
                    context.Result = new RedirectResult("../Public/Login");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Public/Login");
            }

        }
    }
}
