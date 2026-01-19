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

        private static int totalSessions = 0;

        private static Dictionary<string, string> systemSettings = new Dictionary<string, string>
        {
            {"AutoSaveInterval", "300"},
            {"MaxFailedLogins", "3"},
            {"EnableLogging", "true"},
            {"BackupOnExit", "false"},
            {"DefaultLanguage", "ru"}
        };

        public static Dictionary<string, string> GetSystemSettings()
        {
            return new Dictionary<string, string>(systemSettings);
        }

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

        public static int TotalSessions
        {
            get { return totalSessions; }
            private set { totalSessions = value; }
        }

        public static string GetSystemStatistics()
        {
            try
            {
                return $"Общее количество сессий: {TotalSessions}\n" +
                       $"Текущее время: {DateTime.Now}\n" +
                       $"Версия системы: 2.0.0\n" +
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

                TotalSessions++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при инициализации системы: {ex.Message}");
                throw;
            }
        }

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

        public int DeviceCount
        {
            get { return devices.Count; }
        }

        public int RoomCount
        {
            get { return rooms.Count; }
        }

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
            try
            {
                rooms = DataManager.LoadRooms();
                devices = DataManager.LoadDevices(rooms);
                users = DataManager.LoadUsers();
                scenarios = DataManager.LoadScenarios();
                notifications = DataManager.LoadNotifications();

                // Создаем только пользователя admin, если нет пользователей
                if (users.Count == 0)
                {
                    DataManager.CreateInitialUserIfNeeded(users);
                    SaveData(); // Сохраняем созданного пользователя
                }

                // УБРАН вызов AddDemoDevices() - демо устройства не создаются автоматически

                string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                notifications.Add(new Notification(notificationId, NotificationType.INFO,
                    $"Данные загружены: {rooms.Count} комнат, {devices.Count} устройств, {users.Count} пользователей"));
            }
            catch (IOException ex)
            {
                notifications.Add(new Notification("ERR001", NotificationType.ALERT,
                    $"Ошибка загрузки данных: {ex.Message}"));
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                notifications.Add(new Notification("ERR002", NotificationType.ALERT,
                    $"Отсутствуют права доступа к файлам данных"));
                throw;
            }
            catch (Exception ex)
            {
                notifications.Add(new Notification("ERR003", NotificationType.ALERT,
                    $"Критическая ошибка при загрузке данных"));
                throw;
            }
        }

        public void SaveData()
        {
            try
            {
                if (rooms == null || devices == null || users == null)
                {
                    throw new InvalidOperationException("Данные не инициализированы для сохранения");
                }

                DataManager.SaveRooms(rooms);
                DataManager.SaveDevices(devices);
                DataManager.SaveUsers(users);
                DataManager.SaveScenarios(scenarios);
                DataManager.SaveNotifications(notifications);

                string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                notifications.Add(new Notification(notificationId, NotificationType.INFO,
                    $"Данные сохранены: {rooms.Count} комнат, {devices.Count} устройств"));
            }
            catch (IOException ex)
            {
                try
                {
                    DataManager.CreateBackup();
                }
                catch (Exception backupEx)
                {
                }
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
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
            catch (UnauthorizedAccessException)
            {
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
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

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
            Console.Clear();
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("               УМНЫЙ ДОМ - ГЛАВНОЕ МЕНЮ");
            Console.WriteLine("═══════════════════════════════════════════════════════════");

            if (currentUser != null)
            {
                Console.WriteLine($"Текущий пользователь: {currentUser.Username}");
                Console.WriteLine($"Уровень доступа: {(currentUser.AccessLevel == AccessLevel.ADMIN ? "Администратор" : "Пользователь")}");
                Console.WriteLine(new string('─', 60));
            }

            Console.WriteLine("1. Управление устройствами");
            Console.WriteLine("2. Управление комнатами");
            Console.WriteLine("3. Управление пользователями");
            Console.WriteLine("4. Сценарии автоматизации");
            Console.WriteLine("5. Уведомления");
            Console.WriteLine("6. Энергоотчеты");
            Console.WriteLine("7. Статус системы");
            Console.WriteLine("8. Сортировка и поиск устройств");
            Console.WriteLine("9. Сохранить данные");
            Console.WriteLine("0. Выход");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.Write("Выберите опцию: ");
        }

        private void DisplayDevicesMenu()
        {
            Console.Clear();
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("               УПРАВЛЕНИЕ УСТРОЙСТВАМИ");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
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
            Console.Clear();
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("               УПРАВЛЕНИЕ КОМНАТАМИ");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
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
            Console.Clear();
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("               УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ (АДМИН)");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("1. Список пользователей");
            Console.WriteLine("2. Добавить пользователя");
            Console.WriteLine("3. Удалить пользователя");
            Console.WriteLine("4. Изменить права доступа");
            Console.WriteLine("5. Назад в главное меню");
            Console.Write("Выберите опцию: ");
        }

        private void DisplayScenariosMenu()
        {
            Console.Clear();
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("               СЦЕНАРИИ АВТОМАТИЗАЦИИ");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
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
            Console.Clear();
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("                     УВЕДОМЛЕНИЯ");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("1. Показать все уведомления");
            Console.WriteLine("2. Показать непрочитанные");
            Console.WriteLine("3. Отметить как прочитанное");
            Console.WriteLine("4. Назад в главное меню");
            Console.Write("Выберите опцию: ");
        }

        private void DisplaySortingSearchMenu()
        {
            Console.Clear();
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("               СОРТИРОВКА И ПОИСК УСТРОЙСТВ");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("1. Сортировка устройств по потреблению (возрастание)");
            Console.WriteLine("2. Сортировка устройств по потреблению (убывание)");
            Console.WriteLine("3. Сортировка устройств по названию");
            Console.WriteLine("4. Поиск устройств в комнате");
            Console.WriteLine("5. Поиск устройств по производителю");
            Console.WriteLine("6. Поиск устройства по названию");
            Console.WriteLine("7. Поиск устройств с потреблением выше порога");
            Console.WriteLine("8. Назад в главное меню");
            Console.Write("Выберите опцию: ");
        }

        private void DisplayAdditionalActions(Room selectedRoom = null, List<Device> foundDevices = null)
        {
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("               ДОПОЛНИТЕЛЬНЫЕ ДЕЙСТВИЯ");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("1. Включить все устройства в комнате");
            Console.WriteLine("2. Выключить все устройства в комнате");
            Console.WriteLine("3. Добавить новое устройство в эту комнату");
            Console.WriteLine("4. Удалить устройство из комнаты");
            Console.WriteLine("0. Вернуться в меню поиска");
            Console.Write("Выберите опцию: ");
        }

        private void ListAllDevices()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("                    ВСЕ УСТРОЙСТВА");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                if (devices.Count == 0)
                {
                    Console.WriteLine("Устройств не найдено.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("┌────┬────────────────────────┬──────────────────┬─────────────┬────────────────┬────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Тип              │ Статус      │ Комната        │ Потребление    │");
                Console.WriteLine("├────┼────────────────────────┼──────────────────┼─────────────┼────────────────┼────────────────┤");

                for (int i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];
                    string deviceType = device.GetType().Name;
                    string simpleType = deviceType.Replace("Device", "");

                    Console.WriteLine($"│ {i + 1,2} │ {device.Name,-23} │ " +
                        $"{simpleType,-16} │ " +
                        $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                        $"{(device.Location != null ? device.Location.Name : "Нет"),-14} │ " +
                        $"{device.PowerConsumption,10:F1} Вт │");
                }

                Console.WriteLine("└────┴────────────────────────┴──────────────────┴─────────────┴────────────────┴────────────────┘");

                Console.WriteLine("\nВсего устройств: " + devices.Count);
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении устройств: {ex.Message}");
                Console.WriteLine("Нажмите Enter для продолжения...");
                Console.ReadLine();
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
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Неверный номер устройства!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при переключении устройства: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void AddDevice()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               ДОБАВЛЕНИЕ НОВОГО УСТРОЙСТВА");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                Console.Write("Тип устройства (1-Базовое, 2-Климатическое, 3-Безопасность, 4-Мультимедиа): ");
                if (!int.TryParse(Console.ReadLine(), out int typeChoice) || typeChoice < 1 || typeChoice > 4)
                {
                    Console.WriteLine("Неверный выбор типа!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Название: ");
                string name = Console.ReadLine();

                Console.Write("Производитель: ");
                string manufacturer = Console.ReadLine();

                Console.Write("Энергопотребление (Вт): ");
                if (!double.TryParse(Console.ReadLine(), out double power))
                {
                    Console.WriteLine("Неверный формат мощности!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                // Показываем список комнат
                Console.WriteLine("\nСписок комнат:");
                for (int i = 0; i < rooms.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {rooms[i].Name} ({rooms[i].Area} м²)");
                }

                Console.Write("Выберите номер комнаты: ");

                if (int.TryParse(Console.ReadLine(), out int roomChoice) && roomChoice > 0 && roomChoice <= rooms.Count)
                {
                    Room room = rooms[roomChoice - 1];
                    Device newDevice = null;

                    switch (typeChoice)
                    {
                        case 1:
                            Console.Write("Тип устройства (1-Датчик, 2-Исполнительное, 3-Климат, 4-Безопасность, 5-Мультимедиа): ");
                            if (int.TryParse(Console.ReadLine(), out int subTypeChoice) && subTypeChoice >= 1 && subTypeChoice <= 5)
                            {
                                DeviceType deviceType = (DeviceType)(subTypeChoice - 1);
                                newDevice = new Device(
                                    "DEV" + (devices.Count + 1).ToString("D3"),
                                    name,
                                    manufacturer,
                                    deviceType,
                                    room,
                                    power
                                );
                            }
                            break;

                        case 2:
                            Console.Write("Минимальная температура: ");
                            double minTemp = double.Parse(Console.ReadLine());
                            Console.Write("Максимальная температура: ");
                            double maxTemp = double.Parse(Console.ReadLine());

                            newDevice = new ClimateControlDevice(
                                "CLIM" + (devices.Count(d => d is ClimateControlDevice) + 1).ToString("D3"),
                                name,
                                manufacturer,
                                room,
                                power,
                                minTemp,
                                maxTemp
                            );
                            break;

                        case 3:
                            Console.Write("Есть камера? (1-Да, 2-Нет): ");
                            bool hasCamera = Console.ReadLine() == "1";
                            Console.Write("Есть датчик движения? (1-Да, 2-Нет): ");
                            bool hasMotionSensor = Console.ReadLine() == "1";

                            newDevice = new SecurityDevice(
                                "SEC" + (devices.Count(d => d is SecurityDevice) + 1).ToString("D3"),
                                name,
                                manufacturer,
                                room,
                                power,
                                hasCamera,
                                hasMotionSensor
                            );
                            break;

                        case 4:
                            Console.Write("Размер экрана (дюймы): ");
                            int screenSize = int.Parse(Console.ReadLine());
                            Console.Write("Есть Smart функции? (1-Да, 2-Нет): ");
                            bool hasSmartFeatures = Console.ReadLine() == "1";

                            newDevice = new MultimediaDevice(
                                "MULT" + (devices.Count(d => d is MultimediaDevice) + 1).ToString("D3"),
                                name,
                                manufacturer,
                                room,
                                power,
                                screenSize,
                                hasSmartFeatures
                            );
                            break;
                    }

                    if (newDevice != null)
                    {
                        devices.Add(newDevice);
                        room.AddDevice(newDevice);
                        Console.WriteLine($"\nУстройство '{name}' добавлено в комнату '{room.Name}'!");
                        SaveData();

                        string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                        notifications.Add(new Notification(notificationId, NotificationType.INFO,
                            $"Добавлено новое устройство: {name}", newDevice));
                    }
                }
                else
                {
                    Console.WriteLine("Неверный номер комнаты!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении устройства: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void DeleteDevice()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               УДАЛЕНИЕ УСТРОЙСТВА");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                ListAllDevices();
                Console.Write("Выберите номер устройства для удаления: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= devices.Count)
                {
                    Device device = devices[choice - 1];
                    string deviceName = device.Name;

                    if (device.Location != null)
                    {
                        device.Location.RemoveDevice(device);
                    }

                    devices.RemoveAt(choice - 1);
                    Console.WriteLine($"\nУстройство '{deviceName}' удалено!");
                    SaveData();

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.WARNING,
                        $"Удалено устройство: {deviceName}", device));
                }
                else
                {
                    Console.WriteLine("Неверный номер устройства!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении устройства: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ListAllRooms()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("                      ВСЕ КОМНАТЫ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                if (rooms.Count == 0)
                {
                    Console.WriteLine("Комнат не найдено.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("┌────┬────────────────────────┬────────────────┬────────────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Площадь (м²)   │ Устройств            │");
                Console.WriteLine("├────┼────────────────────────┼────────────────┼────────────────────────┤");

                for (int i = 0; i < rooms.Count; i++)
                {
                    Console.WriteLine($"│ {i + 1,2} │ {rooms[i].Name,-23} │ " +
                        $"{rooms[i].Area,14:F1} │ {rooms[i].GetDevices().Count,20} │");
                }

                Console.WriteLine("└────┴────────────────────────┴────────────────┴────────────────────────┘");

                Console.WriteLine("\nВсего комнат: " + rooms.Count);
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении комнат: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ShowRoomDevices()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("           УСТРОЙСТВА В КОМНАТЕ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                ListAllRooms();
                Console.Write("Выберите номер комнаты: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
                {
                    Console.Clear();
                    rooms[choice - 1].DisplayDevices();
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Неверный номер комнаты!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении устройств комнаты: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ShowRoomPower()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("           ПОТРЕБЛЕНИЕ ЭНЕРГИИ КОМНАТЫ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                ListAllRooms();
                Console.Write("Выберите номер комнаты: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
                {
                    double power = rooms[choice - 1].CalculateRoomPowerConsumption();
                    Console.WriteLine($"\nТекущее потребление энергии: {power:F1} Вт");

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
                    Console.WriteLine($"\nВключено устройств: {activeDevices} из {roomDevices.Count}");
                }
                else
                {
                    Console.WriteLine("Неверный номер комнаты!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при расчете потребления комнаты: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void AddRoom()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               ДОБАВЛЕНИЕ НОВОЙ КОМНАТЫ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

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
                    Console.WriteLine($"\nКомната '{name}' добавлена!");
                    SaveData();

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Добавлена новая комната: {name}"));
                }
                else
                {
                    Console.WriteLine("Неверный ввод площади!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении комнаты: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void DeleteRoom()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               УДАЛЕНИЕ КОМНАТЫ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                ListAllRooms();
                Console.Write("Выберите номер комнаты для удаления: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
                {
                    if (rooms[choice - 1].GetDevices().Count > 0)
                    {
                        Console.WriteLine("В комнате есть устройства! Сначала удалите или переместите их.");
                        Console.WriteLine("\nНажмите Enter для продолжения...");
                        Console.ReadLine();
                        return;
                    }

                    string roomName = rooms[choice - 1].Name;
                    rooms.RemoveAt(choice - 1);
                    Console.WriteLine($"\nКомната '{roomName}' удалена!");
                    SaveData();

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.WARNING,
                        $"Удалена комната: {roomName}"));
                }
                else
                {
                    Console.WriteLine("Неверный номер комнаты!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка операции: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении комнаты: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ListUsers()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("                    ВСЕ ПОЛЬЗОВАТЕЛИ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                if (users.Count == 0)
                {
                    Console.WriteLine("Пользователей не найдено.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                for (int i = 0; i < users.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. ");
                    users[i].DisplayInfo();
                    Console.WriteLine();
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении пользователей: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void AddUser()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               ДОБАВЛЕНИЕ НОВОГО ПОЛЬЗОВАТЕЛЯ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

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
                    Console.WriteLine($"\nПользователь '{username}' добавлен!");
                    SaveData();

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Добавлен новый пользователь: {username}"));
                }
                else
                {
                    Console.WriteLine("Неверный уровень доступа!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении пользователя: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void DeleteUser()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               УДАЛЕНИЕ ПОЛЬЗОВАТЕЛЯ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                ListUsers();
                Console.Write("Выберите номер пользователя для удаления: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= users.Count)
                {
                    if (users[choice - 1] == currentUser)
                    {
                        Console.WriteLine("Нельзя удалить текущего пользователя!");
                        Console.WriteLine("\nНажмите Enter для продолжения...");
                        Console.ReadLine();
                        return;
                    }

                    string userName = users[choice - 1].Username;
                    users.RemoveAt(choice - 1);
                    Console.WriteLine($"\nПользователь '{userName}' удален!");
                    SaveData();

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.WARNING,
                        $"Удален пользователь: {userName}"));
                }
                else
                {
                    Console.WriteLine("Неверный номер пользователя!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка операции: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ChangeUserAccess()
        {
            if (!HasAdminAccess()) return;

            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("          ИЗМЕНЕНИЕ ПРАВ ДОСТУПА ПОЛЬЗОВАТЕЛЯ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

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

                        Console.WriteLine($"\nПрава доступа пользователя '{users[choice - 1].Username}' изменены: {oldLevel} -> {newLevel}");
                        SaveData();

                        string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                        notifications.Add(new Notification(notificationId, NotificationType.INFO,
                            $"Изменены права доступа пользователя {users[choice - 1].Username}: {oldLevel} -> {newLevel}"));
                    }
                    else
                    {
                        Console.WriteLine("Неверный уровень доступа!");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный номер пользователя!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при изменении прав доступа: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ShowAllNotifications()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("                    ВСЕ УВЕДОМЛЕНИЯ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                if (notifications.Count == 0)
                {
                    Console.WriteLine("Уведомлений нет.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                for (int i = 0; i < notifications.Count; i++)
                {
                    Console.Write($"{i + 1}. ");
                    notifications[i].DisplayInfo();
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении уведомлений: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ShowUnreadNotifications()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               НЕПРОЧИТАННЫЕ УВЕДОМЛЕНИЯ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

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

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении непрочитанных уведомлений: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
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
                    Console.WriteLine("\nУведомление отмечено как прочитанное.");
                }
                else
                {
                    Console.WriteLine("Неверный номер уведомления!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отметке уведомления: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void CreateEnergyReport()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               СОЗДАНИЕ ОТЧЕТА ПО ЭНЕРГОПОТРЕБЛЕНИЮ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                EnergyReport report = new EnergyReport(
                    "ОТЧЕТ" + (reports.Count + 1).ToString("D3"),
                    DateTime.Now.AddDays(-1),
                    DateTime.Now
                );

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

                string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                notifications.Add(new Notification(notificationId, NotificationType.INFO,
                    $"Создан новый отчет по энергопотреблению: {report.ReportId}"));

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании отчета по энергопотреблению: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void SystemStatus()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("                    СТАТУС СИСТЕМЫ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");
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

                Console.WriteLine($"\nАктивных устройств: {activeDevices}/{devices.Count}");
                Console.WriteLine($"Текущее потребление: {totalPower:F1} Вт");
                Console.WriteLine($"Всего сессий системы: {TotalSessions}");

                Console.WriteLine("\nСтатистика по типам устройств:");
                Console.WriteLine($"- Базовые устройства: {devices.Count(d => d.GetType() == typeof(Device))}");
                Console.WriteLine($"- Климатические устройства: {devices.Count(d => d is ClimateControlDevice)}");
                Console.WriteLine($"- Устройства безопасности: {devices.Count(d => d is SecurityDevice)}");
                Console.WriteLine($"- Мультимедийные устройства: {devices.Count(d => d is MultimediaDevice)}");

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении статуса системы: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void Login()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("                     ВХОД В СИСТЕМУ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.Write("Имя пользователя: ");
                string username = Console.ReadLine();

                Console.Write("Пароль: ");
                string password = Console.ReadLine();

                foreach (var user in users)
                {
                    if (user.Username == username && user.Login(password))
                    {
                        currentUser = user;
                        Console.WriteLine($"\nВход выполнен успешно! Добро пожаловать, {username}!");

                        string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                        notifications.Add(new Notification(notificationId, NotificationType.INFO,
                            $"Пользователь {username} вошел в систему"));
                        Console.WriteLine("\nНажмите Enter для продолжения...");
                        Console.ReadLine();
                        return;
                    }
                }

                Console.WriteLine("\nОшибка входа! Неверное имя пользователя или пароль.");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при входе в систему: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
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
                    Console.WriteLine($"\nПользователь {username} вышел из системы.");

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Пользователь {username} вышел из системы"));

                    currentUser = null;
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выходе из системы: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
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
                                Console.WriteLine("\nНажмите Enter для продолжения...");
                                Console.ReadLine();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления устройствами: {ex.Message}");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
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
                                Console.WriteLine("\nНажмите Enter для продолжения...");
                                Console.ReadLine();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления комнатами: {ex.Message}");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
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
                                Console.WriteLine("\nНажмите Enter для продолжения...");
                                Console.ReadLine();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления пользователями: {ex.Message}");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
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
                                    Console.WriteLine("\nНажмите Enter для продолжения...");
                                    Console.ReadLine();
                                }
                                else
                                {
                                    Console.Clear();
                                    Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                                    Console.WriteLine("                    ВСЕ СЦЕНАРИИ");
                                    Console.WriteLine("═══════════════════════════════════════════════════════════");
                                    foreach (var scenario in scenarios)
                                    {
                                        scenario.DisplayInfo();
                                        Console.WriteLine();
                                    }
                                    Console.WriteLine("\nНажмите Enter для продолжения...");
                                    Console.ReadLine();
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
                                Console.WriteLine("\nНажмите Enter для продолжения...");
                                Console.ReadLine();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления сценариями: {ex.Message}");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
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
                                Console.WriteLine("\nНажмите Enter для продолжения...");
                                Console.ReadLine();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню управления уведомлениями: {ex.Message}");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
            } while (choice != 4);
        }

        private void CreateScenario()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               СОЗДАНИЕ НОВОГО СЦЕНАРИЯ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

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
                Console.WriteLine($"\nСценарий '{name}' создан! Теперь добавьте действия.");
                AddActionToScenario(scenario);
                SaveData();

                string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                notifications.Add(new Notification(notificationId, NotificationType.INFO,
                    $"Создан новый сценарий: {name}"));

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании сценария: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ExecuteScenario()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               ВЫПОЛНЕНИЕ СЦЕНАРИЯ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                if (scenarios.Count == 0)
                {
                    Console.WriteLine("Нет доступных сценариев!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("Доступные сценарии:");
                for (int i = 0; i < scenarios.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {scenarios[i].Name} (Время: {scenarios[i].TriggerCondition})");
                }

                Console.Write("\nВыберите номер сценария: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= scenarios.Count)
                {
                    scenarios[choice - 1].Activate();
                    scenarios[choice - 1].Execute();
                    SaveData();

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Выполнен сценарий: {scenarios[choice - 1].Name}"));
                }
                else
                {
                    Console.WriteLine("Неверный номер сценария!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении сценария: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void ShowScenarioActions()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               ДЕЙСТВИЯ СЦЕНАРИЯ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                if (scenarios.Count == 0)
                {
                    Console.WriteLine("Нет доступных сценариев!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("Доступные сценарии:");
                for (int i = 0; i < scenarios.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {scenarios[i].Name} (Время: {scenarios[i].TriggerCondition})");
                }

                Console.Write("\nВыберите номер сценария: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= scenarios.Count)
                {
                    scenarios[choice - 1].DisplayActions();
                }
                else
                {
                    Console.WriteLine("Неверный номер сценария!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении действий сценария: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void AddActionToScenario(AutomationScenario specificScenario = null)
        {
            try
            {
                if (scenarios.Count == 0)
                {
                    Console.WriteLine("Нет доступных сценариев!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                AutomationScenario scenario = specificScenario;
                if (scenario == null)
                {
                    Console.WriteLine("Доступные сценарии:");
                    for (int i = 0; i < scenarios.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {scenarios[i].Name} (Время: {scenarios[i].TriggerCondition})");
                    }

                    Console.Write("\nВыберите номер сценария: ");
                    if (!int.TryParse(Console.ReadLine(), out int choice) || choice <= 0 || choice > scenarios.Count)
                    {
                        Console.WriteLine("Неверный номер сценария!");
                        Console.WriteLine("\nНажмите Enter для продолжения...");
                        Console.ReadLine();
                        return;
                    }
                    scenario = scenarios[choice - 1];
                }

                Console.Clear();
                Console.WriteLine($"\nДобавление действия в сценарий '{scenario.Name}':");

                // Показываем устройства
                Console.WriteLine("\nДоступные устройства:");
                for (int i = 0; i < devices.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {devices[i].Name} (Комната: {devices[i].Location?.Name ?? "Нет"})");
                }

                Console.Write("\nВыберите номер устройства: ");

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
                        Console.WriteLine($"\nДействие добавлено в сценарий '{scenario.Name}'!");
                        SaveData();
                    }
                    else
                    {
                        Console.WriteLine("Неверный выбор действия!");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный номер устройства!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: необходимо ввести число!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении действия в сценарий: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void SortDevicesByConsumption(bool ascending = true)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine($"    СОРТИРОВКА УСТРОЙСТВ ПО ПОТРЕБЛЕНИЮ ({(ascending ? "ВОЗРАСТАНИЕ" : "УБЫВАНИЕ")})");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                var sortedDevices = ascending ?
                    devices.OrderBy(d => d.PowerConsumption).ToList() :
                    devices.OrderByDescending(d => d.PowerConsumption).ToList();

                if (sortedDevices.Count == 0)
                {
                    Console.WriteLine("Устройств не найдено.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("┌────┬────────────────────────┬──────────────────┬─────────────┬────────────────┬────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Тип              │ Статус      │ Комната        │ Потребление    │");
                Console.WriteLine("├────┼────────────────────────┼──────────────────┼─────────────┼────────────────┼────────────────┤");

                for (int i = 0; i < sortedDevices.Count; i++)
                {
                    var device = sortedDevices[i];
                    string deviceType = device.GetType().Name;
                    string simpleType = deviceType.Replace("Device", "");

                    Console.WriteLine($"│ {i + 1,2} │ {device.Name,-23} │ " +
                        $"{simpleType,-16} │ " +
                        $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                        $"{(device.Location != null ? device.Location.Name : "Нет"),-14} │ " +
                        $"{device.PowerConsumption,10:F1} Вт │");
                }

                Console.WriteLine("└────┴────────────────────────┴──────────────────┴─────────────┴────────────────┴────────────────┘");

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сортировке устройств: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void SortDevicesByName()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("       СОРТИРОВКА УСТРОЙСТВ ПО НАЗВАНИЮ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                var sortedDevices = devices.OrderBy(d => d.Name).ToList();

                if (sortedDevices.Count == 0)
                {
                    Console.WriteLine("Устройств не найдено.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("┌────┬────────────────────────┬──────────────────┬─────────────┬────────────────┬────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Тип              │ Статус      │ Комната        │ Потребление    │");
                Console.WriteLine("├────┼────────────────────────┼──────────────────┼─────────────┼────────────────┼────────────────┤");

                for (int i = 0; i < sortedDevices.Count; i++)
                {
                    var device = sortedDevices[i];
                    string deviceType = device.GetType().Name;
                    string simpleType = deviceType.Replace("Device", "");

                    Console.WriteLine($"│ {i + 1,2} │ {device.Name,-23} │ " +
                        $"{simpleType,-16} │ " +
                        $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                        $"{(device.Location != null ? device.Location.Name : "Нет"),-14} │ " +
                        $"{device.PowerConsumption,10:F1} Вт │");
                }

                Console.WriteLine("└────┴────────────────────────┴──────────────────┴─────────────┴────────────────┴────────────────┘");

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сортировке устройств: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void SearchDevicesInRoom()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("           ПОИСК УСТРОЙСТВ В КОМНАТЕ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                // Сначала показываем список комнат
                Console.WriteLine("Список комнат:");
                for (int i = 0; i < rooms.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {rooms[i].Name} ({rooms[i].Area} м²) - {rooms[i].GetDevices().Count} устройств");
                }

                Console.Write("\nВыберите номер комнаты: ");

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
                {
                    Room selectedRoom = rooms[choice - 1];
                    var roomDevices = devices.Where(d => d.Location == selectedRoom).ToList();

                    Console.Clear();
                    Console.WriteLine($"\nУстройства в комнате '{selectedRoom.Name}':");

                    if (roomDevices.Count == 0)
                    {
                        Console.WriteLine($"В комнате '{selectedRoom.Name}' нет устройств.");
                        Console.WriteLine("\nНажмите Enter для продолжения...");
                        Console.ReadLine();
                        return;
                    }

                    Console.WriteLine("┌────┬────────────────────────┬──────────────────┬─────────────┬────────────────┐");
                    Console.WriteLine("│ №  │ Название               │ Тип              │ Статус      │ Потребление    │");
                    Console.WriteLine("├────┼────────────────────────┼──────────────────┼─────────────┼────────────────┤");

                    for (int i = 0; i < roomDevices.Count; i++)
                    {
                        var device = roomDevices[i];
                        string deviceType = device.GetType().Name;
                        string simpleType = deviceType.Replace("Device", "");

                        Console.WriteLine($"│ {i + 1,2} │ {device.Name,-23} │ " +
                            $"{simpleType,-16} │ " +
                            $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                            $"{device.PowerConsumption,10:F1} Вт │");
                    }

                    Console.WriteLine("└────┴────────────────────────┴──────────────────┴─────────────┴────────────────┘");

                    // Показываем дополнительные действия
                    DisplayAdditionalActions(selectedRoom, roomDevices);
                    HandleAdditionalActions(selectedRoom, roomDevices);
                }
                else
                {
                    Console.WriteLine("Неверный номер комнаты!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске устройств в комнате: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void SearchDevicesByManufacturer()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("       ПОИСК УСТРОЙСТВ ПО ПРОИЗВОДИТЕЛЮ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                Console.Write("Введите производителя: ");
                string manufacturer = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(manufacturer))
                {
                    Console.WriteLine("Не введен производитель!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                var foundDevices = devices.Where(d =>
                    d.Manufacturer.Contains(manufacturer, StringComparison.OrdinalIgnoreCase)).ToList();

                if (foundDevices.Count == 0)
                {
                    Console.WriteLine($"Устройств производителя '{manufacturer}' не найдено.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"\nУстройства производителя '{manufacturer}':");
                Console.WriteLine("┌────┬────────────────────────┬──────────────────┬─────────────┬────────────────┬────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Тип              │ Статус      │ Комната        │ Потребление    │");
                Console.WriteLine("├────┼────────────────────────┼──────────────────┼─────────────┼────────────────┼────────────────┤");

                for (int i = 0; i < foundDevices.Count; i++)
                {
                    var device = foundDevices[i];
                    string deviceType = device.GetType().Name;
                    string simpleType = deviceType.Replace("Device", "");

                    Console.WriteLine($"│ {i + 1,2} │ {device.Name,-23}│ " +
                        $"{simpleType,-16} │ " +
                        $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                        $"{(device.Location != null ? device.Location.Name : "Нет"),-14} │ " +
                        $"{device.PowerConsumption,10:F1} Вт │");
                }

                Console.WriteLine("└────┴────────────────────────┴──────────────────┴─────────────┴────────────────┴────────────────┘");

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске устройств по производителю: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void SearchDeviceByName()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("         ПОИСК УСТРОЙСТВА ПО НАЗВАНИЮ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                Console.Write("Введите название устройства: ");
                string deviceName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    Console.WriteLine("Не введено название устройства!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                var foundDevices = devices.Where(d =>
                    d.Name.Contains(deviceName, StringComparison.OrdinalIgnoreCase)).ToList();

                if (foundDevices.Count == 0)
                {
                    Console.WriteLine($"Устройств с названием '{deviceName}' не найдено.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"\nУстройства с названием '{deviceName}':");
                Console.WriteLine("┌────┬────────────────────────┬──────────────────┬─────────────┬────────────────┬────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Тип              │ Статус      │ Комната        │ Потребление    │");
                Console.WriteLine("├────┼────────────────────────┼──────────────────┼─────────────┼────────────────┼────────────────┤");

                for (int i = 0; i < foundDevices.Count; i++)
                {
                    var device = foundDevices[i];
                    string deviceType = device.GetType().Name;
                    string simpleType = deviceType.Replace("Device", "");

                    Console.WriteLine($"│ {i + 1,2} │ {device.Name,-23} │ " +
                        $"{simpleType,-16} │ " +
                        $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                        $"{(device.Location != null ? device.Location.Name : "Нет"),-14} │ " +
                        $"{device.PowerConsumption,10:F1} Вт │");
                }

                Console.WriteLine("└────┴────────────────────────┴──────────────────┴─────────────┴────────────────┴────────────────┘");

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске устройства по названию: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void SearchDevicesByPowerThreshold()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("  ПОИСК УСТРОЙСТВ С ПОТРЕБЛЕНИЕМ ВЫШЕ ПОРОГА");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                Console.Write("Введите порог потребления (Вт): ");
                if (!double.TryParse(Console.ReadLine(), out double threshold))
                {
                    Console.WriteLine("Неверный формат порога!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                var foundDevices = devices.Where(d => d.PowerConsumption > threshold).ToList();

                if (foundDevices.Count == 0)
                {
                    Console.WriteLine($"Устройств с потреблением выше {threshold} Вт не найдено.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"\nУстройства с потреблением выше {threshold} Вт:");
                Console.WriteLine("┌────┬────────────────────────┬──────────────────┬─────────────┬────────────────┬────────────────┐");
                Console.WriteLine("│ №  │ Название               │ Тип              │ Статус      │ Комната        │ Потребление    │");
                Console.WriteLine("├────┼────────────────────────┼──────────────────┼─────────────┼────────────────┼────────────────┤");

                for (int i = 0; i < foundDevices.Count; i++)
                {
                    var device = foundDevices[i];
                    string deviceType = device.GetType().Name;
                    string simpleType = deviceType.Replace("Device", "");

                    Console.WriteLine($"│ {i + 1,2} │ {device.Name,-23} │ " +
                        $"{simpleType,-16} │ " +
                        $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                        $"{(device.Location != null ? device.Location.Name : "Нет"),-14} │ " +
                        $"{device.PowerConsumption,10:F1} Вт │");
                }

                Console.WriteLine("└────┴────────────────────────┴──────────────────┴─────────────┴────────────────┴────────────────┘");

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске устройств: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void HandleAdditionalActions(Room selectedRoom, List<Device> foundDevices)
        {
            try
            {
                Console.Write("Выберите опцию: ");
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            TurnOnAllDevicesInRoom(selectedRoom);
                            break;
                        case 2:
                            TurnOffAllDevicesInRoom(selectedRoom);
                            break;
                        case 3:
                            AddDeviceToRoom(selectedRoom);
                            break;
                        case 4:
                            DeleteDeviceFromRoom(selectedRoom, foundDevices);
                            break;
                        case 0:
                            return;
                        default:
                            Console.WriteLine("Неверная опция!");
                            Console.WriteLine("\nНажмите Enter для продолжения...");
                            Console.ReadLine();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении действия: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void TurnOnAllDevicesInRoom(Room room)
        {
            try
            {
                int turnedOn = 0;
                foreach (var device in room.GetDevices())
                {
                    if (!device.IsOn)
                    {
                        device.TurnOn();
                        turnedOn++;
                    }
                }
                Console.WriteLine($"\nВключено {turnedOn} устройств в комнате '{room.Name}'.");
                SaveData();
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при включении устройств: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void TurnOffAllDevicesInRoom(Room room)
        {
            try
            {
                int turnedOff = 0;
                foreach (var device in room.GetDevices())
                {
                    if (device.IsOn)
                    {
                        device.TurnOff();
                        turnedOff++;
                    }
                }
                Console.WriteLine($"\nВыключено {turnedOff} устройств в комнате '{room.Name}'.");
                SaveData();
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выключении устройств: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void AddDeviceToRoom(Room room)
        {
            try
            {
                Console.Clear();
                Console.WriteLine($"\nДобавление нового устройства в комнату '{room.Name}':");
                Console.Write("Название: ");
                string name = Console.ReadLine();

                Console.Write("Производитель: ");
                string manufacturer = Console.ReadLine();

                Console.Write("Энергопотребление (Вт): ");
                if (!double.TryParse(Console.ReadLine(), out double power))
                {
                    Console.WriteLine("Неверный формат мощности!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Тип устройства (1-Датчик, 2-Исполнительное, 3-Климат, 4-Безопасность, 5-Мультимедиа): ");
                if (int.TryParse(Console.ReadLine(), out int typeChoice) && typeChoice >= 1 && typeChoice <= 5)
                {
                    DeviceType deviceType = (DeviceType)(typeChoice - 1);
                    Device newDevice = new Device(
                        "DEV" + (devices.Count + 1).ToString("D3"),
                        name,
                        manufacturer,
                        deviceType,
                        room,
                        power
                    );

                    devices.Add(newDevice);
                    room.AddDevice(newDevice);
                    Console.WriteLine($"\nУстройство '{name}' добавлено в комнату '{room.Name}'!");
                    SaveData();

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.INFO,
                        $"Добавлено новое устройство: {name}", newDevice));
                }
                else
                {
                    Console.WriteLine("Неверный тип устройства!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении устройства: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void DeleteDeviceFromRoom(Room room, List<Device> roomDevices)
        {
            try
            {
                if (roomDevices.Count == 0)
                {
                    Console.WriteLine("В комнате нет устройств для удаления.");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"\nУстройства в комнате '{room.Name}':");
                for (int i = 0; i < roomDevices.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {roomDevices[i].Name} ({roomDevices[i].GetDeviceTypeString()})");
                }

                Console.Write("\nВыберите номер устройства для удаления: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= roomDevices.Count)
                {
                    Device device = roomDevices[choice - 1];
                    devices.Remove(device);
                    room.RemoveDevice(device);
                    Console.WriteLine($"\nУстройство '{device.Name}' удалено из комнаты '{room.Name}'.");
                    SaveData();

                    string notificationId = "NOT" + (notifications.Count + 1).ToString("D3");
                    notifications.Add(new Notification(notificationId, NotificationType.WARNING,
                        $"Удалено устройство: {device.Name}", device));
                }
                else
                {
                    Console.WriteLine("Неверный номер устройства!");
                }

                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении устройства: {ex.Message}");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        private void SortingSearchManagement()
        {
            int choice = -1;
            do
            {
                try
                {
                    DisplaySortingSearchMenu();
                    if (int.TryParse(Console.ReadLine(), out choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                SortDevicesByConsumption(true);
                                break;
                            case 2:
                                SortDevicesByConsumption(false);
                                break;
                            case 3:
                                SortDevicesByName();
                                break;
                            case 4:
                                SearchDevicesInRoom();
                                break;
                            case 5:
                                SearchDevicesByManufacturer();
                                break;
                            case 6:
                                SearchDeviceByName();
                                break;
                            case 7:
                                SearchDevicesByPowerThreshold();
                                break;
                            case 8:
                                return;
                            default:
                                Console.WriteLine("Неверная опция!");
                                Console.WriteLine("\nНажмите Enter для продолжения...");
                                Console.ReadLine();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в меню сортировки и поиска: {ex.Message}");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
            } while (choice != 8);
        }

        public void Run()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("               СИСТЕМА УПРАВЛЕНИЯ УМНЫМ ДОМОМ");
                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.WriteLine("           Загрузка данных...");

                LoadData();
                Console.WriteLine("           Данные успешно загружены!");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                while (currentUser == null)
                {
                    Login();

                    if (currentUser == null)
                    {
                        Console.WriteLine("\nДля работы с системой необходимо войти в аккаунт!");
                        Console.WriteLine("Нажмите Enter для повторной попытки...");
                        Console.ReadLine();
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
                                case 1:
                                    DeviceManagement();
                                    break;
                                case 2:
                                    RoomManagement();
                                    break;
                                case 3:
                                    ExecuteWithAccessCheck(() => UserManagement(), true);
                                    break;
                                case 4:
                                    ExecuteWithAccessCheck(() => ScenarioManagement(), false);
                                    break;
                                case 5:
                                    ExecuteWithAccessCheck(() => NotificationManagement(), false);
                                    break;
                                case 6:
                                    ExecuteWithAccessCheck(() => CreateEnergyReport(), false);
                                    break;
                                case 7:
                                    SystemStatus();
                                    break;
                                case 8:
                                    ExecuteWithAccessCheck(() => SortingSearchManagement(), false);
                                    break;
                                case 9:
                                    ExecuteWithAccessCheck(() =>
                                    {
                                        SaveData();
                                        Console.WriteLine("\nДанные сохранены успешно!");
                                        Console.WriteLine("Нажмите Enter для продолжения...");
                                        Console.ReadLine();
                                    }, true);
                                    break;
                                case 0:
                                    ExecuteWithAccessCheck(() =>
                                    {
                                        SaveData();
                                        Console.WriteLine("\nДо свидания!");
                                        Console.WriteLine("Нажмите Enter для выхода...");
                                        Console.ReadLine();
                                    }, false);
                                    break;
                                default:
                                    Console.WriteLine("Неверная опция!");
                                    Console.WriteLine("Нажмите Enter для продолжения...");
                                    Console.ReadLine();
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Неверный ввод!");
                            Console.WriteLine("Нажмите Enter для продолжения...");
                            Console.ReadLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка в меню: {ex.Message}");
                        Console.WriteLine("Нажмите Enter для продолжения...");
                        Console.ReadLine();
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
                if (currentUser != null && bool.Parse(systemSettings["BackupOnExit"]))
                {
                    try
                    {
                        DataManager.CreateBackup();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
    }
}