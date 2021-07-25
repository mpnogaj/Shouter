using System.Collections.Generic;
using System.ComponentModel;

namespace Radio.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void CheckAndSetProperty<T>(ref T storage, T value, string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return;
            storage = value;
            OnPropertyChanged(propertyName);
        }
    }
}
