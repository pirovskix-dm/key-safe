namespace KeySafe.KsApp.Services;

public static class FileSystemService
{
    public static Task<string> SaveFileDialogAsync(this Window window)
    {
        var saveFileDialog = new SaveFileDialog();
        return saveFileDialog.ShowAsync(window);
    }

    public static async Task<string> OpenFileDialogAsync(this Window window)
    {
        var openFileDialog = new OpenFileDialog();
        return (await openFileDialog.ShowAsync(window))?.FirstOrDefault();
    }

    public static string GetFileName(string file)
    {
        return new FileInfo(file).Name;
    }
}
