using System.ComponentModel.DataAnnotations;
using TSZH_Komarov.Models;

namespace TSZH_Komarov.Viewmodels.User
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Укажите ФИО")]
        public string? Fullname { get; set; }

        [Required(ErrorMessage = "Укажите Email")]
        [RegularExpression(pattern: ".*@.*", ErrorMessage = "Неправильный формат Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Укажите Номер телефона")]
        [StringLength(11, ErrorMessage = "Номер должен состоять из 11 цифр")]
        [RegularExpression(@"^7\d{10}$", ErrorMessage = "Номер должен начинаться с \"7\" ")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Пароль должен начинаться от 8 символов")]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Повторите пароль")]
        public string? RepeatPassword { get; set; }

        [Required(ErrorMessage = "Выберите дом")]
        [Display(Name = "Дом")]
        public int SelectedHouseId { get; set; }

        [Required(ErrorMessage = "Выберите квартиру")]
        [Display(Name = "Квартира")]
        public int SelectedApartmentId { get; set; }

        public int CurrentTszhId { get; set; }
        public bool IsHouseSelected { get; set; }

        public List<HousesItem> Houses { get; set; } = new List<HousesItem>();
        public List<ApartmentsItem> Apartments { get; set; } = new List<ApartmentsItem>();
    }
    public class HousesItem
    {
        public int HouseId { get; set; }
        public string Address { get; set; }
    }

    public class ApartmentsItem
    {
        public int ApartmentId { get; set; }
        public string Number { get; set; }
    }
}
