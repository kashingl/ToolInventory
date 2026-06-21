using System.Diagnostics;
using System.Net;
using Microsoft.Playwright;

namespace ToolInventory.E2E.Tests.Fixtures;

public class PlaywrightFixture : IAsyncLifetime
{
    public const string BaseUrl = "http://localhost:4201";

    private Process? _apiProcess;
    private Process? _frontendProcess;

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await StartServersAsync();

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
        StopProcess(_frontendProcess);
        StopProcess(_apiProcess);
    }

    public async Task LoginForTestAsync()
    {
        await E2ETestHelper.LoginAsync(Page);
    }

    private async Task StartServersAsync()
    {
        var repoRoot = FindRepoRoot();
        var frontendRoot = Path.Combine(repoRoot, "frontend", "tool-inventory-app");
        var apiProject = Path.Combine(repoRoot, "src", "ToolInventory.API", "ToolInventory.API.csproj");

        _apiProcess = StartProcess(
            "dotnet",
            $"run --project \"{apiProject}\" --urls \"http://localhost:5177;https://localhost:7226\"",
            repoRoot,
            new Dictionary<string, string>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["TOOLINVENTORY_JWT_KEY"] = "ToolInventory_Test_Secret_Key_AtLeast32Chars!"
            });

        await WaitForUrlAsync("http://localhost:5177/health", TimeSpan.FromSeconds(120));

        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        _frontendProcess = StartProcess(
            isWindows ? "cmd.exe" : "npm",
            isWindows
                ? "/c npm run start -- --port 4201 --host 127.0.0.1"
                : "run start -- --port 4201 --host 127.0.0.1",
            frontendRoot,
            null);

        await WaitForUrlAsync($"{BaseUrl}/login", TimeSpan.FromSeconds(180));
    }

    private static Process StartProcess(string fileName, string arguments, string workingDirectory, IReadOnlyDictionary<string, string>? environment)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (environment is not null)
        {
            foreach (var pair in environment)
            {
                startInfo.Environment[pair.Key] = pair.Value;
            }
        }

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start process: {fileName}");
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static async Task WaitForUrlAsync(string url, TimeSpan timeout)
    {
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            AllowAutoRedirect = true
        };
        using var client = new HttpClient(handler);
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            try
            {
                using var response = await client.GetAsync(url);
                if (response.StatusCode != HttpStatusCode.NotFound && (int)response.StatusCode < 500)
                {
                    return;
                }
            }
            catch
            {
                // Server is still starting.
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException($"Timed out waiting for {url}");
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var frontendPackage = Path.Combine(current.FullName, "frontend", "tool-inventory-app", "package.json");
            var apiProject = Path.Combine(current.FullName, "src", "ToolInventory.API", "ToolInventory.API.csproj");
            if (File.Exists(frontendPackage) && File.Exists(apiProject))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root for E2E tests.");
    }

    private static void StopProcess(Process? process)
    {
        if (process is null || process.HasExited)
        {
            return;
        }

        process.Kill(entireProcessTree: true);
        process.WaitForExit(10000);
    }
}
