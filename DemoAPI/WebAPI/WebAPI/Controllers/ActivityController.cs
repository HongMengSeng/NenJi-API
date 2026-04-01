using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebAPI.Common;
using WebAPI.Data;
using WebAPI.Dtos;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IContentService _contentService;

    public ActivityController(AppDbContext dbContext, IContentService contentService)
    {
        _dbContext = dbContext;
        _contentService = contentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResult>> GetPageList(CancellationToken cancellationToken)
    {
        var allActivities = await LoadActivitySummariesAsync(cancellationToken);
        return Ok(ApiResult.Success(allActivities));
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResult>> List(CancellationToken cancellationToken)
    {
        var allActivities = await LoadActivitySummariesAsync(cancellationToken);
        var data = new ActivityListDto
        {
            Activities = new Dictionary<string, List<ActivitySummaryDto>>
            {
                ["all"] = allActivities
            }
        };

        return Ok(ApiResult.Success(data));
    }

    [HttpGet("detail")]
    public async Task<ActionResult<ApiResult>> Detail([FromQuery] int id, CancellationToken cancellationToken)
    {
        var activity = await _dbContext.Activities
            .AsNoTracking()
            .Where(x => x.Status == 1 && x.ActivityId == id)
            .Select(x => new ActivityDetailSummary
            {
                Id = (int)x.ActivityId,
                Title = x.Title,
                Price = $"¥{x.PriceText}",
                Date = x.DateText,
                Image = x.ImageUrl,
                CategoryName = ResolveCategoryName(x.Title),
                Participants = x.Participants,
                RemainingSlots = x.RemainingSlots
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (activity is null)
        {
            return Ok(ApiResult.Fail("活动不存在", 404));
        }

        var detail = await _contentService.GetActivityDetailAsync(id, cancellationToken);
        var data = detail is null
            ? BuildDetailFallback(activity)
            : MergeDetail(detail, activity);

        return Ok(ApiResult.Success(data));
    }

    [HttpPost("{id:int}/register")]
    public ActionResult<ApiResult> Register(int id)
    {
        if (id <= 0)
        {
            return Ok(ApiResult.Fail("活动 id 参数不正确", 400));
        }

        var orderId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return Ok(ApiResult.Success(new
        {
            id = orderId,
            orderId,
            activityId = id,
            paymentStatus = "pending_payment"
        }));
    }

    private async Task<List<ActivitySummaryDto>> LoadActivitySummariesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Activities
            .AsNoTracking()
            .Where(x => x.Status == 1)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ActivityId)
            .Select(x => new ActivitySummaryDto
            {
                Id = (int)x.ActivityId,
                Title = x.Title,
                Price = $"¥{x.PriceText}",
                Date = x.DateText,
                Image = x.ImageUrl,
                CategoryName = ResolveCategoryName(x.Title)
            })
            .ToListAsync(cancellationToken);
    }

    private static ActivityDetailDto MergeDetail(ActivityDetailDto detail, ActivityDetailSummary summary)
    {
        detail.Id = summary.Id;
        detail.Title = summary.Title;
        detail.Price = summary.Price;
        detail.Date = summary.Date;
        detail.Image = summary.Image;
        detail.CategoryName = summary.CategoryName;
        detail.Images = EnsureFourImages(detail.Images, summary.Image);
        detail.Participants = summary.Participants;
        detail.RemainingSlots = summary.RemainingSlots;
        return detail;
    }

    private static ActivityDetailDto BuildDetailFallback(ActivityDetailSummary summary)
    {
        return new ActivityDetailDto
        {
            Id = summary.Id,
            Title = summary.Title,
            Price = summary.Price,
            Date = summary.Date,
            Image = summary.Image,
            Images = EnsureFourImages([], summary.Image),
            CategoryName = summary.CategoryName,
            Description = summary.Title,
            Location = string.Empty,
            People = string.Empty,
            Content = string.Empty,
            Participants = summary.Participants,
            RemainingSlots = summary.RemainingSlots
        };
    }

    private static List<string> EnsureFourImages(IEnumerable<string>? images, string? fallbackImage)
    {
        var result = (images ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToList();

        var fallback = string.IsNullOrWhiteSpace(fallbackImage) ? string.Empty : fallbackImage.Trim();
        if (result.Count == 0 && !string.IsNullOrWhiteSpace(fallback))
        {
            result.Add(fallback);
        }

        while (result.Count > 0 && result.Count < 4)
        {
            result.Add(result[result.Count - 1]);
        }

        return result;
    }

    private static string ResolveCategoryName(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "采摘活动";
        }

        return title.Contains("露营", StringComparison.OrdinalIgnoreCase)
            || title.Contains("camp", StringComparison.OrdinalIgnoreCase)
            ? "露营"
            : "采摘活动";
    }

    private sealed class ActivityDetailSummary : ActivitySummaryDto
    {
        public int Participants { get; set; }
        public int RemainingSlots { get; set; }
    }
}
