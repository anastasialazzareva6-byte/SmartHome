using System;

namespace SmartHomeSystem
{
    public class MultimediaDevice : Device, IEnergyEfficient, IDeviceDiagnostic
    {
        private bool ecoModeEnabled;
        private string currentContent;
        private int volumeLevel;

        public bool HasSmartFeatures { get; set; }
        public string MediaType { get; set; } = "Видео";
        public int ScreenSize { get; set; }

        public bool IsEcoModeEnabled => ecoModeEnabled;

        public override string DeviceCategory => "Мультимедийное устройство";

        public MultimediaDevice(string deviceId, string name, string manufacturer,
                               Room location, double powerConsumption,
                               int screenSize = 32, bool hasSmartFeatures = true)
            : base(deviceId, name, manufacturer, DeviceType.MULTIMEDIA, location, powerConsumption)
        {
            ScreenSize = screenSize;
            HasSmartFeatures = hasSmartFeatures;
            ecoModeEnabled = false;
            currentContent = "Выключено";
            volumeLevel = 50;
        }

        public override void TurnOn()
        {
            base.TurnOn();
            Console.WriteLine($"Размер экрана: {ScreenSize}\", Текущий контент: {currentContent}");
        }

        public void PlayContent(string content)
        {
            if (!IsOn)
            {
                TurnOn();
            }

            currentContent = content;
            Console.WriteLine($"[MULTIMEDIA] Воспроизведение: {content}");
        }

        public void SetVolume(int level)
        {
            if (level < 0) level = 0;
            if (level > 100) level = 100;

            volumeLevel = level;
            Console.WriteLine($"[MULTIMEDIA] Громкость установлена на {level}%");
        }

        string IDeviceDiagnostic.GetDeviceInfo()
        {
            return $"Мультимедиа: {Name} | Экран: {ScreenSize}\" | Smart: {(HasSmartFeatures ? "Да" : "Нет")}";
        }

        public override string GetFirmwareInfo()
        {
            string baseInfo = base.GetFirmwareInfo();
            return $"{baseInfo} | Мультимедиа | Тип: {MediaType} | Экран: {ScreenSize}\"";
        }

        public override string PerformSelfTest()
        {
            string baseTest = base.PerformSelfTest();
            string smartTest = HasSmartFeatures ? "Smart функции... ОК" : "Smart функции... НЕТ";
            string audioTest = $"Аудиосистема... ОК (Громкость: {volumeLevel}%)";
            return $"{baseTest} | {smartTest} | {audioTest}";
        }

        public double CalculateEnergySaving()
        {
            if (!IsOn) return 0;

            double baseConsumption = PowerConsumption;
            double screenFactor = ScreenSize / 32.0;
            double adjustedConsumption = baseConsumption * screenFactor;
            double ecoConsumption = ecoModeEnabled ? adjustedConsumption * 0.5 : adjustedConsumption;

            return adjustedConsumption - ecoConsumption;
        }

        public void EnableEcoMode()
        {
            ecoModeEnabled = true;
            double saving = CalculateEnergySaving();
            Console.WriteLine($"[ECO] Эко-режим включен для {Name}. Экономия: {saving:F1} Вт");

            if (IsOn)
            {
                Console.WriteLine("[ECO] Яркость экрана снижена для экономии энергии");
                if (volumeLevel > 70)
                {
                    volumeLevel = 70;
                    Console.WriteLine("[ECO] Громкость снижена до 70% для экономии");
                }
            }
        }

        public void DisableEcoMode()
        {
            ecoModeEnabled = false;
            Console.WriteLine($"[ECO] Эко -режим выключен для {Name}");
        }

        public override Device DeepClone()
        {
            var clone = (MultimediaDevice)base.DeepClone();
            clone.currentContent = string.Copy(currentContent);
            clone.MediaType = string.Copy(MediaType);
            return clone;
        }

        public override string ToString()
        {
            return $"{base.ToString()} | Контент: {currentContent} | Громкость: {volumeLevel}%";
        }
    }
}