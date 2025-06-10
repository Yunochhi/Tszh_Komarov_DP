using System.ComponentModel.DataAnnotations;

namespace TSZH_Komarov.Attributes
{
    public class MinCurrentValueAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public MinCurrentValueAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (double?)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                return ValidationResult.Success;

            var comparisonValue = (double)property.GetValue(validationContext.ObjectInstance);

            if (currentValue.HasValue && currentValue.Value < comparisonValue)
            {
                return new ValidationResult(
                    $"Значение {currentValue} не может быть меньше предыдущего ({comparisonValue})");
            }

            return ValidationResult.Success;
        }
    }
}
