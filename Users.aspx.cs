using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Drafters.Models;
using Drafters.Services;
using UserEntity = Drafters.Models.User;   // "User" alone would mean Page.User

namespace Drafters
{
    /// <summary>User Management: Admin only. Roles are stored in the Users table and decide each person's dashboard.</summary>
    public partial class Users : BasePage
    {
        protected override string[] AllowedRoles { get { return new[] { "Admin" }; } }

        private static readonly string[] Roles = { "Admin", "Inventory", "Purchasing", "Delivery", "Requester" };
        private const string DefaultPassword = "Pass@123";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            ddlFilterRole.Items.Add(new ListItem("All roles", ""));
            foreach (string r in Roles) { ddlFilterRole.Items.Add(new ListItem(RoleLabel(r), r)); ddlRole.Items.Add(new ListItem(RoleLabel(r), r)); }
            BindList();
        }

        protected string RoleLabel(object role) { return (role as string) == "Requester" ? "Employee (Requester)" : (string)role; }
        protected string StatusBadge(object active) { return (bool)active ? "<span class=\"badge bg-success\">Active</span>" : "<span class=\"badge bg-secondary\">Inactive</span>"; }
        protected bool IsSelf(object dataItem) { return ((UserEntity)dataItem).UserId == CurrentUserId; }

        private void BindList()
        {
            var q = Db.Users.AsQueryable();
            string term = (txtSearch.Text ?? "").Trim();
            if (term.Length > 0) q = q.Where(u => u.EmployeeId.Contains(term) || u.FullName.Contains(term) || u.Username.Contains(term));
            if (ddlFilterRole.SelectedValue != "") { string r = ddlFilterRole.SelectedValue; q = q.Where(u => u.Role == r); }
            var list = q.OrderBy(u => u.EmployeeId).ToList();
            rptUsers.DataSource = list;
            rptUsers.DataBind();
            lblEmpty.Visible = list.Count == 0;
        }

        protected void btnSearch_Click(object sender, EventArgs e) { BindList(); }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = ""; ddlFilterRole.SelectedIndex = 0; BindList();
        }

        private void ShowForm(UserEntity u)
        {
            pnlForm.Visible = true;
            bool isNew = u == null;
            hfUserId.Value = isNew ? "" : u.UserId.ToString();
            lblFormTitle.Text = isNew ? "Add user" : "Edit user " + u.EmployeeId;
            txtEmp.Text = isNew ? "" : u.EmployeeId;
            txtEmp.ReadOnly = !isNew;
            txtName.Text = isNew ? "" : u.FullName;
            txtUser.Text = isNew ? "" : u.Username;
            txtDept.Text = isNew ? "" : u.Department;
            ddlRole.SelectedValue = isNew ? "Requester" : u.Role;
            ddlRole.Enabled = isNew || u.UserId != CurrentUserId;     // an admin cannot change their own role
            lblPwdHint.Text = isNew ? "Password" : "New password (leave blank to keep)";
        }

        protected void btnAdd_Click(object sender, EventArgs e) { ShowForm(null); }
        protected void btnCancel_Click(object sender, EventArgs e) { pnlForm.Visible = false; }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id; bool isNew = !int.TryParse(hfUserId.Value, out id);
            string emp = (txtEmp.Text ?? "").Trim().ToUpper();
            string name = (txtName.Text ?? "").Trim();
            string username = (txtUser.Text ?? "").Trim().ToLower();
            string dept = (txtDept.Text ?? "").Trim();
            string role = ddlRole.SelectedValue;
            string pwd = txtPwd.Text ?? "";

            if (isNew && !Regex.IsMatch(emp, @"^EMP-\d{4}$")) { FlashError("Employee ID must look like EMP-0005."); return; }
            if (name.Length == 0) { FlashError("Enter the full name."); return; }
            if (!Regex.IsMatch(username, @"^[a-z0-9._]{3,50}$")) { FlashError("Username must be 3-50 letters, numbers, dots or underscores."); return; }
            if (Array.IndexOf(Roles, role) < 0) { FlashError("Choose a valid role."); return; }
            if (isNew && pwd.Length < 6) { FlashError("The password must have at least 6 characters."); return; }
            if (!isNew && pwd.Length > 0 && pwd.Length < 6) { FlashError("The new password must have at least 6 characters."); return; }

            UserEntity u;
            if (isNew)
            {
                if (Db.Users.Any(x => x.EmployeeId == emp)) { FlashError(emp + " already exists."); return; }
                if (Db.Users.Any(x => x.Username == username)) { FlashError("That username is already used."); return; }
                u = new UserEntity { EmployeeId = emp, IsActive = true };
                Db.Users.Add(u);
            }
            else
            {
                u = Db.Users.Find(id);
                if (u == null) { FlashError("User not found."); return; }
                if (Db.Users.Any(x => x.Username == username && x.UserId != id)) { FlashError("That username is already used."); return; }
                if (u.UserId == CurrentUserId && role != u.Role) { FlashError("You cannot change your own role."); return; }
            }
            u.FullName = name; u.Username = username; u.Department = dept.Length == 0 ? null : dept; u.Role = role;
            if (pwd.Length > 0) u.PasswordHash = Security.Hash(pwd);

            try { Db.SaveChanges(); }
            catch (DbUpdateException) { FlashError("The user could not be saved. Check that the database allows the role (run Database\\add_delivery_user.sql once)."); return; }

            FlashSuccess(isNew ? "User " + emp + " was added." : "User " + u.EmployeeId + " was updated.");
            Response.Redirect("Users.aspx", true);
        }

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            var u = Db.Users.Find(id);
            if (u == null) { FlashError("User not found."); Response.Redirect("Users.aspx", true); return; }

            if (e.CommandName == "Edit") { ShowForm(u); return; }

            if (e.CommandName == "Reset")
            {
                u.PasswordHash = Security.Hash(DefaultPassword);
                Db.SaveChanges();
                FlashSuccess("The password of " + u.EmployeeId + " was reset to " + DefaultPassword + ".");
            }
            else if (e.CommandName == "Toggle")
            {
                if (u.UserId == CurrentUserId) FlashError("You cannot deactivate your own account.");
                else
                {
                    u.IsActive = !u.IsActive;
                    Db.SaveChanges();
                    FlashSuccess(u.EmployeeId + (u.IsActive ? " was activated." : " was deactivated."));
                }
            }
            Response.Redirect("Users.aspx", true);
        }
    }
}
