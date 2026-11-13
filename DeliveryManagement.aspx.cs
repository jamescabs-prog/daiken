using System;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Drafters.Delivery;

namespace Drafters
{
    /// <summary>
    /// Delivery Management (Admin). Read-only oversight of every delivery record. It deliberately does not link to
    /// the Delivery employee area, which only the Delivery role can open.
    /// </summary>
    public partial class DeliveryManagement : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Admin" }; } }

        private IDeliveryRepository Repo { get { return DeliveryData.Repository; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            var all = Repo.AllDeliveries();
            var reps = Repo.AllDiscrepancies();
            lblAwaiting.Text = all.Count(d => d.Status == DeliveryStatus.Awaiting).ToString();
            lblOpenRep.Text = reps.Count(r => r.Status == "Open").ToString();
            lblOverdue.Text = reps.Count(r => r.InformOverdue).ToString();
            lblDone.Text = all.Count(d => d.Status == DeliveryStatus.Done) + " / " + all.Count;

            ddlStatus.Items.Add(new ListItem("All statuses", ""));
            foreach (string s in DeliveryStatus.All) ddlStatus.Items.Add(new ListItem(s, s));

            rptReps.DataSource = reps;
            rptReps.DataBind();
            lblNoReps.Visible = reps.Count == 0;

            BindList();
            ShowDetail();
        }

        private void BindList()
        {
            string q = (txtSearch.Text ?? "").Trim().ToLower();
            var list = Repo.AllDeliveries().Where(d =>
                (ddlStatus.SelectedValue == "" || d.Status == ddlStatus.SelectedValue) &&
                (q == "" || d.Id.ToLower().Contains(q) || d.PoNo.ToLower().Contains(q) || d.Supplier.ToLower().Contains(q) ||
                 d.Client.ToLower().Contains(q) || d.DrNos.ToLower().Contains(q))).ToList();
            rptRecords.DataSource = list;
            rptRecords.DataBind();
            lblEmpty.Visible = list.Count == 0;
        }

        protected void btnSearch_Click(object sender, EventArgs e) { BindList(); }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlStatus.SelectedIndex = 0;
            BindList();
        }

        private void ShowDetail()
        {
            string id = Request.QueryString["id"];
            DeliveryRecord d = string.IsNullOrEmpty(id) ? null : Repo.Get(id);
            pnlDetail.Visible = d != null;
            if (d == null) return;

            litTitle.Text = DeliveryHtml.E(d.Id) + " / " + DeliveryHtml.E(d.PoNo);
            litStepper.Text = DeliveryHtml.StepperHtml(d);
            litSummary.Text = DeliveryHtml.SummaryHtml(d);
            litLines.Text = DeliveryHtml.LinesHtml(d, Repo, false);
            litReceipts.Text = DeliveryHtml.ReceiptsHtml(d);
            litDocs.Text = DeliveryHtml.DocsHtml(d);
            litLog.Text = DeliveryHtml.LogHtml(d);

            var sb = new StringBuilder();
            foreach (DiscrepancyReport r in Repo.AllDiscrepancies().Where(x => x.DeliveryId == d.Id))
                sb.Append("<p class=\"mb-2\"><b>" + DeliveryHtml.E(r.Id) + "</b> " + DeliveryHtml.Badge(r.Status) + " " + DeliveryHtml.InformBadge(r) +
                          " &middot; " + DeliveryHtml.E(r.IssueSummary) + "</p>");
            litRep.Text = sb.Length == 0 ? "<p class=\"text-muted mb-0\">No discrepancies were recorded.</p>" : sb.ToString();
        }
    }
}
