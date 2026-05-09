using System.ComponentModel.DataAnnotations;

namespace WebAPI.Dtos;

/// <summary>
/// 新增/编辑餐桌
/// </summary>
public class CreateDiningTableDto
{
    [Required(ErrorMessage = "餐桌编号不能为空")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "区域不能为空")]
    public string Area { get; set; } = string.Empty;

    [Required(ErrorMessage = "类型不能为空")]
    public string Type { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "容纳人数必须大于0")]
    public int Capacity { get; set; }

    [RegularExpression("^(空闲|使用中|停用)$", ErrorMessage = "状态只能为空闲、使用中或停用")]
    public string Status { get; set; } = "空闲";

    public string? Detail { get; set; }
}

/// <summary>
/// 编辑餐桌
/// </summary>
public class UpdateDiningTableDto : CreateDiningTableDto
{
}

/// <summary>
/// 更新餐桌状态
/// </summary>
public class UpdateTableStatusDto
{
    [Required(ErrorMessage = "餐桌编号不能为空")]
    public string Id { get; set; } = string.Empty;

    [RegularExpression("^(空闲|使用中|停用)$", ErrorMessage = "状态只能为空闲、使用中或停用")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 删除餐桌请求
/// </summary>
public class DeleteDiningTableRequest
{
    [Required(ErrorMessage = "餐桌编号不能为空")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// 批量删除餐桌请求
/// </summary>
public class DeleteBatchDiningTableRequest
{
    [Required(ErrorMessage = "餐桌编号不能为空")]
    public string[] Ids { get; set; } = [];
}