using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.Models
{
    public class RoleDescription
    {
        public static string GetRoleDescription(string role)
        {
            switch (role)
            {
                case "ADM":
                    return "Администратор";
                case "LIB":
                    return "Библиотекарь";
                case "USR":
                    return "Пользователь";
                default:
                    return "Неизвестная роль";
            }
        }
    }
}
