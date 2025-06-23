using TSZH_Komarov.Models;

namespace TSZH_Komarov.Viewmodels.Service
{
    public class ServiceRequestsGroupViewModel
    {
        public int ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }
        public List<ServiceRequestViewModel> Requests { get; set; }
    }

}
