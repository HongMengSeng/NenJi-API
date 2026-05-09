using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace WebAPI.Entities;

/// <summary>
/// 券品表
/// </summary>
[Table("coupon")]
[Index("CouponCode", Name = "idx_coupon_code")]
[Index("CreatedAt", Name = "idx_coupon_created_at")]
public class Coupon
{
    /// <summary>
    /// 券品ID (自增)
    /// </summary>
    [Key]
    [Column("coupon_id")]
    public int CouponId { get; set; }

    /// <summary>
    /// 券品编码 (Q + yyyyMMddHHmmss + 两位序号, 如: Q20260409103001)
    /// </summary>
    [Column("coupon_code")]
    [MaxLength(20)]
    [Required]
    public string CouponCode { get; set; } = string.Empty;

    /// <summary>
    /// 券品名称
    /// </summary>
    [Column("name")]
    [MaxLength(100)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 券品类型: 采摘券 / 研学活动券
    /// </summary>
    [Column("type")]
    [MaxLength(50)]
    [Required]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 售价 (精确到小数点后两位)
    /// </summary>
    [Column("price")]
    [Precision(10, 2)]
    public decimal Price { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [Column("stock")]
    public int Stock { get; set; }

    /// <summary>
    /// 单次限购数量
    /// </summary>
    [Column("limit_per_order")]
    public int LimitPerOrder { get; set; }

    /// <summary>
    /// 有效期数值
    /// </summary>
    [Column("validity_period")]
    public int ValidityPeriod { get; set; }

    /// <summary>
    /// 有效期单位: 天 / 月 / 年
    /// </summary>
    [Column("validity_unit")]
    [MaxLength(10)]
    [Required]
    public string ValidityUnit { get; set; } = "天";

    /// <summary>
    /// 退款规则
    /// </summary>
    [Column("refund_rule")]
    [MaxLength(255)]
    [Required]
    public string RefundRule { get; set; } = string.Empty;

    /// <summary>
    /// 使用规则
    /// </summary>
    [Column("usage_rules")]
    [MaxLength(1000)]
    [Required]
    public string UsageRules { get; set; } = string.Empty;

    /// <summary>
    /// 封面图URL
    /// </summary>
    [Column("image_url")]
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    //// 导航属性
    //public virtual ICollection<CouponMaterial> CouponMaterials { get; set; } = [];
    //public virtual CouponStatistic? CouponStatistic { get; set; }
}