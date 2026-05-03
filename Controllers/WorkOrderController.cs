using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;

namespace ERP.Controllers
{
    public class WorkOrderController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index(int? projectId = null, string status = "", string search = "")
        {
            var list = new List<WorkOrder>();
            var sql = @"SELECT wo.*,p.ProjectName,p.ProjectCode,c.CompanyName AS ContractorName
                        FROM WorkOrders wo JOIN Projects p ON p.ProjectId=wo.ProjectId
                        LEFT JOIN Contractors c ON c.ContractorId=wo.ContractorId WHERE 1=1";
            if (projectId.HasValue) sql += " AND wo.ProjectId=@projectId";
            if (!string.IsNullOrEmpty(status)) sql += " AND wo.Status=@status";
            if (!string.IsNullOrEmpty(search)) sql += " AND (wo.Title LIKE @search OR wo.WorkOrderCode LIKE @search)";
            sql += " ORDER BY wo.CreatedDate DESC";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (projectId.HasValue) cmd.Parameters.AddWithValue("@projectId", projectId.Value);
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
                    if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapFull(r));
                }
            }
            ViewBag.ProjectId = projectId;
            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.Projects = GetProjectDropdown();
            return View(list);
        }

        public ActionResult Create()
        {
            LoadDropdowns();
            return View(new WorkOrder { Status = "Pending", Priority = "Medium", CreatedDate = DateTime.Now });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(WorkOrder m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO WorkOrders(WorkOrderCode,ProjectId,ContractorId,Title,Description,WorkType,StartDate,EndDate,ContractAmount,Status,Priority,AssignedTo)
                            VALUES(@code,@proj,@cont,@title,@desc,@type,@start,@end,@amount,@status,@priority,@assigned)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Work Order '{m.Title}' created.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var wo = GetById(id);
            if (wo == null) return HttpNotFound();
            LoadDropdowns();
            return View(wo);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(WorkOrder m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"UPDATE WorkOrders SET ProjectId=@proj,ContractorId=@cont,Title=@title,Description=@desc,WorkType=@type,
                            StartDate=@start,EndDate=@end,ContractAmount=@amount,Status=@status,Priority=@priority,AssignedTo=@assigned
                            WHERE WorkOrderId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.Parameters.AddWithValue("@id", m.WorkOrderId);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Work Order '{m.Title}' updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("UPDATE WorkOrders SET Status='Cancelled' WHERE WorkOrderId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Work Order cancelled.";
            return RedirectToAction("Index");
        }

        private void Bind(SqlCommand cmd, WorkOrder m)
        {
            cmd.Parameters.AddWithValue("@code",     m.WorkOrderCode);
            cmd.Parameters.AddWithValue("@proj",     m.ProjectId);
            cmd.Parameters.AddWithValue("@cont",     (object)m.ContractorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@title",    m.Title);
            cmd.Parameters.AddWithValue("@desc",     (object)m.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@type",     (object)m.WorkType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@start",    (object)m.StartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@end",      (object)m.EndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@amount",   m.ContractAmount);
            cmd.Parameters.AddWithValue("@status",   m.Status ?? "Pending");
            cmd.Parameters.AddWithValue("@priority", m.Priority ?? "Medium");
            cmd.Parameters.AddWithValue("@assigned", (object)m.AssignedTo ?? DBNull.Value);
        }

        private WorkOrder GetById(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"SELECT wo.*,p.ProjectName,p.ProjectCode,c.CompanyName AS ContractorName
                            FROM WorkOrders wo JOIN Projects p ON p.ProjectId=wo.ProjectId
                            LEFT JOIN Contractors c ON c.ContractorId=wo.ContractorId WHERE wo.WorkOrderId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapFull(r) : null;
                }
            }
        }

        private static WorkOrder MapFull(SqlDataReader r) => new WorkOrder
        {
            WorkOrderId    = (int)r["WorkOrderId"],
            WorkOrderCode  = r["WorkOrderCode"]?.ToString(),
            ProjectId      = (int)r["ProjectId"],
            ProjectName    = r["ProjectName"]?.ToString(),
            ProjectCode    = r["ProjectCode"]?.ToString(),
            ContractorId   = r["ContractorId"] is DBNull ? (int?)null : (int)r["ContractorId"],
            ContractorName = r["ContractorName"]?.ToString(),
            Title          = r["Title"]?.ToString(),
            Description    = r["Description"]?.ToString(),
            WorkType       = r["WorkType"]?.ToString(),
            StartDate      = r["StartDate"] is DBNull ? (DateTime?)null : (DateTime)r["StartDate"],
            EndDate        = r["EndDate"] is DBNull ? (DateTime?)null : (DateTime)r["EndDate"],
            ContractAmount = r["ContractAmount"] is DBNull ? 0 : (decimal)r["ContractAmount"],
            PaidAmount     = r["PaidAmount"] is DBNull ? 0 : (decimal)r["PaidAmount"],
            Status         = r["Status"]?.ToString(),
            Priority       = r["Priority"]?.ToString(),
            AssignedTo     = r["AssignedTo"]?.ToString(),
            CreatedDate    = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private void LoadDropdowns()
        {
            ViewBag.Projects     = GetProjectDropdown();
            ViewBag.Contractors  = GetContractorDropdown();
            ViewBag.WorkTypes    = new[] { "Excavation", "Foundation", "Structure", "Masonry", "Plastering", "Flooring", "Painting", "Electrical", "Plumbing", "HVAC", "MEP", "Finishing", "Landscaping", "Other" };
            ViewBag.Statuses     = new[] { "Pending", "InProgress", "Completed", "Cancelled" };
            ViewBag.Priorities   = new[] { "High", "Medium", "Low" };
        }

        private List<SelectListItem> GetProjectDropdown()
        {
            var items = new List<SelectListItem>();
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT ProjectId,ProjectCode+' - '+ProjectName AS Label FROM Projects WHERE Status != 'Cancelled' ORDER BY ProjectName", con))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        items.Add(new SelectListItem { Text = r["Label"].ToString(), Value = r["ProjectId"].ToString() });
            }
            return items;
        }

        private List<SelectListItem> GetContractorDropdown()
        {
            var items = new List<SelectListItem> { new SelectListItem { Text = "-- None / In-house --", Value = "" } };
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT ContractorId,CompanyName FROM Contractors WHERE Status='Active' ORDER BY CompanyName", con))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        items.Add(new SelectListItem { Text = r["CompanyName"].ToString(), Value = r["ContractorId"].ToString() });
            }
            return items;
        }
    }
}
