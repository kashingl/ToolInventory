using Microsoft.Playwright;
using ToolInventory.E2E.Tests.Fixtures;

namespace ToolInventory.E2E.Tests;

[Collection("E2E")]
public class ToolsTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public ToolsTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ToolsPage_ShowsTable()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/tools");

        await _fixture.Page.WaitForSelectorAsync("table[mat-table]");
        var table = _fixture.Page.Locator("table[mat-table]");
        await Assertions.Expect(table).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ToolsPage_AddToolButton_OpensDialog()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/tools");

        await _fixture.Page.ClickAsync("button:has-text('Add Tool')");

        var dialog = _fixture.Page.Locator("mat-dialog-container");
        await Assertions.Expect(dialog).ToBeVisibleAsync();

        var title = _fixture.Page.Locator("h2[mat-dialog-title]");
        await Assertions.Expect(title).ToContainTextAsync("Add Tool");
    }

    [Fact]
    public async Task CreateTool_FillFormAndSubmit_ToolAppearsInList()
    {
        await _fixture.LoginForTestAsync();
        var toolName = await E2ETestHelper.CreateToolAsync(_fixture.Page);

        await Assertions.Expect(E2ETestHelper.ToolRow(_fixture.Page, toolName)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ToolCrud_CreateEditDelete_WorksEndToEnd()
    {
        await _fixture.LoginForTestAsync();
        var originalName = await E2ETestHelper.CreateToolAsync(_fixture.Page);
        var originalRow = E2ETestHelper.ToolRow(_fixture.Page, originalName);

        await originalRow.Locator("button[mat-icon-button]").First.ClickAsync();
        await _fixture.Page.WaitForSelectorAsync("mat-dialog-container");

        var updatedName = $"{originalName} Updated";
        await _fixture.Page.FillAsync("mat-dialog-container input[formcontrolname='name']", updatedName);
        await _fixture.Page.FillAsync("mat-dialog-container input[formcontrolname='location']", "Van 2");
        await _fixture.Page.ClickAsync("mat-dialog-actions button:has-text('Update')");

        await E2ETestHelper.ExpectSnackBarAsync(_fixture.Page, "updated");
        await E2ETestHelper.FilterToolsAsync(_fixture.Page, updatedName);
        await Assertions.Expect(E2ETestHelper.ToolRow(_fixture.Page, updatedName)).ToBeVisibleAsync();

        await E2ETestHelper.ToolRow(_fixture.Page, updatedName).Locator("button[mat-icon-button]").Nth(1).ClickAsync();
        await _fixture.Page.WaitForSelectorAsync("mat-dialog-container");
        await _fixture.Page.ClickAsync("mat-dialog-actions button:has-text('Delete')");

        await E2ETestHelper.ExpectSnackBarAsync(_fixture.Page, "deleted");
        await E2ETestHelper.FilterToolsAsync(_fixture.Page, updatedName);
        await Assertions.Expect(E2ETestHelper.ToolRow(_fixture.Page, updatedName)).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task FilterBox_FiltersTableResults()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/tools");
        await _fixture.Page.WaitForSelectorAsync("table[mat-table]");

        await E2ETestHelper.FilterToolsAsync(_fixture.Page, "zzznomatch");

        var rows = _fixture.Page.Locator("tr[mat-row]");
        await Assertions.Expect(rows).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ScanButton_OpensScannerDialog()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/tools");

        await _fixture.Page.ClickAsync("button:has-text('Scan')");

        var dialog = _fixture.Page.Locator("mat-dialog-container");
        await Assertions.Expect(dialog).ToBeVisibleAsync();

        var title = _fixture.Page.Locator("h2[mat-dialog-title]");
        await Assertions.Expect(title).ToContainTextAsync("Scan");
    }
}

