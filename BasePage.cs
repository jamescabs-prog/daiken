using System;
using System.Linq;
using Drafters.Models;
using Drafters.Services;

namespace Drafters
{
    /// <summary>
    /// Base class of every page: login check, role check against the Users table, database context, flash messages.
    /// Every restricted page overrides AllowedRoles, so access is enforced on the server (hiding a menu is not enough).
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        private RequisitionContext _db;

        protected RequisitionContext Db
        {
            get { if (_db == null) _db = new RequisitionContext(); return _db; }
        }

        protected WorkflowService Flow { get { return new WorkflowService(Db); } }

        /// <summary>The login page overrides this to true.</summary>
        protected virtual bool IsPublic { get { return false; } }

        /// <summary>null = any logged-in user. Otherwise only these roles may open the page.</summary>
        protected virtual string[] AllowedRoles { get { return null; } }

        protected int CurrentUserId { get { return (int)Session["UserId"]; } }
        protected string CurrentRole { get { return Session["Role"] as string; } }

        /// <summary>The one dashboard / start page of each role (null = unknown role).</summary>
        public static string HomeFor(string role)
        {
            switch (role)
            {
                case "Admin": return "~/Default.aspx";
                case "Inventory": return "~/InventoryDashboard.aspx";
                case "Purchasing": return "~/PurchasingDashboard.aspx";
                case "Delivery": return "~/Delivery/Dashboard.aspx";
                case "Requester": return "~/RequestForm.aspx";
            }
            return null;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (IsPublic) return;

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            // The role always comes from the Users table (never from the Employee ID). It is re-checked on every request,
            // so a deactivated account or a changed role takes effect immediately.
            int uid = CurrentUserId;
            var acct = Db.Users.Where(u => u.UserId == uid).Select(u => new { u.Role, u.IsActive }).FirstOrDefault();
            if (acct == null || !acct.IsActive || HomeFor(acct.Role) == null)
            {
                Session.Clear();
                Response.Redirect("~/Login.aspx", true);
                return;
            }
            if (acct.Role != CurrentRole) Session["Role"] = acct.Role;

            var roles = AllowedRoles;
            if (roles != null && Array.IndexOf(roles, CurrentRole) < 0)
            {
                FlashError("You do not have access to that page.");
                Response.Redirect(HomeFor(CurrentRole), true);
            }
        }

        protected bool InRole(params string[] roles) { return Array.IndexOf(roles, CurrentRole) >= 0; }

        protected void FlashSuccess(string message) { Session["FlashSuccess"] = message; }
        protected void FlashError(string message) { Session["FlashError"] = message; }

        protected int? QueryId(string key)
        {
            int value;
            return int.TryParse(Request.QueryString[key], out value) ? (int?)value : null;
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (_db != null) { _db.Dispose(); _db = null; }
        }
    }
}
