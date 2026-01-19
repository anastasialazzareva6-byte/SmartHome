using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeSystem
{
    public class SmartHomeSystem
    {
        private List<Room> rooms;
        private List<Device> devices;
        private List<User> users;
        private List<AutomationScenario> scenarios;
        private List<Notification> notifications;
        private List<EnergyReport> reports;
        private User currentUser;

        // Статическое поле для хранения общего количества сессий
        private static int totalSessions = 0;

        // Статическое поле для хранения системных настроек
        private static Dictionary<string, string> systemSettings = new Dictionary<string, string>
        {
            {"AutoSaveInterval", "300"}, // секунды
            {"MaxFailedLogins", "3"},
            {"EnableLogging", "true"},
            {"BackupOnExit", "false"},
            {"DefaultLanguage", "ru"}
        };

        // Статический метод для получения системных настроек
        public static Dictionary<string, string> GetSystemSettings()
        {
            return new Dictionary<string, string>(systemSettings);
        }

        // Статический метод для изменения системной настройки
        public static void SetSystemSetting(string key, string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Ключ настройки не может быть пустым", nameof(key));

                if (systemSettings.ContainsKey(key))
                {
                    systemSettings[key] = value;
                    Console.WriteLine($"Настройка '{key}' изменена на '{value}'");
                }
                else
                {
                    throw new KeyNotFoundException($"Настройка с ключом '{key}' не найдена");
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка аргумента: {ex.Message}");
                throw;
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                throw;
            }
        }

        // Статическое свойство для получения общего количества сессий
        public static int TotalSessions
        {
            get { return totalSessions; }
            private set { totalSessions = value; }
        }

        // Статический метод для получения статистики системы
        public static string GetSystemStatistics()
        {
            try
            {
                return $"Общее количество сессий: {TotalSessions}\n" +
                       $"Текущее время: {DateTime.Now}\n" +
                       $"Версия системы: 1.0.0\n" +
                       $"Настроек системы: {systemSettings.Count}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении статистики: {ex.Message}");
                throw;
            }
        }

        public SmartHomeSystem()
        {
            try
            {
                rooms = new List<Room>();
                devices = new List<Device>();
                users = new List<User>();
                scenarios = new List<AutomationScenario>();
                notifications = new List<Notification>();
                reports = new List<EnergyReport>();
                currentUser = null;

                // Увеличиваем счетчик сессий
                TotalSessions++;
                Console.WriteLine($"Сессия #{TotalSessions} запущена");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при инициализации системы: {ex.Message}");
                throw;
            }
        }

        // Свойство для доступа к текущему пользователю с проверкой
        public User CurrentUser
        {
            get
            {
                if (currentUser == null)
                    throw new InvalidOperationException("Пользователь не авторизован");
                return currentUser;
            }
            private set { currentUser = value; }
        }

        // Свойство для получения количества устройств
        public int DeviceCount
        {
            get { return devices.Count; }
        }

        // Свойство для получения количества комнат
        public int RoomCount
        {
            get { return rooms.Count; }
        }

        // Свойство для получения количества активных устройств
        public int ActiveDeviceCount
        {
            get
            {
                try
                {
                    return devices.Count(d => d.IsOn);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при подсчете активных устройств: {ex.Message}");
                    return 0;
                }
            }
        }

        public void LoadData()
        {
            Console.WriteLine("Загрузка данных...");

            try
            {
                // Загрузка комнат
                rooms = DataManager.LoadRooms();

                // Загрузка устройств (требует загруженные комнаты)
                devices = DataManager.LoadDevices(rooms);

                // Загрузка пользователей
                users = DataManager.LoadUsers();

                // Загрузка сценариев
                scenarios = DataManager.LoadScenarios();

                // Загрузка уведомлений
                notifications = DataManager.LoadNotifications();

                // Если данных нет, создаем начальные данные
                if (rooms.Count == 0 && devices.Count == 0 && users.Count == 0)
                {
                    Console.WriteLine("Создание начальных данных...");
                    DataManager.InitializeDefaultData(rooms, devices, users, notifications, scenarios);
                    SaveData();
                }

                // Создаем уведомление о загрузке
                string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                notifications.Add(new Notification(notificationId, NotificationType.INFO,
                    $"Данные загружены: {rooms.Count} комнат, {devices.Count} устройств, {users.Count} пользователей"));
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка ввода-вывода при загрузке данных: {ex.Message}");
                notifications.Add(new Notification("ERR001", NotificationType.ALERT,
                    $"Ошибка загрузки данных: {ex.Message}"));
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Нет доступа к файлам данных: {ex.Message}");
                notifications.Add(new Notification("ERR002", NotificationType.ALERT,
                    $"Отсутствуют права доступа к файлам данных"));
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Непредвиденная ошибка при загрузке данных: {ex.Message}");
                notifications.Add(new Notification("ERR003", NotificationType.ALERT,
                    $"Критическая ошибка при загрузке данных"));
                throw;
            }
        }

        public void SaveData()
        {
            try
            {
                Console.WriteLine("Сохранение данных...");

                // Проверяем наличие данных для сохранения
                if (rooms == null || devices == null || users == null)
                {
                    throw new InvalidOperationException("Данные не инициализированы для сохранения");
                }

                DataManager.SaveRooms(rooms);
                DataManager.SaveDevices(devices);
                DataManager.SaveUsers(users);
                DataManager.SaveScenarios(scenarios);
                DataManager.SaveNotifications(notifications);

                Console.WriteLine("Данные сохранены!");

                // Добавляем уведомление о сохранении
                string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                notifications.Add(new Notification(notificationId, NotificationType.INFO,
                    $"Данные сохранены: {rooms.Count} комнат, {devices.Count} устройств"));
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка ввода-вывода при сохранении данных: {ex.Message}");

                // Пытаемся создать резервную копию
                try
                {
                    DataManager.CreateBackup();
                    Console.WriteLine("Создана резервная копия данных");
                }
                catch (Exception backupEx)
                {
                    Console.WriteLine($"Не удалось создать резервную копию: {backupEx.Message}");
                }

                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Нет доступа для сохранения данных: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка операции: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Непредвиденная ошибка при сохранении данных: {ex.Message}");
                throw;
            }
        }

        private bool HasAdminAccess()
        {
            try
            {
                if (currentUser == null)
                {
                    throw new UnauthorizedAccessException("Необходимо войти в систему!");
                }

                if (currentUser.AccessLevel != AccessLevel.ADMIN)
                {
                    throw new UnauthorizedAccessException("Недостаточно прав! Требуются права администратора.");
                }

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Ошибка доступа: {ex.Message}");
                return false;
            }
        }

        private bool HasUserAccess()
        {
            try
            {
                if (currentUser == null)
                {
                    throw new UnauthorizedAccessException("Необходимо войти в систему!");
                }

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Ошибка доступа: {ex.Message}");
                return false;
            }
        }

        // Метод для безопасного выполнения операции с проверкой прав
        private bool ExecuteWithAccessCheck(Action action, bool requireAdmin = false)
        {
            try
            {
                if (requireAdmin && !HasAdminAccess())
                    return false;

                if (!requireAdmin && !HasUserAccess())
                    return false;

                action();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                return false;
            }
        }

        // Перегруженный метод для операций с возвращаемым значением
        private T ExecuteWithAccessCheck<T>(Func<T> func, bool requireAdmin = false, T defaultValue = default)
        {
            try
            {
                if (requireAdmin && !HasAdminAccess())
                    return defaultValue;

                if (!requireAdmin && !HasUserAccess())
                    return defaultValue;

                return func();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                return defaultValue;
            }
        }

        public void DisplayMainMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("               УМНЫЙ ДОМ - ГЛАВНОЕ МЕНЮ");
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

            if (currentUser != null)
            {
                Console.WriteLine($"Текущий пользователь: {currentUser.Username}");
                Console.WriteLine($"Уровень доступа: {(currentUser.AccessLevel == AccessLevel.ADMIN ? "Администратор" : "Пользователь")}");
                Console.WriteLine(new string('─', 60));
            }

            Console.WriteLine("1. Управление устройствами");
            Console.WriteLine("2. Управление комнатами");
            Console.WriteLine("3. Управление пользователями" +
                (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN ? " (Админ)" : ""));
            Console.WriteLine("4. Сценарии автоматизации");
            Console.WriteLine("5. Уведомления");
            Console.WriteLine("6. Энергоотчеты");
            Console.WriteLine("7. Статус системы");
            Console.WriteLine("8. Настройки системы");

            if (currentUser == null)
            {
                Console.WriteLine("9. Войти в систему");
            }
            else
            {
                Console.WriteLine("9. Выйти из системы");
            }

            Console.WriteLine("10. Сохранить данные");
            Console.WriteLine("11. Создать резервную копию");
            Console.WriteLine("12. Проверить подключение устройств");
            Console.WriteLine("13. Отчет по безопасности");
            Console.WriteLine("0. Выход");
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
            Console.Write("Выберите опцию: ");
        }

        private void DisplayDevicesMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("               УПРАВЛЕНИЕ УСТРОЙСТВАМИ");
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("1. Список всех устройств");
            Console.WriteLine("2. Включить/Выключить устройство");

            if (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN)
            {
                Console.WriteLine("3. Добавить устройство");
                Console.WriteLine("4. Удалить устройство");
                Console.WriteLine("5. Назад в главное меню");
            }
            else
            {
                Console.WriteLine("3. Назад в главное меню");
            }
            Console.Write("Выберите опцию: ");
        }

        private void DisplayRoomsMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("               УПРАВЛЕНИЕ КОМНАТАМИ");
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("1. Список всех комнат");
            Console.WriteLine("2. Показать устройства в комнате");
            Console.WriteLine("3. Потребление энергии комнаты");

            if (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN)
            {
                Console.WriteLine("4. Добавить комнату");
                Console.WriteLine("5. Удалить комнату");
                Console.WriteLine("6. Назад в главное меню");
            }
            else
            {
                Console.WriteLine("4. Назад в главное меню");
            }
            Console.Write("Выберите опцию: ");
        }

        private void DisplayUsersMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("               УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ (АДМИН)");
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("1. Список пользователей");
            Console.WriteLine("2. Добавить пользователя");
            Console.WriteLine("3. Удалить пользователя");
            Console.WriteLine("4. Изменить права доступа");
            Console.WriteLine("5. Назад в главное меню");
            Console.Write("Выберите опцию: ");
        }

        private void DisplayScenariosMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("               СЦЕНАРИИ АВТОМАТИЗАЦИИ");
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("1. Список сценариев");
            Console.WriteLine("2. Создать сценарий");
            Console.WriteLine("3. Выполнить сценарий");
            Console.WriteLine("4. Показать действия сценария");
            Console.WriteLine("5. Добавить действие в сценарий");
            Console.WriteLine("6. Назад в главное меню");
            Console.Write("Выберите опцию: ");
        }

        private void DisplayNotificationsMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                     УВЕДОМЛЕНИЯ");
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("1. Показать все уведомления");
            Console.WriteLine("2. Показать непрочитанные");
            Console.WriteLine("3. Отметить как прочитанное");
            Console.WriteLine("4. Назад в главное меню");
            Console.Write("Выберите опцию: ");
        }

        private void DisplaySettingsMenu()
        {
            try
            {
                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("               НАСТРОЙКИ СИСТЕМЫ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                if (!HasAdminAccess()) return;

                var settings = GetSystemSettings();
                int index = 1;

                foreach (var setting in settings)
                {
                    Console.WriteLine($"{index}. {setting.Key}: {setting.Value}");
                    index++;
                }

                Console.WriteLine($"{index}. Статистика системы");
                Console.WriteLine($"{index + 1}. Назад в главное меню");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
                Console.Write("Выберите опцию: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice > 0 && choice <= settings.Count)
                    {
                        var key = settings.Keys.ElementAt(choice - 1);
                        Console.Write($"Введите новое значение для '{key}': ");
                        string value = Console.ReadLine();

                        try
                        {
                            SetSystemSetting(key, value);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Не удалось изменить настройку: {ex.Message}");
                        }
                    }
                    else if (choice == settings.Count + 1)
                    {
                        Console.WriteLine(GetSystemStatistics());
                    }
                    else if (choice == settings.Count + 2)
                    {
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Неверная опция!");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в меню настроек: {ex.Message}");
            }
        }

        private void ListAllDevices()
        {
            try
            {
                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("                    ВСЕ УСТРОЙСТВА");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                if (devices.Count == 0)
                {
                    Console.WriteLine("Устройств не найдено.");
                    return;
                }

                Console.WriteLine("┌────┬────────────────────────┬──────────────────┬─────────────┬────────────────┬────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Тип              │ Статус      │ Комната        │ Потребление    │");
                Console.WriteLine("├────┼────────────────────────┼──────────────────┼─────────────┼────────────────┼────────────────┤");

                for (int i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];
                    Console.WriteLine($"│ {i + 1,2} │ {device.Name,-23} │ " +
                        $"{device.GetDeviceTypeString(),-16} │ " +
                        $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                        $"{(device.Location != null ? device.Location.Name : "Нет"),-14} │ " +
                        $"{device.PowerConsumption,10:F1} Вт │");
                }
                Console.WriteLine("└────┴────────────────────────┴──────────────────┴─────────────┴────────────────┴────────────────┘");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении устройств: {ex.Message}");
            }
        }

        private void ToggleDevice()
        {
            try
            {
                ListAllDevices();
                Console.Write("Выберите номер устройства: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= devices.Count)
                {
                    Device device = devices[choice - 1];
                    if (device.IsOn)
                    {
                        device.TurnOff();
                    }
                    else
                    {
                        device.TurnOn();
                    }
                    SaveData();
                }
                else
                {
                    throw new ArgumentException("Неверный номер устройства!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при переключении устройства: {ex.Message}");
            }
        }

        private void AddDevice()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.WriteLine("Добавление нового устройства:");
                Console.Write("Название: ");
                string name = Console.ReadLine();

                Console.Write("Производитель: ");
                string manufacturer = Console.ReadLine();

                Console.Write("Тип устройства (1-Датчик, 2-Исполнительное, 3-Климат, 4-Безопасность, 5-Мультимедиа): ");
                if (int.TryParse(Console.ReadLine(), out int typeChoice) && typeChoice >= 1 && typeChoice <= 5)
                {
                    DeviceType type = (DeviceType)(typeChoice - 1);

                    Console.Write("Энергопотребление (Вт): ");
                    if (double.TryParse(Console.ReadLine(), out double power))
                    {
                        ListAllRooms();
                        Console.Write("Выберите номер комнаты: ");

                        if (int.TryParse(Console.ReadLine(), out int roomChoice) && roomChoice > 0 && roomChoice <= rooms.Count)
                        {
                            Room room = rooms[roomChoice - 1];
                            Device newDevice = new Device(
                                "DEV" + (devices.Count + 1).ToString("D3"),
                                name,
                                manufacturer,
                                type,
                                room,
                                power
                            );
                            devices.Add(newDevice);
                            room.AddDevice(newDevice);
                            Console.WriteLine($"Устройство добавлено в комнату '{room.Name}'!");
                            SaveData();

                            // Добавляем уведомление
                            string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                            notifications.Add(new Notification(notificationId, NotificationType.INFO,
                                $"Добавлено новое устройство: {name}", newDevice));
                            return;
                        }
                    }
                }

                Console.WriteLine("Неверный ввод!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении устройства: {ex.Message}");
            }
        }

        private void DeleteDevice()
        {
            if (!HasAdminAccess()) return;

            try
            {
                ListAllDevices();
                Console.Write("Выберите номер устройства для удаления: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= devices.Count)
                {
                    Device device = devices[choice - 1];
                    string deviceName = device.Name;

                    // Удаляем устройство из комнаты
                    if (device.Location != null)
                    {
                        device.Location.RemoveDevice(device);
                    }

                    // Удаляем устройство из списка
                    devices.RemoveAt(choice - 1);
                    Console.WriteLine("Устройство удалено!");
                    SaveData();

                    // Добавляем уведомление
                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.WARNING,
                        $"Удалено устройство: {deviceName}", device));
                }
                else
                {
                    throw new ArgumentException("Неверный номер устройства!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении устройства: {ex.Message}");
            }
        }

        private void ListAllRooms()
        {
            try
            {
                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("                      ВСЕ КОМНАТЫ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                if (rooms.Count == 0)
                {
                    Console.WriteLine("Комнат не найдено.");
                    return;
                }

                Console.WriteLine("┌────┬────────────────────────┬────────────────┬──────────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Площадь (м²)   │ Устройств            │");
                Console.WriteLine("├────┼────────────────────────┼────────────────┼──────────────────────┤");

                for (int i = 0; i < rooms.Count; i++)
                {
                    Console.WriteLine($"│ {i + 1,2} │ {rooms[i].Name,-23} │ " +
                        $"{rooms[i].Area,14:F1} │ {rooms[i].GetDevices().Count,20} │");
                }
                Console.WriteLine("└────┴────────────────────────┴────────────────┴──────────────────────┘");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении комнат: {ex.Message}");
            }
        }

        private void ShowRoomDevices()
        {
            try
            {
                ListAllRooms();
                Console.Write("Выберите номер комнаты: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
                {
                    rooms[choice - 1].DisplayDevices();
                }
                else
                {
                    throw new ArgumentException("Неверный номер комнаты!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении устройств комнаты: {ex.Message}");
            }
        }

        private void ShowRoomPower()
        {
            try
            {
                ListAllRooms();
                Console.Write("Выберите номер комнаты: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
                {
                    double power = rooms[choice - 1].CalculateRoomPowerConsumption();
                    Console.WriteLine($"Текущее потребление энергии: {power:F1} Вт");

                    // Показываем какие устройства включены и их потребление
                    var roomDevices = rooms[choice - 1].GetDevices();
                    int activeDevices = 0;
                    foreach (var device in roomDevices)
                    {
                        if (device.IsOn)
                        {
                            activeDevices++;
                            Console.WriteLine($"  - {device.Name}: {device.PowerConsumption:F1} Вт");
                        }
                    }
                    Console.WriteLine($"Включено устройств: {activeDevices} из {roomDevices.Count}");
                }
                else
                {
                    throw new ArgumentException("Неверный номер комнаты!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при расчете потребления комнаты: {ex.Message}");
            }
        }

        private void AddRoom()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.WriteLine("Добавление новой комнаты:");
                Console.Write("Название: ");
                string name = Console.ReadLine();

                Console.Write("Площадь (м^2): ");
                if (double.TryParse(Console.ReadLine(), out double area))
                {
                    Room newRoom = new Room(
                        "ROOM" + (rooms.Count + 1).ToString("D3"),
                        name,
                        area
                    );
                    rooms.Add(newRoom);
                    Console.WriteLine("Комната добавлена!");
                    SaveData();

                    // Добавляем уведомление
                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Добавлена новая комната: {name}", null, null));
                }
                else
                {
                    throw new FormatException("Неверный ввод площади!");
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении комнаты: {ex.Message}");
            }
        }

        private void DeleteRoom()
        {
            if (!HasAdminAccess()) return;

            try
            {
                ListAllRooms();
                Console.Write("Выберите номер комнаты для удаления: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
                {
                    // Проверяем, есть ли устройства в комнате
                    if (rooms[choice - 1].GetDevices().Count > 0)
                    {
                        throw new InvalidOperationException("В комнате есть устройства! Сначала удалите или переместите их.");
                    }

                    string roomName = rooms[choice - 1].Name;
                    rooms.RemoveAt(choice - 1);
                    Console.WriteLine("Комната удалена!");
                    SaveData();

                    // Добавляем уведомление
                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.WARNING,
                        $"Удалена комната: {roomName}", null, null));
                }
                else
                {
                    throw new ArgumentException("Неверный номер комнаты!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка операции: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении комнаты: {ex.Message}");
            }
        }

        private void ListUsers()
        {
            try
            {
                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("                    ВСЕ ПОЛЬЗОВАТЕЛИ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                for (int i = 0; i < users.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. ");
                    users[i].DisplayInfo();
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении пользователей: {ex.Message}");
            }
        }

        private void AddUser()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.WriteLine("Добавление нового пользователя:");
                Console.Write("Логин: ");
                string username = Console.ReadLine();

                Console.Write("Пароль: ");
                string password = Console.ReadLine();

                Console.Write("Уровень доступа (1 - User, 2 - Admin): ");
                if (int.TryParse(Console.ReadLine(), out int level) && (level == 1 || level == 2))
                {
                    User newUser = new User(
                        "USR" + (users.Count + 1).ToString("D3"),
                        username,
                        password,
                        level == 2 ? AccessLevel.ADMIN : AccessLevel.USER,
                        "",
                        ""
                    );
                    users.Add(newUser);
                    Console.WriteLine("Пользователь добавлен!");
                    SaveData();

                    // Добавляем уведомление
                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Добавлен новый пользователь: {username}", null, null));
                }
                else
                {
                    throw new FormatException("Неверный уровень доступа!");
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении пользователя: {ex.Message}");
            }
        }

        private void DeleteUser()
        {
            if (!HasAdminAccess()) return;

            try
            {
                ListUsers();
                Console.Write("Выберите номер пользователя для удаления: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= users.Count)
                {
                    // Нельзя удалить текущего пользователя
                    if (users[choice - 1] == currentUser)
                    {
                        throw new InvalidOperationException("Нельзя удалить текущего пользователя!");
                    }

                    string userName = users[choice - 1].Username;
                    users.RemoveAt(choice - 1);
                    Console.WriteLine("Пользователь удален!");
                    SaveData();

                    // Добавляем уведомление
                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.WARNING,
                        $"Удален пользователь: {userName}", null, null));
                }
                else
                {
                    throw new ArgumentException("Неверный номер пользователя!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка операции: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
            }
        }

        private void ChangeUserAccess()
        {
            if (!HasAdminAccess()) return;

            try
            {
                ListUsers();
                Console.Write("Выберите номер пользователя: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= users.Count)
                {
                    Console.Write("Новый уровень доступа (1 - User, 2 - Admin): ");
                    if (int.TryParse(Console.ReadLine(), out int level) && (level == 1 || level == 2))
                    {
                        string oldLevel = users[choice - 1].AccessLevel.ToString();
                        users[choice - 1].SetAccessLevel(level == 2 ? AccessLevel.ADMIN : AccessLevel.USER);
                        string newLevel = users[choice - 1].AccessLevel.ToString();

                        Console.WriteLine("Права доступа изменены!");
                        SaveData();

                        // Добавляем уведомление
                        string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                        notifications.Add(new Notification(notificationId, NotificationType.INFO,
                            $"Изменены права доступа пользователя {users[choice - 1].Username}: {oldLevel} -> {newLevel}", null, null));
                    }
                    else
                    {
                        throw new FormatException("Неверный уровень доступа!");
                    }
                }
                else
                {
                    throw new ArgumentException("Неверный номер пользователя!");
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при изменении прав доступа: {ex.Message}");
            }
        }

        private void ShowAllNotifications()
        {
            try
            {
                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("                    ВСЕ УВЕДОМЛЕНИЯ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                if (notifications.Count == 0)
                {
                    Console.WriteLine("Уведомлений нет.");
                    return;
                }

                for (int i = 0; i < notifications.Count; i++)
                {
                    Console.Write($"{i + 1}. ");
                    notifications[i].DisplayInfo();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении уведомлений: {ex.Message}");
            }
        }

        private void ShowUnreadNotifications()
        {
            try
            {
                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("               НЕПРОЧИТАННЫЕ УВЕДОМЛЕНИЯ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                bool hasUnread = false;
                for (int i = 0; i < notifications.Count; i++)
                {
                    if (!notifications[i].IsRead)
                    {
                        Console.Write($"{i + 1}. ");
                        notifications[i].DisplayInfo();
                        hasUnread = true;
                    }
                }

                if (!hasUnread)
                {
                    Console.WriteLine("Нет непрочитанных уведомлений.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении непрочитанных уведомлений: {ex.Message}");
            }
        }

        private void MarkNotificationAsRead()
        {
            try
            {
                ShowAllNotifications();
                Console.Write("Выберите номер уведомления: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= notifications.Count)
                {
                    notifications[choice - 1].MarkAsRead();
                    SaveData();
                }
                else
                {
                    throw new ArgumentException("Неверный номер уведомления!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отметке уведомления: {ex.Message}");
            }
        }

        private void CreateEnergyReport()
        {
            try
            {
                EnergyReport report = new EnergyReport(
                    "ОТЧЕТ" + (reports.Count + 1).ToString("D3"),
                    DateTime.Now.AddDays(-1),
                    DateTime.Now
                );

                // Добавляем потребление только включенных устройств
                foreach (var device in devices)
                {
                    if (device.IsOn)
                    {
                        report.AddDeviceConsumption(device, device.PowerConsumption * 24);
                    }
                }

                report.GenerateReport();
                reports.Add(report);
                report.DisplayReport();

                // Добавляем уведомление
                string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                notifications.Add(new Notification(notificationId, NotificationType.INFO,
                    $"Создан новый отчет по энергопотреблению: {report.ReportId}", null, null));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании отчета по энергопотреблению: {ex.Message}");
            }
        }

        private void SystemStatus()
        {
            try
            {
                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("                    СТАТУС СИСТЕМЫ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine($"Комнат: {rooms.Count}");
                Console.WriteLine($"Устройств: {devices.Count}");
                Console.WriteLine($"Пользователей: {users.Count}");
                Console.WriteLine($"Сценариев: {scenarios.Count}");
                Console.WriteLine($"Уведомлений: {notifications.Count}");
                Console.WriteLine($"Отчетов по энергии: {reports.Count}");

                int activeDevices = 0;
                double totalPower = 0.0;
                foreach (var device in devices)
                {
                    if (device.IsOn)
                    {
                        activeDevices++;
                        totalPower += device.PowerConsumption;
                    }
                }

                Console.WriteLine($"Активных устройств: {activeDevices}/{devices.Count}");
                Console.WriteLine($"Текущее потребление: {totalPower:F1} Вт");
                Console.WriteLine($"Всего сессий системы: {TotalSessions}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении статуса системы: {ex.Message}");
            }
        }

        private void Login()
        {
            try
            {
                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("                     ВХОД В СИСТЕМУ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
                Console.Write("Имя пользователя: ");
                string username = Console.ReadLine();

                Console.Write("Пароль: ");
                string password = Console.ReadLine();

                foreach (var user in users)
                {
                    if (user.Username == username && user.Login(password))
                    {
                        currentUser = user;
                        Console.WriteLine($"Вход выполнен успешно! Добро пожаловать, {username}!");

                        // Добавляем уведомление
                        string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                        notifications.Add(new Notification(notificationId, NotificationType.INFO,
                            $"Пользователь {username} вошел в систему", null, null));
                        return;
                    }
                }

                Console.WriteLine("Ошибка входа!");
                throw new UnauthorizedAccessException("Неверное имя пользователя или пароль");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Ошибка авторизации: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при входе в систему: {ex.Message}");
            }
        }

        private void Logout()
        {
            try
            {
                if (currentUser != null)
                {
                    string username = currentUser.Username;
                    currentUser.Logout();
                    Console.WriteLine($"Пользователь {username} вышел из системы.");

                    // Добавляем уведомление
                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Пользователь {username} вышел из системы", null, null));

                    currentUser = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выходе из системы: {ex.Message}");
            }
        }

        private void DeviceManagement()
        {
            int choice = -1;
            do
            {
                try
                {
                    DisplayDevicesMenu();
                    if (int.TryParse(Console.ReadLine(), out choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                ListAllDevices();
                                break;
                            case 2:
                                ToggleDevice();
                                break;
                            case 3:
                                if (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN)
                                {
                                    AddDevice();
                                }
                                else
                                {
                                    return;
                                }
                                break;
                            case 4:
                                if (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN)
                                {
                                    DeleteDevice();
                                }
                                else
                                {
                                    return;
                                }
                                break;
                            case 5:
                                return;
                            default:
                                Console.WriteLine("Неверная опция!");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления устройствами: {ex.Message}");
                }
            } while (choice != (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN ? 5 : 3));
        }

        private void RoomManagement()
        {
            int choice = -1;
            do
            {
                try
                {
                    DisplayRoomsMenu();
                    if (int.TryParse(Console.ReadLine(), out choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                ListAllRooms();
                                break;
                            case 2:
                                ShowRoomDevices();
                                break;
                            case 3:
                                ShowRoomPower();
                                break;
                            case 4:
                                if (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN)
                                {
                                    AddRoom();
                                }
                                else
                                {
                                    return;
                                }
                                break;
                            case 5:
                                if (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN)
                                {
                                    DeleteRoom();
                                }
                                else
                                {
                                    return;
                                }
                                break;
                            case 6:
                                return;
                            default:
                                Console.WriteLine("Неверная опция!");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления комнатами: {ex.Message}");
                }
            } while (choice != (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN ? 6 : 4));
        }

        private void UserManagement()
        {
            if (!HasAdminAccess()) return;

            int choice = -1;
            do
            {
                try
                {
                    DisplayUsersMenu();
                    if (int.TryParse(Console.ReadLine(), out choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                ListUsers();
                                break;
                            case 2:
                                AddUser();
                                break;
                            case 3:
                                DeleteUser();
                                break;
                            case 4:
                                ChangeUserAccess();
                                break;
                            case 5:
                                return;
                            default:
                                Console.WriteLine("Неверная опция!");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления пользователями: {ex.Message}");
                }
            } while (choice != 5);
        }

        private void ScenarioManagement()
        {
            if (!HasUserAccess()) return;

            int choice = -1;
            do
            {
                try
                {
                    DisplayScenariosMenu();
                    if (int.TryParse(Console.ReadLine(), out choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                if (scenarios.Count == 0)
                                {
                                    Console.WriteLine("Сценарии еще не созданы!");
                                }
                                else
                                {
                                    foreach (var scenario in scenarios)
                                    {
                                        scenario.DisplayInfo();
                                    }
                                }
                                break;
                            case 2:
                                CreateScenario();
                                break;
                            case 3:
                                ExecuteScenario();
                                break;
                            case 4:
                                ShowScenarioActions();
                                break;
                            case 5:
                                AddActionToScenario();
                                break;
                            case 6:
                                return;
                            default:
                                Console.WriteLine("Неверная опция!");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления сценариями: {ex.Message}");
                }
            } while (choice != 6);
        }

        private void NotificationManagement()
        {
            if (!HasUserAccess()) return;

            int choice = -1;
            do
            {
                try
                {
                    DisplayNotificationsMenu();
                    if (int.TryParse(Console.ReadLine(), out choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                ShowAllNotifications();
                                break;
                            case 2:
                                ShowUnreadNotifications();
                                break;
                            case 3:
                                MarkNotificationAsRead();
                                break;
                            case 4:
                                return;
                            default:
                                Console.WriteLine("Неверная опция!");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления уведомлениями: {ex.Message}");
                }
            } while (choice != 4);
        }

        private void CreateScenario()
        {
            try
            {
                Console.WriteLine("Создание нового сценария:");
                Console.Write("Название: ");
                string name = Console.ReadLine();

                Console.Write("Время запуска (например, 07:00): ");
                string time = Console.ReadLine();

                AutomationScenario scenario = new AutomationScenario(
                    "SCN" + (scenarios.Count + 1).ToString("D3"),
                    name,
                    time
                );
                scenarios.Add(scenario);
                Console.WriteLine("Сценарий создан! Теперь добавьте действия.");
                AddActionToScenario(scenario);
                SaveData();

                // Добавляем уведомление
                string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                notifications.Add(new Notification(notificationId, NotificationType.INFO,
                    $"Создан новый сценарий: {name}", null, scenario));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании сценария: {ex.Message}");
            }
        }

        private void ExecuteScenario()
        {
            try
            {
                if (scenarios.Count == 0)
                {
                    Console.WriteLine("Нет доступных сценариев!");
                    return;
                }

                Console.WriteLine("Доступные сценарии:");
                for (int i = 0; i < scenarios.Count; i++)
                {
                    Console.Write($"{i + 1}. ");
                    scenarios[i].DisplayInfo();
                }

                Console.Write("Выберите номер сценария: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= scenarios.Count)
                {
                    scenarios[choice - 1].Activate();
                    scenarios[choice - 1].Execute();
                    SaveData();

                    // Добавляем уведомление
                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Выполнен сценарий: {scenarios[choice - 1].Name}", null, scenarios[choice - 1]));
                }
                else
                {
                    throw new ArgumentException("Неверный номер сценария!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении сценария: {ex.Message}");
            }
        }

        private void ShowScenarioActions()
        {
            try
            {
                if (scenarios.Count == 0)
                {
                    Console.WriteLine("Нет доступных сценариев!");
                    return;
                }

                Console.WriteLine("Доступные сценарии:");
                for (int i = 0; i < scenarios.Count; i++)
                {
                    Console.Write($"{i + 1}. ");
                    scenarios[i].DisplayInfo();
                }

                Console.Write("Выберите номер сценария: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= scenarios.Count)
                {
                    scenarios[choice - 1].DisplayActions();
                }
                else
                {
                    throw new ArgumentException("Неверный номер сценария!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении действий сценария: {ex.Message}");
            }
        }

        private void AddActionToScenario(AutomationScenario specificScenario = null)
        {
            try
            {
                if (scenarios.Count == 0)
                {
                    Console.WriteLine("Нет доступных сценариев!");
                    return;
                }

                AutomationScenario scenario = specificScenario;
                if (scenario == null)
                {
                    Console.WriteLine("Доступные сценарии:");
                    for (int i = 0; i < scenarios.Count; i++)
                    {
                        Console.Write($"{i + 1}. ");
                        scenarios[i].DisplayInfo();
                    }

                    Console.Write("Выберите номер сценария: ");
                    if (!int.TryParse(Console.ReadLine(), out int choice) || choice <= 0 || choice > scenarios.Count)
                    {
                        throw new ArgumentException("Неверный номер сценария!");
                    }
                    scenario = scenarios[choice - 1];
                }

                Console.WriteLine($"Добавление действия в сценарий '{scenario.Name}':");
                ListAllDevices();
                Console.Write("Выберите номер устройства: ");

                if (int.TryParse(Console.ReadLine(), out int deviceChoice) && deviceChoice > 0 && deviceChoice <= devices.Count)
                {
                    Console.Write("Действие (1 - Включить, 2 - Выключить): ");
                    if (int.TryParse(Console.ReadLine(), out int actionChoice) && (actionChoice == 1 || actionChoice == 2))
                    {
                        string command = actionChoice == 1 ? "turnOn" : "turnOff";
                        ScenarioAction action = new ScenarioAction(
                            "ACT" + (scenario.GetActionCount() + 1).ToString("D3"),
                            devices[deviceChoice - 1],
                            command
                        );
                        scenario.AddAction(action);
                        Console.WriteLine("Действие добавлено в сценарий!");
                        SaveData();
                    }
                    else
                    {
                        throw new ArgumentException("Неверный выбор действия!");
                    }
                }
                else
                {
                    throw new ArgumentException("Неверный номер устройства!");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении действия в сценарий: {ex.Message}");
            }
        }

        public void CheckDeviceConnections()
        {
            try
            {
                if (!HasUserAccess()) return;

                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("           ПРОВЕРКА ПОДКЛЮЧЕНИЯ УСТРОЙСТВ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                int offlineCount = 0;

                foreach (var device in devices)
                {
                    if (!device.IsOnline)
                    {
                        Console.WriteLine($"⚠ Устройство '{device.Name}' отключено от сети");
                        offlineCount++;

                        // Создаем уведомление об отключенном устройстве
                        string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                        notifications.Add(new Notification(notificationId, NotificationType.WARNING,
                            $"Устройство '{device.Name}' отключено от сети", device));
                    }
                }

                if (offlineCount == 0)
                {
                    Console.WriteLine("✓ Все устройства подключены к сети");
                }
                else
                {
                    Console.WriteLine($"⚠ Обнаружено {offlineCount} отключенных устройств");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке подключения устройств: {ex.Message}");
            }
        }

        public void GenerateSecurityReport()
        {
            try
            {
                if (!HasAdminAccess()) return;

                Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("               ОТЧЕТ ПО БЕЗОПАСНОСТИ");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                int adminCount = users.Count(u => u.AccessLevel == AccessLevel.ADMIN);
                int userCount = users.Count(u => u.AccessLevel == AccessLevel.USER);
                int securityDevices = devices.Count(d => d.DeviceType == DeviceType.SECURITY);
                int cameras = devices.Count(d => d.Name.ToLower().Contains("камера"));

                Console.WriteLine($"Администраторов: {adminCount}");
                Console.WriteLine($"Пользователей: {userCount}");
                Console.WriteLine($"Устройств безопасности: {securityDevices}");
                Console.WriteLine($"Камер наблюдения: {cameras}");
                Console.WriteLine($"Всего сессий системы: {TotalSessions}");
                Console.WriteLine($"Текущий пользователь: {(currentUser != null ? currentUser.Username : "Не авторизован")}");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации отчета безопасности: {ex.Message}");
            }
        }

        public void Run()
        {
            try
            {
                Console.WriteLine($"Запуск системы Умный Дом (сессия #{TotalSessions})");
                Console.WriteLine("Версия системы: 1.0.0");
                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");

                LoadData();

                // Требуем вход в систему перед началом работы
                while (currentUser == null)
                {
                    Login();

                    if (currentUser == null)
                    {
                        Console.WriteLine("Для работы с системой необходимо войти в аккаунт!");
                    }
                }

                int choice = -1;
                do
                {
                    try
                    {
                        DisplayMainMenu();
                        if (int.TryParse(Console.ReadLine(), out choice))
                        {
                            switch (choice)
                            {
                                case 1: // Управление устройствами
                                    DeviceManagement();
                                    break;
                                case 2: // Управление комнатами
                                    RoomManagement();
                                    break;
                                case 3: // Управление пользователями - ТОЛЬКО АДМИН
                                    ExecuteWithAccessCheck(() => UserManagement(), true);
                                    break;
                                case 4: // Сценарии автоматизации
                                    ExecuteWithAccessCheck(() => ScenarioManagement(), false);
                                    break;
                                case 5: // Уведомления
                                    ExecuteWithAccessCheck(() => NotificationManagement(), false);
                                    break;
                                case 6: // Энергоотчеты
                                    ExecuteWithAccessCheck(() => CreateEnergyReport(), false);
                                    break;
                                case 7: // Статус системы - доступно всем
                                    SystemStatus();
                                    break;
                                case 8: // Настройки системы
                                    ExecuteWithAccessCheck(() => DisplaySettingsMenu(), true);
                                    break;
                                case 9: // Войти/выйти из системы
                                    if (currentUser == null)
                                    {
                                        Login();
                                    }
                                    else
                                    {
                                        Logout();
                                    }
                                    break;
                                case 10: // Сохранить данные
                                    ExecuteWithAccessCheck(() => SaveData(), true);
                                    break;
                                case 11: // Создать резервную копию
                                    ExecuteWithAccessCheck(() =>
                                    {
                                        Console.WriteLine("Создание резервной копии...");
                                        DataManager.CreateBackup();
                                        Console.WriteLine("Резервная копия создана успешно!");
                                    }, true);
                                    break;
                                case 12: // Проверить подключение устройств
                                    ExecuteWithAccessCheck(() => CheckDeviceConnections(), false);
                                    break;
                                case 13: // Отчет по безопасности
                                    ExecuteWithAccessCheck(() => GenerateSecurityReport(), true);
                                    break;
                                case 0: // Выход
                                    ExecuteWithAccessCheck(() =>
                                    {
                                        Console.WriteLine("Сохранение данных перед выходом...");
                                        SaveData();
                                        Console.WriteLine("До свидания!");
                                    }, false);
                                    break;
                                default:
                                    Console.WriteLine("Неверная опция!");
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Неверный ввод!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка в меню: {ex.Message}");
                    }
                } while (choice != 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка при работе системы: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
            finally
            {
                // Гарантируем сохранение данных при выходе
                if (currentUser != null && bool.Parse(systemSettings["BackupOnExit"]))
                {
                    try
                    {
                        Console.WriteLine("\nСоздание финальной резервной копии...");
                        DataManager.CreateBackup();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Не удалось создать резервную копию: {ex.Message}");
                    }
                }
            }
        }
    }
}