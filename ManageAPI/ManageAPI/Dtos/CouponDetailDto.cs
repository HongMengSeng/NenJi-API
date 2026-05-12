namespace ManageAPI.Dtos;

/// <summary>
/// 券品详情
/// </summary>
public class CouponDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int LimitPerOrder { get; set; }
    public int ValidityPeriod { get; set; }
    public string ValidityUnit { get; set; } = string.Empty;
    public string Validity { get; set; } = string.Empty;
    public string RefundRule { get; set; } = string.Empty;
    public string UsageRules { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string? ImageName { get; set; }
    public List<CarouselMediaDto> CarouselMedia { get; set; } = [];
    public int SoldCount { get; set; }
    public int VerifiedCount { get; set; }
    public string CreateTime { get; set; } = string.Empty;
}
