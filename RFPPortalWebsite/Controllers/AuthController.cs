using Helpers.Models.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Methods;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.ViewModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Controllers
{
    /// <summary>
    ///  AuthController contains user authorization and registration methods.
    ///  Public users are automatically registered to the system after filling the bidding form. "RegisterPublic" method is triggered after posting bidding form.
    ///  Internal users should be registered from third party admin(DevxDao) with "RegisterInternal" method before they transferred to the portal.
    ///  If user already have a registration in the portal "GetUserAuthKey" method can be used to get user's AuthKey
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        ///  Public user registration method
        ///  This method can be accessed by every user
        /// </summary>
        /// <param name="registerInput">Registration information of the user</param>
        /// <returns>AjaxResponse object with registration result</returns>
        [HttpPost("RegisterPublic", Name = "RegisterPublic")]
        public AjaxResponse RegisterPublic([FromBody] RegisterModel registerInput)
        {
            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Email already exists control
                    if (db.Users.Count(x => x.Email == registerInput.email) > 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Email already exists." };
                    }

                    //Username already exists control
                    if (db.Users.Count(x => x.UserName == registerInput.username) > 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Username already exists." };
                    }
                }

                //Get user's ip and port
                registerInput.ip = Utility.IpHelper.GetClientIpAddress(HttpContext);
                registerInput.port = Utility.IpHelper.GetClientIpAddress(HttpContext);

                //Register user
                string authkey = AuthMethods.RegisterPublic(registerInput);

                if (!string.IsNullOrEmpty(authkey))
                {
                    return new AjaxResponse() { Success = true, Message = "User registration succesful.", Content = new { AuthKey = authkey } };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
            }

            return new AjaxResponse() { Success = false, Message = "Unexpected error" };
        }

        /// <summary>
        ///  Returns user information from user's AuthKey
        ///  This method can be accessed by every user
        /// </summary>
        /// <param name="authkey">User's auth key</param>
        /// <returns>AjaxResponse object with user object</returns>
        [HttpPost("GetUserInfo", Name = "GetUserInfo")]
        public AjaxResponse GetUserInfo(string authkey)
        {
            try
            {
                User user = Methods.AuthMethods.GetUserInfo(authkey);
                if(user.UserId > 0)
                {
                    return new AjaxResponse() { Success = true, Message = "User found.", Content = new { User = user } };
                }
                else
                {
                    return new AjaxResponse() { Success = false, Message = "User not found." };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return new AjaxResponse() { Success = false, Message = "Unexpected error" };
            }
        }


        /// <summary>
        ///  Internal user registration method
        ///  This method can only be accessed by third party admin in whitelisted ip addresses
        ///  Check appsettings.json for ip whitelist
        /// </summary>
        /// <param name="registerInput">Registration information of the user</param>
        /// <returns>AjaxResponse object with registration result</returns>
        [HttpPost("RegisterInternal", Name = "RegisterInternal")]
        [IpWhitelistAuthorization]
        public AjaxResponse RegisterInternal([FromBody] RegisterModel registerInput)
        {
            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Email already exists control
                    if (db.Users.Count(x => x.Email == registerInput.email) > 0)
                    {
                        var user = db.Users.First(x => x.Email == registerInput.email);
                        return new AjaxResponse() { Success = false, Message = "Email already exists." };
                    }

                    //Username already exists control
                    if (db.Users.Count(x => x.UserName == registerInput.username) > 0)
                    {
                        var user = db.Users.First(x => x.Email == registerInput.email);
                        return new AjaxResponse() { Success = false, Message = "Username already exists." };
                    }
                }

                //Register user
                string authkey = AuthMethods.RegisterInternal(registerInput);

                if (!string.IsNullOrEmpty(authkey))
                {
                    return new AjaxResponse() { Success = true, Message = "User registration succesful.", Content = new { AuthKey = authkey } };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
            }

            return new AjaxResponse() { Success = false, Message = "Unexpected error" };
        }


        /// <summary>
        ///  Returns user auth key from email OR password
        ///  This method can only be accessed by third party admin in whitelisted ip addresses
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="email">Email</param>
        /// <returns>AjaxResponse object with user AuthKey</returns>
        [HttpPost("GetUserAuthKey", Name = "GetUserAuthKey")]
        [IpWhitelistAuthorization]
        public AjaxResponse GetUserAuthKey(string username, string email)
        {
            try
            {
                string authkey = Methods.AuthMethods.GetUserAuthKey(username, email);
                if (!string.IsNullOrEmpty(authkey))
                {
                    return new AjaxResponse() { Success = true, Message = "User found.", Content = new { AuthKey = authkey } };
                }
                else
                {
                    return new AjaxResponse() { Success = false, Message = "User not found." };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return new AjaxResponse() { Success = false, Message = "Unexpected error" };
            }
        }
    }
}
