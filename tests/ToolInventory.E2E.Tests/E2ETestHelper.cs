using Microsoft.Playwright;

namespace ToolInventory.E2E.Tests;

internal static class E2ETestHelper
{
    public static async Task<string> CreateToolAsync(IPage page, string? toolName = null)
    {
        await page.GotoAsync("/tools");
        await page.ClickAsync("button:has-text('Add Tool')");
        await page.WaitForSelectorAsync("mat-dialog-container");

        var suffix = DateTime.UtcNow.Ticks.ToString();
        var name = toolName ?? $"E2E Tool {suffix}";

        await page.FillAsync("mat-dialog-container input[formcontrolname='name']", name);
        await page.FillAsync("mat-dialog-container input[formcontrolname='location']", "Workshop");
        await page.FillAsync("mat-dialog-container input[formcontrolname='systainer']", "SYS-01");
        await page.FillAsync("mat-dialog-container input[formcontrolname='barcode']", $"E2E-{suffix}");

        await page.ClickAsync("mat-dialog-actions button:has-text('Create')");

        await ExpectSnackBarAsync(page, "created");
        await FilterToolsAsync(page, name);
        await Assertions.Expect(ToolRow(page, name)).ToBeVisibleAsync();

        return name;
    }

    public static async Task ExpectSnackBarAsync(IPage page, string expectedText)
    {
        var snackBar = page.Locator("mat-snack-bar-container");
        await page.WaitForSelectorAsync("mat-snack-bar-container");
        await Assertions.Expect(snackBar).ToContainTextAsync(expectedText);
    }

    public static async Task FilterToolsAsync(IPage page, string filterText)
    {
        await page.FillAsync("input[placeholder='Search tools...']", filterText);
    }

    public static async Task SelectMatOptionAsync(IPage page, string selectSelector, string optionText)
    {
        await page.ClickAsync(selectSelector);

        var option = page.Locator($"mat-option:has-text('{optionText}')");
        await Assertions.Expect(option).ToBeVisibleAsync();
        await option.ClickAsync();
    }

    public static ILocator ToolRow(IPage page, string toolName) =>
        page.Locator($"tr[mat-row]:has-text('{toolName}')");
}
