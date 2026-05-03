using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using ERP.Models;

namespace ERP.Controllers
{
    public class FinanceController : Controller
    {
        private readonly string _conn = ConfigurationManager.ConnectionStrings["ArcusDB"].ConnectionString;

        public ActionResult Index()
        {
            ViewBag.TotalReceivables    = GetScalar("SELECT ISNULL(SUM(TotalAmount-PaidAmount),0) FROM Invoices WHERE InvoiceType='ClientInvoice' AND Status IN('Pending','PartiallyPaid')");
            ViewBag.TotalPayables       = GetScalar("SELECT ISNULL(SUM(TotalAmount-PaidAmount),0) FROM Invoices WHERE InvoiceType IN('SupplierInvoice','ContractorBill') AND Status IN('Pending','PartiallyPaid')");
            ViewBag.TotalExpensesMonth  = GetScalar("SELECT ISNULL(SUM(Amount),0) FROM Expenses WHERE MONTH(ExpenseDate)=MONTH(GETDATE()) AND YEAR(ExpenseDate)=YEAR(GETDATE())");
            ViewBag.TotalInvoiced       = GetScalar("SELECT ISNULL(SUM(TotalAmount),0) FROM Invoices WHERE InvoiceType='ClientInvoice'");
            ViewBag.TotalCollected      = GetScalar("SELECT ISNULL(SUM(PaidAmount),0) FROM Invoices WHERE InvoiceType='ClientInvoice'");
            return View();
        }

        // ── Invoices ─────────────────────────────────────────────
        public ActionResult Invoices(string type = "", string status = "")
        {
            var list = new List<Invoice>();
            var sql = "SELECT i.*,p.ProjectName FROM Invoices i LEFT JOIN Projects p ON p.ProjectId=i.ProjectId WHERE 1=1";
            if (!string.IsNullOrEmpty(type)) sql += " AND i.InvoiceType=@type";
            if (!string.IsNullOrEmpty(status)) sql += " AND i.Status=@status";
            sql += " ORDER BY i.InvoiceDate DESC";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (!string.IsNullOrEmpty(type)) cmd.Parameters.AddWithValue("@type", type);
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapInvoice(r));
                }
            }
            ViewBag.Type = type;
            ViewBag.Status = status;
            return View(list);
        }

        public ActionResult CreateInvoice()
        {
            LoadInvoiceDropdowns();
            return View(new Invoice { InvoiceDate = DateTime.Today, Status = "Pending" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateInvoice(Invoice m)
        {
            m.TotalAmount = m.Amount + m.TaxAmount;
            if (!ModelState.IsValid) { LoadInvoiceDropdowns(); return View(m); }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO Invoices(InvoiceNumber,ProjectId,InvoiceType,PartyName,InvoiceDate,DueDate,Amount,TaxAmount,TotalAmount,PaidAmount,Status,Description)
                            VALUES(@inv,@proj,@type,@party,@invdate,@due,@amount,@tax,@total,@paid,@status,@desc)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@inv",     m.InvoiceNumber);
                    cmd.Parameters.AddWithValue("@proj",    (object)m.ProjectId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@type",    m.InvoiceType);
                    cmd.Parameters.AddWithValue("@party",   m.PartyName);
                    cmd.Parameters.AddWithValue("@invdate", m.InvoiceDate);
                    cmd.Parameters.AddWithValue("@due",     (object)m.DueDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@amount",  m.Amount);
                    cmd.Parameters.AddWithValue("@tax",     m.TaxAmount);
                    cmd.Parameters.AddWithValue("@total",   m.TotalAmount);
                    cmd.Parameters.AddWithValue("@paid",    m.PaidAmount);
                    cmd.Parameters.AddWithValue("@status",  m.Status);
                    cmd.Parameters.AddWithValue("@desc",    (object)m.Description ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = $"Invoice '{m.InvoiceNumber}' created.";
            return RedirectToAction("Invoices");
        }

        // ── Expenses ──────────────────────────────────────────────
        public ActionResult Expenses(int? projectId = null, string category = "")
        {
            var list = new List<Expense>();
            var sql = "SELECT e.*,p.ProjectName FROM Expenses e LEFT JOIN Projects p ON p.ProjectId=e.ProjectId WHERE 1=1";
            if (projectId.HasValue) sql += " AND e.ProjectId=@proj";
            if (!string.IsNullOrEmpty(category)) sql += " AND e.Category=@cat";
            sql += " ORDER BY e.ExpenseDate DESC";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (projectId.HasValue) cmd.Parameters.AddWithValue("@proj", projectId.Value);
                    if (!string.IsNullOrEmpty(category)) cmd.Parameters.AddWithValue("@cat", category);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapExpense(r));
                }
            }
            ViewBag.ProjectId = projectId;
            ViewBag.Category = category;
            ViewBag.Projects = GetProjectDropdown();
            ViewBag.Categories = GetExpenseCategories();
            return View(list);
        }

        public ActionResult CreateExpense()
        {
            ViewBag.Projects = GetProjectDropdown();
            ViewBag.Categories = GetExpenseCategories();
            ViewBag.PaymentModes = new[] { "Cash", "Bank Transfer", "Cheque", "UPI", "NEFT/RTGS" };
            return View(new Expense { ExpenseDate = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateExpense(Expense m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Projects = GetProjectDropdown();
                ViewBag.Categories = GetExpenseCategories();
                ViewBag.PaymentModes = new[] { "Cash", "Bank Transfer", "Cheque", "UPI", "NEFT/RTGS" };
                return View(m);
            }
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                var sql = @"INSERT INTO Expenses(ProjectId,ExpenseDate,Category,Description,Amount,VoucherNumber,PaymentMode,ApprovedBy)
                            VALUES(@proj,@date,@cat,@desc,@amount,@voucher,@pmode,@approved)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@proj",     (object)m.ProjectId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@date",     m.ExpenseDate);
                    cmd.Parameters.AddWithValue("@cat",      m.Category);
                    cmd.Parameters.AddWithValue("@desc",     (object)m.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@amount",   m.Amount);
                    cmd.Parameters.AddWithValue("@voucher",  (object)m.VoucherNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pmode",    (object)m.PaymentMode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@approved", (object)m.ApprovedBy ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Expense recorded.";
            return RedirectToAction("Expenses");
        }

        private decimal GetScalar(string sql)
        {
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    var val = cmd.ExecuteScalar();
                    return val is DBNull ? 0 : Convert.ToDecimal(val);
                }
            }
        }

        private static Invoice MapInvoice(SqlDataReader r) => new Invoice
        {
            InvoiceId     = (int)r["InvoiceId"],
            InvoiceNumber = r["InvoiceNumber"]?.ToString(),
            ProjectId     = r["ProjectId"] is DBNull ? (int?)null : (int)r["ProjectId"],
            ProjectName   = r["ProjectName"]?.ToString(),
            InvoiceType   = r["InvoiceType"]?.ToString(),
            PartyName     = r["PartyName"]?.ToString(),
            InvoiceDate   = r["InvoiceDate"] is DBNull ? DateTime.Today : (DateTime)r["InvoiceDate"],
            DueDate       = r["DueDate"] is DBNull ? (DateTime?)null : (DateTime)r["DueDate"],
            Amount        = r["Amount"] is DBNull ? 0 : (decimal)r["Amount"],
            TaxAmount     = r["TaxAmount"] is DBNull ? 0 : (decimal)r["TaxAmount"],
            TotalAmount   = r["TotalAmount"] is DBNull ? 0 : (decimal)r["TotalAmount"],
            PaidAmount    = r["PaidAmount"] is DBNull ? 0 : (decimal)r["PaidAmount"],
            Status        = r["Status"]?.ToString(),
            Description   = r["Description"]?.ToString(),
            CreatedDate   = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private static Expense MapExpense(SqlDataReader r) => new Expense
        {
            ExpenseId     = (int)r["ExpenseId"],
            ProjectId     = r["ProjectId"] is DBNull ? (int?)null : (int)r["ProjectId"],
            ProjectName   = r["ProjectName"]?.ToString(),
            ExpenseDate   = r["ExpenseDate"] is DBNull ? DateTime.Today : (DateTime)r["ExpenseDate"],
            Category      = r["Category"]?.ToString(),
            Description   = r["Description"]?.ToString(),
            Amount        = r["Amount"] is DBNull ? 0 : (decimal)r["Amount"],
            VoucherNumber = r["VoucherNumber"]?.ToString(),
            PaymentMode   = r["PaymentMode"]?.ToString(),
            ApprovedBy    = r["ApprovedBy"]?.ToString(),
            CreatedDate   = r["CreatedDate"] is DBNull ? DateTime.Now : (DateTime)r["CreatedDate"]
        };

        private void LoadInvoiceDropdowns()
        {
            ViewBag.Projects     = GetProjectDropdown();
            ViewBag.InvoiceTypes = new[] { "ClientInvoice", "SupplierInvoice", "ContractorBill" };
            ViewBag.Statuses     = new[] { "Pending", "PartiallyPaid", "Paid", "Overdue", "Cancelled" };
        }

        private List<SelectListItem> GetProjectDropdown()
        {
            var items = new List<SelectListItem> { new SelectListItem { Text = "-- General / No Project --", Value = "" } };
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT ProjectId,ProjectCode+' - '+ProjectName AS Label FROM Projects ORDER BY ProjectName", con))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        items.Add(new SelectListItem { Text = r["Label"].ToString(), Value = r["ProjectId"].ToString() });
            }
            return items;
        }

        private string[] GetExpenseCategories() =>
            new[] { "Labour", "Material", "Equipment", "Transportation", "Admin", "Overhead", "Utilities", "Professional Fees", "Other" };
    }
}
