using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.Constants;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using PagedList.Core;
using System.ComponentModel;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stripe;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using RFPPortalWebsite.Models.ViewModels;

namespace RFPPortal_Tests
{
    public class BidInitializeModel{
        public int AdminUserId { get; set; } = 0;
        public string AdminUserName {get; set; }
        public int PublicUserId { get;set; } = 0;
        public string PublicUserName { get; set; }
        public int InternalUserId { get; set; } = 0;
        public string InternalUserName { get; set; }
        public int RfpId { get; set; } = 0;
    }
    
    public static class TestDbInitializer
    {
        static rfpdb_context context;
        static TestDbInitializer()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            RFPPortalWebsite.Startup.LoadConfig(config);

            context = new rfpdb_context();
            context.ChangeTracker.Clear();
            context.Database.EnsureDeleted();
            RFPPortalWebsite.Startup.InitializeService();
        }

        static string Encrypt(string password){
            return RFPPortalWebsite.Utility.Encryption.EncryptPassword(password);
        }

        public static int ActivateUserMock(User usr){
            context.Users.Where(u=>u.UserId == usr.UserId).FirstOrDefault().IsActive = true;            
            return context.SaveChanges();
        }
        public static void SeedUsers(){

            context.Users.AddRange(
                new User{
                    UserName    = "PublicUser1",
                    NameSurname = "User 1",
                    Email       = "public@user1.com",
                    UserType    = Enums.UserIdentityType.Public.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    // Password    = "m8pz2xVkmv9xd1gBoczRTjUALudqlwDqWk42h0KGDr6Ui4Ny",
                    IsActive    = true

                },
                new User{
                    UserName    = "PublicUser2",
                    NameSurname = "User 2",
                    Email       = "public@user2.com",
                    UserType    = Enums.UserIdentityType.Public.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    IsActive    = true

                },
                new User{
                    UserName    = "InternalUser1",
                    NameSurname = "Internal User1",
                    Email       = "internal@user1.com",
                    UserType    = Enums.UserIdentityType.Internal.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    IsActive    = true

                },
                new User{
                    UserName    = "InternalUser2",
                    NameSurname = "Internal User2",
                    Email       = "internal@user2.com",
                    UserType    = Enums.UserIdentityType.Internal.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    IsActive    = true

                },
                new User{
                    UserName    = "AdminUser",
                    NameSurname = "Admin User",
                    Email       = "admin@user.com",
                    UserType    = Enums.UserIdentityType.Admin.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    IsActive    = true

                },
                new User{
                    UserName    = "PublicUser3",
                    NameSurname = "User 3",
                    Email       = "public@user3.com",
                    UserType    = Enums.UserIdentityType.Public.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    IsActive    = true

                },
                new User{
                    UserName    = "PublicUser4",
                    NameSurname = "User 4",
                    Email       = "public@user4.com",
                    UserType    = Enums.UserIdentityType.Public.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    IsActive    = true

                },
                new User{
                    UserName    = "InternalUser3",
                    NameSurname = "Internal User3",
                    Email       = "internal@user3.com",
                    UserType    = Enums.UserIdentityType.Internal.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    IsActive    = true

                },
                new User{
                    UserName    = "InternalUser4",
                    NameSurname = "Internal User4",
                    Email       = "internal@user4.com",
                    UserType    = Enums.UserIdentityType.Internal.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = Encrypt("PassW0rd"),
                    IsActive    = true

                }
            );
            context.SaveChanges();
            context.ChangeTracker.Clear();

        }
        
        /// <summary>
        /// Creates an Admin user and adds to the database
        /// 
        /// </summary>
        public static void SeedRfp(){

            User AdminUser;
            if (context.Users.Where(a => a.UserType == "Admin").Count() == 0){            
                context.Users.Add(AdminUser = 
                    new User{
                        UserName    = "AdminUsr",
                        NameSurname = "Admin User",
                        Email       = "admin@user.com",
                        UserType    = Enums.UserIdentityType.Admin.ToString(),
                        CreateDate  = DateTime.Now,
                        Password    = Encrypt("PassW0rd"),
                        IsActive    = true
                    }
                );
            } else {
                AdminUser = context.Users.Where(a => a.UserType == "Admin").FirstOrDefault();
            }                            

            context.Rfps.AddRange(
                new Rfp{
                    UserId = AdminUser.UserId,
                    CreateDate = Convert.ToDateTime("2021-10-05 15:39:25"), 
                    Status = "Public",
                    Currency = "€",
                    Amount = 500000,
                    Timeframe = "12-17-2021 - 12-31-2021",
                    Title = "This is the first RFP of DEVxDAO RFP Platform",
                    Description = "Description of this job",
                    WinnerRfpBidID = null, 
                    PublicBidEndDate = Convert.ToDateTime("2021-11-17 15:39:25"),
                    InternalBidEndDate = Convert.ToDateTime("2021-10-26 15:39:25")
                },
                new Rfp{
                    UserId = AdminUser.UserId,
                    CreateDate = Convert.ToDateTime("2021-11-05 15:41:25"), 
                    Status = "Internal",
                    Currency = "€",
                    Amount = 1000,
                    Timeframe = "01-13-2022 - 12-31-2022",
                    Title = "Second Rfp ",
                    Description = "Proposal Request Portal is the platform which RFPs are posted by the authenticated third party (DevxDao) and which allows public users can bid. After the bidding process authenticated third party will be able to choose wining bid from the portal. Winning bid will be sent to marketing channels. RFP Portal will also have an API endpoint for read only public information.",
                    WinnerRfpBidID = null, 
                    PublicBidEndDate = Convert.ToDateTime("2021-12-17 15:41:25"),
                    InternalBidEndDate = Convert.ToDateTime("2021-11-17 15:39:25")
                },
                new Rfp{
                    UserId = AdminUser.UserId,
                    CreateDate = Convert.ToDateTime("2021-05-05 15:47:56"), 
                    Status = "Completed",
                    Currency = "€",
                    Amount = 7200,
                    Timeframe = "06-17-2021 - 01-15-2022",
                    Title = "RFP Portal",
                    Description = "Proposal Request Portal is the platform which RFPs are posted by the authenticated third party (DevxDao) and which allows public users can bid. After the bidding process authenticated third party will be able to choose wining bid from the portal. Winning bid will be sent to marketing channels. RFP Portal will also have an API endpoint for read only public information.",
                    WinnerRfpBidID = null, 
                    PublicBidEndDate = Convert.ToDateTime("2021-05-17 15:47:56"),
                    InternalBidEndDate = Convert.ToDateTime("2021-11-26 15:49:46")
                },
                new Rfp{
                    UserId = AdminUser.UserId,
                    CreateDate = Convert.ToDateTime("2021-11-05 15:49:46"), 
                    Status = "Internal",
                    Currency = "€",
                    Amount = 48492,
                    Timeframe = "12-17-2021 - 12-30-2021",
                    Title = "Development of Services DAO Platform",
                    Description = "The services DAO portal will provide a platform for the service DAOs which provide services in a decentralized manner through a modular implementation of MVPR. The platform will serve as a decentralized organization and will have a micro-services architecture which can be adapted for various types of service DAOs such as code review, RFP and bounty systems. The reference implementation will be focused on the code review jobs. Job Posters will post threads(Jobs) in the Reddit style forum. External and internal users will be able to bid on jobs which are posted by the Job Poster, however external users can only participate by paying a DoS fee. Also internal users will have privilige in the auction process. An auction module will be implemented to manage the process and determine the winning bid. After the auction process, informal and formal voting starts respectively. In the informal vote DAO members will bid reputation with their vote to show how they intend to vote and in the formal vote DAO members will bid their reputation. At the end of formal voting reputation staked to losing side will be splitted pro-rata amongst the members of the winning side. The platform will be built with microservice architectur and docker containerazation will be supported. Detailed architecture documents and workflow diagrams can be found in the attachment",
                    WinnerRfpBidID = null, 
                    PublicBidEndDate = Convert.ToDateTime("2021-12-17 15:49:46"),
                    InternalBidEndDate = Convert.ToDateTime("2021-11-26 15:49:46")
                }
            );            
            context.SaveChanges();
            context.ChangeTracker.Clear();
        }

        /// <summary>
        /// Truncates User, RFPs and Bids table in the database
        /// </summary>
        public static void ClearDatabase(){
            foreach(var user in context.Users){
                context.Remove(user);
                context.Entry(user).State = EntityState.Detached;
            }
            foreach(var bid in context.RfpBids){
                context.Remove(bid);
                context.Entry(bid).State = EntityState.Detached;
            }
            foreach(var rfp in context.Rfps){
                context.Remove(rfp);
                context.Entry(rfp).State = EntityState.Detached;
            }
            context.SaveChanges(); 
        }
        
        /// <summary>
        /// Creates a bidding environment with multiple RFPs
        /// </summary>
        /// <returns>(IQueryable<Tuple<int,int>>) How many bids of which RFP</returns>
        public static IQueryable<Tuple<int,int>> SeedRfpBids(){
            
            SeedUsers();
            SeedRfp();
            List<int> public_user_ids = new List<int>();
            List<int> internal_user_ids = new List<int>();
            public_user_ids = context.Users.AsNoTracking().Where(u => u.UserType == "Public").Select(a => a.UserId).ToList();
            internal_user_ids = context.Users.AsNoTracking().Where(u => u.UserType == "Internal").Select(a => a.UserId).ToList();

            List<int> public_Rfp_ids = new List<int>();
            List<int> internal_Rfp_ids = new List<int>();
            public_Rfp_ids = context.Rfps.AsNoTracking().Where(u => u.Status == "Public").Select(a => a.RfpID).ToList();
            internal_Rfp_ids = context.Rfps.AsNoTracking().Where(u => u.Status == "Internal").Select(a => a.RfpID).ToList();
            
            context.RfpBids.AddRange(
                //Bidding on Public RFP public user #1               
                new RfpBid{
                    UserId = public_user_ids.ElementAt(0),
                    CreateDate = DateTime.Now,
                    RfpID = public_Rfp_ids.ElementAt(0),
                    Amount = 500000,
                    Note = "Public RFP Bid -1- notes from User input",
                    Time = "Time frame from User input", 
                },
                //Bidding on Public RFP public user #2                
                new RfpBid{
                    UserId = public_user_ids.ElementAt(1),
                    CreateDate = DateTime.Now,
                    RfpID = public_Rfp_ids.ElementAt(0),
                    Amount = 500000,
                    Note = "Public RFP Bid -2- notes from User input",
                    Time = "Time frame from User input", 
                },
                //Bidding on Public RFP public user #3
                new RfpBid{
                    UserId = public_user_ids.ElementAt(2),
                    CreateDate = DateTime.Now,
                    RfpID = public_Rfp_ids.ElementAt(0),
                    Amount = 500000,
                    Note = "Bid notes from User input",
                    Time = "Time frame from User input", 
                },
                //Bidding on Public RFP public user #4
                new RfpBid{
                    UserId = public_user_ids.ElementAt(3),
                    CreateDate = DateTime.Now,
                    RfpID = public_Rfp_ids.ElementAt(0),
                    Amount = 500000,
                    Note = "Bid notes from User input",
                    Time = "Time frame from User input", 
                },
                //Bidding on Internal RFP -1- internal user #1
                new RfpBid{
                    UserId = internal_user_ids.ElementAt(0),
                    CreateDate = DateTime.Now,
                    RfpID = internal_Rfp_ids.ElementAt(0),
                    Amount = 500000,
                    Note = "Bid notes from User input",
                    Time = "Time frame from User input", 
                },
                //Bidding on Internal RFP -1- internal user #2
                new RfpBid{
                    UserId = internal_user_ids.ElementAt(1),
                    CreateDate = DateTime.Now,
                    RfpID = internal_Rfp_ids.ElementAt(0),
                    Amount = 500000,
                    Note = "Bid notes from User input",
                    Time = "Time frame from User input", 
                },
                //Bidding on Internal RFP -2- internal user #3
                new RfpBid{
                    UserId = internal_user_ids.ElementAt(2),
                    CreateDate = DateTime.Now,
                    RfpID = internal_Rfp_ids.ElementAt(1),
                    Amount = 500000,
                    Note = "Bid notes from User input",
                    Time = "Time frame from User input", 
                },
                //Bidding on Internal RFP -2- internal user #4
                new RfpBid{
                    UserId = internal_user_ids.ElementAt(3),
                    CreateDate = DateTime.Now,
                    RfpID = internal_Rfp_ids.ElementAt(1),
                    Amount = 500000,
                    Note = "Bid notes from User input",
                    Time = "Time frame from User input", 
                }                
            );
            context.SaveChanges();
            context.ChangeTracker.Clear();

            IQueryable<Tuple<int,int>> counts = context.RfpBids.GroupBy(r => r.RfpID)
                .OrderBy(group => group.Key)
                .Select(group => Tuple.Create(group.Key, group.Count()));            

            return counts;
        }

        /// <summary>
        /// Mocks a bidding environment.
        /// Creates and adds 3 different types of users (public, admin and internal) to the database
        /// Creates and adds a public RFP created by admin to the database
        /// </summary>
        /// <returns>An instance of BidInitializeModel. The model holds environmental variables of a bidding instance like user ids, usernames and RFP id </returns>
        public static BidInitializeModel BidInitializer(){

            User public_user = new User{
                    UserName    = "PublicUser",
                    NameSurname = "Public User",
                    Email       = "public@user.com",
                    UserType    = Enums.UserIdentityType.Public.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = "PassW0rd",
                    IsActive    = true
                    };
            User admin_user = new User{
                    UserName    = "AdminUser",
                    NameSurname = "Admin User",
                    Email       = "admin@user.com",
                    UserType    = Enums.UserIdentityType.Admin.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = "PassW0rd",
                    IsActive    = true
                };
            User internal_user = new User{
                    UserName    = "InternalUser",
                    NameSurname = "Internal User",
                    Email       = "internal@user.com",
                    UserType    = Enums.UserIdentityType.Internal.ToString(),
                    CreateDate  = DateTime.Now,
                    Password    = "PassW0rd",
                    IsActive    = true
                };
            context.Users.AddRange(
                public_user, admin_user, internal_user
            );
            context.SaveChanges();
            context.ChangeTracker.Clear();

            Rfp rfp = new Rfp{
                    UserId = admin_user.UserId,
                    CreateDate = Convert.ToDateTime("2021-10-05 15:39:25"), 
                    Status = "Public",
                    Currency = "€",
                    Amount = 500000,
                    Timeframe = "12-17-2021 - 12-31-2021",
                    Title = "This is the first RFP of DEVxDAO RFP Platform",
                    Description = "Description of this job",
                    WinnerRfpBidID = null, 
                    PublicBidEndDate = Convert.ToDateTime("2021-11-17 15:39:25"),
                    InternalBidEndDate = Convert.ToDateTime("2021-10-26 15:39:25")
                };
            context.Rfps.Add(rfp);
            context.SaveChanges();

            BidInitializeModel initialBidEnvironment = new BidInitializeModel{
                PublicUserId       = public_user.UserId
                , PublicUserName   = public_user.UserName
                , AdminUserId      = admin_user.UserId
                , AdminUserName    = admin_user.UserName
                , InternalUserId   = internal_user.UserId
                , InternalUserName = internal_user.UserName
                , RfpId            = rfp.RfpID
            };

            return initialBidEnvironment;

        }

        /// <summary>
        /// Returns all bids for testing purposes.
        /// </summary>
        ///<returns>List<RfpBid></returns>
        public static List<RfpBid> GetBids(){
            return context.RfpBids.ToList();
        }

        /// <summary>
        /// Returns requested bid for testing purposes.
        /// </summary>
        /// <param name="bidId">bid id</param>
        /// <returns>RfpBid</returns>
        public static RfpBid GetBid(int bidId){
            return context.RfpBids.Where(a=>a.RfpBidID == bidId).FirstOrDefault();
        }

        /// <summary>
        /// Returns all users for testing purposes.
        /// </summary>
        /// <returns>List<User></returns>
        public static List<User> GetUsers(){
            return context.Users.ToList();
        }        

        /// <summary>
        /// Returns requested user for testing purposes.
        /// </summary>
        /// <param name="UserId">user id</param>
        /// <returns>User</returns>
        public static User GetUser(int userId){
            return context.Users.Where(u => u.UserId == userId).FirstOrDefault();
        }

        /// <summary>
        /// Returns all Rfps for testing purposes.
        /// </summary>
        /// <returns>List<Rfp></returns>
        public static List<Rfp> GetRfps(){
            return context.Rfps.ToList();
        }

        /// <summary>
        /// Returns requested Rfp for testing purposes.
        /// </summary>
        /// <param name="RfpId">rfp id</param>
        /// <returns>RFP</returns>
        public static Rfp GetRfp(int rfpId){
            return context.Rfps.Where(z => z.RfpID == rfpId).FirstOrDefault();
        }


              
            
    
    
    }
}