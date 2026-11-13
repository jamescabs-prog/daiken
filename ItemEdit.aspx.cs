using System;
using System.Linq;
using System.Web;
using Drafters.Models;

namespace Drafters
{
    public partial class ItemEdit : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Inventory", "Admin" }; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            if (QueryId("id") == null)
            {
                lblHeading.Text = "New item";
                txtUnit.Text = "pc";
                txtQty.Text = "0";
                txtReorder.Text = "0";
                chkActive.Checked = true;
                return;
            }

            var item = Db.Items.Find(QueryId("id").Value);
            if (item == null)
            {
                FlashError("Item not found.");
                Response.Redirect("Inventory.aspx", true);
                return;
            }
            lblHeading.Text = "Edit item";
            txtCode.Text = item.ItemCode;
            txtName.Text = item.ItemName;
            txtUnit.Text = item.Unit;
            txtQty.Text = item.Quantity.ToString();
            txtReorder.Text = item.ReorderLevel.ToString();
            chkActive.Checked = item.IsActive;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string code = (txtCode.Text ?? "").Trim();
            string name = (txtName.Text ?? "").Trim();
            string unit = (txtUnit.Text ?? "").Trim();
            int qty, reorder;

            string error = null;
            if (code.Length == 0 || name.Length == 0 || unit.Length == 0) error = "Item code, name and unit are required.";
            else if (!int.TryParse(txtQty.Text, out qty) || qty < 0) error = "Quantity must be zero or more.";
            else if (!int.TryParse(txtReorder.Text, out reorder) || reorder < 0) error = "Reorder level must be zero or more.";

            if (error != null) { ShowError(error); return; }
            qty = int.Parse(txtQty.Text);
            reorder = int.Parse(txtReorder.Text);

            Item item = QueryId("id") == null ? null : Db.Items.Find(QueryId("id").Value);
            int itemId = item != null ? item.ItemId : 0;

            if (Db.Items.Any(i => i.ItemCode == code && i.ItemId != itemId)) { ShowError("Item code already exists."); return; }
            if (item != null && qty < item.ReservedQty)
            {
                ShowError("Quantity cannot be lower than the reserved quantity (" + item.ReservedQty + ").");
                return;
            }

            var svc = Flow;
            if (item == null)
            {
                item = new Item { ItemCode = code, ItemName = name, Unit = unit, Quantity = qty, ReservedQty = 0, ReorderLevel = reorder, IsActive = chkActive.Checked };
                Db.Items.Add(item);
                Db.SaveChanges();
                svc.Log(null, CurrentUserId, "Item Added", code + " - " + name + " qty " + qty);
            }
            else
            {
                int old = item.Quantity;
                item.ItemCode = code; item.ItemName = name; item.Unit = unit;
                item.Quantity = qty; item.ReorderLevel = reorder; item.IsActive = chkActive.Checked;
                svc.Log(null, CurrentUserId, "Item Updated", code + " qty " + old + " -> " + qty);
            }
            Db.SaveChanges();
            FlashSuccess("Item saved.");
            Response.Redirect("Inventory.aspx", true);
        }

        private void ShowError(string message)
        {
            lblMsg.Text = HttpUtility.HtmlEncode(message);
            lblMsg.Visible = true;
        }
    }
}
