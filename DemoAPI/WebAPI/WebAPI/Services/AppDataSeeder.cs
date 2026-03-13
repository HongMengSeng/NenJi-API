using Microsoft.EntityFrameworkCore;
using MySqlConnector;
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
        await EnsureAdminSchemaAsync(cancellationToken);
        await SeedCategoriesAsync(cancellationToken);
        await SeedTagsAsync(cancellationToken);
        await SeedCommoditiesAsync(cancellationToken);
        await SeedAdminAccountsAsync(cancellationToken);
        await SeedDishesAsync(cancellationToken);
        await SeedCouponsAsync(cancellationToken);
        await SeedSubscriptionFarmsAsync(cancellationToken);
    }

    private async Task EnsureAdminSchemaAsync(CancellationToken cancellationToken)
    {
        var tableCommands = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS dish_category (
                dish_category_id int NOT NULL AUTO_INCREMENT,
                dish_category_name varchar(50) NOT NULL,
                dish_category_description varchar(255) NOT NULL,
                dish_category_status int NOT NULL DEFAULT 1,
                dish_sort_order int NOT NULL DEFAULT 0,
                PRIMARY KEY (dish_category_id)
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS admin_account (
                admin_id int NOT NULL AUTO_INCREMENT,
                username varchar(50) NOT NULL,
                password varchar(100) NOT NULL,
                display_name varchar(50) NOT NULL,
                status int NOT NULL DEFAULT 1,
                created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (admin_id),
                UNIQUE KEY uk_admin_account_username (username)
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS coupon (
                coupon_id int NOT NULL AUTO_INCREMENT,
                name varchar(100) NOT NULL,
                management_type varchar(50) NOT NULL,
                coupon_type varchar(50) NOT NULL,
                min_amount decimal(10,2) NOT NULL DEFAULT 0,
                discount_amount decimal(10,2) NOT NULL DEFAULT 0,
                discount_rate decimal(4,2) NOT NULL DEFAULT 1,
                validity_period int NOT NULL DEFAULT 0,
                validity_unit varchar(20) NOT NULL DEFAULT '天',
                status int NOT NULL DEFAULT 1,
                created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (coupon_id)
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS subscription_farm (
                subscription_farm_id int NOT NULL AUTO_INCREMENT,
                farm_name varchar(100) NOT NULL,
                cover_image varchar(255) NULL,
                carousel_images text NULL,
                spec_images text NULL,
                area decimal(10,2) NOT NULL DEFAULT 0,
                unit_price decimal(10,2) NOT NULL DEFAULT 0,
                min_yield decimal(10,2) NOT NULL DEFAULT 0,
                yield_unit varchar(20) NOT NULL DEFAULT 'kg',
                status int NOT NULL DEFAULT 1,
                intro text NULL,
                created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (subscription_farm_id)
            );
            """
        };

        await EnsureColumnAsync("commodity", "unit_price", "ALTER TABLE commodity ADD COLUMN unit_price decimal(10,2) NOT NULL DEFAULT 0;", cancellationToken);
        await EnsureColumnAsync("commodity", "created_at", "ALTER TABLE commodity ADD COLUMN created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP;", cancellationToken);
        await EnsureColumnAsync("commodity", "storage_condition", "ALTER TABLE commodity ADD COLUMN storage_condition varchar(100) NULL;", cancellationToken);
        await EnsureColumnAsync("commodity", "weight_unit", "ALTER TABLE commodity ADD COLUMN weight_unit varchar(20) NULL DEFAULT 'g';", cancellationToken);
        await EnsureColumnAsync("dish", "created_at", "ALTER TABLE dish ADD COLUMN created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP;", cancellationToken);
        await EnsureColumnAsync("dish", "carousel_images", "ALTER TABLE dish ADD COLUMN carousel_images text NULL;", cancellationToken);
        await EnsureColumnAsync("dish", "spec_images", "ALTER TABLE dish ADD COLUMN spec_images text NULL;", cancellationToken);
        await EnsureColumnAsync("user", "gender", "ALTER TABLE user ADD COLUMN gender varchar(10) NULL;", cancellationToken);
        await EnsureColumnAsync("user", "status", "ALTER TABLE user ADD COLUMN status int NOT NULL DEFAULT 1;", cancellationToken);

        foreach (var command in tableCommands)
        {
            await ExecuteSafeSqlAsync(command, cancellationToken);
        }
    }

    private async Task EnsureColumnAsync(string tableName, string columnName, string sql, CancellationToken cancellationToken)
    {
        await using var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.columns
            WHERE table_schema = DATABASE() AND table_name = @tableName AND column_name = @columnName;
            """;

        var tableParam = command.CreateParameter();
        tableParam.ParameterName = "@tableName";
        tableParam.Value = tableName;
        command.Parameters.Add(tableParam);

        var columnParam = command.CreateParameter();
        columnParam.ParameterName = "@columnName";
        columnParam.Value = columnName;
        command.Parameters.Add(columnParam);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (count == 0)
        {
            await ExecuteSafeSqlAsync(sql, cancellationToken);
        }
    }

    private async Task ExecuteSafeSqlAsync(string sql, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
        catch (MySqlException ex) when (ex.Message.Contains("Duplicate column name", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Column already exists. SQL skipped: {Sql}", sql);
        }
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
            new Category { CategoryName = "蛋类产品", CategoryDescription = "散养蛋类产品", CategoryStatus = 1, SortOrder = 3 },
            new Category { CategoryName = "乳制品", CategoryDescription = "新鲜乳制品", CategoryStatus = 1, SortOrder = 4 },
            new Category { CategoryName = "主食粮油", CategoryDescription = "农场主食粮油", CategoryStatus = 1, SortOrder = 5 }
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
                SpecDescription = "脆嫩清甜，适合沙拉和清炒。",
                InStock = 50,
                Quantity = 500,
                ProductStatus = 1,
                ImageUrl = "https://images.unsplash.com/photo-1622206151226-18ca2c9ab4a1?auto=format&fit=crop&w=600&q=80",
                UnitPrice = 12.8m,
                CreatedAt = DateTime.Now.AddDays(-5),
                StorageCondition = "冷藏保存",
                WeightUnit = "g"
            },
            new Commodity
            {
                ProductName = "农家西红柿",
                CategoryId = categoryMap["新鲜蔬菜"],
                SpecDescription = "自然成熟，酸甜多汁。",
                InStock = 60,
                Quantity = 500,
                ProductStatus = 1,
                ImageUrl = "https://images.unsplash.com/photo-1546094096-0df4bcaaa337?auto=format&fit=crop&w=600&q=80",
                UnitPrice = 9.9m,
                CreatedAt = DateTime.Now.AddDays(-4),
                StorageCondition = "常温通风",
                WeightUnit = "g"
            },
            new Commodity
            {
                ProductName = "土猪肉",
                CategoryId = categoryMap["肉类产品"],
                SpecDescription = "农场散养土猪，现切配送。",
                InStock = 30,
                Quantity = 500,
                ProductStatus = 1,
                ImageUrl = "https://images.unsplash.com/photo-1607623814075-e51df1bdc82f?auto=format&fit=crop&w=600&q=80",
                UnitPrice = 38m,
                CreatedAt = DateTime.Now.AddDays(-3),
                StorageCondition = "冷冻保存",
                WeightUnit = "g"
            },
            new Commodity
            {
                ProductName = "土鸡蛋",
                CategoryId = categoryMap["蛋类产品"],
                SpecDescription = "散养鸡蛋，营养丰富。",
                InStock = 80,
                Quantity = 10,
                ProductStatus = 1,
                ImageUrl = "https://images.unsplash.com/photo-1506976785307-8732e854ad03?auto=format&fit=crop&w=600&q=80",
                UnitPrice = 16.8m,
                CreatedAt = DateTime.Now.AddDays(-2),
                StorageCondition = "阴凉干燥",
                WeightUnit = "枚"
            },
            new Commodity
            {
                ProductName = "鲜牛奶",
                CategoryId = categoryMap["乳制品"],
                SpecDescription = "牧场直供鲜牛奶。",
                InStock = 40,
                Quantity = 500,
                ProductStatus = 1,
                ImageUrl = "https://images.unsplash.com/photo-1550583724-b2692b85b150?auto=format&fit=crop&w=600&q=80",
                UnitPrice = 19.9m,
                CreatedAt = DateTime.Now.AddDays(-1),
                StorageCondition = "冷藏保存",
                WeightUnit = "ml"
            },
            new Commodity
            {
                ProductName = "农家大米",
                CategoryId = categoryMap["主食粮油"],
                SpecDescription = "颗粒饱满，米香浓郁。",
                InStock = 100,
                Quantity = 1000,
                ProductStatus = 1,
                ImageUrl = "https://images.unsplash.com/photo-1586201375761-83865001e31c?auto=format&fit=crop&w=600&q=80",
                UnitPrice = 49.9m,
                CreatedAt = DateTime.Now,
                StorageCondition = "阴凉干燥",
                WeightUnit = "g"
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
            else if (commodity.ProductName is "土猪肉" or "农家大米")
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

    private async Task SeedAdminAccountsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.AdminAccounts.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.AdminAccounts.Add(new AdminAccount
        {
            Username = "admin",
            Password = "123456",
            DisplayName = "农场管理员",
            Status = 1,
            CreatedAt = DateTime.Now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDishesAsync(CancellationToken cancellationToken)
    {
        await ExecuteSafeSqlAsync(
            """
            INSERT INTO dish_category (dish_category_id, dish_category_name, dish_category_description, dish_category_status, dish_sort_order)
            VALUES (1, '默认菜品分类', '系统默认分类', 1, 0)
            ON DUPLICATE KEY UPDATE dish_category_name = VALUES(dish_category_name);
            """,
            cancellationToken);

        if (await _dbContext.Dishes.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.Dishes.AddRange(
            new Dish
            {
                DishName = "农家小炒肉",
                DishDescription = "肥瘦相间，现炒出锅。",
                DishPrice = 32m,
                DishCategoryId = 1,
                ImageUrl = "https://images.unsplash.com/photo-1604908554027-8b0dcdc5f10d?auto=format&fit=crop&w=600&q=80",
                AttributeName = "招牌",
                Status = 1,
                LimitedEdition = 100,
                DishSold = 26,
                DishRemainingQuantity = 74,
                UserPurchaseLimit = 5,
                CreatedAt = DateTime.Now.AddDays(-2)
            },
            new Dish
            {
                DishName = "番茄牛腩",
                DishDescription = "汤汁浓郁，适合配饭。",
                DishPrice = 46m,
                DishCategoryId = 1,
                ImageUrl = "https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=600&q=80",
                AttributeName = "热销",
                Status = 1,
                LimitedEdition = 80,
                DishSold = 15,
                DishRemainingQuantity = 65,
                UserPurchaseLimit = 3,
                CreatedAt = DateTime.Now.AddDays(-1)
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCouponsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Coupons.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.Coupons.AddRange(
            new Coupon
            {
                Name = "满99减10",
                ManagementType = "产品",
                CouponType = "满减券",
                MinAmount = 99,
                DiscountAmount = 10,
                DiscountRate = 1,
                ValidityPeriod = 30,
                ValidityUnit = "天",
                Status = 1,
                CreatedAt = DateTime.Now.AddDays(-5)
            },
            new Coupon
            {
                Name = "餐饮9折券",
                ManagementType = "菜品",
                CouponType = "折扣券",
                MinAmount = 0,
                DiscountAmount = 0,
                DiscountRate = 0.9m,
                ValidityPeriod = 3,
                ValidityUnit = "月",
                Status = 1,
                CreatedAt = DateTime.Now.AddDays(-2)
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSubscriptionFarmsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.SubscriptionFarms.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.SubscriptionFarms.AddRange(
            new SubscriptionFarm
            {
                FarmName = "稻香一号田",
                CoverImage = "https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=600&q=80",
                Area = 666,
                UnitPrice = 16800,
                MinYield = 800,
                YieldUnit = "斤",
                Status = 1,
                Intro = "适合水稻认养，支持成长直播。",
                CreatedAt = DateTime.Now.AddDays(-10)
            },
            new SubscriptionFarm
            {
                FarmName = "蔬菜共享地",
                CoverImage = "https://images.unsplash.com/photo-1464226184884-fa280b87c399?auto=format&fit=crop&w=600&q=80",
                Area = 320,
                UnitPrice = 9800,
                MinYield = 260,
                YieldUnit = "kg",
                Status = 1,
                Intro = "四季蔬菜轮种，支持周配到家。",
                CreatedAt = DateTime.Now.AddDays(-6)
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
