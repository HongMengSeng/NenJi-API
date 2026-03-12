using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;
using WebAPI.Dtos;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/acres")]
public class AcresController : ControllerBase
{
    private static readonly List<AcreDto> Acres =
    [
        new AcreDto
        {
            Id = "1",
            Name = "认购一亩田 A1",
            Status = "available",
            Price = "9999元/亩",
            Image = "https://img.freepik.com/free-photo/yellow-field-with-lines_1127-3388.jpg",
            Description = "认购一亩田是农场推出的共享农业体验项目，让用户参与种植、管理和收获全过程。"
        },
        new AcreDto
        {
            Id = "2",
            Name = "认购一亩田 B1",
            Status = "adopted",
            Price = "8888元/亩",
            Image = "https://img.freepik.com/free-photo/agriculture-field-with-growing-crops_23-2148872538.jpg",
            Description = "标准化整地地块，采光和通风条件良好，适合家庭认购和亲子体验。"
        },
        new AcreDto
        {
            Id = "3",
            Name = "认购一亩田 C1",
            Status = "available",
            Price = "7777元/亩",
            Image = "https://img.freepik.com/free-photo/wheat-field_1127-3185.jpg",
            Description = "适合春夏种植体验，可预约到场查看并定制认购方案。"
        }
    ];

    [HttpGet]
    public ActionResult<ApiResult> GetList([FromQuery] string? status = null, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var items = Acres.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        var result = new AcreListResponseDto
        {
            PageIndex = pageIndex <= 0 ? 1 : pageIndex,
            PageSize = pageSize <= 0 ? 10 : pageSize,
            Total = items.Count(),
            Items = items.ToList()
        };

        return Ok(ApiResult.Success(result));
    }

    [HttpGet("{id}")]
    public ActionResult<ApiResult> GetDetail(string id)
    {
        var acre = Acres.FirstOrDefault(x => x.Id == id) ?? Acres[0];
        return Ok(ApiResult.Success(acre));
    }

    [HttpPost("{id}/adopt")]
    public ActionResult<ApiResult> Adopt(string id, [FromBody] object? body)
    {
        return Ok(ApiResult.Success(new
        {
            acreId = id,
            adopted = true
        }));
    }

    [HttpGet("{id}/logs")]
    public ActionResult<ApiResult> Logs(string id)
    {
        var result = new AcreLogsResponseDto
        {
            Logs =
            [
                new AcreLogDto { Time = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss"), Action = "播种" },
                new AcreLogDto { Time = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss"), Action = "浇水" },
                new AcreLogDto { Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Action = "施肥" }
            ]
        };

        return Ok(ApiResult.Success(result));
    }
}
