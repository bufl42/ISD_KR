using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
using System.Windows.Input;
using System.Windows;

namespace Build_BuildersIS.ViewModels
{
    public class TaskViewModel : BaseViewModel
    {
        // Поля
        private int _userID;
        private int _creatorID;
        private int _projectID;
        private string _projectName;
        private byte[] _projectImage;
        private string _taskDescription;
        private DateTime? _deadline;
        private ObservableCollection<User> _allWorkers;
        private ObservableCollection<User> _assignedWorkers;
        private User _selectedWorker;
        private User _selectedAssignedWorker;

        // Свойства
        public int ProjectID
        {
            get => _projectID;
            set { _projectID = value; OnPropertyChanged(); }
        }
        public int UserID
        {
            get => _userID;
            set { _userID = value; OnPropertyChanged(); }
        }
        public int CreatorID
        {
            get => _creatorID;
            set { _creatorID = value; OnPropertyChanged(); }
        }
        public string ProjectName
        {
            get => _projectName;
            set { _projectName = value; OnPropertyChanged(); }
        }
        public string TaskDescription
        {
            get => _taskDescription;
            set { _taskDescription = value; OnPropertyChanged(); }
        }
        public byte[] ProjectImage
        {
            get => _projectImage;
            set { _projectImage = value; OnPropertyChanged(); }
        }
        public DateTime? Deadline
        {
            get => _deadline;
            set { _deadline = value; OnPropertyChanged(); }
        }

        public ObservableCollection<User> AllWorkers
        {
            get => _allWorkers;
            set { _allWorkers = value; OnPropertyChanged(); }
        }

        public ObservableCollection<User> AssignedWorkers
        {
            get => _assignedWorkers;
            set { _assignedWorkers = value; OnPropertyChanged(); }
        }

        public User SelectedWorker
        {
            get => _selectedWorker;
            set { _selectedWorker = value; OnPropertyChanged(); }
        }

        public User SelectedAssignedWorker
        {
            get => _selectedAssignedWorker;
            set { _selectedAssignedWorker = value; OnPropertyChanged(); }
        }

        // Команды
        public ICommand AddWorkerCommand => new RelayCommand(_ => AddWorker(), _ => SelectedWorker != null);
        public ICommand RemoveWorkerCommand => new RelayCommand(_ => RemoveWorker(), _ => SelectedAssignedWorker != null);
        public ICommand CreateTaskCommand => new RelayCommand(_ => CreateTask(), _ => CanCreateTask());

        // Конструктор
        public TaskViewModel()
        {
            AllWorkers = new ObservableCollection<User>();
            AssignedWorkers = new ObservableCollection<User>();

            LoadWorkers();
        }

        // Метод загрузки рабочих из БД
        private void LoadWorkers()
        {
            string query = @"
                SELECT 
                U.user_id, 
                U.name AS Username, 
                U.email, 
                U.role, 
                PF.LastName, 
                PF.FirstName, 
                PF.MiddleName,
                PF.Address, 
                PF.Photo, 
                PF.WorkBookNumber, 
                PF.BirthDate
                FROM Users U
                LEFT JOIN PersonalFiles PF ON U.user_id = PF.UserID
                WHERE U.role = 'WRK'
                ORDER BY U.name;";

            DataTable workerData = DatabaseHelper.ExecuteQuery(query);

            AllWorkers.Clear();
            foreach (DataRow row in workerData.Rows)
            {
                AllWorkers.Add(new User
                {
                    UserID = Convert.ToInt32(row["user_id"]),
                    Username = row["Username"].ToString(),
                    Role = row["role"].ToString(),
                    LastName = row["LastName"]?.ToString(),
                    FirstName = row["FirstName"]?.ToString(),
                    MiddleName = row["MiddleName"]?.ToString(),
                    Photo = row["Photo"] as byte[]

                });
            }
        }

        // Добавление рабочего в список задачи
        private void AddWorker()
        {
            if (SelectedWorker != null && !AssignedWorkers.Contains(SelectedWorker))
            {
                AssignedWorkers.Add(SelectedWorker);
                OnPropertyChanged(nameof(AssignedWorkers));
            }
        }

        // Удаление рабочего из списка задачи
        private void RemoveWorker()
        {
            if (SelectedAssignedWorker != null)
            {
                AssignedWorkers.Remove(SelectedAssignedWorker);
                OnPropertyChanged(nameof(AssignedWorkers));
            }
        }

        // Проверка, можно ли создать задачу
        private bool CanCreateTask()
        {
            return !string.IsNullOrWhiteSpace(TaskDescription) && Deadline.HasValue && AssignedWorkers.Any();
        }

        // Создание задачи в БД
        private void CreateTask()
        {
            try
            {
                // Вставляем задачу
                string insertTaskQuery = @"
                INSERT INTO Task (description, status, deadline, project_id, creator_id) 
                OUTPUT INSERTED.task_id
                VALUES (@description, 'PRO', @deadline, @project_id, @creator_id)";

                var taskParameters = new Dictionary<string, object>
                {
                    { "@description", TaskDescription },
                    { "@deadline", Deadline.Value },
                    {"@project_id", ProjectID },
                    {"@creator_id", CreatorID }
                };

                int taskId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertTaskQuery, taskParameters));

                // Вставляем связи UserTask
                foreach (var worker in AssignedWorkers)
                {
                    string insertUserTaskQuery = @"
                    INSERT INTO UserTask (user_id, task_id) 
                    VALUES (@userId, @taskId)";

                    var userTaskParameters = new Dictionary<string, object>
                {
                    { "@userId", worker.UserID },
                    { "@taskId", taskId }
                };

                    DatabaseHelper.ExecuteNonQuery(insertUserTaskQuery, userTaskParameters);
                }

                MessageBox.Show("Задача успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
