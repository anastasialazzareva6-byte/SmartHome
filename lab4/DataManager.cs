using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartHomeSystem
{
    public static class DataManager
    {
        private const string ROOMS_FILE = "rooms.dat";
        private const string DEVICES_FILE = "devices.dat";
        private const string USERS_FILE = "users.dat";
        private const string SCENARIOS_FILE = "scenarios.dat";
        private const string NOTIFICATIONS_FILE = "notifications.dat";
        private const string ACTIONS_FILE = "actions.dat";

        public static void SaveRooms(List<Room> rooms)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(ROOMS_FILE))
                {
                    foreach (var room in rooms)
                    {
                        file.WriteLine(room.Serialize());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения комнат: {ex.Message}");
            }
        }

        public static List<Room> LoadRooms()
        {
            List<Room> rooms = new List<Room>();

            if (!File.Exists(ROOMS_FILE)) return rooms;

            try
            {
                using (StreamReader file = new StreamReader(ROOMS_FILE))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var room = Room.Deserialize(line);
                            if (room != null) rooms.Add(room);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки комнат: {ex.Message}");
            }

            return rooms;
        }

        public static void SaveDevices(List<Device> devices)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(DEVICES_FILE))
                {
                    foreach (var device in devices)
                    {
                        file.WriteLine(device.Serialize());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения устройств: {ex.Message}");
            }
        }

        public static List<Device> LoadDevices(List<Room> rooms)
        {
            List<Device> devices = new List<Device>();

            if (!File.Exists(DEVICES_FILE)) return devices;

            try
            {
                using (StreamReader file = new StreamReader(DEVICES_FILE))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // Извлекаем ID комнаты из данных устройства 
                            var parts = line.Split('|');
                            if (parts.Length >= 5)
                            {
                                string locationId = parts[4];
                                Room location = rooms.FirstOrDefault(r => r.RoomId == locationId);

                                if (location != null)
                                {
                                    var device = Device.Deserialize(line, location);
                                    if (device != null)
                                    {
                                        devices.Add(device);
                                        location.AddDevice(device);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки устройств: {ex.Message}");
            }

            return devices;
        }

        public static void SaveUsers(List<User> users)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(USERS_FILE))
                {
                    foreach (var user in users)
                    {
                        file.WriteLine(user.Serialize());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения пользователей: {ex.Message}");
            }
        }

        public static List<User> LoadUsers()
        {
            List<User> users = new List<User>();

            if (!File.Exists(USERS_FILE)) return users;

            try
            {
                using (StreamReader file = new StreamReader(USERS_FILE))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var user = User.Deserialize(line);
                            if (user != null) users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки пользователей: {ex.Message}");
            }

            return users;
        }

        public static void SaveScenarios(List<AutomationScenario> scenarios)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(SCENARIOS_FILE))
                {
                    foreach (var scenario in scenarios)
                    {
                        file.WriteLine(scenario.Serialize());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения сценариев: {ex.Message}");
            }
        }

        public static List<AutomationScenario> LoadScenarios()
        {
            List<AutomationScenario> scenarios = new List<AutomationScenario>();

            if (!File.Exists(SCENARIOS_FILE)) return scenarios;

            try
            {
                using (StreamReader file = new StreamReader(SCENARIOS_FILE))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var scenario = AutomationScenario.Deserialize(line);
                            if (scenario != null) scenarios.Add(scenario);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки сценариев: {ex.Message}");
            }

            return scenarios;
        }

        public static void SaveNotifications(List<Notification> notifications)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(NOTIFICATIONS_FILE))
                {
                    foreach (var notification in notifications)
                    {
                        file.WriteLine(notification.Serialize());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения уведомлений: {ex.Message}");
            }
        }

        public static List<Notification> LoadNotifications()
        {
            List<Notification> notifications = new List<Notification>();

            if (!File.Exists(NOTIFICATIONS_FILE)) return notifications;

            try
            {
                using (StreamReader file = new StreamReader(NOTIFICATIONS_FILE))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var notification = Notification.Deserialize(line);
                            if (notification != null) notifications.Add(notification);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки уведомлений: {ex.Message}");
            }

            return notifications;
        }

        public static void InitializeDefaultData(List<Room> rooms, List<Device> devices,
            List<User> users, List<Notification> notifications,
            List<AutomationScenario> scenarios)
        {
            // Создание комнат 
            rooms.Add(new Room("LR001", "Гостиная", 25.5));
            rooms.Add(new Room("KIT001", "Кухня", 15.0));
            rooms.Add(new Room("BR001", "Спальня", 18.0));

            // Создание устройств 
            devices.Add(new Device("DEV001", "Умный телевизор", "Samsung",
                DeviceType.MULTIMEDIA, rooms[0], 120.0));
            devices.Add(new Device("DEV002", "Кондиционер", "LG",
                DeviceType.CLIMATE_CONTROL, rooms[0], 1500.0));
            devices.Add(new Device("DEV003", "Умный свет", "Philips",
                DeviceType.ACTUATOR, rooms[1], 15.0));
            devices.Add(new Device("DEV004", "Камера безопасности", "Xiaomi",
                DeviceType.SECURITY, rooms[2], 8.0));
            devices.Add(new Device("DEV005", "Датчик температуры", "Bosch",
                DeviceType.SENSOR, rooms[1], 2.0));

            // Добавление устройств в комнаты 
            foreach (var device in devices)
            {
                if (device.Location != null)
                {
                    device.Location.AddDevice(device);
                }
            }

            // Создание пользователей   
            users.Add(new User("USR001", "admin", "123", AccessLevel.ADMIN,
                "admin@home.com", "+79001234567"));
            users.Add(new User("USR002", "user", "123", AccessLevel.USER, "user@home.com",
                "+79007654321"));

            // Создание уведомлений 
            notifications.Add(new Notification("NOT001", NotificationType.INFO,
                "Система успешно запущена"));

            // Создание тестового сценария 
            AutomationScenario morningScenario = new AutomationScenario("SCN001", "Утренний режим", "07:00");
            morningScenario.AddAction(new ScenarioAction("ACT001", devices[2], "turnOn"));
            morningScenario.AddAction(new ScenarioAction("ACT002", devices[0], "turnOn"));
            scenarios.Add(morningScenario);
        }
    }
}