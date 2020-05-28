/* This is a context class that connects to the Sqlite data base.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FlightControlWeb.Data
{

    public class FcwContext : DbContext
    {
        public SqliteConnection conn { get; set; }

        /*
         * Ctor
         */
        public FcwContext(DbContextOptions<FcwContext> opt) : base(opt)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "./Data/flightDB.db";
            this.conn = new SqliteConnection(connectionStringBuilder.ConnectionString);
        }

    }
}