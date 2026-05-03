using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Material
    {
        public int MaterialId { get; set; }

        [Required, Display(Name = "Material Code")]
        public string MaterialCode { get; set; }

        [Required, Display(Name = "Material Name")]
        public string MaterialName { get; set; }

        public string Category { get; set; }

        [Display(Name = "Unit of Measure")]
        public string Unit { get; set; }

        [Display(Name = "Unit Price (₹)"), DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Current Stock"), DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal CurrentStock { get; set; }

        [Display(Name = "Min. Stock Level"), DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal MinStockLevel { get; set; }

        public string Supplier { get; set; }

        [Display(Name = "HSN Code")]
        public string HSNCode { get; set; }

        [Display(Name = "GST Rate (%)")]
        public decimal GSTRate { get; set; }

        public string Description { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }

        public bool IsLowStock => CurrentStock <= MinStockLevel;
    }
}
