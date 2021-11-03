
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using RFPPortalWebsite.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Contexts
{
    /// <summary>
    ///  RFP Portal main database context 
    ///  EntityFrameworkCore CodeFirst approach is used.
    ///  Use Add-Migration <migration_text> command for migration.
    ///  Use Update-Database command to apply changes into database.
    /// </summary>
    public class rfpdb_context : DbContext
    {
        public rfpdb_context()
        {

        }

        public rfpdb_context(DbContextOptions options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL(Program._settings.DbConnectionString);
            }
        }

        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Rfp> Rfps { get; set; }
        public DbSet<RfpBid> RfpBids { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }

    }
}
