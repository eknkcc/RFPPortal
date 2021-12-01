using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Methods;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.ViewModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Controllers
{
    public class AuthController : Controller
    {
        /// <summary>
        ///  User login function
        /// </summary>
        /// <param name="email">User's email or username</param>
        /// <param name="pass">User's password</param>
        /// <returns>SimpleResponse</returns>
        [Route("SignIn")]
        public SimpleResponse SignIn(string email, string pass)
        {
            SimpleResponse res = Methods.AuthMethods.UserSignIn(email, pass);
            if (res.Success)
            {
                var usr = res.Content as User;
                try
                {
                    HttpContext.Session.SetInt32("UserId", usr.UserId);
                    HttpContext.Session.SetString("UserType", usr.UserType);
                    HttpContext.Session.SetString("NameSurname", usr.NameSurname);
                    HttpContext.Session.SetString("Email", usr.Email);
                    HttpContext.Session.SetString("Username", usr.UserName);
                }
                catch(Exception){}
            }

            return res;
        }

        /// <summary>
        ///  Public user registration method
        ///  This method can be accessed by every user
        /// </summary>
        /// <param name="registerInput">Registration information of the user</param>
        /// <returns>SimpleResponse</returns>
        [HttpPost("RegisterUser", Name = "RegisterUser")]
        public SimpleResponse RegisterUser([FromBody] RegisterModel registerInput)
        {
            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Required fields control
                    if(String.IsNullOrEmpty(registerInput.UserName) || String.IsNullOrEmpty(registerInput.Email) || String.IsNullOrEmpty(registerInput.Password) || String.IsNullOrEmpty(registerInput.NameSurname))
                    {
                        return new SimpleResponse() {Success = false, Message = "Required data missing."};
                    }  
                    //Email already exists control
                    if (db.Users.Count(x => x.Email == registerInput.Email) > 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Email already exists." };
                    }

                    //Username already exists control
                    if (db.Users.Count(x => x.UserName == registerInput.UserName) > 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Username already exists." };
                    }
                    

                    //Password match control
                    if (!String.Equals(registerInput.Password, registerInput.RePassword))
                    {
                        return new SimpleResponse() { Success = false, Message = "Passwords does not match." };
                    }
                }

                User usr = AuthMethods.UserRegister(registerInput);

                if (usr.UserId > 0)
                {

                    if (this.Request != null)
                    {
                        //Create encrypted activation key for email approval
                        string enc = Encryption.EncryptString(registerInput.Email + "|" + DateTime.Now.ToString());

                        var baseUrl = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

                        //Set email title and content
                        string emailTitle = "Welcome to DEVxDAO RFP Portal";
                        string emailContent = "Welcome to DEVxDAO RFP Portal, <br><br> Thank you for your registration. Please use the link below to activate your account. <br><br> <a href='" + baseUrl + "/RegisterCompleteView?str=" + enc + "'>Click here to complete the registration.</a>";

                        //Send email
                        EmailHelper.SendEmail(emailTitle, emailContent, new List<string>() { usr.Email }, new List<string>(), new List<string>());
                    }

                    return new SimpleResponse() { Success = true, Message = "User registration succesful.Please verify your account from your email.", Content = usr };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
            }

            return new SimpleResponse() { Success = false, Message = "Unexpected error" };
        }

        /// <summary>
        /// Completes user registration from activation link in the confirmation email
        /// </summary>
        /// <param name="str">Encrypted user information in the registration email</param>
        /// <returns></returns>
        [Route("RegisterCompleteView")]
        public IActionResult RegisterCompleteView(string str)
        {
            SimpleResponse resp = new SimpleResponse();

            User usr = AuthMethods.RegisterComplete(str);

            try
            {
                if (usr.UserId > 0)
                {
                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "User activation successful.";
                }
                else
                {
                    TempData["toastr-type"] = "error";
                    TempData["toastr-message"] = "Invalid user activation request.";
                }

            }
            catch(Exception){}            

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// User logout function
        /// </summary>
        /// <returns></returns>
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Sends password reset email to user's email
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="usercode">Captcha code (Needed after 3 failed requests)</param>
        /// <returns></returns>
        [Route("ResetPassword")]
        [HttpPost]
        public SimpleResponse ResetPassword(string email)
        {
            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Email does not exist
                    if (db.Users.Count(x => x.Email == email) == 0)
                    {
                        return new SimpleResponse { Success = false, Message = "Please enter an e-mail address registered in the system." };
                    }
                }

                if (this.Request != null)
                {
                    //Create encrypted activation key for email approval
                    string enc = Encryption.EncryptString(email + "|" + DateTime.Now.ToString());

                    var baseUrl = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

                    //Set email title and content
                    string emailTitle = "RFP Portal Password Renewal";
                    string emailContent = "We got your password reset request. Please use the link below to set a new password for  your account. <br><br> <a href='" + baseUrl + "/ResetPasswordView?str=" + enc + "'>Click here to reset your password.</a>";

                    //Send email
                    EmailHelper.SendEmail(emailTitle, emailContent, new List<string>() { email }, new List<string>(), new List<string>());

                }

                return new SimpleResponse { Success = true, Message = "Password reset link sent to your email." };

            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new SimpleResponse { Success = false, Message = "An unexpected error has occurred. Please try again later" };
            }
        }

        /// <summary>
        /// Opens password reset model from 
        /// </summary>
        /// <param name="str">Encrypted user information in the password reset email</param>
        /// <returns></returns>
        [Route("ResetPasswordView")]
        public IActionResult ResetPasswordView(string str)
        {
            try
            {
                //Set password change token into session
                HttpContext.Session.SetString("passwordchangetoken", str);

                //Decrypt information
                string decryptedToken = Utility.Encryption.DecryptString(str);

                //Check if format is valid
                if (decryptedToken.Split('|').Length > 1)
                {
                    //Check if password renewal expired
                    DateTime emaildate = Convert.ToDateTime(decryptedToken.Split('|')[1]);
                    if (emaildate.AddMinutes(60) < DateTime.Now)
                    {
                        TempData["message"] = "Password reset request expired. Please submit a new request.";
                    }
                    else
                    {
                        //Set user's email
                        HttpContext.Session.SetString("passwordchangeemail", decryptedToken.Split('|')[0]);
                        TempData["action"] = "resetpassword";
                    }
                }
                else
                {
                    TempData["message"] = "Invalid password reset request.";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "An error occurred during the process. Please try again later. ";

                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Sets user new password
        /// </summary>
        /// <param name="newpass">New password</param>
        /// <param name="newpassagain">New password confirmation</param>
        /// <param name="usercode">Captcha code (Needed after 3 failed requests)</param>
        /// <returns></returns>
        [Route("ResetPasswordComplete")]
        [HttpPost]
        public SimpleResponse ResetPasswordComplete(string newpass, string newpassagain)
        {
            try
            {
                //Password match control
                if (newpass != newpassagain)
                {
                    return new SimpleResponse { Success = false, Message = "Passwords entered are not compatible." };
                }

                //Password strength control
                if (!Regex.IsMatch(newpass, @"^(?=.{8,})(?=.*[a-z])(?=.*[A-Z])"))
                {
                    return new SimpleResponse { Success = false, Message = "The password must contain at least 8 characters and contain 1 digit 1 lowercase 1 uppercase." };
                }


                SimpleResponse resetResponse = AuthMethods.ResetPasswordComplete(newpass, HttpContext.Session.GetString("passwordchangetoken"));

                if (resetResponse.Success)
                {
                    return new SimpleResponse { Success = true, Message = "Your password has been updated. You can sign in with your new password." };
                }
                else
                {
                    if (resetResponse.Message == "Renew expired")
                    {
                        HttpContext.Session.SetString("passwordchangeemail", "true");

                        return new SimpleResponse { Success = false, Message = "This password renewal request has expired." };
                    }
                    else
                    {
                        return new SimpleResponse { Success = false, Message = "An unexpected error has occurred. Please try again later" };
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new SimpleResponse { Success = false, Message = "An error occurred while proccesing your request." };
            }
        }

    }
}
