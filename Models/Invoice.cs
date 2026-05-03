using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        [Required, Display(Name = "Invoice No.")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Project")]
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }

        [Required, Display(Name = "Invoice Type")]
        public string InvoiceType { get; set; }

        [Required, Display(Name = "Party Name")]
        public string PartyName { get; set; }

        [Required, Display(Name = "Invoice Date"), DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; }

        [Display(Name = "Due Date"), DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Amount (₹)"), DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Amount { get; set; }

        [Display(Name = "Tax Amount (₹)"), DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TaxAmount { get; set; }

        [Display(Name = "Total Amount (₹)"), DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Paid Amount (₹)"), DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal PaidAmount { get; set; }

        public string Status { get; set; }
        public string Description { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }

        public decimal BalanceAmount => TotalAmount - PaidAmount;
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Today && Status != "Paid" && Status != "Cancelled";
    }
}
