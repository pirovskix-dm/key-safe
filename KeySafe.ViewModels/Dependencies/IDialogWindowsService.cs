using KeySafe.ViewModels.ViewModels;

namespace KeySafe.ViewModels.Dependencies;

public enum LoginAction
{
    Login,
    Register,
}

public readonly record struct LoginResult(string File, string Password, LoginAction LoginAction);

public readonly record struct SafeItemEditResult(string Name, string Login, string Password, string Web, string Note, bool Cancel);

public readonly record struct PasswordChangeResult(string OldPassword, string NewPassword, bool Cancel);

public readonly record struct PasswordConfirmationResult(string Password, bool Cancel);

public readonly record struct ExportResult(string File);

public readonly record struct ImportResult(string File);

public interface IDialogWindowsService
{
    Task<SafeItemEditResult> ShowSafeItemWindowAsync(SafeItemViewModel safeItem = null, string errorMessage = null);
    Task<LoginResult> ShowLoginWindowAsync(string loginFile, string errorMessage = null);
    Task<PasswordChangeResult> ShowPasswordChangeWindowAsync(string errorMessage = null);
    Task<PasswordConfirmationResult> ShowPasswordConfirmationWindowAsync();
    Task<ExportResult> ShowExportWindowAsync();
    Task<ImportResult> ShowImportWindowAsync();
    void ShutdownApplication();
}
