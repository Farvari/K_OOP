using System.Windows;
using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class RegisterView : Window
    {
        public RegisterView()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel();
            
            var viewModel = DataContext as RegisterViewModel;
            if (viewModel != null)
            {
                viewModel.RegistrationSuccessful += ViewModel_RegistrationSuccessful;
                viewModel.CancelRequested += ViewModel_CancelRequested;
            }
        }

        private void ViewModel_RegistrationSuccessful(object sender, Models.User user)
        {
            this.Close();
        }

        private void ViewModel_CancelRequested(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}

