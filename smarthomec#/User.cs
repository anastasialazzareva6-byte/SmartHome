using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SmartHomeSystem
{
    public class User
    {
        private string passwordHash;
        private List<Activity> activityLog;

        public string UserId { get; private set; }
        public string Username { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsLoggedIn { get; private set; }

        public User(string userId, string username, string password,
                   AccessLevel accessLevel, string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("ID пользователя не может быть пустым", nameof(userId));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Имя пользователя не может быть пустым", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));

            UserId = userId;
            Username = username;
            SetPassword(password);
            AccessLevel = accessLevel;
            Email = email;
            Phone = phone;
            activityLog = new List<Activity>();
            IsLoggedIn = false;
        }

        public bool Login(string password)
        {
            try
            {
                if (VerifyPassword(password))
                {
                    IsLoggedIn = true;
                    AddActivity("Успешный вход в систему");
                    return true;
                }

                AddActivity("Неудачная попытка входа");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при входе: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            IsLoggedIn = false;
            AddActivity("Выход из системы");
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                if (!VerifyPassword(oldPassword))
                    throw new UnauthorizedAccessException("Неверный старый пароль");

                if (newPassword.Length < 3)
                    throw new ArgumentException("Пароль должен содержать минимум 3 символа", nameof(newPassword));

                if (newPassword.Contains(" "))
                    throw new ArgumentException("Пароль не должен содержать пробелов", nameof(newPassword));

                SetPassword(newPassword);
                AddActivity("Пароль изменен");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Ошибка изменения пароля: {ex.Message}");
                AddActivity($"Неудачная попытка изменения пароля: {ex.Message}");
                return false;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка изменения пароля: {ex.Message}");
                AddActivity($"Неудачная попытка изменения пароля: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка при изменении пароля: {ex.Message}");
                return false;
            }
        }

        public void SetAccessLevel(AccessLevel level)
        {
            AccessLevel = level;
            AddActivity($"Уровень доступа изменен на {level}");
        }

        public List<Activity> GetActivityHistory()
        {
            return new List<Activity>(activityLog);
        }

        public void AddActivity(string action, object relatedObject = null)
        {
            try
            {
                string activityId = $"ACT_{DateTime.Now.Ticks}";
                Activity activity = new Activity(activityId, this, action, relatedObject);
                activityLog.Add(activity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления активности: {ex.Message}");
            }
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"┌────────────────────────────────────────");
            Console.WriteLine($"│ Пользователь: {Username}");
            Console.WriteLine($"│ ID: {UserId}");
            Console.WriteLine($"│ Уровень доступа: {(AccessLevel == AccessLevel.ADMIN ? "Администратор" : "Пользователь")}");
            Console.WriteLine($"│ Email: {Email}");
            Console.WriteLine($"│ Телефон: {Phone}");
            Console.WriteLine($"│ В системе: {(IsLoggedIn ? "ДА" : "НЕТ")}");
            Console.WriteLine($"└────────────────────────────────────────");
        }

        private void SetPassword(string password)
        {
            passwordHash = HashPassword(password);
        }

        private bool VerifyPassword(string password)
        {
            return HashPassword(password) == passwordHash;
        }

        private string HashPassword(string password)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                        builder.Append(b.ToString("x2"));
                    return builder.ToString();
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Ошибка хэширования пароля: {ex.Message}");
                throw;
            }
        }

        public string Serialize()
        {
            return $"{UserId}|{Username}|{passwordHash}|{(int)AccessLevel}|{Email}|{Phone}";
        }

        public static User Deserialize(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Данные для десериализации не могут быть пустыми", nameof(data));

            var parts = data.Split('|');
            if (parts.Length != 6)
                throw new FormatException($"Неверный формат данных пользователя: {data}");

            try
            {
                var user = new User(
                    parts[0],
                    parts[1],
                    "temp",
                    (AccessLevel)int.Parse(parts[3]),
                    parts[4],
                    parts[5]
                )
                {
                    passwordHash = parts[2]
                };
                return user;
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Ошибка парсинга данных пользователя: {data}", ex);
            }
        }

        public override string ToString()
        {
            return $"{Username} ({(AccessLevel == AccessLevel.ADMIN ? "Админ" : "Пользователь")})";
        }
    }
}