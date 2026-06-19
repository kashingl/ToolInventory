using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.Services;

public interface IToolApiService
{
    Task<ApiResult<List<ToolDto>>> GetToolsAsync(string? status = null, int page = 1, int pageSize = 200);
    Task<ApiResult<ToolDto>> GetToolByIdAsync(int id);
    Task<ApiResult<ToolDto>> GetToolByBarcodeAsync(string barcode);
    Task<ApiResult<ToolDto>> CreateToolAsync(CreateToolDto dto);
    Task<ApiResult> UpdateToolAsync(int id, UpdateToolDto dto);
    Task<ApiResult> DeleteToolAsync(int id);
    Task<ApiResult<List<CategoryDto>>> GetCategoriesAsync();
    Task<ApiResult<List<CheckoutDto>>> GetCheckoutsAsync();
    Task<ApiResult<CheckoutDto>> CreateCheckoutAsync(CreateCheckoutDto dto);
    Task<ApiResult> CheckInAsync(int checkoutId);
    Task<ApiResult> CheckInByToolIdAsync(int toolId);
    Task<ApiResult<List<MaintenanceRecordDto>>> GetMaintenanceRecordsAsync();
    Task<ApiResult<MaintenanceRecordDto>> CreateMaintenanceRecordAsync(CreateMaintenanceRecordDto dto);
}
