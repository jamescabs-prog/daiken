using System;
using System.Linq;
using System.Web;

namespace Drafters
{
    public partial class SlipPrint : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Requester", "Admin", "Inventory" }; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            var slip = Db.IssuanceSlips.Find(QueryId("id") ?? 0);
            if (slip == null)
            {
                FlashError("Slip not found.");
                Response.Redirect("Requests.aspx", true);
                return;
            }
            var rf = slip.Form;
            if (CurrentRole == "Requester" && rf.RequesterId != CurrentUserId)
            {
                FlashError("You do not have access to that slip.");
                Response.Redirect("Requests.aspx", true);
                return;
            }

            lblHeading.Text = "ISSUANCE SLIP" + (slip.ReleasedAt != null ? " &amp; ACKNOWLEDGEMENT RECEIPT" : "");
            lblIsNo.Text = HttpUtility.HtmlEncode(slip.IsNo);
            lblMrs.Text = HttpUtility.HtmlEncode(rf.MrsNo);
            lblJob.Text = HttpUtility.HtmlEncode(rf.JobOrderNo ?? "-");
            lblDate.Text = slip.GeneratedAt.ToString("MMM dd, yyyy");
            lblArNo.Text = HttpUtility.HtmlEncode(slip.ArNo ?? "(not yet released)");
            lblRequester.Text = HttpUtility.HtmlEncode(rf.Requester.FullName + " (" + rf.Department + ")");
            lblPurpose.Text = HttpUtility.HtmlEncode(rf.Purpose);
            lblReleasedBy.Text = slip.ReleasedBy != null ? HttpUtility.HtmlEncode(slip.ReleasedBy.FullName) : "";
            lblApprovedBy.Text = rf.ApprovedBy != null ? HttpUtility.HtmlEncode(rf.ApprovedBy.FullName) : "";
            lblReceivedBy.Text = HttpUtility.HtmlEncode(slip.ReceivedBy ?? "");
            rptLines.DataSource = slip.Lines.ToList();
            rptLines.DataBind();
        }
    }
}
