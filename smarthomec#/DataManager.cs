using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SmartHomeSystem
{
    public static class DataManager
    {
        private const string ROOMS_FILE = "rooms.dat";
        private const string DEVICES_FILE = "devices.dat";
        private const string USERS_FILE = "users.dat";
        private const string SCENARIOS_FILE = "scenarios.dat";
        private const string NOTIFICATIONS_FILE = "notifications.dat";

        private static List<string> operationLog = new List<string>();

        public static void LogOperation(string operation, bool success, string details = "")
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {operation}: {(success ? "Успешно" : "Ошибка")}";
            if (!string.IsNullOrEmpty(details))
                logEntry += $" - {details}";

            operationLog.Add(logEntry);

            if (!success)
                Console.WriteLine($"[ЛОГ] {logEntry}");
        }

        public static List<string> GetOperationLog()
        {
            return new List<string>(operationLog);
        }

        public static void ClearOperationLog()
        {
            operationLog.Clear();
        }

        public static void SaveRooms(List<Room> rooms)
        {
            try
            {
                using (FileStream fs = new FileStream(ROOMS_FILE, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter file = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var room in rooms)
                    {
                        try
                        {
                            string serializedData = room.Serialize();
                            if (!string.IsNullOrEmpty(serializedData))
                                file.WriteLine(serializedData);
                        }
                        catch (InvalidOperationException ex)
                        {
                            Console.WriteLine($"Ошибка сериализации комнаты {room.Name}: {ex.Message}");
                            throw;
                        }
                    }
                }
                LogOperation("Сохранение комнат", true, $"Сохранено {rooms.Count} комнат");
            }
            catch (IOException ex)
            {
                LogOperation("Сохранение комнат", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при сохранении комнат: {ex.Message}");
                throw new IOException("Не удалось сохранить данные комнат", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Сохранение комнат", false, ex.Message);
                Console.WriteLine($"Нет доступа для сохранения комнат: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                LogOperation("Сохранение комнат", false, ex.Message);
                Console.WriteLine($"Непредвиденная ошибка при сохранении комнат: {ex.Message}");
                throw;
            }
        }

        public static List<Room> LoadRooms()
        {
            List<Room> rooms = new List<Room>();

            if (!File.Exists(ROOMS_FILE))
            {
                LogOperation("Загрузка комнат", true, "Файл не существует, возвращен пустой список");
                return rooms;
            }

            try
            {
                using (FileStream fs = new FileStream(ROOMS_FILE, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader file = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    int lineNumber = 0;
                    int loadedCount = 0;

                    while ((line = file.ReadLine()) != null)
                    {
                        lineNumber++;

                        try
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var room = Room.Deserialize(line);
                                if (room != null)
                                {
                                    rooms.Add(room);
                                    loadedCount++;
                                }
                                else
                                {
                                    Console.WriteLine($"Внимание: Не удалось десериализовать комнату в строке {lineNumber}");
                                }
                            }
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine($"Ошибка формата данных комнаты в строке {lineNumber}: {ex.Message}");
                            continue;
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine($"Ошибка данных комнаты в строке {lineNumber}: {ex.Message}");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Непредвиденная ошибка при десериализации комнаты в строке {lineNumber}: {ex.Message}");
                            continue;
                        }
                    }

                    LogOperation("Загрузка комнат", true, $"Загружено {loadedCount} комнат из файла");
                }
            }
            catch (IOException ex)
            {
                LogOperation("Загрузка комнат", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при загрузке комнат: {ex.Message}");
                throw new IOException("Не удалось загрузить данные комнат", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Загрузка комнат", false, ex.Message);
                Console.WriteLine($"Нет доступа для загрузки комнат: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                LogOperation("Загрузка комнат", false, ex.Message);
                Console.WriteLine($"Непредвиденная ошибка при загрузке комнат: {ex.Message}");
                throw;
            }

            return rooms;
        }

        public static void SaveDevices(List<Device> devices)
        {
            try
            {
                using (FileStream fs = new FileStream(DEVICES_FILE, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter file = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var device in devices)
                    {
                        try
                        {
                            string serializedData = device.Serialize();
                            if (!string.IsNullOrEmpty(serializedData))
                                file.WriteLine(serializedData);
                        }
                        catch (InvalidOperationException ex)
                        {
                            Console.WriteLine($"Ошибка сериализации устройства {device.Name}: {ex.Message}");
                            throw;
                        }
                    }
                }
                LogOperation("Сохранение устройств", true, $"Сохранено {devices.Count} устройств");
            }
            catch (IOException ex)
            {
                LogOperation("Сохранение устройств", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при сохранении устройств: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Сохранение устройств", false, ex.Message);
                Console.WriteLine($"Нет доступа для сохранения устройств: {ex.Message}");
                throw;
            }
        }

        public static List<Device> LoadDevices(List<Room> rooms)
        {
            List<Device> devices = new List<Device>();

            if (!File.Exists(DEVICES_FILE))
            {
                LogOperation("Загрузка устройств", true, "Файл не существует, возвращен пустой список");
                return devices;
            }

            try
            {
                using (FileStream fs = new FileStream(DEVICES_FILE, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader file = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    int lineNumber = 0;
                    int loadedCount = 0;
                    int missingRoomsCount = 0;

                    while ((line = file.ReadLine()) != null)
                    {
                        lineNumber++;

                        try
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var parts = line.Split('|');
                                if (parts.Length >= 5)
                                {
                                    string locationId = parts[4];
                                    Room location = rooms.FirstOrDefault(r => r.RoomId == locationId);

                                    if (location != null || locationId == "NULL")
                                    {
                                        var device = Device.Deserialize(line, location);
                                        if (device != null)
                                        {
                                            devices.Add(device);
                                            if (location != null)
                                                location.AddDevice(device);
                                            loadedCount++;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Внимание: Не найдена комната с ID {locationId} для устройства в строке {lineNumber}");
                                        missingRoomsCount++;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Внимание: Неверный формат данных устройства в строке {lineNumber}");
                                }
                            }
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine($"Ошибка формата данных устройства в строке {lineNumber}: {ex.Message}");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Непредвиденная ошибка при десериализации устройства в строке {lineNumber}: {ex.Message}");
                            continue;
                        }
                    }

                    string details = $"Загружено {loadedCount} устройств";
                    if (missingRoomsCount > 0)
                        details += $", не найдено комнат для {missingRoomsCount} устройств";

                    LogOperation("Загрузка устройств", true, details);
                }
            }
            catch (IOException ex)
            {
                LogOperation("Загрузка устройств", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при загрузке устройств: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Загрузка устройств", false, ex.Message);
                Console.WriteLine($"Нет доступа для загрузки устройств: {ex.Message}");
                throw;
            }

            return devices;
        }

        public static void SaveUsers(List<User> users)
        {
            try
            {
                using (FileStream fs = new FileStream(USERS_FILE, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter file = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var user in users)
                    {
                        file.WriteLine(user.Serialize());
                    }
                }
                LogOperation("Сохранение пользователей", true, $"Сохранено {users.Count} пользователей");
            }
            catch (IOException ex)
            {
                LogOperation("Сохранение пользователей", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при сохранении пользователей: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Сохранение пользователей", false, ex.Message);
                Console.WriteLine($"Нет доступа для сохранения пользователей: {ex.Message}");
                throw;
            }
        }

        public static List<User> LoadUsers()
        {
            List<User> users = new List<User>();

            if (!File.Exists(USERS_FILE))
            {
                LogOperation("Загрузка пользователей", true, "Файл не существует, возвращен пустой список");
                return users;
            }

            try
            {
                using (FileStream fs = new FileStream(USERS_FILE, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader file = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    int loadedCount = 0;

                    while ((line = file.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            try
                            {
                                var user = User.Deserialize(line);
                                if (user != null)
                                {
                                    users.Add(user);
                                    loadedCount++;
                                }
                            }
                            catch (FormatException ex)
                            {
                                Console.WriteLine($"Ошибка формата данных пользователя: {ex.Message}");
                                continue;
                            }
                        }
                    }

                    LogOperation("Загрузка пользователей", true, $"Загружено {loadedCount} пользователей");
                }
            }
            catch (IOException ex)
            {
                LogOperation("Загрузка пользователей", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при загрузке пользователей: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Загрузка пользователей", false, ex.Message);
                Console.WriteLine($"Нет доступа для загрузки пользователей: {ex.Message}");
                throw;
            }

            return users;
        }

        public static void SaveScenarios(List<AutomationScenario> scenarios)
        {
            try
            {
                using (FileStream fs = new FileStream(SCENARIOS_FILE, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter file = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var scenario in scenarios)
                    {
                        file.WriteLine(scenario.Serialize());
                    }
                }
                LogOperation("Сохранение сценариев", true, $"Сохранено {scenarios.Count} сценариев");
            }
            catch (IOException ex)
            {
                LogOperation("Сохранение сценариев", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при сохранении сценариев: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Сохранение сценариев", false, ex.Message);
                Console.WriteLine($"Нет доступа для сохранения сценариев: {ex.Message}");
                throw;
            }
        }

        public static List<AutomationScenario> LoadScenarios()
        {
            List<AutomationScenario> scenarios = new List<AutomationScenario>();

            if (!File.Exists(SCENARIOS_FILE))
            {
                LogOperation("Загрузка сценариев", true, "Файл не существует, возвращен пустой список");
                return scenarios;
            }

            try
            {
                using (FileStream fs = new FileStream(SCENARIOS_FILE, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader file = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    int loadedCount = 0;

                    while ((line = file.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            try
                            {
                                var scenario = AutomationScenario.Deserialize(line);
                                if (scenario != null)
                                {
                                    scenarios.Add(scenario);
                                    loadedCount++;
                                }
                            }
                            catch (FormatException ex)
                            {
                                Console.WriteLine($"Ошибка формата данных сценария: {ex.Message}");
                                continue;
                            }
                        }
                    }

                    LogOperation("Загрузка сценариев", true, $"Загружено {loadedCount} сценариев");
                }
            }
            catch (IOException ex)
            {
                LogOperation("Загрузка сценариев", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при загрузке сценариев: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Загрузка сценариев", false, ex.Message);
                Console.WriteLine($"Нет доступа для загрузки сценариев: {ex.Message}");
                throw;
            }

            return scenarios;
        }

        public static void SaveNotifications(List<Notification> notifications)
        {
            try
            {
                using (FileStream fs = new FileStream(NOTIFICATIONS_FILE, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter file = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var notification in notifications)
                    {
                        file.WriteLine(notification.Serialize());
                    }
                }
                LogOperation("Сохранение уведомлений", true, $"Сохранено {notifications.Count} уведомлений");
            }
            catch (IOException ex)
            {
                LogOperation("Сохранение уведомлений", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при сохранении уведомлений: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Сохранение уведомлений", false, ex.Message);
                Console.WriteLine($"Нет доступа для сохранения уведомлений: {ex.Message}");
                throw;
            }
        }

        public static List<Notification> LoadNotifications()
        {
            List<Notification> notifications = new List<Notification>();

            if (!File.Exists(NOTIFICATIONS_FILE))
            {
                LogOperation("Загрузка уведомлений", true, "Файл не существует, возвращен пустой список");
                return notifications;
            }

            try
            {
                using (FileStream fs = new FileStream(NOTIFICATIONS_FILE, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader file = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    int loadedCount = 0;

                    while ((line = file.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            try
                            {
                                var notification = Notification.Deserialize(line);
                                if (notification != null)
                                {
                                    notifications.Add(notification);
                                    loadedCount++;
                                }
                            }
                            catch (FormatException ex)
                            {
                                Console.WriteLine($"Ошибка формата данных уведомления: {ex.Message}");
                                continue;
                            }
                        }
                    }

                    LogOperation("Загрузка уведомлений", true, $"Загружено {loadedCount} уведомлений");
                }
            }
            catch (IOException ex)
            {
                LogOperation("Загрузка уведомлений", false, ex.Message);
                Console.WriteLine($"Ошибка ввода-вывода при загрузке уведомлений: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogOperation("Загрузка уведомлений", false, ex.Message);
                Console.WriteLine($"Нет доступа для загрузки уведомлений: {ex.Message}");
                throw;
            }

            return notifications;
        }

        public static bool CheckDataFiles()
        {
            string[] files = { ROOMS_FILE, DEVICES_FILE, USERS_FILE, SCENARIOS_FILE, NOTIFICATIONS_FILE };

            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Файл данных не найден: {file}");
                    return false;
                }
            }

            return true;
        }

        public static void CreateBackup()
        {
            string backupFolder = "Backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            Directory.CreateDirectory(backupFolder);

            string[] files = { ROOMS_FILE, DEVICES_FILE, USERS_FILE, SCENARIOS_FILE, NOTIFICATIONS_FILE };

            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        string backupFile = Path.Combine(backupFolder, Path.GetFileName(file));
                        File.Copy(file, backupFile);
                        Console.WriteLine($"Создана резервная копия: {backupFile}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при создании резервной копии файла {file}: {ex.Message}");
                    }
                }
            }

            LogOperation("Создание резервной копии", true, $"Создана папка резервной копии: {backupFolder}");
        }

        // УДАЛЕН метод InitializeDefaultData - больше не создаем начальные данные

        // Новый метод для создания начальных данных только если это нужно
        public static void CreateInitialUserIfNeeded(List<User> users)
        {
            // Создаем только одного пользователя admin, если нет пользователей
            if (users.Count == 0)
            {
                try
                {
                    User adminUser = new User(
                        "USR001",
                        "admin",
                        "admin123", // Пароль по умолчанию
                        AccessLevel.ADMIN,
                        "admin@smarthome.com",
                        "+79001234567"
                    );
                    users.Add(adminUser);

                    LogOperation("Создание начального пользователя", true, "Создан пользователь admin с паролем admin123");
                    Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                    Console.WriteLine("Создан пользователь по умолчанию:");
                    Console.WriteLine("Логин: admin");
                    Console.WriteLine("Пароль: admin123");
                    Console.WriteLine("═══════════════════════════════════════════════════════════\n");
                }
                catch (Exception ex)
                {
                    LogOperation("Создание начального пользователя", false, ex.Message);
                    Console.WriteLine($"Ошибка при создании начального пользователя: {ex.Message}");
                }
            }
        }
    }
}