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
    public class ManageItemsViewModel : ViewModelBase
    {
        private User _currentUser;
        private ObservableCollection<Item> _myItems;
        private Item _selectedItem;
        private bool _isAdmin;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ObservableCollection<Item> MyItems
        {
            get => _myItems;
            set => SetProperty(ref _myItems, value);
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public bool IsAdmin => _isAdmin;

        public string Title => "Мои объявления";

        public ICommand AddItemCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand RefreshCommand { get; }

        public ManageItemsViewModel(User user)
        {
            CurrentUser = user;
            MyItems = new ObservableCollection<Item>();
            _isAdmin = user.UserRole == 1;

            AddItemCommand = new RelayCommand(AddItem, CanAddItem);
            EditItemCommand = new RelayCommand(EditItem, CanEditDelete);
            DeleteItemCommand = new RelayCommand(DeleteItem, CanEditDelete);
            RefreshCommand = new RelayCommand(Refresh);

            LoadMyItems();
        }

        private bool CanEditDelete(object parameter)
        {
            return SelectedItem != null;
        }

        private void LoadMyItems()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var items = context.Items
                        .Include("User")
                        .Where(i => i.UserID == CurrentUser.ID)
                        .OrderByDescending(i => i.CreatedAt)
                        .ToList();

                    MyItems.Clear();
                    foreach (var item in items)
                    {
                        MyItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке объявлений: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddItem(object parameter)
        {
            return true;
        }

        private void AddItem(object parameter)
        {
            var addItemView = new Views.AddEditItemView(null, CurrentUser, _isAdmin);
            if (addItemView.ShowDialog() == true)
            {
                LoadMyItems();
            }
        }

        private void EditItem(object parameter)
        {
            if (SelectedItem == null) return;
            
            var editItemView = new Views.AddEditItemView(SelectedItem, CurrentUser, false);
            if (editItemView.ShowDialog() == true)
            {
                LoadMyItems();
            }
        }

        private void DeleteItem(object parameter)
        {
            if (SelectedItem == null) return;

            var result = MessageBox.Show("Вы уверены, что хотите удалить это объявление?", "Подтверждение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var item = context.Items.Find(SelectedItem.ID);
                        if (item != null)
                        {
                            if (item.UserID != CurrentUser.ID)
                            {
                                MessageBox.Show("Вы можете удалять только свои объявления", "Ошибка", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            context.Items.Remove(item);
                            context.SaveChanges();
                            MessageBox.Show("Объявление удалено", "Успех", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadMyItems();
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
            LoadMyItems();
        }
    }
}

