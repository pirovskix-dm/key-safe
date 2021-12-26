using KeySafe.ViewModels.ViewModels;

namespace KeySafe.ViewModels.Dependencies;

public enum LoginAction
{
    Login,
    Register,
}

public readonly record struct LoginResult(string Login, string Password, LoginAction LoginAction);

public readonly record struct SafeItemEditResult(string Name, string Login, string Password, string Web, string Note, bool Cancel);

public readonly record struct PasswordChangeResult(string OldPassword, string NewPassword, bool Cancel);

public interface IDialogWindowsService
{
    Task<SafeItemEditResult> ShowSafeItemWindowAsync(SafeItemViewModel safeItem = null);
    Task<LoginResult> ShowLoginWindowAsync(string errorMessage = null);
    Task<PasswordChangeResult> ShowPasswordChangeWindowAsync(string errorMessage = null);
}
