# 农场优选与点餐模块后端接口需求文档

更新时间：2026-04-29

## 1. 通用约定

- 基础地址：`http://127.0.0.1:5000`
- 请求格式：除文件接口外，均使用 `application/json`
- 登录鉴权：需要登录的接口携带 `Authorization: Bearer <token>`
- 统一响应：

```json
{
  "code": 0,
  "message": "success",
  "data": {}
}
```

`code === 0` 表示成功；非 0 表示失败。

## 2. 农场优选

### 2.1 获取农场优选商品列表

`GET /api/farm-goods`

鉴权：不需要

Query：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `category` | string | 否 | 分类 ID，不传或 `all` 返回全部 |
| `page` | number | 否 | 页码，默认 1 |
| `pageSize` | number | 否 | 每页数量，默认 50 |

返回 `data`：

```json
{
  "category": "all",
  "categories": [
    { "id": "1", "name": "蔬菜", "color": "#4CAF50", "icon": "蔬" }
  ],
  "items": [
    {
      "id": "1",
      "name": "有机生菜",
      "price": 9.9,
      "originalPrice": 12.0,
      "image": "/api/file/image/Farm_32.jpg",
      "stock": 50,
      "sold": 238,
      "tags": ["有机", "新鲜"]
    }
  ],
  "hasMore": false
}
```

说明：后端不再把 `all` 作为分类项返回，前端可自行插入“全部商品”。

### 2.2 获取指定分类商品

`GET /api/farm-goods/category`

鉴权：不需要

Query：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `category` | string | 是 | 分类 ID |
| `page` | number | 否 | 页码 |
| `pageSize` | number | 否 | 每页数量 |

返回：`items` 商品数组，并兼容返回 `goodsList`、`list`。

### 2.3 搜索商品

`GET /api/farm-goods/search`

鉴权：不需要

Query：`keyword`、`page`、`pageSize`

返回：`items`、`goodsList`、`total`、`page`、`pageSize`、`hasMore`。

## 3. 商品详情

### 3.1 获取商品详情

`GET /api/goods/detail?goodsId={id}`

`GET /api/goods/{id}`

鉴权：不需要

返回 `data`：

```json
{
  "id": "1",
  "name": "有机生菜",
  "price": 9.9,
  "originalPrice": 12.0,
  "image": "/api/file/image/Farm_32.jpg",
  "detailImage": "/api/file/image/Farm_32.jpg",
  "description": "商品描述",
  "desc": "商品描述",
  "weight": "500g",
  "storage": "冷藏保存",
  "videoUrl": "",
  "sold": 238,
  "stock": 50,
  "tags": ["有机"],
  "swiperList": [
    { "id": 1, "image": "/api/file/image/Farm_32.jpg" }
  ]
}
```

## 4. 点餐菜单

### 4.1 获取点餐菜单数据

`GET /api/order`

鉴权：不需要

Query：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `categoryId` | string | 否 | 分类 ID，默认 `vegetables` |
| `page` | number | 否 | 页码 |
| `pageSize` | number | 否 | 每页数量 |

返回 `data`：

```json
{
  "currentCategory": "vegetables",
  "categories": [
    { "id": "vegetables", "name": "蔬菜" }
  ],
  "goodsList": [
    {
      "id": "101",
      "name": "农家番茄炒蛋",
      "price": 18,
      "image": "/api/file/image/food_101.jpg",
      "detailImage": "/api/file/image/food_101.jpg",
      "description": "餐品描述",
      "sold": 238,
      "stock": 20
    }
  ],
  "hasMore": false
}
```

## 5. 购物车

### 5.1 添加商品到购物车

`POST /api/cart/add`

鉴权：需要

请求体：

```json
{
  "goodsId": "1",
  "count": 1
}
```

返回：成功时 `code: 0`。

## 6. 创建订单

### 6.1 商品下单

`POST /api/order/create`

鉴权：需要

适用场景：商品详情“立即购买”、确认订单页商品来源下单。

请求体：

```json
{
  "sourceType": "goods",
  "sourceName": "商品",
  "quantity": 3,
  "totalPrice": 47.7,
  "address": {
    "id": "1",
    "name": "张三",
    "phone": "13800138000",
    "address": "广东省广州市天河区某路1号"
  },
  "items": [
    {
      "id": "1",
      "name": "有机生菜",
      "price": 9.9,
      "quantity": 3,
      "image": "/api/file/image/Farm_32.jpg"
    }
  ]
}
```

返回 `data`：

```json
{
  "orderId": "10001",
  "id": "10001",
  "orderNumber": "20260429123456789",
  "status": "pending",
  "totalPrice": 47.7
}
```

兼容旧请求体：`addressId + cartIds`、`addressId + goodsId + count`。

### 6.2 点餐下单

`POST /api/OrderDetails/create`

鉴权：需要

请求体：

```json
{
  "sourceType": "food",
  "sourceName": "点餐",
  "tableNumber": 3,
  "quantity": 4,
  "totalPrice": 86,
  "items": [
    { "id": "101", "name": "农家番茄炒蛋", "price": 18, "quantity": 2, "image": "/api/file/image/food_101.jpg" }
  ]
}
```

返回：`orderId` 必须非空。

## 7. 地址列表

`GET /api/address/list`

鉴权：需要

返回：

```json
[
  {
    "id": "1",
    "name": "张三",
    "phone": "13800138000",
    "province": "广东省",
    "city": "广州市",
    "district": "天河区",
    "address": "广东省广州市天河区某路1号",
    "isDefault": true
  }
]
```

## 8. 文件服务

### 8.1 图片

`GET /api/file/image/{name}`

鉴权：不需要

返回图片二进制。

### 8.2 视频

`GET /api/file/video/{name}`

鉴权：不需要

返回视频二进制，后端已启用 Range 请求支持。

