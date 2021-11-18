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
    /// <summary>
    /// RFP Portal, RfpController methods tests
    /// RfpController.SubmitRfpForm()
    /// DeleteBid()
    /// EditBid()
    /// ChooseWinningBid
    /// </summary>
    public class RfpController_Tests
    {
        
        PostTestController controllers;
        /// <summary>
        /// Initializing the database, posting controllers and seeds Rfps
        /// </summary>
        public RfpController_Tests(){
            TestDbInitializer.ClearDatabase();
            controllers = new PostTestController();
            TestDbInitializer.SeedRfp();
        }
        /// <summary>
        /// Test of RfpController.SubmitRfpForm() with proper Rfp model instance - Assert.True Succeeded
        /// Sets a fake HttpContext.Session which an Admin user is signed in
        /// </summary>
        [Fact]
        public void SubmitRfpForm(){
            //Arrange
            //Mocking HttpContext.Session
            var mockSession = new MockHttpSession();

            var cont = controllers.rfpController;
            
            controllers.rfpController.ControllerContext = new ControllerContext();
            controllers.rfpController.ControllerContext.HttpContext = new DefaultHttpContext();

            controllers.rfpController.ControllerContext.HttpContext.Session =  new MockHttpSession(); //.SetString("A","B");

            var _session = controllers.rfpController.ControllerContext.HttpContext.Session;

            //Setting Session Parameters
            _session.SetString("UserType", "Admin");
            _session.SetInt32("UserId", 1);            

            //Creating a proper RFP
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

            //Act
            var result = controllers.rfpController.SubmitRfpForm(rfp);
            //Assert
            Assert.True(result.Success);
        }

        /// <summary>
        /// Test of RfpController.GetRfpById(rfpId) method 
        /// Calling the required RFP model instance by RFP Id        
        ///</summary>
        [Fact]       
        public void GetRfpByRfpId_Test(){           
            //Arrange
            int id = 3;
            //Act
            Rfp result = controllers.rfpController.GetRfpById(id);
            //Assert
            result.RfpID.Should().Be(3);
        }

        /// <summary>
        /// Test of RfpController.GetRfpByStatus method.
        /// In the constructor of this class, 3 public RFPs are added by TestDbInitializer class 
        /// Returned list should have 3 elements - Assert.True Succeeded
        /// </summary>        
        [Fact]
        public void GetRfpsByStatus_Test(){
            //Act
            var result = controllers.rfpController.GetRfpsByStatus("Public");
            //Assert
            result.Count.Should().Be(1);
        }

        /// <summary>  
        ///  The TestDbInitializer.SeedRfp() method seeds the RFPs as the Rfp Status of the Rfp(id = 1) is Public
        ///  This method should only be accessed by an admin user. - Assert.True Succeeded
        /// </summary>
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

        ///<summary>
        ///Test of RfpController.GetRfpBidsByRfpId(RfpId) method.        
        ///</summary>
        [Fact]
        public void GetRfpBidsByRfpId_Test(){
            //Set Users, RFPs, Bids and returns bid count of RFPs
            TestDbInitializer.ClearDatabase();
            //method returns 
            IQueryable<Tuple<int,int>> correct_result = TestDbInitializer.SeedRfpBids();
            
            int RfpId = correct_result.FirstOrDefault().Item1;
            int BidCount = correct_result.FirstOrDefault().Item2;
           
            int count = controllers.rfpController.GetRfpBidsByRfpId(RfpId).Count();
            count.Should().Be(BidCount);
        }
    }
}