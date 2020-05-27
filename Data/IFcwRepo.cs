using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FlightControlWeb.Models;

namespace FlightControlWeb.Data
{
    public interface IFcwRepo
    {
        IEnumerable<FlightPlan> GetAllFlightPlans();
        FlightPlan GetFlightPlanById(string id);
        Task<FlightPlan> GetFlightPlanByIdAsync(string id, HttpClient httpClient);
        Task<IEnumerable<Flight>> GetFlightsByTimeAsync(string date, bool isExternal,
                                                        HttpClient httpClient);
        Response DeleteFlightById(string id);
        Response PostFlightPlan(FlightPlan flightPlan);
    }
}