using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;

namespace ERP.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index(string status = "", string search = "")
        {
            var list = new List<Equipment>();
            var sql = @"SELECT e.*, p.ProjectName FROM Equipment e LEFT JOIN Projects p ON p.ProjectId=e.CurrentProjectId WHERE 1=1";
            if (!string.IsNullOrEmpty(status)) sql += " AND e.Status=@status";
            if (!string.IsNullOrEmpty(search)) sql += " AND (e.EquipmentName LIKE @search OR e.EquipmentCode LIKE @search)";
            sql += " ORDER BY e.Category, e.EquipmentName";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
                    if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapFull(r));
                }
            }
            ViewBag.Status = status;
            ViewBag.Search = search;
            return View(list);
        }

        public ActionResult Create()
        {
            LoadDropdowns();
            return View(new Equipment { Status = "Available", IsOwned = true, CreatedDate = DateTime.Now });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Equipment m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO Equipment(EquipmentCode,EquipmentName,Category,Model,Manufacturer,RegistrationNumber,PurchaseDate,PurchaseValue,Status,LastMaintenanceDate,NextMaintenanceDue,RentalCostPerDay,IsOwned,Remarks)
                            VALUES(@code,@name,@cat,@model,@maker,@reg,@pdate,@pval,@status,@lastmaint,@nextmaint,@rental,@owned,@remarks)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Equipment '{m.EquipmentName}' added.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var eq = GetById(id);
            if (eq == null) return HttpNotFound();
            LoadDropdowns();
            return View(eq);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Equipment m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"UPDATE Equipment SET EquipmentName=@name,Category=@cat,Model=@model,Manufacturer=@maker,RegistrationNumber=@reg,
                            PurchaseDate=@pdate,PurchaseValue=@pval,Status=@status,LastMaintenanceDate=@lastmaint,
                            NextMaintenanceDue=@nextmaint,RentalCostPerDay=@rental,IsOwned=@owned,Remarks=@remarks WHERE EquipmentId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.Parameters.AddWithValue("@id", m.EquipmentId);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Equipment '{m.EquipmentName}' updated.";
            return RedirectToAction("Index");
        }

        private void Bind(SqlCommand cmd, Equipment m)
        {
            cmd.Parameters.AddWithValue("@code",      m.EquipmentCode);
            cmd.Parameters.AddWithValue("@name",      m.EquipmentName);
            cmd.Parameters.AddWithValue("@cat",       (object)m.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@model",     (object)m.Model ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@maker",     (object)m.Manufacturer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@reg",       (object)m.RegistrationNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pdate",     (object)m.PurchaseDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pval",      m.PurchaseValue);
            cmd.Parameters.AddWithValue("@status",    m.Status ?? "Available");
            cmd.Parameters.AddWithValue("@lastmaint", (object)m.LastMaintenanceDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@nextmaint", (object)m.NextMaintenanceDue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rental",    m.RentalCostPerDay);
            cmd.Parameters.AddWithValue("@owned",     m.IsOwned ? 1 : 0);
            cmd.Parameters.AddWithValue("@remarks",   (object)m.Remarks ?? DBNull.Value);
        }

        private Equipment GetById(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = "SELECT e.*,p.ProjectName FROM Equipment e LEFT JOIN Projects p ON p.ProjectId=e.CurrentProjectId WHERE e.EquipmentId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapFull(r) : null;
                }
            }
        }

        private static Equipment MapFull(SqlDataReader r) => new Equipment
        {
            EquipmentId         = (int)r["EquipmentId"],
            EquipmentCode       = r["EquipmentCode"]?.ToString(),
            EquipmentName       = r["EquipmentName"]?.ToString(),
            Category            = r["Category"]?.ToString(),
            Model               = r["Model"]?.ToString(),
            Manufacturer        = r["Manufacturer"]?.ToString(),
            RegistrationNumber  = r["RegistrationNumber"]?.ToString(),
            PurchaseDate        = r["PurchaseDate"] is DBNull ? (DateTime?)null : (DateTime)r["PurchaseDate"],
            PurchaseValue       = r["PurchaseValue"] is DBNull ? 0 : (decimal)r["PurchaseValue"],
            CurrentProjectId    = r["CurrentProjectId"] is DBNull ? (int?)null : (int)r["CurrentProjectId"],
            CurrentProjectName  = r["ProjectName"]?.ToString(),
            Status              = r["Status"]?.ToString(),
            LastMaintenanceDate = r["LastMaintenanceDate"] is DBNull ? (DateTime?)null : (DateTime)r["LastMaintenanceDate"],
            NextMaintenanceDue  = r["NextMaintenanceDue"] is DBNull ? (DateTime?)null : (DateTime)r["NextMaintenanceDue"],
            RentalCostPerDay    = r["RentalCostPerDay"] is DBNull ? 0 : (decimal)r["RentalCostPerDay"],
            IsOwned             = r["IsOwned"] is DBNull ? true : (bool)r["IsOwned"],
            Remarks             = r["Remarks"]?.ToString(),
            CreatedDate         = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private void LoadDropdowns()
        {
            ViewBag.Categories = new[] { "Excavator", "Crane", "Mixer", "Generator", "Compactor", "Loader", "Transit Mixer", "Pump", "Survey", "Misc" };
            ViewBag.Statuses   = new[] { "Available", "InUse", "Maintenance", "Disposed" };
            ViewBag.Projects   = GetProjectList();
        }

        private List<SelectListItem> GetProjectList()
        {
            var items = new List<SelectListItem> { new SelectListItem { Text = "-- None --", Value = "" } };
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT ProjectId,ProjectCode+' - '+ProjectName AS Label FROM Projects WHERE Status='Active' ORDER BY ProjectName", con))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        items.Add(new SelectListItem { Text = r["Label"].ToString(), Value = r["ProjectId"].ToString() });
            }
            return items;
        }
    }
}
