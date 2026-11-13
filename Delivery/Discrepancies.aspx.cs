using System;

namespace Drafters.Delivery
{
    public partial class DiscrepanciesPage : DeliveryBasePage
    {
        private DiscrepancyReport Cur()
        {
            string r = Request.QueryString["r"];
            return string.IsNullOrEmpty(r) ? null : Repo.GetDiscrepancy(r);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            var list = Repo.AllDiscrepancies();
            rptReps.DataSource = list;
            rptReps.DataBind();
            lblEmpty.Visible = list.Count == 0;
            ShowDetail();
        }

        private void ShowDetail()
        {
            DiscrepancyReport r = Cur();
            pnlDetail.Visible = r != null;
            if (r == null) return;
            DeliveryRecord d = Repo.Get(r.DeliveryId);
            bool open = r.Status == "Open";
            bool can = Can("discrepancy") && open;

            litHead.Text = DeliveryHtml.E(r.Id) + " &middot; " + DeliveryHtml.E(r.PoNo) + " &middot; " + DeliveryHtml.Badge(r.Status);
            litStepper.Text = DeliveryHtml.StepperHtml(d);
            litSummary.Text = DeliveryHtml.SummaryHtml(d);
            litIssues.Text = DeliveryHtml.IssuesHtml(r);
            litDeadline.Text = "Deadline: <b>" + DeliveryHtml.Dt(r.InformDeadline) + "</b> &nbsp; " + DeliveryHtml.InformBadge(r);

            litInfS.Text = r.SupplierInformedOn.HasValue ? "<span class=\"dlv-pos\">informed " + DeliveryHtml.Dt(r.SupplierInformedOn) + "</span>" : "<span class=\"dlv-neg\">not yet informed</span>";
            litInfC.Text = r.ClientInformedOn.HasValue ? "<span class=\"dlv-pos\">informed " + DeliveryHtml.Dt(r.ClientInformedOn) + "</span>" : "<span class=\"dlv-neg\">not yet informed</span>";
            btnInfS.Visible = can && !r.SupplierInformedOn.HasValue;
            btnInfC.Visible = can && !r.ClientInformedOn.HasValue;

            litRes.Text = r.Resolution == null
                ? "<p class=\"text-muted\">No resolution recorded yet.</p>"
                : "<p><b>" + DeliveryHtml.E(r.Resolution) + "</b> &middot; supplier deadline <b>" + DeliveryHtml.D(r.ReplacementDeadline) + "</b></p>";
            pnlRes.Visible = can;
            if (r.Resolution != null) { ddlRes.SelectedValue = r.Resolution; txtDeadline.Text = r.ReplacementDeadline.Value.ToString("yyyy-MM-dd"); }
            txtDeadline.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");

            litFollow.Text = DeliveryHtml.FollowUpsHtml(r);
            pnlFollow.Visible = can;

            litPo.Text = DeliveryHtml.E(d.PoStatus) + " &nbsp; " + DeliveryHtml.Badge(d.Status);
            lnkReceive.NavigateUrl = "Receiving.aspx?id=" + d.Id;
            lnkReceive.Visible = Can("receive") && open && d.Status == DeliveryStatus.Partial;
        }

        private string Url { get { return "Discrepancies.aspx?r=" + Cur().Id; } }

        protected void btnInfS_Click(object sender, EventArgs e)
        {
            if (!Can("discrepancy")) return;
            string id = Cur().Id, by = CurrentUserName;
            Run(delegate { Repo.MarkInformed(id, true, false, by); }, "Supplier marked as informed.", Url);
        }

        protected void btnInfC_Click(object sender, EventArgs e)
        {
            if (!Can("discrepancy")) return;
            string id = Cur().Id, by = CurrentUserName;
            Run(delegate { Repo.MarkInformed(id, false, true, by); }, "Client marked as informed.", Url);
        }

        protected void btnRes_Click(object sender, EventArgs e)
        {
            if (!Can("discrepancy")) return;
            DateTime dl;
            if (!DateTime.TryParse(txtDeadline.Text, out dl)) { FlashError("Set the supplier deadline for the replacement / correct items."); return; }
            string id = Cur().Id, by = CurrentUserName, res = ddlRes.SelectedValue;
            Run(delegate { Repo.SetResolution(id, res, dl, by); }, "Coordination with supplier saved and PO status updated.", Url);
        }

        protected void btnFollow_Click(object sender, EventArgs e)
        {
            if (!Can("discrepancy")) return;
            string id = Cur().Id, by = CurrentUserName, note = txtNote.Text;
            Run(delegate { Repo.AddFollowUp(id, note, by); }, "Follow-up recorded.", Url);
        }
    }
}
