using System;
using System.Linq;

namespace Drafters
{
    public partial class Requests : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Requester", "Admin", "Inventory" }; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string status = Request.QueryString["status"];
                if (!string.IsNullOrEmpty(status) && ddlStatus.Items.FindByValue(status) != null)
                    ddlStatus.SelectedValue = status;
                chkArchived.Checked = Request.QueryString["archived"] == "1";
                lnkNew.Visible = CurrentRole == "Requester";
                BindList();
            }
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            BindList();
        }

        private void BindList()
        {
            bool archived = chkArchived.Checked;
            string status = ddlStatus.SelectedValue;

            var q = Db.Requisitions.Where(r => r.IsArchived == archived);
            if (CurrentRole == "Requester")
            {
                int uid = CurrentUserId;
                q = q.Where(r => r.RequesterId == uid);
            }
            if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);

            var list = q.OrderByDescending(r => r.CreatedAt).ToList();
            lblTitle.Text = archived ? "Archived Requisitions" : (CurrentRole == "Requester" ? "My Requests" : "Active Requisitions");
            rptRequests.DataSource = list;
            rptRequests.DataBind();
            lblEmpty.Visible = list.Count == 0;
        }
    }
}
