using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WPF.Data;
using WPF.Models;

namespace WPF.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _name;
        private string _surname;
        private string _phoneNum;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<User> RegistrationSuccessful;
        public event EventHandler CancelRequested;

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(Register, CanRegister);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(PhoneNum);
        }

        private void Register(object parameter)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(PhoneNum))
            {
                ErrorMessage = "Заполните все обязательные поля";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Пароль должен содержать минимум 6 символов";
                return;
            }

            if (PhoneNum.Length != 12 || !PhoneNum.StartsWith("375"))
            {
                ErrorMessage = "Номер телефона должен быть в формате 375123456789";
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    if (context.Users.Any(u => u.Email == Email))
                    {
                        ErrorMessage = "Пользователь с таким email уже существует";
                        return;
                    }

                    var userRole = context.Roles.FirstOrDefault(r => r.RoleID == 2);
                    if (userRole == null)
                    {
                        ErrorMessage = "Ошибка: роль пользователя не найдена";
                        return;
                    }

                    var newUser = new User
                    {
                        Email = Email,
                        Password = Password,
                        Name = Name,
                        Surname = string.IsNullOrWhiteSpace(Surname) ? null : Surname,
                        PhoneNum = PhoneNum,
                        RegistrDate = DateTime.Now,
                        UserRole = 2,
                        IsBlocked = 0
                    };

                    context.Configuration.ValidateOnSaveEnabled = false;
                    context.Users.Add(newUser);
                    
                    try
                    {
                        context.SaveChanges();
                    }
                    finally
                    {
                        context.Configuration.ValidateOnSaveEnabled = true;
                    }
                    
                    var savedUser = context.Users
                        .Include("Role")
                        .FirstOrDefault(u => u.Email == Email);

                    MessageBox.Show("Регистрация успешна! Теперь вы можете войти в систему.", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    RegistrationSuccessful?.Invoke(this, savedUser ?? newUser);
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
            {
                var innerEx = dbEx.InnerException;
                if (innerEx != null && innerEx.InnerException != null)
                {
                    ErrorMessage = $"Ошибка базы данных: {innerEx.InnerException.Message}";
                }
                else
                {
                    ErrorMessage = $"Ошибка при сохранении: {dbEx.Message}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при регистрации: {ex.Message}";
                if (ex.InnerException != null)
                {
                    ErrorMessage += $"\nДетали: {ex.InnerException.Message}";
                }
            }
        }

        private void Cancel(object parameter)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

