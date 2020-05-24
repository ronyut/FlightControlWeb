using System.Collections.Generic;
using FlightControlWeb.Models;

namespace FlightControlWeb.Data
{
    public interface IFcwRepo
    {
        IEnumerable<FlightPlan> GetAllFlightPlans();
        FlightPlan GetFlightPlanById(int id);
    }
}