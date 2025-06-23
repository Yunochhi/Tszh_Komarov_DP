using System.ComponentModel.DataAnnotations;

namespace TSZH_Komarov.Viewmodels.User
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Введите FullName")]
        public string FullName { get; set; }

        [RegularExpression(pattern: ".*@.*", ErrorMessage = "Неправильный формат Email")]
        [Required(ErrorMessage ="Введите Email")]
        public string Email { get; set; }

        [StringLength(11, ErrorMessage = "Номер должен состоять из 11 цифр")]
        [RegularExpression(@"^7\d{10}$", ErrorMessage = "Номер должен начинаться с \"7\" ")]
        [Required(ErrorMessage = "Введите PhoneNumber")]
        public string PhoneNumber { get; set; }

        public string PersonalAccount { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        public string OldPasswordFixed { get; set; }

        public string Salt { get; set; }

        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Пароль должен начинаться от 8 символов")]
        [Required(ErrorMessage = "Введите новый пароль")]
        public string NewPassword { get; set; }

        [Required]
        [Display(Name = "Период напоминания")]
        public int ReminderDaysBefore { get; set; }

        public DateTime? LastReminderSent { get; set; }

        public string? ChatId { get; set; }

        public int IsFirtsLogin { get; set; }
    }
}
