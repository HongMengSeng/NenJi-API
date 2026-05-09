namespace WebAPI.Dtos;

/// <summary>
/// 产品详情DTO
/// </summary>
public class ProductDetailDto
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
    /// 上架状态
    /// </summary>
    public string Status { get; set; } = "已下架";

    /// <summary>
    /// 封面图
    /// </summary>
    public string CoverImage { get; set; } = string.Empty;

    /// <summary>
    /// 轮播图/视频
    /// </summary>
    public List<CarouselMediaDto> CarouselMedia { get; set; } = [];

    /// <summary>
    /// 净含量数值
    /// </summary>
    public decimal? NetWeight { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string WeightUnit { get; set; } = string.Empty;

    /// <summary>
    /// 保存条件
    /// </summary>
    public string StorageCondition { get; set; } = string.Empty;

    /// <summary>
    /// 规格图片
    /// </summary>
    public List<string> SpecImages { get; set; } = [];

    /// <summary>
    /// 产品介绍
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 上传时间
    /// </summary>
    public string UploadTime { get; set; } = string.Empty;
}

/// <summary>
/// 轮播媒体
/// </summary>
public class CarouselMediaDto
{
    /// <summary>
    /// 类型：image/video
    /// </summary>
    public string Type { get; set; } = "image";

    /// <summary>
    /// 媒体URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 视频缩略图（仅视频需要）
    /// </summary>
    public string? Thumb { get; set; }
}