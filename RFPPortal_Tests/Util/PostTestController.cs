using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Controllers;
using Microsoft.Extensions.Configuration;

namespace RFPPortal_Tests
{
    /// <summary>
    /// Creates instances of application controllers classes to be tested.
    /// AuthController
    /// RfpController
    /// BidController
    /// </summary>
    public class PostTestController
    {
        /// RFPPortalWebSite.AuthController instance authController
        public AuthController authController;
        /// RFPPortalWebSite.RfpController instance rfpController
        public RfpController rfpController;
        /// RFPPortalWebSite.AuthController instance authController
        public BidController bidController;
        // static constructor to initialize RFPPortalWebsite application using configuration data in "appsettings.json" file via Startup.LoadConfig(config) method.
        static PostTestController()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            RFPPortalWebsite.Startup.LoadConfig(config);
        }

        /// <summary>
        /// 1.Creates application dbcontext
        /// 2.Deletes the database instance
        /// 3.Creates instances of AuthController, RfpController, BidController
        /// </summary>
        public PostTestController()
        {
            var context = new rfpdb_context();
            context.Database.EnsureDeleted();
            RFPPortalWebsite.Startup.InitializeService();

            authController = new AuthController();
            rfpController  = new RfpController();
            bidController = new BidController();

        }        
    }
}