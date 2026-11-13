using System;
using System.Linq;

namespace Drafters
{
    /// <summary>Purchasing Dashboard. Only the Purchasing role can open it.</summary>
    public partial class PurchasingDashboard : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Purchasing" }; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            var pending = Db.ReplenishmentRequests.Where(r => r.Status == "Pending").OrderBy(r => r.CreatedAt).ToList();
            lblPendingRep.Text = pending.Count.ToString();
            lblShort.Text = pending.Sum(r => r.QtyShort).ToString();
            lblReminders.Text = pending.Sum(r => r.ReminderCount).ToString();
            lblReceived.Text = Db.ReplenishmentRequests.Count(r => r.Status == "Received").ToString();

            var top = pending.Take(10).ToList();
            rptPending.DataSource = top;
            rptPending.DataBind();
            lblNone.Visible = top.Count == 0;
        }
    }
}
