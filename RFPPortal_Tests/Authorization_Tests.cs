using System;
using Xunit;
using RFPPortalWebsite.Models.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Utility;

namespace RFPPortal_Tests
{    
    /// <summary>
    /// Collection of authorization test methods.
    /// Regular_Register_Test()
    /// RegisterCompleteView_Test()
    /// Missing_value_Register_Test()
    /// Signin_Test()
    /// VA_Signin_Test()
    /// VA_Register_Test_via_DXDprovided_Api()
    /// Existing_User_Register_Test()
    /// </summary>
    [Collection("Sequential")]
    public class Authorization_Tests
    {
        PostTestController controllers;
        ISession session;        
        
        /// Application controllers and HttpContext Session are initialized.   
        public Authorization_Tests(){
            controllers = new PostTestController();
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            session = controllers.bidController.ControllerContext.HttpContext.Session = new MockHttpSession();
        }
        
        /// <summary>
        /// Test of AuthController.RegisterUser() method with proper public UserRegister model.
        /// 1.Initialize the database
        /// 2.Creates a proper RegisterModel
        /// 3.Tests AuthController.RegisterUser() method - Assert.True has succeeded.
        /// </summary>
        [Fact]        
        public void Regular_Register_Test()
        {           
            // Arrange
            // Initializing the database            
            TestDbInitializer.ClearDatabase();
            //Createing a Public User model to be registered
            RegisterModel regular_user = new RegisterModel{
                UserName    = "Regular_User",
                NameSurname = "Regular User",
                Email       = "regular@user.com",
                Password    = TestDbInitializer.testPassword,
                RePassword  = TestDbInitializer.testPassword
            };

            //Act
            //Attempt to register 
            //register method should return 'SimpleResponse method which Success value should be 'true' and Content value should include a User model'
            SimpleResponse regular_register_result = controllers.authController.RegisterUser(regular_user);

            //Assert
            regular_register_result.Success.Should().Be(true);
            User registered_user = (User)regular_register_result.Content;
            registered_user.UserName.Should().Be("Regular_User");
            registered_user.NameSurname.Should().Be("Regular User");
            registered_user.Email.Should().Be("regular@user.com");            
        }
        
        /// <summary>
        /// Test of the AuthController.RegisterCompleteView(RegisterationCompletionToken).
        /// 1.Initialize Database 
        /// 2.Create a public user 'RegisterModel' instance to be registered
        /// 3.Registeration process start with applying a proper RegisterModel(type public) to RegisterUser() method.
        /// 4.Try to login without email conformation - Assert.False has succeeded
        /// 5.Create fake registeration completion token.
        /// 6.Test AuthController.RegisterCompleteView method with fake registeration completion token - Assert.True has succeeded.
        /// </summary>
        [Fact]
        public void RegisterCompleteView_Test(){
            // Arrange
            // Initializing database
            TestDbInitializer.ClearDatabase();
            // Creating a public user 'Register Model' instance to be registered            
            RegisterModel user = new RegisterModel{
                UserName    = "Public_User",
                NameSurname = "Public User",
                Email       = "public@user.com",
                Password    = TestDbInitializer.testPassword,
                RePassword  = TestDbInitializer.testPassword
            };
            // Starting registration process
            SimpleResponse result = controllers.authController.RegisterUser(user);
            result.Success.Should().Be(true);

            // After registering without email user activation Signin process should fail            
            SimpleResponse login_result = controllers.authController.SignIn(user.Email, user.Password);
            login_result.Success.Should().Be(false);
            
            // Mocking Registeration Completion Token            
            string enc = Encryption.EncryptString(user.Email + "|" + DateTime.Now.ToString());
            
            // Act
            // Registeration complete view should redirect to [Home]/[Index]            
            IActionResult register_completion_view = controllers.authController.RegisterCompleteView(enc);
            var RedirectResult = register_completion_view.Should().BeOfType<RedirectToActionResult>().Subject;
            
            // After completion of registeration with email verification user should login            
            // <returns>bool result of Assert.True</returns>
            SimpleResponse response = controllers.authController.SignIn(user.Email, user.Password);
            response.Success.Should().Be(true);
        }

        /// <summary>
        /// Test of AuthController.RegisterUser() method with improper RegisterUser model.
        /// All user RegistrationModel instances prepared in the MissingValueUsersData class are applied to method.
        /// No registration should take place - Assert.False Succeeded.
        /// </summary>
        [Theory]
        [ClassData(typeof(MissingValueUserData))]    
        public void Missing_value_Register_Test(RegisterModel model){
            
            // Arrange
            // A set of user data with missing required values is prepared in the 'MissingValueData' class
            // and applied to this method one by one using [Theory] & [ClassData] decorations    
            // Initializing the database
            TestDbInitializer.ClearDatabase();  
           
            // Act
            // Attemptions of registering users with missing required data
            // RegisterUser method returns a 'SimpleResponse Model' which has 'boolean Success' property shows the result status of the register process 
            // and 'string Message' property to include the success or error message. 
            SimpleResponse result = controllers.authController.RegisterUser(model);            

            // Assert
            // By case design all assertions should be false
            // </summary>
            Assert.False(result.Success);
        }

        /// <summary>
        /// Tests of AuthController.SignIn(UserEmail, UserPassword) method for 3 use cases.
        /// case -1- : Existing user attempting proper login - Assert.True Succeeded
        /// case -2- : Non-Existing user attempting login - Assert.False Succeeded
        /// case -3- : Existing user attempting improper login - Assert.False Succeeded
        /// </summary>
        [Fact]
        public void Signin_Test(){

            string existingUserEmail = "public@user1.com";
            string existingUserPassword = TestDbInitializer.testPassword;
            string nonExistingUserEmail = "nonexisting@user.com";
            string nonExistingPassword = TestDbInitializer.testPassword;
            string existingUserWrongPassword = Guid.NewGuid().ToString("d").Substring(1,7); 

            // Arrange
            TestDbInitializer.SeedUsers();

            // Act
            // case -1- : Existing user attempting proper login
            SimpleResponse result_success = controllers.authController.SignIn(existingUserEmail, existingUserPassword);
            User successlogin_usr = result_success.Content as User;
            // Assert      
            Assert.True(result_success.Success);
            
            // case -2- : Non-Existing user attempting login
            SimpleResponse result_fail = controllers.authController.SignIn(nonExistingUserEmail, nonExistingPassword);
            User failedlogin_user = result_fail.Content as User;
            // Assert            
            Assert.False(result_fail.Success);

            // case -3- : Existing user attempting improper login
            SimpleResponse result_fail2 = controllers.authController.SignIn(existingUserEmail, existingUserWrongPassword);
            User failedlogin_user2 = result_fail2.Content as User;

            // Assert       
            // </summary>     
            Assert.False(result_fail.Success);
        }

        /// <summary> 
        /// VA recognition on AuthController.SignIn(VA_useremail, Userpassword) test. - Assert.True succeeded
        /// </summary> 
        [Fact]
        public void VA_Signin_Test()
        {
            // Arrange
            // Initializing the database
            TestDbInitializer.ClearDatabase();
            // Registering VA user
            RegisterModel va_usr = new RegisterModel{
                UserName      = "ekincc"
                , NameSurname = "Ekin Kececi"
                , Email       = "ekin@ekonteknoloji.com"
                , Password    = TestDbInitializer.testPassword
                , RePassword  = TestDbInitializer.testPassword
            };
            var result = controllers.authController.RegisterUser(va_usr);
            result.Success.Should().Be(true);
            // Adjusting the account as email verified
            var activation_result  = TestDbInitializer.ActivateUserMock((User)result.Content);
            
            // Preparing VA parameters for login
            string email = "ekin@ekonteknoloji.com";
            string password = TestDbInitializer.testPassword;

            // Act
            // Signin process should be successfull and returned user type should be "Internal"
            SimpleResponse signin_result = controllers.authController.SignIn(email, password);
            // Assert
            signin_result.Success.Should().Be(true);
            ((User)signin_result.Content).UserType.Should().Be("Internal");     
        }       

        /// <summary>
        /// Test of VA recognition on AuthController.RegisterUser(VA_useremail, Userpassword) test. - Assert.True succeed
        /// During signup process RegisterUser method uses the DxDApiForUser ["https://backend.devxdao.com/api/va/email/"] API to check if the email included in the given RegisterModel instance belongs to a VA User
        /// This method also tests the API to recognize VA users provided by DEVx.
        /// </summary>
        [Fact]
        public void VA_Register_Test_via_DXDprovided_Api(){

            // Arrange
            // Initialize the Database
            TestDbInitializer.ClearDatabase();
            //Creating a user who is already a VA
            RegisterModel VA_User = new RegisterModel{
                UserName    = "VA_User",
                NameSurname = "VA User",
                Email       = "ekin@ekonteknoloji.com",
                Password    = TestDbInitializer.testPassword,
                RePassword  = TestDbInitializer.testPassword
            };
            
            // Act
            // Registered user method uses the DxDApiForUser ["https://backend.devxdao.com/api/va/email/"] API to check if the email value of the RegisterModel instance belongs to a VA User
            SimpleResponse result = controllers.authController.RegisterUser(VA_User);
            // Assert
            result.Success.Should().Be(true);
            ((User)result.Content).UserType.Should().Be("Internal");
        }

        /// <summary>
        /// Tests of AuthController.RegisterUser() to see if an already registered user can register again.
        /// 2 use cases are used.
        /// case -1- : attempt of 'Existing UserName' register
        /// case -2- : attempt of 'Existing Email' register
        /// </summary>
        [Fact]
        public void Existing_User_Register_Test(){

            // Arrange
            // Creating a new user which will be act as existing user
            RegisterModel existing_user = new RegisterModel{
                UserName    = "Exsiting_User",
                NameSurname = "Existing User",
                Email       = "existing@user.com",
                Password    = TestDbInitializer.testPassword,
                RePassword  = TestDbInitializer.testPassword
            };

            // model for case -1- : attempt of 'Existing UserName' register 
            RegisterModel new_user_1 = new RegisterModel{
                UserName    = "Exsiting_User",
                NameSurname = "Existing User1",
                Email       = "existing@user1.com",
                Password    = TestDbInitializer.testPassword,
                RePassword  = TestDbInitializer.testPassword
            };

            // model for case -2- : attempt of 'Existing Email' register
            RegisterModel new_user_2 = new RegisterModel{
                UserName    = "Exsiting_User1",
                NameSurname = "Existing User2",
                Email       = "existing@user.com",
                Password    = TestDbInitializer.testPassword,
                RePassword  = TestDbInitializer.testPassword
            };

            // Registering the user model which will act as existing user.
            var registered_user = controllers.authController.RegisterUser(existing_user);

            // Act case : -1-
            var case1_result = controllers.authController.RegisterUser(new_user_1);

            // Assert
            // case -1- : Existing UserName
            case1_result.Success.Should().Be(false);

            // Act case : -2-
            var case2_result = controllers.authController.RegisterUser(new_user_2);

            // Assert
            // case -1- : Existing UserEmail
            case2_result.Success.Should().Be(false);

        }
    }
}
