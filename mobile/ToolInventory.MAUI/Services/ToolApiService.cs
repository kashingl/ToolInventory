using System.Net.Http.Json;
using System.Text.Json;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.Services;

public class ToolApiService : IToolApiService
{
    private readonly HttpClient _http;
    private readonly IApiConfiguration _apiConfiguration;

    public ToolApiService(HttpClient httpClient, IApiConfiguration apiConfiguration)
    {
        _http = httpClient;
        _apiConfiguration = apiConfiguration;
    }

    public async Task<ApiResult<List<ToolDto>>> GetToolsAsync(string? status = null, int page = 1, int pageSize = 200)
    {
        EnsureBaseAddress();
        var query = string.IsNullOrWhiteSpace(status)
            ? $"/api/tools?page={page}&pageSize={pageSize}"
            : $"/api/tools?page={page}&pageSize={pageSize}&status={Uri.EscapeDataString(status)}";
        var response = await _http.GetAsync(query);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<List<ToolDto>>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var tools = await response.Content.ReadFromJsonAsync<List<ToolDto>>() ?? [];
        return ApiResult<List<ToolDto>>.Success(tools);
    }

    public async Task<ApiResult<ToolDto>> GetToolByIdAsync(int id)
    {
        EnsureBaseAddress();
        var response = await _http.GetAsync($"/api/tools/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<ToolDto>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var tool = await response.Content.ReadFromJsonAsync<ToolDto>();
        return tool is null
            ? ApiResult<ToolDto>.Fail((int)response.StatusCode, "Invalid tool response.")
            : ApiResult<ToolDto>.Success(tool);
    }

    public async Task<ApiResult<ToolDto>> GetToolByBarcodeAsync(string barcode)
    {
        EnsureBaseAddress();
        var response = await _http.GetAsync($"/api/tools/barcode/{barcode}");
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<ToolDto>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var tool = await response.Content.ReadFromJsonAsync<ToolDto>();
        return tool is null
            ? ApiResult<ToolDto>.Fail((int)response.StatusCode, "Invalid tool response.")
            : ApiResult<ToolDto>.Success(tool);
    }

    public async Task<ApiResult<ToolDto>> CreateToolAsync(CreateToolDto dto)
    {
        EnsureBaseAddress();
        var response = await _http.PostAsJsonAsync("/api/tools", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<ToolDto>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var tool = await response.Content.ReadFromJsonAsync<ToolDto>();
        return tool is null
            ? ApiResult<ToolDto>.Fail((int)response.StatusCode, "Invalid tool response.")
            : ApiResult<ToolDto>.Success(tool);
    }

    public async Task<ApiResult> UpdateToolAsync(int id, UpdateToolDto dto)
    {
        EnsureBaseAddress();
        var response = await _http.PutAsJsonAsync($"/api/tools/{id}", dto);
        return response.IsSuccessStatusCode
            ? ApiResult.Success()
            : ApiResult.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
    }

    public async Task<ApiResult> DeleteToolAsync(int id)
    {
        EnsureBaseAddress();
        var response = await _http.DeleteAsync($"/api/tools/{id}");
        return response.IsSuccessStatusCode
            ? ApiResult.Success()
            : ApiResult.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
    }

    public async Task<ApiResult<List<CategoryDto>>> GetCategoriesAsync()
    {
        EnsureBaseAddress();
        var response = await _http.GetAsync("/api/categories");
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<List<CategoryDto>>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? [];
        return ApiResult<List<CategoryDto>>.Success(categories);
    }

    public async Task<ApiResult<List<CheckoutDto>>> GetCheckoutsAsync()
    {
        EnsureBaseAddress();
        var response = await _http.GetAsync("/api/checkouts");
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<List<CheckoutDto>>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var checkouts = await response.Content.ReadFromJsonAsync<List<CheckoutDto>>() ?? [];
        return ApiResult<List<CheckoutDto>>.Success(checkouts);
    }

    public async Task<ApiResult<CheckoutDto>> CreateCheckoutAsync(CreateCheckoutDto dto)
    {
        EnsureBaseAddress();
        var response = await _http.PostAsJsonAsync("/api/checkouts", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<CheckoutDto>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutDto>();
        return checkout is null
            ? ApiResult<CheckoutDto>.Fail((int)response.StatusCode, "Invalid checkout response.")
            : ApiResult<CheckoutDto>.Success(checkout);
    }

    public async Task<ApiResult> CheckInAsync(int checkoutId)
    {
        EnsureBaseAddress();
        var response = await _http.PutAsync($"/api/checkouts/{checkoutId}/checkin", null);
        return response.IsSuccessStatusCode
            ? ApiResult.Success()
            : ApiResult.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
    }

    public async Task<ApiResult> CheckInByToolIdAsync(int toolId)
    {
        EnsureBaseAddress();
        var response = await _http.PutAsync($"/api/checkouts/tool/{toolId}/checkin", null);
        return response.IsSuccessStatusCode
            ? ApiResult.Success()
            : ApiResult.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
    }

    public async Task<ApiResult<List<MaintenanceRecordDto>>> GetMaintenanceRecordsAsync()
    {
        EnsureBaseAddress();
        var response = await _http.GetAsync("/api/maintenance");
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<List<MaintenanceRecordDto>>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var records = await response.Content.ReadFromJsonAsync<List<MaintenanceRecordDto>>() ?? [];
        return ApiResult<List<MaintenanceRecordDto>>.Success(records);
    }

    public async Task<ApiResult<MaintenanceRecordDto>> CreateMaintenanceRecordAsync(CreateMaintenanceRecordDto dto)
    {
        EnsureBaseAddress();
        var response = await _http.PostAsJsonAsync("/api/maintenance", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<MaintenanceRecordDto>.Fail((int)response.StatusCode, await ReadErrorMessageAsync(response));
        }

        var record = await response.Content.ReadFromJsonAsync<MaintenanceRecordDto>();
        return record is null
            ? ApiResult<MaintenanceRecordDto>.Fail((int)response.StatusCode, "Invalid maintenance response.")
            : ApiResult<MaintenanceRecordDto>.Success(record);
    }

    private void EnsureBaseAddress()
    {
        var configured = new Uri(_apiConfiguration.BaseUrl);
        if (_http.BaseAddress != configured)
        {
            _http.BaseAddress = configured;
        }
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? "The request failed.";
            }
            if (doc.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? "The request failed.";
            }
        }
        catch
        {
            // Fall through to generic message.
        }

        return "The request failed.";
    }
}
