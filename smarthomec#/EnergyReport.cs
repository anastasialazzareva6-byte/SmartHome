using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeSystem
{
    public class EnergyReport
    {
        public string ReportId { get; private set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public double TotalConsumption { get; private set; }
        public double PeakLoad { get; private set; }
        public Dictionary<Device, double> DeviceConsumptions { get; private set; }

        public EnergyReport(string reportId, DateTime periodStart, DateTime periodEnd)
        {
            ReportId = reportId;
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
            DeviceConsumptions = new Dictionary<Device, double>();
        }

        public void AddDeviceConsumption(Device device, double consumption)
        {
            DeviceConsumptions[device] = consumption;
        }

        public void GenerateReport()
        {
            TotalConsumption = DeviceConsumptions.Values.Sum();
            PeakLoad = DeviceConsumptions.Values.DefaultIfEmpty(0).Max();
        }

        public List<Device> GetTopConsumingDevices(int topN)
        {
            return DeviceConsumptions
                .OrderByDescending(kv => kv.Value)
                .Take(topN)
                .Select(kv => kv.Key)
                .ToList();
        }

        public void DisplayReport()
        {
            Console.WriteLine("\n══════════════════════════════════════════════");
            Console.WriteLine("               ОТЧЕТ ПО ЭНЕРГОПОТРЕБЛЕНИЮ");
            Console.WriteLine($"               {ReportId}");
            Console.WriteLine("══════════════════════════════════════════════");
            Console.WriteLine($"Период: {PeriodStart:dd.MM.yyyy} - {PeriodEnd:dd.MM.yyyy}");
            Console.WriteLine($"Общее потребление: {TotalConsumption:F2} кВт·ч");
            Console.WriteLine($"Пиковая нагрузка: {PeakLoad:F2} кВт");
            Console.WriteLine("\nПотребление по устройствам:");
            Console.WriteLine("┌────┬────────────────────┬─────────────────────┐");
            Console.WriteLine("│ №  │ Устройство         │ Потребление (кВт·ч)  │");
            Console.WriteLine("├────┼────────────────────┼─────────────────────┤");

            int counter = 1;
            foreach (var kvp in DeviceConsumptions.OrderByDescending(kv => kv.Value))
            {
                Console.WriteLine($"│ {counter,2} │ {kvp.Key.Name,-20} │ {kvp.Value,20:F2} │");
                counter++;
            }

            Console.WriteLine("└────┴────────────────────┴─────────────────────┘");
            Console.WriteLine("══════════════════════════════════════════════\n");
        }

        public string Serialize()
        {
            return $"{ReportId}|{PeriodStart}|{PeriodEnd}|{TotalConsumption}|{PeakLoad}";
        }

        public static EnergyReport Deserialize(string data)
        {
            var parts = data.Split('|');
            if (parts.Length != 5) return null;

            return new EnergyReport(
                parts[0],
                DateTime.Parse(parts[1]),
                DateTime.Parse(parts[2])
            )
            {
                TotalConsumption = double.Parse(parts[3]),
                PeakLoad = double.Parse(parts[4])
            };
        }

        public override string ToString()
        {
            return $"{ReportId} - {TotalConsumption:F2} кВт·ч";
        }
    }
}