using Build_BuildersIS.DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Build_BuildersIS.ViewModels
{
    public class MaterialViewModel : BaseViewModel
    {
        private string _name;
        private int _quantity;
        private string _unit;
        private byte[] _imageData;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        public byte[] ImageData
        {
            get => _imageData;
            set
            {
                _imageData = value;
                OnPropertyChanged(nameof(ImageData));
                OnPropertyChanged(nameof(ImagePreview));
            }
        }

        public byte[] ImagePreview => ImageData;

        public ICommand AddMaterialCommand => new RelayCommand(param => AddMaterial(), CanAddMaterial);

        public MaterialViewModel()
        {

        }
        private bool CanAddMaterial(object param)
        {
            if (!string.IsNullOrWhiteSpace(Name) && Quantity > 0 && !string.IsNullOrWhiteSpace(Unit) && ImageData != null)
            {
                return true;
            }
            else { return false; }
        }

        private void AddMaterial()
        {
            string query = @"
            INSERT INTO Material (name, quantity, unit, imagedata)
            VALUES (@Name, @Quantity, @Unit, @ImageData)";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", Name },
                { "@Quantity", Quantity },
                { "@Unit", Unit },
                { "@ImageData", ImageData }
            };

            DatabaseHelper.ExecuteNonQuery(query, parameters);
        }

        // Метод для обработки Drag and Drop изображения
        public void HandleImageDrop(string filePath)
        {
            try
            {
                ImageData = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке изображения: " + ex.Message);
            }
        }
    }
}
