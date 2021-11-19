using System;
using RFPPortalWebsite.Models.ViewModels;
using System.Collections.Generic;
    
/// <summary>
/// This class prepares a set RegisterModel instances that are missing some values ​​to be tested in the AuthController.Register() method.   
/// </summary>
public class MissingValueUserData : IEnumerable<object[]>
{
    readonly List<object[]> _data = new List<object[]>{
        
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