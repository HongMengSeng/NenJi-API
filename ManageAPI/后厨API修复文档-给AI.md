# 后厨出餐系统 — 后端 API 修复文档

将此文档提供给 AI（如 Claude、ChatGPT、DeepSeek 等），并附上你的后端项目源码，AI 即可根据以下要求修复代码。

---

## 一、数据库表结构

### DishOrders（订单表）

| 字段 | 类型 | 说明 |
|------|------|------|
| OrderId | int/long | 主键 |
| OrderNo | string | 订单号 |
| OrderStatusId | int | 1=待付款, 2=已付款, 3=已完成, 4=已取消 |
| CreateTime | datetime | 创建时间 |
| TableNumber | string | 桌台号 |
| TotalAmount | decimal | 订单总价 |
| Remark | string | 备注 |

### DishOrderDetails（菜品明细表）

| 字段 | 类型 | 说明 |
|------|------|------|
| DishOrderDetailsId | int/long | 主键 |
| DishOrderId | int/long | 关联 DishOrders.OrderId |
| Name | string | 菜品名称 |
| Quantity | int | 数量 |
| Price | decimal | 单价 |
| SubtotalAmount | decimal | 小计金额（Price × Quantity） |
| StatusId | int | **1=待出餐, 3=已出餐, 4=已取消** |

---

## 二、枚举定义

```csharp
/// <summary>
/// 菜品状态（DishOrderDetails.StatusId）
/// 注意：与订单状态 OrderStatusId 共用同一套值，但菜品没有"已付款"(2)
/// </summary>
public static class DishStatus
{
    public const int Pending = 1;   // 待出餐
    public const int Finished = 3;  // 已出餐（对应订单状态的"已完成"）
    public const int Cancelled = 4; // 已取消
}

/// <summary>
/// 订单状态（DishOrders.OrderStatusId）
/// </summary>
public static class OrderStatus
{
    public const int PendingPayment = 1; // 待付款
    public const int Paid = 2;           // 已付款（后厨"待出餐"列表查这个）
    public const int Completed = 3;      // 已完成（后厨"已完成"列表查这个）
    public const int Cancelled = 4;      // 已取消
}
```

---

## 三、关键 Bug — 出餐和取消接口问题

### 现象

```
前端 POST /api/Kitchen/dish/finish → { dishOrderDetailsId: 123 }
后端返回 { code: 0, message: "success" }
但数据库 SELECT StatusId FROM DishOrderDetails WHERE DishOrderDetailsId = 123 → 仍然是 1（未改变）
```

### 根因

`dish/finish` 控制器方法中，接口返回了成功，但**没有执行 UPDATE 语句**，或者执行了但没有 `SaveChangesAsync()` / `SubmitChanges()`。

---

### 另一个已出现的问题：dish/cancel 写错了状态值

**症状**：取消出餐后，清空浏览器缓存重新进入，已取消的菜品显示为"已出餐"。

**原因**：`dish/cancel` 接口中错误地将 StatusId 写成了 **3（已出餐）**，应该写 **4（已取消）**。这是复制 `dish/finish` 代码后忘记改值的典型错误。

**检查方法**：查看数据库中该菜品的 StatusId：
- 如果是 **3** → 写错了，应改为 **4**
- 如果是 **4** → 正确

请重点检查 `CancelDish` 方法中赋值语句的值。

### 修复要求

#### POST /api/Kitchen/dish/finish

```csharp
[HttpPost("dish/finish")]
public async Task<IActionResult> FinishDish([FromBody] DishOperateRequest request)
{
    if (request.DishOrderDetailsId <= 0)
        return BadRequest(new { code = -1, message = "参数无效" });

    var dish = await _context.DishOrderDetails
        .FirstOrDefaultAsync(d => d.DishOrderDetailsId == request.DishOrderDetailsId);

    if (dish == null)
        return NotFound(new { code = -1, message = "菜品不存在" });

    if (dish.StatusId != DishStatus.Pending)
        return BadRequest(new { code = -1, message = "该菜品已被处理，无法重复操作" });

    dish.StatusId = DishStatus.Finished; // 改为 3（已出餐）

    // ★★★★★ 关键：必须调用 SaveChanges 才能写入数据库 ★★★★★
    await _context.SaveChangesAsync();

    return Ok(new { code = 0, message = "success" });
}
```

#### POST /api/Kitchen/dish/cancel

```csharp
[HttpPost("dish/cancel")]
public async Task<IActionResult> CancelDish([FromBody] DishOperateRequest request)
{
    if (request.DishOrderDetailsId <= 0)
        return BadRequest(new { code = -1, message = "参数无效" });

    var dish = await _context.DishOrderDetails
        .FirstOrDefaultAsync(d => d.DishOrderDetailsId == request.DishOrderDetailsId);

    if (dish == null)
        return NotFound(new { code = -1, message = "菜品不存在" });

    if (dish.StatusId != DishStatus.Pending)
        return BadRequest(new { code = -1, message = "该菜品已被处理，无法重复操作" });

    dish.StatusId = DishStatus.Cancelled; // 改为 4（已取消）

    // ★★★★★ 关键：必须调用 SaveChanges 才能写入数据库 ★★★★★
    await _context.SaveChangesAsync();

    return Ok(new { code = 0, message = "success" });
}

// 请求 DTO
public class DishOperateRequest
{
    public long DishOrderDetailsId { get; set; }
}
```

---

## 四、订单列表接口 — 需要返回菜品价格和数量

### GET /api/Kitchen/order/list?type={type}

**type 参数说明：**
- `type=2` → 查询 OrderStatusId=2（已付款）的订单 → 前端显示为"待出餐"
- `type=3` → 查询 OrderStatusId=3（已完成）的订单 → 前端显示为"已完成"

**返回数据要求：**
每个订单中的菜品明细（DishList）**必须包含 Price 和 Quantity 字段**，前端需要这些值计算营业额。

```csharp
[HttpGet("order/list")]
public async Task<IActionResult> GetOrderList([FromQuery] int type)
{
    if (type != 2 && type != 3)
        return BadRequest(new { code = -1, message = "type 参数值不正确，仅支持 2 (待出餐) 或 3 (已完成)" });

    var orders = await _context.DishOrders
        .Where(o => o.OrderStatusId == type)
        .OrderByDescending(o => o.CreateTime)
        .Select(o => new
        {
            o.OrderId,
            o.OrderNo,
            o.CreateTime,
            o.TableNumber,
            o.TotalAmount,
            o.Remark,
            DishList = o.DishOrderDetails.Select(d => new
            {
                d.DishOrderDetailsId,
                d.Name,
                d.Quantity,    // ★ 必须返回
                d.Price,       // ★ 必须返回
                d.SubtotalAmount,
                d.StatusId     // ★ 必须返回（1=待出餐, 3=已出餐, 4=已取消）
            })
        })
        .ToListAsync();

    return Ok(new { code = 0, message = "success", data = orders });
}
```

---

## 五、订单详情接口

### GET /api/Kitchen/order/detail?orderId={orderId}

```csharp
[HttpGet("order/detail")]
public async Task<IActionResult> GetOrderDetail([FromQuery] long orderId)
{
    var order = await _context.DishOrders
        .Where(o => o.OrderId == orderId)
        .Select(o => new
        {
            o.OrderId,
            o.OrderNo,
            o.CreateTime,
            o.TableNumber,
            o.TotalAmount,
            o.Remark,
            DishList = o.DishOrderDetails.Select(d => new
            {
                d.DishOrderDetailsId,
                d.Name,
                d.Quantity,
                d.Price,
                d.SubtotalAmount,
                d.StatusId  // 1=待出餐, 3=已出餐, 4=已取消
            })
        })
        .FirstOrDefaultAsync();

    if (order == null)
        return NotFound(new { code = -1, message = "订单不存在" });

    return Ok(new { code = 0, message = "success", data = order });
}
```

---

## 六、今日营业额统计接口

### GET /api/Kitchen/today-statistics

```csharp
[HttpGet("today-statistics")]
public async Task<IActionResult> GetTodayStatistics()
{
    var today = DateTime.Today;
    var tomorrow = today.AddDays(1);

    // 获取今日所有订单（不含已取消的）
    var todayOrders = await _context.DishOrders
        .Where(o => o.CreateTime >= today && o.CreateTime < tomorrow)
        .Select(o => new { o.OrderId, o.OrderStatusId })
        .ToListAsync();

    var orderIds = todayOrders.Select(o => o.OrderId).ToList();

    // 获取这些订单的菜品明细
    var details = await _context.DishOrderDetails
        .Where(d => orderIds.Contains(d.DishOrderId))
        .ToListAsync();

    // 统计（状态值：1=待出餐, 3=已出餐, 4=已取消）
    var pendingDishCount = details.Count(d => d.StatusId == DishStatus.Pending);
    var finishedDishCount = details.Count(d => d.StatusId == DishStatus.Finished);

    var finishedOrderCount = details
        .GroupBy(d => d.DishOrderId)
        .Count(g => g.Any() && g.All(d => d.StatusId == DishStatus.Finished));

    var totalAmount = details
        .Where(d => d.StatusId == DishStatus.Finished)
        .Sum(d => d.SubtotalAmount);

    return Ok(new
    {
        code = 0,
        message = "success",
        data = new KitchenStatisticsDto
        {
            TodayTotalAmount = totalAmount,
            TodayTotalOrder = todayOrders.Count,
            TodayFinishedOrder = finishedOrderCount,
            TodayPendingDish = pendingDishCount,
            TodayFinishedDish = finishedDishCount
        }
    });
}

public class KitchenStatisticsDto
{
    public decimal TodayTotalAmount { get; set; }    // 今日营业额（只算已出餐菜品）
    public int TodayTotalOrder { get; set; }          // 今日总订单数
    public int TodayFinishedOrder { get; set; }       // 已全部出餐的订单数
    public int TodayPendingDish { get; set; }         // 待出餐菜品数
    public int TodayFinishedDish { get; set; }        // 已出餐菜品数
}
```

---

## 七、API 接口汇总

| 方法 | 路径 | 请求参数 | 说明 | 当前状态 |
|------|------|---------|------|---------|
| POST | /api/Kitchen/login | { phoneNumber, password } | 登录 | 正常 |
| GET | /api/Kitchen/order/list?type=2 | type=2/3 | 订单列表，需返回菜品 Price/Quantity/StatusId | ⚠️ 缺价格和数量 |
| GET | /api/Kitchen/order/detail?orderId= | orderId | 订单详情 | ⚠️ 可能缺价格和数量 |
| POST | /api/Kitchen/dish/finish | { dishOrderDetailsId } | 标记菜品已出餐 → StatusId=3 | ❌ 未写库 |
| POST | /api/Kitchen/dish/cancel | { dishOrderDetailsId } | 取消出餐 → StatusId=4 | ❌ 未写库 |
| GET | /api/Kitchen/today-statistics | 无 | 今日营业额统计 | ❌ 状态值用错 |

---

## 八、验证方法

修复后，用以下 SQL 或 API 调用验证：

```sql
-- 查看菜品状态是否被更新
SELECT DishOrderDetailsId, Name, StatusId 
FROM DishOrderDetails 
WHERE DishOrderDetailsId = 刚才操作的ID;
-- StatusId 应为 3（已出餐）或 4（已取消）

-- 查看营业额统计
SELECT SUM(SubtotalAmount) 
FROM DishOrderDetails 
WHERE StatusId = 3  -- 只算已出餐
  AND DishOrderId IN (
    SELECT OrderId FROM DishOrders 
    WHERE CreateTime >= CAST(GETDATE() AS DATE)
  );
```

```http
### 验证出餐
POST /api/Kitchen/dish/finish
Content-Type: application/json

{ "dishOrderDetailsId": 123 }

### 验证订单列表含价格
GET /api/Kitchen/order/list?type=2

### 验证营业额
GET /api/Kitchen/today-statistics
```
