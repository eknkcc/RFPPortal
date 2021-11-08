using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Controllers;
using Microsoft.Extensions.Configuration;

namespace RFPPortal_Tests
{
    public class PostTestController
    {
        public AuthController authController;
        public RfpController rfpController;
        static PostTestController()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            RFPPortalWebsite.Startup.LoadConfig(config);
        }

        public PostTestController()
        {
            var context = new rfpdb_context();
            context.Database.EnsureDeleted();
            RFPPortalWebsite.Startup.InitializeService();

            authController = new AuthController();
            rfpController  = new RfpController();
        }

        // public void Seed_Users(){
        //     TestDbInitializer.SeedUsers(new rfpdb_context());
        // }
        
    }
}