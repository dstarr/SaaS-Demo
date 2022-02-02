using System;

namespace PublisherPortal.ViewModels.Meters
{
    public class InvokedMeterViewModel
    {
        public Guid SubscriptionId { get; set; }
        public int Quantity { get; set; }
        public string DimensionId { get; set; }
        public string PlanId { get; set; }
    }
}
