using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
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

namespace Build_BuildersIS
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string username, string userRole)
        {
            InitializeComponent();
            DataContext = new MainViewModel(username, userRole);
            string imagePath;
            switch (userRole)
            {
                case "ADM":
                    imagePath = "pack://application:,,,/Resources/account-star.png";
                    ProjectsListBox.Visibility = Visibility.Collapsed;
                    TasksListBox.Visibility = Visibility.Collapsed;
                    UsersListBox.Visibility = Visibility.Visible;
                    RequestsListBox.Visibility = Visibility.Collapsed;
                    break;
                case "WHW":
                    imagePath = "pack://application:,,,/Resources/warehouse-worker.png";
                    ProjectsListBox.Visibility = Visibility.Collapsed;
                    TasksListBox.Visibility = Visibility.Collapsed;
                    UsersListBox.Visibility = Visibility.Collapsed;
                    RequestsListBox.Visibility = Visibility.Visible;
                    break;
                case "MNG":
                    imagePath = "pack://application:,,,/Resources/account-tie.png";
                    ProjectsListBox.Visibility = Visibility.Visible;
                    TasksListBox.Visibility = Visibility.Collapsed;
                    RequestsListBox.Visibility= Visibility.Collapsed;
                    UsersListBox.Visibility = Visibility.Collapsed;
                    break;
                default:
                    imagePath = "pack://application:,,,/Resources/worker.png";
                    ProjectsListBox.Visibility = Visibility.Collapsed;
                    TasksListBox.Visibility = Visibility.Visible;
                    RequestsListBox.Visibility = Visibility.Collapsed;
                    UsersListBox.Visibility = Visibility.Collapsed;
                    MainSearch.Visibility = Visibility.Collapsed;
                    break;
            }
            BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
            UserIcon.Source = bitmap;
        }



        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                UserRoleLabel.Content = RoleDescription.GetRoleDescription(viewModel.UserRole);
            }
        }
    }
}
