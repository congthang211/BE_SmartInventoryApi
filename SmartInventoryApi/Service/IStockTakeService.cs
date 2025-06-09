using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IStockTakeService
    {
        Task<PaginatedResponseDto<StockTakeDto>> GetAllStockTakesAsync(
            StockTakeQueryParameters queryParameters,
            string requestingUserRole);

        Task<StockTakeDto?> GetStockTakeByIdAsync(int id, string requestingUserRole);

        Task<StockTakeDto> CreateStockTakeAsync(CreateStockTakeDto createDto, int createdByUserId);
        Task<StockTakeDto> RecordStockTakeCountsAsync(int stockTakeId, RecordStockTakeCountsDto countsDto, int recordedByUserId);
        Task<StockTakeDto> CompleteStockTakeAsync(int stockTakeId, int completedByUserId, string? completionNotes);
        Task<StockTakeDto> CancelStockTakeAsync(int stockTakeId, int cancelledByUserId);
    }
}