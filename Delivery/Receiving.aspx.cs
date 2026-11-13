using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Drafters.Delivery
{
    public partial class ReceivingPage : DeliveryBasePage
    {
        private DeliveryRecord Cur()
        {
            string id = Request.QueryString["id"];
            return string.IsNullOrEmpty(id) ? null : Repo.Get(id);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Can("receive")) litRO.Text = "<div class=\"dlv-note\">You have view-only access here. Receiving deliveries is done by the Delivery staff.</div>";
            if (IsPostBack) return;

            var open = Repo.AllDeliveries().Where(d => d.Status == DeliveryStatus.Awaiting || d.Status == DeliveryStatus.Partial).OrderBy(d => d.ExpectedDate).ToList();
            rptOpen.DataSource = open;
            rptOpen.DataBind();
            lblEmpty.Visible = open.Count == 0;
            ShowDetail();
        }

        private void ShowDetail()
        {
            pnlReceive.Visible = false;
            DeliveryRecord d = Cur();
            if (d == null || !Can("receive")) return;
            if (d.Status != DeliveryStatus.Awaiting && d.Status != DeliveryStatus.Partial)
            {
                FlashError(d.Id + " is not open for receiving (" + d.Status + ").");
                return;
            }
            pnlReceive.Visible = true;
            litTitle.Text = DeliveryHtml.E(d.Id) + " / " + DeliveryHtml.E(d.PoNo);
            litStepper.Text = DeliveryHtml.StepperHtml(d);
            litSummary.Text = DeliveryHtml.SummaryHtml(d);
            DiscrepancyReport rf = Repo.OpenDiscrepancyFor(d.Id);
            litWarn.Text = rf == null ? "" : "<div class=\"dlv-note\">Receiving the replacement / corrected items for <b>" + DeliveryHtml.E(rf.Id) + "</b>. Only the outstanding quantities are expected.</div>";
            txtBy.Text = CurrentUserName;
            rptItems.DataSource = d.Lines.Where(l => l.Outstanding > 0).ToList();
            rptItems.DataBind();
        }

        private static int Num(string s, out bool ok) { int n; ok = int.TryParse((s ?? "").Trim(), out n); return n; }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            DeliveryRecord d = Cur();
            if (d == null || !Can("receive")) return;

            if (string.IsNullOrWhiteSpace(txtDr.Text)) { FlashError("Enter the supplier's Delivery Receipt (DR) number."); return; }
            if (string.IsNullOrWhiteSpace(txtBy.Text)) { FlashError("Enter the name of the person receiving the delivery."); return; }

            var lines = new List<ReceiptLine>();
            foreach (RepeaterItem it in rptItems.Items)
            {
                bool a, b, c;
                var l = new ReceiptLine();
                l.Sku = ((HiddenField)it.FindControl("hfSku")).Value;
                l.Name = ((HiddenField)it.FindControl("hfName")).Value;
                l.Delivered = Num(((TextBox)it.FindControl("txtDel")).Text, out a);
                l.Damaged = Num(((TextBox)it.FindControl("txtDmg")).Text, out b);
                l.Incorrect = Num(((TextBox)it.FindControl("txtInc")).Text, out c);
                l.Remarks = ((TextBox)it.FindControl("txtRem")).Text.Trim();
                if (!a || !b || !c) { FlashError("Quantities must be whole numbers."); return; }
                lines.Add(l);
            }

            int passed = (chk1.Checked ? 1 : 0) + (chk2.Checked ? 1 : 0) + (chk3.Checked ? 1 : 0) + (chk4.Checked ? 1 : 0);
            Receipt r;
            try { r = Repo.ReceiveDelivery(d.Id, txtDr.Text, txtBy.Text, passed, 4, lines, CurrentUserName); }
            catch (InvalidOperationException ex) { FlashError(ex.Message); return; }

            if (r.DiscrepancyId != null)
            {
                FlashSuccess("Partial receipt saved. Good items were accepted and inventory updated. " + r.DiscrepancyId + " was created - inform the supplier and client within 24 hours.");
                Response.Redirect("Discrepancies.aspx?r=" + r.DiscrepancyId, true);
            }
            else
            {
                FlashSuccess("All items accepted. DR attached to the PO, PO marked as delivered and inventory updated. Waiting for verification.");
                Response.Redirect("DeliveryRecords.aspx?id=" + d.Id, true);
            }
        }
    }
}
