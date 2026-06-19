using Microsoft.Playwright;

namespace ToolInventory.E2E.Tests.Fixtures;

public class PlaywrightFixture : IAsyncLifetime
{
    public const string BaseUrl = "http://localhost:4200";

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true
        });
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }
}
