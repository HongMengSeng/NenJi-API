using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    private readonly IContentService _contentService;

    public ActivityController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResult>> List(CancellationToken cancellationToken)
    {
        var data = await _contentService.GetActivitiesAsync(cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpGet("detail")]
    public async Task<ActionResult<ApiResult>> Detail([FromQuery] int id, CancellationToken cancellationToken)
    {
        var data = await _contentService.GetActivityDetailAsync(id, cancellationToken);
        return data is null
            ? Ok(ApiResult.Fail("活动不存在", 404))
            : Ok(ApiResult.Success(data));
    }
}
