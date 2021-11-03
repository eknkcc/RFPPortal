using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite
{
    public class Program
    {
        public class Settings
        {
            public string DbConnectionString { get; set; }
            public int InternalBiddingDays { get; set; }
            public int PublicBiddingDays { get; set; }
            public List<string> IpWhitelist { get; set; }
            public string EncryptionKey { get; set; }
            public string DxDApiForUser { get; set; }
            public string DxDApiToken { get; set; }
         
        }

        public static Settings _settings { get; set; } = new Settings();

        public static Monitizer monitizer;
        public static Utility.Mysql mysql = new Utility.Mysql();
        public static DbContextOptions dbOptions;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
