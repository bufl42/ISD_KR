using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
using Build_BuildersIS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Build_BuildersIS.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _username;
        private string _userRole;
        private MaterialRequest _selectedRequest;
        private User _selectedUser;
        private ObservableCollection<User> _filteredusers;
        private ObservableCollection<Project> _filteredprojects;
        private ObservableCollection<MaterialRequest> _filteredrequsts;
        private string _searchQuery;
        public ObservableCollection<User> Users;
        private byte[] _userPhoto;
        private Project _selectedProject;
        private WorkerTask _selectedTask;

        public MaterialRequest SelectedRequest
        {
            get => _selectedRequest;
            set
            {
                _selectedRequest = value;
                OnPropertyChanged();
            }
        }
        public WorkerTask SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
            }
        }

        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<User> FilteredUsers
        {
            get => _filteredusers;
            set
            {
                _filteredusers = value;
                OnPropertyChanged(nameof(FilteredUsers));
            }
        }

        public ObservableCollection<Project> FilteredProjects
        {
            get => _filteredprojects;
            set
            {
                _filteredprojects = value;
                OnPropertyChanged(nameof(FilteredProjects));
            }
        }
        public ObservableCollection<MaterialRequest> FilteredRequests
        {
            get => _filteredrequsts;
            set
            {
                _filteredrequsts = value;
                OnPropertyChanged(nameof(FilteredRequests));
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                FilterElemets();
            }
        }

        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<WorkerTask> WorkerTasks { get; set; }
        public ObservableCollection<MaterialRequest> Requests { get; set; } = new ObservableCollection<MaterialRequest>();
        public ObservableCollection<MenuItem> MenuItems { get; set; } = new ObservableCollection<MenuItem>();

        public ICommand OpenCatalogCommand => new RelayCommand(param => OpenCatalog(param as Window));
        public ICommand OpenNewProjectCommand => new RelayCommand(param => OpenNewProject(param as Window));
        public ICommand OpenEditProjectCommand => new RelayCommand(param => OpenEditProject(param as Window),CanEditProject);
        public ICommand CreateRequestCommand => new RelayCommand(param => CreateRequest(param as Window), CanEditProject);
        public ICommand CreateTaskCommand => new RelayCommand(param => CreateTask(param as Window), CanEditProject);
        public ICommand DeleteProjectCommand => new RelayCommand(param => DeleteProject(param as Window), CanEditProject);
        public ICommand OpenTaskDescriptionCommand => new RelayCommand(param => OpenTaskDescription(param as Window), CanOpenDescription);
        public ICommand CompliteTaskCommand => new RelayCommand(param => CompliteTask(param as Window), CanOpenDescription);
        public ICommand OpenPersonalFileCommand => new RelayCommand(param => OpenPersonalFile(param as Window,Username));
        public ICommand OpenEditPersonalFileCommand => new RelayCommand(param => OpenEditPersonalFile(param as Window), CanEditUser);
        public ICommand ApproveRequestCommand => new RelayCommand(param => ApproveRequest(param as  Window),CanApproveOrDeny);
        public ICommand DenyRequestCommand => new RelayCommand(param => DenyRequest(param as Window),CanApproveOrDeny);
        public ICommand GoToManagerFunctionalityCommand => new RelayCommand(param => GoToManagerFunctionality(param as Window));
        public ICommand GoToAdminFunctionalityCommand => new RelayCommand(param => GoToAdminFunctionality(param as Window));
        public ICommand GoToWarehouseworkerFunctionalityCommand => new RelayCommand(param => GoToWarehouseworkerFunctionality(param as Window));
        public ICommand SearchCommand => new RelayCommand(param => FilterElemets());
        public ICommand ResetCommand => new RelayCommand(param => ResetSearch());
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
            Users = new ObservableCollection<User>();
            FilteredUsers = new ObservableCollection<User>();
            FilteredProjects = new ObservableCollection<Project>();
            FilteredRequests = new ObservableCollection<MaterialRequest>();
            Username = username;
            UserRole = userRole;
            LoadProjects();
            LoadWorkerTasks();
            LoadRequests();
            LoadMenuItems();
            LoadUsers();
        }


        public string DisplayName

        {
            get
            {
                string query = @"
                    SELECT p.LastName, p.FirstName, p.MiddleName
                    FROM Users u
                    LEFT JOIN PersonalFiles p ON u.user_id = p.UserID
                    WHERE u.name = @Username";

                var parameters = new Dictionary<string, object> { { "@Username", Username } };

                var result = DatabaseHelper.ExecuteQuery(query, parameters);

                if (result.Rows.Count > 0)
                {
                    var row = result.Rows[0];
                    string lastName = row["LastName"] as string;
                    string firstName = row["FirstName"] as string;
                    string middleName = row["MiddleName"] as string;

                    if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                        return $"{lastName} {firstName} {middleName}".Trim();
                }

                return Username;
            }
        }
        public byte[] ReturnUserPhoto
        {
            get
            {
                if (string.IsNullOrEmpty(Username)) return null;

                try
                {
                    string userQuery = "SELECT user_id FROM Users WHERE name = @UserName";
                    var userParams = new Dictionary<string, object> { { "@UserName", Username } };
                    var userResult = DatabaseHelper.ExecuteQuery(userQuery, userParams);

                    if (userResult.Rows.Count == 0)
                        return null; 

                    int userId = Convert.ToInt32(userResult.Rows[0]["user_id"]);

                    string photoQuery = "SELECT Photo FROM PersonalFiles WHERE UserID = @UserId";
                    var photoParams = new Dictionary<string, object> { { "@UserId", userId } };
                    var photoResult = DatabaseHelper.ExecuteQuery(photoQuery, photoParams);

                    if (photoResult.Rows.Count == 0 || photoResult.Rows[0]["Photo"] == DBNull.Value)
                        return null;

                    _userPhoto = photoResult.Rows[0]["Photo"] as byte[];
                    return _userPhoto;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке фото: {ex.Message}");
                    return null;
                }
            }
        }
        private void LoadProjects()
        {
            string projectQuery = "SELECT project_id, name, description, start_date, imagedata FROM Project";
            string projectObjectsQuery = "SELECT po.project_id, o.object_id, o.name, o.description, o.imagedata " +
                                         "FROM ProjectObject po " +
                                         "JOIN Object o ON po.object_id = o.object_id";

            DataTable projectData = DatabaseHelper.ExecuteQuery(projectQuery);
            DataTable projectObjectsData = DatabaseHelper.ExecuteQuery(projectObjectsQuery);

            var projectObjectsDict = projectObjectsData.AsEnumerable()
                .GroupBy(row => Convert.ToInt32(row["project_id"]))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(row => new ObjectItem
                    {
                        ObjectID = Convert.ToInt32(row["object_id"]),
                        Name = row["name"].ToString(),
                        Description = row["description"].ToString(),
                        ImageData = row["imagedata"] as byte[]
                    }).ToList()
                );

            foreach (DataRow row in projectData.Rows)
            {
                int projectId = Convert.ToInt32(row["project_id"]);

                Projects.Add(new Project
                {
                    ProjectID = projectId,
                    ProjectName = row["name"].ToString(),
                    ProjectDescription = row["description"].ToString(),
                    StartDate = Convert.ToDateTime(row["start_date"]),
                    ProjectImage = row["imagedata"] as byte[],
                    ProjectObjects = new ObservableCollection<ObjectItem>(
                        projectObjectsDict.ContainsKey(projectId) ? projectObjectsDict[projectId] : new List<ObjectItem>()
                    )
                });
            }

            FilteredProjects = new ObservableCollection<Project>(Projects);
        }

        private void LoadWorkerTasks()
        {
            try
            {
                // Обновлённый SQL-запрос с правильными именами полей таблиц
                string query = @"
                SELECT T.task_id, T.description, T.status, T.deadline, 
                       P.location AS ProjectAddress, P.imagedata AS ProjectImage
                FROM Task T
                JOIN UserTask UT ON T.task_id = UT.task_id
                JOIN Project P ON T.project_id = P.project_id
                JOIN Users U ON UT.user_id = U.user_id
                WHERE U.name = @Username";

                // Параметры для передачи в запрос
                var parameters = new Dictionary<string, object>
                {
                    { "@Username", Username } // Поле Username из контекста текущего пользователя
                };

                // Выполнение запроса и получение данных
                DataTable taskData = DatabaseHelper.ExecuteQuery(query, parameters);

                // Очистка предыдущего списка задач
                WorkerTasks.Clear();

                // Заполнение списка задач на основе полученных данных
                foreach (DataRow row in taskData.Rows)
                {
                    WorkerTasks.Add(new WorkerTask
                    {
                        TaskID = Convert.ToInt32(row["task_id"]),
                        TaskDescription = row["description"]?.ToString() ?? "Описание отсутствует",
                        TaskStatus = row["status"]?.ToString() ?? "Не указан",
                        Deadline = Convert.ToDateTime(row["deadline"]),
                        ProjectAddress = row["ProjectAddress"]?.ToString() ?? "Адрес отсутствует",
                        ProjectImage = row["ProjectImage"] as byte[]
                    });
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при выполнении запроса или преобразовании данных
                MessageBox.Show($"Произошла ошибка при загрузке задач: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRequests()
        {
            string query = @"
                SELECT R.request_id, R.request_date, 
                R.project_id, P.description AS ProjectDescription, P.imagedata AS ProjectImage,
                M.material_id, M.name AS MaterialName, RM.quantity AS MaterialQuantity, M.unit AS MaterialUnit
                FROM Request R
                JOIN RequestMaterial RM ON R.request_id = RM.request_id
                JOIN Material M ON RM.material_id = M.material_id
                JOIN Project P ON R.project_id = P.project_id
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
                        ProjectID = Convert.ToInt32(row["project_id"]),
                        RequestDate = Convert.ToDateTime(row["request_date"]),
                        ProjectImage = row["ProjectImage"] as byte[],
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

            FilteredRequests = new ObservableCollection<MaterialRequest>(Requests);
        }

        private void LoadUsers()
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
                ORDER BY U.name;";

            DataTable userData = DatabaseHelper.ExecuteQuery(query);

            Users.Clear();
            foreach (DataRow row in userData.Rows)
            {
                string userRole = row["role"].ToString();
                string imagePath;
                switch (userRole)
                {
                    case "ADM":
                        imagePath = "pack://application:,,,/Resources/account-star.png";
                        break;
                    case "WHW":
                        imagePath = "pack://application:,,,/Resources/warehouse-worker.png";
                        break;
                    case "MNG":
                        imagePath = "pack://application:,,,/Resources/account-tie.png";
                        break;
                    default:
                        imagePath = "pack://application:,,,/Resources/worker.png";
                        break;
                }

                BitmapImage roleIcon = new BitmapImage(new Uri(imagePath));

                Users.Add(new User
                {
                    UserID = Convert.ToInt32(row["user_id"]),
                    Username = row["Username"].ToString(),
                    Email = row["email"].ToString(),
                    Role = userRole,
                    RoleIcon = roleIcon,
                    LastName = row["LastName"]?.ToString(),
                    FirstName = row["FirstName"]?.ToString(),
                    MiddleName = row["MiddleName"]?.ToString(),
                    Address = row["Address"]?.ToString(),
                    Photo = row["Photo"] as byte[],
                    WorkBookNumber = row["WorkBookNumber"] == DBNull.Value ? "Не указано" : row["WorkBookNumber"].ToString(),
                    BirthDate = row["BirthDate"] == DBNull.Value ? null : (DateTime?)row["BirthDate"]
                });
            }

            FilteredUsers = new ObservableCollection<User>(Users);

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
                    break;

                case "MNG":
                    MenuItems.Add(new MenuItem { Title = "Новый проект", Command = OpenNewProjectCommand });
                    MenuItems.Add(new MenuItem { Title = "Редактировать", Command = OpenEditProjectCommand });
                    MenuItems.Add(new MenuItem { Title = "Удалить", Command = DeleteProjectCommand });
                    MenuItems.Add(new MenuItem { Title = "Запрос на материалы", Command = CreateRequestCommand });
                    MenuItems.Add(new MenuItem { Title = "Создать задачу", Command = CreateTaskCommand });
                    break;

                case "WRK":
                    MenuItems.Add(new MenuItem { Title = "Описание задачи", Command = OpenTaskDescriptionCommand });
                    MenuItems.Add(new MenuItem { Title = "Завершить задачу", Command = CompliteTaskCommand });
                    break;

                case "ADM":
                    MenuItems.Add(new MenuItem { Title = "Редактирование", Command = OpenEditPersonalFileCommand });
                    MenuItems.Add(new MenuItem { Title = "Менеджер", Command = GoToManagerFunctionalityCommand });
                    MenuItems.Add(new MenuItem { Title = "Кладовщик", Command = GoToWarehouseworkerFunctionalityCommand });
                    break;
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

        private void OpenNewProject(Window window)
        {
            try
            {
                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var projectWindow = new ProjectWindow()
                {
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };
                projectWindow.Left = window.Left + (window.Width - projectWindow.Width) / 2;
                projectWindow.Top = window.Top + (window.Height - projectWindow.Height) / 2;

                projectWindow.Closed += (sender, e) => WindowClosed(window);
                projectWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Projects.Clear();
            LoadProjects();
        }

        private void OpenEditProject(Window window)
        {
            try
            {
                var selectedProject = SelectedProject;

                if (selectedProject == null)
                {
                    MessageBox.Show("Пожалуйста, выберите проект для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var projectWindow = new ProjectWindow()
                {
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                var viewModel = projectWindow.DataContext as ProjectWindowViewModel;
                if (viewModel != null)
                {
                    viewModel.IsEditMode = true;
                    viewModel.EditingProjectID = selectedProject.ProjectID;

                    viewModel.ProjectName = selectedProject.ProjectName;
                    viewModel.ProjectDescription = selectedProject.ProjectDescription;
                    viewModel.ProjectStartDate = selectedProject.StartDate;
                    viewModel.ProjectImage = selectedProject.ProjectImage;

                    viewModel.ProjectObjects = new ObservableCollection<ObjectItem>(selectedProject.ProjectObjects);
                    viewModel.AllObjects = viewModel.LoadAllObjects(); // Загрузка доступных объектов
                }

                projectWindow.Left = window.Left + (window.Width - projectWindow.Width) / 2;
                projectWindow.Top = window.Top + (window.Height - projectWindow.Height) / 2;

                projectWindow.Closed += (sender, e) => WindowClosed(window);
                projectWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Projects.Clear();
            LoadProjects();
        }

        private void OpenTaskDescription(Window window)
        {
            try
            {
                var selectedTask = SelectedTask;

                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var taskDescriptionWindow = new TaskDescriptionWindow()
                {
                    DataContext = new TaskDescriptionViewModel
                    {
                        TaskID = SelectedTask.TaskID
                    },
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };


                taskDescriptionWindow.Left = window.Left + (window.Width - taskDescriptionWindow.Width) / 2;
                taskDescriptionWindow.Top = window.Top + (window.Height - taskDescriptionWindow.Height) / 2;

                taskDescriptionWindow.Closed += (sender, e) => WindowClosed(window);
                taskDescriptionWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CompliteTask(Window window)
        {
            try
            {
                var selectedTask = SelectedTask;

                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var taskReportWindow = new TaskCompletionReportWindow()
                {
                    DataContext = new TaskCompletionReportViewModel
                    {
                        TaskID = SelectedTask.TaskID
                    },
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };


                taskReportWindow.Left = window.Left + (window.Width - taskReportWindow.Width) / 2;
                taskReportWindow.Top = window.Top + (window.Height - taskReportWindow.Height) / 2;

                taskReportWindow.Closed += (sender, e) => WindowClosed(window);
                taskReportWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CreateRequest(Window window)
        {
            try
            {
                string userQuery = "SELECT user_id FROM Users WHERE name = @UserName";
                var userParams = new Dictionary<string, object> { { "@UserName", Username } };
                var userResult = DatabaseHelper.ExecuteQuery(userQuery, userParams);
                int userId = Convert.ToInt32(userResult.Rows[0]["user_id"]);

                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var requestWindow = new RequestWindow()
                {
                    DataContext = new RequestViewModel
                    {
                        UserID = userId,
                        ProjectID = SelectedProject.ProjectID,
                        ProjectImage = SelectedProject.ProjectImage,
                        ProjectName = SelectedProject.ProjectName,
                        ProjectDescription = SelectedProject.ProjectDescription

                    },
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                requestWindow.Left = window.Left + (window.Width - requestWindow.Width) / 2;
                requestWindow.Top = window.Top + (window.Height - requestWindow.Height) / 2;

                requestWindow.Closed += (sender, e) => WindowClosed(window);
                requestWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadRequests();
        }
        private void CreateTask(Window window)
        {
            try
            {
                string userQuery = "SELECT user_id FROM Users WHERE name = @UserName";
                var userParams = new Dictionary<string, object> { { "@UserName", Username } };
                var userResult = DatabaseHelper.ExecuteQuery(userQuery, userParams);
                int userId = Convert.ToInt32(userResult.Rows[0]["user_id"]);

                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var taskWindow = new TaskWindow()
                {
                    DataContext = new TaskViewModel
                    {
                        CreatorID = userId,
                        ProjectID = SelectedProject.ProjectID,
                        ProjectImage = SelectedProject.ProjectImage,
                        ProjectName = SelectedProject.ProjectName,

                    },
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                taskWindow.Left = window.Left + (window.Width - taskWindow.Width) / 2;
                taskWindow.Top = window.Top + (window.Height - taskWindow.Height) / 2;

                taskWindow.Closed += (sender, e) => WindowClosed(window);
                taskWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //LoadWorkerTasks();
        }
        private void DeleteProject(Window window)
        {
            try
            {
                var selectedProject = SelectedProject;

                if (selectedProject == null)
                {
                    MessageBox.Show("Пожалуйста, выберите проект для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить проект \"{selectedProject.ProjectName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Удаление связей проекта с объектами
                    string deleteProjectObjectsQuery = "DELETE FROM ProjectObject WHERE project_id = @project_id";
                    DatabaseHelper.ExecuteNonQuery(deleteProjectObjectsQuery, new Dictionary<string, object>
                    {
                        { "@project_id", selectedProject.ProjectID }
                    });

                    // Удаление самого проекта
                    string deleteProjectQuery = "DELETE FROM Project WHERE project_id = @project_id";
                    DatabaseHelper.ExecuteNonQuery(deleteProjectQuery, new Dictionary<string, object>
                    {
                        { "@project_id", selectedProject.ProjectID }
                    });

                    MessageBox.Show("Проект успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);

                    Projects.Clear();
                    LoadProjects();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanEditProject(object param)
        { 
            return SelectedProject != null;
        }

        private void OpenPersonalFile(Window window, string username)
        {
            try
            {
                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                string query = "SELECT user_id FROM Users WHERE name = @Username";
                var parameters = new Dictionary<string, object> { { "@Username", username } };
                var result = DatabaseHelper.ExecuteQuery(query, parameters);

                if (result.Rows.Count == 0)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int userId = Convert.ToInt32(result.Rows[0]["user_id"]);

                var personalfileWindow = new PersonalFileWindow(userId, username)
                {
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                };
                personalfileWindow.Left = window.Left + (window.Width - personalfileWindow.Width) / 2;
                personalfileWindow.Top = window.Top + (window.Height - personalfileWindow.Height) / 2;

                personalfileWindow.Closed += (sender, e) => WindowClosed(window);
                personalfileWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadUsers();
        }

        private void OpenEditPersonalFile(Window window)
        {
            try
            {
                string username = SelectedUser.Username;
                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                string query = "SELECT user_id FROM Users WHERE name = @Username";
                var parameters = new Dictionary<string, object> { { "@Username", username } };
                var result = DatabaseHelper.ExecuteQuery(query, parameters);

                if (result.Rows.Count == 0)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int userId = Convert.ToInt32(result.Rows[0]["user_id"]);

                var personalfileWindow = new PersonalFileWindow(userId, username)
                {
                    DataContext = new PersonalFileViewModel
                    {
                        UserID = SelectedUser.UserID,
                        LastName = SelectedUser.LastName,
                        FirstName = SelectedUser.FirstName,
                        MiddleName = SelectedUser.MiddleName,
                        Address = SelectedUser.Address,
                        WorkBookNumber = SelectedUser.WorkBookNumber,
                        BirthDate = SelectedUser.BirthDate,
                        Photo = SelectedUser.Photo,
                        Role = SelectedUser.Role,

                        IsRoleEditable = true
                    },
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                };
                personalfileWindow.Left = window.Left + (window.Width - personalfileWindow.Width) / 2;
                personalfileWindow.Top = window.Top + (window.Height - personalfileWindow.Height) / 2;

                personalfileWindow.Closed += (sender, e) => WindowClosed(window);
                personalfileWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadUsers();
        }
        private void ApproveRequest(Window window)
        {
            try
            {
                if (SelectedRequest == null) return;

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

        private bool CanEditUser(object param)
        {
            return SelectedUser != null;
        }

        private void GoToManagerFunctionality(Window window)
        {
            var searchTitle = window.FindName("SearchTitle") as UIElement;
            var searchDescription = window.FindName("SearchDescription") as UIElement;
            var managerListBox = window.FindName("ProjectsListBox") as UIElement;
            var adminListBox = window.FindName("UsersListBox") as UIElement;
            adminListBox.Visibility = Visibility.Collapsed;
            managerListBox.Visibility = Visibility.Visible;
            UserRole = "MNG";
            MenuItems.Clear();
            MenuItems.Add(new MenuItem { Title = "Новый проект", Command = OpenNewProjectCommand });
            MenuItems.Add(new MenuItem { Title = "Редактировать", Command = OpenEditProjectCommand });
            MenuItems.Add(new MenuItem { Title = "Удалить", Command = DeleteProjectCommand });
            MenuItems.Add(new MenuItem { Title = "Запрос на материалы", Command = CreateRequestCommand });
            MenuItems.Add(new MenuItem { Title = "Создать задачу", Command =  CreateTaskCommand});
            MenuItems.Add(new MenuItem { Title = "Вернутся", Command = GoToAdminFunctionalityCommand });
        }

        private void GoToWarehouseworkerFunctionality(Window window)
        {
            var managerListBox = window.FindName("RequestsListBox") as UIElement;
            var adminListBox = window.FindName("UsersListBox") as UIElement;
            adminListBox.Visibility = Visibility.Collapsed;
            managerListBox.Visibility = Visibility.Visible;
            UserRole = "WHW";
            MenuItems.Clear();
            MenuItems.Add(new MenuItem { Title = "Каталог", Command = OpenCatalogCommand });
            MenuItems.Add(new MenuItem { Title = "Утвердить запрос", Command = ApproveRequestCommand });
            MenuItems.Add(new MenuItem { Title = "Отклонить запрос", Command = DenyRequestCommand });
            MenuItems.Add(new MenuItem { Title = "Вернутся", Command = GoToAdminFunctionalityCommand });
        }

        private void GoToAdminFunctionality(Window window)
        {
            var warehouseworkerListBox = window.FindName("RequestsListBox") as UIElement;
            var managerListBox = window.FindName("ProjectsListBox") as UIElement;
            var adminListBox = window.FindName("UsersListBox") as UIElement;
            warehouseworkerListBox.Visibility = Visibility.Collapsed;
            adminListBox.Visibility = Visibility.Visible;
            managerListBox.Visibility = Visibility.Collapsed;
            UserRole = "ADM";
            LoadMenuItems();
        }
        private void FilterElemets()
        {

            switch (UserRole)
            { 

                case "MNG":
                    if (string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        FilteredProjects = new ObservableCollection<Project>(Projects);
                        return;
                    }

                    var lowerQueryM = SearchQuery.ToLower().Trim(' ');
                    FilteredProjects = new ObservableCollection<Project>(
                        Projects.Where(p => 
                        p.ProjectID.ToString().Contains(lowerQueryM) ||
                        p.ProjectName.ToLower().Contains(lowerQueryM)
                        )
                    );
                    break;

                case "WHW":
                    if (string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        FilteredRequests = new ObservableCollection<MaterialRequest>(Requests);
                        return;
                    }

                    var lowerQueryW = SearchQuery.ToLower().Trim(' ');

                    FilteredRequests = new ObservableCollection<MaterialRequest>(
                        Requests.Where(r =>
                        r.RequestID.ToString().Contains(lowerQueryW) ||
                        r.MaterialsSummary.ToLower().Contains(lowerQueryW)
                        )
                    );
                    break;
                case "ADM":

                    if (string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        FilteredUsers = new ObservableCollection<User>(Users);
                        return;
                    }

                    var lowerQueryA = SearchQuery.ToLower().Trim(' ');
                    FilteredUsers = new ObservableCollection<User>(
                        Users.Where(m =>
                            m.FullName.ToLower().Contains(lowerQueryA) ||
                            m.UserID.ToString().Contains(lowerQueryA) ||
                            m.Username.ToLower().Contains(lowerQueryA)
                        )
                    );
                    break;
            }
        }
        private void ResetSearch()
        {
            SearchQuery = string.Empty;
            FilterElemets();
        }
        private bool CanOpenDescription(object param)
        {
            return SelectedTask != null;
        }
    }
}