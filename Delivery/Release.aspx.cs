using System;
using System.Linq;

namespace Drafters.Delivery
{
    public partial class ReleasePage : DeliveryBasePage
    {
        private DeliveryRecord Cur()
        {
            string id = Request.QueryString["id"];
            return string.IsNullOrEmpty(id) ? null : Repo.Get(id);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Can("release")) litRO.Text = "<div class=\"dlv-note\">You have view-only access here. Invoicing and release are done by the Delivery staff.</div>";
            if (IsPostBack) return;

            var waiting = Repo.AllDeliveries().Where(x => x.Status == DeliveryStatus.Invoice || x.Status == DeliveryStatus.Release).ToList();
            rptWaiting.DataSource = waiting;
            rptWaiting.DataBind();
            lblEmpty.Visible = waiting.Count == 0;

            DeliveryRecord d = Cur();
            bool ok = d != null && (d.Status == DeliveryStatus.Invoice || d.Status == DeliveryStatus.Release);
            pnlDetail.Visible = ok;
            if (d != null && !ok) FlashError(d.Id + " is not waiting for invoice or release (" + d.Status + ").");
            if (!ok) return;

            litTitle.Text = DeliveryHtml.E(d.Id) + " / " + DeliveryHtml.E(d.PoNo);
            litStepper.Text = DeliveryHtml.StepperHtml(d);
            litSummary.Text = DeliveryHtml.SummaryHtml(d);
            litLines.Text = DeliveryHtml.LinesHtml(d, Repo, false);
            pnlPrepare.Visible = d.Status == DeliveryStatus.Invoice && Can("release");
            pnlRelease.Visible = d.Status == DeliveryStatus.Release;
            pnlReleaseForm.Visible = Can("release");
            litDocs.Text = DeliveryHtml.DocsHtml(d);
            txtTo.Text = d.Client;
        }

        protected void btnPrepare_Click(object sender, EventArgs e)
        {
            DeliveryRecord d = Cur();
            if (d == null || !Can("release")) return;
            string id = d.Id, by = CurrentUserName;
            Run(delegate { Repo.PrepareDocs(id, by); }, "Sales invoice and delivery receipt prepared. Release the items and record the signed receipt.", "Release.aspx?id=" + id);
        }

        protected void btnRelease_Click(object sender, EventArgs e)
        {
            DeliveryRecord d = Cur();
            if (d == null || !Can("release")) return;
            string id = d.Id, by = CurrentUserName, to = txtTo.Text, signed = txtSigned.Text, rf = txtRef.Text;
            Run(delegate { Repo.ReleaseItems(id, to, signed, rf, by); }, "Items released and the transaction is marked as completed.", "DeliveryRecords.aspx?id=" + id);
        }
    }
}
