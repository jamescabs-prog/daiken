using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using Drafters.Models;

namespace Drafters
{
    /// <summary>Polled by the master page: returns new (unread) notifications for the pop-up toasts.</summary>
    public class NotificationsHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session == null || context.Session["UserId"] == null)
            {
                context.Response.Write("{\"unread\":0,\"items\":[]}");
                return;
            }

            int uid = (int)context.Session["UserId"];
            int after;
            int.TryParse(context.Request["afterId"], out after);

            using (var db = new RequisitionContext())
            {
                var items = db.Notifications
                    .Where(n => n.UserId == uid && !n.IsRead && n.NotifId > after)
                    .OrderBy(n => n.NotifId).Take(5).ToList()
                    .Select(n => new { NotifId = n.NotifId, Message = n.Message, Link = n.Link ?? "" })
                    .ToList();
                int unread = db.Notifications.Count(n => n.UserId == uid && !n.IsRead);
                context.Response.Write(new JavaScriptSerializer().Serialize(new { unread = unread, items = items }));
            }
        }
    }
}
