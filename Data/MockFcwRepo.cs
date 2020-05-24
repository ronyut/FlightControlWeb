using System.Collections.Generic;
using FlightControlWeb.Models;

namespace FlightControlWeb.Data
{
    public class MockFcwRepo : IFcwRepo
    {
        public IEnumerable<FlightPlan> GetAllFlightPlans()
        {
            var flightPlans = new List<FlightPlan>
            {
                new FlightPlan(0, 222, "El Al 0"),
                new FlightPlan(1, 333, "El Al 1"),
                new FlightPlan(2, 444, "El Al 2")
            };

            return flightPlans;
        }

        public FlightPlan GetFlightPlanById(int id)
        {
            return new FlightPlan(0, 222, "El Al 0");
        }
    }
}