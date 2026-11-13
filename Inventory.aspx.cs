using System;
using System.Linq;
using Drafters.Models;

namespace Drafters
{
    public partial class Inventory : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Inventory", "Admin" }; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindItems();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindItems();
        }

        private void BindItems()
        {
            var q = Db.Items.AsQueryable();
            string term = (txtSearch.Text ?? "").Trim();
            if (term.Length > 0) q = q.Where(i => i.ItemName.Contains(term) || i.ItemCode.Contains(term));
            rptItems.DataSource = q.OrderBy(i => i.ItemName).ToList();
            rptItems.DataBind();
        }

        protected string LowClass(object dataItem)
        {
            var i = (Item)dataItem;
            return i.Available <= i.ReorderLevel ? "table-warning" : "";
        }

        protected string InactiveBadge(object dataItem)
        {
            return ((Item)dataItem).IsActive ? "" : "<span class=\"badge bg-secondary\">inactive</span>";
        }
    }
}
