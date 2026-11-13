using System;
using System.Linq;

namespace Drafters.Delivery
{
    public partial class VerificationPage : DeliveryBasePage
    {
        private DeliveryRecord Cur()
        {
            string id = Request.QueryString["id"];
            return string.IsNullOrEmpty(id) ? null : Repo.Get(id);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Can("verify")) litRO.Text = "<div class=\"dlv-note\">You have view-only access here. Verification is done by the Delivery staff.</div>";
            if (IsPostBack) return;

            var waiting = Repo.AllDeliveries().Where(d => d.Status == DeliveryStatus.Verify).ToList();
            rptWaiting.DataSource = waiting;
            rptWaiting.DataBind();
            lblEmpty.Visible = waiting.Count == 0;

            DeliveryRecord d0 = Cur();
            pnlDetail.Visible = d0 != null && d0.Status == DeliveryStatus.Verify;
            if (d0 != null && d0.Status != DeliveryStatus.Verify) FlashError(d0.Id + " is not waiting for verification (" + d0.Status + ").");
            if (!pnlDetail.Visible) return;

            litTitle.Text = DeliveryHtml.E(d0.Id) + " / " + DeliveryHtml.E(d0.PoNo);
            litStepper.Text = DeliveryHtml.StepperHtml(d0);
            litSummary.Text = DeliveryHtml.SummaryHtml(d0);
            litLines.Text = DeliveryHtml.LinesHtml(d0, Repo, true);
            litOut.Text = d0.Outstanding == 0
                ? "<span class=\"dlv-pos\">No - all PO items were accepted.</span>"
                : "<span class=\"dlv-neg\">Yes - " + d0.Outstanding + " unit(s).</span>";
            litReceipts.Text = DeliveryHtml.ReceiptsHtml(d0);
            pnlAct.Visible = Can("verify");
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {
            DeliveryRecord d = Cur();
            if (d == null || !Can("verify")) return;
            if (!(chk1.Checked && chk2.Checked && chk3.Checked)) { FlashError("Tick all three verification checks before proceeding."); return; }
            string id = d.Id, by = CurrentUserName, note = txtNote.Text.Trim();
            Run(delegate { Repo.Verify(id, by, note); }, "Records verified. Next: prepare the sales invoice and delivery receipt.", "Release.aspx?id=" + id);
        }
    }
}
