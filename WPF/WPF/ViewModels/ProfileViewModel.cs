using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WPF.Data;
using WPF.Models;

namespace WPF.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private User _user;
        private string _email;
        private string _name;
        private string _surname;
        private string _phoneNum;
        private string _password;
        private string _confirmPassword;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Surname
        {
            get => _surname;
            set => SetProperty(ref _surname, value);
        }

        public string PhoneNum
        {
            get => _phoneNum;
            set => SetProperty(ref _phoneNum, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string Rating
        {
            get
            {
                if (_user?.Rating == null)
                    return "Нет рейтинга";
                return $"{_user.Rating:F2} / 5.00";
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> SaveCompleted;

        public ProfileViewModel(User user)
        {
            _user = user;
            Email = user.Email;
            Name = user.Name;
            Surname = user.Surname ?? "";
            PhoneNum = user.PhoneNum;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(PhoneNum) &&
                   (string.IsNullOrWhiteSpace(Password) || Password == ConfirmPassword);
        }

        private void Save(object parameter)
        {
            if (!string.IsNullOrWhiteSpace(Password) && Password != ConfirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var user = context.Users.Find(_user.ID);
                    if (user != null)
                    {
                        if (Email != user.Email && context.Users.Any(u => u.Email == Email))
                        {
                            MessageBox.Show("Пользователь с таким email уже существует", "Ошибка", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        user.Email = Email;
                        user.Name = Name;
                        user.Surname = string.IsNullOrWhiteSpace(Surname) ? null : Surname;
                        user.PhoneNum = PhoneNum;

                        if (!string.IsNullOrWhiteSpace(Password))
                        {
                            user.Password = Password;
                        }

                        context.SaveChanges();

                        MessageBox.Show("Данные сохранены", "Успех", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        SaveCompleted?.Invoke(this, true);
                    }
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

