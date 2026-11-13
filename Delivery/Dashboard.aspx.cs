using System;
using System.Linq;
using System.Text;

namespace Drafters.Delivery
{
    public partial class DashboardPage : DeliveryBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var all = Repo.AllDeliveries();
            var reps = Repo.AllDiscrepancies();

            lblAwaiting.Text = all.Count(d => d.Status == DeliveryStatus.Awaiting).ToString();
            lblOpenRep.Text = reps.Count(r => r.Status == "Open").ToString();
            lblVerify.Text = all.Count(d => d.Status == DeliveryStatus.Verify).ToString();
            lblRelease.Text = all.Count(d => d.Status == DeliveryStatus.Invoice || d.Status == DeliveryStatus.Release).ToString();
            lblDone.Text = all.Count(d => d.Status == DeliveryStatus.Done).ToString();
            lblOverdue.Text = reps.Count(r => r.InformOverdue).ToString();
            lblAll.Text = all.Count.ToString();

            var open = all.Where(d => d.Status != DeliveryStatus.Done).OrderBy(d => d.ExpectedDate).ToList();
            rptOpen.DataSource = open;
            rptOpen.DataBind();
            lblNone.Visible = open.Count == 0;

            var sb = new StringBuilder();
            var alerts = Repo.AlertsFor(CurrentRole).Take(8).ToList();
            if (alerts.Count == 0) sb.Append("<div class=\"text-center text-muted py-4\">No alerts.</div>");
            foreach (var n in alerts)
                sb.Append("<div class=\"dlv-alert " + (n.Read ? "read" : "unread") + "\"><span class=\"dot\"></span><div>" + DeliveryHtml.E(n.Text) +
                          "<small>" + DeliveryHtml.Dt(n.When) + (n.Role != null ? " &middot; for " + DeliveryHtml.E(n.Role) : "") + "</small></div></div>");
            litAlerts.Text = sb.ToString();
            if (!IsPostBack) Repo.MarkAllRead(CurrentRole);   // unread alerts stay highlighted on this view only
        }

        protected string NextUrl(object dataItem) { return DeliveryHtml.ActionUrl((DeliveryRecord)dataItem, Repo); }
        protected string NextLabel(object dataItem) { return DeliveryHtml.ActionLabel((DeliveryRecord)dataItem); }
    }
}
