using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ToolInventory.Infrastructure.Data;

namespace ToolInventory.API.Tests;

public class ToolsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ToolsApiTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("TOOLINVENTORY_JWT_KEY", "ToolInventory_Test_Secret_Key_AtLeast32Chars!");
        var databaseName = $"TestDb_{Guid.NewGuid():N}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.PostConfigureAll<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultScheme = "Test";
                });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("Test")
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetTools_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/tools");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateTool_WithDuplicateBarcode_ReturnsConflict()
    {
        var first = await _client.PostAsJsonAsync("/api/tools", new
        {
            name = "Hammer",
            description = "Steel hammer",
            barcode = "DUPLICATE-123"
        });

        var second = await _client.PostAsJsonAsync("/api/tools", new
        {
            name = "Backup Hammer",
            description = "Backup",
            barcode = "DUPLICATE-123"
        });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task UpdateTool_FromRetiredToAvailable_ReturnsBadRequest()
    {
        var created = await _client.PostAsJsonAsync("/api/tools", new
        {
            name = "Drill",
            description = "Cordless drill",
            barcode = "TRANSITION-001"
        });
        var tool = await created.Content.ReadFromJsonAsync<ToolResponse>();

        var retireResponse = await _client.PutAsJsonAsync($"/api/tools/{tool!.Id}", new
        {
            name = "Drill",
            description = "Cordless drill",
            barcode = "TRANSITION-001",
            status = "Retired"
        });

        var reopenResponse = await _client.PutAsJsonAsync($"/api/tools/{tool.Id}", new
        {
            name = "Drill",
            description = "Cordless drill",
            barcode = "TRANSITION-001",
            status = "Available"
        });

        Assert.Equal(HttpStatusCode.NoContent, retireResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, reopenResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_InUseByTool_ReturnsConflict()
    {
        var categoryCreate = await _client.PostAsJsonAsync("/api/categories", new
        {
            name = "Power Tools",
            description = "Category used in delete conflict test"
        });
        var category = await categoryCreate.Content.ReadFromJsonAsync<CategoryResponse>();

        var toolCreate = await _client.PostAsJsonAsync("/api/tools", new
        {
            name = "Circular Saw",
            description = "Saw",
            barcode = "CAT-CONFLICT-001",
            categoryId = category!.Id
        });

        var deleteResponse = await _client.DeleteAsync($"/api/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.Created, categoryCreate.StatusCode);
        Assert.Equal(HttpStatusCode.Created, toolCreate.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task CompleteMaintenance_SetsToolStatusBackToAvailable()
    {
        var createdToolResponse = await _client.PostAsJsonAsync("/api/tools", new
        {
            name = "Maintenance Drill",
            description = "Needs servicing",
            barcode = "MAINT-STATUS-001"
        });
        var createdTool = await createdToolResponse.Content.ReadFromJsonAsync<ToolResponse>();

        var maintenanceCreate = await _client.PostAsJsonAsync("/api/maintenance", new
        {
            toolId = createdTool!.Id,
            date = DateTime.UtcNow,
            description = "Routine maintenance",
            performedBy = "Technician"
        });
        var maintenance = await maintenanceCreate.Content.ReadFromJsonAsync<MaintenanceResponse>();

        var completeResponse = await _client.PutAsync($"/api/maintenance/{maintenance!.Id}/complete", null);
        var toolAfterCompleteResponse = await _client.GetAsync($"/api/tools/{createdTool.Id}");
        var toolAfterComplete = await toolAfterCompleteResponse.Content.ReadFromJsonAsync<ToolResponse>();

        Assert.Equal(HttpStatusCode.Created, createdToolResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, maintenanceCreate.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, toolAfterCompleteResponse.StatusCode);
        Assert.Equal("Available", toolAfterComplete!.Status);
    }

    [Fact]
    public async Task CheckInByToolId_WithActiveCheckout_SetsToolToAvailable()
    {
        var userId = await SeedUserAsync();

        var createToolResponse = await _client.PostAsJsonAsync("/api/tools", new
        {
            name = "CheckIn Drill",
            description = "Used for check-in by tool id test",
            barcode = "CHECKIN-TOOL-001"
        });
        var tool = await createToolResponse.Content.ReadFromJsonAsync<ToolResponse>();

        var createCheckoutResponse = await _client.PostAsJsonAsync("/api/checkouts", new
        {
            toolId = tool!.Id,
            userId,
            notes = "Created by API test"
        });

        var checkInResponse = await _client.PutAsync($"/api/checkouts/tool/{tool.Id}/checkin", null);
        var toolAfterCheckInResponse = await _client.GetAsync($"/api/tools/{tool.Id}");
        var toolAfterCheckIn = await toolAfterCheckInResponse.Content.ReadFromJsonAsync<ToolResponse>();
        var checkoutsResponse = await _client.GetAsync("/api/checkouts");
        var checkouts = await checkoutsResponse.Content.ReadFromJsonAsync<List<CheckoutResponse>>();
        var updatedCheckout = checkouts!.Single(c => c.ToolId == tool.Id);

        Assert.Equal(HttpStatusCode.Created, createToolResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, createCheckoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, checkInResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, toolAfterCheckInResponse.StatusCode);
        Assert.Equal("Available", toolAfterCheckIn!.Status);
        Assert.NotNull(updatedCheckout.ActualReturnDate);
    }

    [Fact]
    public async Task CheckInByToolId_WithoutActiveCheckout_ReturnsNotFound()
    {
        var createToolResponse = await _client.PostAsJsonAsync("/api/tools", new
        {
            name = "Unassigned Tool",
            description = "No active checkout exists",
            barcode = "CHECKIN-TOOL-404"
        });
        var tool = await createToolResponse.Content.ReadFromJsonAsync<ToolResponse>();

        var checkInResponse = await _client.PutAsync($"/api/checkouts/tool/{tool!.Id}/checkin", null);

        Assert.Equal(HttpStatusCode.Created, createToolResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, checkInResponse.StatusCode);
    }

    private async Task<string> SeedUserAsync()
    {
        var userId = $"seed-user-{Guid.NewGuid():N}";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Users.Add(new IdentityUser
        {
            Id = userId,
            UserName = "seed.user@example.com",
            NormalizedUserName = "SEED.USER@EXAMPLE.COM",
            Email = "seed.user@example.com",
            NormalizedEmail = "SEED.USER@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        });
        await db.SaveChangesAsync();
        return userId;
    }

    private sealed class ToolResponse
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class CategoryResponse
    {
        public int Id { get; set; }
    }

    private sealed class MaintenanceResponse
    {
        public int Id { get; set; }
    }

    private sealed class CheckoutResponse
    {
        public int ToolId { get; set; }
        public DateTime? ActualReturnDate { get; set; }
    }
}
