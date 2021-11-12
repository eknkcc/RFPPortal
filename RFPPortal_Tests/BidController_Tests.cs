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

namespace RFPPortal_Tests
{
    public class BidController_Tests
    {
        PostTestController controllers;
        ISession session;


        public BidController_Tests(){
            controllers = new PostTestController();
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            session = controllers.bidController.ControllerContext.HttpContext.Session = new MockHttpSession();
        }

        [Fact]
        public void SubmitBidTest(){

            //Arrange
            //Initialize the database
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            //condition that should work correctly
            RfpBid bid = new RfpBid{
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };

            session.SetString("UserType", "Public");
            session.SetInt32("UserId", envModel.PublicUserId);              

            var correct_response = controllers.bidController.SubmitBid(bid);
            correct_response.Success.Should().Be(true);

            //condition that should not work correctly
            RfpBid bid_2 = new RfpBid{
                UserId = envModel.InternalUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };            
            
            session.SetString("UserType", "Internal");
            session.SetInt32("UserId", envModel.InternalUserId); 

            var incorrect_response = controllers.bidController.SubmitBid(bid);
            incorrect_response.Success.Should().Be(false);
        }

        [Fact]
        public void DeleteBidTest(){

            //Arrange
            //Setting the database for deleting environment
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            //Set
            //Rfp bid to delete is prepared
            RfpBid bid = new RfpBid{
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };            
            
            //Seting up session parameters
            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            controllers.bidController.ControllerContext.HttpContext.Session =  new MockHttpSession();
            var _session = controllers.bidController.ControllerContext.HttpContext.Session;

            _session.SetString("UserType", "Public");
            _session.SetInt32("UserId", envModel.PublicUserId);  
            var response = controllers.bidController.SubmitBid(bid);
            response.Success.Should().Be(true);

            //Act
            //Calling the delete method
            //Response is an instance of the AjaxModel. Success parameter should be 'true'
            var delete_response = controllers.bidController.DeleteBid(((RfpBid)response.Content).RfpBidID);
            delete_response.Success.Should().Be(true);
        }

        [Fact]
        public void EditBidTest(){
            //Arrange
            //Setting the database for editing environment
            TestDbInitializer.ClearDatabase();
            BidInitializeModel envModel =  TestDbInitializer.BidInitializer();

            //Set
            //Preparing the Rfp bid to edit
            RfpBid bid_to_be_edited = new RfpBid
            {
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 500000,
                Note = "Bid notes from User input",
                Time = "Time frame from User input", 
            };   

            controllers.bidController.ControllerContext = new ControllerContext();
            controllers.bidController.ControllerContext.HttpContext = new DefaultHttpContext();
            controllers.bidController.ControllerContext.HttpContext.Session =  new MockHttpSession();
            var _session = controllers.bidController.ControllerContext.HttpContext.Session;

            _session.SetString("UserType", "Public");
            _session.SetInt32("UserId", envModel.PublicUserId);  
            _session.SetString("UserName", envModel.PublicUserName);
            var response = controllers.bidController.SubmitBid(bid_to_be_edited);
            response.Success.Should().Be(true);

            RfpBid call_bid = controllers.rfpController.GetRfpBidsByRfpId(envModel.RfpId).Find(a=>a.Username == envModel.PublicUserName).Bid; //.Find(bid_to_be_edited);

            

            
            RfpBid edited_bid = new RfpBid
            {
                UserId = envModel.PublicUserId,
                CreateDate = DateTime.Now,
                RfpID = envModel.RfpId,
                Amount = 550001,
                Note = "Edited Bid notes from User input",
                Time = "Updated Time frame from User input",
            };

            //AjaxResponse edit_response = controllers.bidController.EditBid(((RfpBid)response.Content).RfpBidID, edited_bid);




            
            
        }

        [Fact]
        public void ChooseWinningBidTest(){
            //Arrange
            //Initialize Database
            //Clear related tables if there are any data
            TestDbInitializer.ClearDatabase();
            //Create Users, RFPs, Bids
            TestDbInitializer.SeedRfpBids();

            List<RfpBid> bids = TestDbInitializer.GetBids().ToList(); 

            session.SetInt32("UserId", bids.ElementAt(3).UserId);           

            //Act
            SimpleResponse result = controllers.bidController.ChooseWinningBid(bids.ElementAt(3).RfpBidID);

            //Assert
            result.Success.Should().Be(true);
        }



        
    }
}