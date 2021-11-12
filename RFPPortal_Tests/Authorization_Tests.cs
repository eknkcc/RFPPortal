using System.Runtime.Intrinsics.X86;
using System;
using Xunit;
using RFPPortalWebsite.Models.ViewModels;
using FluentAssertions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.Constants;
using Org.BouncyCastle.Crypto;
using System.Reflection;
using RFPPortalWebsite.Utility;

namespace RFPPortal_Tests
{
    public class MissingValueUserData : IEnumerable<object[]>
    {
        public readonly List<object[]> _data = new List<object[]>{
            
            new object[]{
                new RegisterModel{
                    UserName    = "",
                    NameSurname = "UserName Doesnotexist",
                    Email       = "UserName@Doesnotexist.com",
                    Password    = "Password",
                    RePassword  = "Password"
                }
            },

            new object[]{
                new RegisterModel{
                    UserName    = "NameSurnameDoesnotexist",
                    NameSurname = "",
                    Email       = "NameSurname@DoesnotExist.com",
                    Password    = "Password",
                    RePassword  = "Password"
                }
            },

            new object[]{
                new RegisterModel{
                    UserName    = "EmailDoesnotexist",
                    NameSurname = "Email DoesnotExist",
                    Email       = "",
                    Password    = "Password",
                    RePassword  = "Password"
                }
            },

            new object[]{
                new RegisterModel{
                    UserName    = "PasswordsDoesnotmatch",
                    NameSurname = "Passwords Doesnotmatch",
                    Email       = "Passwords@Doesnotmatch.com",
                    Password    = "Password",
                    RePassword  = ""
                }
            }    
        
        };

        IEnumerator<object[]> IEnumerable<object[]>.GetEnumerator() => _data.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public class Authorization_Tests
    {

        PostTestController controllers;
        ISession session;
        public Authorization_Tests(){
            controllers = new PostTestController();
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            session = controllers.bidController.ControllerContext.HttpContext.Session = new MockHttpSession();
        }
        
        /// <summary>
        ///  Test of standart public user register.
        ///  Application method returns SimpleResponse model which the "boolean Success" property should be "true".
        /// </summary>
        /// <param name="regular_user_register">Valid model of ViewModel RegisterModel</param>
        /// <returns>bool result of Assert.True</returns>
        [Fact]        
        public void Regular_Register_Test()
        {
            //Arrange
            //Initializing the database
            TestDbInitializer.ClearDatabase();
            //Createing a Public User model to be registered
            RegisterModel regular_user_register = new RegisterModel{
                UserName    = "Regular_User",
                NameSurname = "Regular User",
                Email       = "regular@user.com",
                Password    = "PassW0rd",
                RePassword  = "PassW0rd"
            };

            //Act
            //Attempt to register 
            //register method should return 'SimpleResponse method which Success value should be 'true' and Content value should include a User model'
            SimpleResponse regular_register_result = controllers.authController.RegisterUser(regular_user_register);

            //Assert
            regular_register_result.Success.Should().Be(true);
            User registered_user = (User)regular_register_result.Content;
            registered_user.UserName.Should().Be("Regular_User");
            registered_user.NameSurname.Should().Be("Regular User");
            registered_user.Email.Should().Be("regular@user.com");            
        }
        
        [Fact]
        public void RegisterCompleteView_Test(){

            //Arrange
            //Initializing database
            TestDbInitializer.ClearDatabase();
            //Creating a public user 'Register Model' instance to be registered
            RegisterModel user = new RegisterModel{
                UserName    = "Public_User",
                NameSurname = "Public User",
                Email       = "public@user.com",
                Password    = "PassW0rd",
                RePassword  = "PassW0rd"
            };

            //Starting registration process
            SimpleResponse result = controllers.authController.RegisterUser(user);
            result.Success.Should().Be(true);

            //After registering without email user activation Signin process should fail
            SimpleResponse login_result = controllers.authController.SignIn(user.Email, user.Password);
            login_result.Success.Should().Be(false);
            
            //Mocking Registeration Completion Token
            string enc = Encryption.EncryptString(user.Email + "|" + DateTime.Now.ToString());

            //Act
            //Registeration complete view should redirect to [Home]/[Index]
            IActionResult register_completion_view = controllers.authController.RegisterCompleteView(enc);
            var RedirectResult = register_completion_view.Should().BeOfType<RedirectToActionResult>().Subject;

            //After completion of registeration with email verification user should login
            SimpleResponse response = controllers.authController.SignIn(user.Email, user.Password);
            response.Success.Should().Be(true);
        }


        [Theory]
        [ClassData(typeof(MissingValueUserData))]    
        public void Missing_value_Register_Test(RegisterModel model){
            
            //Arrange
            //A set of user data with missing required values is prepared in the 'MissingValueData' class
            //and applied to this method one by one using [Theory] & [ClassData] decorations    
            //Initializing the database
            TestDbInitializer.ClearDatabase();      
            
            //Act
            //Attemptions of registering users with missing required data
            //RegisterUser method returns a 'SimpleResponse Model' which has 'boolean Success' property shows the result status of the register process 
            //and 'string Message' property to include the success or error message. 
            SimpleResponse result = controllers.authController.RegisterUser(model);            

            //Assert
            //By case design all assertions should be false
            Assert.False(result.Success);
        }

        [Fact]
        public void Signin_Test(){

            string existingUserEmail = "public@user1.com";
            string existingUserPassword = "PassW0rd";
            string nonExistingUserEmail = "nonexisting@user.com";
            string nonExistingPassword = "PassW0rd";
            string existingUserWrongPassword = "Password";

            //Arrange
            TestDbInitializer.SeedUsers();

            //Act
            //case -1- : Existing user attempting proper login
            SimpleResponse result_success = controllers.authController.SignIn(existingUserEmail, existingUserPassword);
            User successlogin_usr = result_success.Content as User;
            //Assert            
            Assert.True(result_success.Success);
            
            //case -2- : Non-Existing user attempting login
            SimpleResponse result_fail = controllers.authController.SignIn(nonExistingUserEmail, nonExistingPassword);
            User failedlogin_user = result_fail.Content as User;
            //Assert            
            Assert.False(result_fail.Success);

            //case -3- : Existing user attempting improper login
            SimpleResponse result_fail2 = controllers.authController.SignIn(existingUserEmail, existingUserWrongPassword);
            User failedlogin_user2 = result_fail2.Content as User;
            //Assert            
            Assert.False(result_fail.Success);
        }

        [Fact]
        public void VA_Signin_Test()
        {
            //Arrange
            //Initializing the database
            TestDbInitializer.ClearDatabase();
            //Registering VA user
            RegisterModel va_usr = new RegisterModel{
                UserName      = "ekincc"
                , NameSurname = "Ekin Kececi"
                , Email       = "ekin@ekonteknoloji.com"
                , Password    = "PassW0rd"
                , RePassword  = "PassW0rd"
            };
            var result = controllers.authController.RegisterUser(va_usr);
            result.Success.Should().Be(true);
            
            //Adjusting the account as email verified
            var activation_result  = TestDbInitializer.ActivateUserMock((User)result.Content);
            
            //Preparing VA parameters for login
            string email = "ekin@ekonteknoloji.com";
            string password = "PassW0rd";

            //Act
            //Signin process should be successfull and returned user type should be "Internal"
            SimpleResponse signin_result = controllers.authController.SignIn(email, password);
            signin_result.Success.Should().Be(true);
            ((User)signin_result.Content).UserType.Should().Be("Internal");     
        }       

        [Fact]
        public void VA_Register_Test_via_DXDprovided_Api(){

            //Arrange
            //Initialize the Database
            TestDbInitializer.ClearDatabase();
            //Creating a user who is already a VA
            RegisterModel VA_User = new RegisterModel{
                UserName    = "VA_User",
                NameSurname = "VA User",
                Email       = "ekin@ekonteknoloji.com",
                Password    = "Password",
                RePassword  = "Password"
            };

            //Act
            //Registered user method uses the DxDApiForUser ["https://backend.devxdao.com/api/va/email/"] API to check if the email value of the RegisterModel instance belongs to a VA User
            SimpleResponse result = controllers.authController.RegisterUser(VA_User);
            
            //Assert
            result.Success.Should().Be(true);
            ((User)result.Content).UserType.Should().Be("Internal");
        }

        [Fact]
        public void Existing_User_Register_Test(){

            //Arrange
            //Creating a new user which will be act as existing user
            RegisterModel existing_user = new RegisterModel{
                UserName    = "Exsiting_User",
                NameSurname = "Existing User",
                Email       = "existing@user.com",
                Password    = "Password",
                RePassword  = "Password"
            };

            //model for case -1- : attempt of 'Existing UserName' register 
            RegisterModel new_user_1 = new RegisterModel{
                UserName    = "Exsiting_User",
                NameSurname = "Existing User1",
                Email       = "existing@user1.com",
                Password    = "Password",
                RePassword  = "Password"
            };

            //model for case -2- : attempt of 'Existing Email' register
            RegisterModel new_user_2 = new RegisterModel{
                UserName    = "Exsiting_User1",
                NameSurname = "Existing User2",
                Email       = "existing@user.com",
                Password    = "Password",
                RePassword  = "Password"
            };

            //Registering the user model which will act as existing user.
            var registered_user = controllers.authController.RegisterUser(existing_user);

            //Act case : -1-
            var case1_result = controllers.authController.RegisterUser(new_user_1);

            //Assert
            //case -1- : Existing UserName
            case1_result.Success.Should().Be(false);

            //Act case : -2-
            var case2_result = controllers.authController.RegisterUser(new_user_2);

            //Assert
            //case -1- : Existing UserEmail
            case2_result.Success.Should().Be(false);

        }
    }
}
