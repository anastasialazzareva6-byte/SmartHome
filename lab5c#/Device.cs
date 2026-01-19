using System;
using System.Text;

namespace SmartHomeSystem
{
    public class Device
    {
        // Статическое поле для подсчета всех устройств
        private static int totalDeviceCount = 0;

        // Статическое свойство для доступа
        public static int TotalDeviceCount => totalDeviceCount;

        // Статический метод для получения информации
        public static string GetDeviceStatistics()
        {
            return $"Всего устройств в системе: {totalDeviceCount}";
        }

        private string name;
        private double powerConsumption;

        public string DeviceId { get; private set; }
        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Название устройства не может быть пустым", nameof(Name));

                if (value.Length > 50)
                    throw new ArgumentException("Название устройства слишком длинное (макс. 50 символов)", nameof(Name));

                name = value;
            }
        }
        public string Manufacturer { get; set; }
        public DeviceType DeviceType { get; set; }
        public Room Location { get; set; }
        public bool IsOn { get; private set; }
        public bool IsOnline { get; set; } = true;
        public double PowerConsumption
        {
            get => powerConsumption;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Потребляемая мощность не может быть отрицательной", nameof(PowerConsumption));

                if (value > 10000)
                    throw new ArgumentException("Слишком высокая мощность (максимум 10000 Вт)", nameof(PowerConsumption));

                powerConsumption = value;
            }
        }

        public Device(string deviceId, string name, string manufacturer,
                     DeviceType deviceType, Room location, double powerConsumption)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("ID устройства не может быть пустым", nameof(deviceId));

            DeviceId = deviceId;
            Name = name;
            Manufacturer = manufacturer;
            DeviceType = deviceType;
            Location = location;
            PowerConsumption = powerConsumption;
            IsOn = false;

            totalDeviceCount++;  // Увеличиваем счетчик при создании устройства
        }

        // Деструктор для уменьшения счетчика
        ~Device()
        {
            totalDeviceCount--;
        }

        public void TurnOn()
        {
            IsOn = true;
            Console.WriteLine($"[+] Устройство '{Name}' включено.");
        }

        public void TurnOff()
        {
            IsOn = false;
            Console.WriteLine($"[−] Устройство '{Name}' выключено.");
        }

        public void UpdatePowerConsumption(double value)
        {
            try
            {
                PowerConsumption = value;
                Console.WriteLine($"Мощность устройства '{Name}' изменена на {value} Вт");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка обновления мощности: {ex.Message}");
                throw;
            }
        }

        public string GetDeviceTypeString()
        {
            switch (DeviceType)
            {
                case DeviceType.SENSOR: return "Датчик";
                case DeviceType.ACTUATOR: return "Исполнительное";
                case DeviceType.CLIMATE_CONTROL: return "Климат";
                case DeviceType.SECURITY: return "Безопасность";
                case DeviceType.MULTIMEDIA: return "Мультимедиа";
                default: return "Неизвестно";
            }
        }

        public string GetStatus()
        {
            return IsOn ? "ВКЛ" : "ВЫКЛ";
        }

        public string Serialize()
        {
            return $"{DeviceId}|{Name}|{Manufacturer}|{(int)DeviceType}|" +
                   $"{(Location != null ? Location.RoomId : "NULL")}|{PowerConsumption}";
        }

        public static Device Deserialize(string data, Room location)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Данные для десериализации не могут быть пустыми", nameof(data));

            var parts = data.Split('|');
            if (parts.Length != 6)
                throw new FormatException($"Неверный формат данных устройства: {data}");

            try
            {
                return new Device(
                    parts[0],
                    parts[1],
                    parts[2],
                    (DeviceType)int.Parse(parts[3]),
                    location,
                    double.Parse(parts[5])
                );
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Ошибка парсинга данных устройства: {data}", ex);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException($"Ошибка валидации данных устройства: {data}", ex);
            }
        }

        public override string ToString()
        {
            return $"{Name} ({GetDeviceTypeString()}) - {GetStatus()}";
        }
    }
}