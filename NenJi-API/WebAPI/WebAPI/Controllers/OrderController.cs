using System.Security.Claims;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Common;
using WebAPI.Data;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/order")]
public class OrderController : ControllerBase
{
    private const string DefaultFlagProperty = "IsDefault";
    private readonly AppDbContext _dbContext;
    private readonly IInventoryStatsService _inventoryStatsService;

    public OrderController(AppDbContext dbContext, IInventoryStatsService inventoryStatsService)
    {
        _dbContext = dbContext;
        _inventoryStatsService = inventoryStatsService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetPageData(
        [FromQuery] string categoryId = "vegetables",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var categories = await LoadMenuCategoriesAsync(cancellationToken);
        var currentCategory = categories.Any(x => x.Id.Equals(categoryId, StringComparison.OrdinalIgnoreCase))
            ? categoryId
            : categories.FirstOrDefault()?.Id ?? "vegetables";
        var categoryMap = categories.ToDictionary(x => x.CategoryId, x => x.Id);

        var query = _dbContext.Commodities.AsNoTracking().Where(x => (x.ProductStatus ?? 0) == 1);
        var commodities = await query.OrderBy(x => x.CategoryId).ThenBy(x => x.CommodityId).ToListAsync(cancellationToken);
        var stats = await _inventoryStatsService.GetCommodityStatsAsync(commodities.Select(x => x.CommodityId), cancellationToken);

        var goods = commodities
            .Where(x => categoryMap.TryGetValue(x.CategoryId, out var key) && key == currentCategory)
            .Select(x => BuildMenuGoods(x, stats.GetValueOrDefault(x.CommodityId)))
            .ToList();
        var total = goods.Count;
        var pageGoods = goods.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(ApiResult.Success(new
        {
            currentCategory,
            categories = categories.Select(x => new { id = x.Id, name = x.Name }),
            goodsList = pageGoods,
            page,
            pageSize,
            total,
            hasMore = page * pageSize < total
        }));
    }

    [AllowAnonymous]
    [HttpGet("getOrderData")]
    public async Task<IActionResult> GetOrderData(CancellationToken cancellationToken)
    {
        var categories = await LoadMenuCategoriesAsync(cancellationToken);
        var categoryMap = categories.ToDictionary(x => x.CategoryId, x => x.Id);
        var commodities = await _dbContext.Commodities.AsNoTracking().Where(x => (x.ProductStatus ?? 0) == 1).ToListAsync(cancellationToken);
        var stats = await _inventoryStatsService.GetCommodityStatsAsync(commodities.Select(x => x.CommodityId), cancellationToken);
        var goodsList = commodities
            .GroupBy(x => categoryMap.TryGetValue(x.CategoryId, out var key) ? key : $"category-{x.CategoryId}")
            .ToDictionary(g => g.Key, g => g.Select(x => BuildMenuGoods(x, stats.GetValueOrDefault(x.CommodityId))).ToList());

        return Ok(ApiResult.Success(new { data = new { data = new { categories, goodsList } } }));
    }

    [AllowAnonymous]
    [HttpPost("updateGoodsQuantity")]
    public IActionResult UpdateGoodsQuantity([FromBody] UpdateGoodsQuantityRequest? request)
    {
        return Ok(ApiResult.Success(new { updated = request?.Updates?.Count ?? 0 }));
    }

    [HttpGet("status-list")]
    public IActionResult GetStatusList()
    {
        return Ok(ApiResult.Success(new[]
        {
            new { value = "all", label = "All" },
            new { value = "pending", label = "Pending" },
            new { value = "paid", label = "Paid" },
            new { value = "shipping", label = "Shipping" },
            new { value = "completed", label = "Completed" },
            new { value = "cancelled", label = "Cancelled" }
        }));
    }

    [HttpPost("create")]
    [HttpPost("getOrderData/create")]
    [HttpPost("create-payment-order")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Ok(ApiResult.Fail("request body is required", 400));
        }

        var userId = GetCurrentUserId();
        var items = await ResolveOrderItemsAsync(userId, request, cancellationToken);
        if (items.Count == 0)
        {
            return Ok(ApiResult.Fail("items cannot be empty", 400));
        }

        var totalAmount = request.TotalPrice > 0 ? request.TotalPrice : items.Sum(x => x.Price * x.Quantity);
        if (totalAmount <= 0)
        {
            return Ok(ApiResult.Fail("totalPrice is invalid", 400));
        }

        var addressInput = request.Address ?? request.AddressAlias ?? new ConfirmOrderAddress();
        var address = await ResolveAddressAsync(userId, request, addressInput, cancellationToken);
        var receiverName = FirstNonEmpty(addressInput.Name, address?.ContactName);
        var receiverPhone = FirstNonEmpty(addressInput.Phone, address?.ContactPhone);
        var receiverAddress = FirstNonEmpty(addressInput.Address, address is null ? null : BuildAddressText(address));
        if (string.IsNullOrWhiteSpace(receiverName) || string.IsNullOrWhiteSpace(receiverPhone) || string.IsNullOrWhiteSpace(receiverAddress))
        {
            return Ok(ApiResult.Fail("address is required", 400));
        }

        var now = DateTime.Now;
        var order = new OrderEntity
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            ActualPayment = totalAmount,
            TotalOrderAmount = totalAmount,
            OrderType = 1,
            OrderStatus = 0,
            PaymentStatus = 0,
            DeliveryMethods = 1,
            ShippingAddress = Truncate(receiverAddress, 45),
            AddressId = address?.AddressId ?? Math.Max(request.AddressId, ParseNumericId(addressInput.Id)),
            ContactPerson = Truncate(receiverName, 45),
            ContactNumber = Truncate(receiverPhone, 45),
            OrderCreationTime = now,
            PaymentTime = now,
            PaymentMethods = 0
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var item in items)
        {
            _dbContext.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.OrderId,
                CommodityId = item.GoodsId,
                ActualUnitPrice = item.Price,
                UnitPrice = item.Price,
                PurchaseQuantity = item.Quantity,
                SubtotalAmount = item.Price * item.Quantity
            });
        }

        if (request.MergedCartIds.Count > 0)
        {
            var carts = _dbContext.ShippingCarts.Where(x => x.UserId == userId && request.MergedCartIds.Contains(x.ShippingCartId));
            _dbContext.ShippingCarts.RemoveRange(carts);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(ApiResult.Success(new
        {
            orderId = order.OrderId.ToString(),
            id = order.OrderId.ToString(),
            orderNumber = order.OrderNumber,
            status = "pending",
            orderStatus = "pending",
            paymentStatus = 0,
            totalPrice = totalAmount,
            amount = totalAmount,
            quantity = items.Sum(x => x.Quantity),
            createTime = order.OrderCreationTime.ToString("yyyy-MM-dd HH:mm:ss")
        }, "order created"));
    }

    [HttpGet("list")]
    [HttpGet("getOrderData/list")]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);
        var userId = GetCurrentUserId();
        var query = _dbContext.Orders.AsNoTracking().Where(x => x.UserId == userId && x.OrderType == 1);
        query = ApplyStatusFilter(query, status);
        var total = await query.CountAsync(cancellationToken);
        var orders = await query.OrderByDescending(x => x.OrderCreationTime).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var orderIds = orders.Select(x => x.OrderId).ToList();
        var itemMap = await LoadOrderItemsAsync(orderIds, cancellationToken);

        return Ok(ApiResult.Success(new
        {
            orders = orders.Select(x => BuildOrderListItem(x, itemMap)).ToList(),
            total,
            page,
            pageSize,
            hasMore = page * pageSize < total
        }));
    }

    [HttpGet("info")]
    [HttpGet("detail")]
    [HttpGet("getOrderData/detail")]
    [HttpGet("getOrderData/{orderId:long}")]
    public async Task<IActionResult> Detail([FromQuery] long orderId, [FromRoute] long routeOrderId, CancellationToken cancellationToken)
    {
        orderId = orderId > 0 ? orderId : routeOrderId;
        if (orderId <= 0)
        {
            return Ok(ApiResult.Fail("orderId is invalid", 400));
        }

        var userId = GetCurrentUserId();
        var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == userId, cancellationToken);
        if (order is null)
        {
            return Ok(ApiResult.Fail("order not found", 404));
        }

        var items = await LoadOrderItemsAsync([order.OrderId], cancellationToken);
        var orderView = BuildOrderListItem(order, items);
        return Ok(ApiResult.Success(new { totalAmount = order.TotalOrderAmount, order = orderView }));
    }

    [HttpPut("cancel")]
    [HttpPost("{id:long}/cancel")]
    [HttpPost("getOrderData/cancel/{id:long}")]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelOrderRequest? request, CancellationToken cancellationToken)
    {
        var orderId = request?.OrderId > 0 ? request.OrderId : id;
        if (orderId <= 0)
        {
            return Ok(ApiResult.Fail("orderId is invalid", 400));
        }

        var userId = GetCurrentUserId();
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == userId, cancellationToken);
        if (order is null)
        {
            return Ok(ApiResult.Fail("order not found", 404));
        }

        if (order.PaymentStatus == 1)
        {
            return Ok(ApiResult.Fail("paid order cannot be cancelled", 409));
        }

        order.OrderStatus = 4;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResult.Success(new { orderId = order.OrderId.ToString(), status = "cancelled" }));
    }

    [HttpPost("{id:long}/pay")]
    [HttpPost("getOrderData/pay/{id:long}")]
    public async Task<IActionResult> Pay(long id, [FromBody] PayOrderRequest? request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == id && x.UserId == userId, cancellationToken);
        if (order is null)
        {
            return Ok(ApiResult.Fail("order not found", 404));
        }

        order.PaymentStatus = 1;
        order.OrderStatus = 1;
        order.PaymentMethods = string.Equals(request?.PaymentMethod, "wallet", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
        order.PaymentTime = DateTime.Now;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResult.Success(new { orderId = order.OrderId.ToString(), status = "paid", statusText = "Paid" }));
    }

    [HttpPost("{id:long}/confirm")]
    [HttpPost("getOrderData/confirm/{id:long}")]
    public async Task<IActionResult> Confirm(long id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == id && x.UserId == userId, cancellationToken);
        if (order is null)
        {
            return Ok(ApiResult.Fail("order not found", 404));
        }

        order.OrderStatus = 3;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResult.Success(new { orderId = order.OrderId.ToString(), status = "completed", statusText = "Completed" }));
    }

    private async Task<List<MenuCategoryItem>> LoadMenuCategoriesAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.Categories.AsNoTracking().Where(x => (x.CategoryStatusId ?? 0) == 1).OrderBy(x => x.SortOrder ?? int.MaxValue).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        return rows.Select(x => new MenuCategoryItem
        {
            CategoryId = x.Id,
            Id = MapCategoryKey(x.Id, x.CategoryName),
            Name = x.CategoryName
        }).ToList();
    }

    private static string MapCategoryKey(int categoryId, string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("meat")) return "meat";
        if (lower.Contains("egg")) return "eggs";
        if (lower.Contains("milk") || lower.Contains("dairy")) return "dairy";
        if (lower.Contains("rice") || lower.Contains("staple")) return "staple";
        return categoryId switch
        {
            1 => "vegetables",
            2 => "meat",
            3 => "eggs",
            4 => "dairy",
            5 => "staple",
            _ => $"category-{categoryId}"
        };
    }

    private object BuildMenuGoods(Commodity commodity, CommodityInventoryStats? stats)
    {
        var image = NormalizeMediaUrl(commodity.ImageUrl);
        return new
        {
            id = commodity.CommodityId.ToString(),
            name = commodity.ProductName,
            price = commodity.UnitPrice ?? 0m,
            image,
            detailImage = image,
            description = commodity.SpecDescription ?? string.Empty,
            sold = stats?.Sold ?? Math.Max(0, commodity.Quantity ?? 0),
            stock = stats?.Stock ?? (commodity.InStock ?? 0)
        };
    }

    private async Task<List<NormalizedOrderItem>> ResolveOrderItemsAsync(int userId, CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var rawItems = request.Items.Count > 0 ? request.Items : request.ItemsAlias;
        if (rawItems.Count > 0)
        {
            return rawItems.Select(x => new NormalizedOrderItem
            {
                GoodsId = ParseNumericId(FirstNonEmpty(x.Id, x.IdAlias)),
                Name = FirstNonEmpty(x.Name, x.NameAlias),
                Price = x.Price > 0 ? x.Price : x.UnitPriceAlias ?? 0m,
                Quantity = Math.Max(1, x.Quantity > 0 ? x.Quantity : x.CountAlias ?? 1),
                Image = FirstNonEmpty(x.Image, x.ImageAlias)
            }).Where(x => x.GoodsId > 0).ToList();
        }

        var cartIds = request.MergedCartIds;
        if (cartIds.Count > 0)
        {
            var carts = await _dbContext.ShippingCarts.AsNoTracking().Where(x => x.UserId == userId && cartIds.Contains(x.ShippingCartId)).ToListAsync(cancellationToken);
            var commodityIds = carts.Where(x => x.CommodityId.HasValue).Select(x => x.CommodityId!.Value).Distinct().ToList();
            var commodityMap = await _dbContext.Commodities.AsNoTracking().Where(x => commodityIds.Contains(x.CommodityId)).ToDictionaryAsync(x => x.CommodityId, cancellationToken);
            return carts.Where(x => x.CommodityId.HasValue && commodityMap.ContainsKey(x.CommodityId.Value)).Select(x =>
            {
                var commodity = commodityMap[x.CommodityId!.Value];
                return new NormalizedOrderItem
                {
                    GoodsId = commodity.CommodityId,
                    Name = commodity.ProductName,
                    Price = commodity.UnitPrice ?? 0m,
                    Quantity = Math.Max(1, x.CartQuantity),
                    Image = NormalizeMediaUrl(commodity.ImageUrl)
                };
            }).ToList();
        }

        var goodsId = request.GoodsId > 0 ? request.GoodsId : request.GoodsIdAlias;
        if (goodsId > 0)
        {
            var commodity = await _dbContext.Commodities.AsNoTracking().FirstOrDefaultAsync(x => x.CommodityId == goodsId, cancellationToken);
            if (commodity is null)
            {
                return [];
            }

            return [new NormalizedOrderItem
            {
                GoodsId = commodity.CommodityId,
                Name = commodity.ProductName,
                Price = commodity.UnitPrice ?? request.TotalPrice,
                Quantity = Math.Max(1, request.Count > 0 ? request.Count : request.Quantity),
                Image = NormalizeMediaUrl(commodity.ImageUrl)
            }];
        }

        return [];
    }

    private async Task<ShippingAddress?> ResolveAddressAsync(int userId, CreateOrderRequest request, ConfirmOrderAddress addressInput, CancellationToken cancellationToken)
    {
        var addressId = request.AddressId > 0 ? request.AddressId : request.AddressIdAlias;
        if (addressId <= 0)
        {
            addressId = addressInput.AddressId > 0 ? addressInput.AddressId : ParseNumericId(addressInput.Id);
        }

        var query = _dbContext.ShippingAddresses.AsNoTracking().Where(x => x.UserId == userId);
        if (addressId > 0)
        {
            var matched = await query.FirstOrDefaultAsync(x => x.AddressId == addressId, cancellationToken);
            if (matched is not null)
            {
                return matched;
            }
        }

        return await query.OrderByDescending(x => EF.Property<bool>(x, DefaultFlagProperty)).ThenByDescending(x => x.AddressId).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Dictionary<long, List<object>>> LoadOrderItemsAsync(IReadOnlyCollection<long> orderIds, CancellationToken cancellationToken)
    {
        if (orderIds.Count == 0)
        {
            return [];
        }

        var details = await _dbContext.OrderDetails.AsNoTracking().Where(x => orderIds.Contains(x.OrderId)).ToListAsync(cancellationToken);
        var commodityIds = details.Select(x => x.CommodityId).Distinct().ToList();
        var commodityMap = await _dbContext.Commodities.AsNoTracking().Where(x => commodityIds.Contains(x.CommodityId)).ToDictionaryAsync(x => x.CommodityId, cancellationToken);
        return details.GroupBy(x => x.OrderId).ToDictionary(g => g.Key, g => g.Select(x =>
        {
            commodityMap.TryGetValue(x.CommodityId, out var commodity);
            return (object)new
            {
                id = x.CommodityId.ToString(),
                name = commodity?.ProductName ?? $"Goods {x.CommodityId}",
                price = x.UnitPrice,
                quantity = x.PurchaseQuantity,
                image = NormalizeMediaUrl(commodity?.ImageUrl)
            };
        }).ToList());
    }

    private static object BuildOrderListItem(OrderEntity order, IReadOnlyDictionary<long, List<object>> itemMap)
    {
        return new
        {
            id = order.OrderId.ToString(),
            status = MapStatus(order.OrderStatus, order.PaymentStatus),
            statusText = MapStatusText(order.OrderStatus, order.PaymentStatus),
            createTime = order.OrderCreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
            totalPrice = order.TotalOrderAmount,
            totalAmount = order.TotalOrderAmount,
            orderType = 1,
            orderTypeText = "Mall Order",
            shippingAddress = new { name = order.ContactPerson, phone = order.ContactNumber, address = order.ShippingAddress },
            items = itemMap.TryGetValue(order.OrderId, out var items) ? items : []
        };
    }

    private static IQueryable<OrderEntity> ApplyStatusFilter(IQueryable<OrderEntity> query, string? status)
    {
        return (status ?? "all").Trim().ToLowerInvariant() switch
        {
            "pending" or "pending_payment" => query.Where(x => x.PaymentStatus == 0 && x.OrderStatus != 4),
            "paid" => query.Where(x => x.PaymentStatus == 1 && x.OrderStatus == 1),
            "shipping" or "shipped" => query.Where(x => x.OrderStatus == 2),
            "completed" => query.Where(x => x.OrderStatus == 3),
            "cancelled" => query.Where(x => x.OrderStatus == 4),
            _ => query
        };
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        return int.TryParse(value, out var userId) && userId > 0
            ? userId
            : throw new InvalidOperationException("unauthorized");
    }

    private static string MapStatus(int orderStatus, int paymentStatus)
    {
        if (orderStatus == 4) return "cancelled";
        if (paymentStatus == 0) return "pending";
        return orderStatus switch
        {
            2 => "shipping",
            3 => "completed",
            _ => "paid"
        };
    }

    private static string MapStatusText(int orderStatus, int paymentStatus)
    {
        return MapStatus(orderStatus, paymentStatus) switch
        {
            "pending" => "Pending Payment",
            "shipping" => "Shipping",
            "completed" => "Completed",
            "cancelled" => "Cancelled",
            _ => "Paid"
        };
    }

    private static string BuildAddressText(ShippingAddress address)
    {
        return $"{address.Province}{address.City}{address.MunicipalDistrict}{address.Addres}";
    }

    private static string NormalizeMediaUrl(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var value = raw.Trim();
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return value;
        if (value.StartsWith("/api/file/", StringComparison.OrdinalIgnoreCase)) return value;
        if (value.StartsWith("api/file/", StringComparison.OrdinalIgnoreCase)) return $"/{value}";
        var name = value.TrimStart('/');
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return ext is ".mp4" or ".mov" or ".avi" or ".mkv" or ".wmv" ? $"/api/file/video/{name}" : $"/api/file/image/{name}";
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;
    }

    private static int ParseNumericId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0;
        if (int.TryParse(raw, out var id)) return id;
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out id) ? id : 0;
    }

    private static string GenerateOrderNumber()
    {
        return $"{DateTime.Now:yyyyMMddHHmmssfff}{Random.Shared.Next(100, 999)}";
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    public sealed class UpdateGoodsQuantityRequest
    {
        public Dictionary<int, int> Updates { get; set; } = [];
    }

    public sealed class CreateOrderRequest
    {
        public string SourceType { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Remark { get; set; } = string.Empty;
        public ConfirmOrderAddress? Address { get; set; }
        public List<ConfirmOrderItemRequest> Items { get; set; } = [];
        public int AddressId { get; set; }
        public List<int> CartIds { get; set; } = [];
        public int GoodsId { get; set; }
        public int Count { get; set; }

        [JsonPropertyName("source_type")]
        public string? SourceTypeAlias { get; set; }

        [JsonPropertyName("source_name")]
        public string? SourceNameAlias { get; set; }

        [JsonPropertyName("address_info")]
        public ConfirmOrderAddress? AddressAlias { get; set; }

        [JsonPropertyName("item_list")]
        public List<ConfirmOrderItemRequest> ItemsAlias { get; set; } = [];

        [JsonPropertyName("address_id")]
        public int AddressIdAlias { get; set; }

        [JsonPropertyName("cart_ids")]
        public List<int> CartIdsAlias { get; set; } = [];

        [JsonPropertyName("goods_id")]
        public int GoodsIdAlias { get; set; }

        [JsonIgnore]
        public List<int> MergedCartIds => CartIds.Count > 0 ? CartIds : CartIdsAlias;
    }

    public sealed class ConfirmOrderAddress
    {
        public string Id { get; set; } = string.Empty;
        public int AddressId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("address_id")]
        public int AddressIdAlias
        {
            get => AddressId;
            set => AddressId = value;
        }
    }

    public sealed class ConfirmOrderItemRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("item_id")]
        public string? IdAlias { get; set; }

        [JsonPropertyName("item_name")]
        public string? NameAlias { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal? UnitPriceAlias { get; set; }

        [JsonPropertyName("count")]
        public int? CountAlias { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageAlias { get; set; }
    }

    public sealed class CancelOrderRequest
    {
        public long OrderId { get; set; }
    }

    public sealed class PayOrderRequest
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal PayAmount { get; set; }
    }

    private sealed class MenuCategoryItem
    {
        public int CategoryId { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private sealed class NormalizedOrderItem
    {
        public int GoodsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; } = string.Empty;
    }
}
