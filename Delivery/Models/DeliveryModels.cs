using System;
using System.Collections.Generic;
using System.Linq;

namespace Drafters.Delivery
{
    /// <summary>Delivery status values (one delivery moves through them in this order).</summary>
    public static class DeliveryStatus
    {
        public const string Awaiting = "Awaiting Delivery";
        public const string Partial = "Partially Received";
        public const string Verify = "For Verification";
        public const string Invoice = "For Invoicing";
        public const string Release = "For Release";
        public const string Done = "Completed";
        public static readonly string[] All = { Awaiting, Partial, Verify, Invoice, Release, Done };
    }

    /// <summary>Line of a purchase order / delivery (what was ordered, noticed and accepted).</summary>
    public class PoLine
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public int Ordered { get; set; }
        public int Accepted { get; set; }
        public int NoticeQty { get; set; }
        public decimal UnitPrice { get; set; }
        public int Outstanding { get { return Ordered - Accepted; } }
        public decimal Amount { get { return Accepted * UnitPrice; } }
    }

    /// <summary>Purchase order issued by the Purchasing subsystem (read by Delivery).</summary>
    public class PurchaseOrder
    {
        public string PoNo { get; set; }
        public string Supplier { get; set; }
        public string Client { get; set; }
        public DateTime DateIssued { get; set; }
        public List<PoLine> Lines { get; set; }
        public PurchaseOrder() { Lines = new List<PoLine>(); }
    }

    public class ReceiptLine
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public int Expected { get; set; }
        public int Delivered { get; set; }
        public int Damaged { get; set; }
        public int Incorrect { get; set; }
        public string Remarks { get; set; }
        public int Accepted { get { return Delivered - Damaged - Incorrect; } }
        public int Missing { get { return Math.Max(0, Expected - Delivered); } }
        public bool HasIssue { get { return Missing > 0 || Damaged > 0 || Incorrect > 0; } }
        public string IssueText
        {
            get
            {
                var p = new List<string>();
                if (Missing > 0) p.Add("missing " + Missing);
                if (Damaged > 0) p.Add("damaged " + Damaged);
                if (Incorrect > 0) p.Add("incorrect " + Incorrect);
                return string.Join(", ", p.ToArray());
            }
        }
    }

    /// <summary>One receiving transaction (a delivery can be received several times - partial receipts).</summary>
    public class Receipt
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string DrNo { get; set; }
        public string ReceivedBy { get; set; }
        public int ChecksPassed { get; set; }
        public int ChecksTotal { get; set; }
        public string DiscrepancyId { get; set; }
        public List<ReceiptLine> Lines { get; set; }
        public Receipt() { Lines = new List<ReceiptLine>(); }
        public string Outcome { get { return DiscrepancyId == null ? "Accepted in full" : "Partial receipt - " + DiscrepancyId; } }
    }

    public class LogEntry
    {
        public DateTime When { get; set; }
        public string By { get; set; }
        public string Text { get; set; }
    }

    public class FollowUp
    {
        public DateTime When { get; set; }
        public string By { get; set; }
        public string Note { get; set; }
    }

    /// <summary>Discrepancy report ("RepF" in the client's delivery process): missing / damaged / incorrect items found on receiving.</summary>
    public class DiscrepancyReport
    {
        public string Id { get; set; }
        public string DeliveryId { get; set; }
        public string PoNo { get; set; }
        public string Supplier { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime? SupplierInformedOn { get; set; }
        public DateTime? ClientInformedOn { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public DateTime? ReplacementDeadline { get; set; }
        public string Resolution { get; set; }
        public List<ReceiptLine> Issues { get; set; }
        public List<FollowUp> FollowUps { get; set; }
        public DiscrepancyReport() { Issues = new List<ReceiptLine>(); FollowUps = new List<FollowUp>(); Status = "Open"; }
        public DateTime InformDeadline { get { return Created.AddHours(24); } }
        public bool Informed { get { return SupplierInformedOn.HasValue && ClientInformedOn.HasValue; } }
        public bool InformOverdue { get { return Status == "Open" && !Informed && DateTime.Now > InformDeadline; } }
        public string IssueSummary
        {
            get { return string.Join("; ", Issues.Select(i => i.Name + " (" + i.IssueText + ")").ToArray()); }
        }
    }

    /// <summary>A delivery transaction: from supplier delivery notice until the items are released and signed for.</summary>
    public class DeliveryRecord
    {
        public string Id { get; set; }
        public string PoNo { get; set; }
        public string Supplier { get; set; }
        public string Client { get; set; }
        public DateTime NoticeDate { get; set; }
        public DateTime ExpectedDate { get; set; }
        public string NoticeNotes { get; set; }
        public string Status { get; set; }
        public string PoStatus { get; set; }
        public List<PoLine> Lines { get; set; }
        public List<Receipt> Receipts { get; set; }
        public List<LogEntry> Log { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedOn { get; set; }
        public string VerifyNote { get; set; }
        public string InvoiceNo { get; set; }
        public string DrReceiptNo { get; set; }
        public DateTime? DocsPreparedOn { get; set; }
        public string ReleasedTo { get; set; }
        public string ReceivedBySigned { get; set; }
        public string SignedReceiptRef { get; set; }
        public string ReleasedBy { get; set; }
        public DateTime? ReleasedOn { get; set; }

        public DeliveryRecord() { Lines = new List<PoLine>(); Receipts = new List<Receipt>(); Log = new List<LogEntry>(); }

        public int OrderedTotal { get { return Lines.Sum(l => l.Ordered); } }
        public int NoticeTotal { get { return Lines.Sum(l => l.NoticeQty); } }
        public int AcceptedTotal { get { return Lines.Sum(l => l.Accepted); } }
        public int Outstanding { get { return Lines.Sum(l => l.Outstanding); } }
        public decimal InvoiceTotal { get { return Lines.Sum(l => l.Amount); } }
        public string DrNos { get { return string.Join(", ", Receipts.Select(r => r.DrNo).Distinct().ToArray()); } }
    }

    /// <summary>Delivery alert shown on the Delivery dashboard (Role == null means everyone).</summary>
    public class DeliveryAlert
    {
        public DateTime When { get; set; }
        public string Role { get; set; }
        public string Text { get; set; }
        public bool Read { get; set; }
    }

    public class StockLevel
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public int OnHand { get; set; }
    }
}
