using FlightControlWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightControlWeb.Data
{
    public class FcwContext : DbContext
    {
        public FcwContext(DbContextOptions<FcwContext> opt) : base(opt)
        {
            
        }

        public DbSet<FlightPlan> FlightPlans { get; set; }
    }
}