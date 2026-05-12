using System.ComponentModel.DataAnnotations;

namespace ManageAPI.Dtos;

public class ActivitySummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class ActivityListDto
{
    public Dictionary<string, List<ActivitySummaryDto>> Activities { get; set; } = [];
}

public class ActivityDetailDto : ActivitySummaryDto
{
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string People { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Participants { get; set; }
    public int RemainingSlots { get; set; }
    public List<string> Images { get; set; } = [];
}
