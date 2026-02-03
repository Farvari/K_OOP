using System.Windows;
using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class ReportView : Window
    {
        public ReportView(User reportedUser, User currentUser, Item reportedItem)
        {
            InitializeComponent();
            var viewModel = new ReportViewModel(reportedUser, currentUser, reportedItem);
            DataContext = viewModel;
            
            viewModel.SaveCompleted += (s, result) => DialogResult = result;
        }
    }
}

