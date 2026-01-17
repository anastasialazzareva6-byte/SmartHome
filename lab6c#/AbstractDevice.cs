using System;

namespace SmartHomeSystem
{
    public abstract class AbstractDevice
    {
        protected string firmwareVersion;

        public abstract string DeviceCategory { get; }

        public AbstractDevice()
        {
            firmwareVersion = "1.0.0";
        }

        public virtual string GetFirmwareInfo()
        {
            return $"Firmware: {firmwareVersion}";
        }

        public abstract string PerformSelfTest();

        public virtual void UpdateFirmware(string version)
        {
            if (!string.IsNullOrEmpty(version))
            {
                firmwareVersion = version;
                Console.WriteLine($"Прошивка обновлена до версии {version}");
            }
        }
    }
}