using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using WPF.Data;
using WPF.Models;

namespace WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private User _currentUser;
        private ObservableCollection<Item> _items;
        private Item _selectedItem;
        private string _searchText;
        private string _selectedCategory;
        private string _selectedSort;
        private string _priceFrom;
        private string _priceTo;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                LoadItems();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                LoadItems();
            }
        }

        public string SelectedSort
        {
            get => _selectedSort;
            set
            {
                SetProperty(ref _selectedSort, value);
                LoadItems();
            }
        }

        public string PriceFrom
        {
            get => _priceFrom;
            set
            {
                SetProperty(ref _priceFrom, value);
                LoadItems();
            }
        }

        public string PriceTo
        {
            get => _priceTo;
            set
            {
                SetProperty(ref _priceTo, value);
                LoadItems();
            }
        }

        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<string> SortOptions { get; set; }

        public ICommand LogoutCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewProfileCommand { get; }
        public ICommand ManageMyItemsCommand { get; }
        public ICommand ViewFavoritesCommand { get; }
        public ICommand ManageReportsCommand { get; }

        public bool IsAdmin => CurrentUser?.UserRole == 1;

        public event System.EventHandler LogoutRequested;

        public MainViewModel(User user)
        {
            CurrentUser = user;
            Items = new ObservableCollection<Item>();
            Categories = new ObservableCollection<string> { "Все категории" };
            SortOptions = new ObservableCollection<string> 
            { 
                "По дате (новые сначала)", 
                "По дате (старые сначала)", 
                "По цене (дешевые сначала)", 
                "По цене (дорогие сначала)",
                "По названию (А-Я)",
                "По названию (Я-А)"
            };

            SelectedSort = SortOptions[0];

            LogoutCommand = new RelayCommand(Logout);
            RefreshCommand = new RelayCommand(Refresh);
            ViewProfileCommand = new RelayCommand(ViewProfile);
            ManageMyItemsCommand = new RelayCommand(ManageMyItems);
            ViewFavoritesCommand = new RelayCommand(ViewFavorites);
            ManageReportsCommand = new RelayCommand(ManageReports);

            LoadCategories();
            SelectedCategory = "Все категории";
            LoadItems();
        }

        private void LoadCategories()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var categories = context.Items
                        .Where(i => i.Category != null)
                        .Select(i => i.Category)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();

                    Categories.Clear();
                    Categories.Add("Все категории");
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadItems()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var query = context.Items
                        .Include("User")
                        .Include("Images")
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        query = query.Where(i => i.ItemTitle.Contains(SearchText) || 
                                                 (i.Description != null && i.Description.Contains(SearchText)));
                    }

                    if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "Все категории")
                    {
                        query = query.Where(i => i.Category == SelectedCategory);
                    }

                    if (!string.IsNullOrWhiteSpace(PriceFrom))
                    {
                        if (int.TryParse(PriceFrom, out int priceFromValue))
                        {
                            query = query.Where(i => i.Price != null && i.Price >= priceFromValue);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(PriceTo))
                    {
                        if (int.TryParse(PriceTo, out int priceToValue))
                        {
                            query = query.Where(i => i.Price != null && i.Price <= priceToValue);
                        }
                    }

                    IQueryable<Item> sortedQuery;
                    if (SelectedSort == "По дате (новые сначала)")
                        sortedQuery = query.OrderByDescending(i => i.CreatedAt);
                    else if (SelectedSort == "По дате (старые сначала)")
                        sortedQuery = query.OrderBy(i => i.CreatedAt);
                    else if (SelectedSort == "По цене (дешевые сначала)")
                        sortedQuery = query.OrderBy(i => i.Price ?? int.MaxValue);
                    else if (SelectedSort == "По цене (дорогие сначала)")
                        sortedQuery = query.OrderByDescending(i => i.Price ?? 0);
                    else if (SelectedSort == "По названию (А-Я)")
                        sortedQuery = query.OrderBy(i => i.ItemTitle);
                    else if (SelectedSort == "По названию (Я-А)")
                        sortedQuery = query.OrderByDescending(i => i.ItemTitle);
                    else
                        sortedQuery = query.OrderByDescending(i => i.CreatedAt);

                    var items = sortedQuery.ToList();

                    Items.Clear();
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке объявлений: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void Logout(object parameter)
        {
            LogoutRequested?.Invoke(this, System.EventArgs.Empty);
        }


        private void Refresh(object parameter)
        {
            LoadItems();
        }

        private void ViewProfile(object parameter)
        {
            try
            {
                User userToEdit;
                using (var context = new ApplicationDbContext())
                {
                    userToEdit = context.Users
                        .Include("Role")
                        .AsNoTracking()
                        .FirstOrDefault(u => u.ID == CurrentUser.ID);
                    
                    if (userToEdit == null)
                    {
                        System.Windows.MessageBox.Show("Пользователь не найден", "Ошибка", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                }

                var profileView = new Views.ProfileView(userToEdit);
                if (profileView.ShowDialog() == true)
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var updatedUser = context.Users
                            .Include("Role")
                            .FirstOrDefault(u => u.ID == CurrentUser.ID);
                        if (updatedUser != null)
                        {
                            CurrentUser = updatedUser;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при открытии профиля: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ManageMyItems(object parameter)
        {
            var manageItemsView = new Views.ManageItemsView(CurrentUser);
            manageItemsView.ShowDialog();
            LoadItems();
        }

        private void ViewFavorites(object parameter)
        {
            try
            {
                var favoritesView = new Views.FavoritesView(CurrentUser);
                favoritesView.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при открытии избранного: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ManageReports(object parameter)
        {
            try
            {
                var reportsView = new Views.ReportsManagementView(CurrentUser);
                reportsView.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при открытии управления жалобами: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

