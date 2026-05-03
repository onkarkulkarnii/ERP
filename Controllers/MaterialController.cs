using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;

namespace ERP.Controllers
{
    public class MaterialController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index(string category = "", string search = "")
        {
            var list = new List<Material>();
            var sql = "SELECT * FROM Materials WHERE 1=1";
            if (!string.IsNullOrEmpty(category)) sql += " AND Category=@cat";
            if (!string.IsNullOrEmpty(search)) sql += " AND (MaterialName LIKE @search OR MaterialCode LIKE @search)";
            sql += " ORDER BY Category, MaterialName";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (!string.IsNullOrEmpty(category)) cmd.Parameters.AddWithValue("@cat", category);
                    if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapFull(r));
                }
            }
            ViewBag.Category = category;
            ViewBag.Search = search;
            ViewBag.Categories = GetCategories();
            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.Categories = GetCategories();
            ViewBag.Units = GetUnits();
            return View(new Material { CreatedDate = DateTime.Now });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Material m)
        {
            if (!ModelState.IsValid) { ViewBag.Categories = GetCategories(); ViewBag.Units = GetUnits(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO Materials(MaterialCode,MaterialName,Category,Unit,UnitPrice,CurrentStock,MinStockLevel,Supplier,HSNCode,GSTRate,Description)
                            VALUES(@code,@name,@cat,@unit,@price,@stock,@min,@supplier,@hsn,@gst,@desc)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Material '{m.MaterialName}' added.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var mat = GetById(id);
            if (mat == null) return HttpNotFound();
            ViewBag.Categories = GetCategories();
            ViewBag.Units = GetUnits();
            return View(mat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Material m)
        {
            if (!ModelState.IsValid) { ViewBag.Categories = GetCategories(); ViewBag.Units = GetUnits(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"UPDATE Materials SET MaterialName=@name,Category=@cat,Unit=@unit,UnitPrice=@price,CurrentStock=@stock,
                            MinStockLevel=@min,Supplier=@supplier,HSNCode=@hsn,GSTRate=@gst,Description=@desc WHERE MaterialId=@id";
                using (var cmd = new SqlCommand(sql, con))
                {
                    Bind(cmd, m);
                    cmd.Parameters.AddWithValue("@id", m.MaterialId);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Material '{m.MaterialName}' updated.";
            return RedirectToAction("Index");
        }

        private void Bind(SqlCommand cmd, Material m)
        {
            cmd.Parameters.AddWithValue("@code",     m.MaterialCode);
            cmd.Parameters.AddWithValue("@name",     m.MaterialName);
            cmd.Parameters.AddWithValue("@cat",      (object)m.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@unit",     (object)m.Unit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@price",    m.UnitPrice);
            cmd.Parameters.AddWithValue("@stock",    m.CurrentStock);
            cmd.Parameters.AddWithValue("@min",      m.MinStockLevel);
            cmd.Parameters.AddWithValue("@supplier", (object)m.Supplier ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hsn",      (object)m.HSNCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@gst",      m.GSTRate);
            cmd.Parameters.AddWithValue("@desc",     (object)m.Description ?? DBNull.Value);
        }

        private Material GetById(int id)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT * FROM Materials WHERE MaterialId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapFull(r) : null;
                }
            }
        }

        private static Material MapFull(SqlDataReader r) => new Material
        {
            MaterialId    = (int)r["MaterialId"],
            MaterialCode  = r["MaterialCode"]?.ToString(),
            MaterialName  = r["MaterialName"]?.ToString(),
            Category      = r["Category"]?.ToString(),
            Unit          = r["Unit"]?.ToString(),
            UnitPrice     = r["UnitPrice"] is DBNull ? 0 : (decimal)r["UnitPrice"],
            CurrentStock  = r["CurrentStock"] is DBNull ? 0 : (decimal)r["CurrentStock"],
            MinStockLevel = r["MinStockLevel"] is DBNull ? 0 : (decimal)r["MinStockLevel"],
            Supplier      = r["Supplier"]?.ToString(),
            HSNCode       = r["HSNCode"]?.ToString(),
            GSTRate       = r["GSTRate"] is DBNull ? 0 : (decimal)r["GSTRate"],
            Description   = r["Description"]?.ToString(),
            CreatedDate   = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private List<string> GetCategories() =>
            new List<string> { "Cement", "Steel", "Aggregates", "Bricks", "Blocks", "Timber", "Sand", "Plumbing", "Electrical", "Finishing", "Chemicals", "Hardware" };

        private List<string> GetUnits() =>
            new List<string> { "Bag", "MT", "CFT", "No.", "Kg", "Litre", "Sqft", "Rmt", "Set", "Box" };
    }
}
