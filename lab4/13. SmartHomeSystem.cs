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

        public SmartHomeSystem()
        {
            rooms = new List<Room>();
            devices = new List<Device>();
            users = new List<User>();
            scenarios = new List<AutomationScenario>();
            notifications = new List<Notification>();
            reports = new List<EnergyReport>();
            currentUser = null;
        }

        public void LoadData()
        {
            Console.WriteLine("Загрузка данных...");

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

            Console.WriteLine($"Данные загружены: {rooms.Count} комнат, " +
                $"{devices.Count} устройств, {users.Count} пользователей");
        }

        public void SaveData()
        {
            Console.WriteLine("Сохранение данных...");
            DataManager.SaveRooms(rooms);
            DataManager.SaveDevices(devices);
            DataManager.SaveUsers(users);
            DataManager.SaveScenarios(scenarios);
            DataManager.SaveNotifications(notifications);
            Console.WriteLine("Данные сохранены!");
        }

        private bool HasAdminAccess()
        {
            if (currentUser == null)
            {
                Console.WriteLine("Ошибка: Необходимо войти в систему!");
                return false;
            }
            if (currentUser.AccessLevel != AccessLevel.ADMIN)
            {
                Console.WriteLine("Ошибка: Недостаточно прав! Требуются права администратора.");
                return false;
            }
            return true;
        }

        private bool HasUserAccess()
        {
            if (currentUser == null)
            {
                Console.WriteLine("Ошибка: Необходимо войти в систему!");
                return false;
            }
            return true;
        }

        public void DisplayMainMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("               УМНЫЙ ДОМ - ГЛАВНОЕ МЕНЮ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");

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

            if (currentUser == null)
            {
                Console.WriteLine("8. Войти в систему");
            }
            else
            {
                Console.WriteLine("8. Выйти из системы");
            }

            Console.WriteLine("9. Сохранить данные");
            Console.WriteLine("0. Выход");
            Console.WriteLine("══════════════════════════════════════════════════════════════");
            Console.Write("Выберите опцию: ");
        }

        private void DisplayDevicesMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("               УПРАВЛЕНИЕ УСТРОЙСТВАМИ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");
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
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("               УПРАВЛЕНИЕ КОМНАТАМИ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");
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
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("          УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ (АДМИН)");
            Console.WriteLine("══════════════════════════════════════════════════════════════");
            Console.WriteLine("1. Список пользователей");
            Console.WriteLine("2. Добавить пользователя");
            Console.WriteLine("3. Удалить пользователя");
            Console.WriteLine("4. Изменить права доступа");
            Console.WriteLine("5. Назад в главное меню");
            Console.Write("Выберите опцию: ");
        }

        private void DisplayScenariosMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("               СЦЕНАРИИ АВТОМАТИЗАЦИИ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");
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
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("                     УВЕДОМЛЕНИЯ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");
            Console.WriteLine("1. Показать все уведомления");
            Console.WriteLine("2. Показать непрочитанные");
            Console.WriteLine("3. Отметить как прочитанное");
            Console.WriteLine("4. Назад в главное меню");
            Console.Write("Выберите опцию: ");
        }

        private void ListAllDevices()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("                    ВСЕ УСТРОЙСТВА");
            Console.WriteLine("══════════════════════════════════════════════════════════════");

            if (devices.Count == 0)
            {
                Console.WriteLine("Устройств не найдено.");
                return;
            }

            Console.WriteLine("┌────┬──────────────────────┬──────────────────┬─────────────┬──────────────┬──────────────┐");
            Console.WriteLine("│ №  │ Название             │ Тип             │ Статус      │ Комната         │ Потребление  │");
            Console.WriteLine("├────┼──────────────────────┼──────────────────┼─────────────┼──────────────┼──────────────┤");

            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                Console.WriteLine($"│ {i + 1,2} │ {device.Name,-20} │ " +
                    $"{device.GetDeviceTypeString(),15} │ " +
                    $"{(device.IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                    $"{(device.Location != null ? device.Location.Name : "Нет"),-15} │ " +
                    $"{device.PowerConsumption,10:F1} Вт │");
            }
            Console.WriteLine("└────┴──────────────────────┴──────────────────┴─────────────┴──────────────┴──────────────┘");
        }

        private void ToggleDevice()
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
                Console.WriteLine("Неверный номер устройства!");
            }
        }

        private void AddDevice()
        {
            if (!HasAdminAccess()) return;

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
                        return;
                    }
                }
            }

            Console.WriteLine("Неверный ввод!");
        }

        private void DeleteDevice()
        {
            if (!HasAdminAccess()) return;

            ListAllDevices();
            Console.Write("Выберите номер устройства для удаления: ");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= devices.Count)
            {
                Device device = devices[choice - 1];
                // Удаляем устройство из комнаты 
                if (device.Location != null)
                {
                    device.Location.RemoveDevice(device);
                }
                // Удаляем устройство из списка 
                devices.RemoveAt(choice - 1);
                Console.WriteLine("Устройство удалено!");
                SaveData();
            }
            else
            {
                Console.WriteLine("Неверный номер устройства!");
            }
        }

        private void ListAllRooms()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("                      ВСЕ КОМНАТЫ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");

            if (rooms.Count == 0)
            {
                Console.WriteLine("Комнат не найдено.");
                return;
            }

            Console.WriteLine("┌────┬──────────────────────┬──────────────┬──────────────────────┐");
            Console.WriteLine("│ №  │ Название             │ Площадь (м²) │ Устройств            │");
            Console.WriteLine("├────┼──────────────────────┼──────────────┼──────────────────────┤");

            for (int i = 0; i < rooms.Count; i++)
            {
                Console.WriteLine($"│ {i + 1,2} │ {rooms[i].Name,-20} │ " +
                    $"{rooms[i].Area,12:F1} │ {rooms[i].GetDevices().Count,20} │");
            }
            Console.WriteLine("└────┴──────────────────────┴──────────────┴──────────────────────┘");
        }

        private void ShowRoomDevices()
        {
            ListAllRooms();
            Console.Write("Выберите номер комнаты: ");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
            {
                rooms[choice - 1].DisplayDevices();
            }
            else
            {
                Console.WriteLine("Неверный номер комнаты!");
            }
        }

        private void ShowRoomPower()
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
                Console.WriteLine("Неверный номер комнаты!");
            }
        }

        private void AddRoom()
        {
            if (!HasAdminAccess()) return;

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
            }
            else
            {
                Console.WriteLine("Неверный ввод площади!");
            }
        }

        private void DeleteRoom()
        {
            if (!HasAdminAccess()) return;

            ListAllRooms();
            Console.Write("Выберите номер комнаты для удаления: ");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= rooms.Count)
            {
                // Проверяем, есть ли устройства в комнате 
                if (rooms[choice - 1].GetDevices().Count > 0)
                {
                    Console.WriteLine("Ошибка: В комнате есть устройства! Сначала удалите или переместите их.");
                    return;
                }

                rooms.RemoveAt(choice - 1);
                Console.WriteLine("Комната удалена!");
                SaveData();
            }
            else
            {
                Console.WriteLine("Неверный номер комнаты!");
            }
        }

        private void ListUsers()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("                    ВСЕ ПОЛЬЗОВАТЕЛИ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");

            for (int i = 0; i < users.Count; i++)
            {
                Console.WriteLine($"{i + 1}. ");
                users[i].DisplayInfo();
                Console.WriteLine();
            }
        }

        private void AddUser()
        {
            if (!HasAdminAccess()) return;

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
            }
            else
            {
                Console.WriteLine("Неверный уровень доступа!");
            }
        }

        private void DeleteUser()
        {
            if (!HasAdminAccess()) return;

            ListUsers();
            Console.Write("Выберите номер пользователя для удаления: ");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= users.Count)
            {
                // Нельзя удалить текущего пользователя 
                if (users[choice - 1] == currentUser)
                {
                    Console.WriteLine("Ошибка: Нельзя удалить текущего пользователя!");
                    return;
                }

                users.RemoveAt(choice - 1);
                Console.WriteLine("Пользователь удален!");
                SaveData();
            }
            else
            {
                Console.WriteLine("Неверный номер пользователя!");
            }
        }

        private void ChangeUserAccess()
        {
            if (!HasAdminAccess()) return;

            ListUsers();
            Console.Write("Выберите номер пользователя: ");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= users.Count)
            {
                Console.Write("Новый уровень доступа (1 - User, 2 - Admin): ");
                if (int.TryParse(Console.ReadLine(), out int level) && (level == 1 || level == 2))
                {
                    users[choice - 1].SetAccessLevel(level == 2 ? AccessLevel.ADMIN : AccessLevel.USER);
                    Console.WriteLine("Права доступа изменены!");
                    SaveData();
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
        }

        private void ShowAllNotifications()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("                    ВСЕ УВЕДОМЛЕНИЯ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");

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

        private void ShowUnreadNotifications()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("                НЕПРОЧИТАННЫЕ УВЕДОМЛЕНИЯ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");

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

        private void MarkNotificationAsRead()
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
                Console.WriteLine("Неверный номер уведомления!");
            }
        }

        private void CreateEnergyReport()
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
        }

        private void SystemStatus()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("                    СТАТУС СИСТЕМЫ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");
            Console.WriteLine($"Комнат: {rooms.Count}");
            Console.WriteLine($"Устройств: {devices.Count}");
            Console.WriteLine($"Пользователей: {users.Count}");
            Console.WriteLine($"Сценариев: {scenarios.Count}");
            Console.WriteLine($"Уведомлений: {notifications.Count}");

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
        }

        private void Login()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════");
            Console.WriteLine("                     ВХОД В СИСТЕМУ");
            Console.WriteLine("══════════════════════════════════════════════════════════════");
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
                    return;
                }
            }
            Console.WriteLine("Ошибка входа!");
        }

        private void Logout()
        {
            if (currentUser != null)
            {
                currentUser.Logout();
                Console.WriteLine($"Пользователь {currentUser.Username} вышел из системы.");
                currentUser = null;
            }
        }

        private void DeviceManagement()
        {
            int choice;
            do
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
            } while (choice != (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN ? 5 : 3));
        }

        private void RoomManagement()
        {
            int choice;
            do
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
            } while (choice != (currentUser != null && currentUser.AccessLevel == AccessLevel.ADMIN ? 6 : 4));
        }

        private void UserManagement()
        {
            if (!HasAdminAccess()) return;

            int choice;
            do
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
            } while (choice != 5);
        }

        private void ScenarioManagement()
        {
            if (!HasUserAccess()) return;

            int choice;
            do
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
            } while (choice != 6);
        }

        private void NotificationManagement()
        {
            if (!HasUserAccess()) return;

            int choice;
            do
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
            } while (choice != 4);
        }

        private void CreateScenario()
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
        }

        private void ExecuteScenario()
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
            }
            else
            {
                Console.WriteLine("Неверный номер сценария!");
            }
        }

        private void ShowScenarioActions()
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
                Console.WriteLine("Неверный номер сценария!");
            }
        }

        private void AddActionToScenario(AutomationScenario specificScenario = null)
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
                    Console.WriteLine("Неверный номер сценария!");
                    return;
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
                    Console.WriteLine("Неверный выбор действия!");
                }
            }
            else
            {
                Console.WriteLine("Неверный номер устройства!");
            }
        }

        public void Run()
        {
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

            int choice;
            do
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
                            if (HasAdminAccess()) UserManagement();
                            break;
                        case 4: // Сценарии автоматизации 
                            if (HasUserAccess()) ScenarioManagement();
                            break;
                        case 5: // Уведомления 
                            if (HasUserAccess()) NotificationManagement();
                            break;
                        case 6: // Энергоотчеты 
                            if (HasUserAccess()) CreateEnergyReport();
                            break;
                        case 7: // Статус системы - доступно всем 
                            SystemStatus();
                            break;
                        case 8: // Войти/выйти из системы 
                            if (currentUser == null)
                            {
                                Login();
                            }
                            else
                            {
                                Logout();
                            }
                            break;
                        case 9: // Сохранить данные 
                            SaveData();
                            break;
                        case 0: // Выход 
                            Console.WriteLine("До свидания!");
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
            } while (choice != 0);
        }
    }
}