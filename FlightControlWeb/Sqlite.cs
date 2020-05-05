using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace FlightControlWeb
{
    public class Sqlite
    {
        public static void Run()
        {
            // create connection
            var connectionStringBuilder = new SQLiteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "./db/flightDB.db";

            // `using` is for auto-close of the db when it's not in use anymore
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                // create table
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = "CREATE TABLE beers(name VARCHAR(50))";
                tableCmd.ExecuteNonQuery();

                // insert some records
                using(var transaction = connection.BeginTransaction())
                {
                    var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO beers VALUES ('corona')";
                    insertCmd.ExecuteNonQuery();

                    insertCmd.CommandText = "INSERT INTO beers VALUES ('carlsberg')";
                    insertCmd.ExecuteNonQuery();

                    insertCmd.CommandText = "INSERT INTO beers VALUES ('heiniken')";
                    insertCmd.ExecuteNonQuery();

                    transaction.Commit();
                }

                // read records
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM beers";
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var result = reader.GetString(0);
                        Console.WriteLine(result);

                    }
                }
            }
        }
    }
}
