using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Build_BuildersIS.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _username;
        private string _userRole;


        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<WorkerTask> WorkerTasks { get; set; }
        public ObservableCollection<MaterialRequest> Requests { get; set; } = new ObservableCollection<MaterialRequest>();
        public ObservableCollection<MenuItem> MenuItems { get; set; } = new ObservableCollection<MenuItem>();

        public ICommand AddMaterialCommand => new RelayCommand(param => AddMaterial());

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string UserRole
        {
            get => _userRole;
            set
            {
                _userRole = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {

        }

        public MainViewModel(string username, string userRole)
        {
            Projects = new ObservableCollection<Project>();
            WorkerTasks = new ObservableCollection<WorkerTask>();
            Username = username;
            UserRole = userRole;
            LoadProjects();
            LoadWorkerTasks();
            LoadRequests();
            LoadMenuItems();
        }


        private void LoadProjects()
        {
            string query = "SELECT project_id, name, description, start_date, end_date, imagedata FROM Project";

            DataTable projectData = DatabaseHelper.ExecuteQuery(query);

            foreach (DataRow row in projectData.Rows)
            {
                Projects.Add(new Project
                {
                    ProjectID = Convert.ToInt32(row["project_id"]),
                    ProjectName = row["name"].ToString(),
                    ProjectDescription = row["description"].ToString(),
                    StartDate = Convert.ToDateTime(row["start_date"]),
                    EndDate = Convert.ToDateTime(row["end_date"]),
                    ProjectImage = row["imagedata"] as byte[]
                });
            }
        }

        private void LoadWorkerTasks()
        {
            try
            {
                string query = @"
                SELECT T.task_id, T.description, T.status, T.deadline, 
                O.location AS ObjectAddress, O.imagedata AS ObjectImage
                FROM Task T
                JOIN Object O ON T.object_id = O.object_id
                JOIN Users U ON T.user_id = U.user_id
                WHERE U.name = @UserName";

                var parameters = new Dictionary<string, object>
                {
                    { "@UserName", Username }
                };

                DataTable taskData = DatabaseHelper.ExecuteQuery(query, parameters);

                foreach (DataRow row in taskData.Rows)
                {
                    WorkerTasks.Add(new WorkerTask
                    {
                        TaskID = Convert.ToInt32(row["task_id"]),
                        ObjectAddress = row["ObjectAddress"].ToString(),
                        TaskDescription = row["description"].ToString(),
                        TaskStatus = row["status"].ToString(),
                        Deadline = Convert.ToDateTime(row["deadline"]),
                        ObjectImage = row["ObjectImage"] as byte[]
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void LoadRequests()
        {
            string query = @"
                SELECT R.request_id, R.request_date, 
                O.object_id, O.location AS ObjectAddress, O.imagedata AS ObjectImage,
                M.name AS MaterialName, M.quantity AS MaterialQuantity, M.unit AS MaterialUnit
                FROM Request R
                JOIN Object O ON R.object_id = O.object_id
                JOIN RequestMaterial RM ON R.request_id = RM.request_id
                JOIN Material M ON RM.material_id = M.material_id";

            DataTable requestData = DatabaseHelper.ExecuteQuery(query);

            Requests.Clear();
            foreach (DataRow row in requestData.Rows)
            {
                // Проверяем, существует ли запрос с таким ID в списке, чтобы не дублировать его
                var existingRequest = Requests.FirstOrDefault(r => r.RequestID == Convert.ToInt32(row["request_id"]));

                if (existingRequest == null)
                {
                    // Создаём новый объект запроса, если он не был добавлен ранее
                    var request = new MaterialRequest
                    {
                        RequestID = Convert.ToInt32(row["request_id"]),
                        ObjectID = Convert.ToInt32(row["object_id"]),
                        ObjectAddress = row["ObjectAddress"].ToString(),
                        RequestDate = Convert.ToDateTime(row["request_date"]),
                        ObjectImage = row["ObjectImage"] as byte[],
                        Materials = new List<MaterialItem>()
                    };

                    // Добавляем материал к новому запросу
                    request.Materials.Add(new MaterialItem
                    {
                        Name = row["MaterialName"].ToString(),
                        Quantity = Convert.ToDouble(row["MaterialQuantity"]),
                        Unit = row["MaterialUnit"].ToString()
                    });

                    Requests.Add(request);
                }
                else
                {
                    // Если запрос уже существует, добавляем только материал
                    existingRequest.Materials.Add(new MaterialItem
                    {
                        Name = row["MaterialName"].ToString(),
                        Quantity = Convert.ToDouble(row["MaterialQuantity"]),
                        Unit = row["MaterialUnit"].ToString()
                    });
                }
            }
        }
        private void LoadMenuItems()
        {
            MenuItems.Clear();

            switch (UserRole)
            {
                case "WHW":
                    MenuItems.Add(new MenuItem { Title = "Каталог", Command = AddMaterialCommand });
                    MenuItems.Add(new MenuItem { Title = "Новый материал", Command = AddMaterialCommand });
                    MenuItems.Add(new MenuItem { Title = "Закрыть запрос", Command = AddMaterialCommand });
                    // Добавьте другие кнопки для Кладовщика при необходимости
                    break;

                //case "Менеджер":
                //    MenuItems.Add(new MenuItem { Title = "Создать новый проект", Command = CreateProjectCommand });
                //    MenuItems.Add(new MenuItem { Title = "Запрос на материалы", Command = RequestMaterialsCommand });
                //    // Добавьте другие кнопки для Менеджера при необходимости
                //    break;

                //case "Рабочий":
                //    MenuItems.Add(new MenuItem { Title = "Просмотр задач", Command = ViewTasksCommand });
                //    // Добавьте другие кнопки для Рабочего при необходимости
                //    break;

                //case "Администратор":
                //    MenuItems.Add(new MenuItem { Title = "Управление пользователями", Command = ManageUsersCommand });
                //    MenuItems.Add(new MenuItem { Title = "Просмотр всех проектов", Command = ViewAllProjectsCommand });
                //    // Добавьте другие кнопки для Администратора при необходимости
                //    break;
            }
        }

        private void AddMaterial()
        {
            // Логика для добавления нового материала в БД
            // Откройте окно для ввода данных о материале и сохраните его в базе данных
        }
    }
}