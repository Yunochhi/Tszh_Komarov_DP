namespace TSZH_Komarov.Viewmodels.Service
{
    public class ServiceRequestViewModel
    {
        public int ServiceRequestId { get; set; }
        public string UserFullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CurrentStatus { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string? Note { get; set; }
        public string? AdminComment { get; set; }
    }
}
