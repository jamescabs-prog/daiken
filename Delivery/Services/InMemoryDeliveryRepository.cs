using System;
using System.Collections.Generic;
using System.Linq;

namespace Drafters.Delivery
{
/// <summary>
/// In-memory implementation of IDeliveryRepository (no database yet; data resets when the app restarts).
/// It also holds the delivery business rules. Replace this class with a MySQL version later.
/// </summary>
public class InMemoryDeliveryRepository : IDeliveryRepository
{
    readonly object Sync = new object();
    readonly List<PurchaseOrder> POs = new List<PurchaseOrder>();
    readonly List<DeliveryRecord> Deliveries = new List<DeliveryRecord>();
    readonly List<DiscrepancyReport> Reports = new List<DiscrepancyReport>();
    readonly List<DeliveryAlert> Alerts = new List<DeliveryAlert>();
    readonly List<StockLevel> Inventory = new List<StockLevel>();
    int _del = 0, _rep = 0, _rcpt = 0, _inv = 0;

    public InMemoryDeliveryRepository() { Seed(); }

    // ---------- Queries ----------
    public List<DeliveryRecord> AllDeliveries() { lock (Sync) { return Deliveries.OrderByDescending(d => d.NoticeDate).ToList(); } }
    public DeliveryRecord Get(string id) { lock (Sync) { return Deliveries.FirstOrDefault(d => d.Id == id); } }
    public List<DiscrepancyReport> AllDiscrepancies() { lock (Sync) { return Reports.OrderByDescending(r => r.Created).ToList(); } }
    public DiscrepancyReport GetDiscrepancy(string id) { lock (Sync) { return Reports.FirstOrDefault(r => r.Id == id); } }
    public DiscrepancyReport OpenDiscrepancyFor(string deliveryId) { lock (Sync) { return Reports.FirstOrDefault(r => r.DeliveryId == deliveryId && r.Status == "Open"); } }
    public PurchaseOrder GetPo(string poNo) { lock (Sync) { return POs.FirstOrDefault(p => p.PoNo == poNo); } }
    public List<PurchaseOrder> PosWithoutNotice()
    {
        lock (Sync) { return POs.Where(p => !Deliveries.Any(d => d.PoNo == p.PoNo)).OrderBy(p => p.PoNo).ToList(); }
    }
    public int OnHand(string sku) { lock (Sync) { StockLevel i = Inventory.FirstOrDefault(x => x.Sku == sku); return i == null ? 0 : i.OnHand; } }

    static bool CanSee(DeliveryAlert n, string role) { return n.Role == null || n.Role == role; }
    public List<DeliveryAlert> AlertsFor(string role) { lock (Sync) { return Alerts.Where(n => CanSee(n, role)).ToList(); } }
    public int UnreadCount(string role) { lock (Sync) { return Alerts.Count(n => !n.Read && CanSee(n, role)); } }
    public void MarkAllRead(string role) { lock (Sync) { foreach (DeliveryAlert n in Alerts) if (CanSee(n, role)) n.Read = true; } }

    // ---------- Step 1: supplier delivery notice ----------
    public DeliveryRecord CreateNotice(string poNo, DateTime expected, string notes, Dictionary<string, int> qty, string by)
    {
        lock (Sync)
        {
            PurchaseOrder po = POs.FirstOrDefault(p => p.PoNo == poNo);
            if (po == null) throw new InvalidOperationException("Purchase order not found.");
            if (Deliveries.Any(x => x.PoNo == poNo)) throw new InvalidOperationException("A delivery notice already exists for " + poNo + ".");
            DeliveryRecord d = new DeliveryRecord();
            d.Id = "DEL-" + (++_del).ToString("0000");
            d.PoNo = po.PoNo; d.Supplier = po.Supplier; d.Client = po.Client;
            d.NoticeDate = DateTime.Now; d.ExpectedDate = expected; d.NoticeNotes = notes;
            d.Status = DeliveryStatus.Awaiting; d.PoStatus = "Notice received";
            foreach (PoLine l in po.Lines)
            {
                int q;
                if (!qty.TryGetValue(l.Sku, out q)) q = l.Ordered;
                if (q < 0 || q > l.Ordered) throw new InvalidOperationException("Notice quantity for " + l.Name + " must be between 0 and " + l.Ordered + ".");
                d.Lines.Add(new PoLine { Sku = l.Sku, Name = l.Name, Unit = l.Unit, Ordered = l.Ordered, UnitPrice = l.UnitPrice, NoticeQty = q });
            }
            Deliveries.Add(d);
            AddLog(d, by, "Supplier delivery notice recorded (expected " + expected.ToString("MMM dd, yyyy") + ").");
            Notify("Delivery", "Delivery notice " + d.Id + " from " + d.Supplier + " expected " + expected.ToString("MMM dd, yyyy") + ".");
            return d;
        }
    }

    // ---------- Steps 2-5: receive, check, accept / partial receipt, update inventory ----------
    public Receipt ReceiveDelivery(string id, string drNo, string receivedBy, int checksPassed, int checksTotal, List<ReceiptLine> lines, string by)
    {
        lock (Sync)
        {
            DeliveryRecord d = Get(id);
            if (d == null) throw new InvalidOperationException("Delivery not found.");
            if (d.Status != DeliveryStatus.Awaiting && d.Status != DeliveryStatus.Partial) throw new InvalidOperationException("This delivery is not open for receiving.");
            if (string.IsNullOrWhiteSpace(drNo)) throw new InvalidOperationException("Enter the supplier's Delivery Receipt (DR) number.");
            if (string.IsNullOrWhiteSpace(receivedBy)) throw new InvalidOperationException("Enter the name of the person receiving the delivery.");
            bool any = false;
            foreach (ReceiptLine rl in lines)
            {
                PoLine pl = d.Lines.First(x => x.Sku == rl.Sku);
                rl.Expected = pl.Outstanding;
                if (rl.Delivered < 0 || rl.Damaged < 0 || rl.Incorrect < 0) throw new InvalidOperationException("Quantities cannot be negative.");
                if (rl.Delivered > rl.Expected) throw new InvalidOperationException("Delivered quantity for " + rl.Name + " exceeds the outstanding PO quantity (" + rl.Expected + ").");
                if (rl.Damaged + rl.Incorrect > rl.Delivered) throw new InvalidOperationException("Damaged + incorrect cannot exceed delivered for " + rl.Name + ".");
                if (rl.Delivered > 0) any = true;
            }
            if (!any) throw new InvalidOperationException("Enter the delivered quantity of at least one item.");

            Receipt r = new Receipt { Id = ++_rcpt, Date = DateTime.Now, DrNo = drNo.Trim(), ReceivedBy = receivedBy.Trim(), ChecksPassed = checksPassed, ChecksTotal = checksTotal, Lines = lines };
            int acceptedNow = 0;
            foreach (ReceiptLine rl in lines)
            {
                PoLine pl = d.Lines.First(x => x.Sku == rl.Sku);
                pl.Accepted += rl.Accepted;
                acceptedNow += rl.Accepted;
                StockLevel it = Inventory.FirstOrDefault(x => x.Sku == rl.Sku);
                if (it == null) { it = new StockLevel { Sku = rl.Sku, Name = rl.Name, OnHand = 0 }; Inventory.Add(it); }
                it.OnHand += rl.Accepted;
            }
            d.Receipts.Add(r);

            foreach (DiscrepancyReport old in Reports.Where(x => x.DeliveryId == d.Id && x.Status == "Open").ToList())
            {
                old.Status = "Resolved"; old.ResolvedOn = DateTime.Now;
                AddLog(d, by, old.Id + " closed - replacement / correction delivery received.");
            }
            AddLog(d, by, "Delivery received under DR " + r.DrNo + " (checklist " + checksPassed + "/" + checksTotal + "). " + acceptedNow + " unit(s) accepted and encoded to inventory.");

            if (d.Outstanding > 0)
            {
                DiscrepancyReport rf = new DiscrepancyReport { Id = "DSC-" + (++_rep).ToString("0000"), DeliveryId = d.Id, PoNo = d.PoNo, Supplier = d.Supplier, Created = DateTime.Now };
                rf.Issues.AddRange(lines.Where(x => x.HasIssue));
                Reports.Add(rf);
                r.DiscrepancyId = rf.Id;
                d.Status = DeliveryStatus.Partial; d.PoStatus = "Partially delivered";
                AddLog(d, by, "Missing / incorrect / damaged items recorded in " + rf.Id + ". Good items accepted (partial receipt). Supplier and client must be informed within 24 hours.");
                Notify("Delivery", rf.Id + " raised for " + d.PoNo + ": inform supplier and client within 24 hours.");
            }
            else
            {
                d.Status = DeliveryStatus.Verify; d.PoStatus = "Delivered";
                AddLog(d, by, "All items acceptable. DR attached to PO and PO marked as delivered. No outstanding items.");
                Notify("Delivery", d.Id + " (" + d.PoNo + ") is ready for verification.");
            }
            return r;
        }
    }

    // ---------- Discrepancy report (RepF) handling ----------
    public void MarkInformed(string discrepancyId, bool supplier, bool client, string by)
    {
        lock (Sync)
        {
            DiscrepancyReport r = GetDiscrepancy(discrepancyId); if (r == null) throw new InvalidOperationException("Discrepancy report not found.");
            DeliveryRecord d = Get(r.DeliveryId);
            if (supplier && !r.SupplierInformedOn.HasValue) { r.SupplierInformedOn = DateTime.Now; AddLog(d, by, "Supplier informed of discrepancy (" + r.Id + ")."); }
            if (client && !r.ClientInformedOn.HasValue) { r.ClientInformedOn = DateTime.Now; AddLog(d, by, "Client / requesting department informed of discrepancy (" + r.Id + ")."); }
        }
    }
    public void SetResolution(string discrepancyId, string resolution, DateTime deadline, string by)
    {
        lock (Sync)
        {
            DiscrepancyReport r = GetDiscrepancy(discrepancyId); if (r == null) throw new InvalidOperationException("Discrepancy report not found.");
            if (deadline.Date < DateTime.Today) throw new InvalidOperationException("The supplier deadline cannot be in the past.");
            DeliveryRecord d = Get(r.DeliveryId);
            r.Resolution = resolution; r.ReplacementDeadline = deadline;
            d.PoStatus = "Partially delivered - " + resolution.ToLower() + " due " + deadline.ToString("MMM dd");
            AddLog(d, by, "Coordinated with supplier: " + resolution.ToLower() + ", deadline " + deadline.ToString("MMM dd, yyyy") + ".");
        }
    }
    public void AddFollowUp(string discrepancyId, string note, string by)
    {
        lock (Sync)
        {
            DiscrepancyReport r = GetDiscrepancy(discrepancyId); if (r == null) throw new InvalidOperationException("Discrepancy report not found.");
            if (string.IsNullOrWhiteSpace(note)) throw new InvalidOperationException("Enter the follow-up note.");
            DeliveryRecord d = Get(r.DeliveryId);
            r.FollowUps.Add(new FollowUp { When = DateTime.Now, By = by, Note = note.Trim() });
            AddLog(d, by, "Follow-up with supplier recorded: " + note.Trim());
        }
    }

    // ---------- Step 6: supervisor verification ----------
    public void Verify(string id, string by, string note)
    {
        lock (Sync)
        {
            DeliveryRecord d = Get(id);
            if (d == null || d.Status != DeliveryStatus.Verify) throw new InvalidOperationException("This delivery is not waiting for verification.");
            if (d.Outstanding > 0) throw new InvalidOperationException("There are still outstanding items.");
            d.VerifiedBy = by; d.VerifiedOn = DateTime.Now; d.VerifyNote = note; d.Status = DeliveryStatus.Invoice;
            AddLog(d, by, "DR, PO and inventory records checked - all consistent.");
        }
    }
    // ---------- Step 7: sales invoice and delivery receipts ----------
    public void PrepareDocs(string id, string by)
    {
        lock (Sync)
        {
            DeliveryRecord d = Get(id);
            if (d == null || d.Status != DeliveryStatus.Invoice) throw new InvalidOperationException("This delivery is not ready for invoicing.");
            int n = ++_inv;
            d.InvoiceNo = "SI-" + DateTime.Now.Year + "-" + n.ToString("0000");
            d.DrReceiptNo = "DRC-" + DateTime.Now.Year + "-" + n.ToString("0000");
            d.DocsPreparedOn = DateTime.Now; d.Status = DeliveryStatus.Release;
            AddLog(d, by, "Sales invoice " + d.InvoiceNo + " and delivery receipt " + d.DrReceiptNo + " prepared.");
        }
    }
    // ---------- Steps 8-9: release items, signed receipt, mark completed ----------
    public void ReleaseItems(string id, string releasedTo, string signedBy, string signedRef, string by)
    {
        lock (Sync)
        {
            DeliveryRecord d = Get(id);
            if (d == null || d.Status != DeliveryStatus.Release) throw new InvalidOperationException("This delivery is not ready for release.");
            if (string.IsNullOrWhiteSpace(releasedTo) || string.IsNullOrWhiteSpace(signedBy) || string.IsNullOrWhiteSpace(signedRef))
                throw new InvalidOperationException("Enter who the items are released to, who signed, and the signed receipt reference.");
            foreach (PoLine l in d.Lines)
            {
                StockLevel it = Inventory.FirstOrDefault(x => x.Sku == l.Sku);
                if (it != null) it.OnHand = Math.Max(0, it.OnHand - l.Accepted);
            }
            d.ReleasedTo = releasedTo.Trim(); d.ReceivedBySigned = signedBy.Trim(); d.SignedReceiptRef = signedRef.Trim();
            d.ReleasedBy = by; d.ReleasedOn = DateTime.Now; d.Status = DeliveryStatus.Done; d.PoStatus = "Completed";
            AddLog(d, by, "Items released to " + d.ReleasedTo + " (signed by " + d.ReceivedBySigned + ", ref " + d.SignedReceiptRef + "). Transaction marked completed.");
            Notify(null, d.Id + " (" + d.PoNo + ") completed and released to " + d.ReleasedTo + ".");
        }
    }

    // ---------- helpers ----------
    void AddLog(DeliveryRecord d, string by, string text) { d.Log.Add(new LogEntry { When = DateTime.Now, By = by, Text = text }); }
    void Notify(string role, string text) { Alerts.Insert(0, new DeliveryAlert { When = DateTime.Now, Role = role, Text = text }); }

    // ---------- demo data (replace with DB later) ----------
    PoLine L(string sku, string name, string unit, int qty, decimal price)
    { return new PoLine { Sku = sku, Name = name, Unit = unit, Ordered = qty, UnitPrice = price }; }
    void AddPo(string no, string sup, string client, int daysAgo, params PoLine[] lines)
    {
        PurchaseOrder p = new PurchaseOrder { PoNo = no, Supplier = sup, Client = client, DateIssued = DateTime.Today.AddDays(-daysAgo) };
        p.Lines.AddRange(lines); POs.Add(p);
    }
    ReceiptLine RL(string sku, string name, int del, int dmg, int inc)
    { return new ReceiptLine { Sku = sku, Name = name, Delivered = del, Damaged = dmg, Incorrect = inc }; }
    void Stock(string sku, string name, int qty) { Inventory.Add(new StockLevel { Sku = sku, Name = name, OnHand = qty }); }

    void Seed()
    {
        Stock("CT-038", "Copper Tube 3/8 in", 4); Stock("FN-038", "Flare Nut 3/8 in", 60);
        Stock("TW-SET", "Torque Wrench Set", 1); Stock("DM-100", "Digital Multimeter", 2);
        Stock("SG-001", "Safety Gloves", 12); Stock("SQ-002", "Safety Goggles", 10);
        Stock("CN-24V", "Contactor 24V", 3); Stock("CB-TIE", "Cable Ties (pack)", 20);
        Stock("R32-CY", "Refrigerant R32 Cylinder", 0); Stock("CO-LTR", "Compressor Oil", 6);

        AddPo("PO-2026-0101", "CoolAir Components Inc.", "Assembly Line 2", 9, L("CT-038", "Copper Tube 3/8 in", "roll", 20, 1850m), L("FN-038", "Flare Nut 3/8 in", "pcs", 200, 12.5m));
        AddPo("PO-2026-0102", "Manila Tools & Fasteners", "Maintenance Department", 8, L("TW-SET", "Torque Wrench Set", "set", 5, 6400m), L("DM-100", "Digital Multimeter", "pcs", 4, 2850m));
        AddPo("PO-2026-0103", "ProSafe Equipment Corp.", "Quality Assurance", 7, L("SG-001", "Safety Gloves", "pair", 50, 95m), L("SQ-002", "Safety Goggles", "pcs", 30, 140m));
        AddPo("PO-2026-0104", "PowerLine Electrical Supply", "Production Planning", 12, L("CN-24V", "Contactor 24V", "pcs", 12, 780m), L("CB-TIE", "Cable Ties (pack)", "pack", 40, 65m));
        AddPo("PO-2026-0105", "CoolAir Components Inc.", "Assembly Line 2", 2, L("R32-CY", "Refrigerant R32 Cylinder", "cyl", 6, 5200m), L("CO-LTR", "Compressor Oil", "ltr", 24, 310m));

        // Run the real workflow so demo records are consistent with the rules.
        DeliveryRecord d1 = CreateNotice("PO-2026-0101", DateTime.Today.AddDays(2), "Truck arrives in the morning.", new Dictionary<string, int>(), "Paula Purchase");
        DeliveryRecord d2 = CreateNotice("PO-2026-0102", DateTime.Today.AddDays(-1), "", new Dictionary<string, int>(), "Paula Purchase");
        DeliveryRecord d3 = CreateNotice("PO-2026-0103", DateTime.Today.AddDays(-2), "", new Dictionary<string, int>(), "Paula Purchase");
        DeliveryRecord d4 = CreateNotice("PO-2026-0104", DateTime.Today.AddDays(-5), "", new Dictionary<string, int>(), "Paula Purchase");

        ReceiveDelivery(d2.Id, "DR-55102", "Grace Duran", 3, 4, new List<ReceiptLine> { RL("TW-SET", "Torque Wrench Set", 5, 0, 0), RL("DM-100", "Digital Multimeter", 3, 1, 0) }, "Grace Duran");
        ReceiveDelivery(d3.Id, "DR-77831", "Grace Duran", 4, 4, new List<ReceiptLine> { RL("SG-001", "Safety Gloves", 50, 0, 0), RL("SQ-002", "Safety Goggles", 30, 0, 0) }, "Grace Duran");
        ReceiveDelivery(d4.Id, "DR-20977", "Grace Duran", 4, 4, new List<ReceiptLine> { RL("CN-24V", "Contactor 24V", 12, 0, 0), RL("CB-TIE", "Cable Ties (pack)", 40, 0, 0) }, "Grace Duran");
        Verify(d4.Id, "Paolo Villanueva", "Records match.");
        PrepareDocs(d4.Id, "Paolo Villanueva");
        ReleaseItems(d4.Id, "Production Planning", "R. Mendoza", "SR-2026-0031", "Paolo Villanueva");
        foreach (DeliveryAlert n in Alerts) n.Read = false;
        Notify("Delivery", "Welcome to the Delivery area. Demo data has been loaded.");
    }
}
}
