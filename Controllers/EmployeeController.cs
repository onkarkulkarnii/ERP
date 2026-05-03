using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;

namespace ERP.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index(string status = "", string department = "", string search = "")
        {
            var list = new List<Employee>();
            var sql = "SELECT * FROM Employees WHERE 1=1";
            if (!string.IsNullOrEmpty(status)) sql += " AND Status=@status";
            if (!string.IsNullOrEmpty(department)) sql += " AND Department=@dept";
            if (!string.IsNullOrEmpty(search)) sql += " AND (FirstName+' '+LastName LIKE @search OR EmployeeCode LIKE @search OR Designation LIKE @search)";
            sql += " ORDER BY FirstName,LastName";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@dept", department);
                    if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapFull(r));
                }
            }
            ViewBag.Status = status;
            ViewBag.Department = department;
            ViewBag.Search = search;
            ViewBag.Departments = GetDepartments();
            return View(list);
        }

        public ActionResult Details(int id)
        {
            var e = GetById(id);
            if (e == null) return HttpNotFound();
            return View(e);
        }

        public ActionResult Create()
        {
            LoadDropdowns();
            return View(new Employee { Status = "Active", CreatedDate = DateTime.Now });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Employee m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO Employees(EmployeeCode,FirstName,LastName,Designation,Department,EmailAddress,PhoneNumber,
                            DateOfJoining,DateOfBirth,BasicSalary,EmploymentType,Status,Address,EmergencyContact,AadhaarNumber,PANNumber,BankAccountNo,IFSCCode)
                            VALUES(@code,@fn,@ln,@desig,@dept,@email,@phone,@doj,@dob,@salary,@emptype,@status,@addr,@emergency,@aadhaar,@pan,@bank,@ifsc)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Employee '{m.FullName}' added successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var e = GetById(id);
            if (e == null) return HttpNotFound();
            LoadDropdowns();
            return View(e);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Employee m)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"UPDATE Employees SET FirstName=@fn,LastName=@ln,Designation=@desig,Department=@dept,EmailAddress=@email,
                            PhoneNumber=@phone,DateOfJoining=@doj,DateOfBirth=@dob,BasicSalary=@salary,EmploymentType=@emptype,
                            Status=@status,Address=@addr,EmergencyContact=@emergency,AadhaarNumber=@aadhaar,PANNumber=@pan,
                            BankAccountNo=@bank,IFSCCode=@ifsc WHERE EmployeeId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.Parameters.AddWithValue("@id", m.EmployeeId);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Employee '{m.FullName}' updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("UPDATE Employees SET Status='Inactive' WHERE EmployeeId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Employee deactivated.";
            return RedirectToAction("Index");
        }

        private void Bind(SqlCommand cmd, Employee m)
        {
            cmd.Parameters.AddWithValue("@code",      m.EmployeeCode);
            cmd.Parameters.AddWithValue("@fn",        m.FirstName);
            cmd.Parameters.AddWithValue("@ln",        m.LastName);
            cmd.Parameters.AddWithValue("@desig",     (object)m.Designation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dept",      (object)m.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email",     (object)m.EmailAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone",     (object)m.PhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@doj",       (object)m.DateOfJoining ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dob",       (object)m.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@salary",    m.BasicSalary);
            cmd.Parameters.AddWithValue("@emptype",   (object)m.EmploymentType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status",    m.Status ?? "Active");
            cmd.Parameters.AddWithValue("@addr",      (object)m.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@emergency", (object)m.EmergencyContact ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@aadhaar",   (object)m.AadhaarNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pan",       (object)m.PANNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bank",      (object)m.BankAccountNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ifsc",      (object)m.IFSCCode ?? DBNull.Value);
        }

        private Employee GetById(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT * FROM Employees WHERE EmployeeId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapFull(r) : null;
                }
            }
        }

        private List<string> GetDepartments()
        {
            return new List<string> { "Projects", "Engineering", "Finance", "HR", "Logistics", "EHS", "MEP", "Civil", "Admin" };
        }

        private static Employee MapFull(SqlDataReader r) => new Employee
        {
            EmployeeId      = (int)r["EmployeeId"],
            EmployeeCode    = r["EmployeeCode"]?.ToString(),
            FirstName       = r["FirstName"]?.ToString(),
            LastName        = r["LastName"]?.ToString(),
            Designation     = r["Designation"]?.ToString(),
            Department      = r["Department"]?.ToString(),
            EmailAddress    = r["EmailAddress"]?.ToString(),
            PhoneNumber     = r["PhoneNumber"]?.ToString(),
            DateOfJoining   = r["DateOfJoining"] is DBNull ? (DateTime?)null : (DateTime)r["DateOfJoining"],
            DateOfBirth     = r["DateOfBirth"] is DBNull ? (DateTime?)null : (DateTime)r["DateOfBirth"],
            BasicSalary     = r["BasicSalary"] is DBNull ? 0 : (decimal)r["BasicSalary"],
            EmploymentType  = r["EmploymentType"]?.ToString(),
            Status          = r["Status"]?.ToString(),
            Address         = r["Address"]?.ToString(),
            EmergencyContact= r["EmergencyContact"]?.ToString(),
            AadhaarNumber   = r["AadhaarNumber"]?.ToString(),
            PANNumber       = r["PANNumber"]?.ToString(),
            BankAccountNo   = r["BankAccountNo"]?.ToString(),
            IFSCCode        = r["IFSCCode"]?.ToString(),
            CreatedDate     = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private void LoadDropdowns()
        {
            ViewBag.Departments     = GetDepartments();
            ViewBag.EmploymentTypes = new[] { "FullTime", "PartTime", "Contract", "DailyWage" };
            ViewBag.Statuses        = new[] { "Active", "Inactive", "OnLeave" };
        }
    }
}
