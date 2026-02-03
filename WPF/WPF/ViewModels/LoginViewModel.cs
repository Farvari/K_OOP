using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WPF.Data;
using WPF.Models;

namespace WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _email;
        private string _password;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public event EventHandler<User> LoginSuccessful;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login, CanLogin);
            RegisterCommand = new RelayCommand(Register);
        }

        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private void Login(object parameter)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Пожалуйста, введите email и пароль";
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var user = context.Users
                        .Include("Role")
                        .FirstOrDefault(u => u.Email == Email && u.Password == Password);

                    if (user == null)
                    {
                        ErrorMessage = "Неверный email или пароль";
                        return;
                    }

                    if (user.IsBlocked == 1)
                    {
                        ErrorMessage = "Ваш аккаунт заблокирован";
                        return;
                    }

                    LoginSuccessful?.Invoke(this, user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при входе: {ex.Message}";
            }
        }

        private void Register(object parameter)
        {
            var registerView = new Views.RegisterView();
            registerView.ShowDialog();
        }
    }
}

