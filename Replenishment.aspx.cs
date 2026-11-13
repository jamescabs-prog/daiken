using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Drafters.Models;

namespace Drafters
{
    public partial class Replenishment : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Purchasing", "Inventory", "Admin" }; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindList();
        }

        protected void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            BindList();
        }

        private void BindList()
        {
            var q = Db.ReplenishmentRequests.AsQueryable();
            if (!chkAll.Checked) q = q.Where(r => r.Status == "Pending");
            var list = q.OrderByDescending(r => r.CreatedAt).ToList();
            rptReps.DataSource = list;
            rptReps.DataBind();
            lblEmpty.Visible = list.Count == 0;
        }

        protected void rptReps_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int repId = Convert.ToInt32(e.CommandArgument);
            try
            {
                if (e.CommandName == "Receive" && InRole("Purchasing", "Inventory"))
                {
                    var box = (TextBox)e.Item.FindControl("txtQty");
                    int qty;
                    if (box == null || !int.TryParse(box.Text, out qty)) throw new InvalidOperationException("Enter the quantity received.");
                    Flow.ReceiveStock(repId, qty, CurrentUserId);
                    FlashSuccess("Stock received and allocated to the waiting requisition.");
                }
                else if (e.CommandName == "Remind" && InRole("Admin", "Inventory"))
                {
                    Flow.SendReminder(repId, CurrentUserId);
                    FlashSuccess("Reminder sent to Purchasing.");
                }
            }
            catch (InvalidOperationException ex) { FlashError(ex.Message); }
            catch (DbUpdateConcurrencyException) { FlashError("Stock changed while you were working. Please try again."); }
            Response.Redirect("Replenishment.aspx", true);
        }

        protected string MrsCell(object dataItem)
        {
            var r = (ReplenishmentRequest)dataItem;
            string mrs = HttpUtility.HtmlEncode(r.Form.MrsNo);
            return InRole("Admin", "Inventory")
                ? "<a href=\"RequestDetails.aspx?id=" + r.RequisitionId + "\">" + mrs + "</a>"
                : mrs;
        }

        protected bool CanReceive(object dataItem)
        {
            return ((ReplenishmentRequest)dataItem).Status == "Pending" && InRole("Purchasing", "Inventory");
        }

        protected bool CanRemind(object dataItem)
        {
            return ((ReplenishmentRequest)dataItem).Status == "Pending" && InRole("Admin", "Inventory");
        }

        protected string LastReminder(object dataItem)
        {
            var r = (ReplenishmentRequest)dataItem;
            return r.LastReminderAt == null ? "" : "<small class=\"text-muted d-block\">last " + r.LastReminderAt.Value.ToString("MMM dd h:mm tt") + "</small>";
        }
    }
}
