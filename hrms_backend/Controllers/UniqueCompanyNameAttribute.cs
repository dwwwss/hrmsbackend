using hrms_backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class UniqueCompanyNameAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var dbContext = (HrmsDbContext)validationContext.GetService(typeof(HrmsDbContext));

        if (value != null)
        {
            string companyName = value.ToString();
            bool isDuplicate = dbContext.Companies.Any(c => c.CompanyName == companyName);

            if (isDuplicate)
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}
