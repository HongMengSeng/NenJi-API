农产品商城 API 需求文档
文档信息
表格
项目	内容
文档版本	v1.0
编写日期	2026-03-17
接口用途	农产品商城商品数据
请求方式	GET
响应格式	JSON
1. 接口概述
该接口用于获取农产品商城的商品分类列表和对应分类下的商品详情数据，为前端页面提供完整的商品展示数据源。
2. 接口详情
2.1 基础信息
接口 URL：/api/goods/list
请求方法：GET
请求参数：无
响应状态码：200（成功）
2.2 响应数据结构
json
{
  "code": 200,
  "message": "success",
  "data": {
    "categories": [
      {
        "id": "string",
        "name": "string"
      }
    ],
    "goodsList": {
      "[categoryId]": [
        {
          "id": "number",
          "name": "string",
          "image": "string",
          "price": "number",
          "sold": "number",
          "stock": "number"
        }
      ]
    }
  }
}
2.3 字段说明
表格
层级	字段名	类型	说明	示例值
根节点	code	number	响应状态码	200
根节点	message	string	响应信息	"success"
根节点	data	object	核心数据体	-
data	categories	array	商品分类列表	-
categories[i]	id	string	分类唯一标识	"vegetables"
categories[i]	name	string	分类名称	"新鲜蔬菜"
data	goodsList	object	按分类聚合的商品列表	-
goodsList[categoryId]	array	对应分类下的商品数组	-	
goodsList[categoryId][i]	id	number	商品唯一 ID	1
goodsList[categoryId][i]	name	string	商品名称	"有机生菜"
goodsList[categoryId][i]	image	string	商品图片 URL	图片链接
goodsList[categoryId][i]	price	number	商品价格（元）	30
goodsList[categoryId][i]	sold	number	已售数量	150
goodsList[categoryId][i]	stock	number	库存数量	30
2.4 完整响应示例（与虚拟数据一致）
json
{
  "code": 200,
  "message": "success",
  "data": {
    "categories": [
      { "id": "vegetables", "name": "新鲜蔬菜" },
      { "id": "meat", "name": "肉类产品" },
      { "id": "eggs", "name": "禽蛋产品" },
      { "id": "dairy", "name": "乳制品" },
      { "id": "staple", "name": "主食" }
    ],
    "goodsList": {
      "vegetables": [
        {
          "id": 1,
          "name": "有机生菜",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20organic%20lettuce&image_size=square",
          "price": 30,
          "sold": 150,
          "stock": 30
        },
        {
          "id": 2,
          "name": "农家西红柿",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20tomatoes&image_size=square",
          "price": 30,
          "sold": 200,
          "stock": 30
        },
        {
          "id": 3,
          "name": "新鲜黄瓜",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20cucumbers&image_size=square",
          "price": 30,
          "sold": 180,
          "stock": 30
        }
      ],
      "meat": [
        {
          "id": 4,
          "name": "土猪肉",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20pork%20meat&image_size=square",
          "price": 30,
          "sold": 100,
          "stock": 30
        },
        {
          "id": 5,
          "name": "农家土鸡",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20chicken&image_size=square",
          "price": 30,
          "sold": 80,
          "stock": 30
        }
      ],
      "eggs": [
        {
          "id": 6,
          "name": "土鸡蛋",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20eggs&image_size=square",
          "price": 30,
          "sold": 300,
          "stock": 30
        },
        {
          "id": 7,
          "name": "鸭蛋",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20duck%20eggs&image_size=square",
          "price": 30,
          "sold": 150,
          "stock": 30
        },
        {
          "id": 8,
          "name": "鹅蛋",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20goose%20eggs&image_size=square",
          "price": 30,
          "sold": 50,
          "stock": 30
        }
      ],
      "dairy": [
        {
          "id": 9,
          "name": "新鲜牛奶",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20milk&image_size=square",
          "price": 30,
          "sold": 200,
          "stock": 30
        },
        {
          "id": 10,
          "name": "农家酸奶",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=homemade%20yogurt&image_size=square",
          "price": 30,
          "sold": 180,
          "stock": 30
        }
      ],
      "staple": [
        {
          "id": 11,
          "name": "农家大米",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20rice&image_size=square",
          "price": 30,
          "sold": 250,
          "stock": 30
        },
        {
          "id": 12,
          "name": "手工面条",
          "image": "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=homemade%20noodles&image_size=square",
          "price": 30,
          "sold": 150,
          "stock": 30
        }
      ]
    }
  }
}
3. 接口要求
接口返回的数据结构、字段名、字段类型必须与上述示例完全一致
初始数据值使用文档中提供的虚拟数据，后续可替换为真实数据库数据
接口响应时间控制在 500ms 以内
确保所有图片 URL 可正常访问（或后续替换为真实图片地址）
接口需支持跨域访问（如为前端调用）
4. 错误响应示例
json
{
  "code": 500,
  "message": "服务器内部错误",
  "data": null
}