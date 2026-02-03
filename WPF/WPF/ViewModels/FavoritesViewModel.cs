using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WPF.Data;
using WPF.Models;

namespace WPF.ViewModels
{
    public class FavoritesViewModel : ViewModelBase
    {
        private User _currentUser;
        private ObservableCollection<Item> _favoriteItems;
        private Item _selectedItem;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ObservableCollection<Item> FavoriteItems
        {
            get => _favoriteItems;
            set => SetProperty(ref _favoriteItems, value);
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand RemoveFromFavoritesCommand { get; }
        public ICommand RefreshCommand { get; }

        public FavoritesViewModel(User user)
        {
            CurrentUser = user;
            FavoriteItems = new ObservableCollection<Item>();

            RemoveFromFavoritesCommand = new RelayCommand(RemoveFromFavorites, CanRemove);
            RefreshCommand = new RelayCommand(Refresh);

            LoadFavorites();
        }

        private bool CanRemove(object parameter)
        {
            return SelectedItem != null;
        }

        private void LoadFavorites()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var favoriteItemIds = context.Favorites
                        .Where(f => f.UserID == CurrentUser.ID)
                        .Select(f => f.FavItemID)
                        .ToList();

                    var items = context.Items
                        .Include("User")
                        .Include("Images")
                        .Where(i => favoriteItemIds.Contains(i.ID))
                        .OrderByDescending(i => i.CreatedAt)
                        .ToList();

                    FavoriteItems.Clear();
                    foreach (var item in items)
                    {
                        FavoriteItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке избранного: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveFromFavorites(object parameter)
        {
            if (SelectedItem == null) return;

            var result = MessageBox.Show("Удалить объявление из избранного?", "Подтверждение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var favorite = context.Favorites
                            .FirstOrDefault(f => f.UserID == CurrentUser.ID && f.FavItemID == SelectedItem.ID);

                        if (favorite != null)
                        {
                            context.Favorites.Remove(favorite);
                            context.SaveChanges();
                            MessageBox.Show("Объявление удалено из избранного", "Успех", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadFavorites();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Refresh(object parameter)
        {
            LoadFavorites();
        }
    }
}

