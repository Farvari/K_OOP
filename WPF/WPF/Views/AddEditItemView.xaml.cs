using System.Windows;
using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class AddEditItemView : Window
    {
        public AddEditItemView(Item item, User currentUser, bool isAdmin = false)
        {
            InitializeComponent();
            var viewModel = new AddEditItemViewModel(item, currentUser, isAdmin);
            DataContext = viewModel;
            Title = item == null ? "Добавить объявление" : "Редактировать объявление";
            
            viewModel.SaveCompleted += (s, result) => DialogResult = result;
        }
    }
}

