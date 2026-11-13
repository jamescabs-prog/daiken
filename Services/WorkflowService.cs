using System;
using System.Collections.Generic;
using System.Linq;
using Drafters.Models;
using Drafters.ViewModels;

namespace Drafters.Services
{
    /// <summary>
    /// Implements the Proposed Requisition System Flowchart:
    ///  Create RF -> notify Admin -> Approve or Return for revision -> check stock -> reserve + generate IS
    ///  -> (shortage? auto RepF to Purchasing, status Pending) -> release items + AR
    ///  -> update inventory -> Completed + archive.
    /// </summary>
    public class WorkflowService
    {
        private readonly RequisitionContext db;
        public WorkflowService(RequisitionContext context) { db = context; }

        // ------------------------------------------------------------ helpers
        private static string TempNo() { return "TMP" + Guid.NewGuid().ToString("N").Substring(0, 20); }

        public void Notify(int userId, string message, string link)
        {
            db.Notifications.Add(new Notification { UserId = userId, Message = message, Link = link, CreatedAt = DateTime.Now });
        }

        public void NotifyRole(string role, string message, string link)
        {
            var ids = db.Users.Where(u => u.Role == role && u.IsActive).Select(u => u.UserId).ToList();
            foreach (var id in ids) Notify(id, message, link);
        }

        public void Log(int? requisitionId, int? userId, string action, string details)
        {
            db.TransactionLogs.Add(new TransactionLog
            {
                RequisitionId = requisitionId, UserId = userId, Action = action,
                Details = details, CreatedAt = DateTime.Now
            });
        }

        // ------------------------------------------------------------ 1. Fill in RF -> insert requisition record -> notify Admin
        public RequisitionForm CreateRequisition(CreateRequisitionVM vm, int userId)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                var user = db.Users.Find(userId);
                var now = DateTime.Now;
                var rf = new RequisitionForm
                {
                    MrsNo = TempNo(),
                    JobOrderNo = vm.JobOrderNo,
                    RequesterId = userId,
                    Department = user.Department,
                    Purpose = vm.Purpose,
                    DateNeeded = vm.DateNeeded,
                    Status = "Pending Approval",
                    ApprovalStatus = "Pending",
                    IsArchived = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                foreach (var g in vm.Lines.Where(l => l.ItemId > 0 && l.Quantity > 0).GroupBy(l => l.ItemId))
                    rf.Lines.Add(new RequisitionLine { ItemId = g.Key, QtyRequested = g.Sum(x => x.Quantity) });

                db.Requisitions.Add(rf);
                db.SaveChanges();

                rf.MrsNo = "MRS-" + now.Year + "-" + rf.RequisitionId.ToString("D5");
                Log(rf.RequisitionId, userId, "RF Submitted", "Requisition " + rf.MrsNo + " submitted by " + user.FullName);
                NotifyRole("Admin", "New requisition " + rf.MrsNo + " from " + user.FullName + " needs your review.",
                           "RequestDetails.aspx?id=" + rf.RequisitionId);
                db.SaveChanges();
                tx.Commit();
                return rf;
            }
        }

        // ------------------------------------------------------------ 2. Admin approves -> check stock -> reserve -> IS -> RepF
        public void Approve(int requisitionId, int adminId, string comments)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                var rf = db.Requisitions.Find(requisitionId);
                if (rf == null || rf.Status != "Pending Approval")
                    throw new InvalidOperationException("This requisition is no longer awaiting approval.");

                var now = DateTime.Now;
                rf.ApprovalStatus = "Approved";
                rf.ApprovedById = adminId;
                rf.ApprovalDate = now;
                rf.NoteComments = comments;
                rf.UpdatedAt = now;

                var slip = new IssuanceSlip { IsNo = TempNo(), RequisitionId = rf.RequisitionId, GeneratedAt = now };
                var shortages = new List<ReplenishmentRequest>();

                foreach (var line in rf.Lines.ToList())
                {
                    var item = line.Item;
                    int available = Math.Max(0, item.Quantity - item.ReservedQty);
                    int reserve = Math.Min(available, line.QtyRequested);

                    if (reserve > 0)
                    {
                        item.ReservedQty += reserve;           // reserve the available quantity
                        line.QtyReserved += reserve;
                        slip.Lines.Add(new IssuanceSlipLine { ItemId = item.ItemId, LineId = line.LineId, Qty = reserve });
                    }

                    int shortage = line.QtyRequested - reserve; // compute the shortage (if any)
                    if (shortage > 0)
                        shortages.Add(new ReplenishmentRequest
                        {
                            RepNo = TempNo(), RequisitionId = rf.RequisitionId, LineId = line.LineId,
                            ItemId = item.ItemId, QtyShort = shortage, Status = "Pending", CreatedAt = now
                        });
                }

                if (slip.Lines.Count > 0) db.IssuanceSlips.Add(slip);
                foreach (var r in shortages) db.ReplenishmentRequests.Add(r);

                bool hasShortage = shortages.Count > 0;
                rf.Status = hasShortage ? "Pending - Awaiting Stock" : "Ready for Release";
                rf.ReplenishmentStatus = hasShortage ? "Pending" : null;
                db.SaveChanges();

                // generate document numbers now that IDs exist
                if (slip.Lines.Count > 0) slip.IsNo = "IS-" + now.Year + "-" + slip.SlipId.ToString("D5");
                foreach (var r in shortages) r.RepNo = "REPF-" + now.Year + "-" + r.RepId.ToString("D5");

                string link = "RequestDetails.aspx?id=" + rf.RequisitionId;
                Log(rf.RequisitionId, adminId, "Approved", "RF approved. " + (hasShortage ? "Stock shortage detected; RepF auto-generated." : "Stock sufficient; IS generated."));
                NotifyRole("Inventory", rf.MrsNo + " approved. " + (hasShortage ? "Partial/none in stock - see slip and RepF." : "Stock reserved - ready for release."), link);
                if (hasShortage)
                {
                    NotifyRole("Purchasing", "RepF auto-generated for " + rf.MrsNo + " (" + shortages.Count + " item(s) short).", "Replenishment.aspx");
                    Notify(rf.RequesterId, rf.MrsNo + " approved but is PENDING: some items need replenishment.", link);
                }
                else
                {
                    Notify(rf.RequesterId, rf.MrsNo + " approved. Items are reserved and ready for release.", link);
                }
                db.SaveChanges();
                tx.Commit();
            }
        }

        // ------------------------------------------------------------ 2b. Admin does NOT approve -> reason -> RF goes back to requester for revision
        public void ReturnForRevision(int requisitionId, int adminId, string reason)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                var rf = db.Requisitions.Find(requisitionId);
                if (rf == null || rf.Status != "Pending Approval")
                    throw new InvalidOperationException("This requisition is no longer awaiting approval.");

                rf.ApprovalStatus = "Returned";
                rf.ApprovedById = adminId;
                rf.ApprovalDate = DateTime.Now;
                rf.NoteComments = reason;
                rf.Status = "For Revision";
                rf.UpdatedAt = DateTime.Now;

                Log(rf.RequisitionId, adminId, "Returned for Revision", reason);
                Notify(rf.RequesterId, rf.MrsNo + " was returned for revision. Reason: " + reason, "RequestDetails.aspx?id=" + rf.RequisitionId);
                db.SaveChanges();
                tx.Commit();
            }
        }

        // ------------------------------------------------------------ 2c. Requester revises the RF and sends it back to the admin
        public void Resubmit(int requisitionId, int userId, CreateRequisitionVM vm)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                var rf = db.Requisitions.Find(requisitionId);
                if (rf == null || rf.RequesterId != userId || rf.Status != "For Revision")
                    throw new InvalidOperationException("This requisition cannot be edited.");

                var now = DateTime.Now;
                rf.JobOrderNo = vm.JobOrderNo;
                rf.Purpose = vm.Purpose;
                rf.DateNeeded = vm.DateNeeded;

                db.RequisitionLines.RemoveRange(rf.Lines.ToList());
                foreach (var g in vm.Lines.Where(l => l.ItemId > 0 && l.Quantity > 0).GroupBy(l => l.ItemId))
                    db.RequisitionLines.Add(new RequisitionLine { RequisitionId = rf.RequisitionId, ItemId = g.Key, QtyRequested = g.Sum(x => x.Quantity) });

                rf.Status = "Pending Approval";
                rf.ApprovalStatus = "Pending";
                rf.NoteComments = null;
                rf.RevisionCount++;
                rf.UpdatedAt = now;

                Log(rf.RequisitionId, userId, "RF Revised", "Revised and resubmitted (revision #" + rf.RevisionCount + ").");
                NotifyRole("Admin", rf.MrsNo + " was revised and resubmitted for your review.", "RequestDetails.aspx?id=" + rf.RequisitionId);
                db.SaveChanges();
                tx.Commit();
            }
        }

        // ------------------------------------------------------------ 2d. Requester deletes the request completely (before approval)
        public void DeleteRequisition(int requisitionId, int userId)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                var rf = db.Requisitions.Find(requisitionId);
                if (rf == null || rf.RequesterId != userId || (rf.Status != "For Revision" && rf.Status != "Pending Approval"))
                    throw new InvalidOperationException("This requisition cannot be deleted.");
                if (rf.Slips.Any() || rf.Replenishments.Any())
                    throw new InvalidOperationException("This requisition already has stock records and cannot be deleted.");

                string mrs = rf.MrsNo;
                string link = "RequestDetails.aspx?id=" + rf.RequisitionId;

                db.RequisitionLines.RemoveRange(rf.Lines.ToList());
                db.Notifications.RemoveRange(db.Notifications.Where(n => n.Link == link).ToList());
                db.Requisitions.Remove(rf);

                Log(null, userId, "Deleted", "Requisition " + mrs + " was deleted completely by the requester.");
                NotifyRole("Admin", mrs + " was deleted by the requester.", "Requests.aspx");
                db.SaveChanges();
                tx.Commit();
            }
        }

        // ------------------------------------------------------------ 3. Release items (requester verifies/signs IS) -> AR -> update inventory
        public void Release(int slipId, string receivedBy, int inventoryUserId)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                var slip = db.IssuanceSlips.Find(slipId);
                if (slip == null) throw new InvalidOperationException("Issuance slip not found.");
                if (slip.ReleasedAt != null) throw new InvalidOperationException("This slip was already released.");

                var rf = db.Requisitions.Find(slip.RequisitionId);
                var now = DateTime.Now;

                foreach (var sl in slip.Lines.ToList())
                {
                    var item = db.Items.Find(sl.ItemId);
                    if (item.Quantity < sl.Qty)
                        throw new InvalidOperationException("Not enough physical stock of " + item.ItemName + " to release.");

                    item.Quantity -= sl.Qty;        // system deducts issued quantity from stock
                    item.ReservedQty -= sl.Qty;
                    if (item.ReservedQty < 0) item.ReservedQty = 0;

                    var line = db.RequisitionLines.Find(sl.LineId);
                    line.QtyIssued += sl.Qty;
                    line.QtyReserved -= sl.Qty;
                }

                slip.ReleasedAt = now;
                slip.ReleasedById = inventoryUserId;
                slip.ReceivedBy = receivedBy;
                slip.ArNo = "AR-" + now.Year + "-" + slip.SlipId.ToString("D5");
                rf.UpdatedAt = now;

                bool allIssued = rf.Lines.All(l => l.QtyIssued >= l.QtyRequested);
                string link = "RequestDetails.aspx?id=" + rf.RequisitionId;

                if (allIssued)
                {
                    rf.Status = "Completed";            // mark fulfilled
                    rf.IsArchived = true;               // archive linked records
                    Log(rf.RequisitionId, inventoryUserId, "Completed", "All items issued. " + slip.IsNo + " / " + slip.ArNo + " received by " + receivedBy);
                    Notify(rf.RequesterId, rf.MrsNo + " is COMPLETED. All items released.", link);
                }
                else
                {
                    Log(rf.RequisitionId, inventoryUserId, "Partial Release", slip.IsNo + " / " + slip.ArNo + " released to " + receivedBy + ". Balance still pending.");
                    Notify(rf.RequesterId, "Partial release for " + rf.MrsNo + ". Remaining items are awaiting stock.", link);
                }
                db.SaveChanges();
                tx.Commit();
            }
        }

        // ------------------------------------------------------------ 4. Purchasing receives additional stock -> reserve for the pending RF
        public void ReceiveStock(int repId, int qtyReceived, int userId)
        {
            if (qtyReceived <= 0) throw new InvalidOperationException("Quantity received must be greater than zero.");

            using (var tx = db.Database.BeginTransaction())
            {
                var rep = db.ReplenishmentRequests.Find(repId);
                if (rep == null || rep.Status != "Pending")
                    throw new InvalidOperationException("This replenishment request is not pending.");

                var now = DateTime.Now;
                var item = db.Items.Find(rep.ItemId);
                var line = db.RequisitionLines.Find(rep.LineId);
                var rf = db.Requisitions.Find(rep.RequisitionId);

                item.Quantity += qtyReceived;                         // stock-in
                int reserve = Math.Min(qtyReceived, rep.QtyShort);    // allocate to the waiting requisition
                int available = Math.Max(0, item.Quantity - item.ReservedQty);
                reserve = Math.Min(reserve, available);

                item.ReservedQty += reserve;
                line.QtyReserved += reserve;
                rep.QtyReceived += qtyReceived;
                rep.QtyShort -= reserve;
                if (rep.QtyShort <= 0) { rep.Status = "Received"; rep.ReceivedAt = now; }

                IssuanceSlip slip = null;
                if (reserve > 0)
                {
                    slip = new IssuanceSlip { IsNo = TempNo(), RequisitionId = rf.RequisitionId, GeneratedAt = now };
                    slip.Lines.Add(new IssuanceSlipLine { ItemId = item.ItemId, LineId = line.LineId, Qty = reserve });
                    db.IssuanceSlips.Add(slip);
                }
                db.SaveChanges();
                if (slip != null) slip.IsNo = "IS-" + now.Year + "-" + slip.SlipId.ToString("D5");

                bool stillPending = db.ReplenishmentRequests.Any(r => r.RequisitionId == rf.RequisitionId && r.Status == "Pending");
                rf.ReplenishmentStatus = stillPending ? "Pending" : "Received";
                if (!stillPending && rf.Status == "Pending - Awaiting Stock") rf.Status = "Ready for Release";
                rf.UpdatedAt = now;

                string link = "RequestDetails.aspx?id=" + rf.RequisitionId;
                Log(rf.RequisitionId, userId, "Stock Received", qtyReceived + " x " + item.ItemName + " received (" + rep.RepNo + ").");
                NotifyRole("Inventory", "Stock received for " + rf.MrsNo + ". " + (slip != null ? "New issuance slip " + slip.IsNo + " ready for release." : ""), link);
                Notify(rf.RequesterId, "Additional stock received for " + rf.MrsNo + (stillPending ? " (some items still pending)." : ". Ready for release."), link);
                db.SaveChanges();
                tx.Commit();
            }
        }

        // ------------------------------------------------------------ 5. Additional stock NOT received -> reminder to Purchasing
        public void SendReminder(int repId, int userId)
        {
            var rep = db.ReplenishmentRequests.Find(repId);
            if (rep == null || rep.Status != "Pending") throw new InvalidOperationException("Request is not pending.");
            rep.ReminderCount++;
            rep.LastReminderAt = DateTime.Now;
            NotifyRole("Purchasing", "REMINDER: " + rep.RepNo + " is still waiting for stock (reminder #" + rep.ReminderCount + ").", "Replenishment.aspx");
            Log(rep.RequisitionId, userId, "Reminder Sent", "Reminder sent to Purchasing for " + rep.RepNo);
            db.SaveChanges();
        }
    }
}
