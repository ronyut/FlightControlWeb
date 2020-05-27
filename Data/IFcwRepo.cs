using System.Collections.Generic;
using System.Threading.Tasks;
using FlightControlWeb.Models;

namespace FlightControlWeb.Data
{
    public interface IFcwRepo
    {
        Task<FlightPlan> GetFlightPlanByIdAsync(string id);
        Task<IEnumerable<Flight>> GetFlightsByTimeAsync(string date, bool isExternal);
        Response DeleteFlightById(string id);
        Response PostFlightPlan(FlightPlan flightPlan);
    }
}