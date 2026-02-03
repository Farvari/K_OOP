using System.Windows;
using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class RateSellerView : Window
    {
        public RateSellerView(User seller, User currentUser)
        {
            InitializeComponent();
            var viewModel = new RateSellerViewModel(seller, currentUser);
            DataContext = viewModel;
            
            viewModel.SaveCompleted += (s, result) => DialogResult = result;
        }
    }
}

