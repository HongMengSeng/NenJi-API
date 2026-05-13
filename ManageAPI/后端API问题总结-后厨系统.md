# 后厨出餐管理系统 — 后端 API 问题总结

## 问题一：订单列表接口 type 参数值不匹配

**接口**: `GET /api/Kitchen/order/list?type={value}`

**当前行为**：
- `type=2` → 待出餐 ✅
- `type=4` → **接口报错**: "type 参数值不正确，仅支持 2 (待出餐) 或 3 (已完成)"

**期望行为**：
- `type=2` → 待出餐
- `type=3` → 已完成
- `type=4` → 也应该是已完成（代码文档中写的是 type=4 对应已完成），请统一为标准

**影响范围**：前端 `dashboard.js` 已临时改为传 `type=3`，但建议后端统一常量定义，避免混淆。

---

## 问题二：今日营业额统计接口未按菜品实际出餐状态计算

**接口**: `GET /api/Kitchen/today-statistics`

**返回示例**：
```json
{
  "code": 0,
  "message": "success",
  "data": {
    "todayTotalAmount": 0,
    "todayTotalOrder": 0,
    "todayFinishedOrder": 0,
    "todayPendingDish": 0,
    "todayFinishedDish": 0
  }
}
```

**当前问题**：`todayTotalAmount` 始终返回 0（或未按预期累加），经排查后端未根据菜品实际出餐状态计算营业额。

**期望逻辑**：

```
今日营业额 = SUM(已出餐菜品的 price × quantity)
```

- **菜品状态定义**：
  - `status = 1` → 待出餐（Pending）
  - `status = 3` → 已取消（Cancelled）
  - `status = 4` → 已出餐（Finished）

- **营业额计算规则**：
  - 只统计 `status = 4`（已出餐）的菜品
  - `status = 3`（已取消）的菜品**不计入**营业额
  - 金额 = 菜品单价 × 数量
  - 统计范围：**当天所有订单**中已出餐的菜品

- **示例场景**：
  > 一笔订单有 3 道菜：A(¥30)、B(¥20)、C(¥10)
  > - 出餐了 A、B → 今日营业额 = ¥50
  > - 取消出餐 C → C 不计入
  > - 若全部出餐 → 今日营业额 = ¥60
  > - 若全部取消 → 今日营业额 = ¥0

---

## 补充说明

由于问题二后端未能正确返回统计数据，前端已改为在本地从前端拉取的订单数据中自行计算营业额（只统计 status=4 的菜品）。但这只是临时方案，后续后端修复后建议前端仍以后端接口为准，请及时同步修复进度。
