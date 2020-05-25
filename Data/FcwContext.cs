using FlightControlWeb.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FlightControlWeb.Data
{

    public class FcwContext : DbContext
    {
        public SqliteConnection conn { get; set; }

        public FcwContext(DbContextOptions<FcwContext> opt) : base(opt)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "./Data/flightDB.db";
            this.conn = new SqliteConnection(connectionStringBuilder.ConnectionString);
        }

    }
}