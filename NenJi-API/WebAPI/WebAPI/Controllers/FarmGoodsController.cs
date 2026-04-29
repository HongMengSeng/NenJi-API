using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Common;
using WebAPI.Data;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/farm-goods")]
public class FarmGoodsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IInventoryStatsService _inventoryStatsService;

    private static readonly string[] CategoryColors = ["#4CAF50", "#FF9800", "#2F7D8C", "#C66B3D", "#D94F70"];

    public FarmGoodsController(AppDbContext dbContext, IInventoryStatsService inventoryStatsService)
    {
        _dbContext = dbContext;
        _inventoryStatsService = inventoryStatsService;
    }

    [HttpGet]
    public Task<IActionResult> GetGoodsPage(
        [FromQuery] string category = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return BuildPagedGoodsResponseAsync(category, page, pageSize, includeCategories: true, cancellationToken);
    }

    [HttpGet("index")]
    public async Task<IActionResult> GetFarmGoodsIndex(CancellationToken cancellationToken)
    {
        var categories = await LoadCategoriesAsync(cancellationToken);
        var query = _dbContext.Commodities
            .AsNoTracking()
            .Where(x => (x.ProductStatus ?? 0) == 1)
            .OrderByDescending(x => x.CommodityId)
            .Take(12);
        var goods = await LoadGoodsCardsAsync(query, cancellationToken);
        var swiperList = await LoadGoodsSwiperAsync(cancellationToken);

        return Ok(ApiResult.Success(new
        {
            swiperList,
            categories,
            todayGoods = goods.Take(6).ToList(),
            hotGoods = goods.Skip(6).Take(6).ToList()
        }));
    }

    [HttpGet("category")]
    public Task<IActionResult> GetCategoryGoods(
        [FromQuery] string? categoryId,
        [FromQuery] string? category,
        [FromQuery] string? id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return BuildPagedGoodsResponseAsync(categoryId ?? category ?? id ?? "all", page, pageSize, includeCategories: false, cancellationToken);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await LoadCategoriesAsync(cancellationToken);
        return Ok(ApiResult.Success(new { categories, list = categories }));
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchGoods(
        [FromQuery] string keyword = "",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
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

        query = query.OrderByDescending(x => x.CommodityId);
        var total = await query.CountAsync(cancellationToken);
        var items = await LoadGoodsCardsAsync(query.Skip((page - 1) * pageSize).Take(pageSize), cancellationToken);

        return Ok(ApiResult.Success(new
        {
            keyword,
            items,
            goodsList = items,
            list = items,
            total,
            page,
            pageSize,
            hasMore = page * pageSize < total
        }));
    }

    private async Task<IActionResult> BuildPagedGoodsResponseAsync(string? category, int page, int pageSize, bool includeCategories, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var categories = await LoadCategoriesAsync(cancellationToken);
        var normalizedCategory = NormalizeCategory(category, categories);
        var query = _dbContext.Commodities.AsNoTracking().Where(x => (x.ProductStatus ?? 0) == 1);
        if (normalizedCategory != "all" && int.TryParse(normalizedCategory, out var categoryId))
        {
            query = query.Where(x => x.CategoryId == categoryId);
        }

        query = query.OrderByDescending(x => x.CommodityId);
        var total = await query.CountAsync(cancellationToken);
        var items = await LoadGoodsCardsAsync(query.Skip((page - 1) * pageSize).Take(pageSize), cancellationToken);

        return Ok(ApiResult.Success(new
        {
            category = normalizedCategory,
            categories = includeCategories ? categories : [],
            items,
            goodsList = items,
            list = items,
            page,
            pageSize,
            total,
            hasMore = page * pageSize < total
        }));
    }

    private async Task<List<object>> LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.Categories
            .AsNoTracking()
            .Where(x => (x.CategoryStatusId ?? 0) == 1)
            .OrderBy(x => x.SortOrder ?? int.MaxValue)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return rows.Select((x, index) => (object)new
        {
            id = x.Id.ToString(),
            name = x.CategoryName,
            color = CategoryColors[index % CategoryColors.Length],
            icon = string.IsNullOrWhiteSpace(x.CategoryName) ? string.Empty : x.CategoryName[..1]
        }).ToList();
    }

    private static string NormalizeCategory(string? category, IReadOnlyCollection<object> categories)
    {
        var value = string.IsNullOrWhiteSpace(category) ? "all" : category.Trim();
        if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return "all";
        }

        foreach (dynamic item in categories)
        {
            string id = item.id;
            string name = item.name;
            if (id.Equals(value, StringComparison.OrdinalIgnoreCase) || name.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return id;
            }
        }

        return value;
    }

    private async Task<List<object>> LoadGoodsCardsAsync(IQueryable<Commodity> query, CancellationToken cancellationToken)
    {
        var commodities = await query.ToListAsync(cancellationToken);
        var ids = commodities.Select(x => x.CommodityId).ToList();
        var stats = await _inventoryStatsService.GetCommodityStatsAsync(ids, cancellationToken);
        var tags = await LoadCommodityTagsAsync(ids, cancellationToken);

        return commodities.Select(x =>
        {
            var price = x.UnitPrice ?? 0m;
            var stat = stats.GetValueOrDefault(x.CommodityId);
            return (object)new
            {
                id = x.CommodityId.ToString(),
                name = x.ProductName,
                price,
                originalPrice = x.OriginalPrice ?? price,
                image = NormalizeMediaUrl(x.ImageUrl),
                stock = stat?.Stock ?? (x.InStock ?? 0),
                sold = stat?.Sold ?? Math.Max(0, x.Quantity ?? 0),
                tags = tags.GetValueOrDefault(x.CommodityId) ?? []
            };
        }).ToList();
    }

    private async Task<Dictionary<int, List<string>>> LoadCommodityTagsAsync(IReadOnlyCollection<int> commodityIds, CancellationToken cancellationToken)
    {
        if (commodityIds.Count == 0)
        {
            return [];
        }

        var rows = await (
            from relation in _dbContext.CommodityTagRelations.AsNoTracking()
            join tag in _dbContext.Tags.AsNoTracking() on relation.TagId equals tag.TagId
            where commodityIds.Contains(relation.CommodityId)
            select new { relation.CommodityId, tag.TagName }
        ).ToListAsync(cancellationToken);

        return rows.GroupBy(x => x.CommodityId).ToDictionary(g => g.Key, g => g.Select(x => x.TagName).Distinct().ToList());
    }

    private async Task<List<object>> LoadGoodsSwiperAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.Carousels
            .AsNoTracking()
            .Where(x => x.Position == "goods")
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CarouselId)
            .ToListAsync(cancellationToken);

        return rows.Select(x => (object)new { id = x.CarouselId, image = NormalizeMediaUrl(x.ImageUrl) }).ToList();
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
