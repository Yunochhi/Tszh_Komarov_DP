using System.ComponentModel.DataAnnotations;
using TSZH_Komarov.Models;

namespace TSZH_Komarov.Viewmodels.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Укажите Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

    }
}
