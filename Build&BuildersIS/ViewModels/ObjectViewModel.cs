using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
using System.Windows.Input;
using System.Windows;
using System.IO;

namespace Build_BuildersIS.ViewModels
{
    public class ObjectViewModel : BaseViewModel
    {
        private int? _objectId; // Используется только в режиме редактирования
        private string _name;
        private string _description;
        private byte[] _imagePreview;

        // Свойства для привязки к интерфейсу
        public int? ObjectID
        {
            get => _objectId;
            set => SetProperty(ref _objectId, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public byte[] ImagePreview
        {
            get => _imagePreview;
            set => SetProperty(ref _imagePreview, value);
        }

        public ICommand SaveCommand => new RelayCommand(param => SaveObject(param as Window));

        public ObjectViewModel()
        {
        }

        // Метод сохранения объекта
        private void SaveObject(Window window)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    MessageBox.Show("Название объекта не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ObjectID.HasValue) // Режим редактирования
                {
                    string updateQuery = "UPDATE Object SET name = @name, description = @description, imagedata = @image WHERE object_id = @object_id";
                    DatabaseHelper.ExecuteNonQuery(updateQuery, new Dictionary<string, object>
                    {
                        { "@name", Name },
                        { "@description", Description },
                        { "@image", ImagePreview ?? Array.Empty<byte>() },
                        { "@object_id", ObjectID }
                    });

                    MessageBox.Show("Объект успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // Режим добавления
                {
                    string insertQuery = "INSERT INTO Object (name, description, imagedata) VALUES (@name, @description, @image)";
                    DatabaseHelper.ExecuteNonQuery(insertQuery, new Dictionary<string, object>
                    {
                        { "@name", Name },
                        { "@description", Description },
                        { "@image", ImagePreview ?? Array.Empty<byte>() }
                    });

                    MessageBox.Show("Объект успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Инициализация модели в режиме редактирования
        public void InitializeForEdit(ObjectItem objectItem)
        {
            if (objectItem != null)
            {
                ObjectID = objectItem.ObjectID;
                Name = objectItem.Name;
                Description = objectItem.Description;
                ImagePreview = objectItem.ImageData;
            }
        }

        public void HandleImageDrop(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;
                ImagePreview = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
