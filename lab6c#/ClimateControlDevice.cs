using System;

namespace SmartHomeSystem
{
    public class ClimateControlDevice : Device, IEnergyEfficient
    {
        private double currentTemperature;
        private double targetTemperature;
        private bool ecoModeEnabled;

        public double MinTemperature { get; set; } = 16.0;
        public double MaxTemperature { get; set; } = 30.0;
        public string ClimateMode { get; set; } = "Auto";

        public bool IsEcoModeEnabled => ecoModeEnabled;

        public override string DeviceCategory => "Климатическое оборудование";

        public ClimateControlDevice(string deviceId, string name, string manufacturer,
                                   Room location, double powerConsumption,
                                   double minTemp = 16.0, double maxTemp = 30.0)
            : base(deviceId, name, manufacturer, DeviceType.CLIMATE_CONTROL, location, powerConsumption)
        {
            MinTemperature = minTemp;
            MaxTemperature = maxTemp;
            currentTemperature = 22.0;
            targetTemperature = 22.0;
            ecoModeEnabled = false;
        }

        public ClimateControlDevice(string deviceId, string name, string manufacturer,
                                   Room location, double powerConsumption,
                                   DateTime manufacturingDate, double minTemp, double maxTemp)
            : base(deviceId, name, manufacturer, DeviceType.CLIMATE_CONTROL,
                  location, powerConsumption, manufacturingDate)
        {
            MinTemperature = minTemp;
            MaxTemperature = maxTemp;
            currentTemperature = 22.0;
            targetTemperature = 22.0;
            ecoModeEnabled = false;
        }

        public override void TurnOn()
        {
            base.TurnOn();
            Console.WriteLine($"Текущая температура: {currentTemperature}°C, Целевая: {targetTemperature}°C");
        }

        public override void TurnOn(string reason)
        {
            base.TurnOn(reason);
            Console.WriteLine($"Режим климата: {ClimateMode}");
        }

        public override void TurnOff()
        {
            base.TurnOff();
            Console.WriteLine($"[CLIMATE] Климатическое устройство '{Name}' выключено. Последняя температура: {currentTemperature}°C");
        }

        public void SetTemperature(double temperature)
        {
            if (temperature < MinTemperature || temperature > MaxTemperature)
            {
                Console.WriteLine($"Ошибка: температура должна быть в диапазоне {MinTemperature}-{MaxTemperature}°C");
                return;
            }

            targetTemperature = temperature;
            Console.WriteLine($"[CLIMATE] Установлена целевая температура: {temperature}°C");

            if (!IsOn)
            {
                TurnOn("Установка температуры");
            }
        }

        public void UpdateCurrentTemperature(double newTemp)
        {
            currentTemperature = newTemp;
            Console.WriteLine($"[CLIMATE] Текущая температура обновлена: {newTemp}°C");

            if (Math.Abs(currentTemperature - targetTemperature) > 2.0 && IsOn)
            {
                Console.WriteLine($"[CLIMATE] Регулировка температуры для достижения {targetTemperature}°C");
            }
        }

        public override string GetFirmwareInfo()
        {
            string baseInfo = base.GetFirmwareInfo();
            return $"{baseInfo} | Климатический контроллер | Режим: {ClimateMode}";
        }

        public override string PerformSelfTest()
        {
            string baseTest = base.PerformSelfTest();
            return $"{baseTest} | Проверка температурных датчиков... ОК | Диапазон: {MinTemperature}-{MaxTemperature}°C";
        }

        public override string RunDiagnostics()
        {
            string baseDiagnostics = base.RunDiagnostics();
            return $"{baseDiagnostics} | Температура: {currentTemperature}°C/{targetTemperature}°C | Режим: {ClimateMode}";
        }

        public double CalculateEnergySaving()
        {
            if (!IsOn) return 0;

            double baseConsumption = PowerConsumption;
            double ecoConsumption = ecoModeEnabled ? baseConsumption * 0.7 : baseConsumption;
            return baseConsumption - ecoConsumption;
        }

        public void EnableEcoMode()
        {
            ecoModeEnabled = true;
            Console.WriteLine($"[ECO] Эко-режим включен для {Name}. Экономия энергии: {CalculateEnergySaving():F1} Вт");

            if (IsOn && targetTemperature > 20)
            {
                targetTemperature = 20;
                Console.WriteLine($"[ECO] Температура скорректирована до {targetTemperature}°C для экономии энергии");
            }
        }

        public void DisableEcoMode()
        {
            ecoModeEnabled = false;
            Console.WriteLine($"[ECO] Эко-режим выключен для {Name}");
        }

        public void DisplayManufacturingInfo()
        {
            Console.WriteLine(GetManufacturingInfo());
            Console.WriteLine($"Диапазон температур: {MinTemperature}-{MaxTemperature}°C");
        }

        public override Device DeepClone()
        {
            var clone = (ClimateControlDevice)base.DeepClone();
            clone.ClimateMode = string.Copy(ClimateMode);
            return clone;
        }

        public override string ToString()
        {
            return $"{base.ToString()} | Температура: {currentTemperature}°C | Режим: {ClimateMode}";
        }
    }
}