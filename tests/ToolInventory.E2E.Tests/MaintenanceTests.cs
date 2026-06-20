using Microsoft.Playwright;
using ToolInventory.E2E.Tests.Fixtures;

namespace ToolInventory.E2E.Tests;

[Collection("E2E")]
public class MaintenanceTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public MaintenanceTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MaintenancePage_ShowsTable()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/maintenance");

        await _fixture.Page.WaitForSelectorAsync("table[mat-table]");
        var table = _fixture.Page.Locator("table[mat-table]");
        await Assertions.Expect(table).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AddRecordButton_OpensDialog()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/maintenance");

        await _fixture.Page.ClickAsync("button:has-text('Add Record')");

        var dialog = _fixture.Page.Locator("mat-dialog-container");
        await Assertions.Expect(dialog).ToBeVisibleAsync();

        var title = _fixture.Page.Locator("h2[mat-dialog-title]");
        await Assertions.Expect(title).ToContainTextAsync("Add Maintenance Record");
    }

    [Fact]
    public async Task AddRecordDialog_Cancel_ClosesDialog()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/maintenance");
        await _fixture.Page.ClickAsync("button:has-text('Add Record')");
        await _fixture.Page.WaitForSelectorAsync("mat-dialog-container");

        await _fixture.Page.ClickAsync("mat-dialog-actions button:has-text('Cancel')");

        var dialog = _fixture.Page.Locator("mat-dialog-container");
        await Assertions.Expect(dialog).ToBeHiddenAsync();
    }

    [Fact]
    public async Task AddMaintenanceRecord_Submit_ShowsNewRecord()
    {
        await _fixture.LoginForTestAsync();
        var toolName = await E2ETestHelper.CreateToolAsync(_fixture.Page);

        await _fixture.Page.GotoAsync("/maintenance");
        await _fixture.Page.ClickAsync("button:has-text('Add Record')");
        await _fixture.Page.WaitForSelectorAsync("mat-dialog-container");

        await E2ETestHelper.SelectMatOptionAsync(_fixture.Page, "mat-dialog-container mat-select[formcontrolname='toolId']", toolName);
        await _fixture.Page.FillAsync("mat-dialog-container textarea[formcontrolname='description']", "E2E maintenance record");
        await _fixture.Page.FillAsync("mat-dialog-container input[formcontrolname='performedBy']", "E2E Tester");
        await _fixture.Page.FillAsync("mat-dialog-container input[formcontrolname='cost']", "25.50");
        await _fixture.Page.ClickAsync("mat-dialog-actions button:has-text('Save')");

        await E2ETestHelper.ExpectSnackBarAsync(_fixture.Page, "created");

        var row = _fixture.Page.Locator($"tr[mat-row]:has-text('{toolName}')");
        await Assertions.Expect(row).ToBeVisibleAsync();
        await Assertions.Expect(row).ToContainTextAsync("E2E maintenance record");
    }
}
