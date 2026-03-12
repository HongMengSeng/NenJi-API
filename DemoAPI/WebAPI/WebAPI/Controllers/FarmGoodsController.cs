using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/farm-goods")]
public class FarmGoodsController : ControllerBase
{
    private readonly IAppService _appService;

    public FarmGoodsController(IAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("index")]
    public async Task<ActionResult<ApiResult>> Index(CancellationToken cancellationToken)
    {
        var data = await _appService.GetFarmGoodsIndexAsync(cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpGet("category")]
    public async Task<ActionResult<ApiResult>> Category([FromQuery] int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var data = await _appService.GetGoodsByCategoryAsync(categoryId, NormalizePage(page), NormalizePageSize(pageSize), cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResult>> Search([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var data = await _appService.SearchGoodsAsync(keyword ?? string.Empty, NormalizePage(page), NormalizePageSize(pageSize), cancellationToken);
        return Ok(ApiResult.Success(data));
    }

    private static int NormalizePage(int page) => page <= 0 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize <= 0 ? 10 : pageSize;
}
