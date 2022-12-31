using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;

namespace KeySafe.ViewModels.Commands;

public class AsyncDelegateUnit
{
}

public class AsyncDelegateCommand : AsyncDelegateCommand<AsyncDelegateUnit>
{
    public AsyncDelegateCommand(Func<Task> command, Func<bool> canExecute = null, Func<Exception, Task> exceptionHandler = null)
        : base(_ => command(), _ => canExecute?.Invoke() ?? true, exceptionHandler)
    {
    }
}

public class AsyncDelegateCommand<T> : IAsyncCommand<T>
{
    public event EventHandler CanExecuteChanged;

    private bool _isExecuting;

    private readonly Func<T, Task> _command;

    private readonly Func<T, bool> _canExecute;

    private readonly Func<Exception, Task> _errorHandler;

    public AsyncDelegateCommand(Func<T, Task> command, Func<T, bool> canExecute = null, Func<Exception, Task> errorHandler = null)
    {
        _isExecuting = false;
        _command = command;
        _canExecute = canExecute;
        _errorHandler = errorHandler;
    }

    public bool CanExecute(object parameter)
    {
        if (typeof(T) == typeof(AsyncDelegateUnit))
        {
            return !_isExecuting && (_canExecute?.Invoke(default) ?? true);
        }

        return !_isExecuting && (_canExecute?.Invoke((T)parameter) ?? true);
    }

    public void Execute(object parameter)
    {
        ExecuteAsync(typeof(T) == typeof(AsyncDelegateUnit) ? default : (T)parameter).SafeFireAndForget();
    }

    public async Task ExecuteAsync(T parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                await _command.Invoke(parameter);
            }
            catch (Exception ex)
            {
                await (_errorHandler?.Invoke(ex) ?? Task.CompletedTask);
                var actualException = ex.InnerException?.InnerException ?? ex.InnerException ?? ex;
                var type = $"{ex.GetType().FullName} | {actualException.GetType().FullName}{Environment.NewLine}";
                Console.WriteLine($"{type}{actualException.Message}{Environment.NewLine}{actualException.StackTrace}");
                _isExecuting = false;
            }
            finally
            {
                _isExecuting = false;
            }
        }

        RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
