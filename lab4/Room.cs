using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHomeSystem
{
    public class Room
    {
        public string RoomId { get; private set; }
        public string Name { get; set; }
        public double Area { get; set; }
        private List<Device> devices;

        public Room(string roomId, string name, double area)
        {
            RoomId = roomId;
            Name = name;
            Area = area;
            devices = new List<Device>();
        }

        public void AddDevice(Device device)
        {
            if (!devices.Contains(device))
            {
                devices.Add(device);
                device.Location = this;
            }
        }

        public bool RemoveDevice(Device device)
        {
            if (devices.Remove(device))
            {
                device.Location = null;
                return true;
            }
            return false;
        }

        public List<Device> GetDevices()
        {
            return new List<Device>(devices);
        }

        public double CalculateRoomPowerConsumption()
        {
            double total = 0;
            foreach (var device in devices)
            {
                if (device.IsOn)
                {
                    total += device.PowerConsumption;
                }
            }
            return total;
        }

        public void DisplayDevices()
        {
            Console.WriteLine($"\nУстройства в комнате '{Name}':");

            if (devices.Count == 0)
            {
                Console.WriteLine("  Устройств нет.");
                return;
            }

            Console.WriteLine("┌────┬──────────────────────┬──────────────────┬─────────────┬───────────────┐");
            Console.WriteLine("│ №  │ Название             │ Тип             │ Статус      │ Потребление  │");
            Console.WriteLine("├────┼──────────────────────┼──────────────────┼─────────────┼───────────────┤");

            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($"│ {i + 1,2} │ {devices[i].Name,-20} │ " +
                    $"{GetDeviceTypeString(devices[i].DeviceType),-15} │ " +
                    $"{(devices[i].IsOn ? "ВКЛ" : "ВЫКЛ"),-11} │ " +
                    $"{devices[i].PowerConsumption,10:F1} Вт │");
            }
            Console.WriteLine("└────┴──────────────────────┴──────────────────┴─────────────┴───────────────┘");
        }

        private string GetDeviceTypeString(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.SENSOR: return "Датчик";
                case DeviceType.ACTUATOR: return "Исполнительное";
                case DeviceType.CLIMATE_CONTROL: return "Климат";
                case DeviceType.SECURITY: return "Безопасность";
                case DeviceType.MULTIMEDIA: return "Мультимедиа";
                default: return "Неизвестно";
            }
        }

        public string Serialize()
        {
            return $"{RoomId}|{Name}|{Area}";
        }

        public static Room Deserialize(string data)
        {
            var parts = data.Split('|');
            if (parts.Length != 3) return null;

            return new Room(
                parts[0],
                parts[1],
                double.Parse(parts[2])
            );
        }

        public override string ToString()
        {
            return $"{Name} ({Area} м²) - {devices.Count} устройств";
        }
    }
}