using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("admin_staff")]
    public class User
    {
        [Key]
        public int id { get; set; }

        public string admin_id { get; set; } = null!;
        public string phone { get; set; } = null!;
        public string nickname { get; set; } = null!;
        public string? gender { get; set; }
        public string? address { get; set; }
        //public string role { get; set; } = "普通用户";
        public string status { get; set; } = "启用";
        //public string? password { get; set; }
        //public DateTime? loginTime { get; set; }
        //public DateTime registerTime { get; set; } = DateTime.Now;
        //public DateTime? updateTime { get; set; }
        //public string? WxOpenId { get; set; }
        //public string? WxImage { get; set; }
    }
}
