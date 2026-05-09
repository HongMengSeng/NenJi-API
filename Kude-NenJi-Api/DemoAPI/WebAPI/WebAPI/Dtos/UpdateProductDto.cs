namespace WebAPI.Dtos;

/// <summary>
/// 编辑产品DTO
/// </summary>
public class UpdateProductDto : CreateProductDto
{
    /// <summary>
    /// 产品ID
    /// </summary>
    public int Id { get; set; }
}