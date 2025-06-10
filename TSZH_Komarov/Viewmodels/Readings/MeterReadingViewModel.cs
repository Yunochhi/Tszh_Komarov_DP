using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using TSZH_Komarov.Attributes;

namespace TSZH_Komarov.Viewmodels.Readings
{
    public class MeterReadingViewModel
    {
        public int ApartmentId { get; set; }
        public int MeterTypeId { get; set; }
        public string MeterName { get; set; }
        public double PreviousValue { get; set; }

        [Required(ErrorMessage = "Укажите текущие показания!")]
        [MinCurrentValue(nameof(PreviousValue))]
        [Range(0, double.MaxValue, ErrorMessage = "Некорректное значение")]
        public double? CurrentValue { get; set; }
        public string Unit { get; set; }
    }
    public class ApartmentMetersViewModel
    {
        public int ApartmentId { get; set; }
        public string AccountNumber { get; set; }
        public List<MeterReadingViewModel> Meters { get; set; } = new();
    }
}
