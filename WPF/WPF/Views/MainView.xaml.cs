using System.Windows;
using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class MainView : Window
    {
        public MainView(User user)
        {
            InitializeComponent();
            DataContext = new MainViewModel(user);
            
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                viewModel.LogoutRequested += ViewModel_LogoutRequested;
            }
        }

        private void ViewModel_LogoutRequested(object sender, System.EventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();
            this.Close();
        }

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel?.SelectedItem != null)
            {
                var itemDetailsView = new ItemDetailsView(viewModel.SelectedItem, viewModel.CurrentUser);
                itemDetailsView.ShowDialog();
            }
        }

        private void ItemCard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var border = sender as System.Windows.Controls.Border;
                if (border?.DataContext is Models.Item item)
                {
                    var viewModel = DataContext as MainViewModel;
                    if (viewModel != null)
                    {
                        var itemDetailsView = new ItemDetailsView(item, viewModel.CurrentUser);
                        itemDetailsView.ShowDialog();
                    }
                }
            }
        }
    }
}

