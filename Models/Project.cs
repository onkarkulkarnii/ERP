using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Project
    {
        public int ProjectId { get; set; }

        [Required, Display(Name = "Project Code")]
        public string ProjectCode { get; set; }

        [Required, Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Client Name")]
        public string ClientName { get; set; }

        [Display(Name = "Project Type")]
        public string ProjectType { get; set; }

        [Display(Name = "Start Date"), DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date"), DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Budget (₹)"), DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Budget { get; set; }

        [Display(Name = "Contract Value (₹)"), DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ContractValue { get; set; }

        public string Location { get; set; }

        public string Status { get; set; }

        [Display(Name = "Progress (%)"), Range(0, 100)]
        public int ProgressPercent { get; set; }

        public string Description { get; set; }

        [Display(Name = "Project Manager")]
        public string ProjectManager { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }

        // Computed helpers
        public int DaysRemaining => EndDate.HasValue ? (int)(EndDate.Value - DateTime.Today).TotalDays : 0;
        public bool IsOverdue => EndDate.HasValue && EndDate.Value < DateTime.Today && Status != "Completed";
    }
}
