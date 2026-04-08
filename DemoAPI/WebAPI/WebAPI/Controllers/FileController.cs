using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WebAPI.Common;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private string IconPath => Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "icons");
    private string VideoPath => Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "videos");

    public FileController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// 获取图标库图片列表
    /// </summary>
    /// <returns>返回图标文件名列表</returns>
    [HttpGet("images")]
    public IActionResult ListImages()
    {
        try
        {
            if (!Directory.Exists(IconPath))
            {
                return Ok(ApiResult.Fail("图标目录不存在", 404));
            }

            var files = Directory.GetFiles(IconPath)
                .Where(f => IsImageFile(f))
                .Select(Path.GetFileName)
                .ToList();

            return Ok(ApiResult.Success(new
            {
                path = "wwwroot/icons",
                files = files
            }));
        }
        catch (Exception ex)
        {
            return Ok(ApiResult.Fail($"获取图片列表失败：{ex.Message}"));
        }
    }

    /// <summary>
    /// 获取指定图标图片内容
    /// </summary>
    /// <param name="fileName">文件名 (例如: rroom.png)</param>
    /// <returns>返回图片文件流</returns>
    [HttpGet("image/{fileName}")]
    public IActionResult GetImage(string fileName)
    {
        try
        {
            var filePath = Path.Combine(IconPath, fileName);

            // 安全性检查
            if (!Path.GetFullPath(filePath).StartsWith(Path.GetFullPath(IconPath), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("无效的文件名");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("图片不存在");
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(filePath, contentType);
        }
        catch (Exception ex)
        {
            return BadRequest($"获取图片失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取视频列表
    /// </summary>
    /// <returns>返回视频文件名列表</returns>
    [HttpGet("videos")]
    public IActionResult ListVideos()
    {
        try
        {
            if (!Directory.Exists(VideoPath))
            {
                return Ok(ApiResult.Fail("视频目录不存在", 404));
            }

            var files = Directory.GetFiles(VideoPath)
                .Where(f => IsVideoFile(f))
                .Select(Path.GetFileName)
                .ToList();

            return Ok(ApiResult.Success(new
            {
                path = "wwwroot/videos",
                files = files
            }));
        }
        catch (Exception ex)
        {
            return Ok(ApiResult.Fail($"获取视频列表失败：{ex.Message}"));
        }
    }

    /// <summary>
    /// 获取并播放指定视频内容 (支持分段传输/流播放)
    /// </summary>
    /// <param name="fileName">文件名 (例如: farm_intro.mp4)</param>
    /// <returns>返回视频流</returns>
    [HttpGet("video/{fileName}")]
    public IActionResult GetVideo(string fileName)
    {
        try
        {
            var filePath = Path.Combine(VideoPath, fileName);

            // 安全性检查
            if (!Path.GetFullPath(filePath).StartsWith(Path.GetFullPath(VideoPath), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("无效的文件名");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("视频不存在");
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "video/mp4"; // 默认为 mp4
            }

            // 使用 PhysicalFile 并启用 Range 处理，支持视频快进和拖动
            return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            return BadRequest($"获取视频失败：{ex.Message}");
        }
    }

    private static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp" || ext == ".webp";
    }

    private static bool IsVideoFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".mp4" || ext == ".mov" || ext == ".avi" || ext == ".mkv" || ext == ".wmv";
    }
}
