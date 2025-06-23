using TSZH_Komarov.Models;

namespace TSZH_Komarov.Viewmodels.Service
{
    public class ServiceViewModel
    {
        public string CurrentFilter { get; set; }
        public List<ServiceType> Services { get; set; }
        public List<ServiceRequest> Requests { get; set; }

        public List<ServiceRequest> ActiveRequests { get; set; }
    }
}
