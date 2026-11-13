using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Drafters.Delivery
{
    public partial class NoticesPage : DeliveryBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            pnlForm.Visible = Can("notice");
            if (!Can("notice")) litRO.Text = "<div class=\"dlv-note\">You have view-only access here. Delivery notices are recorded by the Delivery staff.</div>";
            if (IsPostBack) return;

            txtExpected.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");
            LoadPos();
            BindList();
        }

        private void LoadPos()
        {
            ddlPo.Items.Clear();
            List<PurchaseOrder> pos = Repo.PosWithoutNotice();
            ddlPo.Items.Add(new ListItem(pos.Count == 0 ? "No purchase orders awaiting a notice" : "- Select purchase order -", ""));
            foreach (PurchaseOrder p in pos) ddlPo.Items.Add(new ListItem(p.PoNo + " | " + p.Supplier, p.PoNo));
        }

        private void BindList()
        {
            var list = Repo.AllDeliveries();
            rptNotices.DataSource = list;
            rptNotices.DataBind();
            lblEmpty.Visible = list.Count == 0;
        }

        protected void ddlPo_SelectedIndexChanged(object sender, EventArgs e)
        {
            PurchaseOrder p = ddlPo.SelectedValue == "" ? null : Repo.GetPo(ddlPo.SelectedValue);
            if (p == null) { litPoInfo.Text = ""; rptLines.DataSource = null; rptLines.DataBind(); return; }
            litPoInfo.Text = "<div class=\"dlv-kv\">" + DeliveryHtml.Row("Supplier", DeliveryHtml.E(p.Supplier)) +
                             DeliveryHtml.Row("Requesting dept. / client", DeliveryHtml.E(p.Client)) +
                             DeliveryHtml.Row("PO date", DeliveryHtml.D(p.DateIssued)) + "</div>";
            rptLines.DataSource = p.Lines;
            rptLines.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Can("notice")) return;
            if (ddlPo.SelectedValue == "") { FlashError("Select the purchase order the notice refers to."); return; }
            DateTime exp;
            if (!DateTime.TryParse(txtExpected.Text, out exp)) { FlashError("Enter the expected delivery date."); return; }
            if (exp.Date < DateTime.Today) { FlashError("The expected delivery date cannot be in the past."); return; }

            var qty = new Dictionary<string, int>();
            int total = 0;
            foreach (RepeaterItem it in rptLines.Items)
            {
                string sku = ((HiddenField)it.FindControl("hfSku")).Value;
                int q;
                if (!int.TryParse(((TextBox)it.FindControl("txtQty")).Text, out q) || q < 0) { FlashError("Quantities must be whole numbers (0 or more)."); return; }
                qty[sku] = q; total += q;
            }
            if (total == 0) { FlashError("The notice must include at least one item quantity."); return; }

            string po = ddlPo.SelectedValue, notes = txtNotes.Text, by = CurrentUserName;
            Run(delegate { Repo.CreateNotice(po, exp, notes, qty, by); },
                "Delivery notice recorded for " + po + ". It is now listed under Receiving.", "DeliveryNotices.aspx");
        }
    }
}
