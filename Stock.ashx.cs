using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using Drafters.Models;

namespace Drafters
{
    /// <summary>Live stock levels for the requisition form.</summary>
    public class StockHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session == null || context.Session["UserId"] == null)
            {
                context.Response.Write("[]");
                return;
            }

            string role = context.Session["Role"] as string;
            if (role != "Requester" && role != "Admin" && role != "Inventory")
            {
                context.Response.StatusCode = 403;
                context.Response.Write("[]");
                return;
            }

            using (var db = new RequisitionContext())
            {
                var data = db.Items.Where(i => i.IsActive).OrderBy(i => i.ItemName).ToList()
                    .Select(i => new { ItemId = i.ItemId, ItemCode = i.ItemCode, ItemName = i.ItemName, Unit = i.Unit, Available = i.Quantity - i.ReservedQty })
                    .ToList();
                context.Response.Write(new JavaScriptSerializer().Serialize(data));
            }
        }
    }
}
