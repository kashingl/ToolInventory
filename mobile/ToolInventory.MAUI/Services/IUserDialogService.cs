namespace ToolInventory.MAUI.Services;

public interface IUserDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");
    Task ShowAlertAsync(string title, string message, string cancel = "OK");
    Task<string?> PromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? initialValue = null, Keyboard? keyboard = null);
    Task<string?> ActionSheetAsync(string title, string cancel, params string[] buttons);
    Task NavigateAsync(string route, IDictionary<string, object>? parameters = null);
}
