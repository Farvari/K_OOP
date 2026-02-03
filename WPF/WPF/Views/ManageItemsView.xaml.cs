using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class ManageItemsView : System.Windows.Window
    {
        public ManageItemsView(User user)
        {
            InitializeComponent();
            DataContext = new ManageItemsViewModel(user);
        }
    }
}

