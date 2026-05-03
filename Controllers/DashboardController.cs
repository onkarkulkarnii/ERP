using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;
using ERP.Models.ViewModels;

namespace ERP.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index()
        {
            var vm = new DashboardViewModel();
            using (var con = new SqlConnection(_conn))
            {
                con.Open();

                // Stats
                using (var cmd = new SqlCommand("EXEC sp_GetDashboardStats", con))
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        vm.ActiveProjects          = r["ActiveProjects"]          is DBNull ? 0 : (int)r["ActiveProjects"];
                        vm.TotalProjects           = r["TotalProjects"]           is DBNull ? 0 : (int)r["TotalProjects"];
                        vm.ActiveEmployees         = r["ActiveEmployees"]         is DBNull ? 0 : (int)r["ActiveEmployees"];
                        vm.ActiveWorkOrders        = r["ActiveWorkOrders"]        is DBNull ? 0 : (int)r["ActiveWorkOrders"];
                        vm.PendingWorkOrders       = r["PendingWorkOrders"]       is DBNull ? 0 : (int)r["PendingWorkOrders"];
                        vm.OutstandingReceivables  = r["OutstandingReceivables"]  is DBNull ? 0 : (decimal)r["OutstandingReceivables"];
                        vm.OutstandingPayables     = r["OutstandingPayables"]     is DBNull ? 0 : (decimal)r["OutstandingPayables"];
                        vm.OpenIncidents           = r["OpenIncidents"]           is DBNull ? 0 : (int)r["OpenIncidents"];
                        vm.EquipmentInMaintenance  = r["EquipmentInMaintenance"]  is DBNull ? 0 : (int)r["EquipmentInMaintenance"];
                        vm.LowStockMaterials       = r["LowStockMaterials"]       is DBNull ? 0 : (int)r["LowStockMaterials"];
                    }
                }

                // Recent active projects
                using (var cmd = new SqlCommand(
                    "SELECT TOP 5 ProjectId,ProjectCode,ProjectName,ClientName,Status,ProgressPercent,EndDate,Location FROM Projects WHERE Status IN ('Active','OnHold') ORDER BY CreatedDate DESC", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        vm.RecentProjects.Add(MapProject(r));
                }

                // Recent work orders
                using (var cmd = new SqlCommand(
                    "SELECT TOP 6 wo.WorkOrderId,wo.WorkOrderCode,wo.Title,wo.Status,wo.Priority,wo.EndDate,p.ProjectName,c.CompanyName AS ContractorName " +
                    "FROM WorkOrders wo JOIN Projects p ON p.ProjectId=wo.ProjectId LEFT JOIN Contractors c ON c.ContractorId=wo.ContractorId " +
                    "WHERE wo.Status IN ('Pending','InProgress') ORDER BY wo.CreatedDate DESC", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        vm.RecentWorkOrders.Add(MapWorkOrder(r));
                }

                // Recent safety incidents
                using (var cmd = new SqlCommand(
                    "SELECT TOP 5 si.IncidentId,si.IncidentCode,si.IncidentDate,si.IncidentType,si.Description,si.Status,p.ProjectName " +
                    "FROM SafetyIncidents si LEFT JOIN Projects p ON p.ProjectId=si.ProjectId ORDER BY si.IncidentDate DESC", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        vm.RecentIncidents.Add(MapIncident(r));
                }
            }
            return View(vm);
        }

        private static Project MapProject(SqlDataReader r) => new Project
        {
            ProjectId       = (int)r["ProjectId"],
            ProjectCode     = r["ProjectCode"]?.ToString(),
            ProjectName     = r["ProjectName"]?.ToString(),
            ClientName      = r["ClientName"]?.ToString(),
            Status          = r["Status"]?.ToString(),
            ProgressPercent = r["ProgressPercent"] is DBNull ? 0 : (int)r["ProgressPercent"],
            EndDate         = r["EndDate"] is DBNull ? (DateTime?)null : (DateTime)r["EndDate"],
            Location        = r["Location"]?.ToString()
        };

        private static WorkOrder MapWorkOrder(SqlDataReader r) => new WorkOrder
        {
            WorkOrderId    = (int)r["WorkOrderId"],
            WorkOrderCode  = r["WorkOrderCode"]?.ToString(),
            Title          = r["Title"]?.ToString(),
            Status         = r["Status"]?.ToString(),
            Priority       = r["Priority"]?.ToString(),
            EndDate        = r["EndDate"] is DBNull ? (DateTime?)null : (DateTime)r["EndDate"],
            ProjectName    = r["ProjectName"]?.ToString(),
            ContractorName = r["ContractorName"]?.ToString()
        };

        private static SafetyIncident MapIncident(SqlDataReader r) => new SafetyIncident
        {
            IncidentId   = (int)r["IncidentId"],
            IncidentCode = r["IncidentCode"]?.ToString(),
            IncidentDate = r["IncidentDate"] is DBNull ? DateTime.Today : (DateTime)r["IncidentDate"],
            IncidentType = r["IncidentType"]?.ToString(),
            Description  = r["Description"]?.ToString(),
            Status       = r["Status"]?.ToString(),
            ProjectName  = r["ProjectName"]?.ToString()
        };
    }
}
