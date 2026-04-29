using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Common;
using WebAPI.Data;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IInventoryStatsService _inventoryStatsService;

    private static readonly string[] FunctionColors =
    [
        "#4E8B3A",
        "#FF8A3D",
        "#2F7D8C",
        "#C66B3D"
    ];

    public HomeController(AppDbContext dbContext, IInventoryStatsService inventoryStatsService)
    {
        _dbContext = dbContext;
        _inventoryStatsService = inventoryStatsService;
    }

    [HttpGet]
    public Task<IActionResult> GetHomePage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        return BuildHomeResponseAsync(page, pageSize, cancellationToken);
    }

    [HttpGet("index")]
    public Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        return BuildHomeResponseAsync(page, pageSize, cancellationToken);
    }

    [HttpGet("video")]
    public async Task<IActionResult> GetHomeVideo(CancellationToken cancellationToken)
    {
        var items = await LoadHomeVideosAsync(cancellationToken);
        return Ok(ApiResult.Success(new { items }));
    }

    private async Task<IActionResult> BuildHomeResponseAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        try
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 6 : pageSize;

            var homeSwiperRows = await _dbContext.Carousels
                .AsNoTracking()
                .Where(x => x.Position == "home")
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CarouselId)
                .Select(x => new
                {
                    x.CarouselId,
                    x.ImageUrl,
                    x.LinkUrl
                })
                .ToListAsync(cancellationToken);

            var homeSwiperList = homeSwiperRows.Select(x => new SwiperItem
            {
                Id = (int)x.CarouselId,
                Image = NormalizeMediaUrl(x.ImageUrl) ?? string.Empty,
                LinkUrl = x.LinkUrl ?? string.Empty,
                Title = string.Empty
            }).ToList();

            var homeVideos = await LoadHomeVideosAsync(cancellationToken);

            var allFarmGoods = await LoadCommodityCardsAsync(
                _dbContext.Commodities
                    .AsNoTracking()
                    .Where(x => (x.ProductStatus ?? 0) == 1)
                    .OrderByDescending(x => x.CommodityId),
                cancellationToken);

            var hotDishRows = await _dbContext.Dishes
                .AsNoTracking()
                .Where(x => x.Status == 1)
                .OrderByDescending(x => x.DishSold)
                .ThenByDescending(x => x.DishId)
                .Select(x => new
                {
                    x.DishId,
                    x.DishName,
                    x.ImageUrl,
                    x.DishPrice,
                    x.AttributeName
                })
                .ToListAsync(cancellationToken);

            var dishStats = await _inventoryStatsService.GetDishStatsAsync(
                hotDishRows.Select(x => x.DishId),
                cancellationToken);

            var allHotDishes = hotDishRows.Select(x => new HotDishItem
            {
                Id = x.DishId,
                Name = x.DishName,
                Image = NormalizeMediaUrl(x.ImageUrl) ?? string.Empty,
                Price = x.DishPrice,
                Sold = dishStats.GetValueOrDefault(x.DishId)?.Sold ?? 0,
                Stock = dishStats.GetValueOrDefault(x.DishId)?.Stock ?? 0,
                Tags = string.IsNullOrWhiteSpace(x.AttributeName)
                    ? new List<string>()
                    : new List<string> { x.AttributeName }
            })
            .OrderByDescending(x => x.Sold)
            .ThenByDescending(x => x.Id)
            .ToList();

            var farmGoods = allFarmGoods
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var hotDishes = allHotDishes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var data = new HomeIndexResponse
            {
                SwiperList = page == 1 ? homeSwiperList : [],
                FunctionButtons = page == 1 ? BuildFunctionButtons() : [],
                Videos = page == 1 ? homeVideos : [],
                FarmGoods = farmGoods,
                HotDishes = hotDishes,
                HasMore = page * pageSize < Math.Max(allFarmGoods.Count, allHotDishes.Count)
            };

            return Ok(ApiResult.Success(data));
        }
        catch (Exception ex)
        {
            return Ok(ApiResult.Fail($"获取首页数据失败：{ex.Message}"));
        }
    }

    private async Task<List<FarmGoodsItem>> LoadCommodityCardsAsync(
        IQueryable<Commodity> query,
        CancellationToken cancellationToken)
    {
        var commodities = await query.ToListAsync(cancellationToken);

        var commodityIds = commodities
            .Select(x => x.CommodityId)
            .Distinct()
            .ToList();

        var tags = await LoadCommodityTagsAsync(commodityIds, cancellationToken);

        var commodityStats = await _inventoryStatsService.GetCommodityStatsAsync(
            commodityIds,
            cancellationToken);

        return commodities.Select(x =>
        {
            var price = x.UnitPrice ?? ResolveCommodityPrice(x.ProductName);

            return new FarmGoodsItem
            {
                Id = x.CommodityId,
                Name = x.ProductName ?? string.Empty,
                Image = NormalizeMediaUrl(x.ImageUrl) ?? string.Empty,
                Price = price,
                OriginalPrice = x.OriginalPrice ?? price + 3m,
                Tags = tags.TryGetValue(x.CommodityId, out var itemTags) ? itemTags : [],
                Sold = commodityStats.GetValueOrDefault(x.CommodityId)?.Sold ?? 0,
                Stock = commodityStats.GetValueOrDefault(x.CommodityId)?.Stock ?? (x.Quantity ?? 0)
            };
        }).ToList();
    }

    private async Task<List<HomeVideoItem>> LoadHomeVideosAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.Videos
            .AsNoTracking()
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.VideoUrl))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.VideoId)
            .Select(x => new
            {
                x.VideoId,
                x.VideoUrl
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new HomeVideoItem
            {
                Id = (int)x.VideoId,
                CoverImage = string.Empty,
                VideoUrl = NormalizeMediaUrl(x.VideoUrl) ?? string.Empty
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.VideoUrl))
            .ToList();
    }

    private static List<FunctionButton> BuildFunctionButtons()
    {
        return
        [
            new FunctionButton
            {
                Id = 1,
                Name = "农场优选",
                Color = FunctionColors[0],
                Path = "/pages/farm-goods/farm-goods"
            },
            new FunctionButton
            {
                Id = 2,
                Name = "热销菜品",
                Color = FunctionColors[1],
                Path = "/pages/dish/dish"
            },
            new FunctionButton
            {
                Id = 3,
                Name = "活动报名",
                Color = FunctionColors[2],
                Path = "/pages/activity/activity"
            },
            new FunctionButton
            {
                Id = 4,
                Name = "购物车",
                Color = FunctionColors[3],
                Path = "/pages/cart/cart"
            }
        ];
    }

    private static decimal ResolveCommodityPrice(string? productName)
    {
        return productName switch
        {
            "甜养玉米500g" => 8.90m,
            "农家土豆500g" => 5.90m,
            "生态草莓礼盒" => 29.90m,
            "五谷杂粮礼盒" => 49.90m,
            _ => 19.90m
        };
    }

    private async Task<Dictionary<int, List<string>>> LoadCommodityTagsAsync(
        IReadOnlyCollection<int> commodityIds,
        CancellationToken cancellationToken)
    {
        if (commodityIds.Count == 0)
        {
            return [];
        }

        var rows = await (
            from relation in _dbContext.CommodityTagRelations.AsNoTracking()
            join tag in _dbContext.Tags.AsNoTracking()
                on relation.TagId equals tag.TagId
            where commodityIds.Contains(relation.CommodityId)
            select new
            {
                relation.CommodityId,
                tag.TagName
            }
        ).ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.CommodityId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(x => x.TagName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList());
    }

    private string? NormalizeMediaUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var trimmed = url.Trim();

        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        // 数据库里如果是 /images/farm/Farm_15.jpg
        // 直接返回 http://localhost:xxxx/images/farm/Farm_15.jpg
        if (trimmed.StartsWith("/"))
        {
            return $"{baseUrl}{trimmed}";
        }

        var ext = Path.GetExtension(trimmed).ToLowerInvariant();

        // 如果数据库里只存了 farm_intro.mp4
        if (ext is ".mp4" or ".mov" or ".avi" or ".mkv" or ".wmv")
        {
            return $"{baseUrl}/videos/{trimmed}";
        }

        // 如果数据库里只存了 Farm_15.jpg
        if (ext is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif")
        {
            return $"{baseUrl}/images/farm/{trimmed}";
        }

        return $"{baseUrl}/{trimmed}";
    }

    public sealed class SwiperItem
    {
        public int Id { get; set; }
        public string Image { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public sealed class FunctionButton
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public sealed class HomeVideoItem
    {
        public int Id { get; set; }
        public string CoverImage { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
    }

    public sealed class FarmGoodsItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public List<string> Tags { get; set; } = [];
        public int Sold { get; set; }
        public int Stock { get; set; }
    }

    public sealed class HotDishItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Sold { get; set; }
        public int Stock { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    public sealed class HomeIndexResponse
    {
        public List<SwiperItem> SwiperList { get; set; } = [];
        public List<FunctionButton> FunctionButtons { get; set; } = [];
        public List<HomeVideoItem> Videos { get; set; } = [];
        public List<FarmGoodsItem> FarmGoods { get; set; } = [];
        public List<HotDishItem> HotDishes { get; set; } = [];
        public bool HasMore { get; set; }
    }
}