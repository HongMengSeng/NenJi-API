# 后厨出餐系统 — 前端对接文档

## 一、状态码体系

### DishOrderDetails.StatusId（菜品明细状态）

| 值 | 含义 | 说明 |
|----|------|------|
| 1 | 待出餐 | 刚下单，厨师还没做 |
| 2 | 已出餐 | 厨师标记完成 |
| 3 | 已取消 | 取消出餐 |

### DishOrders.OrderStatusId（订单状态）

| 值 | 含义 | 说明 |
|----|------|------|
| 1 | 待付款 | 后厨不关心 |
| 2 | 待出餐/制作中 | 订单中还有 StatusId=1 的菜品 |
| 3 | 已完成 | 订单中所有菜品都是 2 或 3（无待出餐） |
| 4 | 已取消 | 整单取消 |

### 联动规则

- **出餐**：标记菜品 StatusId 1→2 后，系统自动检查该订单下是否还有 StatusId=1 的菜品。如果全部出完，自动将订单 OrderStatusId 从 2 改为 3。
- **取消**：标记菜品 StatusId 1→3 后，同样触发联动检查，若订单下无待出餐菜品则自动完成订单。

---

## 二、API 接口

### 基础地址

```
基路径: /api/Kitchen
Content-Type: application/json
```

### 接口列表

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | /login | 登录 |
| GET | /order/list?type={type} | 订单列表 |
| GET | /order/detail?orderId={id} | 订单详情 |
| POST | /dish/finish | 标记出餐 |
| POST | /dish/cancel | 取消出餐 |
| GET | /today-statistics | 今日统计 |
| POST | /logout | 退出 |

---

### 1. 登录

```
POST /api/Kitchen/login
```

**请求：**
```json
{
  "phoneNumber": "13800138000",
  "password": "123456"
}
```

**成功响应：**
```json
{
  "code": 0,
  "message": "success",
  "data": {
    "token": "eyJhbGciOi...",
    "user_id": 1,
    "user_name": "张三",
    "phone_number": "13800138000"
  }
}
```

---

### 2. 订单列表

```
GET /api/Kitchen/order/list?type={type}
```

**参数：**

| 参数 | 值 | 说明 |
|------|-----|------|
| type | 2 | 待出餐订单（OrderStatusId=2） |
| type | 3 | 已完成订单（OrderStatusId=3） |

**type=2 响应：**
```json
{
  "code": 0,
  "message": "success",
  "data": [
    {
      "id": 1001,
      "no": "202605131200001",
      "time": "2026-05-13 12:00:01",
      "table": "A01",
      "total": 156.00,
      "items": [
        {
          "dishOrderDetailsId": 5001,
          "name": "肥牛卷",
          "quantity": 2,
          "price": 35.00,
          "status": 1
        },
        {
          "dishOrderDetailsId": 5002,
          "name": "金针菇",
          "quantity": 1,
          "price": 18.00,
          "status": 2
        }
      ]
    }
  ]
}
```

**字段说明：**

| 字段 | 类型 | 说明 |
|------|------|------|
| id | long | 订单ID |
| no | string | 订单号 |
| time | string | 下单时间 |
| table | string | 桌台号 |
| total | decimal | 订单总金额 |
| items | array | 菜品列表 |
| items[].dishOrderDetailsId | long | 菜品明细ID（用于出餐/取消操作） |
| items[].name | string | 菜品名称 |
| items[].quantity | int | 数量 |
| items[].price | decimal | 单价 |
| items[].status | int | 菜品状态：1=待出餐, 2=已出餐, 3=已取消 |

---

### 3. 订单详情

```
GET /api/Kitchen/order/detail?orderId={orderId}
```

**响应：** 与列表中单个订单结构一致，`items` 包含完整菜品信息。

---

### 4. 标记出餐

```
POST /api/Kitchen/dish/finish
```

**请求：**
```json
{
  "dishOrderDetailsId": 5001
}
```

**成功响应：**
```json
{
  "code": 0,
  "message": "success",
  "data": {
    "allFinished": true
  }
}
```

**失败响应（已处理过）：**
```json
{
  "code": -1,
  "message": "该菜品已被处理，无法重复操作"
}
```

**注意：** 标记出餐后，系统会自动检查该订单是否全部完成。若是，订单状态自动从 2→3，该订单将从"待出餐"列表消失，出现在"已完成"列表。

---

### 5. 取消出餐

```
POST /api/Kitchen/dish/cancel
```

**请求：**
```json
{
  "dishOrderDetailsId": 5001
}
```

**成功响应：**
```json
{
  "code": 0,
  "message": "success",
  "data": {
    "dishOrderDetailsId": 5001,
    "status": 3
  }
}
```

**失败响应（已处理过）：**
```json
{
  "code": -1,
  "message": "该菜品已被处理，无法重复操作"
}
```

**注意：** 只能取消"待出餐"(status=1) 的菜品。已出餐(status=2)的不可取消。取消后同样触发订单联动检查。

---

### 6. 今日统计

```
GET /api/Kitchen/today-statistics
```

**响应：**
```json
{
  "code": 0,
  "message": "success",
  "data": {
    "todayTotalAmount": 2880.00,
    "todayTotalOrder": 25,
    "todayFinishedOrder": 18,
    "todayPendingDish": 12,
    "todayFinishedDish": 45
  }
}
```

**字段说明：**

| 字段 | 类型 | 说明 |
|------|------|------|
| todayTotalAmount | decimal | 今日营业额（仅统计已出餐菜品金额） |
| todayTotalOrder | int | 今日订单总数 |
| todayFinishedOrder | int | 已全部出餐的订单数 |
| todayPendingDish | int | 待出餐菜品数（StatusId=1） |
| todayFinishedDish | int | 已出餐菜品数（StatusId=2） |

---

## 三、前端交互流程

### 待出餐列表页

1. 调用 `GET /order/list?type=2` 获取待出餐订单
2. 展示订单列表，每行显示桌号、订单号、时间、金额
3. 展开订单显示菜品列表，status=1 的菜品显示"出餐"/"取消"按钮
4. 点击"出餐" → `POST /dish/finish` → 刷新列表
5. 点击"取消" → `POST /dish/cancel` → 刷新列表

### 已完成列表页

1. 调用 `GET /order/list?type=3` 获取已完成订单
2. 展示历史订单，菜品状态均为 2 或 3

### 统计页

1. 调用 `GET /today-statistics` 获取实时数据
2. 展示今日营业额、订单数、待出餐/已出餐菜品数

---

## 四、错误码

| HTTP Status | code | 含义 |
|-------------|------|------|
| 200 | 0 | 成功 |
| 200 | -1 | 业务失败（message 中有具体原因） |
| 200 | 400 | 参数错误 |
| 200 | 404 | 数据不存在 |
