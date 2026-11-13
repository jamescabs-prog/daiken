using System;

namespace Drafters
{
    public partial class Logout : BasePage
    {
        protected override bool IsPublic { get { return true; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx", true);
        }
    }
}
