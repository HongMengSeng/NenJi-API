using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/goods")]
public class GoodsController : ControllerBase
{
    private readonly IAppService _appService;

    public GoodsController(IAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("detail")]
    public async Task<ActionResult<ApiResult>> Detail([FromQuery] int goodsId, CancellationToken cancellationToken)
    {
        var data = await _appService.GetGoodsDetailAsync(goodsId, cancellationToken);
        return data is null
            ? Ok(ApiResult.Fail("商品不存在", 404))
            : Ok(ApiResult.Success(data));
    }
}
