namespace WebAPI.Dtos;

/// <summary>
/// 产品列表项DTO
/// </summary>
public class ProductListItemDto
{
    /// <summary>
    /// 产品ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 产品价格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// 上架状态：已上架、已下架
    /// </summary>
    public string Status { get; set; } = "已下架";

    /// <summary>
    /// 列表主图
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// 上传/创建时间
    /// </summary>
    public string UploadTime { get; set; } = string.Empty;
}