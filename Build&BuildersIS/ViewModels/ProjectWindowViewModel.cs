using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Build_BuildersIS.DataBase;
using System.Windows.Input;
using System.Windows;
using Build_BuildersIS.Models;
using System.IO;
using Build_BuildersIS.Views;
using System.Xml.Linq;

namespace Build_BuildersIS.ViewModels
{
    public class ProjectWindowViewModel : BaseViewModel
    {
        private ObservableCollection<ObjectItem> _allObjects;
        private ObservableCollection<ObjectItem> _projectObjects;
        private ObjectItem _selectedObject;
        private ObjectItem _selectedProjectObject;
        private string _projectName;
        private string _projectDescription;
        private DateTime? _projectStartDate;
        private byte[] _projectImage;
        public bool IsEditMode { get; set; }
        public int? EditingProjectID { get; set; }

        public ObservableCollection<ObjectItem> AllObjects
        {
            get => _allObjects;
            set { _allObjects = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ObjectItem> ProjectObjects
        {
            get => _projectObjects;
            set { _projectObjects = value; OnPropertyChanged(); }
        }

        public ObjectItem SelectedObject
        {
            get => _selectedObject;
            set { _selectedObject = value; OnPropertyChanged(); }
        }

        public ObjectItem SelectedProjectObject
        {
            get => _selectedProjectObject;
            set { _selectedProjectObject = value; OnPropertyChanged(); }
        }

        public string ProjectName
        {
            get => _projectName;
            set { _projectName = value; OnPropertyChanged(); }
        }

        public string ProjectDescription
        {
            get => _projectDescription;
            set { _projectDescription = value; OnPropertyChanged(); }
        }

        public DateTime? ProjectStartDate
        {
            get => _projectStartDate;
            set { _projectStartDate = value; OnPropertyChanged(); }
        }

        public byte[] ProjectImage
        {
            get => _projectImage;
            set { _projectImage = value; OnPropertyChanged(); }
        }

        public ICommand AddObjectToProjectCommand => new RelayCommand(_ => AddObjectToProject(), _ => SelectedObject != null);
        public ICommand RemoveObjectFromProjectCommand => new RelayCommand(_ => RemoveObjectFromProject(), _ => SelectedProjectObject != null);
        public ICommand SaveProjectCommand => new RelayCommand(_ => SaveProject(), _ => CanSaveProject());
        public ICommand AddObjectCommand => new RelayCommand(param => OpenNewObject(param as Window));
        public ICommand EditObjectCommand => new RelayCommand(param => OpenEditObject(param as Window), _ => SelectedObject != null);
        public ICommand DeleteObjectCommand => new RelayCommand(param => DeleteObject(param as Window), _ => SelectedObject != null);

        public ProjectWindowViewModel()
        {
            AllObjects = LoadAllObjects();
            ProjectObjects = new ObservableCollection<ObjectItem>();
            ProjectStartDate = DateTime.Now;
        }

        public ObservableCollection<ObjectItem> LoadAllObjects()
        {
            var objects = new ObservableCollection<ObjectItem>();

            string query = @"
                SELECT o.object_id, o.name, o.description, o.imagedata 
                FROM Object o
                LEFT JOIN ProjectObject po ON o.object_id = po.object_id AND po.project_id = @project_id
                WHERE po.project_id IS NULL";

            var data = DatabaseHelper.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@project_id", EditingProjectID ?? -1 } // Если проект не редактируется, выводим все объекты
            });

            foreach (DataRow row in data.Rows)
            {
                objects.Add(new ObjectItem
                {
                    ObjectID = Convert.ToInt32(row["object_id"]),
                    Name = row["name"].ToString(),
                    Description = row["description"].ToString(),
                    ImageData = row["imagedata"] as byte[]
                });
            }

            return objects;
        }

        private void AddObjectToProject()
        {
            if (SelectedObject == null) return;
            ProjectObjects.Add(SelectedObject);
            AllObjects.Remove(SelectedObject);
        }

        private void RemoveObjectFromProject()
        {
            if (SelectedProjectObject == null) return;
            AllObjects.Add(SelectedProjectObject);
            ProjectObjects.Remove(SelectedProjectObject);
        }

        public void HandleImageDrop(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;
                ProjectImage = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveProject()
        {
            byte[] imageData = ProjectImage ?? Array.Empty<byte>();

            if (IsEditMode && EditingProjectID.HasValue)
            {
                // Обновление существующего проекта
                string updateProjectQuery = "UPDATE Project SET name = @name, description = @description, start_date = @start_date, imagedata = @image WHERE project_id = @project_id";
                DatabaseHelper.ExecuteNonQuery(updateProjectQuery, new Dictionary<string, object>
                {
                    { "@name", ProjectName },
                    { "@description", ProjectDescription },
                    { "@start_date", ProjectStartDate },
                    { "@image", imageData },
                    { "@project_id", EditingProjectID.Value }
                });

                // Очистка старых связей объекта с проектом
                string deleteProjectObjectsQuery = "DELETE FROM ProjectObject WHERE project_id = @project_id";
                DatabaseHelper.ExecuteNonQuery(deleteProjectObjectsQuery, new Dictionary<string, object>
                {
                    { "@project_id", EditingProjectID.Value }
                });

                // Добавление новых связей
                foreach (var obj in ProjectObjects)
                {
                    string insertProjectObjectQuery = "INSERT INTO ProjectObject (project_id, object_id) VALUES (@project_id, @object_id)";
                    DatabaseHelper.ExecuteNonQuery(insertProjectObjectQuery, new Dictionary<string, object>
                    {
                        { "@project_id", EditingProjectID.Value },
                        { "@object_id", obj.ObjectID }
                    });
                }

                MessageBox.Show("Проект успешно обновлён!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Создание нового проекта
                string insertProjectQuery = "INSERT INTO Project (name, description, start_date, imagedata) OUTPUT INSERTED.project_id VALUES (@name, @description, @start_date, @image)";
                var projectId = DatabaseHelper.ExecuteScalar(insertProjectQuery, new Dictionary<string, object>
                {
                    { "@name", ProjectName },
                    { "@description", ProjectDescription },
                    { "@start_date", ProjectStartDate },
                    { "@image", imageData }
                });

                if (projectId != null)
                {
                    foreach (var obj in ProjectObjects)
                    {
                        string insertProjectObjectQuery = "INSERT INTO ProjectObject (project_id, object_id) VALUES (@project_id, @object_id)";
                        DatabaseHelper.ExecuteNonQuery(insertProjectObjectQuery, new Dictionary<string, object>
                {
                    { "@project_id", projectId },
                    { "@object_id", obj.ObjectID }
                });
                    }

                    MessageBox.Show("Проект успешно сохранён!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при сохранении проекта. Проверьте данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanSaveProject()
        {
            return !string.IsNullOrWhiteSpace(ProjectName) &&
                   !string.IsNullOrWhiteSpace(ProjectDescription) &&
                   ProjectStartDate != null &&
                   ProjectObjects.Count > 0;
        }


        private void OpenNewObject(Window window)
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
                ojectWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            AllObjects.Clear();
            AllObjects = LoadAllObjects();
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
            AllObjects.Clear();
            AllObjects = LoadAllObjects();
        }

        private void DeleteObject(Window window)
        {
            try
            {
                var selectedObject = SelectedObject;

                if (selectedObject == null)
                {
                    MessageBox.Show("Пожалуйста, выберите объект для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить объект \"{selectedObject.Name}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Удаление связей объекта с проектами
                    string deleteObjectProjectsQuery = "DELETE FROM ProjectObject WHERE object_id = @object_id";
                    DatabaseHelper.ExecuteNonQuery(deleteObjectProjectsQuery, new Dictionary<string, object>
                    {
                        { "@object_id", selectedObject.ObjectID }
                    });

                    // Удаление самого объекта
                    string deleteObjectQuery = "DELETE FROM Object WHERE object_id = @object_id";
                    DatabaseHelper.ExecuteNonQuery(deleteObjectQuery, new Dictionary<string, object>
                    {
                        { "@object_id", selectedObject.ObjectID }
                    });
                    AllObjects.Clear();
                    AllObjects = LoadAllObjects();
                    MessageBox.Show("Объект успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении объекта: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
