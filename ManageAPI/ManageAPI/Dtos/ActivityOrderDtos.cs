namespace ManageAPI.Dtos;

public class ActivityOrderListItemDto
{
    public long OrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
    public int OrderStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? ActivityTitle { get; set; }
    public string CreateTime { get; set; } = string.Empty;
}

public class ActivityOrderItemDto
{
    public long ActivityOrderDetailsId { get; set; }
    public long ActivityId { get; set; }
    public string ActivityTitle { get; set; } = string.Empty;
    public string? ActivityImage { get; set; }
    public string? ActivityDescription { get; set; }
    public string? ActivityLocation { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubtotalAmount { get; set; }
    public string? ActivityQrcode { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationTime { get; set; }
}

public class ActivityOrderFullDetailDto
{
    public long OrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string? WxPayNo { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
    public int OrderStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserPhone { get; set; }
    public string CreateTime { get; set; } = string.Empty;
    public List<ActivityOrderItemDto> Items { get; set; } = new();
}

public class VerifyActivityOrderRequest
{
    public long ActivityOrderDetailsId { get; set; }
}
