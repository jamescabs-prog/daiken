using System;
using System.Collections.Generic;

namespace Drafters.Delivery
{
    /// <summary>
    /// Single data-access seam of the Delivery subsystem. Every Delivery page talks only to this interface.
    /// Today it is implemented in memory (InMemoryDeliveryRepository). For the MySQL phase, create a
    /// MySqlDeliveryRepository that implements this interface and change the one line in DeliveryData.
    /// </summary>
    public interface IDeliveryRepository
    {
        // ----- queries -----
        List<DeliveryRecord> AllDeliveries();
        DeliveryRecord Get(string id);
        List<DiscrepancyReport> AllDiscrepancies();
        DiscrepancyReport GetDiscrepancy(string id);
        DiscrepancyReport OpenDiscrepancyFor(string deliveryId);
        PurchaseOrder GetPo(string poNo);
        List<PurchaseOrder> PosWithoutNotice();
        int OnHand(string sku);

        // ----- alerts -----
        List<DeliveryAlert> AlertsFor(string role);
        int UnreadCount(string role);
        void MarkAllRead(string role);

        // ----- process steps (throw InvalidOperationException on a business-rule violation) -----
        DeliveryRecord CreateNotice(string poNo, DateTime expected, string notes, Dictionary<string, int> qty, string by);
        Receipt ReceiveDelivery(string id, string drNo, string receivedBy, int checksPassed, int checksTotal, List<ReceiptLine> lines, string by);
        void MarkInformed(string discrepancyId, bool supplier, bool client, string by);
        void SetResolution(string discrepancyId, string resolution, DateTime deadline, string by);
        void AddFollowUp(string discrepancyId, string note, string by);
        void Verify(string id, string by, string note);
        void PrepareDocs(string id, string by);
        void ReleaseItems(string id, string releasedTo, string signedBy, string signedRef, string by);
    }

    /// <summary>Gives every page the one shared repository. Swap the implementation here when MySQL is added.</summary>
    public static class DeliveryData
    {
        private static readonly IDeliveryRepository _repo = new InMemoryDeliveryRepository();
        public static IDeliveryRepository Repository { get { return _repo; } }
    }
}
