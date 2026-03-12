using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Entities;

[Table("order_details")]
public class OrderDetail
{
    [Key]
    [Column("order_details_id")]
    public long OrderDetailsId { get; set; }

    [Column("order_id")]
    public long OrderId { get; set; }

    [Column("commodity_id")]
    public int CommodityId { get; set; }

    [Column("actual_unit_price")]
    public decimal ActualUnitPrice { get; set; }

    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("purchase_quantity")]
    public int PurchaseQuantity { get; set; }

    [Column("subtotal_amount")]
    public decimal SubtotalAmount { get; set; }
}
