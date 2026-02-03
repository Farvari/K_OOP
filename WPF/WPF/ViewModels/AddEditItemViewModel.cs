using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WPF.Data;
using WPF.Models;

namespace WPF.ViewModels
{
    public class AddEditItemViewModel : ViewModelBase
    {
        private Item _item;
        private User _currentUser;
        private bool _isEditMode;
        private bool _isAdmin;
        private string _itemTitle;
        private string _description;
        private string _category;
        private string _price;
        private string _contactInfo;
        private string _status;
        private ObservableCollection<string> _categories;
        private ObservableCollection<string> _statuses;

        public string ItemTitle
        {
            get => _itemTitle;
            set => SetProperty(ref _itemTitle, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public string ContactInfo
        {
            get => _contactInfo;
            set => SetProperty(ref _contactInfo, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<string> Statuses { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> SaveCompleted;

        public AddEditItemViewModel(Item item, User currentUser, bool isAdmin = false)
        {
            _item = item;
            _currentUser = currentUser;
            _isEditMode = item != null;
            _isAdmin = isAdmin;

            Categories = new ObservableCollection<string>();
            Statuses = new ObservableCollection<string> { "Активно", "Неактивно", "Продано" };

            LoadCategories();

            if (_isEditMode)
            {
                ItemTitle = item.ItemTitle;
                Description = item.Description;
                Category = item.Category;
                Price = item.Price?.ToString() ?? "";
                ContactInfo = item.ContactInfo;
                Status = item.Status;
            }
            else
            {
                Status = "Активно";
                ContactInfo = currentUser.PhoneNum;
            }

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadCategories()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var cats = context.Items
                        .Where(i => i.Category != null)
                        .Select(i => i.Category)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();

                    Categories.Clear();
                    foreach (var cat in cats)
                    {
                        Categories.Add(cat);
                    }
                }
            }
            catch { }
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(ItemTitle) &&
                   !string.IsNullOrWhiteSpace(Category) &&
                   !string.IsNullOrWhiteSpace(Status);
        }

        private void Save(object parameter)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    if (_isEditMode)
                    {
                        var item = context.Items.Find(_item.ID);
                        if (item != null)
                        {
                            if (!_isAdmin && item.UserID != _currentUser.ID)
                            {
                                MessageBox.Show("Вы можете редактировать только свои объявления", "Ошибка", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            item.ItemTitle = ItemTitle;
                            item.Description = Description;
                            item.Category = Category;
                            item.Price = int.TryParse(Price, out int priceValue) ? priceValue : (int?)null;
                            item.ContactInfo = ContactInfo;
                            item.Status = Status;
                            item.UpdatedAt = DateTime.Now;
                        }
                    }
                    else
                    {
                        var newItem = new Item
                        {
                            UserID = _currentUser.ID,
                            ItemTitle = ItemTitle,
                            Description = Description,
                            Category = Category,
                            Price = int.TryParse(Price, out int priceValue) ? priceValue : (int?)null,
                            ContactInfo = ContactInfo,
                            Status = Status,
                            CreatedAt = DateTime.Now
                        };
                        context.Items.Add(newItem);
                    }

                    context.SaveChanges();
                    MessageBox.Show(_isEditMode ? "Объявление обновлено" : "Объявление создано", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    SaveCompleted?.Invoke(this, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object parameter)
        {
            SaveCompleted?.Invoke(this, false);
        }
    }
}

