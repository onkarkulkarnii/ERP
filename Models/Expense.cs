using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }

        [Display(Name = "Project")]
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }

        [Required, Display(Name = "Expense Date"), DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public string Category { get; set; }

        public string Description { get; set; }

        [Required, DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Amount { get; set; }

        [Display(Name = "Voucher No.")]
        public string VoucherNumber { get; set; }

        [Display(Name = "Payment Mode")]
        public string PaymentMode { get; set; }

        [Display(Name = "Approved By")]
        public string ApprovedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }
    }
}
