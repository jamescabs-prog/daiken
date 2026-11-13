using System;
using System.Linq;
using System.Text;
using System.Web;
using Drafters.Models;

namespace Drafters
{
    /// <summary>Admin Dashboard. Only the Admin role can open it (other roles are sent to their own dashboard).</summary>
    public partial class Default : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Admin" }; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            var q = Db.Requisitions.AsQueryable();
            lblPending.Text = q.Count(r => r.Status == "Pending Approval").ToString();
            lblReady.Text = q.Count(r => r.Status == "Ready for Release").ToString();
            lblAwaiting.Text = q.Count(r => r.Status == "Pending - Awaiting Stock").ToString();
            lblRevision.Text = q.Count(r => r.Status == "For Revision").ToString();
            lblCompleted.Text = q.Count(r => r.Status == "Completed").ToString();
            lblLow.Text = Db.Items.Count(i => i.IsActive && (i.Quantity - i.ReservedQty) <= i.ReorderLevel).ToString();
            lblRepF.Text = Db.ReplenishmentRequests.Count(r => r.Status == "Pending").ToString();

            var pending = Db.Requisitions.Where(r => r.Status == "Pending Approval").OrderBy(r => r.CreatedAt).ToList();
            rptPending.DataSource = pending;
            rptPending.DataBind();
            lblNoPending.Visible = pending.Count == 0;
        }

        protected string ItemsHtml(object dataItem)
        {
            var rf = (RequisitionForm)dataItem;
            var sb = new StringBuilder();
            foreach (var l in rf.Lines)
                sb.Append("<div>").Append(l.QtyRequested).Append(" x ").Append(HttpUtility.HtmlEncode(l.Item.ItemName)).Append("</div>");
            return sb.ToString();
        }

        protected string StockBadge(object dataItem)
        {
            var rf = (RequisitionForm)dataItem;
            bool lack = rf.Lines.Any(l => l.QtyRequested > l.Item.Available);
            return lack ? "<span class=\"badge bg-danger\">Out of stock / short</span>"
                        : "<span class=\"badge bg-success\">Available</span>";
        }

        protected string RevisionBadge(object dataItem)
        {
            var rf = (RequisitionForm)dataItem;
            return rf.RevisionCount > 0 ? "<span class=\"badge bg-info text-dark\">Rev #" + rf.RevisionCount + "</span>" : "";
        }
    }
}
