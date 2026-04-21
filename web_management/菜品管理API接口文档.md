# 菜品管理 API 接口文档

本文档基于当前页面 [dish.html](/e:/Kuade-NengJiFarm/web_management/dish.html)、[dish-add.html](/e:/Kuade-NengJiFarm/web_management/dish-add.html)、[dish-edit.html](/e:/Kuade-NengJiFarm/web_management/dish-edit.html) 的字段和交互整理，覆盖菜品列表、新增、编辑、删除。

说明：`dish.html` 页面同时带有餐桌管理模块，但本文档只描述菜品管理接口。

## 一、公共约定

### 1.1 请求头

| 字段 | 是否必传 | 类型 | 说明 |
|---|---|---|---|
| `Content-Type` | 是 | String | 固定为 `application/json` |
| `token` | 建议 | String | 后台登录后返回的认证令牌 |

### 1.2 统一响应结构

| 字段 | 类型 | 说明 |
|---|---|---|
| `code` | Number | `200` 表示成功 |
| `message` | String | 提示文案 |
| `data` | Object / Array / Null | 业务数据 |

```json
{
  "code": 200,
  "message": "操作成功",
  "data": {}
}
```

### 1.3 时间格式

菜品时间字段建议统一使用 `yyyy-MM-dd HH:mm` 格式。

- 示例：`2026-04-01 12:00`
- 示例：`2026-12-01 09:30`

## 二、菜品字段说明

| 字段 | 类型 | 是否必传 | 示例 | 说明 |
|---|---|---|---|---|
| `id` | String | 编辑/删除必传 | `20260301120000` | 菜品唯一 ID |
| `name` | String | 是 | `宫保鸡丁` | 菜品名称 |
| `price` | Number | 是 | `28.5` | 菜品价格 |
| `stock` | Number | 列表建议返回 | `25` | 库存数量 |
| `status` | String | 是 | `已上架` | 状态，`已上架` / `已下架` |
| `image` | String | 列表建议返回 | `https://example.com/dish-cover.jpg` | 列表主图 |
| `coverImage` | String | 新增/编辑建议返回 | `https://example.com/dish-cover.jpg` | 菜品封面 |
| `carouselImages` | String[] | 否 | `["https://example.com/a.jpg"]` | 轮播图，前端最多 5 张 |
| `specImages` | String[] | 否 | `["https://example.com/spec.jpg"]` | 规格图，前端最多 5 张 |
| `description` | String | 否 | `经典川味，下饭爽口` | 菜品介绍 |
| `uploadTime` | String | 列表建议返回 | `2026-04-01 12:00` | 上架时间或创建时间 |

说明：

- 当前新增页、编辑页没有库存输入框，所以 `stock` 可以由后端默认值生成，或由库存模块单独维护
- 为了减少前端改动，详情接口建议同时返回 `image` 与 `coverImage`

## 三、接口 1：获取菜品列表

| 项目 | 内容 |
|---|---|
| 接口名称 | 获取菜品列表 |
| 请求路径 | `/api/dish/list` |
| 请求方式 | `GET` |
| 接口说明 | 用于菜品管理页初始化、分页、搜索、刷新 |

### Query 参数

| 字段 | 是否必传 | 类型 | 示例 | 说明 |
|---|---|---|---|---|
| `pageNum` | 是 | Number | `1` | 页码 |
| `pageSize` | 是 | Number | `15` | 每页条数 |
| `keyword` | 否 | String | `宫保` | 按菜品 ID 或菜品名称模糊搜索 |

### 成功响应示例

```json
{
  "code": 200,
  "message": "获取成功",
  "data": {
    "records": [
      {
        "id": "20240601120000",
        "name": "宫保鸡丁",
        "image": "https://example.com/dish/gongbao-cover.jpg",
        "coverImage": "https://example.com/dish/gongbao-cover.jpg",
        "uploadTime": "2026-04-01 12:00",
        "price": 28.5,
        "stock": 25,
        "status": "已上架"
      },
      {
        "id": "20240701120000",
        "name": "麻婆豆腐",
        "image": "https://example.com/dish/mapo-cover.jpg",
        "coverImage": "https://example.com/dish/mapo-cover.jpg",
        "uploadTime": "2026-04-02 12:00",
        "price": 18.9,
        "stock": 30,
        "status": "已上架"
      }
    ],
    "total": 2,
    "pageNum": 1,
    "pageSize": 15,
    "pages": 1
  }
}
```

## 四、接口 2：获取菜品详情

| 项目 | 内容 |
|---|---|
| 接口名称 | 获取菜品详情 |
| 请求路径 | `/api/dish/detail` |
| 请求方式 | `GET` |
| 接口说明 | 用于 `dish-edit.html?id=菜品ID` 回填表单 |

### Query 参数

| 字段 | 是否必传 | 类型 | 示例 | 说明 |
|---|---|---|---|---|
| `id` | 是 | String | `20240601120000` | 菜品 ID |

### 成功响应示例

```json
{
  "code": 200,
  "message": "获取成功",
  "data": {
    "id": "20240601120000",
    "name": "宫保鸡丁",
    "price": 28.5,
    "status": "已上架",
    "stock": 25,
    "image": "https://example.com/dish/gongbao-cover.jpg",
    "coverImage": "https://example.com/dish/gongbao-cover.jpg",
    "carouselImages": [
      "https://example.com/dish/gongbao-banner-1.jpg",
      "https://example.com/dish/gongbao-banner-2.jpg"
    ],
    "specImages": [
      "https://example.com/dish/gongbao-spec-1.jpg"
    ],
    "description": "经典川味，下饭爽口",
    "uploadTime": "2026-04-01 12:00"
  }
}
```

## 五、接口 3：新增菜品

| 项目 | 内容 |
|---|---|
| 接口名称 | 新增菜品 |
| 请求路径 | `/api/dish/add` |
| 请求方式 | `POST` |
| 接口说明 | 用于 `dish-add.html` 提交新增表单 |

### Body 参数

| 字段 | 是否必传 | 类型 | 示例 | 说明 |
|---|---|---|---|---|
| `name` | 是 | String | `宫保鸡丁` | 菜品名称 |
| `price` | 是 | Number | `28.5` | 菜品价格 |
| `status` | 是 | String | `已上架` | 状态 |
| `coverImage` | 是 | String | `https://example.com/dish/gongbao-cover.jpg` | 封面图 |
| `carouselImages` | 否 | String[] | `["https://example.com/a.jpg"]` | 轮播图 |
| `specImages` | 否 | String[] | `["https://example.com/spec.jpg"]` | 规格图 |
| `description` | 否 | String | `经典川味，下饭爽口` | 菜品介绍 |
| `stock` | 否 | Number | `25` | 初始库存，当前页面未输入时可由后端默认 |

### 请求示例

```json
{
  "name": "宫保鸡丁",
  "price": 28.5,
  "status": "已上架",
  "coverImage": "https://example.com/dish/gongbao-cover.jpg",
  "carouselImages": [
    "https://example.com/dish/gongbao-banner-1.jpg",
    "https://example.com/dish/gongbao-banner-2.jpg"
  ],
  "specImages": [
    "https://example.com/dish/gongbao-spec-1.jpg"
  ],
  "description": "经典川味，下饭爽口",
  "stock": 25
}
```

### 成功响应示例

```json
{
  "code": 200,
  "message": "新增成功",
  "data": {
    "id": "20260413103001"
  }
}
```

## 六、接口 4：编辑菜品

| 项目 | 内容 |
|---|---|
| 接口名称 | 编辑菜品 |
| 请求路径 | `/api/dish/edit` |
| 请求方式 | `PUT` |
| 接口说明 | 用于 `dish-edit.html` 提交编辑结果 |

### Body 参数

| 字段 | 是否必传 | 类型 | 示例 | 说明 |
|---|---|---|---|---|
| `id` | 是 | String | `20240601120000` | 菜品 ID |
| `name` | 是 | String | `宫保鸡丁` | 菜品名称 |
| `price` | 是 | Number | `29.9` | 菜品价格 |
| `status` | 是 | String | `已下架` | 状态 |
| `coverImage` | 是 | String | `https://example.com/dish/gongbao-cover.jpg` | 封面图 |
| `carouselImages` | 否 | String[] | `["https://example.com/a.jpg"]` | 轮播图 |
| `specImages` | 否 | String[] | `["https://example.com/spec.jpg"]` | 规格图 |
| `description` | 否 | String | `经典川味，下饭爽口` | 菜品介绍 |
| `stock` | 否 | Number | `18` | 库存 |

### 请求示例

```json
{
  "id": "20240601120000",
  "name": "宫保鸡丁",
  "price": 29.9,
  "status": "已下架",
  "coverImage": "https://example.com/dish/gongbao-cover.jpg",
  "carouselImages": [
    "https://example.com/dish/gongbao-banner-1.jpg"
  ],
  "specImages": [
    "https://example.com/dish/gongbao-spec-1.jpg"
  ],
  "description": "经典川味，下饭爽口",
  "stock": 18
}
```

## 七、接口 5：删除菜品

| 项目 | 内容 |
|---|---|
| 接口名称 | 删除菜品 |
| 请求路径 | `/api/dish/delete` |
| 请求方式 | `POST` |
| 接口说明 | 用于列表页单条删除 |

### Body 参数

| 字段 | 是否必传 | 类型 | 示例 | 说明 |
|---|---|---|---|---|
| `id` | 是 | String | `20240601120000` | 菜品 ID |

```json
{
  "id": "20240601120000"
}
```

## 八、接口 6：批量删除菜品

说明：`dish.html` 顶部已有多选删除入口，后端建议直接支持批量删除。

| 项目 | 内容 |
|---|---|
| 接口名称 | 批量删除菜品 |
| 请求路径 | `/api/dish/deleteBatch` |
| 请求方式 | `POST` |

### Body 参数

| 字段 | 是否必传 | 类型 | 示例 | 说明 |
|---|---|---|---|---|
| `ids` | 是 | String[] | `["20240601120000","20240701120000"]` | 菜品 ID 集合 |

```json
{
  "ids": [
    "20240601120000",
    "20240701120000"
  ]
}
```

## 九、接口边界说明

- 列表页搜索只依赖 `id` 和 `name`
- 列表页表格至少依赖：`id`、`image`、`name`、`uploadTime`、`price`、`stock`、`status`
- 新增页和编辑页当前没有库存输入框，如果需要人工改库存，建议单独补一个库存修改接口

## 十、通用失败场景

| 场景 | 状态码 | 提示文案 |
|---|---|---|
| token 无效或过期 | `401` | 登录已过期，请重新登录 |
| 权限不足 | `403` | 权限不足，仅管理员可操作 |
| 参数错误 | `400` | 请求参数不完整或格式错误 |
| 菜品不存在 | `404` | 菜品不存在或已被删除 |
| 服务异常 | `500` | 服务器异常，请稍后重试 |
