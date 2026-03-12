using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Entities;

namespace WebAPI.Services;

public class AppDataSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AppDataSeeder> _logger;

    public AppDataSeeder(AppDbContext dbContext, ILogger<AppDataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedCategoriesAsync(cancellationToken);
        await SeedTagsAsync(cancellationToken);
        await SeedCommoditiesAsync(cancellationToken);
    }

    private async Task SeedCategoriesAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = new[]
        {
            new Category { CategoryName = "新鲜蔬菜", CategoryDescription = "每日采摘的新鲜蔬菜", CategoryStatus = 1, SortOrder = 1 },
            new Category { CategoryName = "肉类产品", CategoryDescription = "农场直供肉类产品", CategoryStatus = 1, SortOrder = 2 },
            new Category { CategoryName = "禽蛋产品", CategoryDescription = "散养禽蛋产品", CategoryStatus = 1, SortOrder = 3 },
            new Category { CategoryName = "乳制品", CategoryDescription = "新鲜乳制品", CategoryStatus = 1, SortOrder = 4 },
            new Category { CategoryName = "主食", CategoryDescription = "农场主食粮油", CategoryStatus = 1, SortOrder = 5 }
        };

        _dbContext.Categories.AddRange(categories);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} mall categories.", categories.Length);
    }

    private async Task SeedTagsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Tags.AnyAsync(cancellationToken))
        {
            return;
        }

        var tags = new[]
        {
            new Tag { TagName = "有机" },
            new Tag { TagName = "新鲜" },
            new Tag { TagName = "热销" },
            new Tag { TagName = "农场直供" }
        };

        _dbContext.Tags.AddRange(tags);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} commodity tags.", tags.Length);
    }

    private async Task SeedCommoditiesAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Commodities.AnyAsync(cancellationToken))
        {
            return;
        }

        var categoryMap = await _dbContext.Categories
            .AsNoTracking()
            .ToDictionaryAsync(x => x.CategoryName, x => x.Id, cancellationToken);

        var commodities = new[]
        {
            new Commodity
            {
                ProductName = "有机生菜",
                CategoryId = categoryMap["新鲜蔬菜"],
                SpecDescription = "脆嫩爽口，适合沙拉和清炒",
                InStock = 50,
                Quantity = 500,
                ProductStatus = 1,
                ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=organic%20lettuce&image_size=square"
            },
            new Commodity
            {
                ProductName = "农家西红柿",
                CategoryId = categoryMap["新鲜蔬菜"],
                SpecDescription = "自然成熟，酸甜多汁",
                InStock = 60,
                Quantity = 500,
                ProductStatus = 1,
                ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20tomatoes&image_size=square"
            },
            new Commodity
            {
                ProductName = "土猪肉",
                CategoryId = categoryMap["肉类产品"],
                SpecDescription = "农家散养土猪，新鲜现切",
                InStock = 30,
                Quantity = 500,
                ProductStatus = 1,
                ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=pork%20meat&image_size=square"
            },
            new Commodity
            {
                ProductName = "土鸡蛋",
                CategoryId = categoryMap["禽蛋产品"],
                SpecDescription = "散养土鸡蛋，营养丰富",
                InStock = 80,
                Quantity = 10,
                ProductStatus = 1,
                ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20eggs&image_size=square"
            },
            new Commodity
            {
                ProductName = "新鲜牛奶",
                CategoryId = categoryMap["乳制品"],
                SpecDescription = "牧场直供鲜牛奶",
                InStock = 40,
                Quantity = 500,
                ProductStatus = 1,
                ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20milk&image_size=square"
            },
            new Commodity
            {
                ProductName = "农家大米",
                CategoryId = categoryMap["主食"],
                SpecDescription = "颗粒饱满，口感软糯",
                InStock = 100,
                Quantity = 1000,
                ProductStatus = 1,
                ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=bag%20of%20rice&image_size=square"
            }
        };

        _dbContext.Commodities.AddRange(commodities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var insertedCommodities = await _dbContext.Commodities
            .AsNoTracking()
            .Where(x => commodities.Select(y => y.ProductName).Contains(x.ProductName))
            .ToListAsync(cancellationToken);
        var tagMap = await _dbContext.Tags
            .AsNoTracking()
            .ToDictionaryAsync(x => x.TagName, x => x.TagId, cancellationToken);

        var images = insertedCommodities.Select(x => new CommodityImage
        {
            CommodityId = x.CommodityId,
            Url = x.ImageUrl,
            SortOrder = 1,
            ImageType = 1
        }).ToList();

        var relations = new List<CommodityTagRelation>();
        foreach (var commodity in insertedCommodities)
        {
            relations.Add(new CommodityTagRelation
            {
                CommodityId = commodity.CommodityId,
                TagId = tagMap["新鲜"]
            });

            if (commodity.ProductName == "有机生菜")
            {
                relations.Add(new CommodityTagRelation
                {
                    CommodityId = commodity.CommodityId,
                    TagId = tagMap["有机"]
                });
            }
            else if (commodity.ProductName == "土猪肉" || commodity.ProductName == "农家大米")
            {
                relations.Add(new CommodityTagRelation
                {
                    CommodityId = commodity.CommodityId,
                    TagId = tagMap["农场直供"]
                });
            }
            else
            {
                relations.Add(new CommodityTagRelation
                {
                    CommodityId = commodity.CommodityId,
                    TagId = tagMap["热销"]
                });
            }
        }

        _dbContext.CommodityImages.AddRange(images);
        _dbContext.CommodityTagRelations.AddRange(relations);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} mall commodities.", commodities.Length);
    }
}
