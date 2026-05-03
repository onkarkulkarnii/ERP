using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;

namespace ERP.Controllers
{
    public class ProjectController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index(string status = "", string search = "")
        {
            var list = new List<Project>();
            var sql = "SELECT * FROM Projects WHERE 1=1";
            if (!string.IsNullOrEmpty(status)) sql += $" AND Status=@status";
            if (!string.IsNullOrEmpty(search)) sql += " AND (ProjectName LIKE @search OR ProjectCode LIKE @search OR ClientName LIKE @search)";
            sql += " ORDER BY CreatedDate DESC";

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

        public ActionResult Details(int id)
        {
            var p = GetById(id);
            if (p == null) return HttpNotFound();
            return View(p);
        }

        public ActionResult Create()
        {
            LoadDropdowns();
            return View(new Project { Status = "Planning", CreatedDate = DateTime.Now });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Project m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO Projects(ProjectCode,ProjectName,ClientName,ProjectType,StartDate,EndDate,Budget,ContractValue,Location,Status,ProgressPercent,Description,ProjectManager)
                            VALUES(@code,@name,@client,@type,@start,@end,@budget,@contract,@loc,@status,@progress,@desc,@pm)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@code",     m.ProjectCode);
                    cmd.Parameters.AddWithValue("@name",     m.ProjectName);
                    cmd.Parameters.AddWithValue("@client",   (object)m.ClientName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@type",     (object)m.ProjectType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@start",    (object)m.StartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@end",      (object)m.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@budget",   m.Budget);
                    cmd.Parameters.AddWithValue("@contract", m.ContractValue);
                    cmd.Parameters.AddWithValue("@loc",      (object)m.Location ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status",   m.Status ?? "Planning");
                    cmd.Parameters.AddWithValue("@progress", m.ProgressPercent);
                    cmd.Parameters.AddWithValue("@desc",     (object)m.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pm",       (object)m.ProjectManager ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Project '{m.ProjectName}' created successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var p = GetById(id);
            if (p == null) return HttpNotFound();
            LoadDropdowns();
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Project m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"UPDATE Projects SET ProjectName=@name,ClientName=@client,ProjectType=@type,StartDate=@start,EndDate=@end,
                            Budget=@budget,ContractValue=@contract,Location=@loc,Status=@status,ProgressPercent=@progress,
                            Description=@desc,ProjectManager=@pm WHERE ProjectId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@name",     m.ProjectName);
                    cmd.Parameters.AddWithValue("@client",   (object)m.ClientName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@type",     (object)m.ProjectType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@start",    (object)m.StartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@end",      (object)m.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@budget",   m.Budget);
                    cmd.Parameters.AddWithValue("@contract", m.ContractValue);
                    cmd.Parameters.AddWithValue("@loc",      (object)m.Location ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status",   m.Status);
                    cmd.Parameters.AddWithValue("@progress", m.ProgressPercent);
                    cmd.Parameters.AddWithValue("@desc",     (object)m.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pm",       (object)m.ProjectManager ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id",       m.ProjectId);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Project '{m.ProjectName}' updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("DELETE FROM Projects WHERE ProjectId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Project deleted.";
            return RedirectToAction("Index");
        }

        private Project GetById(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT * FROM Projects WHERE ProjectId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapFull(r) : null;
                }
            }
        }

        private static Project MapFull(SqlDataReader r) => new Project
        {
            ProjectId       = (int)r["ProjectId"],
            ProjectCode     = r["ProjectCode"]?.ToString(),
            ProjectName     = r["ProjectName"]?.ToString(),
            ClientName      = r["ClientName"]?.ToString(),
            ProjectType     = r["ProjectType"]?.ToString(),
            StartDate       = r["StartDate"] is DBNull ? (DateTime?)null : (DateTime)r["StartDate"],
            EndDate         = r["EndDate"] is DBNull ? (DateTime?)null : (DateTime)r["EndDate"],
            Budget          = r["Budget"] is DBNull ? 0 : (decimal)r["Budget"],
            ContractValue   = r["ContractValue"] is DBNull ? 0 : (decimal)r["ContractValue"],
            Location        = r["Location"]?.ToString(),
            Status          = r["Status"]?.ToString(),
            ProgressPercent = r["ProgressPercent"] is DBNull ? 0 : (int)r["ProgressPercent"],
            Description     = r["Description"]?.ToString(),
            ProjectManager  = r["ProjectManager"]?.ToString(),
            CreatedDate     = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private void LoadDropdowns()
        {
            ViewBag.ProjectTypes = new[] { "Residential", "Commercial", "Infrastructure", "Industrial", "Renovation", "Other" };
            ViewBag.Statuses     = new[] { "Planning", "Active", "OnHold", "Completed", "Cancelled" };
        }
    }
}
