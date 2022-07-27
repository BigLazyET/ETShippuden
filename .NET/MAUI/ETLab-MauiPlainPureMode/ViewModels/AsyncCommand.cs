using System.ComponentModel;
using System.Windows.Input;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class AsyncCommand : ICommand
    {
        private Func<object, Task> _execute;
        private Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public AsyncCommand(Func<object, Task> execute, Func<object, bool> canExecute = null, INotifyPropertyChanged notificationSource = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (_ => true);
            if (notificationSource != null)
                notificationSource.PropertyChanged += RaiseCanExecuteChanged;
        }

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null, INotifyPropertyChanged notificationSource = null)
            : this(_ => execute.Invoke(), _ => (canExecute ?? (() => true)).Invoke(), notificationSource) { }

        private void RaiseCanExecuteChanged(object sender, PropertyChangedEventArgs e)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter = null)
        {
            return _canExecute(parameter);
        }

        public Task ExecuteAsync(object parameter = null)
        {
            return _execute(parameter);
        }

        public async void Execute(object parameter = null)
        {
            try
            {
                await ExecuteAsync(parameter);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
