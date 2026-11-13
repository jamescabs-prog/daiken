using System;
using System.Linq;
using System.Text;
using System.Web;

namespace Drafters.Delivery
{
    /// <summary>HTML helpers shared by the Delivery pages (formatting, status badges and the repeated detail blocks).</summary>
    public static class DeliveryHtml
    {
        // ---------------- formatting ----------------
        public static string E(object o) { return HttpUtility.HtmlEncode(o == null ? "" : o.ToString()); }
        public static string Dt(object o) { if (o == null) return "-"; return ((DateTime)o).ToString("MMM dd, yyyy h:mm tt"); }
        public static string D(object o) { if (o == null) return "-"; return ((DateTime)o).ToString("MMM dd, yyyy"); }
        public static string Money(decimal m) { return "&#8369;" + string.Format("{0:N2}", m); }

        public static string Badge(object status)
        {
            string s = status == null ? "" : status.ToString();
            string c = "dlv-b-await";
            switch (s)
            {
                case DeliveryStatus.Partial: c = "dlv-b-partial"; break;
                case DeliveryStatus.Verify: c = "dlv-b-verify"; break;
                case DeliveryStatus.Invoice: c = "dlv-b-invoice"; break;
                case DeliveryStatus.Release: c = "dlv-b-release"; break;
                case DeliveryStatus.Done: c = "dlv-b-done"; break;
                case "Resolved": c = "dlv-b-done"; break;
                case "Open": c = "dlv-b-open"; break;
            }
            return "<span class=\"badge rounded-pill " + c + "\">" + E(s) + "</span>";
        }

        /// <summary>Page (inside the Delivery folder) where the next step of a delivery is done.</summary>
        public static string ActionUrl(DeliveryRecord d, IDeliveryRepository repo)
        {
            switch (d.Status)
            {
                case DeliveryStatus.Awaiting: return "Receiving.aspx?id=" + d.Id;
                case DeliveryStatus.Partial:
                    {
                        DiscrepancyReport r = repo.OpenDiscrepancyFor(d.Id);
                        return "Discrepancies.aspx" + (r != null ? "?r=" + r.Id : "");
                    }
                case DeliveryStatus.Verify: return "Verification.aspx?id=" + d.Id;
                case DeliveryStatus.Invoice:
                case DeliveryStatus.Release: return "Release.aspx?id=" + d.Id;
            }
            return "DeliveryRecords.aspx?id=" + d.Id;
        }

        public static string ActionLabel(DeliveryRecord d)
        {
            switch (d.Status)
            {
                case DeliveryStatus.Awaiting: return "Receive delivery";
                case DeliveryStatus.Partial: return "Follow up supplier";
                case DeliveryStatus.Verify: return "Verify records";
                case DeliveryStatus.Invoice: return "Prepare invoice &amp; DR";
                case DeliveryStatus.Release: return "Release items";
            }
            return "View record";
        }

        public static string InformBadge(DiscrepancyReport r)
        {
            if (r.Status == "Resolved") return "<span class=\"badge rounded-pill dlv-b-done\">Closed</span>";
            if (r.Informed) return "<span class=\"badge rounded-pill dlv-b-done\">Informed</span>";
            if (DateTime.Now > r.InformDeadline) return "<span class=\"badge rounded-pill dlv-b-open\">Overdue</span>";
            TimeSpan left = r.InformDeadline - DateTime.Now;
            return "<span class=\"badge rounded-pill dlv-b-partial\">Due in " + (int)left.TotalHours + "h " + left.Minutes + "m</span>";
        }

        // ---------------- reusable blocks ----------------
        public static string Row(string label, string html)
        { return "<div class=\"dlv-kv-item\"><span>" + label + "</span><b>" + html + "</b></div>"; }

        public static string StepperHtml(DeliveryRecord d)
        {
            string[] labels = { "Delivery notice", "Receive &amp; check", "Inventory update", "Records check", "Invoice &amp; DR", "Release &amp; sign", "Completed" };
            int cur;
            switch (d.Status)
            {
                case DeliveryStatus.Awaiting: cur = 1; break;
                case DeliveryStatus.Partial: cur = 1; break;
                case DeliveryStatus.Verify: cur = 3; break;
                case DeliveryStatus.Invoice: cur = 4; break;
                case DeliveryStatus.Release: cur = 5; break;
                default: cur = 7; break;
            }
            var sb = new StringBuilder("<div class=\"dlv-stepper\">");
            for (int i = 0; i < labels.Length; i++)
            {
                string cls = i < cur ? "done" : (i == cur ? (d.Status == DeliveryStatus.Partial ? "active warn" : "active") : "");
                sb.Append("<div class=\"dlv-step " + cls + "\"><span class=\"num\">" + (i < cur ? "&#10003;" : (i + 1).ToString()) + "</span><em>" + labels[i] + "</em></div>");
            }
            return sb.Append("</div>").ToString();
        }

        public static string SummaryHtml(DeliveryRecord d)
        {
            var sb = new StringBuilder("<div class=\"dlv-kv\">");
            sb.Append(Row("Delivery No.", E(d.Id)));
            sb.Append(Row("PO No.", E(d.PoNo)));
            sb.Append(Row("Supplier", E(d.Supplier)));
            sb.Append(Row("Requesting dept. / client", E(d.Client)));
            sb.Append(Row("Notice received", D(d.NoticeDate)));
            sb.Append(Row("Expected delivery", D(d.ExpectedDate)));
            sb.Append(Row("Supplier DR No(s).", d.DrNos == "" ? "-" : E(d.DrNos)));
            sb.Append(Row("PO status", E(d.PoStatus)));
            sb.Append(Row("Delivery status", Badge(d.Status)));
            return sb.Append("</div>").ToString();
        }

        public static string LinesHtml(DeliveryRecord d, IDeliveryRepository repo, bool showInventory)
        {
            var sb = new StringBuilder("<div class=\"table-responsive\"><table class=\"table table-sm align-middle mb-0\"><thead class=\"table-light\"><tr><th>Item</th><th>Unit</th><th class=\"text-end\">Ordered</th><th class=\"text-end\">Per notice</th><th class=\"text-end\">Accepted</th><th class=\"text-end\">Outstanding</th>");
            if (showInventory) sb.Append("<th class=\"text-end\">Inventory on hand</th>");
            sb.Append("</tr></thead><tbody>");
            foreach (PoLine l in d.Lines)
            {
                sb.Append("<tr><td><b>" + E(l.Name) + "</b><div class=\"dlv-sub\">" + E(l.Sku) + "</div></td><td>" + E(l.Unit) + "</td><td class=\"text-end\">" + l.Ordered + "</td><td class=\"text-end\">" + l.NoticeQty + "</td><td class=\"text-end\">" + l.Accepted + "</td><td class=\"text-end\">" + (l.Outstanding > 0 ? "<span class=\"dlv-neg\">" + l.Outstanding + "</span>" : "0") + "</td>");
                if (showInventory) sb.Append("<td class=\"text-end\">" + repo.OnHand(l.Sku) + "</td>");
                sb.Append("</tr>");
            }
            return sb.Append("</tbody></table></div>").ToString();
        }

        public static string IssuesHtml(DiscrepancyReport r)
        {
            var sb = new StringBuilder("<div class=\"table-responsive\"><table class=\"table table-sm align-middle mb-0\"><thead class=\"table-light\"><tr><th>Item</th><th class=\"text-end\">Missing</th><th class=\"text-end\">Damaged</th><th class=\"text-end\">Incorrect</th><th>Remarks</th></tr></thead><tbody>");
            foreach (ReceiptLine l in r.Issues)
                sb.Append("<tr><td><b>" + E(l.Name) + "</b><div class=\"dlv-sub\">" + E(l.Sku) + "</div></td><td class=\"text-end\">" + l.Missing + "</td><td class=\"text-end\">" + l.Damaged + "</td><td class=\"text-end\">" + l.Incorrect + "</td><td>" + E(l.Remarks) + "</td></tr>");
            return sb.Append("</tbody></table></div>").ToString();
        }

        public static string ReceiptsHtml(DeliveryRecord d)
        {
            if (d.Receipts.Count == 0) return "<p class=\"text-muted mb-0\">No delivery has been received yet.</p>";
            var sb = new StringBuilder();
            int n = 0;
            foreach (Receipt r in d.Receipts)
            {
                n++;
                sb.Append("<div class=\"dlv-receipt\"><div class=\"dlv-receipt-head\"><b>Receiving #" + n + "</b> &middot; DR " + E(r.DrNo) + " &middot; " + Dt(r.Date) + " &middot; received by " + E(r.ReceivedBy) + " &middot; checklist " + r.ChecksPassed + "/" + r.ChecksTotal + " &middot; <span class=\"" + (r.DiscrepancyId == null ? "dlv-pos" : "dlv-neg") + "\">" + E(r.Outcome) + "</span></div>");
                sb.Append("<div class=\"table-responsive\"><table class=\"table table-sm align-middle mb-0\"><thead class=\"table-light\"><tr><th>Item</th><th class=\"text-end\">Expected</th><th class=\"text-end\">Delivered</th><th class=\"text-end\">Missing</th><th class=\"text-end\">Damaged</th><th class=\"text-end\">Incorrect</th><th class=\"text-end\">Accepted</th><th>Remarks</th></tr></thead><tbody>");
                foreach (ReceiptLine l in r.Lines)
                    sb.Append("<tr><td>" + E(l.Name) + "</td><td class=\"text-end\">" + l.Expected + "</td><td class=\"text-end\">" + l.Delivered + "</td><td class=\"text-end\">" + l.Missing + "</td><td class=\"text-end\">" + l.Damaged + "</td><td class=\"text-end\">" + l.Incorrect + "</td><td class=\"text-end\"><b>" + l.Accepted + "</b></td><td>" + E(l.Remarks) + "</td></tr>");
                sb.Append("</tbody></table></div></div>");
            }
            return sb.ToString();
        }

        public static string LogHtml(DeliveryRecord d)
        {
            var sb = new StringBuilder("<ul class=\"dlv-timeline\">");
            foreach (LogEntry l in d.Log.OrderByDescending(x => x.When))
                sb.Append("<li><span class=\"dot\"></span><div>" + E(l.Text) + "<small>" + Dt(l.When) + " &middot; " + E(l.By) + "</small></div></li>");
            return sb.Append("</ul>").ToString();
        }

        public static string FollowUpsHtml(DiscrepancyReport r)
        {
            if (r.FollowUps.Count == 0) return "<p class=\"text-muted\">No follow-ups yet.</p>";
            var sb = new StringBuilder("<ul class=\"dlv-timeline\">");
            foreach (FollowUp f in r.FollowUps)
                sb.Append("<li><span class=\"dot\"></span><div>" + E(f.Note) + "<small>" + Dt(f.When) + " &middot; " + E(f.By) + "</small></div></li>");
            return sb.Append("</ul>").ToString();
        }

        public static string DocsHtml(DeliveryRecord d)
        {
            if (d.InvoiceNo == null) return "<p class=\"text-muted mb-0\">Sales invoice and delivery receipt have not been prepared yet.</p>";
            var sb = new StringBuilder("<div class=\"dlv-doc\">");
            sb.Append("<div class=\"dlv-doc-head\"><div><h5>Sales Invoice</h5><div class=\"dlv-sub\">No. " + E(d.InvoiceNo) + "</div></div><div class=\"text-end\"><h5>Delivery Receipt</h5><div class=\"dlv-sub\">No. " + E(d.DrReceiptNo) + "</div></div></div>");
            sb.Append("<div class=\"dlv-kv\">" + Row("Deliver to", E(d.Client)) + Row("PO No.", E(d.PoNo)) + Row("Supplier DR No(s).", E(d.DrNos)) + Row("Prepared", Dt(d.DocsPreparedOn)) + "</div>");
            sb.Append("<div class=\"table-responsive\"><table class=\"table table-sm align-middle\"><thead class=\"table-light\"><tr><th>Item</th><th class=\"text-end\">Qty</th><th>Unit</th><th class=\"text-end\">Unit price</th><th class=\"text-end\">Amount</th></tr></thead><tbody>");
            foreach (PoLine l in d.Lines)
                sb.Append("<tr><td>" + E(l.Name) + "</td><td class=\"text-end\">" + l.Accepted + "</td><td>" + E(l.Unit) + "</td><td class=\"text-end\">" + Money(l.UnitPrice) + "</td><td class=\"text-end\">" + Money(l.Amount) + "</td></tr>");
            sb.Append("<tr class=\"table-light\"><td colspan=\"4\" class=\"text-end\"><b>Total</b></td><td class=\"text-end\"><b>" + Money(d.InvoiceTotal) + "</b></td></tr></tbody></table></div>");
            if (d.ReleasedOn.HasValue)
                sb.Append("<div class=\"dlv-kv\">" + Row("Released to", E(d.ReleasedTo)) + Row("Signed by", E(d.ReceivedBySigned)) + Row("Signed receipt ref.", E(d.SignedReceiptRef)) + Row("Released on", Dt(d.ReleasedOn)) + "</div>");
            return sb.Append("</div>").ToString();
        }
    }
}
