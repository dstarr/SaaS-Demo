using System;
using Microsoft.Marketplace.Metering.Models;

namespace PublisherPortal.ViewModels.Meters
{
    public class InvokeMeterViewModel
    {
        public Guid? SubscriptionId { get; set; }
        public double? Quantity { get; set; }
        public string DimensionId { get; set; }
        public string PlanId { get; set; }
        public UsageEventStatusEnum? ResultStatus { get; set; }
    }
}
