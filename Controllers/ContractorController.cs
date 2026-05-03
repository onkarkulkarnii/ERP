using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;

namespace ERP.Controllers
{
    public class ContractorController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index(string specialty = "", string search = "")
        {
            var list = new List<Contractor>();
            var sql = "SELECT * FROM Contractors WHERE 1=1";
            if (!string.IsNullOrEmpty(specialty)) sql += " AND SpecialtyType=@specialty";
            if (!string.IsNullOrEmpty(search)) sql += " AND (CompanyName LIKE @search OR ContractorCode LIKE @search OR ContactPerson LIKE @search)";
            sql += " ORDER BY CompanyName";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (!string.IsNullOrEmpty(specialty)) cmd.Parameters.AddWithValue("@specialty", specialty);
                    if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapFull(r));
                }
            }
            ViewBag.Specialty = specialty;
            ViewBag.Search = search;
            ViewBag.Specialties = GetSpecialties();
            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.Specialties = GetSpecialties();
            return View(new Contractor { Status = "Active", Rating = 3, CreatedDate = DateTime.Now });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Contractor m)
        {
            if (!ModelState.IsValid) { ViewBag.Specialties = GetSpecialties(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO Contractors(ContractorCode,CompanyName,ContactPerson,PhoneNumber,EmailAddress,Address,SpecialtyType,LicenseNumber,LicenseExpiry,GSTNumber,Rating,Status,Remarks)
                            VALUES(@code,@name,@contact,@phone,@email,@addr,@spec,@lic,@licexp,@gst,@rating,@status,@remarks)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Contractor '{m.CompanyName}' added successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var c = GetById(id);
            if (c == null) return HttpNotFound();
            ViewBag.Specialties = GetSpecialties();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Contractor m)
        {
            if (!ModelState.IsValid) { ViewBag.Specialties = GetSpecialties(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"UPDATE Contractors SET CompanyName=@name,ContactPerson=@contact,PhoneNumber=@phone,EmailAddress=@email,
                            Address=@addr,SpecialtyType=@spec,LicenseNumber=@lic,LicenseExpiry=@licexp,GSTNumber=@gst,
                            Rating=@rating,Status=@status,Remarks=@remarks WHERE ContractorId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.Parameters.AddWithValue("@id", m.ContractorId);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Contractor '{m.CompanyName}' updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("UPDATE Contractors SET Status='Inactive' WHERE ContractorId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Contractor deactivated.";
            return RedirectToAction("Index");
        }

        private void Bind(SqlCommand cmd, Contractor m)
        {
            cmd.Parameters.AddWithValue("@code",    m.ContractorCode);
            cmd.Parameters.AddWithValue("@name",    m.CompanyName);
            cmd.Parameters.AddWithValue("@contact", (object)m.ContactPerson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone",   (object)m.PhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email",   (object)m.EmailAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@addr",    (object)m.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@spec",    (object)m.SpecialtyType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lic",     (object)m.LicenseNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@licexp",  (object)m.LicenseExpiry ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@gst",     (object)m.GSTNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rating",  m.Rating);
            cmd.Parameters.AddWithValue("@status",  m.Status ?? "Active");
            cmd.Parameters.AddWithValue("@remarks", (object)m.Remarks ?? DBNull.Value);
        }

        private Contractor GetById(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT * FROM Contractors WHERE ContractorId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapFull(r) : null;
                }
            }
        }

        private static Contractor MapFull(SqlDataReader r) => new Contractor
        {
            ContractorId   = (int)r["ContractorId"],
            ContractorCode = r["ContractorCode"]?.ToString(),
            CompanyName    = r["CompanyName"]?.ToString(),
            ContactPerson  = r["ContactPerson"]?.ToString(),
            PhoneNumber    = r["PhoneNumber"]?.ToString(),
            EmailAddress   = r["EmailAddress"]?.ToString(),
            Address        = r["Address"]?.ToString(),
            SpecialtyType  = r["SpecialtyType"]?.ToString(),
            LicenseNumber  = r["LicenseNumber"]?.ToString(),
            LicenseExpiry  = r["LicenseExpiry"] is DBNull ? (DateTime?)null : (DateTime)r["LicenseExpiry"],
            GSTNumber      = r["GSTNumber"]?.ToString(),
            Rating         = r["Rating"] is DBNull ? 3 : (int)r["Rating"],
            Status         = r["Status"]?.ToString(),
            Remarks        = r["Remarks"]?.ToString(),
            CreatedDate    = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private List<string> GetSpecialties() =>
            new List<string> { "Civil", "Electrical", "Plumbing", "Steel", "Masonry", "Painting", "Flooring", "Excavation", "Carpentry", "Glazing", "HVAC", "Fire Safety" };
    }
}
