using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Build_BuildersIS.DataBase;

namespace Build_BuildersIS.ViewModels
{
    public class TaskCompletionReportViewModel : BaseViewModel
    {
        private int _taskID;
        private string _projectName;
        private string _projectAddress;
        private string _taskDescription;
        private string _taskCreator;
        private string _assignedWorkers;
        private DateTime _completionDate;
        private DateTime _deadline;

        public int TaskID
        {
            get => _taskID;
            set { _taskID = value; OnPropertyChanged(); LoadTaskDetails(); }
        }

        public string ProjectName
        {
            get => _projectName;
            set { _projectName = value; OnPropertyChanged(); }
        }

        public string ProjectAddress
        {
            get => _projectAddress;
            set { _projectAddress = value; OnPropertyChanged(); }
        }

        public string TaskDescription
        {
            get => _taskDescription;
            set { _taskDescription = value; OnPropertyChanged(); }
        }

        public string TaskCreator
        {
            get => _taskCreator;
            set { _taskCreator = value; OnPropertyChanged(); }
        }

        public string AssignedWorkers
        {
            get => _assignedWorkers;
            set { _assignedWorkers = value; OnPropertyChanged(); }
        }

        public DateTime CompletionDate
        {
            get => _completionDate;
            set { _completionDate = value; OnPropertyChanged(); }
        }

        public DateTime Deadline
        {
            get => _deadline;
            set { _deadline = value; OnPropertyChanged(); }
        }

        // Загрузка данных задачи
        private void LoadTaskDetails()
        {
            if (TaskID <= 0) return;

            // Запрос на получение данных задачи
            string taskQuery = @"
            SELECT t.description, t.deadline, p.name AS project_name, p.location AS project_address, 
                   t.creator_id
            FROM Task t
            JOIN Project p ON t.project_id = p.project_id
            WHERE t.task_id = @taskID";

            var taskData = DatabaseHelper.ExecuteQuery(taskQuery, new Dictionary<string, object>
            {
                { "@taskID", TaskID }
            });

            if (taskData.Rows.Count > 0)
            {
                var row = taskData.Rows[0];
                TaskDescription = row["description"].ToString();
                ProjectName = row["project_name"].ToString();
                ProjectAddress = row["project_address"].ToString();
                Deadline = Convert.ToDateTime(row["deadline"]);
                CompletionDate = DateTime.Now; // Текущая дата выполнения

                // Получаем ФИО создателя задачи
                TaskCreator = GetFullNameById(Convert.ToInt32(row["creator_id"]));
            }

            // Получаем всех рабочих, которые выполняли задачу
            string workersQuery = @"
            SELECT u.user_id
            FROM UserTask ut
            JOIN Users u ON ut.user_id = u.user_id
            WHERE ut.task_id = @taskID";

            var workersData = DatabaseHelper.ExecuteQuery(workersQuery, new Dictionary<string, object>
            {
                { "@taskID", TaskID }
            });

            var workerNames = new List<string>();
            foreach (DataRow row in workersData.Rows)
            {
                int userId = Convert.ToInt32(row["user_id"]);
                workerNames.Add(GetFullNameById(userId)); // Получаем ФИО рабочего по его user_id
            }
            AssignedWorkers = string.Join(", ", workerNames);
        }

        // Метод для получения ФИО по user_id из таблицы PersonalFile
        private string GetFullNameById(int userId)
        {
            string query = @"
            SELECT pf.LastName, pf.FirstName, pf.MiddleName
            FROM PersonalFiles pf
            WHERE pf.UserID = @userId";

            var data = DatabaseHelper.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@userId", userId }
            });

            if (data.Rows.Count > 0)
            {
                var row = data.Rows[0];
                return $"{row["LastName"]} {row["FirstName"]} {row["MiddleName"]}";
            }
            return "Неизвестно"; // Если не найдено, возвращаем "Неизвестно"
        }
    }
}
