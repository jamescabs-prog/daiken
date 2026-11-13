using System;
using System.Linq;

namespace Drafters
{
    /// <summary>Inventory Dashboard. Only the Inventory role can open it.</summary>
    public partial class InventoryDashboard : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Inventory" }; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            var q = Db.Requisitions.AsQueryable();
            lblReady.Text = q.Count(r => r.Status == "Ready for Release").ToString();
            lblAwaiting.Text = q.Count(r => r.Status == "Pending - Awaiting Stock").ToString();
            lblLow.Text = Db.Items.Count(i => i.IsActive && (i.Quantity - i.ReservedQty) <= i.ReorderLevel).ToString();
            lblRepF.Text = Db.ReplenishmentRequests.Count(r => r.Status == "Pending").ToString();

            var rel = Db.Requisitions.Where(r => r.Status == "Ready for Release" || r.Status == "Pending - Awaiting Stock").OrderBy(r => r.CreatedAt).ToList();
            rptRelease.DataSource = rel;
            rptRelease.DataBind();
            lblNoRelease.Visible = rel.Count == 0;
        }
    }
}
