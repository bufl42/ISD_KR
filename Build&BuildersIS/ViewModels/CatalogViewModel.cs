using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
using Build_BuildersIS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Build_BuildersIS.ViewModels
{
    public class CatalogViewModel : BaseViewModel
    {
        private ObservableCollection<MaterialItem> _materials;
        private ObservableCollection<MaterialItem> _filteredMaterials;
        private string _searchQuery;
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

        public ObservableCollection<MaterialItem> Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                OnPropertyChanged(nameof(Materials));
            }
        }

        public ObservableCollection<MaterialItem> FilteredMaterials
        {
            get => _filteredMaterials;
            set
            {
                _filteredMaterials = value;
                OnPropertyChanged(nameof(FilteredMaterials));
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                FilterMaterials();
            }
        }

        public ICommand SearchCommand => new RelayCommand(param => FilterMaterials());
        public ICommand ResetCommand => new RelayCommand(param => ResetSearch());
        public ICommand AddMaterialCommand => new RelayCommand(param => AddMaterial(param as Window));
        public ICommand EditMaterialCommand => new RelayCommand(param => EditMaterial(param as Window),CanEditOrDelete);
        public ICommand DeleteMaterialCommand => new RelayCommand(param => DeleteMaterial(), CanEditOrDelete);


        public CatalogViewModel()
        {
            Materials = new ObservableCollection<MaterialItem>();
            FilteredMaterials = new ObservableCollection<MaterialItem>();

            LoadMaterials();
        }

        private void LoadMaterials()
        {
            string query = "SELECT material_id, name, quantity, unit, imagedata FROM Material";
            DataTable materialData = DatabaseHelper.ExecuteQuery(query);

            Materials.Clear();
            foreach (DataRow row in materialData.Rows)
            {
                Materials.Add(new MaterialItem
                {
                    MaterialID = Convert.ToInt32(row["material_id"]),
                    Name = row["name"].ToString(),
                    Quantity = Convert.ToDouble(row["quantity"]),
                    Unit = row["unit"].ToString(),
                    ImageData = row["imagedata"] as byte[]
                });
            }

            FilteredMaterials = new ObservableCollection<MaterialItem>(Materials);
        }

        private void FilterMaterials()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredMaterials = new ObservableCollection<MaterialItem>(Materials);
                return;
            }

            var lowerQuery = SearchQuery.ToLower();
            FilteredMaterials = new ObservableCollection<MaterialItem>(
                Materials.Where(m =>
                    m.Name.ToLower().Contains(lowerQuery) ||
                    m.MaterialID.ToString().Contains(lowerQuery)
                )
            );
        }

        private void ResetSearch()
        {
            SearchQuery = string.Empty; 
            FilterMaterials();
        }

        private void AddMaterial(Window window)
        {
            try
            {
                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }

                var materialWindow = new MaterialWindow
                {
                    Owner = window,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };
                materialWindow.Left = window.Left + (window.Width - materialWindow.Width) / 2;
                materialWindow.Top = window.Top + (window.Height - materialWindow.Height) / 2;

                materialWindow.Closed += (sender, e) => WindowClosed(window);
                materialWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadMaterials();
            FilterMaterials();
        }
        private void EditMaterial(Window window)
        {
            try
            {
                if (SelectedMaterial == null) return;

                var materialWindow = new MaterialWindow
                {
                    DataContext = new MaterialViewModel
                    {
                        Name = SelectedMaterial.Name,
                        Quantity = (int)SelectedMaterial.Quantity,
                        Unit = SelectedMaterial.Unit,
                        ImageData = SelectedMaterial.ImageData,
                        MaterialID = SelectedMaterial.MaterialID
                    }
                };
                var overlay = window.FindName("Overlay") as UIElement;
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }
                materialWindow.Left = window.Left + (window.Width - materialWindow.Width) / 2;
                materialWindow.Top = window.Top + (window.Height - materialWindow.Height) / 2;

                materialWindow.Closed += (sender, e) => WindowClosed(window);
                materialWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadMaterials();
            FilterMaterials();
        }

        private void DeleteMaterial()
        {
            if (SelectedMaterial == null) return;
            // Удаляем материал из базы данных
            string query = "DELETE FROM Material WHERE material_id = @MaterialID";
            var parameters = new Dictionary<string, object>
            {
                { "@MaterialID", SelectedMaterial.MaterialID }
            };

            DatabaseHelper.ExecuteNonQuery(query, parameters);
            LoadMaterials();
            FilterMaterials();
        }

        private bool CanEditOrDelete(object param)
        {
            return SelectedMaterial != null;
        }
    }
}
