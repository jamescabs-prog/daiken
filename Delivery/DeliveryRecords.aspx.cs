using System;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Drafters.Delivery
{
    public partial class RecordsPage : DeliveryBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            ddlStatus.Items.Add(new ListItem("All statuses", ""));
            foreach (string s in DeliveryStatus.All) ddlStatus.Items.Add(new ListItem(s, s));
            BindList();
            ShowDetail();
        }

        private void BindList()
        {
            string q = (txtSearch.Text ?? "").Trim().ToLower();
            var list = Repo.AllDeliveries().Where(d =>
                (ddlStatus.SelectedValue == "" || d.Status == ddlStatus.SelectedValue) &&
                (q == "" || d.Id.ToLower().Contains(q) || d.PoNo.ToLower().Contains(q) ||
                 d.Supplier.ToLower().Contains(q) || d.Client.ToLower().Contains(q) || d.DrNos.ToLower().Contains(q))).ToList();
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
                sb.Append("<p class=\"mb-2\"><a href=\"Discrepancies.aspx?r=" + r.Id + "\"><b>" + DeliveryHtml.E(r.Id) + "</b></a> " + DeliveryHtml.Badge(r.Status) + " &middot; " + DeliveryHtml.E(r.IssueSummary) + "</p>");
            litRep.Text = sb.Length == 0 ? "<p class=\"text-muted mb-0\">No discrepancies were recorded.</p>" : sb.ToString();

            lnkNext.Visible = d.Status != DeliveryStatus.Done;
            lnkNext.NavigateUrl = DeliveryHtml.ActionUrl(d, Repo);
            lnkNext.Text = DeliveryHtml.ActionLabel(d);
        }
    }
}
