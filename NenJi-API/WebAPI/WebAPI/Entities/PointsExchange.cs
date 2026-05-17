using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Entities;

[Table("points_exchange")]
public class PointsExchange
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("commodity_id")]
    public int CommodityId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("points_spent")]
    public int PointsSpent { get; set; }

    [Column("order_no")]
    [MaxLength(64)]
    public string OrderNo { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "completed";

    [Column("create_time")]
    public DateTime CreateTime { get; set; }
}
