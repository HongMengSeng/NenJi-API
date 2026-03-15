using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Entities;

[Table("user")]
public class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("user_no")]
    [MaxLength(45)]
    public string UserNo { get; set; } = string.Empty;

    [Column("phone_number")]
    [MaxLength(45)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("register_time")]
    public DateTime RegisterTime { get; set; }

    [Column("wx_openid")]
    [MaxLength(255)]
    public string WxOpenId { get; set; } = string.Empty;

    [Column("wx_image")]
    [MaxLength(255)]
    public string WxImage { get; set; } = string.Empty;

    [Column("wx_name")]
    [MaxLength(45)]
    public string WxName { get; set; } = string.Empty;

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("gender")]
    [MaxLength(10)]
    public string? Gender { get; set; }

    [Column("status")]
    public int Status { get; set; }
}
