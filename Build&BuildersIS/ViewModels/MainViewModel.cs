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
        public MaterialRequest SelectedRequest
        {
            get => _selectedRequest;
            set
            {
                _selectedRequest = value;
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

            FilteredProjects = new ObservableCollection<Project>(Projects);
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

                //case "Менеджер":
                //    MenuItems.Add(new MenuItem { Title = "Создать новый проект", Command = CreateProjectCommand });
                //    MenuItems.Add(new MenuItem { Title = "Запрос на материалы", Command = RequestMaterialsCommand });
                //    // Добавьте другие кнопки для Менеджера при необходимости
                //    break;

                //case "Рабочий":
                //    MenuItems.Add(new MenuItem { Title = "Просмотр задач", Command = ViewTasksCommand });
                //    // Добавьте другие кнопки для Рабочего при необходимости
                //    break;

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
                        Photo = SelectedUser.Photo
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
            MenuItems.Add(new MenuItem { Title = "Создать новый проект", Command = OpenCatalogCommand });
            MenuItems.Add(new MenuItem { Title = "Запрос на материалы", Command = OpenCatalogCommand });
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
                        r.ObjectAddress.ToLower().Contains(lowerQueryW) ||
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
    }
}