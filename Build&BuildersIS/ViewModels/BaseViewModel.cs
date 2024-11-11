using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using LibraryIS;

namespace Build_BuildersIS.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ICommand CloseCommand
        {
            get
            {
                return new RelayCommand(param => CloseWindow(param as Window));
            }
        }

        public ICommand MinimizeCommand
        {
            get
            {
                return new RelayCommand(param => MinimizeWindow(param as Window));
            }
        }

        protected void WindowClosed(Window window)
        {
            if (window == null)
                return;

            var overlay = window.FindName("Overlay") as UIElement;
            if (overlay != null)
            {
                overlay.Visibility = Visibility.Collapsed;
            }

            window.Show();
        }

        protected void ShowErrorMessage(Window window, string message, bool action)
        {
            if (window == null)
                return;

            var overlay = window.FindName("Overlay") as UIElement;
            if (overlay != null)
            {
                overlay.Visibility = Visibility.Visible;
            }

            var messageBox = new MessageBoxWindow(message, action)
            {
                Owner = window,
                WindowStartupLocation = WindowStartupLocation.Manual
            };
            messageBox.Left = window.Left + (window.Width - messageBox.Width) / 2;
            messageBox.Top = window.Top + (window.Height - messageBox.Height) / 2;

            messageBox.Closed += (sender, e) => WindowClosed(window);
            messageBox.ShowDialog();
        }

        private void CloseWindow(Window window)
        {
            window?.Close();
        }

        private void MinimizeWindow(Window window)
        {
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}
