using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.ViewModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Methods
{
    /// <summary>
    ///  Authorization methods bussiness layer
    /// </summary>
    public class AuthMethods
    {
        /// <summary>
        ///  Public user registration method
        /// </summary>
        /// <param name="registerInput">Registration information of the user</param>
        /// <returns>AjaxResponse object with registration result</returns>
        public static User UserRegister(RegisterModel registerInput)
        {
            try
            {
                DxDUserModel model = CheckDxDUser(registerInput.Email);

                using (rfpdb_context db = new rfpdb_context())
                {

                    //Create new user object
                    User userModel = new User();
                    if (model.success)
                    {
                        userModel.UserType = Models.Constants.Enums.UserIdentityType.Internal.ToString();
                        userModel.UserName = model.User.forum_name;
                    }
                    else
                    {
                        userModel.UserType = Models.Constants.Enums.UserIdentityType.Public.ToString();
                        userModel.UserName = registerInput.UserName;
                    }
                    var hashPass = Utility.Encryption.EncryptPassword(registerInput.Password);
                    userModel.Email = registerInput.Email.ToLower();
                    userModel.NameSurname = registerInput.NameSurname;
                    userModel.CreateDate = DateTime.Now;
                    userModel.Password = hashPass;
                    userModel.IsActive = false;

                    Guid g = Guid.NewGuid();

                    //Insert user object to database
                    db.Users.Add(userModel);
                    db.SaveChanges();

                    if (userModel != null && userModel.UserId != 0)
                    {
                        //Logging
                        Program.monitizer.AddUserLog(userModel.UserId, Models.Constants.Enums.UserLogType.Auth, "User register successful.");

                        return userModel;
                    }
                    else
                    {
                        return new User();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return new User();
            }
        }

        /// <summary>
        ///  Returns user information from user's email
        /// </summary>
        /// <param name="email">User's email or username</param>
        /// <returns>AjaxResponse object with user object</returns>
        public static SimpleResponse UserSignIn(string email, string pass)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Control with email
                    if (!string.IsNullOrEmpty(email))
                    {
                        //Get user with email
                        var user = db.Users.FirstOrDefault(x => x.Email == email);

                        //Email not found, try to find with username
                        if (user == null)
                        {
                            user = db.Users.FirstOrDefault(x => x.UserName == email);
                        }

                        //User not found
                        if (user == null || user.IsActive == false)
                        {
                            return new SimpleResponse() { Success= false, Message = "Incorrect username or password" };
                        }

                        //User not active (email confirmation)
                        if (user == null || user.IsActive == false)
                        {
                            return new SimpleResponse() { Success = false, Message = "Please activate your account from registration email." };
                        }

                        //Password check
                        if (Utility.Encryption.CheckPassword(user.Password, pass))
                        {
                            //Logging
                            Program.monitizer.AddUserLog(user.UserId, Models.Constants.Enums.UserLogType.Auth, "User sign in successful.");

                            return new SimpleResponse() { Success = true, Message = "User sign in successful.", Content = user };
                        }
                    }

                    return new SimpleResponse() { Success = false, Message = "Sign in failed." };
                }
            }
            
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return new SimpleResponse() { Success = false, Message = "Sign in failed." };
            }
        }

        /// <summary>
        ///  Check if user is VA in DEVxDAO platform
        /// </summary>
        /// <param name="email">User's email</param>
        /// <returns>User information in DEVxDAO</returns>
        public static DxDUserModel CheckDxDUser(string email)
        {
            DxDUserModel registerResponse = new DxDUserModel();
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Control with email

                    var checkUserJson = Utility.Request.GetDxD(Program._settings.DxDApiForUser + email, Program._settings.DxDApiToken);
                    registerResponse = Utility.Serializers.DeserializeJson<DxDUserModel>(checkUserJson);


                    if (registerResponse == null)
                        return new DxDUserModel();

                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return new DxDUserModel();
            }

            return registerResponse;
        }

        /// <summary>
        ///  Email approval method after registration
        /// </summary>
        /// <param name="model">Token generated from the Register method</param>
        /// <returns>Generic AjaxResponse class</returns>
        public static User RegisterComplete(string registerToken)
        {
            try
            {
                //Decrypt token in the email
                string stre = Encryption.DecryptString(registerToken);

                //Check if it's a valid token
                if (stre.Split('|').Length > 1)
                {
                    string email = stre.Split('|')[0];

                    //Find user in database
                    using (rfpdb_context db = new rfpdb_context())
                    {
                        User modelUser = db.Users.SingleOrDefault(x => x.Email == email);

                        if (modelUser != null && modelUser.UserId > 0)
                        {
                            //Change active status of the user and update database
                            modelUser.IsActive = true;
                            db.SaveChanges();

                            //Logging
                            Program.monitizer.AddUserLog(modelUser.UserId, Models.Constants.Enums.UserLogType.Auth, "User email activation successful.");

                            return modelUser;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
            }

            return new User();
        }

        /// <summary>
        ///  Change password method after reset password request
        /// </summary>
        /// <param name="model">passwordChangeToken: Token generated from ResetPassword method</param>
        /// <returns>Generic SimpleResponse class</returns>
        public static SimpleResponse ResetPasswordComplete(string newPass, string passwordChangeToken)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Decrypt token in password renewal email
                    string tokendec = Utility.Encryption.DecryptString(passwordChangeToken);
                    string email = tokendec.Split('|')[0];

                    //Find user in database
                    var usr = db.Users.FirstOrDefault(x => x.Email == email);

                    DateTime emaildate = Convert.ToDateTime(tokendec.Split('|')[1]);
                    emaildate = emaildate.AddMinutes(60);

                    //Check if user is valid and password renewal is expired
                    if (usr != null && usr.Email == email && emaildate > DateTime.Now)
                    {
                        //Reset password
                        usr.Password = Utility.Encryption.EncryptPassword(newPass);
                        usr.IsActive = true;
                        db.SaveChanges();

                        //Logging
                        Program.monitizer.AddUserLog(usr.UserId, Models.Constants.Enums.UserLogType.Auth, "Password reset completed.");

                        return new SimpleResponse { Success = true, Message = "Password reset completed." };
                    }
                    else
                    {
                        return new SimpleResponse { Success = false, Message = "Renew expired" };
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return new SimpleResponse() { Success = false };
            }
        }
    }
}
