# 农场小程序 API 接口文档

> 文档版本：2026-03-12  
> 项目位置：`C:\Users\Administrator\Desktop\WebAPI`  
> 接口基础地址：`http://localhost:5162`  
> 文档格式：Markdown + `details/summary` 收缩块  
> 模板存储位置：`C:\Users\Administrator\Desktop\WebAPI\templates\接口文档Markdown模板.md`

---

## 1. 基本说明

| 项目 | 说明 |
| --- | --- |
| 接口统一前缀 | `/api` |
| 请求格式 | `application/json` |
| 返回格式 | 统一 JSON 结构 |
| 认证方式 | JWT Bearer Token |
| Token 请求头 | `Authorization: Bearer {token}` |

### 统一返回结构

```json
{
  "code": 0,
  "message": "success",
  "data": {}
}
```

### 常用业务码

| code | 说明 |
| --- | --- |
| 0 | 成功 |
| 401 | 登录状态无效或未授权 |
| 404 | 资源不存在 |
| 1003 | 购物车商品不存在 |
| 1004 | 订单不存在 |
| -1 | 服务器异常，请稍后重试 |

---

## 2. 认证模块

<details>
<summary><strong>POST /api/auth/login</strong> - 一键登录</summary>

### 中文注释
- 用于设备一键登录。
- 首次登录会自动创建用户。
- 返回 `token` 和用户信息。

### 是否鉴权
- 否

### 请求参数

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| deviceId | string | 是 | 设备唯一标识 |
| platform | string | 否 | 平台信息 |
| version | string | 否 | 版本号 |

### 请求示例

```json
{
  "deviceId": "wx-device-001",
  "platform": "wx小程序",
  "version": "1.0.0"
}
```

### 成功响应

```json
{
  "code": 0,
  "message": "success",
  "data": {
    "token": "xxxxx",
    "expireMinutes": 43200,
    "user": {
      "id": 1,
      "userNo": "wx-device-001",
      "nickname": "",
      "avatar": "",
      "phone": ""
    },
    "userInfo": {
      "id": 1,
      "userNo": "wx-device-001",
      "nickname": "",
      "avatar": "",
      "phone": ""
    }
  }
}
```

</details>

<details>
<summary><strong>POST /api/auth/wechat</strong> - 微信登录</summary>

### 中文注释
- 用于微信授权登录。
- 前端通常先调用 `wx.login` 获取 `code`。
- 当前开发联调阶段，后端使用 `code` 作为微信唯一标识的一部分。

### 是否鉴权
- 否

### 请求参数

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| code | string | 是 | 微信登录 code |
| encryptedData | string | 否 | 加密数据，可为空 |
| iv | string | 否 | 解密向量，可为空 |
| nickname | string | 否 | 微信昵称 |
| avatar | string | 否 | 微信头像地址 |

### 请求示例

```json
{
  "code": "081xxxxxxx",
  "encryptedData": "",
  "iv": "",
  "nickname": "微信用户",
  "avatar": "https://thirdwx.qlogo.cn/xxx"
}
```

### 成功响应

```json
{
  "code": 0,
  "message": "success",
  "data": {
    "token": "xxxxx",
    "expireMinutes": 43200,
    "user": {
      "id": 2,
      "userNo": "wx_1741766400000",
      "nickname": "微信用户",
      "avatar": "https://thirdwx.qlogo.cn/xxx",
      "phone": ""
    },
    "userInfo": {
      "id": 2,
      "userNo": "wx_1741766400000",
      "nickname": "微信用户",
      "avatar": "https://thirdwx.qlogo.cn/xxx",
      "phone": ""
    }
  }
}
```

</details>

<details>
<summary><strong>POST /api/auth/phone</strong> - 手机号登录</summary>

### 中文注释
- 用于手机号登录或绑定手机号。
- 当前后端支持直接按手机号创建或更新用户。

### 是否鉴权
- 否

### 请求参数

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| phone | string | 是 | 手机号 |
| code | string | 否 | 验证码 |

### 请求示例

```json
{
  "phone": "13800138000",
  "code": "123456"
}
```

</details>

<details>
<summary><strong>GET /api/auth/check</strong> - 检查登录状态</summary>

### 中文注释
- 用于校验当前 token 是否有效。
- 前端进入登录页或应用启动时可调用。

### 是否鉴权
- 是

### 请求头

```text
Authorization: Bearer {token}
```

</details>

<details>
<summary><strong>POST /api/auth/logout</strong> - 退出登录</summary>

### 中文注释
- 用于前端退出登录。
- 当前后端返回成功结果，token 失效控制主要依赖前端清理本地缓存。

### 是否鉴权
- 是

</details>

---

## 3. 首页模块

<details>
<summary><strong>GET /api/home/index</strong> - 首页数据</summary>

### 中文注释
- 返回首页轮播图、功能按钮、农场优选商品、热销菜品。
- 首页页面初始化时调用。

### 是否鉴权
- 否

</details>

---

## 4. 农场优选模块

<details>
<summary><strong>GET /api/farm-goods/index</strong> - 农场优选首页</summary>

### 中文注释
- 返回农场优选首页所需数据。
- 包含轮播图、分类列表、今日推荐、热门商品。

### 是否鉴权
- 否

</details>

<details>
<summary><strong>GET /api/farm-goods/category</strong> - 分类商品列表</summary>

### 中文注释
- 按分类分页获取商品列表。
- 农场优选分类切换时调用。

### 是否鉴权
- 否

### 查询参数

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| categoryId | int | 是 | 分类 ID |
| page | int | 否 | 页码，默认 1 |
| pageSize | int | 否 | 每页数量，默认 10 |

</details>

<details>
<summary><strong>GET /api/farm-goods/search</strong> - 商品搜索</summary>

### 中文注释
- 按关键词搜索农场商品。
- 支持分页。

### 是否鉴权
- 否

### 查询参数

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| keyword | string | 是 | 搜索关键词 |
| page | int | 否 | 页码 |
| pageSize | int | 否 | 每页数量 |

</details>

---

## 5. 商城商品模块

<details>
<summary><strong>GET /api/goods/detail</strong> - 商城商品详情</summary>

### 中文注释
- 用于商城商品详情页。
- 根据 `goodsId` 返回单个商品详细信息。

### 是否鉴权
- 否

### 查询参数

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| goodsId | int | 是 | 商品 ID |

</details>

---

## 6. 商城购物车模块

<details>
<summary><strong>GET /api/cart/list</strong> - 获取商城购物车</summary>

### 中文注释
- 获取当前登录用户的商城购物车列表。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>POST /api/cart/add</strong> - 加入商城购物车</summary>

### 中文注释
- 将商品加入商城购物车。
- 如果已存在同商品，则累加数量。

### 是否鉴权
- 是

### 请求示例

```json
{
  "goodsId": 1,
  "count": 2
}
```

</details>

<details>
<summary><strong>PUT /api/cart/update</strong> - 修改商城购物车数量</summary>

### 中文注释
- 修改某一条商城购物车记录的购买数量。

### 是否鉴权
- 是

### 请求示例

```json
{
  "cartId": 3,
  "count": 5
}
```

</details>

<details>
<summary><strong>DELETE /api/cart/delete</strong> - 删除商城购物车商品</summary>

### 中文注释
- 删除指定购物车项。

### 是否鉴权
- 是

### 请求示例

```json
{
  "cartId": 3
}
```

</details>

<details>
<summary><strong>DELETE /api/cart/clear</strong> - 清空商城购物车</summary>

### 中文注释
- 清空当前用户所有商城购物车项。

### 是否鉴权
- 是

</details>

---

## 7. 商城订单模块

<details>
<summary><strong>POST /api/order/create</strong> - 创建商城订单</summary>

### 中文注释
- 使用商城购物车中的商品创建订单。
- 创建成功后会清除对应购物车项。

### 是否鉴权
- 是

### 请求参数

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| addressId | int | 是 | 收货地址 ID |
| cartIds | int[] | 是 | 购物车 ID 数组 |
| totalPrice | decimal | 否 | 订单总价 |
| remark | string | 否 | 订单备注 |

### 请求示例

```json
{
  "addressId": 7,
  "cartIds": [3],
  "totalPrice": 0,
  "remark": "商城联调"
}
```

</details>

<details>
<summary><strong>GET /api/order/list</strong> - 商城订单列表</summary>

### 中文注释
- 按状态分页查询当前用户的商城订单。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>GET /api/order/detail</strong> - 商城订单详情</summary>

### 中文注释
- 根据订单 ID 查询订单详情。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>PUT /api/order/cancel</strong> - 取消商城订单</summary>

### 中文注释
- 将订单状态改为取消。

### 是否鉴权
- 是

### 请求示例

```json
{
  "orderId": 1
}
```

</details>

---

## 8. 活动模块

<details>
<summary><strong>GET /api/activity/list</strong> - 活动列表</summary>

### 中文注释
- 返回活动列表数据。
- 当前按 `all`、`picking`、`camping` 分类返回。

### 是否鉴权
- 否

</details>

<details>
<summary><strong>GET /api/activity/detail</strong> - 活动详情</summary>

### 中文注释
- 根据活动 ID 查询活动详情。

### 是否鉴权
- 否

</details>

---

## 9. 点餐模块

<details>
<summary><strong>GET /api/order/getOrderData</strong> - 点餐首页数据</summary>

### 中文注释
- 返回点餐页面的分类和菜品列表。
- 可匿名访问。

### 是否鉴权
- 否

</details>

<details>
<summary><strong>GET /api/order/goods/detail</strong> - 点餐商品详情</summary>

### 中文注释
- 返回单个菜品详情。
- 可匿名访问。

### 是否鉴权
- 否

</details>

<details>
<summary><strong>GET /api/order/cart</strong> - 点餐购物车</summary>

### 中文注释
- 获取当前用户的点餐购物车内容。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>POST /api/order/cart/add</strong> - 点餐加入购物车</summary>

### 中文注释
- 将菜品加入点餐购物车。

### 是否鉴权
- 是

### 请求示例

```json
{
  "goodsId": 1,
  "count": 1
}
```

</details>

<details>
<summary><strong>PUT /api/order/cart/update</strong> - 点餐修改购物车</summary>

### 中文注释
- 修改点餐购物车某个菜品数量。

### 是否鉴权
- 是

### 请求示例

```json
{
  "goodsId": 1,
  "quantity": 3
}
```

</details>

<details>
<summary><strong>DELETE /api/order/cart/clear</strong> - 清空点餐购物车</summary>

### 中文注释
- 清空当前用户的点餐购物车。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>POST /api/order/submit</strong> - 提交点餐订单</summary>

### 中文注释
- 将当前点餐购物车内容提交为订单。
- 创建成功后清空点餐购物车。

### 是否鉴权
- 是

### 请求示例

```json
{
  "remark": "少辣，不要香菜"
}
```

</details>

---

## 10. 用户中心模块

<details>
<summary><strong>GET /api/user/profile</strong> - 获取个人资料</summary>

### 中文注释
- 获取当前登录用户资料。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>PUT /api/user/profile</strong> - 修改个人资料</summary>

### 中文注释
- 修改昵称、头像等个人信息。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>GET /api/user/address</strong> - 获取地址列表</summary>

### 中文注释
- 获取当前用户所有收货地址。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>POST /api/user/address</strong> - 新增地址</summary>

### 中文注释
- 新增一条收货地址。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>PUT /api/user/address</strong> - 修改地址</summary>

### 中文注释
- 根据地址 ID 修改收货地址信息。

### 是否鉴权
- 是

</details>

<details>
<summary><strong>DELETE /api/user/address</strong> - 删除地址</summary>

### 中文注释
- 删除指定收货地址。

### 是否鉴权
- 是

</details>

---

## 11. 接口汇总表

| 模块 | 方法 | 路径 | 是否登录 |
| --- | --- | --- | --- |
| 认证 | POST | `/api/auth/login` | 否 |
| 认证 | POST | `/api/auth/wechat` | 否 |
| 认证 | POST | `/api/auth/phone` | 否 |
| 认证 | GET | `/api/auth/check` | 是 |
| 认证 | POST | `/api/auth/logout` | 是 |
| 首页 | GET | `/api/home/index` | 否 |
| 农场优选 | GET | `/api/farm-goods/index` | 否 |
| 农场优选 | GET | `/api/farm-goods/category` | 否 |
| 农场优选 | GET | `/api/farm-goods/search` | 否 |
| 商城商品 | GET | `/api/goods/detail` | 否 |
| 商城购物车 | GET | `/api/cart/list` | 是 |
| 商城购物车 | POST | `/api/cart/add` | 是 |
| 商城购物车 | PUT | `/api/cart/update` | 是 |
| 商城购物车 | DELETE | `/api/cart/delete` | 是 |
| 商城购物车 | DELETE | `/api/cart/clear` | 是 |
| 商城订单 | POST | `/api/order/create` | 是 |
| 商城订单 | GET | `/api/order/list` | 是 |
| 商城订单 | GET | `/api/order/detail` | 是 |
| 商城订单 | PUT | `/api/order/cancel` | 是 |
| 活动 | GET | `/api/activity/list` | 否 |
| 活动 | GET | `/api/activity/detail` | 否 |
| 点餐 | GET | `/api/order/getOrderData` | 否 |
| 点餐 | GET | `/api/order/goods/detail` | 否 |
| 点餐 | GET | `/api/order/cart` | 是 |
| 点餐 | POST | `/api/order/cart/add` | 是 |
| 点餐 | PUT | `/api/order/cart/update` | 是 |
| 点餐 | DELETE | `/api/order/cart/clear` | 是 |
| 点餐 | POST | `/api/order/submit` | 是 |
| 用户 | GET | `/api/user/profile` | 是 |
| 用户 | PUT | `/api/user/profile` | 是 |
| 用户 | GET | `/api/user/address` | 是 |
| 用户 | POST | `/api/user/address` | 是 |
| 用户 | PUT | `/api/user/address` | 是 |
| 用户 | DELETE | `/api/user/address` | 是 |
