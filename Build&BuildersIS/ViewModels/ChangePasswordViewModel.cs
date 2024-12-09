using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Build_BuildersIS.DataBase;
using Build_BuildersIS.Entarence;
using System.Windows.Input;
using System.Windows;

namespace Build_BuildersIS.ViewModels
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        private string _newPassword;
        private string _confirmPassword;
        private readonly int _userId;

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public ICommand SavePasswordCommand => new RelayCommand(SavePassword, CanSavePassword);
        public ChangePasswordViewModel()
        {

        }

        public ChangePasswordViewModel(int userId)
        {
            _userId = userId;
        }

        private bool CanSavePassword(object param)
        {
            return !string.IsNullOrWhiteSpace(NewPassword) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   NewPassword == ConfirmPassword;
        }

        private void SavePassword(object param)
        {
            try
            {
                string passwordHash = HashFunc.ComputeSha256Hash(NewPassword);
                string query = "UPDATE Users SET passwordhash = @PasswordHash WHERE user_id = @UserId";
                var parameters = new Dictionary<string, object>
                {
                    { "@PasswordHash", passwordHash },
                    { "@UserId", _userId }
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);

                MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                if (param is Window window)
                    window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при смене пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
