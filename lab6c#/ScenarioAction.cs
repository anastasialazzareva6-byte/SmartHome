using System;

namespace SmartHomeSystem
{
    public class ScenarioAction
    {
        public string ActionId { get; private set; }
        public Device TargetDevice { get; set; }
        public string Command { get; set; }

        public ScenarioAction(string actionId, Device targetDevice, string command)
        {
            ActionId = actionId;
            TargetDevice = targetDevice;
            Command = command;
        }

        public void Execute()
        {
            Console.WriteLine($"[ACTION] Выполнение: {Command} на {TargetDevice.Name}");
            if (Command == "turnOn")
            {
                TargetDevice.TurnOn();
            }
            else if (Command == "turnOff")
            {
                TargetDevice.TurnOff();
            }
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"  [{ActionId}] {Command} -> {TargetDevice.Name}");
        }

        public string Serialize()
        {
            return $"{ActionId}|{TargetDevice.DeviceId}|{Command}";
        }

        public static ScenarioAction Deserialize(string data, Device device)
        {
            var parts = data.Split('|');
            if (parts.Length != 3) return null;

            return new ScenarioAction(
                parts[0],
                device,
                parts[2]
            );
        }

        public override string ToString()
        {
            return $"{Command} -> {TargetDevice.Name}";
        }
    }
}