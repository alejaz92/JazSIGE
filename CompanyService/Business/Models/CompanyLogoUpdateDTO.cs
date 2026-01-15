using System.ComponentModel.DataAnnotations;

namespace CompanyService.Business.Models
{
    /// <summary>
    /// Data Transfer Object for updating company logo URL
    /// Used as input for logo URL update operations
    /// </summary>
    public class CompanyLogoUpdateDTO
    {
        [Required(ErrorMessage = "LogoUrl is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "LogoUrl cannot exceed 500 characters")]
        public string LogoUrl { get; set; }
    }
}
