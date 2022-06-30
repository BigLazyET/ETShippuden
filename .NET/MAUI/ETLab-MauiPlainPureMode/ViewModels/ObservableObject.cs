using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChange = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;
            backingStore = value;
            onChange?.Invoke();
            // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
