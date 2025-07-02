namespace TSZH_Komarov.Viewmodels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class PollCreateViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов")]
        [DisplayName("Заголовок голосования")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("Тип голосования")]
        public int VoteTypeId { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("Дата окончания")]
        [FutureDate(ErrorMessage = "Дата должна быть в будущем")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7).AddMilliseconds(-DateTime.Now.Millisecond).AddSeconds(-DateTime.Now.Second);

        [MinLength(3, ErrorMessage = "Добавьте минимум 3 варианта ответа")]
        [DisplayName("Варианты ответов")]
        public List<string> Options { get; set; } = new List<string> { "", "", ""};

        public SelectList? VoteTypes { get; set; }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is DateTime date && date > DateTime.Now;
        }
    }
}
