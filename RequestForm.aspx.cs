using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Drafters.ViewModels;

namespace Drafters
{
    public partial class RequestForm : BasePage
    {
        protected string ItemsJson = "[]";
        protected string InitialJson = "[]";

        protected override string[] AllowedRoles { get { return new[] { "Requester" }; } }

        private int? EditId { get { return QueryId("id"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            var ser = new JavaScriptSerializer();

            // live stock list for the item drop-downs
            var items = Db.Items.Where(i => i.IsActive).OrderBy(i => i.ItemName).ToList()
                .Select(i => new { ItemId = i.ItemId, ItemCode = i.ItemCode, ItemName = i.ItemName, Unit = i.Unit, Available = i.Quantity - i.ReservedQty })
                .ToList();
            ItemsJson = Safe(ser.Serialize(items));

            // requester information (from the login)
            lblEmpId.Text = HttpUtility.HtmlEncode(Session["EmployeeId"] as string ?? "");
            lblFullName.Text = HttpUtility.HtmlEncode(Session["FullName"] as string ?? "");
            lblDepartment.Text = HttpUtility.HtmlEncode(Session["Department"] as string ?? "");
            lblToday.Text = DateTime.Today.ToString("MMM dd, yyyy");

            if (IsPostBack) return;

            if (EditId == null)
            {
                lblHeading.Text = "Requisition Form (RF)";
                btnSubmit.Text = "Submit for approval";
                txtDateNeeded.Text = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");
                return;
            }

            // revise mode: load the returned requisition
            var rf = Db.Requisitions.Find(EditId.Value);
            if (rf == null || rf.RequesterId != CurrentUserId || rf.Status != "For Revision")
            {
                FlashError("Only your requisitions that were returned for revision can be edited.");
                Response.Redirect("Requests.aspx", true);
                return;
            }
            lblHeading.Text = "Revise Requisition " + HttpUtility.HtmlEncode(rf.MrsNo);
            btnSubmit.Text = "Resubmit to admin";
            pnlRevise.Visible = true;
            lblReason.Text = HttpUtility.HtmlEncode(rf.NoteComments ?? "");
            txtJobOrder.Text = rf.JobOrderNo;
            txtPurpose.Text = rf.Purpose;
            txtDateNeeded.Text = rf.DateNeeded.ToString("yyyy-MM-dd");
            InitialJson = Safe(ser.Serialize(rf.Lines.Select(l => new { ItemId = l.ItemId, Quantity = l.QtyRequested }).ToList()));
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            var lines = ReadPostedLines();
            var errors = new List<string>();

            string purpose = (txtPurpose.Text ?? "").Trim();
            if (purpose.Length == 0) errors.Add("Purpose is required.");
            if (purpose.Length > 500) errors.Add("Purpose must be 500 characters or fewer.");

            DateTime needed;
            if (!DateTime.TryParseExact((txtDateNeeded.Text ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out needed))
                errors.Add("Please enter a valid date needed.");
            else if (needed.Date < DateTime.Today)
                errors.Add("Date needed cannot be in the past.");

            if (lines.Count == 0) errors.Add("Add at least one item with a quantity greater than zero.");

            if (errors.Count > 0)
            {
                lblFormError.Text = string.Join("<br />", errors.Select(x => HttpUtility.HtmlEncode(x)));
                lblFormError.Visible = true;
                InitialJson = Safe(new JavaScriptSerializer().Serialize(lines.Select(l => new { ItemId = l.ItemId, Quantity = l.Quantity }).ToList()));
                return;
            }

            var vm = new CreateRequisitionVM
            {
                JobOrderNo = string.IsNullOrWhiteSpace(txtJobOrder.Text) ? null : txtJobOrder.Text.Trim(),
                Purpose = purpose,
                DateNeeded = needed,
                Lines = lines
            };

            if (EditId == null)
            {
                var rf = Flow.CreateRequisition(vm, CurrentUserId);
                FlashSuccess("Requisition " + rf.MrsNo + " submitted. The admin has been notified.");
                Response.Redirect("RequestDetails.aspx?id=" + rf.RequisitionId, true);
            }
            else
            {
                int id = EditId.Value;
                try
                {
                    Flow.Resubmit(id, CurrentUserId, vm);
                    FlashSuccess("Revised RF sent back to the admin for approval.");
                }
                catch (InvalidOperationException ex) { FlashError(ex.Message); }
                catch (DbUpdateConcurrencyException) { FlashError("Something changed while saving. Please try again."); }
                Response.Redirect("RequestDetails.aspx?id=" + id, true);
            }
        }

        // The item rows are plain HTML inputs (name="lineItem" / "lineQty"), read straight from the posted form.
        private List<LineInput> ReadPostedLines()
        {
            var list = new List<LineInput>();
            string[] ids = Request.Form.GetValues("lineItem");
            string[] qtys = Request.Form.GetValues("lineQty");
            if (ids == null || qtys == null) return list;

            int count = Math.Min(ids.Length, qtys.Length);
            for (int i = 0; i < count; i++)
            {
                int itemId, qty;
                if (int.TryParse(ids[i], out itemId) && int.TryParse(qtys[i], out qty) && itemId > 0 && qty > 0)
                    list.Add(new LineInput { ItemId = itemId, Quantity = qty });
            }
            return list;
        }

        private static string Safe(string json)
        {
            return json.Replace("<", "\\u003c").Replace(">", "\\u003e");
        }
    }
}
