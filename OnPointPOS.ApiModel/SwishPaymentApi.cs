using POSSUM.Model;
using System;
using System.Collections.Generic;

namespace POSSUM.ApiModel
{
    public class SwishPaymentApi
    {
        public Guid Id { get; set; }
        public string SwishId { get; set; }
        public string SwishPaymentId { get; set; }
        public int SwishPaymentStatus { get; set; }
        public string SwishResponse { get; set; }
        public string SwishLocation { get; set; }
        public Guid? OrderId { get; set; }

        public static SwishPaymentApi ConvertModelToApiModel(SwishPayment swishPayment)
        {
            return new SwishPaymentApi
            {
                Id = swishPayment.Id,
                SwishId = swishPayment.SwishId,
                SwishPaymentId = swishPayment.SwishPaymentId,
                SwishPaymentStatus = swishPayment.SwishPaymentStatus,
                SwishResponse = swishPayment.SwishResponse,
                SwishLocation = swishPayment.SwishLocation,
                OrderId = swishPayment.OrderId
            };
        }

        public static SwishPayment ConvertApiModelToModel(SwishPaymentApi swishPayment)
        {
            return new SwishPayment
            {
                Id = swishPayment.Id,
                SwishId = swishPayment.SwishId,
                SwishPaymentId = swishPayment.SwishPaymentId,
                SwishPaymentStatus = swishPayment.SwishPaymentStatus,
                SwishResponse = swishPayment.SwishResponse,
                SwishLocation = swishPayment.SwishLocation,
                OrderId = swishPayment.OrderId
            };
        }

        public static SwishPayment UpdateModel(SwishPayment dbObject, SwishPaymentApi swishPayment)
        {
            dbObject.SwishId = swishPayment.SwishId;
            dbObject.SwishPaymentId = swishPayment.SwishPaymentId;
            dbObject.SwishPaymentStatus = swishPayment.SwishPaymentStatus;
            dbObject.SwishResponse = swishPayment.SwishResponse;
            dbObject.SwishLocation = swishPayment.SwishLocation;

            return dbObject;
        }
    }
}
