using System;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Mvc;

namespace PublisherPortal.Controllers.ParameterModels
{
    public class InvokeMeterParameter
    {
        public Guid SubscriptionId { get; set; }

        public string PlanId { get; set; }

        public string DimensionId { get; set; }

        public int Quantity { get; set; }
    }
}
