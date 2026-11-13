using System;
using System.Linq;
using System.Web.Script.Serialization;
using Drafters.Models;

namespace Drafters
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected bool LoggedIn;
        protected string Role = "";
        protected string FullName = "";
        protected string EmpId = "";
        protected int Unread;
        protected string PageName = "";
        protected string Initials = "";
        protected string RoleLabel = "";
        protected string FlashSuccessJson = "\"\"";
        protected string FlashErrorJson = "\"\"";
        protected bool InDelivery;          // current page is inside the Delivery subsystem folder

        // the signed-in user's own dashboard (used by the logo link)
        protected string HomeUrl { get { return ResolveUrl(BasePage.HomeFor(Role) ?? "~/Login.aspx"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            PageName = System.IO.Path.GetFileName(Request.Path) ?? "";
            LoggedIn = Session["UserId"] != null;
            InDelivery = Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/Delivery/", StringComparison.OrdinalIgnoreCase);
            if (!LoggedIn) return;

            Role = Session["Role"] as string ?? "";
            FullName = Session["FullName"] as string ?? "";
            EmpId = Session["EmployeeId"] as string ?? "";
            Initials = MakeInitials(FullName);
            RoleLabel = (Role == "Requester") ? "Employee" : Role;
            int uid = (int)Session["UserId"];
            using (var db = new RequisitionContext())
            {
                Unread = db.Notifications.Count(n => n.UserId == uid && !n.IsRead);
            }
        }

        // PreRender runs after the page's button handlers, so messages set during a click are shown too.
        protected void Page_PreRender(object sender, EventArgs e)
        {
            var ser = new JavaScriptSerializer();
            FlashSuccessJson = Safe(ser.Serialize(Session["FlashSuccess"] as string ?? ""));
            FlashErrorJson = Safe(ser.Serialize(Session["FlashError"] as string ?? ""));
            Session.Remove("FlashSuccess");
            Session.Remove("FlashError");
        }

        // returns "active" when the current page is one of the given pages (highlights the sidebar link)
        protected string Active(params string[] pages)
        {
            foreach (string p in pages)
                if (string.Equals(PageName, p, StringComparison.OrdinalIgnoreCase)) return "active";
            return "";
        }

        private static string MakeInitials(string name)
        {
            var parts = (name ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private static string Safe(string json)
        {
            return json.Replace("<", "\\u003c").Replace(">", "\\u003e");
        }
    }
}
