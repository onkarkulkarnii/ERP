using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class SafetyIncident
    {
        public int IncidentId { get; set; }

        [Display(Name = "Incident Code")]
        public string IncidentCode { get; set; }

        [Display(Name = "Project")]
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }

        [Required, Display(Name = "Incident Date"), DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; }

        [Display(Name = "Incident Time")]
        public string IncidentTime { get; set; }

        [Required, Display(Name = "Incident Type")]
        public string IncidentType { get; set; }

        [Required]
        public string Description { get; set; }

        [Display(Name = "Injured Person")]
        public string InjuredPerson { get; set; }

        [Display(Name = "Injury Type")]
        public string InjuryType { get; set; }

        public string Location { get; set; }

        [Display(Name = "Root Cause")]
        public string RootCause { get; set; }

        [Display(Name = "Corrective Action")]
        public string CorrectiveAction { get; set; }

        [Display(Name = "Reported By")]
        public string ReportedBy { get; set; }

        public string Status { get; set; }

        [Display(Name = "Closed Date"), DataType(DataType.Date)]
        public DateTime? ClosedDate { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedDate { get; set; }
    }
}
