using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Contractor
    {
        public int ContractorId { get; set; }

        [Required, Display(Name = "Contractor Code")]
        public string ContractorCode { get; set; }

        [Required, Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        [EmailAddress, Display(Name = "Email")]
        public string EmailAddress { get; set; }

        public string Address { get; set; }

        [Display(Name = "Specialty / Trade")]
        public string SpecialtyType { get; set; }

        [Display(Name = "License No.")]
        public string LicenseNumber { get; set; }

        [Display(Name = "License Expiry"), DataType(DataType.Date)]
        public DateTime? LicenseExpiry { get; set; }

        [Display(Name = "GST No.")]
        public string GSTNumber { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Status { get; set; }
        public string Remarks { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }
    }
}
