namespace WebApplication1.Models.Entities
{
    /// <summary>
    /// 对应 shipping_address 表，存储用户的收货地址信息。
    /// </summary>
    public class ShippingAddress
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public string ContentName { get; set; } = null!;
        public string Province { get; set; } = null!;
        public string City { get; set; } = null!;
        public string MunicipalDistricts { get; set; } = null!;
        public string? Town { get; set; }
        public string HouseNumber { get; set; } = null!;
    }
}

