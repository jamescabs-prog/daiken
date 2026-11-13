using System;

namespace Drafters.Delivery
{
    /// <summary>
    /// Base class of every Delivery page. It reuses the Drafters login/session (BasePage) and adds the
    /// delivery repository and the delivery permissions.
    /// </summary>
    public class DeliveryBasePage : BasePage
    {
        /// <summary>Roles that can open the Delivery pages. Only the Delivery role (EMP-0004) can open the Delivery area. The Admin manages deliveries from Delivery Management instead.</summary>
        protected override string[] AllowedRoles
        {
            get { return new[] { "Delivery" }; }
        }

        /// <summary>The only data-access object the Delivery pages use (in memory now, MySQL later).</summary>
        protected IDeliveryRepository Repo { get { return DeliveryData.Repository; } }

        protected string CurrentUserName { get { return Session["FullName"] as string ?? ""; } }

        /// <summary>What each role may do in the delivery process.</summary>
        protected bool Can(string permission)
        {
            string role = CurrentRole;
            switch (permission)
            {
                case "notice":
                case "receive":
                case "discrepancy":
                case "verify":
                case "release": return role == "Delivery";
            }
            return false;
        }

        /// <summary>
        /// Runs a repository action. On success: shows a success message and redirects.
        /// On a business-rule violation: shows the error and stays on the page.
        /// </summary>
        protected void Run(Action action, string okMessage, string redirectUrl)
        {
            try { action(); }
            catch (InvalidOperationException ex) { FlashError(ex.Message); return; }
            FlashSuccess(okMessage);
            Response.Redirect(redirectUrl, true);
        }
    }
}
