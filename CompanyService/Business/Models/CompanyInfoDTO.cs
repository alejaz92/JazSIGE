namespace CompanyService.Business.Models
{
    /// <summary>
    /// Data Transfer Object for company information
    /// Used to return company data to API consumers
    /// </summary>
    public class CompanyInfoDTO
    {
        /// <summary>
        /// Full company name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Short name or abbreviation
        /// </summary>
        public string ShortName { get; set; }
        
        /// <summary>
        /// Tax identification number (CUIT/CUIL)
        /// </summary>
        public string TaxId { get; set; }
        
        /// <summary>
        /// Street address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Postal code ID reference
        /// </summary>
        public int PostalCodeId { get; set; }
        
        /// <summary>
        /// Postal code description
        /// </summary>
        public string PostalCode { get; set; }
        
        /// <summary>
        /// City name
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// Province/State name
        /// </summary>
        public string Province { get; set; }
        
        /// <summary>
        /// Country name
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Contact phone number (optional)
        /// </summary>
        public string? Phone { get; set; }
        
        /// <summary>
        /// Contact email address (optional)
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// URL to company logo image (optional)
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// IVA (VAT) type ID reference
        /// </summary>
        public int IVATypeId { get; set; }
        
        /// <summary>
        /// IVA type description
        /// </summary>
        public string IVAType { get; set; }

        /// <summary>
        /// Gross income registration number
        /// </summary>
        public string GrossIncome { get; set; }
        
        /// <summary>
        /// Date when the company was incorporated
        /// </summary>
        public DateTime DateOfIncorporation { get; set; }
    }
}
