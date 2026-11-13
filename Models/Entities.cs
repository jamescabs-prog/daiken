using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drafters.Models
{
    [Table("Users")]
    public class User
    {
        [Key] public int UserId { get; set; }
        [Required, StringLength(20)] public string EmployeeId { get; set; }
        [Required, StringLength(100)] public string FullName { get; set; }
        [Required, StringLength(50)] public string Username { get; set; }
        [Required, StringLength(64)] public string PasswordHash { get; set; }
        [Required, StringLength(20)] public string Role { get; set; }
        [StringLength(100)] public string Department { get; set; }
        public bool IsActive { get; set; }
    }

    [Table("Items")]
    public class Item
    {
        [Key] public int ItemId { get; set; }
        [Required, StringLength(30), Display(Name = "Item Code")] public string ItemCode { get; set; }
        [Required, StringLength(150), Display(Name = "Item Name")] public string ItemName { get; set; }
        [Required, StringLength(20)] public string Unit { get; set; }
        [ConcurrencyCheck, Range(0, 1000000), Display(Name = "Qty On Hand")] public int Quantity { get; set; }
        [ConcurrencyCheck] public int ReservedQty { get; set; }
        [Range(0, 1000000), Display(Name = "Reorder Level")] public int ReorderLevel { get; set; }
        [Display(Name = "Active")] public bool IsActive { get; set; }

        [NotMapped] public int Available { get { return Quantity - ReservedQty; } }
    }

    // Named RequisitionForm (RF) because "Requisition" is the project namespace.
    [Table("Requisitions")]
    public class RequisitionForm
    {
        public RequisitionForm()
        {
            Lines = new List<RequisitionLine>();
            Slips = new List<IssuanceSlip>();
            Replenishments = new List<ReplenishmentRequest>();
        }

        [Key] public int RequisitionId { get; set; }
        [StringLength(30)] public string MrsNo { get; set; }
        [StringLength(30)] public string JobOrderNo { get; set; }
        public int RequesterId { get; set; }
        [ForeignKey("RequesterId")] public virtual User Requester { get; set; }
        [StringLength(100)] public string Department { get; set; }
        [StringLength(500)] public string Purpose { get; set; }
        public DateTime DateNeeded { get; set; }
        [StringLength(30)] public string Status { get; set; }
        [StringLength(20)] public string ApprovalStatus { get; set; }
        public int? ApprovedById { get; set; }
        [ForeignKey("ApprovedById")] public virtual User ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        [StringLength(500)] public string NoteComments { get; set; }
        [StringLength(20)] public string ReplenishmentStatus { get; set; }
        public bool IsArchived { get; set; }
        public int RevisionCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<RequisitionLine> Lines { get; set; }
        public virtual ICollection<IssuanceSlip> Slips { get; set; }
        public virtual ICollection<ReplenishmentRequest> Replenishments { get; set; }
    }

    [Table("RequisitionItems")]
    public class RequisitionLine
    {
        [Key] public int LineId { get; set; }
        public int RequisitionId { get; set; }
        [ForeignKey("RequisitionId")] public virtual RequisitionForm Form { get; set; }
        public int ItemId { get; set; }
        [ForeignKey("ItemId")] public virtual Item Item { get; set; }
        public int QtyRequested { get; set; }
        public int QtyReserved { get; set; }
        public int QtyIssued { get; set; }
    }

    [Table("IssuanceSlips")]
    public class IssuanceSlip
    {
        public IssuanceSlip() { Lines = new List<IssuanceSlipLine>(); }
        [Key] public int SlipId { get; set; }
        [StringLength(30)] public string IsNo { get; set; }
        [StringLength(30)] public string ArNo { get; set; }
        public int RequisitionId { get; set; }
        [ForeignKey("RequisitionId")] public virtual RequisitionForm Form { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public int? ReleasedById { get; set; }
        [ForeignKey("ReleasedById")] public virtual User ReleasedBy { get; set; }
        [StringLength(100)] public string ReceivedBy { get; set; }
        public virtual ICollection<IssuanceSlipLine> Lines { get; set; }
    }

    [Table("IssuanceSlipLines")]
    public class IssuanceSlipLine
    {
        [Key] public int SlipLineId { get; set; }
        public int SlipId { get; set; }
        public int ItemId { get; set; }
        [ForeignKey("ItemId")] public virtual Item Item { get; set; }
        public int LineId { get; set; }
        public int Qty { get; set; }
    }

    [Table("ReplenishmentRequests")]
    public class ReplenishmentRequest
    {
        [Key] public int RepId { get; set; }
        [StringLength(30)] public string RepNo { get; set; }
        public int RequisitionId { get; set; }
        [ForeignKey("RequisitionId")] public virtual RequisitionForm Form { get; set; }
        public int LineId { get; set; }
        public int ItemId { get; set; }
        [ForeignKey("ItemId")] public virtual Item Item { get; set; }
        public int QtyShort { get; set; }
        public int QtyReceived { get; set; }
        [StringLength(20)] public string Status { get; set; }
        public int ReminderCount { get; set; }
        public DateTime? LastReminderAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
    }

    [Table("Notifications")]
    public class Notification
    {
        [Key] public int NotifId { get; set; }
        public int UserId { get; set; }
        [StringLength(300)] public string Message { get; set; }
        [StringLength(200)] public string Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [Table("TransactionLogs")]
    public class TransactionLog
    {
        [Key] public int TransactionId { get; set; }
        public int? RequisitionId { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")] public virtual User User { get; set; }
        [StringLength(50)] public string Action { get; set; }
        [StringLength(400)] public string Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
