using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required, Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; }

        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Designation { get; set; }
        public string Department { get; set; }

        [EmailAddress, Display(Name = "Email")]
        public string EmailAddress { get; set; }

        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Date of Joining"), DataType(DataType.Date)]
        public DateTime? DateOfJoining { get; set; }

        [Display(Name = "Date of Birth"), DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Basic Salary (₹)"), DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal BasicSalary { get; set; }

        [Display(Name = "Employment Type")]
        public string EmploymentType { get; set; }

        public string Status { get; set; }
        public string Address { get; set; }

        [Display(Name = "Emergency Contact")]
        public string EmergencyContact { get; set; }

        [Display(Name = "Aadhaar No.")]
        public string AadhaarNumber { get; set; }

        [Display(Name = "PAN No.")]
        public string PANNumber { get; set; }

        [Display(Name = "Bank Account No.")]
        public string BankAccountNo { get; set; }

        [Display(Name = "IFSC Code")]
        public string IFSCCode { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
