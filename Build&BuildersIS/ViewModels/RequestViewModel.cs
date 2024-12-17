using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;

namespace Build_BuildersIS.ViewModels
{
    public class RequestViewModel : BaseViewModel
    {
        private int _userID;
        private int _projectID;
        private string _projectName;
        private string _projectDescription;
        private byte[] _projectImage;
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

        public byte[] ProjectImage
        {
            get => _projectImage;
            set { _projectImage = value; OnPropertyChanged(); }
        }
        public ObservableCollection<MaterialItem> AllMaterials { get; set; }
        public ObservableCollection<MaterialItem> ProjectMaterials { get; set; }

        private MaterialItem _selectedMaterial;
        public MaterialItem SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                _selectedMaterial = value;
                OnPropertyChanged();
            }
        }

        private MaterialItem _selectedProjectMaterial;
        public MaterialItem SelectedProjectMaterial
        {
            get => _selectedProjectMaterial;
            set
            {
                _selectedProjectMaterial = value;
                OnPropertyChanged();
            }
        }

        private string _quantity;
        public string Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        // Команды
        public ICommand AddToRequestCommand => new RelayCommand(_ => AddToRequest(), _ => CanAddToRequest());
        public ICommand RemoveMaterialFromRequestCommand => new RelayCommand(_ => RemoveFromRequest(), _ => CanRemoveFromRequest());
        public ICommand CreateRequestCommand => new RelayCommand(_ => CreateRequest(), _ => ProjectMaterials.Any());
        public RequestViewModel()
        {
            AllMaterials = new ObservableCollection<MaterialItem>();
            ProjectMaterials = new ObservableCollection<MaterialItem>();

            LoadMaterials();
        }

        private void LoadMaterials()
        {
            // Загружаем материалы из базы данных
            string query = "SELECT material_id, name, quantity, unit, imagedata FROM Material";
            var materialData = DatabaseHelper.ExecuteQuery(query);

            AllMaterials.Clear();
            foreach (DataRow row in materialData.Rows)
            {
                AllMaterials.Add(new MaterialItem
                {
                    MaterialID = Convert.ToInt32(row["material_id"]),
                    Name = row["name"].ToString(),
                    Quantity = Convert.ToDouble(row["quantity"]),
                    Unit = row["unit"].ToString(),
                    ImageData = row["imagedata"] as byte[]
                });
            }
        }

        private bool CanAddToRequest()
        {
            return SelectedMaterial != null && double.TryParse(Quantity, out double qty) && qty > 0;
        }

        private void AddToRequest()
        {
            if (SelectedMaterial == null || !double.TryParse(Quantity, out double qty) || qty <= 0) return;

            var existingMaterial = ProjectMaterials.FirstOrDefault(m => m.MaterialID == SelectedMaterial.MaterialID);
            if (existingMaterial != null)
            {
                existingMaterial.Quantity += qty;
            }
            else
            {
                ProjectMaterials.Add(new MaterialItem
                {
                    MaterialID = SelectedMaterial.MaterialID,
                    Name = SelectedMaterial.Name,
                    Quantity = qty,
                    Unit = SelectedMaterial.Unit,
                    ImageData = SelectedMaterial.ImageData
                });
            }
            OnPropertyChanged(nameof(ProjectMaterials));
        }

        private bool CanRemoveFromRequest()
        {
            return SelectedProjectMaterial != null;
        }

        private void RemoveFromRequest()
        {
            if (SelectedProjectMaterial != null)
            {
                ProjectMaterials.Remove(SelectedProjectMaterial);
                OnPropertyChanged(nameof(ProjectMaterials));
            }
        }

        private void CreateRequest()
        {
            if (!ProjectMaterials.Any())
            {
                MessageBox.Show("Запрос не может быть создан без материалов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Создание запроса
                string insertRequestQuery = "INSERT INTO Request (project_id, status, request_date, user_id) OUTPUT INSERTED.request_id VALUES (@projectId, 'PRO', @requestDate, @userId)";
                var parameters = new Dictionary<string, object>
                {
                    { "@projectId", ProjectID},
                    { "@requestDate", DateTime.Now },
                    { "@userId", UserID}
                };

                int requestId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertRequestQuery, parameters));

                foreach (var material in ProjectMaterials)
                {
                    string insertMaterialQuery = "INSERT INTO RequestMaterial (request_id, material_id, quantity) VALUES (@requestId, @materialId, @quantity)";
                    var materialParameters = new Dictionary<string, object>
                    {
                        { "@requestId", requestId },
                        { "@materialId", material.MaterialID },
                        { "@quantity", material.Quantity }
                    };

                    DatabaseHelper.ExecuteNonQuery(insertMaterialQuery, materialParameters);
                }

                MessageBox.Show("Запрос успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании запроса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}