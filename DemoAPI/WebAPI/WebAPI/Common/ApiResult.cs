namespace WebAPI.Common;

public class ApiResult
{
    public int Code { get; set; }

    public string Message { get; set; } = "success";

    public object? Data { get; set; }

    public static ApiResult Success(object? data = null, string message = "success")
    {
        return new ApiResult
        {
            Code = 0,
            Message = message,
            Data = data
        };
    }

    public static ApiResult Fail(string message = "fail", int code = -1, object? data = null)
    {
        return new ApiResult
        {
            Code = code,
            Message = message,
            Data = data
        };
    }
}
