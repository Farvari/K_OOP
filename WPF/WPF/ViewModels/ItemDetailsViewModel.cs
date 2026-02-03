using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using WPF.Data;
using WPF.Models;

namespace WPF.ViewModels
{
    public class ItemDetailsViewModel : ViewModelBase
    {
        private Item _item;
        private User _currentUser;
        private int _selectedImageIndex = 0;

        public Item Item
        {
            get => _item;
            set
            {
                SetProperty(ref _item, value);
                SelectedImageIndex = 0;
                OnPropertyChanged(nameof(CurrentImage));
                OnPropertyChanged(nameof(ImageCounter));
                OnPropertyChanged(nameof(CanGoToPrevious));
                OnPropertyChanged(nameof(CanGoToNext));
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public int SelectedImageIndex
        {
            get => _selectedImageIndex;
            set
            {
                if (value >= 0 && Item?.Images != null && value < Item.Images.Count)
                {
                    SetProperty(ref _selectedImageIndex, value);
                    OnPropertyChanged(nameof(CurrentImage));
                    OnPropertyChanged(nameof(ImageCounter));
                    OnPropertyChanged(nameof(CanGoToPrevious));
                    OnPropertyChanged(nameof(CanGoToNext));
                }
            }
        }

        public Image CurrentImage
        {
            get
            {
                if (Item?.Images != null && Item.Images.Count > 0 && SelectedImageIndex >= 0 && SelectedImageIndex < Item.Images.Count)
                {
                    return Item.Images.OrderBy(img => img.ImgID).ElementAt(SelectedImageIndex);
                }
                return null;
            }
        }

        public string ImageCounter
        {
            get
            {
                if (Item?.Images == null || Item.Images.Count == 0)
                    return "Нет изображений";
                return $"{SelectedImageIndex + 1} из {Item.Images.Count}";
            }
        }

        public bool CanGoToPrevious => Item?.Images != null && Item.Images.Count > 0 && SelectedImageIndex > 0;
        public bool CanGoToNext => Item?.Images != null && Item.Images.Count > 0 && SelectedImageIndex < Item.Images.Count - 1;

        public ICommand AddToFavoriteCommand { get; }
        public ICommand RateSellerCommand { get; }
        public ICommand ReportSellerCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand PreviousImageCommand { get; }
        public ICommand NextImageCommand { get; }

        public bool IsAdmin => CurrentUser?.UserRole == 1;

        public ItemDetailsViewModel(Item item, User currentUser)
        {
            Item = item;
            CurrentUser = currentUser;

            AddToFavoriteCommand = new RelayCommand(AddToFavorite, CanAddToFavorite);
            RateSellerCommand = new RelayCommand(RateSeller, CanRateSeller);
            ReportSellerCommand = new RelayCommand(ReportSeller, CanReportSeller);
            EditItemCommand = new RelayCommand(EditItem, CanEditItem);
            PreviousImageCommand = new RelayCommand(PreviousImage, _ => CanGoToPrevious);
            NextImageCommand = new RelayCommand(NextImage, _ => CanGoToNext);
        }

        private void PreviousImage(object parameter)
        {
            if (CanGoToPrevious)
            {
                SelectedImageIndex--;
            }
        }

        private void NextImage(object parameter)
        {
            if (CanGoToNext)
            {
                SelectedImageIndex++;
            }
        }

        private bool CanAddToFavorite(object parameter)
        {
            return Item != null && Item.UserID != CurrentUser.ID;
        }

        private void AddToFavorite(object parameter)
        {
            if (Item == null) return;

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var existingFavorite = context.Favorites
                        .FirstOrDefault(f => f.UserID == CurrentUser.ID && f.FavItemID == Item.ID);

                    if (existingFavorite != null)
                    {
                        System.Windows.MessageBox.Show("Объявление уже в избранном", "Информация", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        return;
                    }

                    var favorite = new Favorite
                    {
                        UserID = CurrentUser.ID,
                        FavItemID = Item.ID,
                        AddedAt = DateTime.Now
                    };

                    context.Favorites.Add(favorite);
                    context.SaveChanges();

                    System.Windows.MessageBox.Show("Объявление добавлено в избранное", "Успех", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при добавлении в избранное: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanRateSeller(object parameter)
        {
            return Item != null && Item.UserID != CurrentUser.ID;
        }

        private void RateSeller(object parameter)
        {
            if (Item == null) return;
            var rateView = new Views.RateSellerView(Item.User, CurrentUser);
            rateView.ShowDialog();
        }

        private bool CanReportSeller(object parameter)
        {
            return Item != null && Item.UserID != CurrentUser.ID;
        }

        private void ReportSeller(object parameter)
        {
            if (Item == null) return;
            var reportView = new Views.ReportView(Item.User, CurrentUser, Item);
            reportView.ShowDialog();
        }

        private bool CanEditItem(object parameter)
        {
            return IsAdmin && Item != null;
        }

        private void EditItem(object parameter)
        {
            if (Item == null) return;

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var itemOwner = context.Users.Find(Item.UserID);
                    if (itemOwner == null)
                    {
                        System.Windows.MessageBox.Show("Владелец объявления не найден", "Ошибка",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }

                    var editItemView = new Views.AddEditItemView(Item, itemOwner, true);
                    if (editItemView.ShowDialog() == true)
                    {
                        var updatedItem = context.Items
                            .Include("User")
                            .Include("Images")
                            .AsNoTracking()
                            .FirstOrDefault(i => i.ID == Item.ID);
                        
                        if (updatedItem != null)
                        {
                            if (updatedItem.Images != null)
                            {
                                updatedItem.Images = updatedItem.Images.OrderBy(img => img.ImgID).ToList();
                            }
                            Item = updatedItem;
                            OnPropertyChanged(nameof(Item));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при редактировании объявления: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

