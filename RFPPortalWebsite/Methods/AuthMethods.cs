using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.ViewModels;
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
        public static string RegisterPublic(RegisterModel registerInput)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Create new user object
                    User userModel = new User();
                    userModel.Email = registerInput.email.ToLower();
                    userModel.UserName = registerInput.username;
                    userModel.NameSurname = registerInput.namesurname;
                    userModel.CreateDate = DateTime.Now;
                    userModel.UserType = UserIdentityType.Public.ToString();
                    Guid g = Guid.NewGuid();
                    userModel.AuthKey = g.ToString();

                    //Insert user object to database
                    db.Users.Add(userModel);
                    db.SaveChanges();

                    if (userModel != null && userModel.UserId != 0)
                    {
                        //Logging
                        Program.monitizer.AddUserLog(userModel.UserId, Models.Constants.Enums.UserLogType.Auth, "User register successful.", registerInput.ip, registerInput.port);

                        return userModel.AuthKey;
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return String.Empty;
            }
        }

        /// <summary>
        ///  Returns user information from user's AuthKey
        /// </summary>
        /// <param name="authkey">User's auth key</param>
        /// <returns>AjaxResponse object with user object</returns>
        public static User GetUserInfo(string authkey)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Control with email
                    if (!string.IsNullOrEmpty(authkey) && db.Users.Count(x => x.AuthKey == authkey) > 0)
                    {
                        var user = db.Users.First(x => x.AuthKey == authkey);
                        return user;
                    }

                    return new User();
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return new User();
            }
        }


        /// <summary>
        ///  Internal user registration method
        /// </summary>
        /// <param name="registerInput">Registration information of the user</param>
        /// <returns>AjaxResponse object with registration result</returns>
        public static string RegisterInternal(RegisterModel registerInput)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Create new user object
                    User userModel = new User();
                    userModel.Email = registerInput.email.ToLower();
                    userModel.UserName = registerInput.username;
                    userModel.NameSurname = registerInput.namesurname;
                    userModel.CreateDate = DateTime.Now;
                    userModel.UserType = UserIdentityType.Internal.ToString();
                    Guid g = Guid.NewGuid();
                    userModel.AuthKey = g.ToString();

                    //Insert user object to database
                    db.Users.Add(userModel);
                    db.SaveChanges();

                    if (userModel != null && userModel.UserId != 0)
                    {
                        //Logging
                        Program.monitizer.AddUserLog(userModel.UserId, Models.Constants.Enums.UserLogType.Auth, "User register successful.", registerInput.ip, registerInput.port);

                        return userModel.AuthKey;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return string.Empty;
            }
        }


        /// <summary>
        ///  Returns user auth key from email OR password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="email">Email</param>
        /// <returns>AjaxResponse object with user AuthKey</returns>
        public static string GetUserAuthKey(string username, string email)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Control with email
                    if (!string.IsNullOrEmpty(email) && db.Users.Count(x => x.Email == email) > 0)
                    {
                        var user = db.Users.First(x => x.Email == email);
                        return  user.AuthKey;
                    }

                    //Control with username
                    if (!string.IsNullOrEmpty(username) && db.Users.Count(x => x.UserName == username) > 0)
                    {
                        var user = db.Users.First(x => x.Email == email);
                        return user.AuthKey;
                    }

                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return string.Empty;
            }
        }
    }
}
