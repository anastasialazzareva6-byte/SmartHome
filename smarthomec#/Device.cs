using System;
using System.Text;

namespace SmartHomeSystem
{
    public class Device : AbstractDevice, ICloneableDevice, IDeviceDiagnostic
    {
        private static int totalDeviceCount = 0;

        private string name;
        private double powerConsumption;
        private bool isOn;

        protected DateTime manufacturingDate;

        public static int TotalDeviceCount => totalDeviceCount;

        public static string GetDeviceStatistics()
        {
            return $"Всего устройств в системе: {totalDeviceCount}";
        }

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
        public bool IsOn
        {
            get => isOn;
            protected set => isOn = value;
        }
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

        public override string DeviceCategory => "Базовое устройство";

        public Device(string deviceId, string name, string manufacturer,
                     DeviceType deviceType, Room location, double powerConsumption)
            : this(deviceId, name, manufacturer, deviceType, location, powerConsumption, DateTime.Now)
        {
        }

        protected Device(string deviceId, string name, string manufacturer,
                        DeviceType deviceType, Room location, double powerConsumption, DateTime manufacturingDate)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("ID устройства не может быть пустым", nameof(deviceId));

            DeviceId = deviceId;
            Name = name;
            Manufacturer = manufacturer;
            DeviceType = deviceType;
            Location = location;
            PowerConsumption = powerConsumption;
            this.manufacturingDate = manufacturingDate;
            isOn = false;

            totalDeviceCount++;
        }

        ~Device()
        {
            totalDeviceCount--;
        }

        public virtual void TurnOn()
        {
            isOn = true;
            Console.WriteLine($"[+] Устройство '{Name}' включено.");
        }

        public virtual void TurnOn(string reason)
        {
            isOn = true;
            Console.WriteLine($"[+] Устройство '{Name}' включено. Причина: {reason}");
        }

        public virtual void TurnOff()
        {
            isOn = false;
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

        public void DisplayDeviceInfo()
        {
            Console.WriteLine($"Информация об устройстве {Name}:");
            Console.WriteLine(GetFirmwareInfo());
            Console.WriteLine($"Категория: {DeviceCategory}");
            Console.WriteLine($"Дата производства: {manufacturingDate:dd.MM.yyyy}");
        }

        public override string PerformSelfTest()
        {
            return $"Самотестирование устройства {Name}... ОК";
        }

        public override string GetFirmwareInfo()
        {
            return $"Firmware: {firmwareVersion} | Устройство: {Name}";
        }

        public virtual string RunDiagnostics()
        {
            return $"Диагностика устройства {Name}: Статус - {(IsOn ? "Включено" : "Выключено")}, " +
                   $"Подключение - {(IsOnline ? "Онлайн" : "Оффлайн")}";
        }

        public virtual string GetDeviceInfo()
        {
            return $"{Name} ({GetDeviceTypeString()}) - Производитель: {Manufacturer}, Потребление: {PowerConsumption} Вт";
        }

        public virtual Device ShallowClone()
        {
            return (Device)this.MemberwiseClone();
        }

        public virtual Device DeepClone()
        {
            var clone = (Device)this.MemberwiseClone();
            clone.name = string.Copy(name);
            clone.DeviceId = string.Copy(DeviceId);
            clone.Manufacturer = string.Copy(Manufacturer);
            return clone;
        }

        public virtual object Clone()
        {
            return DeepClone();
        }

        public string Serialize()
        {
            // Упрощенная сериализация без даты производства
            return $"{DeviceId}|{Name}|{Manufacturer}|{(int)DeviceType}|" +
                   $"{(Location != null ? Location.RoomId : "NULL")}|{PowerConsumption}|{IsOn}";
        }

        public static Device Deserialize(string data, Room location)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Данные для десериализации не могут быть пустыми", nameof(data));

            var parts = data.Split('|');
            if (parts.Length != 7)  // Теперь 7 частей: 6 полей + статус включения
                throw new FormatException($"Неверный формат данных устройства: {data}");

            try
            {
                var device = new Device(
                    parts[0],
                    parts[1],
                    parts[2],
                    (DeviceType)int.Parse(parts[3]),
                    location,
                    double.Parse(parts[5])
                );

                // Устанавливаем статус включения/выключения
                if (bool.Parse(parts[6]))
                {
                    device.IsOn = true;
                }

                return device;
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

        protected string GetManufacturingInfo()
        {
            return $"Устройство произведено: {manufacturingDate:dd.MM.yyyy}";
        }

        public override void UpdateFirmware(string version)
        {
            base.UpdateFirmware(version);
            Console.WriteLine($"Устройство {Name} успешно обновлено до версии {version}");
        }
    }
}