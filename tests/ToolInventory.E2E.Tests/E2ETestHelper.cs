using Microsoft.Playwright;

namespace ToolInventory.E2E.Tests;

internal static class E2ETestHelper
{
    private const string TestEmail = "test@example.com";
    private const string TestPassword = "Test123!@#";

    public static async Task LoginAsync(IPage page)
    {
        await page.GotoAsync("/login");
        await page.WaitForSelectorAsync("input[formcontrolname='email']");
        
        var emailField = page.Locator("input[formcontrolname='email']");
        await emailField.FillAsync(TestEmail);
        
        var passwordField = page.Locator("input[formcontrolname='password']");
        await passwordField.FillAsync(TestPassword);
        
        var signInButton = page.Locator("button:has-text('Sign In')");
        
        try
        {
            // Try to sign in
            await signInButton.ClickAsync();
            await page.WaitForURLAsync("**/tools", new PageWaitForURLOptions { Timeout = 3000 });
        }
        catch
        {
            // Login failed, try to register
            await page.GotoAsync("/register");
            await page.WaitForSelectorAsync("input[formcontrolname='displayName']");
            
            var displayName = page.Locator("input[formcontrolname='displayName']");
            await displayName.FillAsync("E2E Test User");
            
            var regEmail = page.Locator("input[formcontrolname='email']");
            await regEmail.FillAsync(TestEmail);
            
            var regPassword = page.Locator("input[formcontrolname='password']");
            await regPassword.FillAsync(TestPassword);
            
            var registerButton = page.Locator("button:has-text('Register')");
            await registerButton.ClickAsync();
            
            // Wait a bit for the response
            await Task.Delay(2000);
            
            // If we're on login page, try login again
            if (page.Url.Contains("/login"))
            {
                var emailFieldAgain = page.Locator("input[formcontrolname='email']");
                await emailFieldAgain.FillAsync(TestEmail);
                
                var passwordFieldAgain = page.Locator("input[formcontrolname='password']");
                await passwordFieldAgain.FillAsync(TestPassword);
                
                var signInButtonAgain = page.Locator("button:has-text('Sign In')");
                await signInButtonAgain.ClickAsync();
            }
            
            // Wait for tools page
            await page.WaitForURLAsync("**/tools", new PageWaitForURLOptions { Timeout = 5000 });
        }
    }

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
        var snackBar = page.Locator("mat-snack-bar-container").Last;
        await page.WaitForSelectorAsync("mat-snack-bar-container");
        await Assertions.Expect(snackBar).ToContainTextAsync(expectedText);
    }

    public static async Task<string> GetCurrentUserIdAsync(IPage page)
    {
        var userId = await page.EvaluateAsync<string>(
            @"() => {
                const raw = localStorage.getItem('tool_inventory_user');
                if (!raw) return '';
                const token = JSON.parse(raw)?.token;
                if (!token) return '';
                const parts = token.split('.');
                if (parts.length < 2) return '';
                const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
                const payload = JSON.parse(atob(base64));
                return payload?.sub ?? '';
            }");

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Could not resolve authenticated user id from JWT token.");
        }

        return userId;
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
