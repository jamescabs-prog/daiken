using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Drafters.Services;

namespace Drafters
{
    public partial class Login : BasePage
    {
        protected override bool IsPublic { get { return true; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["UserId"] != null) GoHome(Session["Role"] as string);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // accepts EMP-1001, emp-1001 or EMP1001
            string empId = (txtEmployeeId.Text ?? "").Trim().ToUpper().Replace(" ", "");
            if (Regex.IsMatch(empId, @"^EMP\d+$")) empId = "EMP-" + empId.Substring(3);
            string hash = Security.Hash(txtPassword.Text);

            Drafters.Models.User user = null;
            try
            {
                user = Db.Users.FirstOrDefault(u => u.EmployeeId == empId && u.PasswordHash == hash && u.IsActive);
            }
            catch (Exception ex)
            {
                // show the real reason (connection string, MySQL not running, package missing, ...) instead of crashing
                Exception inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                lblError.Text = "Database problem: " + HttpUtility.HtmlEncode(inner.Message) +
                    "<br /><small>The app is using: " + HttpUtility.HtmlEncode(DescribeConnection()) + "</small>";
                lblError.Visible = true;
                return;
            }

            if (user == null)
            {
                lblError.Text = "Invalid Employee ID or password.";
                lblError.Visible = true;
                return;
            }

            Session.Clear();
            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["EmployeeId"] = user.EmployeeId;
            Session["Department"] = user.Department;
            Session["Role"] = user.Role;
            GoHome(user.Role);
        }

        // Shows the connection string the app really reads from Web.config, with the password hidden.
        private static string DescribeConnection()
        {
            try
            {
                var cs = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["RequisitionDB"];
                if (cs == null) return "(no connection string named RequisitionDB was found in Web.config)";
                return Regex.Replace(cs.ConnectionString, @"(?i)(pwd|password)\s*=\s*([^;]*)", delegate(Match m)
                {
                    string pw = m.Groups[2].Value;
                    string shown = pw.Length > 2 ? pw.Substring(0, 1) + new string('*', pw.Length - 2) + pw.Substring(pw.Length - 1) : "**";
                    return m.Groups[1].Value + "=" + shown + " (" + pw.Length + " characters)";
                });
            }
            catch (Exception)
            {
                return "(could not read the connection string)";
            }
        }

        // The system identifies the role from the Employee ID and opens the matching page.
        private void GoHome(string role)
        {
            string home = HomeFor(role);
            if (home == null)
            {
                Session.Clear();
                lblError.Text = "Your account has no valid role. Please contact the administrator.";
                lblError.Visible = true;
                return;
            }
            Response.Redirect(home, true);
        }
    }
}
