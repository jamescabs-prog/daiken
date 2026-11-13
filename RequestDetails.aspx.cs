using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Drafters.Models;
using Drafters.Services;

namespace Drafters
{
    public partial class RequestDetails : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Requester", "Admin", "Inventory" }; } }

        protected string RequesterName = "";
        protected bool Closed;

        private int RequisitionId { get { return QueryId("id") ?? 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            var rf = Db.Requisitions.Find(RequisitionId);
            if (rf == null)
            {
                FlashError("Requisition not found.");
                Response.Redirect("Requests.aspx", true);
                return;
            }
            if (CurrentRole == "Requester" && rf.RequesterId != CurrentUserId)
            {
                FlashError("You do not have access to that requisition.");
                Response.Redirect("Requests.aspx", true);
                return;
            }
            LoadData(rf);
        }

        private void LoadData(RequisitionForm rf)
        {
            RequesterName = rf.Requester.FullName;
            Closed = rf.Status == "Rejected" || rf.Status == "Completed" || rf.Status == "Cancelled";

            lblMrs.Text = HttpUtility.HtmlEncode(rf.MrsNo);
            lblStatus.Text = HttpUtility.HtmlEncode(rf.Status);
            lblStatus.CssClass = "badge fs-6 bg-" + Ui.Badge(rf.Status);
            lblRequester.Text = "<b>" + HttpUtility.HtmlEncode(rf.Requester.FullName) + "</b> <small>(" +
                HttpUtility.HtmlEncode(rf.Requester.EmployeeId) + " &middot; " + HttpUtility.HtmlEncode(rf.Department) + ")</small>";
            lblJob.Text = HttpUtility.HtmlEncode(rf.JobOrderNo ?? "-");
            lblNeeded.Text = rf.DateNeeded.ToString("MMM dd, yyyy");
            lblSubmitted.Text = rf.CreatedAt.ToString("MMM dd, yyyy h:mm tt");
            lblPurpose.Text = HttpUtility.HtmlEncode(rf.Purpose);

            if (rf.ApprovedBy != null && rf.ApprovalDate != null)
            {
                pnlApproval.Visible = true;
                lblApproval.Text = HttpUtility.HtmlEncode(rf.ApprovalStatus) + " by " + HttpUtility.HtmlEncode(rf.ApprovedBy.FullName) +
                    " on " + rf.ApprovalDate.Value.ToString("MMM dd, yyyy h:mm tt");
                lblNote.Text = HttpUtility.HtmlEncode(rf.NoteComments ?? "");
            }

            // requester actions
            if (CurrentRole == "Requester" && rf.Status == "For Revision")
            {
                pnlRevision.Visible = true;
                lblAdminReason.Text = HttpUtility.HtmlEncode(rf.NoteComments ?? "");
                lnkRevise.NavigateUrl = "RequestForm.aspx?id=" + rf.RequisitionId;
            }
            if (CurrentRole == "Requester" && rf.Status == "Pending Approval")
                pnlWithdraw.Visible = true;

            // admin review panel, with stock warning
            if (CurrentRole == "Admin" && rf.Status == "Pending Approval")
            {
                pnlReview.Visible = true;
                lblRevNo.Text = rf.RevisionCount > 0 ? "<span class=\"badge bg-info text-dark\">Revision #" + rf.RevisionCount + "</span>" : "";

                var low = rf.Lines.Where(l => l.QtyRequested > l.Item.Available).ToList();
                if (low.Count > 0)
                {
                    pnlStock.Visible = true;
                    var html = new StringBuilder();
                    var text = new StringBuilder("Out of stock / insufficient: ");
                    for (int i = 0; i < low.Count; i++)
                    {
                        var l = low[i];
                        int avail = Math.Max(0, l.Item.Available);
                        html.Append("<div>").Append(HttpUtility.HtmlEncode(l.Item.ItemName))
                            .Append(" &mdash; requested ").Append(l.QtyRequested).Append(", available ").Append(avail).Append("</div>");
                        if (i > 0) text.Append(", ");
                        text.Append(l.Item.ItemName).Append(" (requested ").Append(l.QtyRequested).Append(", available ").Append(avail).Append(")");
                    }
                    text.Append(". Please revise or delete this request.");
                    litStock.Text = html.ToString();
                    txtReason.Text = text.ToString();   // suggested reason, the admin can edit it
                }
            }

            rptLines.DataSource = rf.Lines.ToList();
            rptLines.DataBind();

            var slips = rf.Slips.OrderBy(s => s.SlipId).ToList();
            pnlSlips.Visible = slips.Count > 0;
            rptSlips.DataSource = slips;
            rptSlips.DataBind();

            var reps = rf.Replenishments.ToList();
            pnlReps.Visible = reps.Count > 0;
            rptReps.DataSource = reps;
            rptReps.DataBind();

            int id = rf.RequisitionId;
            rptLogs.DataSource = Db.TransactionLogs.Where(l => l.RequisitionId == id).OrderByDescending(l => l.CreatedAt).ToList();
            rptLogs.DataBind();
        }

        // ---------------------------------------------------------------- Admin actions
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            if (CurrentRole != "Admin") return;
            string comments = txtComments.Text.Trim();
            RunAndReload(delegate
            {
                Flow.Approve(RequisitionId, CurrentUserId, comments);
                FlashSuccess("Requisition approved. Stock checked and issuance slip generated.");
            });
        }

        protected void btnReturn_Click(object sender, EventArgs e)
        {
            if (CurrentRole != "Admin") return;
            string reason = txtReason.Text.Trim();
            if (reason.Length == 0)
            {
                FlashError("Please enter what the requester needs to revise.");
                Response.Redirect("RequestDetails.aspx?id=" + RequisitionId, true);
                return;
            }
            RunAndReload(delegate
            {
                Flow.ReturnForRevision(RequisitionId, CurrentUserId, reason);
                FlashSuccess("The RF was returned to the requester for revision.");
            });
        }

        // ---------------------------------------------------------------- Requester: delete completely
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (CurrentRole != "Requester") return;
            try
            {
                Flow.DeleteRequisition(RequisitionId, CurrentUserId);
                FlashSuccess("The requisition was deleted completely.");
                Response.Redirect("Requests.aspx", true);
            }
            catch (InvalidOperationException ex)
            {
                FlashError(ex.Message);
                Response.Redirect("RequestDetails.aspx?id=" + RequisitionId, true);
            }
        }

        // ---------------------------------------------------------------- Inventory: release items
        protected void rptSlips_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Release" || CurrentRole != "Inventory") return;

            int slipId = Convert.ToInt32(e.CommandArgument);
            var box = (TextBox)e.Item.FindControl("txtReceivedBy");
            string receivedBy = box == null ? "" : box.Text.Trim();
            if (receivedBy.Length == 0)
            {
                FlashError("Enter the name of the person receiving the items (requester signature).");
                Response.Redirect("RequestDetails.aspx?id=" + RequisitionId, true);
                return;
            }
            RunAndReload(delegate
            {
                Flow.Release(slipId, receivedBy, CurrentUserId);
                FlashSuccess("Items released. Inventory updated and Acknowledgement Receipt generated.");
            });
        }

        private void RunAndReload(Action action)
        {
            try { action(); }
            catch (InvalidOperationException ex) { FlashError(ex.Message); }
            catch (DbUpdateConcurrencyException) { FlashError("Stock changed while you were working. Please try again."); }
            Response.Redirect("RequestDetails.aspx?id=" + RequisitionId, true);
        }

        // ---------------------------------------------------------------- helpers used by the page markup
        protected string SlipItemsHtml(object dataItem)
        {
            var s = (IssuanceSlip)dataItem;
            var sb = new StringBuilder();
            foreach (var l in s.Lines)
                sb.Append("<div>").Append(l.Qty).Append(" x ").Append(HttpUtility.HtmlEncode(l.Item.ItemName)).Append("</div>");
            return sb.ToString();
        }

        protected string ReleaseInfo(object dataItem)
        {
            var s = (IssuanceSlip)dataItem;
            if (s.ReleasedAt == null) return "<span class=\"text-muted\">Not yet released</span>";
            return HttpUtility.HtmlEncode(s.ArNo) + "<br /><small class=\"text-muted\">" + s.ReleasedAt.Value.ToString("MMM dd, h:mm tt") +
                " &middot; received by " + HttpUtility.HtmlEncode(s.ReceivedBy) + "</small>";
        }

        protected bool CanRelease(object dataItem)
        {
            var s = (IssuanceSlip)dataItem;
            return CurrentRole == "Inventory" && s.ReleasedAt == null && !Closed;
        }

        protected string LogUser(object dataItem)
        {
            var l = (TransactionLog)dataItem;
            return l.User != null ? "· " + l.User.FullName : "";
        }
    }
}
