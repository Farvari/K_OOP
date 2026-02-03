using System.Windows;
using System.Windows.Controls;
using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class ProfileView : Window
    {
        public ProfileView(User user)
        {
            InitializeComponent();
            var viewModel = new ProfileViewModel(user);
            DataContext = viewModel;
            
            viewModel.SaveCompleted += (s, result) => DialogResult = result;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}

