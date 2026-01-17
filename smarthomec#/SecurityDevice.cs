using System;

namespace SmartHomeSystem
{
    public class SecurityDevice : Device, IEnergyEfficient
    {
        private bool isArmed;
        private int securityLevel;
        private bool ecoModeEnabled;

        public bool HasCamera { get; set; }
        public bool HasMotionSensor { get; set; }
        public string SecurityZone { get; set; } = "Общая";

        public bool IsEcoModeEnabled => ecoModeEnabled;

        public override string DeviceCategory => "Устройство безопасности";

        public SecurityDevice(string deviceId, string name, string manufacturer,
                             Room location, double powerConsumption,
                             bool hasCamera = false, bool hasMotionSensor = false)
            : base(deviceId, name, manufacturer, DeviceType.SECURITY, location, powerConsumption)
        {
            HasCamera = hasCamera;
            HasMotionSensor = hasMotionSensor;
            isArmed = false;
            securityLevel = 1;
            ecoModeEnabled = false;
        }

        public override void TurnOn()
        {
            base.TurnOn();
            Console.WriteLine($"Режим охраны: {(isArmed ? "АКТИВЕН" : "НЕАКТИВЕН")}");
        }

        public void ArmSecurity()
        {
            if (!IsOn)
            {
                TurnOn();
            }

            isArmed = true;
            securityLevel = 3;
            Console.WriteLine($"[SECURITY] Устройство '{Name}' поставлено на охрану. Уровень: {securityLevel}");
        }

        public void DisarmSecurity()
        {
            isArmed = false;
            securityLevel = 1;
            Console.WriteLine($"[SECURITY] Устройство '{Name}' снято с охраны.");
        }

        public void TriggerSecurityAlert(string reason)
        {
            if (isArmed && IsOn)
            {
                Console.WriteLine($"[ALERT!] Сработала сигнализация устройства '{Name}'!");
                Console.WriteLine($"Причина: {reason}");
                Console.WriteLine($"Зона: {SecurityZone}");
                Console.WriteLine("Отправка уведомления владельцу...");
            }
        }

        public override string GetFirmwareInfo()
        {
            string baseInfo = base.GetFirmwareInfo();
            return $"{baseInfo} | Безопасность | Уровень: {securityLevel}";
        }

        public override string PerformSelfTest()
        {
            string baseTest = base.PerformSelfTest();
            string cameraTest = HasCamera ? "Камера... ОК" : "Камера... НЕТ";
            string sensorTest = HasMotionSensor ? "Датчик движения... ОК" : "Датчик движения... НЕТ";
            return $"{baseTest} | {cameraTest} | {sensorTest} | Уровень безопасности: {securityLevel}";
        }

        public override string RunDiagnostics()
        {
            string baseDiagnostics = base.RunDiagnostics();
            return $"{baseDiagnostics} | Охрана: {(isArmed ? "Активна" : "Неактивна")} | " +
                   $"Камера: {(HasCamera ? "Есть" : "Нет")} | Зона: {SecurityZone}";
        }

        public double CalculateEnergySaving()
        {
            if (!IsOn) return 0;

            double baseConsumption = PowerConsumption;
            double ecoConsumption = ecoModeEnabled ? baseConsumption * 0.6 : baseConsumption;
            return baseConsumption - ecoConsumption;
        }

        public void EnableEcoMode()
        {
            ecoModeEnabled = true;
            Console.WriteLine($"[ECO] Эко-режим включен для {Name}. Экономия: {CalculateEnergySaving():F1} Вт");

            if (HasCamera && IsOn)
            {
                Console.WriteLine("[ECO] Частота обновления камеры снижена для экономии энергии");
            }
        }

        public void DisableEcoMode()
        {
            ecoModeEnabled = false;
            Console.WriteLine($"[ECO] Эко-режим выключен для {Name}");
        }

        public override Device DeepClone()
        {
            var clone = (SecurityDevice)base.DeepClone();
            clone.SecurityZone = string.Copy(SecurityZone);
            return clone;
        }

        public override string ToString()
        {
            string securityStatus = isArmed ? "НА ОХРАНЕ" : "ОХРАНА ВЫКЛ";
            return $"{base.ToString()} | {securityStatus} | Зона: {SecurityZone}";
        }
    }
}