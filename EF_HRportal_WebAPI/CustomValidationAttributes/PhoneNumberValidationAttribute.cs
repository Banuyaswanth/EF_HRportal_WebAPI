﻿using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EF_HRportal_WebAPI.CustomValidationAttributes
{
    public class PhoneNumberValidationAttribute : ValidationAttribute
    {
        public string pattern = @"^[6789]\d{9}$";
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value == null)
            {
                return new ValidationResult("Provide a valid Phone number");
            }
            if (Regex.IsMatch(value.ToString(), pattern))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Provide a valid Phone number");
        }
    }
}
