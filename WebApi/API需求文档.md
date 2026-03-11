# WebAPI 接口文档

以下文档基于 `WebApi/WebApplication1/WebApplication1/Controllers` 中的源代码生成，涵盖示例接口、请求参数和返回结构。

> **说明**：项目目前使用伪数据返回值。生产环境请根据实际业务逻辑补充验证、错误处理和持久化。

---

## 通用类型

- 所有接口都返回 `ApiResponse<T>` 结构：
  ```json
  {
    "code": 0,
    "message": "",
    "data": /* T */
  }
  ```
- 常见 DTO 定义在 `WebApplication1/Models/Dtos.cs`，如 `UserDto`、`GoodsDto`、`AcreDto`、`ActivityDto` 等。
- 分页结果使用 `PagedResult<T>` 包装。

---

## 认证与登录（`/api/auth`）

| 方法 | 路径 | 描述 | 参数 | 返回示例 |
|------|------|------|------|----------|
| POST | `/login` | 普通用户名/密码登录 | `{ username, password }` | `{ token, userInfo }` |
| POST | `/wechat` | 微信授权登录 | `{ code }` | `{ token, userInfo }` |
| POST | `/phone` | 手机号登录 | `{ phone, code }` | `{ token, userInfo }` |
| POST | `/logout` | 注销 | 无 | `null` |
| GET  | `/check` | 检查当前会话 | 无 | `{ isLoggedIn, userInfo }` |

**示例：登录**
```http
POST /api/auth/login
Content-Type: application/json

{ "username": "test", "password": "1234" }
```
```json
{
  "code": 0,
  "message": "",
  "data": {
    "token": "fake-token-...",
    "userInfo": { "id": "...", "nickname": "游客", "avatar": "" }
  }
}
```

---

## 用户 (`/api/user`)

| 方法 | 路径 | 描述 | 参数 | 返回示例 |
|------|------|------|------|----------|
| GET  | `/` | 获取当前用户信息 | 无 | `UserDto` |
| PUT  | `/` | 更新基本信息 | `UserDto` | `null` |
| GET  | `/statistics` | 统计数据 | 无 | `{ OrderCount, AdoptedAcres, Points }` |

### 地址管理
| 方法 | 路径 | 描述 | 参数 | 返回 |
|------|------|------|------|------|
| GET  | `/address` | 列表 | 无 | `AddressDto[]` |
| POST | `/address` | 添加 | `AddressDto` | `null` |
| PUT  | `/address` | 修改 | `AddressDto` | `null` |
| DELETE| `/address` | 删除 | `?id=123` | `null` |

`AddressDto`:
```json
{ "id": 1, "name": "张三", "phone": "138...", "province": "北京市", ... }
```

---

## 购物车 (`/api/cart`)

| 方法 | 路径 | 描述 | 请求 | 说明 |
|------|------|------|------|------|
| GET  | `/list` | 查询购物车项 | 无 | 返回 `CartItemDto[]` |
| POST | `/add` | 添加商品 | body 任意（简化） |  |
| PUT  | `/update` | 更新商品 | body 任意 |  |
| DELETE | `/delete` | 删除单项 | body 任意 |  |
| DELETE | `/clear` | 清空购物车 | 无 |  |

`CartItemDto` 包含 `Id`、`Quantity`、`Goods` 等。

---

## 商品 (`/api/goods`)

| 方法 | 路径                            | 描述 | 查询/路径参数 | 返回 |
|------|---------------------------------|------|---------------|------|
| GET  | `/`                             | 列表（分页） | `pageIndex,pageSize,categoryId,keyword` | `PagedResult<GoodsDto>` |
| GET  | `/{id}`                         | 详情 | 路径 id=GUID | `GoodsDto` |
| GET  | `/detail`                       | 详情（兼容） | `?goodsId=GUID` | `GoodsDto` |
| GET  | `/recommend`                    | 推荐商品 | 无 | `GoodsDto[]` |

---

## 田地/地块 (`/api/acres`)

| 方法 | 路径 | 描述 | 参数 | 返回 |
|------|------|------|------|------|
| GET  | `/` | 列表，可按状态筛选 | `?status=&pageIndex=&pageSize=` | `PagedResult<AcreDto>` |
| GET  | `/{id}` | 详情 | id 可为 GUID 或数字字符串 | `AcreDto` |
| POST | `/{id}/adopt` | 认养某地块 | body 任意（months、remark 等） | `null` |
| GET  | `/{id}/logs` | 操作记录 | | `object[]` |

---

## 活动 (`/api/activities`)

| 方法 | 路径 | 描述 | 参数 | 返回 |
|------|------|------|------|------|
| GET  | `/` | 活动列表，支持状态过滤 | `?pageIndex=&pageSize=&status=` | `PagedResult<ActivityDto>` |
| GET  | `/{id}` | 活动详情 | 路径 id | `ActivityDto` |

---

## 演示专用接口 (`/api/demo`)

> 这些接口用于前端 demo 页面，与上述正式接口结构类似，返回硬编码样本数据。

- **首页** `GET /home` → 轮播、按钮、推荐商品等
- **商品列表** `GET /goods?category=`
- **商品详情** `GET /goods/{id}`
- **地块列表/详情/认养** `/acres`, `/acres/{id}`, `POST /acres/{id}/adopt`
- **活动** `GET /activities`
- **购物车** `GET /cart`, `POST /cart/items`, `PUT /cart/items/{id}`, `DELETE /cart/items/{id}`, `DELETE /cart`
- **订单** `GET /orders`, `POST /orders` (`{ orderId }`)
- **用户** `GET /profile` (代理 `/api/user`)

具体返回结构可参考源代码中的匿名对象。

---

## 其他控制器

以下控制器为 MVC 视图或尚未实现具体逻辑：

- `FarmController`：返回视图 `Index`
- `ConfigController`、`CommodityController`、`PaymentsController` 等均遵循类似风格，可按需扩展。

---

## 使用说明

1. **地址前缀**：所有接口在本机开发环境默认以 `http://localhost:{port}/api/...` 访问。
2. **请求头**：JSON 接口使用 `Content-Type: application/json`。
3. **认证**：当前 demo 使用伪 token，真实场景应添加 JWT/Session 中间件。
4. **错误处理**：本示例只演示成功结果，建议统一 `ApiResponse` 中 `code`/`message` 用于错误信息。

---

**附录**：可把此文档整合进 README 或部署成 Swagger/OpenAPI。代码中已有简单注释可作为生成 Swagger 的基础。