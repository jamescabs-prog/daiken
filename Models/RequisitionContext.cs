using System;
using System.Data.Entity;
using System.Web.Configuration;

namespace Drafters.Models
{
    /// <summary>
    /// Entity Framework context for the MySQL database "requisition_db"
    /// (created in MySQL Workbench with Database/requisition_db_mysql.sql).
    /// The connection string is "RequisitionDB" in Web.config.
    /// The MySQL provider is loaded by name, so the project compiles even before the NuGet package is installed.
    /// </summary>
    [DbConfigurationType("MySql.Data.EntityFramework.MySqlEFConfiguration, MySql.Data.EntityFramework")]
    public class RequisitionContext : DbContext
    {
        public RequisitionContext() : base(CheckedConnectionName())
        {
            // the database and tables already exist (made by the MySQL script) - EF must not try to create or change them
            Database.SetInitializer<RequisitionContext>(null);
        }

        private static string CheckedConnectionName()
        {
            var cs = WebConfigurationManager.ConnectionStrings["RequisitionDB"];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new InvalidOperationException("Web.config has no connection string named 'RequisitionDB'. Add the MySQL connection string.");
            return "name=RequisitionDB";
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<RequisitionForm> Requisitions { get; set; }
        public DbSet<RequisitionLine> RequisitionLines { get; set; }
        public DbSet<IssuanceSlip> IssuanceSlips { get; set; }
        public DbSet<IssuanceSlipLine> IssuanceSlipLines { get; set; }
        public DbSet<ReplenishmentRequest> ReplenishmentRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TransactionLog> TransactionLogs { get; set; }
    }
}
