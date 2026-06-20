using Microsoft.Playwright;
using ToolInventory.E2E.Tests.Fixtures;

namespace ToolInventory.E2E.Tests;

[Collection("E2E")]
public class CheckoutsTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public CheckoutsTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CheckoutsPage_ShowsTable()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/checkouts");

        await _fixture.Page.WaitForSelectorAsync("table[mat-table]");
        var table = _fixture.Page.Locator("table[mat-table]");
        await Assertions.Expect(table).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CheckoutButton_OpensDialog()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/checkouts");

        await _fixture.Page.ClickAsync("button:has-text('Check Out Tool')");

        var dialog = _fixture.Page.Locator("mat-dialog-container");
        await Assertions.Expect(dialog).ToBeVisibleAsync();

        var title = _fixture.Page.Locator("h2[mat-dialog-title]");
        await Assertions.Expect(title).ToContainTextAsync("Check Out Tool");
    }

    [Fact]
    public async Task CheckoutDialog_Cancel_ClosesDialog()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/checkouts");
        await _fixture.Page.ClickAsync("button:has-text('Check Out Tool')");
        await _fixture.Page.WaitForSelectorAsync("mat-dialog-container");

        await _fixture.Page.ClickAsync("mat-dialog-actions button:has-text('Cancel')");

        var dialog = _fixture.Page.Locator("mat-dialog-container");
        await Assertions.Expect(dialog).ToBeHiddenAsync();
    }

    [Fact]
    public async Task CheckoutAndCheckIn_FlowUpdatesStatus()
    {
        await _fixture.LoginForTestAsync();
        var toolName = await E2ETestHelper.CreateToolAsync(_fixture.Page);
        var currentUserId = await E2ETestHelper.GetCurrentUserIdAsync(_fixture.Page);

        await _fixture.Page.GotoAsync("/checkouts");
        await _fixture.Page.ClickAsync("button:has-text('Check Out Tool')");
        await _fixture.Page.WaitForSelectorAsync("mat-dialog-container");

        await E2ETestHelper.SelectMatOptionAsync(_fixture.Page, "mat-dialog-container mat-select[formcontrolname='toolId']", toolName);
        await _fixture.Page.FillAsync("mat-dialog-container input[formcontrolname='userId']", currentUserId);
        await _fixture.Page.FillAsync("mat-dialog-container textarea[formcontrolname='notes']", "E2E checkout");
        await _fixture.Page.ClickAsync("mat-dialog-actions button:has-text('Check Out')");

        await E2ETestHelper.ExpectSnackBarAsync(_fixture.Page, "checked out");

        var row = _fixture.Page.Locator($"tr[mat-row]:has-text('{toolName}')");
        await Assertions.Expect(row).ToBeVisibleAsync();
        await Assertions.Expect(row).ToContainTextAsync("Active");

        await row.Locator("button:has-text('Check In')").ClickAsync();

        await E2ETestHelper.ExpectSnackBarAsync(_fixture.Page, "checked in");
        await Assertions.Expect(row).ToContainTextAsync("Returned");
    }
}
