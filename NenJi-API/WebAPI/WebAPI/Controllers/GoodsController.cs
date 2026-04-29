using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Common;
using WebAPI.Data;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/goods")]
public class GoodsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IInventoryStatsService _inventoryStatsService;

    public GoodsController(AppDbContext dbContext, IInventoryStatsService inventoryStatsService)
    {
        _dbContext = dbContext;
        _inventoryStatsService = inventoryStatsService;
    }

    [HttpGet("{id:int}")]
    public Task<IActionResult> DetailByRoute(int id, CancellationToken cancellationToken)
    {
        return BuildDetailResponseAsync(id, cancellationToken);
    }

    [HttpGet("detail")]
    public Task<IActionResult> Detail(
        [FromQuery(Name = "goodsId")] int? goodsId,
        [FromQuery(Name = "goods_id")] int? goodsIdAlias,
        CancellationToken cancellationToken)
    {
        return BuildDetailResponseAsync(goodsId ?? goodsIdAlias ?? 0, cancellationToken);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);
        keyword = (keyword ?? string.Empty).Trim();

        var query = _dbContext.Commodities.AsNoTracking().Where(x => (x.ProductStatus ?? 0) == 1);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.ProductName.Contains(keyword) || (x.SpecDescription ?? string.Empty).Contains(keyword));
        }

        var total = await query.CountAsync(cancellationToken);
        var commodities = await query
            .OrderByDescending(x => x.ProductName.Contains(keyword))
            .ThenBy(x => x.UnitPrice ?? 0m)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var ids = commodities.Select(x => x.CommodityId).ToList();
        var stats = await _inventoryStatsService.GetCommodityStatsAsync(ids, cancellationToken);

        var goodsList = commodities.Select(x =>
        {
            var stat = stats.GetValueOrDefault(x.CommodityId);
            return new
            {
                id = x.CommodityId.ToString(),
                name = x.ProductName,
                price = x.UnitPrice ?? 0m,
                originalPrice = x.OriginalPrice ?? (x.UnitPrice ?? 0m),
                image = NormalizeMediaUrl(x.ImageUrl),
                stock = stat?.Stock ?? (x.InStock ?? 0),
                sold = stat?.Sold ?? Math.Max(0, x.Quantity ?? 0),
                description = x.SpecDescription ?? string.Empty
            };
        }).ToList();

        return Ok(ApiResult.Success(new { goodsList, items = goodsList, total, page, pageSize }));
    }

    private async Task<IActionResult> BuildDetailResponseAsync(int goodsId, CancellationToken cancellationToken)
    {
        if (goodsId <= 0)
        {
            return Ok(ApiResult.Fail("goodsId is invalid", 400));
        }

        var commodity = await _dbContext.Commodities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CommodityId == goodsId && (x.ProductStatus ?? 0) == 1, cancellationToken);
        if (commodity is null)
        {
            return Ok(ApiResult.Fail("goods not found", 404));
        }

        var detailRows = await _dbContext.CommodityImages
            .AsNoTracking()
            .Where(x => x.CommodityId == goodsId)
            .OrderBy(x => x.SortOrder ?? int.MaxValue)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        var images = detailRows
            .Select(x => NormalizeMediaUrl(x.Url))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var mainImage = NormalizeMediaUrl(commodity.ImageUrl);
        if (!string.IsNullOrWhiteSpace(mainImage) && images.All(x => !x.Equals(mainImage, StringComparison.OrdinalIgnoreCase)))
        {
            images.Insert(0, mainImage);
        }

        var tags = await (
            from relation in _dbContext.CommodityTagRelations.AsNoTracking()
            join tag in _dbContext.Tags.AsNoTracking() on relation.TagId equals tag.TagId
            where relation.CommodityId == goodsId
            select tag.TagName
        ).Distinct().ToListAsync(cancellationToken);
        var stats = (await _inventoryStatsService.GetCommodityStatsAsync([goodsId], cancellationToken)).GetValueOrDefault(goodsId);
        var price = commodity.UnitPrice ?? 0m;
        var stock = stats?.Stock ?? (commodity.InStock ?? 0);
        var sold = stats?.Sold ?? Math.Max(0, commodity.Quantity ?? 0);
        var detailImage = images.FirstOrDefault() ?? mainImage;

        return Ok(ApiResult.Success(new
        {
            id = commodity.CommodityId.ToString(),
            name = commodity.ProductName,
            price,
            originalPrice = commodity.OriginalPrice ?? price,
            image = mainImage,
            mainImage,
            main_image = mainImage,
            detailImage,
            detail_image = detailImage,
            detailImages = images,
            detail_images = images,
            description = commodity.SpecDescription ?? string.Empty,
            desc = commodity.SpecDescription ?? string.Empty,
            weight = commodity.WeightText ?? string.Empty,
            storage = commodity.StorageCondition ?? string.Empty,
            videoUrl = string.Empty,
            sold,
            stock,
            tags,
            swiperList = images.Select((image, index) => new { id = index + 1, image }).ToList()
        }));
    }

    private static string NormalizeMediaUrl(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var value = raw.Trim();
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (value.StartsWith("/api/file/", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (value.StartsWith("api/file/", StringComparison.OrdinalIgnoreCase))
        {
            return $"/{value}";
        }

        var name = value.TrimStart('/');
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return ext is ".mp4" or ".mov" or ".avi" or ".mkv" or ".wmv"
            ? $"/api/file/video/{name}"
            : $"/api/file/image/{name}";
    }
}
