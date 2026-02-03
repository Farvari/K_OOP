using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class FavoritesView : System.Windows.Window
    {
        public FavoritesView(User user)
        {
            InitializeComponent();
            DataContext = new FavoritesViewModel(user);
        }
    }
}

