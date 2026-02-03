using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class ReportsManagementView : System.Windows.Window
    {
        private ReportsManagementViewModel _viewModel;

        public ReportsManagementView(User currentUser)
        {
            InitializeComponent();
            _viewModel = new ReportsManagementViewModel(currentUser);
            DataContext = _viewModel;

            _viewModel.EditItemRequested += OnEditItemRequested;
        }

        private void OnEditItemRequested(object sender, Models.Item item)
        {
            try
            {
                var addEditView = new AddEditItemView(item, _viewModel.CurrentUser, true);
                var result = addEditView.ShowDialog();
                if (result == true)
                {
                    _viewModel.OnItemSaved();
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при открытии окна редактирования: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

