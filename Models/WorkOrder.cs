using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class WorkOrder
    {
        public int WorkOrderId { get; set; }

        [Required, Display(Name = "Work Order Code")]
        public string WorkOrderCode { get; set; }

        [Required, Display(Name = "Project")]
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }

        [Display(Name = "Contractor")]
        public int? ContractorId { get; set; }
        public string ContractorName { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Display(Name = "Work Type")]
        public string WorkType { get; set; }

        [Display(Name = "Start Date"), DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date"), DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Contract Amount (₹)"), DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ContractAmount { get; set; }

        [Display(Name = "Paid Amount (₹)"), DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PaidAmount { get; set; }

        public string Status { get; set; }
        public string Priority { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }

        public decimal BalanceAmount => ContractAmount - PaidAmount;
        public bool IsOverdue => EndDate.HasValue && EndDate.Value < DateTime.Today && Status != "Completed" && Status != "Cancelled";
    }
}
