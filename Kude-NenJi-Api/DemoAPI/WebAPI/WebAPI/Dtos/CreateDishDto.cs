using System.ComponentModel.DataAnnotations;

namespace WebAPI.Dtos;

/// <summary>
/// 新增/编辑菜品
/// </summary>
public class CreateDishDto
{
    [Required(ErrorMessage = "菜品名称不能为空")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "菜品价格必须大于0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "库存不能为负")]
    public int Stock { get; set; }

    [RegularExpression("^(已上架|已下架)$", ErrorMessage = "状态只能为已上架或已下架")]
    public string Status { get; set; } = "已下架";

    [Required(ErrorMessage = "封面图不能为空")]
    public string CoverImage { get; set; } = string.Empty;

    public List<CarouselMediaDto> CarouselMedia { get; set; } = [];

    public List<string> SpecImages { get; set; } = [];

    public string? Description { get; set; }
}

/// <summary>
/// 编辑菜品
/// </summary>
public class UpdateDishDto : CreateDishDto
{
    [Required(ErrorMessage = "菜品ID不能为空")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// 删除菜品请求
/// </summary>
public class DeleteDishRequest
{
    [Required(ErrorMessage = "菜品ID不能为空")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// 批量删除菜品请求
/// </summary>
public class DeleteBatchDishRequest
{
    [Required(ErrorMessage = "菜品ID不能为空")]
    public string[] Ids { get; set; } = [];
}