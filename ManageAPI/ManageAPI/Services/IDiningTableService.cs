using ManageAPI.Dtos;

namespace ManageAPI.Services;

public interface IDiningTableService
{
    Task<(List<TableListItemDto> Records, int Total)> GetTableListAsync(
        int pageNum, int pageSize, string? keyword, string? statusFilter,
        CancellationToken cancellationToken = default);

    Task<TableDetailDto?> GetTableDetailAsync(string tableNo, CancellationToken cancellationToken = default);

    Task<TableMutationResponseDto?> CreateTableAsync(CreateTableRequestDto dto, string baseUrl, CancellationToken cancellationToken = default);

    Task<TableMutationResponseDto?> UpdateTableAsync(UpdateTableRequestDto dto, string baseUrl, CancellationToken cancellationToken = default);

    Task<bool> DeleteTableAsync(string tableNo, CancellationToken cancellationToken = default);

    Task<object?> UpdateTableStatusAsync(UpdateTableStatusRequestDto dto, CancellationToken cancellationToken = default);
}
