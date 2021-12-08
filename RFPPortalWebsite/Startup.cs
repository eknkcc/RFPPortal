using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static RFPPortalWebsite.Models.Constants.Enums;
using static RFPPortalWebsite.Program;
using Stripe;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace RFPPortalWebsite
{
    public class Startup
    {
        public static System.Timers.Timer rfpStatusTimer;
        public static bool rfpCheckInProgress = false;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            //Get related appsettings.json file (Development or Production)
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            LoadConfig(configuration);
            InitializeService();
        }

        /// <summary>
        ///  Loads application config from appsettings.json
        /// </summary>
        /// <param name="configuration"></param>
        public static void LoadConfig(IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            var config = configuration.GetSection("PlatformSettings");
            config.Bind(_settings);
        }

        /// <summary>
        ///  Initializes application (Db migrations, connection check, timer construction)
        /// </summary>
        public static void InitializeService()
        {
            Encryption.EncryptionKey = Program._settings.EncryptionKey;

            monitizer = new Monitizer();

            //Mysql migration 
            ApplicationStartResult mysqlMigrationcontrol = mysql.Migrate(new rfpdb_context().Database);
            if (!mysqlMigrationcontrol.Success)
            {
                monitizer.startSuccesful = -1;
                monitizer.AddException(mysqlMigrationcontrol.Exception, LogTypes.ApplicationError, true);
            }

            //Mysql connection check
            ApplicationStartResult mysqlcontrol = mysql.Connect(_settings.DbConnectionString);
            if (!mysqlcontrol.Success)
            {
                monitizer.startSuccesful = -1;
                monitizer.AddException(mysqlcontrol.Exception, LogTypes.ApplicationError, true);
            }

            if (monitizer.startSuccesful != -1)
            {
                monitizer.startSuccesful = 1;
                monitizer.AddApplicationLog(LogTypes.ApplicationLog, monitizer.appName + " application started successfully.");
            }

            //Rfp status timer
            rfpStatusTimer = new System.Timers.Timer(10000);
            rfpStatusTimer.Elapsed += CheckRfpStatus;
            rfpStatusTimer.AutoReset = true;
            rfpStatusTimer.Enabled = true;
        }

        /// <summary>
        ///  Checks RFP statuses with time interval.
        ///  If RFP internal bidding is ended, updates RFP status as Public
        ///  If RFP public bidding is ended without winner, updates status as Expired
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void CheckRfpStatus(Object source, ElapsedEventArgs e)
        {
            if (rfpCheckInProgress) return;

            rfpCheckInProgress = true;

            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Check if Rfp internal bidding ended and public bidding started
                    var publicRfps = db.Rfps.Where(x => x.Status == Models.Constants.Enums.RfpStatusTypes.Internal.ToString() && x.InternalBidEndDate < DateTime.Now && x.WinnerRfpBidID == null).ToList();

                    //Update rfp status
                    foreach (var rfp in publicRfps)
                    {
                        rfp.Status = Models.Constants.Enums.RfpStatusTypes.Public.ToString();
                        db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        db.SaveChanges();

                        Program.monitizer.AddApplicationLog(LogTypes.ApplicationLog, "Public bidding started for RFP #" + rfp.RfpID, "RfpID", rfp.RfpID);
                    }

                    ////This section is commented out because at the end of public bidding, bids will be posted to DEVxDAO survey and survey result will determine the winner.
                    ////Check if rfp public bidding ended without any winner
                    //var expiredRfps = db.Rfps.Where(x => x.Status == Models.Constants.Enums.RfpStatusTypes.Public.ToString() && x.PublicBidEndDate < DateTime.Now && x.WinnerRfpBidID == null).ToList();
                    ////Update rfp status
                    //foreach (var rfp in expiredRfps)
                    //{
                    //    rfp.Status = Models.Constants.Enums.RfpStatusTypes.Expired.ToString();
                    //    db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    //    db.SaveChanges();
                    //}

                    //Check if rfp public bidding completed
                    var completedRfps = db.Rfps.Where(x => x.Status == Models.Constants.Enums.RfpStatusTypes.Public.ToString() && x.PublicBidEndDate < DateTime.Now && x.WinnerRfpBidID == null).ToList();

                    foreach (var rfp in completedRfps)
                    {
                        rfp.Status = Models.Constants.Enums.RfpStatusTypes.Completed.ToString();
                        db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        db.SaveChanges();


                        //Post survey to DxD
                        DxDSurvey survey = new DxDSurvey();
                        survey.job_description = rfp.Description;
                        string[] startDate = rfp.Timeframe.Split('-')[0].Trim().Split("/");
                        survey.job_start_date = new DateTime(Convert.ToInt32(startDate[2]), Convert.ToInt32(startDate[0]), Convert.ToInt32(startDate[1])).ToString("yyyy-MM-dd HH:mm:ss");
                        string[] endDate = rfp.Timeframe.Split('-')[1].Trim().Split("/");
                        survey.job_end_date = new DateTime(Convert.ToInt32(endDate[2]), Convert.ToInt32(endDate[0]), Convert.ToInt32(endDate[1])).ToString("yyyy-MM-dd HH:mm:ss");
                        survey.job_title = rfp.Title;
                        survey.survey_hours = Program._settings.SurveyHours;
                        survey.total_price = Convert.ToInt32(rfp.Amount);

                        var bids = db.RfpBids.Where(x => x.RfpID == rfp.RfpID).ToList();
                        foreach (var bid in bids)
                        {
                            var bidUser = db.Users.Find(bid.UserId);

                            DateTime expectedDelivery = new DateTime(Convert.ToInt32(startDate[2]), Convert.ToInt32(startDate[0]), Convert.ToInt32(startDate[1])).AddDays(Convert.ToInt32(bid.Time));

                            DxDBid dxdbid = new DxDBid() { name = bidUser.NameSurname, forum = bidUser.UserName, email = bidUser.Email, amount_of_bid = Convert.ToInt32(bid.Amount), additional_note = bid.Note, delivery_date = expectedDelivery.ToString("yyyy-MM-dd HH:mm:ss") };

                            survey.bids.Add(dxdbid);
                        }
                        
                        //Send to DEVxDAO survey
                        var surveyJson = Utility.Request.PostDxD(Program._settings.DxDApiForUser + "/api/rfp/survey", Utility.Serializers.SerializeJson(survey), Program._settings.DxDApiToken);

                        Program.monitizer.AddApplicationLog(LogTypes.ApplicationLog, "Public bidding ended for RFP #" + rfp.RfpID + ". DEVxDAO survey post response:" + surveyJson, "RfpID", rfp.RfpID);
                    }

                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddConsole("Exception in timer CheckRfpStatus. Ex:" + ex.Message);
            }

            rfpCheckInProgress = false;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(6);
                //options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddControllersWithViews();
            services.AddMvc().AddControllersAsServices();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseStaticFiles();

            var defaultDateCulture = "en-US";
            var ci = new CultureInfo(defaultDateCulture);
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            ci.NumberFormat.NumberGroupSeparator = ",";

            // Configure the Localization middleware
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(ci),
                SupportedCultures = new List<CultureInfo> { ci, },
                SupportedUICultures = new List<CultureInfo> { ci, }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
