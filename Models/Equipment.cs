using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }

        [Required, Display(Name = "Equipment Code")]
        public string EquipmentCode { get; set; }

        [Required, Display(Name = "Equipment Name")]
        public string EquipmentName { get; set; }

        public string Category { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }

        [Display(Name = "Registration No.")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "Purchase Date"), DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "Purchase Value (₹)"), DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PurchaseValue { get; set; }

        [Display(Name = "Current Project")]
        public int? CurrentProjectId { get; set; }
        public string CurrentProjectName { get; set; }

        public string Status { get; set; }

        [Display(Name = "Last Maintenance"), DataType(DataType.Date)]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "Next Maintenance Due"), DataType(DataType.Date)]
        public DateTime? NextMaintenanceDue { get; set; }

        [Display(Name = "Rental Cost/Day (₹)"), DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RentalCostPerDay { get; set; }

        [Display(Name = "Ownership")]
        public bool IsOwned { get; set; }

        public string Remarks { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }

        public bool IsMaintenanceDue => NextMaintenanceDue.HasValue && NextMaintenanceDue.Value <= DateTime.Today.AddDays(7);
        public string OwnershipLabel => IsOwned ? "Owned" : "Rented";
    }
}
