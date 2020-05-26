using System.Collections.Generic;
using FlightControlWeb.Models;

namespace FlightControlWeb.Data
{
    public interface IFcwRepo
    {
        IEnumerable<FlightPlan> GetAllFlightPlans();
        FlightPlan GetFlightPlanById(string id);
        IEnumerable<Flight> GetFlightsByTime(string date, bool isExternal);
        Response DeleteFlightById(string id);
        Response PostFlightPlan(FlightPlan flightPlan);
    }
}