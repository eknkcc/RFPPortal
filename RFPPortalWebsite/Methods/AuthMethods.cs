using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
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
        /// <param name="email">User's email</param>
        /// <returns>AjaxResponse object with user object</returns>
        public static User GetUserInfo(string email, string pass)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Control with email
                    if (!string.IsNullOrEmpty(email) && db.Users.Count(x => x.Email == email) > 0)
                    {
                        var user = db.Users.First(x => x.Email == email);

                        if(Utility.Encryption.CheckPassword(user.Password, pass))
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


        private static DxDUserModel CheckDxDUser(string email)
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

    }
}
