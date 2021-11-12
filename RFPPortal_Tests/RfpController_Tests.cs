using System;
using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.Constants;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Linq;

namespace RFPPortal_Tests
{
 
    public class RfpController_Tests
    {
        PostTestController controllers;
        public RfpController_Tests(){
            controllers = new PostTestController();
            TestDbInitializer.SeedRfp();
        }

        public class RFPData : IEnumerable<object[]>
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

        [Fact]
        public void SubmitRfpForm(){

            var mockSession = new MockHttpSession();

            var cont = controllers.rfpController;
            
            controllers.rfpController.ControllerContext = new ControllerContext();
            controllers.rfpController.ControllerContext.HttpContext = new DefaultHttpContext();

            controllers.rfpController.ControllerContext.HttpContext.Session =  new MockHttpSession(); //.SetString("A","B");

            var _session = controllers.rfpController.ControllerContext.HttpContext.Session;

            _session.SetString("UserType", "Admin");
            _session.SetInt32("UserId", 1);            

            Rfp rfp = new Rfp{
                UserId = 1,
                CreateDate = DateTime.Now, 
                Status = "",
                Currency = "",
                Amount = 0,
                Timeframe = "",
                Title = "",
                Description = "",
                WinnerRfpBidID = null, 
                PublicBidEndDate = DateTime.Now.AddDays(40),
                InternalBidEndDate = DateTime.Now.AddDays(30)
            };

            //var mock = new Mock<ControllerContext>();
            
            var t = controllers.rfpController.SubmitRfpForm(rfp);
            Assert.True(t.Success);
            //t.Message.Should().Be("Rfp form succesfully posted.");
        }

        [Fact]       
        public void GetRfpByRfpId_Test(){           

            int id = 3;
            Rfp t = controllers.rfpController.GetRfpById(id);
            t.RfpID.Should().Be(3);
        }

        /// <summary>
        ///  DB seed method adds 3 public RFPs 
        ///  Returned list should have 3 elements
        /// </summary>
        /// <param name="Status">Rfp model</param>
        /// <returns>List<Rfp></returns>
        [Fact]
        public void GetRfpsByStatus_Test(){
            var result = controllers.rfpController.GetRfpsByStatus("Public");
            result.Count.Should().Be(1);
        }

        /// <summary>
        ///  By definition the Rfp Status of the Rfp(id = 1) is Public
        ///  This method can only be accessed by third party admin in ip whitelist.
        /// </summary>
        /// <param name="model">Rfp model</param>
        /// <returns>Ajax Response</returns>
        [Fact]
        public void ChangeRfpStatus_Test(){
            //Arrange
            Rfp rfp = controllers.rfpController.GetRfpById(1);
            rfp.Status.Should().Be("Public");
            rfp.Status = "Internal";

            var result = controllers.rfpController.ChangeRfpStatus(rfp);
            Rfp updatedRfp = (Rfp)result.Content;
            updatedRfp.Status.Should().Be("Internal");

            //RollBack
            updatedRfp.Status = "Public";
            controllers.rfpController.ChangeRfpStatus(updatedRfp);
        }

        [Fact]
        public void GetRfpBidsByRfpId_Test(){
            //Set Users, RFPs, Bids and returns bid count of RFPs
            TestDbInitializer.ClearDatabase();
            IQueryable<Tuple<int,int>> correct_result = TestDbInitializer.SeedRfpBids();
            
            int RfpId = correct_result.FirstOrDefault().Item1;
            int BidCount = correct_result.FirstOrDefault().Item2;
           
            int count = controllers.rfpController.GetRfpBidsByRfpId(RfpId).Count();
            count.Should().Be(BidCount);
        }
    }
}