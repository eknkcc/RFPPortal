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
        /// Application controllers and Mock HttpContext Session are initialized.
        /// </summary>
        public BidController_Tests(){
            controllers = new PostTestController();
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            session = controllers.bidController.ControllerContext.HttpContext.Session = new MockHttpSession();
        }

        /// <summary>
        /// Test of BidController.SubmidBid() method for 2 use cases
        /// case 1 : proper bid is prepared and submitted
        /// case 2 : an internal user (VA) tries to bid on a public RFP.
        /// </summary>
        [Fact]
        public void SubmitBidTest(){

            // Arrange
            // Initialize the database
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            // case -1- : condition that should work correctly
            RfpBid bid = new RfpBid{
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };

            // inserting user type and user id to httpcontext mock session            
            session.SetString("UserType", "Public");
            session.SetInt32("UserId", envModel.PublicUserId);              

            // Act            
            var correct_response = controllers.bidController.SubmitBid(bid);
            // Assert            
            correct_response.Success.Should().Be(true);

            // case -2- : condition that should not work correctly
            RfpBid bid_2 = new RfpBid{
                UserId = envModel.InternalUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };            
            
            // inserting user type and user id to httpcontext mock session            
            session.SetString("UserType", "Internal");
            session.SetInt32("UserId", envModel.InternalUserId); 

            // Act            
            var incorrect_response = controllers.bidController.SubmitBid(bid);
            // Assert            
            incorrect_response.Success.Should().Be(false);
        }

        /// <summary>
        /// Test of BidController.DeleteBid(Bid) method.
        /// </summary>
        [Fact]
        public void DeleteBidTest(){

            // Arrange
            // Setting the database for deleting process environment
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            // Set
            // Rfp bid to delete is prepared
            RfpBid bid = new RfpBid{
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };            
            
            // Setting up mock session 
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            controllers.bidController.ControllerContext.HttpContext.Session =  new MockHttpSession();
            var _session = controllers.bidController.ControllerContext.HttpContext.Session;

            // inserting user type and user id to httpcontext mock session            
            _session.SetString("UserType", "Public");
            _session.SetInt32("UserId", envModel.PublicUserId);  
            // Adding bid to be deleted to the database            
            var response = controllers.bidController.SubmitBid(bid);
            response.Success.Should().Be(true);

            // Act
            // Calling the delete method
            var delete_response = controllers.bidController.DeleteBid(((RfpBid)response.Content).RfpBidID);
            // Assert
            delete_response.Success.Should().Be(true);
        }

        /// <summary>
        /// Testing of BidController.EditBid(Bid) method.
        /// Creates a bidding environment by creating an admin user and portal users, an RFP posted by the admin and biddings posted by users.
        /// The user edits the bid he posted. -Assert.True succeeded.
        /// </summary>
        [Fact]
        public void EditBidTest(){
            // Arrange
            // Setting the database for editing a bid environment by using TestDbInitializer class
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            // Set
            // Preparing the Rfp bid to edit
            RfpBid bid_to_be_edited = new RfpBid
            {
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };   

            // Setting up mock session
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            controllers.bidController.ControllerContext.HttpContext.Session =  new MockHttpSession();
            var _session = controllers.bidController.ControllerContext.HttpContext.Session;

            // inserting user type and user id to httpcontext mock session            
            _session.SetString("UserType", "Public");
            _session.SetInt32("UserId", envModel.PublicUserId);  
            _session.SetString("UserName", envModel.PublicUserName);

            // posting the new bid by user
            var response = controllers.bidController.SubmitBid(bid_to_be_edited);
            response.Success.Should().Be(true);

            // Getting the Id of the bid to be edited
            RfpBid called_bid = controllers.rfpController.GetRfpBidsByRfpId(envModel.RfpId).Find(a=>a.Username == envModel.PublicUserName).Bid;            

            // Preparing the edited bid.
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

            // Act
            // Calling the editing method
            SimpleResponse edit_response = controllers.bidController.EditBid(edited_bid);
            // Assert 
            // SimpleResponse model returned by the method has only boolean result of the process and status message by default
            // First assertion is, the 'Success' property of the returned SimpleResponse model should be true.
            edit_response.Success.Should().Be(true);

            // Calling the GetRfpBidsByRfpId method to check if the edited bid properties has changed.
            RfpBid latest_version_of_bid = controllers.rfpController.GetRfpBidsByRfpId(envModel.RfpId).Find(a=>a.Username == envModel.PublicUserName).Bid; 
            latest_version_of_bid.Amount.Should().Be(550001);            
        }

        /// <summary>
        /// Test of BidController.ChooseWinningBidTest() - Assert.True Succeeded.
        /// </summary>
        [Fact]
        public void ChooseWinningBidTest(){
            // Arrange
            // Initialize Database
            // Clear related tables if there are any data
            TestDbInitializer.ClearDatabase();
            // <summary>
            // Create and add Users, RFPs, Bids to the database
            TestDbInitializer.SeedRfpBids();

            // Gets the list of the bids
            // </summary>
            List<RfpBid> bids = TestDbInitializer.GetBids().ToList(); 
            // Sets the UserId session paramater with the Admin user id.
            session.SetInt32("UserId", bids.ElementAt(3).UserId);           

            // Act
            SimpleResponse result = controllers.bidController.ChooseWinningBid(bids.ElementAt(3).RfpBidID);

            // Assert
            result.Success.Should().Be(true);
        }        
    }
}