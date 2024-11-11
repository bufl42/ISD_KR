using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.Models
{
    public class User : INotifyPropertyChanged
    {
        private string _username;
        private string _userRole;

        public int UserID { get; set; }
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
    }
}
