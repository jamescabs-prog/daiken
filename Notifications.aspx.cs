using System;
using System.Collections.Generic;
using System.Linq;
using Drafters.Models;

namespace Drafters
{
    public partial class Notifications : BasePage
    {
        private HashSet<int> unreadIds = new HashSet<int>();

        protected void Page_Load(object sender, EventArgs e)
        {
            int uid = CurrentUserId;
            var list = Db.Notifications.Where(n => n.UserId == uid).OrderByDescending(n => n.CreatedAt).Take(100).ToList();
            unreadIds = new HashSet<int>(list.Where(n => !n.IsRead).Select(n => n.NotifId));

            rptNotifs.DataSource = list;
            rptNotifs.DataBind();
            lblEmpty.Visible = list.Count == 0;

            // mark everything as read after showing it
            foreach (var n in Db.Notifications.Where(n => n.UserId == uid && !n.IsRead)) n.IsRead = true;
            Db.SaveChanges();
        }

        protected string BoldClass(object dataItem)
        {
            return unreadIds.Contains(((Notification)dataItem).NotifId) ? "fw-bold" : "";
        }
    }
}
