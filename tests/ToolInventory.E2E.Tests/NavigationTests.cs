using Microsoft.Playwright;
using ToolInventory.E2E.Tests.Fixtures;

namespace ToolInventory.E2E.Tests;

[Collection("E2E")]
public class NavigationTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public NavigationTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SideNav_ShowsAllMenuItems()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/");

        await _fixture.Page.WaitForSelectorAsync("mat-sidenav");

        var toolsLink = _fixture.Page.Locator("mat-nav-list a:has-text('Tools')");
        var checkoutsLink = _fixture.Page.Locator("mat-nav-list a:has-text('Check-in / Out')");
        var maintenanceLink = _fixture.Page.Locator("mat-nav-list a:has-text('Maintenance')");

        await Assertions.Expect(toolsLink).ToBeVisibleAsync();
        await Assertions.Expect(checkoutsLink).ToBeVisibleAsync();
        await Assertions.Expect(maintenanceLink).ToBeVisibleAsync();
    }

    [Fact]
    public async Task RootUrl_RedirectsToTools()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/");
        await _fixture.Page.WaitForURLAsync("**/tools");

        Assert.Contains("/tools", _fixture.Page.Url);
    }

    [Fact]
    public async Task CheckoutsLink_NavigatesToCheckoutsPage()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/");
        await _fixture.Page.ClickAsync("mat-nav-list a:has-text('Check-in / Out')");
        await _fixture.Page.WaitForURLAsync("**/checkouts");

        Assert.Contains("/checkouts", _fixture.Page.Url);
    }

    [Fact]
    public async Task MaintenanceLink_NavigatesToMaintenancePage()
    {
        await _fixture.LoginForTestAsync();
        await _fixture.Page.GotoAsync("/");
        await _fixture.Page.ClickAsync("mat-nav-list a:has-text('Maintenance')");
        await _fixture.Page.WaitForURLAsync("**/maintenance");

        Assert.Contains("/maintenance", _fixture.Page.Url);
    }
}
