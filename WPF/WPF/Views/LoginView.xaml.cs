using System.Windows;
using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
            
            var viewModel = DataContext as LoginViewModel;
            if (viewModel != null)
            {
                viewModel.LoginSuccessful += ViewModel_LoginSuccessful;
            }
        }

        private void ViewModel_LoginSuccessful(object sender, Models.User user)
        {
            var mainView = new MainView(user);
            mainView.Show();
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}

