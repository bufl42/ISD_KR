using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
using Build_BuildersIS.Views;
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
        private MaterialRequest _selectedRequest;
        public MaterialRequest SelectedRequest
        {
            get => _selectedRequest;
            set
            {
                _selectedRequest = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<WorkerTask> WorkerTasks { get; set; }
        public ObservableCollection<MaterialRequest> Requests { get; set; } = new ObservableCollection<MaterialRequest>();
        public ObservableCollection<MenuItem> MenuItems { get; set; } = new ObservableCollection<MenuItem>();

        public ICommand OpenCatalogCommand => new RelayCommand(param => OpenCatalog(param as Window));
        public ICommand ApproveRequestCommand => new RelayCommand(param => ApproveRequest(param as  Window),CanApproveOrDeny);
        public ICommand DenyRequestCommand => new RelayCommand(param => DenyRequest(param as Window),CanApproveOrDeny);

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
                M.material_id, M.name AS MaterialName, RM.quantity AS MaterialQuantity, M.unit AS MaterialUnit
                FROM Request R
                JOIN RequestMaterial RM ON R.request_id = RM.request_id
                JOIN Material M ON RM.material_id = M.material_id
                JOIN Object O ON R.object_id = O.object_id
                WHERE R.status = 'PRO'
                ORDER BY R.request_id, M.material_id;";

            DataTable requestData = DatabaseHelper.ExecuteQuery(query);

            Requests.Clear();
            foreach (DataRow row in requestData.Rows)
            {
                int requestId = Convert.ToInt32(row["request_id"]);

                // Проверяем, существует ли запрос с таким ID в списке, чтобы не дублировать его
                var existingRequest = Requests.FirstOrDefault(r => r.RequestID == requestId);

                if (existingRequest == null)
                {
                    // Создаём новый объект запроса, если он не был добавлен ранее
                    var request = new MaterialRequest
                    {
                        RequestID = requestId,
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
                        Quantity = Convert.ToDouble(row["MaterialQuantity"]), // Теперь это запрашиваемое количество
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
                        Quantity = Convert.ToDouble(row["MaterialQuantity"]), // Теперь это запрашиваемое количество
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
                    MenuItems.Add(new MenuItem { Title = "Каталог", Command = OpenCatalogCommand });
                    MenuItems.Add(new MenuItem { Title = "Утвердить запрос", Command = ApproveRequestCommand });
                    MenuItems.Add(new MenuItem { Title = "Отклонить запрос", Command = DenyRequestCommand });
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
        private void OpenCatalog(Window window)
        {
            try
            {
                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var catalogWindow = new CatalogWindow()
                {
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };
                catalogWindow.Left = window.Left + (window.Width - catalogWindow.Width) / 2;
                catalogWindow.Top = window.Top + (window.Height - catalogWindow.Height) / 2;

                catalogWindow.Closed += (sender, e) => WindowClosed(window);
                catalogWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApproveRequest(Window window)
        {
            try
            {
                if (SelectedRequest == null) return;

                // Проверяем наличие материалов на складе
                foreach (var material in SelectedRequest.Materials)
                {
                    string checkQuery = "SELECT quantity FROM Material WHERE name = @MaterialName";
                    var parameters = new Dictionary<string, object>
                    {
                        { "@MaterialName", material.Name }
                    };

                    object result = DatabaseHelper.ExecuteScalar(checkQuery, parameters);
                    double quantityInStock = result == null ? 0 : Convert.ToDouble(result);
                    if (quantityInStock < material.Quantity)
                    {
                        ShowErrorMessage(window, "Недостаточно материалов.", false);
                        return;
                    }
                }

                // Списываем материалы со склада
                foreach (var material in SelectedRequest.Materials)
                {
                    string updateQuery = @"
                        UPDATE Material 
                        SET quantity = quantity - @Quantity 
                        WHERE name = @MaterialName";
                    var parameters = new Dictionary<string, object>
                        {
                            { "@Quantity", material.Quantity },
                            { "@MaterialName", material.Name }
                        };

                    DatabaseHelper.ExecuteNonQuery(updateQuery, parameters);
                }

                // Обновляем статус запроса
                string approveQuery = @"
                        UPDATE Request 
                        SET status = 'APR' 
                        WHERE request_id = @RequestID";
                var approveParameters = new Dictionary<string, object>
                        {
                            { "@RequestID", SelectedRequest.RequestID }
                        };

                DatabaseHelper.ExecuteNonQuery(approveQuery, approveParameters);

                ShowErrorMessage(window, "Запрос успешно подтвержден.", true);
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DenyRequest(Window window)
        {
            if (SelectedRequest == null) return;

            // Обновляем статус запроса
            string denyQuery = @"
                UPDATE Request 
                SET status = 'DEN' 
                WHERE request_id = @RequestID";
            var parameters = new Dictionary<string, object>
            {
                { "@RequestID", SelectedRequest.RequestID }
            };

            DatabaseHelper.ExecuteNonQuery(denyQuery, parameters);
            ShowErrorMessage(window, "Запрос отклонен.", true);
            LoadRequests();
        }

        private bool CanApproveOrDeny(object param)
        {
            return SelectedRequest != null;
        }

    }
}