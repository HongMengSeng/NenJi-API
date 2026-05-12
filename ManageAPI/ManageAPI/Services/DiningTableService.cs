using Microsoft.EntityFrameworkCore;

using ManageAPI.Data;
using ManageAPI.Dtos;
using ManageAPI.Entity;

using QRCoder;

namespace ManageAPI.Services;

public class DiningTableService : IDiningTableService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DiningTableService> _logger;
    private static readonly string QrCodeDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qrcodes");

    private static readonly Dictionary<int, string> StatusNameMap = new()
    {
        { 1, "空闲" },
        { 2, "停用" },
        { 3, "使用中" }
    };

    private static readonly Dictionary<string, int> StatusValueMap = new()
    {
        { "空闲", 1 },
        { "停用", 2 },
        { "使用中", 3 }
    };

    public DiningTableService(AppDbContext context, ILogger<DiningTableService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<TableListItemDto> Records, int Total)> GetTableListAsync(
        int pageNum, int pageSize, string? keyword, string? statusFilter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DiningTables.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(t => t.TableNo.Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            if (StatusValueMap.TryGetValue(statusFilter.Trim(), out var statusValue))
            {
                query = query.Where(t => t.TableStatus == statusValue);
            }
        }

        var total = await query.CountAsync(cancellationToken);

        var raw = await query
            .OrderBy(t => t.TableNo)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var records = raw.Select(t => new TableListItemDto
        {
            Id = t.TableNo,
            Tableno = t.TableNo,
            Capacity = t.SeatCount,
            Status = StatusNameMap.GetValueOrDefault(t.TableStatus, "未知"),
            CreateTime = t.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        }).ToList();

        return (records, total);
    }

    public async Task<TableDetailDto?> GetTableDetailAsync(string tableNo, CancellationToken cancellationToken = default)
    {
        var table = await _context.DiningTables
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TableNo == tableNo, cancellationToken);

        if (table is null) return null;

        return new TableDetailDto
        {
            Id = table.TableNo,
            Tableno = table.TableNo,
            Capacity = table.SeatCount,
            Status = table.TableStatus,
            CreateTime = table.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            QrCodeUrl = table.QrCodeImageUrl
        };
    }

    public async Task<TableMutationResponseDto?> CreateTableAsync(
        CreateTableRequestDto dto, string baseUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Tableno))
            throw new ArgumentException("餐桌号不能为空");

        if (dto.Capacity < 1 || dto.Capacity > 30)
            throw new ArgumentException("容纳人数必须在 1-30 之间");

        var exists = await _context.DiningTables
            .AnyAsync(t => t.TableNo == dto.Tableno.Trim(), cancellationToken);

        if (exists)
            throw new InvalidOperationException($"餐桌号 '{dto.Tableno}' 已存在");

        var status = dto.Status ?? 1;

        var tableNo = dto.Tableno.Trim();
        var qrCodeUrl = GenerateQrCode(tableNo, baseUrl);

        var entity = new DiningTables
        {
            TableNo = tableNo,
            SeatCount = dto.Capacity,
            TableStatus = status,
            QrCodeImageUrl = qrCodeUrl,
            CreatedAt = DateTime.Now
        };

        _context.DiningTables.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("新增餐桌成功 - TableNo: {TableNo}, Capacity: {Capacity}", tableNo, dto.Capacity);

        return new TableMutationResponseDto
        {
            Id = tableNo,
            Tableno = tableNo,
            Capacity = dto.Capacity,
            Status = status,
            QrCodeUrl = qrCodeUrl
        };
    }

    public async Task<TableMutationResponseDto?> UpdateTableAsync(
        UpdateTableRequestDto dto, string baseUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
            throw new ArgumentException("餐桌ID不能为空");

        var table = await _context.DiningTables
            .FirstOrDefaultAsync(t => t.TableNo == dto.Id.Trim(), cancellationToken);

        if (table is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Tableno) && dto.Tableno.Trim() != table.TableNo)
        {
            var newNo = dto.Tableno.Trim();
            var conflict = await _context.DiningTables
                .AnyAsync(t => t.TableNo == newNo, cancellationToken);

            if (conflict)
                throw new InvalidOperationException($"餐桌号 '{newNo}' 已存在");

            table.TableNo = newNo;
            table.QrCodeImageUrl = GenerateQrCode(newNo, baseUrl);
        }

        if (dto.Capacity.HasValue)
        {
            if (dto.Capacity.Value < 1 || dto.Capacity.Value > 30)
                throw new ArgumentException("容纳人数必须在 1-30 之间");

            table.SeatCount = dto.Capacity.Value;
        }

        if (dto.Status.HasValue)
        {
            table.TableStatus = dto.Status.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("更新餐桌成功 - Id: {Id}, NewTableNo: {TableNo}", dto.Id, table.TableNo);

        return new TableMutationResponseDto
        {
            Id = table.TableNo,
            Tableno = table.TableNo,
            Capacity = table.SeatCount,
            Status = table.TableStatus,
            QrCodeUrl = table.QrCodeImageUrl
        };
    }

    public async Task<bool> DeleteTableAsync(string tableNo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tableNo))
            throw new ArgumentException("餐桌ID不能为空");

        var table = await _context.DiningTables
            .FirstOrDefaultAsync(t => t.TableNo == tableNo.Trim(), cancellationToken);

        if (table is null) return false;

        _context.DiningTables.Remove(table);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("删除餐桌成功 - TableNo: {TableNo}", tableNo);

        return true;
    }

    public async Task<object?> UpdateTableStatusAsync(
        UpdateTableStatusRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Tableno))
            throw new ArgumentException("餐桌号不能为空");

        if (dto.Status < 1 || dto.Status > 3)
            throw new ArgumentException("状态值不正确，仅支持 1=空闲, 2=停用, 3=使用中");

        var table = await _context.DiningTables
            .FirstOrDefaultAsync(t => t.TableNo == dto.Tableno.Trim(), cancellationToken);

        if (table is null) return null;

        table.TableStatus = dto.Status;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("更新餐桌状态成功 - TableNo: {TableNo}, Status: {Status}", dto.Tableno, dto.Status);

        return new { tableno = dto.Tableno.Trim(), status = dto.Status };
    }

    private string GenerateQrCode(string tableNo, string baseUrl)
    {
        try
        {
            if (!Directory.Exists(QrCodeDir))
                Directory.CreateDirectory(QrCodeDir);

            var scanUrl = $"{baseUrl.TrimEnd('/')}/table/scan/{tableNo}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(scanUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(20);

            var fileName = $"{tableNo}.png";
            var filePath = Path.Combine(QrCodeDir, fileName);
            File.WriteAllBytes(filePath, qrBytes);

            var qrCodeUrl = $"{baseUrl.TrimEnd('/')}/qrcodes/{fileName}";
            _logger.LogInformation("二维码生成成功 - URL: {QrCodeUrl}", qrCodeUrl);

            return qrCodeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "二维码生成失败，将使用占位URL");
            return $"{baseUrl.TrimEnd('/')}/qrcodes/{tableNo}.png";
        }
    }
}
