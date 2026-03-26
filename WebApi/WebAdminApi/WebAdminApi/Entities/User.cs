using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAdminApi.Entities
{
    [Table("admin_staff")]
    public class AdminStaffs
    {
        [Key]
        public int id { get; set; }

        [Column("admin_id")]
        public int AdminId { get; set; }
        [Column("phone")]
        public string Phone { get; set; } = "Œ¥…Ë÷√";
        [Column("nickname")]
        public string NickName { get; set; } = "";
        [Column("gender")]
        public string Gender { get; set; } = "±£√‹";
        [Column("address")]
        public string Address { get; set; } = "Œ¥…Ë÷√";
        [Column("role_id")]    
        public int Role { get; set; }
        [Column("status")]
        public string Status { get; set; } = "∆Ù”√";
        [Column("register_time")]
        public DateTime RegisterTime { get; set; } = DateTime.Now;
        [Column("token")]
        public string Token { get; set; } = "";
    }

    [Table("users")]
    public class WeChatUser
    {
        [Key]
        public int user_id { get; set; }

        public string phone_number { get; set; } = "Œ¥…Ë÷√";
        public DateTime register_time { get; set; } = DateTime.Now;
        public string wx_open_id { get; set; } = "Œ¥…Ë÷√";
        public string wx_image { get; set; } = "Œ¥…Ë÷√";   
        public string wx_name { get; set; } = "Œ¥…Ë÷√";

        public int role_id { get; set; }

    }
}
