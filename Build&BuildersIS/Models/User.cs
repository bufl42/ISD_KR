using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Build_BuildersIS.Models
{
    public class User : INotifyPropertyChanged
    {
        private string _username;
        private string _userRole;

        public int UserID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
        public string Email { get; set; }
        public string Role { get; set; }
        public BitmapImage RoleIcon { get; set; }
        public string Address { get; set; }
        public byte[] Photo { get; set; }
        public string WorkBookNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    IsUsernameModified = true;
                    OnPropertyChanged();
                }
            }
        }

        public string Password { get; set; }

        public string UserRole
        {
            get => _userRole;
            set
            {
                if (_userRole != value)
                {
                    _userRole = value;
                    IsUserRoleModified = true;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsUsernameModified { get; set; }
        public bool IsUserRoleModified { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(LastName) && !string.IsNullOrEmpty(FirstName))
                    return $"{LastName} {FirstName} {MiddleName}".Trim();
                return Username;
            }
        }
        public string DisplayBirthDate
        {
            get
            {
                return BirthDate.HasValue ? BirthDate.Value.ToString("dd-MM-yyyy") : "Не указано";
            }
        }
        public string DisplayWorkBookNumber
        {
            get
            {
                return string.IsNullOrEmpty(WorkBookNumber) ? "Не указано" : WorkBookNumber;
            }
        }
        public string DisplayRole
        {
            get
            {
                string role;
                switch (Role)
                {
                    case "ADM":
                        role = "Администратор";
                        break;
                    case "WHW":
                        role = "Кладовщик";
                        break;
                    case "MNG":
                        role = "Менеджер проекта";
                        break;
                    default:
                        role = "Рабочий";
                        break;
                }
                return role;
            }
        }
    }
}
