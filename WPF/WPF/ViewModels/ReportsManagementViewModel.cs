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
    public class ReportsManagementViewModel : ViewModelBase
    {
        private User _currentUser;
        private ObservableCollection<Report> _reports;
        private Report _selectedReport;
        private string _statusFilter;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ObservableCollection<Report> Reports
        {
            get => _reports;
            set => SetProperty(ref _reports, value);
        }

        public Report SelectedReport
        {
            get => _selectedReport;
            set
            {
                if (SetProperty(ref _selectedReport, value))
                {
                    if (value != null)
                    {
                        LoadReportNavigationProperties(value);
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private void LoadReportNavigationProperties(Report report)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var loadedReport = context.Reports
                        .Include("Reporter")
                        .Include("ReportedUser")
                        .Include("Item")
                        .Include("Resolver")
                        .FirstOrDefault(r => r.ID == report.ID);

                    if (loadedReport != null)
                    {
                        report.Reporter = loadedReport.Reporter;
                        report.ReportedUser = loadedReport.ReportedUser;
                        report.Item = loadedReport.Item;
                        report.Resolver = loadedReport.Resolver;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных жалобы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                SetProperty(ref _statusFilter, value);
                LoadReports();
            }
        }

        public ObservableCollection<string> StatusFilters { get; set; }

        public ICommand RefreshCommand { get; }
        public ICommand BlockUserCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ResolveReportCommand { get; }

        public event EventHandler<bool> CloseRequested;
        public event EventHandler<Item> EditItemRequested;

        public ReportsManagementViewModel(User currentUser)
        {
            CurrentUser = currentUser;
            Reports = new ObservableCollection<Report>();
            StatusFilters = new ObservableCollection<string> { "Все", "Открыто", "В обработке", "Решено", "Отклонено" };
            StatusFilter = "Все";

            RefreshCommand = new RelayCommand(Refresh);
            BlockUserCommand = new RelayCommand(BlockUser, CanAction);
            EditItemCommand = new RelayCommand(EditItem, CanAction);
            DeleteItemCommand = new RelayCommand(DeleteItem, CanAction);
            ResolveReportCommand = new RelayCommand(ResolveReport, CanAction);

            LoadReports();
        }

        private bool CanAction(object parameter)
        {
            return SelectedReport != null && SelectedReport.Status != "Решено";
        }

        private void LoadReports()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var query = context.Reports
                        .Include("Reporter")
                        .Include("ReportedUser")
                        .Include("Item")
                        .Include("Resolver")
                        .AsQueryable();

                    if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "Все")
                    {
                        query = query.Where(r => r.Status == StatusFilter);
                    }

                    var reports = query
                        .OrderByDescending(r => r.SentAt)
                        .ToList();

                    Reports.Clear();
                    foreach (var report in reports)
                    {
                        Reports.Add(report);
                    }
                    
                    OnPropertyChanged(nameof(SelectedReport));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке жалоб: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh(object parameter)
        {
            LoadReports();
        }

        private void BlockUser(object parameter)
        {
            if (SelectedReport == null || SelectedReport.ReportedUser == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите заблокировать пользователя {SelectedReport.ReportedUser.Name} {SelectedReport.ReportedUser.Surname}?",
                "Подтверждение блокировки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var user = context.Users.Find(SelectedReport.ReportedUser.ID);
                        if (user != null)
                        {
                            user.IsBlocked = 1;
                            context.SaveChanges();

                            ResolveReportInternal("Решено", "Пользователь заблокирован");
                            
                            MessageBox.Show("Пользователь успешно заблокирован", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadReports();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при блокировке пользователя: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditItem(object parameter)
        {
            if (SelectedReport == null || SelectedReport.Item == null)
            {
                MessageBox.Show("К этой жалобе не привязано объявление", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var item = context.Items
                        .AsNoTracking()
                        .FirstOrDefault(i => i.ID == SelectedReport.Item.ID);

                    if (item != null)
                    {
                        EditItemRequested?.Invoke(this, item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке объявления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteItem(object parameter)
        {
            if (SelectedReport == null || SelectedReport.Item == null)
            {
                MessageBox.Show("К этой жалобе не привязано объявление", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить объявление \"{SelectedReport.Item.ItemTitle}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var item = context.Items.Find(SelectedReport.Item.ID);
                        if (item != null)
                        {
                            context.Items.Remove(item);
                            context.SaveChanges();

                            ResolveReportInternal("Решено", "Объявление удалено");

                            MessageBox.Show("Объявление успешно удалено", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadReports();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении объявления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResolveReport(object parameter)
        {
            if (SelectedReport == null)
                return;

            var result = MessageBox.Show(
                "Вы уверены, что хотите закрыть эту жалобу?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ResolveReportInternal("Решено", "Жалоба рассмотрена администратором");
                MessageBox.Show("Жалоба успешно закрыта", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadReports();
            }
        }

        private void ResolveReportInternal(string status, string reason = null)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var report = context.Reports.Find(SelectedReport.ID);
                    if (report != null)
                    {
                        report.Status = status;
                        report.ResolverID = CurrentUser.ID;
                        report.ResolvedAt = DateTime.Now;
                        report.UpdatedAt = DateTime.Now;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении жалобы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OnItemSaved()
        {
            LoadReports();
        }
    }
}

