using System.Collections.Generic;

namespace ERP.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int ActiveProjects { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveEmployees { get; set; }
        public int ActiveWorkOrders { get; set; }
        public int PendingWorkOrders { get; set; }
        public decimal OutstandingReceivables { get; set; }
        public decimal OutstandingPayables { get; set; }
        public int OpenIncidents { get; set; }
        public int EquipmentInMaintenance { get; set; }
        public int LowStockMaterials { get; set; }

        public List<Project> RecentProjects { get; set; } = new List<Project>();
        public List<WorkOrder> RecentWorkOrders { get; set; } = new List<WorkOrder>();
        public List<SafetyIncident> RecentIncidents { get; set; } = new List<SafetyIncident>();
    }
}
