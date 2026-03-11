using System;

namespace WebApplication1.Models.Entities
{
    /// <summary>
    /// 对应 order 表的主订单信息，用于订单列表与详情展示。
    /// </summary>
    public class OrderMain
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public int UserId { get; set; }
        public decimal? ActualPayment { get; set; }
        public int AddressId { get; set; }
        public int? OrderType { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? OrderStatus { get; set; }
        public int? PaymentStatus { get; set; }
        public int? DeliveryMethods { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactNumber { get; set; }
        public DateTime? OrderCreateTime { get; set; }
        public DateTime? PaymentTime { get; set; }
        public int? PaymentMethods { get; set; }
        public int? OrderFormId { get; set; }
        public string? SnapshotReceiverName { get; set; }
        public string? SnapshotReceiverPhone { get; set; }
        public string? SnapshotDeliveryAddress { get; set; }
        public string? SnapshotUserNickname { get; set; }
    }
}

