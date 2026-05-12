using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ManageAPI.Entity;

namespace ManageAPI.Entity;

[Table("activity")]
public class ActivityEntity
{
    [Key]
    [Column("activity_id")]
    public long ActivityId { get; set; }

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("price")]
    public decimal Price { get; set; } // ���ݿ��� decimal(10,2)��������� decimal

    [Column("start_date")]
    public DateTime StartDate { get; set; } // ���ݿ��� datetime

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; } = null!;

    [Column("status_id")]
    public int StatusId { get; set; } // ��Ӧ���ݿ� status_id

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("type_id")]
    public int TypeId { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [Column("limit_per_order")]
    public int LimitPerOrder { get; set; }

    [Column("refund_rule")]
    [MaxLength(100)]
    public string? RefundRule { get; set; }

    [Column("usage_rules")]
    [MaxLength(500)]
    public string? UsageRules { get; set; }

    [InverseProperty("Activity")] // ��Ӧ ActivityMaterial ��� Activity ����
    public virtual ICollection<ActivityMaterial> ActivityMaterials { get; set; } = new List<ActivityMaterial>();

    [InverseProperty("Activity")] // ��Ӧ ActivityOrderDetail ����� Activity ������
    public virtual ICollection<ActivityOrderDetail> ActivityOrderDetails { get; set; } = new List<ActivityOrderDetail>();
}
