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
using System.Windows.Media.Imaging;

namespace Build_BuildersIS.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _username;
        private string _userRole;


        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<WorkerTask> WorkerTasks { get; set; }

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

        public BitmapImage ConvertToImageSource(byte[] imageData)
        {
            if (imageData == null) return null;

            using (MemoryStream ms = new MemoryStream(imageData))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }
    }
}