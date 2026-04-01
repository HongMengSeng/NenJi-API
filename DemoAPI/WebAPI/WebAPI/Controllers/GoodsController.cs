using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Common;
using WebAPI.Data;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/goods")]
public class GoodsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public GoodsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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

    private async Task<IActionResult> BuildDetailResponseAsync(int goodsId, CancellationToken cancellationToken)
    {
        try
        {
            if (goodsId <= 0)
            {
                return Ok(ApiResult.Fail("goodsId 参数不正确", 400));
            }

            var commodity = await _dbContext.Commodities
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CommodityId == goodsId && (x.ProductStatus ?? 0) == 1,
                    cancellationToken);

            if (commodity is null)
            {
                return Ok(ApiResult.Fail("商品不存在", 404));
            }

            var category = await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == commodity.CategoryId, cancellationToken);

            var tags = await (
                from relation in _dbContext.CommodityTagRelations.AsNoTracking()
                join tag in _dbContext.Tags.AsNoTracking() on relation.TagId equals tag.TagId
                where relation.CommodityId == goodsId
                select tag.TagName
            ).Distinct().ToListAsync(cancellationToken);

            var detailImageRows = await _dbContext.CommodityImages
                .AsNoTracking()
                .Where(x => x.CommodityId == goodsId)
                .OrderBy(x => x.SortOrder ?? int.MaxValue)
                .ThenBy(x => x.Id)
                .Select(x => new
                {
                    x.Url,
                    x.ImageType
                })
                .ToListAsync(cancellationToken);

            var normalizedDetailImages = detailImageRows
                .Select(x => NormalizeImageUrl(x.Url))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<string>()
                .ToList();

            var primaryImage = ResolvePrimaryImageUrl(commodity.ImageUrl, normalizedDetailImages);
            if (normalizedDetailImages.Count == 0 && !string.IsNullOrWhiteSpace(primaryImage))
            {
                normalizedDetailImages.Add(primaryImage);
            }

            var longDetailImages = detailImageRows
                .Where(x => x.ImageType == 2)
                .Select(x => NormalizeImageUrl(x.Url))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<string>()
                .Take(4)
                .ToList();

            if (longDetailImages.Count == 0)
            {
                longDetailImages = normalizedDetailImages.Take(4).ToList();
            }

            if (longDetailImages.Count == 0 && !string.IsNullOrWhiteSpace(primaryImage))
            {
                longDetailImages.Add(primaryImage);
            }

            var detailImage = ResolveDetailImageUrl(primaryImage, longDetailImages);
            var price = commodity.UnitPrice ?? 0m;
            var originalPrice = commodity.OriginalPrice ?? price;
            var description = commodity.SpecDescription ?? string.Empty;
            var stock = commodity.InStock ?? 0;
            var status = (commodity.ProductStatus ?? 0) == 1 && stock > 0 ? 1 : 0;
            var unit = commodity.UnitName ?? string.Empty;
            var weight = commodity.WeightText ?? string.Empty;
            var storage = commodity.StorageCondition ?? string.Empty;

            return Ok(ApiResult.Success(new
            {
                id = commodity.CommodityId,
                name = commodity.ProductName,
                price,
                originalPrice,
                stock,
                mainImage = primaryImage,
                main_image = primaryImage,
                desc = description,
                detailImages = longDetailImages,
                detail_images = longDetailImages,
                unit,
                status,
                image = primaryImage,
                detailImage = detailImage,
                detail_image = detailImage,
                description,
                weight,
                storage,
                tags,
                categoryId = commodity.CategoryId,
                category_id = commodity.CategoryId,
                categoryName = category?.CategoryName ?? string.Empty,
                category_name = category?.CategoryName ?? string.Empty,
                canBuy = status == 1,
                salesStatus = status == 1 ? "on_sale" : "sold_out",
                bottomImages = longDetailImages
            }));
        }
        catch (Exception ex)
        {
            return Ok(ApiResult.Fail($"获取商品详情失败：{ex.Message}"));
        }
    }

    private static string ResolveDetailImageUrl(string primaryImage, IReadOnlyCollection<string?> detailImages)
    {
        var detailImage = detailImages
            .Select(NormalizeImageUrl)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        return string.IsNullOrWhiteSpace(detailImage) ? primaryImage : detailImage;
    }

    private static string ResolvePrimaryImageUrl(string? imageUrl, IReadOnlyCollection<string?> detailImages)
    {
        var normalizedMainImage = NormalizeImageUrl(imageUrl);
        if (!string.IsNullOrWhiteSpace(normalizedMainImage))
        {
            return normalizedMainImage;
        }

        return detailImages
            .Select(NormalizeImageUrl)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            ?? string.Empty;
    }

    private static string? NormalizeImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var trimmed = imageUrl.Trim();
        var duplicateMarkerIndex = trimmed.IndexOf("https://", 8, StringComparison.OrdinalIgnoreCase);
        if (duplicateMarkerIndex > 0)
        {
            trimmed = trimmed[..duplicateMarkerIndex];
        }

        duplicateMarkerIndex = trimmed.IndexOf("http://", 7, StringComparison.OrdinalIgnoreCase);
        if (duplicateMarkerIndex > 0)
        {
            trimmed = trimmed[..duplicateMarkerIndex];
        }

        return trimmed.Trim();
    }
}
