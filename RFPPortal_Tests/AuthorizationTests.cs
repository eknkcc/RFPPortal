using System;
using Xunit;
using RFPPortalWebsite.Models.ViewModels;
using FluentAssertions;
using System.Collections.Generic;

namespace RFPPortal_Tests
{
    public class UserData : IEnumerable<object[]>
    {
        public readonly List<object[]> _data = new List<object[]>{
            
            new object[]{
                new RegisterModel{
                    UserName    = "A_B",
                    NameSurname = "Regular User",
                    Email       = "regular@user.com",
                    Password    = "Password",
                    RePassword  = "Password"
                }
            },
            
            new object[]{
                new RegisterModel{
                    UserName    = "",
                    NameSurname = "Regular User",
                    Email       = "regular@user.com",
                    Password    = "Password",
                    RePassword  = "Password"
                }
            },

            new object[]{
                new RegisterModel{
                    UserName    = "regular_user",
                    NameSurname = "",
                    Email       = "regular@user.com",
                    Password    = "Password",
                    RePassword  = "Password"
                }
            },

            new object[]{
                new RegisterModel{
                    UserName    = "regular_user",
                    NameSurname = "Regular User",
                    Email       = "",
                    Password    = "Password",
                    RePassword  = "Password"
                }
            },

            new object[]{
                new RegisterModel{
                    UserName    = "regular_user",
                    NameSurname = "Regular User",
                    Email       = "regular@user.com",
                    Password    = "Password",
                    RePassword  = "Password1"
                }
            }    
        };

        IEnumerator<object[]> IEnumerable<object[]>.GetEnumerator() => _data.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public class AuthorizationTests
    {
        PostTestController controllers = new PostTestController();

        /// <summary>
        ///  Test of standart public user register.
        ///  Application method returns AjaxResponse model which the "bool Success" property is "true".
        /// </summary>
        /// <param name="regular_user_register">Valid model of ViewModel RegisterModel</param>
        /// <returns>bool result of Assert True</returns>
        [Fact]        
        public void Regular_Register_Test()
        {
            //Arrange
            RegisterModel regular_user_register = new RegisterModel{
                UserName    = "Regular_User",
                NameSurname = "Regular User",
                Email       = "regular@user.com",
                Password    = "Password",
                RePassword  = "Password"
            };

            //Act
            var c = controllers.authController.RegisterUser(regular_user_register);

            //Assert
            c.Success.Should().Be(true);
        }

        [Theory]
        [ClassData(typeof(UserData))]    
        public void Missing_value_Register_Test(RegisterModel model){
            
            var t = controllers.authController.RegisterUser(model);            

            //Arrange Act Assert
            Assert.False(controllers.authController.RegisterUser(model).Success);
        }

        [Theory]
        // [InlineData("taylan", "")]
        // [InlineData("", "taylan@ekonteknoloji.com")]
        [InlineData("User1", "PassW0rd")]
        public void Get_User_Info_Test(string email, string pass){

            //Arrange
            TestDbInitializer.SeedUsers();

            //Act
            var t = controllers.authController.GetUserInfo(email, pass);

            //Assert
            Assert.True(!t.Success);
        }

        [Fact]
        public void Existing_User_Register_Test(){

            //Arrange
            RegisterModel existing_user = new RegisterModel{
                UserName    = "Exsiting_User",
                NameSurname = "Existing User",
                Email       = "existing@user.com",
                Password    = "Password",
                RePassword  = "Password"
            };

            RegisterModel new_user = new RegisterModel{
                UserName    = "Exsiting_User",
                NameSurname = "Existing User1",
                Email       = "existing@user1.com",
                Password    = "Password",
                RePassword  = "Password"
            };

            var t = controllers.authController.RegisterUser(existing_user);

            var t2 = controllers.authController.RegisterUser(new_user);

            t.Success.Should().Be(true);
            t2.Success.Should().Be(false);
        }
    }
}
