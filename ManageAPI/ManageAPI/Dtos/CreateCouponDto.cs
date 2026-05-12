namespace ManageAPI.Dtos;

/// <summary>
/// 创建/编辑券品
/// </summary>
public class CreateCouponDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int LimitPerOrder { get; set; }
    public int ValidityPeriod { get; set; }
    public string ValidityUnit { get; set; } = "天";
    public string RefundRule { get; set; } = string.Empty;
    public string UsageRules { get; set; } = string.Empty;
    public string? Image { get; set; }
    public List<CarouselMediaDto> CarouselMedia { get; set; } = [];
}

/// <summary>
/// 编辑券品
/// </summary>
public class UpdateCouponDto : CreateCouponDto
{
    public long Id { get; set; }
}
