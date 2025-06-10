using System.ComponentModel.DataAnnotations;

namespace TSZH_Komarov.Viewmodels
{
    public class PreAnnouncementViewModel
    {
        public int PreAnnouncementId { get; set; }

        public int TszhId { get; set; }

        [Required(ErrorMessage = "Заголовок обязателен")]
        public string Topic { get; set; }

        [Required(ErrorMessage = "Описание обязательно")]
        public string Description { get; set; }
  
        public int Priority { get; set; }
    }
}
