using System;
using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Xunit.Sdk;

namespace RFPPortal_Tests
{
    /// <summary>
    /// RFP Portal, bid controller methods tests
    /// SubmitBid()
    /// DeleteBid()
    /// EditBid()
    /// ChooseWinningBid
    /// </summary>
    [Collection("Sequential")]
    public class BidController_Tests
    {
        PostTestController controllers;
        ISession session;

        /// <summary>
        /// Application controllers and HttpContext Session are initialized.
        /// </summary>
        public BidController_Tests(){
            controllers = new PostTestController();
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            session = controllers.bidController.ControllerContext.HttpContext.Session = new MockHttpSession();
        }

        /// <summary>
        /// SubmidBid method is tested for 2 use cases
        /// case 1 : proper bid is prepared and submitted
        /// case 2 : an internal user (VA) tries to bid on a public RFP.
        /// </summary>
        [Fact]
        public void SubmitBidTest(){

            /// <summary>
            /// Arrange
            /// Initialize the database
            /// </summary>
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            /// <summary>
            /// case -1- : condition that should work correctly
            /// </summary>
            RfpBid bid = new RfpBid{
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };

            /// <summary>
            /// inserting user type and user id to httpcontext mock session            
            /// </summary>
            session.SetString("UserType", "Public");
            session.SetInt32("UserId", envModel.PublicUserId);              

            /// <summary>
            /// Act            
            /// </summary>
            var correct_response = controllers.bidController.SubmitBid(bid);
            /// <summary>
            /// Assert            
            /// </summary>
            /// <returns>bool result of Assert.True</returns>
            correct_response.Success.Should().Be(true);

            /// <summary>
            /// case -2- : condition that should not work correctly
            /// </summary>
            RfpBid bid_2 = new RfpBid{
                UserId = envModel.InternalUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };            
            
            /// <summary>
            /// inserting user type and user id to httpcontext mock session            
            /// </summary>
            session.SetString("UserType", "Internal");
            session.SetInt32("UserId", envModel.InternalUserId); 

            /// <summary>
            /// Act            
            /// </summary>
            var incorrect_response = controllers.bidController.SubmitBid(bid);
            /// <summary>
            /// Assert            
            /// </summary>
            /// <returns>bool result of Assert.False</returns>
            incorrect_response.Success.Should().Be(false);
        }

        /// <summary>
        /// Bid deleting test        
        /// </summary>
        [Fact]
        public void DeleteBidTest(){

            /// <summary>
            /// Arrange
            /// Setting the database for deleting process environment
            /// </summary>
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            /// <summary>
            /// Set
            /// Rfp bid to delete is prepared
            /// </summary>
            RfpBid bid = new RfpBid{
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };            
            
            /// <summary>
            /// Setting up mock session 
            /// </summary>
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            controllers.bidController.ControllerContext.HttpContext.Session =  new MockHttpSession();
            var _session = controllers.bidController.ControllerContext.HttpContext.Session;

            /// <summary>
            /// inserting user type and user id to httpcontext mock session            
            /// </summary>
            _session.SetString("UserType", "Public");
            _session.SetInt32("UserId", envModel.PublicUserId);  
            /// <summary>
            /// Adding bid to be deleted to the database            
            /// </summary>
            var response = controllers.bidController.SubmitBid(bid);
            response.Success.Should().Be(true);

            /// <summary>
            /// Act
            /// Calling the delete method
            /// </summary>
            var delete_response = controllers.bidController.DeleteBid(((RfpBid)response.Content).RfpBidID);
            /// <summary>
            /// Assert
            /// </summary>
            /// <returns>bool result of Assert.False</returns>
            delete_response.Success.Should().Be(true);
        }

        /// <summary>
        /// Testing the bid editin process
        /// Creates a bidding environment by creating an admin user and portal users, an RFP registered by the admin and biddings posted by users.
        /// The user edits the bid he posted.
        /// </summary>
        [Fact]
        public void EditBidTest(){
            /// <summary>
            /// Arrange
            /// Setting the database for editing a bid environment by using TestDbInitializer class
            /// </summary>
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            /// <summary>
            /// Set
            /// Preparing the Rfp bid to edit
            /// </summary>
            RfpBid bid_to_be_edited = new RfpBid
            {
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };   

            /// <summary>
            /// Setting up mock session
            /// </summary>
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            controllers.bidController.ControllerContext.HttpContext.Session =  new MockHttpSession();
            var _session = controllers.bidController.ControllerContext.HttpContext.Session;

            /// <summary>
            /// inserting user type and user id to httpcontext mock session            
            /// </summary>
            _session.SetString("UserType", "Public");
            _session.SetInt32("UserId", envModel.PublicUserId);  
            _session.SetString("UserName", envModel.PublicUserName);

            /// <summary>
            /// posting the new bid by user
            /// </summary>
            var response = controllers.bidController.SubmitBid(bid_to_be_edited);
            response.Success.Should().Be(true);

            /// <summary>
            /// Getting the Id of the bid to be edited
            /// </summary>
            RfpBid called_bid = controllers.rfpController.GetRfpBidsByRfpId(envModel.RfpId).Find(a=>a.Username == envModel.PublicUserName).Bid;            

            /// <summary>
            /// Preparing the edited bid.
            /// </summary>
            RfpBid edited_bid = new RfpBid
            {
                RfpBidID = called_bid.RfpBidID,
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 550001,
                Note = "Edited Bid notes from User input",
                Time = "Updated Time frame from User input",
            };

            /// <summary>
            /// Act
            /// Calling the editing method
            /// </summary>
            /// <param name="edited_bid">RfpBid model instance with edited properties</param>
            /// <returns>instance of SimpleResponse </returns>
            SimpleResponse edit_response = controllers.bidController.EditBid(edited_bid);
            /// <summary>
            /// Assert 
            /// SimpleResponse model returned by the method has only boolean result of the process and status message by default
            /// First assertion is, the 'Success' property of the returned SimpleResponse model should be true.
            /// </summary>
            /// <returns>Boolean result of Assert.True</returns>
            edit_response.Success.Should().Be(true);

            /// <summary>
            /// Act
            /// Calling the GetRfpBidsByRfpId method to check if the edited bid properties has changed.
            /// </summary>
            /// <returns>Instance of SimpleResponse model filled with the latest version of the edited bid</returns>
            RfpBid latest_version_of_bid = controllers.rfpController.GetRfpBidsByRfpId(envModel.RfpId).Find(a=>a.Username == envModel.PublicUserName).Bid; 
            latest_version_of_bid.Amount.Should().Be(550001);            
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void ChooseWinningBidTest(){
            /// <summary>
            /// Arrange
            /// Initialize Database
            /// Clear related tables if there are any data
            /// </summary>
            TestDbInitializer.ClearDatabase();
            /// <summary>
            /// Create and add Users, RFPs, Bids to the database
            /// </summary>
            TestDbInitializer.SeedRfpBids();

            /// <summary>
            /// Gets the list of the bids
            /// </summary>
            /// <returns>List of bids</returns>
            List<RfpBid> bids = TestDbInitializer.GetBids().ToList(); 
            /// <summary>Sets the UserId session paramater with the</summary>
            session.SetInt32("UserId", bids.ElementAt(3).UserId);           

            //Act
            SimpleResponse result = controllers.bidController.ChooseWinningBid(bids.ElementAt(3).RfpBidID);

            //Assert
            result.Success.Should().Be(true);
        }        
    }
}