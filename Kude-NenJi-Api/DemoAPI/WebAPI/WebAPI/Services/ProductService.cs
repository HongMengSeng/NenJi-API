namespace WebAPI.Services;

using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Dtos;
using WebAPI.Entities;

/// <summary>
/// 产品管理服务
/// </summary>
public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;

    public ProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取产品列表
    /// </summary>
    public async Task<(List<ProductListItemDto> Records, int Total)> GetProductListAsync(
        int pageNum,
        int pageSize,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Commodities.AsNoTracking();

        // 搜索过滤：按ID或名称模糊搜索
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keywordTrimmed = keyword.Trim();
            query = query.Where(c => 
                (c.ProductName != null && c.ProductName.Contains(keywordTrimmed)) ||
                (c.CommodityId.ToString().Contains(keywordTrimmed)));
        }

        var total = await query.CountAsync(cancellationToken);

        var records = await query
            .OrderByDescending(c => c.CommodityId)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ProductListItemDto
            {
                Id = c.CommodityId,
                Name = c.ProductName ?? string.Empty,
                Price = c.UnitPrice ?? 0m,
                Stock = c.InStock ?? 0,
                Status = (c.ProductStatus ?? 0) == 1 ? "已上架" : "已下架",
                Image = c.ImageUrl ?? string.Empty,
                UploadTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            })
            .ToListAsync(cancellationToken);

        return (records, total);
    }

    /// <summary>
    /// 获取产品详情
    /// </summary>
    public async Task<ProductDetailDto?> GetProductDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var commodity = await _dbContext.Commodities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CommodityId == id, cancellationToken);

        if (commodity is null)
        {
            return null;
        }

        // 获取轮播图和规格图
        var images = await _dbContext.CommodityImages
            .AsNoTracking()
            .Where(i => i.CommodityId == id)
            .OrderBy(i => i.SortOrder ?? int.MaxValue)
            .ToListAsync(cancellationToken);

        var carouselMedia = images
            .Where(i => i.ImageType == 1) // 1=轮播图
            .Select(i => new CarouselMediaDto
            {
                Type = "image",
                Url = i.Url ?? string.Empty,
                Thumb = null
            })
            .ToList();

        var specImages = images
            .Where(i => i.ImageType == 2) // 2=规格图
            .Select(i => i.Url ?? string.Empty)
            .ToList();

        return new ProductDetailDto
        {
            Id = commodity.CommodityId,
            Name = commodity.ProductName ?? string.Empty,
            Price = commodity.UnitPrice ?? 0m,
            Stock = commodity.InStock ?? 0,
            Status = (commodity.ProductStatus ?? 0) == 1 ? "已上架" : "已下架",
            CoverImage = commodity.ImageUrl ?? string.Empty,
            CarouselMedia = carouselMedia,
            NetWeight = null, // 需要解析 weight_text
            WeightUnit = commodity.UnitName ?? string.Empty,
            StorageCondition = commodity.StorageCondition ?? string.Empty,
            SpecImages = specImages,
            Description = commodity.SpecDescription ?? string.Empty,
            UploadTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };
    }

    /// <summary>
    /// 新增产品
    /// </summary>
    public async Task<int> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var commodity = new Commodity
        {
            ProductName = dto.Name,
            UnitPrice = dto.Price,
            InStock = dto.Stock,
            ProductStatus = dto.Status == "已上架" ? 1 : 0,
            ImageUrl = dto.CoverImage,
            WeightText = dto.NetWeight?.ToString(),
            UnitName = dto.WeightUnit,
            StorageCondition = dto.StorageCondition,
            SpecDescription = dto.Description,
            CategoryId = 1 // 默认分类
        };

        _dbContext.Commodities.Add(commodity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 保存轮播图和规格图
        if (dto.CarouselMedia.Count > 0)
        {
            var carouselImages = dto.CarouselMedia
                .Select((m, index) => new CommodityImage
                {
                    CommodityId = commodity.CommodityId,
                    Url = m.Url,
                    SortOrder = index,
                    ImageType = 1 // 轮播图
                })
                .ToList();

            _dbContext.CommodityImages.AddRange(carouselImages);
        }

        if (dto.SpecImages.Count > 0)
        {
            var specImages = dto.SpecImages
                .Select((url, index) => new CommodityImage
                {
                    CommodityId = commodity.CommodityId,
                    Url = url,
                    SortOrder = index,
                    ImageType = 2 // 规格图
                })
                .ToList();

            _dbContext.CommodityImages.AddRange(specImages);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return commodity.CommodityId;
    }

    /// <summary>
    /// 编辑产品
    /// </summary>
    public async Task<bool> UpdateProductAsync(UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var commodity = await _dbContext.Commodities
            .FirstOrDefaultAsync(c => c.CommodityId == dto.Id, cancellationToken);

        if (commodity is null)
        {
            return false;
        }

        commodity.ProductName = dto.Name;
        commodity.UnitPrice = dto.Price;
        commodity.InStock = dto.Stock;
        commodity.ProductStatus = dto.Status == "已上架" ? 1 : 0;
        commodity.ImageUrl = dto.CoverImage;
        commodity.WeightText = dto.NetWeight?.ToString();
        commodity.UnitName = dto.WeightUnit;
        commodity.StorageCondition = dto.StorageCondition;
        commodity.SpecDescription = dto.Description;

        // 删除旧的轮播图和规格图
        var oldImages = await _dbContext.CommodityImages
            .Where(i => i.CommodityId == dto.Id)
            .ToListAsync(cancellationToken);

        _dbContext.CommodityImages.RemoveRange(oldImages);

        // 添加新的轮播图和规格图
        if (dto.CarouselMedia.Count > 0)
        {
            var carouselImages = dto.CarouselMedia
                .Select((m, index) => new CommodityImage
                {
                    CommodityId = commodity.CommodityId,
                    Url = m.Url,
                    SortOrder = index,
                    ImageType = 1
                })
                .ToList();

            _dbContext.CommodityImages.AddRange(carouselImages);
        }

        if (dto.SpecImages.Count > 0)
        {
            var specImages = dto.SpecImages
                .Select((url, index) => new CommodityImage
                {
                    CommodityId = commodity.CommodityId,
                    Url = url,
                    SortOrder = index,
                    ImageType = 2
                })
                .ToList();

            _dbContext.CommodityImages.AddRange(specImages);
        }

        _dbContext.Commodities.Update(commodity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 删除产品
    /// </summary>
    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var commodity = await _dbContext.Commodities
            .FirstOrDefaultAsync(c => c.CommodityId == id, cancellationToken);

        if (commodity is null)
        {
            return false;
        }

        // 删除关联的图片
        var images = await _dbContext.CommodityImages
            .Where(i => i.CommodityId == id)
            .ToListAsync(cancellationToken);

        _dbContext.CommodityImages.RemoveRange(images);
        _dbContext.Commodities.Remove(commodity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 批量删除产品
    /// </summary>
    public async Task<bool> DeleteProductBatchAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        var commodities = await _dbContext.Commodities
            .Where(c => ids.Contains(c.CommodityId))
            .ToListAsync(cancellationToken);

        if (commodities.Count == 0)
        {
            return false;
        }

        var commodityIds = commodities.Select(c => c.CommodityId).ToList();

        // 删除关联的图片
        var images = await _dbContext.CommodityImages
            .Where(i => commodityIds.Contains(i.CommodityId ?? 0))
            .ToListAsync(cancellationToken);

        _dbContext.CommodityImages.RemoveRange(images);
        _dbContext.Commodities.RemoveRange(commodities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}