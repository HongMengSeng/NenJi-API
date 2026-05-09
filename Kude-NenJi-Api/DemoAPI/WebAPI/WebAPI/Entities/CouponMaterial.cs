using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace WebAPI.Entities;

/// <summary>
/// 券品素材表 - 轮播图、规格图、视频
/// </summary>
[Table("coupon_material")]
[Index("CouponId", "MaterialType", "SortOrder", Name = "idx_coupon_material_type_sort")]
public class CouponMaterial
{
    [Key]
    [Column("material_id")]
    public long MaterialId { get; set; }

    /// <summary>
    /// 券品ID
    /// </summary>
    [Column("coupon_id")]
    public int CouponId { get; set; }

    /// <summary>
    /// 素材类型: carousel(轮播图) / spec(规格图) / video(视频)
    /// </summary>
    [Column("material_type")]
    [MaxLength(20)]
    [Required]
    public string MaterialType { get; set; } = "carousel";

    /// <summary>
    /// 素材URL
    /// </summary>
    [Column("material_url")]
    [MaxLength(500)]
    [Required]
    public string MaterialUrl { get; set; } = string.Empty;

    /// <summary>
    /// 视频缩略图URL (仅视频类型需要)
    /// </summary>
    [Column("thumb_url")]
    [MaxLength(500)]
    public string? ThumbUrl { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CouponId")]
    public virtual Coupon? Coupon { get; set; }
}