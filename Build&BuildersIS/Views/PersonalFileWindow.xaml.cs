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
using Build_BuildersIS.ViewModels;

namespace Build_BuildersIS.Views
{
    /// <summary>
    /// Логика взаимодействия для PersonalFileWindow.xaml
    /// </summary>
    public partial class PersonalFileWindow : Window
    {
        public PersonalFileWindow(int userId)
        {
            InitializeComponent();
            (DataContext as PersonalFileViewModel).Initialize(userId);
        }

        private void ImageDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    if (DataContext is PersonalFileViewModel viewModel)
                    {
                        viewModel.HandleImageDrop(filePath);
                    }
                }
            }
        }

        private void ImagePreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }
    }
}
