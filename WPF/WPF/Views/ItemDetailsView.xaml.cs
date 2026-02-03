using System.Data.Entity;
using System.Linq;
using WPF.Data;
using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class ItemDetailsView : System.Windows.Window
    {
        public ItemDetailsView(Item item, User currentUser)
        {
            InitializeComponent();
            
            Item loadedItem;
            using (var context = new ApplicationDbContext())
            {
                loadedItem = context.Items
                    .Include("User")
                    .Include("Images")
                    .AsNoTracking()
                    .FirstOrDefault(i => i.ID == item.ID);
                
                if (loadedItem != null && loadedItem.Images != null)
                {
                    loadedItem.Images = loadedItem.Images.OrderBy(img => img.ImgID).ToList();
                }
            }

            if (loadedItem != null)
            {
                var viewModel = new ItemDetailsViewModel(loadedItem, currentUser);
                DataContext = viewModel;
            }
        }
    }
}

