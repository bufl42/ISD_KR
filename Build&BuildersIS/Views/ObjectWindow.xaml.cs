using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Build_BuildersIS.ViewModels;

namespace Build_BuildersIS.Views
{
    /// <summary>
    /// Логика взаимодействия для ObjectWindow.xaml
    /// </summary>
    public partial class ObjectWindow : Window
    {
        public ObjectWindow()
        {
            InitializeComponent();
        }

        private void ImageDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];

                    try
                    {
                        // Чтение изображения как байтового массива
                        byte[] imageBytes = File.ReadAllBytes(filePath);

                        // Устанавливаем изображение в привязанное свойство ViewModel
                        if (DataContext is ObjectViewModel viewModel)
                        {
                            viewModel.ImagePreview = imageBytes;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ImagePreviewDragOver(object sender, DragEventArgs e)
        {
            // Разрешаем Drag and Drop только для файлов
            e.Handled = true;
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }
    }
}
