using Build_BuildersIS.ViewModels;
using System;
using System.Collections.Generic;
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

namespace Build_BuildersIS.Views
{
    /// <summary>
    /// Логика взаимодействия для MaterialWindow.xaml
    /// </summary>
    public partial class MaterialWindow : Window
    {
        public MaterialWindow()
        {
            InitializeComponent();
        }

        // Обработка события PreviewDragOver для изменения курсора
        private void ImagePreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        // Обработка события Drop для загрузки изображения
        private void ImageDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    var viewModel = DataContext as MaterialViewModel;
                    viewModel?.HandleImageDrop(filePath);
                }
            }
        }
    }
}
