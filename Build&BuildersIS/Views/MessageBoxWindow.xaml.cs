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

namespace LibraryIS
{
    /// <summary>
    /// Логика взаимодействия для MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow(string text, bool action)
        {
            InitializeComponent();
            MessageText.Text = text;

            string imagePath = action
                ? "pack://application:,,,/Resources/check.png"
                : "pack://application:,,,/Resources/cancel.png";

            BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
            Image.Source = bitmap;

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
