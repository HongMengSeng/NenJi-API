using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace WebAPI.Entities;

/// <summary>
/// 券品统计表 - 维护已售数量和已核销数量
/// </summary>
[Table("coupon_statistic")]
[Index("CouponId", Name = "idx_coupon_statistic_coupon_id")]
public class CouponStatistic
{
    [Key]
    [Column("stat_id")]
    public int StatId { get; set; }

    /// <summary>
    /// 券品ID
    /// </summary>
    [Column("coupon_id")]
    public int CouponId { get; set; }

    /// <summary>
    /// 已售数量
    /// </summary>
    [Column("sold_count")]
    public int SoldCount { get; set; }

    /// <summary>
    /// 已核销数量
    /// </summary>
    [Column("verified_count")]
    public int VerifiedCount { get; set; }

    [ForeignKey("CouponId")]
    public virtual Coupon? Coupon { get; set; }
}