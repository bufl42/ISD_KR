using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
using Build_BuildersIS.Views;

namespace Build_BuildersIS.ViewModels
{
    public class TaskDescriptionViewModel : BaseViewModel
    {
        private int _taskID;
        private string _taskDescription;
        private DateTime? _deadline;
        private string _projectName;
        private byte[] _projectImage;
        private ObservableCollection<ObjectItem> _allObjects;
        private ObservableCollection<User> _assignedWorkers;
        private ObjectItem _selectedObject;

        // Свойства
        public int TaskID
        {
            get => _taskID;
            set { _taskID = value; OnPropertyChanged(); LoadTaskDetails(); }
        }

        public string TaskDescription
        {
            get => _taskDescription;
            set { _taskDescription = value; OnPropertyChanged(); }
        }

        public DateTime? Deadline
        {
            get => _deadline;
            set { _deadline = value; OnPropertyChanged(); }
        }

        public string ProjectName
        {
            get => _projectName;
            set { _projectName = value; OnPropertyChanged(); }
        }

        public byte[] ProjectImage
        {
            get => _projectImage;
            set { _projectImage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ObjectItem> AllObjects
        {
            get => _allObjects;
            set { _allObjects = value; OnPropertyChanged(); }
        }

        public ObservableCollection<User> AssignedWorkers
        {
            get => _assignedWorkers;
            set { _assignedWorkers = value; OnPropertyChanged(); }
        }
        public ObjectItem SelectedObject
        {
            get => _selectedObject;
            set { _selectedObject = value; OnPropertyChanged(); }
        }
        public ICommand EditObjectCommand => new RelayCommand(param => OpenEditObject(param as Window), _ => SelectedObject != null);

        // Конструктор
        public TaskDescriptionViewModel()
        {
            AllObjects = new ObservableCollection<ObjectItem>();
            AssignedWorkers = new ObservableCollection<User>();
        }

        // Загрузка данных задачи
        private void LoadTaskDetails()
        {
            if (TaskID <= 0) return;

            // Загружаем информацию о задаче
            string taskQuery = @"
            SELECT t.description, t.deadline, p.name AS project_name, p.imagedata 
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
                Deadline = row["deadline"] as DateTime?;
                ProjectName = row["project_name"].ToString();
                ProjectImage = row["imagedata"] as byte[];
            }

            // Загружаем назначенных рабочих
            string workersQuery = @"
            SELECT u.user_id, u.name, u.role, pf.photo 
            FROM Users u
            JOIN UserTask ut ON u.user_id = ut.user_id
            LEFT JOIN PersonalFiles pf ON u.user_id = pf.UserID
            WHERE ut.task_id = @taskID";

            var workersData = DatabaseHelper.ExecuteQuery(workersQuery, new Dictionary<string, object>
            {
                { "@taskID", TaskID }
            });

            AssignedWorkers.Clear();
            foreach (DataRow row in workersData.Rows)
            {
                AssignedWorkers.Add(new User
                {
                    UserID = Convert.ToInt32(row["user_id"]),
                    Username = row["name"].ToString(),
                    Role = row["role"].ToString(),
                    Photo = row["photo"] as byte[]
                });
            }

            // Загружаем все объекты проекта
            LoadAllObjects();
        }

        // Загрузка всех объектов проекта
        private void LoadAllObjects()
        {
            string query = @"
            SELECT o.object_id, o.name, o.description, o.imagedata 
            FROM Object o
            LEFT JOIN ProjectObject po ON o.object_id = po.object_id
            WHERE po.project_id = (SELECT project_id FROM Task WHERE task_id = @taskID)";

            var data = DatabaseHelper.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@taskID", TaskID }
            });

            AllObjects.Clear();
            foreach (DataRow row in data.Rows)
            {
                AllObjects.Add(new ObjectItem
                {
                    ObjectID = Convert.ToInt32(row["object_id"]),
                    Name = row["name"].ToString(),
                    Description = row["description"].ToString(),
                    ImageData = row["imagedata"] as byte[]
                });
            }
        }
        private void OpenEditObject(Window window)
        {
            try
            {
                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var ojectWindow = new ObjectWindow()
                {
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };
                ojectWindow.Left = window.Left + (window.Width - ojectWindow.Width) / 2;
                ojectWindow.Top = window.Top + (window.Height - ojectWindow.Height) / 2;

                ojectWindow.Closed += (sender, e) => WindowClosed(window);

                var viewModel = ojectWindow.DataContext as ObjectViewModel;
                viewModel?.InitializeForEdit(SelectedObject);
                ojectWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
