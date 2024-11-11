using Build_BuildersIS.Entarence;
using Build_BuildersIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Build_BuildersIS.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _confirmPassword;
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
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand
        {
            get
            {
                if (_registerCommand == null)
                {
                    _registerCommand = new RelayCommand(param => Register(param as Window));
                }
                return _registerCommand;
            }
        }

        private void Register(Window window)
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

            if (!Registration.IsUsernameUnique(Username))
            {
                ShowErrorMessage(window, "Пользователь с таким именем уже существует.", false);
                return;
            }

            if (Password != ConfirmPassword)
            {
                ShowErrorMessage(window, "Пароли не совпадают.", false);
                return;
            }

            string passwordHash = HashFunc.ComputeSha256Hash(Password);

            if (Registration.RegisterUser(Username, passwordHash))
            {
                ShowErrorMessage(window, "Регистрация успешна!\nТеперь вы можете войти в систему.", true);
                window.Close();
            }
            else
            {
                ShowErrorMessage(window, "Произошла ошибка при регистрации. Попробуйте снова.", false);
            }
        }
    }
}
