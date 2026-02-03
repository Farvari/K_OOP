using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WPF.Data;
using WPF.Models;

namespace WPF.ViewModels
{
    public class RateSellerViewModel : ViewModelBase
    {
        private User _seller;
        private User _currentUser;
        private decimal _rating = 5;
        private string _ratingTags;

        public User Seller
        {
            get => _seller;
            set => SetProperty(ref _seller, value);
        }

        public decimal Rating
        {
            get => _rating;
            set
            {
                decimal validValue = Math.Max(1.00m, Math.Min(5.00m, value));
                validValue = Math.Round(validValue, 2);
                SetProperty(ref _rating, validValue);
            }
        }

        public string RatingTags
        {
            get => _ratingTags;
            set => SetProperty(ref _ratingTags, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> SaveCompleted;

        public RateSellerViewModel(User seller, User currentUser)
        {
            Seller = seller;
            _currentUser = currentUser;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save(object parameter)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var existingReview = context.Reviews
                        .FirstOrDefault(r => r.UserID == Seller.ID && r.ReviewerID == _currentUser.ID);

                    if (existingReview != null)
                    {
                        MessageBox.Show("Вы уже оставляли отзыв этому продавцу", "Информация", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    decimal validRating = Math.Max(1.00m, Math.Min(5.00m, Rating));
                    validRating = Math.Round(validRating, 2);

                    var review = new Review
                    {
                        UserID = Seller.ID,
                        ReviewerID = _currentUser.ID,
                        Rating = validRating,
                        RatingTags = string.IsNullOrWhiteSpace(RatingTags) ? null : RatingTags
                    };

                    context.Reviews.Add(review);

                    context.Configuration.ValidateOnSaveEnabled = false;
                    try
                    {
                        context.SaveChanges();
                    }
                    finally
                    {
                        context.Configuration.ValidateOnSaveEnabled = true;
                    }

                    var avgRating = context.Reviews
                        .Where(r => r.UserID == Seller.ID)
                        .Average(r => (double?)r.Rating) ?? 0;

                    var seller = context.Users.Find(Seller.ID);
                    if (seller != null)
                    {
                        decimal calculatedRating = (decimal)Math.Round(avgRating, 2);
                        calculatedRating = Math.Max(1.00m, Math.Min(5.00m, calculatedRating));
                        
                        var entry = context.Entry(seller);
                        entry.Property(u => u.Rating).CurrentValue = calculatedRating;
                        entry.Property(u => u.Rating).IsModified = true;
                        
                        context.Configuration.ValidateOnSaveEnabled = false;
                        try
                        {
                            context.SaveChanges();
                        }
                        finally
                        {
                            context.Configuration.ValidateOnSaveEnabled = true;
                        }
                    }

                    MessageBox.Show("Отзыв добавлен", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    SaveCompleted?.Invoke(this, true);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Ошибка валидации при сохранении отзыва:");
                sb.AppendLine(dbEx.Message);
                
                foreach (var validationError in dbEx.EntityValidationErrors)
                {
                    sb.AppendLine($"\nСущность: {validationError.Entry.Entity.GetType().Name}");
                    foreach (var error in validationError.ValidationErrors)
                    {
                        sb.AppendLine($"  Свойство: {error.PropertyName}");
                        sb.AppendLine($"  Ошибка: {error.ErrorMessage}");
                    }
                }
                
                if (dbEx.InnerException != null)
                {
                    sb.AppendLine($"\nВнутреннее исключение: {dbEx.InnerException.Message}");
                }
                
                MessageBox.Show(sb.ToString(), "Ошибка валидации", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbUpdateEx)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Ошибка при обновлении базы данных:");
                sb.AppendLine(dbUpdateEx.Message);
                
                if (dbUpdateEx.InnerException != null)
                {
                    sb.AppendLine($"\nВнутреннее исключение: {dbUpdateEx.InnerException.Message}");
                    if (dbUpdateEx.InnerException.InnerException != null)
                    {
                        sb.AppendLine($"Детали: {dbUpdateEx.InnerException.InnerException.Message}");
                    }
                }
                
                MessageBox.Show(sb.ToString(), "Ошибка базы данных", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Ошибка при сохранении отзыва: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nДетали: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object parameter)
        {
            SaveCompleted?.Invoke(this, false);
        }
    }
}

