using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Build_BuildersIS.DataBase;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Security.Policy;
using Build_BuildersIS.Views;

namespace Build_BuildersIS.ViewModels
{
    public class PersonalFileViewModel : BaseViewModel
    {
        private int _userID;
        private string _lastName;
        private string _firstName;
        private string _middleName;
        private string _address;
        private string _workBookNumber;
        private DateTime? _birthDate;
        private byte[] _photo;

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string MiddleName
        {
            get => _middleName;
            set { _middleName = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public string WorkBookNumber
        {
            get => _workBookNumber;
            set { _workBookNumber = value; OnPropertyChanged(); }
        }

        public DateTime? BirthDate
        {
            get => _birthDate;
            set { _birthDate = value; OnPropertyChanged(); }
        }

        public byte[] Photo
        {
            get => _photo;
            set { _photo = value; OnPropertyChanged(); OnPropertyChanged(nameof(PhotoPreview)); }
        }

        public int UserID
        {
            get => _userID;
            set
            {
                _userID = value;
                OnPropertyChanged();
            }
        }

        public byte[] PhotoPreview => Photo;

        public ICommand SaveCommand => new RelayCommand(SavePersonalFile, CanSavePersonalFile);
        public ICommand ChangePasswordCommand => new RelayCommand(param => OpenChangePasswordWindow(param as Window));

        public PersonalFileViewModel()
        {
            LoadPersonalFile();
        }

        public void Initialize(int userId)
        {
            _userID = userId;
            LoadPersonalFile();
        }

        private void LoadPersonalFile()
        {
            string query = "SELECT LastName, FirstName, MiddleName, Address, WorkBookNumber, BirthDate, Photo FROM PersonalFiles WHERE UserID = @UserId";
            var parameters = new Dictionary<string, object> { { "@UserId", _userID } };

            var result = DatabaseHelper.ExecuteQuery(query, parameters);
            if (result.Rows.Count > 0)
            {
                var row = result.Rows[0];
                LastName = row["LastName"] as string;
                FirstName = row["FirstName"] as string;
                MiddleName = row["MiddleName"] as string;
                Address = row["Address"] as string;
                WorkBookNumber = row["WorkBookNumber"] as string;
                BirthDate = row["BirthDate"] as DateTime?;
                Photo = row["Photo"] as byte[];
            }
        }

        private void SavePersonalFile(object param)
        {
            string query = @"
                IF EXISTS (SELECT 1 FROM PersonalFiles WHERE UserID = @UserId)
                BEGIN
                    UPDATE PersonalFiles
                    SET LastName = @LastName, FirstName = @FirstName, MiddleName = @MiddleName,
                        Address = @Address, WorkBookNumber = @WorkBookNumber, BirthDate = @BirthDate, Photo = @Photo
                    WHERE UserID = @UserId
                END
                ELSE
                BEGIN
                    INSERT INTO PersonalFiles (UserID, LastName, FirstName, MiddleName, Address, WorkBookNumber, BirthDate, Photo)
                    VALUES (@UserId, @LastName, @FirstName, @MiddleName, @Address, @WorkBookNumber, @BirthDate, @Photo)
                END";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", UserID },
                { "@LastName", LastName },
                { "@FirstName", FirstName },
                { "@MiddleName", MiddleName },
                { "@Address", Address },
                { "@WorkBookNumber", WorkBookNumber },
                { "@BirthDate", BirthDate },
                { "@Photo", Photo }
            };

            DatabaseHelper.ExecuteNonQuery(query, parameters);

            if (param is Window window)
            {
                window.Close();
            }
        }
        private bool CanSavePersonalFile(object param)
        {
            return !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(MiddleName) &&
                   !string.IsNullOrWhiteSpace(WorkBookNumber) &&
                   !string.IsNullOrWhiteSpace(Address) &&
                   Photo != null;
        }
        public void HandleImageDrop(string filePath)
        {
            try
            {
                Photo = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
            }
        }

        private void OpenChangePasswordWindow(Window window)
        {
            var overlay = window.FindName("Overlay") as UIElement;
            if (overlay != null)
            {
                overlay.Visibility = Visibility.Visible;
            }
            var changePasswordWindow = new ChangePasswordWindow
            {
                Owner = window,
                WindowStartupLocation = WindowStartupLocation.Manual,
                DataContext = new ChangePasswordViewModel(UserID)
            };

            changePasswordWindow.Left = window.Left + (window.Width - changePasswordWindow.Width) / 2;
            changePasswordWindow.Top = window.Top + (window.Height - changePasswordWindow.Height) / 2;

            changePasswordWindow.Closed += (sender, e) => WindowClosed(window);
            changePasswordWindow.ShowDialog();

        }

    }
}
