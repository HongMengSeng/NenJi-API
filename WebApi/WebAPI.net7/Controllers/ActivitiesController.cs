using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivitiesController : ControllerBase
    {
        // Used by: demo/pages/activity/activity.js (活动列表)
        #region GetList - demo/pages/activity/activity.js
        [HttpGet]
        public ActionResult<ApiResponse<PagedResult<ActivityDto>>> GetList([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            var all = new[] {
        new ActivityDto {
            Id = new Guid("62ad1eec-ebca-451b-91a9-9a83c1e8a4c3"),
            Title = "农家研学活动报名中",
            Price = "门票: 10-20 ¥",
            Date = "2025.2.25-2025.3.6",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20playing%20football%20on%20farm&image_size=landscape_16_9",
            Category = "picking",
            Content = ""
        },
        new ActivityDto {
            Id = new Guid("d041c07b-6247-40a5-9af0-9475d0bd3ed3"),
            Title = "采摘活动报名中",
            Price = "门票: 10-50 ¥",
            Date = "2025.2.25-2025.3.6",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20lettuce%20field&image_size=landscape_16_9",
            Category = "picking",
            Content = ""
        },
        new ActivityDto {
            Id = new Guid("4cd15cd6-8e24-443a-aee0-390e3a653a15"),
            Title = "草莓采摘体验",
            Price = "门票: 30 ¥/人",
            Date = "2025.3.1-2025.4.30",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=strawberry%20picking&image_size=landscape_16_9",
            Category = "picking",
            Content = ""
        },
        new ActivityDto {
            Id = new Guid("d5f0de3d-bad3-48de-89a5-721e6e1050a2"),
            Title = "葡萄采摘节",
            Price = "门票: 50 ¥/人",
            Date = "2025.7.1-2025.8.31",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=grape%20picking&image_size=landscape_16_9",
            Category = "picking",
            Content = ""
        },
        new ActivityDto {
            Id = new Guid("74a27e95-9b0f-4205-ace4-8cf24e309bd4"),
            Title = "农场露营体验",
            Price = "费用: 120 ¥/晚",
            Date = "2025.4.1-2025.10.31",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20camping%20tent&image_size=landscape_16_9",
            Category = "camping",
            Content = ""
        },
        new ActivityDto {
            Id = new Guid("28dbeec8-8c6e-480d-9dff-b12ba3a41041"),
            Title = "篝火露营晚会",
            Price = "费用: 180 ¥/人",
            Date = "2025.5.1-2025.9.30",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=camping%20with%20campfire&image_size=landscape_16_9",
            Category = "camping",
            Content = ""
        }
    };

            IEnumerable<ActivityDto> items = all;
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                items = all.Where(a => a.Category == status);
            }

            var paged = new PagedResult<ActivityDto>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Total = items.Count(),
                Items = items
            };

            return ApiResponse<PagedResult<ActivityDto>>.Ok(paged);
        }
        #endregion

        // Used by: demo/pages/activity/activity.js (活动详情)
        #region Get - demo/pages/activity/activity.js
        [HttpGet("{id}")]
        public ActionResult<ApiResponse<ActivityDto>> Get(Guid id)
        {
            var all = new[] {
        new ActivityDto {
            Id = new Guid("62ad1eec-ebca-451b-91a9-9a83c1e8a4c3"),
            Title = "农家研学活动报名中",
            Price = "门票: 10-20 ¥",
            Date = "2025.2.25-2025.3.6",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=children%20playing%20football%20on%20farm&image_size=landscape_16_9",
            Category = "picking",
            Content = "农家研学活动是一项针对青少年的教育活动，通过参与农场劳动、了解农业知识，培养孩子们的动手能力和对自然的热爱。活动包括采摘体验、动物喂养、农产品制作等多个环节，让孩子们在快乐中学习。"
        },
        new ActivityDto {
            Id = new Guid("d041c07b-6247-40a5-9af0-9475d0bd3ed3"),
            Title = "采摘活动报名中",
            Price = "门票: 10-50 ¥",
            Date = "2025.2.25-2025.3.6",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=fresh%20lettuce%20field&image_size=landscape_16_9",
            Category = "picking",
            Content = "采摘活动是一项亲子互动的好选择，让您和家人一起体验采摘的乐趣，感受田园风光。农场提供多种时令蔬菜水果供您采摘，新鲜美味，健康营养。"
        },
        new ActivityDto {
            Id = new Guid("4cd15cd6-8e24-443a-aee0-390e3a653a15"),
            Title = "草莓采摘体验",
            Price = "门票: 30 ¥/人",
            Date = "2025.3.1-2025.4.30",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=strawberry%20picking&image_size=landscape_16_9",
            Category = "picking",
            Content = "草莓采摘体验是春季最受欢迎的活动之一，新鲜的草莓酸甜可口，营养丰富。在草莓大棚里，您可以亲手采摘最新鲜的草莓，感受春天的气息。"
        },
        new ActivityDto {
            Id = new Guid("d5f0de3d-bad3-48de-89a5-721e6e1050a2"),
            Title = "葡萄采摘节",
            Price = "门票: 50 ¥/人",
            Date = "2025.7.1-2025.8.31",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=grape%20picking&image_size=landscape_16_9",
            Category = "picking",
            Content = "葡萄采摘节是夏季的重头戏，农场种植了多种品种的葡萄，包括巨峰、玫瑰香、阳光玫瑰等。在葡萄架下，您可以品尝到最新鲜的葡萄，感受夏日的清凉。"
        },
        new ActivityDto {
            Id = new Guid("74a27e95-9b0f-4205-ace4-8cf24e309bd4"),
            Title = "农场露营体验",
            Price = "费用: 120 ¥/晚",
            Date = "2025.4.1-2025.10.31",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=farm%20camping%20tent&image_size=landscape_16_9",
            Category = "camping",
            Content = "农场露营体验是一种亲近自然的方式，在农场的草地上搭建帐篷，夜晚欣赏星空，清晨听着鸟鸣醒来。农场提供帐篷租赁服务，也可以自带帐篷。"
        },
        new ActivityDto {
            Id = new Guid("28dbeec8-8c6e-480d-9dff-b12ba3a41041"),
            Title = "篝火露营晚会",
            Price = "费用: 180 ¥/人",
            Date = "2025.5.1-2025.9.30",
            ImageUrl = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=camping%20with%20campfire&image_size=landscape_16_9",
            Category = "camping",
            Content = "篝火露营晚会是一种充满乐趣的户外活动，在篝火旁唱歌跳舞，品尝烤串，度过一个难忘的夜晚。农场提供全套露营装备和食材，让您的露营体验更加舒适。"
        }
    };

            var activity = all.FirstOrDefault(a => a.Id == id);
            if (activity == null)
            {
                return ApiResponse<ActivityDto>.Fail("活动不存在");
            }
            return ApiResponse<ActivityDto>.Ok(activity);
        }
        #endregion
    }
}
