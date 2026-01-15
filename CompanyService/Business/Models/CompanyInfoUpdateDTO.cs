using System.ComponentModel.DataAnnotations;

namespace CompanyService.Business.Models
{
    /// <summary>
    /// Data Transfer Object for updating company information
    /// Used as input for company update operations
    /// </summary>
    public class CompanyInfoUpdateDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "ShortName is required")]
        [StringLength(50, ErrorMessage = "ShortName cannot exceed 50 characters")]
        public string ShortName { get; set; }

        [Required(ErrorMessage = "TaxId is required")]
        [StringLength(50, ErrorMessage = "TaxId cannot exceed 50 characters")]
        public string TaxId { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "PostalCodeId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "PostalCodeId must be greater than 0")]
        public int PostalCodeId { get; set; }

        [Required(ErrorMessage = "IVATypeId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "IVATypeId must be greater than 0")]
        public int IVATypeId { get; set; }

        [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        public string? Email { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "LogoUrl cannot exceed 500 characters")]
        public string? LogoUrl { get; set; }

        [Required(ErrorMessage = "GrossIncome is required")]
        [StringLength(50, ErrorMessage = "GrossIncome cannot exceed 50 characters")]
        public string GrossIncome { get; set; }

        [Required(ErrorMessage = "DateOfIncorporation is required")]
        public DateTime DateOfIncorporation { get; set; }
    }
}
