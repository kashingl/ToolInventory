namespace ToolInventory.MAUI.Services;

public class UserDialogService : IUserDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
        => Shell.Current.DisplayAlertAsync(title, message, accept, cancel);

    public Task ShowAlertAsync(string title, string message, string cancel = "OK")
        => Shell.Current.DisplayAlertAsync(title, message, cancel);

    public Task<string?> PromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? initialValue = null, Keyboard? keyboard = null)
        => Shell.Current.DisplayPromptAsync(title, message, accept, cancel, initialValue: initialValue, keyboard: keyboard);

    public Task<string?> ActionSheetAsync(string title, string cancel, params string[] buttons)
        => Shell.Current.DisplayActionSheetAsync(title, cancel, null, buttons);

    public Task NavigateAsync(string route, IDictionary<string, object>? parameters = null)
        => parameters is null
            ? Shell.Current.GoToAsync(route)
            : Shell.Current.GoToAsync(route, parameters);
}
