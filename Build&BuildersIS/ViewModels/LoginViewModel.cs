using Build_BuildersIS.Entarence;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using LibraryIS;

namespace Build_BuildersIS.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private ICommand _loginCommand;
        private ICommand _registerCommand;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(param => Login(param as Window));
                }
                return _loginCommand;
            }
        }

        public ICommand RegisterCommand
        {
            get
            {
                if (_registerCommand == null)
                {
                    _registerCommand = new RelayCommand(param => OpenRegisterWindow(param as Window));
                }
                return _registerCommand;
            }
        }

        private void Login(Window window)
        {
            try
            {
                if (string.IsNullOrEmpty(Username))
                {
                    ShowErrorMessage(window, "Имя пользователя не может быть пустым.", false);
                    return;
                }

                if (string.IsNullOrEmpty(Password))
                {
                    ShowErrorMessage(window, "Пароль не может быть пустым.", false);
                    return;
                }

                string passwordHash = HashFunc.ComputeSha256Hash(Password);
                if (Authentication.AuthenticateUserByName(Username, passwordHash, out string userRole))
                {
                    var message = new MessageBoxWindow("Успешная авторизация!", true)
                    {
                        Owner = window,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    window.Hide();

                    var mainWindow = new MainWindow(Username, userRole);
                    mainWindow.Closed += (sender, e) => WindowClosed(window);
                    mainWindow.Show();
                    message.ShowDialog();
                }
                else
                {
                    ShowErrorMessage(window, "Неверное имя пользователя или пароль.", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRegisterWindow(Window window)
        {
            try
            {
                var registerWindow = new RegisterWindow();
                registerWindow.Closed += (sender, e) => WindowClosed(window);
                registerWindow.Show();
                window.Hide();
            }
            catch (Exception ex) { MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }

        }
    }

}
