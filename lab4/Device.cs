using System;
using System.Text;

namespace SmartHomeSystem
{
    public class Device
    {
        public string DeviceId { get; private set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public DeviceType DeviceType { get; set; }
        public Room Location { get; set; }
        public bool IsOn { get; private set; }
        public bool IsOnline { get; set; } = true;
        public double PowerConsumption { get; set; }

        public Device(string deviceId, string name, string manufacturer,
                     DeviceType deviceType, Room location, double powerConsumption)
        {
            DeviceId = deviceId;
            Name = name;
            Manufacturer = manufacturer;
            DeviceType = deviceType;
            Location = location;
            PowerConsumption = powerConsumption;
            IsOn = false;
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
            var parts = data.Split('|');
            if (parts.Length != 6) return null;

            return new Device(
                parts[0],
                parts[1],
                parts[2],
                (DeviceType)int.Parse(parts[3]),
                location,
                double.Parse(parts[5])
            );
        }

        public override string ToString()
        {
            return $"{Name} ({GetDeviceTypeString()}) - {GetStatus()}";
        }
    }
}