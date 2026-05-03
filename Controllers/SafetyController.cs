using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;

namespace ERP.Controllers
{
    public class SafetyController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index(string status = "", string type = "", string search = "")
        {
            var list = new List<SafetyIncident>();
            var sql = @"SELECT si.*,p.ProjectName FROM SafetyIncidents si
                        LEFT JOIN Projects p ON p.ProjectId=si.ProjectId WHERE 1=1";
            if (!string.IsNullOrEmpty(status)) sql += " AND si.Status=@status";
            if (!string.IsNullOrEmpty(type)) sql += " AND si.IncidentType=@type";
            if (!string.IsNullOrEmpty(search)) sql += " AND (si.Description LIKE @search OR si.ReportedBy LIKE @search OR si.InjuredPerson LIKE @search)";
            sql += " ORDER BY si.IncidentDate DESC";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
                    if (!string.IsNullOrEmpty(type)) cmd.Parameters.AddWithValue("@type", type);
                    if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapFull(r));
                }
            }

            ViewBag.Status = status;
            ViewBag.Type = type;
            ViewBag.Search = search;
            ViewBag.TotalOpen   = GetCount("SELECT COUNT(*) FROM SafetyIncidents WHERE Status='Open'");
            ViewBag.TotalLTI    = GetCount("SELECT COUNT(*) FROM SafetyIncidents WHERE IncidentType='LostTime'");
            ViewBag.TotalNearMiss = GetCount("SELECT COUNT(*) FROM SafetyIncidents WHERE IncidentType='NearMiss'");
            return View(list);
        }

        public ActionResult Create()
        {
            LoadDropdowns();
            return View(new SafetyIncident { IncidentDate = DateTime.Today, Status = "Open" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(SafetyIncident m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO SafetyIncidents(IncidentCode,ProjectId,IncidentDate,IncidentTime,IncidentType,Description,InjuredPerson,InjuryType,Location,RootCause,CorrectiveAction,ReportedBy,Status)
                            VALUES(@code,@proj,@date,@time,@type,@desc,@injured,@injury,@loc,@cause,@action,@reported,@status)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Safety incident recorded.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var inc = GetById(id);
            if (inc == null) return HttpNotFound();
            LoadDropdowns();
            return View(inc);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(SafetyIncident m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"UPDATE SafetyIncidents SET ProjectId=@proj,IncidentDate=@date,IncidentTime=@time,IncidentType=@type,
                            Description=@desc,InjuredPerson=@injured,InjuryType=@injury,Location=@loc,RootCause=@cause,
                            CorrectiveAction=@action,ReportedBy=@reported,Status=@status,
                            ClosedDate=CASE WHEN @status='Closed' THEN GETDATE() ELSE ClosedDate END
                            WHERE IncidentId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.Parameters.AddWithValue("@id", m.IncidentId);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Incident updated.";
            return RedirectToAction("Index");
        }

        private void Bind(SqlCommand cmd, SafetyIncident m)
        {
            cmd.Parameters.AddWithValue("@code",     (object)m.IncidentCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@proj",     (object)m.ProjectId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@date",     m.IncidentDate);
            cmd.Parameters.AddWithValue("@time",     (object)m.IncidentTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@type",     m.IncidentType);
            cmd.Parameters.AddWithValue("@desc",     m.Description);
            cmd.Parameters.AddWithValue("@injured",  (object)m.InjuredPerson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@injury",   (object)m.InjuryType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@loc",      (object)m.Location ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cause",    (object)m.RootCause ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@action",   (object)m.CorrectiveAction ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@reported", (object)m.ReportedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status",   m.Status ?? "Open");
        }

        private SafetyIncident GetById(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = "SELECT si.*,p.ProjectName FROM SafetyIncidents si LEFT JOIN Projects p ON p.ProjectId=si.ProjectId WHERE si.IncidentId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapFull(r) : null;
                }
            }
        }

        private int GetCount(string sql)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                    return (int)cmd.ExecuteScalar();
            }
        }

        private static SafetyIncident MapFull(SqlDataReader r) => new SafetyIncident
        {
            IncidentId        = (int)r["IncidentId"],
            IncidentCode      = r["IncidentCode"]?.ToString(),
            ProjectId         = r["ProjectId"] is DBNull ? (int?)null : (int)r["ProjectId"],
            ProjectName       = r["ProjectName"]?.ToString(),
            IncidentDate      = r["IncidentDate"] is DBNull ? DateTime.Today : (DateTime)r["IncidentDate"],
            IncidentTime      = r["IncidentTime"]?.ToString(),
            IncidentType      = r["IncidentType"]?.ToString(),
            Description       = r["Description"]?.ToString(),
            InjuredPerson     = r["InjuredPerson"]?.ToString(),
            InjuryType        = r["InjuryType"]?.ToString(),
            Location          = r["Location"]?.ToString(),
            RootCause         = r["RootCause"]?.ToString(),
            CorrectiveAction  = r["CorrectiveAction"]?.ToString(),
            ReportedBy        = r["ReportedBy"]?.ToString(),
            Status            = r["Status"]?.ToString(),
            ClosedDate        = r["ClosedDate"] is DBNull ? (DateTime?)null : (DateTime)r["ClosedDate"],
            CreatedDate       = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private void LoadDropdowns()
        {
            ViewBag.IncidentTypes = new[] { "NearMiss", "FirstAid", "LostTime", "Fatality", "PropertyDamage", "FireAlarm", "EnvironmentalSpill" };
            ViewBag.Statuses      = new[] { "Open", "UnderInvestigation", "Closed" };
            ViewBag.Projects      = GetProjectDropdown();
        }

        private List<SelectListItem> GetProjectDropdown()
        {
            var items = new List<SelectListItem> { new SelectListItem { Text = "-- Select Project --", Value = "" } };
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
