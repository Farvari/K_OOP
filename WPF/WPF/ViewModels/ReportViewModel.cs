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
    public class ReportViewModel : ViewModelBase
    {
        private User _reportedUser;
        private User _currentUser;
        private Item _reportedItem;
        private string _reportTitle;
        private string _description;

        public string ReportedUserName
        {
            get
            {
                if (_reportedUser == null) return "";
                return $"{_reportedUser.Name} {_reportedUser.Surname}".Trim();
            }
        }

        public string ReportedItemTitle
        {
            get => _reportedItem?.ItemTitle ?? "";
        }

        public string ReportTitle
        {
            get => _reportTitle;
            set => SetProperty(ref _reportTitle, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> SaveCompleted;

        public ReportViewModel(User reportedUser, User currentUser, Item reportedItem)
        {
            _reportedUser = reportedUser;
            _currentUser = currentUser;
            _reportedItem = reportedItem;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(ReportTitle) && 
                   !string.IsNullOrWhiteSpace(Description);
        }

        private void Save(object parameter)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var report = new Report
                    {
                        ReporterID = _currentUser.ID,
                        ReportedID = _reportedUser.ID,
                        ItemID = _reportedItem?.ID,
                        ReportTitle = ReportTitle,
                        Description = Description,
                        Status = "На рассмотрении",
                        SentAt = DateTime.Now
                    };

                    context.Reports.Add(report);
                    context.SaveChanges();

                    MessageBox.Show("Жалоба отправлена", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    SaveCompleted?.Invoke(this, true);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Ошибка валидации при сохранении:");
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
                
                MessageBox.Show(sb.ToString(), "Ошибка БД", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при отправке жалобы: {ex.Message}";
                
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nВнутреннее исключение: {ex.InnerException.Message}";
                    
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nДетали: {ex.InnerException.InnerException.Message}";
                    }
                }
                
                errorMessage += $"\n\nТип исключения: {ex.GetType().Name}";
                errorMessage += $"\n\nStackTrace:\n{ex.StackTrace}";
                
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

